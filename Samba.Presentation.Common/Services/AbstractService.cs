using Microsoft.Practices.Prism.Events;
using Samba.Services;

namespace Samba.Presentation.Common.Services
{
    public abstract class AbstractService : IService
    {
        protected AbstractService()
        {
            EventServiceFactory.EventService.GetEvent<GenericEvent<EventAggregator>>().Subscribe(
                x =>
                {
                    if (x.Topic == EventTopicNames.ResetCache)
                    {
                        Reset();
                    }
                });
        }

        public abstract void Reset();
    }
}
