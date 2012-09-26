using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using Microsoft.Practices.Prism.Events;
using Samba.Domain.Models.Accounts;
using Samba.Domain.Models.Settings;
using Samba.Domain.Models.Tickets;
using Samba.Infrastructure.Settings;
using Samba.Localization.Properties;
using Samba.Presentation.Common;
using Samba.Presentation.Common.Services;
using Samba.Presentation.ViewModels;
using Samba.Services;
using Samba.Services.Common;

namespace Samba.Modules.PaymentModule
{
    [Export]
    public class PaymentEditorViewModel : ObservableObject
    {
        private readonly ITicketService _ticketService;
        private readonly ICacheService _cacheService;
        private readonly IAccountService _accountService;
        private readonly ISettingService _settingService;
        private readonly ICaptionCommand _executeAutomationCommand;
        private readonly ICaptionCommand _makePaymentCommand;
        private readonly ICaptionCommand _selectChangePaymentTemplateCommand;
        private readonly ICaptionCommand _serviceSelectedCommand;
        private readonly ICaptionCommand _foreignCurrencySelectedCommand;
        private readonly IAutomationService _automationService;

        [ImportingConstructor]
        public PaymentEditorViewModel(ITicketService ticketService, ICacheService cacheService, IAccountService accountService,
            ISettingService settingService, IAutomationService automationService, TicketTotalsViewModel totals)
        {
            _ticketService = ticketService;
            _cacheService = cacheService;
            _accountService = accountService;
            _settingService = settingService;
            _automationService = automationService;

            _executeAutomationCommand = new CaptionCommand<AutomationCommandData>("", OnExecuteAutomationCommand, CanExecuteAutomationCommand);
            _makePaymentCommand = new CaptionCommand<PaymentTemplate>("", OnMakePayment, CanMakePayment);
            _selectChangePaymentTemplateCommand = new CaptionCommand<PaymentData>("", OnSelectChangePaymentTemplate);
            _serviceSelectedCommand = new CaptionCommand<CalculationSelector>("", OnSelectCalculationSelector, CanSelectCalculationSelector);
            _foreignCurrencySelectedCommand = new CaptionCommand<ForeignCurrency>("", OnForeignCurrencySelected);

            ClosePaymentScreenCommand = new CaptionCommand<string>(Resources.Close, OnClosePaymentScreen, CanClosePaymentScreen);

            ChangeTemplates = new ObservableCollection<CommandButtonViewModel<PaymentData>>();
            ReturningAmountVisibility = Visibility.Collapsed;

            Totals = totals;

            PaymentButtonGroup = new PaymentButtonGroupViewModel(_makePaymentCommand, null, ClosePaymentScreenCommand);
            ForeignCurrencyButtons = new List<CommandButtonViewModel<ForeignCurrency>>();
            NumberPadViewModel = new NumberPadViewModel();
            NumberPadViewModel.TypedValueChanged += NumberPadViewModelTypedValueChanged;
            NumberPadViewModel.ResetValue += NumberPadViewModelResetValue;
            NumberPadViewModel.PaymentDueChanged += NumberPadViewModelPaymentDueChanged;
            OrderSelector = new OrderSelectorViewModel(new OrderSelector(), NumberPadViewModel);

            EventServiceFactory.EventService.GetEvent<GenericEvent<EventAggregator>>().Subscribe(x =>
            {
                if (SelectedTicket != null && x.Topic == EventTopicNames.CloseTicketRequested)
                {
                    SelectedTicket = null;
                }
            });
        }

        void NumberPadViewModelPaymentDueChanged(object sender, EventArgs e)
        {
            RaisePropertyChanged(() => PaymentAmount);
            UpdateCurrencyButtons();
        }

        void NumberPadViewModelTypedValueChanged(object sender, EventArgs e)
        {
            ReturningAmountVisibility = Visibility.Collapsed;
            RaisePropertyChanged(() => TenderedAmount);
        }

        void NumberPadViewModelResetValue(object sender, EventArgs e)
        {
            UpdatePaymentAmount(0);
            OrderSelector.ClearSelection();
            RefreshValues();
            RaisePropertyChanged(() => PaymentAmount);
            RaisePropertyChanged(() => TenderedAmount);
        }

        public NumberPadViewModel NumberPadViewModel { get; set; }
        public TicketTotalsViewModel Totals { get; set; }
        public PaymentButtonGroupViewModel PaymentButtonGroup { get; set; }

        public CaptionCommand<string> ClosePaymentScreenCommand { get; set; }

        public OrderSelectorViewModel OrderSelector { get; set; }
        public ObservableCollection<CommandButtonViewModel<PaymentData>> ChangeTemplates { get; set; }

        public string SelectedTicketTitle { get { return SelectedTicket != null ? Totals.Title : ""; } }

        private ForeignCurrency _foreignCurrency;
        protected ForeignCurrency ForeignCurrency
        {
            get { return _foreignCurrency; }
            set
            {
                if (_foreignCurrency != value)
                {
                    _foreignCurrency = value;
                    ExchangeRate = value != null ? value.ExchangeRate : 1;
                }
                else
                {
                    _foreignCurrency = null;
                    ExchangeRate = 1;
                }
                OrderSelector.UpdateAutoRoundValue(ForeignCurrency != null ? ForeignCurrency.Rounding : _settingService.ProgramSettings.AutoRoundDiscount);
                OrderSelector.UpdateExchangeRate(ExchangeRate);
                NumberPadViewModel.UpdateExchangeRate(ExchangeRate);
                if (SelectedTicket != null)
                {
                    var selected = OrderSelector.SelectedTotal * ExchangeRate;
                    var remaining = GetRemainingAmount();
                    if (selected == 0 || selected > remaining) selected = remaining;
                    UpdatePaymentAmount(selected);
                }
            }
        }

        protected decimal ExchangeRate;

        public string PaymentAmount
        {
            get { return NumberPadViewModel.PaymentDueAmount; }
            set { NumberPadViewModel.PaymentDueAmount = value; RaisePropertyChanged(() => PaymentAmount); }
        }

        public string TenderedAmount
        {
            get { return NumberPadViewModel.TenderedAmount; }
            set { NumberPadViewModel.TenderedAmount = value; RaisePropertyChanged(() => TenderedAmount); }
        }

        public string ReturningAmount
        {
            get { return _returningAmount; }
            set { _returningAmount = value; RaisePropertyChanged(() => ReturningAmount); }
        }

        private Visibility _returningAmountVisibility;
        public Visibility ReturningAmountVisibility
        {
            get { return _returningAmountVisibility; }
            set { _returningAmountVisibility = value; RaisePropertyChanged(() => ReturningAmountVisibility); }
        }

        private bool _isChangeOptionsVisible;
        public bool IsChangeOptionsVisible
        {
            get { return _isChangeOptionsVisible; }
            set
            {
                _isChangeOptionsVisible = value;
                RaisePropertyChanged(() => IsChangeOptionsVisible);
            }
        }

        private Ticket _selectedTicket;
        private string _returningAmount;

        public Ticket SelectedTicket
        {
            get { return _selectedTicket; }
            private set
            {
                _selectedTicket = value;
                RaisePropertyChanged(() => SelectedTicket);
                RaisePropertyChanged(() => SelectedTicketTitle);
            }
        }

        public decimal TicketRemainingValue { get; set; }

        public IList<CommandButtonViewModel<ForeignCurrency>> ForeignCurrencyButtons { get; set; }

        public IEnumerable<CommandButtonViewModel<object>> CommandButtons { get; set; }

        private IEnumerable<CommandButtonViewModel<ForeignCurrency>> CreateForeignCurrencyButtons()
        {
            return _cacheService.GetForeignCurrencies().Select(x => new CommandButtonViewModel<ForeignCurrency> { Caption = x.CurrencySymbol, Command = _foreignCurrencySelectedCommand, Parameter = x }).ToList();
        }

        private void UpdateCurrencyButtons()
        {
            if (ForeignCurrencyButtons.Count() == 1 && ForeignCurrencyButtons.First().Parameter.ExchangeRate == 1)
            {
                ForeignCurrency = ForeignCurrencyButtons.First().Parameter;
                ForeignCurrencyButtons.Clear();
            }
            foreach (var commandButtonViewModel in ForeignCurrencyButtons)
            {
                var pm = GetPaymentDueValue() / commandButtonViewModel.Parameter.ExchangeRate;
                commandButtonViewModel.Caption = string.Format(commandButtonViewModel.Parameter.CurrencySymbol, pm);
            }
        }

        private IEnumerable<CommandButtonViewModel<object>> CreateCommandButtons()
        {
            var result = new List<CommandButtonViewModel<object>>();

            if (SelectedTicket != null)
            {
                result.AddRange(_cacheService.GetCalculationSelectors().Where(x => !string.IsNullOrEmpty(x.ButtonHeader))
                    .Select(x => new CommandButtonViewModel<object>
                    {
                        Caption = x.ButtonHeader,
                        Command = _serviceSelectedCommand,
                        Color = x.ButtonColor,
                        Parameter = x
                    }));

                result.AddRange(_cacheService.GetAutomationCommands().Where(x => x.DisplayOnPayment)
                    .Select(x => new CommandButtonViewModel<object>
                    {
                        Caption = x.AutomationCommand.Name,
                        Command = _executeAutomationCommand,
                        Color = x.AutomationCommand.Color,
                        Parameter = x
                    }));
            }
            return result;
        }

        private void OnForeignCurrencySelected(ForeignCurrency obj)
        {
            ForeignCurrency = obj;
            CreateButtons(SelectedTicket);
        }

        private void OnExecuteAutomationCommand(AutomationCommandData obj)
        {
            _automationService.NotifyEvent(RuleEventNames.AutomationCommandExecuted, new { Ticket = SelectedTicket, AutomationCommandName = obj.AutomationCommand.Name });
        }

        private bool CanExecuteAutomationCommand(AutomationCommandData arg)
        {
            if (GetTenderedValue() <= 0) return false;
            if (SelectedTicket != null && SelectedTicket.Locked && arg != null && arg.VisualBehaviour == 1) return false;
            return true;
        }

        private void OnSelectCalculationSelector(CalculationSelector calculationSelector)
        {
            foreach (var calculationTemplate in calculationSelector.CalculationTemplates)
            {
                var amount = calculationTemplate.Amount;
                if (amount == 0) amount = GetTenderedValue();
                if (calculationTemplate.CalculationMethod == 0 || calculationTemplate.CalculationMethod == 1) amount = amount / ExchangeRate;
                SelectedTicket.AddCalculation(calculationTemplate, amount);
            }
            UpdatePaymentAmount(0);
            OrderSelector.UpdateTicket(SelectedTicket);
            RefreshValues();
        }

        private bool CanSelectCalculationSelector(CalculationSelector calculationSelector)
        {
            if (SelectedTicket != null && (SelectedTicket.Locked || SelectedTicket.IsClosed)) return false;
            if (GetPaymentDueValue() == 0 && SelectedTicket != null && !calculationSelector.CalculationTemplates.Any(x => SelectedTicket.Calculations.Any(y => y.CalculationTemplateId == x.Id))) return false;
            return calculationSelector == null || !calculationSelector.CalculationTemplates.Any(x => x.MaxAmount > 0 && GetTenderedValue() > x.MaxAmount);
        }

        private bool CanMakePayment(PaymentTemplate arg)
        {
            return SelectedTicket != null
                && !SelectedTicket.IsClosed
                && GetTenderedValue() != 0
                && GetRemainingAmount() != 0
                && (arg.Account != null || SelectedTicket.TicketResources.Any(x => CanMakeAccountTransaction(x, arg)));
        }

        private decimal GetRemainingAmount()
        {
            return SelectedTicket.GetRemainingAmount();
        }

        private bool CanMakeAccountTransaction(TicketResource ticketResource, PaymentTemplate paymentTemplate)
        {
            if (ticketResource.AccountId == 0) return false;
            var resourceTemplate = _cacheService.GetResourceTemplateById(ticketResource.ResourceTemplateId);
            if (resourceTemplate.AccountTemplateId != paymentTemplate.AccountTransactionTemplate.TargetAccountTemplateId) return false;
            return true;
        }

        private Account GetAccountForTransaction(PaymentTemplate paymentTemplate, IEnumerable<TicketResource> ticketResources)
        {
            var rt = _cacheService.GetResourceTemplates().Where(
                x => x.AccountTemplateId == paymentTemplate.AccountTransactionTemplate.TargetAccountTemplateId).Select(x => x.Id);
            var tr = ticketResources.FirstOrDefault(x => rt.Contains(x.ResourceTemplateId));
            return tr != null ? _accountService.GetAccountById(tr.AccountId) : null;
        }

        private void OnMakePayment(PaymentTemplate obj)
        {
            SubmitPayment(obj);
        }

        private bool CanClosePaymentScreen(string arg)
        {
            return string.IsNullOrEmpty(TenderedAmount) || (SelectedTicket != null && GetRemainingAmount() == 0);
        }

        private void OnClosePaymentScreen(string obj)
        {
            ClosePaymentScreen();
        }

        private void ClosePaymentScreen()
        {
            OrderSelector.PersistTicket();

            EventServiceFactory.EventService.PublishEvent(EventTopicNames.CloseTicketRequested);
            TenderedAmount = "";
            ReturningAmount = "";
            ReturningAmountVisibility = Visibility.Collapsed;
            IsChangeOptionsVisible = false;
            SelectedTicket = null;
        }

        private decimal GetTenderedValue()
        {
            decimal result;
            decimal.TryParse(TenderedAmount, out result);
            return decimal.Round(result * ExchangeRate, 2);
        }

        private decimal GetPaymentDueValue()
        {
            decimal result;
            decimal.TryParse(PaymentAmount, out result);
            return decimal.Round(result * ExchangeRate, 2);
        }

        private void SubmitPayment(PaymentTemplate paymentTemplate)
        {
            var paymentDueAmount = GetPaymentDueValue();
            var tenderedAmount = GetTenderedValue();

            if (Math.Abs(paymentDueAmount - GetRemainingAmount()) <= 0.01m)
                paymentDueAmount = GetRemainingAmount();

            if (tenderedAmount == 0 || Math.Abs(paymentDueAmount - tenderedAmount) <= 0.01m)
                tenderedAmount = paymentDueAmount;

            if (tenderedAmount <= paymentDueAmount)
            {
                SubmitPaymentAmount(paymentTemplate, null, paymentDueAmount, tenderedAmount);
                return;
            }

            var changeTemplates = GetChangePaymentTemplates();
            if (changeTemplates.Count() < 2)
            {
                SubmitPaymentAmount(paymentTemplate, changeTemplates.SingleOrDefault(), paymentDueAmount, tenderedAmount);
            }
            else
            {
                ChangeTemplates.Clear();
                ChangeTemplates.AddRange(changeTemplates.Select(x => new CommandButtonViewModel<PaymentData>
                {
                    Caption = GetChangeAmountCaption(paymentDueAmount, tenderedAmount, x),
                    Parameter = new PaymentData
                    {
                        ChangePaymentTemplate = x,
                        PaymentDueAmount = paymentDueAmount,
                        TenderedAmount = tenderedAmount,
                        PaymentTemplate = paymentTemplate
                    },
                    Command = _selectChangePaymentTemplateCommand
                }));
                IsChangeOptionsVisible = true;
            }
        }

        private void OnSelectChangePaymentTemplate(PaymentData paymentData)
        {
            SubmitPaymentAmount(paymentData.PaymentTemplate, paymentData.ChangePaymentTemplate,
                paymentData.PaymentDueAmount, paymentData.TenderedAmount);
            IsChangeOptionsVisible = false;
        }

        private string GetChangeAmountCaption(decimal paymentDueAmount, decimal tenderedAmount, ChangePaymentTemplate changeTemplate)
        {
            var returningAmount = (tenderedAmount - paymentDueAmount);
            if (changeTemplate != null)
            {
                var currency =
                    _cacheService.GetForeignCurrencies().SingleOrDefault(
                        x => x.Id == changeTemplate.Account.ForeignCurrencyId);
                if (currency != null)
                {
                    returningAmount = returningAmount / currency.ExchangeRate;
                    return string.Format(currency.CurrencySymbol, returningAmount);
                }
            }

            return returningAmount.ToString(LocalSettings.DefaultCurrencyFormat);
        }

        private decimal DisplayReturningAmount(decimal tenderedAmount, decimal paymentDueAmount,
                                               ChangePaymentTemplate changeTemplate)
        {
            var returningAmount = 0m;

            if (tenderedAmount > paymentDueAmount)
            {
                ReturningAmount = "";
                returningAmount = (tenderedAmount - paymentDueAmount);
                if (changeTemplate != null)
                {
                    var currency =
                        _cacheService.GetForeignCurrencies().SingleOrDefault(
                            x => x.Id == changeTemplate.Account.ForeignCurrencyId);

                    ReturningAmount = string.Format(Resources.ChangeAmount_f,
                            currency != null
                                ? string.Format(currency.CurrencySymbol, returningAmount / currency.ExchangeRate)
                                : returningAmount.ToString(LocalSettings.DefaultCurrencyFormat));
                }
            }

            if (string.IsNullOrEmpty(ReturningAmount))
                ReturningAmount = string.Format(Resources.ChangeAmount_f,
                    (returningAmount / ExchangeRate).ToString(LocalSettings.DefaultCurrencyFormat));

            ReturningAmountVisibility = returningAmount > 0 ? Visibility.Visible : Visibility.Collapsed;

            if (returningAmount != 0)
            {
                _automationService.NotifyEvent(RuleEventNames.ChangeAmountChanged,
                                               new
                                                   {
                                                       Ticket = SelectedTicket,
                                                       TicketAmount = SelectedTicket.TotalAmount,
                                                       ChangeAmount = returningAmount,
                                                       TenderedAmount = tenderedAmount
                                                   });
            }
            return returningAmount;
        }

        private void SubmitPaymentAmount(PaymentTemplate paymentTemplate, ChangePaymentTemplate changeTemplate, decimal paymentDueAmount, decimal tenderedAmount)
        {
            var returningAmount = DisplayReturningAmount(tenderedAmount, paymentDueAmount, changeTemplate);
            if (changeTemplate == null) tenderedAmount -= returningAmount;

            var paymentAccount = paymentTemplate.Account ?? GetAccountForTransaction(paymentTemplate, SelectedTicket.TicketResources);

            _ticketService.AddPayment(SelectedTicket, paymentTemplate, paymentAccount, tenderedAmount);

            if (tenderedAmount > paymentDueAmount && changeTemplate != null)
            {
                _ticketService.AddChangePayment(SelectedTicket, changeTemplate, changeTemplate.Account, tenderedAmount - paymentDueAmount);
            }

            NumberPadViewModel.LastTenderedAmount = (tenderedAmount / ExchangeRate).ToString("#,#0.00");

            UpdatePaymentAmount(GetRemainingAmount());

            if (returningAmount == 0 && GetRemainingAmount() == 0)
            {
                ClosePaymentScreen();
            }
            else
            {
                OrderSelector.PersistSelectedItems();
                RefreshValues();
            }
        }

        private IList<ChangePaymentTemplate> GetChangePaymentTemplates()
        {
            if (ForeignCurrency == null) return new List<ChangePaymentTemplate>();
            return _cacheService.GetChangePaymentTemplates().ToList();
        }

        public void RefreshValues()
        {
            if (SelectedTicket == null) return;

            _ticketService.RecalculateTicket(SelectedTicket);
            if (GetRemainingAmount() < 0)
            {
                foreach (var cSelector in _cacheService.GetCalculationSelectors().Where(x => !string.IsNullOrEmpty(x.ButtonHeader)))
                {
                    foreach (var ctemplate in cSelector.CalculationTemplates)
                    {
                        while (SelectedTicket.Calculations.Any(x => x.CalculationTemplateId == ctemplate.Id))
                            SelectedTicket.AddCalculation(ctemplate, 0);
                    }
                }

                _ticketService.RecalculateTicket(SelectedTicket);
                if (GetRemainingAmount() >= 0)
                    InteractionService.UserIntraction.GiveFeedback(Resources.AllDiscountsRemoved);
            }

            if (GetPaymentDueValue() <= 0)
                UpdatePaymentAmount(SelectedTicket != null ? GetRemainingAmount() : 0);

            Totals.ResetCache();

            RaisePropertyChanged(() => SelectedTicket);
            RaisePropertyChanged(() => Totals);
            RaisePropertyChanged(() => ReturningAmountVisibility);
            RaisePropertyChanged(() => ReturningAmount);
            TenderedAmount = "";
        }

        public void CreateButtons(Ticket selectedTicket)
        {
            CommandButtons = CreateCommandButtons();
            RaisePropertyChanged(() => CommandButtons);
            PaymentButtonGroup.UpdatePaymentButtons(_cacheService.GetPaymentScreenPaymentTemplates(), ForeignCurrency);
            RaisePropertyChanged(() => PaymentButtonGroup);
            ForeignCurrencyButtons = CreateForeignCurrencyButtons().ToList();
            UpdateCurrencyButtons();
            RaisePropertyChanged(() => ForeignCurrencyButtons);
        }

        public void Prepare(Ticket selectedTicket)
        {
            ForeignCurrency = null;
            Debug.Assert(SelectedTicket == null);
            Totals.Model = selectedTicket;
            SelectedTicket = selectedTicket;
            TicketRemainingValue = GetRemainingAmount();
            UpdatePaymentAmount(0);
            OrderSelector.UpdateTicket(selectedTicket);
            RefreshValues();
            NumberPadViewModel.LastTenderedAmount = PaymentAmount;
            CreateButtons(selectedTicket);
        }

        public void UpdatePaymentAmount(decimal value)
        {
            var remaining = GetRemainingAmount();
            if (value == 0 || value > remaining) value = remaining;
            value /= ExchangeRate;
            PaymentAmount = value.ToString("#,#0.00");
            UpdateCurrencyButtons();
        }
    }
}