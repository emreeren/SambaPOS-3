using System;
using System.Globalization;

namespace Samba.Infrastructure
{
    public static class DateTimeExtensions
    {
        public static string ToMonthName(this DateTime dateTime)
        {
            return CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(dateTime.Month);
        }

        public static string ToShortMonthName(this DateTime dateTime)
        {
            return CultureInfo.CurrentCulture.DateTimeFormat.GetAbbreviatedMonthName(dateTime.Month);
        }

        public static string ToNextMonthName(this DateTime dateTime)
        {
            return new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1).AddMonths(1).ToMonthName();
        }

        public static DateTime MonthStart(this DateTime dateTime)
        {
            return new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
        }

        public static int WeekOfYear(this DateTime dateTime)
        {
            return CultureInfo.CurrentCulture.Calendar.GetWeekOfYear(dateTime,
                                                                     CultureInfo.CurrentCulture.DateTimeFormat.
                                                                         CalendarWeekRule,
                                                                     CultureInfo.CurrentCulture.DateTimeFormat.
                                                                         FirstDayOfWeek);
        }

        public static int NextWeekOfYear(this DateTime dateTime)
        {
            return WeekOfYear(dateTime.AddDays(7));
        }

        public static DateTime FirstDateOfWeek(int year, int weekOfYear)
        {
            var jan1 = new DateTime(year, 1, 1);

            var daysOffset = (int)CultureInfo.CurrentCulture.DateTimeFormat.FirstDayOfWeek - (int)jan1.DayOfWeek;

            DateTime firstMonday = jan1.AddDays(daysOffset);

            int firstWeek = CultureInfo.CurrentCulture.Calendar.GetWeekOfYear(jan1, CultureInfo.CurrentCulture.DateTimeFormat.CalendarWeekRule, CultureInfo.CurrentCulture.DateTimeFormat.FirstDayOfWeek);

            if (firstWeek <= 1)
            {
                weekOfYear -= 1;
            }

            return firstMonday.AddDays(weekOfYear * 7);
        }
    }
}
