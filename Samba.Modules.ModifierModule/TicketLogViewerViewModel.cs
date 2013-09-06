using System.Collections.Generic;
using System.ComponentModel.Composition;
using Samba.Domain.Models.Tickets;
using Samba.Localization.Properties;
using Samba.Presentation.Common;
using Samba.Presentation.Common.Commands;
using Samba.Presentation.Services.Common;

namespace Samba.Modules.ModifierModule
{
    [Export]
    public class TicketLogViewerViewModel : ObservableObject
    {

        public ICaptionCommand CloseCommand { get; set; }

        public TicketLogViewerViewModel()
        {
            CloseCommand = new CaptionCommand<string>(Resources.Close, OnClose);
        }

        private Ticket _selectedTicket;
        public Ticket SelectedTicket
        {
            get { return _selectedTicket; }
            set
            {
                _selectedTicket = value;
                RaisePropertyChanged(() => SelectedTicket);
                if (SelectedTicket != null)
                {
                    _logs = null;
                    RaisePropertyChanged(() => Logs);
                }
            }
        }

        private IEnumerable<TicketLogValue> _logs;
        public IEnumerable<TicketLogValue> Logs
        {
            get { return _logs ?? (_logs = SelectedTicket != null ? SelectedTicket.GetTicketLogValues() : null); }
            set { _logs = value; RaisePropertyChanged(() => Logs); }
        }

        private void OnClose(string obj)
        {
            SelectedTicket = null;
            EventServiceFactory.EventService.PublishEvent(EventTopicNames.ActivatePosView);
        }
    }
}
