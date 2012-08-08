using Samba.Domain.Models.Tickets;

namespace Samba.Modules.PosModule
{
    public class OrderStateButton
    {
        public OrderStateButton(OrderStateGroup orderStateGroup)
        {
            Model = orderStateGroup;
            Name = Model.Name;
        }

        public OrderStateGroup Model { get; set; }
        public string Name { get; set; }
    }
}
