using System.Linq;
using Samba.Domain.Models.Settings;
using Samba.Domain.Models.Tickets;

namespace Samba.Services.Implementations.PrinterModule.ValueChangers
{
    public class TicketValueChanger : AbstractValueChanger<Ticket>
    {
        private static readonly ResourceValueChanger ResourceValueChanger = new ResourceValueChanger();
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
            result = ResourceValueChanger.Replace(template, result, model.TicketResources);
            result = TaxValueChanger.Replace(template, result, model.Orders.GroupBy(x => x.TaxTemplateName).Select(x => new TaxValue { Name = x.Key, Amount = x.Average(y => y.TaxRate), OrderAmount = x.Sum(y => y.GetItemValue() + (!y.TaxIncluded ? y.GetTotalTaxAmount(model.GetPlainSum(), model.GetPreTaxServicesTotal()) : 0)), TaxAmount = x.Sum(y => y.GetTotalTaxAmount(model.GetPlainSum(), model.GetPreTaxServicesTotal())) }));
            result = OrderValueChanger.Replace(template, result, model.Orders);
            return result;
        }
    }
}
