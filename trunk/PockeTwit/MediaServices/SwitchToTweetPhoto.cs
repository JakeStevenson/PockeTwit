using System;

using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace PockeTwit.MediaServices
{
    public partial class SwitchToTweetPhoto : Form
    {
        public SwitchToTweetPhoto()
        {
            InitializeComponent();
            PockeTwit.Themes.FormColors.SetColors(this);
            PockeTwit.Localization.XmlBasedResourceManager.LocalizeForm(this);
            if (ClientSettings.IsMaximized)
            {
                this.WindowState = FormWindowState.Maximized;
            }
        }

        private void menuItem1_Click(object sender, EventArgs e)
        {
            ClientSettings.SelectedMediaService = ClientSettings.PreviousMediaService;
            ClientSettings.PreviousMediaService = null;
            ClientSettings.SaveSettings();
            DialogResult = System.Windows.Forms.DialogResult.OK;
        }

        private void menuItem2_Click(object sender, EventArgs e)
        {
            ClientSettings.SelectedMediaService = "TweetPhoto";
            ClientSettings.SaveSettings();
            DialogResult = System.Windows.Forms.DialogResult.OK;
        }
    }
}