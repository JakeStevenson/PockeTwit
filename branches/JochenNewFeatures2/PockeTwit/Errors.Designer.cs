namespace PockeTwit
{
    partial class Errors
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
            this.lblErrors = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // mainMenu1
            // 
            this.mainMenu1.MenuItems.Add(this.menuCancel);
            // 
            // menuCancel
            // 
            this.menuCancel.Text = "Close";
            this.menuCancel.Click += new System.EventHandler(this.menuCancel_Click);
            // 
            // lblErrors
            // 
            this.lblErrors.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblErrors.ForeColor = ClientSettings.ForeColor;
            this.lblErrors.BackColor = ClientSettings.BackColor;
            this.lblErrors.Location = new System.Drawing.Point(0, 0);
            this.lblErrors.Name = "lblErrors";
            this.lblErrors.Size = new System.Drawing.Size(240, 268);
            this.lblErrors.Text = "Errors:";
            // 
            // Errors
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.AutoScroll = true;
            this.ClientSize = new System.Drawing.Size(240, 268);
            this.Controls.Add(this.lblErrors);
            this.Menu = this.mainMenu1;
            this.Name = "Errors";
            this.Text = "Errors";
            this.ForeColor = ClientSettings.ForeColor;
            this.BackColor = ClientSettings.BackColor;
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Label lblErrors;
        private System.Windows.Forms.MenuItem menuCancel;
    }
}