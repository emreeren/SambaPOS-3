using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using Samba.Domain.Models.Tickets;
using Samba.Localization.Properties;
using Samba.Presentation.Services;
using Samba.Presentation.Services.Common;
using Samba.Services;
using Samba.Services.Common;

namespace Samba.Modules.TicketModule.ActionProcessors
{
    [Export(typeof(IActionType))]
    class AddOrder : ActionType
    {
        private readonly ICacheService _cacheService;
        private readonly ITicketService _ticketService;

        [ImportingConstructor]
        public AddOrder(ICacheService cacheService, ITicketService ticketService)
        {
            _cacheService = cacheService;
            _ticketService = ticketService;
        }

        public override void Process(ActionData actionData)
        {
            var ticket = actionData.GetDataValue<Ticket>("Ticket");

            if (ticket != null)
            {
                var menuItemName = actionData.GetAsString("MenuItemName");
                var menuItem = _cacheService.GetMenuItem(y => y.Name == menuItemName);
                var portionName = actionData.GetAsString("PortionName");
                var quantity = actionData.GetAsDecimal("Quantity");
                var tag = actionData.GetAsString("Tag");
                var orderStateName = actionData.GetAsString("OrderStateName");
                var orderState = actionData.GetAsString("OrderState");
                var osv = orderState.Contains("=") ? orderState : orderStateName + "=" + orderState;
                var order = _ticketService.AddOrder(ticket, menuItem.Id, quantity, portionName, osv);
                if (order != null) order.Tag = tag;
                actionData.DataObject.Order = order;
                order.PublishEvent(EventTopicNames.OrderAdded);
            }
        }

        protected override object GetDefaultData()
        {
            return new { MenuItemName = "", PortionName = "", Quantity = 0, Tag = "", OrderStateName = "", OrderState = "" };
        }

        protected override string GetActionName()
        {
            return Resources.AddOrder;
        }

        protected override string GetActionKey()
        {
            return ActionNames.AddOrder;
        }
    }
}
