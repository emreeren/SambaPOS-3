namespace Samba.Infrastructure.Cron
{
	public class DaysOfWeekCronEntry : CronEntryBase
	{
		public DaysOfWeekCronEntry(string expression)
		{
			Initialize(expression, 0, 6);
		}
	}
}