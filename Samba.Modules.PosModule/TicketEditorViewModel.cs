using System;
using System.ComponentModel.Composition;
using System.Linq;
using Microsoft.Practices.Prism.Events;
using Microsoft.Practices.Prism.Regions;
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
    public class TicketEditorViewModel : ObservableObject
    {
        private readonly ITicketService _ticketService;
        private readonly IUserService _userService;
        private readonly IApplicationState _applicationState;
        private readonly IRegionManager _regionManager;
        private readonly MenuItemSelectorViewModel _menuItemSelectorViewModel;
        private readonly SelectedOrdersViewModel _selectedOrdersViewModel;
        private readonly TicketExplorerViewModel _ticketExplorerViewModel;

        [ImportingConstructor]
        public TicketEditorViewModel(IRegionManager regionManager, IApplicationState applicationState, IApplicationStateSetter applicationStateSetter,
            ITicketService ticketService, IUserService userService, TicketExplorerViewModel ticketExplorerViewModel,
            SelectedOrdersViewModel selectedOrdersViewModel, MenuItemSelectorViewModel menuItemSelectorViewModel)
        {
            _ticketService = ticketService;
            _userService = userService;
            _applicationState = applicationState;
            _regionManager = regionManager;

            _menuItemSelectorViewModel = menuItemSelectorViewModel;
            _ticketExplorerViewModel = ticketExplorerViewModel;
            _selectedOrdersViewModel = selectedOrdersViewModel;
            DisplayCategoriesScreen();

            EventServiceFactory.EventService.GetEvent<GenericEvent<TicketViewModel>>().Subscribe(OnTicketViewModelEvent);
            EventServiceFactory.EventService.GetEvent<GenericEvent<Ticket>>().Subscribe(OnTicketEvent);
            EventServiceFactory.EventService.GetEvent<GenericEvent<User>>().Subscribe(OnUserLoginEvent);
            EventServiceFactory.EventService.GetEvent<GenericEvent<WorkPeriod>>().Subscribe(OnWorkPeriodEvent);

            EventServiceFactory.EventService.GetEvent<GenericEvent<EventAggregator>>().Subscribe(
                 x =>
                 {
                     if (x.Topic == EventTopicNames.ActivateTicketView || x.Topic == EventTopicNames.DisplayTicketView)
                     {
                         DisplayCategoriesScreen();
                     }
                     else if (x.Topic == EventTopicNames.DisplayTicketOrderDetails)
                     {
                         DisplayTicketDetailsScreen();
                     }
                 });

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
            if (_applicationState.CurrentTicket != null)
                _ticketService.CloseTicket(_applicationState.CurrentTicket);
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

        private void OnTicketViewModelEvent(EventParameters<TicketViewModel> obj)
        {
            if (obj.Topic == EventTopicNames.SelectedOrdersChanged)
            {
                if (_selectedOrdersViewModel.ShouldDisplay(obj.Value.Model, obj.Value.SelectedOrders.Select(x => x.Model)))
                    DisplayTicketDetailsScreen();
                else DisplayCategoriesScreen();
            }

            //if (obj.Topic == EventTopicNames.SelectExtraProperty
            //    || obj.Topic == EventTopicNames.SelectTicketTag
            //    || obj.Topic == EventTopicNames.SelectOrderTag
            //    || obj.Topic == EventTopicNames.EditTicketNote)
            //{
            //    DisplayTicketDetailsScreen();
            //}
        }

        private void OnTicketEvent(EventParameters<Ticket> obj)
        {
            switch (obj.Topic)
            {
                case EventTopicNames.PaymentSubmitted:
                    DisplayCategoriesScreen();
                    break;
            }
        }

        private void DisplayCategoriesScreen()
        {
            DisplayOrdersScreen();
        }

        private bool _handleText;

        public void DisplayOrdersScreen()
        {
            _handleText = true;
            _regionManager.RequestNavigate(RegionNames.MainRegion, new Uri("TicketEditorView", UriKind.Relative));
            _regionManager.RequestNavigate(RegionNames.TicketSubRegion, new Uri("MenuItemSelectorView", UriKind.Relative));
        }

        public void DisplayTicketDetailsScreen()
        {
            _handleText = false;
            _regionManager.RequestNavigate(RegionNames.MainRegion, new Uri("TicketEditorView", UriKind.Relative));
            _regionManager.RequestNavigate(RegionNames.TicketSubRegion, new Uri("SelectedOrdersView", UriKind.Relative));
        }

        public void DisplayTicketExplorerScreen()
        {
            _handleText = true;
            _regionManager.RequestNavigate(RegionNames.MainRegion, new Uri("TicketEditorView", UriKind.Relative));
            _regionManager.RequestNavigate(RegionNames.TicketSubRegion, new Uri("TicketExplorerView", UriKind.Relative));

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
            return _handleText && _menuItemSelectorViewModel.HandleTextInput(text);
        }
    }
}
