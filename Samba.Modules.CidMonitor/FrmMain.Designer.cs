namespace Samba.Modules.CidMonitor
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FrmMain));
            this.axCIDv51 = new Axcidv5callerid.AxCIDv5();
            ((System.ComponentModel.ISupportInitialize)(this.axCIDv51)).BeginInit();
            this.SuspendLayout();
            // 
            // axCIDv51
            // 
            this.axCIDv51.Location = new System.Drawing.Point(0, 0);
            this.axCIDv51.Name = "axCIDv51";
            this.axCIDv51.OcxState = ((System.Windows.Forms.AxHost.State)(resources.GetObject("axCIDv51.OcxState")));
            this.axCIDv51.Size = new System.Drawing.Size(26, 26);
            this.axCIDv51.TabIndex = 0;
            // 
            // FrmMain
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(116, 42);
            this.Controls.Add(this.axCIDv51);
            this.Name = "FrmMain";
            this.Text = "FrmMain";
            this.Load += new System.EventHandler(this.FrmMain_Load);
            ((System.ComponentModel.ISupportInitialize)(this.axCIDv51)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        public Axcidv5callerid.AxCIDv5 axCIDv51;

    }
}