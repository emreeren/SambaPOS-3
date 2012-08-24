using Samba.Infrastructure.Data;

namespace Samba.Domain.Models.Accounts
{
    public class AccountScreenValue : Value, IOrderable
    {
        public string AccountTemplateName { get; set; }
        public bool DisplayDetails { get; set; }

        public string Name
        {
            get { return AccountTemplateName; }
        }

        public int Order { get; set; }

        public string UserString
        {
            get { return Name; }
        }
    }
}