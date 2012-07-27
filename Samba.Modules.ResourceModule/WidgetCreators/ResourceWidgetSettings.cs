using System.Collections.Generic;
using System.ComponentModel;
using Samba.Presentation.Common.ModelBase;

namespace Samba.Modules.ResourceModule.WidgetCreators
{
    public class ResourceNameValue : IValueWithSource
    {
        public string Text { get; set; }

        private IEnumerable<string> _values;
        public IEnumerable<string> Values
        {
            get { return _values ?? (_values = new List<string>(new[] { "a", "b" })); }
        }

        public void UpdateValues(IEnumerable<string> values)
        {
            _values = values;
        }
    }


    public class ResourceWidgetSettings
    {
        private ResourceNameValue _resourceNameValue;
        public ResourceNameValue ResourceNameValue
        {
            get { return _resourceNameValue ?? (_resourceNameValue = new ResourceNameValue()); }
        }

        [Browsable(false)]
        public string ResourceName { get { return ResourceNameValue.Text; } set { ResourceNameValue.Text = value; } }
        public string Caption { get; set; }
    }
}