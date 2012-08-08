using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Samba.Domain.Models.Tickets;
using Samba.Presentation.Common;

namespace Samba.Modules.ModifierModule
{
    public class SelectedOrderStateGroupViewModel : ObservableObject
    {
        private readonly IEnumerable<Order> _selectedOrders;

        public SelectedOrderStateGroupViewModel(OrderStateGroup model, IEnumerable<Order> selectedOrders)
        {
            Model = model;
            _selectedOrders = selectedOrders;
        }

        private ObservableCollection<OrderStateButtonViewModel> _orderStates;
        public ObservableCollection<OrderStateButtonViewModel> OrderStates { get { return _orderStates ?? (_orderStates = new ObservableCollection<OrderStateButtonViewModel>(GetOrderStates(_selectedOrders, Model))); } }

        public int ButtonHeight { get { return Model.ButtonHeight; } set { Model.ButtonHeight = value; } }
        public int ColumnCount { get { return Model.ColumnCount; } set { Model.ColumnCount = value; } }

        public OrderStateGroup Model { get; private set; }
        public string Name { get { return Model.Name; } }

        private static IEnumerable<OrderStateButtonViewModel> GetOrderStates(IEnumerable<Order> selectedOrders, OrderStateGroup baseModel)
        {
            return baseModel.OrderStates.Select(item => new OrderStateButtonViewModel(selectedOrders, baseModel, item));
        }

        public void Refresh()
        {
            foreach (var orderStateButtonViewModel in OrderStates)
            {
                orderStateButtonViewModel.Refresh();
            }
        }
    }
}
