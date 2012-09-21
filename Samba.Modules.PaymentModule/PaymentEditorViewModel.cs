using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Windows;
using Microsoft.Practices.Prism.Commands;
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
    public class PaymentData
    {
        public PaymentTemplate PaymentTemplate { get; set; }
        public ChangePaymentTemplate ChangePaymentTemplate { get; set; }
        public decimal PaymentDueAmount { get; set; }
        public decimal TenderedAmount { get; set; }
    }

    [Export]
    public class PaymentEditorViewModel : ObservableObject
    {
        private bool _resetAmount;
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
            TenderAllCommand = new CaptionCommand<string>(Resources.All, OnTenderAllCommand);
            TypeValueCommand = new DelegateCommand<string>(OnTypeValueExecuted);
            SetValueCommand = new DelegateCommand<string>(OnSetValue);
            DivideValueCommand = new DelegateCommand<string>(OnDivideValue);
            SelectMergedItemCommand = new DelegateCommand<MergedItem>(OnMergedItemSelected);

            ChangeTemplates = new ObservableCollection<CommandButtonViewModel<PaymentData>>();
            MergedItems = new ObservableCollection<MergedItem>();
            ReturningAmountVisibility = Visibility.Collapsed;

            Totals = totals;

            PaymentButtonGroup = new PaymentButtonGroupViewModel(_makePaymentCommand, null, ClosePaymentScreenCommand);
            ForeignCurrencyButtons = new List<CommandButtonViewModel<ForeignCurrency>>();

            LastTenderedAmount = "1";

            EventServiceFactory.EventService.GetEvent<GenericEvent<EventAggregator>>().Subscribe(x =>
            {
                if (SelectedTicket != null && x.Topic == EventTopicNames.CloseTicketRequested)
                {
                    SelectedTicket = null;
                }
            });
        }

        public TicketTotalsViewModel Totals { get; set; }
        public PaymentButtonGroupViewModel PaymentButtonGroup { get; set; }

        public CaptionCommand<string> ClosePaymentScreenCommand { get; set; }
        public CaptionCommand<string> TenderAllCommand { get; set; }
        public DelegateCommand<string> TypeValueCommand { get; set; }
        public DelegateCommand<string> SetValueCommand { get; set; }
        public DelegateCommand<string> DivideValueCommand { get; set; }
        public DelegateCommand<MergedItem> SelectMergedItemCommand { get; set; }

        public ObservableCollection<MergedItem> MergedItems { get; set; }
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
                if (SelectedTicket != null)
                {
                    UpdateMergedItems();
                    RefreshValues();
                }
            }
        }

        protected decimal ExchangeRate;

        private string _paymentAmount;
        public string PaymentAmount
        {
            get { return _paymentAmount; }
            set { _paymentAmount = value; RaisePropertyChanged(() => PaymentAmount); }
        }

        private string _tenderedAmount;
        public string TenderedAmount
        {
            get { return _tenderedAmount; }
            set { _tenderedAmount = value; RaisePropertyChanged(() => TenderedAmount); }
        }

        private string _lastTenderedAmount;
        public string LastTenderedAmount
        {
            get { return _lastTenderedAmount; }
            set { _lastTenderedAmount = value; RaisePropertyChanged(() => LastTenderedAmount); }
        }

        public string ReturningAmount { get; set; }

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

        public IList<CommandButtonViewModel<ForeignCurrency>> ForeignCurrencyButtons { get; set; }

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
                var pm = GetPaymentValue() / commandButtonViewModel.Parameter.ExchangeRate;
                commandButtonViewModel.Caption = string.Format(commandButtonViewModel.Parameter.CurrencySymbol, pm);
            }
        }

        public IEnumerable<object> CommandButtons { get; set; }

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
            PrepareMergedItems();
            RefreshValues();
        }

        private bool CanSelectCalculationSelector(CalculationSelector calculationSelector)
        {
            if (SelectedTicket != null && (SelectedTicket.Locked || SelectedTicket.IsClosed)) return false;
            if (GetPaymentValue() == 0 && SelectedTicket != null && !calculationSelector.CalculationTemplates.Any(x => SelectedTicket.Calculations.Any(y => y.CalculationTemplateId == x.Id))) return false;
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

        private void OnTenderAllCommand(string obj)
        {
            TenderedAmount = PaymentAmount;
            _resetAmount = true;
        }

        private void OnDivideValue(string obj)
        {
            decimal tenderedValue = GetTenderedValue();
            CancelMergedItems();
            _resetAmount = true;
            string dc = CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator;
            obj = obj.Replace(",", dc);
            obj = obj.Replace(".", dc);

            decimal value = Convert.ToDecimal(obj);
            var remainingTicketAmount = GetPaymentValue() / ExchangeRate;

            if (value > 0)
            {
                var amount = remainingTicketAmount / value;
                if (amount > remainingTicketAmount) amount = remainingTicketAmount;
                TenderedAmount = amount.ToString("#,#0.00");
            }
            else
            {
                value = tenderedValue;
                if (value > 0)
                {
                    var amount = remainingTicketAmount / value;
                    if (amount > remainingTicketAmount) amount = remainingTicketAmount;
                    TenderedAmount = (amount).ToString("#,#0.00");
                }
            }
        }

        private void OnSetValue(string obj)
        {
            _resetAmount = true;
            ReturningAmountVisibility = Visibility.Collapsed;
            if (string.IsNullOrEmpty(obj))
            {
                TenderedAmount = "";
                UpdatePaymentAmount(0);
                CancelMergedItems();
                return;
            }

            var value = Convert.ToDecimal(obj);
            if (string.IsNullOrEmpty(TenderedAmount))
                TenderedAmount = "0";
            var tenderedValue = Convert.ToDecimal(TenderedAmount.Replace(
                CultureInfo.CurrentCulture.NumberFormat.NumberGroupSeparator, ""));
            tenderedValue += value;
            TenderedAmount = tenderedValue.ToString("#,#0.00");
        }

        private void OnTypeValueExecuted(string obj)
        {
            if (_resetAmount) TenderedAmount = "";
            _resetAmount = false;
            ReturningAmountVisibility = Visibility.Collapsed;
            TenderedAmount = Helpers.AddTypedValue(TenderedAmount, obj, "#,#0.");
        }

        private void OnClosePaymentScreen(string obj)
        {
            ClosePaymentScreen();
        }

        private void ClosePaymentScreen()
        {
            var paidItems = MergedItems.SelectMany(x => x.PaidItems);

            SelectedTicket.PaidItems.Clear();
            foreach (var paidItem in paidItems)
            {
                SelectedTicket.PaidItems.Add(paidItem);
            }

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

        private decimal GetPaymentValue()
        {
            decimal result;
            decimal.TryParse(PaymentAmount, out result);
            return decimal.Round(result * ExchangeRate, 2);
        }

        private void SubmitPayment(PaymentTemplate paymentTemplate)
        {
            var paymentDueAmount = GetPaymentValue();
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
                    if (currency != null)
                    {
                        returningAmount = returningAmount / currency.ExchangeRate;
                        ReturningAmount = string.Format(currency.CurrencySymbol, returningAmount);
                    }
                }
            }

            if (string.IsNullOrEmpty(ReturningAmount))
                ReturningAmount = string.Format(Resources.ChangeAmount_f,
                                                returningAmount.ToString(LocalSettings.DefaultCurrencyFormat));

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

            var paymentAccount = paymentTemplate.Account ?? GetAccountForTransaction(paymentTemplate, SelectedTicket.TicketResources);

            if (tenderedAmount > paymentDueAmount && changeTemplate == null)
                tenderedAmount = paymentDueAmount;

            _ticketService.AddPayment(SelectedTicket, paymentTemplate, paymentAccount, tenderedAmount);

            if (tenderedAmount > paymentDueAmount && changeTemplate != null)
            {
                _ticketService.AddChangePayment(SelectedTicket, changeTemplate, changeTemplate.Account, tenderedAmount - paymentDueAmount);
            }

            LastTenderedAmount = tenderedAmount - paymentDueAmount <= GetRemainingAmount()
                                     ? (tenderedAmount - paymentDueAmount).ToString("#,#0.00")
                                     : GetRemainingAmount().ToString("#,#0.00");

            var returningAmount = DisplayReturningAmount(tenderedAmount, paymentDueAmount, changeTemplate);

            UpdatePaymentAmount(GetRemainingAmount());

            if (returningAmount == 0 && GetRemainingAmount() == 0)
            {
                ClosePaymentScreen();
            }
            else PersistMergedItems();
        }

        private IList<ChangePaymentTemplate> GetChangePaymentTemplates()
        {
            if (ForeignCurrency == null) return new List<ChangePaymentTemplate>();
            return _cacheService.GetChangePaymentTemplates().ToList();
        }

        private Ticket _selectedTicket;
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

            if (GetPaymentValue() <= 0)
                UpdatePaymentAmount(SelectedTicket != null ? GetRemainingAmount() : 0);

            Totals.ResetCache();

            RaisePropertyChanged(() => SelectedTicket);
            RaisePropertyChanged(() => Totals);
            RaisePropertyChanged(() => ReturningAmountVisibility);
            RaisePropertyChanged(() => ReturningAmount);
            TenderedAmount = "";
        }

        public void PrepareMergedItems()
        {
            MergedItems.Clear();
            UpdatePaymentAmount(0);

            if (SelectedTicket.GetSum() == 0) return;

            var serviceAmount = SelectedTicket.GetPreTaxServicesTotal() + SelectedTicket.GetPostTaxServicesTotal();
            SelectedTicket.Orders.Where(x => x.CalculatePrice && (x.ProductTimerValue == null || !x.ProductTimerValue.IsActive))
                .ToList().ForEach(x => CreateMergedItem(SelectedTicket.GetPlainSum(), x, serviceAmount));

            RoundMergedItems();

            foreach (var paidItem in SelectedTicket.PaidItems)
            {
                var item = paidItem;
                var mi = MergedItems.SingleOrDefault(x => x.Key == item.Key);
                if (mi != null)
                    mi.PaidItems.Add(paidItem);
            }
        }

        public void UpdateMergedItems()
        {
            if (SelectedTicket.GetSum() == 0) return;
            var serviceAmount = SelectedTicket.GetPreTaxServicesTotal() + SelectedTicket.GetPostTaxServicesTotal();

            MergedItems.ToList().ForEach(x => x.Quantity = 0);
            SelectedTicket.Orders.Where(x => x.CalculatePrice && (x.ProductTimerValue == null || !x.ProductTimerValue.IsActive))
                .ToList().ForEach(x => CreateMergedItem(SelectedTicket.GetPlainSum(), x, serviceAmount));

            RoundMergedItems();

            PaymentAmount = MergedItems.Sum(x => x.GetNewQuantity() * x.Price).ToString("#,#0.00");
            RaisePropertyChanged(() => MergedItems);
        }

        private void RoundMergedItems()
        {
            var ra = _settingService.ProgramSettings.AutoRoundDiscount;
            if (ra != 0)
            {
                var amount = 0m;
                foreach (var mergedItem in MergedItems)
                {
                    var price = mergedItem.Price;
                    var newPrice = decimal.Round(price / ra, MidpointRounding.AwayFromZero) * ra;
                    mergedItem.Price = newPrice;
                    amount += (newPrice * mergedItem.Quantity);
                }
                var mLast = MergedItems.OrderBy(x => x.Total).First();
                mLast.Price += (SelectedTicket.GetSum() / ExchangeRate) - amount;
            }
        }

        private void CreateMergedItem(decimal sum, Order item, decimal serviceAmount)
        {
            var price = item.GetItemPrice();
            price += (price * serviceAmount) / sum;
            if (!item.TaxIncluded) price += item.TaxAmount;
            price = price / ExchangeRate;
            //todo:fiyata göre ürün eşleştirmeyi kontrol et
            var mitem = MergedItems.SingleOrDefault(x => x.Key == item.MenuItemId + "-" + item.GetItemPrice());
            if (mitem == null)
            {
                mitem = new MergedItem();
                try
                {
                    mitem.Description = item.MenuItemName + item.GetPortionDesc();
                    mitem.Key = item.MenuItemId + "-" + item.GetItemPrice();
                    mitem.MenuItemId = item.MenuItemId;
                    MergedItems.Add(mitem);
                }
                finally
                {
                    mitem.Dispose();
                }
            }
            mitem.Price = price;
            mitem.Quantity += item.Quantity;
        }

        private decimal _selectedTotal;

        private void OnMergedItemSelected(MergedItem obj)
        {
            if (obj.RemainingQuantity > 0)
            {
                decimal quantity = 1;
                if (GetTenderedValue() > 0) quantity = GetTenderedValue();
                if (quantity > obj.RemainingQuantity) quantity = obj.RemainingQuantity;
                _selectedTotal += obj.Price * quantity;
                if (_selectedTotal > GetRemainingAmount())
                    _selectedTotal = GetRemainingAmount();
                UpdatePaymentAmount(_selectedTotal * ExchangeRate);
                TenderedAmount = "";
                _resetAmount = true;
                obj.IncQuantity(quantity);
            }
            ReturningAmountVisibility = Visibility.Collapsed;
        }

        private void PersistMergedItems()
        {
            _selectedTotal = 0;
            foreach (var mergedItem in MergedItems)
            {
                mergedItem.PersistPaidItems();
            }
            RefreshValues();
        }

        private void CancelMergedItems()
        {
            _selectedTotal = 0;
            foreach (var mergedItem in MergedItems)
            {
                mergedItem.CancelPaidItems();
            }
            RefreshValues();
            ReturningAmountVisibility = Visibility.Collapsed;
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
            PrepareMergedItems();
            RefreshValues();
            LastTenderedAmount = PaymentAmount;
            CreateButtons(selectedTicket);
        }

        public void UpdatePaymentAmount(decimal value)
        {
            if (value != 0) value = value / ExchangeRate;
            PaymentAmount = value == 0 ? "" : value.ToString("#,#0.00");
            UpdateCurrencyButtons();
        }
    }
}