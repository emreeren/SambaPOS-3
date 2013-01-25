using System.ComponentModel.Composition;
using Microsoft.Practices.Prism.MefExtensions.Modularity;
using Microsoft.Practices.Prism.Regions;
using Samba.Domain.Models.Menus;
using Samba.Localization.Properties;
using Samba.Presentation.Common;
using Samba.Presentation.Common.ModelBase;
using Samba.Presentation.Services.Common;

namespace Samba.Modules.MenuModule
{
    [ModuleExport(typeof(MenuModule))]
    public class MenuModule : VisibleModuleBase
    {
        private readonly IRegionManager _regionManager;
        private readonly MenuModuleView _menuModuleView;

        [ImportingConstructor]
        public MenuModule(IRegionManager regionManager, MenuModuleView menuModuleView)
            : base(regionManager, AppScreens.ProductView)
        {
            _regionManager = regionManager;
            _menuModuleView = menuModuleView;
            AddDashboardCommand<EntityCollectionViewModelBase<MenuItemViewModel, MenuItem>>(Resources.ProductList, Resources.Products, 33);
            AddDashboardCommand<EntityCollectionViewModelBase<ScreenMenuViewModel, ScreenMenu>>(Resources.MenuList, Resources.Products, 33);
            AddDashboardCommand<PriceListViewModel>(Resources.BatchPriceList, Resources.Products, 33);
            AddDashboardCommand<MenuItemPriceDefinitionListViewModel>(Resources.PriceDefinitions, Resources.Products, 33);
            AddDashboardCommand<EntityCollectionViewModelBase<TaxTemplateViewModel, TaxTemplate>>(Resources.TaxTemplate.ToPlural(), Resources.Products, 33);
            AddDashboardCommand<EntityCollectionViewModelBase<ProductTimerViewModel, ProductTimer>>(Resources.ProductTimer.ToPlural(), Resources.Products, 33);

            SetNavigationCommand(Resources.Products, Resources.Common, "Images/dcn.png", 30);
        }

        protected override void OnInitialization()
        {
            _regionManager.RegisterViewWithRegion(RegionNames.MainRegion, typeof(MenuModuleView));
        }

        public override object GetVisibleView()
        {
            return _menuModuleView;
        }
    }
}
