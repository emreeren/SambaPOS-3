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

        [ImportingConstructor]
        public RuleViewModel(IAutomationService automationService, IAutomationDao automationDao)
        {
            _automationService = automationService;
            _automationDao = automationDao;
            SelectActionsCommand = new CaptionCommand<string>(Resources.SelectActions, OnSelectActions);
            Constraints = new ObservableCollection<RuleConstraint>();
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
            _actions = new ObservableCollection<ActionContainerViewModel>(Model.Actions.Select(x => new ActionContainerViewModel(x, this, _automationService, _automationDao)));

            RaisePropertyChanged(() => Actions);

        }

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

        public string CustomConstraint { get { return Model.CustomConstraint; } set { Model.CustomConstraint = value; } }

        public IEnumerable<string> Operations { get { return new[] { "=", ">", "<", "!=" }; } }

        public IEnumerable<RuleEvent> Events { get { return _automationService.GetRuleEvents(); } }

        public ICaptionCommand SelectActionsCommand { get; set; }

        public string GroupValue { get { return Events.Any(x => x.EventKey == EventName) ? Events.First(x => x.EventKey == EventName).EventName : ""; } }

        public string EventName
        {
            get { return Model.EventName; }
            set
            {
                Model.EventName = value;
                Constraints = new ObservableCollection<RuleConstraint>(_automationService.GetEventConstraints(Model.EventName));
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
            base.OnSave(value);
        }

        protected override void Initialize()
        {
            MapController = new MapController<AppRuleMap, AbstractMapViewModel<AppRuleMap>>(Model.AppRuleMaps, Workspace);

            _actions = new ObservableCollection<ActionContainerViewModel>(Model.Actions.Select(x => new ActionContainerViewModel(x, this, _automationService, _automationDao)));

            if (!string.IsNullOrEmpty(Model.EventConstraints))
            {
                Constraints.AddRange(_automationService.CreateRuleConstraints(Model.EventConstraints));
            }

            base.Initialize();
        }
    }
}
