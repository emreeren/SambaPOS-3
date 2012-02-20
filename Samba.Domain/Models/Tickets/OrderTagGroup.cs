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

        public string ButtonHeader { get; set; }
        public int ColumnCount { get; set; }
        public int ButtonHeight { get; set; }
        public int TerminalColumnCount { get; set; }
        public int TerminalButtonHeight { get; set; }
        public int SelectionType { get; set; } // 0 multiple, 1 single, 2 quantity
        public bool AddTagPriceToOrderPrice { get; set; }

        public bool IsMultipleSelection { get { return SelectionType == 0; } }
        public bool IsSingleSelection { get { return SelectionType == 1; } }
        public bool UnlocksOrder { get; set; }
        public bool CalculateOrderPrice { get; set; }
        public bool DecreaseOrderInventory { get; set; }

        private IList<OrderTag> _orderTags;
        public virtual IList<OrderTag> OrderTags
        {
            get { return _orderTags; }
            set { _orderTags = value; }
        }

        private IList<OrderTagMap> _orderTagMaps;
        public virtual IList<OrderTagMap> OrderTagMaps
        {
            get { return _orderTagMaps; }
            set { _orderTagMaps = value; }
        }

        public OrderTagGroup()
        {
            _orderTags = new List<OrderTag>();
            _orderTagMaps = new List<OrderTagMap>();
            ColumnCount = 5;
            ButtonHeight = 65;
            TerminalColumnCount = 4;
            TerminalButtonHeight = 35;
            CalculateOrderPrice = true;
            DecreaseOrderInventory = true;
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
