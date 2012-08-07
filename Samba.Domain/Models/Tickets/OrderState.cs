using System;
using Samba.Infrastructure.Data;

namespace Samba.Domain.Models.Tickets
{
    public class OrderState : Value, IOrderable
    {
        public string Name { get; set; }
        public int OrderStateGroupId { get; set; }
        public int Order { get; set; }
        public string UserString { get { return Name; } }
    }
}
