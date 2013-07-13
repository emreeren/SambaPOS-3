using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Samba.Domain.Models.Tickets;
using Samba.Presentation.Common;
using Samba.Services.Common;

namespace Samba.Modules.ModifierModule
{
    public class OrderTagGroupViewModel : ObservableObject
    {
        private readonly IEnumerable<Order> _selectedOrders;

        public OrderTagGroupViewModel(OrderTagGroup model, IEnumerable<Order> selectedOrders)
        {
            Model = model;
            _selectedOrders = selectedOrders;
        }

        private ObservableCollection<OrderTagButtonViewModel> _orderTags;
        public ObservableCollection<OrderTagButtonViewModel> OrderTags { get { return _orderTags ?? (_orderTags = new ObservableCollection<OrderTagButtonViewModel>(GetOrderTags(_selectedOrders, Model))); } }

        public int ButtonHeight { get { return Model.ButtonHeight; } set { Model.ButtonHeight = value; } }
        public int ColumnCount { get { return Model.ColumnCount; } set { Model.ColumnCount = value; } }

        public OrderTagGroup Model { get; private set; }
        public string Name { get { return Model.Name; } }
        public bool FreeTagging { get { return Model.FreeTagging; } }

        private string _freeTagName;
        public string FreeTagName
        {
            get { return _freeTagName; }
            set { _freeTagName = value; RaisePropertyChanged(() => FreeTagName); }
        }

        public string FreeTagPriceStr
        {
            get { return FreeTagPrice.ToString(); }
            set { FreeTagPrice = Convert.ToDecimal(value); RaisePropertyChanged(() => FreeTagPriceStr); }
        }

        private decimal _freeTagPrice;
        public decimal FreeTagPrice
        {
            get { return _freeTagPrice; }
            set
            {
                _freeTagPrice = value;
                RaisePropertyChanged(() => FreeTagPrice);
            }
        }

        private static IEnumerable<OrderTagButtonViewModel> GetOrderTags(IEnumerable<Order> selectedOrders, OrderTagGroup baseModel)
        {
            return baseModel.OrderTags.OrderBy(x => x.SortOrder).Select(item => new OrderTagButtonViewModel(selectedOrders, baseModel, item));
        }

        public void Refresh()
        {
            foreach (var orderTagButtonViewModel in OrderTags)
            {
                orderTagButtonViewModel.Refresh();
            }
        }

        public void CreateOrderTagButton(OrderTagData orderTagData)
        {
            if (OrderTags.All(x => x.Name != orderTagData.SelectedOrderTag.Name))
            {
                OrderTags.Add(new OrderTagButtonViewModel(_selectedOrders, orderTagData.OrderTagGroup, orderTagData.SelectedOrderTag));
            }
        }
    }
}
