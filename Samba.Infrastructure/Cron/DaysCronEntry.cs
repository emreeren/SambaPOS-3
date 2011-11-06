namespace Samba.Infrastructure.Cron
{
	public class DaysCronEntry : CronEntryBase
	{
		public DaysCronEntry(string expression)
		{
			Initialize(expression, 1, 31);
		}
	}
}