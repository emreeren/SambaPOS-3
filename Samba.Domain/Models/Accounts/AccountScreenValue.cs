using Samba.Infrastructure.Data;

namespace Samba.Domain.Models.Accounts
{
    public class AccountScreenValue : Value, IOrderable
    {
        public int AccountTypeId { get; set; }
        public string AccountTypeName { get; set; }
        public bool DisplayDetails { get; set; }

        public string Name
        {
            get { return AccountTypeName; }
        }

        public int Order { get; set; }

        public string UserString
        {
            get { return Name; }
        }
    }
}