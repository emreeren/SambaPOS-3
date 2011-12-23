using System;
using System.ComponentModel;
using System.Globalization;
using System.Windows;
using System.ComponentModel.Composition;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Markup;
using System.Windows.Threading;
using Samba.Domain.Models.Users;
using Samba.Infrastructure.Settings;
using Samba.Presentation.Common;
using Samba.Services;

namespace Samba.Presentation
{
    /// <summary>
    /// Interaction logic for Shell.xaml
    /// </summary>

    [Export]
    public partial class Shell : Window
    {
        private readonly DispatcherTimer _timer;
        private readonly IApplicationState _applicationState;

        [ImportingConstructor]
        public Shell(IApplicationState applicationState)
        {
            _applicationState = applicationState;
            InitializeComponent();
            LanguageProperty.OverrideMetadata(
                                  typeof(FrameworkElement),
                                  new FrameworkPropertyMetadata(
                                      XmlLanguage.GetLanguage(CultureInfo.CurrentCulture.IetfLanguageTag)));

            var selectedIndexChange = DependencyPropertyDescriptor.FromProperty(Selector.SelectedIndexProperty, typeof(TabControl));

            selectedIndexChange.AddValueChanged(MainTabControl, MainTabControlSelectedIndexChanged);

            EventServiceFactory.EventService.GetEvent<GenericEvent<User>>().Subscribe(x =>
            {
                if (x.Topic == EventTopicNames.UserLoggedIn) UserLoggedIn(x.Value);
                if (x.Topic == EventTopicNames.UserLoggedOut) UserLoggedOut(x.Value);
            });

            EventServiceFactory.EventService.GetEvent<GenericEvent<UserControl>>().Subscribe(
                x =>
                {
                    if (x.Topic == EventTopicNames.DashboardClosed)
                    {
                        EventServiceFactory.EventService._PublishEvent(EventTopicNames.ResetCache);
                        //todo fix
                        AppServices.ResetCache();
                    }
                     
                });

            UserRegion.Visibility = Visibility.Collapsed;
            RightUserRegion.Visibility = Visibility.Collapsed;
            Height = Properties.Settings.Default.ShellHeight;
            Width = Properties.Settings.Default.ShellWidth;

            _timer = new DispatcherTimer();
            _timer.Tick += TimerTick;
            TimeLabel.Text = "...";

#if !DEBUG
            WindowStyle = WindowStyle.None;
            WindowState = WindowState.Maximized;
#endif
        }

        void TimerTick(object sender, EventArgs e)
        {
            var time = DateTime.Now.ToLongDateString() + " " + DateTime.Now.ToShortTimeString();
            TimeLabel.Text = TimeLabel.Text.Contains(":") ? time.Replace(":", " ") : time;
            MethodQueue.RunQueue();
        }

        private void MainTabControlSelectedIndexChanged(object sender, EventArgs e)
        {
            MoveFocus(new TraversalRequest(FocusNavigationDirection.Next));
        }

        public void UserLoggedIn(User user)
        {
            UserRegion.Visibility = Visibility.Visible;
            RightUserRegion.Visibility = Visibility.Visible;
        }

        public void UserLoggedOut(User user)
        {
            UserRegion.Visibility = Visibility.Collapsed;
            RightUserRegion.Visibility = Visibility.Collapsed;
        }

        private void TextBlockMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount == 2)
            {
                if (WindowStyle != WindowStyle.SingleBorderWindow)
                {
                    WindowStyle = WindowStyle.SingleBorderWindow;
                    WindowState = WindowState.Normal;
                }
                else
                {
                    WindowStyle = WindowStyle.None;
                    WindowState = WindowState.Maximized;
                }
            }
        }

        private void WindowClosing(object sender, CancelEventArgs e)
        {
            if (_applicationState.CurrentTicket != null)
            {
                e.Cancel = true;
                return;
            }

            if (WindowState == WindowState.Normal)
            {
                Properties.Settings.Default.ShellHeight = Height;
                Properties.Settings.Default.ShellWidth = Width;
            }
            Properties.Settings.Default.Save();
        }

        private void WindowLoaded(object sender, RoutedEventArgs e)
        {
            Title = Title + " [App: " + LocalSettings.AppVersion + "]";
            if (LocalSettings.CurrentDbVersion > 0)
                Title += " [DB: " + LocalSettings.DbVersion + "-" + LocalSettings.CurrentDbVersion + "]";
            _timer.Interval = TimeSpan.FromSeconds(1);
            _timer.Start();

            var source = HwndSource.FromHwnd(new WindowInteropHelper(this).Handle);
            if (source != null) source.AddHook(WndProc);
        }

        private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            if (msg == NativeWin32.WM_SHOWSAMBAPOS)
            {
                ShowMe();
            }
            return IntPtr.Zero;
        }

        private void ShowMe()
        {
            if (WindowState == WindowState.Minimized)
            {
                WindowState = WindowState.Normal;
            }
            var top = Topmost;
            Topmost = true;
            Topmost = top;
        }
    }
}
