using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using Samba.Domain.Models.Dashboards;
using Samba.Localization.Properties;
using Samba.Presentation.Common;
using Samba.Presentation.Common.ModelBase;
using Samba.Services;

namespace Samba.Modules.DashboardModule
{
    [Export, PartCreationPolicy(CreationPolicy.NonShared)]
    public class DashboardViewModel : EntityViewModelBase<Dashboard>
    {
        private readonly IUserService _userService;
        private readonly IDepartmentService _departmentService;
        private readonly ISettingService _settingService;

        public CaptionCommand<string> DeleteDashboardMapCommand { get; set; }
        public CaptionCommand<string> AddDashboardMapCommand { get; set; }
        public DashboardMapViewModel SelectedDashboardMap { get; set; }

        [ImportingConstructor]
        public DashboardViewModel(IUserService userService, IDepartmentService departmentService, ISettingService settingService)
        {
            _userService = userService;
            _departmentService = departmentService;
            _settingService = settingService;
            AddDashboardMapCommand = new CaptionCommand<string>(Resources.Add,OnAddDashboardMap);
            DeleteDashboardMapCommand = new CaptionCommand<string>(Resources.Delete,OnDeleteDashboadMap,CanDeleteDashboardMap);
        }

        private bool CanDeleteDashboardMap(string arg)
        {
            return SelectedDashboardMap != null;
        }

        private void OnDeleteDashboadMap(string obj)
        {
            if (SelectedDashboardMap.Id > 0)
                Workspace.Delete(SelectedDashboardMap.Model);
            Model.DashboardMaps.Remove(SelectedDashboardMap.Model);
            DashboardMaps.Remove(SelectedDashboardMap);
        }

        private void OnAddDashboardMap(string obj)
        {
            DashboardMaps.Add(new DashboardMapViewModel(Model.AddDasboardMap(), _userService, _departmentService, _settingService));
        }

        private ObservableCollection<DashboardMapViewModel> _dashboardMaps;
        public ObservableCollection<DashboardMapViewModel> DashboardMaps
        {
            get { return _dashboardMaps ?? (_dashboardMaps = new ObservableCollection<DashboardMapViewModel>(Model.DashboardMaps.Select(x => new DashboardMapViewModel(x, _userService, _departmentService, _settingService)))); }
        }

        public override Type GetViewType()
        {
            return typeof(DashboardView);
        }

        public override string GetModelTypeString()
        {
            return Resources.Dashboard;
        }
    }
}
