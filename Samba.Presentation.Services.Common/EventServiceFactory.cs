using Microsoft.Practices.Prism.Events;

namespace Samba.Presentation.Services.Common
{
    public static class EventServiceFactory
    {
        // Singleton instance of the EventAggregator service
        private static EventAggregator _eventSerice;

        // Lock (sync) object
        private static readonly object _syncRoot = new object();

        // Factory method
        public static EventAggregator EventService
        {
            get
            {
                // Lock execution thread in case of multi-threaded
                // (concurrent) access.
                lock (_syncRoot)
                {
                    return _eventSerice ?? (_eventSerice = new EventAggregator());
                    // Return singleton instance
                } // lock
            }
        }
    }
}
