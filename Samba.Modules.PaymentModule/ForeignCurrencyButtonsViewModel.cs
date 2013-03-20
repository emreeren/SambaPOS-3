using System.Collections.ObjectModel;
using System.ComponentModel.Composition;
using System.Linq;
using Samba.Domain.Models.Settings;
using Samba.Presentation.Common;
using Samba.Presentation.Common.Commands;
using Samba.Presentation.ViewModels;
using Samba.Services;

namespace Samba.Modules.PaymentModule
{
    [Export]
    public class ForeignCurrencyButtonsViewModel : ObservableObject
    {
        private readonly ICaptionCommand _foreignCurrencySelectedCommand;
        private readonly PaymentEditor _paymentEditor;
        private readonly OrderSelectorViewModel _orderSelectorViewModel;
        private readonly ICacheService _cacheService;
        private readonly PaymentButtonsViewModel _paymentButtonsViewModel;
        private readonly ISettingService _settingService;
        private readonly TenderedValueViewModel _tenderedValueViewModel;

        [ImportingConstructor]
        public ForeignCurrencyButtonsViewModel(PaymentEditor paymentEditor, OrderSelectorViewModel orderSelectorViewModel,
            ICacheService cacheService, PaymentButtonsViewModel paymentButtonsViewModel, ISettingService settingService, TenderedValueViewModel tenderedValueViewModel)
        {
            _paymentEditor = paymentEditor;
            _orderSelectorViewModel = orderSelectorViewModel;
            _cacheService = cacheService;
            _paymentButtonsViewModel = paymentButtonsViewModel;
            _settingService = settingService;
            _tenderedValueViewModel = tenderedValueViewModel;
            _tenderedValueViewModel.PaymentDueChanged += TenderedValueViewModelPaymentDueChanged;
            _foreignCurrencySelectedCommand = new CaptionCommand<ForeignCurrency>("", OnForeignCurrencySelected);
            ForeignCurrencyButtons = new ObservableCollection<CommandButtonViewModel<ForeignCurrency>>();
        }

        private void RefreshForeignCurrencyButtons()
        {
            ForeignCurrencyButtons.Clear();

            if (_cacheService.GetForeignCurrencies().Count() == 1 &&
                _cacheService.GetForeignCurrencies().First().ExchangeRate == 1)
            {
                ForeignCurrency = _cacheService.GetForeignCurrencies().First();
            }
            else
            {
                ForeignCurrencyButtons.AddRange(
                    _cacheService.GetForeignCurrencies()
                                 .Select(
                                     x =>
                                     new CommandButtonViewModel<ForeignCurrency>
                                         {
                                             Caption = x.CurrencySymbol,
                                             Command = _foreignCurrencySelectedCommand,
                                             Parameter = x
                                         }));
            }
        }

        void TenderedValueViewModelPaymentDueChanged(object sender, System.EventArgs e)
        {
            UpdateCurrencyButtons();
        }

        public ObservableCollection<CommandButtonViewModel<ForeignCurrency>> ForeignCurrencyButtons { get; set; }

        private ForeignCurrency _foreignCurrency;
        public ForeignCurrency ForeignCurrency
        {
            get { return _foreignCurrency; }
            private set
            {
                if (_foreignCurrency != value)
                {
                    _foreignCurrency = value;
                    _paymentEditor.ExchangeRate = value != null ? value.ExchangeRate : 1;
                }
                else
                {
                    _foreignCurrency = null;
                    _paymentEditor.ExchangeRate = 1;
                }
                _orderSelectorViewModel.UpdateAutoRoundValue(ForeignCurrency != null ? ForeignCurrency.Rounding : _settingService.ProgramSettings.AutoRoundDiscount);
                _orderSelectorViewModel.UpdateExchangeRate(_paymentEditor.ExchangeRate);
                if (_paymentEditor.SelectedTicket != null)
                {
                    var selected = _orderSelectorViewModel.SelectedTotal * _paymentEditor.ExchangeRate;
                    var remaining = _paymentEditor.GetRemainingAmount();
                    if (selected == 0 || selected > remaining) selected = remaining;
                    UpdatePaymentAmount(selected);
                }
            }
        }

        public void UpdateCurrencyButtons()
        {
            _paymentButtonsViewModel.Update(ForeignCurrency);

            foreach (var commandButtonViewModel in ForeignCurrencyButtons)
            {
                var format = commandButtonViewModel.Parameter.CurrencySymbol;
                if (!format.Contains("{0")) format = format + " {0:N2}";
                var pm = _tenderedValueViewModel.GetPaymentDueValue() / commandButtonViewModel.Parameter.ExchangeRate;
                commandButtonViewModel.Caption = string.Format(format, pm);
            }
            RaisePropertyChanged(() => ForeignCurrencyButtons);
        }

        public void UpdatePaymentAmount(decimal value)
        {
            _tenderedValueViewModel.UpdatePaymentAmount(value);
            UpdateCurrencyButtons();
        }

        private void OnForeignCurrencySelected(ForeignCurrency obj)
        {
            ForeignCurrency = obj;
            _paymentButtonsViewModel.Update(ForeignCurrency);
        }

        public void Prepare()
        {
            ForeignCurrency = null;
            RefreshForeignCurrencyButtons();
        }
    }
}
