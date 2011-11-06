using System;
using Samba.Domain.Models.Tickets;
using Samba.Localization.Properties;
using Samba.Presentation.Common.ModelBase;

namespace Samba.Modules.SettingsModule
{
    public class GiftReasonViewModel : EntityViewModelBase<Reason>
    {
        public GiftReasonViewModel(Reason model)
            : base(model)
        {
        }

        public override Type GetViewType()
        {
            return typeof(GiftReasonView);
        }

        public override string GetModelTypeString()
        {
            return Resources.GiftReason;
        }
    }
}
