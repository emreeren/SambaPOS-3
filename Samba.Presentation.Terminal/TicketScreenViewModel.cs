using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Microsoft.Practices.Prism.Commands;
using Samba.Domain.Models.Tickets;
using Samba.Localization.Properties;
using Samba.Persistance.Data;
using Samba.Presentation.Common;
using Samba.Presentation.ViewModels;
using Samba.Services;

namespace Samba.Presentation.Terminal
{
    public delegate void TicketSelectionEventHandler(int selectedTicketId);

    public class TicketScreenViewModel : ObservableObject
    {
        public event TicketSelectionEventHandler TicketSelectedEvent;

        public DelegateCommand<OpenTicketViewModel> SelectTicketCommand { get; set; }
        public ICaptionCommand CreateNewTicketCommand { get; set; }

        public IEnumerable<OpenTicketViewModel> OpenTickets { get; set; }

        public TicketScreenViewModel()
        {
            SelectTicketCommand = new DelegateCommand<OpenTicketViewModel>(OnSelectTicket);
            CreateNewTicketCommand = new CaptionCommand<string>(Resources.NewTicket, OnCreateNewTicket);
        }

        private void OnCreateNewTicket(string obj)
        {
            InvokeTicketSelectedEvent(0);
        }

        public void InvokeTicketSelectedEvent(int ticketId)
        {
            TicketSelectionEventHandler handler = TicketSelectedEvent;
            if (handler != null) handler(ticketId);
        }

        private void OnSelectTicket(OpenTicketViewModel obj)
        {
            InvokeTicketSelectedEvent(obj.Id);
        }

        public void Refresh()
        {
            UpdateOpenTickets(AppServices.MainDataContext.SelectedDepartment);
            RaisePropertyChanged(() => OpenTickets);
        }

        public void UpdateOpenTickets(Department department)
        {
            Expression<Func<Ticket, bool>> prediction;

            if (department != null)
                prediction = x => !x.IsPaid && x.DepartmentId == department.Id;
            else
                prediction = x => !x.IsPaid;

            OpenTickets = Dao.Select(x => new OpenTicketViewModel
            {
                Id = x.Id,
                TicketNumber = x.TicketNumber,
                LocationName = x.LocationName,
                AccountName = x.AccountName,
                IsLocked = x.Locked
            }, prediction).OrderBy(x => x.Title);
        }
    }
}
