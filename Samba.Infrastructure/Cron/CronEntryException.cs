using System;

namespace Samba.Infrastructure.Cron
{
	public class CronEntryException : Exception
	{
		public CronEntryException(string message)
			: base(message)
		{

		}
	}
}