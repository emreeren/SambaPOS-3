using System;
using System.Collections.Generic;
using Samba.Infrastructure.Data;

namespace Samba.Domain.Models.Tickets
{
    public class OrderTagGroup : Entity, IOrderable
    {
        public int Order { get; set; }

        public string UserString
        {
            get { return Name; }
        }

        public int ColumnCount { get; set; }
        public int ButtonHeight { get; set; }
        public int MaxSelectedItems { get; set; }
        public int MinSelectedItems { get; set; }
        public bool AddTagPriceToOrderPrice { get; set; }
        public bool FreeTagging { get; set; }
        public bool SaveFreeTags { get; set; }
        public string GroupTag { get; set; }

        private readonly IList<OrderTag> _orderTags;
        public virtual IList<OrderTag> OrderTags
        {
            get { return _orderTags; }
        }

        private readonly IList<OrderTagMap> _orderTagMaps;
        public virtual IList<OrderTagMap> OrderTagMaps
        {
            get { return _orderTagMaps; }
        }

        public OrderTagGroup()
        {
            _orderTags = new List<OrderTag>();
            _orderTagMaps = new List<OrderTagMap>();
            ColumnCount = 5;
            ButtonHeight = 65;
        }

        public OrderTag AddOrderTag(string name, decimal price)
        {
            var prp = new OrderTag { Name = name, Price = price };
            OrderTags.Add(prp);
            return prp;
        }

        public OrderTagMap AddOrderTagMap()
        {
            var map = new OrderTagMap();
            OrderTagMaps.Add(map);
            return map;
        }
    }
}
