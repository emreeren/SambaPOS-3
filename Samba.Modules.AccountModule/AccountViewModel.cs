using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.Serialization;
using FluentValidation;
using Samba.Domain.Models.Accounts;
using Samba.Infrastructure;
using Samba.Localization.Properties;
using Samba.Presentation.Common.ModelBase;

namespace Samba.Modules.AccountModule
{
    [DataContract]
    public class CustomDataValue
    {
        [DataMember]
        public string Name { get; set; }
        [DataMember]
        public string Value { get; set; }

        public AccountCustomField CustomField { get; set; }
    }

    public class AccountViewModel : EntityViewModelBase<Account>
    {
        private IEnumerable<AccountTemplate> _accountTemplates;
        public IEnumerable<AccountTemplate> AccountTemplates
        {
            get { return _accountTemplates ?? (_accountTemplates = Workspace.All<AccountTemplate>()); }
        }

        public AccountTemplate AccountTemplate
        {
            get { return Model.AccountTemplate; }
            set
            {
                Model.AccountTemplate = value;
                _customData = null;
                RaisePropertyChanged(() => CustomData);
            }
        }

        private ObservableCollection<CustomDataValue> _customData;
        public ObservableCollection<CustomDataValue> CustomData
        {
            get { return _customData ?? (_customData = GetCustomData(Model.CustomData)); }
        }

        private ObservableCollection<CustomDataValue> GetCustomData(string customData)
        {
            var data = new ObservableCollection<CustomDataValue>();
            try
            {
                if (!string.IsNullOrWhiteSpace(customData))
                    data = JsonHelper.Deserialize<ObservableCollection<CustomDataValue>>(customData) ??
                    new ObservableCollection<CustomDataValue>();
            }
            finally
            {
                GenerateFields(data);
            }
            return data;
        }

        private void GenerateFields(ObservableCollection<CustomDataValue> data)
        {
            if (AccountTemplate != null)
            {
                foreach (var cf in AccountTemplate.AccountCustomFields)
                {
                    var customField = cf;
                    var d = data.FirstOrDefault(x => x.Name == customField.Name);
                    if (d == null) data.Add(new CustomDataValue { Name = cf.Name, CustomField = cf });
                    else d.CustomField = cf;
                }
            }
        }

        public override Type GetViewType()
        {
            return typeof(AccountView);
        }

        public override string GetModelTypeString()
        {
            return Resources.Account;
        }

        public string SearchString { get { return Model.SearchString; } set { Model.SearchString = value; } }

        protected override AbstractValidator<Account> GetValidator()
        {
            return new AccountValidator();
        }

        protected override void OnSave(string value)
        {
            Model.CustomData = JsonHelper.Serialize(_customData);
            base.OnSave(value);
        }
    }

    internal class AccountValidator : EntityValidator<Account>
    {
        public AccountValidator()
        {
            RuleFor(x => x.AccountTemplate).NotNull();
        }
    }
}
