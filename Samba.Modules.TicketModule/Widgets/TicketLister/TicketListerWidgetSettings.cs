using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using PropertyTools.DataAnnotations;
using Samba.Localization.Properties;
using Samba.Presentation.Common.ModelBase;

namespace Samba.Modules.TicketModule.Widgets.TicketLister
{
    public class TicketListerWidgetSettings
    {
        public TicketListerWidgetSettings()
        {
            Width = 0;
            FontSize = 12;
            FontName = "";
            SelectedBackground = "Silver";
            SelectedForeground = "Black";
            Background = "Transparent";
            Foreground = "Black";
            Border = "Transparent";
            MinWidth = 20;
            State = null;
            Format = null;
        }

        private string _format;
        private string _state;
        private int _width;
        private string _fontName;

        public string State
        {
            get { return _state; }
            set { _state = value ?? (Resources.Unpaid); }
        }

        public string OrderState { get; set; }

        [WideProperty]
        [Height(80)]
        public string Format
        {
            get { return _format; }
            set { _format = value ?? ("<J>{TICKET NO}|{TICKET TOTAL}"); }
        }

        public int Width
        {
            get { return _width; }
            set { _width = value > 0 ? value : 40; }
        }

        public int MinWidth { get; set; }

        public string FontName
        {
            get { return _fontName; }
            set { _fontName = !string.IsNullOrEmpty(value) ? value : "Lucida Console"; }
        }

        public int FontSize { get; set; }

        public string Background { get; set; }
        public string Foreground { get; set; }
        public string SelectedBackground { get; set; }
        public string SelectedForeground { get; set; }
        public string Border { get; set; }
        public bool MultiSelection { get; set; }

        private NameWithValue _commandNameValue;
        public NameWithValue CommandNameValue
        {
            get { return _commandNameValue ?? (_commandNameValue = new NameWithValue()); }
        }

        [Browsable(false)]
        public string CommandName { get { return CommandNameValue.Text; } set { CommandNameValue.Text = value; } }

        public string CommandValue { get; set; }

        private NameWithValue _orderByNameValue;
        public NameWithValue OrderByNameValue
        {
            get { return _orderByNameValue ?? (_orderByNameValue = new NameWithValue()); }
        }

        [Browsable(false)]
        public string OrderBy { get { return OrderByNameValue.Text; } set { OrderByNameValue.Text = value; } }

    }
}
