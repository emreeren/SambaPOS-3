using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Runtime.CompilerServices;
using Samba.Domain.Models.Users;

namespace Samba.Presentation.Services.Implementations
{
    [Export(typeof(IMethodQueue))]
    public class MethodQueue : IMethodQueue
    {
        private readonly IApplicationState _applicationState;

        [ImportingConstructor]
        public MethodQueue(IApplicationState applicationState)
        {
            _applicationState = applicationState;
        }

        private static readonly Dictionary<string, Action> MethodList = new Dictionary<string, Action>();

        public void Queue(string key, Action action)
        {
            if (!MethodList.ContainsKey(key))
                MethodList.Add(key, action);
            else MethodList[key] = action;
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public void RunQueue()
        {
            if (MethodList.Count == 0 || _applicationState.CurrentLoggedInUser == User.Nobody || _applicationState.IsLocked) return;
            lock (MethodList)
            {
                MethodList.Values.ToList().ForEach(x => x.Invoke());
                MethodList.Clear();
            }
        }
    }
}
