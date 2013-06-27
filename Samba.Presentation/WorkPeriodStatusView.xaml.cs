using System;
using System.ComponentModel.Composition;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using Samba.Domain.Models.Settings;
using Samba.Presentation.Services;
using Samba.Presentation.Services.Common;

namespace Samba.Presentation
{
    /// <summary>
    /// Interaction logic for WorkPeriodStatusViewModel.xaml
    /// </summary>

    [Export]
    public partial class WorkPeriodStatusView : UserControl
    {
        private Timer _timer;
        private readonly IApplicationState _applicationState;

        [ImportingConstructor]
        public WorkPeriodStatusView(IApplicationState applicationState)
        {
            InitializeComponent();
            _applicationState = applicationState;
            Application.Current.MainWindow.Closing += MainWindow_Closing;
            EventServiceFactory.EventService.GetEvent<GenericEvent<WorkPeriod>>().Subscribe(OnWorkperiodStatusChanged);
        }

        void MainWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            _timer.Dispose();
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
                    if (_applicationState.IsCurrentWorkPeriodOpen)
                    {
                        var ts = new TimeSpan(DateTime.Now.Ticks - _applicationState.CurrentWorkPeriod.StartDate.Ticks);
                        tbWorkPeriodStatus.Visibility = ts.TotalHours > 24 ? System.Windows.Visibility.Visible : System.Windows.Visibility.Collapsed;
                    }
                    else tbWorkPeriodStatus.Visibility = System.Windows.Visibility.Collapsed;
                }
                catch (Exception)
                {
                    _timer.Change(Timeout.Infinite, Timeout.Infinite);
                }
            }));
        }
    }
}
