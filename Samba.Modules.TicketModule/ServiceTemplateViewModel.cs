using System;
using Samba.Domain.Models.Menus;
using Samba.Localization.Properties;
using Samba.Presentation.Common.ModelBase;

namespace Samba.Modules.TicketModule
{
    public class ServiceTemplateViewModel : EntityViewModelBase<ServiceTemplate>
    {
        private string[] _calculationMethods;
        public string[] CalculationMethods
        {
            get { return _calculationMethods ?? (_calculationMethods = new[] { Resources.RateFromTicketAmount,Resources.RateFromTaxIncludedTicketAmount, Resources.RateFromPreviousTemplate, Resources.FixedAmount }); }
        }

        public string SelectedCalculationMethod { get { return CalculationMethods[CalculationMethod]; } set { CalculationMethod = Array.IndexOf(CalculationMethods, value); } }

        public int CalculationMethod { get { return Model.CalculationMethod; } set { Model.CalculationMethod = value; } }
        public decimal Amount { get { return Model.Amount; } set { Model.Amount = value; } }

        public override Type GetViewType()
        {
            return typeof(ServiceTemplateView);
        }

        public override string GetModelTypeString()
        {
            return Resources.ServiceTemplate;
        }
    }
}
