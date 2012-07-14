using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using Samba.Domain.Models.Resources;
using Samba.Domain.Models.Tickets;
using Samba.Infrastructure.Data;
using Samba.Infrastructure.Settings;
using Samba.Localization.Properties;
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
            AddTicketCommand = new CaptionCommand<string>(string.Format(Resources.Add_f, Resources.Ticket), OnAddTicket);
        }

        private IEnumerable<TicketButtonViewModel> _tickets;
        public IEnumerable<TicketButtonViewModel> Tickets
        {
            get { return _tickets; }
        }

        public IEntity SelectedEntity { get; set; }
        public string ListName { get { return SelectedEntity != null ? SelectedEntity.Name : ""; } }
        public ICaptionCommand AddTicketCommand { get; set; }

        public string TotalRemainingAmountLabel { get { return _tickets != null ? Tickets.Sum(x => x.RemainingAmount).ToString(LocalSettings.DefaultCurrencyFormat) : ""; } }
        public int RowCount { get { return _tickets != null && _tickets.Count() > 8 ? _tickets.Count() : 8; } }

        private void OnAddTicket(string obj)
        {
            EventServiceFactory.EventService.PublishEvent(EventTopicNames.CreateTicket, true);
            var r = SelectedEntity as Resource;
            new EntityOperationRequest<Resource>(r, null).PublishEvent(EventTopicNames.ResourceSelected, true);
            EventServiceFactory.EventService.PublishEvent(EventTopicNames.RefreshSelectedTicket);
        }

        public void UpdateListByResource(Resource resource)
        {
            if (resource != null)
            {
                SelectedEntity = resource;
                _tickets = _ticketService.GetOpenTickets(resource.Id).Select(x => new TicketButtonViewModel(x, resource));
                Refresh();
            }
        }
        public void UpdateListByTicketTagGroup(TicketTagGroup tagGroup)
        {
            SelectedEntity = tagGroup;
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
