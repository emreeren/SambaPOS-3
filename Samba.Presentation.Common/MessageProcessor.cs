using Samba.Presentation.Services.Common;

namespace Samba.Presentation.Common
{
    public static class MessageProcessor
    {
        public static void ProcessMessage(string message)
        {
            new Message(message).PublishEvent(EventTopicNames.MessageReceivedEvent);
        }
    }
}
