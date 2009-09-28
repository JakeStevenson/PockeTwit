namespace PockeTwit
{
    partial class ProfileMap
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
            this.menuItem2 = new System.Windows.Forms.MenuItem();
            this.menuZoomOut = new System.Windows.Forms.MenuItem();
            this.menuZoomIn = new System.Windows.Forms.MenuItem();
            this.menuItem3 = new System.Windows.Forms.MenuItem();
            this.menuItem5 = new System.Windows.Forms.MenuItem();
            this.menuItem4 = new System.Windows.Forms.MenuItem();
            this.myPictureBox = new System.Windows.Forms.PictureBox();
            this.pictureBoxJump = new System.Windows.Forms.PictureBox();
            this.txtJump = new System.Windows.Forms.TextBox();
            this.lblJump = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // mainMenu1
            // 
            this.mainMenu1.MenuItems.Add(this.menuItem1);
            this.mainMenu1.MenuItems.Add(this.menuItem2);
            // 
            // menuItem1
            // 
            this.menuItem1.Text = "Close Map";
            this.menuItem1.Click += new System.EventHandler(this.menuItem1_Click);
            // 
            // menuItem2
            // 
            this.menuItem2.MenuItems.Add(this.menuZoomOut);
            this.menuItem2.MenuItems.Add(this.menuZoomIn);
            this.menuItem2.MenuItems.Add(this.menuItem3);
            this.menuItem2.MenuItems.Add(this.menuItem5);
            this.menuItem2.MenuItems.Add(this.menuItem4);
            this.menuItem2.Text = "Actions";
            // 
            // menuZoomOut
            // 
            this.menuZoomOut.Text = "Zoom Out";
            this.menuZoomOut.Click += new System.EventHandler(this.menuItem4_Click);
            // 
            // menuZoomIn
            // 
            this.menuZoomIn.Text = "Zoom In";
            this.menuZoomIn.Click += new System.EventHandler(this.menuZoomIn_Click);
            // 
            // menuItem3
            // 
            this.menuItem3.Text = "-";
            // 
            // menuItem5
            // 
            this.menuItem5.Text = "Jump To...";
            this.menuItem5.Click += new System.EventHandler(this.menuItem5_Click);
            // 
            // menuItem4
            // 
            this.menuItem4.Text = "Search Here";
            this.menuItem4.Click += new System.EventHandler(this.menuItem4_Click_1);
            // 
            // myPictureBox
            // 
            this.myPictureBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.myPictureBox.Location = new System.Drawing.Point(0, 0);
            this.myPictureBox.Name = "myPictureBox";
            this.myPictureBox.Size = new System.Drawing.Size(240, 268);
            // 
            // pictureBoxJump
            // 
            this.pictureBoxJump.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.pictureBoxJump.Location = new System.Drawing.Point(21, 82);
            this.pictureBoxJump.Name = "pictureBoxJump";
            this.pictureBoxJump.Size = new System.Drawing.Size(198, 59);
            this.pictureBoxJump.Visible = false;
            // 
            // txtJump
            // 
            this.txtJump.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.txtJump.Location = new System.Drawing.Point(30, 110);
            this.txtJump.Name = "txtJump";
            this.txtJump.Size = new System.Drawing.Size(176, 21);
            this.txtJump.TabIndex = 2;
            this.txtJump.Visible = false;
            this.txtJump.KeyUp += new System.Windows.Forms.KeyEventHandler(this.txtJump_KeyUp);
            this.txtJump.LostFocus += new System.EventHandler(this.txtJump_LostFocus);
            // 
            // lblJump
            // 
            this.lblJump.Location = new System.Drawing.Point(30, 87);
            this.lblJump.Name = "lblJump";
            this.lblJump.Size = new System.Drawing.Size(100, 20);
            this.lblJump.Text = "Jump To:";
            this.lblJump.Visible = false;
            // 
            // ProfileMap
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.AutoScroll = true;
            this.ClientSize = new System.Drawing.Size(240, 268);
            this.Controls.Add(this.lblJump);
            this.Controls.Add(this.txtJump);
            this.Controls.Add(this.pictureBoxJump);
            this.Controls.Add(this.myPictureBox);
            this.Menu = this.mainMenu1;
            this.Name = "ProfileMap";
            this.Text = "ProfileMap";
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.PictureBox myPictureBox;
        private System.Windows.Forms.MenuItem menuItem1;
        private System.Windows.Forms.MenuItem menuItem2;
        private System.Windows.Forms.MenuItem menuZoomOut;
        private System.Windows.Forms.MenuItem menuZoomIn;
        private System.Windows.Forms.MenuItem menuItem3;
        private System.Windows.Forms.MenuItem menuItem4;
        private System.Windows.Forms.PictureBox pictureBoxJump;
        private System.Windows.Forms.TextBox txtJump;
        private System.Windows.Forms.Label lblJump;
        private System.Windows.Forms.MenuItem menuItem5;
    }
}