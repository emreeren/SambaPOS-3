using Samba.Infrastructure.Data;

namespace Samba.Domain.Models.Resources
{
    public class ResourceState : Entity
    {
        public int ResourceTempletId { get; set; }
        public string Color { get; set; }
    }
}
