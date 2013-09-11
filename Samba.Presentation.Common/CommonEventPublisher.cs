using System;
using System.Collections.Generic;
using System.Windows.Controls;
using Samba.Domain.Models.Tickets;
using Samba.Presentation.Common.ModelBase;
using Samba.Presentation.Services.Common;

namespace Samba.Presentation.Common
{
    public static class CommonEventPublisher
    {
        private static readonly IList<string> Events = new List<string>();

        public static void EnqueueTicketEvent(string eventName)
        {
            Events.Add(eventName);
        }

        public static void ExecuteEvents(Ticket ticket)
        {
            foreach (var eventName in Events)
            {
                ticket.PublishEvent(eventName);
            }
            Events.Clear();
        }

        public static void PublishViewAddedEvent(VisibleViewModelBase view)
        {
            view.PublishEvent(EventTopicNames.ViewAdded, true);
        }

        public static void PublishViewClosedEvent(VisibleViewModelBase view)
        {
            view.PublishEvent(EventTopicNames.ViewClosed, true);
        }

        public static void PublishDashboardUnloadedEvent(UserControl userControl)
        {
            userControl.PublishEvent(EventTopicNames.DashboardClosed);
        }

        public static void PublishEntityOperation<T>(T entity, string requestedEvent, string expectedEvent = "", string data = "")
        {
            OperationRequest<T>.Publish(entity, requestedEvent, expectedEvent, data);
        }
    }
}
