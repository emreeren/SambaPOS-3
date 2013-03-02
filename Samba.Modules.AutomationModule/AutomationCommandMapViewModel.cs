﻿using System.Collections.Generic;
using Samba.Domain.Models.Automation;
using Samba.Localization.Properties;
using Samba.Presentation.Common.ModelBase;

namespace Samba.Modules.AutomationModule
{
    public class AutomationCommandMapViewModel : AbstractMapViewModel<AutomationCommandMap>
    {
        private readonly IList<string> _screens = new[] { Resources.Ticket, Resources.Payment, string.Format("{0}&{1}", Resources.Ticket, Resources.Payment), Resources.OrderLine };
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
