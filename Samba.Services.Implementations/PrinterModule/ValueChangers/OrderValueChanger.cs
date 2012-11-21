using System.Linq;
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

        protected override bool FilterMatch(Order model, string key)
        {
            if (key.Contains("_"))
            {
                var parts = key.Split('_');
                if (parts.Count() == 2)
                {
                    return model.OrderTagValues.Any(x => x.TagName == parts[0] && x.TagValue == parts[1]);
                }
            }
            return model.OrderTagValues.Any(x => x.TagValue.ToLower() == key.ToLower());
        }

        protected override string GetModelName(Order model)
        {
            return model.OrderStateGroupName;
        }

        protected override string ReplaceTemplateValues(string templatePart, Order model, PrinterTemplate template)
        {
            return OrderTagValueChanger.Replace(template, templatePart, model.OrderTagValues.Where(x => !x.IsSubTag));
        }

        protected override decimal GetSumSelector(Order x)
        {
            return x.GetItemValue();
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
            }
            else if (switchValue == "ORDER STATE")
            {
                return new GroupingKey { Name = arg.OrderStateGroupName ?? "", Key = arg.OrderStateGroupName ?? "" };
            }

            return base.GetGroupSelector(arg, switchValue);
        }

        protected override void ProcessItem(Order obj, string switchValue)
        {
            if (!string.IsNullOrEmpty(switchValue) && switchValue.Contains(":"))
            {
                var parts = switchValue.Split(':');
                if (parts[0] == "ORDER TAG" && obj.OrderTagValues.Any(x => x.TagName == parts[1]))
                {
                    obj.OrderTagValues.Where(x => x.TagName == parts[1]).ToList().ForEach(x => obj.OrderTagValues.Remove(x));
                }
            }
        }
    }
}
