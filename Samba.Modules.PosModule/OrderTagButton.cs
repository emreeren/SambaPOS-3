using Samba.Domain.Models.Tickets;

namespace Samba.Modules.PosModule
{
    public class OrderTagButton
    {
        public OrderTagButton(OrderTagGroup orderTagGroup)
        {
            Model = orderTagGroup;
            Name = Model.ButtonHeader;
        }

        public OrderTagGroup Model { get; set; }
        public string Name { get; set; }
    }
}
