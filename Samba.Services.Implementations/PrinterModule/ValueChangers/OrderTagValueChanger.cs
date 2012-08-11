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
            return model.Name;
        }

        protected override string ReplaceValues(string templatePart, OrderTagValue model, PrinterTemplate template)
        {
            var otResult = templatePart;
            otResult = FormatDataIf(model.Price != 0, otResult, TagNames.OrderTagPrice, () => model.AddTagPriceToOrderPrice ? "" : model.Price.ToString("#,#0.00"));
            otResult = FormatDataIf(model.Quantity != 0, otResult, TagNames.OrderTagQuantity, () => model.Quantity.ToString("#.##"));
            otResult = FormatDataIf(!string.IsNullOrEmpty(model.Name), otResult, TagNames.OrderTagName, () => model.Name);
            return otResult;
        }
    }
}
