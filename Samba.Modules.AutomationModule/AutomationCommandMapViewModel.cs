using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Samba.Domain.Models.Actions;
using Samba.Infrastructure.Data;
using Samba.Localization.Properties;
using Samba.Presentation.ViewModels;
using Samba.Services;

namespace Samba.Modules.AutomationModule
{
    public class AutomationCommandMapViewModel : AbstractMapViewModel
    {
        public AutomationCommandMap Model { get; set; }

        public AutomationCommandMapViewModel(AutomationCommandMap model, IUserService userService, IDepartmentService departmentService, ISettingService settingService)
            : base(model, userService, departmentService, settingService)
        {
            Model = model;
        }

        private readonly IList<string> _visualBehaviours = new[] { Resources.Default, "Disable when ticket locked", "Display when ticket locked", "Disable when ticket active", "Display when ticket active" };
        public IList<string> VisualBehaviours { get { return _visualBehaviours; } }
        public string VisualBehaviour { get { return VisualBehaviours[Model.VisualBehaviour]; } set { Model.VisualBehaviour = VisualBehaviours.IndexOf(value); } }

        private readonly IList<string> _screens = new[] { "Ticket Screen", "Payment Screen", "Ticket + Payment Screens" };
        public IList<string> Screens { get { return _screens; } }
        public string Screen { get { return Screens[ScreenId]; } set { ScreenId = Screens.IndexOf(value); } }

        public int ScreenId
        {
            get
            {
                if (Model.DisplayOnTicket && Model.DisplayOnPayment) return 2;
                return Model.DisplayOnPayment ? 1 : 0;
            }

            set
            {
                Model.DisplayOnPayment = value == 2 || value == 1;
                Model.DisplayOnTicket = value == 2 || value == 0;
            }
        }
    }
}
