using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Samba.Domain.Models.Settings;
using Samba.Localization;
using Samba.Localization.Properties;
using Samba.Presentation.Common.ModelBase;

namespace Samba.Modules.SettingsModule
{
    class ForeignCurrencyViewModel : EntityViewModelBase<ForeignCurrency>
    {
        [LocalizedDisplayName("Name")]
        public string NumeratorName
        {
            get { return Model.Name; }
            set { Model.Name = value; }
        }

        [LocalizedDisplayName("CurrencySymbol")]
        public string CurrencySymbol
        {
            get { return Model.CurrencySymbol; }
            set { Model.CurrencySymbol = value; }
        }

        [LocalizedDisplayName("ExchangeRate")]
        public decimal ExchangeRate
        {
            get { return Model.ExchangeRate; }
            set { Model.ExchangeRate = value; }
        }

        public override Type GetViewType()
        {
            return typeof(GenericEntityView);
        }

        public override string GetModelTypeString()
        {
            return Resources.ForeignCurrency;
        }
    }
}
