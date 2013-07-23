using Samba.MessagingServerServiceTool;

namespace Samba.MessagingServerServiceTool
{
    partial class FrmMain
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FrmMain));
            this.label1 = new System.Windows.Forms.Label();
            this.notifyIcon1 = new System.Windows.Forms.NotifyIcon(this.components);
            this.WindowsService = new System.Windows.Forms.GroupBox();
            this.StopService = new System.Windows.Forms.Button();
            this.StartService = new System.Windows.Forms.Button();
            this.UninstallService = new System.Windows.Forms.Button();
            this.InstallService = new System.Windows.Forms.Button();
            this.edPort = new System.Windows.Forms.TextBox();
            this.UpdateUiTimer = new System.Windows.Forms.Timer(this.components);
            this.updatePortBt = new System.Windows.Forms.Button();
            this.WindowsService.SuspendLayout();
            this.SuspendLayout();
            // 
            // label1
            // 
            resources.ApplyResources(this.label1, "label1");
            this.label1.Name = "label1";
            // 
            // notifyIcon1
            // 
            resources.ApplyResources(this.notifyIcon1, "notifyIcon1");
            this.notifyIcon1.Click += new System.EventHandler(this.NotifyIcon1Click);
            this.notifyIcon1.DoubleClick += new System.EventHandler(this.NotifyIcon1DoubleClick);
            // 
            // WindowsService
            // 
            resources.ApplyResources(this.WindowsService, "WindowsService");
            this.WindowsService.Controls.Add(this.StopService);
            this.WindowsService.Controls.Add(this.StartService);
            this.WindowsService.Controls.Add(this.UninstallService);
            this.WindowsService.Controls.Add(this.InstallService);
            this.WindowsService.Name = "WindowsService";
            this.WindowsService.TabStop = false;
            // 
            // StopService
            // 
            resources.ApplyResources(this.StopService, "StopService");
            this.StopService.Name = "StopService";
            this.StopService.UseVisualStyleBackColor = true;
            this.StopService.Click += new System.EventHandler(this.StopServiceClick);
            // 
            // StartService
            // 
            resources.ApplyResources(this.StartService, "StartService");
            this.StartService.Name = "StartService";
            this.StartService.UseVisualStyleBackColor = true;
            this.StartService.Click += new System.EventHandler(this.StartServiceClick);
            // 
            // UninstallService
            // 
            resources.ApplyResources(this.UninstallService, "UninstallService");
            this.UninstallService.Name = "UninstallService";
            this.UninstallService.UseVisualStyleBackColor = true;
            this.UninstallService.Click += new System.EventHandler(this.UninstallServiceClick);
            // 
            // InstallService
            // 
            resources.ApplyResources(this.InstallService, "InstallService");
            this.InstallService.Name = "InstallService";
            this.InstallService.UseVisualStyleBackColor = true;
            this.InstallService.Click += new System.EventHandler(this.InstallServiceClick);
            // 
            // edPort
            // 
            resources.ApplyResources(this.edPort, "edPort");
            this.edPort.Name = "edPort";
            // 
            // UpdateUiTimer
            // 
            this.UpdateUiTimer.Tick += new System.EventHandler(this.UpdateUiTimerTick);
            // 
            // updatePortBt
            // 
            resources.ApplyResources(this.updatePortBt, "updatePortBt");
            this.updatePortBt.Name = "updatePortBt";
            this.updatePortBt.UseVisualStyleBackColor = true;
            this.updatePortBt.Click += new System.EventHandler(this.UpdatePortBtClick);
            // 
            // FrmMain
            // 
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.updatePortBt);
            this.Controls.Add(this.WindowsService);
            this.Controls.Add(this.edPort);
            this.Controls.Add(this.label1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.Name = "FrmMain";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.FrmMainFormClosing);
            this.Shown += new System.EventHandler(this.FrmMainShown);
            this.Resize += new System.EventHandler(this.FrmMainResize);
            this.WindowsService.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox edPort;
        private System.Windows.Forms.NotifyIcon notifyIcon1;
        private System.Windows.Forms.GroupBox WindowsService;
        private System.Windows.Forms.Button InstallService;
        private System.Windows.Forms.Button UninstallService;
        private System.Windows.Forms.Timer UpdateUiTimer;
        private System.Windows.Forms.Button StopService;
        private System.Windows.Forms.Button StartService;
        private System.Windows.Forms.Button updatePortBt;
    }
}

