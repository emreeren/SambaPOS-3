using System;
using System.Collections;
using System.Linq;
using System.Net.Sockets;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Tcp;
using System.Threading;
using Samba.Infrastructure.Settings;

namespace Samba.Infrastructure.Messaging
{
    public static class MessagingClient
    {
        private static TcpChannel _channel;
        private static MessagingServerObject _serverObject;
        private static MessagingClientObject _clientObject;
        private static readonly Timer Timer = new Timer(OnTimerTick, null, Timeout.Infinite, 1000);
        private static IMessageListener _messageListener;

        public static bool IsConnected { get; set; }

        private static void OnTimerTick(object state)
        {
            if (_clientObject != null && _messageListener != null && IsConnected)
            {
                string[] arrData;
                _clientObject.GetData(out arrData);

                foreach (var t in arrData.Distinct())
                {
                    _messageListener.ProcessMessage(t);
                }
            }
        }

        public static void Stop()
        {
            if (!IsConnected) return;
            Timer.Dispose();
            Disconnect();
        }

        public static void Disconnect()
        {
            IsConnected = false;
            _messageListener = null;
            try
            {
                try
                {
                    if (_serverObject != null)
                        _serverObject.Detach(_clientObject);
                }
                catch (Exception)
                {
                    _serverObject = null;
                }
            }
            finally
            {
                if (_channel != null)
                {
                    ChannelServices.UnregisterChannel(_channel);
                    _channel = null;
                }
            }
        }

        public static void SendMessage(string message)
        {
            if (_serverObject != null)
            {
                try
                {
                    _serverObject.SetValue(string.Format("{0}", message));
                }
                catch (Exception)
                {
                    if (IsConnected) Disconnect();
                }
            }
        }

        public static bool CanPing()
        {
            try
            {
                if (_serverObject != null)
                {
                    _serverObject.Ping();
                    return true;
                }
                return false;
            }
            catch (Exception)
            {
                if (IsConnected) Disconnect();
                return false;
            }
        }

        public static void Reconnect(IMessageListener messageListener)
        {
            try
            {
                Disconnect();
            }
            catch (SocketException)
            {
            }
            _messageListener = messageListener;
            Connect(_messageListener);
        }

        public static void Connect(IMessageListener messageListener)
        {
            Timer.Change(0, Timeout.Infinite);
            if (messageListener == null) return;
            if (string.IsNullOrWhiteSpace(LocalSettings.MessagingServerName)) return;

            _messageListener = messageListener;
            var serverProv = new BinaryServerFormatterSinkProvider
                                 {
                                     TypeFilterLevel = System.Runtime.Serialization.Formatters.TypeFilterLevel.Full
                                 };

            var clientProv = new BinaryClientFormatterSinkProvider();

            IDictionary props = new Hashtable();
            props["port"] = 0;

            _channel = new TcpChannel(props, clientProv, serverProv);

            ChannelServices.RegisterChannel(_channel, false);

            var url = String.Format("tcp://{0}:{1}/ChatServer", LocalSettings.MessagingServerName, LocalSettings.MessagingServerPort);

            try
            {
                _serverObject = (MessagingServerObject)Activator.GetObject(typeof(MessagingServerObject), url);
                _clientObject = new MessagingClientObject();
                _serverObject.Attach(_clientObject);
            }
            catch
            {
                HandleError();
                return;
            }
            IsConnected = true;
            Timer.Change(0, 1000);
        }

        private static void HandleError()
        {
            _messageListener = null;
            _serverObject = null;
            _clientObject = null;
            if (_channel != null)
            {
                ChannelServices.UnregisterChannel(_channel);
                _channel = null;
            }
            IsConnected = false;
        }
    }
}
