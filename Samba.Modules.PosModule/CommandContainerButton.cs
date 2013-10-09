using System;
using System.Collections.Generic;
using System.Linq;
using Samba.Domain.Models.Tickets;
using Samba.Presentation.Common;
using Samba.Services.Common;

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
            if (Values.Count > 0 && commandContainer.AutomationCommand.ToggleValues) SelectedValue = Values.ElementAt(0);
        }

        public AutomationCommandData CommandContainer { get { return _commandContainer; } }
        public string Color { get { return CommandContainer.AutomationCommand.Color; } }
        public string ButtonHeader { get { return CommandContainer.AutomationCommand.ButtonHeader ?? ""; } }
        public string Name { get { return CommandContainer.AutomationCommand.Name; } }
        public int FontSize { get { return CommandContainer.AutomationCommand.FontSize; } }
        public string SelectedValue { get; set; }
        public string Caption { get { return !string.IsNullOrEmpty(SelectedValue) ? SelectedValue : ButtonHeader; } }
        public string Display { get { return Caption.Replace("\\r", Environment.NewLine); } }
        public List<string> Values { get { return (CommandContainer.AutomationCommand.Values ?? "").Split('|').ToList(); } }

        public bool IsVisible
        {
            get
            {
                if (!_selectedTicket.IsLocked && _commandContainer.VisualBehaviour == 2) return false;
                if (_selectedTicket.Orders.Count == 0 && _commandContainer.VisualBehaviour == 4) return false;
                return true;
            }
        }
        public bool IsEnabled
        {
            get
            {
                if ((_selectedTicket.IsLocked || _selectedTicket.Orders.Count == 0) && _commandContainer.VisualBehaviour == 1) return false;
                if (_selectedTicket.Orders.Count > 0 && _commandContainer.VisualBehaviour == 3) return false;

                return true;
            }
        }

        public void NextValue()
        {
            if (Values.Count > 1 && _commandContainer.AutomationCommand.ToggleValues)
            {
                SelectedValue = GetNextValue();
            }
            RaisePropertyChanged(() => Display);
        }

        internal string GetNextValue()
        {
            return Values[(Values.IndexOf(SelectedValue) + 1) % Values.Count];
        }
    }
}