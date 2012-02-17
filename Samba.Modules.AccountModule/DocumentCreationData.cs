using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Samba.Domain.Models.Accounts;

namespace Samba.Modules.AccountModule
{
    class DocumentCreationData
    {
        public DocumentCreationData(Account account,AccountTransactionDocumentTemplate documentTemplate)
        {
            Account = account;
            DocumentTemplate = documentTemplate;
        }

        public Account Account { get; set; }
        public AccountTransactionDocumentTemplate DocumentTemplate { get; set; }
    }
}
