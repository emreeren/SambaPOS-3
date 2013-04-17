using System.Collections.Generic;
using System.Linq;
using Samba.Domain.Models.Tickets;
using Samba.Presentation.Common;
using Samba.Services;

namespace Samba.Modules.TicketModule
{
    public class TicketExplorerRowData : ObservableObject
    {
        private readonly ITicketServiceBase _ticketService;

        public TicketExplorerRowData(Ticket model, ITicketServiceBase ticketService)
        {
            _ticketService = ticketService;
            Model = model;
        }

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
                var resource = Model.TicketEntities.FirstOrDefault(x => x.EntityTypeId == templateId);
                return resource != null ? resource.EntityName : "";
            }
        }

        public IEnumerable<string> Details { get; set; }

        public void UpdateDetails()
        {
            if (Details == null)
            {
                Details = _ticketService
                    .GetOrders(Model.Id)
                    .OrderBy(x => x.MenuItemName)
                    .Select(x => string.Format("{0:#} {1} {2}", x.Quantity, x.Description, x.GetVisiblePrice()));
                RaisePropertyChanged(() => Details);
            }
        }
    }
}