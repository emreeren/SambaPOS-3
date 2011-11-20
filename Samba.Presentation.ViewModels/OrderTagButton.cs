using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Samba.Domain.Models.Tickets;

namespace Samba.Presentation.ViewModels
{
    public class OrderTagButton
    {
        public OrderTagButton(OrderTagGroup ticketTagGroup)
        {
            Model = ticketTagGroup;
            Caption = Model.Name;
        }

        public OrderTagGroup Model { get; set; }
        public string Caption { get; set; }
    }
}
