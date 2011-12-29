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

        public string TestSaveOperation<T>(T model) where T : class
        {
            return ValidatorRegistry.GetSaveErrorMessage(model);
        }

        public string TestDeleteOperation<T>(T model) where T : class
        {
            return ValidatorRegistry.GetDeleteErrorMessage(model);
        }

        public abstract void Reset();
    }
}
