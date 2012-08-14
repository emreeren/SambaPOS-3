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

        protected override decimal GetSumSelector(Order x)
        {
            return x.GetItemValue();
        }

        protected override object GetGroupSelector(Order arg, string switchValue)
        {
            if (!string.IsNullOrEmpty(switchValue) && switchValue.Contains(":"))
            {
                var parts = switchValue.Split(':');
                if (parts[0] == "ORDER TAG")
                {
                    return arg.GetOrderTagValue(parts[1]);
                }
            }
            else if (switchValue == "ORDER STATE")
            {
                return arg.OrderStateGroupName ?? "";
            }
            return "";
        }

        protected override void ProcessItem(Order obj, string switchValue)
        {
            if (!string.IsNullOrEmpty(switchValue) && switchValue.Contains(":"))
            {
                var parts = switchValue.Split(':');
                if (parts[0] == "ORDER TAG" && obj.OrderTagValues.Any(x => x.OrderTagGroupName == parts[1]))
                {
                    obj.OrderTagValues.Where(x => x.OrderTagGroupName == parts[1]).ToList().ForEach(x => obj.OrderTagValues.Remove(x));
                }
            }
        }
    }
}
