using Samba.Domain.Models.Tickets;

namespace Samba.Modules.PosModule
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
