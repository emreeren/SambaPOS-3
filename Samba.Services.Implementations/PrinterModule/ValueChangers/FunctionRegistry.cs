using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Practices.ServiceLocation;
using Samba.Domain.Models.Settings;
using Samba.Domain.Models.Tickets;
using Samba.Localization.Properties;

namespace Samba.Services.Implementations.PrinterModule.ValueChangers
{
    public static class FunctionRegistry
    {
        private static readonly IAccountService AccountService = ServiceLocator.Current.GetInstance<IAccountService>();
        private static readonly IDepartmentService DepartmentService = ServiceLocator.Current.GetInstance<IDepartmentService>();
        private static readonly ISettingService SettingService = ServiceLocator.Current.GetInstance<ISettingService>();
        private static readonly ICacheService CacheService = ServiceLocator.Current.GetInstance<ICacheService>();

        public static IDictionary<Type, ArrayList> Functions = new Dictionary<Type, ArrayList>();

        static FunctionRegistry()
        {
            //ORDER TAG VALUES
            RegisterFunction<OrderTagValue>(TagNames.OrderTagPrice, (x, d) => x.AddTagPriceToOrderPrice ? "" : x.Price.ToString("#,#0.00"), x => x.Price != 0);
            RegisterFunction<OrderTagValue>(TagNames.OrderTagQuantity, (x, d) => x.Quantity.ToString("#.##"));
            RegisterFunction<OrderTagValue>(TagNames.OrderTagName, (x, d) => x.Name, x => !string.IsNullOrEmpty(x.Name));

            //ORDERS
            RegisterFunction<Order>(TagNames.Quantity, (x, d) => x.Quantity.ToString("#,#0.##"));
            RegisterFunction<Order>(TagNames.Name, (x, d) => x.MenuItemName + x.GetPortionDesc());
            RegisterFunction<Order>(TagNames.Price, (x, d) => x.Price.ToString("#,#0.00"));
            RegisterFunction<Order>(TagNames.Total, (x, d) => x.GetItemPrice().ToString("#,#0.00"));
            RegisterFunction<Order>(TagNames.TotalAmount, (x, d) => x.GetItemValue().ToString("#,#0.00"));
            RegisterFunction<Order>(TagNames.Cents, (x, d) => (x.Price * 100).ToString("#,##"));
            RegisterFunction<Order>(TagNames.LineAmount, (x, d) => x.GetTotal().ToString("#,#0.00"));
            RegisterFunction<Order>(TagNames.OrderNo, (x, d) => x.OrderNumber.ToString());
            RegisterFunction<Order>(TagNames.PriceTag, (x, d) => x.PriceTag);
            RegisterFunction<Order>("{ORDER STATE NAME}", (x, d) => x.OrderStateGroupName);
            RegisterFunction<Order>("{ORDER TAG:([^}]+)}", (x, d) => x.GetOrderTagValue(d));

            //TICKET RESOURCES
            RegisterFunction<TicketResource>("{RESOURCE NAME}", (x, d) => x.ResourceName);
            RegisterFunction<TicketResource>("{RESOURCE BALANCE}", (x, d) => AccountService.GetAccountBalance(x.AccountId).ToString("#,#0.00"), x => x.AccountId > 0);
            RegisterFunction<TicketResource>("{RESOURCE DATA:([^}]+)}", (x, d) => x.GetCustomData(d));

            //TICKETS
            RegisterFunction<Ticket>(TagNames.TicketDate, (x, d) => x.Date.ToShortDateString());
            RegisterFunction<Ticket>(TagNames.TicketTime, (x, d) => x.Date.ToShortTimeString());
            RegisterFunction<Ticket>(TagNames.Date, (x, d) => DateTime.Now.ToShortDateString());
            RegisterFunction<Ticket>(TagNames.Time, (x, d) => DateTime.Now.ToShortTimeString());
            RegisterFunction<Ticket>(TagNames.TicketId, (x, d) => x.Id.ToString());
            RegisterFunction<Ticket>(TagNames.TicketNo, (x, d) => x.TicketNumber);
            RegisterFunction<Ticket>(TagNames.OrderNo, (x, d) => x.Orders.Last().OrderNumber.ToString());
            RegisterFunction<Ticket>(TagNames.UserName, (x, d) => x.Orders.Last().CreatingUserName);
            RegisterFunction<Ticket>(TagNames.Department, (x, d) => GetDepartmentName(x.DepartmentId));
            RegisterFunction<Ticket>(TagNames.Note, (x, d) => x.Note);
            RegisterFunction<Ticket>(TagNames.PlainTotal, (x, d) => x.GetPlainSum().ToString("#,#0.00"), x => x.GetSum() != x.GetPlainSum());
            RegisterFunction<Ticket>(TagNames.DiscountTotal, (x, d) => x.GetPreTaxServicesTotal().ToString("#,#0.00"));
            RegisterFunction<Ticket>(TagNames.TaxTotal, (x, d) => x.CalculateTax(x.GetPlainSum(), x.GetPreTaxServicesTotal()).ToString("#,#0.00"));
            RegisterFunction<Ticket>(TagNames.TicketTotal, (x, d) => x.GetSum().ToString("#,#0.00"));
            RegisterFunction<Ticket>(TagNames.PaymentTotal, (x, d) => x.GetPaymentAmount().ToString("#,#0.00"));
            RegisterFunction<Ticket>(TagNames.Balance, (x, d) => x.GetRemainingAmount().ToString("#,#0.00"), x => x.GetRemainingAmount() != x.GetSum());
            RegisterFunction<Ticket>(TagNames.TotalText, (x, d) => HumanFriendlyInteger.CurrencyToWritten(x.GetSum()));
            RegisterFunction<Ticket>(TagNames.Totaltext, (x, d) => HumanFriendlyInteger.CurrencyToWritten(x.GetSum(), true));
            RegisterFunction<Ticket>("{TICKET TAG:([^}]+)}", (x, d) => x.GetTagValue(d));
            RegisterFunction<Ticket>("{SETTING:([^}]+)}", (x, d) => SettingService.ReadSetting(d).StringValue);
            RegisterFunction<Ticket>("{CALCULATION TOTAL:([^}]+)}", (x, d) => x.GetCalculationTotal(d).ToString("#,#0.00"), x => x.Calculations.Count > 0);
            RegisterFunction<Ticket>("{RESOURCE NAME:([^}]+)}", (x, d) => x.GetResourceName(CacheService.GetResourceTemplateIdByEntityName(d)));
            RegisterFunction<Ticket>("{ORDER STATE TOTAL:([^}]+)}", (x, d) => x.GetOrderStateTotal(d).ToString("#,#0.00"));
            RegisterFunction<Ticket>("{SERVICE TOTAL}", (x, d) => x.GetPostTaxServicesTotal().ToString("#,#0.00"));

            //CALCULATIONS
            RegisterFunction<Calculation>("{CALCULATION NAME}", (x, d) => x.Name);
            RegisterFunction<Calculation>("{CALCULATION AMOUNT}", (x, d) => x.Amount.ToString("#,#0.##"));
            RegisterFunction<Calculation>("{CALCULATION TOTAL}", (x, d) => x.CalculationAmount.ToString("#,#0.00"), x => x.CalculationAmount != 0);

            //PAYMENTS
            RegisterFunction<Payment>("{PAYMENT AMOUNT}", (x, d) => x.Amount.ToString("#,#0.00"), x => x.Amount > 0);
            RegisterFunction<Payment>("{PAYMENT NAME}", (x, d) => x.Name);

            //TAXES
            RegisterFunction<TaxValue>("{TAX AMOUNT}", (x, d) => x.TaxAmount.ToString("#,#0.00"), x => x.TaxAmount > 0);
            RegisterFunction<TaxValue>("{TAX RATE}", (x, d) => x.Amount.ToString("#,#0.##"), x => x.Amount > 0);
            RegisterFunction<TaxValue>("{TAXABLE AMOUNT}", (x, d) => x.OrderAmount.ToString("#,#0.00"), x => x.OrderAmount > 0);
            RegisterFunction<TaxValue>("{TAX NAME}", (x, d) => x.Name);
        }

        public static void RegisterFunction<TModel>(string tag, Func<TModel, string, string> function, Func<TModel, bool> condition = null)
        {
            if (!Functions.ContainsKey(typeof(TModel)))
            {
                Functions.Add(typeof(TModel), new ArrayList());
            }
            Functions[typeof(TModel)].Add(new FunctionData<TModel> { Tag = tag, Func = function, Condition = condition });
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
