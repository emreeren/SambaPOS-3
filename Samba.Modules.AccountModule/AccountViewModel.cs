using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using FluentValidation;
using Samba.Domain.Models.Accounts;
using Samba.Infrastructure;
using Samba.Infrastructure.Data.BinarySerializer;
using Samba.Infrastructure.Data.Serializer;
using Samba.Localization.Properties;
using Samba.Presentation.Common.ModelBase;

namespace Samba.Modules.AccountModule
{
    public class CustomDataValue
    {
        public string Name { get; set; }
        public string Value { get; set; }
    }

    public class AccountViewModel : EntityViewModelBase<Account>
    {
        public AccountViewModel(Account model)
            : base(model)
        {

        }

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
                foreach (var cf in AccountTemplate.AccountCustomFields.Where(cf => data.FirstOrDefault(x => x.Name == cf.Name) == null))
                {
                    data.Add(new CustomDataValue { Name = cf.Name });
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
