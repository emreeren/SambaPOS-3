using System.ComponentModel.Composition;
using Samba.Domain.Models.Tickets;

namespace Samba.Services.Implementations.PrinterModule.ValueChangers
{
    [Export]
    public class ChangePaymentValueChanger : AbstractValueChanger<ChangePayment>
    {
        public override string GetTargetTag()
        {
            return "CHANGES";
        }

        protected override string GetModelName(ChangePayment model)
        {
            return model.Name;
        }
    }
}
