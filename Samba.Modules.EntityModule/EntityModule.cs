using System.ComponentModel.Composition;
using Microsoft.Practices.Prism.MefExtensions.Modularity;
using Microsoft.Practices.Prism.Regions;
using Samba.Domain.Models;
using Samba.Domain.Models.Entities;
using Samba.Localization.Properties;
using Samba.Presentation.Common;
using Samba.Presentation.Common.ModelBase;
using Samba.Presentation.Controls.Interaction;
using Samba.Presentation.Services;
using Samba.Presentation.Services.Common;

namespace Samba.Modules.EntityModule
{
    [ModuleExport(typeof(EntityModule))]
    class EntityModule : ModuleBase
    {
        private readonly IRegionManager _regionManager;
        private readonly IApplicationStateSetter _applicationStateSetter;
        private readonly EntityEditorView _entityEditorView;
        private readonly EntitySwitcherView _entitySwitcherView;

        [ImportingConstructor]
        public EntityModule(IRegionManager regionManager,
            IUserService userService,IApplicationStateSetter applicationStateSetter,
            EntitySwitcherView entitySwitcherView,
            EntityEditorView entityEditorView)
        {
            _entitySwitcherView = entitySwitcherView;
            _entityEditorView = entityEditorView;
            _regionManager = regionManager;
            _applicationStateSetter = applicationStateSetter;

            AddDashboardCommand<EntityCollectionViewModelBase<EntityTypeViewModel, EntityType>>(Resources.EntityType.ToPlural(), Resources.Entities, 40);
            AddDashboardCommand<EntityCollectionViewModelBase<EntityViewModel, Entity>>(Resources.Entity.ToPlural(), Resources.Entities, 40);
            AddDashboardCommand<EntityCollectionViewModelBase<EntityScreenViewModel, EntityScreen>>(Resources.EntityScreen.ToPlural(), Resources.Entities, 41);
        }

        protected override void OnInitialization()
        {
            _regionManager.RegisterViewWithRegion(RegionNames.MainRegion, typeof(EntitySwitcherView));
            _regionManager.RegisterViewWithRegion(RegionNames.EntityScreenRegion, typeof(EntitySelectorView));
            _regionManager.RegisterViewWithRegion(RegionNames.EntityScreenRegion, typeof(EntitySearchView));
            _regionManager.RegisterViewWithRegion(RegionNames.EntityScreenRegion, typeof(EntityEditorView));
            _regionManager.RegisterViewWithRegion(RegionNames.EntityScreenRegion, typeof(EntityDashboardView));

            EventServiceFactory.EventService.GetEvent<GenericEvent<EntityOperationRequest<Entity>>>().Subscribe(x =>
            {
                if (x.Topic == EventTopicNames.SelectEntity) ActivateEntitySwitcher();
                if (x.Topic == EventTopicNames.EditEntityDetails) ActivateEntityEditor();
            });

            EventServiceFactory.EventService.GetEvent<GenericEvent<EntityOperationRequest<AccountData>>>().Subscribe(x =>
            {
                if (x.Topic == EventTopicNames.SelectEntity) ActivateEntitySwitcher();
            });

            EventServiceFactory.EventService.GetEvent<GenericEvent<PopupData>>().Subscribe(x =>
            {
                if (x.Topic == EventTopicNames.PopupClicked && x.Value.EventMessage == EventTopicNames.SelectEntity)
                {
                    ActivateEntitySwitcher();
                }
            });
        }

        private void ActivateEntityEditor()
        {
            _applicationStateSetter.SetCurrentApplicationScreen(AppScreens.EntityView);
            _regionManager.Regions[RegionNames.EntityScreenRegion].Activate(_entityEditorView);
        }

        private void ActivateEntitySwitcher()
        {
            _applicationStateSetter.SetCurrentApplicationScreen(AppScreens.EntityView);
            _regionManager.Regions[RegionNames.MainRegion].Activate(_entitySwitcherView);
        }
    }
}
