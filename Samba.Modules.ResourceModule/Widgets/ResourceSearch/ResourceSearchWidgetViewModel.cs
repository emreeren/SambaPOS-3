using System.ComponentModel;
using System.Linq;
using Samba.Domain.Models.Resources;
using Samba.Infrastructure.Helpers;
using Samba.Presentation.Common;
using Samba.Presentation.Services;
using Samba.Presentation.Services.Common;
using Samba.Services;

namespace Samba.Modules.ResourceModule.Widgets.ResourceSearch
{
    class ResourceSearchWidgetViewModel : WidgetViewModel
    {
        private readonly IApplicationState _applicationState;
        private readonly ICacheService _cacheService;
        private EntityOperationRequest<Resource> _request = new EntityOperationRequest<Resource>(null, EventTopicNames.ResourceSelected);

        public ResourceSearchWidgetViewModel(Widget model, IApplicationState applicationState, ICacheService cacheService, IResourceService resourceService)
            : base(model, applicationState)
        {
            _applicationState = applicationState;
            _cacheService = cacheService;
            ResourceSearchViewModel = new ResourceSearchViewModel(applicationState, cacheService, resourceService) { IsKeyboardVisible = false };
            EventServiceFactory.EventService.GetEvent<GenericEvent<EntityOperationRequest<Resource>>>().Subscribe(x =>
            {
                if (x.Topic == EventTopicNames.SelectResource)
                {
                    _request = x.Value;
                }
            });
        }

        [Browsable(false)]
        public ResourceSearchWidgetSettings Settings { get { return SettingsObject as ResourceSearchWidgetSettings; } }
        [Browsable(false)]
        public ResourceSearchViewModel ResourceSearchViewModel { get; private set; }

        protected override object CreateSettingsObject()
        {
            return JsonHelper.Deserialize<ResourceSearchWidgetSettings>(_model.Properties);
        }

        protected override void BeforeEditSettings()
        {
            Settings.ResourceTypeNameValue.UpdateValues(_cacheService.GetResourceTypes().Select(x => x.EntityName));
            Settings.StateFilterNameValue.UpdateValues(_cacheService.GetResourceStates().Select(x => x.Name));
        }

        public override void Refresh()
        {
            ResourceSearchViewModel.IsKeyboardVisible = Settings.IsKeyboardVisible;
            var resourceTypeId = _cacheService.GetResourceTypeIdByEntityName(Settings.ResourceTypeName);
            if (resourceTypeId == 0) resourceTypeId = _applicationState.SelectedResourceScreen.ResourceTypeId;
            var stateFilter = !string.IsNullOrEmpty(Settings.StateFilterName) ? (Settings.StateFilterName) : _applicationState.SelectedResourceScreen.StateFilter;
            ResourceSearchViewModel.Refresh(resourceTypeId, stateFilter, _request);
        }
    }
}
