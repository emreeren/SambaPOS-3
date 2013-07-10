namespace Samba.Presentation.Services.Common
{
    public class OperationRequest<T>
    {
        private readonly string _expectedEvent;

        public OperationRequest(T selectedItem, string expectedEvent)
        {
            SelectedItem = selectedItem;
            _expectedEvent = expectedEvent;
            Data = "";
        }

        public T SelectedItem { get; set; }
        public string Data { get; set; }

        public void Publish(T selectedEntity)
        {
            SelectedItem = selectedEntity;
            this.PublishEvent(_expectedEvent);
        }

        public static void Publish(T selectedItem, string requestedEvent, string expectedEvent, string data)
        {
            var request = new OperationRequest<T>(selectedItem, expectedEvent) { Data = data };
            request.PublishEvent(requestedEvent);
        }

        public string GetExpectedEvent()
        {
            return _expectedEvent;
        }
    }
}