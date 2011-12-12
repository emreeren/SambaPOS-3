using System;
using System.ComponentModel.Composition;
using Microsoft.Practices.Prism.MefExtensions.Modularity;
using Microsoft.Practices.Prism.Modularity;
using Samba.Domain.Models.Inventories;
using Samba.Domain.Models.Settings;
using Samba.Localization.Properties;
using Samba.Modules.InventoryModule.ServiceImplementations;
using Samba.Persistance.Data;
using Samba.Presentation.Common;
using Samba.Presentation.Common.ModelBase;
using Samba.Services;

namespace Samba.Modules.InventoryModule
{
    [ModuleExport(typeof(InventoryModule))]
    public class InventoryModule : ModuleBase
    {
        private readonly IWorkPeriodService _workPeriodService;
        private readonly IInventoryService _inventoryService;

        [ImportingConstructor]
        public InventoryModule(IWorkPeriodService workPeriodService, IInventoryService inventoryService)
        {
            _workPeriodService = workPeriodService;
            _inventoryService = inventoryService;
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
                if (!_workPeriodService.IsCurrentWorkPeriodOpen)
                {
                    var pc = _inventoryService.GetCurrentPeriodicConsumption(ws);
                    if (pc.Id == 0) ws.Add(pc);
                    ws.CommitChanges();
                }
                else
                {
                    if (_workPeriodService.PreviousWorkPeriod == null) return;
                    var pc = _inventoryService.GetPreviousPeriodicConsumption(ws);
                    if (pc == null) return;
                    _inventoryService.CalculateCost(pc, _workPeriodService.PreviousWorkPeriod);
                    ws.CommitChanges();
                }
            }
        }

    }
}
