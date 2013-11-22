using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.Composition;
using System.Linq;
using System.Windows;
using Microsoft.Practices.Prism.Commands;
using Samba.Domain.Models.Entities;
using Samba.Domain.Models.Tickets;
using Samba.Infrastructure.Messaging;
using Samba.Localization.Properties;
using Samba.Presentation.Common;
using Samba.Presentation.Common.Commands;
using Samba.Presentation.Services;
using Samba.Presentation.Services.Common;
using Samba.Services;
using Samba.Services.Common;

namespace Samba.Modules.EntityModule
{
    [Export]
    public class EntitySelectorViewModel : ObservableObject
    {
        public DelegateCommand<EntityScreenItemViewModel> EntitySelectionCommand { get; set; }
        public ICaptionCommand IncPageNumberCommand { get; set; }
        public ICaptionCommand DecPageNumberCommand { get; set; }
        public ObservableCollection<EntityScreenItemViewModel> EntityScreenItems { get; set; }
        public EntityScreen SelectedEntityScreen { get { return _applicationState.SelectedEntityScreen; } }
        public int CurrentPageNo { get; set; }
        public bool IsPageNavigatorVisible { get { return SelectedEntityScreen != null && SelectedEntityScreen.PageCount > 1; } }

        public VerticalAlignment ScreenVerticalAlignment { get { return SelectedEntityScreen != null && SelectedEntityScreen.ButtonHeight > 0 ? VerticalAlignment.Top : VerticalAlignment.Stretch; } }

        private readonly IApplicationState _applicationState;
        private readonly IEntityService _entityService;
        private readonly IUserService _userService;
        private readonly ICacheService _cacheService;
        private readonly IPrinterService _printerService;
        private OperationRequest<Entity> _currentOperationRequest;

        [ImportingConstructor]
        public EntitySelectorViewModel(IApplicationState applicationState, IEntityService entityService,
            IUserService userService, ICacheService cacheService, IPrinterService printerService)
        {
            _applicationState = applicationState;
            _entityService = entityService;
            _userService = userService;
            _cacheService = cacheService;
            _printerService = printerService;

            EntitySelectionCommand = new DelegateCommand<EntityScreenItemViewModel>(OnSelectEntityExecuted);
            IncPageNumberCommand = new CaptionCommand<string>(Resources.NextPage + " >>", OnIncPageNumber, CanIncPageNumber);
            DecPageNumberCommand = new CaptionCommand<string>("<< " + Resources.PreviousPage, OnDecPageNumber, CanDecPageNumber);

            EventServiceFactory.EventService.GetEvent<GenericEvent<Message>>().Subscribe(
                x =>
                {
                    if (_applicationState.ActiveAppScreen == AppScreens.EntityView
                        && x.Topic == EventTopicNames.MessageReceivedEvent
                        && x.Value.Command == Messages.TicketRefreshMessage)
                    {
                        RefreshEntityScreenItems();
                    }
                });
        }

        public string AutomationCommandName { get; set; }
        public string AutomationCommandValue { get; set; }

        public string StateFilter { get; set; }

        public void RefreshEntityScreenItems()
        {
            if (SelectedEntityScreen != null)
                UpdateEntityScreenItems(SelectedEntityScreen);
        }

        private bool CanDecPageNumber(string arg)
        {
            return SelectedEntityScreen != null && CurrentPageNo > 0;
        }

        private void OnDecPageNumber(string obj)
        {
            CurrentPageNo--;
            RefreshEntityScreenItems();
        }

        private bool CanIncPageNumber(string arg)
        {
            return SelectedEntityScreen != null && CurrentPageNo < SelectedEntityScreen.PageCount - 1;
        }

        private void OnIncPageNumber(string obj)
        {
            CurrentPageNo++;
            RefreshEntityScreenItems();
        }

        private void OnSelectEntityExecuted(EntityScreenItemViewModel obj)
        {
            if (!string.IsNullOrWhiteSpace(AutomationCommandName))
            {
                var commandValue = obj.Model.EntityState;
                if (!string.IsNullOrWhiteSpace(AutomationCommandValue))
                {
                    commandValue = AutomationCommandValue;
                    if (commandValue.Contains("{"))
                    {
                        var entity = _cacheService.GetEntityById(obj.Model.EntityId);
                        commandValue = _printerService.ExecuteFunctions(commandValue, entity);
                    }
                }

                _applicationState.NotifyEvent(RuleEventNames.AutomationCommandExecuted,
                                              new
                                                  {
                                                      Ticket = Ticket.Empty,
                                                      EntityId = obj.Model.EntityId,
                                                      EntityTypeId = SelectedEntityScreen.EntityTypeId,
                                                      AutomationCommandName = AutomationCommandName,
                                                      CommandValue = commandValue
                                                  });
                return;
            }
            if (obj.Model.EntityId > 0 && obj.Model.ItemId == 0)
                _currentOperationRequest.Publish(_cacheService.GetEntityById(obj.Model.EntityId));
            else if (obj.Model.ItemId > 0)
            {
                ExtensionMethods.PublishIdEvent(obj.Model.ItemId, EventTopicNames.DisplayTicket);
            }
        }

        private void UpdateEntityScreenItems(EntityScreen entityScreen)
        {
            var entityData = GetEntityScreenItems(entityScreen, StateFilter);
            if (EntityScreenItems != null && (!EntityScreenItems.Any() || EntityScreenItems.Count != entityData.Count() || EntityScreenItems.First().Name != entityData.First().Name)) EntityScreenItems = null;

            UpdateEntityButtons(entityData);

            RaisePropertyChanged(() => EntityScreenItems);
            RaisePropertyChanged(() => SelectedEntityScreen);
            RaisePropertyChanged(() => IsPageNavigatorVisible);
            RaisePropertyChanged(() => ScreenVerticalAlignment);
        }

        private List<EntityScreenItem> GetEntityScreenItems(EntityScreen entityScreen, string stateFilter)
        {
            if (entityScreen.ScreenItems.Count > 0)
                return _entityService.GetCurrentEntityScreenItems(entityScreen, CurrentPageNo, stateFilter).OrderBy(x => x.SortOrder).ToList();
            return
                _entityService.GetEntitiesByState(stateFilter, entityScreen.EntityTypeId)
                .Select(x => new EntityScreenItem(_cacheService.GetEntityTypeById(entityScreen.EntityTypeId), x, stateFilter)).ToList();
        }

        private void UpdateEntityButtons(ICollection<EntityScreenItem> entityData)
        {
            if (EntityScreenItems == null)
            {
                if (SelectedEntityScreen.RowCount > 0 && SelectedEntityScreen.ColumnCount > 0 && entityData.Count > SelectedEntityScreen.ColumnCount * SelectedEntityScreen.RowCount)
                    SelectedEntityScreen.RowCount = 0;
                EntityScreenItems = new ObservableCollection<EntityScreenItemViewModel>();
                EntityScreenItems.AddRange(entityData.Select(x => new
                    EntityScreenItemViewModel(_cacheService, x, SelectedEntityScreen, EntitySelectionCommand,
                   _currentOperationRequest != null && _currentOperationRequest.SelectedItem != null,
                   _userService.IsUserPermittedFor(PermissionNames.MergeTickets))));
            }
            else
            {
                for (var i = 0; i < entityData.Count(); i++)
                {
                    EntityScreenItems[i].Model = entityData.ElementAt(i);
                }
            }
        }

        public void Refresh(EntityScreen entityScreen, string stateFilter, OperationRequest<Entity> currentOperationRequest)
        {
            StateFilter = stateFilter;
            _currentOperationRequest = currentOperationRequest;
            UpdateEntityScreenItems(entityScreen);
        }
    }
}
