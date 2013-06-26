using System;
using System.ComponentModel.Composition;
using System.Runtime.CompilerServices;
using System.Threading;
using Samba.Infrastructure;
using Samba.Infrastructure.Messaging;
using Samba.Infrastructure.Settings;

namespace Samba.Services.Implementations.Messaging
{
    internal class MessageData
    {
        public string Command { get; set; }
        public string Value { get; set; }
    }

    [Export(typeof(IMessagingService))]
    public class MessagingService : IMessagingService
    {
        private IMessageListener _messageListener;
        public bool Reconnecting { get; set; }
        public bool IsConnected { get { return MessagingClient.IsConnected; } }

        public void RegisterMessageListener(IMessageListener listener)
        {
            _messageListener = listener;
        }

        public bool CanStartMessagingClient()
        {
            return _messageListener != null && !MessagingClient.IsConnected;
        }

        public void StartMessagingClient()
        {
            if (_messageListener != null)
            {
                if (!CanStartMessagingClient())
                    throw new Exception("Mesaj istemcisi başlatılamaz.");
                MessagingClient.Connect(_messageListener);
            }
        }

        public void SendMessage(string command, string value)
        {
            ThreadPool.QueueUserWorkItem(SendMessageAsync, new MessageData { Command = command, Value = value });
        }

        public string FormatMessage(string command, string value)
        {
            return string.Format("{0}:<{1}>{2}", _messageListener.Key, command, value);
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        private void SendMessageAsync(object data)
        {
            var mData = data as MessageData;
            try
            {
                if (MessagingClient.IsConnected)
                {
                    if (mData != null)
                        MessagingClient.SendMessage(FormatMessage(mData.Command, mData.Value));
                }
                else if (LocalSettings.StartMessagingClient && !Reconnecting)
                    Reconnect();
            }
            catch (Exception)
            {
                //AppServices.MainDispatcher.BeginInvoke(DispatcherPriority.ApplicationIdle, new Action(Reconnect));
            }
        }

        public bool Connected()
        {
            return MessagingClient.CanPing();
        }

        public void Reconnect()
        {
            Reconnecting = true;
            try
            {
                MessagingClient.Reconnect(_messageListener);
            }
            finally
            {
                Reconnecting = false;
            }
        }


    }
}
