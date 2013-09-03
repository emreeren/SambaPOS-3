using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using Microsoft.Practices.Prism.Commands;
using Microsoft.Practices.Prism.Events;
using Microsoft.Practices.Prism.Regions;
using Samba.Domain.Models.Entities;
using Samba.Presentation.Common;
using Samba.Presentation.Services;
using Samba.Presentation.Services.Common;
using Samba.Services;

namespace Samba.Modules.EntityModule
{
    [Export]
    public class EntitySwitcherViewModel : ObservableObject
    {
        private readonly IRegionManager _regionManager;
        private readonly IApplicationState _applicationState;
        private readonly IApplicationStateSetter _applicationStateSetter;
        private readonly ICacheService _cacheService;
        private readonly EntitySelectorView _entitySelectorView;
        private readonly EntitySelectorViewModel _entitySelectorViewModel;
        private readonly EntitySearchView _entitySearchView;
        private readonly EntitySearchViewModel _entitySearchViewModel;
        private readonly EntityDashboardView _entityDashboardView;
        private readonly EntityDashboardViewModel _entityDashboardViewModel;

        private OperationRequest<Entity> _currentOperationRequest;

        [ImportingConstructor]
        public EntitySwitcherViewModel(IRegionManager regionManager,
            IApplicationState applicationState, IApplicationStateSetter applicationStateSetter, ICacheService cacheService,
            EntitySelectorView entitySelectorView, EntitySelectorViewModel entitySelectorViewModel,
            EntitySearchView entitySearchView, EntitySearchViewModel entitySearchViewModel,
            EntityDashboardView entityDashboardView, EntityDashboardViewModel entityDashboardViewModel)
        {
            _regionManager = regionManager;
            _applicationState = applicationState;
            _applicationStateSetter = applicationStateSetter;
            _cacheService = cacheService;
            _entitySelectorView = entitySelectorView;
            _entitySelectorViewModel = entitySelectorViewModel;
            _entitySearchView = entitySearchView;
            _entitySearchViewModel = entitySearchViewModel;
            _entityDashboardView = entityDashboardView;
            _entityDashboardViewModel = entityDashboardViewModel;

            SelectEntityCategoryCommand = new DelegateCommand<EntityScreen>(OnSelectEntityCategoryExecuted);

            EventServiceFactory.EventService.GetEvent<GenericEvent<EventAggregator>>().Subscribe(
            x =>
            {
                if (x.Topic == EventTopicNames.ResetCache)
                {
                    _entityScreens = null;
                    _entitySwitcherButtons = null;
                    RaisePropertyChanged(() => EntitySwitcherButtons);
                }
            });

            EventServiceFactory.EventService.GetEvent<GenericEvent<OperationRequest<Entity>>>().Subscribe(x =>
            {
                if (x.Topic == EventTopicNames.SelectEntity)
                {
                    var ss = UpdateEntityScreens(x.Value);
                    _currentOperationRequest = x.Value;
                    ActivateEntityScreen(ss);
                    if (ss != null && ss.DisplayMode == 1)
                        _entitySearchViewModel.SearchString = x.Value.Data;
                }
            });
        }

        private EntityScreen UpdateEntityScreens(OperationRequest<Entity> value)
        {
            var entityScreens =
                _applicationState.IsLocked ?
                _applicationState.GetTicketEntityScreens().ToList() :
                _applicationState.GetEntityScreens().ToList();
            if (!entityScreens.Any()) return null;
            _entityScreens = entityScreens.OrderBy(x => x.SortOrder).ToList();
            _entitySwitcherButtons = null;
            var selectedScreen = _applicationState.SelectedEntityScreen;
            if (value != null && value.SelectedItem != null && _applicationState.CurrentDepartment != null)
            {
                if (_applicationState.IsLocked || _applicationState.CurrentDepartment.TicketCreationMethod == 1)
                    _entityScreens = _entityScreens.Where(x => x.EntityTypeId == value.SelectedItem.EntityTypeId).OrderBy(x => x.SortOrder);
                if (!_entityScreens.Any())
                    return entityScreens.ElementAt(0);
                if (selectedScreen == null || selectedScreen.EntityTypeId != value.SelectedItem.EntityTypeId)
                {
                    selectedScreen = null;
                    if (!string.IsNullOrEmpty(value.Data))
                    {
                        selectedScreen = _entityScreens.Where(x => x.DisplayMode == 1).FirstOrDefault(x => x.EntityTypeId == value.SelectedItem.EntityTypeId);
                    }
                    if (selectedScreen == null)
                    {
                        selectedScreen = _entityScreens.FirstOrDefault(x => x.EntityTypeId == value.SelectedItem.EntityTypeId);
                    }
                }
                if (selectedScreen == null) selectedScreen = _entityScreens.ElementAt(0);
            }
            return selectedScreen ?? EntityScreens.ElementAt(0);
        }

        public DelegateCommand<EntityScreen> SelectEntityCategoryCommand { get; set; }

        private IEnumerable<EntityScreen> _entityScreens;
        public IEnumerable<EntityScreen> EntityScreens
        {
            get
            {
                if (_applicationState.CurrentDepartment == null) return new List<EntityScreen>();
                return _entityScreens ?? (_entityScreens = _applicationState.GetEntityScreens().OrderBy(x => x.SortOrder));
            }
        }

        private List<EntitySwitcherButtonViewModel> _entitySwitcherButtons;
        public List<EntitySwitcherButtonViewModel> EntitySwitcherButtons
        {
            get
            {
                return _entitySwitcherButtons ?? (_entitySwitcherButtons = EntityScreens.Select(
                    x => new EntitySwitcherButtonViewModel(x, _applicationState, EntityScreens.Count() > 1)).ToList());
            }
        }

        private void OnSelectEntityCategoryExecuted(EntityScreen obj)
        {
            ActivateEntityScreen(obj);
        }

        private void ActivateEntityScreen(EntityScreen entityScreen)
        {
            entityScreen = _applicationStateSetter.SetSelectedEntityScreen(entityScreen);
            _applicationStateSetter.SetCurrentTicketType(entityScreen != null ? _cacheService.GetTicketTypeById(entityScreen.TicketTypeId) : null);

            if (entityScreen != null)
            {
                if (entityScreen.DisplayMode == 1)
                    ActivateEntitySearcher(entityScreen);
                else if (entityScreen.DisplayMode == 2)
                    ActivateDashboard(entityScreen);
                else ActivateButtonSelector(entityScreen);
            }
            RaisePropertyChanged(() => EntitySwitcherButtons);
            EntitySwitcherButtons.ForEach(x => x.Refresh());
        }

        private void ActivateDashboard(EntityScreen entityScreen)
        {
            _entityDashboardViewModel.Refresh(entityScreen, _currentOperationRequest);
            _regionManager.ActivateRegion(RegionNames.EntityScreenRegion, _entityDashboardView);
        }

        private void ActivateEntitySearcher(EntityScreen entityScreen)
        {
            _entitySearchViewModel.Refresh(entityScreen.EntityTypeId, entityScreen.StateFilter, _currentOperationRequest);
            _regionManager.ActivateRegion(RegionNames.EntityScreenRegion, _entitySearchView);
        }

        private void ActivateButtonSelector(EntityScreen entityScreen)
        {
            _entitySelectorViewModel.Refresh(entityScreen, entityScreen.StateFilter, _currentOperationRequest);
            _regionManager.ActivateRegion(RegionNames.EntityScreenRegion, _entitySelectorView);
        }
    }
}
