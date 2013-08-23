using System.ComponentModel.Composition;
using System.Linq;
using Microsoft.Practices.Prism.Events;
using Samba.Domain.Models.Tickets;
using Samba.Localization.Properties;
using Samba.Presentation.Common.Services;
using Samba.Presentation.Services;
using Samba.Presentation.Services.Common;
using Samba.Presentation.ViewModels;

namespace Samba.Modules.PaymentModule
{
    [Export]
    public class PaymentEditor
    {
        private readonly IApplicationState _applicationState;
        private readonly ITicketService _ticketService;
        private readonly AccountBalances _accountBalances;
        private Ticket _selectedTicket;

        [ImportingConstructor]
        public PaymentEditor(IApplicationState applicationState, ITicketService ticketService, AccountBalances accountBalances)
        {
            _applicationState = applicationState;
            _ticketService = ticketService;
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

        public void UpdateTicketPayment(PaymentType paymentType, ChangePaymentType changeTemplate, decimal paymentDueAmount, decimal paidAmount, decimal tenderedAmount)
        {
            var paymentAccount = paymentType.Account ?? _ticketService.GetAccountForPayment(SelectedTicket, paymentType);

            if (paymentDueAmount > SelectedTicket.GetRemainingAmount() && paidAmount > SelectedTicket.GetRemainingAmount())
            {
                var account = _accountBalances.GetActiveAccount();
                if (account != null)
                {
                    var ticketAmount = SelectedTicket.GetRemainingAmount();
                    var accountAmount = paidAmount - ticketAmount;
                    var accountBalance = _accountBalances.GetAccountBalance(account.Id);
                    if (accountAmount > accountBalance) accountAmount = accountBalance;
                    if (ticketAmount > 0)
                        _ticketService.AddPayment(SelectedTicket, paymentType, paymentAccount, ticketAmount, tenderedAmount - accountAmount);
                    if (accountAmount > 0)
                        _ticketService.AddAccountTransaction(SelectedTicket, account, paymentAccount, accountAmount, ExchangeRate);
                }
                _accountBalances.Refresh();
            }
            else
            {
                _ticketService.AddPayment(SelectedTicket, paymentType, paymentAccount, paidAmount, tenderedAmount);
                if (paidAmount > paymentDueAmount && changeTemplate != null)
                {
                    _ticketService.AddChangePayment(SelectedTicket, changeTemplate, changeTemplate.Account,
                                                    paidAmount - paymentDueAmount);
                }
            }
        }

        public void UpdateCalculations()
        {
            _ticketService.RecalculateTicket(SelectedTicket);
            if (GetRemainingAmount() < 0)
            {
                foreach (var cSelector in _applicationState.GetCalculationSelectors().Where(x => !string.IsNullOrEmpty(x.ButtonHeader)))
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
