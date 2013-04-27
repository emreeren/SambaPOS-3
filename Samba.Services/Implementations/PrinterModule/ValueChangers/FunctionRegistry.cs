using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using Samba.Domain.Models.Settings;
using Samba.Domain.Models.Tickets;
using Samba.Infrastructure.Settings;
using Samba.Localization.Properties;

namespace Samba.Services.Implementations.PrinterModule.ValueChangers
{
    [Export]
    public class FunctionRegistry
    {
        private readonly IAccountService _accountService;
        private readonly IDepartmentService _departmentService;
        private readonly ISettingService _settingService;
        private readonly ICacheService _cacheService;

        public IDictionary<Type, ArrayList> Functions = new Dictionary<Type, ArrayList>();
        public IDictionary<string, string> Descriptions = new Dictionary<string, string>();

        [ImportingConstructor]
        public FunctionRegistry(IAccountService accountService, IDepartmentService departmentService, ISettingService settingService, ICacheService cacheService)
        {
            _accountService = accountService;
            _departmentService = departmentService;
            _settingService = settingService;
            _cacheService = cacheService;
        }

        public void RegisterFunctions()
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
            RegisterFunction<Ticket>(TagNames.PlainTotal, (x, d) => x.GetPlainSum().ToString(LocalSettings.CurrencyFormat), Resources.TicketSubTotal, x => x.GetSum() != x.GetPlainSum());
            RegisterFunction<Ticket>(TagNames.DiscountTotal, (x, d) => x.GetPreTaxServicesTotal().ToString(LocalSettings.CurrencyFormat), Resources.DiscountTotal);
            RegisterFunction<Ticket>(TagNames.TaxTotal, (x, d) => x.CalculateTax(x.GetPlainSum(), x.GetPreTaxServicesTotal()).ToString(LocalSettings.CurrencyFormat), Resources.TaxTotal);
            RegisterFunction<Ticket>(TagNames.TicketTotal, (x, d) => x.GetSum().ToString(LocalSettings.CurrencyFormat), Resources.TicketTotal);
            RegisterFunction<Ticket>(TagNames.PaymentTotal, (x, d) => x.GetPaymentAmount().ToString(LocalSettings.CurrencyFormat), Resources.PaymentTotal);
            RegisterFunction<Ticket>(TagNames.Balance, (x, d) => x.GetRemainingAmount().ToString(LocalSettings.CurrencyFormat), Resources.Balance, x => x.GetRemainingAmount() != x.GetSum());
            RegisterFunction<Ticket>(TagNames.TotalText, (x, d) => HumanFriendlyInteger.CurrencyToWritten(x.GetSum()), Resources.TextWrittenTotalValue);
            RegisterFunction<Ticket>(TagNames.Totaltext, (x, d) => HumanFriendlyInteger.CurrencyToWritten(x.GetSum(), true), Resources.TextWrittenTotalValue);
            RegisterFunction<Ticket>("{TICKET TAG:([^}]+)}", (x, d) => x.GetTagValue(d), Resources.TicketTag);
            RegisterFunction<Ticket>("{TICKET STATE:([^}]+)}", (x, d) => x.GetStateStr(d), "Ticket State");
            RegisterFunction<Ticket>("{SETTING:([^}]+)}", (x, d) => _settingService.ReadSetting(d).StringValue, Resources.SettingValue);
            RegisterFunction<Ticket>("{CALCULATION TOTAL:([^}]+)}", (x, d) => x.GetCalculationTotal(d).ToString(LocalSettings.CurrencyFormat), "Calculation Total", x => x.Calculations.Count > 0);
            RegisterFunction<Ticket>("{ENTITY NAME:([^}]+)}", (x, d) => x.GetEntityName(_cacheService.GetEntityTypeIdByEntityName(d)), "Entity Name");
            RegisterFunction<Ticket>("{ORDER STATE TOTAL:([^}]+)}", (x, d) => x.GetOrderStateTotal(d).ToString(LocalSettings.CurrencyFormat), "Order State Total");
            RegisterFunction<Ticket>("{SERVICE TOTAL}", (x, d) => x.GetPostTaxServicesTotal().ToString(LocalSettings.CurrencyFormat), "Service Total");

            //ORDERS
            RegisterFunction<Order>(TagNames.Quantity, (x, d) => x.Quantity.ToString(LocalSettings.QuantityFormat), Resources.LineItemQuantity);
            RegisterFunction<Order>(TagNames.Name, (x, d) => x.MenuItemName + x.GetPortionDesc(), Resources.LineItemName);
            RegisterFunction<Order>(TagNames.Price, (x, d) => x.Price.ToString(LocalSettings.CurrencyFormat), Resources.LineItemPrice);
            RegisterFunction<Order>(TagNames.Total, (x, d) => x.GetPrice().ToString(LocalSettings.CurrencyFormat), Resources.LineItemTotal);
            RegisterFunction<Order>(TagNames.TotalAmount, (x, d) => x.GetValue().ToString(LocalSettings.CurrencyFormat), Resources.LineItemTotalAndQuantity);
            RegisterFunction<Order>(TagNames.Cents, (x, d) => (x.Price * 100).ToString(LocalSettings.QuantityFormat), Resources.LineItemPriceCents);
            RegisterFunction<Order>(TagNames.LineAmount, (x, d) => x.GetTotal().ToString(LocalSettings.CurrencyFormat), Resources.LineItemTotalWithoutGifts);
            RegisterFunction<Order>(TagNames.OrderNo, (x, d) => x.OrderNumber.ToString(), Resources.LineOrderNumber);
            RegisterFunction<Order>(TagNames.PriceTag, (x, d) => x.PriceTag, Resources.LinePriceTag);
            RegisterFunction<Order>("{ORDER TAG:([^}]+)}", (x, d) => x.GetOrderTagValue(d).TagValue, "Order Tag Value");
            RegisterFunction<Order>("{ORDER STATE:([^}]+)}", (x, d) => x.GetStateValue(d).StateValue, "Order State Value");
            RegisterFunction<Order>("{ORDER TAX RATE:([^}]+)}", (x, d) => x.GetTaxValue(d).TaxRate.ToString(LocalSettings.QuantityFormat), "Order Tax Rate");
            RegisterFunction<Order>("{ORDER TAX TEMPLATE NAMES}", (x, d) => string.Join(", ", x.GetTaxValues().Select(y => y.TaxTemplateName)), "Order Tax Template Names");

            //ORDER TAG VALUES
            RegisterFunction<OrderTagValue>(TagNames.OrderTagPrice, (x, d) => x.AddTagPriceToOrderPrice ? "" : x.Price.ToString(LocalSettings.CurrencyFormat), Resources.OrderTagPrice, x => x.Price != 0);
            RegisterFunction<OrderTagValue>(TagNames.OrderTagQuantity, (x, d) => x.Quantity.ToString(LocalSettings.QuantityFormat), Resources.OrderTagQuantity);
            RegisterFunction<OrderTagValue>(TagNames.OrderTagName, (x, d) => x.TagValue, Resources.OrderTagName, x => !string.IsNullOrEmpty(x.TagValue));

            //TICKET RESOURCES
            RegisterFunction<TicketEntity>("{ENTITY NAME}", (x, d) => x.EntityName, "Entity Name");
            RegisterFunction<TicketEntity>("{ENTITY BALANCE}", (x, d) => _accountService.GetAccountBalance(x.AccountId).ToString(LocalSettings.CurrencyFormat), "Entity Account Balance", x => x.AccountId > 0);
            RegisterFunction<TicketEntity>("{ENTITY DATA:([^}]+)}", (x, d) => x.GetCustomData(d), "Entity Data");

            //CALCULATIONS
            RegisterFunction<Calculation>("{CALCULATION NAME}", (x, d) => x.Name, "Calculation Name");
            RegisterFunction<Calculation>("{CALCULATION AMOUNT}", (x, d) => x.Amount.ToString(LocalSettings.QuantityFormat), "Caluculation Amount");
            RegisterFunction<Calculation>("{CALCULATION TOTAL}", (x, d) => x.CalculationAmount.ToString(LocalSettings.CurrencyFormat), "Calculation Total", x => x.CalculationAmount != 0);

            //PAYMENTS
            RegisterFunction<Payment>("{PAYMENT AMOUNT}", (x, d) => x.Amount.ToString(LocalSettings.CurrencyFormat), "Payment Amount", x => x.Amount > 0);
            RegisterFunction<Payment>("{PAYMENT NAME}", (x, d) => x.Name, "Payment Name");

            //CHANGE PAYMENTS
            RegisterFunction<ChangePayment>("{CHANGE PAYMENT AMOUNT}", (x, d) => x.Amount.ToString(LocalSettings.CurrencyFormat), "Change Payment Amount", x => x.Amount > 0);
            RegisterFunction<ChangePayment>("{CHANGE PAYMENT NAME}", (x, d) => x.Name, "Change Payment Name");

            //TAXES
            RegisterFunction<TaxValue>("{TAX AMOUNT}", (x, d) => x.TaxAmount.ToString(LocalSettings.CurrencyFormat), "Tax Amount", x => x.TaxAmount > 0);
            RegisterFunction<TaxValue>("{TAX RATE}", (x, d) => x.Amount.ToString(LocalSettings.QuantityFormat), "Tax Rate", x => x.Amount > 0);
            RegisterFunction<TaxValue>("{TAXABLE AMOUNT}", (x, d) => x.OrderAmount.ToString(LocalSettings.CurrencyFormat), "Taxable Amount", x => x.OrderAmount > 0);
            RegisterFunction<TaxValue>("{TOTAL TAXABLE AMOUNT}", (x, d) => x.TotalAmount.ToString(LocalSettings.CurrencyFormat), "Total Taxable Amount", x => x.TotalAmount > 0);
            RegisterFunction<TaxValue>("{TAX NAME}", (x, d) => x.Name, "Tax Template Name");
        }

        public void RegisterFunction<TModel>(string tag, Func<TModel, string, string> function, string desc = "", Func<TModel, bool> condition = null)
        {
            if (!Functions.ContainsKey(typeof(TModel)))
            {
                Descriptions.Add("-- " + UpperWhitespace(typeof(TModel).Name) + " Value Tags --", "");
                Functions.Add(typeof(TModel), new ArrayList());
            }
            Functions[typeof(TModel)].Add(new FunctionData<TModel> { Tag = tag, Func = function, Condition = condition });
            if (!string.IsNullOrEmpty(desc)) Descriptions.Add(tag.Replace(":([^}]+)", ":X}"), desc);
        }

        private static string UpperWhitespace(string value)
        {
            return string.Join("", value.Select(x => Char.IsUpper(x) ? " " + x : x.ToString())).Trim();
        }

        public string ExecuteFunctions<TModel>(string content, TModel model, PrinterTemplate printerTemplate)
        {
            if (!Functions.ContainsKey(typeof(TModel))) return content;
            return Functions[typeof(TModel)]
                .Cast<FunctionData<TModel>>()
                .Aggregate(content, (current, func) => (func.GetResult(model, current, printerTemplate)));
        }

        private string GetDepartmentName(int departmentId)
        {
            var dep = _departmentService.GetDepartment(departmentId);
            return dep != null ? dep.Name : Resources.UndefinedWithBrackets;
        }

    }
}
