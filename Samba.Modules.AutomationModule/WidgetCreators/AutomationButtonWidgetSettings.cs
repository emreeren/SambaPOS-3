using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using Samba.Presentation.Common.ModelBase;

namespace Samba.Modules.AutomationModule.WidgetCreators
{
    public class CommandNameValue : IValueWithSource
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

    public class AutomationButtonWidgetSettings
    {
        private CommandNameValue _commandNameValue;
        public CommandNameValue CommandNameValue
        {
            get { return _commandNameValue ?? (_commandNameValue = new CommandNameValue()); }
        }

        [Browsable(false)]
        public string CommandName { get { return CommandNameValue.Text; } set { CommandNameValue.Text = value; } }
        public string Caption { get; set; }
        public string ButtonColor { get; set; }
    }
}
