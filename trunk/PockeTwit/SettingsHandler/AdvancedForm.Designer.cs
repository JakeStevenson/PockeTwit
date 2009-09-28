namespace PockeTwit.SettingsHandler
{
    partial class AdvancedForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;
        private System.Windows.Forms.MainMenu mainMenu1;

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
            this.mainMenu1 = new System.Windows.Forms.MainMenu();
            this.menuItem1 = new System.Windows.Forms.MenuItem();
            this.lnkClearCaches = new System.Windows.Forms.LinkLabel();
            this.lnkClearSettings = new System.Windows.Forms.LinkLabel();
            this.chkDIB = new System.Windows.Forms.CheckBox();
            this.lblRenderingMethod = new System.Windows.Forms.Label();
            this.lblCompact = new System.Windows.Forms.LinkLabel();
            this.SuspendLayout();
            // 
            // mainMenu1
            // 
            this.mainMenu1.MenuItems.Add(this.menuItem1);
            // 
            // menuItem1
            // 
            this.menuItem1.Text = "Done";
            this.menuItem1.Click += new System.EventHandler(this.menuItem1_Click);
            // 
            // lnkClearCaches
            // 
            this.lnkClearCaches.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.lnkClearCaches.Location = new System.Drawing.Point(3, 0);
            this.lnkClearCaches.Name = "lnkClearCaches";
            this.lnkClearCaches.Size = new System.Drawing.Size(233, 20);
            this.lnkClearCaches.TabIndex = 1;
            this.lnkClearCaches.Text = "Clear Cached statuses";
            this.lnkClearCaches.Click += new System.EventHandler(this.lnkClearCaches_Click);
            // 
            // lnkClearSettings
            // 
            this.lnkClearSettings.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.lnkClearSettings.Location = new System.Drawing.Point(3, 20);
            this.lnkClearSettings.Name = "lnkClearSettings";
            this.lnkClearSettings.Size = new System.Drawing.Size(233, 20);
            this.lnkClearSettings.TabIndex = 2;
            this.lnkClearSettings.Text = "Clear Application Settings";
            this.lnkClearSettings.Click += new System.EventHandler(this.lnkClearSettings_Click);
            // 
            // chkDIB
            // 
            this.chkDIB.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.chkDIB.Location = new System.Drawing.Point(-1, 96);
            this.chkDIB.Name = "chkDIB";
            this.chkDIB.Size = new System.Drawing.Size(161, 20);
            this.chkDIB.TabIndex = 3;
            this.chkDIB.Text = "Alternate rendering";
            this.chkDIB.CheckStateChanged += new System.EventHandler(this.chkDIB_CheckStateChanged);
            // 
            // lblRenderingMethod
            // 
            this.lblRenderingMethod.Location = new System.Drawing.Point(3, 73);
            this.lblRenderingMethod.Name = "lblRenderingMethod";
            this.lblRenderingMethod.Size = new System.Drawing.Size(100, 20);
            this.lblRenderingMethod.Text = "lblDI";
            // 
            // lblCompact
            // 
            this.lblCompact.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.lblCompact.Location = new System.Drawing.Point(3, 40);
            this.lblCompact.Name = "lblCompact";
            this.lblCompact.Size = new System.Drawing.Size(233, 20);
            this.lblCompact.TabIndex = 4;
            this.lblCompact.Text = "Compact Database";
            this.lblCompact.Click += new System.EventHandler(this.lblCompact_Click);
            // 
            // AdvancedForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.AutoScroll = true;
            this.ClientSize = new System.Drawing.Size(240, 268);
            this.Controls.Add(this.lblCompact);
            this.Controls.Add(this.lblRenderingMethod);
            this.Controls.Add(this.chkDIB);
            this.Controls.Add(this.lnkClearSettings);
            this.Controls.Add(this.lnkClearCaches);
            this.Menu = this.mainMenu1;
            this.Name = "AdvancedForm";
            this.Text = "AdvancedForm";
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.LinkLabel lnkClearCaches;
        private System.Windows.Forms.LinkLabel lnkClearSettings;
        private System.Windows.Forms.MenuItem menuItem1;
        private System.Windows.Forms.CheckBox chkDIB;
        private System.Windows.Forms.Label lblRenderingMethod;
        private System.Windows.Forms.LinkLabel lblCompact;
    }
}