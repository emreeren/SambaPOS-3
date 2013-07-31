using System;
using System.Linq;

namespace Samba.Modules.SettingsModule.BrowserViews
{
    class SambaPosDevelopment : BrowserViewModel
    {
        public SambaPosDevelopment()
        {
            header = "SambaPOS Development"; //TODO: make localisation-string
            url = "https://github.com/emreeren/SambaPOS-3";
        }
    }
}
