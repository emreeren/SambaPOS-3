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
            AddDashboardCommand<MenuItemListViewModel>(Resources.ProductList, Resources.Products);
            AddDashboardCommand<ScreenMenuListViewModel>(Resources.MenuList, Resources.Products);
            AddDashboardCommand<PriceListViewModel>(Resources.BatchPriceList, Resources.Products);
            AddDashboardCommand<MenuItemPriceDefinitionListViewModel>(Resources.PriceDefinitions, Resources.Products);
            AddDashboardCommand<TaxTemplateListViewModel>(Resources.TaxTemplates, Resources.Products);
        }
    }
}
