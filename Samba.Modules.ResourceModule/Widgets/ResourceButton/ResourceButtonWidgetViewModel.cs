using System.ComponentModel;
using System.Linq;
using Samba.Domain.Models.Resources;
using Samba.Infrastructure;
using Samba.Infrastructure.Helpers;
using Samba.Presentation.Common;
using Samba.Presentation.Common.Commands;
using Samba.Presentation.Services;
using Samba.Presentation.Services.Common;
using Samba.Services;

namespace Samba.Modules.ResourceModule.Widgets.ResourceButton
{
    public class ResourceButtonWidgetViewModel : WidgetViewModel
    {
        private readonly IPresentationCacheService _cacheService;
        private readonly IApplicationState _applicationState;
        private readonly IResourceService _resourceService;

        [Browsable(false)]
        public CaptionCommand<ResourceButtonWidgetViewModel> ItemClickedCommand { get; set; }

        public ResourceButtonWidgetViewModel(Widget model, IPresentationCacheService cacheService, IApplicationState applicationState, IResourceService resourceService)
            : base(model, applicationState)
        {
            _cacheService = cacheService;
            _applicationState = applicationState;
            _resourceService = resourceService;
            ItemClickedCommand = new CaptionCommand<ResourceButtonWidgetViewModel>("", OnItemClickExecute);
        }

        private void OnItemClickExecute(ResourceButtonWidgetViewModel obj)
        {
            if (DesignMode) return;
            if (_applicationState.SelectedResourceScreen == null) return;
            var si = _applicationState.SelectedResourceScreen.ScreenItems.SingleOrDefault(x => x.Name == Settings.ResourceName);
            if (si == null) return;
            var resource = _cacheService.GetResourceById(si.ResourceId);
            EntityOperationRequest<Resource>.Publish(resource, EventTopicNames.ResourceSelected, null);
        }

        protected override object CreateSettingsObject()
        {
            return JsonHelper.Deserialize<ResourceButtonWidgetSettings>(_model.Properties);
        }

        protected override void BeforeEditSettings()
        {
            Settings.ResourceNameValue.UpdateValues(_resourceService.GetCurrentResourceScreenItems(_applicationState.SelectedResourceScreen, 0, 0).Select(x => x.Name));
        }

        public override void Refresh()
        {
            var resourceState = GetResourceState();
            ButtonColor = resourceState != null ? resourceState.Color : "Gainsboro";
        }

        [Browsable(false)]
        public ResourceButtonWidgetSettings Settings { get { return SettingsObject as ResourceButtonWidgetSettings; } }

        public ResourceState GetResourceState()
        {
            if (_applicationState.SelectedResourceScreen == null) return null;
            var si = _applicationState.SelectedResourceScreen.ScreenItems.SingleOrDefault(x => x.Name == Settings.ResourceName);
            if (si == null) return null;
            return _cacheService.GetResourceStateById(si.ResourceStateId);
        }

        private string _buttonColor;
        [Browsable(false)]
        public string ButtonColor
        {
            get { return _buttonColor; }
            set
            {
                if (_buttonColor != value)
                {
                    _buttonColor = value;
                    RaisePropertyChanged(() => ButtonColor);
                }
            }
        }


    }
}