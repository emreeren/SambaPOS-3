using System.ComponentModel;
using Samba.Presentation.Common.ModelBase;

namespace Samba.Modules.EntityModule.Widgets.EntitySearch
{
    public class EntitySearchWidgetSettings
    {
        private NameWithValue _entityTypeNameValue;
        public NameWithValue EntityTypeNameValue
        {
            get { return _entityTypeNameValue ?? (_entityTypeNameValue = new NameWithValue()); }
        }

        private NameWithValue _stateFilterNameValue;
        public NameWithValue StateFilterNameValue
        {
            get { return _stateFilterNameValue ?? (_stateFilterNameValue = new NameWithValue()); }
        }

        [Browsable(false)]
        public string EntityTypeName { get { return EntityTypeNameValue.Text; } set { EntityTypeNameValue.Text = value; } }
        [Browsable(false)]
        public string StateFilterName { get { return StateFilterNameValue.Text; } set { StateFilterNameValue.Text = value; } }
        public bool IsKeyboardVisible { get; set; }
        public bool CanEditEntity { get; set; }
        public bool CanCreateEntity { get; set; }
        public bool CanDisplayAccount { get; set; }
        public string SearchLabel { get; set; }
    }
}
