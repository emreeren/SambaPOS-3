using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using Microsoft.Practices.Prism.Commands;
using Samba.Domain.Models.Tickets;
using Samba.Presentation.Common;
using Samba.Services;
using Samba.Services.Common;

namespace Samba.Modules.PosModule
{
    [Export]
    public class OpenTicketsViewModel : ObservableObject
    {
        private readonly Timer _timer;
        private readonly IApplicationState _applicationState;
        private readonly ITicketService _ticketService;
        private Department _department;

        public DelegateCommand<int?> OpenTicketCommand { get; set; }
        public int OpenTicketListViewColumnCount { get { return _department != null ? _department.OpenTicketViewColumnCount : 5; } }

        [ImportingConstructor]
        public OpenTicketsViewModel(IApplicationState applicationState, ITicketService ticketService)
        {
            _timer = new Timer(OnTimer, null, Timeout.Infinite, 1000);
            _applicationState = applicationState;
            _ticketService = ticketService;

            OpenTicketCommand = new DelegateCommand<int?>(OnOpenTicketExecute);

            EventServiceFactory.EventService.GetEvent<GenericEvent<Department>>().Subscribe(
                x =>
                {
                    if (x.Topic == EventTopicNames.ActivateOpenTickets)
                    {
                        _department = x.Value;
                        RaisePropertyChanged(() => OpenTicketListViewColumnCount);
                        UpdateOpenTickets(_department);
                    }
                });
        }

        public IEnumerable<OpenTicketButtonViewModel> OpenTickets { get; set; }

        private void OnTimer(object state)
        {
            if (_applicationState.ActiveAppScreen == AppScreens.TicketList && OpenTickets != null)
                foreach (var openTicketView in OpenTickets)
                {
                    openTicketView.Refresh();
                }
        }

        private void OnOpenTicketExecute(int? id)
        {
            if (id == null) return;
            ExtensionMethods.PublishIdEvent(id.GetValueOrDefault(0), EventTopicNames.DisplayTicket);
        }


        public void UpdateOpenTickets(Department department)
        {
            StopTimer();

            Expression<Func<Ticket, bool>> prediction = x => !x.IsPaid && x.DepartmentId == department.Id;

            var openTickets = _ticketService.GetOpenTickets(prediction);
            var shouldWrap = !department.IsTakeAway;

            OpenTickets = openTickets.Select(x => new OpenTicketButtonViewModel(x, shouldWrap)).OrderBy(x => x.LastOrderDate);

            RaisePropertyChanged(() => OpenTickets);

            StartTimer();
        }

        private void StartTimer()
        {
            if (_applicationState.ActiveAppScreen == AppScreens.TicketList)
                _timer.Change(60000, 60000);
        }

        private void StopTimer()
        {
            _timer.Change(Timeout.Infinite, 60000);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_timer != null) _timer.Dispose();
            }
        }
    }
}
