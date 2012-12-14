using System.Collections.Generic;
using System.Linq;
using Samba.Domain.Models.Tickets;
using Samba.Presentation.Common;
using Samba.Presentation.Common.Commands;
using Samba.Presentation.Services;
using Samba.Presentation.Services.Common;

namespace Samba.Modules.PosModule.Widgets.TicketExplorer
{
    public class TicketExplorerRowData : ObservableObject
    {
        private readonly ITicketService _ticketService;

        public TicketExplorerRowData(Ticket model, ITicketService ticketService)
        {
            _ticketService = ticketService;
            Model = model;
            DisplayTicketCommand = new CaptionCommand<string>("Display", OnDisplayTicket);
        }

        private void OnDisplayTicket(string obj)
        {
            ExtensionMethods.PublishIdEvent(Model.Id, EventTopicNames.DisplayTicket);
        }

        public ICaptionCommand DisplayTicketCommand { get; set; }

        public Ticket Model { get; set; }
        public int Id { get { return Model.Id; } }
        public string TicketNumber { get { return Model.TicketNumber; } }
        public string Date { get { return Model.Date.ToShortDateString(); } }
        public string CreationTime { get { return Model.Date.ToShortTimeString(); } }
        public string LastPaymentTime { get { return Model.LastPaymentDate.ToShortTimeString(); } }
        public decimal Sum { get { return Model.TotalAmount; } }
        public bool IsPaid { get { return Model.IsClosed; } }
        public string TimeInfo { get { return CreationTime != LastPaymentTime || IsPaid ? CreationTime + " - " + LastPaymentTime : CreationTime; } }
        public string TicketNote { get { return Model.Note; } }
        public string this[int templateId]
        {
            get
            {
                var resource = Model.TicketResources.FirstOrDefault(x => x.ResourceTypeId == templateId);
                return resource != null ? resource.ResourceName : "";
            }
        }

        public IEnumerable<string> Details { get; set; }

        public void UpdateDetails()
        {
            if (Details == null)
            {
                Details = _ticketService.GetOrders(Model.Id).Select(x => string.Format("{0:#} {1} {2}", x.Quantity, x.Description, x.GetVisiblePrice()));
                RaisePropertyChanged(() => Details);
            }
        }
    }
}