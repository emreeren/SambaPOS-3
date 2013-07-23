using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Tcp;
using System.ServiceProcess;
using Samba.Infrastructure.Messaging;

namespace Samba.MessagingServer.WindowsService
{
    public partial class MessagingServer : ServiceBase
    {
        internal static readonly string MessagingServerPortFile = string.Format("{0}{1}",
            Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData),
            @"\Ozgu Tech\SambaPOS3\MessagingServerPort.dat");
        internal static readonly int StdPort = 8383;
        private static TcpChannel _channel;

        public MessagingServer()
        { InitializeComponent(); }

        public int MessagingServerPort
        { 
            get
            { 
                if (!File.Exists(MessagingServerPortFile))
                { File.WriteAllText(MessagingServerPortFile, StdPort.ToString()); }

                return Convert.ToInt32(File.ReadAllText(MessagingServerPortFile));
            }
            set
            {
                File.WriteAllText(MessagingServerPortFile, value.ToString());
                if (EventLog != null)
                {
                    EventLog.WriteEntry(string.Format("Setting MessagingServerPort to {0}.", value.ToString()));
                }
            }
        }

        protected override void OnContinue()
        { StartServer(); }

        protected override void OnPause()
        { StopServer(); }

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
        { StopServer(); }

        // Argument "Port=8080" sets the Port to 8080!
        protected override void OnStart(string[] args)
        {
            Dictionary<string, string> argsDic = ParseArgs(args);

            string argsEventLogString = string.Empty;

            foreach (KeyValuePair<string, string> arg in argsDic)
            { argsEventLogString += string.Format("{0}: {1}", arg.Key, arg.Value); }

            if (argsEventLogString != string.Empty)
            {
                argsEventLogString = string.Format("OnStart Arguments:\n{0}", argsEventLogString);
                EventLog.WriteEntry(argsEventLogString);
            }

            if (argsDic.ContainsKey("Port") && argsDic["Port"] != string.Empty)
            { MessagingServerPort = Convert.ToInt32(argsDic["Port"]); }

            StartServer();
        }
  
        protected override void OnStop()
        { StopServer(); }

        private Dictionary<string, string> ParseArgs(string[] args)
        {
            Dictionary<string, string> ret = new Dictionary<string, string>();

            foreach (string arg in args)
            {
                var split = arg.Split('=');
                if (split != null && split[0] != null && split[1] != null && split[0] != string.Empty)
                { ret.Add(split[0], split[1]); }
            }
            return ret;
        }

        private void StartServer()
        {
            EventLog.WriteEntry(string.Format("Starting MessagingServer on port {0}.", MessagingServerPort));
            var serverProv = new BinaryServerFormatterSinkProvider
                                                                  { TypeFilterLevel = System.Runtime.Serialization.Formatters.TypeFilterLevel.Full };

            var clientProv = new BinaryClientFormatterSinkProvider();

            IDictionary props = new Hashtable();
            props["port"] = MessagingServerPort;

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
    }
}