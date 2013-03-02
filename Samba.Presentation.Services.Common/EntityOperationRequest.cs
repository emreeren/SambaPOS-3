namespace Samba.Presentation.Services.Common
{
    public class EntityOperationRequest<T>
    {
        private readonly string _expectedEvent;

        public EntityOperationRequest(T selectedEntity, string expectedEvent)
        {
            SelectedEntity = selectedEntity;
            _expectedEvent = expectedEvent;
        }

        public T SelectedEntity { get; set; }

        public void Publish(T selectedEntity)
        {
            SelectedEntity = selectedEntity;
            this.PublishEvent(_expectedEvent);
        }

        public static void Publish(T selectedEntity, string requestedEvent, string expectedEvent)
        {
            var request = new EntityOperationRequest<T>(selectedEntity, expectedEvent);
            request.PublishEvent(requestedEvent);
        }

        public string GetExpectedEvent()
        {
            return _expectedEvent;
        }
    }
}