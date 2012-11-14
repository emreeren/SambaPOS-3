using System;
using System.Windows;
using System.Windows.Interop;
using Samba.Presentation.Common;

namespace Samba.Presentation.Controls.Interaction
{
    /// <summary>
    /// Interaction logic for PopupWindow.xaml
    /// </summary>
    public partial class PopupWindow : Window
    {
        public PopupWindow()
        {
            InitializeComponent();
            Height = Application.Current.MainWindow.WindowState == WindowState.Normal
                ? SystemParameters.WorkArea.Bottom
                : SystemParameters.PrimaryScreenHeight-25;
            Width = 250;
            Top = 0;
            Left = SystemParameters.PrimaryScreenWidth - Width;
            Show();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            SetWindowStyle();
        }

        private void SetWindowStyle()
        {
            var hwnd = new WindowInteropHelper(this).Handle;
            const int gwlExstyle = (-20);
            const int wsExNoactivate = 0x08000000;
            const int wsExToolWindow = 0x00000080;
            NativeWin32.SetWindowLong(hwnd, gwlExstyle, (IntPtr)(wsExNoactivate | wsExToolWindow));
        }
    }
}
