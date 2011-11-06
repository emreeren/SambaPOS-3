using Samba.Infrastructure.Data;

namespace Samba.Domain.Models.Tickets
{
    public class Reason : IEntity
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public byte[] LastUpdateTime { get; set; }
        public int ReasonType { get; set; }
    }
}
