using System;
using Samba.Domain.Models.Settings;
using Samba.Localization;
using Samba.Localization.Properties;
using Samba.Presentation.Common.ModelBase;

namespace Samba.Modules.SettingsModule
{
    public class NumeratorViewModel : EntityViewModelBase<Numerator>
    {
        [LocalizedDisplayName("Name")]
        public string NumeratorName
        {
            get { return Model.Name; }
            set { Model.Name = value; }
        }

        [LocalizedDisplayName("NumberFormat")]
        public string NumberFormat
        {
            get { return Model.NumberFormat; }
            set { Model.NumberFormat = value; }
        }

        public override Type GetViewType()
        {
            return typeof(GenericEntityView);
        }

        public override string GetModelTypeString()
        {
            return Resources.Numerator;
        }
    }
}
