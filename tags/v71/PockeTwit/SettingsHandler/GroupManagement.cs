using System;

using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace PockeTwit.SettingsHandler
{
    public partial class GroupManagement : BaseSettingsForm
    {
        public GroupManagement()
        {
            InitializeComponent();
            
            PockeTwit.Themes.FormColors.SetColors(this);
            if (ClientSettings.IsMaximized)
            {
                this.WindowState = FormWindowState.Maximized;
            }

            SpecialTimeLine[] Times = SpecialTimeLines.GetList();
            if (Times.Length == 0)
            {
                cmbChooseGroup.Visible = false;
                pnlUsers.Visible = false;
                return;
            }
            label1.Visible = false;
            foreach (SpecialTimeLine t in Times)
            {
                cmbChooseGroup.Items.Add(t);
            }
            cmbChooseGroup.SelectedIndex = 0;
            
        }

        private void menuItem1_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.OK;
        }

        private void cmbChooseGroup_SelectedIndexChanged(object sender, EventArgs e)
        {
            ResetGroupItems();
        }

        private void ResetGroupItems()
        {
            int width = pnlUsers.Width - (ClientSettings.TextSize+10);
            
            pnlUsers.Controls.Clear();

            Label lblName = new Label();
            Label lblExclusive = new Label();
            lblName.Text = "User";
            lblName.Width = width;
            lblName.Height = ClientSettings.TextSize + 5;
            lblExclusive.Text = "Exclusive";
            lblExclusive.Left = pnlUsers.Width - ((ClientSettings.TextSize*4) + 10);
            lblExclusive.Height = lblName.Height;
            pnlUsers.Controls.Add(lblExclusive);
            pnlUsers.Controls.Add(lblName);
            

            SpecialTimeLine t = (SpecialTimeLine)cmbChooseGroup.SelectedItem;
            int topOflabel = lblName.Bottom+5;
            foreach (SpecialTimeLine.groupTerm gt in t.Terms)
            {
                SpecialTimeLine.groupTerm gt1 = gt;
                LinkLabel nameLabel = new LinkLabel();
                nameLabel.Width = lblName.Width;

                nameLabel.Click += ((o,e1) => DeleteItem(t, gt1));
                nameLabel.Text = gt.Name;
                nameLabel.Top = topOflabel;
                nameLabel.Height = (int)ClientSettings.TextSize + 5;
                
                
                CheckBox chkExclusive = new CheckBox();
                
                chkExclusive.Click += ((o, e1) => ToggleExclusive(t, gt1, chkExclusive));
                chkExclusive.Left = nameLabel.Right - ClientSettings.TextSize;
                chkExclusive.Top = nameLabel.Top;
                chkExclusive.Width = ClientSettings.TextSize + 10;
                chkExclusive.Height = nameLabel.Height;
                chkExclusive.Checked = gt.Exclusive;
                pnlUsers.Controls.Add(chkExclusive);
                pnlUsers.Controls.Add(nameLabel);
                topOflabel = nameLabel.Bottom + 5;
            }
        }

        private  void DeleteItem(SpecialTimeLine t, SpecialTimeLine.groupTerm gt)
        {
            if(MessageBox.Show("Are you sure you want to remove " + gt.Name + " from this group?", "Remove "+ gt.Name, MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2)==DialogResult.Yes)
            {
                t.RemoveItem(gt.Term);
                ResetGroupItems();
                this.NeedsReRender = true;
            }

        }

        private void ToggleExclusive(SpecialTimeLine t, SpecialTimeLine.groupTerm gt, CheckBox sender)
        {
            gt.Exclusive = sender.Checked;
            this.NeedsReRender = true;
            SpecialTimeLines.Save();
        }

        private void lnkDeleteGroup_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("This will move all users from the " + cmbChooseGroup.SelectedItem + " group and delete the group.  The users will all appear in your friends timeline.\n\nProceed?", "Delete " + cmbChooseGroup.SelectedItem + " Group", MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2) == DialogResult.Yes)
            {
                SpecialTimeLines.Remove((SpecialTimeLine)cmbChooseGroup.SelectedItem);
                cmbChooseGroup.Items.Remove(cmbChooseGroup.SelectedItem);
                this.NeedsReRender = true;
            }
            if(cmbChooseGroup.Items.Count>0)
            {
                cmbChooseGroup.SelectedIndex = 0;
            }
            else
            {
                this.DialogResult = DialogResult.OK;
            }
        }

        private void menuExport_Click(object sender, EventArgs e)
        {
            if(saveFileDialog1.ShowDialog() == DialogResult.OK)
            {
                SpecialTimeLines.Export(saveFileDialog1.FileName);
            }
        }

        private void menuImport_Click(object sender, EventArgs e)
        {
            if(openFileDialog1.ShowDialog()== DialogResult.OK)
            {
                SpecialTimeLines.Import(openFileDialog1.FileName);
            }
        }
    }
}