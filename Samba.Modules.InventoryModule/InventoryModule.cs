using System.ComponentModel.Composition;
using Microsoft.Practices.Prism.MefExtensions.Modularity;
using Microsoft.Practices.Prism.Regions;
using Samba.Domain.Models.Inventory;
using Samba.Domain.Models.Resources;
using Samba.Localization.Properties;
using Samba.Presentation.Common;
using Samba.Presentation.Common.ModelBase;
using Samba.Presentation.Services.Common;

namespace Samba.Modules.InventoryModule
{
    [ModuleExport(typeof(InventoryModule))]
    public class InventoryModule : VisibleModuleBase
    {
        private readonly IRegionManager _regionManager;
        private readonly InventoryModuleView _inventoryModuleView;
        private readonly ResourceInventoryView _resourceInventoryView;
        private readonly ResourceInventoryViewModel _resourceInventoryViewModel;

        [ImportingConstructor]
        public InventoryModule(IRegionManager regionManager, InventoryModuleView inventoryModuleView,
            ResourceInventoryView resourceInventoryView, ResourceInventoryViewModel resourceInventoryViewModel)
            : base(regionManager, AppScreens.InventoryView)
        {
            _regionManager = regionManager;
            _inventoryModuleView = inventoryModuleView;
            _resourceInventoryView = resourceInventoryView;
            _resourceInventoryViewModel = resourceInventoryViewModel;

            AddDashboardCommand<EntityCollectionViewModelBase<InventoryItemViewModel, InventoryItem>>(Resources.InventoryItems, Resources.Inventory, 35);
            AddDashboardCommand<EntityCollectionViewModelBase<RecipeViewModel, Recipe>>(Resources.Recipes, Resources.Inventory, 35);
            AddDashboardCommand<PeriodicConsumptionListViewModel>(Resources.EndOfDayRecords, Resources.Inventory, 36);

            SetNavigationCommand(Resources.Inventory, Resources.Common, "Images/box.png",40);

            EventServiceFactory.EventService.GetEvent<GenericEvent<Resource>>().Subscribe(OnResourceEvent);
        }

        private void OnResourceEvent(EventParameters<Resource> obj)
        {
            if (obj.Topic == EventTopicNames.DisplayInventory)
            {
                _resourceInventoryViewModel.Refresh(obj.Value);
                ActivateInventoryView();
            }
        }

        protected override void OnInitialization()
        {
            _regionManager.RegisterViewWithRegion(RegionNames.MainRegion, typeof(InventoryModuleView));
            _regionManager.RegisterViewWithRegion(RegionNames.MainRegion, typeof(ResourceInventoryView));
        }

        private void ActivateInventoryView()
        {
            _regionManager.Regions[RegionNames.MainRegion].Activate(_resourceInventoryView);
        }

        public override object GetVisibleView()
        {
            return _inventoryModuleView;
        }
    }
}
