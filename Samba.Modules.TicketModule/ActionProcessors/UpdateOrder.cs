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
    class UpdateOrder : ActionType
    {
        private readonly ITicketService _ticketService;
        private readonly ICacheService _cacheService;

        [ImportingConstructor]
        public UpdateOrder(ITicketService ticketService, ICacheService cacheService)
        {
            _ticketService = ticketService;
            _cacheService = cacheService;
        }

        public override void Process(ActionData actionData)
        {
            var ticket = actionData.GetDataValue<Ticket>("Ticket");
            var orders = Helper.GetOrders(actionData, ticket);
            if (orders.Any())
            {
                foreach (var order in orders)
                {
                    if (!string.IsNullOrEmpty(actionData.GetAsString("Quantity")))
                    {
                        order.Quantity = actionData.GetAsDecimal("Quantity");
                        order.ResetSelectedQuantity();
                    }
                    if (!string.IsNullOrEmpty(actionData.GetAsString("Price")))
                        order.UpdatePrice(actionData.GetAsDecimal("Price"), "");
                    if (!string.IsNullOrEmpty(actionData.GetAsString("IncreaseInventory")))
                        order.IncreaseInventory = actionData.GetAsBoolean("IncreaseInventory");
                    if (!string.IsNullOrEmpty(actionData.GetAsString("DecreaseInventory")))
                        order.DecreaseInventory = actionData.GetAsBoolean("DecreaseInventory");
                    if (!string.IsNullOrEmpty(actionData.GetAsString("Locked")))
                        order.Locked = actionData.GetAsBoolean("Locked");
                    if (!string.IsNullOrEmpty(actionData.GetAsString("CalculatePrice")))
                        order.CalculatePrice = actionData.GetAsBoolean("CalculatePrice");
                    if (!string.IsNullOrEmpty(actionData.GetAsString("AccountTransactionType")))
                        _ticketService.ChangeOrdersAccountTransactionTypeId(ticket, new List<Order> { order },
                                                                           _cacheService.GetAccountTransactionTypeIdByName
                                                                               (actionData.GetAsString("AccountTransactionType")));

                    if (!string.IsNullOrEmpty(actionData.GetAsString("PortionName")) || !string.IsNullOrEmpty(actionData.GetAsString("PriceTag")))
                    {
                        var portionName = actionData.GetAsString("PortionName");
                        var priceTag = actionData.GetAsString("PriceTag");
                        _ticketService.UpdateOrderPrice(order, portionName, priceTag);
                    }
                }
            }
        }

        protected override object GetDefaultData()
        {
            return new
                    {
                        Quantity = 0m,
                        Price = 0m,
                        PortionName = "",
                        PriceTag = "",
                        IncreaseInventory = false,
                        DecreaseInventory = false,
                        CalculatePrice = false,
                        Locked = false,
                        AccountTransactionType = ""
                    };
        }

        protected override string GetActionName()
        {
            return Resources.UpdateOrder;
        }

        protected override string GetActionKey()
        {
            return ActionNames.UpdateOrder;
        }
    }
}
