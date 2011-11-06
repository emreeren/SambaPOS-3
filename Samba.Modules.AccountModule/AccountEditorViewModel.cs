using System;
using Samba.Domain.Models.Accounts;
using Samba.Localization.Properties;
using Samba.Presentation.Common.ModelBase;

namespace Samba.Modules.AccountModule
{
    public class AccountEditorViewModel : EntityViewModelBase<Account>
    {
        public AccountEditorViewModel(Account model)
            : base(model)
        {
        }

        public override Type GetViewType()
        {
            return typeof(AccountEditorView);
        }

        public override string GetModelTypeString()
        {
            return Resources.Account;
        }

        public string PhoneNumber { get { return Model.PhoneNumber; } set { Model.PhoneNumber = value; } }
        public string Address { get { return Model.Address; } set { Model.Address = value; } }
        public string Note { get { return Model.Note; } set { Model.Note = value; } }
        public bool InternalAccount { get { return Model.InternalAccount; } set { Model.InternalAccount = value; } }
    }
}
