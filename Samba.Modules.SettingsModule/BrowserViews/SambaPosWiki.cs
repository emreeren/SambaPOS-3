using System;
using System.Linq;

namespace Samba.Modules.SettingsModule.BrowserViews
{
    class SambaPosWiki : BrowserViewModel
    {
        public SambaPosWiki()
        {
            header = "SambaPOS Wiki"; //TODO: make localisation-string
            url = "http://doc.sambapos.com/doku.php/en/v3/samba";
        }
    }
}
