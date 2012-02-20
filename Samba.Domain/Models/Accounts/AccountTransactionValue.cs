using System;
using Samba.Infrastructure.Data;

namespace Samba.Domain.Models.Accounts
{
    public class AccountTransactionValue : Entity
    {
        public AccountTransactionValue()
        {
            Date = DateTime.Now;
        }

        public int AccountId { get; set; }
        public int AccountTemplateId { get; set; }
        public DateTime Date { get; set; }
        public decimal Debit { get; set; }
        public decimal Credit { get; set; }
    }
}