using Samba.Domain.Models.Tickets;

namespace Samba.Presentation.Services.Implementations.PrinterModule.ValueChangers
{
    class ChangePaymentValueChanger : AbstractValueChanger<ChangePayment>
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
