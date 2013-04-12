using System.ComponentModel;
using System.Linq;
using Samba.Domain.Models.Entities;
using Samba.Infrastructure.Helpers;
using Samba.Presentation.Common;
using Samba.Presentation.Common.Commands;
using Samba.Presentation.Common.Widgets;
using Samba.Presentation.Services;
using Samba.Presentation.Services.Common;
using Samba.Services;

namespace Samba.Modules.EntityModule.Widgets.EntityButton
{
    public class EntityButtonWidgetViewModel : WidgetViewModel
    {
        private readonly ICacheService _cacheService;
        private readonly IApplicationState _applicationState;
        private readonly IEntityService _resourceService;

        [Browsable(false)]
        public CaptionCommand<EntityButtonWidgetViewModel> ItemClickedCommand { get; set; }

        public EntityButtonWidgetViewModel(Widget model, ICacheService cacheService, IApplicationState applicationState, IEntityService resourceService)
            : base(model, applicationState)
        {
            _cacheService = cacheService;
            _applicationState = applicationState;
            _resourceService = resourceService;
            ItemClickedCommand = new CaptionCommand<EntityButtonWidgetViewModel>("", OnItemClickExecute);
        }

        private void OnItemClickExecute(EntityButtonWidgetViewModel obj)
        {
            if (DesignMode) return;
            if (_applicationState.SelectedEntityScreen == null) return;
            var si = _applicationState.SelectedEntityScreen.ScreenItems.SingleOrDefault(x => x.Name == Settings.ResourceName);
            if (si == null) return;
            var resource = _cacheService.GetEntityById(si.EntityId);
            EntityOperationRequest<Entity>.Publish(resource, EventTopicNames.EntitySelected, null);
        }

        protected override object CreateSettingsObject()
        {
            return JsonHelper.Deserialize<EntityButtonWidgetSettings>(_model.Properties);
        }

        protected override void BeforeEditSettings()
        {
            Settings.ResourceNameValue.UpdateValues(_resourceService.GetCurrentEntityScreenItems(_applicationState.SelectedEntityScreen, 0, "").Select(x => x.Name));
        }

        public override void Refresh()
        {
            var resourceState = GetResourceState();
            ButtonColor = _cacheService.GetStateColor(resourceState); 
        }

        [Browsable(false)]
        public EntityButtonWidgetSettings Settings { get { return SettingsObject as EntityButtonWidgetSettings; } }

        public string GetResourceState()
        {
            if (_applicationState.SelectedEntityScreen == null) return null;
            var si = _applicationState.SelectedEntityScreen.ScreenItems.SingleOrDefault(x => x.Name == Settings.ResourceName);
            if (si == null) return null;
            return si.EntityState;
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