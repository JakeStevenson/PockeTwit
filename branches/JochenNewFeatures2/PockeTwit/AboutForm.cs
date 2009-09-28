using System;

using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using PockeTwit.OtherServices;

namespace PockeTwit
{
    public partial class AboutForm : Form
    {
        delegate void delNothing();
		#region Fields (1) 

        private UpgradeChecker Checker = new UpgradeChecker(false);
        private Contributors ContributorChecker;
        public string AskedToSeeUser = null;
		#endregion Fields 

		#region Constructors (1) 

        public AboutForm()
        {
            InitializeComponent();
            PockeTwit.Themes.FormColors.SetColors(this);
            PockeTwit.Localization.XmlBasedResourceManager.LocalizeForm(this);
            lblWait.ForeColor = ClientSettings.FieldForeColor;
            ContributorChecker = new Contributors();
            ContributorChecker.ContributorsReady += new Contributors.delContributorsReady(ContributorChecker_ContributorsReady);
            if (ClientSettings.IsMaximized)
            {
                this.WindowState = FormWindowState.Maximized;
            }
            Checker.UpgradeFound += new UpgradeChecker.delUpgradeFound(Checker_UpdateFound);
            Checker.CurrentVersion += new UpgradeChecker.delUpgradeFound(Checker_CurrentVersion);
            lblVersion.Text = "PockeTwit v" + UpgradeChecker.currentVersion.ToString();
            if (UpgradeChecker.devBuild)
            {
                lblVersion.Text = "PockeTwit v" + UpgradeChecker.currentVersion.ToString() + " d" + System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.Revision;
                menuUpdate.Text = Localization.XmlBasedResourceManager.GetString("Upgrade");
            }
            if(UpgradeChecker.isBeta)
            {
                lblVersion.Text = "PockeTwit v" + UpgradeChecker.currentVersion.ToString() + " beta";
            }
            this.lnkContributors.Focus();
        }

        void ContributorChecker_ContributorsReady()
        {
            if (InvokeRequired)
            {
                delNothing d = new delNothing(ContributorChecker_ContributorsReady);
                this.Invoke(d);
            }
            else
            {
                panel1.SuspendLayout();
                panel1.Controls.Remove(lblWait);
                int topOfLabel = 0;
                int labelWidth = panel1.Width / 2;
                foreach (Contributors.Contributor s in ContributorChecker.ContributorsList)
                {

                    LinkLabel nameLabel = new LinkLabel();
                    nameLabel.Text = s.Name;
                    nameLabel.ForeColor = ClientSettings.LinkColor;

                    if (s.Name.StartsWith("@"))
                    {
                        nameLabel.Click += new EventHandler(nameLabel_Click);
                    }
                    nameLabel.Top = topOfLabel;
                    nameLabel.Width = labelWidth;
                    nameLabel.Height = ClientSettings.TextSize+5;
                    panel1.Controls.Add(nameLabel);
                    
                    Label typeLabel = new Label();
                    typeLabel.Text = s.Contribution;
                    typeLabel.Top = topOfLabel;
                    typeLabel.Left = nameLabel.Right;
                    typeLabel.Width = labelWidth;
                    typeLabel.Height = ClientSettings.TextSize + 5;
                    typeLabel.ForeColor = ClientSettings.FieldForeColor;
                    panel1.Controls.Add(typeLabel);

                    

                    topOfLabel = nameLabel.Bottom + 5;
                }
                LinkLabel YouToo = new LinkLabel();
                YouToo.Text = Localization.XmlBasedResourceManager.GetString("Your name can be here!");
                YouToo.ForeColor = ClientSettings.LinkColor;
                YouToo.Width = panel1.Width - 2;
                YouToo.Height = ClientSettings.TextSize+5;
                YouToo.Click += new EventHandler(YouToo_Click);
                YouToo.Top = topOfLabel;
                panel1.Controls.Add(YouToo);
                panel1.ResumeLayout();
            }
        }

        void YouToo_Click(object sender, EventArgs e)
        {
            LaunchSite("http://code.google.com/p/pocketwit/wiki/Contribute");
        }

        void nameLabel_Click(object sender, EventArgs e)
        {
            LinkLabel l = (LinkLabel)sender;
            if (l.Text.StartsWith("@"))
            {
                AskedToSeeUser = l.Text;
                this.DialogResult = DialogResult.OK;
            }
        }

		#endregion Constructors 

		#region Methods (5) 


		// Private Methods (5) 

        void Checker_CurrentVersion(UpgradeChecker.UpgradeInfo Info)
        {
            PockeTwit.Localization.LocalizedMessageBox.Show("{0} is the latest version.", "No upgrades found.", Info.webVersion.ToString()); 
        }

        void Checker_UpdateFound(UpgradeChecker.UpgradeInfo Info)
        {
            using (UpgradeForm uf = new UpgradeForm())
            {
                uf.NewVersion = Info;
                uf.ShowDialog();
            }
        }

        private void linkLabel1_Click(object sender, EventArgs e)
        {
            Yedda.Twitter Twitter = new Yedda.Twitter();
            using (PostUpdate s = new PostUpdate(false))
            {
                s.StatusText = "@PockeTwitDev ";
                s.ShowDialog();
                s.Hide();
                string UpdateText = s.StatusText;
                if (s.DialogResult == DialogResult.OK)
                {
                    Cursor.Current = Cursors.WaitCursor;
                    Twitter.AccountInfo = s.AccountToSet;
                    Twitter.Update(UpdateText, Yedda.Twitter.OutputFormatType.XML);
                    Cursor.Current = Cursors.Default;
                }
                this.Show();
                s.Close();
            }
        }

        private void menuClose_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
        }

        private void menuUpdate_Click(object sender, EventArgs e)
        {
            if (UpgradeChecker.devBuild)
            {
                using (UpgradeForm f = new UpgradeForm())
                {
                    f.ShowDialog();
                }
            }
            else
            {
                Checker.CheckForUpgrade();
            }
        }


		#endregion Methods 

        private void lnkContributors_Click(object sender, EventArgs e)
        {
            LaunchSite("http://code.google.com/p/pocketwit/wiki/Contribute");
        }

        private void linkLabel2_Click(object sender, EventArgs e)
        {
            LaunchSite("http://code.google.com/p/pocketwit/");
        }

        private void LaunchSite(string URL)
        {
            System.Diagnostics.ProcessStartInfo pi = new System.Diagnostics.ProcessStartInfo();
            if (ClientSettings.UseSkweezer)
            {
                URL = Skweezer.GetSkweezerURL(URL);
            }
            pi.FileName = URL;
            pi.UseShellExecute = true;
            try
            {
                System.Diagnostics.Process p = System.Diagnostics.Process.Start(pi);
            }
            catch
            {
                PockeTwit.Localization.LocalizedMessageBox.Show("There is no default web browser defined for the OS.");
            }
        }

    }
}