using System.Diagnostics;
using Microsoft.Practices.ServiceLocation;
using Samba.Presentation.Common;
using Samba.Presentation.Services;
using Samba.Presentation.Services.Common;
using Samba.Services;
using Samba.Services.Common;

namespace Samba.Presentation.ViewModels
{
    public static class GenericRuleRegistator
    {
        private static readonly IApplicationState ApplicationState = ServiceLocator.Current.GetInstance<IApplicationState>();
        private static readonly IActionService ActionService = ServiceLocator.Current.GetInstance<IActionService>();

        private static bool _registered;

        public static void RegisterOnce()
        {
            Debug.Assert(_registered == false);
            ActionService.Register();
            HandleEvents();
            RegisterNotifiers();
            _registered = true;
        }

        private static void HandleEvents()
        {
            EventServiceFactory.EventService.GetEvent<GenericEvent<ActionData>>().Subscribe(x =>
            {
                if (ActionService.CanProcessAction(x.Value.Action.ActionType))
                {
                    ActionService.ProcessAction(x.Value.Action.ActionType, x.Value);
                }
            });
        }


        private static void RegisterNotifiers()
        {
            EventServiceFactory.EventService.GetEvent<GenericEvent<Message>>().Subscribe(x =>
            {
                if (x.Topic == EventTopicNames.MessageReceivedEvent && x.Value.Command == "ActionMessage")
                {
                    ApplicationState.NotifyEvent(RuleEventNames.MessageReceived, new { Command = x.Value.Data });
                }
            });
        }
    }
}
