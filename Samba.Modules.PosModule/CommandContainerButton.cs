using Samba.Domain.Models.Actions;
using Samba.Domain.Models.Tickets;
using Samba.Presentation.Common;
using Samba.Services;

namespace Samba.Modules.PosModule
{
    public class CommandContainerButton : ObservableObject
    {
        private readonly AutomationCommandData _commandContainer;
        private readonly Ticket _selectedTicket;

        public CommandContainerButton(AutomationCommandData commandContainer, Ticket selectedTicket)
        {
            _commandContainer = commandContainer;
            _selectedTicket = selectedTicket;
        }

        public AutomationCommandData CommandContainer { get { return _commandContainer; } }
        public string Color { get { return CommandContainer.AutomationCommand.Color; } }
        public string ButtonHeader { get { return CommandContainer.AutomationCommand.ButtonHeader; } }
        public string Name { get { return CommandContainer.AutomationCommand.Name; } }
        public bool IsVisible
        {
            get
            {
                if (!_selectedTicket.Locked && _commandContainer.VisualBehaviour == 2) return false;
                if (_selectedTicket.Id == 0 && _commandContainer.VisualBehaviour == 3) return false;
                 return true;
            }
        }
        public bool IsEnabled
        {
            get
            {
                if (_selectedTicket.Locked && _commandContainer.VisualBehaviour == 1) return false;
                return true;
            }
        }
    }
}