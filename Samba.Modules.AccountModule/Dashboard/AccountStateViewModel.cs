using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Samba.Domain.Models.Accounts;
using Samba.Localization.Properties;
using Samba.Presentation.Common.ModelBase;

namespace Samba.Modules.AccountModule.Dashboard
{
    class AccountStateViewModel : EntityViewModelBase<AccountState>
    {
        public string Color { get { return Model.Color; } set { Model.Color = value; } }

        public override Type GetViewType()
        {
            return typeof(AccountStateView);
        }

        public override string GetModelTypeString()
        {
            return Resources.AccountState;
        }
    }
}
