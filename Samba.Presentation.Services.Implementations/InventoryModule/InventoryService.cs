using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using Samba.Domain.Models.Inventory;
using Samba.Domain.Models.Resources;
using Samba.Domain.Models.Settings;
using Samba.Domain.Models.Tickets;
using Samba.Persistance.DaoClasses;
using Samba.Persistance.Data;
using Samba.Presentation.Services.Common;
using Samba.Services;

namespace Samba.Presentation.Services.Implementations.InventoryModule
{
    internal class SalesData
    {
        public string MenuItemName { get; set; }
        public int MenuItemId { get; set; }
        public string PortionName { get; set; }
        public decimal Total { get; set; }
    }

    [Export(typeof(IInventoryService))]
    public class InventoryService : IInventoryService
    {
        private readonly IInventoryDao _inventoryDao;
        private readonly IApplicationState _applicationState;
        private readonly ICacheService _cacheService;

        [ImportingConstructor]
        public InventoryService(IInventoryDao inventoryDao, IApplicationState applicationState, ICacheService cacheService)
        {
            _inventoryDao = inventoryDao;
            _applicationState = applicationState;
            _cacheService = cacheService;

            EventServiceFactory.EventService.GetEvent<GenericEvent<WorkPeriod>>().Subscribe(OnWorkperiodStatusChanged);
        }

        private IEnumerable<InventoryTransactionData> GetTransactionItems(int warehouseId)
        {
            return _inventoryDao.GetTransactionItems(_applicationState.CurrentWorkPeriod.StartDate, warehouseId);
        }

        private IEnumerable<InventoryTransactionData> GetTransactionItems(InventoryItem inventoryItem, Resource resource)
        {
            return _inventoryDao.GetTransactionItems(_applicationState.CurrentWorkPeriod.StartDate, inventoryItem.Id, resource.Id);
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
                var recipe = _inventoryDao.GetRecipe(portionName, menuItemId);
                pc.UpdateConsumption(recipe, sale.Total, warehouseId);
                pc.CreateCostItem(recipe, sale.MenuItemName, sale.Total, warehouseId);
            }
        }

        private PeriodicConsumption CreateNewPeriodicConsumption()
        {
            var wids = _cacheService.GetWarehouseResources().Select(x => x.Id).ToList();
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
            return pc;
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
                ?? CreateNewPeriodicConsumption();
            return pc;
        }

        public void CalculateCost(PeriodicConsumption pc, WorkPeriod workPeriod)
        {
            foreach (var warehouseConsumption in pc.WarehouseConsumptions)
            {
                var recipes = GetSales(workPeriod, warehouseConsumption.WarehouseId).Select(sale => _inventoryDao.GetRecipe(sale.PortionName, sale.MenuItemId));
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

        public decimal GetInventory(InventoryItem inventoryItem, Resource resource)
        {
            var previousInventory = 0m;
            if (_applicationState.PreviousWorkPeriod != null)
            {
                var ppci = _inventoryDao.GetPeriodConsumptionItem(_applicationState.PreviousWorkPeriod.Id, inventoryItem.Id, resource.Id);
                previousInventory = ppci.GetPhysicalInventory();
            }
            var transactions = GetTransactionItems(inventoryItem, resource).ToList();
            var positiveSum = transactions.Where(x => x.TargetWarehouseId == resource.Id).Sum(y => (y.InventoryTransactionItem.Quantity * y.InventoryTransactionItem.Multiplier) / inventoryItem.Multiplier);
            var negativeSum = transactions.Where(x => x.SourceWarehouseId == resource.Id).Sum(y => (y.InventoryTransactionItem.Quantity * y.InventoryTransactionItem.Multiplier) / inventoryItem.Multiplier);

            var currentConsumption = (
                from sale in GetSales(_applicationState.CurrentWorkPeriod, inventoryItem.Id, resource.Id)
                let recipe = _inventoryDao.GetRecipe(sale.PortionName, sale.MenuItemId)
                let rip = recipe.RecipeItems.Where(x => x.InventoryItem.Id == inventoryItem.Id)
                select (rip.Sum(x => x.Quantity) * sale.Total) / (inventoryItem.Multiplier)).Sum();

            return previousInventory + (positiveSum - negativeSum) - currentConsumption;
        }

        private void OnWorkperiodStatusChanged(EventParameters<WorkPeriod> obj)
        {
            if (obj.Topic != EventTopicNames.WorkPeriodStatusChanged) return;
            if (_applicationState.IsCurrentWorkPeriodOpen)
            {
                DoWorkPeriodStart();
            }
            else
            {
                DoWorkPeriodEnd();
            }
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
            CreatePeriodicConsumption();
        }

        private void UpdatePeriodicConsumptionCost()
        {
            var pc = GetPreviousPeriodicConsumption();
            if (pc == null) return;
            CalculateCost(pc, _applicationState.PreviousWorkPeriod);
            SavePeriodicConsumption(pc);
        }

        private void CreatePeriodicConsumption()
        {
            var pc = GetCurrentPeriodicConsumption();
            SavePeriodicConsumption(pc);
        }
    }
}
