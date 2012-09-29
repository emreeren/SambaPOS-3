using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Samba.Domain.Models.Accounts;
using Samba.Domain.Models.Resources;

namespace Samba.Modules.AccountModule
{
    class DocumentCreationData
    {
        public DocumentCreationData(Account account,AccountTransactionDocumentType DocumentType)
        {
            Account = account;
            DocumentType = DocumentType;
        }

        public Account Account { get; set; }
        public AccountTransactionDocumentType DocumentType { get; set; }
    }
}
