using System;
using System.Collections.Generic;
using System.Linq;
using Samba.Domain.Models.Tickets;
using Samba.Localization.Properties;
using Samba.Presentation.Common;

namespace Samba.Modules.ModifierModule
{
    public class OrderStateButtonViewModel : ObservableObject
    {
        public OrderState Model { get; set; }
        public OrderStateGroup OrderStateGroup { get; set; }
        private readonly IEnumerable<Order> _selectedOrders;

        public OrderStateButtonViewModel(IEnumerable<Order> selectedOrders, OrderStateGroup stateGroup, OrderState model)
        {
            _selectedOrders = selectedOrders;
            Model = model;
            OrderStateGroup = stateGroup;
            if (string.IsNullOrEmpty(model.Name))
                model.Name = string.Format("[{0}]", Resources.NewProperty);
        }

        public string Name { get { return Model.Name; } set { Model.Name = value; } }
        public string Color { get { return "Transparent"; } }
        public string DisplayText
        {
            get
            {
                return Name;
            }
        }

        public void Refresh()
        {
            RaisePropertyChanged(() => Color);
            RaisePropertyChanged(() => DisplayText);
        }
    }
}
