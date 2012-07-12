using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.Linq;
using Microsoft.Practices.Prism.Events;
using Microsoft.Practices.Prism.Regions;
using Samba.Domain.Models.Resources;
using Samba.Domain.Models.Settings;
using Samba.Domain.Models.Tickets;
using Samba.Infrastructure;
using Samba.Localization.Properties;
using Samba.Presentation.Common;
using Samba.Presentation.Common.Services;
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
        private readonly TicketExplorerViewModel _ticketExplorerViewModel;
        private readonly TicketListViewModel _ticketListViewModel;
        private readonly MenuItemSelectorView _menuItemSelectorView;
        private readonly TicketViewModel _ticketViewModel;
        private readonly TicketOrdersViewModel _ticketOrdersViewModel;

        private Resource _lastSelectedResource;

        private Ticket _selectedTicket;
        public Ticket SelectedTicket
        {
            get { return _selectedTicket; }
            set
            {
                _selectedTicket = value;
                _ticketViewModel.SelectedTicket = value;
            }
        }

        [ImportingConstructor]
        public PosViewModel(IRegionManager regionManager, IApplicationState applicationState, IApplicationStateSetter applicationStateSetter,
            ITicketService ticketService, IUserService userService, ICacheService cacheService,
            TicketExplorerViewModel ticketExplorerViewModel, TicketListViewModel ticketListViewModel,
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
            _ticketExplorerViewModel = ticketExplorerViewModel;
            _ticketListViewModel = ticketListViewModel;

            EventServiceFactory.EventService.GetEvent<GenericEvent<Ticket>>().Subscribe(OnTicketEventReceived);
            EventServiceFactory.EventService.GetEvent<GenericEvent<WorkPeriod>>().Subscribe(OnWorkPeriodEvent);
            EventServiceFactory.EventService.GetEvent<GenericEvent<SelectedOrdersData>>().Subscribe(OnSelectedOrdersChanged);
            EventServiceFactory.EventService.GetEvent<GenericEvent<EventAggregator>>().Subscribe(OnTicketEvent);
            EventServiceFactory.EventService.GetEvent<GenericEvent<ScreenMenuItemData>>().Subscribe(OnMenuItemSelected);
            EventServiceFactory.EventService.GetEvent<GenericIdEvent>().Subscribe(OnTicketIdPublished);
            EventServiceFactory.EventService.GetEvent<GenericEvent<EntityOperationRequest<Resource>>>().Subscribe(OnResourceSelectedForTicket);
            EventServiceFactory.EventService.GetEvent<GenericEvent<TicketTagGroup>>().Subscribe(OnTicketTagSelected);
        }

        private void OnTicketTagSelected(EventParameters<TicketTagGroup> obj)
        {
            if (obj.Topic == EventTopicNames.ActivateTicketList)
            {
                _ticketListViewModel.UpdateListByTicketTagGroup(obj.Value);
                if (_ticketListViewModel.Tickets.Count() > 0)
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
                    CloseTicket();
                }
                else
                {
                    var openTickets = _ticketService.GetOpenTicketIds(eventParameters.Value.SelectedEntity.Id).ToList();
                    if (openTickets.Count() == 0)
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
                if(SelectedTicket != null) return;
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
                    DisplaySingleTicket();
                }
                Debug.Assert(SelectedTicket != null);
                _ticketOrdersViewModel.AddOrder(obj.Value);
                _ticketViewModel.RefreshSelectedTicket();
                _ticketViewModel.RefreshSelectedItems();
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
                _ticketService.UpdateResource(SelectedTicket, ticketResource.ResourceTemplateId, ticketResource.ResourceId, ticketResource.ResourceName);
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
                    DisplayTickets();
                    DisplayMenuScreen();
                    break;
                case EventTopicNames.RefreshSelectedTicket:
                    DisplaySingleTicket();
                    break;
                case EventTopicNames.CloseTicketRequested:
                    CloseTicket();
                    DisplayMenuScreen();
                    break;
            }
        }

        private void OnSelectedOrdersChanged(EventParameters<SelectedOrdersData> obj)
        {
            if (obj.Topic == EventTopicNames.SelectedOrdersChanged)
            {
                if (obj.Value.SelectedOrders.Count() != 1)
                    DisplayMenuScreen();
            }
        }

        private void OnWorkPeriodEvent(EventParameters<WorkPeriod> obj)
        {
            if (obj.Topic == EventTopicNames.DisplayTicketExplorer)
            {
                DisplayTicketExplorerScreen();
            }
        }

        public void DisplayTickets()
        {
            _lastSelectedResource = null;

            if (_applicationState.CurrentDepartment == null && _applicationState.CurrentLoggedInUser.UserRole.DepartmentId > 0)
                _applicationStateSetter.SetCurrentDepartment(_applicationState.CurrentLoggedInUser.UserRole.DepartmentId);

            Debug.Assert(_applicationState.CurrentDepartment != null);

            if (SelectedTicket != null || _applicationState.CurrentDepartment.ResourceScreens.Count == 0 || _applicationState.CurrentDepartment.TicketCreationMethod == 1)
            {
                DisplaySingleTicket();
                return;
            }
            CommonEventPublisher.PublishEntityOperation<Resource>(null, EventTopicNames.SelectResource, EventTopicNames.ResourceSelected);
        }

        private void DisplaySingleTicket()
        {
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

        public void DisplayMenuScreen()
        {
            _regionManager.RequestNavigate(RegionNames.MainRegion, new Uri("PosView", UriKind.Relative));
            _regionManager.RequestNavigate(RegionNames.PosSubRegion, new Uri("MenuItemSelectorView", UriKind.Relative));
        }

        public void DisplayTicketExplorerScreen()
        {
            _regionManager.RequestNavigate(RegionNames.MainRegion, new Uri("PosView", UriKind.Relative));
            _regionManager.RequestNavigate(RegionNames.PosSubRegion, new Uri("TicketExplorerView", UriKind.Relative));

            _ticketExplorerViewModel.StartDate = _applicationState.CurrentWorkPeriod.StartDate.Date;
            if (!_userService.IsUserPermittedFor(PermissionNames.DisplayOldTickets))
            {
                _ticketExplorerViewModel.StartDate = _applicationState.CurrentWorkPeriod.StartDate;
            }
            _ticketExplorerViewModel.EndDate = DateTime.Now;
            _ticketExplorerViewModel.Refresh();
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
            else EventServiceFactory.EventService.PublishEvent(EventTopicNames.ActivatePosView);

            AppServices.MessagingService.SendMessage(Messages.TicketRefreshMessage, result.TicketId.ToString());
            _applicationStateSetter.SetApplicationLocked(false);
        }

        public string GetPrintError()
        {
            if (SelectedTicket.Orders.Count(x => x.Price == 0 && x.CalculatePrice) > 0)
                return Resources.CantCompleteOperationWhenThereIsZeroPricedProduct;
            if (!SelectedTicket.IsPaid && SelectedTicket.Orders.Count > 0)
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
