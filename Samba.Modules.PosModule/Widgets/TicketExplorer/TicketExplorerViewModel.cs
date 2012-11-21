using System;
using System.ComponentModel;
using System.Linq;
using System.Collections.Generic;
using System.Timers;
using Samba.Domain.Models.Resources;
using Samba.Localization.Properties;
using Samba.Persistance;
using Samba.Presentation.Common;
using Samba.Presentation.Common.Commands;
using Samba.Presentation.Services;
using Samba.Presentation.Services.Common;
using Samba.Services;

namespace Samba.Modules.PosModule.Widgets.TicketExplorer
{
    public class TicketExplorerViewModel : WidgetViewModel
    {
        private readonly Timer _timer;
        private readonly ITicketService _ticketService;
        private readonly IUserService _userService;
        private readonly ICacheService _cacheService;

        public TicketExplorerViewModel(Widget widget, IApplicationState applicationState, ITicketService ticketService,
            IUserService userService, ICacheService cacheService)
            : base(widget, applicationState)
        {
            _ticketService = ticketService;
            _userService = userService;
            _cacheService = cacheService;

            ResetFilters();

            _timer = new Timer(250);
            _timer.Elapsed += TimerElapsed;

            CloseCommand = new CaptionCommand<string>(Resources.Close, OnCloseCommandExecuted);
            PreviousWorkPeriod = new CaptionCommand<string>("<<", OnExecutePreviousWorkPeriod);
            NextWorkPeriod = new CaptionCommand<string>(">>", OnExecuteNextWorkPeriod);
            RefreshDatesCommand = new CaptionCommand<string>(Resources.Refresh, OnRefreshDates);
        }

        private IList<ITicketExplorerFilter> _filters;
        [Browsable(false)]
        public IList<ITicketExplorerFilter> Filters
        {
            get { return _filters; }
            set
            {
                _filters = value;
                RaisePropertyChanged(() => Filters);
            }
        }

        [Browsable(false)]
        public ICaptionCommand PreviousWorkPeriod { get; set; }
        [Browsable(false)]
        public ICaptionCommand NextWorkPeriod { get; set; }
        [Browsable(false)]
        public ICaptionCommand RefreshDatesCommand { get; set; }
        [Browsable(false)]
        public ICaptionCommand CloseCommand { get; set; }

        [Browsable(false)]
        public bool CanChanageDateFilter { get { return _userService.IsUserPermittedFor(PermissionNames.DisplayOldTickets); } }

        private DateTime _startDate;
        [Browsable(false)]
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
        [Browsable(false)]
        public DateTime EndDate
        {
            get { return _endDate; }
            set { _endDate = value; RaisePropertyChanged(() => EndDate); }
        }

        private IList<TicketExplorerRowData> _tickets;
        [Browsable(false)]
        public IList<TicketExplorerRowData> Tickets
        {
            get { return _tickets; }
            set
            {
                _tickets = value;
                RaisePropertyChanged(() => Tickets);
            }
        }

        [Browsable(false)]
        public IEnumerable<ResourceType> ResourceTypes
        {
            get { return _cacheService.GetResourceTypes(); }
        }

        [Browsable(false)]
        public TicketExplorerRowData SelectedRow { get; set; }

        private decimal _total;
        [Browsable(false)]
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

        [Browsable(false)]
        public string TotalStr { get { return string.Format("{0}: {1:N}", Resources.Total, Total); } }

        protected override object CreateSettingsObject()
        {
            return null;
        }

        public override void Refresh()
        {
            if (StartDate.Year == 1)
            {
                StartDate = DateTime.Today;
                EndDate = DateTime.Now;
            }
            Tickets = _ticketService.GetFilteredTickets(StartDate, EndDate, Filters).Select(
                    x => new TicketExplorerRowData(x, _ticketService)).ToList();
            Total = Tickets.Sum(x => x.Sum);
            RaisePropertyChanged(() => CanChanageDateFilter);
        }

        void TimerElapsed(object sender, ElapsedEventArgs e)
        {
            _timer.Stop();
            if (SelectedRow != null)
                SelectedRow.UpdateDetails();
        }

        public void QueueDisplayTicket()
        {
            _timer.Stop();
            _timer.Start();
        }

        private static void OnCloseCommandExecuted(string obj)
        {
            EventServiceFactory.EventService.PublishEvent(EventTopicNames.ActivatePosView);
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
            Filters = _ticketService.CreateTicketExplorerFilters();
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
