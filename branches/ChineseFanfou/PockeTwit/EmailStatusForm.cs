using System;

using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace PockeTwit
{
    public partial class EmailStatusForm : Form
    {
        private string MessageText;
        
        private Microsoft.WindowsMobile.PocketOutlook.OutlookSession sess = new Microsoft.WindowsMobile.PocketOutlook.OutlookSession();
        private Microsoft.WindowsMobile.PocketOutlook.EmailAccountCollection accounts;

        public EmailStatusForm(string Message)
        {
            InitializeComponent();
            PockeTwit.Themes.FormColors.SetColors(this);
            PockeTwit.Localization.XmlBasedResourceManager.LocalizeForm(this);

            MessageText = Message;
            accounts = sess.EmailAccounts;

            foreach (Microsoft.WindowsMobile.PocketOutlook.EmailAccount acc in accounts)
            {
                comboBox1.Items.Add(acc.Name);
            }
        }

        
        private void menuItem2_Click(object sender, EventArgs e)
        {
            if (comboBox1.SelectedItem != null)
            {

                Microsoft.WindowsMobile.PocketOutlook.EmailMessage m = new Microsoft.WindowsMobile.PocketOutlook.EmailMessage();
                m.BodyText = MessageText;
                string accName = (string)comboBox1.SelectedItem;
                Microsoft.WindowsMobile.PocketOutlook.MessagingApplication.DisplayComposeForm(accounts[accName], m);
                //Microsoft.WindowsMobile.PocketOutlook.MessagingApplication.Synchronize();
                this.DialogResult = DialogResult.OK;
            }
        }

        private void menuItem1_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}