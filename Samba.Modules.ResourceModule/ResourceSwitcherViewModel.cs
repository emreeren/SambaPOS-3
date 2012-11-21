using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using Microsoft.Practices.Prism.Commands;
using Microsoft.Practices.Prism.Events;
using Microsoft.Practices.Prism.Regions;
using Samba.Domain.Models.Resources;
using Samba.Presentation.Common;
using Samba.Presentation.Services;
using Samba.Presentation.Services.Common;
using Samba.Services;

namespace Samba.Modules.ResourceModule
{
    [Export]
    public class ResourceSwitcherViewModel : ObservableObject
    {
        private readonly IRegionManager _regionManager;
        private readonly IPresentationCacheService _presentationCacheService;
        private readonly IApplicationState _applicationState;
        private readonly IApplicationStateSetter _applicationStateSetter;
        private readonly ICacheService _cacheService;
        private readonly ResourceSelectorView _resourceSelectorView;
        private readonly ResourceSelectorViewModel _resourceSelectorViewModel;
        private readonly ResourceSearchView _resourceSearchView;
        private readonly ResourceSearchViewModel _resourceSearchViewModel;
        private readonly ResourceDashboardView _resourceDashboardView;
        private readonly ResourceDashboardViewModel _resourceDashboardViewModel;

        private EntityOperationRequest<Resource> _currentOperationRequest;

        [ImportingConstructor]
        public ResourceSwitcherViewModel(IRegionManager regionManager, IPresentationCacheService presentationCacheService, 
            IApplicationState applicationState, IApplicationStateSetter applicationStateSetter,ICacheService cacheService,
            ResourceSelectorView resourceSelectorView, ResourceSelectorViewModel resourceSelectorViewModel,
            ResourceSearchView resourceSearchView, ResourceSearchViewModel resourceSearchViewModel,
            ResourceDashboardView resourceDashboardView, ResourceDashboardViewModel resourceDashboardViewModel)
        {
            _regionManager = regionManager;
            _presentationCacheService = presentationCacheService;
            _applicationState = applicationState;
            _applicationStateSetter = applicationStateSetter;
            _cacheService = cacheService;
            _resourceSelectorView = resourceSelectorView;
            _resourceSelectorViewModel = resourceSelectorViewModel;
            _resourceSearchView = resourceSearchView;
            _resourceSearchViewModel = resourceSearchViewModel;
            _resourceDashboardView = resourceDashboardView;
            _resourceDashboardViewModel = resourceDashboardViewModel;

            SelectResourceCategoryCommand = new DelegateCommand<ResourceScreen>(OnSelectResourceCategoryExecuted);

            EventServiceFactory.EventService.GetEvent<GenericEvent<EventAggregator>>().Subscribe(
            x =>
            {
                if (x.Topic == EventTopicNames.ResetCache)
                {
                    _resourceScreens = null;
                    _resourceSwitcherButtons = null;
                    RaisePropertyChanged(() => ResourceSwitcherButtons);
                }
            });

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
            var resourceScreens =
                _applicationState.IsLocked ?
                _presentationCacheService.GetTicketResourceScreens().ToList() :
                _presentationCacheService.GetResourceScreens().ToList();
            if (!resourceScreens.Any()) return null;
            _resourceScreens = resourceScreens.OrderBy(x => x.Order).ToList();
            _resourceSwitcherButtons = null;
            var selectedScreen = _applicationState.SelectedResourceScreen;
            if (value != null && value.SelectedEntity != null && _applicationState.CurrentDepartment != null)
            {
                if (_applicationState.IsLocked || _applicationState.CurrentDepartment.TicketCreationMethod == 1)
                    _resourceScreens = _resourceScreens.Where(x => x.ResourceTypeId == value.SelectedEntity.ResourceTypeId).OrderBy(x => x.Order);
                if (!_resourceScreens.Any())
                    return resourceScreens.ElementAt(0);
                if (selectedScreen == null || selectedScreen.ResourceTypeId != value.SelectedEntity.ResourceTypeId)
                    selectedScreen = _resourceScreens.First(x => x.ResourceTypeId == value.SelectedEntity.ResourceTypeId);
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
                return _resourceScreens ?? (_resourceScreens = _presentationCacheService.GetResourceScreens().OrderBy(x => x.Order));
            }
        }

        private List<ResourceSwitcherButtonViewModel> _resourceSwitcherButtons;
        public List<ResourceSwitcherButtonViewModel> ResourceSwitcherButtons
        {
            get
            {
                return _resourceSwitcherButtons ?? (_resourceSwitcherButtons = ResourceScreens.Select(
                    x => new ResourceSwitcherButtonViewModel(x, _applicationState, ResourceScreens.Count() > 1)).ToList());
            }
        }

        private void OnSelectResourceCategoryExecuted(ResourceScreen obj)
        {
            ActivateResourceScreen(obj);
        }

        private void ActivateResourceScreen(ResourceScreen resourceScreen)
        {
            resourceScreen = _applicationStateSetter.SetSelectedResourceScreen(resourceScreen);
            _applicationStateSetter.SetCurrentTicketType(resourceScreen != null ? _cacheService.GetTicketTypeById(resourceScreen.TicketTypeId) : null);

            if (resourceScreen != null)
            {
                if (resourceScreen.DisplayMode == 1)
                    ActivateResourceSearcher(resourceScreen);
                else if (resourceScreen.DisplayMode == 2)
                    ActivateDashboard(resourceScreen);
                else ActivateButtonSelector(resourceScreen);
            }
            RaisePropertyChanged(() => ResourceSwitcherButtons);
            ResourceSwitcherButtons.ForEach(x => x.Refresh());
        }

        private void ActivateDashboard(ResourceScreen resourceScreen)
        {
            _resourceDashboardViewModel.Refresh(resourceScreen, _currentOperationRequest);
            _regionManager.Regions[RegionNames.ResourceScreenRegion].Activate(_resourceDashboardView);
        }

        private void ActivateResourceSearcher(ResourceScreen resourceScreen)
        {
            _resourceSearchViewModel.Refresh(resourceScreen.ResourceTypeId, resourceScreen.StateFilterId, _currentOperationRequest);
            _regionManager.Regions[RegionNames.ResourceScreenRegion].Activate(_resourceSearchView);
        }

        private void ActivateButtonSelector(ResourceScreen resourceScreen)
        {
            _resourceSelectorViewModel.Refresh(resourceScreen, resourceScreen.StateFilterId, _currentOperationRequest);
            _regionManager.Regions[RegionNames.ResourceScreenRegion].Activate(_resourceSelectorView);
        }
    }
}
