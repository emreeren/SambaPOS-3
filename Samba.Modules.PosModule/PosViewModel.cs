using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using Microsoft.Practices.Prism.Events;
using Microsoft.Practices.Prism.Regions;
using Samba.Domain.Models.Entities;
using Samba.Domain.Models.Tickets;
using Samba.Infrastructure.Messaging;
using Samba.Localization.Properties;
using Samba.Presentation.Common;
using Samba.Presentation.Common.Services;
using Samba.Presentation.Services;
using Samba.Presentation.Services.Common;
using Samba.Presentation.ViewModels;
using Samba.Services;
using Samba.Services.Common;

namespace Samba.Modules.PosModule
{
    [Export]
    public class PosViewModel : ObservableObject
    {
        private readonly ITicketService _ticketService;
        private readonly IUserService _userService;
        private readonly ICacheService _cacheService;
        private readonly IApplicationState _applicationState;
        private readonly IApplicationStateSetter _applicationStateSetter;
        private readonly IRegionManager _regionManager;
        private readonly MenuItemSelectorViewModel _menuItemSelectorViewModel;
        private readonly TicketListViewModel _ticketListViewModel;
        private readonly TicketTagListViewModel _ticketTagListViewModel;
        private readonly TicketEntityListViewModel _ticketEntityListViewModel;
        private readonly TicketTypeListViewModel _ticketTypeListViewModel;
        private readonly MenuItemSelectorView _menuItemSelectorView;
        private readonly TicketViewModel _ticketViewModel;
        private readonly TicketOrdersViewModel _ticketOrdersViewModel;

        private Entity _lastSelectedEntity;
        protected Action ExpectedAction { get; set; }

        private Ticket _selectedTicket;
        public Ticket SelectedTicket
        {
            get { return _selectedTicket; }
            set
            {
                _selectedTicket = value;
                _ticketViewModel.SelectedTicket = value;
                if (value != null)
                {
                    var template = _cacheService.GetTicketTypeById(SelectedTicket.TicketTypeId);
                    if (template != null) _menuItemSelectorViewModel.UpdateCurrentScreenMenu(template.ScreenMenuId);
                }
            }
        }

        [ImportingConstructor]
        public PosViewModel(IRegionManager regionManager, IApplicationState applicationState, IApplicationStateSetter applicationStateSetter,
            ITicketService ticketService, IUserService userService, ICacheService cacheService,
            TicketListViewModel ticketListViewModel, TicketTagListViewModel ticketTagListViewModel, MenuItemSelectorViewModel menuItemSelectorViewModel,
            MenuItemSelectorView menuItemSelectorView, TicketViewModel ticketViewModel, TicketOrdersViewModel ticketOrdersViewModel,
            TicketEntityListViewModel ticketEntityListViewModel, TicketTypeListViewModel ticketTypeListViewModel)
        {
            _ticketService = ticketService;
            _userService = userService;
            _cacheService = cacheService;
            _applicationState = applicationState;
            _applicationStateSetter = applicationStateSetter;
            _regionManager = regionManager;
            _menuItemSelectorView = menuItemSelectorView;
            _ticketViewModel = ticketViewModel;
            _ticketOrdersViewModel = ticketOrdersViewModel;
            _menuItemSelectorViewModel = menuItemSelectorViewModel;
            _ticketListViewModel = ticketListViewModel;
            _ticketTagListViewModel = ticketTagListViewModel;
            _ticketEntityListViewModel = ticketEntityListViewModel;
            _ticketTypeListViewModel = ticketTypeListViewModel;

            EventServiceFactory.EventService.GetEvent<GenericEvent<Ticket>>().Subscribe(OnTicketEventReceived);
            EventServiceFactory.EventService.GetEvent<GenericEvent<SelectedOrdersData>>().Subscribe(OnSelectedOrdersChanged);
            EventServiceFactory.EventService.GetEvent<GenericEvent<EventAggregator>>().Subscribe(OnTicketEvent);
            EventServiceFactory.EventService.GetEvent<GenericEvent<ScreenMenuItemData>>().Subscribe(OnMenuItemSelected);
            EventServiceFactory.EventService.GetEvent<GenericIdEvent>().Subscribe(OnTicketIdPublished);
            EventServiceFactory.EventService.GetEvent<GenericEvent<EntityOperationRequest<Entity>>>().Subscribe(OnEntitySelectedForTicket);
            EventServiceFactory.EventService.GetEvent<GenericEvent<TicketTagGroup>>().Subscribe(OnTicketTagSelected);
            EventServiceFactory.EventService.GetEvent<GenericEvent<TicketStateData>>().Subscribe(OnTicketStateSelected);
            EventServiceFactory.EventService.GetEvent<GenericEvent<TicketType>>().Subscribe(OnTicketTypeChanged);

            EventServiceFactory.EventService.GetEvent<GenericEvent<EventAggregator>>().Subscribe(
            x =>
            {
                if (x.Topic == EventTopicNames.ResetCache && _applicationState.CurrentTicketType != null)
                {
                    _menuItemSelectorViewModel.Reset();
                    _menuItemSelectorViewModel.UpdateCurrentScreenMenu(_applicationState.CurrentTicketType.ScreenMenuId);
                }
            });
        }

        private void OnTicketTypeChanged(EventParameters<TicketType> obj)
        {
            if (obj.Topic == EventTopicNames.TicketTypeChanged && obj.Value != null)
            {
                _menuItemSelectorViewModel.UpdateCurrentScreenMenu(obj.Value.ScreenMenuId);
            }

            if (obj.Topic == EventTopicNames.TicketTypeSelected && obj.Value != null)
            {
                _applicationState.TempTicketType = obj.Value;
                new EntityOperationRequest<Entity>(_lastSelectedEntity, null).PublishEvent(EventTopicNames.EntitySelected, true);
            }
        }

        private void OnTicketStateSelected(EventParameters<TicketStateData> obj)
        {
            if (obj.Topic == EventTopicNames.ActivateTicketList)
            {
                if (SelectedTicket != null) CloseTicket();
                _ticketListViewModel.UpdateListByTicketState(obj.Value);
                if (_ticketListViewModel.Tickets.Any())
                    DisplayTicketList();
                else DisplayTickets();
            }
        }

        private void OnTicketTagSelected(EventParameters<TicketTagGroup> obj)
        {
            if (obj.Topic == EventTopicNames.ActivateTicketList)
            {
                if (SelectedTicket != null) CloseTicket();
                _ticketListViewModel.UpdateListByTicketTagGroup(obj.Value);
                if (_ticketListViewModel.Tickets.Any())
                    DisplayTicketList();
                else DisplayTickets();
            }
        }

        private void OnTicketEventReceived(EventParameters<Ticket> obj)
        {
            if (obj.Topic == EventTopicNames.MoveSelectedOrders)
            {
                var newTicketId = _ticketService.MoveOrders(SelectedTicket, SelectedTicket.ExtractSelectedOrders().ToArray(), 0).TicketId;
                SelectedTicket = null;
                OpenTicket(newTicketId);
                DisplaySingleTicket();
            }
        }

        private void OnEntitySelectedForTicket(EventParameters<EntityOperationRequest<Entity>> eventParameters)
        {
            if (eventParameters.Topic == EventTopicNames.EntitySelected)
            {
                FireEntitySelectedRule(eventParameters.Value.SelectedEntity);

                if (SelectedTicket != null)
                {
                    _ticketService.UpdateEntity(SelectedTicket, eventParameters.Value.SelectedEntity);
                    if (_applicationState.SelectedEntityScreen != null
                        && SelectedTicket.Orders.Count > 0 && eventParameters.Value.SelectedEntity.Id > 0
                        && _applicationState.TempEntityScreen != null
                        && eventParameters.Value.SelectedEntity.EntityTypeId == _applicationState.TempEntityScreen.EntityTypeId)
                        CloseTicket();
                    else DisplaySingleTicket();
                }
                else
                {
                    var openTickets = _ticketService.GetOpenTicketIds(eventParameters.Value.SelectedEntity.Id).ToList();
                    if (!openTickets.Any())
                    {
                        if (_applicationState.SelectedEntityScreen != null &&
                            _applicationState.SelectedEntityScreen.AskTicketType &&
                            _cacheService.GetTicketTypes().Count() > 1 &&
                            _applicationState.TempTicketType == null)
                        {
                            _lastSelectedEntity = eventParameters.Value.SelectedEntity;
                            DisplayTicketTypeList();
                            return;
                        }

                        if (_applicationState.TempTicketType != null)
                        {
                            _applicationStateSetter.SetCurrentTicketType(_applicationState.TempTicketType);
                            _applicationState.TempTicketType = null;
                        }

                        OpenTicket(0);
                        _ticketService.UpdateEntity(SelectedTicket, eventParameters.Value.SelectedEntity);
                    }
                    else if (openTickets.Count > 1)
                    {
                        _lastSelectedEntity = eventParameters.Value.SelectedEntity;
                        _ticketListViewModel.UpdateListByEntity(eventParameters.Value.SelectedEntity);
                        DisplayTicketList();
                        return;
                    }
                    else
                    {
                        OpenTicket(openTickets.ElementAt(0));
                    }
                    EventServiceFactory.EventService.PublishEvent(EventTopicNames.ActivatePosView);
                }
            }
        }

        private void FireEntitySelectedRule(Entity entity)
        {
            if (entity != null && entity != Entity.Null)
            {
                var entityType = _cacheService.GetEntityTypeById(entity.EntityTypeId);
                if (entityType != null)
                {
                    _applicationState.NotifyEvent(RuleEventNames.EntitySelected, new
                        {
                            Ticket = SelectedTicket,
                            EntityTypeName = entityType.Name,
                            EntityName = entity.Name,
                            EntityCustomData = entity.CustomData,
                            IsTicketSelected = SelectedTicket != null
                        });
                }
            }
        }

        private void OnTicketIdPublished(EventParameters<int> obj)
        {
            if (obj.Topic == EventTopicNames.DisplayTicket)
            {
                if (SelectedTicket != null) CloseTicket();
                if (SelectedTicket != null) return;
                ExpectedAction = obj.ExpectedAction;
                OpenTicket(obj.Value);
                EventServiceFactory.EventService.PublishEvent(EventTopicNames.RefreshSelectedTicket);
            }
        }

        private void OnMenuItemSelected(EventParameters<ScreenMenuItemData> obj)
        {
            if (obj.Topic == EventTopicNames.ScreenMenuItemDataSelected)
            {
                if (SelectedTicket == null)
                {
                    OpenTicket(0);
                    _ticketService.UpdateEntity(SelectedTicket, _lastSelectedEntity);
                }

                Debug.Assert(SelectedTicket != null);
                _ticketOrdersViewModel.AddOrder(obj.Value);
                DisplaySingleTicket();
            }
        }

        private void CreateTicket()
        {
            IEnumerable<TicketEntity> tr = new List<TicketEntity>();
            if (SelectedTicket != null)
            {
                tr = SelectedTicket.TicketEntities;
                CloseTicket();
                if (SelectedTicket != null) return;
            }

            OpenTicket(0);
            foreach (var ticketEntity in tr)
            {
                _ticketService.UpdateEntity(SelectedTicket, ticketEntity.EntityTypeId, ticketEntity.EntityId, ticketEntity.EntityName, ticketEntity.AccountTypeId, ticketEntity.AccountId, ticketEntity.EntityCustomData);
            }
        }

        private void OnTicketEvent(EventParameters<EventAggregator> obj)
        {
            switch (obj.Topic)
            {
                case EventTopicNames.CreateTicket:
                    CreateTicket();
                    EventServiceFactory.EventService.PublishEvent(EventTopicNames.RefreshSelectedTicket);
                    break;
                case EventTopicNames.ActivatePosView:
                    if (SelectedTicket == null || _ticketService.CanDeselectOrders(SelectedTicket.SelectedOrders))
                    {
                        DisplayTickets();
                        DisplayMenuScreen();
                        _ticketViewModel.ResetTicket();
                    }
                    break;
                case EventTopicNames.RefreshSelectedTicket:
                    DisplayMenuScreen();
                    DisplaySingleTicket();
                    break;
                case EventTopicNames.CloseTicketRequested:
                    DisplayMenuScreen();
                    CloseTicket();
                    break;
            }
        }

        private void OnSelectedOrdersChanged(EventParameters<SelectedOrdersData> obj)
        {
            if (obj.Topic == EventTopicNames.SelectedOrdersChanged)
            {
                if (obj.Value.SelectedOrders == null || obj.Value.SelectedOrders.Count() != 1)
                    DisplayMenuScreen();
            }
        }

        public void DisplayTickets()
        {
            _lastSelectedEntity = null;
            Debug.Assert(_applicationState.CurrentDepartment != null);
            if (SelectedTicket != null || !_applicationState.GetTicketEntityScreens().Any() || _applicationState.CurrentDepartment.TicketCreationMethod == 1)
            {
                _applicationStateSetter.SetCurrentApplicationScreen(AppScreens.TicketView);
                DisplaySingleTicket();
                return;
            }
            CommonEventPublisher.PublishEntityOperation<Entity>(null, EventTopicNames.SelectEntity, EventTopicNames.EntitySelected);
        }

        private void DisplaySingleTicket()
        {
            _applicationStateSetter.SetCurrentApplicationScreen(AppScreens.TicketView);
            if (SelectedTicket != null && SelectedTicket.Orders.Count == 0 && _cacheService.GetTicketTypeById(SelectedTicket.TicketTypeId).EntityTypeAssignments.Any(x => x.AskBeforeCreatingTicket && SelectedTicket.TicketEntities.All(y => y.EntityTypeId != x.EntityTypeId)))
            {
                _ticketEntityListViewModel.Update(SelectedTicket);
                DisplayTicketEntityList();
                return;
            }
            if (SelectedTicket != null && SelectedTicket.Orders.Count == 0 && _applicationState.GetTicketTagGroups().Count(x => x.AskBeforeCreatingTicket && !SelectedTicket.IsTaggedWith(x.Name)) > 0)
            {
                _ticketTagListViewModel.Update(SelectedTicket);
                DisplayTicketTagList();
                return;
            }
            _regionManager.RequestNavigate(RegionNames.MainRegion, new Uri("PosView", UriKind.Relative));
            _regionManager.RequestNavigate(RegionNames.PosMainRegion, new Uri("TicketView", UriKind.Relative));
            _ticketViewModel.RefreshSelectedItems();
            _ticketViewModel.RefreshVisuals();
        }

        private void DisplayTicketTypeList()
        {
            _ticketTypeListViewModel.Update();
            _applicationStateSetter.SetCurrentApplicationScreen(AppScreens.TicketView);
            _regionManager.RequestNavigate(RegionNames.MainRegion, new Uri("PosView", UriKind.Relative));
            _regionManager.RequestNavigate(RegionNames.PosMainRegion, new Uri("TicketTypeListView", UriKind.Relative));
        }

        private void DisplayTicketList()
        {
            _applicationStateSetter.SetCurrentApplicationScreen(AppScreens.TicketView);
            _regionManager.RequestNavigate(RegionNames.MainRegion, new Uri("PosView", UriKind.Relative));
            _regionManager.RequestNavigate(RegionNames.PosMainRegion, new Uri("TicketListView", UriKind.Relative));
        }

        private void DisplayTicketTagList()
        {
            _applicationStateSetter.SetCurrentApplicationScreen(AppScreens.TicketView);
            _regionManager.RequestNavigate(RegionNames.MainRegion, new Uri("PosView", UriKind.Relative));
            _regionManager.RequestNavigate(RegionNames.PosMainRegion, new Uri("TicketTagListView", UriKind.Relative));
        }

        private void DisplayTicketEntityList()
        {
            _applicationStateSetter.SetCurrentApplicationScreen(AppScreens.TicketView);
            _regionManager.RequestNavigate(RegionNames.MainRegion, new Uri("PosView", UriKind.Relative));
            _regionManager.RequestNavigate(RegionNames.PosMainRegion, new Uri("TicketEntityListView", UriKind.Relative));
        }

        public void DisplayMenuScreen()
        {
            _regionManager.RequestNavigate(RegionNames.MainRegion, new Uri("PosView", UriKind.Relative));
            _regionManager.RequestNavigate(RegionNames.PosSubRegion, new Uri("MenuItemSelectorView", UriKind.Relative));
        }

        public bool HandleTextInput(string text)
        {
            return _regionManager.Regions[RegionNames.PosSubRegion].ActiveViews.Contains(_menuItemSelectorView)
                && _menuItemSelectorViewModel.HandleTextInput(text);
        }

        public void OpenTicket(int id)
        {
            _applicationStateSetter.SetApplicationLocked(true);
            SelectedTicket = _ticketService.OpenTicket(id);
        }

        private void CloseTicket()
        {
            if (SelectedTicket == null) return;

            if (!SelectedTicket.CanCloseTicket())
            {
                SaveTicketIfNew();
                _ticketViewModel.RefreshVisuals();
                return;
            }

            if (_ticketOrdersViewModel.Orders.Count > 0 && SelectedTicket.GetRemainingAmount() == 0)
            {
                var message = GetPrintError();
                if (!string.IsNullOrEmpty(message))
                {
                    _ticketOrdersViewModel.ClearSelectedOrders();
                    _ticketViewModel.RefreshVisuals();
                    InteractionService.UserIntraction.GiveFeedback(message);
                    return;
                }
            }

            _ticketOrdersViewModel.ClearSelectedOrders();
            var result = _ticketService.CloseTicket(SelectedTicket);
            if (!string.IsNullOrEmpty(result.ErrorMessage))
            {
                InteractionService.UserIntraction.GiveFeedback(result.ErrorMessage);
            }

            SelectedTicket = null;

            if (_applicationState.CurrentTerminal.AutoLogout)
            {
                _userService.LogoutUser(false);
            }
            else
            {
                if (ExpectedAction != null)
                {
                    ExpectedAction.Invoke();
                }
                else EventServiceFactory.EventService.PublishEvent(EventTopicNames.ActivatePosView);
            }
            ExpectedAction = null;
            AppServices.MessagingService.SendMessage(Messages.TicketRefreshMessage, result.TicketId.ToString(CultureInfo.InvariantCulture));
            _applicationStateSetter.SetApplicationLocked(false);
        }

        public string GetPrintError()
        {
            if (SelectedTicket.Orders.Count(x => x.Price == 0 && x.CalculatePrice) > 0)
                return Resources.CantCompleteOperationWhenThereIsZeroPricedProduct;
            if (!SelectedTicket.IsClosed && SelectedTicket.Orders.Count > 0)
            {
                if (_applicationState.GetTicketTagGroups().Any(x => x.ForceValue && !_ticketViewModel.IsTaggedWith(x.Name)))
                    return string.Format(Resources.TagCantBeEmpty_f, _applicationState.GetTicketTagGroups().First(x => x.ForceValue && !_ticketViewModel.IsTaggedWith(x.Name)).Name);
            }
            return "";
        }

        private void SaveTicketIfNew()
        {
            if ((SelectedTicket.Id == 0 || _ticketOrdersViewModel.Orders.Any(x => x.Model.Id == 0)) && _ticketOrdersViewModel.Orders.Count > 0)
            {
                var result = _ticketService.CloseTicket(SelectedTicket);
                OpenTicket(result.TicketId);
            }
        }
    }
}
