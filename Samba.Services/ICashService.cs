using System;
using System.Collections.Generic;
using Samba.Domain;
using Samba.Domain.Models.Settings;
using Samba.Domain.Models.Transactions;

namespace Samba.Services
{
    public class CashTransactionData
    {
        public string Name { get; set; }
        public DateTime Date { get; set; }
        public int PaymentType { get; set; }
        public int TransactionType { get; set; }
        public decimal Amount { get; set; }
        public string AccountName { get; set; }
    }

    public interface ICashService : IService
    {
        dynamic GetCurrentCashOperationData();
        void AddIncome(int accountId, decimal amount, string description, PaymentType paymentType);
        void AddExpense(int accountId, decimal amount, string description, PaymentType paymentType);
        void AddLiability(int accountId, decimal amount, string description);
        void AddReceivable(int accountId, decimal amount, string description);
        IEnumerable<CashTransaction> GetTransactions(WorkPeriod workPeriod);
        IEnumerable<CashTransactionData> GetTransactionsWithAccountData(WorkPeriod workPeriod);
    }
}