using System;
using System.Linq;

namespace Samba.Modules.SettingsModule.BrowserViews
{
    class SambaPosWebsite : BrowserViewModel
    {
        public SambaPosWebsite()
        {
            header = "SambaPOS Website"; //TODO: make localisation-string
            url = "http://www.sambapos.com";
        }
    }
}
