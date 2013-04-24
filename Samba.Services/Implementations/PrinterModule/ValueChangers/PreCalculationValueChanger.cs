using System.ComponentModel.Composition;
using Samba.Domain.Models.Tickets;

namespace Samba.Services.Implementations.PrinterModule.ValueChangers
{
    [Export]
    public class PreCalculationValueChanger : AbstractValueChanger<Calculation>
    {
        public override string GetTargetTag()
        {
            return "DISCOUNTS";
        }

        protected override string GetModelName(Calculation model)
        {
            return model.Name;
        }
    }
}
