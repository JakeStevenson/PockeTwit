using System;

using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace PockeTwit
{
    public partial class DefineGroup : Form
    {
        public DefineGroup()
        {
            InitializeComponent();
            Themes.FormColors.SetColors(this);
            PockeTwit.Localization.XmlBasedResourceManager.LocalizeForm(this);

        }

        public string GroupName
        {
            get
            {
                return txtName.Text;
            }
            set
            {
                txtName.Text = value;
            }
        }

        private void menuItem2_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
        }

        private void menuItem1_Click(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(GroupName))
            {
                this.DialogResult = DialogResult.OK;
            }
        }
    }
}