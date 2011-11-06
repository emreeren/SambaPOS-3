using System;
using System.Collections.Generic;

namespace Samba.Infrastructure.Cron
{
	/// <summary>
	/// An implementation of the Cron scheduler.
	/// </summary>
	public class CronSchedule
	{
		private readonly MinutesCronEntry _minutes;
		private readonly HoursCronEntry _hours;
		private readonly DaysCronEntry _days;
		private readonly MonthsCronEntry _months;
		private readonly DaysOfWeekCronEntry _daysOfWeek;

        //Bol Acýlý SambaPos
		public static CronSchedule Parse(string cronExpression)
		{  
			if (string.IsNullOrEmpty(cronExpression))
			{
				throw new ArgumentException("cronExpression");
			}
			string[] parts = cronExpression.Split(' ');
			if(parts.Length != 5)
			{
				throw new ArgumentException("cronExpression");
			}
			return Parse(parts[0], parts[1], parts[2], parts[3], parts[4]);
		}

		public static CronSchedule Parse(string minutes, string hours, string days, string months, string daysOfWeek)
		{
			if(string.IsNullOrEmpty(minutes))
			{
				throw new ArgumentException("minutes");
			}
			if (string.IsNullOrEmpty(hours))
			{
				throw new ArgumentException("hours");
			}
			if (string.IsNullOrEmpty(days))
			{
				throw new ArgumentException("days");
			}
			if (string.IsNullOrEmpty(months))
			{
				throw new ArgumentException("months");
			}
			if (string.IsNullOrEmpty(daysOfWeek))
			{
				throw new ArgumentException("daysOfWeek");
			}
			return new CronSchedule(minutes, hours, days, months, daysOfWeek);
		}

		private CronSchedule(string minutes, string hours, string days, string months, string daysOfWeek)
		{
			_minutes = new MinutesCronEntry(minutes);
			_hours = new HoursCronEntry(hours);
			_days = new DaysCronEntry(days);
			_months = new MonthsCronEntry(months);
			_daysOfWeek = new DaysOfWeekCronEntry(daysOfWeek); // 0 = Sunday
		}

		public List<DateTime> GetAll(DateTime start, DateTime end)
		{
			List<DateTime> result = new List<DateTime>();

			DateTime current = start;
			while (current <= end)
			{
				DateTime next;
				if (!GetNext(current, end, out next))
				{
					// Did not find any new ones...return what we have
					//
					break;
				}
				result.Add(next);
				current = next;
			}
			return result;
		}

		public bool GetNext(DateTime start, out DateTime next)
		{
			return GetNext(start, DateTime.MaxValue, out next);
		}

		public bool GetNext(DateTime start, DateTime end, out DateTime next)
		{
			// Initialize the next output
			//
			next = DateTime.MinValue;

			// Don't want to select the actual start date.
			//
			DateTime baseSearch = start.AddMinutes(1.0);
			int baseMinute = baseSearch.Minute;
			int baseHour = baseSearch.Hour;
			int baseDay = baseSearch.Day;
			int baseMonth = baseSearch.Month;
			int baseYear = baseSearch.Year;

			// Get the next minute value
			//
			int minute = _minutes.Next(baseMinute);
			if (minute == CronEntryBase.RolledOver)
			{
				// We need to roll forward to the next hour.
				//
				minute = _minutes.First;
				baseHour++;
				// Don't need to worry about baseHour>23 case because
				//	that will roll off our list in the next step.
			}

			// Get the next hour value
			//
			int hour = _hours.Next(baseHour);
			if (hour == CronEntryBase.RolledOver)
			{
				// Roll forward to the next day.
				//
				minute = _minutes.First;
				hour = _hours.First;
				baseDay++;
				// Don't need to worry about baseDay>31 case because
				//	that will roll off our list in the next step.
			}
			else if (hour > baseHour)
			{
				// Original hour must not have been in the list.
				//	Reset the minutes.
				//
				minute = _minutes.First;
			}

			// Get the next day value.
			//
			int day = _days.Next(baseDay);
			if (day == CronEntryBase.RolledOver)
			{
				// Roll forward to the next month
				//
				minute = _minutes.First;
				hour = _hours.First;
				day = _days.First;
				baseMonth++;
				// Need to worry about rolling over to the next year here
				//	because we need to know the number of days in a month
				//	and that is year dependent (leap year).
				//
				if (baseMonth > 12)
				{
					// Roll over to next year.
					//
					baseMonth = 1;
					baseYear++;
				}
			}
			else if (day > baseDay)
			{
				// Original day no in the value list...reset.
				//
				minute = _minutes.First;
				hour = _hours.First;
			}
			while (day > DateTime.DaysInMonth(baseYear, baseMonth))
			{
				// Have a value for the day that is not a valid day
				//	in the current month. Move to the next month.
				//
				minute = _minutes.First;
				hour = _hours.First;
				day = _days.First;
				baseMonth++;
				// This original month could not be December because
				//	it can handle the maximum value of days (31). So
				//	we do not have to worry about baseMonth == 13 case.
			}

			// Get the next month value.
			//
			int month = _months.Next(baseMonth);
			if (month == CronEntryBase.RolledOver)
			{
				// Roll forward to the next year.
				//
				minute = _minutes.First;
				hour = _hours.First;
				day = _days.First;
				month = _months.First;
				baseYear++;
			}
			else if (month > baseMonth)
			{
				// Original month not in the value list...reset.
				//
				minute = _minutes.First;
				hour = _hours.First;
				day = _days.First;
			}
			while (day > DateTime.DaysInMonth(baseYear, month))
			{
				// Have a value for the day that is not a valid day
				//	in the current month. Move to the next month.
				//
				minute = _minutes.First;
				hour = _hours.First;
				day = _days.First;
				month = _months.Next(month + 1);
				if (month == CronEntryBase.RolledOver)
				{
					// Roll forward to the next year.
					//
					minute = _minutes.First;
					hour = _hours.First;
					day = _days.First;
					month = _months.First;
					baseYear++;
				}
			}

			// Is the date / time we found beyond the end search contraint?
			//
			DateTime suggested = new DateTime(baseYear, month, day, hour, minute, 0, 0);
			if (suggested >= end)
			{
				return false;
			}

			// Does the date / time we found satisfy the day of the week contraint?
			//
			if (_daysOfWeek.Values.Contains((int)suggested.DayOfWeek))
			{
				// We have a good date.
				//
				next = suggested;
				return true;
			}

			// We need to recursively look for a date in the future. Because this
			//	search resulted in a day that does not satisfy the day of week 
			//	contraint, start the search on the next day.
			//
			return GetNext(new DateTime(baseYear, month, day, 23, 59, 0, 0), out next);
		}

		public override string ToString()
		{
			return _minutes.Expression + " " +
			       _hours.Expression + " " +
			       _days.Expression + " " +
			       _months.Expression + " " +
			       _daysOfWeek.Expression;
		}
	}
}