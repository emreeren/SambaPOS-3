using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using Samba.Presentation.Common.ModelBase;

namespace Samba.Modules.ResourceModule.Widgets.ResourceSearch
{
    public class ResourceSearchWidgetSettings
    {
        private NameWithValue _resourceTypeNameValue;
        public NameWithValue ResourceTypeNameValue
        {
            get { return _resourceTypeNameValue ?? (_resourceTypeNameValue = new NameWithValue()); }
        }

        private NameWithValue _stateFilterNameValue;
        public NameWithValue StateFilterNameValue
        {
            get { return _stateFilterNameValue ?? (_stateFilterNameValue = new NameWithValue()); }
        }

        [Browsable(false)]
        public string ResourceTypeName { get { return ResourceTypeNameValue.Text; } set { ResourceTypeNameValue.Text = value; } }
        [Browsable(false)]
        public string StateFilterName { get { return StateFilterNameValue.Text; } set { StateFilterNameValue.Text = value; } }
        public bool IsKeyboardVisible { get; set; }
    }
}
