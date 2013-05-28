using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PropertyTools.DataAnnotations;
using Samba.Localization.Properties;

namespace Samba.Modules.TicketModule.Widgets.TicketLister
{
    public class TicketListerWidgetSettings
    {
        public string State
        {
            get { return _state; }
            set { _state = value ?? (Resources.Unpaid); }
        }

        private string _format;
        private string _state;
        private int _width;
        private string _fontName;
        private string _selectedTicketSettingName;

        [WideProperty]
        [Height(80)]
        public string Format
        {
            get { return _format; }
            set { _format = value ?? ("{TICKET NO}"); }
        }

        public int Width
        {
            get { return _width; }
            set { _width = value > 0 ? value : 40; }
        }

        public string FontName
        {
            get { return _fontName; }
            set { _fontName = !string.IsNullOrEmpty(value) ? value : "Lucida Console"; }
        }

        public string SelectedTicketSettingName
        {
            get { return _selectedTicketSettingName; }
            set { _selectedTicketSettingName = !string.IsNullOrEmpty(value) ? value : "LISTER_ID"; }
        }
    }
}
