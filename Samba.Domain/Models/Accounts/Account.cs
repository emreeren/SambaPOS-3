using System;
using System.Xml.Linq;
using Samba.Infrastructure.Data;

namespace Samba.Domain.Models.Accounts
{
    public class Account : IEntity
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public virtual AccountTemplate AccountTemplate { get; set; }
        public string SearchString { get; set; }
        public DateTime AccountOpeningDate { get; set; }
        public string CustomData { get; set; }
        
        private static Account _null;
        public static Account Null { get { return _null ?? (_null = new Account { Name = "*" }); } }

        public Account()
        {
            AccountOpeningDate = DateTime.Now;
        }
    }
}
