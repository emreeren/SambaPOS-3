using System;
using System.ComponentModel.Composition;
using System.Linq;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Timers;
using System.Windows.Input;
using System.Windows.Threading;
using Samba.Domain.Models.Tickets;
using Samba.Localization.Properties;
using Samba.Persistance.Data;
using Samba.Presentation.Common;
using Samba.Services;

namespace Samba.Modules.TicketModule
{
    [Export]
    public class TicketExplorerViewModel : ObservableObject
    {
        private readonly Timer _timer;
        private readonly ITicketService _ticketService;
        private readonly IApplicationState _applicationState;

        private readonly IUserService _userService;

        [ImportingConstructor]
        public TicketExplorerViewModel(ITicketService ticketService, IUserService userService, IApplicationState applicationState)
        {
            ResetFilters();
            _ticketService = ticketService;
            _userService = userService;
            _applicationState = applicationState;

            _timer = new Timer(250);
            _timer.Elapsed += TimerElapsed;

            CloseCommand = new CaptionCommand<string>(Resources.Close, OnCloseCommandExecuted);
            PreviousWorkPeriod = new CaptionCommand<string>("<<", OnExecutePreviousWorkPeriod);
            NextWorkPeriod = new CaptionCommand<string>(">>", OnExecuteNextWorkPeriod);
            RefreshDatesCommand = new CaptionCommand<string>(Resources.Refresh, OnRefreshDates);
        }

        private IList<TicketExplorerFilter> _filters;
        public IList<TicketExplorerFilter> Filters
        {
            get { return _filters; }
            set
            {
                _filters = value;
                RaisePropertyChanged(() => Filters);
            }
        }

        public ICaptionCommand PreviousWorkPeriod { get; set; }
        public ICaptionCommand NextWorkPeriod { get; set; }
        public ICaptionCommand RefreshDatesCommand { get; set; }
        public ICaptionCommand CloseCommand { get; set; }

        public bool CanChanageDateFilter { get { return _userService.IsUserPermittedFor(PermissionNames.DisplayOldTickets); } }

        private DateTime _startDate;
        public DateTime StartDate
        {
            get { return _startDate; }
            set
            {
                _startDate = value;
                EndDate = value;
                RaisePropertyChanged(() => StartDate);
            }
        }

        private DateTime _endDate;
        public DateTime EndDate
        {
            get { return _endDate; }
            set { _endDate = value; RaisePropertyChanged(() => EndDate); }
        }

        private IList<TicketExplorerRowViewModel> _tickets;
        public IList<TicketExplorerRowViewModel> Tickets
        {
            get { return _tickets; }
            set
            {
                _tickets = value;
                RaisePropertyChanged(() => Tickets);
            }
        }

        public TicketExplorerRowViewModel SelectedRow { get; set; }

        private decimal _total;
        public decimal Total
        {
            get { return _total; }
            set
            {
                _total = value;
                RaisePropertyChanged(() => Total);
                RaisePropertyChanged(() => TotalStr);
            }
        }

        public string TotalStr { get { return string.Format(Resources.Total_f, Total); } }

        public void Refresh()
        {
            EndDate = EndDate.Date.AddDays(1).AddMinutes(-1);
            Expression<Func<Ticket, bool>> qFilter = x => x.Date >= StartDate && x.Date < EndDate;
            qFilter = Filters.Aggregate(qFilter, (current, filter) => current.And(filter.GetExpression()));
            Tickets = Dao.Query(qFilter).Select(x => new TicketExplorerRowViewModel(x)).ToList();
            Total = Tickets.Sum(x => x.Sum);
            RaisePropertyChanged(() => CanChanageDateFilter);
        }

        void TimerElapsed(object sender, ElapsedEventArgs e)
        {
            _timer.Stop();
            AppServices.MainDispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(DisplaySelectedRow));
        }

        public void QueueDisplayTicket()
        {
            _timer.Stop();
            _timer.Start();
        }

        public void DisplaySelectedRow()
        {
            if (SelectedRow != null)
            {
                var ticket = _applicationState.CurrentTicket;
                if (_applicationState.CurrentTicket != null)
                    _ticketService.CloseTicket(ticket);
                ticket = _ticketService.OpenTicket(SelectedRow.Id);
                if (ticket != null)
                    EventServiceFactory.EventService.PublishEvent(EventTopicNames.RefreshSelectedTicket);
                CommandManager.InvalidateRequerySuggested();
            }
        }

        private static void OnCloseCommandExecuted(string obj)
        {
            EventServiceFactory.EventService.PublishEvent(EventTopicNames.DisplayTicketView);
        }

        private void OnRefreshDates(string obj)
        {
            Refresh();
        }

        private void OnExecuteNextWorkPeriod(string obj)
        {
            StartDate = StartDate.Date.AddDays(1);
            EndDate = StartDate;
            Refresh();
        }

        private void OnExecutePreviousWorkPeriod(string obj)
        {
            StartDate = StartDate.Date.AddDays(-1);
            EndDate = StartDate;
            Refresh();
        }

        public void ResetFilters()
        {
            var item = new TicketExplorerFilter();
            try
            {
                item.FilterType = FilterType.OpenTickets;
                Filters = new List<TicketExplorerFilter> { item };
            }
            finally
            {
                item.Dispose();
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_timer != null)
                    _timer.Dispose();
            }
        }
    }
}
