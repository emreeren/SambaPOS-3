using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using Samba.Domain.Models.Settings;
using Samba.Localization.Properties;
using Samba.Presentation.Common;
using Samba.Presentation.Common.Commands;
using Samba.Presentation.Common.Services;
using Samba.Presentation.Services;
using Samba.Presentation.Services.Common;
using Samba.Services;
using Samba.Services.Common;

namespace Samba.Modules.WorkperiodModule
{
    [Export]
    public class WorkPeriodsViewModel : ObservableObject
    {
        private readonly IWorkPeriodService _workPeriodService;
        private readonly IApplicationState _applicationState;
        private readonly ITicketServiceBase _ticketService;

        [ImportingConstructor]
        public WorkPeriodsViewModel(IWorkPeriodService workPeriodService, IApplicationState applicationState, ITicketServiceBase ticketService)
        {
            _workPeriodService = workPeriodService;
            _applicationState = applicationState;
            _ticketService = ticketService;

            StartOfDayCommand = new CaptionCommand<string>(Resources.StartWorkPeriod, OnStartOfDayExecute, CanStartOfDayExecute);
            EndOfDayCommand = new CaptionCommand<string>(Resources.EndWorkPeriod, OnEndOfDayExecute, CanEndOfDayExecute);
            DisplayStartOfDayScreenCommand = new CaptionCommand<string>(Resources.StartWorkPeriod, OnDisplayStartOfDayScreenCommand, CanStartOfDayExecute);
            DisplayEndOfDayScreenCommand = new CaptionCommand<string>(Resources.EndWorkPeriod, OnDisplayEndOfDayScreenCommand, CanEndOfDayExecute);
            CancelCommand = new CaptionCommand<string>(Resources.Cancel, OnCancel);
        }

        private IEnumerable<WorkPeriodViewModel> _workPeriods;
        public IEnumerable<WorkPeriodViewModel> WorkPeriods
        {
            get
            {
                return _workPeriods ?? (_workPeriods = _workPeriodService.GetLastWorkPeriods(30)
                                                           .OrderByDescending(x => x.Id)
                                                           .Select(x => new WorkPeriodViewModel(x)));
            }
        }

        public ICaptionCommand StartOfDayCommand { get; set; }
        public ICaptionCommand EndOfDayCommand { get; set; }
        public ICaptionCommand DisplayStartOfDayScreenCommand { get; set; }
        public ICaptionCommand DisplayEndOfDayScreenCommand { get; set; }
        public ICaptionCommand CancelCommand { get; set; }

        public WorkPeriod LastWorkPeriod { get { return _applicationState.CurrentWorkPeriod; } }

        public TimeSpan WorkPeriodTime { get; set; }

        private int _openTicketCount;
        public int OpenTicketCount
        {
            get { return _openTicketCount; }
            set { _openTicketCount = value; RaisePropertyChanged(() => OpenTicketCount); }
        }

        private string _openTicketLabel;
        public string OpenTicketLabel
        {
            get { return _openTicketLabel; }
            set { _openTicketLabel = value; RaisePropertyChanged(() => OpenTicketLabel); }
        }

        private int _activeScreen;
        public int ActiveScreen
        {
            get { return _activeScreen; }
            set { _activeScreen = value; RaisePropertyChanged(() => ActiveScreen); }
        }

        public string StartDescription { get; set; }
        public string EndDescription { get; set; }

        public string LastEndOfDayLabel
        {
            get
            {
                if (_applicationState.IsCurrentWorkPeriodOpen)
                {
                    var title1 = string.Format(Resources.WorkPeriodStartDate_f, LastWorkPeriod.StartDate.ToShortDateString());
                    var title2 = string.Format(Resources.WorkPeriodStartTime, LastWorkPeriod.StartDate.ToShortTimeString());
                    var title3 = string.Format(Resources.TotalWorkTimeDays_f, WorkPeriodTime.Days, WorkPeriodTime.Hours, WorkPeriodTime.Minutes);
                    if (WorkPeriodTime.Days == 0) title3 = string.Format(Resources.TotalWorkTimeHours_f, WorkPeriodTime.Hours, WorkPeriodTime.Minutes);
                    if (WorkPeriodTime.Hours == 0) title3 = string.Format(Resources.TotalWorkTimeMinutes_f, WorkPeriodTime.TotalMinutes);
                    return string.Format("{0}\r\n{1}\r\n{2}", title1, title2, title3);
                }
                return Resources.StartWorkPeriodToEnablePos;
            }
        }

        private void OnCancel(string obj)
        {
            ActiveScreen = 0;
        }

        private void OnDisplayEndOfDayScreenCommand(string obj)
        {
            ActiveScreen = 2;
        }

        private void OnDisplayStartOfDayScreenCommand(string obj)
        {
            ActiveScreen = 1;
        }

        private bool CanEndOfDayExecute(string arg)
        {
            return _applicationState.ActiveAppScreen == AppScreens.WorkPeriods
                && OpenTicketCount == 0
                && WorkPeriodTime.TotalMinutes > 1
                && _applicationState.IsCurrentWorkPeriodOpen;
        }

        private void OnEndOfDayExecute(string obj)
        {
            _applicationState.NotifyEvent(RuleEventNames.BeforeWorkPeriodEnds, new { WorkPeriod = _applicationState.CurrentWorkPeriod });
            var success = _workPeriodService.StopWorkPeriod(EndDescription);
            if (success)
            {
                Refresh();
                _applicationState.CurrentWorkPeriod.PublishEvent(EventTopicNames.WorkPeriodStatusChanged);
                _applicationState.NotifyEvent(RuleEventNames.WorkPeriodEnds, new { WorkPeriod = _applicationState.CurrentWorkPeriod });
                InteractionService.UserIntraction.GiveFeedback(Resources.WorkPeriodEndsMessage);
            }
            else
            {
                InteractionService.UserIntraction.GiveFeedback(Resources.CantComplateOperationError);
            }
            EventServiceFactory.EventService.PublishEvent(EventTopicNames.ActivateNavigation);
        }

        private bool CanStartOfDayExecute(string arg)
        {
            return _applicationState.ActiveAppScreen == AppScreens.WorkPeriods
                && (LastWorkPeriod == null || LastWorkPeriod.StartDate != LastWorkPeriod.EndDate);
        }

        private void OnStartOfDayExecute(string obj)
        {
            var success = _workPeriodService.StartWorkPeriod(StartDescription);
            if (success)
            {
                Refresh();
                _applicationState.CurrentWorkPeriod.PublishEvent(EventTopicNames.WorkPeriodStatusChanged);
                _applicationState.NotifyEvent(RuleEventNames.WorkPeriodStarts, new { WorkPeriod = _applicationState.CurrentWorkPeriod });
                InteractionService.UserIntraction.GiveFeedback(Resources.StartingWorkPeriodCompleted);
            }
            else
            {
                InteractionService.UserIntraction.GiveFeedback(Resources.CantComplateOperationError);
            }
            EventServiceFactory.EventService.PublishEvent(EventTopicNames.ActivateNavigation);
        }

        public void Refresh()
        {
            _workPeriods = null;
            if (LastWorkPeriod != null)
                WorkPeriodTime = new TimeSpan(DateTime.Now.Ticks - LastWorkPeriod.StartDate.Ticks);

            OpenTicketCount = _ticketService.GetOpenTicketCount();

            if (OpenTicketCount > 0)
            {
                OpenTicketLabel = string.Format(Resources.ThereAreOpenTicketsWarning_f,
                                  OpenTicketCount);
            }
            else OpenTicketLabel = "";

            RaisePropertyChanged(() => WorkPeriods);
            RaisePropertyChanged(() => LastEndOfDayLabel);
            RaisePropertyChanged(() => WorkPeriods);

            StartDescription = "";
            EndDescription = "";

            ActiveScreen = 0;
        }
    }
}
