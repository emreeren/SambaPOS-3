using System;
using System.Collections.ObjectModel;
using System.ComponentModel.Composition;
using System.Linq;
using Samba.Domain.Models.Accounts;
using Samba.Localization.Properties;
using Samba.Presentation.Common;
using Samba.Presentation.Common.ModelBase;

namespace Samba.Modules.AccountModule
{
    [Export, PartCreationPolicy(CreationPolicy.NonShared)]
    public class AccountTemplateViewModel : EntityViewModelBase<AccountTemplate>
    {
        [ImportingConstructor]
        public AccountTemplateViewModel()
        {
            AddCustomFieldCommand = new CaptionCommand<string>(string.Format(Resources.Add_f, Resources.CustomField), OnAddCustomField);
            DeleteCustomFieldCommand = new CaptionCommand<AccountCustomFieldViewModel>(string.Format(Resources.Delete_f, Resources.CustomField), OnDeleteCustomField, CanDeleteCustomField);
        }

        private bool CanDeleteCustomField(AccountCustomFieldViewModel arg)
        {
            return SelectedCustomField != null;
        }

        private void OnDeleteCustomField(AccountCustomFieldViewModel accountCustomFieldViewModel)
        {
            if (SelectedCustomField != null)
            {
                Model.AccountCustomFields.Remove(SelectedCustomField.Model);
                if (SelectedCustomField.Model.Id > 0)
                    Workspace.Delete(SelectedCustomField.Model);
                AccountCustomFields.Remove(SelectedCustomField);
            }
        }

        private void OnAddCustomField(string s)
        {
            var result = Model.AddCustomField(string.Format(Resources.New_f, Resources.CustomField), 0);
            AccountCustomFields.Add(new AccountCustomFieldViewModel(result));
        }

        public ICaptionCommand AddCustomFieldCommand { get; set; }
        public ICaptionCommand DeleteCustomFieldCommand { get; set; }

        public AccountCustomFieldViewModel SelectedCustomField { get; set; }

        private ObservableCollection<AccountCustomFieldViewModel> _accountCustomFields;
        public ObservableCollection<AccountCustomFieldViewModel> AccountCustomFields
        {
            get { return _accountCustomFields ?? (_accountCustomFields = new ObservableCollection<AccountCustomFieldViewModel>(Model.AccountCustomFields.Select(x => new AccountCustomFieldViewModel(x)))); }
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
