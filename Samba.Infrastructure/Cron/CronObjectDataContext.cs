using System;
using System.Collections.Generic;

namespace Samba.Infrastructure.Cron
{
	public class CronObjectDataContext
	{
		public object Object { get; set; }
		public DateTime LastTrigger { get; set; }

        private readonly List<CronSchedule> _cronSchedules;
        public List<CronSchedule> CronSchedules
        {
            get { return _cronSchedules; }
        }

	    public CronObjectDataContext(List<CronSchedule> cronSchedules)
	    {
	        _cronSchedules = cronSchedules;
	    }
	}
}