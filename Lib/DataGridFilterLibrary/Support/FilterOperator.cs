using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DataGridFilterLibrary.Support
{
    public enum FilterOperator
    {
        Undefined,
        LessThan,
        LessThanOrEqual,
        GreaterThan,
        GreaterThanOrEqual,
        Equals,
        Like,
        Between
    }
}
