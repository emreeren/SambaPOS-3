using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using Samba.Domain.Models.Resources;
using Samba.Presentation.Common;
using Samba.Services;
using Samba.Services.Common;

namespace Samba.Modules.ResourceModule.Widgets.ResourceGrid
{
    public class ResourceGridWidgetViewModel : WidgetViewModel
    {
        private readonly IApplicationState _applicationState;

        public ResourceGridWidgetViewModel(Widget model, IApplicationState applicationState, IApplicationStateSetter applicationStateSetter,
            IResourceService resourceService, IUserService userService, ICacheService cacheService)
            : base(model)
        {
            _applicationState = applicationState;
            ResourceSelectorViewModel = new ResourceSelectorViewModel(applicationState, applicationStateSetter, resourceService, userService, cacheService);
        }

        readonly EntityOperationRequest<Resource> _request = new EntityOperationRequest<Resource>(null, EventTopicNames.ResourceSelected);

        [Browsable(false)]
        public ResourceSelectorViewModel ResourceSelectorViewModel { get; private set; }

        protected override object CreateSettingsObject()
        {
            return null;
        }

        public override void Refresh()
        {
            ResourceSelectorViewModel.Refresh(_applicationState.SelectedResourceScreen, _request);
        }
    }
}
