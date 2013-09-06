using System;

namespace Samba.Services.Common
{
    public class DateRange
    {
        public DateRange(DateTime? start, DateTime? end)
        {
            Start = start;
            End = end;
        }

        public DateTime? Start { get; set; }
        public DateTime? End { get; set; }
    }
}