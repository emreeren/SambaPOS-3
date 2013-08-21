using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using Samba.Domain.Models.Tickets;
using Samba.Localization.Properties;
using Samba.Presentation.Common;

namespace Samba.Modules.PosModule
{
    [Export]
    public class TicketInfoViewModel : ObservableObject
    {
        public TicketInfoViewModel()
        {
            SelectedTicket = Ticket.Empty;
        }

        private Ticket _selectedTicket;
        public Ticket SelectedTicket
        {
            get { return _selectedTicket; }
            set
            {
                _selectedTicket = value;
                //Refresh();
            }
        }

        public bool IsTicketTagged { get { return SelectedTicket.IsTagged; } }
        public string TicketTagDisplay { get { return SelectedTicket.GetTagData().Split('\r').Select(x => !string.IsNullOrEmpty(x) && x.Contains(":") && x.Split(':')[0].Trim() == x.Split(':')[1].Trim() ? x.Split(':')[0] : x).Aggregate("", (c, v) => c + v + "\r").Trim('\r'); } }
        public bool IsTicketNoteVisible { get { return !string.IsNullOrEmpty(Note); } }
        public string Note { get { return SelectedTicket.Note; } }
        public bool IsTicketTimeVisible { get { return SelectedTicket.Id != 0; } }
        public bool IsLastPaymentDateVisible { get { return SelectedTicket.Payments.Count > 0; } }

        public bool IsLastOrderDateVisible
        {
            get
            {
                return SelectedTicket.Orders.Count > 1 && SelectedTicket.Orders[SelectedTicket.Orders.Count - 1].OrderNumber != 0 &&
                    SelectedTicket.Orders[0].OrderNumber != SelectedTicket.Orders[SelectedTicket.Orders.Count - 1].OrderNumber;
            }
        }

        public string TicketCreationDate
        {
            get
            {
                if (SelectedTicket.IsClosed) return SelectedTicket.Date.ToString();
                var time = SelectedTicket.GetTicketCreationMinuteStr();

                return !string.IsNullOrEmpty(time)
                    ? string.Format(Resources.TicketTimeDisplay_f, SelectedTicket.Date.ToShortTimeString(), time)
                    : SelectedTicket.Date.ToShortTimeString();
            }
        }

        public string TicketLastOrderDate
        {
            get
            {
                if (SelectedTicket.IsClosed) return SelectedTicket.LastOrderDate.ToString();
                var time = SelectedTicket.GetTicketLastOrderMinuteStr();

                return !string.IsNullOrEmpty(time)
                    ? string.Format(Resources.TicketTimeDisplay_f, SelectedTicket.LastOrderDate.ToShortTimeString(), time)
                    : SelectedTicket.LastOrderDate.ToShortTimeString();
            }
        }

        public string TicketLastPaymentDate
        {
            get
            {
                if (!SelectedTicket.IsClosed) return SelectedTicket.LastPaymentDate != SelectedTicket.Date ? SelectedTicket.LastPaymentDate.ToShortTimeString() : "-";
                var time = new TimeSpan(SelectedTicket.LastPaymentDate.Ticks - SelectedTicket.Date.Ticks).TotalMinutes.ToString("#");
                return !string.IsNullOrEmpty(time)
                    ? string.Format(Resources.TicketTimeDisplay_f, SelectedTicket.LastPaymentDate, time)
                    : SelectedTicket.LastPaymentDate.ToString();
            }
        }

        public void Refresh()
        {
            RaisePropertyChanged(() => Note);
            RaisePropertyChanged(() => IsTicketNoteVisible);
            RaisePropertyChanged(() => IsTicketTagged);
            RaisePropertyChanged(() => TicketTagDisplay);
            RaisePropertyChanged(() => IsTicketTimeVisible);
            RaisePropertyChanged(() => IsLastPaymentDateVisible);
            RaisePropertyChanged(() => IsLastOrderDateVisible);
            RaisePropertyChanged(() => TicketCreationDate);
            RaisePropertyChanged(() => TicketLastOrderDate);
            RaisePropertyChanged(() => TicketLastPaymentDate);
        }
    }

}
