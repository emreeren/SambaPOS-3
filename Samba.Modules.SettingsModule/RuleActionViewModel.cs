using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Samba.Domain.Models.Actions;
using Samba.Infrastructure;
using Samba.Localization.Properties;
using Samba.Presentation.Common.ModelBase;
using Samba.Services;

namespace Samba.Modules.SettingsModule
{
    internal class ParameterValue
    {
        private readonly PropertyInfo _parameterInfo;

        public string Name { get { return _parameterInfo.Name; } }
        public string NameDisplay
        {
            get
            {
                var result = Resources.ResourceManager.GetString(Name);
                return !string.IsNullOrEmpty(result) ? result + ":" : Name;
            }
        }

        public Type ValueType { get { return _parameterInfo.PropertyType; } }
        public string Value { get; set; }

        private IEnumerable<string> _values;

        public ParameterValue(PropertyInfo propertyInfo)
        {
            _parameterInfo = propertyInfo;
        }

        public IEnumerable<string> Values
        {
            get
            {
                if (ValueType == typeof(bool)) return new[] { "True", "False" };
                return _values ?? (_values = RuleActionTypeRegistry.GetParameterSource(Name));
            }
        }
    }

    class RuleActionViewModel : EntityViewModelBase<AppAction>
    {
        public RuleActionViewModel(AppAction model)
            : base(model)
        {

        }

        public Dictionary<string, string> Parameters
        {
            get
            {
                if (string.IsNullOrEmpty(Model.Parameter)) return new Dictionary<string, string>();
                return JsonHelper.Deserialize<Dictionary<string, string>>(Model.Parameter);
                //Model.Parameter.Split('#').ToDictionary(x => x.Split('=')[0], x => x.Split('=')[1]);
            }
        }

        public string SelectedActionType
        {
            get { return Model.ActionType; }
            set
            {
                Model.ActionType = value;
                ParameterValues = CreateParameterValues(value);
                RaisePropertyChanged(() => IsParameterLabelVisible);
            }
        }

        public bool IsParameterLabelVisible { get { return ParameterValues.Count > 0; } }

        private List<ParameterValue> CreateParameterValues(string value)
        {
            if (string.IsNullOrEmpty(value)) return new List<ParameterValue>();

            var result = CreateParemeterValues(RuleActionTypeRegistry.ActionTypes[value]).ToList();


            result.ForEach(x =>
                                        {
                                            if (Parameters.ContainsKey(x.Name))
                                                x.Value = Parameters[x.Name];
                                        });
            return result;
        }

        private List<ParameterValue> _parameterValues;
        public List<ParameterValue> ParameterValues
        {
            get { return _parameterValues ?? (_parameterValues = CreateParameterValues(Model.ActionType)); }
            set
            {
                _parameterValues = value;
                RaisePropertyChanged(() => ParameterValues);
            }
        }

        public IEnumerable<RuleActionType> ActionTypes { get { return RuleActionTypeRegistry.ActionTypes.Values; } }

        private static IEnumerable<ParameterValue> CreateParemeterValues(RuleActionType actionType)
        {
            if (actionType.ParameterObject != null)
                return actionType.ParameterObject.GetType().GetProperties().Select(x => new ParameterValue(x));
            return new List<ParameterValue>();
        }

        protected override void OnSave(string value)
        {
            base.OnSave(value);
            Model.Parameter = JsonHelper.Serialize(ParameterValues.ToDictionary(x => x.Name, x => x.Value));
            //string.Join("#", ParameterValues.Select(x => x.Name + "=" + x.Value));
        }

        public override Type GetViewType()
        {
            return typeof(RuleActionView);
        }

        public override string GetModelTypeString()
        {
            return Resources.RuleAction;
        }
    }
}
