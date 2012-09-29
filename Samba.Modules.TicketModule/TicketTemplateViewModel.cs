using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using FluentValidation;
using Samba.Domain.Models.Accounts;
using Samba.Domain.Models.Settings;
using Samba.Domain.Models.Tickets;
using Samba.Localization.Properties;
using Samba.Presentation.Common.ModelBase;

namespace Samba.Modules.TicketModule
{
    [Export, PartCreationPolicy(CreationPolicy.NonShared)]
    class TicketTemplateViewModel : EntityViewModelBase<TicketTemplate>
    {
       
        private IEnumerable<Numerator> _numerators;
        public IEnumerable<Numerator> Numerators { get { return _numerators ?? (_numerators = Workspace.All<Numerator>()); } set { _numerators = value; } }

        public Numerator TicketNumerator { get { return Model.TicketNumerator; } set { Model.TicketNumerator = value; } }
        public Numerator OrderNumerator { get { return Model.OrderNumerator; } set { Model.OrderNumerator = value; } }

        private IEnumerable<AccountTransactionType> _accountTransactionTypes;
        public IEnumerable<AccountTransactionType> AccountTransactionTypes { get { return _accountTransactionTypes ?? (_accountTransactionTypes = Workspace.All<AccountTransactionType>()); } }

        public AccountTransactionType SaleTransactionType { get { return Model.SaleTransactionType; } set { Model.SaleTransactionType = value; } }

        public override string GetModelTypeString()
        {
            return Resources.TicketTemplate;
        }

        public override Type GetViewType()
        {
            return typeof(TicketTemplateView);
        }

        protected override AbstractValidator<TicketTemplate> GetValidator()
        {
            return new TicketTemplateValidator();
        }
    }

    internal class TicketTemplateValidator : EntityValidator<TicketTemplate>
    {
        public TicketTemplateValidator()
        {
            RuleFor(x => x.TicketNumerator).NotNull();
            RuleFor(x => x.OrderNumerator).NotNull();
            RuleFor(x => x.SaleTransactionType).NotNull();
            RuleFor(x => x.SaleTransactionType.DefaultSourceAccountId).GreaterThan(0).When(x => x.SaleTransactionType != null);
            RuleFor(x => x.TicketNumerator).NotEqual(x => x.OrderNumerator).When(x => x.TicketNumerator != null);
        }
    }
}