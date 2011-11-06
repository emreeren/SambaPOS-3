using Microsoft.Practices.Prism.Events;

namespace Samba.Presentation.Common
{
    public class GenericEvent<TValue> : CompositePresentationEvent<EventParameters<TValue>> { }

    public class EventParameters<TValue>
    {
        public string Topic { get; set; }
        public TValue Value { get; set; }
    }
}
