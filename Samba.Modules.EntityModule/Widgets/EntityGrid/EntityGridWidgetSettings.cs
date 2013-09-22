using System.ComponentModel;
using Samba.Localization;
using Samba.Presentation.Common.ModelBase;

namespace Samba.Modules.EntityModule.Widgets.EntityGrid
{
    public class EntityGridWidgetSettings
    {
        private NameWithValue _stateFilterNameValue;
        private NameWithValue _automationCommandNameValue;

        [LocalizedDisplayName("StateFilter")]
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
        [Browsable(false)]
        public string AutomationCommandName { get { return AutomationCommandNameValue.Text; } set { AutomationCommandNameValue.Text = value; } }

        [LocalizedDisplayName("AutomationCommand")]
        public NameWithValue AutomationCommandNameValue
        {
            get { return _automationCommandNameValue??(_automationCommandNameValue=new NameWithValue()); }
        }

        public string CommandValue { get; set; }
    }
}
