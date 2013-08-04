using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using System.Windows.Threading;
using Samba.Domain.Models.Accounts;
using Samba.Domain.Models.Tickets;
using Samba.Persistance;
using Samba.Presentation.Services;
using Samba.Services;

namespace Samba.Presentation.ViewModels
{
    [Export]
    public class AccountBalances
    {
        private readonly IApplicationState _applicationState;
        private readonly ICacheService _cacheService;
        private readonly IAccountDao _accountDao;

        [ImportingConstructor]
        public AccountBalances(IApplicationState applicationState, ICacheService cacheService, IAccountDao accountDao)
        {
            _applicationState = applicationState;
            _cacheService = cacheService;
            _accountDao = accountDao;
            Balances = new Dictionary<int, decimal>();
        }

        public Ticket SelectedTicket { get; set; }

        private void UpdateBalances()
        {
            Balances.Clear();
            if (SelectedTicket == null) return;
            foreach (var ticketEntity in SelectedTicket.TicketEntities)
            {
                if (ticketEntity.AccountId > 0)
                {
                    var entityType = _cacheService.GetEntityTypeById(ticketEntity.EntityTypeId);
                    if (_applicationState.GetPaymentScreenPaymentTypes().Any(x => x.AccountTransactionType.TargetAccountTypeId == entityType.AccountTypeId))
                    {
                        var balance = _accountDao.GetAccountBalance(ticketEntity.AccountId);
                        balance +=
                            SelectedTicket.Payments.Where(
                                x =>
                                x.Id == 0 &&
                                x.AccountTransaction.AccountTransactionValues.Any(
                                    y => y.AccountId == ticketEntity.AccountId)).Sum(x => x.Amount);
                        Balances.Add(ticketEntity.AccountId, balance);
                    }
                }
            }
        }

        public IDictionary<int, decimal> Balances { get; set; }

        public decimal GetAccountBalance(int accountId)
        {
            return Balances.ContainsKey(accountId) ? Balances[accountId] : 0;
        }

        public decimal GetActiveAccountBalance()
        {
            return Balances.Count == 1 ? Balances.First().Value : 0;
        }

        public bool ContainsActiveAccount()
        {
            return Balances.Count == 1;
        }

        public void Refresh()
        {
            UpdateBalances();
        }

        public void RefreshAsync(Action callBack)
        {
            var di = Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Background, new Action(UpdateBalances));
            di.Completed += (sender, e) => Application.Current.MainWindow.Dispatcher.Invoke(callBack);
        }

        public Account GetActiveAccount()
        {
            return Balances.Count == 1 ? _accountDao.GetAccountById(Balances.First().Key) : null;
        }

        public int GetActiveAccountId()
        {
            return ContainsActiveAccount() ? Balances.Keys.First() : 0;
        }
    }
}