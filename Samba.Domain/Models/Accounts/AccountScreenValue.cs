using Samba.Infrastructure.Data;

namespace Samba.Domain.Models.Accounts
{
    public class AccountScreenValue : ValueClass, IOrderable
    {
        public int AccountScreenId { get; set; }
        public int AccountTypeId { get; set; }
        public string AccountTypeName { get; set; }
        public bool DisplayDetails { get; set; }
        public bool HideZeroBalanceAccounts { get; set; }

        public string Name
        {
            get { return AccountTypeName; }
        }

        public int SortOrder { get; set; }

        public string UserString
        {
            get { return Name; }
        }
    }
}