using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Samba.Domain.Models.Accounts;
using Samba.Domain.Models.Resources;

namespace Samba.Services
{
    public interface IAccountService
    {
        int GetAccountCount();
        void CreateNewTransactionDocument(Account account, AccountTransactionDocumentTemplate documentTemplate, string description, decimal amount);
        decimal GetAccountBalance(int accountId);
        string GetCustomData(Resource account, string fieldName);
        string GetDescription(AccountTransactionDocumentTemplate documentTemplate, Account account);
        decimal GetDefaultAmount(AccountTransactionDocumentTemplate documentTemplate, Account account);
        void UpdateAccountState(int accountId, int stateId);
    }
}
