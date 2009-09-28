using System;
using System.IO;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Net;
using System.Web;
using System.Windows.Forms;

namespace PockeTwit
{
    public partial class UpgradeForm : Form
    {

		#region Fields (1) 

        private UpgradeChecker.UpgradeInfo _NewVersion;
        private HttpWebRequest request;
        private HttpWebResponse response;
        private FileStream filestream;

        private int pbVal, maxVal;

        // Data buffer for stream operations
        private byte[] dataBuffer;
        private const int DataBlockSize = 65536;
		#endregion Fields 

		#region Constructors (1) 

        public UpgradeForm()
        {
            InitializeComponent();
            Themes.FormColors.SetColors(this);
            PockeTwit.Localization.XmlBasedResourceManager.LocalizeForm(this);
            if (ClientSettings.IsMaximized)
            {
                WindowState = FormWindowState.Maximized;
            }
            if (UpgradeChecker.devBuild)
            {
                _NewVersion.DownloadURL = @"http://pocketwit.googlecode.com/svn/trunk/PockeTwit%20Dev%20Install/DevBuild/PockeTwit%20Dev%20Install.CAB";
                PerformUpdate();
            }
        }

		#endregion Constructors 

		#region Properties (1) 

        public UpgradeChecker.UpgradeInfo NewVersion 
        {
            set
            {
                lblVersion.Text = value.webVersion.ToString();
                lblInfo.Text = value.UpgradeNotes;
                _NewVersion = value;
            }
        }

		#endregion Properties 

		#region Methods (2) 


		// Private Methods (2) 

        private void menuIgnore_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void menuUpdate_Click(object sender, EventArgs e)
        {
            PerformUpdate();
        }

        private delegate void delToggleState(bool state);
        private void EnableMenu(bool state)
        {
            if (InvokeRequired)
            {
                delToggleState d = EnableMenu;
                Invoke(d, state);
            }
            else
            {
                menuUpdate.Enabled = state;
                menuIgnore.Enabled = state;
            }
        }

        private delegate void delNothing();
        private void PerformUpdate()
        {
            if (InvokeRequired)
            {
                delNothing d = PerformUpdate;
                Invoke(d);
            }
            else
            {
                Directory.CreateDirectory(ClientSettings.AppPath + "\\Update");
                request = WebRequestFactory.CreateHttpRequest(_NewVersion.DownloadURL);
                request.BeginGetResponse((ResponseReceived), null);
                EnableMenu(false);

                lblDownloading.Visible = true;
                progressDownload.Visible = true;
                lblInfo.Visible = false;
                lblVersion.Visible = false;
                label1.Visible = false;
                label2.Visible = false;
            }

            Cursor.Current = Cursors.WaitCursor;
        }

        public void SetProgressMax(object sender, EventArgs e)
        {
            progressDownload.Maximum = maxVal;
            Application.DoEvents();
        }
        public void UpdateProgressValue(object sender, EventArgs e)
        {
            progressDownload.Value = pbVal;
            Application.DoEvents();
        }

        void ResponseReceived(IAsyncResult res)
        {
            try
            {
                response = (HttpWebResponse)request.EndGetResponse(res);
                // Allocate data buffer
                dataBuffer = new byte[DataBlockSize];
                // Set up progrees bar
                maxVal = (int)response.ContentLength;
                progressDownload.Invoke(new EventHandler(SetProgressMax));

                // Open file stream to save received data
                filestream = new FileStream(ClientSettings.AppPath + "\\Update\\PockeTwitUpgrade.cab", FileMode.Create);
                // Request the first chunk
                response.GetResponseStream().BeginRead(dataBuffer, 0, DataBlockSize, new AsyncCallback(OnDataRead), this);
            }
            catch
            {
                if (PockeTwit.Localization.LocalizedMessageBox.Show("There was an error downloading the upgrade.  Would you like to try again?", "PockeTwit", MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation, MessageBoxDefaultButton.Button1) == DialogResult.Yes)
                {
                    PerformUpdate();
                }
                else
                {
                    PockeTwit.Localization.LocalizedMessageBox.Show("You can download the upgrade manually from http://code.google.com/p/pocketwit/");
                    Close();
                }
            }
        }

        void OnDataRead(IAsyncResult res)
        {
            try
            {
                // How many bytes did we get this time
                int nBytes = response.GetResponseStream().EndRead(res);
                // Write buffer
                filestream.Write(dataBuffer, 0, nBytes);
                // Update progress bar using Invoke()
                pbVal += nBytes;

                progressDownload.Invoke(new EventHandler(UpdateProgressValue));

                // Are we done yet?
                if (nBytes > 0)
                {
                    // No, keep reading
                    response.GetResponseStream().BeginRead(dataBuffer, 0, DataBlockSize, new AsyncCallback(OnDataRead), this);
                }
                else
                {
                    // Yes, perform cleanup and update UI.
                    filestream.Close();
                    filestream = null;
                    DoneDownloading();
                }
            }
            catch 
            {
                if (PockeTwit.Localization.LocalizedMessageBox.Show("There was an error downloading the upgrade.  Would you like to try again?", "PockeTwit", MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation, MessageBoxDefaultButton.Button1) == DialogResult.Yes)
                {
                    PerformUpdate();
                }
                else
                {
                    PockeTwit.Localization.LocalizedMessageBox.Show("You can download the upgrade manually from http://code.google.com/p/pocketwit/");
                    Close();
                }

            }
        }

        void DoneDownloading()
        {
            System.Diagnostics.Process p = new System.Diagnostics.Process();
            p.StartInfo.FileName = ClientSettings.AppPath + "\\Update\\PockeTwitUpgrade.cab";
            p.StartInfo.UseShellExecute = true;
            p.Start();
            Application.Exit();
        }

		#endregion Methods 

    }
}