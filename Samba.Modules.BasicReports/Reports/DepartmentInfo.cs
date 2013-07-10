using System.Linq;

namespace Samba.Modules.BasicReports.Reports
{
    internal class TicketTypeInfo
    {
        public int TicketTypeId { get; set; }
        public decimal Amount { get; set; }
        public decimal Discount { get; set; }
        public decimal Tax { get; set; }
        public decimal Services { get; set; }
        public int TicketCount { get; set; }
        public string TicketTypeName
        {
            get
            {
                var d = ReportContext.TicketTypes.SingleOrDefault(x => x.Id == TicketTypeId);
                return d != null ? d.Name : Localization.Properties.Resources.UndefinedWithBrackets;
            }
        }
    }
}