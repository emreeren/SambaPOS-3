using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using Samba.Localization.Properties;
using Samba.Presentation.Services;
using Samba.Presentation.Services.Common;
using Samba.Services.Common;

namespace Samba.Presentation.Common.ActionProcessors
{
    [Export(typeof(IActionType))]
    class RefreshCache : ActionType
    {
        private readonly IMethodQueue _methodQueue;
        private readonly ITriggerService _triggerService;
        private readonly IApplicationState _applicationState;

        [ImportingConstructor]
        public RefreshCache(IMethodQueue methodQueue, ITriggerService triggerService, IApplicationState applicationState)
        {
            _methodQueue = methodQueue;
            _triggerService = triggerService;
            _applicationState = applicationState;
        }

        public override void Process(ActionData actionData)
        {
            _methodQueue.Queue("ResetCache", () => Helper.ResetCache(_triggerService, _applicationState));
        }

        protected override object GetDefaultData()
        {
            return new object();
        }

        protected override string GetActionName()
        {
            return Resources.RefreshCache;
        }

        protected override string GetActionKey()
        {
            return ActionNames.RefreshCache;
        }
    }
}
