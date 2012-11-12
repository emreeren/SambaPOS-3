using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using Samba.Domain.Models.Inventories;
using Samba.Domain.Models.Settings;
using Samba.Domain.Models.Tickets;
using Samba.Infrastructure.Data;
using Samba.Localization.Properties;
using Samba.Persistance.Data;
using Samba.Presentation.Services.Common;
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

    [Export(typeof(IInventoryService))]
    public class InventoryService : AbstractService, IInventoryService
    {
        private readonly IApplicationState _applicationState;
        private readonly ICacheService _cacheService;

        [ImportingConstructor]
        public InventoryService(IApplicationState applicationState, ICacheService cacheService)
        {
            _applicationState = applicationState;
            _cacheService = cacheService;

            EventServiceFactory.EventService.GetEvent<GenericEvent<WorkPeriod>>().Subscribe(OnWorkperiodStatusChanged);
            ValidatorRegistry.RegisterSaveValidator(new NonDuplicateSaveValidator<InventoryItem>(string.Format(Resources.SaveErrorDuplicateItemName_f, Resources.InventoryItem)));
            ValidatorRegistry.RegisterDeleteValidator<InventoryItem>(x => Dao.Exists<PeriodicConsumptionItem>(y => y.InventoryItem.Id == x.Id), Resources.InventoryItem, Resources.EndOfDayRecord);
            ValidatorRegistry.RegisterSaveValidator(new RecipeSaveValidator());
        }

        private IEnumerable<InventoryTransactionItem> GetTransactionItems()
        {
            return Dao.Query<InventoryTransaction>(x =>
                                                   x.Date > _applicationState.CurrentWorkPeriod.StartDate,
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
                    .Where(x => (x.DecreaseInventory || x.IncreaseInventory) && recipeItemIds.Contains(x.MenuItemId));
        }

        private IEnumerable<SalesData> GetSales(WorkPeriod workPeriod)
        {
            var orders = GetOrdersFromRecipes(workPeriod).ToList();
            var salesData = orders.GroupBy(x => new { x.MenuItemName, x.MenuItemId, x.PortionName })
                    .Select(x => new SalesData { MenuItemName = x.Key.MenuItemName, MenuItemId = x.Key.MenuItemId, PortionName = x.Key.PortionName, Total = x.Sum(y => y.Quantity) }).ToList();

            var orderTagValues = orders.SelectMany(x => x.OrderTagValues, (ti, pr) => new { OrderTagValues = pr, ti.Quantity })
                    .Where(x => x.OrderTagValues.MenuItemId > 0)
                    .GroupBy(x => new { x.OrderTagValues.MenuItemId, x.OrderTagValues.PortionName });

            foreach (var orderTagValue in orderTagValues)
            {
                var tip = orderTagValue;
                var mi = _cacheService.GetMenuItem(x => x.Id == tip.Key.MenuItemId);
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
            var transactionItems = GetTransactionItems().ToList();
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
                var tim = transactionItems.Where(x => x.InventoryItem.Id == iItem.Id).ToList();
                pci.Purchase = tim.Sum(x => x.Quantity * x.Multiplier) / pci.UnitMultiplier;
                var totalPrice = tim.Sum(x => x.Price * x.Quantity);
                if (pci.InStock > 0 || pci.Purchase > 0)
                    pci.Cost = decimal.Round((totalPrice + previousCost) / (pci.InStock + pci.Purchase), 2);
            }
        }

        private void UpdateConsumption(PeriodicConsumption pc, IWorkspace workspace)
        {
            var sales = GetSales(_applicationState.CurrentWorkPeriod);

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
                WorkPeriodId = _applicationState.CurrentWorkPeriod.Id,
                Name = _applicationState.CurrentWorkPeriod.StartDate + " - " +
                       _applicationState.CurrentWorkPeriod.EndDate,
                StartDate = _applicationState.CurrentWorkPeriod.StartDate,
                EndDate = _applicationState.CurrentWorkPeriod.EndDate
            };

            CreatePeriodicConsumptionItems(pc, workspace);
            UpdateConsumption(pc, workspace);
            CalculateCost(pc, _applicationState.CurrentWorkPeriod);
            return pc;
        }

        public PeriodicConsumption GetPreviousPeriodicConsumption(IWorkspace workspace)
        {
            return _applicationState.PreviousWorkPeriod == null ? null :
               workspace.Single<PeriodicConsumption>(x => x.WorkPeriodId == _applicationState.PreviousWorkPeriod.Id);
        }

        public PeriodicConsumption GetCurrentPeriodicConsumption(IWorkspace workspace)
        {
            var pc = workspace.Single<PeriodicConsumption>(x =>
                x.WorkPeriodId == _applicationState.CurrentWorkPeriod.Id) ??
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
                        var pci = pc.PeriodicConsumptionItems.SingleOrDefault(x => x.InventoryItem.Id == item.InventoryItem.Id);
                        if (pci != null && pci.GetPredictedConsumption() > 0)
                        {
                            var cost = recipeItem.Quantity * (pci.Cost / pci.UnitMultiplier);
                            cost = (pci.GetConsumption() * cost) / pci.GetPredictedConsumption();
                            totalcost += cost;
                        }
                    }
                    var ci = pc.CostItems.SingleOrDefault(x => x.Portion.Id == recipe.Portion.Id);
                    if (ci != null) ci.Cost = decimal.Round(totalcost, 2);
                }
            }
        }

        public IEnumerable<string> GetInventoryItemNames()
        {
            return Dao.Select<InventoryItem, string>(x => x.Name, x => !string.IsNullOrEmpty(x.Name));
        }

        public IEnumerable<string> GetGroupCodes()
        {
            return Dao.Distinct<InventoryItem>(x => x.GroupCode);
        }

        private void OnWorkperiodStatusChanged(EventParameters<WorkPeriod> obj)
        {
            if (obj.Topic != EventTopicNames.WorkPeriodStatusChanged) return;
            using (var ws = WorkspaceFactory.Create())
            {
                if (ws.Count<Recipe>() <= 0) return;
                if (!_applicationState.IsCurrentWorkPeriodOpen)
                {
                    var pc = GetCurrentPeriodicConsumption(ws);
                    if (pc.Id == 0) ws.Add(pc);
                    ws.CommitChanges();
                }
                else
                {
                    if (_applicationState.PreviousWorkPeriod == null) return;
                    var pc = GetPreviousPeriodicConsumption(ws);
                    if (pc == null) return;
                    CalculateCost(pc, _applicationState.PreviousWorkPeriod);
                    ws.CommitChanges();
                }
            }
        }

        public override void Reset()
        {

        }
    }

    public class RecipeSaveValidator : SpecificationValidator<Recipe>
    {
        public override string GetErrorMessage(Recipe model)
        {
            if (model.RecipeItems.Any(x => x.InventoryItem == null || x.Quantity == 0))
                return Resources.SaveErrorZeroOrNullInventoryLines;
            if (model.Portion == null)
                return Resources.APortionShouldSelected;
            if (Dao.Exists<Recipe>(x => x.Portion.Id == model.Portion.Id && x.Id != model.Id))
            {
                const string mitemName = "[Menu Item Name]"; // todo:fix;
                return string.Format(Resources.ThereIsAnotherRecipeFor_f, mitemName);
            }
            return "";
        }
    }
}
