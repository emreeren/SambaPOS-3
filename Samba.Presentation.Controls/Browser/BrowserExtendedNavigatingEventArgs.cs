using System;
using System.ComponentModel;

namespace Samba.Presentation.Controls.Browser
{
    /// <summary>
    /// Used in the new navigation events
    /// </summary>
    public class BrowserExtendedNavigatingEventArgs : CancelEventArgs
    {
        private Uri _Url;
        /// <summary>
        /// The URL to navigate to
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        public Uri Url
        {
            get { return _Url; }
        }

        private string _Frame;
        /// <summary>
        /// The name of the frame to navigate to
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        public string Frame
        {
            get { return _Frame; }
        }

        /// <summary>
        /// The pointer to ppDisp
        /// </summary>
        public object AutomationObject { get; set; }

        /// <summary>
        /// Creates a new instance of WebBrowserExtendedNavigatingEventArgs
        /// </summary>
        /// <param name="automation">Pointer to the automation object of the browser</param>
        /// <param name="url">The URL to go to</param>
        /// <param name="frame">The name of the frame</param>
        /// <param name="navigationContext">The new window flags</param>
        public BrowserExtendedNavigatingEventArgs(object automation, Uri url, string frame)
            : base()
        {
            _Url = url;
            _Frame = frame;
            this.AutomationObject = automation;
        }
    }

    public class SizeChangedEventArgs : EventArgs
    {
        public int Size { get; set; }

        public SizeChangedEventArgs(int size)
        {
            Size = size;
        }
    }
}
