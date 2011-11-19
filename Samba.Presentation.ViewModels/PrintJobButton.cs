using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Input;
using Samba.Domain.Models.Settings;
using Samba.Domain.Models.Tickets;

namespace Samba.Presentation.ViewModels
{
    public class PrintJobButton
    {
        public PrintJobButton(PrintJob model, Ticket ticket)
        {
            Model = model;
            Ticket = ticket;
        }

        public PrintJob Model { get; set; }
        public string Caption { get { return GetCaption(); } }
        public Ticket Ticket { get; set; }

        public string GetCaption()
        {
            var c = Model.ButtonHeader ?? Model.Name;
            var i = Ticket.GetPrintCount(Model.Id);
            return i > 0 ? string.Format("{0}-{1}", c, i) : c;
        }
    }
}
