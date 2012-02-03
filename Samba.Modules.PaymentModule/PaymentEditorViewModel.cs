using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Windows;
using Microsoft.Practices.Prism.Commands;
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
        private bool _resetAmount;
        private readonly IApplicationState _applicationState;
        private readonly ITicketService _ticketService;
        private readonly ICaptionCommand _manualPrintCommand;
        private readonly ICaptionCommand _makePaymentCommand;
        private readonly IPrinterService _printerService;
        private readonly IUserService _userService;
        private readonly IAutomationService _automationService;
        private readonly ICacheService _cacheService;

        [ImportingConstructor]
        public PaymentEditorViewModel(IApplicationState applicationState, ITicketService ticketService, ICacheService cacheService,
            IPrinterService printerService, IUserService userService, IAutomationService automationService)
        {
            _applicationState = applicationState;
            _ticketService = ticketService;
            _printerService = printerService;
            _userService = userService;
            _automationService = automationService;
            _cacheService = cacheService;

            _manualPrintCommand = new CaptionCommand<PrintJob>(Resources.Print, OnManualPrint, CanManualPrint);
            _makePaymentCommand = new CaptionCommand<Account>("", OnMakePayment, CanMakePayment);

            SubmitAccountPaymentCommand = new CaptionCommand<string>(Resources.AccountBalance_r, OnSubmitAccountPayment, CanSubmitAccountPayment);
            ClosePaymentScreenCommand = new CaptionCommand<string>(Resources.Close, OnClosePaymentScreen, CanClosePaymentScreen);
            TenderAllCommand = new CaptionCommand<string>(Resources.All, OnTenderAllCommand);
            TypeValueCommand = new DelegateCommand<string>(OnTypeValueExecuted);
            SetValueCommand = new DelegateCommand<string>(OnSetValue);
            DivideValueCommand = new DelegateCommand<string>(OnDivideValue);
            SelectMergedItemCommand = new DelegateCommand<MergedItem>(OnMergedItemSelected);

            SetDiscountAmountCommand = new CaptionCommand<string>(Resources.Round, OnSetDiscountAmountCommand, CanSetDiscount);
            AutoSetDiscountAmountCommand = new CaptionCommand<string>(Resources.Flat, OnAutoSetDiscount, CanAutoSetDiscount);
            SetDiscountRateCommand = new CaptionCommand<string>(Resources.DiscountPercentSign, OnSetDiscountRateCommand, CanSetDiscountRate);

            MergedItems = new ObservableCollection<MergedItem>();
            ReturningAmountVisibility = Visibility.Collapsed;

            LastTenderedAmount = "1";
        }

        public TicketTotalsViewModel Totals { get; set; }

        public CaptionCommand<string> SubmitAccountPaymentCommand { get; set; }
        public CaptionCommand<string> ClosePaymentScreenCommand { get; set; }
        public CaptionCommand<string> TenderAllCommand { get; set; }
        public DelegateCommand<string> TypeValueCommand { get; set; }
        public DelegateCommand<string> SetValueCommand { get; set; }
        public DelegateCommand<string> DivideValueCommand { get; set; }
        public DelegateCommand<MergedItem> SelectMergedItemCommand { get; set; }
        public CaptionCommand<string> SetDiscountRateCommand { get; set; }
        public CaptionCommand<string> SetDiscountAmountCommand { get; set; }
        public CaptionCommand<string> AutoSetDiscountAmountCommand { get; set; }

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

        public Visibility PaymentsVisibility
        {
            get
            {
                return Visibility.Visible;
                //return SelectedTicket != null && SelectedTicket.Payments.Count() > 0
                //           ? Visibility.Visible
                //           : Visibility.Collapsed;
            }
        }

        public IEnumerable<CommandButtonViewModel<PrintJob>> CommandButtons { get; set; }
        public IEnumerable<CommandButtonViewModel<Account>> PaymentButtons { get; set; }

        private IEnumerable<CommandButtonViewModel<Account>> CreatePaymentButtons(Ticket ticket)
        {
            var result = new List<CommandButtonViewModel<Account>>();
            var paymentTemplate = _cacheService.GetAccountTransactionTemplateById(ticket.PaymentTransactionTemplateId);
            var paymentAccounts = _cacheService.GetAccountsByTemplateId(paymentTemplate.TargetAccountTemplate.Id);
            result.AddRange(
                paymentAccounts.Select(x => new CommandButtonViewModel<Account>
                                                   {
                                                       Caption = x.Name,
                                                       Command = _makePaymentCommand,
                                                       Parameter = x
                                                   }));

            result.Add(new CommandButtonViewModel<Account>
            {
                Caption = "Close",
                Command = ClosePaymentScreenCommand,
                Color = "Red"
            });

            return result;
        }

        private IEnumerable<CommandButtonViewModel<PrintJob>> CreateCommandButtons()
        {
            var result = new List<CommandButtonViewModel<PrintJob>>();

            result.Add(new CommandButtonViewModel<PrintJob>
                           {
                               Caption = SetDiscountRateCommand.Caption,
                               Command = SetDiscountRateCommand
                           });

            result.Add(new CommandButtonViewModel<PrintJob>
                           {
                               Caption = SetDiscountAmountCommand.Caption,
                               Command = SetDiscountAmountCommand
                           });

            result.Add(new CommandButtonViewModel<PrintJob>
                           {
                               Caption = AutoSetDiscountAmountCommand.Caption,
                               Command = AutoSetDiscountAmountCommand
                           });

            if (SelectedTicket != null)
            {
                result.AddRange(_applicationState.CurrentTerminal.PrintJobs.Where(x => !string.IsNullOrEmpty(x.ButtonHeader) && x.UseFromPaymentScreen)
                    .Select(x => new CommandButtonViewModel<PrintJob>
                                     {
                                         Caption = x.ButtonHeader,
                                         Command = _manualPrintCommand,
                                         Parameter = x
                                     })
                );
            }
            return result;
        }

        private bool CanMakePayment(Account arg)
        {
            return SelectedTicket != null
                && GetTenderedValue() > 0
                && SelectedTicket.GetRemainingAmount() > 0;
        }

        private void OnMakePayment(Account obj)
        {
            SubmitPayment(obj);
        }

        private bool CanManualPrint(PrintJob arg)
        {
            return arg != null && SelectedTicket != null && (!SelectedTicket.Locked || SelectedTicket.GetPrintCount(arg.Id) == 0);
        }

        private void OnManualPrint(PrintJob obj)
        {
            _ticketService.UpdateTicketNumber(SelectedTicket, _applicationState.CurrentDepartment.TicketTemplate.TicketNumerator);
            _printerService.ManualPrintTicket(SelectedTicket, obj);
        }

        private bool CanAutoSetDiscount(string arg)
        {
            return SelectedTicket != null && SelectedTicket.GetRemainingAmount() > 0;
        }

        private bool CanSetDiscount(string arg)
        {
            if (GetTenderedValue() == 0) return true;
            return SelectedTicket != null
                && GetTenderedValue() <= SelectedTicket.GetRemainingAmount()
                && (SelectedTicket.TotalAmount > 0)
                && _userService.IsUserPermittedFor(PermissionNames.MakeDiscount)
                && _userService.IsUserPermittedFor(PermissionNames.RoundPayment);
        }

        private bool CanSetDiscountRate(string arg)
        {
            if (GetTenderedValue() == 0) return true;
            return SelectedTicket != null
                && SelectedTicket.TotalAmount > 0
                && SelectedTicket.GetRemainingAmount() > 0
                && GetTenderedValue() <= 100 && _userService.IsUserPermittedFor(PermissionNames.MakeDiscount);
        }

        private bool CanClosePaymentScreen(string arg)
        {
            return string.IsNullOrEmpty(TenderedAmount) || (SelectedTicket != null && SelectedTicket.GetRemainingAmount() == 0);
        }

        private void OnTenderAllCommand(string obj)
        {
            TenderedAmount = PaymentAmount;
            _resetAmount = true;
        }

        private void OnSubmitAccountPayment(string obj)
        {
            //SubmitPayment(PaymentType.Account);
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
            var remainingTicketAmount = SelectedTicket.GetRemainingAmount();

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

            SelectedTicket.PublishEvent(EventTopicNames.PaymentSubmitted);
            TenderedAmount = "";
            ReturningAmount = "";
            ReturningAmountVisibility = Visibility.Collapsed;
            SelectedTicket = null;
        }

        private bool CanSubmitAccountPayment(string arg)
        {
            return SelectedTicket != null
                && SelectedTicket.SaleTransaction.TargetTransactionValue.AccountId > 0
                && GetTenderedValue() == GetPaymentValue()
                && SelectedTicket.GetRemainingAmount() > 0;
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

        private void SubmitPayment(Account paymentAccount)
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

            if (tenderedAmount > 0)
            {
                if (tenderedAmount > SelectedTicket.GetRemainingAmount())
                    tenderedAmount = SelectedTicket.GetRemainingAmount();
                _ticketService.AddPayment(SelectedTicket, tenderedAmount, DateTime.Now, paymentAccount);
                PaymentAmount = (GetPaymentValue() - tenderedAmount).ToString("#,#0.00");

                LastTenderedAmount = tenderedAmount <= SelectedTicket.GetRemainingAmount()
                    ? tenderedAmount.ToString("#,#0.00")
                    : SelectedTicket.GetRemainingAmount().ToString("#,#0.00");
            }

            ReturningAmount = string.Format(Resources.ChangeAmount_f, returningAmount.ToString(LocalSettings.DefaultCurrencyFormat));
            ReturningAmountVisibility = returningAmount > 0 ? Visibility.Visible : Visibility.Collapsed;

            if (returningAmount > 0)
            {
                _automationService.NotifyEvent(RuleEventNames.ChangeAmountChanged,
                    new { Ticket = SelectedTicket, TicketAmount = SelectedTicket.TotalAmount, ChangeAmount = returningAmount, TenderedAmount = tenderedAmount });
            }

            if (returningAmount == 0 && SelectedTicket.GetRemainingAmount() == 0)
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

        private void OnSetDiscountRateCommand(string obj)
        {
            var tenderedvalue = GetTenderedValue();
            if (tenderedvalue == 0 && SelectedTicket.GetDiscountTotal() == 0)
            {
                InteractionService.UserIntraction.GiveFeedback(Resources.EmptyDiscountRateFeedback);
            }
            if (tenderedvalue > 0 && SelectedTicket.GetPlainSum() > 0)
            {
                SelectedTicket.AddTicketDiscount(DiscountType.Percent, GetTenderedValue(), _applicationState.CurrentLoggedInUser.Id);
            }
            else SelectedTicket.AddTicketDiscount(DiscountType.Percent, 0, _applicationState.CurrentLoggedInUser.Id);
            PaymentAmount = "";
            RefreshValues();
        }

        private void OnAutoSetDiscount(string obj)
        {
            if (GetTenderedValue() == 0) return;
            if (!_userService.IsUserPermittedFor(PermissionNames.FixPayment) && GetTenderedValue() > GetPaymentValue()) return;
            if (!_userService.IsUserPermittedFor(PermissionNames.RoundPayment)) return;
            SelectedTicket.AddTicketDiscount(DiscountType.Amount, 0, _applicationState.CurrentLoggedInUser.Id);
            SelectedTicket.AddTicketDiscount(DiscountType.Auto, 0, _applicationState.CurrentLoggedInUser.Id);
            SelectedTicket.AddTicketDiscount(DiscountType.Amount, SelectedTicket.GetRemainingAmount() - GetTenderedValue(),
                _applicationState.CurrentLoggedInUser.Id);
            PaymentAmount = "";
            RefreshValues();
        }

        private void OnSetDiscountAmountCommand(string obj)
        {
            if (GetTenderedValue() > GetPaymentValue()) return;
            SelectedTicket.AddTicketDiscount(DiscountType.Amount, GetTenderedValue(), _applicationState.CurrentLoggedInUser.Id);
            PaymentAmount = "";
            RefreshValues();
        }

        public void RefreshValues()
        {
            _ticketService.RecalculateTicket(SelectedTicket);
            if (SelectedTicket.GetRemainingAmount() < 0)
            {
                SelectedTicket.Discounts.Clear();
                _ticketService.RecalculateTicket(SelectedTicket);
                InteractionService.UserIntraction.GiveFeedback(Resources.AllDiscountsRemoved);
            }
            if (GetPaymentValue() <= 0)
                PaymentAmount = SelectedTicket != null
                    ? SelectedTicket.GetRemainingAmount().ToString("#,#0.00")
                    : "";

            Totals.ResetCache();

            RaisePropertyChanged(() => SelectedTicket);
            RaisePropertyChanged(() => Totals);
            RaisePropertyChanged(() => ReturningAmountVisibility);
            RaisePropertyChanged(() => PaymentsVisibility);
            RaisePropertyChanged(() => ReturningAmount);
            TenderedAmount = "";
        }

        public void PrepareMergedItems()
        {
            MergedItems.Clear();
            PaymentAmount = "";
            _selectedTotal = 0;

            var serviceAmount = SelectedTicket.GetServicesTotal();
            var sum = SelectedTicket.GetSumWithoutTax();

            if (sum == 0) return;

            SelectedTicket.Orders.Where(x => x.CalculatePrice).ToList().ForEach(x => CreateMergedItem(sum, x, serviceAmount));

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
                if (_selectedTotal > SelectedTicket.GetRemainingAmount())
                    _selectedTotal = SelectedTicket.GetRemainingAmount();
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
            PaymentButtons = CreatePaymentButtons(selectedTicket);
            RaisePropertyChanged(() => PaymentButtons);
        }

        public void Prepare(Ticket selectedTicket)
        {
            Debug.Assert(SelectedTicket == null);
            Totals = new TicketTotalsViewModel(selectedTicket);
            SelectedTicket = selectedTicket;
            TicketRemainingValue = selectedTicket.GetRemainingAmount();
            PrepareMergedItems();
            RefreshValues();
            LastTenderedAmount = PaymentAmount;
            CreateButtons(selectedTicket);

        }
    }
}