using System;
using System.ComponentModel.Composition;
using Microsoft.Practices.Prism.Events;
using Microsoft.Practices.Prism.MefExtensions.Modularity;
using Microsoft.Practices.Prism.Regions;
using Samba.Domain.Models.Resources;
using Samba.Domain.Models.Tickets;
using Samba.Localization.Properties;
using Samba.Presentation.Common;
using Samba.Presentation.Common.Interaction;
using Samba.Presentation.Common.ModelBase;
using Samba.Services;
using Samba.Services.Common;

namespace Samba.Modules.ResourceModule
{
    [ModuleExport(typeof(ResourceModule))]
    class ResourceModule : VisibleModuleBase
    {
        private readonly IUserService _userService;
        private readonly IApplicationStateSetter _applicationStateSetter;
        private readonly IRegionManager _regionManager;
        private readonly ResourceSearchView _resourceSearchView;
        private readonly ResourceEditorView _resourceEditorView;

        [ImportingConstructor]
        public ResourceModule(IRegionManager regionManager,
            IApplicationStateSetter applicationStateSetter,
            IUserService userService,
            ResourceSearchView resourceSearchView,
            ResourceEditorView resourceEditorView)
            : base(regionManager, AppScreens.AccountView)
        {
            _resourceSearchView = resourceSearchView;
            _resourceEditorView = resourceEditorView;
            _regionManager = regionManager;
            _userService = userService;
            _applicationStateSetter = applicationStateSetter;

            AddDashboardCommand<EntityCollectionViewModelBase<ResourceViewModel, Resource>>(string.Format(Resources.List_f, Resources.Resource), Resources.Resourceses, 40);
            AddDashboardCommand<EntityCollectionViewModelBase<ResourceTemplateViewModel, ResourceTemplate>>(string.Format(Resources.List_f, Resources.ResourceTemplate), Resources.Resourceses, 40);
            AddDashboardCommand<EntityCollectionViewModelBase<ResourceStateViewModel, ResourceState>>(string.Format(Resources.List_f, Resources.ResourceState), Resources.Resourceses, 40);
            AddDashboardCommand<EntityCollectionViewModelBase<ResourceScreenViewModel, ResourceScreen>>(string.Format(Resources.List_f, Resources.ResourceScreen), Resources.Resourceses, 41);
            AddDashboardCommand<EntityCollectionViewModelBase<ResourceScreenItemViewModel, ResourceScreenItem>>(string.Format(Resources.List_f, Resources.ResourceScreenItem), Resources.Resourceses, 41);

            PermissionRegistry.RegisterPermission(PermissionNames.NavigateResourceView, PermissionCategories.Navigation, Resources.CanNavigateCash);
            SetNavigationCommand(Resources.Resourceses, Resources.Common, "Images/Xls.png", 70);
        }

        protected override void OnInitialization()
        {
            _regionManager.RegisterViewWithRegion(RegionNames.MainRegion, typeof(ResourceSearchView));
            _regionManager.RegisterViewWithRegion(RegionNames.MainRegion, typeof(ResourceEditorView));

            EventServiceFactory.EventService.GetEvent<GenericEvent<EventAggregator>>().Subscribe(x =>
            {
                if (x.Topic == EventTopicNames.ActivateResourceView)
                    ActivateResourceSelector();
            });

            EventServiceFactory.EventService.GetEvent<GenericEvent<Department>>().Subscribe(x =>
            {
                if (x.Topic == EventTopicNames.SelectResource)
                    ActivateResourceSelector();
            });

            EventServiceFactory.EventService.GetEvent<GenericEvent<EntityOperationRequest<Resource>>>().Subscribe(x =>
            {
                if (x.Topic == EventTopicNames.SelectResource)
                {
                    ActivateResourceSelector(x.Value);
                }
            });

            EventServiceFactory.EventService.GetEvent<GenericEvent<PopupData>>().Subscribe(x =>
            {
                if (x.Topic == EventTopicNames.PopupClicked && x.Value.EventMessage == EventTopicNames.SelectResource)
                {
                    Activate();
                }
            });

            EventServiceFactory.EventService.GetEvent<GenericEvent<EntityOperationRequest<Resource>>>().Subscribe(x =>
            {
                if (x.Topic == EventTopicNames.EditResourceDetails)
                {
                    ActivateResourceEditor();
                }
            });
        }

        private void ActivateResourceEditor()
        {
            _regionManager.Regions[RegionNames.MainRegion].Activate(_resourceEditorView);
        }

        private void ActivateResourceSelector(EntityOperationRequest<Resource> value = null)
        {
            Activate();
            ((ResourceSearchViewModel)_resourceSearchView.DataContext).RefreshSelectedAccount(value);
            _regionManager.Regions[RegionNames.MainRegion].Activate(_resourceSearchView);
        }

        protected override bool CanNavigate(string arg)
        {
            return _userService.IsUserPermittedFor(PermissionNames.NavigateResourceView);
        }

        protected override void OnNavigate(string obj)
        {
            _applicationStateSetter.SetCurrentDepartment(0);
            ActivateResourceSelector();
            base.OnNavigate(obj);
        }

        public override object GetVisibleView()
        {
            return _resourceSearchView;
        }
    }
}
