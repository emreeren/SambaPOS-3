using System.Collections.ObjectModel;
using System.ComponentModel.Composition;
using System.Linq;
using Microsoft.Practices.Prism.Commands;
using Samba.Domain.Models.Tickets;
using Samba.Localization.Properties;
using Samba.Presentation.Common;
using Samba.Presentation.Common.Commands;
using Samba.Presentation.Services;
using Samba.Presentation.Services.Common;
using Samba.Services.Common;

namespace Samba.Modules.PosModule
{
    [Export]
    public class TicketTagListViewModel : ObservableObject
    {
        private Ticket _selectedTicket;
        private readonly IApplicationState _applicationState;
        public DelegateCommand<TicketTagGroup> SelectionCommand { get; set; }
        public CaptionCommand<string> CloseCommand { get; set; }

        [ImportingConstructor]
        public TicketTagListViewModel(IApplicationState applicationState)
        {
            _applicationState = applicationState;
            SelectionCommand = new DelegateCommand<TicketTagGroup>(OnSelectTicketTagGroup);
            CloseCommand = new CaptionCommand<string>(Resources.Close, OnClose);
            TicketTagValueViewModels = new ObservableCollection<TicketTagValueViewModel>();
        }

        private void OnClose(string obj)
        {
            EventServiceFactory.EventService.PublishEvent(EventTopicNames.CloseTicketRequested);
        }

        private void OnSelectTicketTagGroup(TicketTagGroup obj)
        {
            var ticketTagData = new TicketTagData
            {
                TicketTagGroup = obj,
                Ticket = _selectedTicket
            };
            ticketTagData.PublishEvent(EventTopicNames.SelectTicketTag);
        }

        public ObservableCollection<TicketTagValueViewModel> TicketTagValueViewModels { get; set; }

        public void Update(Ticket selectedTicket)
        {
            _selectedTicket = selectedTicket;
            var tags = _applicationState.GetTicketTagGroups().Where(x => x.AskBeforeCreatingTicket).ToList();
            TicketTagValueViewModels.Clear();
            TicketTagValueViewModels.AddRange(tags.Select(x => new TicketTagValueViewModel(x, selectedTicket)));
            RaisePropertyChanged(() => RowCount);
            OnSelectTicketTagGroup(TicketTagValueViewModels.First().Model);
        }

        public int RowCount { get { return TicketTagValueViewModels.Count > 7 ? TicketTagValueViewModels.Count : 8; } }
    }

    public class TicketTagValueViewModel
    {
        private readonly TicketTagGroup _ticketTagGroup;
        private readonly Ticket _ticket;

        public TicketTagValueViewModel(TicketTagGroup ticketTagGroup, Ticket ticket)
        {
            _ticketTagGroup = ticketTagGroup;
            _ticket = ticket;
        }

        public CaptionCommand<TicketTagGroup> SelectionCommand { get; set; }
        public TicketTagGroup Model { get { return _ticketTagGroup; } }
        public string SelectedValue { get { return _ticket.GetTagValue(Model.Name); } }
        public string Caption { get { return _ticketTagGroup.Name + (!string.IsNullOrEmpty(SelectedValue) ? ": " + SelectedValue : ""); } }
        public string ButtonColor
        {
            get
            {
                return !string.IsNullOrEmpty(SelectedValue)
                           ? _ticketTagGroup.ButtonColorWhenTagSelected
                           : _ticketTagGroup.ButtonColorWhenNoTagSelected;
            }
        }
    }
}
