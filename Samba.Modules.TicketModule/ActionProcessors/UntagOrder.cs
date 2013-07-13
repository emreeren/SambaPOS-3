using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using Samba.Localization.Properties;
using Samba.Presentation.Services;
using Samba.Presentation.Services.Common;
using Samba.Services;
using Samba.Services.Common;

namespace Samba.Modules.TicketModule.ActionProcessors
{
    [Export(typeof(IActionType))]
    class UntagOrder : OrderTagOperation
    {
        [ImportingConstructor]
        public UntagOrder(ICacheService cacheService, ITicketService ticketService)
            : base(cacheService, ticketService)
        {
        }

        protected override string GetActionName()
        {
            return Resources.UntagOrder;
        }

        protected override string GetActionKey()
        {
            return ActionNames.UntagOrder;
        }

        protected override object GetDefaultData()
        {
            return new { OrderTagName = "", OrderTagValue = "" };
        }
    }
}
