using System;
using Samba.Infrastructure.Data;

namespace Samba.Domain.Models.Resources
{
    public class ResourceScreenItem : Value, IOrderable, ICacheable
    {
        public string Name { get; set; }
        public int ResourceScreenId { get; set; }
        public int ResourceId { get; set; }
        public string ResourceState { get; set; }
        public int SortOrder { get; set; }
        public DateTime LastUpdateTime { get; set; }

        private readonly int _itemId;
        public int ItemId
        {
            get { return _itemId; }
        }

        public string UserString
        {
            get { return Name; }
        }

        public ResourceScreenItem(int itemId)
            : this()
        {
            _itemId = itemId;
        }

        public ResourceScreenItem()
        {
            LastUpdateTime = DateTime.Now;
            _itemId = 0;
        }
    }
}
