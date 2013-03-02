﻿using System.ComponentModel;
using System.Linq;
using Samba.Domain.Models.Entities;
using Samba.Infrastructure.Helpers;
using Samba.Presentation.Common;
using Samba.Presentation.Services;
using Samba.Presentation.Services.Common;
using Samba.Services;

namespace Samba.Modules.EntityModule.Widgets.EntitySearch
{
    class EntitySearchWidgetViewModel : WidgetViewModel
    {
        private readonly IApplicationState _applicationState;
        private readonly ICacheService _cacheService;
        private EntityOperationRequest<Entity> _request = new EntityOperationRequest<Entity>(null, EventTopicNames.EntitySelected);

        public EntitySearchWidgetViewModel(Widget model, IApplicationState applicationState, ICacheService cacheService, IEntityService entityService)
            : base(model, applicationState)
        {
            _applicationState = applicationState;
            _cacheService = cacheService;
            EntitySearchViewModel = new EntitySearchViewModel(applicationState, cacheService, entityService) { IsKeyboardVisible = false };
            EventServiceFactory.EventService.GetEvent<GenericEvent<EntityOperationRequest<Entity>>>().Subscribe(x =>
            {
                if (x.Topic == EventTopicNames.SelectEntity)
                {
                    _request = x.Value;
                }
            });
        }

        [Browsable(false)]
        public EntitySearchWidgetSettings Settings { get { return SettingsObject as EntitySearchWidgetSettings; } }
        [Browsable(false)]
        public EntitySearchViewModel EntitySearchViewModel { get; private set; }

        protected override object CreateSettingsObject()
        {
            return JsonHelper.Deserialize<EntitySearchWidgetSettings>(_model.Properties);
        }

        protected override void BeforeEditSettings()
        {
            Settings.EntityTypeNameValue.UpdateValues(_cacheService.GetEntityTypes().Select(x => x.EntityName));
            Settings.StateFilterNameValue.UpdateValues(_cacheService.GetStates(0).Select(x => x.Name));
        }

        public override void Refresh()
        {
            EntitySearchViewModel.IsKeyboardVisible = Settings.IsKeyboardVisible;
            var entityTypeId = _cacheService.GetEntityTypeIdByEntityName(Settings.EntityTypeName);
            if (entityTypeId == 0) entityTypeId = _applicationState.SelectedEntityScreen.EntityTypeId;
            var stateFilter = !string.IsNullOrEmpty(Settings.StateFilterName) ? (Settings.StateFilterName) : _applicationState.SelectedEntityScreen.StateFilter;
            EntitySearchViewModel.Refresh(entityTypeId, stateFilter, _request);
        }
    }
}
