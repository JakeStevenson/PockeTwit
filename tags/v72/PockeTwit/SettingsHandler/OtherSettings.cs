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

        #region Constructors (1) 

        public OtherSettings()
        {
            InitializeComponent();
            PockeTwit.Themes.FormColors.SetColors(this);
            
            if (ClientSettings.IsMaximized)
            {
                this.WindowState = FormWindowState.Maximized;
            }

            PopulateForm();
        }

		#endregion Constructors 

		#region Methods (4) 


		// Private Methods (4) 

        

        private void menuAccept_Click(object sender, EventArgs e)
        {
            IFormatProvider format = new System.Globalization.CultureInfo(1033);
            ClientSettings.UseGPS = chkGPS.Checked;
            ClientSettings.CheckVersion = chkVersion.Checked;
            ClientSettings.AutoTranslate = chkTranslate.Checked;
            ClientSettings.UseSkweezer = chkSkweezer.Checked;
            
            if (ClientSettings.UpdateMinutes != int.Parse(txtUpdate.Text, format))
            {
                MessageBox.Show("You will need to restart PockeTwit for the update interval to change.", "PockeTwit");
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
                    MessageBox.Show("Unable to use that folder as a cache directory");
                }
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

            chkSkweezer.Checked = ClientSettings.UseSkweezer;
            chkVersion.Checked = ClientSettings.CheckVersion;
            chkGPS.Checked = ClientSettings.UseGPS;
            txtUpdate.Text = ClientSettings.UpdateMinutes.ToString();
            chkTranslate.Checked = ClientSettings.AutoTranslate;
            txtCaheDir.Text = ClientSettings.CacheDir;
            chkTranslate.Text = "Auto-translate to " + ClientSettings.TranslationLanguage;
            this.DialogResult = DialogResult.Cancel;
        }


		#endregion Methods 

       
        private void OtherSettings_Load(object sender, EventArgs e)
        {

        }

    }
}