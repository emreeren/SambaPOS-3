using System.ComponentModel.Composition;
using Microsoft.Practices.Prism.MefExtensions.Modularity;
using Microsoft.Practices.Prism.Regions;
using Samba.Domain.Models.Tickets;
using Samba.Localization.Properties;
using Samba.Persistance.Data;
using Samba.Presentation.Common;
using Samba.Services;

namespace Samba.Modules.DepartmentModule
{
    [ModuleExport(typeof(DepartmentModule))]
    public class DepartmentModule : ModuleBase
    {
        private readonly IRegionManager _regionManager;

        [ImportingConstructor]
        public DepartmentModule(IRegionManager regionManager)
        {
            _regionManager = regionManager;
            AddDashboardCommand<DepartmentListViewModel>(Resources.Departments, Resources.Settings);
            PermissionRegistry.RegisterPermission(PermissionNames.ChangeDepartment, PermissionCategories.Department, Resources.CanChangeDepartment);

            foreach (var department in Dao.Query<Department>())
            {
                PermissionRegistry.RegisterPermission(PermissionNames.UseDepartment + department.Id, PermissionCategories.Department, department.Name);
            }
        }

        protected override void OnInitialization()
        {
            _regionManager.RegisterViewWithRegion(RegionNames.UserRegion, typeof(DepartmentButtonView));
        }
    }
}
