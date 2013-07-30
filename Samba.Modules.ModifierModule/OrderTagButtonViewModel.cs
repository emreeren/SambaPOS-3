using System.Collections.Generic;
using System.Linq;
using Samba.Domain.Models.Tickets;
using Samba.Localization.Properties;
using Samba.Presentation.Common;

namespace Samba.Modules.ModifierModule
{
    public class OrderTagButtonViewModel : ObservableObject
    {
        public OrderTag Model { get; set; }
        public OrderTagGroup OrderTagGroup { get; set; }
        private readonly IEnumerable<Order> _selectedOrders;

        public OrderTagButtonViewModel(IEnumerable<Order> selectedOrders, OrderTagGroup tagGroup, OrderTag model)
        {
            _selectedOrders = selectedOrders;
            Model = model;
            OrderTagGroup = tagGroup;
            if (string.IsNullOrEmpty(model.Name))
                model.Name = string.Format("[{0}]", Resources.NewProperty);
        }

        public string Name { get { return Model.Name; } set { Model.Name = value; } }
        public string Color { get { return _selectedOrders != null && _selectedOrders.All(x => x.IsTaggedWith(Model)) ? "Red" : OrderTagGroup.ButtonColor ?? "Gainsboro"; } }
        public string DisplayText
        {
            get
            {
                if (_selectedOrders.Any(x => x.OrderTagExists(y => y.OrderTagGroupId == OrderTagGroup.Id)))
                {
                    var q = _selectedOrders.SelectMany(x => x.GetOrderTagValues()).SingleOrDefault(
                            x => x.OrderTagGroupId == OrderTagGroup.Id && x.TagValue == Name);
                    if (q != null && q.Quantity > 1)
                        return string.Format("{0} x {1}", q.Quantity, Name);
                }
                return Name;
            }
        }
        public int FontSize { get { return OrderTagGroup.FontSize > 0 ? OrderTagGroup.FontSize : 16; } }

        public void Refresh()
        {
            RaisePropertyChanged(() => Color);
            RaisePropertyChanged(() => DisplayText);
        }
    }
}
