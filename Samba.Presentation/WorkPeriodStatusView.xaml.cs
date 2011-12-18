using System;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using Microsoft.Practices.ServiceLocation;
using Samba.Domain.Models.Settings;
using Samba.Presentation.Common;
using Samba.Services;

namespace Samba.Presentation
{
    /// <summary>
    /// Interaction logic for WorkPeriodStatusViewModel.xaml
    /// </summary>
    public partial class WorkPeriodStatusView : UserControl
    {
        private Timer _timer;
        private readonly IApplicationState _applicationState;
        
        public WorkPeriodStatusView()
        {
            InitializeComponent();
            _applicationState = ServiceLocator.Current.GetInstance<IApplicationState>();
            EventServiceFactory.EventService.GetEvent<GenericEvent<WorkPeriod>>().Subscribe(OnWorkperiodStatusChanged);
        }

        private void OnWorkperiodStatusChanged(EventParameters<WorkPeriod> obj)
        {
            if (obj.Topic == EventTopicNames.WorkPeriodStatusChanged)
            {
                _timer.Change(1, 60000);
            }
        }

        protected override void OnInitialized(EventArgs e)
        {
            _timer = new Timer(OnTimerTick, null, 30000, 60000);
            base.OnInitialized(e);
        }

        private void OnTimerTick(object state)
        {
            Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.ApplicationIdle, new Action(delegate
            {
                try
                {
                    if (_applicationState.ActiveAppScreen == AppScreens.LoginScreen) return;

                    //todo: fix
                    //if (AppServices.MainDataContext.IsCurrentWorkPeriodOpen)
                    //{
                    //    var ts = new TimeSpan(DateTime.Now.Ticks - AppServices.MainDataContext.CurrentWorkPeriod.StartDate.Ticks);
                    //    tbWorkPeriodStatus.Visibility = ts.TotalHours > 24 ? Visibility.Visible : Visibility.Collapsed;
                    //}
                    //else tbWorkPeriodStatus.Visibility = Visibility.Collapsed;
                }
                catch (Exception)
                {
                    _timer.Change(Timeout.Infinite, Timeout.Infinite);
                }
            }));
        }
    }
}
