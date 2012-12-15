using System.Linq;
using Samba.Domain.Models.Settings;
using Samba.Domain.Models.Tickets;

namespace Samba.Presentation.Services.Implementations.PrinterModule.ValueChangers
{
    public class OrderValueChanger : AbstractValueChanger<Order>
    {
        private static readonly OrderTagValueChanger OrderTagValueChanger = new OrderTagValueChanger();
        private static readonly OrderStateValueChanger OrderStateValueChanger = new OrderStateValueChanger();

        public override string GetTargetTag()
        {
            return "ORDERS";
        }

        protected override bool FilterMatch(Order model, string key)
        {
            if (key.Contains("="))
            {
                var parts = key.Split('=');
                if (parts.Count() == 2)
                {
                    return model.IsInState(parts[0], parts[1]);
                }
            }
            return model.IsInState("*", key);
        }

        protected override string ReplaceTemplateValues(string templatePart, Order model, PrinterTemplate template)
        {
            var result = OrderStateValueChanger.Replace(template, templatePart, model.GetOrderStateValues());
            return OrderTagValueChanger.Replace(template, result, model.GetOrderTagValues());
        }

        protected override decimal GetSumSelector(Order x)
        {
            return x.GetValue();
        }

        protected override GroupingKey GetGroupSelector(Order arg, string switchValue)
        {
            if (!string.IsNullOrEmpty(switchValue) && switchValue.Contains(":"))
            {
                var parts = switchValue.Split(':');
                if (parts[0] == "ORDER TAG")
                {
                    var r = arg.GetOrderTagValue(parts[1]);
                    return new GroupingKey { Key = r.OrderKey, Name = r.TagValue };
                }
                if (parts[0] == "ORDER STATE")
                {
                    var r = arg.GetStateValue(parts[1]);
                    return new GroupingKey { Key = r.OrderKey, Name = r.StateValue };
                }
            }
            return base.GetGroupSelector(arg, switchValue);
        }

        protected override void ProcessItem(Order obj, string switchValue)
        {
            if (!string.IsNullOrEmpty(switchValue) && switchValue.Contains(":"))
            {
                var parts = switchValue.Split(':');
                if (parts[0] == "ORDER TAG" && obj.OrderTagExists(x => x.TagName == parts[1]))
                {
                    obj.GetOrderTagValues(x => x.TagName == parts[1]).ToList().ForEach(obj.UntagOrder);
                }
            }
        }
    }
}
