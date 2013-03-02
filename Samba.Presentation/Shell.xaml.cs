﻿using System;
using System.ComponentModel;
using System.Globalization;
using System.Windows;
using System.ComponentModel.Composition;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Threading;
using Samba.Domain.Models.Users;
using Samba.Infrastructure.Settings;
using Samba.Presentation.Common;
using Samba.Presentation.Services;
using Samba.Presentation.Services.Common;
using Samba.Services.Common;

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
        private readonly IMethodQueue _methodQueue;

        [ImportingConstructor]
        public Shell(IApplicationState applicationState, IMethodQueue methodQueue)
        {

            _applicationState = applicationState;
            _methodQueue = methodQueue;
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
                if (x.Topic == EventTopicNames.UserLoggedOut) { UserLoggedOut(x.Value); }
            });

            EventServiceFactory.EventService.GetEvent<GenericEvent<UserControl>>().Subscribe(
                x =>
                {
                    if (x.Topic == EventTopicNames.DashboardClosed)
                    {
                        SerialPortService.ResetCache();
                        EventServiceFactory.EventService.PublishEvent(EventTopicNames.ResetCache, true);
                    }

                });



            UserRegion.Visibility = Visibility.Collapsed;
            RightUserRegion.Visibility = Visibility.Collapsed;
            Height = Properties.Settings.Default.ShellHeight;
            Width = Properties.Settings.Default.ShellWidth;

            if (Properties.Settings.Default.WindowScale > 0)
            {
                (MainGrid.LayoutTransform as ScaleTransform).ScaleX = Properties.Settings.Default.WindowScale;
                (MainGrid.LayoutTransform as ScaleTransform).ScaleY = Properties.Settings.Default.WindowScale;
            }

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
            _methodQueue.RunQueue();
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



        private void WindowClosing(object sender, CancelEventArgs e)
        {
            if (_applicationState.IsLocked)
            {
                e.Cancel = true;
                return;
            }

            if (WindowState == WindowState.Normal)
            {
                Properties.Settings.Default.ShellHeight = Height;
                Properties.Settings.Default.ShellWidth = Width;
            }

            Properties.Settings.Default.WindowScale = (MainGrid.LayoutTransform as ScaleTransform).ScaleX;
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

        private void TextBlockMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount == 2)
            {
                if (Keyboard.Modifiers == (ModifierKeys.Control | ModifierKeys.Shift))
                {
                    (MainGrid.LayoutTransform as ScaleTransform).ScaleX = 1;
                    (MainGrid.LayoutTransform as ScaleTransform).ScaleY = 1;
                    return;
                }

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

        private void UIElement_OnMouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (Keyboard.Modifiers != (ModifierKeys.Control | ModifierKeys.Shift)) return;
            //if ((Keyboard.Modifiers & ModifierKeys.Shift) == 0) return;
            //if ((Keyboard.Modifiers & ModifierKeys.Control) == 0) return;
            var val = e.Delta / 3000d;
            var sc = MainGrid.LayoutTransform as ScaleTransform;
            if (sc == null || sc.ScaleX + val < 0.05) return;
            sc.ScaleX += val;
            sc.ScaleY += val;
        }
    }
}
