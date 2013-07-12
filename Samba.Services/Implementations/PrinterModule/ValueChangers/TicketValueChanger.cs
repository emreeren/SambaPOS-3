using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using Samba.Domain.Models.Settings;
using Samba.Domain.Models.Tickets;

namespace Samba.Services.Implementations.PrinterModule.ValueChangers
{
    [Export]
    public class TicketValueChanger : AbstractValueChanger<Ticket>
    {
        private readonly TicketEntityValueChanger _entityValueChanger;
        private readonly PreCalculationValueChanger _preCalculationValueChanger;
        private readonly PostCalculationValueChanger _postCalculationValueChanger;
        private readonly PaymentValueChanger _paymentValueChanger;
        private readonly ChangePaymentValueChanger _changePaymentValueChanger;
        private readonly OrderValueChanger _orderValueChanger;
        private readonly TaxValueChanger _taxValueChanger;

        [ImportingConstructor]
        public TicketValueChanger(TicketEntityValueChanger entityValueChanger, PreCalculationValueChanger preCalculationValueChanger, PostCalculationValueChanger postCalculationValueChanger,
            PaymentValueChanger paymentValueChanger, ChangePaymentValueChanger changePaymentValueChanger, OrderValueChanger orderValueChanger, TaxValueChanger taxValueChanger)
        {
            _entityValueChanger = entityValueChanger;
            _preCalculationValueChanger = preCalculationValueChanger;
            _postCalculationValueChanger = postCalculationValueChanger;
            _paymentValueChanger = paymentValueChanger;
            _changePaymentValueChanger = changePaymentValueChanger;
            _orderValueChanger = orderValueChanger;
            _taxValueChanger = taxValueChanger;
        }

        public override string GetTargetTag()
        {
            return "LAYOUT";
        }

        protected override string ReplaceTemplateValues(string templatePart, Ticket model, PrinterTemplate template)
        {
            var result = _preCalculationValueChanger.Replace(template, templatePart, model.Calculations.Where(x => !x.IncludeTax));
            result = _postCalculationValueChanger.Replace(template, result, model.Calculations.Where(x => x.IncludeTax));
            result = _paymentValueChanger.Replace(template, result, model.Payments);
            result = _changePaymentValueChanger.Replace(template, result, model.ChangePayments);
            result = _entityValueChanger.Replace(template, result, model.TicketEntities);
            result = _taxValueChanger.Replace(template, result, GetTaxValues(model));
            result = _orderValueChanger.Replace(template, result, model.Orders);
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
