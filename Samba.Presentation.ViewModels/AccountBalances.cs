using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using Samba.Domain.Models.Accounts;
using Samba.Domain.Models.Tickets;
using Samba.Services;

namespace Samba.Presentation.ViewModels
{
    [Export]
    public class AccountBalances
    {
        private readonly ICacheService _cacheService;
        private readonly IAccountService _accountService;

        [ImportingConstructor]
        public AccountBalances(ICacheService cacheService, IAccountService accountService)
        {
            _cacheService = cacheService;
            _accountService = accountService;
            Balances = new Dictionary<int, decimal>();
        }

        private Ticket _selectedTicket;
        public Ticket SelectedTicket
        {
            get { return _selectedTicket; }
            set
            {
                _selectedTicket = value;
                UpdateBalances();
            }
        }

        private void UpdateBalances()
        {
            Balances.Clear();
            foreach (var ticketResource in SelectedTicket.TicketResources)
            {
                if (ticketResource.AccountId > 0)
                {
                    var resourceType = _cacheService.GetResourceTypeById(ticketResource.ResourceTypeId);
                    if (_cacheService.GetPaymentScreenPaymentTypes().Any(x => x.AccountTransactionType.TargetAccountTypeId == resourceType.AccountTypeId))
                    {
                        var balance = _accountService.GetAccountBalance(ticketResource.AccountId);
                        Balances.Add(ticketResource.AccountId, balance);
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

        public Account GetActiveAccount()
        {
            return Balances.Count == 1 ? _accountService.GetAccountById(Balances.First().Key) : null;
        }

        public int GetActiveAccountId()
        {
            return ContainsActiveAccount() ? Balances.Keys.First() : 0;
        }
    }
}