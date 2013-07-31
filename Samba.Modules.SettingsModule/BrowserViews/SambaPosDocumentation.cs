using System;
using System.Linq;
using Samba.Localization.Properties;

namespace Samba.Modules.SettingsModule.BrowserViews
{
    class SambaPosDocumentation : BrowserViewModel
    {
        public SambaPosDocumentation()
        {
            Header = string.Format("SambaPOS {0}", Resources.Documentation);
            Url = "http://www.sambapos.com/en/content/sambapos-documentation";
        }
    }
}
