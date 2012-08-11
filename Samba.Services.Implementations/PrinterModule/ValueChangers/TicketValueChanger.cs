using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Samba.Domain.Models.Settings;
using Samba.Domain.Models.Tickets;
using Samba.Localization.Properties;

namespace Samba.Services.Implementations.PrinterModule.ValueChangers
{
    public class TicketValueChanger : AbstractValueChanger<Ticket>
    {
        public override string GetTargetTag()
        {
            return "LAYOUT";
        }

        protected override string GetModelName(Ticket model)
        {
            return "";
        }

        protected override string ReplaceValues(string templatePart, Ticket model, PrinterTemplate template)
        {
            var unlockedOrder = model.Orders.Last();
            var orderNo = unlockedOrder != null ? unlockedOrder.OrderNumber : 0;
            var userName = unlockedOrder != null ? unlockedOrder.CreatingUserName : "";

            string result = templatePart;
            if (string.IsNullOrEmpty(result)) return "";

            result = FormatData(result, TagNames.TicketDate, () => model.Date.ToShortDateString());
            result = FormatData(result, TagNames.TicketTime, () => model.Date.ToShortTimeString());
            result = FormatData(result, TagNames.Date, () => DateTime.Now.ToShortDateString());
            result = FormatData(result, TagNames.Time, () => DateTime.Now.ToShortTimeString());
            result = FormatData(result, TagNames.TicketId, () => model.Id.ToString());
            result = FormatData(result, TagNames.TicketNo, () => model.TicketNumber);
            result = FormatData(result, TagNames.OrderNo, orderNo.ToString);
            result = FormatData(result, TagNames.TicketTag, model.GetTagData);
            result = FormatDataIf(true, result, TagNames.Department, () => GetDepartmentName(model.DepartmentId));

            const string ticketTagPattern = TagNames.TicketTag2 + "[^}]+}";

            while (Regex.IsMatch(result, ticketTagPattern))
            {
                var value = Regex.Match(result, ticketTagPattern).Groups[0].Value;
                var tags = "";
                try
                {
                    var tag = value.Trim('{', '}').Split(':')[1];
                    tags = tag.Split(',').Aggregate(tags, (current, t) => current +
                        (!string.IsNullOrEmpty(model.GetTagValue(t.Trim()))
                        ? (t + ": " + model.GetTagValue(t.Trim()) + "\r")
                        : ""));
                    result = FormatData(result.Trim('\r'), value, () => tags);
                }
                catch (Exception)
                {
                    result = FormatData(result, value, () => "");
                }
            }

            const string ticketTag2Pattern = TagNames.TicketTag3 + "[^}]+}";

            while (Regex.IsMatch(result, ticketTag2Pattern))
            {
                var value = Regex.Match(result, ticketTag2Pattern).Groups[0].Value;
                var tag = value.Trim('{', '}').Split(':')[1];
                var tagValue = model.GetTagValue(tag);
                try
                {
                    result = FormatData(result, value, () => tagValue);
                }
                catch (Exception)
                {
                    result = FormatData(result, value, () => "");
                }
            }

            var title = userName;

            result = FormatData(result, TagNames.LocationUser, () => title);
            result = FormatData(result, TagNames.UserName, () => userName);
            result = FormatData(result, TagNames.Note, () => model.Note);
            result = FormatData(result, TagNames.AccName, () => model.AccountName);


            var payment = model.GetPaymentAmount();
            var remaining = model.GetRemainingAmount();
            var plainTotal = model.GetPlainSum();
            var preTaxServices = model.GetPreTaxServicesTotal();
            var taxAmount = model.CalculateTax(plainTotal, preTaxServices);  //GetTaxTotal(ticket.Orders, plainTotal, ticket.GetDiscountTotal());
            var servicesTotal = model.GetPostTaxServicesTotal();
            //ticket.CalculateTax(plainTotal, preTaxServices);

            result = FormatDataIf(taxAmount > 0 || preTaxServices > 0 || servicesTotal > 0, result, TagNames.PlainTotal, () => plainTotal.ToString("#,#0.00"));
            result = FormatDataIf(preTaxServices > 0, result, TagNames.DiscountTotal, () => preTaxServices.ToString("#,#0.00"));
            result = FormatDataIf(taxAmount > 0, result, TagNames.TaxTotal, () => taxAmount.ToString("#,#0.00"));
            result = FormatDataIf(taxAmount > 0, result, TagNames.TaxDetails, () => GetTaxDetails(model.Orders, plainTotal, preTaxServices));
            result = FormatDataIf(servicesTotal > 0, result, TagNames.CalculationDetails, () => GetServiceDetails(model));

            result = FormatDataIf(payment > 0, result, TagNames.IfPaid,
                () => string.Format(Resources.RemainingAmountIfPaidValue_f, payment.ToString("#,#0.00"), remaining.ToString("#,#0.00")));

            result = FormatDataIf(preTaxServices > 0, result, TagNames.IfDiscount,
                () => string.Format(Resources.DiscountTotalAndTicketTotalValue_f, (plainTotal).ToString("#,#0.00"), preTaxServices.ToString("#,#0.00")));

            result = FormatDataIf(preTaxServices < 0, result, TagNames.IfFlatten, () => string.Format(Resources.IfNegativeDiscountValue_f, preTaxServices.ToString("#,#0.00")));

            result = FormatData(result, TagNames.TicketTotal, () => model.GetSum().ToString("#,#0.00"));
            result = FormatData(result, TagNames.PaymentTotal, () => model.GetPaymentAmount().ToString("#,#0.00"));
            result = FormatData(result, TagNames.Balance, () => model.GetRemainingAmount().ToString("#,#0.00"));

            result = FormatData(result, TagNames.TotalText, () => HumanFriendlyInteger.CurrencyToWritten(model.GetSum()));
            result = FormatData(result, TagNames.Totaltext, () => HumanFriendlyInteger.CurrencyToWritten(model.GetSum(), true));

            return result;
        }

        private string GetDepartmentName(int departmentId)
        {
            var dep = DepartmentService.GetDepartment(departmentId);
            return dep != null ? dep.Name : Resources.UndefinedWithBrackets;
        }

        private string GetServiceDetails(Ticket ticket)
        {
            var sb = new StringBuilder();
            foreach (var service in ticket.Calculations)
            {
                var lservice = service;
                var ts = SettingService.GetCalculationTemplateById(lservice.ServiceId);
                var tsTitle = ts != null ? ts.Name : Resources.UndefinedWithBrackets;
                sb.AppendLine("<J>" + tsTitle + ":|" + lservice.CalculationAmount.ToString("#,#0.00"));
            }
            return string.Join("\r", sb);
        }

        private string GetTaxDetails(IEnumerable<Order> orders, decimal plainSum, decimal discount)
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
            return string.Join("\r", sb);
        }
    }
}
