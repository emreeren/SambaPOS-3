using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Globalization;
using System.Linq;
using System.Timers;
using System.Windows.Input;
using System.Windows.Threading;
using Microsoft.Practices.Prism.Commands;
using Samba.Domain.Models.Entities;
using Samba.Localization.Properties;
using Samba.Presentation.Common;
using Samba.Presentation.Common.Commands;
using Samba.Presentation.Services;
using Samba.Presentation.Services.Common;
using Samba.Services;

namespace Samba.Modules.EntityModule
{
    [Export]
    public class EntitySearchViewModel : ObservableObject
    {
        public event EventHandler SelectedEntityTypeChanged;

        private void InvokeSelectedEntityTypeChanged(EventArgs e)
        {
            var handler = SelectedEntityTypeChanged;
            if (handler != null) handler(this, e);
        }

        private EntityOperationRequest<Entity> _currentEntitySelectionRequest;

        public DelegateCommand<string> SelectEntityCommand { get; set; }
        public DelegateCommand<string> CreateEntityCommand { get; set; }
        public DelegateCommand<string> EditEntityCommand { get; set; }
        public DelegateCommand<string> RemoveEntityCommand { get; set; }
        public ICaptionCommand DisplayAccountCommand { get; set; }
        public ICaptionCommand DisplayInventoryCommand { get; set; }

        public string SelectEntityCommandCaption { get { return string.Format(Resources.Select_f, SelectedEntityName()).Replace(" ", "\r"); } }
        public string CreateEntityCommandCaption { get { return string.Format(Resources.New_f, SelectedEntityName()).Replace(" ", "\r"); } }
        public string EditEntityCommandCaption { get { return string.Format(Resources.Edit_f, SelectedEntityName()).Replace(" ", "\r"); } }
        public string RemoveEntityCommandCaption { get { return string.Format(Resources.No_f, SelectedEntityName()).Replace(" ", "\r"); } }
        private readonly IApplicationState _applicationState;
        private readonly ICacheService _cacheService;
        private readonly IEntityService _entityService;
        private readonly Timer _updateTimer;

        private bool _canCreateNewEntity;
        public bool CanCreateNewEntity
        {
            get { return _canCreateNewEntity; }
            set
            {
                _canCreateNewEntity = value;
                RaisePropertyChanged(() => CanCreateNewEntity);
            }
        }

        [ImportingConstructor]
        public EntitySearchViewModel(IApplicationState applicationState, ICacheService cacheService, IEntityService entityService)
        {
            _updateTimer = new Timer(500);
            _updateTimer.Elapsed += UpdateTimerElapsed;

            _applicationState = applicationState;
            _cacheService = cacheService;
            _entityService = entityService;

            IsKeyboardVisible = true;
            FoundEntities = new ObservableCollection<EntitySearchResultViewModel>();

            SelectEntityCommand = new CaptionCommand<string>("", OnSelectEntity, CanSelectEntity);
            EditEntityCommand = new CaptionCommand<string>("", OnEditEntity, CanEditEntity);
            CreateEntityCommand = new CaptionCommand<string>("", OnCreateEntity, CanCreateEntity);
            DisplayAccountCommand = new CaptionCommand<string>(Resources.AccountDetails.Replace(" ", "\r"), OnDisplayAccount, CanDisplayAccount);
            DisplayInventoryCommand = new CaptionCommand<string>(Resources.Inventory, OnDisplayInventory, CanDisplayInventory);
            RemoveEntityCommand = new CaptionCommand<string>("", OnRemoveEntity, CanRemoveEntity);
        }

        private bool CanDisplayInventory(string arg)
        {
            return SelectedEntity != null && SelectedEntity.Model.WarehouseId > 0;
        }

        private void OnDisplayInventory(string obj)
        {
            SelectedEntity.Model.PublishEvent(EventTopicNames.DisplayInventory);
        }

        protected string StateFilter { get; set; }
        public bool IsKeyboardVisible { get; set; }

        public IEnumerable<EntityType> EntityTypes { get { return _cacheService.GetEntityTypes(); } }

        private EntityType _selectedEntityType;
        public EntityType SelectedEntityType
        {
            get { return _selectedEntityType; }
            set
            {
                _selectedEntityType = value;
                ClearSearchValues();
                RaisePropertyChanged(() => SelectedEntityType);
                RaisePropertyChanged(() => SelectEntityCommandCaption);
                RaisePropertyChanged(() => CreateEntityCommandCaption);
                RaisePropertyChanged(() => EditEntityCommandCaption);
                RaisePropertyChanged(() => RemoveEntityCommandCaption);
                RaisePropertyChanged(() => IsEntitySelectorVisible);
                RaisePropertyChanged(() => IsInventorySelectorVisible);
                InvokeSelectedEntityTypeChanged(EventArgs.Empty);
            }
        }

        private string SelectedEntityName()
        {
            return SelectedEntityType != null ? SelectedEntityType.EntityName : Resources.Entity;
        }

        public ObservableCollection<EntitySearchResultViewModel> FoundEntities { get; set; }

        public EntitySearchResultViewModel SelectedEntity
        {
            get
            {
                return FoundEntities.Count == 1 ? FoundEntities[0] : FocusedEntity;
            }
        }

        private EntitySearchResultViewModel _focusedEntity;
        public EntitySearchResultViewModel FocusedEntity
        {
            get { return _focusedEntity; }
            set
            {
                _focusedEntity = value;
                RaisePropertyChanged(() => FocusedEntity);
                RaisePropertyChanged(() => SelectedEntity);
            }
        }

        private string _searchString;
        public string SearchString
        {
            get { return string.IsNullOrEmpty(_searchString) ? null : _searchString.TrimStart('+', '0'); }
            set
            {
                if (value != _searchString)
                {
                    _searchString = value;
                    RaisePropertyChanged(() => SearchString);
                    ResetTimer();
                }
            }
        }

        public bool IsEntitySelectorVisible
        {
            get
            {
                if (_applicationState.SelectedEntityScreen != null)
                {
                    var ticketType = _cacheService.GetTicketTypeById(_applicationState.SelectedEntityScreen.TicketTypeId);
                    return ticketType.EntityTypeAssignments.Any(x => x.EntityTypeId == SelectedEntityType.Id);
                }
                return false;
            }
        }

        public bool IsInventorySelectorVisible
        {
            get
            {
                return SelectedEntityType != null && SelectedEntityType.WarehouseTypeId > 0;
            }
        }

        private void OnDisplayAccount(string obj)
        {
            CommonEventPublisher.PublishEntityOperation(new AccountData { AccountId = SelectedEntity.Model.AccountId }, EventTopicNames.DisplayAccountTransactions, EventTopicNames.SelectEntity);
        }

        private bool CanDisplayAccount(string arg)
        {
            return SelectedEntity != null && SelectedEntity.Model.AccountId > 0;
        }

        private bool CanEditEntity(string arg)
        {
            return SelectedEntity != null;
        }

        private void OnEditEntity(string obj)
        {
            var targetEvent = _currentEntitySelectionRequest != null
                                  ? _currentEntitySelectionRequest.GetExpectedEvent()
                                  : EventTopicNames.SelectEntity;

            CommonEventPublisher.PublishEntityOperation(SelectedEntity.Model,
                EventTopicNames.EditEntityDetails, targetEvent);
        }

        private void OnRemoveEntity(string obj)
        {
            _currentEntitySelectionRequest.Publish(Entity.GetNullEntity(SelectedEntityType.Id));
        }

        private bool CanRemoveEntity(string arg)
        {
            return _applicationState.IsLocked && _currentEntitySelectionRequest != null &&
                   _currentEntitySelectionRequest.SelectedEntity != null &&
                   ((SelectedEntity != null && _currentEntitySelectionRequest.SelectedEntity.Id == SelectedEntity.Id) ||
                    _currentEntitySelectionRequest.SelectedEntity.Id == 0);
        }

        private bool CanCreateEntity(string arg)
        {
            return SelectedEntityType != null && CanCreateNewEntity;
        }

        private void OnCreateEntity(string obj)
        {
            var targetEvent = _currentEntitySelectionRequest != null
                                  ? _currentEntitySelectionRequest.GetExpectedEvent()
                                  : EventTopicNames.SelectEntity;
            var newEntity = new Entity { EntityTypeId = SelectedEntityType.Id, Name = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(SearchString ?? "") };
            ClearSearchValues();
            CommonEventPublisher.PublishEntityOperation(newEntity, EventTopicNames.EditEntityDetails, targetEvent);
        }

        private bool CanSelectEntity(string arg)
        {
            return
                _applicationState.IsCurrentWorkPeriodOpen
                && _applicationState.CurrentDepartment != null
                && SelectedEntity != null
                && !string.IsNullOrEmpty(SelectedEntity.Name);
        }

        private void OnSelectEntity(string obj)
        {
            if (_currentEntitySelectionRequest != null)
            {
                _currentEntitySelectionRequest.Publish(SelectedEntity.Model);
            }
            ClearSearchValues();
        }

        public void RefreshSelectedEntity(EntityOperationRequest<Entity> value)
        {
            ClearSearchValues();
            _currentEntitySelectionRequest = value;

            if (_currentEntitySelectionRequest != null && _currentEntitySelectionRequest.SelectedEntity != null && !string.IsNullOrEmpty(_currentEntitySelectionRequest.SelectedEntity.Name))
            {
                ClearSearchValues();
                if (_currentEntitySelectionRequest.SelectedEntity.Name != "*" && _currentEntitySelectionRequest.SelectedEntity.EntityTypeId == SelectedEntityType.Id)
                {
                    FoundEntities.Add(new EntitySearchResultViewModel(_currentEntitySelectionRequest.SelectedEntity, SelectedEntityType));
                }
            }

            RaisePropertyChanged(() => SelectedEntityType);
            RaisePropertyChanged(() => SelectedEntity);
            RaisePropertyChanged(() => EntityTypes);
        }

        private void ClearSearchValues()
        {
            FoundEntities.Clear();
            SearchString = "";
        }

        private void ResetTimer()
        {
            CanCreateNewEntity = true;
            _updateTimer.Stop();
            if (!string.IsNullOrEmpty(SearchString))
            {
                CanCreateNewEntity = false;
                _updateTimer.Start();
            }
            else FoundEntities.Clear();
        }

        void UpdateTimerElapsed(object sender, ElapsedEventArgs e)
        {
            _updateTimer.Stop();
            UpdateFoundEntities();
        }

        private void UpdateFoundEntities()
        {
            CanCreateNewEntity = true;
            var result = new List<Entity>();

            using (var worker = new BackgroundWorker())
            {
                worker.DoWork += delegate
                                     {
                                         result = _entityService.SearchEntities(SearchString, SelectedEntityType, StateFilter);
                                     };

                worker.RunWorkerCompleted +=
                    delegate
                    {

                        _applicationState.MainDispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(
                               delegate
                               {
                                   FoundEntities.Clear();
                                   FoundEntities.AddRange(result.Select(x => new EntitySearchResultViewModel(x, SelectedEntityType)));

                                   if (SelectedEntity != null && SearchString == SelectedEntity.PhoneNumber)
                                   {
                                       SelectedEntity.UpdateDetailedInfo();
                                   }

                                   RaisePropertyChanged(() => SelectedEntity);
                                   CommandManager.InvalidateRequerySuggested();
                               }));

                    };

                worker.RunWorkerAsync();
            }
        }

        public void Refresh(int entityType, string stateFilter, EntityOperationRequest<Entity> currentOperationRequest)
        {
            StateFilter = stateFilter;
            SelectedEntityType = _cacheService.GetEntityTypeById(entityType);
            RefreshSelectedEntity(currentOperationRequest);
        }
    }
}
