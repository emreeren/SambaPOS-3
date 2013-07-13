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
    class TagOrder : OrderTagOperation
    {
        [ImportingConstructor]
        public TagOrder(ICacheService cacheService, ITicketService ticketService)
            : base(cacheService, ticketService)
        {
        }

        protected override string GetActionName()
        {
            return Resources.TagOrder;
        }

        protected override string GetActionKey()
        {
            return ActionNames.TagOrder;
        }
    }
}
