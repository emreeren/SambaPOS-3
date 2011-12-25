using System;
using Samba.Domain.Models.Menus;
using Samba.Localization.Properties;
using Samba.Presentation.Common.ModelBase;

namespace Samba.Modules.MenuModule
{
    public class TaxTemplateViewModel : EntityViewModelBase<TaxTemplate>
    {
        public string DisplayName
        {
            get
            {
                return string.Format("{0} - {1}", Name, (TaxIncluded ? Resources.Included : Resources.Excluded));
            }
        }

        public decimal Rate { get { return Model.Rate; } set { Model.Rate = value; } }

        public bool TaxIncluded { get { return Model.TaxIncluded; } set { Model.TaxIncluded = value; } }

        public override Type GetViewType()
        {
            return typeof(TaxTemplateView);
        }

        public override string GetModelTypeString()
        {
            return Resources.TaxTemplate;
        }
    }
}
