using System.ComponentModel.Composition;
using Microsoft.Practices.Prism.MefExtensions.Modularity;
using Microsoft.Practices.Prism.Regions;
using Samba.Localization.Properties;
using Samba.Presentation.Common;
using Samba.Presentation.Services.Common;

namespace Samba.Modules.MarketModule
{
    [ModuleExport(typeof(MarketModule))]
    public class MarketModule : VisibleModuleBase
    {
        private readonly IRegionManager _regionManager;
        private readonly MarketModuleView _marketModuleView;
        private readonly MarketModuleViewModel _marketModuleViewModel;

        [ImportingConstructor]
        public MarketModule(IRegionManager regionManager, MarketModuleView marketModuleView,MarketModuleViewModel marketModuleViewModel)
            : base(regionManager, AppScreens.MarketView)
        {
            _regionManager = regionManager;
            _marketModuleView = marketModuleView;
            _marketModuleViewModel = marketModuleViewModel;

            SetNavigationCommand(Resources.SambaMarket, Resources.Common, "Images/dcn.png", 50);
        }

        protected override void OnInitialization()
        {
            _regionManager.RegisterViewWithRegion(RegionNames.MainRegion, typeof(MarketModuleView));
        }

        protected override void OnNavigate(string obj)
        {
            base.OnNavigate(obj);
            _marketModuleViewModel.ActiveUrl = "about:blank";
            _marketModuleViewModel.ActiveUrl = "http://www.sambamarket.com/";
        }

        public override object GetVisibleView()
        {
            return _marketModuleView;
        }
    }
}
