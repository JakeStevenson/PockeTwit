using System;

using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace PockeTwit.SettingsHandler.ThemeCreator
{
    public partial class NewThemeName : Form
    {
        public string ThemeName;
        public NewThemeName()
        {
            InitializeComponent();
            PockeTwit.Themes.FormColors.SetColors(this);
        }

        private void menuItem1_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }

        private void menuItem2_Click(object sender, EventArgs e)
        {
            ThemeName = textBox1.Text;
            this.DialogResult = DialogResult.OK;
            this.Close();
        }
    }
}