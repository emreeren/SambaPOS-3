using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using FluentValidation;
using Samba.Domain.Models.Accounts;
using Samba.Domain.Models.Tickets;
using Samba.Localization.Properties;
using Samba.Presentation.Common.ModelBase;

namespace Samba.Modules.TicketModule
{
    [Export, PartCreationPolicy(CreationPolicy.NonShared)]
    public class CalculationTemplateViewModel : EntityViewModelBase<CalculationTemplate>
    {
        private string[] _calculationMethods;
        public string[] CalculationMethods
        {
            get
            {
                return _calculationMethods ?? (_calculationMethods = new[] {
                    Resources.RateFromTicketAmount,
                    Resources.RateFromTaxIncludedTicketAmount, 
                    Resources.RateFromPreviousTemplate, 
                    Resources.FixedAmount });
            }
        }

        public string SelectedCalculationMethod { get { return CalculationMethods[CalculationMethod]; } set { CalculationMethod = Array.IndexOf(CalculationMethods, value); } }

        public int CalculationMethod { get { return Model.CalculationMethod; } set { Model.CalculationMethod = value; } }
        public decimal Amount { get { return Model.Amount; } set { Model.Amount = value; } }
        public string ButtonHeader { get { return Model.ButtonHeader; } set { Model.ButtonHeader = value; } }
        public string ButtonColor { get { return Model.ButtonColor; } set { Model.ButtonColor = value; } }
        public bool IncludeTax { get { return Model.IncludeTax; } set { Model.IncludeTax = value; } }
        public bool DecreaseAmount { get { return Model.DecreaseAmount; } set { Model.DecreaseAmount = value; } }

        private IEnumerable<AccountTransactionTemplate> _accountTransactionTemplates;
        public IEnumerable<AccountTransactionTemplate> AccountTransactionTemplates { get { return _accountTransactionTemplates ?? (_accountTransactionTemplates = Workspace.All<AccountTransactionTemplate>()); } }

        public AccountTransactionTemplate AccountTransactionTemplate { get { return Model.AccountTransactionTemplate; } set { Model.AccountTransactionTemplate = value; } }

        public override Type GetViewType()
        {
            return typeof(CalculationTemplateView);
        }

        public override string GetModelTypeString()
        {
            return Resources.CalculationTemplate;
        }

        protected override AbstractValidator<CalculationTemplate> GetValidator()
        {
            return new CalculationTemplateValidator();
        }
    }

    internal class CalculationTemplateValidator : EntityValidator<CalculationTemplate>
    {
        public CalculationTemplateValidator()
        {
            RuleFor(x => x.AccountTransactionTemplate).NotNull();
        }
    }
}
