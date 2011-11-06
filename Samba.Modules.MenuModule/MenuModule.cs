using System.ComponentModel.Composition;
using Microsoft.Practices.Prism.MefExtensions.Modularity;
using Samba.Localization.Properties;
using Samba.Presentation.Common;
using Samba.Services;

namespace Samba.Modules.MenuModule
{
    [ModuleExport(typeof(MenuModule))]
    public class MenuModule : ModuleBase
    {
        [ImportingConstructor]
        public MenuModule()
        {
            AddDashboardCommand<DepartmentListViewModel>(Resources.Departments, Resources.Settings);
            AddDashboardCommand<MenuItemListViewModel>(Resources.ProductList, Resources.Products);
            AddDashboardCommand<ScreenMenuListViewModel>(Resources.MenuList, Resources.Products);
            AddDashboardCommand<MenuItemPropertyGroupListViewModel>(Resources.ModifierGroups, Resources.Products);
            AddDashboardCommand<PriceListViewModel>(Resources.BatchPriceList, Resources.Products);
            AddDashboardCommand<TicketTagGroupListViewModel>(Resources.TicketTags, Resources.Settings, 10);
            AddDashboardCommand<MenuItemPriceDefinitionListViewModel>(Resources.PriceDefinitions, Resources.Products);
            AddDashboardCommand<TaxTemplateListViewModel>(Resources.TaxTemplates, Resources.Products);
            AddDashboardCommand<ServiceTemplateListViewModel>(Resources.ServiceTemplates, Resources.Products);

            PermissionRegistry.RegisterPermission(PermissionNames.ChangeDepartment, PermissionCategories.Department, Resources.CanChangeDepartment);
            
            foreach (var department in AppServices.MainDataContext.Departments)
            {
                PermissionRegistry.RegisterPermission(PermissionNames.UseDepartment + department.Id, PermissionCategories.Department, department.Name);
            }
        }
    }
}
