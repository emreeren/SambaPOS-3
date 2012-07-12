using System;
using Samba.Infrastructure.Data;

namespace Samba.Domain.Models.Resources
{
    public class ResourceScreenItem : Entity, IOrderable, ICacheable
    {
        public int ResourceId { get; set; }
        public int ResourceStateId { get; set; }
        public int Order { get; set; }
        public DateTime LastUpdateTime { get; set; }
        public string Category { get; set; }
        public int XLocation { get; set; }
        public int YLocation { get; set; }
        public int Height { get; set; }
        public int Width { get; set; }
        public int CornerRadius { get; set; }
        public double Angle { get; set; }

        private readonly int _itemId;
        public int ItemId
        {
            get { return _itemId; }
        }

        public string UserString
        {
            get { return Name + " [" + Category + "]"; }
        }

        public ResourceScreenItem(int itemId)
            : this()
        {
            _itemId = itemId;
        }

        public ResourceScreenItem()
        {
            Height = 70;
            Width = 70;
            LastUpdateTime = DateTime.Now;
            _itemId = 0;
        }
    }
}
