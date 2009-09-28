namespace PockeTwit.SettingsHandler
{
    partial class GroupManagement
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
            this.menuDone = new System.Windows.Forms.MenuItem();
            this.menuImportExport = new System.Windows.Forms.MenuItem();
            this.menuExport = new System.Windows.Forms.MenuItem();
            this.menuImport = new System.Windows.Forms.MenuItem();
            this.cmbChooseGroup = new System.Windows.Forms.ComboBox();
            this.lnkDeleteGroup = new System.Windows.Forms.LinkLabel();
            this.pnlUsers = new System.Windows.Forms.Panel();
            this.lblNoGroups = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // mainMenu1
            // 
            this.mainMenu1.MenuItems.Add(this.menuDone);
            this.mainMenu1.MenuItems.Add(this.menuImportExport);
            // 
            // menuDone
            // 
            this.menuDone.Text = "Done";
            this.menuDone.Click += new System.EventHandler(this.menuItem1_Click);
            // 
            // menuImportExport
            // 
            this.menuImportExport.MenuItems.Add(this.menuExport);
            this.menuImportExport.MenuItems.Add(this.menuImport);
            this.menuImportExport.Text = "Backup";
            // 
            // menuExport
            // 
            this.menuExport.Text = "Export...";
            this.menuExport.Click += new System.EventHandler(this.menuExport_Click);
            // 
            // menuImport
            // 
            this.menuImport.Text = "Import...";
            this.menuImport.Click += new System.EventHandler(this.menuImport_Click);
            // 
            // cmbChooseGroup
            // 
            this.cmbChooseGroup.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.cmbChooseGroup.Location = new System.Drawing.Point(3, 3);
            this.cmbChooseGroup.Name = "cmbChooseGroup";
            this.cmbChooseGroup.Size = new System.Drawing.Size(183, 22);
            this.cmbChooseGroup.TabIndex = 1;
            this.cmbChooseGroup.SelectedIndexChanged += new System.EventHandler(this.cmbChooseGroup_SelectedIndexChanged);
            // 
            // lnkDeleteGroup
            // 
            this.lnkDeleteGroup.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lnkDeleteGroup.Location = new System.Drawing.Point(192, 5);
            this.lnkDeleteGroup.Name = "lnkDeleteGroup";
            this.lnkDeleteGroup.Size = new System.Drawing.Size(45, 20);
            this.lnkDeleteGroup.TabIndex = 2;
            this.lnkDeleteGroup.Text = "Delete";
            this.lnkDeleteGroup.Click += new System.EventHandler(this.lnkDeleteGroup_Click);
            // 
            // pnlUsers
            // 
            this.pnlUsers.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.pnlUsers.AutoScroll = true;
            this.pnlUsers.Location = new System.Drawing.Point(4, 32);
            this.pnlUsers.Name = "pnlUsers";
            this.pnlUsers.Size = new System.Drawing.Size(233, 233);
            // 
            // lblNoGroups
            // 
            this.lblNoGroups.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.lblNoGroups.Location = new System.Drawing.Point(4, 5);
            this.lblNoGroups.Name = "lblNoGroups";
            this.lblNoGroups.Size = new System.Drawing.Size(233, 20);
            this.lblNoGroups.Text = "There are no groups defined";
            this.lblNoGroups.Visible = false;
            // 
            // GroupManagement
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.AutoScroll = true;
            this.ClientSize = new System.Drawing.Size(240, 268);
            this.Controls.Add(this.pnlUsers);
            this.Controls.Add(this.lnkDeleteGroup);
            this.Controls.Add(this.cmbChooseGroup);
            this.Controls.Add(this.lblNoGroups);
            this.Menu = this.mainMenu1;
            this.Name = "GroupManagement";
            this.Text = "Group Management";
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.MenuItem menuDone;
        private System.Windows.Forms.ComboBox cmbChooseGroup;
        private System.Windows.Forms.LinkLabel lnkDeleteGroup;
        private System.Windows.Forms.Panel pnlUsers;
        private System.Windows.Forms.MenuItem menuImportExport;
        private System.Windows.Forms.MenuItem menuExport;
        private System.Windows.Forms.MenuItem menuImport;
        private System.Windows.Forms.Label lblNoGroups;
    }
}