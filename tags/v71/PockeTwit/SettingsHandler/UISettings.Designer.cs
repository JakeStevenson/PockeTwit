namespace PockeTwit
{
    partial class UISettings
    {

		#region Fields (17) 

        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label lblTweets;
        private System.Windows.Forms.MainMenu mainMenu1;
        private System.Windows.Forms.MenuItem menuAccept;
        private System.Windows.Forms.MenuItem menuCancel;
        private System.Windows.Forms.TextBox txtMaxTweets;

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
            this.label4 = new System.Windows.Forms.Label();
            this.txtMaxTweets = new System.Windows.Forms.TextBox();
            this.lblTweets = new System.Windows.Forms.Label();
            this.chkTimestamps = new System.Windows.Forms.CheckBox();
            this.chkClickables = new System.Windows.Forms.CheckBox();
            this.chkScreenName = new System.Windows.Forms.CheckBox();
            this.cmbTheme = new System.Windows.Forms.ComboBox();
            this.label1 = new System.Windows.Forms.Label();
            this.linkLabel1 = new System.Windows.Forms.LinkLabel();
            this.label2 = new System.Windows.Forms.Label();
            this.txtFontSize = new System.Windows.Forms.TextBox();
            this.chkAutoScrroll = new System.Windows.Forms.CheckBox();
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
            // label4
            // 
            this.label4.BackColor = System.Drawing.Color.Black;
            this.label4.ForeColor = System.Drawing.Color.LightGray;
            this.label4.Location = new System.Drawing.Point(4, 4);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(83, 20);
            this.label4.Text = "Max Length:";
            // 
            // txtMaxTweets
            // 
            this.txtMaxTweets.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.txtMaxTweets.BackColor = System.Drawing.Color.White;
            this.txtMaxTweets.ForeColor = System.Drawing.Color.Black;
            this.txtMaxTweets.Location = new System.Drawing.Point(81, 3);
            this.txtMaxTweets.Name = "txtMaxTweets";
            this.txtMaxTweets.Size = new System.Drawing.Size(73, 21);
            this.txtMaxTweets.TabIndex = 0;
            // 
            // lblTweets
            // 
            this.lblTweets.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lblTweets.BackColor = System.Drawing.Color.Black;
            this.lblTweets.ForeColor = System.Drawing.Color.LightGray;
            this.lblTweets.Location = new System.Drawing.Point(160, 4);
            this.lblTweets.Name = "lblTweets";
            this.lblTweets.Size = new System.Drawing.Size(58, 20);
            this.lblTweets.Text = "(10-200)";
            // 
            // chkTimestamps
            // 
            this.chkTimestamps.ForeColor = System.Drawing.Color.LightGray;
            this.chkTimestamps.Location = new System.Drawing.Point(2, 30);
            this.chkTimestamps.Name = "chkTimestamps";
            this.chkTimestamps.Size = new System.Drawing.Size(216, 20);
            this.chkTimestamps.TabIndex = 1;
            this.chkTimestamps.Text = "Show times";
            // 
            // chkClickables
            // 
            this.chkClickables.ForeColor = System.Drawing.Color.LightGray;
            this.chkClickables.Location = new System.Drawing.Point(2, 56);
            this.chkClickables.Name = "chkClickables";
            this.chkClickables.Size = new System.Drawing.Size(216, 20);
            this.chkClickables.TabIndex = 2;
            this.chkClickables.Text = "Clickable Links";
            // 
            // chkScreenName
            // 
            this.chkScreenName.ForeColor = System.Drawing.Color.LightGray;
            this.chkScreenName.Location = new System.Drawing.Point(2, 82);
            this.chkScreenName.Name = "chkScreenName";
            this.chkScreenName.Size = new System.Drawing.Size(216, 20);
            this.chkScreenName.TabIndex = 3;
            this.chkScreenName.Text = "Include screenname";
            // 
            // cmbTheme
            // 
            this.cmbTheme.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.cmbTheme.Location = new System.Drawing.Point(81, 134);
            this.cmbTheme.Name = "cmbTheme";
            this.cmbTheme.Size = new System.Drawing.Size(102, 22);
            this.cmbTheme.TabIndex = 6;
            this.cmbTheme.SelectedIndexChanged += new System.EventHandler(this.cmbTheme_SelectedIndexChanged);
            // 
            // label1
            // 
            this.label1.BackColor = System.Drawing.Color.Black;
            this.label1.ForeColor = System.Drawing.Color.LightGray;
            this.label1.Location = new System.Drawing.Point(4, 136);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(50, 20);
            this.label1.Text = "Theme:";
            // 
            // linkLabel1
            // 
            this.linkLabel1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.linkLabel1.Location = new System.Drawing.Point(189, 136);
            this.linkLabel1.Name = "linkLabel1";
            this.linkLabel1.Size = new System.Drawing.Size(36, 20);
            this.linkLabel1.TabIndex = 7;
            this.linkLabel1.Text = "Edit";
            this.linkLabel1.Click += new System.EventHandler(this.linkLabel1_Click);
            // 
            // label2
            // 
            this.label2.ForeColor = System.Drawing.Color.White;
            this.label2.Location = new System.Drawing.Point(4, 164);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(66, 20);
            this.label2.Text = "Font Size:";
            // 
            // txtFontSize
            // 
            this.txtFontSize.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.txtFontSize.Location = new System.Drawing.Point(81, 163);
            this.txtFontSize.Name = "txtFontSize";
            this.txtFontSize.Size = new System.Drawing.Size(102, 21);
            this.txtFontSize.TabIndex = 8;
            this.txtFontSize.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.txtFontSize_KeyPress);
            // 
            // chkAutoScrroll
            // 
            this.chkAutoScrroll.ForeColor = System.Drawing.Color.LightGray;
            this.chkAutoScrroll.Location = new System.Drawing.Point(2, 108);
            this.chkAutoScrroll.Name = "chkAutoScrroll";
            this.chkAutoScrroll.Size = new System.Drawing.Size(216, 20);
            this.chkAutoScrroll.TabIndex = 5;
            this.chkAutoScrroll.Text = "Auto-Scroll To Top";
            // 
            // UISettings
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.AutoScroll = true;
            this.BackColor = System.Drawing.Color.Black;
            this.ClientSize = new System.Drawing.Size(240, 268);
            this.Controls.Add(this.chkAutoScrroll);
            this.Controls.Add(this.txtFontSize);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.linkLabel1);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.cmbTheme);
            this.Controls.Add(this.chkScreenName);
            this.Controls.Add(this.chkClickables);
            this.Controls.Add(this.chkTimestamps);
            this.Controls.Add(this.lblTweets);
            this.Controls.Add(this.txtMaxTweets);
            this.Controls.Add(this.label4);
            this.Menu = this.mainMenu1;
            this.Name = "UISettings";
            this.Text = "UI Settings";
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.CheckBox chkTimestamps;
        private System.Windows.Forms.CheckBox chkClickables;
        private System.Windows.Forms.CheckBox chkScreenName;
        private System.Windows.Forms.ComboBox cmbTheme;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.LinkLabel linkLabel1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox txtFontSize;
        private System.Windows.Forms.CheckBox chkAutoScrroll;

    }
}