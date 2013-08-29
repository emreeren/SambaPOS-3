using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using Samba.Domain.Models.Inventory;
using Samba.Domain.Models.Settings;
using Samba.Domain.Models.Tickets;
using Samba.Persistance;
using Samba.Persistance.Data;
using Samba.Services;
using Samba.Services.Common;

namespace Samba.Presentation.Services.Implementations.InventoryModule
{
    internal class SalesData
    {
        public string MenuItemName { get; set; }
        public int MenuItemId { get; set; }
        public string PortionName { get; set; }
        public decimal Total { get; set; }
    }

    [Export(typeof(IWorkPeriodProcessor))]
    public class InventoryWorkperiodProcessor : IWorkPeriodProcessor
    {
        private readonly IInventoryService _inventoryService;

        [ImportingConstructor]
        public InventoryWorkperiodProcessor(IInventoryService inventoryService)
        {
            _inventoryService = inventoryService;
        }

        public void ProcessWorkPeriodStart(WorkPeriod workPeriod)
        {
            _inventoryService.DoWorkPeriodStart();
        }

        public void ProcessWorkPeriodEnd(WorkPeriod workPeriod)
        {
            _inventoryService.DoWorkPeriodEnd();
        }
    }

    [Export(typeof(IInventoryService))]
    public class InventoryService : IInventoryService
    {
        private readonly IInventoryDao _inventoryDao;
        private readonly IApplicationState _applicationState;
        private readonly ICacheService _cacheService;
        private readonly IMenuService _menuService;

        [ImportingConstructor]
        public InventoryService(IInventoryDao inventoryDao, IApplicationState applicationState, ICacheService cacheService, IMenuService menuService)
        {
            _inventoryDao = inventoryDao;
            _applicationState = applicationState;
            _cacheService = cacheService;
            _menuService = menuService;
        }

        private IEnumerable<InventoryTransaction> GetTransactionItems(int warehouseId)
        {
            return _inventoryDao.GetTransactionItems(_applicationState.CurrentWorkPeriod.StartDate, warehouseId);
        }

        private IEnumerable<InventoryTransaction> GetTransactionItems(InventoryItem inventoryItem, Warehouse warehouse)
        {
            return _inventoryDao.GetTransactionItems(_applicationState.CurrentWorkPeriod.StartDate, inventoryItem.Id, warehouse.Id);
        }

        private IEnumerable<Order> GetOrdersFromRecipes(WorkPeriod workPeriod, int warehouseId)
        {
            return _inventoryDao.GetOrdersFromRecipes(workPeriod.StartDate, warehouseId);
        }

        private IEnumerable<Order> GetOrdersFromRecipes(WorkPeriod workPeriod, int inventoryItemId, int warehouseId)
        {
            return _inventoryDao.GetOrdersFromRecipes(workPeriod.StartDate, inventoryItemId, warehouseId);
        }

        private IEnumerable<SalesData> GetSales(WorkPeriod workPeriod, int inventoryItemId, int warehouseId)
        {
            var orders = GetOrdersFromRecipes(workPeriod, inventoryItemId, warehouseId).ToList();
            return GetSaleTransactions(orders);
        }

        private IEnumerable<SalesData> GetSales(WorkPeriod workPeriod, int warehouseId)
        {
            var orders = GetOrdersFromRecipes(workPeriod, warehouseId).ToList();
            return GetSaleTransactions(orders);
        }

        private IEnumerable<SalesData> GetSaleTransactions(List<Order> orders)
        {
            var salesData = orders.GroupBy(x => new { x.MenuItemName, x.MenuItemId, x.PortionName })
                                  .Select(
                                      x =>
                                      new SalesData
                                      {
                                          MenuItemName = x.Key.MenuItemName,
                                          MenuItemId = x.Key.MenuItemId,
                                          PortionName = x.Key.PortionName,
                                          Total = x.Sum(y => y.Quantity)
                                      }).ToList();

            var orderTagValues = orders.SelectMany(x => x.GetOrderTagValues(),
                                                   (order, ot) => new { OrderTagValues = ot, order.Quantity })
                                       .Where(x => x.OrderTagValues.MenuItemId > 0)
                                       .GroupBy(x => new { x.OrderTagValues.MenuItemId, x.OrderTagValues.PortionName });

            foreach (var orderTagValue in orderTagValues)
            {
                var tip = orderTagValue;
                var mi = _cacheService.GetMenuItem(x => x.Id == tip.Key.MenuItemId);
                var port = mi.Portions.FirstOrDefault(x => x.Name == tip.Key.PortionName) ?? mi.Portions[0];
                var sd =
                    salesData.SingleOrDefault(
                        x => x.MenuItemId == mi.Id && x.MenuItemName == mi.Name && x.PortionName == port.Name) ??
                    new SalesData();
                sd.MenuItemId = mi.Id;
                sd.MenuItemName = mi.Name;
                sd.PortionName = port.Name;
                sd.Total += tip.Sum(x => x.OrderTagValues.Quantity * x.Quantity);
                if (!salesData.Contains(sd))
                    salesData.Add(sd);
            }

            return salesData;
        }

        private void UpdateConsumption(PeriodicConsumption pc, int warehouseId)
        {
            var sales = GetSales(_applicationState.CurrentWorkPeriod, warehouseId);
            foreach (var sale in sales)
            {
                var portionName = sale.PortionName;
                var menuItemId = sale.MenuItemId;
                var recipe = _cacheService.GetRecipe(portionName, menuItemId);
                pc.UpdateConsumption(recipe, sale.Total, warehouseId);
                pc.CreateCostItem(recipe, sale.MenuItemName, sale.Total, warehouseId);
            }
        }

        public IEnumerable<string> GetRequiredRecipesForSales()
        {
            var result = new List<string>();
            var wids = _cacheService.GetWarehouses().Select(x => x.Id).ToList();
            foreach (var wid in wids)
            {
                var sales = GetSales(_applicationState.CurrentWorkPeriod, wid);
                foreach (var sale in sales)
                {
                    var portionName = sale.PortionName;
                    var menuItemId = sale.MenuItemId;
                    try
                    {
                        _cacheService.GetRecipe(portionName, menuItemId);
                    }
                    catch (Exception)
                    {
                        result.Add(sale.MenuItemName + "." + sale.PortionName);
                    }
                }
            }
            return result;
        }

        public IEnumerable<string> GetMissingRecipes()
        {
            var result = new List<string>();
            var menuItems = _menuService.GetMenuItemsWithPortions();
            foreach (var menuItem in menuItems)
            {
                foreach (var portion in menuItem.Portions)
                {
                    try
                    {
                        _cacheService.GetRecipe(portion.Name, menuItem.Id);
                    }
                    catch (Exception)
                    {
                        result.Add(menuItem.Name + "." + portion.Name);
                    }
                }
            }
            return result;
        }

        private PeriodicConsumption CreateNewPeriodicConsumption(bool filter)
        {
            var wids = _cacheService.GetWarehouses().Select(x => x.Id).ToList();
            var previousPc = GetPreviousPeriodicConsumption();
            var pc = PeriodicConsumption.Create(_applicationState.CurrentWorkPeriod, wids);
            var inventoryItems = _inventoryDao.GetInventoryItems().ToList();
            foreach (var wid in wids)
            {
                var transactionItems = GetTransactionItems(wid).ToList();
                pc.CreatePeriodicConsumptionItems(wid, inventoryItems, previousPc, transactionItems);
                UpdateConsumption(pc, wid);
                CalculateCost(pc, _applicationState.CurrentWorkPeriod);
            }

            if (filter) Filter(pc, inventoryItems, true);

            return pc;
        }

        public void FilterUnneededItems(PeriodicConsumption pc)
        {
            var inventoryItems = _inventoryDao.GetInventoryItems();
            Filter(pc, inventoryItems, false);
        }

        public void AddMissingItems(WarehouseConsumption whc)
        {
            var inventoryItems = _inventoryDao.GetInventoryItems();
            foreach (var inventoryItem in inventoryItems.Where(inventoryItem => whc.PeriodicConsumptionItems.All(x => x.InventoryItemId != inventoryItem.Id)))
            {
                whc.PeriodicConsumptionItems.Add(PeriodicConsumptionItem.Create(inventoryItem));
            }
        }

        private void Filter(PeriodicConsumption pc, IEnumerable<InventoryItem> inventoryItems, bool keepMappedItems)
        {
            foreach (var warehouseConsumption in pc.WarehouseConsumptions)
            {
                var warehouse = _cacheService.GetWarehouses().Single(x => x.Id == warehouseConsumption.WarehouseId);
                var items =
                    warehouseConsumption.PeriodicConsumptionItems.Where(
                        x => x.InStock == 0 && x.Consumption == 0 && x.Added == 0 && x.Removed == 0 && x.PhysicalInventory == null);
                var removingItems = keepMappedItems ? items.Where(x => !ShouldKeep(x, inventoryItems, warehouse)).ToList() : items.ToList();
                if (removingItems.Any())
                {
                    removingItems.ForEach(x => warehouseConsumption.PeriodicConsumptionItems.Remove(x));
                }
            }
        }

        private bool ShouldKeep(PeriodicConsumptionItem periodicConsumptionItem, IEnumerable<InventoryItem> inventoryItems, Warehouse warehouse)
        {
            var wname = warehouse.Name;
            var inventoryItem = inventoryItems.Single(y => y.Id == periodicConsumptionItem.InventoryItemId);
            return inventoryItem.IsMappedToWarehouse(wname);
        }

        public PeriodicConsumption GetPreviousPeriodicConsumption()
        {
            return _applicationState.PreviousWorkPeriod == null
                       ? null
                       : _inventoryDao.GetPeriodicConsumptionByWorkPeriodId(_applicationState.PreviousWorkPeriod.Id);
        }

        public PeriodicConsumption GetCurrentPeriodicConsumption()
        {
            var pc = _inventoryDao.GetPeriodicConsumptionByWorkPeriodId(_applicationState.CurrentWorkPeriod.Id)
                ?? CreateNewPeriodicConsumption(true);
            return pc;
        }

        public void CalculateCost(PeriodicConsumption pc, WorkPeriod workPeriod)
        {
            foreach (var warehouseConsumption in pc.WarehouseConsumptions)
            {
                var recipes = GetSales(workPeriod, warehouseConsumption.WarehouseId).Select(sale => _cacheService.GetRecipe(sale.PortionName, sale.MenuItemId));
                pc.UpdateFinalCost(recipes.ToList(), warehouseConsumption.WarehouseId);
            }
        }

        public IEnumerable<string> GetInventoryItemNames()
        {
            return _inventoryDao.GetInventoryItemNames();
        }

        public IEnumerable<string> GetGroupCodes()
        {
            return _inventoryDao.GetGroupCodes();
        }

        public void SavePeriodicConsumption(PeriodicConsumption pc)
        {
            Dao.Save(pc);
        }

        public decimal GetInventory(InventoryItem inventoryItem, Warehouse warehouse)
        {
            var previousInventory = 0m;
            if (_applicationState.PreviousWorkPeriod != null)
            {
                var ppci = _inventoryDao.GetPeriodConsumptionItem(_applicationState.PreviousWorkPeriod.Id, inventoryItem.Id, warehouse.Id);
                previousInventory = ppci != null ? ppci.GetPhysicalInventory() : 0;
            }

            var transactions = GetTransactionItems(inventoryItem, warehouse).ToList();
            var positiveSum = transactions.Where(x => x.TargetWarehouseId == warehouse.Id).Sum(y => (y.Quantity * y.Multiplier) / inventoryItem.Multiplier);
            var negativeSum = transactions.Where(x => x.SourceWarehouseId == warehouse.Id).Sum(y => (y.Quantity * y.Multiplier) / inventoryItem.Multiplier);

            var currentConsumption = (
                from sale in GetSales(_applicationState.CurrentWorkPeriod, inventoryItem.Id, warehouse.Id)
                let recipe = _cacheService.GetRecipe(sale.PortionName, sale.MenuItemId)
                let rip = recipe.RecipeItems.Where(x => x.InventoryItem.Id == inventoryItem.Id)
                select (rip.Sum(x => x.Quantity) * sale.Total) / (inventoryItem.Multiplier)).Sum();
            var cpci = _inventoryDao.GetPeriodConsumptionItem(_applicationState.CurrentWorkPeriod.Id, inventoryItem.Id, warehouse.Id);
            var currentInventory = cpci != null ? cpci.GetPhysicalInventory() : 0;
            return currentInventory + previousInventory + (positiveSum - negativeSum) - currentConsumption;
        }

        public void DoWorkPeriodStart()
        {
            if (_applicationState.PreviousWorkPeriod == null) return;
            if (!_inventoryDao.RecipeExists()) return;
            UpdatePeriodicConsumptionCost();
        }

        public void DoWorkPeriodEnd()
        {
            if (!_inventoryDao.RecipeExists()) return;
            var pc = GetCurrentPeriodicConsumption();
            FilterUnneededItems(pc);
            SavePeriodicConsumption(pc);
        }

        public IEnumerable<string> GetWarehouseNames()
        {
            return _inventoryDao.GetWarehouseNames();
        }

        private void UpdatePeriodicConsumptionCost()
        {
            var pc = GetPreviousPeriodicConsumption();
            if (pc == null) return;
            CalculateCost(pc, _applicationState.PreviousWorkPeriod);
            SavePeriodicConsumption(pc);
        }
    }
}
