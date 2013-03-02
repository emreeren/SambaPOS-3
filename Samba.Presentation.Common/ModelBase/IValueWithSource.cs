﻿using System.Collections.Generic;

namespace Samba.Presentation.Common.ModelBase
{
    public interface IValueWithSource
    {
        string Text { get; set; }
        IEnumerable<string> Values { get; }
    }

    public class NameWithValue : IValueWithSource
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
}
