using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Samba.Domain.Models.Tickets;

namespace Samba.Presentation.ViewModels
{
    public class TicketTagButton
    {
        private readonly TicketViewModel _ticket;
        public TicketTagButton(TicketTagGroup ticketTagGroup, TicketViewModel ticket)
        {
            Model = ticketTagGroup;
            Caption = Model.Name;
            _ticket = ticket;
        }

        public TicketTagGroup Model { get; set; }
        public string Caption { get; set; }

        public string ButtonColor
        {
            get
            {
                if (_ticket != null)
                    return !string.IsNullOrEmpty(_ticket.Model.GetTagValue(Model.Name))
                        ? Model.ButtonColorWhenTagSelected
                        : Model.ButtonColorWhenNoTagSelected;
                return "Gainsboro";
            }
        }
    }
}
