using System.Collections.Generic;
using System.ComponentModel;
using Samba.Presentation.Common.ModelBase;

namespace Samba.Modules.ResourceModule.Widgets.ResourceButton
{
    public class ResourceButtonWidgetSettings
    {
        private NameWithValue _resourceNameValue;
        public NameWithValue ResourceNameValue
        {
            get { return _resourceNameValue ?? (_resourceNameValue = new NameWithValue()); }
        }

        [Browsable(false)]
        public string ResourceName { get { return ResourceNameValue.Text; } set { ResourceNameValue.Text = value; } }
        public string Caption { get; set; }
    }
}