using System;

using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace PockeTwit
{
    public partial class AccountsForm : BaseSettingsForm
    {
        public bool Initialized = false;
        private List<Yedda.Twitter.Account> LocalList = new List<Yedda.Twitter.Account>();
        public AccountsForm()
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
        protected override void OnActivated(EventArgs e)
        {
            base.OnActivated(e);
            if (!Initialized)
            {
                Initialized = true;
                if (ClientSettings.AccountsList.Count == 0)
                {
                    if (!AddAccount())
                    {
                        this.DialogResult = DialogResult.Cancel;
                    }
                }
            }
        }

        private void btnAdd_Click(object sender, EventArgs e)
        {
            AddAccount();
        }

        private bool AddAccount()
        {
            using (AccountInfoForm ai = new AccountInfoForm())
            {
                ai.Hide();
                if (ai.ShowDialog() == DialogResult.OK)
                {
                    LocalList.Add(ai.AccountInfo);
                    NeedsReset = true;
                    ListAccounts();
                    ai.Close();
                    return true;
                }
                else
                {
                    ai.Close();
                    return false;
                }
            }
        }

        
       

        private void menuAccept_Click(object sender, EventArgs e)
        {
            if (cmbAccounts.Items.Count == 0)
            {
                PockeTwit.Localization.LocalizedMessageBox.Show("You must enter at least one account or cancel.");
                return;
            }
            this.DialogResult = DialogResult.OK;
            if (NeedsReset)
            {
                ClientSettings.AccountsList.Clear();
                foreach (Yedda.Twitter.Account a in LocalList)
                {
                    
                    ClientSettings.AccountsList.Add(a);
                    if (a.IsDefault) { ClientSettings.DefaultAccount = a; }
                }
                ClientSettings.SaveSettings();
            }
        }

        private void menuCancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
        }

        
        private void lnkAdd_Click(object sender, EventArgs e)
        {
            AddAccount();
        }

        private void lnkEdit_Click(object sender, EventArgs e)
        {
            if (cmbAccounts.SelectedItem != null)
            {
                Yedda.Twitter.Account toEdit = (Yedda.Twitter.Account)cmbAccounts.SelectedItem;
                using (AccountInfoForm ai = new AccountInfoForm(toEdit))
                {
                    if (ai.ShowDialog() == DialogResult.OK)
                    {
                        LocalList.Remove(toEdit);
                        LocalList.Add(ai.AccountInfo);
                        ListAccounts();
                        NeedsReset = true;
                    }
                    ai.Close();
                }
            }
        }

        private void lnkRemove_Click(object sender, EventArgs e)
        {
            if (cmbAccounts.SelectedItem != null)
            {
                Yedda.Twitter.Account toRemove = (Yedda.Twitter.Account)cmbAccounts.SelectedItem;
                if (toRemove != null)
                {
                    LocalList.Remove(toRemove);
                    ListAccounts();
                    NeedsReset = true;
                }
            }
        }

    }
}