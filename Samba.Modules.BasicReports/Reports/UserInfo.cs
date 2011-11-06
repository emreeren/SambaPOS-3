using System.Linq;

namespace Samba.Modules.BasicReports.Reports
{
    internal class UserInfo
    {
        public int UserId { get; set; }
        public string UserName
        {
            get
            {
                var user = ReportContext.Users.SingleOrDefault(x => x.Id == UserId);
                return user != null ? user.Name : Localization.Properties.Resources.UndefinedWithBrackets;
            }
        }
        public decimal Amount { get; set; }
    }
}