using System;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.Linq;
using Microsoft.Practices.Prism.Events;
using Microsoft.Practices.Prism.Regions;
using Samba.Domain.Models.Accounts;
using Samba.Domain.Models.Settings;
using Samba.Domain.Models.Tickets;
using Samba.Domain.Models.Users;
using Samba.Presentation.Common;
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
        private readonly IApplicationState _applicationState;
        private readonly IApplicationStateSetter _applicationStateSetter;
        private readonly IRegionManager _regionManager;
        private readonly MenuItemSelectorViewModel _menuItemSelectorViewModel;
        private readonly TicketExplorerViewModel _ticketExplorerViewModel;
        private readonly MenuItemSelectorView _menuItemSelectorView;
        private readonly TicketListViewModel _ticketListViewModel;

        [ImportingConstructor]
        public PosViewModel(IRegionManager regionManager, IApplicationState applicationState, IApplicationStateSetter applicationStateSetter,
            ITicketService ticketService, IUserService userService, TicketExplorerViewModel ticketExplorerViewModel,
            MenuItemSelectorViewModel menuItemSelectorViewModel, MenuItemSelectorView menuItemSelectorView, TicketListViewModel ticketListViewModel)
        {
            _ticketService = ticketService;
            _userService = userService;
            _applicationState = applicationState;
            _applicationStateSetter = applicationStateSetter;
            _regionManager = regionManager;
            _menuItemSelectorView = menuItemSelectorView;
            _ticketListViewModel = ticketListViewModel;

            _menuItemSelectorViewModel = menuItemSelectorViewModel;
            _ticketExplorerViewModel = ticketExplorerViewModel;

            EventServiceFactory.EventService.GetEvent<GenericEvent<NavigationRequest>>().Subscribe(OnNavigationRequest);
            EventServiceFactory.EventService.GetEvent<GenericEvent<User>>().Subscribe(OnUserLoginEvent);
            EventServiceFactory.EventService.GetEvent<GenericEvent<WorkPeriod>>().Subscribe(OnWorkPeriodEvent);
            EventServiceFactory.EventService.GetEvent<GenericEvent<SelectedOrdersData>>().Subscribe(
                x =>
                {
                    if (x.Topic == EventTopicNames.SelectedOrdersChanged)
                    {
                        if (x.Value.SelectedOrders.Count() != 1)
                            DisplayMenuScreen();
                    }
                });

            EventServiceFactory.EventService.GetEvent<GenericEvent<EventAggregator>>().Subscribe(
                 x =>
                 {
                     switch (x.Topic)
                     {
                         case EventTopicNames.ActivatePosView:
                             DisplayTickets();
                             DisplayMenuScreen();
                             break;
                         case EventTopicNames.ActivateTicket:
                             DisplaySingleTicket();
                             break;
                         case EventTopicNames.PaymentSubmitted:
                             DisplayMenuScreen();
                             break;
                     }
                 });

            EventServiceFactory.EventService.GetEvent<GenericEvent<Department>>().Subscribe(
                x =>
                {
                    if (x.Topic == EventTopicNames.ActivateOpenTickets)
                    {
                        DisplayOpenTickets();
                    }
                });
        }

        private void OnNavigationRequest(EventParameters<NavigationRequest> obj)
        {
            EventServiceFactory.EventService.PublishEvent(_ticketListViewModel.SelectedTicket != null
                                                              ? EventTopicNames.ActivateTicket
                                                              : obj.Value.RequestedEvent);
        }

        public CaptionCommand<Ticket> DisplayPaymentScreenCommand { get; set; }

        private void OnUserLoginEvent(EventParameters<User> obj)
        {
            if (obj.Topic == EventTopicNames.UserLoggedOut)
            {
                CloseTicket();
            }
        }

        private void CloseTicket()
        {
            if (_ticketListViewModel.SelectedTicket != null)
                _ticketService.CloseTicket(_ticketListViewModel.SelectedTicket.Model);
            //todo fix
            //_ticketListViewModel.SelectedDepartment = null;
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
            if (_applicationState.CurrentDepartment == null && _applicationState.CurrentLoggedInUser.UserRole.DepartmentId > 0)
                _applicationStateSetter.SetCurrentDepartment(_applicationState.CurrentLoggedInUser.UserRole.DepartmentId);
            
            Debug.Assert(_applicationState.CurrentDepartment != null);

            if (_ticketListViewModel.SelectedTicket != null)
            {
                EventServiceFactory.EventService.PublishEvent(EventTopicNames.ActivateTicket);
                return;
            }

            _applicationState.CurrentDepartment.PublishEvent(EventTopicNames.ActivateOpenTickets);

            if (_applicationState.CurrentDepartment.IsAlaCarte && _applicationState.CurrentDepartment.LocationScreens.Count > 0)
            {
                CommonEventPublisher.PublishEntityOperation<AccountScreenItem>(null, EventTopicNames.SelectLocation, EventTopicNames.LocationSelectedForTicket);
            }
            else if (_applicationState.CurrentDepartment.IsTakeAway)
            {
                _applicationState.CurrentDepartment.PublishEvent(EventTopicNames.SelectAccount);
            }
            else if (_applicationState.CurrentDepartment.IsFastFood)
                EventServiceFactory.EventService.PublishEvent(EventTopicNames.ActivateTicket);
        }

        private void DisplaySingleTicket()
        {
            _regionManager.RequestNavigate(RegionNames.MainRegion, new Uri("PosView", UriKind.Relative));
            _regionManager.RequestNavigate(RegionNames.PosMainRegion, new Uri("TicketListView", UriKind.Relative));
        }

        public void DisplayOpenTickets()
        {
            _regionManager.RequestNavigate(RegionNames.MainRegion, new Uri("PosView", UriKind.Relative));
            _regionManager.RequestNavigate(RegionNames.PosMainRegion, new Uri("OpenTicketsView", UriKind.Relative));
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
    }
}
