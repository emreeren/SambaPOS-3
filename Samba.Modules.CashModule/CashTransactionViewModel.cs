using System;
using Samba.Domain;
using Samba.Domain.Models.Transactions;
using Samba.Presentation.Common;

namespace Samba.Modules.CashModule
{
    public interface ICashTransactionViewModel
    {
        PaymentType PaymentType { get; set; }
        decimal Amount { get; }
        string Description { get; set; }
        string DateString { get; }
        decimal CashPaymentValue { get; set; }
        decimal CreditCardPaymentValue { get; set; }
        decimal TicketPaymentValue { get; set; }
        bool IsSelected { get; set; }
        string TextColor { get; }
    }

    public abstract class CashTransactionViewModelBase : ObservableObject
    {
        public string TextColor { get {return GetTextColor(); } }
        internal abstract string GetTextColor();

        private bool _isSelected;
        public bool IsSelected
        {
            get { return _isSelected; }
            set
            {
                if (_isSelected != value)
                {
                    _isSelected = value;
                    RaisePropertyChanged(() => TextColor);
                }
            }
        }
    }

    public class CashOperationViewModel : CashTransactionViewModelBase, ICashTransactionViewModel
    {
        public PaymentType PaymentType { get; set; }
        public decimal Amount { get { return 0; } }
        public string Description { get; set; }
        public DateTime Date { get; set; }
        public string DateString { get { return DateTime.Now.Day == Date.Day ? Date.ToShortTimeString() : Date.ToShortDateString(); } }
        public decimal CashPaymentValue { get; set; }
        public decimal CreditCardPaymentValue { get; set; }
        public decimal TicketPaymentValue { get; set; }

        internal override string GetTextColor()
        {
            return IsSelected ? "White" : "Black"; 
        }
    }

    public class CashTransactionViewModel : CashTransactionViewModelBase, ICashTransactionViewModel
    {
        public CashTransactionViewModel(CashTransaction transaction)
        {
            Model = transaction;
        }

        public CashTransaction Model { get; set; }
        public TransactionType TransactionType { get { return (TransactionType)Model.TransactionType; } set { Model.TransactionType = (int)value; } }
        public PaymentType PaymentType { get { return (PaymentType)Model.PaymentType; } set { Model.PaymentType = (int)value; } }
        public string Description { get { return Model.Name; } set { Model.Name = value; } }
        public decimal Amount { get { return Model.Amount; } set { Model.Amount = value; } }
        public DateTime Date { get { return Model.Date; } set { Model.Date = value; } }
        public string DateString { get { return DateTime.Now.Day == Date.Day ? Date.ToShortTimeString() : Date.ToShortDateString(); } }
        public decimal CashPaymentValue { get { return PaymentType == PaymentType.Cash ? Model.Amount : 0; } set { } }
        public decimal CreditCardPaymentValue { get { return PaymentType == PaymentType.CreditCard ? Model.Amount : 0; } set { } }
        public decimal TicketPaymentValue { get { return PaymentType == PaymentType.Ticket ? Model.Amount : 0; } set { } }

        internal override string GetTextColor()
        {
            if (IsSelected) return "White"; return Amount < 0 ? "Red" : "Black"; 
        }
    }
}
