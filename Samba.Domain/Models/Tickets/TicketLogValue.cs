using System;
using System.Runtime.Serialization;

namespace Samba.Domain.Models.Tickets
{
    [DataContract]
    public class TicketLogValue
    {
        public TicketLogValue()
        {
            
        }

        public TicketLogValue(string ticketNo, string userName)
        {
            DateTime = DateTime.Now;
            TicketNo = ticketNo;
            UserName = userName;
        }

        [DataMember(Name = "N")]
        public string TicketNo { get; set; }
        [DataMember(Name = "D")]
        public DateTime DateTime { get; set; }
        [DataMember(Name = "U")]
        public string UserName { get; set; }
        [DataMember(Name = "C")]
        public string Category { get; set; }
        [DataMember(Name = "L")]
        public string Log { get; set; }
    }
}