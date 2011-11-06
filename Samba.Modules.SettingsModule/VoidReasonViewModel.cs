using System;
using Samba.Domain.Models.Tickets;
using Samba.Localization.Properties;
using Samba.Presentation.Common.ModelBase;

namespace Samba.Modules.SettingsModule
{
    public class VoidReasonViewModel : EntityViewModelBase<Reason>
    {
        public VoidReasonViewModel(Reason model)
            : base(model)
        {

        }

        public override Type GetViewType()
        {
            return typeof(VoidReasonView);
        }

        public override string GetModelTypeString()
        {
            return Resources.VoidReason;
        }
    }
}
