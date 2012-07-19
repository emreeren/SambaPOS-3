using System;
using System.Collections.ObjectModel;
using System.ComponentModel.Composition;
using System.Linq;
using Samba.Domain.Models.Actions;
using Samba.Localization.Properties;
using Samba.Presentation.Common;
using Samba.Presentation.Common.ModelBase;
using Samba.Services;

namespace Samba.Modules.AutomationModule
{
    [Export, PartCreationPolicy(CreationPolicy.NonShared)]
    public class AutomationCommandViewModel : EntityViewModelBase<AutomationCommand>
    {
        private readonly IUserService _userService;
        private readonly IDepartmentService _departmentService;
        private readonly ISettingService _settingService;

        public CaptionCommand<string> DeleteAutomationCommandMapCommand { get; set; }
        public CaptionCommand<string> AddAutomationCommandMapCommand { get; set; }

        public AutomationCommandMapViewModel SelectedAutomationCommandMap { get; set; }

        [ImportingConstructor]
        public AutomationCommandViewModel(IUserService userService, IDepartmentService departmentService, ISettingService settingService)
        {
            _userService = userService;
            _departmentService = departmentService;
            _settingService = settingService;
            AddAutomationCommandMapCommand = new CaptionCommand<string>(Resources.Add, OnAddAutomationCommand);
            DeleteAutomationCommandMapCommand = new CaptionCommand<string>(Resources.Delete, OnDeleteAutomationCommand, CanDeleteAutomationCommand);
        }

        private bool CanDeleteAutomationCommand(string arg)
        {
            return SelectedAutomationCommandMap != null;
        }

        private void OnDeleteAutomationCommand(string obj)
        {
            if (SelectedAutomationCommandMap.Id > 0)
                Workspace.Delete(SelectedAutomationCommandMap.Model);
            Model.AutomationCommandMaps.Remove(SelectedAutomationCommandMap.Model);
            AutomationCommandMaps.Remove(SelectedAutomationCommandMap);
        }

        private void OnAddAutomationCommand(string obj)
        {
            AutomationCommandMaps.Add(new AutomationCommandMapViewModel(Model.AddAutomationCommandMap(), _userService, _departmentService, _settingService));
        }

        public string ButtonHeader { get { return Model.ButtonHeader; } set { Model.ButtonHeader = value; } }
        public string Color { get { return Model.Color; } set { Model.Color = value; } }

        private ObservableCollection<AutomationCommandMapViewModel> _automationCommandMaps;
        public ObservableCollection<AutomationCommandMapViewModel> AutomationCommandMaps
        {
            get { return _automationCommandMaps ?? (_automationCommandMaps = new ObservableCollection<AutomationCommandMapViewModel>(Model.AutomationCommandMaps.Select(x => new AutomationCommandMapViewModel(x, _userService, _departmentService, _settingService)))); }
        }

        public override Type GetViewType()
        {
            return typeof(AutomationCommandView);
        }

        public override string GetModelTypeString()
        {
            return Resources.AutomationCommand;
        }
    }
}
