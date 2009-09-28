using System;

using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using Yedda;
using PockeTwit.MediaServices;

namespace PockeTwit
{
    public partial class MediaService : BaseSettingsForm
    {
        public MediaService()
        {
            InitializeComponent();
            PockeTwit.Themes.FormColors.SetColors(this);
            PockeTwit.Themes.FormColors.SetColors(this.pnlCapabilites);
            PockeTwit.Localization.XmlBasedResourceManager.LocalizeForm(this);

            if (ClientSettings.IsMaximized)
            {
                this.WindowState = FormWindowState.Maximized;
            }

            setMediaService(ClientSettings.SelectedMediaService);
            cbPreUpload.Checked = !ClientSettings.SendMessageToMediaService;
        }

        private void menuAccept_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(cmbMediaService.Items[cmbMediaService.SelectedIndex].ToString()))
            {
                ClientSettings.SelectedMediaService = "TweetPhoto";
            }
            else
            {
                ClientSettings.SelectedMediaService = cmbMediaService.Items[cmbMediaService.SelectedIndex].ToString();
            }
            ClientSettings.SendMessageToMediaService = !cbPreUpload.Checked;

            ClientSettings.SaveSettings();

            this.DialogResult = System.Windows.Forms.DialogResult.OK;
        }

        private void menuCancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = System.Windows.Forms.DialogResult.Cancel;
        }

        private void setMediaService(string value)
        {
            foreach (string serviceName in PictureServiceFactory.Instance.GetServiceNames())
            {
                cmbMediaService.Items.Add(serviceName);
            }

            foreach (string comboValue in cmbMediaService.Items)
            {
                if (comboValue == value)
                {
                    cmbMediaService.SelectedItem = value;
                    return;
                }
            }
        }


        private void cmbMediaService_SelectedValueChanged(object sender, EventArgs e)
        {
            string ServiceName = (string) cmbMediaService.SelectedItem;
            IPictureService service = PictureServiceFactory.Instance.GetServiceByName(ServiceName);

            chkMessage.Checked = service.CanUploadMessage;
            chkGPS.Checked = service.CanUploadGPS;
            this.lblMediaLabel.Text = String.Format(PockeTwit.Localization.XmlBasedResourceManager.GetString("{0} can: "), ServiceName);
        }
    }
}