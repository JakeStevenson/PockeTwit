using System.Windows.Forms;
namespace PockeTwit
{
    partial class PostUpdate
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
            this.menuCancel = new System.Windows.Forms.MenuItem();
            this.menuSubmit = new System.Windows.Forms.MenuItem();
            this.cmbAccount = new System.Windows.Forms.ComboBox();
            this.lblFromAccount = new System.Windows.Forms.Label();
            this.pictureURL = new System.Windows.Forms.PictureBox();
            this.pictureFromStorage = new System.Windows.Forms.PictureBox();
            this.pictureFromCamers = new System.Windows.Forms.PictureBox();
            this.txtStatusUpdate = new System.Windows.Forms.TextBox();
            this.lblCharsLeft = new System.Windows.Forms.Label();
            this.pictureLocation = new System.Windows.Forms.PictureBox();
            this.lblGPS = new System.Windows.Forms.Label();
            this.picAddressBook = new System.Windows.Forms.PictureBox();
            this.userListControl1 = new PockeTwit.userListControl();
            this.SuspendLayout();
            // 
            // menuCancel
            // 
            this.menuCancel.Text = "Cancel";
            this.menuCancel.Click += new System.EventHandler(this.menuCancel_Click);
            // 
            // menuSubmit
            // 
            this.menuSubmit.Text = "Submit";
            this.menuSubmit.Click += new System.EventHandler(this.menuSubmit_Click);
            // 
            // cmbAccount
            // 
            this.cmbAccount.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.cmbAccount.Location = new System.Drawing.Point(104, 3);
            this.cmbAccount.Name = "cmbAccount";
            this.cmbAccount.Size = new System.Drawing.Size(133, 22);
            this.cmbAccount.TabIndex = 0;
            this.cmbAccount.SelectedIndexChanged += new System.EventHandler(this.cmbAccount_SelectedIndexChanged);
            // 
            // lblFromAccount
            // 
            this.lblFromAccount.Location = new System.Drawing.Point(4, 4);
            this.lblFromAccount.Name = "lblFromAccount";
            this.lblFromAccount.Size = new System.Drawing.Size(94, 20);
            this.lblFromAccount.Text = "From Account:";
            // 
            // pictureURL
            // 
            this.pictureURL.Location = new System.Drawing.Point(4, 40);
            this.pictureURL.Name = "pictureURL";
            this.pictureURL.Size = new System.Drawing.Size(25, 25);
            this.pictureURL.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            // 
            // pictureFromStorage
            // 
            this.pictureFromStorage.Location = new System.Drawing.Point(35, 40);
            this.pictureFromStorage.Name = "pictureFromStorage";
            this.pictureFromStorage.Size = new System.Drawing.Size(25, 25);
            this.pictureFromStorage.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            // 
            // pictureFromCamers
            // 
            this.pictureFromCamers.Location = new System.Drawing.Point(66, 40);
            this.pictureFromCamers.Name = "pictureFromCamers";
            this.pictureFromCamers.Size = new System.Drawing.Size(25, 25);
            this.pictureFromCamers.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            // 
            // txtStatusUpdate
            // 
            this.txtStatusUpdate.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.txtStatusUpdate.Location = new System.Drawing.Point(4, 72);
            this.txtStatusUpdate.Multiline = true;
            this.txtStatusUpdate.Name = "txtStatusUpdate";
            this.txtStatusUpdate.Size = new System.Drawing.Size(232, 193);
            this.txtStatusUpdate.TabIndex = 7;
            this.txtStatusUpdate.TextChanged += new System.EventHandler(this.txtStatusUpdate_TextChanged);
            // 
            // lblCharsLeft
            // 
            this.lblCharsLeft.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lblCharsLeft.Location = new System.Drawing.Point(191, 45);
            this.lblCharsLeft.Name = "lblCharsLeft";
            this.lblCharsLeft.Size = new System.Drawing.Size(45, 20);
            this.lblCharsLeft.Text = "label2";
            // 
            // pictureLocation
            // 
            this.pictureLocation.Location = new System.Drawing.Point(128, 40);
            this.pictureLocation.Name = "pictureLocation";
            this.pictureLocation.Size = new System.Drawing.Size(25, 25);
            this.pictureLocation.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            // 
            // lblGPS
            // 
            this.lblGPS.Location = new System.Drawing.Point(128, 45);
            this.lblGPS.Name = "lblGPS";
            this.lblGPS.Size = new System.Drawing.Size(95, 20);
            this.lblGPS.Text = "Seeking GPS";
            this.lblGPS.Visible = false;
            // 
            // picAddressBook
            // 
            this.picAddressBook.Location = new System.Drawing.Point(97, 40);
            this.picAddressBook.Name = "picAddressBook";
            this.picAddressBook.Size = new System.Drawing.Size(25, 25);
            this.picAddressBook.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            // 
            // userListControl1
            // 
            this.userListControl1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.userListControl1.Location = new System.Drawing.Point(42, 68);
            this.userListControl1.Name = "userListControl1";
            this.userListControl1.Size = new System.Drawing.Size(150, 22);
            this.userListControl1.TabIndex = 12;
            this.userListControl1.Visible = false;
            // 
            // PostUpdate
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.AutoScroll = true;
            this.ClientSize = new System.Drawing.Size(240, 268);
            this.Controls.Add(this.picAddressBook);
            this.Controls.Add(this.userListControl1);
            this.Controls.Add(this.lblGPS);
            this.Controls.Add(this.lblCharsLeft);
            this.Controls.Add(this.pictureLocation);
            this.Controls.Add(this.txtStatusUpdate);
            this.Controls.Add(this.pictureFromCamers);
            this.Controls.Add(this.pictureFromStorage);
            this.Controls.Add(this.pictureURL);
            this.Controls.Add(this.lblFromAccount);
            this.Controls.Add(this.cmbAccount);
            this.Menu = this.mainMenu1;
            this.Name = "PostUpdate";
            this.Text = "Post Update";
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.ComboBox cmbAccount;
        private System.Windows.Forms.Label lblFromAccount;
        private System.Windows.Forms.PictureBox pictureURL;
        private System.Windows.Forms.PictureBox pictureFromStorage;
        private System.Windows.Forms.PictureBox pictureFromCamers;
        private System.Windows.Forms.TextBox txtStatusUpdate;
        private System.Windows.Forms.MenuItem menuSubmit;
        private System.Windows.Forms.MenuItem menuCancel;
        private System.Windows.Forms.Label lblCharsLeft;
        private System.Windows.Forms.PictureBox pictureLocation;
        private userListControl userListControl1;
        private Label lblGPS;
        private PictureBox picAddressBook;
    }
}
