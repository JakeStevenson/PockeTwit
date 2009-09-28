using System;

using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Globalization;
using System.Collections.ObjectModel;

namespace PockeTwit
{
    public partial class UISettings : BaseSettingsForm
    {
        private string OriginalTheme = ClientSettings.ThemeName;

        #region Constructors (1) 

        public UISettings()
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

		#endregion Constructors 


		#region Methods (4) 


		// Private Methods (4) 

        

        private void menuAccept_Click(object sender, EventArgs e)
        {
            lblTweets.ForeColor = ClientSettings.ForeColor;
            int MaxTweets = 0;
            try
            {
                MaxTweets = int.Parse(txtMaxTweets.Text);
                if (MaxTweets > 200 || MaxTweets < 10)
                {
                    lblTweets.ForeColor = ClientSettings.ErrorColor;
                    Cursor.Current = Cursors.Default;
                    return;
                }
            }
            catch
            {
                lblTweets.ForeColor = ClientSettings.ErrorColor;
                Cursor.Current = Cursors.Default;
                return;
            }
            if (MaxTweets != ClientSettings.MaxTweets) { NeedsReset = true; }
            if (chkTimestamps.Checked != ClientSettings.ShowExtra) { this.NeedsReRender = true; }
            if (chkScreenName.Checked != ClientSettings.IncludeUserName) { this.NeedsReRender = true; }
            
            ClientSettings.MaxTweets = MaxTweets;
            ClientSettings.UseClickables = chkClickables.Checked;
            ClientSettings.ShowExtra = chkTimestamps.Checked;
            ClientSettings.IncludeUserName = chkScreenName.Checked;
            ClientSettings.AutoScrollToTop = chkAutoScrroll.Checked;

            int newSize = int.Parse(this.txtFontSize.Text);
            if(ClientSettings.FontSize != newSize && newSize < 13 && newSize > 6)
            {
                ClientSettings.FontSize = int.Parse(txtFontSize.Text);
                NeedsReset = true;
            }
            
            CultureInfo selectedCuture = (CultureInfo)cmbLanguage.SelectedItem;
            if (PockeTwit.Localization.XmlBasedResourceManager.CultureInfo != selectedCuture)
            {
                PockeTwit.Localization.XmlBasedResourceManager.CultureInfo = selectedCuture;
                NeedsReset = true;
            }

            string selectedTheme = (string)cmbTheme.SelectedItem;
            if (selectedTheme != OriginalTheme)
            {
                ClientSettings.ThemeName = selectedTheme;
                this.NeedsReRender = true;
            }
            ClientSettings.LoadColors();

            ClientSettings.SaveSettings();
            this.DialogResult = DialogResult.OK;
            this.Close();

        }

        private void menuCancel_Click(object sender, EventArgs e)
        {
            if (ClientSettings.ThemeName != OriginalTheme)
            {
                ClientSettings.ThemeName = OriginalTheme;
            }
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }

        private void PopulateForm()
        {
            txtMaxTweets.Text = ClientSettings.MaxTweets.ToString();
            chkTimestamps.Checked = ClientSettings.ShowExtra;
            chkClickables.Checked = ClientSettings.UseClickables;
            chkScreenName.Checked = ClientSettings.IncludeUserName;
            chkAutoScrroll.Checked = ClientSettings.AutoScrollToTop;
            txtFontSize.Text = ClientSettings.FontSize.ToString();
            
            ListThemes();
            ListLanguages();
            this.DialogResult = DialogResult.Cancel;
        }
        private void ListThemes()
        {
            cmbTheme.Items.Clear();
            List<string> ItemList = new List<string>();
            foreach (string ThemeFile in System.IO.Directory.GetDirectories(ClientSettings.AppPath + "\\Themes\\"))
            {
                string themeName = System.IO.Path.GetFileNameWithoutExtension(ThemeFile);
                ItemList.Add(themeName);
            }

            ItemList.Sort();
            foreach (string item in ItemList)
            {
                cmbTheme.Items.Add(item);
            }
            cmbTheme.SelectedItem = ClientSettings.ThemeName;
        }

        private void ListLanguages()
        {
            cmbLanguage.Items.Clear();
            cmbLanguage.Items.Add(new CultureInfo("en"));

            ReadOnlyCollection<CultureInfo> langs = PockeTwit.Localization.XmlBasedResourceManager.AvailableCultures();
            foreach (CultureInfo info in langs)
            {
                cmbLanguage.Items.Add(info);
            }

            cmbLanguage.SelectedItem = PockeTwit.Localization.XmlBasedResourceManager.CultureInfo;
        }

		#endregion Methods 

        private void cmbTheme_SelectedIndexChanged(object sender, EventArgs e)
        {
            string selectedTheme = (string)cmbTheme.SelectedItem;
            ClientSettings.ThemeName = selectedTheme;
            ClientSettings.LoadColors();
            Themes.FormColors.SetColors(this);
        }

        private void linkLabel1_Click(object sender, EventArgs e)
        {
            string selectedTheme = (string)cmbTheme.SelectedItem;
            using (ColorPick c = new ColorPick(selectedTheme))
            {
                c.ShowDialog();
            }
            ListThemes();
            cmbTheme_SelectedIndexChanged(null, new EventArgs());
        }

        

        private void txtFontSize_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!Char.IsDigit(e.KeyChar) & e.KeyChar != '\b')
            {
                e.Handled = true;
            }
        }

        
    }
}