using System;
using Samba.Infrastructure.Data;

namespace Samba.Domain.Models.Entities
{
    public class EntityScreenItem : ValueClass, IOrderable, ICacheable
    {
        public string Name { get; set; }
        public int EntityScreenId { get; set; }
        public int EntityId { get; set; }
        public string EntityState { get; set; }
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

        public EntityScreenItem(EntityType entityType, Entity entity, string entityState = null)
            : this()
        {
            EntityId = entity.Id;
            Name = entityType.GetFormattedDisplayName(entity.Name, entity);
            EntityState = entityState;
        }

        public EntityScreenItem()
        {
            LastUpdateTime = DateTime.Now;
            _itemId = 0;
        }
    }
}
