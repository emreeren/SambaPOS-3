using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DataGridFilterLibrary.Querying
{
    public class FilteringEventArgs : EventArgs
    {
        public Exception Error { get; private set; }

        public FilteringEventArgs(Exception ex)
        {
            Error = ex;
        }
    }
}
