using Samba.Infrastructure.Data;

namespace Samba.Domain.Models.Tickets
{
    public class ResourceTypeAssignment : Value, IOrderable
    {
        public int ResourceTypeId { get; set; }
        public string ResourceTypeName { get; set; }

        public bool AskBeforeCreatingTicket { get; set; }
        public string State { get; set; }

        public string Name
        {
            get { return ResourceTypeName; }
        }

        public int SortOrder { get; set; }

        public string UserString
        {
            get { return Name; }
        }
    }
}