using System.ComponentModel.Composition;
using Microsoft.Practices.Prism.MefExtensions.Modularity;
using Samba.Domain.Models.Inventories;
using Samba.Domain.Models.Settings;
using Samba.Localization.Properties;
using Samba.Persistance.Data;
using Samba.Presentation.Common;
using Samba.Services;
using Samba.Services.Common;

namespace Samba.Modules.InventoryModule
{
    [ModuleExport(typeof(InventoryModule))]
    public class InventoryModule : ModuleBase
    {
        private readonly IInventoryService _inventoryService;
        private readonly IApplicationState _applicationState;

        [ImportingConstructor]
        public InventoryModule(IInventoryService inventoryService, IApplicationState applicationState)
        {
            _inventoryService = inventoryService;
            _applicationState = applicationState;
            AddDashboardCommand<InventoryItemListViewModel>(Resources.InventoryItems, Resources.Products, 26);
            AddDashboardCommand<RecipeListViewModel>(Resources.Recipes, Resources.Products, 27);
            AddDashboardCommand<TransactionListViewModel>(Resources.Transactions, Resources.Products, 28);
            AddDashboardCommand<PeriodicConsumptionListViewModel>(Resources.EndOfDayRecords, Resources.Products, 29);

            EventServiceFactory.EventService.GetEvent<GenericEvent<WorkPeriod>>().Subscribe(OnWorkperiodStatusChanged);
        }

        private void OnWorkperiodStatusChanged(EventParameters<WorkPeriod> obj)
        {
            if (obj.Topic != EventTopicNames.WorkPeriodStatusChanged) return;
            using (var ws = WorkspaceFactory.Create())
            {
                if (ws.Count<Recipe>() <= 0) return;
                if (!_applicationState.IsCurrentWorkPeriodOpen)
                {
                    var pc = _inventoryService.GetCurrentPeriodicConsumption(ws);
                    if (pc.Id == 0) ws.Add(pc);
                    ws.CommitChanges();
                }
                else
                {
                    if (_applicationState.PreviousWorkPeriod == null) return;
                    var pc = _inventoryService.GetPreviousPeriodicConsumption(ws);
                    if (pc == null) return;
                    _inventoryService.CalculateCost(pc, _applicationState.PreviousWorkPeriod);
                    ws.CommitChanges();
                }
            }
        }

    }
}
