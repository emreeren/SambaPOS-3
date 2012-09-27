using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.Composition;
using System.Linq;
using Samba.Domain.Models.Actions;
using Samba.Infrastructure.Data;
using Samba.Localization.Properties;
using Samba.Presentation.Common;
using Samba.Presentation.Common.ModelBase;
using Samba.Presentation.Common.Services;
using Samba.Presentation.ViewModels;
using Samba.Services;

namespace Samba.Modules.AutomationModule
{
    [Export, PartCreationPolicy(CreationPolicy.NonShared)]
    public class RuleViewModel : EntityViewModelBaseWithMap<AppRule, AppRuleMap, AbstractMapViewModel<AppRuleMap>>
    {
        private readonly IAutomationService _automationService;

        [ImportingConstructor]
        public RuleViewModel(IAutomationService automationService)
        {
            _automationService = automationService;
            SelectActionsCommand = new CaptionCommand<string>(Resources.SelectActions, OnSelectActions);
            Constraints = new ObservableCollection<IRuleConstraint>();
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
            choosenValues.Cast<ActionContainer>().ToList().ForEach(x => Model.Actions.Add(x));
            _actions = new ObservableCollection<ActionContainerViewModel>(Model.Actions.Select(x => new ActionContainerViewModel(x, this, _automationService)));

            RaisePropertyChanged(() => Actions);

        }

        private ObservableCollection<ActionContainerViewModel> _actions;
        public ObservableCollection<ActionContainerViewModel> Actions
        {
            get { return _actions; }
        }

        private ObservableCollection<IRuleConstraint> _constraints;
        public ObservableCollection<IRuleConstraint> Constraints
        {
            get { return _constraints; }
            set
            {
                _constraints = value;
                RaisePropertyChanged(() => Constraints);
            }
        }

        public string SettingConstraintName { get; set; }
        public string SettingConstraintOperation { get; set; }
        public string SettingConstraintValue { get; set; }
        public IEnumerable<string> Operations { get { return new[] { "=", ">", "<", "!=" }; } }

        public IEnumerable<RuleEvent> Events { get { return _automationService.GetRuleEvents(); } }

        public ICaptionCommand SelectActionsCommand { get; set; }

        public string EventName
        {
            get { return Model.EventName; }
            set
            {
                Model.EventName = value;
                Constraints = new ObservableCollection<IRuleConstraint>(_automationService.GetEventConstraints(Model.EventName));
            }
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
            if (!string.IsNullOrEmpty(SettingConstraintName))
            {
                if (!string.IsNullOrEmpty(Model.EventConstraints))
                    Model.EventConstraints = Model.EventConstraints + "#";
                Model.EventConstraints += "SN$" + SettingConstraintName + ";" + (SettingConstraintOperation ?? "=") + ";" + SettingConstraintValue;
            }

            base.OnSave(value);
        }

        protected override void Initialize()
        {
            MapController = new MapController<AppRuleMap, AbstractMapViewModel<AppRuleMap>>(Model.AppRuleMaps, Workspace);

            _actions = new ObservableCollection<ActionContainerViewModel>(Model.Actions.Select(x => new ActionContainerViewModel(x, this, _automationService)));

            if (!string.IsNullOrEmpty(Model.EventConstraints))
            {
                Constraints.AddRange(_automationService.CreateRuleConstraints(Model.EventConstraints));

                var settingData = Model.EventConstraints.Split('#').Where(x => x.StartsWith("SN$")).FirstOrDefault();

                if (!string.IsNullOrEmpty(settingData))
                {
                    var settingParts = settingData.Split(';');
                    if (settingParts.Length == 3)
                    {
                        SettingConstraintName = settingParts[0].Replace("SN$", "");
                        SettingConstraintOperation = settingParts[1];
                        SettingConstraintValue = settingParts[2];
                    }
                }
            }

            base.Initialize();
        }
    }
}
