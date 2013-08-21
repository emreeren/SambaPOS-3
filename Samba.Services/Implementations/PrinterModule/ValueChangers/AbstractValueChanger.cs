using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using Samba.Domain.Models.Settings;
using Samba.Infrastructure.Settings;

namespace Samba.Services.Implementations.PrinterModule.ValueChangers
{
    public class GroupingKey : IEquatable<GroupingKey>, IComparable<GroupingKey>
    {
        public GroupingKey()
        {
            Name = "";
        }

        private string _key;
        public string Key
        {
            get { return _key; }
            set { _key = value ?? ""; }
        }

        private string _name;
        public string Name
        {
            get { return _name; }
            set { _name = value ?? ""; }
        }

        private static GroupingKey _empty;
        public static GroupingKey Empty { get { return _empty ?? (_empty = new GroupingKey()); } }

        public bool Equals(GroupingKey other)
        {
            return other != null && Name.Equals(other.Name);
        }

        public override int GetHashCode()
        {
            return Name.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            var gk = obj as GroupingKey;
            return gk != null && Name.Equals(gk.Key);
        }

        public int CompareTo(GroupingKey other)
        {
            return String.Compare(Key, other.Key, StringComparison.Ordinal);
        }
    }

    public class AbstractValueChanger<T> : IValueChanger<T>
    {
        [Import]
        public FunctionRegistry FunctionRegistry { get; set; }

        public string Replace(PrinterTemplate template, string content, IEnumerable<T> models)
        {
            return Helper.FormatData(content, "{" + GetTargetTag() + "}", () => GetValue(template, models.OrderBy(x => GetModelOrder(template, x))));
        }

        private string GetValue(PrinterTemplate template, IEnumerable<T> models)
        {
            var groupSwitchValue = template.GetSwitch(GetTargetTag() + " GROUP");
            if (groupSwitchValue != null)
            {
                var result = "";
                var groups = models.GroupBy(x => GetGroupSelector(x, groupSwitchValue)).OrderBy(x => x.Key);
                foreach (var @group in groups)
                {
                    var grp = @group;
                    var gtn = string.Format("{0} GROUP{1}", GetTargetTag(), grp.Key.Name != null ? ":" + grp.Key.Name : "");
                    var groupTemplate = UpdateGroupTemplateValues(template, gtn, grp);
                    result += ReplaceValues(groupTemplate, grp.ElementAt(0), template) + "\r\n";
                    result += string.Join("\r\n", grp.SelectMany(x => GetValue(template, x).Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries)));

                    var ftr = string.Format("{0} FOOTER{1}", GetTargetTag(), grp.Key.Name != null ? ":" + grp.Key.Name : "");
                    var ftrTemplate = UpdateGroupTemplateValues(template, ftr, grp);
                    result += "\r\n" + ftrTemplate + "\r\n";
                }
                return result;
            }
            return string.Join("\r\n", models.SelectMany(x => GetValue(template, x).Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries)));
        }

        private string UpdateGroupTemplateValues(PrinterTemplate template, string templateKey, IGrouping<GroupingKey, T> grouping)
        {
            var groupKeyName = grouping.Key.Name ?? "";
            var templateStr = template.GetPart(templateKey) ?? "";
            templateStr = Helper.FormatDataIf(grouping.Key != null, templateStr, "{GROUP KEY}", () => (groupKeyName));
            templateStr = Helper.FormatDataIf(grouping.Key != null, templateStr, "{GROUP SUM}",
                                              () => grouping.Sum(x => GetSumSelector(x)).ToString(LocalSettings.CurrencyFormat));
            templateStr = Helper.FormatDataIf(grouping.Key != null, templateStr, "{QUANTITY SUM}",
                                              () => grouping.Sum(x => GetQuantitySelector(x)).ToString(LocalSettings.QuantityFormat));
            return templateStr;
        }

        protected virtual decimal GetQuantitySelector(T x)
        {
            return 0;
        }

        protected virtual decimal GetSumSelector(T x)
        {
            return 0;
        }

        protected virtual void ProcessItem(T obj, string groupSwitchValue)
        {
            // override if needed
        }

        protected virtual GroupingKey GetGroupSelector(T arg, string switchValue)
        {
            return GroupingKey.Empty;
        }

        private object GetModelOrder(PrinterTemplate template, T model)
        {
            return template
                .GetFilters(GetTargetTag()).Select(x => x.Key)
                .ToList()
                .IndexOf(GetModelName(model));
        }

        public string GetValue(PrinterTemplate template, T model)
        {
            var filters = template.GetFilters(GetTargetTag());
            var templateName = filters.FirstOrDefault(x => FilterMatch(model, x.Key)).Value;
            if (string.IsNullOrEmpty(templateName))
            {
                var modelName = GetModelName(model);
                templateName = GetTargetTag() + (!string.IsNullOrEmpty(modelName) ? ":" + modelName : "");
            }
            var templatePart = template.GetPart(templateName);
            return !string.IsNullOrEmpty(templatePart) ? ReplaceValues(templatePart, model, template) : "";
        }

        public string ReplaceValues(string templatePart, T model, PrinterTemplate printerTemplate)
        {
            var result = ReplaceTemplateValues(templatePart, model, printerTemplate);
            return FunctionRegistry.ExecuteFunctions(result, model, printerTemplate);
        }

        protected virtual bool FilterMatch(T model, string key)
        {
            return false;
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
