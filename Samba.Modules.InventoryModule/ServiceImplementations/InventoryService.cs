using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.Linq;
using Samba.Domain.Models.Inventories;
using Samba.Domain.Models.Settings;
using Samba.Domain.Models.Tickets;
using Samba.Infrastructure.Data;
using Samba.Persistance.Data;
using Samba.Presentation.Common;
using Samba.Presentation.Common.Services;
using Samba.Services;

namespace Samba.Modules.InventoryModule.ServiceImplementations
{
    internal class SalesData
    {
        public string MenuItemName { get; set; }
        public int MenuItemId { get; set; }
        public string PortionName { get; set; }
        public decimal Total { get; set; }
    }

    [Export(typeof(IInventoryService))]
    public class InventoryService : AbstractService, IInventoryService
    {
        private readonly IWorkPeriodService _workPeriodService;

        [ImportingConstructor]
        public InventoryService(IWorkPeriodService workPeriodService)
        {
            _workPeriodService = workPeriodService;
        }

        private IEnumerable<InventoryTransactionItem> GetTransactionItems()
        {
            return Dao.Query<InventoryTransaction>(x =>
                                                   x.Date > _workPeriodService.CurrentWorkPeriod.StartDate,
                                                   x => x.TransactionItems,
                                                   x => x.TransactionItems.Select(y => y.InventoryItem)).SelectMany(x => x.TransactionItems);
        }

        private static IEnumerable<Order> GetOrdersFromRecipes(WorkPeriod workPeriod)
        {
            var recipeItemIds = Dao.Select<Recipe, int>(x => x.Portion.MenuItemId, x => x.Portion != null).Distinct();
            var tickets = Dao.Query<Ticket>(x => x.Date > workPeriod.StartDate,
                                            x => x.Orders,
                                            x => x.Orders.Select(y => y.OrderTagValues));
            return tickets.SelectMany(x => x.Orders)
                    .Where(x => x.DecreaseInventory && recipeItemIds.Contains(x.MenuItemId));
        }

        private static IEnumerable<SalesData> GetSales(WorkPeriod workPeriod)
        {
            var orders = GetOrdersFromRecipes(workPeriod);
            var salesData = orders.GroupBy(x => new { x.MenuItemName, x.MenuItemId, x.PortionName })
                    .Select(x => new SalesData { MenuItemName = x.Key.MenuItemName, MenuItemId = x.Key.MenuItemId, PortionName = x.Key.PortionName, Total = x.Sum(y => y.Quantity) }).ToList();

            var orderTagValues = orders.SelectMany(x => x.OrderTagValues, (ti, pr) => new { OrderTagValues = pr, ti.Quantity })
                    .Where(x => x.OrderTagValues.MenuItemId > 0)
                    .GroupBy(x => new { x.OrderTagValues.MenuItemId, x.OrderTagValues.PortionName });

            foreach (var orderTagValue in orderTagValues)
            {
                var tip = orderTagValue;
                var mi = AppServices.DataAccessService.GetMenuItem(tip.Key.MenuItemId);
                var port = mi.Portions.FirstOrDefault(x => x.Name == tip.Key.PortionName) ?? mi.Portions[0];
                var sd = salesData.SingleOrDefault(x => x.MenuItemId == mi.Id && x.MenuItemName == mi.Name && x.PortionName == port.Name) ?? new SalesData();
                sd.MenuItemId = mi.Id;
                sd.MenuItemName = mi.Name;
                sd.PortionName = port.Name;
                sd.Total += tip.Sum(x => x.OrderTagValues.Quantity * x.Quantity);
                if (!salesData.Contains(sd))
                    salesData.Add(sd);
            }

            return salesData;
        }

        private void CreatePeriodicConsumptionItems(PeriodicConsumption pc, IWorkspace workspace)
        {
            var previousPc = GetPreviousPeriodicConsumption(workspace);
            var transactionItems = GetTransactionItems();
            foreach (var inventoryItem in workspace.All<InventoryItem>())
            {
                var iItem = inventoryItem;
                var pci = new PeriodicConsumptionItem { InventoryItem = inventoryItem };
                pci.UnitMultiplier = pci.InventoryItem.TransactionUnitMultiplier > 0 ? pci.InventoryItem.TransactionUnitMultiplier : 1;
                pc.PeriodicConsumptionItems.Add(pci);
                var previousCost = 0m;
                if (previousPc != null)
                {
                    var previousPci = previousPc.PeriodicConsumptionItems.SingleOrDefault(x => x.InventoryItem.Id == iItem.Id);
                    if (previousPci != null) pci.InStock =
                        previousPci.PhysicalInventory != null
                        ? previousPci.PhysicalInventory.GetValueOrDefault(0)
                        : previousPci.GetInventoryPrediction();
                    if (previousPci != null)
                        previousCost = previousPci.Cost * pci.InStock;
                }
                var tim = transactionItems.Where(x => x.InventoryItem.Id == iItem.Id);
                pci.Purchase = tim.Sum(x => x.Quantity * x.Multiplier) / pci.UnitMultiplier;
                var totalPrice = tim.Sum(x => x.Price * x.Quantity);
                if (pci.InStock > 0 || pci.Purchase > 0)
                    pci.Cost = decimal.Round((totalPrice + previousCost) / (pci.InStock + pci.Purchase), 2);
            }
        }

        private void UpdateConsumption(PeriodicConsumption pc, IWorkspace workspace)
        {
            var sales = GetSales(_workPeriodService.CurrentWorkPeriod);

            foreach (var sale in sales)
            {
                var lSale = sale;
                var recipe = workspace.Single<Recipe>(x => x.Portion.Name == lSale.PortionName && x.Portion.MenuItemId == lSale.MenuItemId);
                if (recipe != null)
                {
                    var cost = 0m;
                    foreach (var recipeItem in recipe.RecipeItems.Where(x => x.InventoryItem != null && x.Quantity > 0))
                    {
                        var item = recipeItem;
                        var pci = pc.PeriodicConsumptionItems.Single(x => x.InventoryItem.Id == item.InventoryItem.Id);
                        pci.Consumption += (item.Quantity * sale.Total) / pci.UnitMultiplier;
                        Debug.Assert(pci.Consumption > 0);
                        cost += recipeItem.Quantity * (pci.Cost / pci.UnitMultiplier);
                    }
                    pc.CostItems.Add(new CostItem { Name = sale.MenuItemName, Portion = recipe.Portion, CostPrediction = cost, Quantity = sale.Total });
                }
            }
        }

        private PeriodicConsumption CreateNewPeriodicConsumption(IWorkspace workspace)
        {
            var pc = new PeriodicConsumption
            {
                WorkPeriodId = _workPeriodService.CurrentWorkPeriod.Id,
                Name = _workPeriodService.CurrentWorkPeriod.StartDate + " - " +
                       _workPeriodService.CurrentWorkPeriod.EndDate,
                StartDate = _workPeriodService.CurrentWorkPeriod.StartDate,
                EndDate = _workPeriodService.CurrentWorkPeriod.EndDate
            };

            CreatePeriodicConsumptionItems(pc, workspace);
            UpdateConsumption(pc, workspace);
            CalculateCost(pc, _workPeriodService.CurrentWorkPeriod);
            return pc;
        }

        public PeriodicConsumption GetPreviousPeriodicConsumption(IWorkspace workspace)
        {
            return _workPeriodService.PreviousWorkPeriod == null ? null :
               workspace.Single<PeriodicConsumption>(x => x.WorkPeriodId == _workPeriodService.PreviousWorkPeriod.Id);
        }

        public PeriodicConsumption GetCurrentPeriodicConsumption(IWorkspace workspace)
        {
            var pc = workspace.Single<PeriodicConsumption>(x =>
                x.WorkPeriodId == _workPeriodService.CurrentWorkPeriod.Id) ??
                     CreateNewPeriodicConsumption(workspace);
            return pc;
        }

        public void CalculateCost(PeriodicConsumption pc, WorkPeriod workPeriod)
        {
            var sales = GetSales(workPeriod);
            foreach (var sale in sales)
            {
                var lSale = sale;
                var recipe = Dao.Single<Recipe>(x => x.Portion.Name == lSale.PortionName && x.Portion.MenuItemId == lSale.MenuItemId, x => x.Portion, x => x.RecipeItems, x => x.RecipeItems.Select(y => y.InventoryItem));
                if (recipe != null)
                {
                    var totalcost = recipe.FixedCost;
                    foreach (var recipeItem in recipe.RecipeItems.Where(x => x.InventoryItem != null && x.Quantity > 0))
                    {
                        var item = recipeItem;
                        var pci = pc.PeriodicConsumptionItems.Single(x => x.InventoryItem.Id == item.InventoryItem.Id);
                        if (pci.GetPredictedConsumption() > 0)
                        {
                            var cost = recipeItem.Quantity * (pci.Cost / pci.UnitMultiplier);
                            cost = (pci.GetConsumption() * cost) / pci.GetPredictedConsumption();
                            totalcost += cost;
                        }
                    }
                    pc.CostItems.Single(x => x.Portion.Id == recipe.Portion.Id).Cost = decimal.Round(totalcost, 2);
                }
            }
        }

        public override void Reset()
        {

        }
    }
}
