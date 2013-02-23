using System;
using System.Collections;

namespace Samba.Infrastructure.Messaging
{
    public class MessagingClientObject : MarshalByRefObject, IObserver
    {
        private readonly ArrayList _newData = new ArrayList();

        public int GetData(out string[] arrData)
        {
            arrData = new String[_newData.Count];
            _newData.CopyTo(arrData);
            _newData.Clear();
            return arrData.Length;
        }

        public bool Update(ISubject sender, string data, short objState)
        {
            _newData.Add(data);
            return true;
        }

        public override object InitializeLifetimeService()
        {
            return null;
        }
    }
}
