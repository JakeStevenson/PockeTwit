using System;

using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using PockeTwit.Localization;
using System.IO;

namespace PockeTwit
{
    public partial class FollowUserForm : Form
    {
        public bool Initialized = false;
        private List<Yedda.Twitter.Account> LocalList = new List<Yedda.Twitter.Account>();
        public FollowUserForm()
        {
            InitializeComponent();
            PockeTwit.Themes.FormColors.SetColors(this);
            PockeTwit.Localization.XmlBasedResourceManager.LocalizeForm(this);

            if (ClientSettings.IsMaximized)
            {
                this.WindowState = FormWindowState.Maximized;
            } 
            foreach (Yedda.Twitter.Account a in ClientSettings.AccountsList)
            {
                LocalList.Add(a);
            }
            ListAccounts();
        }
        private void ListAccounts()
        {
            cmbAccounts.Items.Clear();   
            foreach (Yedda.Twitter.Account a in LocalList)
            {
                cmbAccounts.Items.Add(a);
            }
            if (cmbAccounts.Items.Count > 0)
            {
                cmbAccounts.SelectedIndex = 0;
            }
        }

        private void menuCancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
        }

        private void menuOK_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.OK;
        }

        public Yedda.Twitter.Account Account
        {
            get
            {
                return (cmbAccounts.SelectedItem) as Yedda.Twitter.Account;
            }
        }

        public string UserName
        {
            get
            {
                return txtUserName.Text;
            }
        }
    }
}