using System;
using System.Collections.Generic;
using Samba.Infrastructure.Data;

namespace Samba.Domain.Models.Tickets
{
    public class OrderStateGroup : Entity, IOrderable
    {
        public string UserString { get { return Name; } }
        public int Order { get; set; }
        public string ButtonHeader { get; set; }
        public int ColumnCount { get; set; }
        public int ButtonHeight { get; set; }
        public bool UnlocksOrder { get; set; }
        public bool CalculateOrderPrice { get; set; }
        public bool DecreaseOrderInventory { get; set; }
        public bool IncreaseOrderInventory { get; set; }
        public int AccountTransactionTypeId { get; set; }

        private readonly IList<OrderState> _orderStates;
        public virtual IList<OrderState> OrderStates
        {
            get { return _orderStates; }
        }

        private readonly IList<OrderStateMap> _orderStateMaps;
        public virtual IList<OrderStateMap> OrderStateMaps
        {
            get { return _orderStateMaps; }
        }

        public OrderStateGroup()
        {
            _orderStates = new List<OrderState>();
            _orderStateMaps = new List<OrderStateMap>();
            ColumnCount = 5;
            ButtonHeight = 65;
            CalculateOrderPrice = true;
            DecreaseOrderInventory = true;
        }

        public OrderState AddOrderState(string name)
        {
            var prp = new OrderState { Name = name };
            OrderStates.Add(prp);
            return prp;
        }

        public OrderStateMap AddOrderStateMap()
        {
            var map = new OrderStateMap();
            OrderStateMaps.Add(map);
            return map;
        }
    }
}
