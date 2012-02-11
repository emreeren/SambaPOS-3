using System;
using Samba.Domain.Models.Tickets;
using Samba.Presentation.Common;

namespace Samba.Presentation.ViewModels
{
    public class PaymentViewModel : ObservableObject
    {
        public Payment Model { get; set; }

        public PaymentViewModel(Payment model)
        {
            Model = model;
        }

        public decimal Amount
        {
            get { return Model.Amount; }
        }

        public DateTime Date
        {
            get { return Model.Date; }
            set { Model.Date = value; }
        }

        public string DateDisplay
        {
            get
            {
                return Date.ToString("ddMMyyyy") == DateTime.Now.ToString("ddMMyyyy")
                           ? Date.ToShortTimeString()
                           : Date.ToShortDateString() + " " + Date.ToShortTimeString();
            }
        }

        public string PaymentTypeDisplay
        {
            get { return Model.Name; }
        }

        public string AmountDisplay
        {
            get { return Amount.ToString("#,#0.00"); }
        }
    }
}
