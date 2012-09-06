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
        private bool _resetAmount;
        private readonly ITicketService _ticketService;
        private readonly ICacheService _cacheService;
        private readonly IAccountService _accountService;
        private readonly ISettingService _settingService;
        private readonly ICaptionCommand _executeAutomationCommand;
        private readonly ICaptionCommand _makePaymentCommand;
        private readonly ICaptionCommand _serviceSelectedCommand;
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
            _serviceSelectedCommand = new CaptionCommand<CalculationSelector>("", OnSelectCalculationSelector, CanSelectCalculationSelector);

            ClosePaymentScreenCommand = new CaptionCommand<string>(Resources.Close, OnClosePaymentScreen, CanClosePaymentScreen);
            TenderAllCommand = new CaptionCommand<string>(Resources.All, OnTenderAllCommand);
            TypeValueCommand = new DelegateCommand<string>(OnTypeValueExecuted);
            SetValueCommand = new DelegateCommand<string>(OnSetValue);
            DivideValueCommand = new DelegateCommand<string>(OnDivideValue);
            SelectMergedItemCommand = new DelegateCommand<MergedItem>(OnMergedItemSelected);

            MergedItems = new ObservableCollection<MergedItem>();
            ReturningAmountVisibility = Visibility.Collapsed;

            Totals = totals;

            PaymentButtonGroup = new PaymentButtonGroupViewModel(_makePaymentCommand, null, ClosePaymentScreenCommand);

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

        public string SelectedTicketTitle { get { return SelectedTicket != null ? Totals.Title : ""; } }

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
                SelectedTicket.AddCalculation(calculationTemplate, amount);
            }

            PaymentAmount = "";
            PrepareMergedItems();
            RefreshValues();
        }

        private bool CanSelectCalculationSelector(CalculationSelector calculationSelector)
        {
            if (SelectedTicket != null && SelectedTicket.Locked) return false;
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
            var tr = ticketResources.Where(x => rt.Contains(x.ResourceTemplateId)).FirstOrDefault();
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
            var remainingTicketAmount = GetRemainingAmount();

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
                PaymentAmount = "";
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
            SelectedTicket = null;
        }

        private decimal GetTenderedValue()
        {
            decimal result;
            decimal.TryParse(TenderedAmount, out result);
            return result;
        }

        private decimal GetPaymentValue()
        {
            decimal result;
            decimal.TryParse(PaymentAmount, out result);
            return result;
        }

        private void SubmitPayment(PaymentTemplate paymentTemplate)
        {
            var tenderedAmount = GetTenderedValue();
            var remainingTicketAmount = GetPaymentValue();
            var returningAmount = 0m;
            if (tenderedAmount == 0) tenderedAmount = remainingTicketAmount;

            if (tenderedAmount > remainingTicketAmount)
            {
                returningAmount = tenderedAmount - remainingTicketAmount;
                tenderedAmount = remainingTicketAmount;
            }

            if (tenderedAmount != 0)
            {
                if (tenderedAmount > GetRemainingAmount())
                    tenderedAmount = GetRemainingAmount();
                var account = paymentTemplate.Account ?? GetAccountForTransaction(paymentTemplate, SelectedTicket.TicketResources);
                _ticketService.AddPayment(SelectedTicket, paymentTemplate, account, tenderedAmount);
                PaymentAmount = (GetPaymentValue() - tenderedAmount).ToString("#,#0.00");

                LastTenderedAmount = tenderedAmount <= GetRemainingAmount()
                    ? tenderedAmount.ToString("#,#0.00")
                    : GetRemainingAmount().ToString("#,#0.00");
            }

            ReturningAmount = string.Format(Resources.ChangeAmount_f, returningAmount.ToString(LocalSettings.DefaultCurrencyFormat));
            ReturningAmountVisibility = returningAmount > 0 ? Visibility.Visible : Visibility.Collapsed;

            if (returningAmount != 0)
            {
                _automationService.NotifyEvent(RuleEventNames.ChangeAmountChanged,
                    new { Ticket = SelectedTicket, TicketAmount = SelectedTicket.TotalAmount, ChangeAmount = returningAmount, TenderedAmount = tenderedAmount });
            }

            if (returningAmount == 0 && GetRemainingAmount() == 0)
            {
                ClosePaymentScreen();
            }
            else PersistMergedItems();
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
            _ticketService.RecalculateTicket(SelectedTicket);
            if (GetRemainingAmount() < 0)
            {
                foreach (var cSelector in _cacheService.GetCalculationSelectors().Where(x => !string.IsNullOrEmpty(x.ButtonHeader)))
                {
                    foreach (var ctemplate in cSelector.CalculationTemplates)
                    {
                        while (SelectedTicket.Calculations.Any(x => x.ServiceId == ctemplate.Id))
                            SelectedTicket.AddCalculation(ctemplate, 0);
                    }
                }

                _ticketService.RecalculateTicket(SelectedTicket);
                if (GetRemainingAmount() >= 0)
                    InteractionService.UserIntraction.GiveFeedback(Resources.AllDiscountsRemoved);
            }

            if (GetPaymentValue() <= 0)
                PaymentAmount = SelectedTicket != null
                    ? GetRemainingAmount().ToString("#,#0.00")
                    : "";

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
            PaymentAmount = "";
            _selectedTotal = 0;

            var serviceAmount = SelectedTicket.GetPreTaxServicesTotal() + SelectedTicket.GetPostTaxServicesTotal();
            var sum = SelectedTicket.GetSum();

            if (sum == 0) return;

            SelectedTicket.Orders.Where(x => x.CalculatePrice && (x.MenuItemTimerValue == null || !x.MenuItemTimerValue.IsActive))
                .ToList().ForEach(x => CreateMergedItem(SelectedTicket.GetPlainSum(), x, serviceAmount));

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
                var mLast = MergedItems.Last();
                mLast.Price += SelectedTicket.GetSum() - amount;
            }

            foreach (var paidItem in SelectedTicket.PaidItems)
            {
                var item = paidItem;
                var mi = MergedItems.SingleOrDefault(x => x.MenuItemId == item.MenuItemId && x.Price == item.Price);
                if (mi != null)
                    mi.PaidItems.Add(paidItem);
            }
        }

        private void CreateMergedItem(decimal sum, Order item, decimal serviceAmount)
        {
            var price = item.GetItemPrice();
            price += (price * serviceAmount) / sum;
            if (!item.TaxIncluded) price += item.TaxAmount;
            var mitem = MergedItems.SingleOrDefault(x => x.MenuItemId == item.MenuItemId && x.Price == price);
            if (mitem == null)
            {
                mitem = new MergedItem();
                try
                {
                    mitem.Description = item.MenuItemName + item.GetPortionDesc();
                    mitem.Price = price;
                    mitem.MenuItemId = item.MenuItemId;
                    MergedItems.Add(mitem);
                }
                finally
                {
                    mitem.Dispose();
                }
            }
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
                PaymentAmount = _selectedTotal.ToString("#,#0.00");
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
            PaymentButtonGroup.UpdatePaymentButtons(_cacheService.GetPaymentScreenPaymentTemplates());
            RaisePropertyChanged(() => PaymentButtonGroup);
        }

        public void Prepare(Ticket selectedTicket)
        {
            Debug.Assert(SelectedTicket == null);
            Totals.Model = selectedTicket;
            SelectedTicket = selectedTicket;
            TicketRemainingValue = GetRemainingAmount();
            PrepareMergedItems();
            RefreshValues();
            LastTenderedAmount = PaymentAmount;
            CreateButtons(selectedTicket);

        }
    }
}