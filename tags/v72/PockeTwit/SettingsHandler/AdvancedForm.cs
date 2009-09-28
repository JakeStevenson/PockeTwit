using System;

using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace PockeTwit.SettingsHandler
{
    public partial class AdvancedForm : BaseSettingsForm
    {
        public AdvancedForm()
        {
            InitializeComponent();
            PockeTwit.Themes.FormColors.SetColors(this);
            if (ClientSettings.IsMaximized)
            {
                this.WindowState = FormWindowState.Maximized;
            }
            SetRenderingMethod();
        }

        private void SetRenderingMethod()
        {
            if (ClientSettings.UseDIB)
            {
                lblRenderingMethod.Text = "Using DIB";
                chkDIB.Checked = true;
            }
            else
            {
                chkDIB.Checked = false;
                lblRenderingMethod.Text = "Using DDB";
            }
        }
        
        private void lnkClearCaches_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("Are you sure you want to delete all cached statuses?", "PockeTwit", MessageBoxButtons.YesNo, MessageBoxIcon.Hand, MessageBoxDefaultButton.Button2) == DialogResult.Yes)
            {
                LocalStorage.DataBaseUtility.CleanDB(0);
            }
        }


        private void lnkClearSettings_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("Are you sure you want to delete your settings?", "PockeTwit", MessageBoxButtons.YesNo, MessageBoxIcon.Hand, MessageBoxDefaultButton.Button2) == DialogResult.Yes)
            {
                System.IO.File.Delete(ClientSettings.AppPath + "\\app.config");
            }
        }

        private void menuItem1_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void chkDIB_CheckStateChanged(object sender, EventArgs e)
        {
            ClientSettings.UseDIB = chkDIB.Checked;
            ClientSettings.SaveSettings();
            this.NeedsReset = true;
            SetRenderingMethod();
        }

        private void lblCompact_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("Compacting the database may take a few minutes.\n\nProceed?", "Compact Database", MessageBoxButtons.YesNo,MessageBoxIcon.Question, MessageBoxDefaultButton.Button1) == DialogResult.Yes)
            {
                Cursor.Current = Cursors.WaitCursor;
                LocalStorage.DataBaseUtility.VacuumDB();
                Cursor.Current = Cursors.Default;
            }
        }

    }
}