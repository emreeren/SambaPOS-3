using System.ComponentModel.Composition;
using Microsoft.Practices.Prism.MefExtensions.Modularity;
using Samba.Localization.Properties;
using Samba.Presentation.Common;
using Samba.Services;

namespace Samba.Modules.DepartmentModule
{
    [ModuleExport(typeof(DepartmentModule))]
    public class DepartmentModule : ModuleBase
    {
        [ImportingConstructor]
        public DepartmentModule()
        {
            AddDashboardCommand<DepartmentListViewModel>(Resources.Departments, Resources.Settings);
            PermissionRegistry.RegisterPermission(PermissionNames.ChangeDepartment, PermissionCategories.Department, Resources.CanChangeDepartment);
            
            foreach (var department in AppServices.MainDataContext.Departments)
            {
                PermissionRegistry.RegisterPermission(PermissionNames.UseDepartment + department.Id, PermissionCategories.Department, department.Name);
            }
        }
    }
}
