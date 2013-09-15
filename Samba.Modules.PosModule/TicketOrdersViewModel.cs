using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.Composition;
using System.Linq;
using System.Windows.Data;
using Samba.Domain.Models.Tickets;
using Samba.Presentation.Common;
using Samba.Presentation.Services;
using Samba.Presentation.Services.Common;
using Samba.Presentation.ViewModels;
using Samba.Services;
using Samba.Services.Common;

namespace Samba.Modules.PosModule
{
    [Export]
    public class TicketOrdersViewModel : ObservableObject
    {
        private readonly ITicketService _ticketService;
        private readonly IApplicationState _applicationState;
        private readonly ICacheService _cacheService;

        [ImportingConstructor]
        public TicketOrdersViewModel(ITicketService ticketService, IApplicationState applicationState, ICacheService cacheService)
        {
            _ticketService = ticketService;
            _applicationState = applicationState;
            _cacheService = cacheService;
            _orders = new ObservableCollection<OrderViewModel>();
            _itemsViewSource = new CollectionViewSource { Source = _orders };
            _itemsViewSource.GroupDescriptions.Add(new PropertyGroupDescription("GroupObject"));
        }

        public IEnumerable<OrderViewModel> SelectedOrders { get { return Orders.Where(x => x.Selected); } }

        private readonly ObservableCollection<OrderViewModel> _orders;
        public ObservableCollection<OrderViewModel> Orders
        {
            get { return _orders; }
        }

        private CollectionViewSource _itemsViewSource;
        public CollectionViewSource ItemsViewSource
        {
            get { return _itemsViewSource; }
            set { _itemsViewSource = value; }
        }

        public string TicketBackground
        {
            get
            {
                return SelectedTicket != null && (SelectedTicket.IsLocked || SelectedTicket.IsClosed)
                           ? "Transparent"
                           : "White";
            }
        }

        private Ticket _selectedTicket;
        public Ticket SelectedTicket
        {
            get { return _selectedTicket; }
            set
            {
                _selectedTicket = value;
                _orders.Clear();
                _orders.AddRange(_selectedTicket.Orders.Select(x => new OrderViewModel(x, _cacheService, _applicationState)).OrderBy(x => x.Model.CreatedDateTime.Ticks).ThenBy(x => x.OrderNumber).ThenBy(x => x.OrderKey).ThenBy(x => x.Model.Id));
                RaisePropertyChanged(() => SelectedTicket);
                Refresh();
            }
        }

        public void CancelSelectedOrders()
        {
            var selectedOrders = SelectedTicket.SelectedOrders.ToList();
            ClearSelectedOrders();
            SelectedTicket.CancelOrders(selectedOrders);
            Orders.Clear();
            Orders.AddRange(SelectedTicket.Orders.Select(x => new OrderViewModel(x, _cacheService, _applicationState)));
        }

        public void ClearSelectedOrders()
        {
            if (Orders.Any(x => x.Selected))
            {
                foreach (var item in Orders.Where(x => x.Selected)) item.NotSelected();
                EventServiceFactory.EventService.PublishEvent(EventTopicNames.RefreshSelectedTicket);
            }
        }

        public bool CanCancelSelectedOrders()
        {
            return SelectedTicket.CanCancelSelectedOrders(SelectedTicket.SelectedOrders);
        }

        public void Refresh()
        {
            RaisePropertyChanged(() => TicketBackground);
        }

        public void RefreshSelectedOrders()
        {
            SelectedOrders.ToList().ForEach(x => x.RefreshOrder());
        }

        public void UpdateLastSelectedOrder(OrderViewModel lastSelectedOrder)
        {
            foreach (var item in Orders.Where(x => x.Selected))
            {
                item.IsLastSelected = item == lastSelectedOrder;
            }
        }

        private OrderViewModel Add(Order ti)
        {
            var result = new OrderViewModel(ti, _cacheService, _applicationState);
            Orders.Add(result);
            return result;
        }

        public OrderViewModel AddOrder(int menuItemId, decimal quantity, string portionName)
        {
            ClearSelectedOrders();
            var order = _ticketService.AddOrder(SelectedTicket, menuItemId, quantity, portionName, "");
            var result = order == null ? null : Add(order);
            order.PublishEvent(EventTopicNames.OrderAdded);
            return result;
        }

        public void AddOrder(ScreenMenuItemData data)
        {
            var ti = AddOrder(data.ScreenMenuItem.MenuItemId, data.Quantity, data.ScreenMenuItem.ItemPortion);
            if (ti != null)
            {
                UpdateOrderTags(SelectedTicket, ti.Model, data.ScreenMenuItem.OrderTags);
                UpdateOrderStates(SelectedTicket, ti.Model, data.ScreenMenuItem.OrderStates);
                ExecuteAutomationCommand(SelectedTicket, ti.Model, data.ScreenMenuItem.AutomationCommand, data.ScreenMenuItem.AutomationCommandValue);
                ti.UpdateItemColor();
                if (data.ScreenMenuItem.AutoSelect)
                {
                    ti.ToggleSelection();
                    if (!_applicationState.IsLandscape)
                    {
                        var so = new SelectedOrdersData { SelectedOrders = new List<Order> { ti.Model }, Ticket = SelectedTicket };
                        OperationRequest<SelectedOrdersData>.Publish(so, EventTopicNames.DisplayTicketOrderDetails, EventTopicNames.RefreshSelectedTicket, "");
                    }
                }
            }
        }

        private void UpdateOrderTags(Ticket ticket, Order order, string orderTags)
        {
            if (string.IsNullOrEmpty(orderTags)) return;
            foreach (var orderTag in orderTags.Split(','))
            {
                if (orderTag.Contains("="))
                {
                    var parts = orderTag.Split('=');
                    UpdateOrderTag(ticket, order, parts[0], parts[1]);
                }
                else
                {
                    UpdateOrderTag(ticket, order, "", orderTag);
                }
            }
        }

        private void UpdateOrderTag(Ticket ticket, Order order, string orderTagGroup, string orderTag)
        {
            var ot = string.IsNullOrEmpty(orderTagGroup)
                ? _cacheService.GetOrderTagGroupByOrderTagName(orderTag)
                : _cacheService.GetOrderTagGroupByName(orderTagGroup);
            if (ot != null)
            {
                var otn = ot.OrderTags.First(x => x.Name == orderTag);
                _ticketService.TagOrders(ticket, new List<Order> { order }, ot, otn, "");
            }
        }

        private void UpdateOrderStates(Ticket ticket, Order order, string orderStates)
        {
            if (string.IsNullOrEmpty(orderStates)) return;

            orderStates.Split(',')
                           .Where(x => x.Contains('='))
                           .Select(x => x.Split(new[] { '=' }, 2))
                           .ToList()
                           .ForEach(x => UpdateOrderState(ticket, order, x[0], x[1]));
        }

        private void UpdateOrderState(Ticket ticket, Order order, string stateName, string state)
        {
            _ticketService.UpdateOrderStates(ticket, new List<Order> { order }, stateName, null, 0, state, 0, "");
        }

        private void ExecuteAutomationCommand(Ticket selectedTicket, Order order, string automationCommand, string automationCommandValue)
        {
            if (string.IsNullOrEmpty(automationCommand)) return;

            if (string.IsNullOrEmpty(automationCommandValue))
            {
                var ac = _cacheService.GetAutomationCommandByName(automationCommand);
                if (ac != null)
                {
                    if (!string.IsNullOrEmpty(ac.Values))
                    {
                        ac.PublishEvent(EventTopicNames.SelectAutomationCommandValue);
                        return;
                    }
                }
            }

            _applicationState.NotifyEvent(RuleEventNames.AutomationCommandExecuted, new
            {
                Ticket = selectedTicket,
                Order = order,
                AutomationCommandName = automationCommand,
                CommandValue = automationCommandValue ?? ""
            });
        }
    }
}
