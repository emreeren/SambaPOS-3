using System;
using System.ComponentModel.Composition;
using Microsoft.Practices.Prism.MefExtensions.Modularity;
using Microsoft.Practices.Prism.Modularity;
using Samba.Domain.Models.Inventories;
using Samba.Domain.Models.Settings;
using Samba.Localization.Properties;
using Samba.Persistance.Data;
using Samba.Presentation.Common;
using Samba.Presentation.Common.ModelBase;
using Samba.Services;

namespace Samba.Modules.InventoryModule
{
    [ModuleExport(typeof(InventoryModule))]
    public class InventoryModule : ModuleBase
    {
        [ImportingConstructor]
        public InventoryModule()
        {
            AddDashboardCommand<InventoryItemListViewModel>(Resources.InventoryItems, Resources.Products, 26);
            AddDashboardCommand<RecipeListViewModel>(Resources.Recipes, Resources.Products, 27);
            AddDashboardCommand<TransactionListViewModel>(Resources.Transactions, Resources.Products, 28);
            AddDashboardCommand<PeriodicConsumptionListViewModel>(Resources.EndOfDayRecords, Resources.Products, 29);

            EventServiceFactory.EventService.GetEvent<GenericEvent<WorkPeriod>>().Subscribe(OnWorkperiodStatusChanged);
        }

        private static void OnWorkperiodStatusChanged(EventParameters<WorkPeriod> obj)
        {
            if (obj.Topic == EventTopicNames.WorkPeriodStatusChanged)
            {
                using (var ws = WorkspaceFactory.Create())
                {
                    if (ws.Count<Recipe>() > 0)
                    {
                        if (!AppServices.MainDataContext.IsCurrentWorkPeriodOpen)
                        {
                            var pc = InventoryService.GetCurrentPeriodicConsumption(ws);
                            if (pc.Id == 0) ws.Add(pc);
                            ws.CommitChanges();
                        }
                        else
                        {
                            if (AppServices.MainDataContext.PreviousWorkPeriod != null)
                            {
                                var pc = InventoryService.GetPreviousPeriodicConsumption(ws);
                                if (pc != null)
                                {
                                    InventoryService.CalculateCost(pc, AppServices.MainDataContext.PreviousWorkPeriod);
                                    ws.CommitChanges();
                                }
                            }
                        }
                    }
                }
            }
        }
    }
}
