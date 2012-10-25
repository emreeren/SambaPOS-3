using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using Samba.Presentation.Common.ModelBase;

namespace Samba.Modules.ResourceModule.Widgets.ResourceGrid
{
    public class ResourceGridWidgetSettings
    {
        private NameWithValue _stateFilterNameValue;
        public NameWithValue StateFilterNameValue
        {
            get { return _stateFilterNameValue ?? (_stateFilterNameValue = new NameWithValue()); }
        }

        [Browsable(false)]
        public string StateFilterName { get { return StateFilterNameValue.Text; } set { StateFilterNameValue.Text = value; } }
    }
}
