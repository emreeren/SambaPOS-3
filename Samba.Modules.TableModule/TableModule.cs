using System.ComponentModel.Composition;
using Microsoft.Practices.Prism.MefExtensions.Modularity;
using Microsoft.Practices.Prism.Regions;
using Samba.Domain.Models.Tickets;
using Samba.Localization.Properties;
using Samba.Presentation.Common;
using Samba.Presentation.Common.ModelBase;
using Samba.Services;

namespace Samba.Modules.TableModule
{
    [ModuleExport(typeof(TableModule))]
    public class TableModule : VisibleModuleBase
    {
        private readonly IRegionManager _regionManager;
        private readonly TableSelectorView _tableSelectorView;

        [ImportingConstructor]
        public TableModule(IRegionManager regionManager, TableSelectorView tableSelectorView)
            : base(regionManager, AppScreens.TableList)
        {
            _regionManager = regionManager;
            _tableSelectorView = tableSelectorView;

            AddDashboardCommand<TableListViewModel>(Resources.TableList, Resources.Tables, 30);
            AddDashboardCommand<TableScreenListViewModel>(Resources.TableViews, Resources.Tables);
        }

        public override object GetVisibleView()
        {
            return _tableSelectorView;
        }

        protected override void OnInitialization()
        {
            _regionManager.RegisterViewWithRegion(RegionNames.MainRegion, typeof(TableSelectorView));
            PermissionRegistry.RegisterPermission(PermissionNames.OpenTables, PermissionCategories.Navigation, Resources.CanOpenTableList);
            PermissionRegistry.RegisterPermission(PermissionNames.ChangeTable, PermissionCategories.Ticket, Resources.CanChangeTable);

            EventServiceFactory.EventService.GetEvent<GenericEvent<Department>>().Subscribe(x =>
            {
                if (x.Topic == EventTopicNames.SelectTable)
                {
                    ActivateTableView();
                }
            });
        }

        private void ActivateTableView()
        {
            Activate();
            ((TableSelectorViewModel)_tableSelectorView.DataContext).IsNavigated = false;
        }
    }
}
