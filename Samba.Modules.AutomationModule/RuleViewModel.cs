using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Samba.Domain.Models.Actions;
using Samba.Infrastructure.Data;
using Samba.Localization.Properties;
using Samba.Presentation.Common;
using Samba.Presentation.Common.ModelBase;
using Samba.Presentation.Common.Services;
using Samba.Services;

namespace Samba.Modules.AutomationModule
{
    public class RuleViewModel : EntityViewModelBase<AppRule>
    {
        public RuleViewModel(AppRule model)
            : base(model)
        {
            _actions = new ObservableCollection<ActionContainerViewModel>(Model.Actions.Select(x => new ActionContainerViewModel(x, this)));

            SelectActionsCommand = new CaptionCommand<string>(Resources.SelectActions, OnSelectActions);
            Constraints = new ObservableCollection<RuleConstraintViewModel>();
            if (!string.IsNullOrEmpty(model.EventConstraints))
            {
                Constraints.AddRange(
                    model.EventConstraints.Split('#')
                    .Where(x => !x.StartsWith("SN$"))
                    .Select(x => new RuleConstraintViewModel(x)));
                var settingData =
                model.EventConstraints.Split('#').Where(x => x.StartsWith("SN$")).FirstOrDefault();
                if (!string.IsNullOrEmpty(settingData))
                {
                    var settingParts = settingData.Split(';');
                    if (settingParts.Length == 3)
                    {
                        SettingConstraintName = settingParts[0].Replace("SN$", "");
                        SettingConstraintValue = settingParts[2];
                    }
                }
            }
        }

        private void OnSelectActions(string obj)
        {
            IList<IOrderable> selectedValues = new List<IOrderable>(Model.Actions);
            var selectedIds = selectedValues.Select(x => ((ActionContainer)x).AppActionId);
            IList<IOrderable> values = new List<IOrderable>(Workspace.All<AppAction>(x => !selectedIds.Contains(x.Id)).Select(x => new ActionContainer(x)));

            var choosenValues = InteractionService.UserIntraction.ChooseValuesFrom(values, selectedValues, Resources.ActionList,
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
            _actions = new ObservableCollection<ActionContainerViewModel>(Model.Actions.Select(x => new ActionContainerViewModel(x, this)));

            RaisePropertyChanged(() => Actions);

        }

        private ObservableCollection<ActionContainerViewModel> _actions;
        public ObservableCollection<ActionContainerViewModel> Actions
        {
            get { return _actions; }
        }

        private ObservableCollection<RuleConstraintViewModel> _constraints;
        public ObservableCollection<RuleConstraintViewModel> Constraints
        {
            get { return _constraints; }
            set
            {
                _constraints = value;
                RaisePropertyChanged(() => Constraints);
            }
        }

        public string SettingConstraintName { get; set; }
        public string SettingConstraintValue { get; set; }

        public IEnumerable<RuleEvent> Events { get { return RuleActionTypeRegistry.RuleEvents.Values; } }

        public ICaptionCommand SelectActionsCommand { get; set; }

        public string EventName
        {
            get { return Model.EventName; }
            set
            {
                Model.EventName = value;
                Constraints = new ObservableCollection<RuleConstraintViewModel>(
                    RuleActionTypeRegistry.GetEventConstraints(Model.EventName));
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
                Model.EventConstraints += "SN$" + SettingConstraintName + ";=;" + SettingConstraintValue;
            base.OnSave(value);
        }
    }
}
