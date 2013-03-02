using System;
using Samba.Infrastructure.Data;

namespace Samba.Domain.Models.Settings
{
    public class WorkPeriod : EntityClass
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string StartDescription { get; set; }
        public string EndDescription { get; set; }

        public override string ToString()
        {
            if (!string.IsNullOrEmpty(Name))
                return Name;

            return StartDate == EndDate 
                ? StartDate.ToString("dd MMMMM yyyy HH:mm") 
                : string.Format("{0} - {1}", StartDate.ToString("dd MMMMM yyyy HH:mm"), EndDate.ToString("dd MMMMM yyyy HH:mm"));
        }
    }
}
