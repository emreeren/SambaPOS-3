using System;
using System.Collections.Generic;
using Samba.Infrastructure.Data;

namespace Samba.Domain.Models.Accounts
{
    public class AccountTransactionValue : Entity
    {
        public AccountTransactionValue()
        {
            Date = DateTime.Now;
        }

        public int AccountTemplateId { get; set; }
        public int AccountId { get; set; }
        public DateTime Date { get; set; }
        public decimal Debit { get; set; }
        public decimal Credit { get; set; }

        public int AccountTransactionId { get; set; }
        public int AccountTransactionDocumentId { get; set; }
        //public bool IsSource { get; set; }
    }
}