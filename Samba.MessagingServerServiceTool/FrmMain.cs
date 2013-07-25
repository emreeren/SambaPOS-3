using System;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Tcp;
using System.ServiceProcess;
using System.Threading;
using System.Windows.Forms;
using Samba.MessagingServer.WindowsService;

namespace Samba.MessagingServerServiceTool
{
    public partial class FrmMain : Form
    {
        private static TcpChannel _channel;

        public FrmMain()
        {
            InitializeComponent();
            DisableAll();
            edPort.Text = ServiceHelper.MessagingServerPort.ToString();
            CheckServices();
            UpdateUiTimer.Start();
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
                updatePortBt.Enabled = false;
            }
            else
            {
                InstallService.Enabled = false;
                updatePortBt.Enabled = true;
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

        private void DisableAll()
        {
            InstallService.Enabled = false;
            UninstallService.Enabled = false;
            StopService.Enabled = false;
            StartService.Enabled = false;
            updatePortBt.Enabled = false;
        }

        private void FrmMainFormClosing(object sender, FormClosingEventArgs e)
        {
            if (_channel != null)
            { ChannelServices.UnregisterChannel(_channel); }
        }

        private void FrmMainResize(object sender, EventArgs e)
        {
            if (WindowState == FormWindowState.Minimized)
            { Hide(); }
            else
            { Show(); }
        }

        private void FrmMainShown(object sender, EventArgs e)
        { }

        private void InstallServiceClick(object sender, EventArgs e)
        {
            DisableAll();
            ServiceHelper.InstallWindowsService();
        }

        private void NotifyIcon1Click(object sender, EventArgs e)
        {
            if (WindowState != FormWindowState.Minimized)
            { WindowState = FormWindowState.Minimized; }
            Show();
            WindowState = FormWindowState.Normal;
        }

        private void NotifyIcon1DoubleClick(object sender, EventArgs e)
        {
            if (WindowState != FormWindowState.Minimized)
            { WindowState = FormWindowState.Minimized; }
            Show();
            WindowState = FormWindowState.Normal;
        }

        private void StartServiceClick(object sender, EventArgs e)
        {
            DisableAll();
            ServiceHelper.StartService();
        }

        private void StopServiceClick(object sender, EventArgs e)
        {
            DisableAll();
            ServiceHelper.StopService();
        }

        private void UninstallServiceClick(object sender, EventArgs e)
        {
            DisableAll();
            ServiceHelper.UninstallWindowsService();
        }

        private void UpdatePortBtClick(object sender, EventArgs e)
        {
            DisableAll();
            ServiceHelper.StopService();
            while (ServiceHelper.CheckServiceStatus() == ServiceControllerStatus.StopPending)
            { Thread.Sleep(100); }
            ServiceHelper.StartService(new string[] { string.Format("Port={0}", edPort.Text) });
        }

        private void UpdateUiTimerTick(object sender, EventArgs e)
        { CheckServices(); }
    }
}
