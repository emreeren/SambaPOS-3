using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using Samba.Domain.Models.Resources;
using Samba.Infrastructure;
using Samba.Presentation.Common;
using Samba.Services;
using Samba.Services.Common;

namespace Samba.Modules.ResourceModule.Widgets.ResourceGrid
{
    public class ResourceGridWidgetViewModel : WidgetViewModel
    {
        private readonly IApplicationState _applicationState;
        private readonly ICacheService _cacheService;

        public ResourceGridWidgetViewModel(Widget model, IApplicationState applicationState, IApplicationStateSetter applicationStateSetter,
            IResourceService resourceService, IUserService userService, ICacheService cacheService)
            : base(model)
        {
            _applicationState = applicationState;
            _cacheService = cacheService;
            ResourceSelectorViewModel = new ResourceSelectorViewModel(applicationState, applicationStateSetter, resourceService, userService, cacheService);
        }

        readonly EntityOperationRequest<Resource> _request = new EntityOperationRequest<Resource>(null, EventTopicNames.ResourceSelected);

        [Browsable(false)]
        public ResourceGridWidgetSettings Settings { get { return SettingsObject as ResourceGridWidgetSettings; } }
        [Browsable(false)]
        public ResourceSelectorViewModel ResourceSelectorViewModel { get; private set; }

        protected override object CreateSettingsObject()
        {
            return JsonHelper.Deserialize<ResourceGridWidgetSettings>(_model.Properties);
        }

        protected override void BeforeEditSettings()
        {
            Settings.StateFilterNameValue.UpdateValues(_cacheService.GetResourceStates().Select(x => x.Name));
        }

        public override void Refresh()
        {
            var stateFilter = _cacheService.GetResourceStateByName(Settings.StateFilterName);
            var stateFilterId = stateFilter != null ? stateFilter.Id : _applicationState.SelectedResourceScreen.StateFilterId;
            ResourceSelectorViewModel.Refresh(_applicationState.SelectedResourceScreen, stateFilterId, _request);
        }
    }
}
