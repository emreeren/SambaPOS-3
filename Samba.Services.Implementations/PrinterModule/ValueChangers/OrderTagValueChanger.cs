using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using Samba.Domain.Models.Settings;
using Samba.Domain.Models.Tickets;

namespace Samba.Services.Implementations.PrinterModule.ValueChangers
{
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
