using System;
using Samba.Domain.Models.Tickets;

namespace Samba.Modules.PosModule
{
    public class TicketTagButton
    {
        private readonly Ticket _ticket;
        public TicketTagButton(TicketTagGroup ticketTagGroup, Ticket ticket)
        {
            Model = ticketTagGroup;
            Caption = Model.Name.Replace(" ", Environment.NewLine);
            _ticket = ticket;
        }

        public TicketTagGroup Model { get; set; }
        public string Caption { get; set; }

        public string ButtonColor
        {
            get
            {
                if (_ticket != null)
                    return !string.IsNullOrEmpty(_ticket.GetTagValue(Model.Name))
                        ? Model.ButtonColorWhenTagSelected
                        : Model.ButtonColorWhenNoTagSelected;
                return "Gainsboro";
            }
        }
    }
}
