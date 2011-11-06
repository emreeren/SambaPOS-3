using System;
using Samba.Domain;
using Samba.Domain.Models.Tickets;
using Samba.Localization.Properties;
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
            set { Model.Amount = value; RaisePropertyChanged(() => Amount); }
        }

        public PaymentType PaymentType
        {
            get { return (PaymentType)Model.PaymentType; }
            set { Model.PaymentType = (int)value; }
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
            get
            {
                switch (PaymentType)
                {
                    case PaymentType.Ticket: return Resources.Voucher;
                    case PaymentType.Cash: return Resources.Cash;
                    case PaymentType.CreditCard: return Resources.CreditCard;
                    case PaymentType.Account: return Resources.AccountBalance;
                }
                return Resources.UndefinedPaymentType;
            }
        }

        public string AmountDisplay
        {
            get { return Amount.ToString("#,#0.00"); }
        }
    }
}
