using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Samba.Domain.Models.Accounts;
using Samba.Infrastructure.Data;
using Samba.Persistance.Data;
using Samba.Presentation.Common;

namespace Samba.Modules.AccountModule
{
    class AccountTransactionViewModel : ObservableObject
    {
        private readonly IWorkspace _workspace;
        public AccountTransaction Model { get; set; }

        public AccountTransactionViewModel(IWorkspace workspace, AccountTransaction model)
        {
            Model = model;
            _workspace = workspace;
            _accountTransactionTemplate = model != null ? model.AccountTransactionTemplate : null;
        }

        private IEnumerable<AccountTransactionTemplate> _accountTransactionTemplates;
        public IEnumerable<AccountTransactionTemplate> AccountTransactionTemplates
        {
            get { return _accountTransactionTemplates ?? (_accountTransactionTemplates = _workspace.All<AccountTransactionTemplate>()); }
        }

        private AccountTransactionTemplate _accountTransactionTemplate;
        public AccountTransactionTemplate AccountTransactionTemplate
        {
            get { return _accountTransactionTemplate; }
            set
            {
                _accountTransactionTemplate = value;
                if (Model == null) Model = AccountTransaction.Create(value);
            }
        }
    }
}
