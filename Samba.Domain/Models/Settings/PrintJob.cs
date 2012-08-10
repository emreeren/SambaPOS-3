using System.Collections.Generic;
using Samba.Infrastructure.Data;

namespace Samba.Domain.Models.Settings
{
    public enum WhatToPrintTypes
    {
        Everything,
        NewLines,
        GroupedByBarcode,
        GroupedByGroupCode,
        GroupedByTag,
        LastLinesByPrinterLineCount
    }

    public class PrintJob : Entity
    {
        public int WhatToPrint { get; set; }
        public bool LocksTicket { get; set; }
        public bool UseForPaidTickets { get; set; }
        public bool ExcludeTax { get; set; }

        private readonly IList<PrinterMap> _printerMaps;
        public virtual IList<PrinterMap> PrinterMaps
        {
            get { return _printerMaps; }
        }
        
        public PrintJob()
        {
            _printerMaps = new List<PrinterMap>();
        }
    }
}
