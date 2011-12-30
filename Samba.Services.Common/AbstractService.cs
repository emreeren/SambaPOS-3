using Microsoft.Practices.Prism.Events;

namespace Samba.Services.Common
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
