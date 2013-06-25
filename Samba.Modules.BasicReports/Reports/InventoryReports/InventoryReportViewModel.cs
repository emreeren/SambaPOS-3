using System;
using System.Linq;
using System.Windows;
using System.Windows.Documents;
using Samba.Domain.Models.Inventory;
using Samba.Infrastructure.Settings;
using Samba.Localization.Properties;
using Samba.Presentation.Common;
using Samba.Presentation.Services;
using Samba.Services;

namespace Samba.Modules.BasicReports.Reports.InventoryReports
{
    class InventoryReportViewModel : ReportViewModelBase
    {
        private readonly ICacheService _cacheService;

        public InventoryReportViewModel(IUserService userService, IApplicationState applicationState, ICacheService cacheService, ILogService logService)
            : base(userService, applicationState, logService)
        {
            _cacheService = cacheService;
        }

        protected override void CreateFilterGroups()
        {
            FilterGroups.Clear();
            FilterGroups.Add(CreateWorkPeriodFilterGroup());
        }

        protected override FlowDocument GetReport()
        {
            var report = new SimpleReport("8cm");
            report.AddHeader("Samba POS");
            report.AddHeader(Resources.InventoryReport);
            report.AddHeader(string.Format(Resources.As_f, DateTime.Now));

            var lastPeriodicConsumption = ReportContext.GetCurrentPeriodicConsumption();

            foreach (var warehouseConsumption in lastPeriodicConsumption.WarehouseConsumptions.OrderBy(GetWarehouseOrder))
            {
                var warehouse =
                    _cacheService.GetWarehouses().SingleOrDefault(x => x.Id == warehouseConsumption.WarehouseId) ??
                    Warehouse.Undefined;
                var inventoryTableSlug = "InventoryTable_" + warehouseConsumption.WarehouseId;
                report.AddColumTextAlignment(inventoryTableSlug, TextAlignment.Left, TextAlignment.Left, TextAlignment.Right);
                report.AddColumnLength(inventoryTableSlug, "55*", "15*", "30*");
                report.AddTable(inventoryTableSlug, warehouse.Name, "", "");

                foreach (var periodicConsumptionItem in warehouseConsumption.PeriodicConsumptionItems)
                {
                    report.AddRow(inventoryTableSlug,
                        periodicConsumptionItem.InventoryItemName,
                        periodicConsumptionItem.UnitName,
                        periodicConsumptionItem.GetPhysicalInventory().ToString(LocalSettings.ReportQuantityFormat));
                }
            }

            return report.Document;
        }

        private int GetWarehouseOrder(WarehouseConsumption arg)
        {
            var warehouse =
                _cacheService.GetWarehouses().SingleOrDefault(x => x.Id == arg.WarehouseId) ??
                Warehouse.Undefined;
            return warehouse.SortOrder;
        }

        protected override string GetHeader()
        {
            return Resources.InventoryReport;
        }
    }
}
