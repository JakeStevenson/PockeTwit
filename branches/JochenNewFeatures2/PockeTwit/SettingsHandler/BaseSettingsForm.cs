using System;

using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace PockeTwit
{
    public partial class BaseSettingsForm : Form
    {

        public bool NeedsReset { get; set; }
        public bool NeedsReRender { get; set; }
        public BaseSettingsForm()
        {
            InitializeComponent();
        }
    }
}