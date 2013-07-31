using System;
using System.Linq;
using Samba.Localization.Properties;

namespace Samba.Modules.SettingsModule.BrowserViews
{
    class SambaPosForum : BrowserViewModel
    {
        public SambaPosForum()
        {
            Header = string.Format("SambaPOS {0}", Resources.Forum);
            Url = "http://forum2.sambapos.com";
        }
    }
}
