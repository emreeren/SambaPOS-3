using Samba.Domain.Models.Tickets;

namespace Samba.Services.Common
{
    public class TicketTagData
    {
        public string TagName { get; set; }

        private string _tagValue;
        public string TagValue
        {
            get { return _tagValue ?? string.Empty; }
            set { _tagValue = value; }
        }

        public TicketTagGroup TicketTagGroup { get; set; }
        public Ticket Ticket { get; set; }
    }
}