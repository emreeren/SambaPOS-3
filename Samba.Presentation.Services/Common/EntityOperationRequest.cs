namespace Samba.Presentation.Services.Common
{
    public class EntityOperationRequest<T>
    {
        private readonly string _expectedEvent;

        public EntityOperationRequest(T selectedEntity, string expectedEvent)
        {
            SelectedEntity = selectedEntity;
            _expectedEvent = expectedEvent;
            Data = "";
        }

        public T SelectedEntity { get; set; }
        public string Data { get; set; }

        public void Publish(T selectedEntity)
        {
            SelectedEntity = selectedEntity;
            this.PublishEvent(_expectedEvent);
        }

        public static void Publish(T selectedEntity, string requestedEvent, string expectedEvent, string data)
        {
            var request = new EntityOperationRequest<T>(selectedEntity, expectedEvent) { Data = data };
            request.PublishEvent(requestedEvent);
        }

        public string GetExpectedEvent()
        {
            return _expectedEvent;
        }
    }
}