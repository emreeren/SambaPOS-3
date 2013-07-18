namespace Samba.MessagingServer
{
    partial class frmMain
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(frmMain));
            this.btnStart = new System.Windows.Forms.Button();
            this.btnStop = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.lbStatus = new System.Windows.Forms.Label();
            this.notifyIcon1 = new System.Windows.Forms.NotifyIcon(this.components);
            this.WindowsService = new System.Windows.Forms.GroupBox();
            this.UninstallService = new System.Windows.Forms.Button();
            this.InstallService = new System.Windows.Forms.Button();
            this.checkBox1 = new System.Windows.Forms.CheckBox();
            this.edPort = new System.Windows.Forms.TextBox();
            this.UpdateUiTimer = new System.Windows.Forms.Timer(this.components);
            this.StartService = new System.Windows.Forms.Button();
            this.StopService = new System.Windows.Forms.Button();
            this.WindowsService.SuspendLayout();
            this.SuspendLayout();
            // 
            // btnStart
            // 
            resources.ApplyResources(this.btnStart, "btnStart");
            this.btnStart.Name = "btnStart";
            this.btnStart.UseVisualStyleBackColor = true;
            this.btnStart.Click += new System.EventHandler(this.button1_Click);
            // 
            // btnStop
            // 
            resources.ApplyResources(this.btnStop, "btnStop");
            this.btnStop.Name = "btnStop";
            this.btnStop.UseVisualStyleBackColor = true;
            this.btnStop.Click += new System.EventHandler(this.button2_Click);
            // 
            // label1
            // 
            resources.ApplyResources(this.label1, "label1");
            this.label1.Name = "label1";
            // 
            // lbStatus
            // 
            resources.ApplyResources(this.lbStatus, "lbStatus");
            this.lbStatus.Name = "lbStatus";
            // 
            // notifyIcon1
            // 
            resources.ApplyResources(this.notifyIcon1, "notifyIcon1");
            this.notifyIcon1.Click += new System.EventHandler(this.notifyIcon1_Click);
            this.notifyIcon1.DoubleClick += new System.EventHandler(this.notifyIcon1_DoubleClick);
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
            // UninstallService
            // 
            resources.ApplyResources(this.UninstallService, "UninstallService");
            this.UninstallService.Name = "UninstallService";
            this.UninstallService.UseVisualStyleBackColor = true;
            this.UninstallService.Click += new System.EventHandler(this.UninstallService_Click);
            // 
            // InstallService
            // 
            resources.ApplyResources(this.InstallService, "InstallService");
            this.InstallService.Name = "InstallService";
            this.InstallService.UseVisualStyleBackColor = true;
            this.InstallService.Click += new System.EventHandler(this.InstallService_Click);
            // 
            // checkBox1
            // 
            resources.ApplyResources(this.checkBox1, "checkBox1");
            this.checkBox1.Checked = global::Samba.MessagingServer.Properties.Settings.Default.AutoStartServer;
            this.checkBox1.DataBindings.Add(new System.Windows.Forms.Binding("Checked", global::Samba.MessagingServer.Properties.Settings.Default, "AutoStartServer", true, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged));
            this.checkBox1.Name = "checkBox1";
            this.checkBox1.UseVisualStyleBackColor = true;
            // 
            // edPort
            // 
            this.edPort.DataBindings.Add(new System.Windows.Forms.Binding("Text", global::Samba.MessagingServer.Properties.Settings.Default, "MessageServerPort", true, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged));
            resources.ApplyResources(this.edPort, "edPort");
            this.edPort.Name = "edPort";
            this.edPort.Text = global::Samba.MessagingServer.Properties.Settings.Default.MessageServerPort;
            // 
            // UpdateUiTimer
            // 
            this.UpdateUiTimer.Tick += new System.EventHandler(this.UpdateUiTimer_Tick);
            // 
            // StartService
            // 
            resources.ApplyResources(this.StartService, "StartService");
            this.StartService.Name = "StartService";
            this.StartService.UseVisualStyleBackColor = true;
            this.StartService.Click += new System.EventHandler(this.StartService_Click);
            // 
            // StopService
            // 
            resources.ApplyResources(this.StopService, "StopService");
            this.StopService.Name = "StopService";
            this.StopService.UseVisualStyleBackColor = true;
            this.StopService.Click += new System.EventHandler(this.StopService_Click);
            // 
            // frmMain
            // 
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.WindowsService);
            this.Controls.Add(this.checkBox1);
            this.Controls.Add(this.lbStatus);
            this.Controls.Add(this.edPort);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.btnStop);
            this.Controls.Add(this.btnStart);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.Name = "frmMain";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.frmMain_FormClosing);
            this.Shown += new System.EventHandler(this.frmMain_Shown);
            this.Resize += new System.EventHandler(this.frmMain_Resize);
            this.WindowsService.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button btnStart;
        private System.Windows.Forms.Button btnStop;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox edPort;
        private System.Windows.Forms.Label lbStatus;
        private System.Windows.Forms.NotifyIcon notifyIcon1;
        private System.Windows.Forms.CheckBox checkBox1;
        private System.Windows.Forms.GroupBox WindowsService;
        private System.Windows.Forms.Button InstallService;
        private System.Windows.Forms.Button UninstallService;
        private System.Windows.Forms.Timer UpdateUiTimer;
        private System.Windows.Forms.Button StopService;
        private System.Windows.Forms.Button StartService;
    }
}

