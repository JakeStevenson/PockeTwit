using System;

using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace PockeTwit.SettingsHandler
{
    public partial class NotificationSettings : BaseSettingsForm
    {
        private struct SoundInfo
        {
            public string Name
            {
                get
                {
                    return System.IO.Path.GetFileNameWithoutExtension(Path);
                }
            }
            public string Path;
            public override string ToString()
            {
                return Name;
            }
        }

        private NotificationHandler.NotificationInfoClass currentInfo;

        public NotificationSettings()
        {
            InitializeComponent();
            foreach (var infoClass in NotificationHandler.GetList())
            {
                this.cmbNotificationType.Items.Add(infoClass);
            }
            PockeTwit.Themes.FormColors.SetColors(this);
            if (DetectDevice.DeviceType == DeviceType.Professional)
            {
                chkNotification.Visible = true;
            }
            ListSounds();
            cmbNotificationType.SelectedIndex = 0;
            
        }

        private void ListSounds()
        {
            string[] Sounds = System.IO.Directory.GetFiles("\\Windows", "*.wav");
            List<string> SoundNames = new List<string>();

            foreach (string Sound in Sounds)
            {
                SoundNames.Add(Sound);
            }
            Sounds = System.IO.Directory.GetFiles("\\Windows", "*.wma");
            foreach (string Sound in Sounds)
            {
                SoundNames.Add(Sound);
            }
            SoundNames.Sort();
            foreach (string SoundName in SoundNames)
            {
                SoundInfo SoundI = new SoundInfo();
                SoundI.Path = SoundName;
                cmbSound.Items.Add(SoundI);
            }
        }

        private void cmbNotificationType_SelectedIndexChanged(object sender, EventArgs e)
        {
            //Friends Update
            //New Messages
            currentInfo = (NotificationHandler.NotificationInfoClass)cmbNotificationType.SelectedItem;
            this.chkPlaySound.Checked = (currentInfo.Options & NotificationHandler.Options.Sound) == NotificationHandler.Options.Sound;
            this.cmbSound.Enabled = (currentInfo.Options & NotificationHandler.Options.Sound) == NotificationHandler.Options.Sound;
            this.chkVibrate.Checked = (currentInfo.Options & NotificationHandler.Options.Vibrate) == NotificationHandler.Options.Vibrate;
            this.chkNotification.Checked = (currentInfo.Options & NotificationHandler.Options.Message) == NotificationHandler.Options.Message;
            if (!string.IsNullOrEmpty(currentInfo.Sound))
            {
                SoundInfo s = new SoundInfo();
                s.Path = currentInfo.Sound;
                this.cmbSound.SelectedItem = s;
            }
            else
            {
                this.cmbSound.SelectedIndex = 0;
                SetSoundInfo();
            }
        }

        private void chkPlaySound_CheckStateChanged(object sender, EventArgs e)
        {
            cmbSound.Enabled = chkPlaySound.Checked;
        }

        private void cmbSound_SelectedIndexChanged(object sender, EventArgs e)
        {
            SetSoundInfo();
            NotificationHandler.SaveSettings(currentInfo);
        }

        private void SetSoundInfo()
        {
            SoundInfo selectedSound = (SoundInfo)cmbSound.SelectedItem;
            currentInfo.Sound = selectedSound.Path;
        }

        
        private void chkPlaySound_Click(object sender, EventArgs e)
        {
            if (chkPlaySound.Checked)
            {
                currentInfo.Options = currentInfo.Options | NotificationHandler.Options.Sound;
            }
            else
            {
                currentInfo.Options &= ~NotificationHandler.Options.Sound;
            }
            NotificationHandler.SaveSettings(currentInfo);
        }

        private void chkVibrate_Click(object sender, EventArgs e)
        {
            if (chkVibrate.Checked)
            {
                currentInfo.Options = currentInfo.Options | NotificationHandler.Options.Vibrate;
            }
            else
            {
                currentInfo.Options &= ~NotificationHandler.Options.Vibrate;
            }
            NotificationHandler.SaveSettings(currentInfo);
        }

        private void mnuDone_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void chkNotification_Click(object sender, EventArgs e)
        {
            if(chkNotification.Checked)
            {
                currentInfo.Options = currentInfo.Options | NotificationHandler.Options.Message;
            }
            else
            {
                currentInfo.Options &= ~NotificationHandler.Options.Message;
            }
            NotificationHandler.SaveSettings(currentInfo);
        }

        private Sound s;
        private void lnkPlay_Click(object sender, EventArgs e)
        {
            SoundInfo selectedSound = (SoundInfo)cmbSound.SelectedItem;
            
            s = new Sound(selectedSound.Path);
            s.Play();
        }
    }
}