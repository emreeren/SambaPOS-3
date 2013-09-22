using System;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows.Threading;
using Samba.Domain.Models.Entities;
using Samba.Infrastructure.Data.Serializer;
using Samba.Infrastructure.Helpers;
using Samba.Persistance;
using Samba.Presentation.Common.Widgets;
using Samba.Presentation.Services;
using Samba.Presentation.Services.Common;
using Samba.Services;

namespace Samba.Modules.EntityModule.Widgets.EntityGrid
{
    public class EntityGridWidgetViewModel : WidgetViewModel
    {
        private readonly IApplicationState _applicationState;
        private readonly ICacheService _cacheService;
        private readonly IAutomationDao _automationDao;

        public EntityGridWidgetViewModel(Widget model, IApplicationState applicationState, IPrinterService printerService,
            IEntityService entityService, IUserService userService, ICacheService cacheService, IAutomationDao automationDao)
            : base(model, applicationState)
        {
            _applicationState = applicationState;
            _cacheService = cacheService;
            _automationDao = automationDao;
            ResourceSelectorViewModel = new EntitySelectorViewModel(applicationState, entityService, userService, cacheService, printerService);

            EventServiceFactory.EventService.GetEvent<GenericEvent<WidgetEventData>>().Subscribe(
                x =>
                {
                    if (x.Value.WidgetName == Name)
                    {
                        Settings.StateFilterName = x.Value.Value;
                        Refresh();
                    }
                });
        }

        readonly OperationRequest<Entity> _request = new OperationRequest<Entity>(null, EventTopicNames.EntitySelected);


        [Browsable(false)]
        public EntityGridWidgetSettings Settings { get { return SettingsObject as EntityGridWidgetSettings; } }
        [Browsable(false)]
        public EntitySelectorViewModel ResourceSelectorViewModel { get; private set; }

        protected override object CreateSettingsObject()
        {
            return JsonHelper.Deserialize<EntityGridWidgetSettings>(_model.Properties);
        }

        protected override void BeforeEditSettings()
        {
            _entityScreen = null;
            Settings.StateFilterNameValue.UpdateValues(_cacheService.GetStates(0).Select(x => x.Name));
            Settings.AutomationCommandNameValue.UpdateValues(_automationDao.GetAutomationCommandNames());
        }

        private EntityScreen _entityScreen;
        public EntityScreen EntityScreen
        {
            get { return _entityScreen ?? (_entityScreen = CloneResourceScreen()); }
        }

        private EntityScreen CloneResourceScreen()
        {
            var result = ObjectCloner.EntityClone(_applicationState.SelectedEntityScreen);
            result.RowCount = Settings.Rows;
            result.ColumnCount = Settings.Columns;
            result.PageCount = Settings.PageCount > 0 ? Settings.PageCount : 1;
            result.FontSize = Settings.FontSize;
            return result;
        }

        public override void Refresh()
        {
            System.Windows.Application.Current.Dispatcher.Invoke(DispatcherPriority.Normal, (Action)RefreshSync);
        }

        protected static bool IsRefreshing { get; set; }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public void RefreshSync()
        {
            var stateFilter = !string.IsNullOrEmpty(Settings.StateFilterName)
                                  ? Settings.StateFilterName
                                  : EntityScreen.StateFilter;
            if (stateFilter == "*") stateFilter = "";
            ResourceSelectorViewModel.Refresh(EntityScreen, stateFilter, _request);
            ResourceSelectorViewModel.AutomationCommandName = Settings.AutomationCommandName;
            ResourceSelectorViewModel.AutomationCommandValue = Settings.CommandValue;
        }
    }
}
