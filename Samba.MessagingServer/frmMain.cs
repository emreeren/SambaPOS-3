using System;
using System.Collections;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Tcp;
using System.Windows.Forms;
using Samba.Infrastructure.Messaging;
using Samba.MessagingServer.Properties;

namespace Samba.MessagingServer
{
    public partial class frmMain : Form
    {
        public frmMain()
        {
            InitializeComponent();
        }

        private static TcpChannel _channel;

        private void button1_Click(object sender, EventArgs e)
        {
            StartServer();
        }

        private void StartServer()
        {
            var port = Convert.ToInt32(edPort.Text);
            var serverProv = new BinaryServerFormatterSinkProvider
                                 {
                                     TypeFilterLevel = System.Runtime.Serialization.Formatters.TypeFilterLevel.Full
                                 };

            var clientProv = new BinaryClientFormatterSinkProvider();

            IDictionary props = new Hashtable();
            props["port"] = port;

            _channel = new TcpChannel(props, clientProv, serverProv);
            ChannelServices.RegisterChannel(_channel, false);
            RemotingConfiguration.RegisterWellKnownServiceType(typeof(MessagingServerObject),
                                                               "ChatServer", WellKnownObjectMode.Singleton);

            lbStatus.Text = Resources.status_working;
            btnStart.Enabled = false;
            btnStop.Enabled = true;
            WindowState = FormWindowState.Minimized;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (_channel != null)
            {
                ChannelServices.UnregisterChannel(_channel);
                _channel = null;
                lbStatus.Text = Resources.status_stopped;
                btnStart.Enabled = true;
                btnStop.Enabled = false;
            }
        }

        private void frmMain_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (_channel != null)
                ChannelServices.UnregisterChannel(_channel);
            Properties.Settings.Default.Save();
        }

        private void notifyIcon1_DoubleClick(object sender, EventArgs e)
        {
            Show();
            WindowState = FormWindowState.Normal;
        }

        private void frmMain_Resize(object sender, EventArgs e)
        {
            if (WindowState == FormWindowState.Minimized)
                Hide();
            else
            {
                Show();
            }
        }

        private void frmMain_Shown(object sender, EventArgs e)
        {
            if (Properties.Settings.Default.AutoStartServer)
                StartServer();
        }

        private void notifyIcon1_Click(object sender, EventArgs e)
        {
            Show();
            WindowState = FormWindowState.Normal;
        }
    }
}
