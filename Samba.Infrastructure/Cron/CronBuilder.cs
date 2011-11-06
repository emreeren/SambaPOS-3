using System.Collections.Generic;
using System.Text;

namespace Samba.Infrastructure.Cron
{
	public static class CronBuilder
	{
		public enum DayOfWeek
		{
			Sunday,
			Monday,
			Tuesday,
			Wednesday,
			Thursday,
			Friday,
			Saturday
		} ;

		#region Minutely Triggers

		public static CronExpression CreateMinutelyTrigger()
		{
			var cronExpression = new CronExpression
			                                	{
			                                		Minutes = "*",
			                                		Hours = "*",
			                                		Days = "*",
			                                		Months = "*",
			                                		DaysOfWeek = "*"
			                                	};
			return cronExpression;
		}

		#endregion

		#region Hourly Triggers

		public static CronExpression CreateHourlyTrigger()
		{
			return CreateHourlyTrigger(0);
		}
		public static CronExpression CreateHourlyTrigger(int triggerMinute)
		{
			CronExpression cronExpression = new CronExpression
			                                	{
			                                		Minutes = triggerMinute.ToString(),
			                                		Hours = "*",
			                                		Days = "*",
			                                		Months = "*",
			                                		DaysOfWeek = "*"
			                                	};
			return cronExpression;
		}
		public static CronExpression CreateHourlyTrigger(int[] triggerMinutes)
		{

			CronExpression cronExpression = new CronExpression
			                                	{
			                                		Minutes = triggerMinutes.ConvertArrayToString(),
			                                		Hours = "*",
			                                		Days = "*",
			                                		Months = "*",
			                                		DaysOfWeek = "*"
			                                	};
			return cronExpression;
		}
		public static CronExpression CreateHourlyTrigger(int firstMinuteToTrigger, int lastMinuteToTrigger)
		{
			return CreateHourlyTrigger(firstMinuteToTrigger, lastMinuteToTrigger, 1);
		}
		public static CronExpression CreateHourlyTrigger(int firstMinuteToTrigger, int lastMinuteToTrigger, int interval)
		{
			string value = firstMinuteToTrigger + "-" + lastMinuteToTrigger;
			if(interval != 1)
			{
				value += "/" + interval;
			}
			CronExpression cronExpression = new CronExpression
			                                	{
			                                		Minutes = value,
			                                		Hours = "*",
			                                		Days = "*",
			                                		Months = "*",
			                                		DaysOfWeek = "*"
			                                	};
			return cronExpression;
		}

		#endregion

		#region Daily Triggers

		public static CronExpression CreateDailyTrigger()
		{
			return CreateDailyTrigger(0);
		}
		public static CronExpression CreateDailyTrigger(int triggerHour)
		{
			CronExpression cronExpression = new CronExpression
			                                	{
			                                		Minutes = "0",
			                                		Hours = triggerHour.ToString(),
			                                		Days = "*",
			                                		Months = "*",
			                                		DaysOfWeek = "*"
			                                	};
			return cronExpression;
		}
		public static CronExpression CreateDailyTrigger(int[] triggerHours)
		{
			CronExpression cronExpression = new CronExpression
			                                	{
			                                		Minutes = "0",
			                                		Hours = triggerHours.ConvertArrayToString(),
			                                		Days = "*",
			                                		Months = "*",
			                                		DaysOfWeek = "*"
			                                	};
			return cronExpression;
		}
		public static CronExpression CreateDailyTrigger(int firstHourToTrigger, int lastHourToTrigger)
		{
			return CreateDailyTrigger(firstHourToTrigger, lastHourToTrigger, 1);
		}
		public static CronExpression CreateDailyTrigger(int firstHourToTrigger, int lastHourToTrigger, int interval)
		{
			string value = firstHourToTrigger + "-" + lastHourToTrigger;
			if (interval != 1)
			{
				value += "/" + interval;
			}
			CronExpression cronExpression = new CronExpression
			                                	{
			                                		Minutes = "0",
			                                		Hours = value,
			                                		Days = "*",
			                                		Months = "*",
			                                		DaysOfWeek = "*"
			                                	};
			return cronExpression;
		}
		public static CronExpression CreateDailyTrigger(DayOfWeek[] daysOfWeekFilter)
		{
			return CreateDailyTrigger(0, daysOfWeekFilter);
		}
		public static CronExpression CreateDailyTrigger(int triggerHour, DayOfWeek[] daysOfWeekFilter)
		{
			CronExpression cronExpression = new CronExpression
			                                	{
			                                		Minutes = "0",
			                                		Hours = triggerHour.ToString(),
			                                		Days = "*",
			                                		Months = "*",
			                                		DaysOfWeek = daysOfWeekFilter.ConvertArrayToString()
			                                	};
			return cronExpression;
		}
		public static CronExpression CreateDailyTrigger(int[] triggerHours, DayOfWeek[] daysOfWeekFilter)
		{
			CronExpression cronExpression = new CronExpression
			                                	{
			                                		Minutes = "0",
			                                		Hours = triggerHours.ConvertArrayToString(),
			                                		Days = "*",
			                                		Months = "*",
			                                		DaysOfWeek = daysOfWeekFilter.ConvertArrayToString()
			                                	};
			return cronExpression;
		}
		public static CronExpression CreateDailyTrigger(int firstHourToTrigger, int lastHourToTrigger, DayOfWeek[] daysOfWeekFilter)
		{
			return CreateDailyTrigger(firstHourToTrigger, lastHourToTrigger, 1, daysOfWeekFilter);
		}
		public static CronExpression CreateDailyTrigger(int firstHourToTrigger, int lastHourToTrigger, int interval, DayOfWeek[] daysOfWeekFilter)
		{
			string value = firstHourToTrigger + "-" + lastHourToTrigger;
			if (interval != 1)
			{
				value += "/" + interval;
			}
			CronExpression cronExpression = new CronExpression
			                                	{
			                                		Minutes = "0",
			                                		Hours = value,
			                                		Days = "*",
			                                		Months = "*",
			                                		DaysOfWeek = daysOfWeekFilter.ConvertArrayToString()
			                                	};
			return cronExpression;
		}
		public static CronExpression CreateDailyOnlyWeekDayTrigger()
		{
			return CreateDailyOnlyWeekDayTrigger(0);
		}
		public static CronExpression CreateDailyOnlyWeekDayTrigger(int triggerHour)
		{
			return CreateDailyTrigger(triggerHour, GetWeekDays());
		}
		public static CronExpression CreateDailyOnlyWeekDayTrigger(int[] triggerHours)
		{
			return CreateDailyTrigger(triggerHours, GetWeekDays());
		}
		public static CronExpression CreateDailyOnlyWeekDayTrigger(int firstHourToTrigger, int lastHourToTrigger)
		{
			return CreateDailyTrigger(firstHourToTrigger, lastHourToTrigger, GetWeekDays());
		}
		public static CronExpression CreateDailyOnlyWeekDayTrigger(int firstHourToTrigger, int lastHourToTrigger, int interval)
		{
			return CreateDailyTrigger(firstHourToTrigger, lastHourToTrigger, interval, GetWeekDays());
		}
		public static CronExpression CreateDailyOnlyWeekEndTrigger()
		{
			return CreateDailyOnlyWeekEndTrigger(0);
		}
		public static CronExpression CreateDailyOnlyWeekEndTrigger(int triggerHour)
		{
			return CreateDailyTrigger(triggerHour, GetWeekEndDays());
		}
		public static CronExpression CreateDailyOnlyWeekEndTrigger(int[] triggerHours)
		{
			return CreateDailyTrigger(triggerHours, GetWeekEndDays());
		}
		public static CronExpression CreateDailyOnlyWeekEndTrigger(int firstHourToTrigger, int lastHourToTrigger)
		{
			return CreateDailyTrigger(firstHourToTrigger, lastHourToTrigger, GetWeekEndDays());
		}
		public static CronExpression CreateDailyOnlyWeekEndTrigger(int firstHourToTrigger, int lastHourToTrigger, int interval)
		{
			return CreateDailyTrigger(firstHourToTrigger, lastHourToTrigger, interval, GetWeekEndDays());
		}

		#endregion

		#region Monthly Triggers

		public static CronExpression CreateMonthlyTrigger()
		{
			return CreateMonthlyTrigger(0);
		}
		public static CronExpression CreateMonthlyTrigger(int triggerDay)
		{
			CronExpression cronExpression = new CronExpression
			                                	{
			                                		Minutes = "0",
			                                		Hours = "0",
			                                		Days = triggerDay.ToString(),
			                                		Months = "*",
			                                		DaysOfWeek = "*"
			                                	};
			return cronExpression;
		}
		public static CronExpression CreateMonthlyTrigger(int[] triggerDays)
		{
			CronExpression cronExpression = new CronExpression
			                                	{
			                                		Minutes = "0",
			                                		Hours = "0",
			                                		Days = triggerDays.ConvertArrayToString(),
			                                		Months = "*",
			                                		DaysOfWeek = "*"
			                                	};
			return cronExpression;
		}
		public static CronExpression CreateMonthlyTrigger(int firstDayToTrigger, int lastDayToTrigger)
		{
			return CreateMonthlyTrigger(firstDayToTrigger, lastDayToTrigger, 1);
		}
		public static CronExpression CreateMonthlyTrigger(int firstDayToTrigger, int lastDayToTrigger, int interval)
		{
			string value = firstDayToTrigger + "-" + lastDayToTrigger;
			if (interval != 1)
			{
				value += "/" + interval;
			}
			CronExpression cronExpression = new CronExpression
			                                	{
			                                		Minutes = "0",
			                                		Hours = "0",
			                                		Days = value,
			                                		Months = "*",
			                                		DaysOfWeek = "*"
			                                	};
			return cronExpression;
		}

		#endregion

		#region Yearly Triggers

		public static CronExpression CreateYearlyTrigger()
		{
			return CreateYearlyTrigger(0);
		}
		public static CronExpression CreateYearlyTrigger(int triggerMonth)
		{
			CronExpression cronExpression = new CronExpression
			                                	{
			                                		Minutes = "0",
			                                		Hours = "0",
			                                		Days = "0",
			                                		Months = triggerMonth.ToString(),
			                                		DaysOfWeek = "*"
			                                	};
			return cronExpression;
		}
		public static CronExpression CreateYearlyTrigger(int[] triggerMonths)
		{
			CronExpression cronExpression = new CronExpression
			                                	{
			                                		Minutes = "0",
			                                		Hours = "0",
			                                		Days = "0",
			                                		Months = triggerMonths.ConvertArrayToString(),
			                                		DaysOfWeek = "*"
			                                	};
			return cronExpression;
		}
		public static CronExpression CreateYearlyTrigger(int firstMonthToTrigger, int lastMonthToTrigger)
		{
			return CreateYearlyTrigger(firstMonthToTrigger, lastMonthToTrigger, 1);
		}
		public static CronExpression CreateYearlyTrigger(int firstMonthToTrigger, int lastMonthToTrigger, int interval)
		{
			string value = firstMonthToTrigger + "-" + lastMonthToTrigger;
			if (interval != 1)
			{
				value += "/" + interval;
			}
			CronExpression cronExpression = new CronExpression
			                                	{
			                                		Minutes = "0",
			                                		Hours = "0",
			                                		Days = "0",
			                                		Months = value,
			                                		DaysOfWeek = "*"
			                                	};
			return cronExpression;
		}

		#endregion

		private static string ConvertArrayToString(this IEnumerable<int> list)
		{
			StringBuilder result = new StringBuilder();
			List<int> values = new List<int>(list);
			values.Sort();
			for (int i = 0; i < values.Count; i++)
			{
				result.Append(values[i].ToString());
				if (i != values.Count - 1)
				{
					result.Append(",");
				}
			}
			return result.ToString();
		}
		private static string ConvertArrayToString(this DayOfWeek[] list)
		{
			StringBuilder result = new StringBuilder();
			List<int> values = new List<int>();
			for (int i = 0; i < list.Length; i++)
			{
				values.Add((int) list[i]);
			}
			values.Sort();
			for (int i = 0; i < values.Count; i++)
			{
				result.Append(values[i].ToString());
				if (i != values.Count - 1)
				{
					result.Append(",");
				}
			}
			return result.ToString();
		}
		private static DayOfWeek[] GetWeekDays()
		{
			return new[]
			       	{
			       		DayOfWeek.Monday, 
			       		DayOfWeek.Tuesday, 
			       		DayOfWeek.Wednesday, 
			       		DayOfWeek.Thursday,
			       		DayOfWeek.Friday
			       	};
		}
		private static DayOfWeek[] GetWeekEndDays()
		{
			return new[]
			       	{
			       		DayOfWeek.Sunday, 
			       		DayOfWeek.Saturday
			       	};
		}
	}
}
