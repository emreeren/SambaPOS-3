using Samba.Infrastructure.Data;

namespace Samba.Domain.Models.Menus
{
    public class ServiceTemplate : IEntity, IOrderable
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int Order { get; set; }
        public string UserString { get { return Name; } }
        public int CalculationMethod { get; set; }
        public decimal Amount { get; set; }
    }
}
