using System.ComponentModel;
using System.Linq;
using Samba.Domain.Models.Resources;
using Samba.Infrastructure;
using Samba.Presentation.Common;
using Samba.Presentation.Common.Services;
using Samba.Services;
using Samba.Services.Common;

namespace Samba.Modules.ResourceModule.WidgetCreators
{
    public class ResourceButtonWidgetViewModel : WidgetViewModel
    {
        private readonly ICacheService _cacheService;
        private readonly IApplicationState _applicationState;
        public CaptionCommand<ResourceButtonWidgetViewModel> ItemClickedCommand { get; set; }

        public ResourceButtonWidgetViewModel(Widget model, ICacheService cacheService, IApplicationState applicationState)
            : base(model)
        {
            _cacheService = cacheService;
            _applicationState = applicationState;
            ItemClickedCommand = new CaptionCommand<ResourceButtonWidgetViewModel>("", OnItemClickExecute);
        }

        private void OnItemClickExecute(ResourceButtonWidgetViewModel obj)
        {
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

        public override void Refresh()
        {
            var resourceState = GetResourceState();
            if (resourceState != null)
                ButtonColor = resourceState.Color;
            else ButtonColor = "Gainsboro";
        }

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