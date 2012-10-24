using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Samba.Domain.Models.Resources;
using Samba.Presentation.Common;
using Samba.Services;
using Samba.Services.Common;

namespace Samba.Modules.ResourceModule.Widgets.ResourceSearch
{
    class ResourceSearchWidgetViewModel : WidgetViewModel
    {
        private readonly IApplicationState _applicationState;

        public ResourceSearchWidgetViewModel(Widget model, IApplicationState applicationState, ICacheService cacheService, IResourceService resourceService)
            : base(model)
        {
            _applicationState = applicationState;
            ResourceSearchViewModel = new ResourceSearchViewModel(applicationState, cacheService, resourceService);
        }

        public ResourceSearchViewModel ResourceSearchViewModel { get; set; }
        readonly EntityOperationRequest<Resource> _request = new EntityOperationRequest<Resource>(null, EventTopicNames.ResourceSelected);

        protected override object CreateSettingsObject()
        {
            return null;
        }

        public override void Refresh()
        {
            ResourceSearchViewModel.Refresh(_applicationState.SelectedResourceScreen, _request);
        }
    }
}
