using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using Microsoft.Practices.Prism.MefExtensions.Modularity;
using Microsoft.Practices.Prism.Regions;
using Samba.Domain.Models.Entities;
using Samba.Domain.Models.Inventory;
using Samba.Localization.Properties;
using Samba.Presentation.Common;
using Samba.Presentation.Common.ErrorReport;
using Samba.Presentation.Common.ModelBase;
using Samba.Presentation.Services;
using Samba.Presentation.Services.Common;
using Samba.Services;

namespace Samba.Modules.InventoryModule
{
    [ModuleExport(typeof(InventoryModule))]
    public class InventoryModule : VisibleModuleBase
    {
        private readonly IRegionManager _regionManager;
        private readonly ICacheService _cacheService;
        private readonly IUserService _userService;
        private readonly IInventoryService _inventoryService;
        private readonly WarehouseInventoryView _warehouseInventoryView;
        private readonly WarehouseInventoryViewModel _warehouseInventoryViewModel;
        private readonly ILogService _logService;

        [ImportingConstructor]
        public InventoryModule(IRegionManager regionManager, ICacheService cacheService, IUserService userService, IInventoryService inventoryService,
            WarehouseInventoryView resourceInventoryView, WarehouseInventoryViewModel resourceInventoryViewModel, ILogService logService)
            : base(regionManager, AppScreens.InventoryView)
        {
            _regionManager = regionManager;
            _cacheService = cacheService;
            _userService = userService;
            _inventoryService = inventoryService;
            _warehouseInventoryView = resourceInventoryView;
            _warehouseInventoryViewModel = resourceInventoryViewModel;
            _logService = logService;

            AddDashboardCommand<EntityCollectionViewModelBase<WarehouseTypeViewModel, WarehouseType>>(Resources.WarehouseType.ToPlural(), Resources.Inventory, 35);
            AddDashboardCommand<EntityCollectionViewModelBase<WarehouseViewModel, Warehouse>>(Resources.Warehouse.ToPlural(), Resources.Inventory, 35);
            AddDashboardCommand<EntityCollectionViewModelBase<TransactionTypeViewModel, InventoryTransactionType>>(Resources.TransactionType.ToPlural(), Resources.Inventory, 35);
            AddDashboardCommand<EntityCollectionViewModelBase<TransactionDocumentTypeViewModel, InventoryTransactionDocumentType>>(Resources.DocumentType.ToPlural(), Resources.Inventory, 35);
            AddDashboardCommand<TransactionDocumentListViewModel>(Resources.Transaction.ToPlural(), Resources.Inventory, 35);

            AddDashboardCommand<EntityCollectionViewModelBase<InventoryItemViewModel, InventoryItem>>(Resources.InventoryItems, Resources.Inventory, 35);
            AddDashboardCommand<RecipeListViewModel>(Resources.Recipes, Resources.Inventory, 35);
            AddDashboardCommand<PeriodicConsumptionListViewModel>(Resources.EndOfDayRecords, Resources.Inventory, 36);

            SetNavigationCommand(Resources.Warehouses, Resources.Common, "Images/box.png", 40);

            EventServiceFactory.EventService.GetEvent<GenericEvent<Entity>>().Subscribe(OnResourceEvent);
            EventServiceFactory.EventService.GetEvent<GenericEvent<Warehouse>>().Subscribe(OnWarehouseEvent);

            PermissionRegistry.RegisterPermission(PermissionNames.OpenInventory, PermissionCategories.Navigation, string.Format(Resources.CanNavigate_f, Resources.Inventory));
        }

        private void OnResourceEvent(EventParameters<Entity> obj)
        {
            if (obj.Topic == EventTopicNames.DisplayInventory)
            {
                var warehouse = _cacheService.GetWarehouses().Single(x => x.Id == obj.Value.WarehouseId);
                _warehouseInventoryViewModel.Refresh(warehouse.Id);
                ActivateInventoryView();
            }
        }


        private void OnWarehouseEvent(EventParameters<Warehouse> obj)
        {
            if (obj.Topic == EventTopicNames.DisplayInventory)
            {
                _warehouseInventoryViewModel.Refresh(obj.Value.Id);
                ActivateInventoryView();
            }
        }

        protected override bool CanNavigate(string arg)
        {
            return _userService.IsUserPermittedFor(PermissionNames.OpenInventory);
        }

        protected override void OnNavigate(string obj)
        {
            base.OnNavigate(obj);
            try
            {
                _warehouseInventoryViewModel.Refresh(_cacheService.GetWarehouses().First().Id);
            }
            catch (Exception e)
            {
                var exceptionMessage = new StringBuilder(Resources.InventoryCalculationError);
                var requiredRecipes = _inventoryService.GetRequiredRecipesForSales().ToList();
                if (requiredRecipes.Any())
                {
                    exceptionMessage.AppendLine(" ");
                    foreach (var requiredRecipe in requiredRecipes)
                    {
                        exceptionMessage.AppendLine(requiredRecipe);
                    }
                }
                var list = new List<Exception> { new Exception(exceptionMessage.ToString()), e };
                ExceptionReporter.Show(list.ToArray());
            }
        }

        protected override void OnInitialization()
        {
            _regionManager.RegisterViewWithRegion(RegionNames.MainRegion, typeof(WarehouseInventoryView));
        }

        private void ActivateInventoryView()
        {
            _regionManager.ActivateRegion(RegionNames.MainRegion, _warehouseInventoryView);
        }

        public override object GetVisibleView()
        {
            return _warehouseInventoryView;
        }
    }
}
