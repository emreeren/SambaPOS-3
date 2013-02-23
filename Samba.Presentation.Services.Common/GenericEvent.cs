using System;
using Microsoft.Practices.Prism.Events;

namespace Samba.Presentation.Services.Common
{
    public class GenericEvent<TValue> : CompositePresentationEvent<EventParameters<TValue>> { }
    public class GenericIdEvent : CompositePresentationEvent<EventParameters<int>> { }

    public class EventParameters<TValue>
    {
        public string Topic { get; set; }
        public Action ExpectedAction { get; set; }
        public TValue Value { get; set; }
    }
}
