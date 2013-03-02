using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using Samba.Presentation.Common;

namespace Samba.Presentation.Controls.Interaction
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class KeyboardWindow : Window
    {
        public TextBox TargetTextBox { get; set; }

        public KeyboardWindow()
        {
            InitializeComponent();
            Top = Properties.Settings.Default.KeyboardTop;
            Left = Properties.Settings.Default.KeyboardLeft;
            Height = Properties.Settings.Default.KeyboardHeight;
            Width = Properties.Settings.Default.KeyboardWidth;
            if (Height <= 0) ResetWindowSize();
            else if ((Top + Height) > SystemParameters.PrimaryScreenHeight) ResetWindowSize();
            else if (Left > Application.Current.MainWindow.Left + Application.Current.MainWindow.Width) ResetWindowSize();
            else if (Left < 0) ResetWindowSize();
        }

        private void SetWindowStyle()
        {
            var hwnd = new WindowInteropHelper(this).Handle;
            const int gwlExstyle = (-20);
            const int wsExNoactivate = 0x08000000;
            const int wsExToolWindow = 0x00000080;
            NativeWin32.SetWindowLong(hwnd, gwlExstyle, (IntPtr)(wsExNoactivate | wsExToolWindow));
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            SetWindowStyle();
            var source = HwndSource.FromHwnd(new WindowInteropHelper(this).Handle);
            if (source != null) source.AddHook(WndProc);
        }

        public void ResetWindowSize()
        {
            if (Application.Current.MainWindow.WindowState == WindowState.Normal)
            {
                Height = Application.Current.MainWindow.Height / 2;
                Width = Application.Current.MainWindow.Width;
                Top = Application.Current.MainWindow.Top + Height;
                Left = Application.Current.MainWindow.Left;
            }
            else
            {
                Height = SystemParameters.PrimaryScreenHeight / 2;
                Width = SystemParameters.PrimaryScreenWidth;
                Top = (SystemParameters.PrimaryScreenHeight / 2) * 1;
                Left = 0;
            }
        }

        private static IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            if (msg == UnsafeNativeMethods.WM_MOVING || msg == UnsafeNativeMethods.WM_SIZING)
            {
                var m = new System.Windows.Forms.Message
                {
                    HWnd = hwnd,
                    Msg = msg,
                    WParam = wParam,
                    LParam = lParam,
                    Result = IntPtr.Zero
                };
                UnsafeNativeMethods.ReDrawWindow(m);
                handled = true;
            }

            if (msg == UnsafeNativeMethods.WM_MOUSEACTIVATE)
            {
                handled = true;
                return new IntPtr(UnsafeNativeMethods.MA_NOACTIVATE);
            }

            return IntPtr.Zero;
        }

        private void Border_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            DragMove();
        }

        private void Window_Unloaded(object sender, RoutedEventArgs e)
        {
            Properties.Settings.Default.KeyboardHeight = Height;
            Properties.Settings.Default.KeyboardWidth = Width;
            Properties.Settings.Default.KeyboardTop = Top;
            Properties.Settings.Default.KeyboardLeft = Left;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            HideKeyboard();
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            ResetWindowSize();
        }

        private void Window_Activated(object sender, EventArgs e)
        {
            TextBox.Focus();
        }

        private Thickness _oldBorderThickness;
        private Brush _oldBorderBrush;
        private Brush _oldForeground;
        private Brush _oldBackground;

        public void ShowKeyboard()
        {
            TargetTextBox = Keyboard.FocusedElement as TextBox;
            if (TargetTextBox != null)
            {
                TextBox.Text = TargetTextBox.Text;
                TextBox.SelectionStart = TargetTextBox.SelectionStart;
                TextBox.AcceptsReturn = TargetTextBox.AcceptsReturn;

                _oldBorderThickness = TargetTextBox.BorderThickness;
                TargetTextBox.BorderThickness = new Thickness(3);
                _oldBorderBrush = TargetTextBox.BorderBrush;
                TargetTextBox.BorderBrush = Brushes.Red;
                _oldForeground = TargetTextBox.Foreground;
                TargetTextBox.Foreground = SystemColors.ControlTextBrush;
                _oldBackground = TargetTextBox.Background;
                TargetTextBox.Background = SystemColors.ControlBrush;
            }
            Show();
            if (TargetTextBox != null)
            {
                Keyboard.Focus(TextBox);
                TextBox.SelectAll();
            }
        }

        public void HideKeyboard()
        {
            if (TargetTextBox != null)
            {
                TargetTextBox.Text = TextBox.Text;
                TargetTextBox.SelectionStart = TextBox.SelectionStart;
                TargetTextBox.BorderThickness = _oldBorderThickness;
                TargetTextBox.BorderBrush = _oldBorderBrush;
                TargetTextBox.Foreground = _oldForeground;
                TargetTextBox.Background = _oldBackground;
            }

            TextBox.Text = "";
            TargetTextBox = null;
            Properties.Settings.Default.KeyboardHeight = Height;
            Properties.Settings.Default.KeyboardWidth = Width;
            Properties.Settings.Default.KeyboardTop = Top;
            Properties.Settings.Default.KeyboardLeft = Left;
            Hide();
        }

        private void TextBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (!TextBox.AcceptsReturn && e.Key == Key.Enter)
            {
                e.Handled = true;
                HideKeyboard();
            }

            if (TargetTextBox != null && e.Key == Key.Tab)
            {
                var tb = TargetTextBox;

                e.Handled = true;
                HideKeyboard();
                Keyboard.Focus(tb);
                tb.Focus();
                tb.MoveFocus(new TraversalRequest(FocusNavigationDirection.Next));
                if (Keyboard.FocusedElement is TextBox) ShowKeyboard();
            }
        }

        private void Window_Deactivated(object sender, EventArgs e)
        {
            HideKeyboard();
        }
    }
}
