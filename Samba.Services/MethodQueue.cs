using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Microsoft.Practices.ServiceLocation;
using Samba.Domain.Models.Users;

namespace Samba.Services
{
    public static class MethodQueue
    {
        private static readonly ITicketService TicketService =
            ServiceLocator.Current.GetInstance(typeof(ITicketService)) as ITicketService;

        private static readonly Dictionary<string, Action> MethodList = new Dictionary<string, Action>();

        public static void Queue(string key, Action action)
        {
            if (!MethodList.ContainsKey(key))
                MethodList.Add(key, action);
            else MethodList[key] = action;
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public static void RunQueue()
        {
            if (MethodList.Count == 0 || AppServices.CurrentLoggedInUser == User.Nobody
                || TicketService.CurrentTicket != null) return;
            lock (MethodList)
            {
                MethodList.Values.ToList().ForEach(x => x.Invoke());
                MethodList.Clear();
            }
        }
    }
}
