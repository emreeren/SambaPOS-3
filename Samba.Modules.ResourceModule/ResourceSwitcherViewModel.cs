using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using Microsoft.Practices.Prism.Commands;
using Microsoft.Practices.Prism.Regions;
using Samba.Domain.Models.Resources;
using Samba.Presentation.Common;
using Samba.Services;
using Samba.Services.Common;

namespace Samba.Modules.ResourceModule
{
    [Export]
    public class ResourceSwitcherViewModel : ObservableObject
    {
        private readonly IRegionManager _regionManager;
        private readonly IApplicationState _applicationState;
        private readonly IApplicationStateSetter _applicationStateSetter;
        private readonly ResourceSelectorView _resourceSelectorView;
        private readonly ResourceSelectorViewModel _resourceSelectorViewModel;
        private readonly ResourceSearchView _resourceSearchView;
        private readonly ResourceSearchViewModel _resourceSearchViewModel;
        private readonly ResourceDashboardView _resourceDashboardView;
        private readonly ResourceDashboardViewModel _resourceDashboardViewModel;

        private EntityOperationRequest<Resource> _currentOperationRequest;

        [ImportingConstructor]
        public ResourceSwitcherViewModel(IRegionManager regionManager, IApplicationState applicationState, IApplicationStateSetter applicationStateSetter,
            ResourceSelectorView resourceSelectorView, ResourceSelectorViewModel resourceSelectorViewModel,
            ResourceSearchView resourceSearchView, ResourceSearchViewModel resourceSearchViewModel,
            ResourceDashboardView resourceDashboardView, ResourceDashboardViewModel resourceDashboardViewModel)
        {
            _regionManager = regionManager;
            _applicationState = applicationState;
            _applicationStateSetter = applicationStateSetter;
            _resourceSelectorView = resourceSelectorView;
            _resourceSelectorViewModel = resourceSelectorViewModel;
            _resourceSearchView = resourceSearchView;
            _resourceSearchViewModel = resourceSearchViewModel;
            _resourceDashboardView = resourceDashboardView;
            _resourceDashboardViewModel = resourceDashboardViewModel;

            SelectResourceCategoryCommand = new DelegateCommand<ResourceScreen>(OnSelectResourceCategoryExecuted);

            EventServiceFactory.EventService.GetEvent<GenericEvent<EntityOperationRequest<Resource>>>().Subscribe(x =>
            {
                if (x.Topic == EventTopicNames.SelectResource)
                {
                    var ss = UpdateResourceScreens(x.Value);
                    _currentOperationRequest = x.Value;
                    ActivateResourceScreen(ss);
                }
            });
        }

        private ResourceScreen UpdateResourceScreens(EntityOperationRequest<Resource> value)
        {
            if (_applicationState.CurrentDepartment.ResourceScreens.Count == 0) return null;
            _resourceScreens = _applicationState.CurrentDepartment.ResourceScreens.OrderBy(x => x.Order).ToList();
            _resourceSwitcherButtons = null;
            var selectedScreen = _applicationState.SelectedResourceScreen;
            if (value != null && value.SelectedEntity != null && _applicationState.CurrentDepartment != null)
            {
                if (_applicationState.IsLocked)
                    _resourceScreens = _resourceScreens.Where(x => x.ResourceTemplateId == value.SelectedEntity.ResourceTemplateId).OrderBy(x => x.Order);
                if (_resourceScreens.Count() == 0)
                    return _applicationState.CurrentDepartment.ResourceScreens.ElementAt(0);
                if (selectedScreen == null || selectedScreen.ResourceTemplateId != value.SelectedEntity.ResourceTemplateId)
                    selectedScreen = _resourceScreens.Where(x => x.ResourceTemplateId == value.SelectedEntity.ResourceTemplateId).First();
                if (selectedScreen == null) selectedScreen = _resourceScreens.ElementAt(0);
            }
            return selectedScreen ?? ResourceScreens.ElementAt(0);
        }

        public DelegateCommand<ResourceScreen> SelectResourceCategoryCommand { get; set; }

        private IEnumerable<ResourceScreen> _resourceScreens;
        public IEnumerable<ResourceScreen> ResourceScreens
        {
            get
            {
                if (_applicationState.CurrentDepartment == null) return new List<ResourceScreen>();
                return _resourceScreens ?? (_resourceScreens = _applicationState.CurrentDepartment.ResourceScreens.OrderBy(x => x.Order));
            }
        }

        private IEnumerable<ResourceSwitcherButtonViewModel> _resourceSwitcherButtons;
        public IEnumerable<ResourceSwitcherButtonViewModel> ResourceSwitcherButtons
        {
            get { return _resourceSwitcherButtons ?? (ResourceScreens.Select(x => new ResourceSwitcherButtonViewModel(x, _applicationState, ResourceScreens.Count() > 1))); }
        }

        private void OnSelectResourceCategoryExecuted(ResourceScreen obj)
        {
            ActivateResourceScreen(obj);
        }

        private void ActivateResourceScreen(ResourceScreen resourceScreen)
        {
            if (!_applicationState.IsLocked)
                _applicationStateSetter.SetSelectedResourceScreen(resourceScreen);

            if (resourceScreen != null)
            {
                if (resourceScreen.DisplayMode == 2)
                    ActivateResourceSearcher(resourceScreen);
                else if (resourceScreen.DisplayMode == 1)
                    ActivateDashboard(resourceScreen);
                else ActivateButtonSelector(resourceScreen);
            }

            RaisePropertyChanged(() => ResourceSwitcherButtons);
            ResourceSwitcherButtons.ToList().ForEach(x => x.Refresh());
        }

        private void ActivateDashboard(ResourceScreen resourceScreen)
        {
            _resourceDashboardViewModel.Refresh(resourceScreen, _currentOperationRequest);
            _regionManager.Regions[RegionNames.ResourceScreenRegion].Activate(_resourceDashboardView);
        }

        private void ActivateResourceSearcher(ResourceScreen resourceScreen)
        {
            _resourceSearchViewModel.Refresh(resourceScreen, _currentOperationRequest);
            _regionManager.Regions[RegionNames.ResourceScreenRegion].Activate(_resourceSearchView);
        }

        private void ActivateButtonSelector(ResourceScreen resourceScreen)
        {
            _resourceSelectorViewModel.Refresh(resourceScreen, _currentOperationRequest);
            _regionManager.Regions[RegionNames.ResourceScreenRegion].Activate(_resourceSelectorView);
        }
    }
}
