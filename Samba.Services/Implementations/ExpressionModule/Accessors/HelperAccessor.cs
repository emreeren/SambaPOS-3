using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Samba.Services.Implementations.ExpressionModule.Accessors
{
    public static class HelperAccessor
    {
        public static string GetUniqueString()
        {
            var date = DateTime.Now;
            return string.Format("{0}{1:00}{2:00}{3:00}{4:00}{5:000}", date.Year, date.Month, date.Day, date.Hour, date.Minute, date.Millisecond);
        }
    }
}
