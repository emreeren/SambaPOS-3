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
            return string.Join("\r\n", models.SelectMany(x => GetValue(template, x).Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries)));
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
