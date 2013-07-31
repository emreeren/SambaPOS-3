using System;
using System.Linq;

namespace Samba.Modules.SettingsModule.BrowserViews
{
    class SambaPosDocumentation : BrowserViewModel
    {
        public SambaPosDocumentation()
        {
            header = "SambaPOS Documentation"; //TODO: make localisation-string
            url = "http://www.sambapos.com/en/content/sambapos-documentation";
        }
    }
}
