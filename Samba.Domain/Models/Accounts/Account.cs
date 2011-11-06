using System;
using Samba.Infrastructure.Data;

namespace Samba.Domain.Models.Accounts
{
    public class Account : IEntity
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string PhoneNumber { get; set; }
        public string Address { get; set; }
        public string Note { get; set; }
        public DateTime AccountOpeningDate { get; set; }
        public bool InternalAccount { get; set; }

        private static Account _null;
        public static Account Null { get { return _null ?? (_null = new Account { Name = "*" }); } }

        public Account()
        {
            AccountOpeningDate = DateTime.Now;
        }
    }
}
