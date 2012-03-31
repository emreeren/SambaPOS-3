using System;
using System.ComponentModel.Composition;
using System.Linq;
using Samba.Domain.Models.Accounts;
using Samba.Localization.Properties;
using Samba.Presentation.Common.ModelBase;

namespace Samba.Modules.AccountModule.Dashboard
{
    [Export, PartCreationPolicy(CreationPolicy.NonShared)]
    public class AccountTemplateViewModel : EntityViewModelBase<AccountTemplate>
    {
        [ImportingConstructor]
        public AccountTemplateViewModel()
        {
        }

        public string[] FilterTypes { get { return new[] { Resources.All, Resources.Month, Resources.Week, Resources.WorkPeriod }; } }
        public string FilterType { get { return FilterTypes[Model.DefaultFilterType]; } set { Model.DefaultFilterType = FilterTypes.ToList().IndexOf(value); } }
        
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
