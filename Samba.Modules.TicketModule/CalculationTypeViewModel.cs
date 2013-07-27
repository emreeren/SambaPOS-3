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
    public class CalculationTypeViewModel : EntityViewModelBase<CalculationType>
    {
        private string[] _calculationMethods;
        public string[] CalculationMethods
        {
            get
            {
                return _calculationMethods ?? (_calculationMethods = new[] {
                    Resources.RateFromTicketAmount,
                    Resources.RateFromPreviousTemplate, 
                    Resources.FixedAmount,
                    Resources.FixedAmountFromTicketTotal,
                    Resources.RoundTicketTotal,
                    Resources.Custom
                });
            }
        }

        public string SelectedCalculationMethod { get { return CalculationMethods[CalculationMethod]; } set { CalculationMethod = Array.IndexOf(CalculationMethods, value); } }

        public int CalculationMethod { get { return Model.CalculationMethod; } set { Model.CalculationMethod = value; } }
        public decimal Amount { get { return Model.Amount; } set { Model.Amount = value; } }
        public decimal MaxAmount { get { return Model.MaxAmount; } set { Model.MaxAmount = value; } }
        public bool IncludeTax { get { return Model.IncludeTax; } set { Model.IncludeTax = value; } }
        public bool DecreaseAmount { get { return Model.DecreaseAmount; } set { Model.DecreaseAmount = value; } }
        public bool UsePlainSum { get { return Model.UsePlainSum; } set { Model.UsePlainSum = value; } }
        public bool ToggleCalculation { get { return Model.ToggleCalculation; } set { Model.ToggleCalculation = value; } }

        private IEnumerable<AccountTransactionType> _accountTransactionTypes;
        public IEnumerable<AccountTransactionType> AccountTransactionTypes { get { return _accountTransactionTypes ?? (_accountTransactionTypes = Workspace.All<AccountTransactionType>()); } }

        public AccountTransactionType AccountTransactionType { get { return Model.AccountTransactionType; } set { Model.AccountTransactionType = value; } }

        public override Type GetViewType()
        {
            return typeof(CalculationTypeView);
        }

        public override string GetModelTypeString()
        {
            return Resources.CalculationType;
        }

        protected override AbstractValidator<CalculationType> GetValidator()
        {
            return new CalculationTypeValidator();
        }
    }

    internal class CalculationTypeValidator : EntityValidator<CalculationType>
    {
        public CalculationTypeValidator()
        {
            RuleFor(x => x.AccountTransactionType).NotNull();
        }
    }
}
