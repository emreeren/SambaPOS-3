using System;
using System.Collections.Generic;
using System.Linq;
using Samba.Domain.Models.Settings;

namespace Samba.Services.Implementations.PrinterModule.ValueChangers
{
    public class AbstractValueChanger<T> : IValueChanger<T>
    {
        public string Replace(PrinterTemplate template, string content, IEnumerable<T> models)
        {
            return Helper.FormatData(content, "{" + GetTargetTag() + "}", () => GetValue(template, models));
        }

        private string GetValue(PrinterTemplate template, IEnumerable<T> models)
        {

            var groupSwitchValue = template.GetSwitch(GetTargetTag() + " GROUP");
            if (groupSwitchValue != null)
            {
                var result = "";
                var groups = models.GroupBy(x => GetGroupSelector(x, groupSwitchValue));
                foreach (var @group in groups)
                {
                    IGrouping<object, T> grp = @group;
                    var gtn = string.Format("{0} GROUP{1}", GetTargetTag(), grp.Key != null ? ":" + grp.Key : "");
                    var groupTemplate = (template.GetPart(gtn) ?? "");
                    Helper.FormatDataIf(grp.Key != null, groupTemplate, "{GROUP KEY}", () => (grp.Key ?? "").ToString());
                    Helper.FormatDataIf(grp.Key != null, groupTemplate, "{GROUP SUM}", () => grp.Sum(x => GetSumSelector(x)).ToString("#,#0.00"));
                    result += ReplaceValues(groupTemplate, grp.ElementAt(0), template) + "\r\n";
                    group.ToList().ForEach(x => ProcessItem(x, groupSwitchValue));
                    result += string.Join("\r\n", grp.SelectMany(x => GetValue(template, x).Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries)));
                    result += "\r\n";
                }
                return result;
            }
            return string.Join("\r\n", models.SelectMany(x => GetValue(template, x).Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries)));
        }

        protected virtual decimal GetSumSelector(T x)
        {
            return 0;
        }

        protected virtual void ProcessItem(T obj, string groupSwitchValue)
        {
            // override if needed
        }

        protected virtual object GetGroupSelector(T arg, string switchValue)
        {
            return null;
        }

        public string GetValue(PrinterTemplate template, T model)
        {
            var modelName = GetModelName(model);
            var templateName = GetTargetTag() + (!string.IsNullOrEmpty(modelName) ? ":" + modelName : "");
            var templatePart = template.GetPart(templateName);
            return !string.IsNullOrEmpty(templatePart) ? ReplaceValues(templatePart, model, template) : "";
        }

        private string ReplaceValues(string templatePart, T model, PrinterTemplate printerTemplate)
        {
            var result = ReplaceTemplateValues(templatePart, model, printerTemplate);
            return FunctionRegistry.ExecuteFunctions(result, model, printerTemplate);
        }

        public virtual string GetTargetTag()
        {
            return "";
        }

        protected virtual string GetModelName(T model)
        {
            return "";
        }

        protected virtual string ReplaceTemplateValues(string templatePart, T model, PrinterTemplate template)
        {
            return templatePart;
        }
    }
}
