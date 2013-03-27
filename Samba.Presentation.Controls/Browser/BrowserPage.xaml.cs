using System;
using System.Windows;
using System.Windows.Controls;

namespace Samba.Presentation.Controls.Browser
{
    /// <summary>
    /// Interaction logic for BrowserPage.xaml
    /// </summary>
    public partial class BrowserPage : Window
    {
        private bool _closing;

        public BrowserPage()
        {
            InitializeComponent();
            Closing += new System.ComponentModel.CancelEventHandler(BrowserPage_Closing);
        }

        void BrowserPage_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            _closing = true;
        }

        public void AssignTab(TabItem tab, ExtendedWebBrowser browserTab)
        {
            TabMain.Items.Add(tab);
            browserTab.Quit += browserTab_Quit;
            browserTab.DocumentTitleChanged += browserTab_DocumentTitleChanged;
            browserTab.WindowSetWidth += browserTab_WindowSetWidth;
            browserTab.WindowSetHeight += browserTab_WindowSetHeight;
            browserTab.WindowSetLeft += browserTab_WindowSetLeft;
            browserTab.WindowsSetTop += browserTab_WindowsSetTop;
            browserTab.DocumentCompleted += browserTab_DocumentCompleted;
        }

        void browserTab_WindowsSetTop(object sender, SizeChangedEventArgs e)
        {
            Top = e.Size;
        }

        void browserTab_WindowSetLeft(object sender, SizeChangedEventArgs e)
        {
            Left = e.Size;
        }

        void browserTab_DocumentCompleted(object sender, System.Windows.Forms.WebBrowserDocumentCompletedEventArgs e)
        {
            if (!IsVisible)
            {
                Show();
                (sender as ExtendedWebBrowser).Refresh();
            }
        }

        void browserTab_DocumentTitleChanged(object sender, EventArgs e)
        {
            try
            {
                Title = ((ExtendedWebBrowser)sender).DocumentTitle;
            }
            catch (Exception)
            {
            }
        }

        void browserTab_WindowSetHeight(object sender, SizeChangedEventArgs e)
        {
            Height = e.Size + 30;
        }

        void browserTab_WindowSetWidth(object sender, SizeChangedEventArgs e)
        {
            Width = e.Size + 10;
        }

        void browserTab_Quit(object sender, EventArgs e)
        {
            if (!_closing) Close();
        }
    }
}
