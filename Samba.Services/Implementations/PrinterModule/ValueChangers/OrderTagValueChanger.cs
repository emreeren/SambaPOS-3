using System.ComponentModel.Composition;
using Samba.Domain.Models.Tickets;

namespace Samba.Services.Implementations.PrinterModule.ValueChangers
{
    [Export]
    public class OrderTagValueChanger : AbstractValueChanger<OrderTagValue>
    {
        public override string GetTargetTag()
        {
            return "ORDER TAGS";
        }

        protected override string GetModelName(OrderTagValue model)
        {
            return model.TagValue;
        }
    }
}
