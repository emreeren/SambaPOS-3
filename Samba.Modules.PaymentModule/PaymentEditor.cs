using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using Microsoft.Practices.Prism.Events;
using Samba.Domain.Models.Accounts;
using Samba.Domain.Models.Settings;
using Samba.Domain.Models.Tickets;
using Samba.Localization.Properties;
using Samba.Presentation.Common.Services;
using Samba.Presentation.ViewModels;
using Samba.Services;
using Samba.Services.Common;

namespace Samba.Modules.PaymentModule
{
    [Export]
    public class PaymentEditor
    {
        private readonly ICacheService _cacheService;
        private readonly ITicketService _ticketService;
        private readonly IAccountService _accountService;
        private readonly AccountBalances _accountBalances;
        private Ticket _selectedTicket;

        [ImportingConstructor]
        public PaymentEditor(ICacheService cacheService, ITicketService ticketService, IAccountService accountService,
            AccountBalances accountBalances)
        {
            _cacheService = cacheService;
            _ticketService = ticketService;
            _accountService = accountService;
            _accountBalances = accountBalances;
            _selectedTicket = Ticket.Empty;

            EventServiceFactory.EventService.GetEvent<GenericEvent<EventAggregator>>().Subscribe(x =>
            {
                if (SelectedTicket != Ticket.Empty && x.Topic == EventTopicNames.CloseTicketRequested)
                {
                    SelectedTicket = Ticket.Empty;
                }
            });
        }

        public Ticket SelectedTicket
        {
            get { return _selectedTicket; }
            set
            {
                _selectedTicket = value;
                _accountBalances.SelectedTicket = value;
            }
        }

        public bool AccountMode { get; set; }
        public decimal ExchangeRate { get; set; }

        public decimal GetRemainingAmount()
        {
            return AccountMode && _accountBalances.ContainsActiveAccount()
                       ? SelectedTicket.GetRemainingAmount() + _accountBalances.GetActiveAccountBalance() - SelectedTicket.TransactionDocument.AccountTransactions.Where(x => x.ContainsAccountId(_accountBalances.GetActiveAccountId())).Sum(y => y.Amount)
                       : SelectedTicket.GetRemainingAmount();
        }

        public IEnumerable<ForeignCurrency> GetForeignCurrencies()
        {
            return _cacheService.GetForeignCurrencies();
        }

        public void UpdateTicketPayment(PaymentType paymentType, ChangePaymentType changeTemplate, decimal paymentDueAmount, decimal tenderedAmount)
        {
            var paymentAccount = paymentType.Account ?? GetAccountForTransaction(paymentType, SelectedTicket.TicketResources);

            if (paymentDueAmount > SelectedTicket.GetRemainingAmount() && tenderedAmount > SelectedTicket.GetRemainingAmount())
            {
                var account = _accountBalances.GetActiveAccount();
                if (account != null)
                {
                    var ticketAmount = SelectedTicket.GetRemainingAmount();
                    var accountAmount = tenderedAmount - ticketAmount;
                    var accountBalance = _accountBalances.GetAccountBalance(account.Id);
                    if (accountAmount > accountBalance) accountAmount = accountBalance;
                    if (ticketAmount > 0)
                        _ticketService.AddPayment(SelectedTicket, paymentType, paymentAccount, ticketAmount);
                    if (accountAmount > 0)
                        _ticketService.AddAccountTransaction(SelectedTicket, account, paymentAccount, accountAmount, ExchangeRate);
                }
                _accountBalances.Refresh();
            }
            else
            {
                _ticketService.AddPayment(SelectedTicket, paymentType, paymentAccount, tenderedAmount);
                if (tenderedAmount > paymentDueAmount && changeTemplate != null)
                {
                    _ticketService.AddChangePayment(SelectedTicket, changeTemplate, changeTemplate.Account,
                                                    tenderedAmount - paymentDueAmount);
                }
            }
        }

        private Account GetAccountForTransaction(PaymentType paymentType, IEnumerable<TicketResource> ticketResources)
        {
            var rt = _cacheService.GetResourceTypes().Where(
                x => x.AccountTypeId == paymentType.AccountTransactionType.TargetAccountTypeId).Select(x => x.Id);
            var tr = ticketResources.FirstOrDefault(x => rt.Contains(x.ResourceTypeId));
            return tr != null ? _accountService.GetAccountById(tr.AccountId) : null;
        }

        public void UpdateCalculations()
        {
            _ticketService.RecalculateTicket(SelectedTicket);
            if (GetRemainingAmount() < 0)
            {
                foreach (var cSelector in _cacheService.GetCalculationSelectors().Where(x => !string.IsNullOrEmpty(x.ButtonHeader)))
                {
                    foreach (var ctemplate in cSelector.CalculationTypes)
                    {
                        while (SelectedTicket.Calculations.Any(x => x.CalculationTypeId == ctemplate.Id))
                            SelectedTicket.AddCalculation(ctemplate, 0);
                    }
                }

                _ticketService.RecalculateTicket(SelectedTicket);
                if (GetRemainingAmount() >= 0)
                    InteractionService.UserIntraction.GiveFeedback(Resources.AllDiscountsRemoved);
            }
        }

        public void Close()
        {
            EventServiceFactory.EventService.PublishEvent(SelectedTicket.RemainingAmount > 0
                                                              ? EventTopicNames.RefreshSelectedTicket
                                                              : EventTopicNames.CloseTicketRequested);
        }
    }
}
