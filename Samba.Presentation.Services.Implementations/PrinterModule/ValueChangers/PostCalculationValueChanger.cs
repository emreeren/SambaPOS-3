using Samba.Domain.Models.Tickets;

namespace Samba.Presentation.Services.Implementations.PrinterModule.ValueChangers
{
    class PostCalculationValueChanger : AbstractValueChanger<Calculation>
    {
        public override string GetTargetTag()
        {
            return "SERVICES";
        }

        protected override string GetModelName(Calculation model)
        {
            return model.Name;
        }
    }
}
