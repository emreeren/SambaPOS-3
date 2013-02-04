using System.ComponentModel;
using Samba.Presentation.Common.ModelBase;

namespace Samba.Modules.EntityModule.Widgets.EntityGrid
{
    public class EntityGridWidgetSettings
    {
        private NameWithValue _stateFilterNameValue;
        public NameWithValue StateFilterNameValue
        {
            get { return _stateFilterNameValue ?? (_stateFilterNameValue = new NameWithValue()); }
        }

        [Browsable(false)]
        public string StateFilterName { get { return StateFilterNameValue.Text; } set { StateFilterNameValue.Text = value; } }
        public int Rows { get; set; }
        public int Columns { get; set; }
        public int PageCount { get; set; }
        public int FontSize { get; set; }
    }
}
