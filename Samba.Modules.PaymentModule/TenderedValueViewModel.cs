using System;
using System.ComponentModel.Composition;
using Samba.Presentation.Common;

namespace Samba.Modules.PaymentModule
{
    [Export]
    public class TenderedValueViewModel : ObservableObject
    {
        private readonly PaymentEditor _paymentEditor;

        [ImportingConstructor]
        public TenderedValueViewModel(PaymentEditor paymentEditor)
        {
            _paymentEditor = paymentEditor;
        }

        private string _tenderedAmount;
        public string TenderedAmount
        {
            get { return _tenderedAmount; }
            set
            {
                _tenderedAmount = value;
                RaisePropertyChanged(() => TenderedAmount);
            }
        }

        private string _paymentDueAmount;
        public string PaymentDueAmount
        {
            get { return _paymentDueAmount; }
            set
            {
                if (_paymentDueAmount != value)
                {
                    _paymentDueAmount = value;
                    RaisePropertyChanged(() => PaymentDueAmount);
                    OnPaymentDueChanged();
                }
            }
        }

        public decimal GetTenderedValue()
        {
            decimal result;
            decimal.TryParse(TenderedAmount, out result);
            return decimal.Round(result * _paymentEditor.ExchangeRate, 2);
        }

        public decimal GetPaymentDueValue()
        {
            decimal result;
            decimal.TryParse(PaymentDueAmount, out result);
            return decimal.Round(result * _paymentEditor.ExchangeRate, 2);
        }

        public event EventHandler PaymentDueChanged;
        public void OnPaymentDueChanged()
        {
            var handler = PaymentDueChanged;
            if (handler != null) handler(this, EventArgs.Empty);
        }

        public void UpdatePaymentAmount(decimal value)
        {
            var remaining = _paymentEditor.GetRemainingAmount();
            if (value == 0 || value > remaining) value = remaining;
            value /= _paymentEditor.ExchangeRate;
            PaymentDueAmount = value.ToString("#,#0.00");
        }
    }
}
