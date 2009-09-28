namespace PockeTwit.SettingsHandler
{
    partial class NotificationSettings
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
            this.mnuDone = new System.Windows.Forms.MenuItem();
            this.label1 = new System.Windows.Forms.Label();
            this.cmbNotificationType = new System.Windows.Forms.ComboBox();
            this.chkPlaySound = new System.Windows.Forms.CheckBox();
            this.cmbSound = new System.Windows.Forms.ComboBox();
            this.lblSound = new System.Windows.Forms.Label();
            this.chkVibrate = new System.Windows.Forms.CheckBox();
            this.chkNotification = new System.Windows.Forms.CheckBox();
            this.lnkPlay = new System.Windows.Forms.LinkLabel();
            this.SuspendLayout();
            // 
            // mainMenu1
            // 
            this.mainMenu1.MenuItems.Add(this.mnuDone);
            // 
            // mnuDone
            // 
            this.mnuDone.Text = "Done";
            this.mnuDone.Click += new System.EventHandler(this.mnuDone_Click);
            // 
            // label1
            // 
            this.label1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.label1.Location = new System.Drawing.Point(4, 4);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(233, 20);
            this.label1.Text = "Notification Type:";
            // 
            // cmbNotificationType
            // 
            this.cmbNotificationType.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.cmbNotificationType.Location = new System.Drawing.Point(4, 28);
            this.cmbNotificationType.Name = "cmbNotificationType";
            this.cmbNotificationType.Size = new System.Drawing.Size(233, 22);
            this.cmbNotificationType.TabIndex = 1;
            this.cmbNotificationType.SelectedIndexChanged += new System.EventHandler(this.cmbNotificationType_SelectedIndexChanged);
            // 
            // chkPlaySound
            // 
            this.chkPlaySound.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.chkPlaySound.Location = new System.Drawing.Point(0, 80);
            this.chkPlaySound.Name = "chkPlaySound";
            this.chkPlaySound.Size = new System.Drawing.Size(100, 20);
            this.chkPlaySound.TabIndex = 2;
            this.chkPlaySound.Text = "Play Sound";
            this.chkPlaySound.CheckStateChanged += new System.EventHandler(this.chkPlaySound_CheckStateChanged);
            this.chkPlaySound.Click += new System.EventHandler(this.chkPlaySound_Click);
            // 
            // cmbSound
            // 
            this.cmbSound.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.cmbSound.Location = new System.Drawing.Point(4, 126);
            this.cmbSound.Name = "cmbSound";
            this.cmbSound.Size = new System.Drawing.Size(197, 22);
            this.cmbSound.TabIndex = 3;
            this.cmbSound.SelectedIndexChanged += new System.EventHandler(this.cmbSound_SelectedIndexChanged);
            // 
            // lblSound
            // 
            this.lblSound.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.lblSound.Location = new System.Drawing.Point(3, 103);
            this.lblSound.Name = "lblSound";
            this.lblSound.Size = new System.Drawing.Size(233, 20);
            this.lblSound.Text = "Sound";
            // 
            // chkVibrate
            // 
            this.chkVibrate.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.chkVibrate.Location = new System.Drawing.Point(0, 154);
            this.chkVibrate.Name = "chkVibrate";
            this.chkVibrate.Size = new System.Drawing.Size(236, 20);
            this.chkVibrate.TabIndex = 5;
            this.chkVibrate.Text = "Vibrate";
            this.chkVibrate.Click += new System.EventHandler(this.chkVibrate_Click);
            // 
            // chkNotification
            // 
            this.chkNotification.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.chkNotification.Location = new System.Drawing.Point(0, 180);
            this.chkNotification.Name = "chkNotification";
            this.chkNotification.Size = new System.Drawing.Size(236, 20);
            this.chkNotification.TabIndex = 8;
            this.chkNotification.Text = "Show Notification";
            this.chkNotification.Visible = false;
            this.chkNotification.Click += new System.EventHandler(this.chkNotification_Click);
            // 
            // lnkPlay
            // 
            this.lnkPlay.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lnkPlay.Location = new System.Drawing.Point(207, 126);
            this.lnkPlay.Name = "lnkPlay";
            this.lnkPlay.Size = new System.Drawing.Size(29, 20);
            this.lnkPlay.TabIndex = 11;
            this.lnkPlay.Text = "Play";
            this.lnkPlay.Click += new System.EventHandler(this.lnkPlay_Click);
            // 
            // NotificationSettings
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.AutoScroll = true;
            this.ClientSize = new System.Drawing.Size(240, 268);
            this.Controls.Add(this.lnkPlay);
            this.Controls.Add(this.chkNotification);
            this.Controls.Add(this.chkVibrate);
            this.Controls.Add(this.lblSound);
            this.Controls.Add(this.cmbSound);
            this.Controls.Add(this.chkPlaySound);
            this.Controls.Add(this.cmbNotificationType);
            this.Controls.Add(this.label1);
            this.Menu = this.mainMenu1;
            this.Name = "NotificationSettings";
            this.Text = "Notification Settings";
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.ComboBox cmbNotificationType;
        private System.Windows.Forms.CheckBox chkPlaySound;
        private System.Windows.Forms.ComboBox cmbSound;
        private System.Windows.Forms.Label lblSound;
        private System.Windows.Forms.CheckBox chkVibrate;
        private System.Windows.Forms.MenuItem mnuDone;
        private System.Windows.Forms.CheckBox chkNotification;
        private System.Windows.Forms.LinkLabel lnkPlay;
    }
}