namespace PockeTwit
{
    partial class TweetList
    {

		#region Fields (8) 

        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;
        private System.Windows.Forms.Label lblTitle;
        private FingerUI.KListControl statList;
        
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
            this.lblTitle = new System.Windows.Forms.Label();
            this.mainMenu1 = new System.Windows.Forms.MainMenu();
            this.globalMenu = new System.Windows.Forms.MenuItem();
            this.specificMenu = new System.Windows.Forms.MenuItem();
            this.progressBar1 = new System.Windows.Forms.ProgressBar();
            this.statList = new PockeTwit.FingerUI.KListControl();
            this.lblProgress = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // lblTitle
            // 
            this.lblTitle.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.lblTitle.Font = new System.Drawing.Font("Tahoma", 12F, System.Drawing.FontStyle.Regular);
            this.lblTitle.ForeColor = System.Drawing.Color.LightGray;
            this.lblTitle.Location = new System.Drawing.Point(0, 0);
            this.lblTitle.Name = "lblTitle";
            this.lblTitle.Size = new System.Drawing.Size(240, 31);
            this.lblTitle.Text = PockeTwit.Localization.XmlBasedResourceManager.GetString("Launching PockeTwit", "Launching PockeTwit");
            this.lblTitle.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // mainMenu1
            // 
            this.mainMenu1.MenuItems.Add(this.globalMenu);
            this.mainMenu1.MenuItems.Add(this.specificMenu);
            // 
            // globalMenu
            // 
            this.globalMenu.Text = "Global";
            this.globalMenu.Click += new System.EventHandler(this.globalMenu_Click);
            // 
            // specificMenu
            // 
            this.specificMenu.Text = "Specific";
            this.specificMenu.Click += new System.EventHandler(this.specificMenu_Click);
            // 
            // progressBar1
            // 
            this.progressBar1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.progressBar1.Location = new System.Drawing.Point(23, 34);
            this.progressBar1.Name = "progressBar1";
            this.progressBar1.Size = new System.Drawing.Size(193, 15);
            // 
            // statList
            // 
            this.statList.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.statList.Location = new System.Drawing.Point(0, 0);
            this.statList.Name = "statList";
            this.statList.Size = new System.Drawing.Size(240, 268);
            this.statList.TabIndex = 0;
            this.statList.Visible = false;
            // 
            // lblProgress
            // 
            this.lblProgress.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.lblProgress.Location = new System.Drawing.Point(4, 35);
            this.lblProgress.Name = "lblProgress";
            this.lblProgress.Size = new System.Drawing.Size(233, 20);
            this.lblProgress.Visible = false;
            // 
            // TweetList
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.AutoScroll = true;
            this.ClientSize = new System.Drawing.Size(240, 268);
            this.Controls.Add(this.lblProgress);
            this.Controls.Add(this.progressBar1);
            this.Controls.Add(this.lblTitle);
            this.Controls.Add(this.statList);
            this.Name = "TweetList";
            this.Text = "PockeTwit";
            this.LostFocus += new System.EventHandler(this.TweetList_LostFocus);
            this.ResumeLayout(false);

        }

        #endregion

        private Microsoft.WindowsCE.Forms.InputPanel inputPanel1;
        private System.Windows.Forms.MainMenu mainMenu1;
        private System.Windows.Forms.MenuItem globalMenu;
        private System.Windows.Forms.MenuItem specificMenu;
        private System.Windows.Forms.ProgressBar progressBar1;
        private System.Windows.Forms.Label lblProgress;
    }
}

