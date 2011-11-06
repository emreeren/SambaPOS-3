using System;
using System.Collections.Generic;

namespace Samba.Modules.BasicReports
{
    public class FilterGroup
    {
        public string Name { get; set; }
        public IEnumerable<object> Values { get; set; }
        private object _selectedValue;
        public object SelectedValue
        {
            get { return _selectedValue; }
            set
            {
                if (value != _selectedValue)
                {
                    _selectedValue = value;
                    if (ValueChanged != null) ValueChanged();
                }
            }
        }

        public Action ValueChanged { get; set; }
    }
}