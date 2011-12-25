using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;

namespace Samba.Services.Common
{
    delegate void PublishEventDelegate<in TEventSubject>(TEventSubject eventArgs, string eventTopic);

    public static class ExtensionMethods
    {
        private static void Publish<TEventsubject>(this TEventsubject eventArgs, string eventTopic)
        {
            var e = EventServiceFactory.EventService.GetEvent<GenericEvent<TEventsubject>>();
            e.Publish(new EventParameters<TEventsubject> { Topic = eventTopic, Value = eventArgs });
        }

        public static void PublishEvent<TEventsubject>(this TEventsubject eventArgs, string eventTopic)
        {
            PublishEvent(eventArgs, eventTopic, false);
        }

        public static void PublishEvent<TEventsubject>(this TEventsubject eventArgs, string eventTopic, bool wait)
        {
            if (wait) Application.Current.Dispatcher.Invoke(new PublishEventDelegate<TEventsubject>(Publish), eventArgs, eventTopic);
            else Application.Current.Dispatcher.BeginInvoke(new PublishEventDelegate<TEventsubject>(Publish), eventArgs, eventTopic);
        }
    }
}
