using System;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Linq;
using System.Collections.Generic;
using System.Timers;
using Samba.Domain.Models.Entities;
using Samba.Localization.Properties;
using Samba.Persistance;
using Samba.Persistance.Common;
using Samba.Presentation.Common;
using Samba.Presentation.Common.Commands;
using Samba.Presentation.Services;
using Samba.Presentation.Services.Common;
using Samba.Services;

namespace Samba.Modules.TicketModule
{
    [Export]
    public class TicketExplorerViewModel : ObservableObject
    {
        private readonly Timer _timer;
        private readonly ITicketServiceBase _ticketServiceBase;
        private readonly IUserService _userService;
        private readonly ICacheService _cacheService;
        private readonly IApplicationState _applicationState;

        public ICaptionCommand DisplayTicketCommand { get; set; }

        [ImportingConstructor]
        public TicketExplorerViewModel(ITicketServiceBase ticketServiceBase,
            IUserService userService, ICacheService cacheService, IApplicationState applicationState)
        {
            _ticketServiceBase = ticketServiceBase;
            _userService = userService;
            _cacheService = cacheService;
            _applicationState = applicationState;

            ResetFilters();

            _timer = new Timer(250);
            _timer.Elapsed += TimerElapsed;

            CloseCommand = new CaptionCommand<string>(Resources.Close, OnCloseCommandExecuted);
            PreviousWorkPeriod = new CaptionCommand<string>("<<", OnExecutePreviousWorkPeriod);
            NextWorkPeriod = new CaptionCommand<string>(">>", OnExecuteNextWorkPeriod);
            RefreshDatesCommand = new CaptionCommand<string>(Resources.Refresh, OnRefreshDates);
            DisplayTicketCommand = new CaptionCommand<string>(Resources.Display, OnDisplayTicket);
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
        public IEnumerable<EntityType> EntityTypes
        {
            get { return _cacheService.GetEntityTypes(); }
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

        public Action TicketAction { get; set; }

        private void OnDisplayTicket(string obj)
        {
            ExtensionMethods.PublishIdEvent(SelectedRow.Model.Id, EventTopicNames.DisplayTicket, TicketAction);
        }

        public void Refresh()
        {
            if (StartDate.Year == 1)
            {
                StartDate = DateTime.Today;
                EndDate = DateTime.Now;
            }
            var tickets = _ticketServiceBase.GetFilteredTickets(StartDate, EndDate, Filters);
            if (!_userService.IsUserPermittedFor(PermissionNames.DisplayOtherWaitersTickets))
                tickets = tickets.Where(x => x.LastModifiedUserName == _applicationState.CurrentLoggedInUser.Name);
            Tickets = tickets.Select(x => new TicketExplorerRowData(x, _ticketServiceBase)).ToList();
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
            Filters = _ticketServiceBase.CreateTicketExplorerFilters();
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
