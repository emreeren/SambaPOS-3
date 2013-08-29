using System.Collections.ObjectModel;
using System.ComponentModel.Composition;
using System.Linq;
using Microsoft.Practices.Prism.Commands;
using Samba.Domain.Models.Automation;
using Samba.Localization.Properties;
using Samba.Presentation.Common;
using Samba.Presentation.Common.Commands;
using Samba.Presentation.Services.Common;
using Samba.Presentation.ViewModels;

namespace Samba.Modules.ModifierModule
{
    [Export]
    public class AutomationCommandValueSelectorViewModel : ObservableObject
    {
        [ImportingConstructor]
        public AutomationCommandValueSelectorViewModel()
        {
            CloseCommand = new CaptionCommand<string>(Resources.Close, OnCloseCommandExecuted);
            AutomationCommandSelectedCommand = new DelegateCommand<string>(OnAutomationCommandValueSelected);
            CommandValues = new ObservableCollection<string>();
            EventServiceFactory.EventService.GetEvent<GenericEvent<AutomationCommand>>().Subscribe(OnAutomationCommandEvent);
        }

        private void OnAutomationCommandEvent(EventParameters<AutomationCommand> obj)
        {
            if (obj.Topic == EventTopicNames.SelectAutomationCommandValue)
            {
                CommandValues.Clear();
                SetSelectedAutomationCommand(obj.Value);
                CommandValues.AddRange(obj.Value.Values.Split('|'));
                if (CommandValues.Count == 1)
                {
                    OnAutomationCommandValueSelected(CommandValues.ElementAt(0));
                    return;
                }
                RaisePropertyChanged(() => ColumnCount);
            }
        }

        public AutomationCommand SelectedAutomationCommand { get; private set; }
        public ICaptionCommand CloseCommand { get; set; }
        public DelegateCommand<string> AutomationCommandSelectedCommand { get; set; }

        public ObservableCollection<string> CommandValues { get; set; }
        public int ColumnCount { get { return CommandValues.Count % 7 == 0 ? CommandValues.Count / 7 : (CommandValues.Count / 7) + 1; } }

        private static void OnCloseCommandExecuted(string obj)
        {
            EventServiceFactory.EventService.PublishEvent(EventTopicNames.ActivatePosView);
        }

        private void OnAutomationCommandValueSelected(string commandValue)
        {
            var automationCommandData = new AutomationCommandValueData
                                   {
                                       AutomationCommand = SelectedAutomationCommand,
                                       Value = commandValue
                                   };

            automationCommandData.PublishEvent(EventTopicNames.HandlerRequested, true);
        }

        private void SetSelectedAutomationCommand(AutomationCommand command)
        {
            SelectedAutomationCommand = command;
            RaisePropertyChanged(() => SelectedAutomationCommand);
        }
    }
}
