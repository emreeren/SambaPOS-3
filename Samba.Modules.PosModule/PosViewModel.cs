using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using Microsoft.Practices.Prism.Events;
using Microsoft.Practices.Prism.Regions;
using Samba.Domain.Models.Resources;
using Samba.Domain.Models.Tickets;
using Samba.Infrastructure;
using Samba.Infrastructure.Messaging;
using Samba.Localization.Properties;
using Samba.Presentation.Common;
using Samba.Presentation.Common.Services;
using Samba.Presentation.Services;
using Samba.Presentation.Services.Common;
using Samba.Presentation.ViewModels;
using Samba.Services;

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
        private readonly MenuItemSelectorView _menuItemSelectorView;
        private readonly TicketViewModel _ticketViewModel;
        private readonly TicketOrdersViewModel _ticketOrdersViewModel;

        private Resource _lastSelectedResource;
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
                    _menuItemSelectorViewModel.UpdateCurrentScreenMenu(template.ScreenMenuId);
                }
            }
        }

        [ImportingConstructor]
        public PosViewModel(IRegionManager regionManager, IApplicationState applicationState, IApplicationStateSetter applicationStateSetter,
            ITicketService ticketService, IUserService userService, ICacheService cacheService,
            TicketListViewModel ticketListViewModel, TicketTagListViewModel ticketTagListViewModel,
            MenuItemSelectorViewModel menuItemSelectorViewModel, MenuItemSelectorView menuItemSelectorView, TicketViewModel ticketViewModel,
            TicketOrdersViewModel ticketOrdersViewModel)
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

            EventServiceFactory.EventService.GetEvent<GenericEvent<Ticket>>().Subscribe(OnTicketEventReceived);
            EventServiceFactory.EventService.GetEvent<GenericEvent<SelectedOrdersData>>().Subscribe(OnSelectedOrdersChanged);
            EventServiceFactory.EventService.GetEvent<GenericEvent<EventAggregator>>().Subscribe(OnTicketEvent);
            EventServiceFactory.EventService.GetEvent<GenericEvent<ScreenMenuItemData>>().Subscribe(OnMenuItemSelected);
            EventServiceFactory.EventService.GetEvent<GenericIdEvent>().Subscribe(OnTicketIdPublished);
            EventServiceFactory.EventService.GetEvent<GenericEvent<EntityOperationRequest<Resource>>>().Subscribe(OnResourceSelectedForTicket);
            EventServiceFactory.EventService.GetEvent<GenericEvent<TicketTagGroup>>().Subscribe(OnTicketTagSelected);
            EventServiceFactory.EventService.GetEvent<GenericEvent<Department>>().Subscribe(OnDepartmentChanged);
        }

        private void OnDepartmentChanged(EventParameters<Department> obj)
        {
            if (obj.Topic == EventTopicNames.SelectedDepartmentChanged)
            {
                _menuItemSelectorViewModel.UpdateCurrentScreenMenu(obj.Value.TicketTypeId);
            }
        }

        private void OnTicketTagSelected(EventParameters<TicketTagGroup> obj)
        {
            if (obj.Topic == EventTopicNames.ActivateTicketList)
            {
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
                _ticketOrdersViewModel.FixSelectedItems();
                var newTicketId = _ticketService.MoveOrders(SelectedTicket, _ticketOrdersViewModel.SelectedOrderModels.ToArray(), 0).TicketId;
                SelectedTicket = null;
                OpenTicket(newTicketId);
                DisplaySingleTicket();
            }
        }

        private void OnResourceSelectedForTicket(EventParameters<EntityOperationRequest<Resource>> eventParameters)
        {
            if (eventParameters.Topic == EventTopicNames.ResourceSelected)
            {
                if (SelectedTicket != null)
                {
                    _ticketService.UpdateResource(SelectedTicket, eventParameters.Value.SelectedEntity);
                    if (_applicationState.SelectedResourceScreen != null
                        && SelectedTicket.Orders.Count > 0 && eventParameters.Value.SelectedEntity.Id > 0
                        && _applicationState.ActiveResourceScreen != null
                        && eventParameters.Value.SelectedEntity.ResourceTypeId == _applicationState.ActiveResourceScreen.ResourceTypeId)
                        CloseTicket();
                    else DisplaySingleTicket();
                }
                else
                {
                    var openTickets = _ticketService.GetOpenTicketIds(eventParameters.Value.SelectedEntity.Id).ToList();
                    if (!openTickets.Any())
                    {
                        OpenTicket(0);
                        _ticketService.UpdateResource(SelectedTicket, eventParameters.Value.SelectedEntity);
                    }
                    else if (openTickets.Count > 1)
                    {
                        _lastSelectedResource = eventParameters.Value.SelectedEntity;
                        _ticketListViewModel.UpdateListByResource(eventParameters.Value.SelectedEntity);
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
                    _ticketService.UpdateResource(SelectedTicket, _lastSelectedResource);
                }

                Debug.Assert(SelectedTicket != null);
                _ticketOrdersViewModel.AddOrder(obj.Value);
                DisplaySingleTicket();
            }
        }

        private void CreateTicket()
        {
            IEnumerable<TicketResource> tr = new List<TicketResource>();
            if (SelectedTicket != null)
            {
                tr = SelectedTicket.TicketResources;
                CloseTicket();
                if (SelectedTicket != null) return;
            }

            OpenTicket(0);
            foreach (var ticketResource in tr)
            {
                _ticketService.UpdateResource(SelectedTicket, ticketResource.ResourceTypeId, ticketResource.ResourceId, ticketResource.ResourceName, ticketResource.AccountId, ticketResource.ResourceCustomData);
            }
        }

        private void OnTicketEvent(EventParameters<EventAggregator> obj)
        {
            switch (obj.Topic)
            {
                case EventTopicNames.CreateTicket:
                    CreateTicket();
                    break;
                case EventTopicNames.ActivatePosView:
                    if (_ticketService.CanDeselectOrders(_ticketOrdersViewModel.SelectedOrderModels))
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
            _lastSelectedResource = null;

            Debug.Assert(_applicationState.CurrentDepartment != null);

            if (SelectedTicket != null || !_cacheService.GetTicketResourceScreens().Any() || _applicationState.CurrentDepartment.TicketCreationMethod == 1)
            {
                DisplaySingleTicket();
                return;
            }
            CommonEventPublisher.PublishEntityOperation<Resource>(null, EventTopicNames.SelectResource, EventTopicNames.ResourceSelected);
        }

        private void DisplaySingleTicket()
        {
            if (SelectedTicket != null && SelectedTicket.Orders.Count == 0 && _cacheService.GetTicketTagGroups().Count(x => x.AskBeforeCreatingTicket && !SelectedTicket.IsTaggedWith(x.Name)) > 0)
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

        private void DisplayTicketList()
        {
            _regionManager.RequestNavigate(RegionNames.MainRegion, new Uri("PosView", UriKind.Relative));
            _regionManager.RequestNavigate(RegionNames.PosMainRegion, new Uri("TicketListView", UriKind.Relative));
        }

        private void DisplayTicketTagList()
        {
            _regionManager.RequestNavigate(RegionNames.MainRegion, new Uri("PosView", UriKind.Relative));
            _regionManager.RequestNavigate(RegionNames.PosMainRegion, new Uri("TicketTagListView", UriKind.Relative));
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
                if (_cacheService.GetTicketTagGroups().Any(x => x.ForceValue && !_ticketViewModel.IsTaggedWith(x.Name)))
                    return string.Format(Resources.TagCantBeEmpty_f, _cacheService.GetTicketTagGroups().First(x => x.ForceValue && !_ticketViewModel.IsTaggedWith(x.Name)).Name);
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
