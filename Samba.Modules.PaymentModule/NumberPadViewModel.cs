using System;
using System.ComponentModel.Composition;
using System.Globalization;
using Microsoft.Practices.Prism.Commands;
using Microsoft.Practices.Prism.Events;
using Samba.Infrastructure.Helpers;
using Samba.Infrastructure.Settings;
using Samba.Localization.Properties;
using Samba.Presentation.Common;
using Samba.Presentation.Common.Commands;
using Samba.Presentation.Services.Common;
using Samba.Presentation.ViewModels;
using Samba.Services;

namespace Samba.Modules.PaymentModule
{
    [Export]
    public class NumberPadViewModel : ObservableObject
    {
        private readonly ISettingService _settingService;
        private readonly PaymentEditor _paymentEditor;
        private readonly TenderedValueViewModel _tenderedValueViewModel;
        private readonly OrderSelectorViewModel _orderSelectorViewModel;
        private readonly AccountBalances _accountBalances;
        private readonly ForeignCurrencyButtonsViewModel _foreignCurrencyButtonsViewModel;
        private readonly TicketTotalsViewModel _paymentTotals;

        [ImportingConstructor]
        public NumberPadViewModel(ISettingService settingService, PaymentEditor paymentEditor, TenderedValueViewModel tenderedValueViewModel,
            OrderSelectorViewModel orderSelectorViewModel, AccountBalances accountBalances,
            ForeignCurrencyButtonsViewModel foreignCurrencyButtonsViewModel, TicketTotalsViewModel paymentTotals)
        {
            _settingService = settingService;
            _paymentEditor = paymentEditor;
            _tenderedValueViewModel = tenderedValueViewModel;
            _orderSelectorViewModel = orderSelectorViewModel;
            _accountBalances = accountBalances;
            _foreignCurrencyButtonsViewModel = foreignCurrencyButtonsViewModel;
            _paymentTotals = paymentTotals;

            TenderAllCommand = new CaptionCommand<string>(Resources.All, OnTenderAllCommand);
            ChangeBalanceModeCommand = new DelegateCommand<string>(OnChangeBalanceMode);
            TypeValueCommand = new DelegateCommand<string>(OnTypeValueExecuted);
            SetValueCommand = new DelegateCommand<string>(OnSetValue);
            DivideValueCommand = new DelegateCommand<string>(OnDivideValue);

            EventServiceFactory.EventService.GetEvent<GenericEvent<EventAggregator>>().Subscribe(OnEventGenerated);
        }

        public string DecimalSeparator { get { return CultureInfo.CurrentCulture.NumberFormat.CurrencyDecimalSeparator; } }

        private void OnEventGenerated(EventParameters<EventAggregator> obj)
        {
            if (obj.Topic == EventTopicNames.ResetCache)
            {
                _paymentScreenValues = null;
                RaisePropertyChanged(() => PaymentScreenValues);
            }
        }

        public event EventHandler TypedValueChanged;
        private void OnTypedValueChanged()
        {
            EventHandler handler = TypedValueChanged;
            if (handler != null) handler(this, EventArgs.Empty);
        }

        public bool ResetAmount { get; set; }

        private string _lastTenderedAmount;
        public string LastTenderedAmount
        {
            get { return _lastTenderedAmount; }
            set { _lastTenderedAmount = value; RaisePropertyChanged(() => LastTenderedAmount); }
        }

        private bool _balanceMode;
        public bool BalanceMode
        {
            get { return _balanceMode; }
            set
            {
                _balanceMode = value;
                _paymentEditor.AccountMode = value;
                RaisePropertyChanged(() => BalanceModeCaption);
            }
        }

        public string BalanceModeCaption { get { return BalanceMode ? Resources.Balance : Resources.Ticket; } }

        private string[] _paymentScreenValues;
        public string[] PaymentScreenValues
        {
            get { return _paymentScreenValues ?? (_paymentScreenValues = GetPaymentScreenValues()); }
            set { _paymentScreenValues = value; }
        }

        private string[] GetPaymentScreenValues()
        {
            var result = _settingService.ProgramSettings.PaymentScreenValues;
            if (string.IsNullOrEmpty(result)) result = "1,5,10,20,50,100";
            return result.Split(result.Contains(";") ? ';' : ',');
        }

        public DelegateCommand<string> ChangeBalanceModeCommand { get; set; }
        public CaptionCommand<string> TenderAllCommand { get; set; }
        public DelegateCommand<string> TypeValueCommand { get; set; }
        public DelegateCommand<string> SetValueCommand { get; set; }
        public DelegateCommand<string> DivideValueCommand { get; set; }

        private void OnChangeBalanceMode(string obj)
        {
            BalanceMode = (!BalanceMode || _paymentEditor.SelectedTicket.GetRemainingAmount() == 0) && _accountBalances.GetActiveAccountBalance() > 0;
            if (BalanceMode)
                TenderAllBalance();
            else
            {
                _tenderedValueViewModel.UpdatePaymentAmount(0);
                TenderAll();
            }
        }

        private void OnTenderAllCommand(string obj)
        {
            TenderAll();
        }

        private void TenderAllBalance()
        {
            ResetValues();
            _orderSelectorViewModel.ClearSelection();
            var paymentDue = _paymentEditor.GetRemainingAmount() / _paymentEditor.ExchangeRate;
            _tenderedValueViewModel.PaymentDueAmount = paymentDue.ToString(LocalSettings.ReportCurrencyFormat);
            _tenderedValueViewModel.TenderedAmount = _tenderedValueViewModel.PaymentDueAmount;
            _foreignCurrencyButtonsViewModel.UpdateCurrencyButtons();
            ResetAmount = true;
        }

        private void TenderAll()
        {
            if (_tenderedValueViewModel.GetTenderedValue() > _paymentEditor.GetRemainingAmount())
                _tenderedValueViewModel.UpdatePaymentAmount(0);

            _tenderedValueViewModel.TenderedAmount = _tenderedValueViewModel.PaymentDueAmount;
            ResetAmount = true;
            OnTypedValueChanged();
        }

        private void OnDivideValue(string obj)
        {
            var tenderedValue = _tenderedValueViewModel.GetTenderedValue();
            ResetAmount = true;
            var value = ConvertToDecimal(obj);
            var remainingTicketAmount = _tenderedValueViewModel.GetPaymentDueValue() / _paymentEditor.ExchangeRate;

            if (value > 0)
            {
                var amount = remainingTicketAmount / value;
                if (amount > remainingTicketAmount) amount = remainingTicketAmount;
                _tenderedValueViewModel.TenderedAmount = amount.ToString(LocalSettings.ReportCurrencyFormat);
            }
            else
            {
                value = tenderedValue;
                if (value > 0)
                {
                    var amount = remainingTicketAmount / value;
                    if (amount > remainingTicketAmount) amount = remainingTicketAmount;
                    _tenderedValueViewModel.TenderedAmount = (amount).ToString(LocalSettings.ReportCurrencyFormat);
                }
            }
            OnTypedValueChanged();
        }

        private void OnSetValue(string obj)
        {
            ResetAmount = true;
            if (string.IsNullOrEmpty(obj))
            {
                _orderSelectorViewModel.ClearSelection();
                _tenderedValueViewModel.UpdatePaymentAmount(0);
                ResetValues();
                return;
            }
            var value = ConvertToDecimal(obj);
            if (string.IsNullOrEmpty(_tenderedValueViewModel.TenderedAmount))
                _tenderedValueViewModel.TenderedAmount = "0";
            decimal tenderedValue;
            try
            {
                tenderedValue = Convert.ToDecimal(_tenderedValueViewModel.TenderedAmount.Replace(
                    CultureInfo.CurrentCulture.NumberFormat.NumberGroupSeparator, ""));
            }
            catch (Exception)
            {
                tenderedValue = 0m;
            }
            tenderedValue += value;
            _tenderedValueViewModel.TenderedAmount = tenderedValue.ToString(LocalSettings.ReportCurrencyFormat);
            OnTypedValueChanged();
        }

        private decimal ConvertToDecimal(string s)
        {
            var dc = CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator;
            s = s.Replace(",", dc);
            s = s.Replace(".", dc);
            if (s.StartsWith(dc)) s = "0" + s;
            return Convert.ToDecimal(s);
        }

        private void OnTypeValueExecuted(string obj)
        {
            if (ResetAmount) _tenderedValueViewModel.TenderedAmount = "";
            ResetAmount = false;
            _tenderedValueViewModel.TenderedAmount = Utility.AddTypedValue(_tenderedValueViewModel.TenderedAmount, obj, "#,#0.");
            OnTypedValueChanged();
        }

        public void ResetValues()
        {
            _paymentEditor.UpdateCalculations();

            if (_tenderedValueViewModel.GetPaymentDueValue() <= 0)
                _tenderedValueViewModel.UpdatePaymentAmount(0);

            _paymentTotals.ResetCache();
            _paymentTotals.Refresh();
            _accountBalances.Refresh();
            _tenderedValueViewModel.TenderedAmount = "";
            RaisePropertyChanged(() => BalanceModeCaption);
        }
    }
}
