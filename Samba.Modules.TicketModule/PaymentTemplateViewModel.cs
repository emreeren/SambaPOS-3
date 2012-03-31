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
    class PaymentTemplateViewModel : EntityViewModelBase<PaymentTemplate>
    {
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
                    ? Workspace.All<Account>(x => x.AccountTemplateId == AccountTransactionTemplate.TargetAccountTemplateId).ToList()
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

        public string ButtonColor { get { return Model.ButtonColor; } set { Model.ButtonColor = value; } }
        public bool DisplayAtPaymentScreen { get { return Model.DisplayAtPaymentScreen; } set { Model.DisplayAtPaymentScreen = value; } }
        public bool DisplayUnderTicket { get { return Model.DisplayUnderTicket; } set { Model.DisplayUnderTicket = value; } }

        public override Type GetViewType()
        {
            return typeof(PaymentTemplateView);
        }

        public override string GetModelTypeString()
        {
            return Resources.PaymentTemplate;
        }

        protected override AbstractValidator<PaymentTemplate> GetValidator()
        {
            return new PaymentTemplateValidator();
        }
    }

    internal class PaymentTemplateValidator : EntityValidator<PaymentTemplate>
    {
        public PaymentTemplateValidator()
        {
            RuleFor(x => x.AccountTransactionTemplate).NotNull();
            RuleFor(x => x.Account).NotNull();
        }
    }
}
