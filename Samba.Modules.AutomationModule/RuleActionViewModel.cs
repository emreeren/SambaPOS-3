﻿using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using Samba.Domain.Models.Automation;
using Samba.Infrastructure.Helpers;
using Samba.Localization.Properties;
using Samba.Presentation.Common.ModelBase;
using Samba.Presentation.Services;
using Samba.Presentation.Services.Common;

namespace Samba.Modules.AutomationModule
{
    [Export, PartCreationPolicy(CreationPolicy.NonShared)]
    class RuleActionViewModel : EntityViewModelBase<AppAction>
    {
        private readonly IAutomationService _automationService;

        [ImportingConstructor]
        public RuleActionViewModel(IAutomationService automationService)
        {
            _automationService = automationService;
        }

        public Dictionary<string, string> Parameters
        {
            get
            {
                if (string.IsNullOrEmpty(Model.Parameter)) return new Dictionary<string, string>();
                return JsonHelper.Deserialize<Dictionary<string, string>>(Model.Parameter);
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

        private List<IParameterValue> CreateParameterValues(string value)
        {
            if (string.IsNullOrEmpty(value)) return new List<IParameterValue>();

            var result = CreateParemeterValues(_automationService.GetActionType(value)).ToList();

            result.ForEach(x =>
                            {
                                if (Parameters.ContainsKey(x.Name))
                                    x.Value = Parameters[x.Name];
                            });
            return result;
        }

        private List<IParameterValue> _parameterValues;
        public List<IParameterValue> ParameterValues
        {
            get { return _parameterValues ?? (_parameterValues = CreateParameterValues(Model.ActionType)); }
            set
            {
                _parameterValues = value;
                RaisePropertyChanged(() => ParameterValues);
            }
        }

        public IEnumerable<RuleActionType> ActionTypes { get { return _automationService.GetActionTypes(); } }

        private IEnumerable<IParameterValue> CreateParemeterValues(RuleActionType actionType)
        {
            return _automationService.CreateParameterValues(actionType);
        }

        protected override void OnSave(string value)
        {
            base.OnSave(value);
            Model.Parameter = JsonHelper.Serialize(ParameterValues.ToDictionary(x => x.Name, x => x.Value));
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
