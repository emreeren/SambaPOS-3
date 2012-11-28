using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Samba.Domain.Models.Automation;
using Samba.Infrastructure.Data;
using Samba.Localization.Properties;
using Samba.Presentation.Common.ModelBase;
using Samba.Presentation.ViewModels;
using Samba.Services;

namespace Samba.Modules.AutomationModule
{
    public class AutomationCommandMapViewModel : AbstractMapViewModel<AutomationCommandMap>
    {
        private readonly IList<string> _visualBehaviours = new[] { Resources.Default, "Disable when ticket locked", "Display when ticket locked", "Disable when ticket active", "Display when ticket active" };
        public IList<string> VisualBehaviours { get { return _visualBehaviours; } }
        public string VisualBehaviour { get { return VisualBehaviours[Model.VisualBehaviour]; } set { Model.VisualBehaviour = VisualBehaviours.IndexOf(value); } }

        private readonly IList<string> _screens = new[] { "Ticket Screen", "Payment Screen", "Ticket + Payment Screens", "Orders" };
        public IList<string> Screens { get { return _screens; } }
        public string Screen { get { return Screens[ScreenId]; } set { ScreenId = Screens.IndexOf(value); } }
        public string EnabledStates { get { return Model.EnabledStates; } set { Model.EnabledStates = value; } }
        public string VisibleStates { get { return Model.VisibleStates; } set { Model.VisibleStates = value; } }

        public int ScreenId
        {
            get
            {
                if (Model.DisplayOnOrders) return 3;
                if (Model.DisplayOnTicket && Model.DisplayOnPayment) return 2;
                return Model.DisplayOnPayment ? 1 : 0;
            }

            set
            {
                Model.DisplayOnPayment = value == 2 || value == 1;
                Model.DisplayOnTicket = value == 2 || value == 0;
                Model.DisplayOnOrders = value == 3;
            }
        }
    }
}
