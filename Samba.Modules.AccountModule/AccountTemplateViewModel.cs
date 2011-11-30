using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Samba.Domain.Models.Accounts;
using Samba.Localization.Properties;
using Samba.Presentation.Common.ModelBase;

namespace Samba.Modules.AccountModule
{
    public class AccountTemplateViewModel : EntityViewModelBase<AccountTemplate>
    {
        public AccountTemplateViewModel(AccountTemplate model)
            : base(model)
        {
        }

        public override string GetModelTypeString()
        {
            return Resources.AccountTemplate;
        }

        public override Type GetViewType()
        {
            return typeof(AccountTemplateView);
        }
    }
}
