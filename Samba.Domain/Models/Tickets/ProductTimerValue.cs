using System;
using Samba.Infrastructure.Data;
using Samba.Infrastructure.Settings;

namespace Samba.Domain.Models.Tickets
{
    public class ProductTimerValue : ValueClass
    {
        public ProductTimerValue()
        {
            Start = DateTime.Now;
            End = Start;
        }

        public int ProductTimerId { get; set; }
        public int PriceType { get; set; } //0 min, 1 hr, 2 day
        public decimal PriceDuration { get; set; }
        public decimal MinTime { get; set; }
        public decimal TimeRounding { get; set; }
        public DateTime Start { get; set; }
        public DateTime End { get; set; }

        public bool IsActive { get { return Start == End; } }

        public void Stop()
        {
            if (Start == End) End = DateTime.Now;
        }

        public decimal GetPrice(decimal unitPrice)
        {
            if (unitPrice == 0) return 0;
            return decimal.Round((unitPrice / PriceDuration) * GetTime(), LocalSettings.Decimals);
        }

        public decimal GetTime()
        {
            var time = GetTimePeriod();
            if (time < MinTime)
                time = MinTime;
            else if (TimeRounding > 0 && TimeRounding != time)
                time = (Math.Truncate(time / TimeRounding) + 1) * TimeRounding;
            return time;
        }

        public decimal GetTimePeriod()
        {
            var s = Start;
            var e = End != Start ? End : DateTime.Now;
            var ts = new TimeSpan(e.Ticks - s.Ticks);
            switch (PriceType)
            {
                case 2: return Convert.ToDecimal(ts.TotalDays);
                case 1: return Convert.ToDecimal(ts.TotalHours);
                default: return Convert.ToDecimal(ts.TotalMinutes);
            }
        }

        public TimeSpan GetDuration()
        {
            switch (PriceType)
            {
                case 2: return TimeSpan.FromDays(Convert.ToDouble(GetTime()));
                case 1: return TimeSpan.FromHours(Convert.ToDouble(GetTime()));
                default: return TimeSpan.FromMinutes(Convert.ToDouble(GetTime()));
            }
        }
    }
}
