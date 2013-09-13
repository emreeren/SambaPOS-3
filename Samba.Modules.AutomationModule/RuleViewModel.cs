using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.Composition;
using System.Linq;
using Samba.Domain.Models.Automation;
using Samba.Infrastructure.Data;
using Samba.Localization.Properties;
using Samba.Persistance;
using Samba.Presentation.Common;
using Samba.Presentation.Common.Commands;
using Samba.Presentation.Common.ModelBase;
using Samba.Presentation.Common.Services;
using Samba.Services;
using Samba.Services.Common;

namespace Samba.Modules.AutomationModule
{
    [Export, PartCreationPolicy(CreationPolicy.NonShared)]
    public class RuleViewModel : EntityViewModelBaseWithMap<AppRule, AppRuleMap, AbstractMapViewModel<AppRuleMap>>
    {
        private readonly IAutomationService _automationService;
        private readonly IAutomationDao _automationDao;

        public ICaptionCommand SelectActionsCommand { get; set; }
        public ICaptionCommand AddConstraintCommand { get; set; }
        public ICaptionCommand RemoveConstraintCommand { get; set; }

        [ImportingConstructor]
        public RuleViewModel(IAutomationService automationService, IAutomationDao automationDao)
        {
            _automationService = automationService;
            _automationDao = automationDao;
            SelectActionsCommand = new CaptionCommand<string>(Resources.SelectActions, OnSelectActions);
            AddConstraintCommand = new CaptionCommand<string>(string.Format(Resources.Add_f, Resources.CustomConstraint), OnAddConstraint,CanAddConstraint);
            RemoveConstraintCommand = new CaptionCommand<RuleConstraintValueViewModel>(Resources.Remove, OnRemoveConstraint);
            Constraints = new ObservableCollection<RuleConstraint>();
        }

        private IList<string> _ruleMatchNames;
        public IList<string> RuleMatchNames { get { return _ruleMatchNames ?? (_ruleMatchNames = EnumToLocalizedList(typeof(RuleConstraintMatch))); } }


        private ObservableCollection<ActionContainerViewModel> _actions;
        public ObservableCollection<ActionContainerViewModel> Actions
        {
            get { return _actions; }
        }

        private ObservableCollection<RuleConstraint> _constraints;
        public ObservableCollection<RuleConstraint> Constraints
        {
            get { return _constraints; }
            set
            {
                _constraints = value;
                RaisePropertyChanged(() => Constraints);
            }
        }

        private ObservableCollection<RuleConstraintValueViewModel> _ruleConstraintValues;
        public ObservableCollection<RuleConstraintValueViewModel> RuleConstraintValues
        {
            get
            {
                if (_ruleConstraintValues == null)
                {
                    _ruleConstraintValues = new ObservableCollection<RuleConstraintValueViewModel>();
                    UpdateCustomRuleConstraints();
                }
                return _ruleConstraintValues;
            }
            set
            {
                _ruleConstraintValues = value;
                RaisePropertyChanged(() => RuleConstraintValues);
            }
        }

        public bool IsConstraintsVisible { get { return !string.IsNullOrEmpty(CustomConstraint) || Constraints.Count > 0; } }

        public string RuleMatchName { get { return RuleMatchNames[Model.ConstraintMatch]; } set { Model.ConstraintMatch = RuleMatchNames.IndexOf(value); } }

        public string CustomConstraint { get { return Model.CustomConstraint; } set { Model.CustomConstraint = value; } }

        public IEnumerable<string> Operations { get { return new[] { "=", ">", "<", "!=" }; } }

        public IEnumerable<RuleEvent> Events { get { return _automationService.GetRuleEvents(); } }

        public string GroupValue { get { return Events.Any(x => x.EventKey == EventName) ? Events.First(x => x.EventKey == EventName).EventName : ""; } }

        public string EventName
        {
            get { return Model.EventName; }
            set
            {
                Model.EventName = value;
                Constraints.Clear();
                UpdateCustomRuleConstraints();
            }
        }

        private void OnRemoveConstraint(RuleConstraintValueViewModel obj)
        {
            Model.DeleteRuleConstraint(obj.Name);
            UpdateCustomRuleConstraints();
        }

        private void OnAddConstraint(string obj)
        {
            Model.AddRuleConstraint("", "Equal", "");
            UpdateCustomRuleConstraints();
        }

        private bool CanAddConstraint(string arg)
        {
            return Model != null && !string.IsNullOrEmpty(Model.EventName);
        }

        private void OnSelectActions(string obj)
        {
            IList<IOrderable> selectedValues = new List<IOrderable>(Model.Actions);
            var selectedIds = selectedValues.Select(x => ((ActionContainer)x).AppActionId);
            IList<IOrderable> values = new List<IOrderable>(Workspace.All<AppAction>(x => !selectedIds.Contains(x.Id)).Select(x => new ActionContainer(x)));

            var choosenValues = InteractionService.UserIntraction.ChooseValuesFrom(values, selectedValues, string.Format(Resources.List_f, Resources.Action),
                                                                                   Resources.SelectActions, Resources.Action, Resources.Actions);

            foreach (var action in Model.Actions.ToList())
            {
                var laction = action;
                if (choosenValues.FirstOrDefault(x => ((ActionContainer)x).AppActionId == laction.AppActionId) == null)
                {
                    if (action.Id > 0)
                        Workspace.Delete(action);
                }
            }

            Model.Actions.Clear();
            choosenValues.Cast<ActionContainer>().OrderBy(x => x.SortOrder).ToList().ForEach(x => Model.Actions.Add(x));
            _actions = new ObservableCollection<ActionContainerViewModel>(Model.Actions.OrderBy(x => x.SortOrder).Select(x => new ActionContainerViewModel(x, this, _automationService, _automationDao)));

            RaisePropertyChanged(() => Actions);

        }

        public override Type GetViewType()
        {
            return typeof(RuleView);
        }

        public override string GetModelTypeString()
        {
            return Resources.Rule;
        }

        protected override void OnSave(string value)
        {
            Model.EventConstraints = string.Join("#", Constraints
                .Where(x => x.Value != null)
                .Select(x => x.GetConstraintData()));
            Model.RemoveInvalidConstraints();
            _ruleConstraintValues = null;
            base.OnSave(value);
        }

        public override void OnShown()
        {
            _ruleConstraintValues = new ObservableCollection<RuleConstraintValueViewModel>();
            UpdateCustomRuleConstraints();
            base.OnShown();
        }

        protected override void Initialize()
        {
            MapController = new MapController<AppRuleMap, AbstractMapViewModel<AppRuleMap>>(Model.AppRuleMaps, Workspace);

            _actions = new ObservableCollection<ActionContainerViewModel>(Model.Actions.OrderBy(x => x.SortOrder).Select(x => new ActionContainerViewModel(x, this, _automationService, _automationDao)));

            if (!string.IsNullOrEmpty(Model.EventConstraints))
            {
                Constraints.AddRange(_automationService.CreateRuleConstraints(Model.EventConstraints));
            }
            base.Initialize();
        }

        private void UpdateCustomRuleConstraints()
        {
            if (string.IsNullOrEmpty(Model.EventName)) return;
            var customRuleConstraintNames = _automationService.GetCustomRuleConstraintNames(Model.EventName).Select(x => new RuleConstraintName(x)).ToList();
            _ruleConstraintValues.Clear();
            _ruleConstraintValues.AddRange(Model.GetRuleConstraintValues()
                .Select(x => new RuleConstraintValueViewModel(x, customRuleConstraintNames, RemoveConstraintCommand)));
            RaisePropertyChanged(() => RuleConstraintValues);
            RaisePropertyChanged(() => IsConstraintsVisible);
        }

        private static IList<string> EnumToLocalizedList(Type type)
        {
            var result = new List<string>();
            foreach (Enum val in Enum.GetValues(type))
            {
                var name = val.ToString();
                var localized = Resources.ResourceManager.GetString(name);
                if (string.IsNullOrEmpty(localized)) localized = name;
                result.Add(localized);
            }
            return result;
        }
    }
}
