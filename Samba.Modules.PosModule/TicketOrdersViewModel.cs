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

namespace Samba.Modules.PosModule
{
    [Export]
    public class TicketOrdersViewModel : ObservableObject
    {
        private readonly ITicketService _ticketService;

        [ImportingConstructor]
        public TicketOrdersViewModel(ITicketService ticketService)
        {
            _ticketService = ticketService;
            _orders = new ObservableCollection<OrderViewModel>();
        }

        public IEnumerable<OrderViewModel> SelectedOrders { get { return Orders.Where(x => x.Selected); } }
        public IList<Order> SelectedOrderModels { get { return SelectedOrders.Select(x => x.Model).ToList(); } }

        private ObservableCollection<OrderViewModel> _orders;
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
                _orders = new ObservableCollection<OrderViewModel>(_selectedTicket.Orders.Select(x => new OrderViewModel(x)).OrderBy(x => x.Model.CreatedDateTime).ThenBy(x => x.OrderNumber).ThenBy(x => x.OrderKey).ThenBy(x => x.Model.Id));
                _itemsViewSource = new CollectionViewSource { Source = _orders };
                _itemsViewSource.GroupDescriptions.Add(new PropertyGroupDescription("GroupObject"));
                RaisePropertyChanged(() => SelectedTicket);
                RaisePropertyChanged(() => ItemsViewSource);
                Refresh();
            }
        }

        public void CancelSelectedOrders()
        {
            var selectedOrders = SelectedOrderModels;
            ClearSelectedOrders();
            SelectedTicket.CancelOrders(selectedOrders);
            Orders.Clear();
            Orders.AddRange(SelectedTicket.Orders.Select(x => new OrderViewModel(x)));
        }

        public void ClearSelectedOrders()
        {
            if (Orders.Any(x => x.Selected))
            {
                foreach (var item in Orders.Where(x => x.Selected))
                    item.NotSelected();
                var so = new SelectedOrdersData { SelectedOrders = SelectedOrderModels, Ticket = SelectedTicket };
                so.PublishEvent(EventTopicNames.SelectedOrdersChanged);
            }
        }

        public bool CanCancelSelectedOrders()
        {
            return SelectedTicket.CanCancelSelectedOrders(SelectedOrderModels);
        }

        public void FixSelectedItems()
        {
            var selectedItems = SelectedOrderModels.Where(x => x.SelectedQuantity > 0 && x.SelectedQuantity < x.Quantity).ToList();
            if (selectedItems.Count > 0)
            {
                var newItems = _ticketService.ExtractSelectedOrders(SelectedTicket, selectedItems);
                _orders.ToList().ForEach(x => x.NotSelected());
                foreach (var newItem in newItems)
                {
                    _orders.Add(new OrderViewModel(newItem) { Selected = true });
                }
            }
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
            var result = new OrderViewModel(ti);
            Orders.Add(result);
            return result;
        }

        public OrderViewModel AddOrder(int menuItemId, decimal quantity, string portionName, OrderTagTemplate template)
        {
            ClearSelectedOrders();
            var order = _ticketService.AddOrder(SelectedTicket, menuItemId, quantity, portionName, template);
            return order == null ? null : Add(order);
        }

        public void AddOrder(ScreenMenuItemData data)
        {
            var ti = AddOrder(data.ScreenMenuItem.MenuItemId, data.Quantity, data.ScreenMenuItem.ItemPortion, data.ScreenMenuItem.OrderTagTemplate);

            if (data.ScreenMenuItem.AutoSelect && ti != null)
            {
                ti.ItemSelectedCommand.Execute(ti);
            }
        }
    }
}
