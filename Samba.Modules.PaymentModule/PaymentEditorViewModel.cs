using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using Samba.Domain.Models.Tickets;
using Samba.Localization.Properties;
using Samba.Presentation.Common;
using Samba.Presentation.Common.Commands;
using Samba.Presentation.Services;
using Samba.Presentation.Services.Common;
using Samba.Presentation.ViewModels;
using Samba.Services;

namespace Samba.Modules.PaymentModule
{
    [Export]
    public class PaymentEditorViewModel : ObservableObject
    {
        private readonly IPresentationCacheService _cacheService;

        private readonly ICaptionCommand _makePaymentCommand;
        private readonly ICaptionCommand _selectChangePaymentTypeCommand;

        private readonly TicketTotalsViewModel _paymentTotals;
        private readonly PaymentEditor _paymentEditor;
        private readonly NumberPadViewModel _numberPadViewModel;
        private readonly OrderSelectorViewModel _orderSelectorViewModel;
        private readonly ForeignCurrencyButtonsViewModel _foreignCurrencyButtonsViewModel;
        private readonly CommandButtonsViewModel _commandButtonsViewModel;
        private readonly TenderedValueViewModel _tenderedValueViewModel;
        private readonly ReturningAmountViewModel _returningAmountViewModel;
        private readonly ChangeTemplatesViewModel _changeTemplatesViewModel;

        [ImportingConstructor]
        public PaymentEditorViewModel(IPresentationCacheService cacheService, TicketTotalsViewModel paymentTotals,
            PaymentEditor paymentEditor, NumberPadViewModel numberPadViewModel, OrderSelectorViewModel orderSelectorViewModel,
            ForeignCurrencyButtonsViewModel foreignCurrencyButtonsViewModel, PaymentButtonsViewModel paymentButtonsViewModel,
            CommandButtonsViewModel commandButtonsViewModel, TenderedValueViewModel tenderedValueViewModel,
            ReturningAmountViewModel returningAmountViewModel, ChangeTemplatesViewModel changeTemplatesViewModel)
        {
            _cacheService = cacheService;
            _paymentTotals = paymentTotals;
            _paymentEditor = paymentEditor;
            _numberPadViewModel = numberPadViewModel;
            _orderSelectorViewModel = orderSelectorViewModel;
            _foreignCurrencyButtonsViewModel = foreignCurrencyButtonsViewModel;
            _commandButtonsViewModel = commandButtonsViewModel;
            _tenderedValueViewModel = tenderedValueViewModel;
            _returningAmountViewModel = returningAmountViewModel;
            _changeTemplatesViewModel = changeTemplatesViewModel;

            _makePaymentCommand = new CaptionCommand<PaymentType>("", OnMakePayment, CanMakePayment);
            _selectChangePaymentTypeCommand = new CaptionCommand<PaymentData>("", OnSelectChangePaymentType);

            ClosePaymentScreenCommand = new CaptionCommand<string>(Resources.Close, OnClosePaymentScreen, CanClosePaymentScreen);
            paymentButtonsViewModel.SetButtonCommands(_makePaymentCommand, null, ClosePaymentScreenCommand);
        }

        public CaptionCommand<string> ClosePaymentScreenCommand { get; set; }

        public string SelectedTicketTitle { get { return _paymentTotals.TitleWithAccountBalances; } }

        private bool CanMakePayment(PaymentType arg)
        {
            if (arg == null) return false;
            if (_paymentEditor.AccountMode && _tenderedValueViewModel.GetTenderedValue() > _tenderedValueViewModel.GetPaymentDueValue()) return false;
            return _paymentEditor.SelectedTicket != null
                && !_paymentEditor.SelectedTicket.IsClosed
                && _tenderedValueViewModel.GetTenderedValue() != 0
                && _paymentEditor.GetRemainingAmount() != 0
                && (arg.Account != null || _paymentEditor.SelectedTicket.TicketResources.Any(x => CanMakeAccountTransaction(x, arg)));
        }

        private bool CanMakeAccountTransaction(TicketResource ticketResource, PaymentType paymentType)
        {
            if (ticketResource.AccountId == 0) return false;
            if (_tenderedValueViewModel.GetTenderedValue() > _paymentEditor.SelectedTicket.GetRemainingAmount()) return false;
            var resourceType = _cacheService.GetResourceTypeById(ticketResource.ResourceTypeId);
            return resourceType.AccountTypeId == paymentType.AccountTransactionType.TargetAccountTypeId;
        }

        private void OnMakePayment(PaymentType obj)
        {
            SubmitPayment(obj);
        }

        private bool CanClosePaymentScreen(string arg)
        {
            return string.IsNullOrEmpty(_tenderedValueViewModel.TenderedAmount) || (_paymentEditor.SelectedTicket != null && _paymentEditor.GetRemainingAmount() == 0);
        }

        private void OnClosePaymentScreen(string obj)
        {
            _orderSelectorViewModel.PersistTicket();
            _paymentEditor.Close();
        }

        private void SubmitPayment(PaymentType paymentType)
        {
            var paymentDueAmount = _tenderedValueViewModel.GetPaymentDueValue();
            var tenderedAmount = _tenderedValueViewModel.GetTenderedValue();

            if (Math.Abs(paymentDueAmount - _paymentEditor.GetRemainingAmount()) <= 0.01m)
                paymentDueAmount = _paymentEditor.GetRemainingAmount();

            if (tenderedAmount == 0 || Math.Abs(paymentDueAmount - tenderedAmount) <= 0.01m)
                tenderedAmount = paymentDueAmount;

            if (tenderedAmount <= paymentDueAmount)
            {
                SubmitPaymentAmount(paymentType, null, paymentDueAmount, tenderedAmount);
                return;
            }

            var changeTemplates = GetChangePaymentTypes(paymentType);
            if (changeTemplates.Count() < 2)
            {
                SubmitPaymentAmount(paymentType, changeTemplates.SingleOrDefault(), paymentDueAmount, tenderedAmount);
            }
            else
            {
                _changeTemplatesViewModel.Display(changeTemplates, tenderedAmount, paymentDueAmount, paymentType, _selectChangePaymentTypeCommand);
            }
        }

        private void OnSelectChangePaymentType(PaymentData paymentData)
        {
            SubmitPaymentAmount(paymentData.PaymentType, paymentData.ChangePaymentType,
                paymentData.PaymentDueAmount, paymentData.TenderedAmount);
        }

        private void SubmitPaymentAmount(PaymentType paymentType, ChangePaymentType changeTemplate, decimal paymentDueAmount, decimal tenderedAmount)
        {
            var returningAmount = _returningAmountViewModel.GetReturningAmount(tenderedAmount, paymentDueAmount, changeTemplate);

            if (changeTemplate == null) tenderedAmount -= returningAmount;

            _paymentEditor.UpdateTicketPayment(paymentType, changeTemplate, paymentDueAmount, tenderedAmount);
            _numberPadViewModel.LastTenderedAmount = (tenderedAmount / _paymentEditor.ExchangeRate).ToString("#,#0.00");
            _tenderedValueViewModel.UpdatePaymentAmount(_paymentEditor.GetRemainingAmount());

            if (returningAmount == 0 && _paymentEditor.GetRemainingAmount() == 0)
            {
                OnClosePaymentScreen("");
            }
            else
            {
                if (returningAmount > 0)
                {
                    _returningAmountViewModel.PublishEvent(EventTopicNames.Activate);
                }
                _orderSelectorViewModel.PersistSelectedItems();
                _numberPadViewModel.ResetValues();
                RaisePropertyChanged(() => SelectedTicketTitle);
            }
        }

        private IList<ChangePaymentType> GetChangePaymentTypes(PaymentType paymentType)
        {
            return _foreignCurrencyButtonsViewModel.ForeignCurrency == null
                ? new List<ChangePaymentType>()
                : _cacheService.GetChangePaymentTypes().Where(x => x.AccountTransactionType.TargetAccountTypeId == paymentType.AccountTransactionType.SourceAccountTypeId).ToList();
        }

        public void Prepare(Ticket selectedTicket)
        {
            _foreignCurrencyButtonsViewModel.ForeignCurrency = null;
            _paymentTotals.Model = selectedTicket;
            _paymentEditor.SelectedTicket = selectedTicket;
            _orderSelectorViewModel.UpdateTicket(selectedTicket);
            _numberPadViewModel.ResetValues();
            _numberPadViewModel.LastTenderedAmount = _tenderedValueViewModel.PaymentDueAmount;
            _numberPadViewModel.BalanceMode = false;
            _commandButtonsViewModel.Update();
            _foreignCurrencyButtonsViewModel.UpdateCurrencyButtons();

            RaisePropertyChanged(() => SelectedTicketTitle);
        }
    }
}