using System.Linq;
using Samba.Domain.Models.Tickets;
using Samba.Presentation.Common;

namespace Samba.Modules.ModifierModule
{
    public class GroupedOrderTagButtonViewModel : ObservableObject
    {
        public GroupedOrderTagButtonViewModel(Order selectedItem, OrderTagGroup orderTagGroup)
        {
            _selectedItem = selectedItem;
            OrderTagGroup = orderTagGroup;
            UpdateNextTag(null);
        }

        public void UpdateNextTag(OrderTag current)
        {
            CurrentTag = GetCurrentTag(current);
            NextTag = OrderTagGroup.OrderTags.First();

            if (CurrentTag != null)
            {
                var nProp = OrderTagGroup.OrderTags.SkipWhile(x => x.Name != CurrentTag.Name).Skip(1).FirstOrDefault();
                if (nProp != null) NextTag = nProp;
            }

            Name = CurrentTag != null ? CurrentTag.Name : OrderTagGroup.Name;
        }

        private OrderTag GetCurrentTag(OrderTag current)
        {
            if (current != null) return current;
            var selected = _selectedItem.GetOrderTagValues().FirstOrDefault(x => x.OrderTagGroupId == OrderTagGroup.Id);
            if (selected == null) return null;
            return OrderTagGroup.OrderTags.SingleOrDefault(x => x.Name == selected.TagValue);
        }

        public OrderTagGroup OrderTagGroup { get; set; }
        public OrderTag NextTag { get; set; }
        public OrderTag CurrentTag { get; set; }

        private OrderTagValue _orderTagValue;
        public OrderTagValue OrderTagValue
        {
            get { return _orderTagValue; }
            set
            {
                _orderTagValue = value;
                RaisePropertyChanged(() => OrderTagValue);
            }
        }

        private readonly Order _selectedItem;

        private string _name;
        public string Name
        {
            get { return _name; }
            set
            {
                _name = value;
                RaisePropertyChanged(() => Name);
            }
        }

        public int FontSize { get { return OrderTagGroup.FontSize > 0 ? OrderTagGroup.FontSize : 16; } }
        public string Color { get { return OrderTagGroup.ButtonColor ?? "Gainsboro"; } }

    }
}