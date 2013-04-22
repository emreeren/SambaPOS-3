using System;
using System.Windows;
using Samba.Localization.Pluralization;

namespace Samba.Presentation.Services.Common
{
    delegate void PublishEventDelegate<in TEventSubject>(TEventSubject eventArgs, string eventTopic, Action expectedAction);

    public static class ExtensionMethods
    {
        private static void Publish<TEventsubject>(this TEventsubject eventArgs, string eventTopic, Action expectedAction)
        {
            var e = EventServiceFactory.EventService.GetEvent<GenericEvent<TEventsubject>>();
            e.Publish(new EventParameters<TEventsubject> { Topic = eventTopic, Value = eventArgs, ExpectedAction = expectedAction });
        }

        public static void PublishEvent<TEventsubject>(this TEventsubject eventArgs, string eventTopic)
        {
            PublishEvent(eventArgs, eventTopic, false);
        }

        public static void PublishEvent<TEventsubject>(this TEventsubject eventArgs, string eventTopic, bool wait)
        {
            if(Application.Current == null) return;

            if (wait)
                Application.Current.Dispatcher.Invoke(new PublishEventDelegate<TEventsubject>(Publish), eventArgs, eventTopic, null);
            else
                Application.Current.Dispatcher.BeginInvoke(new PublishEventDelegate<TEventsubject>(Publish), eventArgs, eventTopic, null);
        }

        public static void PublishIdEvent(int id, string eventTopic)
        {
            PublishIdEvent(id, eventTopic, null);
        }

        public static void PublishIdEvent(int id, string eventTopic, Action expectedAction)
        {
            Application.Current.Dispatcher.BeginInvoke(new PublishEventDelegate<int>(InternalPublishIdEvent), id, eventTopic, expectedAction);
        }

        private static void InternalPublishIdEvent(int id, string eventTopic, Action expectedAction)
        {
            var e = EventServiceFactory.EventService.GetEvent<GenericIdEvent>();
            e.Publish(new EventParameters<int> { Topic = eventTopic, Value = id, ExpectedAction = expectedAction });
        }

        public static string ToPlural(this string singular)
        {
            return Pluralizer.ToPlural(singular);
        }
    }
}
