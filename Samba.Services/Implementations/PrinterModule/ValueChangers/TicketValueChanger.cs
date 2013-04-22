using System.Collections.Generic;
using System.Linq;
using Samba.Domain.Models.Settings;
using Samba.Domain.Models.Tickets;

namespace Samba.Services.Implementations.PrinterModule.ValueChangers
{
    public class TicketValueChanger : AbstractValueChanger<Ticket>
    {
        private static readonly EntityValueChanger ResourceValueChanger = new EntityValueChanger();
        private static readonly PreCalculationValueChanger PreCalculationValueChanger = new PreCalculationValueChanger();
        private static readonly PostCalculationValueChanger PostCalculationValueChanger = new PostCalculationValueChanger();
        private static readonly PaymentValueChanger PaymentValueChanger = new PaymentValueChanger();
        private static readonly ChangePaymentValueChanger ChangePaymentValueChanger = new ChangePaymentValueChanger();
        private static readonly OrderValueChanger OrderValueChanger = new OrderValueChanger();
        private static readonly TaxValueChanger TaxValueChanger = new TaxValueChanger();

        public override string GetTargetTag()
        {
            return "LAYOUT";
        }

        protected override string ReplaceTemplateValues(string templatePart, Ticket model, PrinterTemplate template)
        {
            var result = PreCalculationValueChanger.Replace(template, templatePart, model.Calculations.Where(x => !x.IncludeTax));
            result = PostCalculationValueChanger.Replace(template, result, model.Calculations.Where(x => x.IncludeTax));
            result = PaymentValueChanger.Replace(template, result, model.Payments);
            result = ChangePaymentValueChanger.Replace(template, result, model.ChangePayments);
            result = ResourceValueChanger.Replace(template, result, model.TicketEntities);
            result = TaxValueChanger.Replace(template, result, GetTaxValues(model));
            result = OrderValueChanger.Replace(template, result, model.Orders);
            return result;

        }

        internal IEnumerable<TaxValue> GetTaxValues(Ticket ticket)
        {
            var taxValues = ticket.Orders.SelectMany(x => x.GetTaxValues())
                      .GroupBy(x => new { x.TaxTempleteAccountTransactionTypeId, x.TaxTemplateName }).ToList();
            var totalTax = ticket.GetTaxTotal();
            return taxValues.Select(x => new TaxValue
                {
                    TotalTax = totalTax,
                    TaxIncluded = ticket.TaxIncluded,
                    Name = x.Key.TaxTemplateName,
                    Amount = x.Average(y => y.TaxRate),
                    OrderTotal = ticket.GetSum(x.Key.TaxTempleteAccountTransactionTypeId),
                    TaxAmount =
                        ticket.GetTaxTotal(x.Key.TaxTempleteAccountTransactionTypeId, ticket.GetPreTaxServicesTotal(),
                                           ticket.GetPlainSum()),
                });
        }
    }
}
