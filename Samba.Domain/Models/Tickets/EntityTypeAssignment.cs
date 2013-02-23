using Samba.Infrastructure.Data;

namespace Samba.Domain.Models.Tickets
{
    public class EntityTypeAssignment : ValueClass, IOrderable
    {
        public int EntityTypeId { get; set; }
        public string EntityTypeName { get; set; }

        public bool AskBeforeCreatingTicket { get; set; }
        public string State { get; set; }

        public string Name
        {
            get { return EntityTypeName; }
        }

        public int SortOrder { get; set; }

        public string UserString
        {
            get { return Name; }
        }
    }
}