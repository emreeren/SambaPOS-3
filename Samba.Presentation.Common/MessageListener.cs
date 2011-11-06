using System;
using Samba.Infrastructure;

namespace Samba.Presentation.Common
{
    public class MessageListener : IMessageListener
    {
        public MessageListener()
        {
            _key = Guid.NewGuid().ToString("D");
        }

        private readonly string _key;
        public string Key
        {
            get { return _key; }
        }

        public void ProcessMessage(string message)
        {
            if (!message.Contains(Key))
                MessageProcessor.ProcessMessage(message);
        }
    }
}
