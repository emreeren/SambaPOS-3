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
            RegisterFunction<Ticket>(TagNames.TaxDetails, (x, d) => GetTaxDetails(x.Orders, x.GetPlainSum(), x.GetPreTaxServicesTotal()));
            RegisterFunction<Ticket>(TagNames.CalculationDetails, (x, d) => GetServiceDetails(x));
            RegisterFunction<Ticket>(TagNames.TicketTotal, (x, d) => x.GetSum().ToString("#,#0.00"));
            RegisterFunction<Ticket>(TagNames.PaymentTotal, (x, d) => x.GetPaymentAmount().ToString("#,#0.00"));
            RegisterFunction<Ticket>(TagNames.Balance, (x, d) => x.GetRemainingAmount().ToString("#,#0.00"));
            RegisterFunction<Ticket>(TagNames.TotalText, (x, d) => HumanFriendlyInteger.CurrencyToWritten(x.GetSum()));
            RegisterFunction<Ticket>(TagNames.Totaltext, (x, d) => HumanFriendlyInteger.CurrencyToWritten(x.GetSum(), true));
            RegisterFunction<Ticket>("{TICKET TAG:([^}]+)}", (x, d) => x.GetTagValue(d));
            RegisterFunction<Ticket>("{SETTING:([^}]+)}", (x, d) => SettingService.ReadSetting(d).StringValue);
            RegisterFunction<Ticket>("{CALCULATION TOTAL:([^}]+)}", (x, d) => x.GetCalculationTotal(d).ToString("#,#0.00"), x => x.Calculations.Count > 0);
            RegisterFunction<Ticket>("{RESOURCE NAME:([^}]+)}", (x, d) => x.GetResourceName(CacheService.GetResourceTemplateIdByEntityName(d)));

            RegisterFunction<Calculation>("{CALCULATION NAME}", (x, d) => x.Name);
            RegisterFunction<Calculation>("{CALCULATION AMOUNT}", (x, d) => x.Amount.ToString("#,#0.##"));
            RegisterFunction<Calculation>("{CALCULATION RESULT}", (x, d) => x.CalculationAmount.ToString("#,#0.00"), x => x.CalculationAmount != 0);
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

        private static string GetServiceDetails(Ticket ticket)
        {
            var sb = new StringBuilder();
            foreach (var service in ticket.Calculations)
            {
                var lservice = service;
                var ts = SettingService.GetCalculationTemplateById(lservice.ServiceId);
                var tsTitle = ts != null ? ts.Name : Resources.UndefinedWithBrackets;
                sb.AppendLine("<J>" + tsTitle + ":|" + lservice.CalculationAmount.ToString("#,#0.00"));
            }
            return String.Join("\r", sb);
        }

        private static string GetTaxDetails(IEnumerable<Order> orders, decimal plainSum, decimal discount)
        {
            var sb = new StringBuilder();
            var groups = orders.Where(x => x.TaxTemplateId > 0).GroupBy(x => x.TaxTemplateId);
            foreach (var @group in groups)
            {
                var iGroup = @group;
                var tb = SettingService.GetTaxTemplateById(iGroup.Key);
                var tbTitle = tb != null ? tb.Name : Resources.UndefinedWithBrackets;
                var total = @group.Sum(x => x.TaxAmount * x.Quantity);
                if (discount > 0)
                {
                    total -= (total * discount) / plainSum;
                }
                if (total > 0) sb.AppendLine("<J>" + tbTitle + ":|" + total.ToString("#,#0.00"));
            }
            return String.Join("\r", sb);
        }
    }
}
