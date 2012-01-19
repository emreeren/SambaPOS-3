using Samba.Domain.Models.Tickets;

namespace Samba.Modules.PosModule
{
    public class OrderTagGroupButton
    {
        public OrderTagGroupButton(OrderTagGroup orderTagGroup)
        {
            Model = orderTagGroup;
            Name = Model.Name;
        }

        public OrderTagGroup Model { get; set; }
        public string Name { get; set; }
    }
}
