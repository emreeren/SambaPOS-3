using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Tcp;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;
using Samba.Infrastructure.Messaging;
using Samba.MessagingServer.Properties;

namespace Samba.MessagingServer.WindowsService
{
    public partial class MessagingServer : ServiceBase
    {
        public MessagingServer()
        { InitializeComponent(); }

        protected override void OnStart(string[] args)
        {
                StartServer();
        }

        private static TcpChannel _channel;

        private void StartServer()
        {
            var port = Convert.ToInt32(Settings.Default.MessageServerPort);
            var serverProv = new BinaryServerFormatterSinkProvider
            { TypeFilterLevel = System.Runtime.Serialization.Formatters.TypeFilterLevel.Full };

            var clientProv = new BinaryClientFormatterSinkProvider();

            IDictionary props = new Hashtable();
            props["port"] = port;

            _channel = new TcpChannel(props, clientProv, serverProv);
            ChannelServices.RegisterChannel(_channel, false);
            RemotingConfiguration.RegisterWellKnownServiceType(typeof(MessagingServerObject),
                "ChatServer", WellKnownObjectMode.Singleton);
        }

        private void StopServer()
        {
            if (_channel != null)
            {
                ChannelServices.UnregisterChannel(_channel);
                _channel = null;
            }
        }

        protected override void OnStop()
        { StopServer(); }

        protected override void OnPause()
        { StopServer(); }

        protected override void OnContinue()
        { StartServer(); }

        protected override bool OnPowerEvent(PowerBroadcastStatus powerStatus)
        {
            switch (powerStatus)
            {
                case PowerBroadcastStatus.BatteryLow:
                    break;
                case PowerBroadcastStatus.OemEvent:
                    break;
                case PowerBroadcastStatus.PowerStatusChange:
                    break;
                case PowerBroadcastStatus.QuerySuspend:
                    break;
                case PowerBroadcastStatus.QuerySuspendFailed:
                    break;
                case PowerBroadcastStatus.ResumeAutomatic:
                    break;
                case PowerBroadcastStatus.ResumeCritical:
                    break;
                case PowerBroadcastStatus.ResumeSuspend:
                    break;
                case PowerBroadcastStatus.Suspend:
                    break;
                default:
                    break;
            }

            return true;
        }

        protected override void OnShutdown()
        {             
            StopServer();
        }
    }
}
