using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Samba.Modules.BasicReports.Reports.AccountReport
{
    public class AccountData
    {
        public int Id { get; set; }
        public string PhoneNumber { get; set; }
        public string AccountName { get; set; }
        public decimal Amount { get; set; }
    }
}
