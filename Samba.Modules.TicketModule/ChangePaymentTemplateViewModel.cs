using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using FluentValidation;
using Samba.Domain.Models.Accounts;
using Samba.Domain.Models.Tickets;
using Samba.Localization.Properties;
using Samba.Presentation.Common.ModelBase;

namespace Samba.Modules.TicketModule
{
    [Export, PartCreationPolicy(CreationPolicy.NonShared)]
    public class ChangePaymentTemplateViewModel : EntityViewModelBaseWithMap<ChangePaymentTemplate, ChangePaymentTemplateMap, AbstractMapViewModel<ChangePaymentTemplateMap>>
    {
        [ImportingConstructor]
        public ChangePaymentTemplateViewModel()
        {
        }

        private IEnumerable<AccountTransactionTemplate> _accountTransactionTemplates;
        public IEnumerable<AccountTransactionTemplate> AccountTransactionTemplates
        {
            get
            {
                return _accountTransactionTemplates ?? (_accountTransactionTemplates =
                    Workspace.All<AccountTransactionTemplate>().ToList());
            }
        }

        private IEnumerable<Account> _accounts;
        public IEnumerable<Account> Accounts
        {
            get
            {
                return _accounts ?? (_accounts =
                    AccountTransactionTemplate != null
                    ? Workspace.All<Account>(x => x.AccountTemplateId == AccountTransactionTemplate.SourceAccountTemplateId).ToList()
                    : new List<Account>());
            }
        }

        public AccountTransactionTemplate AccountTransactionTemplate
        {
            get { return Model.AccountTransactionTemplate; }
            set
            {
                Model.AccountTransactionTemplate = value;
                Account = null;
                _accounts = null;
                RaisePropertyChanged(() => Accounts);
            }
        }

        public Account Account
        {
            get { return Model.Account; }
            set
            {
                Model.Account = value;
                RaisePropertyChanged(() => Account);
            }
        }

        public override Type GetViewType()
        {
            return typeof(ChangePaymentTemplateView);
        }

        public override string GetModelTypeString()
        {
            return Resources.ChangePaymentTemplate;
        }

        protected override AbstractValidator<ChangePaymentTemplate> GetValidator()
        {
            return new ChangePaymentTemplateValidator();
        }

        protected override void Initialize()
        {
            base.Initialize();
            MapController = new MapController<ChangePaymentTemplateMap, AbstractMapViewModel<ChangePaymentTemplateMap>>(Model.ChangePaymentTemplateMaps, Workspace);
        }
    }

    internal class ChangePaymentTemplateValidator : EntityValidator<ChangePaymentTemplate>
    {
        public ChangePaymentTemplateValidator()
        {
            RuleFor(x => x.AccountTransactionTemplate).NotNull();
        }
    }
}
