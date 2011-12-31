using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Threading;
using Samba.Domain;
using Samba.Domain.Models.Menus;
using Samba.Domain.Models.Settings;
using Samba.Domain.Models.Tickets;
using Samba.Infrastructure.Data.Serializer;
using Samba.Localization.Properties;
using Samba.Persistance.Data;
using Samba.Services.Common;
using Samba.Services.Implementations.PrinterModule.PrintJobs;
using Samba.Services.Implementations.PrinterModule.Tools;
using Samba.Services.Implementations.PrinterModule.ValueChangers;

namespace Samba.Services.Implementations.PrinterModule
{
    internal class PrinterData
    {
        public Printer Printer { get; set; }
        public PrinterTemplate PrinterTemplate { get; set; }
        public Ticket Ticket { get; set; }
    }

    internal class TicketData
    {
        public Ticket Ticket { get; set; }
        public IEnumerable<Order> Orders { get; set; }
        public PrintJob PrintJob { get; set; }
    }

    [Export(typeof(IPrinterService))]
    public class PrinterService : AbstractService, IPrinterService
    {
        private readonly IApplicationState _applicationState;
        private readonly ICacheService _cacheService;

        [ImportingConstructor]
        public PrinterService(IApplicationState applicationState, ICacheService cacheService)
        {
            _applicationState = applicationState;
            _cacheService = cacheService;

            ValidatorRegistry.RegisterDeleteValidator(new PrinterDeleteValidator());
            ValidatorRegistry.RegisterDeleteValidator(new PrinterTemplateDeleteValidator());
        }

        private IEnumerable<Printer> _printers;
        public IEnumerable<Printer> Printers
        {
            get { return _printers ?? (_printers = Dao.Query<Printer>()); }
        }

        private IEnumerable<PrinterTemplate> _printerTemplates;
        protected IEnumerable<PrinterTemplate> PrinterTemplates
        {
            get { return _printerTemplates ?? (_printerTemplates = Dao.Query<PrinterTemplate>()); }
        }

        public Printer PrinterById(int id)
        {
            return Printers.Single(x => x.Id == id);
        }

        private PrinterTemplate PrinterTemplateById(int printerTemplateId)
        {
            return PrinterTemplates.Single(x => x.Id == printerTemplateId);
        }

        public IEnumerable<PrinterTemplate> GetAllPrinterTemplates()
        {
            return Dao.Query<PrinterTemplate>();
        }

        public IEnumerable<string> GetPrinterNames()
        {
            return PrinterInfo.GetPrinterNames();
        }

        public IEnumerable<Printer> GetPrinters()
        {
            return Printers;
        }

        public override void Reset()
        {
            _printers = null;
            PrinterInfo.ResetCache();
        }

        private static PrinterMap GetPrinterMapForItem(IEnumerable<PrinterMap> printerMaps, Ticket ticket, int menuItemId)
        {
            var menuItemGroupCode = Dao.Single<MenuItem, string>(menuItemId, x => x.GroupCode);

            var maps = printerMaps;

            maps = maps.Count(x => !string.IsNullOrEmpty(x.TicketTag) && !string.IsNullOrEmpty(ticket.GetTagValue(x.TicketTag))) > 0
                ? maps.Where(x => !string.IsNullOrEmpty(x.TicketTag) && !string.IsNullOrEmpty(ticket.GetTagValue(x.TicketTag)))
                : maps.Where(x => string.IsNullOrEmpty(x.TicketTag));

            maps = maps.Count(x => x.DepartmentId == ticket.DepartmentId) > 0
                       ? maps.Where(x => x.DepartmentId == ticket.DepartmentId)
                       : maps.Where(x => x.DepartmentId == 0);

            maps = maps.Count(x => x.MenuItemGroupCode == menuItemGroupCode) > 0
                       ? maps.Where(x => x.MenuItemGroupCode == menuItemGroupCode)
                       : maps.Where(x => x.MenuItemGroupCode == null);

            maps = maps.Count(x => x.MenuItemId == menuItemId) > 0
                       ? maps.Where(x => x.MenuItemId == menuItemId)
                       : maps.Where(x => x.MenuItemId == 0);

            return maps.FirstOrDefault();
        }

        public void AutoPrintTicket(Ticket ticket)
        {
            foreach (var customPrinter in _applicationState.CurrentTerminal.PrintJobs.Where(x => !x.UseForPaidTickets))
            {
                if (ShouldAutoPrint(ticket, customPrinter))
                    ManualPrintTicket(ticket, customPrinter);
            }
        }

        public void ManualPrintTicket(Ticket ticket, PrintJob customPrinter)
        {
            Debug.Assert(!string.IsNullOrEmpty(ticket.TicketNumber));
            if (customPrinter.LocksTicket) ticket.RequestLock();
            ticket.AddPrintJob(customPrinter.Id);
            PrintOrders(customPrinter, ticket);
        }

        private static bool ShouldAutoPrint(Ticket ticket, PrintJob customPrinter)
        {
            if (customPrinter.WhenToPrint == (int)WhenToPrintTypes.Manual) return false;
            if (customPrinter.WhenToPrint == (int)WhenToPrintTypes.Paid)
            {
                if (ticket.DidPrintJobExecuted(customPrinter.Id)) return false;
                if (!ticket.IsPaid) return false;
                if (!customPrinter.AutoPrintIfCash && !customPrinter.AutoPrintIfCreditCard && !customPrinter.AutoPrintIfTicket) return false;
                if (customPrinter.AutoPrintIfCash && ticket.Payments.Count(x => x.PaymentType == (int)PaymentType.Cash) > 0) return true;
                if (customPrinter.AutoPrintIfCreditCard && ticket.Payments.Count(x => x.PaymentType == (int)PaymentType.CreditCard) > 0) return true;
                if (customPrinter.AutoPrintIfTicket && ticket.Payments.Count(x => x.PaymentType == (int)PaymentType.Ticket) > 0) return true;
            }
            if (customPrinter.WhenToPrint == (int)WhenToPrintTypes.NewLinesAdded && ticket.GetUnlockedOrders().Count() > 0) return true;
            return false;
        }

        public void PrintOrders(PrintJob printJob, Ticket ticket)
        {
            if (printJob.ExcludeTax)
            {
                ticket = ObjectCloner.Clone(ticket);
                ticket.Orders.ToList().ForEach(x => x.TaxIncluded = false);
            }

            IEnumerable<Order> ti;
            switch (printJob.WhatToPrint)
            {
                case (int)WhatToPrintTypes.NewLines:
                    ti = ticket.GetUnlockedOrders();
                    break;
                case (int)WhatToPrintTypes.GroupedByBarcode:
                    ti = GroupLinesByValue(ticket, x => x.Barcode ?? "", "1", true);
                    break;
                case (int)WhatToPrintTypes.GroupedByGroupCode:
                    ti = GroupLinesByValue(ticket, x => x.GroupCode ?? "", Resources.UndefinedWithBrackets);
                    break;
                case (int)WhatToPrintTypes.GroupedByTag:
                    ti = GroupLinesByValue(ticket, x => x.Tag ?? "", Resources.UndefinedWithBrackets);
                    break;
                case (int)WhatToPrintTypes.LastLinesByPrinterLineCount:
                    ti = ticket.Orders.OrderBy(x => x.Id).Take(1); // todo: make it configurable
                    break;
                default:
                    ti = ticket.Orders.OrderBy(x => x.Id).ToList();
                    break;
            }

            Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.ApplicationIdle,
                new Action(
                    delegate
                    {
                        try
                        {
                            InternalPrintOrders(printJob, ticket, ti);
                        }
                        catch (Exception e)
                        {
                            AppServices.LogError(e, "Yazdırma işlemi sırasında bir sorun meydana geldi. Lütfen yazıcı ve şablon ayarlarını kontrol ediniz.\r\n\r\nMesaj:\r\n" + e.Message);
                        }
                    }));
        }

        private IEnumerable<Order> GroupLinesByValue(Ticket ticket, Func<MenuItem, object> selector, string defaultValue, bool calcDiscounts = false)
        {
            var discounts = calcDiscounts ? ticket.GetDiscountAndRoundingTotal() : 0;
            var di = discounts > 0 ? discounts / ticket.GetPlainSum() : 0;
            var cache = new Dictionary<string, decimal>();
            foreach (var order in ticket.Orders.OrderBy(x => x.Id).ToList())
            {
                var item = order;
                var value = selector(_cacheService.GetMenuItem(x => x.Id == item.MenuItemId)).ToString();
                if (string.IsNullOrEmpty(value)) value = defaultValue;
                if (!cache.ContainsKey(value))
                    cache.Add(value, 0);
                var total = (item.GetTotal());
                cache[value] += Decimal.Round(total - (total * di), 2);
            }
            return cache.Select(x => new Order
            {
                MenuItemName = x.Key,
                Price = x.Value,
                Quantity = 1,
                PortionCount = 1
            });
        }

        private void InternalPrintOrders(PrintJob printJob, Ticket ticket, IEnumerable<Order> orders)
        {
            if (printJob.PrinterMaps.Count == 1
                && printJob.PrinterMaps[0].TicketTag == null
                && printJob.PrinterMaps[0].MenuItemId == 0
                && printJob.PrinterMaps[0].MenuItemGroupCode == null
                && printJob.PrinterMaps[0].DepartmentId == 0)
            {
                PrintOrderLines(ticket, orders, printJob.PrinterMaps[0]);
                return;
            }

            var ordersCache = new Dictionary<PrinterMap, IList<Order>>();

            foreach (var item in orders)
            {
                var p = GetPrinterMapForItem(printJob.PrinterMaps, ticket, item.MenuItemId);
                if (p != null)
                {
                    var lmap = p;
                    var pmap = ordersCache.SingleOrDefault(
                            x => x.Key.PrinterId == lmap.PrinterId && x.Key.PrinterTemplateId == lmap.PrinterTemplateId).Key;
                    if (pmap == null)
                        ordersCache.Add(p, new List<Order>());
                    else p = pmap;
                    ordersCache[p].Add(item);
                }
            }

            foreach (var order in ordersCache)
            {
                PrintOrderLines(ticket, order.Value, order.Key);
            }
        }

        private void PrintOrderLines(Ticket ticket, IEnumerable<Order> lines, PrinterMap p)
        {
            if (lines.Count() <= 0) return;
            if (p == null)
            {
                //todo: globalize
                MessageBox.Show("Yazdırma sırasında bir problem tespit edildi: Yazıcı Haritası null");
                AppServices.Log("Yazıcı Haritası NULL problemi tespit edildi.");
                return;
            }
            var printer = PrinterById(p.PrinterId);
            var prinerTemplate = PrinterTemplateById(p.PrinterTemplateId);
            if (printer == null || string.IsNullOrEmpty(printer.ShareName) || prinerTemplate == null) return;
            var ticketLines = TicketFormatter.GetFormattedTicket(ticket, lines, prinerTemplate);
            PrintJobFactory.CreatePrintJob(printer).DoPrint(ticketLines);
        }

        public void PrintReport(FlowDocument document)
        {
            var printer = _applicationState.CurrentTerminal.ReportPrinter;
            if (printer == null || string.IsNullOrEmpty(printer.ShareName)) return;
            PrintJobFactory.CreatePrintJob(printer).DoPrint(document);
        }

        public void PrintSlipReport(FlowDocument document)
        {
            var printer = _applicationState.CurrentTerminal.SlipReportPrinter;
            if (printer == null || string.IsNullOrEmpty(printer.ShareName)) return;
            PrintJobFactory.CreatePrintJob(printer).DoPrint(document);
        }

        public void ExecutePrintJob(PrintJob printJob)
        {
            if (printJob.PrinterMaps.Count > 0)
            {
                var printerMap = printJob.PrinterMaps[0];
                var printerTemplate = PrinterTemplates.Single(x => x.Id == printerMap.PrinterTemplateId);
                var content = printerTemplate
                    .HeaderTemplate
                    .Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
                PrintJobFactory.CreatePrintJob(PrinterById(printerMap.PrinterId)).DoPrint(content);
            }
        }

        public IDictionary<string, string> CreateTagDescriptions()
        {
            var result = new Dictionary<string, string>();
            result.Add(TagNames.TicketDate, Resources.TicketDate);
            result.Add(TagNames.TicketTime, Resources.TicketTime);
            result.Add(TagNames.Date, Resources.DayDate);
            result.Add(TagNames.Time, Resources.DayTime);
            result.Add(TagNames.TicketId, Resources.UniqueTicketId);
            result.Add(TagNames.TicketNo, Resources.TicketNumber);
            result.Add(TagNames.TicketTag, Resources.TicketTag);
            result.Add(TagNames.Department, Resources.DepartmentName);
            result.Add(TagNames.TicketTag2, Resources.OptionalTicketTag);
            result.Add(TagNames.LocationUser, Resources.LocationOrUserName);
            result.Add(TagNames.UserName, Resources.UserName);
            result.Add(TagNames.Location, Resources.LocationName);
            result.Add(TagNames.Note, Resources.TicketNote);
            result.Add(TagNames.AccName, Resources.AccountName);
            result.Add(TagNames.AccAddress, Resources.AccountAddress);
            result.Add(TagNames.AccPhone, Resources.AccountPhone);
            result.Add(TagNames.Quantity, Resources.LineItemQuantity);
            result.Add(TagNames.Name, Resources.LineItemName);
            result.Add(TagNames.Price, Resources.LineItemPrice);
            result.Add(TagNames.Cents, Resources.LineItemPriceCents);
            result.Add(TagNames.Total, Resources.LineItemTotal);
            result.Add(TagNames.TotalAmount, Resources.LineItemQuantity);
            result.Add(TagNames.LineAmount, Resources.LineItemTotalWithoutGifts);
            result.Add(TagNames.Properties, Resources.LineItemDetails);
            result.Add(TagNames.PropPrice, Resources.LineItemDetailPrice);
            result.Add(TagNames.PropQuantity, Resources.LineItemDetailQuantity);
            result.Add(TagNames.OrderNo, Resources.LineOrderNumber);
            result.Add(TagNames.PriceTag, Resources.LinePriceTag);
            result.Add(TagNames.TicketTotal, Resources.TicketTotal);
            result.Add(TagNames.PaymentTotal, Resources.TicketPaidTotal);
            result.Add(TagNames.PlainTotal, Resources.TicketSubTotal);
            result.Add(TagNames.DiscountTotal, Resources.DiscountTotal);
            result.Add(TagNames.TaxTotal, Resources.TaxTotal);
            result.Add(TagNames.TaxDetails, Resources.TotalsGroupedByTaxTemplate);
            result.Add(TagNames.ServiceTotal, Resources.ServiceTotal);
            result.Add(TagNames.ServiceDetails, Resources.TotalsGroupedByServiceTemplate);
            result.Add(TagNames.Balance, Resources.TicketRemainingAmount);
            result.Add(TagNames.IfPaid, Resources.RemainingAmountIfPaid);
            result.Add(TagNames.TotalText, Resources.TextWrittenTotalValue);
            result.Add(TagNames.IfDiscount, Resources.DiscountTotalAndTicketTotal);
            return result;
        }
    }

    public class PrinterTemplateDeleteValidator : SpecificationValidator<PrinterTemplate>
    {
        public override string GetErrorMessage(PrinterTemplate model)
        {
            if (Dao.Exists<PrinterMap>(x => x.PrinterTemplateId == model.Id))
                return Resources.DeleteErrorTemplateUsedInPrintJob;
            return "";
        }
    }

    public class PrinterDeleteValidator : SpecificationValidator<Printer>
    {
        public override string GetErrorMessage(Printer model)
        {
            if (Dao.Exists<Terminal>(x => x.ReportPrinter.Id == model.Id || x.SlipReportPrinter.Id == model.Id))
                return Resources.DeleteErrorPrinterAssignedToTerminal;
            if (Dao.Exists<PrinterMap>(x => x.PrinterId == model.Id))
                return Resources.DeleteErrorPrinterAssignedToPrinterMap;
            return "";
        }
    }
}
