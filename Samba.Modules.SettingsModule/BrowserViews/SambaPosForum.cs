using System;
using System.Linq;

namespace Samba.Modules.SettingsModule.BrowserViews
{
    class SambaPosForum : BrowserViewModel
    {
        public SambaPosForum()
        {
            header = "SambaPOS Forum"; //TODO: make localisation-string
            url = "http://forum2.sambapos.com";
        }
    }
}
