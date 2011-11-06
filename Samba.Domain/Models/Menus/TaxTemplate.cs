using Samba.Infrastructure.Data;

namespace Samba.Domain.Models.Menus
{
    public class TaxTemplate : IEntity
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public decimal Rate { get; set; }
        public bool TaxIncluded { get; set; }
    }
}
