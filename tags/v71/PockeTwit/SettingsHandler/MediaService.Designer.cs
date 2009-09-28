namespace PockeTwit
{
    partial class MediaService
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
            this.menuAccept = new System.Windows.Forms.MenuItem();
            this.menuCancel = new System.Windows.Forms.MenuItem();
            this.cbPreUpload = new System.Windows.Forms.CheckBox();
            this.cmbMediaService = new System.Windows.Forms.ComboBox();
            this.lblPhotoService = new System.Windows.Forms.Label();
            this.pnlCapabilites = new System.Windows.Forms.Panel();
            this.lblMediaLabel = new System.Windows.Forms.Label();
            this.chkGPS = new System.Windows.Forms.CheckBox();
            this.chkMessage = new System.Windows.Forms.CheckBox();
            this.lblPreLoadText = new System.Windows.Forms.Label();
            this.pnlCapabilites.SuspendLayout();
            this.SuspendLayout();
            // 
            // mainMenu1
            // 
            this.mainMenu1.MenuItems.Add(this.menuAccept);
            this.mainMenu1.MenuItems.Add(this.menuCancel);
            // 
            // menuAccept
            // 
            this.menuAccept.Text = "Accept";
            this.menuAccept.Click += new System.EventHandler(this.menuAccept_Click);
            // 
            // menuCancel
            // 
            this.menuCancel.Text = "Cancel";
            this.menuCancel.Click += new System.EventHandler(this.menuCancel_Click);
            // 
            // cbPreUpload
            // 
            this.cbPreUpload.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.cbPreUpload.ForeColor = System.Drawing.SystemColors.ControlDarkDark;
            this.cbPreUpload.Location = new System.Drawing.Point(3, 113);
            this.cbPreUpload.Name = "cbPreUpload";
            this.cbPreUpload.Size = new System.Drawing.Size(234, 20);
            this.cbPreUpload.TabIndex = 14;
            this.cbPreUpload.Text = "Pre-Upload pictures";
            // 
            // cmbMediaService
            // 
            this.cmbMediaService.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.cmbMediaService.Location = new System.Drawing.Point(99, 3);
            this.cmbMediaService.Name = "cmbMediaService";
            this.cmbMediaService.Size = new System.Drawing.Size(138, 22);
            this.cmbMediaService.TabIndex = 13;
            this.cmbMediaService.SelectedValueChanged += new System.EventHandler(this.cmbMediaService_SelectedValueChanged);
            // 
            // lblPhotoService
            // 
            this.lblPhotoService.ForeColor = System.Drawing.Color.Black;
            this.lblPhotoService.Location = new System.Drawing.Point(3, 5);
            this.lblPhotoService.Name = "lblPhotoService";
            this.lblPhotoService.Size = new System.Drawing.Size(90, 20);
            this.lblPhotoService.Text = "Media Service";
            // 
            // pnlCapabilites
            // 
            this.pnlCapabilites.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.pnlCapabilites.Controls.Add(this.lblMediaLabel);
            this.pnlCapabilites.Controls.Add(this.chkGPS);
            this.pnlCapabilites.Controls.Add(this.chkMessage);
            this.pnlCapabilites.Location = new System.Drawing.Point(20, 29);
            this.pnlCapabilites.Name = "pnlCapabilites";
            this.pnlCapabilites.Size = new System.Drawing.Size(202, 78);
            // 
            // lblMediaLabel
            // 
            this.lblMediaLabel.Location = new System.Drawing.Point(4, 4);
            this.lblMediaLabel.Name = "lblMediaLabel";
            this.lblMediaLabel.Size = new System.Drawing.Size(226, 20);
            this.lblMediaLabel.Text = "This Media service can:";
            // 
            // chkGPS
            // 
            this.chkGPS.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.chkGPS.Enabled = false;
            this.chkGPS.Location = new System.Drawing.Point(3, 54);
            this.chkGPS.Name = "chkGPS";
            this.chkGPS.Size = new System.Drawing.Size(196, 20);
            this.chkGPS.TabIndex = 19;
            this.chkGPS.Text = "include GPS";
            // 
            // chkMessage
            // 
            this.chkMessage.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.chkMessage.Enabled = false;
            this.chkMessage.Location = new System.Drawing.Point(3, 27);
            this.chkMessage.Name = "chkMessage";
            this.chkMessage.Size = new System.Drawing.Size(196, 20);
            this.chkMessage.TabIndex = 18;
            this.chkMessage.Text = "include message";
            // 
            // lblPreLoadText
            // 
            this.lblPreLoadText.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.lblPreLoadText.Location = new System.Drawing.Point(24, 136);
            this.lblPreLoadText.Name = "lblPreLoadText";
            this.lblPreLoadText.Size = new System.Drawing.Size(195, 114);
            this.lblPreLoadText.Text = "Turning this ON will upload pictures instantly to the service without a message. " +
                "A URL to the picture is placed in the message field.";
            // 
            // MediaService
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.AutoScroll = true;
            this.ClientSize = new System.Drawing.Size(240, 268);
            this.Controls.Add(this.lblPreLoadText);
            this.Controls.Add(this.pnlCapabilites);
            this.Controls.Add(this.cbPreUpload);
            this.Controls.Add(this.cmbMediaService);
            this.Controls.Add(this.lblPhotoService);
            this.Menu = this.mainMenu1;
            this.Name = "MediaService";
            this.Text = "Media Service";
            this.pnlCapabilites.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.CheckBox cbPreUpload;
        private System.Windows.Forms.ComboBox cmbMediaService;
        private System.Windows.Forms.Label lblPhotoService;
        private System.Windows.Forms.MenuItem menuAccept;
        private System.Windows.Forms.MenuItem menuCancel;
        private System.Windows.Forms.Panel pnlCapabilites;
        private System.Windows.Forms.CheckBox chkGPS;
        private System.Windows.Forms.CheckBox chkMessage;
        private System.Windows.Forms.Label lblMediaLabel;
        private System.Windows.Forms.Label lblPreLoadText;
    }
}