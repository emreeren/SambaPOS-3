using System.ComponentModel;
using System.Linq;
using Samba.Domain.Models.Resources;
using Samba.Infrastructure;
using Samba.Presentation.Common;
using Samba.Services;
using Samba.Services.Common;

namespace Samba.Modules.ResourceModule.WidgetCreators
{
    public class ResourceButtonWidgetViewModel : WidgetViewModel
    {
        private readonly ICacheService _cacheService;
        private readonly IApplicationState _applicationState;
        private readonly IResourceService _resourceService;

        [Browsable(false)]
        public CaptionCommand<ResourceButtonWidgetViewModel> ItemClickedCommand { get; set; }

        public ResourceButtonWidgetViewModel(Widget model, ICacheService cacheService, IApplicationState applicationState, IResourceService resourceService)
            : base(model)
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
            return JsonHelper.Deserialize<ResourceWidgetSettings>(_model.Properties);
        }

        protected override void BeforeEditSettings()
        {
            Settings.ResourceNameValue.UpdateValues(_resourceService.GetCurrentResourceScreenItems(_applicationState.SelectedResourceScreen, 0, 0).Select(x => x.Name));
        }

        public override void Refresh()
        {
            var resourceState = GetResourceState();
            if (resourceState != null)
                ButtonColor = resourceState.Color;
            else ButtonColor = "Gainsboro";
        }

        [Browsable(false)]
        public ResourceWidgetSettings Settings { get { return SettingsObject as ResourceWidgetSettings; } }

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