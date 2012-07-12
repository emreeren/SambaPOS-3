using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using Samba.Domain.Models.Resources;
using Samba.Domain.Models.Tickets;
using Samba.Infrastructure.Settings;
using Samba.Presentation.Common;
using Samba.Services;
using Samba.Services.Common;

namespace Samba.Modules.PosModule
{
    [Export]
    public class TicketListViewModel : ObservableObject
    {
        private readonly ITicketService _ticketService;

        [ImportingConstructor]
        public TicketListViewModel(ITicketService ticketService)
        {
            _ticketService = ticketService;

        }

        private IEnumerable<TicketButtonViewModel> _tickets;
        public IEnumerable<TicketButtonViewModel> Tickets
        {
            get { return _tickets; }
        }

        public string ListName { get; set; }

        public string TotalRemainingAmountLabel { get { return _tickets != null ? Tickets.Sum(x => x.RemainingAmount).ToString(LocalSettings.DefaultCurrencyFormat) : ""; } }
        public int RowCount { get { return _tickets != null && _tickets.Count() > 8 ? _tickets.Count() : 8; } }

        public void UpdateListByResource(Resource resource)
        {
            if (resource != null)
            {
                ListName = resource.Name;
                _tickets = _ticketService.GetOpenTickets(resource.Id).Select(x => new TicketButtonViewModel(x, resource));
                Refresh();
            }
        }
        public void UpdateListByTicketTagGroup(TicketTagGroup tagGroup)
        {
            ListName = tagGroup.Name;
            var tagValue = string.Format("\"TagName\":\"{0}\"", tagGroup.Name);
            _tickets = _ticketService.GetOpenTickets(x => x.RemainingAmount > 0 && x.TicketTags.Contains(tagValue)).Select(x => new TicketButtonViewModel(x, null));
            Refresh();
        }

        private void Refresh()
        {
            RaisePropertyChanged(() => Tickets);
            RaisePropertyChanged(() => RowCount);
            RaisePropertyChanged(() => TotalRemainingAmountLabel);
            RaisePropertyChanged(() => ListName);
        }
    }
}
