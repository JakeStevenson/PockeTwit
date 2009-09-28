namespace FingerUI
{
    partial class FullScreenTweet
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

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.avatarBox = new System.Windows.Forms.PictureBox();
            this.lblUserName = new System.Windows.Forms.Label();
            this.lblTime = new System.Windows.Forms.Label();
            this.lblSource = new System.Windows.Forms.Label();
            this.lblText = new System.Windows.Forms.Label();
            this.lnkDismiss = new System.Windows.Forms.LinkLabel();
            this.SuspendLayout();
            // 
            // avatarBox
            // 
            this.avatarBox.Location = new System.Drawing.Point(3, 3);
            this.avatarBox.Name = "avatarBox";
            this.avatarBox.Size = new System.Drawing.Size(83, 71);
            // 
            // lblUserName
            // 
            this.lblUserName.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.lblUserName.Location = new System.Drawing.Point(92, 4);
            this.lblUserName.Name = "lblUserName";
            this.lblUserName.Size = new System.Drawing.Size(127, 20);
            this.lblUserName.Text = "label1";
            // 
            // lblTime
            // 
            this.lblTime.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.lblTime.Location = new System.Drawing.Point(93, 28);
            this.lblTime.Name = "lblTime";
            this.lblTime.Size = new System.Drawing.Size(125, 20);
            this.lblTime.Text = "label2";
            // 
            // lblSource
            // 
            this.lblSource.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.lblSource.Location = new System.Drawing.Point(92, 53);
            this.lblSource.Name = "lblSource";
            this.lblSource.Size = new System.Drawing.Size(125, 20);
            this.lblSource.Text = "label3";
            // 
            // lblText
            // 
            this.lblText.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.lblText.Location = new System.Drawing.Point(4, 81);
            this.lblText.Name = "lblText";
            this.lblText.Size = new System.Drawing.Size(213, 164);
            this.lblText.Text = "label1";
            // 
            // lnkDismiss
            // 
            this.lnkDismiss.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.lnkDismiss.Location = new System.Drawing.Point(4, 245);
            this.lnkDismiss.Name = "lnkDismiss";
            this.lnkDismiss.Size = new System.Drawing.Size(212, 20);
            this.lnkDismiss.TabIndex = 5;
            this.lnkDismiss.Text = "Close";
            this.lnkDismiss.TextAlign = System.Drawing.ContentAlignment.TopRight;
            this.lnkDismiss.Click += new System.EventHandler(this.lnkDismiss_Click);
            // 
            // FullScreenTweet
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.Controls.Add(this.lnkDismiss);
            this.Controls.Add(this.lblText);
            this.Controls.Add(this.lblSource);
            this.Controls.Add(this.lblTime);
            this.Controls.Add(this.lblUserName);
            this.Controls.Add(this.avatarBox);
            this.Name = "FullScreenTweet";
            this.Size = new System.Drawing.Size(222, 268);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.PictureBox avatarBox;
        private System.Windows.Forms.Label lblUserName;
        private System.Windows.Forms.Label lblTime;
        private System.Windows.Forms.Label lblSource;
        private System.Windows.Forms.Label lblText;
        private System.Windows.Forms.LinkLabel lnkDismiss;
    }
}
