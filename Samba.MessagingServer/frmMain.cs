using System;
using System.Collections;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Tcp;
using System.ServiceProcess;
using System.Windows.Forms;
using Samba.Infrastructure.Messaging;
using Samba.MessagingServer.Properties;

namespace Samba.MessagingServer
{
    public partial class frmMain : Form
    {
        private static TcpChannel _channel;

        public frmMain()
        {
            InitializeComponent();
            CheckServices();
            UpdateUiTimer.Start();
        }

        private void button1_Click(object sender, EventArgs e)
        { StartServer(); }

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

        private void CheckServices()
        {
            ServiceControllerStatus? status = ServiceHelper.CheckServiceStatus();
    
            if (status == null)
            {
                InstallService.Enabled = true; 
                UninstallService.Enabled = false;
                StopService.Enabled = false;
                StartService.Enabled = false;
            }
            else
            {
                InstallService.Enabled = false;
                UninstallService.Enabled = true;
                switch (status)
                {
                    case ServiceControllerStatus.ContinuePending:
                        StopService.Enabled = false;
                        StartService.Enabled = false;
                        break;
                    case ServiceControllerStatus.PausePending:
                        StopService.Enabled = false;
                        StartService.Enabled = false;
                        break;
                    case ServiceControllerStatus.Paused:
                        StopService.Enabled = false;
                        StartService.Enabled = true;
                        break;
                    case ServiceControllerStatus.Running:
                        StopService.Enabled = true;
                        StartService.Enabled = false;
                        break;
                    case ServiceControllerStatus.StartPending:
                        StopService.Enabled = false;
                        StartService.Enabled = false;
                        break;
                    case ServiceControllerStatus.StopPending:
                        StopService.Enabled = false;
                        StartService.Enabled = false;
                        break;
                    case ServiceControllerStatus.Stopped:
                        StopService.Enabled = false;
                        StartService.Enabled = true;
                        break;
                    default:
                        break;
                }
            }
        }

        private void frmMain_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (_channel != null)
            { ChannelServices.UnregisterChannel(_channel); }
            Properties.Settings.Default.Save();
        }

        private void frmMain_Resize(object sender, EventArgs e)
        {
            if (WindowState == FormWindowState.Minimized)
            { Hide(); }
            else
            { Show(); }
        }

        private void frmMain_Shown(object sender, EventArgs e)
        {
            if (Properties.Settings.Default.AutoStartServer)
            { StartServer(); }
        }

        private void InstallService_Click(object sender, EventArgs e)
        { ServiceHelper.InstallWindowsService(); }

        private void notifyIcon1_Click(object sender, EventArgs e)
        {
            if (WindowState != FormWindowState.Minimized)
            { WindowState = FormWindowState.Minimized; }
            Show();
            WindowState = FormWindowState.Normal;
        }

        private void notifyIcon1_DoubleClick(object sender, EventArgs e)
        {
            if (WindowState != FormWindowState.Minimized)
            { WindowState = FormWindowState.Minimized; }
            Show();
            WindowState = FormWindowState.Normal;
        }

        private void StartServer()
        {
            var port = Convert.ToInt32(edPort.Text);
            var serverProv = new BinaryServerFormatterSinkProvider
            { TypeFilterLevel = System.Runtime.Serialization.Formatters.TypeFilterLevel.Full };

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

        private void UninstallService_Click(object sender, EventArgs e)
        { ServiceHelper.UninstallWindowsService(); }

        private void UpdateUiTimer_Tick(object sender, EventArgs e)
        { CheckServices(); }

        private void StartService_Click(object sender, EventArgs e)
        {
            ServiceHelper.StartService();
        }

        private void StopService_Click(object sender, EventArgs e)
        {
            ServiceHelper.StopService();
        }
        }
}
