using System;

namespace Samba.Infrastructure
{
    public interface IMessageListener
    {
        string Key { get; }
        void ProcessMessage(string message);
    }
}
