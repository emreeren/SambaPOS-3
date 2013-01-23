using System.ComponentModel.Composition;
using Microsoft.Practices.Prism.MefExtensions.Modularity;
using Microsoft.Practices.Prism.Regions;
using Samba.Domain.Models.Inventory;
using Samba.Localization.Properties;
using Samba.Presentation.Common;
using Samba.Presentation.Common.ModelBase;
using Samba.Presentation.Services.Common;

namespace Samba.Modules.WarehouseModule
{
    [ModuleExport(typeof(WarehouseModule))]
    public class WarehouseModule : VisibleModuleBase
    {
        private readonly IRegionManager _regionManager;
        private readonly WarehouseModuleView _warehouseModuleView;

        [ImportingConstructor]
        public WarehouseModule(IRegionManager regionManager, WarehouseModuleView warehouseModuleView)
            : base(regionManager, AppScreens.WarehouseView)
        {
            _regionManager = regionManager;
            _warehouseModuleView = warehouseModuleView;
            AddDashboardCommand<EntityCollectionViewModelBase<WarehouseTypeViewModel, WarehouseType>>(Resources.WarehouseType.ToPlural(), Resources.Inventory, 35);
            AddDashboardCommand<EntityCollectionViewModelBase<WarehouseViewModel, Warehouse>>(Resources.Warehouse.ToPlural(), Resources.Inventory, 35);
            AddDashboardCommand<EntityCollectionViewModelBase<TransactionTypeViewModel, InventoryTransactionType>>(Resources.TransactionType.ToPlural(), Resources.Inventory, 35);
            AddDashboardCommand<EntityCollectionViewModelBase<TransactionDocumentTypeViewModel, InventoryTransactionDocumentType>>(Resources.DocumentType.ToPlural(), Resources.Inventory, 35);
            AddDashboardCommand<TransactionListViewModel>(Resources.Transaction.ToPlural(), Resources.Inventory, 35);

            SetNavigationCommand(Resources.Warehouse, Resources.Common, "Images/dcn.png",30);
        }

        protected override void OnInitialization()
        {
            _regionManager.RegisterViewWithRegion(RegionNames.MainRegion, typeof(WarehouseModuleView));
        }

        public override object GetVisibleView()
        {
            return _warehouseModuleView;
        }
    }
}
