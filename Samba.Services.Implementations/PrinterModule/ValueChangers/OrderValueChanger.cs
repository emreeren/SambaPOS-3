using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using Samba.Domain.Models.Settings;
using Samba.Domain.Models.Tickets;

namespace Samba.Services.Implementations.PrinterModule.ValueChangers
{
    public class OrderValueChanger : AbstractValueChanger<Order>
    {
        private static readonly OrderTagValueChanger OrderTagValueChanger = new OrderTagValueChanger();

        public override string GetTargetTag()
        {
            return "ORDERS";
        }

        protected override string GetModelName(Order model)
        {
            return model.OrderStateGroupName;
        }

        protected override string ReplaceTemplateValues(string templatePart, Order model, PrinterTemplate template)
        {
            return OrderTagValueChanger.Replace(template, templatePart, model.OrderTagValues);
        }
    }
}
