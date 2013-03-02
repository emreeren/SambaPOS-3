using System;
using System.Runtime.CompilerServices;

namespace Samba.Presentation.Services
{
    public interface IMethodQueue
    {
        void Queue(string key, Action action);
        
        [MethodImpl(MethodImplOptions.Synchronized)]
        void RunQueue();
    }
}