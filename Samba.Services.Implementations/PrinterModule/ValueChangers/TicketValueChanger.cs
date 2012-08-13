using Samba.Domain.Models.Settings;
using Samba.Domain.Models.Tickets;

namespace Samba.Services.Implementations.PrinterModule.ValueChangers
{
    public class TicketValueChanger : AbstractValueChanger<Ticket>
    {
        private static readonly ResourceValueChanger ResourceValueChanger = new ResourceValueChanger();
        private static readonly CalculationValueChanger CalculationValueChanger = new CalculationValueChanger();
        private static readonly OrderValueChanger OrderValueChanger = new OrderValueChanger();

        public override string GetTargetTag()
        {
            return "LAYOUT";
        }

        protected override string ReplaceTemplateValues(string templatePart, Ticket model, PrinterTemplate template)
        {
            var result = CalculationValueChanger.Replace(template, templatePart, model.Calculations);
            result = ResourceValueChanger.Replace(template, result, model.TicketResources);
            result = OrderValueChanger.Replace(template, result, model.Orders);
            return result;
        }
    }
}
