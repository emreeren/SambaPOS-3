using System.Collections.Generic;
using Samba.Infrastructure.Data;

namespace Samba.Domain.Models.Settings
{
    public class Terminal : Entity
    {
        public byte[] LastUpdateTime { get; set; }
        public bool IsDefault { get; set; }
        public bool AutoLogout { get; set; }
        
        public virtual Printer SlipReportPrinter { get; set; }
        public virtual Printer ReportPrinter { get; set; }

        private IList<PrintJob> _printJobs;
        public virtual IList<PrintJob> PrintJobs
        {
            get { return _printJobs; }
            set { _printJobs = value; }
        }

        private static Terminal _defaultTerminal;
        public static Terminal DefaultTerminal { get { return _defaultTerminal ?? (_defaultTerminal = new Terminal { Name = "Default Terminal" }); } }

        public Terminal()
        {
            _printJobs = new List<PrintJob>();
        }
    }
}
