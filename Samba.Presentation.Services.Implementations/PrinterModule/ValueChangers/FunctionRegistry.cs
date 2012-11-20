using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Practices.ServiceLocation;
using Samba.Domain.Models.Settings;
using Samba.Domain.Models.Tickets;
using Samba.Localization.Properties;
using Samba.Services;

namespace Samba.Presentation.Services.Implementations.PrinterModule.ValueChangers
{
    public static class FunctionRegistry
    {
        private static readonly IAccountService AccountService = ServiceLocator.Current.GetInstance<IAccountService>();
        private static readonly IDepartmentService DepartmentService = ServiceLocator.Current.GetInstance<IDepartmentService>();
        private static readonly ISettingService SettingService = ServiceLocator.Current.GetInstance<ISettingService>();
        private static readonly IPresentationCacheService CacheService = ServiceLocator.Current.GetInstance<IPresentationCacheService>();

        public static IDictionary<Type, ArrayList> Functions = new Dictionary<Type, ArrayList>();
        public static IDictionary<string, string> Descriptions = new Dictionary<string, string>();

        static FunctionRegistry()
        {
            //TICKETS
            RegisterFunction<Ticket>(TagNames.TicketDate, (x, d) => x.Date.ToShortDateString(), Resources.TicketDate);
            RegisterFunction<Ticket>(TagNames.TicketTime, (x, d) => x.Date.ToShortTimeString(), Resources.TicketTime);
            RegisterFunction<Ticket>(TagNames.Date, (x, d) => DateTime.Now.ToShortDateString(), Resources.DayDate);
            RegisterFunction<Ticket>(TagNames.Time, (x, d) => DateTime.Now.ToShortTimeString(), Resources.DayTime);
            RegisterFunction<Ticket>(TagNames.TicketId, (x, d) => x.Id.ToString(), Resources.UniqueTicketId);
            RegisterFunction<Ticket>(TagNames.TicketNo, (x, d) => x.TicketNumber, Resources.TicketNumber);
            //RegisterFunction<Ticket>(TagNames.OrderNo, (x, d) => x.Orders.Last().OrderNumber.ToString(), Resources.LineOrderNumber);
            RegisterFunction<Ticket>(TagNames.UserName, (x, d) => x.Orders.Last().CreatingUserName, Resources.UserName);
            RegisterFunction<Ticket>(TagNames.Department, (x, d) => GetDepartmentName(x.DepartmentId), Resources.Department);
            RegisterFunction<Ticket>(TagNames.Note, (x, d) => x.Note, Resources.TicketNote);
            RegisterFunction<Ticket>(TagNames.PlainTotal, (x, d) => x.GetPlainSum().ToString("#,#0.00"), Resources.TicketSubTotal, x => x.GetSum() != x.GetPlainSum());
            RegisterFunction<Ticket>(TagNames.DiscountTotal, (x, d) => x.GetPreTaxServicesTotal().ToString("#,#0.00"), Resources.DiscountTotal);
            RegisterFunction<Ticket>(TagNames.TaxTotal, (x, d) => x.CalculateTax(x.GetPlainSum(), x.GetPreTaxServicesTotal()).ToString("#,#0.00"), Resources.TaxTotal);
            RegisterFunction<Ticket>(TagNames.TicketTotal, (x, d) => x.GetSum().ToString("#,#0.00"), Resources.TicketTotal);
            RegisterFunction<Ticket>(TagNames.PaymentTotal, (x, d) => x.GetPaymentAmount().ToString("#,#0.00"), Resources.PaymentTotal);
            RegisterFunction<Ticket>(TagNames.Balance, (x, d) => x.GetRemainingAmount().ToString("#,#0.00"), Resources.Balance, x => x.GetRemainingAmount() != x.GetSum());
            RegisterFunction<Ticket>(TagNames.TotalText, (x, d) => HumanFriendlyInteger.CurrencyToWritten(x.GetSum()), Resources.TextWrittenTotalValue);
            RegisterFunction<Ticket>(TagNames.Totaltext, (x, d) => HumanFriendlyInteger.CurrencyToWritten(x.GetSum(), true), Resources.TextWrittenTotalValue);
            RegisterFunction<Ticket>("{TICKET TAG:([^}]+)}", (x, d) => x.GetTagValue(d), Resources.TicketTag);
            RegisterFunction<Ticket>("{SETTING:([^}]+)}", (x, d) => SettingService.ReadSetting(d).StringValue, Resources.SettingValue);
            RegisterFunction<Ticket>("{CALCULATION TOTAL:([^}]+)}", (x, d) => x.GetCalculationTotal(d).ToString("#,#0.00"), "Calculation Total", x => x.Calculations.Count > 0);
            RegisterFunction<Ticket>("{RESOURCE NAME:([^}]+)}", (x, d) => x.GetResourceName(CacheService.GetResourceTypeIdByEntityName(d)), "Resource Name");
            RegisterFunction<Ticket>("{ORDER STATE TOTAL:([^}]+)}", (x, d) => x.GetOrderStateTotal(d).ToString("#,#0.00"), "Order State Total");
            RegisterFunction<Ticket>("{SERVICE TOTAL}", (x, d) => x.GetPostTaxServicesTotal().ToString("#,#0.00"), "Service Total");

            //ORDERS
            RegisterFunction<Order>(TagNames.Quantity, (x, d) => x.Quantity.ToString("#,#0.##"), Resources.LineItemQuantity);
            RegisterFunction<Order>(TagNames.Name, (x, d) => x.MenuItemName + x.GetPortionDesc(), Resources.LineItemName);
            RegisterFunction<Order>(TagNames.Price, (x, d) => x.Price.ToString("#,#0.00"), Resources.LineItemPrice);
            RegisterFunction<Order>(TagNames.Total, (x, d) => x.GetItemPrice().ToString("#,#0.00"), Resources.LineItemTotal);
            RegisterFunction<Order>(TagNames.TotalAmount, (x, d) => x.GetItemValue().ToString("#,#0.00"), Resources.LineItemTotalAndQuantity);
            RegisterFunction<Order>(TagNames.Cents, (x, d) => (x.Price * 100).ToString("#,##"), Resources.LineItemPriceCents);
            RegisterFunction<Order>(TagNames.LineAmount, (x, d) => x.GetTotal().ToString("#,#0.00"), Resources.LineItemTotalWithoutGifts);
            RegisterFunction<Order>(TagNames.OrderNo, (x, d) => x.OrderNumber.ToString(), Resources.LineOrderNumber);
            RegisterFunction<Order>(TagNames.PriceTag, (x, d) => x.PriceTag, Resources.LinePriceTag);
            RegisterFunction<Order>("{ORDER STATE NAME}", (x, d) => x.OrderStateGroupName, "Order State Name");
            RegisterFunction<Order>("{ORDER TAG:([^}]+)}", (x, d) => x.GetOrderTagValue(d).TagValue, "Order Tag Value");

            //ORDER TAG VALUES
            RegisterFunction<OrderTagValue>(TagNames.OrderTagPrice, (x, d) => x.AddTagPriceToOrderPrice ? "" : x.Price.ToString("#,#0.00"), Resources.OrderTagPrice, x => x.Price != 0);
            RegisterFunction<OrderTagValue>(TagNames.OrderTagQuantity, (x, d) => x.Quantity.ToString("#.##"), Resources.OrderTagQuantity);
            RegisterFunction<OrderTagValue>(TagNames.OrderTagName, (x, d) => x.TagValue, Resources.OrderTagName, x => !string.IsNullOrEmpty(x.TagValue));

            //TICKET RESOURCES
            RegisterFunction<TicketResource>("{RESOURCE NAME}", (x, d) => x.ResourceName, "Resource Name");
            RegisterFunction<TicketResource>("{RESOURCE BALANCE}", (x, d) => AccountService.GetAccountBalance(x.AccountId).ToString("#,#0.00"), "Resource Account Balance", x => x.AccountId > 0);
            RegisterFunction<TicketResource>("{RESOURCE DATA:([^}]+)}", (x, d) => x.GetCustomData(d), "Resource Data");

            //CALCULATIONS
            RegisterFunction<Calculation>("{CALCULATION NAME}", (x, d) => x.Name, "Calculation Name");
            RegisterFunction<Calculation>("{CALCULATION AMOUNT}", (x, d) => x.Amount.ToString("#,#0.##"), "Caluculation Amount");
            RegisterFunction<Calculation>("{CALCULATION TOTAL}", (x, d) => x.CalculationAmount.ToString("#,#0.00"), "Calculation Total", x => x.CalculationAmount != 0);

            //PAYMENTS
            RegisterFunction<Payment>("{PAYMENT AMOUNT}", (x, d) => x.Amount.ToString("#,#0.00"), "Payment Amount", x => x.Amount > 0);
            RegisterFunction<Payment>("{PAYMENT NAME}", (x, d) => x.Name, "Payment Name");

            //CHANGE PAYMENTS
            RegisterFunction<ChangePayment>("{CHANGE PAYMENT AMOUNT}", (x, d) => x.Amount.ToString("#,#0.00"), "Change Payment Amount", x => x.Amount > 0);
            RegisterFunction<ChangePayment>("{CHANGE PAYMENT NAME}", (x, d) => x.Name, "Change Payment Name");

            //TAXES
            RegisterFunction<TaxValue>("{TAX AMOUNT}", (x, d) => x.TaxAmount.ToString("#,#0.00"), "Tax Amount", x => x.TaxAmount > 0);
            RegisterFunction<TaxValue>("{TAX RATE}", (x, d) => x.Amount.ToString("#,#0.##"), "Tax Rate", x => x.Amount > 0);
            RegisterFunction<TaxValue>("{TAXABLE AMOUNT}", (x, d) => x.OrderAmount.ToString("#,#0.00"), "Taxable Amount Total", x => x.OrderAmount > 0);
            RegisterFunction<TaxValue>("{TAX NAME}", (x, d) => x.Name, "Tax Template Name");
        }

        public static void RegisterFunction<TModel>(string tag, Func<TModel, string, string> function, string desc = "", Func<TModel, bool> condition = null)
        {
            if (!Functions.ContainsKey(typeof(TModel)))
            {
                Descriptions.Add("-- " + typeof(TModel).Name.UpperWhitespace() + " Value Tags --", "");
                Functions.Add(typeof(TModel), new ArrayList());
            }
            Functions[typeof(TModel)].Add(new FunctionData<TModel> { Tag = tag, Func = function, Condition = condition });
            if (!string.IsNullOrEmpty(desc)) Descriptions.Add(tag.Replace(":([^}]+)", ":X}"), desc);
        }

        private static string UpperWhitespace(this string value)
        {
            return string.Join("", value.Select(x => Char.IsUpper(x) ? " " + x : x.ToString())).Trim();
        }

        public static string ExecuteFunctions<TModel>(string content, TModel model, PrinterTemplate printerTemplate)
        {
            if (!Functions.ContainsKey(typeof(TModel))) return content;
            return Functions[typeof(TModel)]
                .Cast<FunctionData<TModel>>()
                .Aggregate(content, (current, func) => (func.GetResult(model, current, printerTemplate)));
        }

        private static string GetDepartmentName(int departmentId)
        {
            var dep = DepartmentService.GetDepartment(departmentId);
            return dep != null ? dep.Name : Resources.UndefinedWithBrackets;
        }

    }
}
