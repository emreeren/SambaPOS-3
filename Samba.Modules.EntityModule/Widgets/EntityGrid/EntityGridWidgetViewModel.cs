using System.ComponentModel;
using System.Linq;
using Samba.Domain.Models.Entities;
using Samba.Infrastructure.Data.Serializer;
using Samba.Infrastructure.Helpers;
using Samba.Presentation.Common;
using Samba.Presentation.Services;
using Samba.Presentation.Services.Common;
using Samba.Services;

namespace Samba.Modules.EntityModule.Widgets.EntityGrid
{
    public class EntityGridWidgetViewModel : WidgetViewModel
    {
        private readonly IApplicationState _applicationState;
        private readonly ICacheService _cacheService;

        public EntityGridWidgetViewModel(Widget model, IApplicationState applicationState,
            IEntityService resourceService, IUserService userService, ICacheService cacheService)
            : base(model, applicationState)
        {
            _applicationState = applicationState;
            _cacheService = cacheService;
            ResourceSelectorViewModel = new EntitySelectorViewModel(applicationState, resourceService, userService, cacheService);
        }

        readonly EntityOperationRequest<Entity> _request = new EntityOperationRequest<Entity>(null, EventTopicNames.EntitySelected);


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
            var stateFilter = !string.IsNullOrEmpty(Settings.StateFilterName)
                                  ? Settings.StateFilterName
                                  : EntityScreen.StateFilter;
            ResourceSelectorViewModel.Refresh(EntityScreen, stateFilter, _request);
        }
    }
}
