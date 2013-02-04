using System.Collections.Generic;
using Samba.Infrastructure.Data;

namespace Samba.Domain.Models.Settings
{
    public class Terminal : EntityClass
    {
        public bool IsDefault { get; set; }
        public bool AutoLogout { get; set; }
        
        public virtual Printer ReportPrinter { get; set; }

        private static Terminal _defaultTerminal;
        public static Terminal DefaultTerminal { get { return _defaultTerminal ?? (_defaultTerminal = new Terminal { Name = "Default Terminal" }); } }

    }
}
