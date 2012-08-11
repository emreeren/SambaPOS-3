using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Practices.ServiceLocation;
using Samba.Domain.Models.Settings;

namespace Samba.Services.Implementations.PrinterModule.ValueChangers
{
    public abstract class AbstractValueChanger<T> : IValueChanger<T>
    {
        protected static readonly IDepartmentService DepartmentService = ServiceLocator.Current.GetInstance<IDepartmentService>();
        protected static readonly ISettingService SettingService = ServiceLocator.Current.GetInstance<ISettingService>();
        protected static readonly ICacheService CacheService = ServiceLocator.Current.GetInstance<ICacheService>();
        protected static readonly IAccountService AccountService = ServiceLocator.Current.GetInstance<IAccountService>();

        protected string FormatData(string data, string tag, Func<string> valueFunc)
        {
            if (!data.Contains(tag)) return data;

            var value = valueFunc.Invoke();
            var tagData = new TagData(data, tag);
            if (!string.IsNullOrEmpty(value)) value =
                !string.IsNullOrEmpty(tagData.Title) && tagData.Title.Contains("<value>")
                ? tagData.Title.Replace("<value>", value)
                : tagData.Title + value;
            return data.Replace(tagData.DataString, value);
        }

        protected string FormatDataIf(bool condition, string data, string tag, Func<string> valueFunc)
        {
            if (condition && data.Contains(tag)) return FormatData(data, tag, valueFunc.Invoke);
            return RemoveTag(data, tag);
        }

        protected string RemoveTag(string data, string tag)
        {
            var tagData = new TagData(data, tag);
            return data.Replace(tagData.DataString, "");
        }

        public string Replace(PrinterTemplate template, string content, IEnumerable<T> models)
        {
            return FormatData(content, "{" + GetTargetTag() + "}", () => GetValue(template, models));
        }

        public string GetValue(PrinterTemplate template, IEnumerable<T> models)
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

        public abstract string GetTargetTag();
        protected abstract string GetModelName(T model);
        protected abstract string ReplaceValues(string templatePart, T model, PrinterTemplate template);
    }
}
