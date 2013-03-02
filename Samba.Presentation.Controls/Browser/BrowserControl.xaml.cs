﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms.Integration;
using System.Windows.Input;

namespace Samba.Presentation.Controls.Browser
{
    /// <summary>
    /// Interaction logic for BrowserControl.xaml
    /// </summary>
    public partial class BrowserControl
    {
        private readonly Dictionary<TabItem, ExtendedWebBrowser> _browserTabs = new Dictionary<TabItem, ExtendedWebBrowser>();
        private readonly List<TabItem> _tabQueue = new List<TabItem>();

        private string _activeUrl;
        public string ActiveUrl
        {
            get { return _activeUrl; }
            set
            {
                _activeUrl = value;
                Navigate(_activeUrl);
            }
        }

        public static readonly DependencyProperty ActiveUrlProperty =
            DependencyProperty.RegisterAttached("ActiveUrl", typeof(string), typeof(BrowserControl), new UIPropertyMetadata(null, ActiveUrlPropertyChanged));

        public static string GetActiveUrl(DependencyObject obj)
        {
            return (string)obj.GetValue(ActiveUrlProperty);
        }

        public static void SetActiveUrl(DependencyObject obj, string value)
        {
            obj.SetValue(ActiveUrlProperty, value);
        }

        public static void ActiveUrlPropertyChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
        {
            BrowserControl browser = o as BrowserControl;
            if (browser != null)
            {
                string uri = e.NewValue as string;
                browser.ActiveUrl = uri;
            }
        }

        public BrowserControl()
        {
            InitializeComponent();
            Loaded += BrowserControl_Loaded;
        }

        private void ControlShown()
        {
            edAddress.Focus();
            edAddress.SelectAll();
        }

        void BrowserControl_Loaded(object sender, RoutedEventArgs e)
        {
            ControlShown();
            if (_browserTabs.Count == 0)
            {
                CreateNewBrowserTab();
                ResizeToolbar(MainToolbar, edAddress);
            }
        }

        private TabItem CreateNewBrowserTab()
        {
            var t = new TabItem();
            tbMain.Items.Add(t);
            tbMain.SelectedItem = t;
            t.Header = "Yeni Sayfa";
            var host = new WindowsFormsHost();
            var b = new ExtendedWebBrowser { Tag = t, ScriptErrorsSuppressed = true };
            host.Child = b;

            t.Content = host;

            WrapBrowserEvents(b);

            _browserTabs.Add(t, b);
            _tabQueue.Add(t);
            return t;
        }

        private void WrapBrowserEvents(ExtendedWebBrowser browser)
        {
            browser.DocumentTitleChanged += browser_DocumentTitleChanged;
            browser.StartNewWindow += browser_StartNewWindow;
        }

        private void UnWrapBrowserEvents(ExtendedWebBrowser browser)
        {
            browser.DocumentTitleChanged -= browser_DocumentTitleChanged;
            browser.StartNewWindow -= browser_StartNewWindow;
        }

        void browser_StartNewWindow(object sender, BrowserExtendedNavigatingEventArgs e)
        {
            e.Cancel = true;
            CreateNewBrowserTab();
            Navigate(e.Url.ToString());
        }

        void browser_DocumentTitleChanged(object sender, EventArgs e)
        {
            var wb = sender as ExtendedWebBrowser;
            if (wb != null)
            {
                string title = wb.DocumentTitle;
                if (string.IsNullOrEmpty(title))
                {
                    title = "(Yeni Sayfa)";
                }
                if (title.Length > 20)
                {
                    title = title.Substring(0, 20) + "...";
                }
                GetBrowserTab(sender).Header = title;
                edAddress.Text = wb.Url.ToString();
                edAddress.SelectAll();
            }
        }

        public void CreateNewTab()
        {
            TabItem t = CreateNewBrowserTab();
            _browserTabs[t].Navigate(new Uri("about:blank"));
            ControlShown();
        }

        private void CloseTab(TabItem tabPage)
        {
            if (_tabQueue.IndexOf(tabPage) > 0)
                tbMain.SelectedItem = _tabQueue[_tabQueue.IndexOf(tabPage) - 1];
            UnWrapBrowserEvents(_browserTabs[tabPage]);
            _browserTabs[tabPage].Dispose();
            _browserTabs.Remove(tabPage);
            tbMain.Items.Remove(tabPage);
            _tabQueue.Remove(tabPage);
        }

        private static TabItem GetBrowserTab(object browser)
        {
            return browser is ExtendedWebBrowser ? (browser as ExtendedWebBrowser).Tag as TabItem : null;
        }

        private TabItem GetActiveTab()
        {
            if (tbMain.Items.Count == 0)
            {
                return CreateNewBrowserTab();
            }
            return tbMain.SelectedItem as TabItem;
        }

        private ExtendedWebBrowser GetActiveBrowser()
        {
            return _browserTabs[GetActiveTab()];
        }

        private bool HasActiveBrowser()
        {
            return tbMain.Items.Count > 0 
                && tbMain.SelectedItem != null
                && _browserTabs.ContainsKey(tbMain.SelectedItem as TabItem)
                && _browserTabs[tbMain.SelectedItem as TabItem].Url != null;
        }

        public void Navigate(string urlString)
        {
            TabItem t = GetActiveTab();
            if (urlString.ToLower() != "about:blank" && !urlString.StartsWith(Uri.UriSchemeHttp + Uri.SchemeDelimiter))
                if (urlString.Contains(" ") || !urlString.Contains("."))
                    urlString = "http://www.google.com/search?q=" + urlString;
                else
                    urlString = Uri.UriSchemeHttp + Uri.SchemeDelimiter + urlString;
            _browserTabs[t].Navigate(new Uri(urlString));
        }

        private void ActiveBrowserForward()
        {
            if (HasActiveBrowser())
            {
                ExtendedWebBrowser b = GetActiveBrowser();
                if (b.CanGoForward)
                    b.GoForward();
            }
        }

        private void ActiveBrowserBack()
        {
            if (HasActiveBrowser())
            {
                ExtendedWebBrowser b = GetActiveBrowser();
                if (b.CanGoBack)
                    b.GoBack();
            }
        }

        private void btnBack_Click(object sender, RoutedEventArgs e)
        {
            ActiveBrowserBack();
        }

        private void btnForward_Click(object sender, RoutedEventArgs e)
        {
            ActiveBrowserForward();
        }

        private void edAddress_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                Navigate(edAddress.Text);
                edAddress.SelectAll();
            }
        }

        private void btnAddTab_Click(object sender, RoutedEventArgs e)
        {
            CreateNewTab();
        }

        private void btnRemoveTab_Click(object sender, RoutedEventArgs e)
        {
            CloseTab(GetActiveTab());
        }

        private void tbMain_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            edAddress.Text = HasActiveBrowser() ? GetActiveBrowser().Url.ToString() : "";
        }

        private void UserControl_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            ResizeToolbar(MainToolbar, edAddress);
        }

        public static void ResizeToolbar(ToolBar toolStrip, FrameworkElement resizingItem)
        {
            var w = (from FrameworkElement t in toolStrip.Items where t != resizingItem select t.ActualWidth).Sum();

            if (((toolStrip.ActualWidth - w) - 50) > 50)
                resizingItem.Width = (toolStrip.ActualWidth - w) - 50;
            else
                resizingItem.Width = 50;
        }
    }
}
