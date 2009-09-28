using System;

using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace PockeTwit
{
    public partial class URLForm : Form
    {

		#region Fields (1) 

        private string _URL;

		#endregion Fields 

		#region Constructors (1) 
        ContextMenu contextMen;
        MenuItem pasteItem;
        public URLForm()
        {
            InitializeComponent();
            PockeTwit.Themes.FormColors.SetColors(this);
            if (ClientSettings.IsMaximized)
            {
                this.WindowState = FormWindowState.Maximized;
            }
            
            if (DetectDevice.DeviceType == DeviceType.Professional)
            {
                ProfesionalMenus();
            }
            else
            {
                StandardMenus();
            }
        }
        private void StandardMenus()
        {
            pasteItem = new MenuItem();
            pasteItem.Text = "Paste";
            pasteItem.Click+=new EventHandler(pasteItem_Click);

            this.mnuCancel.Text = "Cancel";
            this.mnuCancel.Click += new System.EventHandler(this.menuCancel_Click);

            MenuItem SubmitItem = new MenuItem();
            SubmitItem.Text = "Submit";
            SubmitItem.Click += new EventHandler(this.menuSubmit_Click);

            mnuAction.MenuItems.Add(SubmitItem);
            mnuAction.MenuItems.Add(pasteItem);
            
            this.mnuAction.Text = "Action";

            this.mainMenu1.MenuItems.Add(this.mnuAction);
            this.mainMenu1.MenuItems.Add(this.mnuCancel);
        }

        private void ProfesionalMenus()
        {
            contextMen = new ContextMenu();
            pasteItem = new MenuItem();
            pasteItem.Text = "Paste";
            contextMen.MenuItems.Add(pasteItem);
            this.txtURL.ContextMenu = contextMen;
            pasteItem.Click += new EventHandler(pasteItem_Click);

            this.mnuCancel.Text = "Cancel";
            this.mnuCancel.Click += new System.EventHandler(this.menuCancel_Click);

            this.mnuAction.Text = "Ok";
            this.mnuAction.Click += new System.EventHandler(this.menuSubmit_Click);
           

            this.mainMenu1.MenuItems.Add(this.mnuAction);
            this.mainMenu1.MenuItems.Add(this.mnuCancel);
        }

        void pasteItem_Click(object sender, EventArgs e)
        {
            IDataObject iData = Clipboard.GetDataObject();
            if (iData.GetDataPresent(DataFormats.Text))
            {
                txtURL.SelectedText = (string)iData.GetData(DataFormats.Text); 
            }
        }

		#endregion Constructors 

		#region Properties (1) 

        public string URL
        {
            get
            {
                return _URL;
            }
        }

		#endregion Properties 

		#region Methods (2) 


		// Private Methods (2) 

        private void menuSubmit_Click(object sender, EventArgs e)
        {
            try
            {
                _URL = isgd.ShortenURL(this.txtURL.Text);
                if (string.IsNullOrEmpty(_URL))
                {
                    MessageBox.Show("A communication error occured shortening the URL. Please try again later.");
                    return;
                }
                this.DialogResult = DialogResult.OK;
                this.Hide();
            }
            catch 
            {
                return;
            }
            
        }

        private void menuCancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Hide();
            
        }


		#endregion Methods 

    }
}