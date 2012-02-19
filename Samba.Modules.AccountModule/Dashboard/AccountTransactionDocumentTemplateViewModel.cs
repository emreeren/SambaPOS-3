using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.Composition;
using System.Linq;
using Samba.Domain.Models.Accounts;
using Samba.Infrastructure.Data;
using Samba.Localization.Properties;
using Samba.Presentation.Common;
using Samba.Presentation.Common.ModelBase;
using Samba.Presentation.Common.Services;

namespace Samba.Modules.AccountModule.Dashboard
{
    [Export, PartCreationPolicy(CreationPolicy.NonShared)]
    class AccountTransactionDocumentTemplateViewModel : EntityViewModelBase<AccountTransactionDocumentTemplate>
    {
        [ImportingConstructor]
        public AccountTransactionDocumentTemplateViewModel()
        {
            AddTransactionTemplateCommand = new CaptionCommand<string>(string.Format(Resources.Add_f, Resources.AccountTransactionTemplate), OnAddTransactionTemplate);
            DeleteTransactionTemplateCommand = new CaptionCommand<string>(string.Format(Resources.Delete_f, Resources.AccountTransactionTemplate), OnDeleteTransactionTemplate);
        }

        public string ButtonHeader { get { return Model.ButtonHeader; } set { Model.ButtonHeader = value; } }
        public string ButtonColor { get { return Model.ButtonColor; } set { Model.ButtonColor = value; } }
        public string DefaultAmount { get { return Model.DefaultAmount; } set { Model.DefaultAmount = value; RaisePropertyChanged(() => DefaultAmount); } }
        public string DescriptionTemplate { get { return Model.DescriptionTemplate; } set { Model.DescriptionTemplate = value; } }

        private IEnumerable<string> _defaultAmounts;
        public IEnumerable<string> DefaultAmounts
        {
            get { return _defaultAmounts ?? (_defaultAmounts = GetDefaultAmounts()); }
        }

        public ICaptionCommand AddTransactionTemplateCommand { get; set; }
        public ICaptionCommand DeleteTransactionTemplateCommand { get; set; }

        public AccountTransactionTemplate SelectedTransactionTemplate { get; set; }

        private IEnumerable<AccountTemplate> _accountTemplates;
        public IEnumerable<AccountTemplate> AccountTemplates
        {
            get { return _accountTemplates ?? (_accountTemplates = Workspace.All<AccountTemplate>()); }
        }

        public AccountTemplate MasterAccountTemplate
        {
            get { return AccountTemplates.SingleOrDefault(x => x.Id == Model.MasterAccountTemplateId); }
            set
            {
                _defaultAmounts = null;
                Model.MasterAccountTemplateId = value.Id;
                RaisePropertyChanged(() => MasterAccountTemplate);
                RaisePropertyChanged(() => DefaultAmounts);
            }
        }

        private ObservableCollection<AccountTransactionTemplate> _transactionTemplates;
        public ObservableCollection<AccountTransactionTemplate> TransactionTemplates
        {
            get { return _transactionTemplates ?? (_transactionTemplates = new ObservableCollection<AccountTransactionTemplate>(Model.TransactionTemplates)); }
        }

        private void OnDeleteTransactionTemplate(string obj)
        {
            Model.TransactionTemplates.Remove(SelectedTransactionTemplate);
            TransactionTemplates.Remove(SelectedTransactionTemplate);
        }

        private void OnAddTransactionTemplate(string obj)
        {
            var selectedValues =
                InteractionService.UserIntraction.ChooseValuesFrom(Workspace.All<AccountTransactionTemplate>().ToList<IOrderable>(),
                Model.TransactionTemplates.ToList<IOrderable>(), Resources.TicketTags, string.Format(Resources.ChooseTagsForDepartmentHint, Model.Name),
                Resources.TicketTag, Resources.TicketTags);

            foreach (AccountTransactionTemplate selectedValue in selectedValues)
            {
                if (!Model.TransactionTemplates.Contains(selectedValue))
                    Model.TransactionTemplates.Add(selectedValue);
            }

            _transactionTemplates = null;
            RaisePropertyChanged(() => TransactionTemplates);
        }

        private IEnumerable<string> GetDefaultAmounts()
        {
            var result = new List<string> { string.Format("[{0}]", Resources.Balance) };
            if (MasterAccountTemplate != null)
            {
                result.AddRange(MasterAccountTemplate.AccountCustomFields.Select(x => string.Format("[:{0}]", x.Name)));
            }
            return result;
        }

        public override Type GetViewType()
        {
            return typeof(AccountTransactionDocumentTemplateView);
        }

        public override string GetModelTypeString()
        {
            return Resources.DocumentTemplate;
        }
    }
}
