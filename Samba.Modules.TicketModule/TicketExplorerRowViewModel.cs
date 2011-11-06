using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Samba.Domain.Models.Tickets;

namespace Samba.Modules.TicketModule
{
    public class TicketExplorerRowViewModel
    {
        public TicketExplorerRowViewModel(Ticket model)
        {
            Model = model;
        }
        public Ticket Model { get; set; }

        public int Id { get { return Model.Id; } }
        public string TicketNumber { get { return Model.TicketNumber; } }
        public string Location { get { return Model.LocationName; } }
        public string Date { get { return Model.Date.ToShortDateString(); } }
        public string AccountName { get { return Model.AccountName; } }
        public string CreationTime { get { return Model.Date.ToShortTimeString(); } }
        public string LastPaymentTime { get { return Model.LastPaymentDate.ToShortTimeString(); } }
        public decimal Sum { get { return Model.TotalAmount; } }
        public bool IsPaid { get { return Model.IsPaid; } }
        public string TimeInfo { get { return CreationTime != LastPaymentTime || IsPaid ? CreationTime + " - " + LastPaymentTime : CreationTime; } }
    }
}
