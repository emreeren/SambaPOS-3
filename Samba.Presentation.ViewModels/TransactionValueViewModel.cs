using System;
using Samba.Domain.Models.Accounts;
using Samba.Presentation.Common;

namespace Samba.Presentation.ViewModels
{
    public class TransactionValueViewModel : ObservableObject
    {
        public AccountTransactionValue Model { get; set; }

        public TransactionValueViewModel(AccountTransactionValue model)
        {
            Model = model;
        }

        public decimal Amount
        {
            get { return Model.Receivable+Model.Liability; }
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
            get { return Model.AccountName; }
        }

        public string AmountDisplay
        {
            get { return Amount.ToString("#,#0.00"); }
        }
    }
}
