using System;

namespace Samba.Infrastructure.Cron
{
	public class CronExpression
	{
		public string Minutes { get; set; }
		public string Hours { get; set; }
		public string Days { get; set; }
		public string Months { get; set; }
		public string DaysOfWeek { get; set; }

		public CronExpression()
			: this("*", "*", "*", "*", "*")
		{

		}
		public CronExpression(string minutes, string hours, string days, string months, string daysOfWeek)
		{
			if (string.IsNullOrEmpty(minutes))
			{
				throw new ArgumentNullException("minutes");
			}
			if (string.IsNullOrEmpty(hours))
			{
				throw new ArgumentNullException("hours");
			}
			if (string.IsNullOrEmpty(days))
			{
				throw new ArgumentNullException("days");
			}
			if (string.IsNullOrEmpty(months))
			{
				throw new ArgumentNullException("months");
			}
			if (string.IsNullOrEmpty(daysOfWeek))
			{
				throw new ArgumentNullException("daysOfWeek");
			}
			Minutes = minutes;
			Hours = hours;
			Days = days;
			Months = months;
			DaysOfWeek = daysOfWeek;
		}

		public override string ToString()
		{
			return Minutes + " " + Hours + " " + Days + " " + Months + " " + DaysOfWeek;
		}
	}
}