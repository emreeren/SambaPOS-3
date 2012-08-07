using System.Collections.Generic;
using Samba.Domain.Models.Menus;
using Samba.Domain.Models.Tickets;
using Samba.Localization.Properties;
using Samba.Presentation.Common;
using Samba.Services;

namespace Samba.Modules.TicketModule
{
    public class OrderStateViewModel : ObservableObject
    {
        public OrderState Model { get; set; }

        public OrderStateViewModel(OrderState model)
        {
            Model = model;
            if (string.IsNullOrEmpty(model.Name))
                model.Name = string.Format("[{0}]", Resources.NewProperty);
        }

        public string Name { get { return Model.Name; } set { Model.Name = value; } }
    }
}
