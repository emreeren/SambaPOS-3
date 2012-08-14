using System.Linq;
using Samba.Domain.Models.Settings;
using Samba.Domain.Models.Tickets;

namespace Samba.Services.Implementations.PrinterModule.ValueChangers
{
    public class TicketValueChanger : AbstractValueChanger<Ticket>
    {
        private static readonly ResourceValueChanger ResourceValueChanger = new ResourceValueChanger();
        private static readonly CalculationValueChanger CalculationValueChanger = new CalculationValueChanger();
        private static readonly PaymentValueChanger PaymentValueChanger = new PaymentValueChanger();
        private static readonly OrderValueChanger OrderValueChanger = new OrderValueChanger();
        private static readonly TaxValueChanger TaxValueChanger = new TaxValueChanger();

        public override string GetTargetTag()
        {
            return "LAYOUT";
        }

        protected override string ReplaceTemplateValues(string templatePart, Ticket model, PrinterTemplate template)
        {
            var result = CalculationValueChanger.Replace(template, templatePart, model.Calculations);
            result = PaymentValueChanger.Replace(template, result, model.Payments);
            result = ResourceValueChanger.Replace(template, result, model.TicketResources);
            result = TaxValueChanger.Replace(template, result, model.Orders.GroupBy(x => x.TaxTemplateName).Select(x => new TaxValue { Name = x.Key, Amount = x.Average(y => y.TaxRate), OrderAmount = x.Sum(y => y.GetItemValue()), TaxAmount = x.Sum(y => y.Quantity * y.TaxAmount) }));
            result = OrderValueChanger.Replace(template, result, model.Orders);
            return result;
        }
    }
}
