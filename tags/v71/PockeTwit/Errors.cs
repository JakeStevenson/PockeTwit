using System;

using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace PockeTwit
{
    public partial class Errors : Form
    {
        public Errors()
        {
            InitializeComponent();
            PockeTwit.Themes.FormColors.SetColors(this);
            StringBuilder erstring = new StringBuilder();
            erstring.Append("Communication Errors:\r\n");
            foreach (Yedda.Twitter.Account accountKey in Yedda.Twitter.Failures.Keys)
            {
                lock (Yedda.Twitter.Failures[accountKey])
                {
                    Dictionary<Yedda.Twitter.ActionType, int> Failure = Yedda.Twitter.Failures[accountKey];
                    foreach (Yedda.Twitter.ActionType key in Failure.Keys)
                    {
                        if (Failure[key] > 0)
                        {
                            erstring.Append(accountKey.ToString() + "-" + key.ToString() + ": " + Failure[key].ToString() + "\r\n");
                        }
                    }
                }
            }
            lblErrors.Text = erstring.ToString();
        }

        private void menuCancel_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}