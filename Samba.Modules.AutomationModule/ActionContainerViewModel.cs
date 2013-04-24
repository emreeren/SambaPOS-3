using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text.RegularExpressions;
using Samba.Domain.Models.Automation;
using Samba.Persistance;
using Samba.Presentation.Common;
using Samba.Services;

namespace Samba.Modules.AutomationModule
{
    public class ActionParameterValue : ObservableObject
    {
        public string Name { get; set; }

        private string _value;
        public string Value
        {
            get { return _value; }
            set
            {
                _value = value;
                ActionContainer.UpdateParameters();
            }
        }

        public IEnumerable<string> ParameterValues { get; set; }

        public ActionContainerViewModel ActionContainer { get; set; }
        public ActionParameterValue(ActionContainerViewModel actionContainer, string name, string value, IEnumerable<string> parameterValues)
        {
            Name = name;
            _value = value;
            ActionContainer = actionContainer;
            ParameterValues = parameterValues.Select(x => string.Format("[:{0}]", x));
        }
    }

    public class ActionContainerViewModel : ObservableObject
    {
        public ActionContainerViewModel(ActionContainer model, RuleViewModel ruleViewModel, IAutomationService automationService,IAutomationDao automationDao)
        {
            Model = model;
            _ruleViewModel = ruleViewModel;
            _automationService = automationService;
            _automationDao = automationDao;
        }

        private readonly RuleViewModel _ruleViewModel;
        private readonly IAutomationService _automationService;
        private readonly IAutomationDao _automationDao;

        private AppAction _action;
        public AppAction Action { get { return _action ?? (_action = _automationDao.GetActionById(Model.AppActionId)); } set { _action = value; } }

        public ActionContainer Model { get; set; }

        private ObservableCollection<ActionParameterValue> _parameterValues;
        public ObservableCollection<ActionParameterValue> ParameterValues
        {
            get { return _parameterValues ?? (_parameterValues = GetParameterValues()); }
        }

        private ObservableCollection<ActionParameterValue> GetParameterValues()
        {
            IEnumerable<ActionParameterValue> result;
            if (!string.IsNullOrEmpty(_ruleViewModel.EventName))
            {
                if (string.IsNullOrEmpty(Model.ParameterValues))
                {
                    result = Action.Parameters.Values.Where(x => !string.IsNullOrEmpty(x) && Regex.IsMatch(x, "\\[:([^\\]]+)\\]"))
                        .Select(x => new ActionParameterValue(this, Regex.Match(x, "\\[:([^\\]]+)\\]").Groups[1].Value, "",
                                                     _automationService.GetParameterNames(_ruleViewModel.EventName)));
                }
                else
                {
                    result = Model.ParameterValues.Split('#').Select(
                    x => new ActionParameterValue(this, x.Split(new[] { '=' }, 2)[0], x.Split(new[] { '=' }, 2)[1], _automationService.GetParameterNames(_ruleViewModel.EventName)));
                }
            }
            else result = new List<ActionParameterValue>();

            return new ObservableCollection<ActionParameterValue>(result);
        }

        public string Name
        {
            get { return Model.Name; }
            set { Model.Name = value; }
        }

        public string CustomConstraint
        {
            get { return Model.CustomConstraint; }
            set { Model.CustomConstraint = value; }
        }

        public void UpdateParameters()
        {
            Model.ParameterValues = string.Join("#", ParameterValues.Select(x => x.Name + "=" + x.Value));
        }
    }
}
