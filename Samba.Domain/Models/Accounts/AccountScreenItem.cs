using System;
using Samba.Infrastructure.Data;

namespace Samba.Domain.Models.Accounts
{
    public class AccountScreenItem : Entity, IOrderable, ICacheable
    {
        public int Order { get; set; }
        public DateTime LastUpdateTime { get; set; }
        public string Category { get; set; }
        public int AccountStateId { get; set; }
        public int XLocation { get; set; }
        public int YLocation { get; set; }
        public int Height { get; set; }
        public int Width { get; set; }
        public int CornerRadius { get; set; }
        public double Angle { get; set; }
        public virtual Account Account { get; set; }
        
        public string UserString
        {
            get { return Name + " [" + Category + "]"; }
        }

        public AccountScreenItem()
        {
            Height = 70;
            Width = 70;
            LastUpdateTime = DateTime.Now;
        }
    }
}
