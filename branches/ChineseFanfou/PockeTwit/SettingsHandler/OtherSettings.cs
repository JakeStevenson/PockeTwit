using System;

using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using Yedda;

namespace PockeTwit
{
    public partial class OtherSettings : BaseSettingsForm
    {

        #region Constructors (1) 

        public OtherSettings()
        {
            InitializeComponent();
            PockeTwit.Themes.FormColors.SetColors(this);
            PockeTwit.Localization.XmlBasedResourceManager.LocalizeForm(this);

            
            if (ClientSettings.IsMaximized)
            {
                this.WindowState = FormWindowState.Maximized;
            }

            PopulateForm();
        }

		#endregion Constructors 

		#region Methods (4) 


		// Private Methods (4) 

        

        private void menuAccept_Click(object sender, EventArgs e)
        {
            IFormatProvider format = new System.Globalization.CultureInfo(1033);
            ClientSettings.UseGPS = chkGPS.Checked;
            ClientSettings.CheckVersion = chkVersion.Checked;
            ClientSettings.AutoTranslate = chkTranslate.Checked;
            ClientSettings.UseSkweezer = chkSkweezer.Checked;
            ClientSettings.AutoCompleteAddressBook = chkAutoComplete.Checked;
            
            if (ClientSettings.UpdateMinutes != int.Parse(txtUpdate.Text, format))
            {
                PockeTwit.Localization.LocalizedMessageBox.Show("You will need to restart PockeTwit for the update interval to change.", "PockeTwit");
                ClientSettings.UpdateMinutes = int.Parse(txtUpdate.Text, format);
            }
            if (ClientSettings.CacheDir != txtCaheDir.Text)
            {
                try
                {
                    if (!System.IO.Directory.Exists(txtCaheDir.Text))
                    {
                        System.IO.Directory.CreateDirectory(txtCaheDir.Text);
                    }
                    ClientSettings.CacheDir = txtCaheDir.Text;
                }
                catch
                {
                    PockeTwit.Localization.LocalizedMessageBox.Show("Unable to use that folder as a cache directory.");
                }
            }
            // Proxy settings
            if (chkEnableProxy.Checked)
            {
                ClientSettings.ProxyServer = txtProxyServer.Text.Trim();
                try
                {
                    ClientSettings.ProxyPort = int.Parse(txtProxyPort.Text);
                }
                catch
                {
                    PockeTwit.Localization.LocalizedMessageBox.Show("The proxy setting is invalid. No proxy will be set.");
                    ClientSettings.ProxyServer = string.Empty;
                    ClientSettings.ProxyPort = 0;
                }
            }
            else
            {
                ClientSettings.ProxyServer = string.Empty;
                ClientSettings.ProxyPort = 0;
            }
            ClientSettings.SaveSettings();
            
            this.DialogResult = DialogResult.OK;
            this.Close();

        }

        private void menuCancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }

        private void PopulateForm()
        {
            chkAutoComplete.Checked = ClientSettings.AutoCompleteAddressBook;
            chkSkweezer.Checked = ClientSettings.UseSkweezer;
            chkVersion.Checked = ClientSettings.CheckVersion;
            chkGPS.Checked = ClientSettings.UseGPS;
            txtUpdate.Text = ClientSettings.UpdateMinutes.ToString();
            chkTranslate.Checked = ClientSettings.AutoTranslate;
            txtCaheDir.Text = ClientSettings.CacheDir;
            chkTranslate.Text = String.Format(PockeTwit.Localization.XmlBasedResourceManager.GetString("Auto-translate to {0}"), ClientSettings.TranslationLanguage);
            chkEnableProxy.Checked = !string.IsNullOrEmpty(ClientSettings.ProxyServer);
            if (chkEnableProxy.Checked)
            {
                txtProxyServer.Text = ClientSettings.ProxyServer;
                txtProxyPort.Text = ClientSettings.ProxyPort.ToString();
                txtProxyServer.Enabled = true;
                txtProxyPort.Enabled = true;
            }
            else
            {
                txtProxyServer.Text = txtProxyPort.Text = string.Empty;
                txtProxyServer.Enabled = false;
                txtProxyPort.Enabled = false;
            }
            this.DialogResult = DialogResult.Cancel;
        }


		#endregion Methods 

       
        private void OtherSettings_Load(object sender, EventArgs e)
        {

        }

        private void chkEnableProxy_CheckStateChanged(object sender, EventArgs e)
        {
            txtProxyServer.Enabled = txtProxyPort.Enabled = chkEnableProxy.Checked;
        }

    }
}