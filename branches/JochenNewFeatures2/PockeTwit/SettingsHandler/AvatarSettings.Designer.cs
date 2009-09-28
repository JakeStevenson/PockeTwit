namespace PockeTwit
{
    partial class AvatarSettings
    {

		#region Fields (17) 

        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;
        private System.Windows.Forms.MainMenu mainMenu1;
        private System.Windows.Forms.MenuItem menuAccept;
        private System.Windows.Forms.MenuItem menuCancel;

		#endregion Fields 

		#region Methods (1) 


		// Protected Methods (1) 

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


		#endregion Methods 


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
            this.lnkClearAvatars = new System.Windows.Forms.LinkLabel();
            this.chkAvatar = new System.Windows.Forms.CheckBox();
            this.chkHighQuality = new System.Windows.Forms.CheckBox();
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
            // lnkClearAvatars
            // 
            this.lnkClearAvatars.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.lnkClearAvatars.ForeColor = System.Drawing.Color.LightBlue;
            this.lnkClearAvatars.Location = new System.Drawing.Point(3, 248);
            this.lnkClearAvatars.Name = "lnkClearAvatars";
            this.lnkClearAvatars.Size = new System.Drawing.Size(232, 20);
            this.lnkClearAvatars.TabIndex = 3;
            this.lnkClearAvatars.Text = "Clear Avatar Cache";
            this.lnkClearAvatars.TextAlign = System.Drawing.ContentAlignment.TopRight;
            this.lnkClearAvatars.Click += new System.EventHandler(this.lnkClearAvatars_Click);
            // 
            // chkAvatar
            // 
            this.chkAvatar.ForeColor = System.Drawing.Color.LightGray;
            this.chkAvatar.Location = new System.Drawing.Point(2, 3);
            this.chkAvatar.Name = "chkAvatar";
            this.chkAvatar.Size = new System.Drawing.Size(235, 20);
            this.chkAvatar.TabIndex = 0;
            this.chkAvatar.Text = "Show Avatars";
            // 
            // chkHighQuality
            // 
            this.chkHighQuality.ForeColor = System.Drawing.Color.LightGray;
            this.chkHighQuality.Location = new System.Drawing.Point(3, 29);
            this.chkHighQuality.Name = "chkHighQuality";
            this.chkHighQuality.Size = new System.Drawing.Size(235, 20);
            this.chkHighQuality.TabIndex = 1;
            this.chkHighQuality.Text = "High-Quality Avatars";
            // 
            // AvatarSettings
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.AutoScroll = true;
            this.BackColor = System.Drawing.Color.Black;
            this.ClientSize = new System.Drawing.Size(240, 268);
            this.Controls.Add(this.chkHighQuality);
            this.Controls.Add(this.chkAvatar);
            this.Controls.Add(this.lnkClearAvatars);
            this.Menu = this.mainMenu1;
            this.Name = "AvatarSettings";
            this.Text = "Avatar Settings";
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.LinkLabel lnkClearAvatars;
        private System.Windows.Forms.CheckBox chkAvatar;
        private System.Windows.Forms.CheckBox chkHighQuality;

    }
}