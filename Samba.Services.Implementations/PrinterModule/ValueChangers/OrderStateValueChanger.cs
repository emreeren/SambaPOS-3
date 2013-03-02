﻿using Samba.Domain.Models.Tickets;

namespace Samba.Services.Implementations.PrinterModule.ValueChangers
{
    class OrderStateValueChanger : AbstractValueChanger<OrderStateValue>
    {
        public override string GetTargetTag()
        {
            return "ORDER STATES";
        }

        protected override string GetModelName(OrderStateValue model)
        {
            return model.StateValue;
        }
    }
}
