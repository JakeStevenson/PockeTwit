using System;

using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace PockeTwit
{
    public partial class CrashReport : Form
    {
        private string ErrorText;
        
        private Microsoft.WindowsMobile.PocketOutlook.OutlookSession sess = new Microsoft.WindowsMobile.PocketOutlook.OutlookSession();
        private Microsoft.WindowsMobile.PocketOutlook.EmailAccountCollection accounts;
        public CrashReport()
        {
            InitializeComponent();
            accounts = sess.EmailAccounts;

            using(System.IO.StreamReader r = new System.IO.StreamReader(ClientSettings.AppPath + "\\crash.txt"))
            {
                ErrorText = r.ReadToEnd();
            }

            System.IO.File.Delete(ClientSettings.AppPath + "\\crash.txt");

            foreach (Microsoft.WindowsMobile.PocketOutlook.EmailAccount acc in accounts)
            {
                comboBox1.Items.Add(acc.Name);
            }
        }
        public CrashReport(string Error)
        {
            InitializeComponent();
            ErrorText = Error;
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
                m.BodyText = ErrorText;
                m.To.Add(new Microsoft.WindowsMobile.PocketOutlook.Recipient("pocketwitdev@gmail.com"));
                m.Subject = "Crash Report: v" +UpgradeChecker.currentVersion.ToString();
                if (UpgradeChecker.devBuild)
                {
                    m.Subject = m.Subject + " dev build" + System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.Revision;
                }
                if (ClientSettings.AccountsList.Count > 0)
                {
                    m.Subject += " from @" + ClientSettings.AccountsList[0].UserName;
                }
                string accName = (string)comboBox1.SelectedItem;
                accounts[accName].Send(m);
                Microsoft.WindowsMobile.PocketOutlook.MessagingApplication.Synchronize();
                this.Close();
            }
        }

        private void menuItem1_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}