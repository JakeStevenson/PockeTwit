using System;

using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using PockeTwit.OtherServices;
using PockeTwit.SpecialTimelines;

namespace PockeTwit
{
    public partial class SearchForm : Form
    {
        private delegate void delEnableDistance();

        public SpecialTimelines.SavedSearchTimeLine SavedSearch;

        public string GPSLocation = null;
        private LocationManager Locator = new LocationManager();

        private string _providedLocation;
        public string providedLocation
        {
            get
            {
                return _providedLocation;
            }
            set
            {
                _providedLocation = value;
                cmbLocation.Items.Add(_providedLocation);
                cmbLocation.SelectedItem = _providedLocation;
                cmbLocation.Text = _providedLocation;
            }
        }

        public string providedDistnce
        {
            set
            {
                cmbDistance.Items.Add(value);
                cmbDistance.SelectedItem = value;
                cmbDistance.Text = value;
                cmbMeasurement.Text = PockeTwit.Localization.XmlBasedResourceManager.GetString("Miles");
            }
        }

        #region Constructors (1)

        public SearchForm()
        {
            Locator.LocationReady += new LocationManager.delLocationReady(Locator_LocationReady);
            InitializeComponent();
            SetUpSearchBox();
            SetupLocationBox();
            cmbMeasurement.Tag = "AutoLocalize";
            this.ResumeLayout(false);
            PockeTwit.Themes.FormColors.SetColors(this); 
            PockeTwit.Localization.XmlBasedResourceManager.LocalizeForm(this);
            if (ClientSettings.IsMaximized)
            {
                this.WindowState = FormWindowState.Maximized;
            }
            this.DialogResult = DialogResult.Cancel;

            if (ClientSettings.UseGPS)
            {
                Locator.StartGPS();

            }
            //cmbMeasurement.SelectedValue = ClientSettings.DistancePreference;
            if (!string.IsNullOrEmpty(ClientSettings.DistancePreference))
            {
                cmbMeasurement.Text = ClientSettings.DistancePreference;
            }
            else
            {
                cmbMeasurement.Text = PockeTwit.Localization.XmlBasedResourceManager.GetString("Miles");
            }

            cmbDistance.Text = "15";

            txtSearch.Focus();
        }

        void Locator_LocationReady(string Location)
        {
            GPSLocked();
            GPSLocation = Location;
        }

        private delegate void delGPSLocked();
        private void GPSLocked()
        {
            if (InvokeRequired)
            {
                delGPSLocked d = new delGPSLocked(GPSLocked);
                this.Invoke(d, null);
            }
            else
            {
                if (!string.IsNullOrEmpty(this.GPSLocation))
                {
                    cmbLocation.Items.Clear();
                    cmbLocation.Items.Add(PockeTwit.Localization.XmlBasedResourceManager.GetString("Anywhere"));
                    if (!string.IsNullOrEmpty(_providedLocation))
                    {
                        cmbLocation.Items.Add(_providedLocation);
                        cmbLocation.SelectedItem = _providedLocation;
                    }
                    cmbLocation.Items.Add(PockeTwit.Localization.XmlBasedResourceManager.GetString("Current GPS Position"));
                    cmbLocation.Items.Add(Geocode.GetAddress(this.GPSLocation).Replace("\r\n", ""));
                    Locator.StopGPS();
                }
            }
        }

        #endregion Constructors

        #region Properties (1)

        public string SearchText { get; set; }

        #endregion Properties

        #region Methods (2)


        // Private Methods (2) 

        private void menuCancel_Click(object sender, EventArgs e)
        {
            if (ClientSettings.UseGPS)
            {
                Locator.StopGPS();
            }
            this.DialogResult = DialogResult.Cancel;

        }

        private void menuSearch_Click(object sender, EventArgs e)
        {

            if (DetectDevice.DeviceType == DeviceType.Professional)
            {
                if (!string.IsNullOrEmpty(txtSearch.Text) && !ClientSettings.SearchItems.Contains(txtSearch.Text))
                {
                    ClientSettings.SearchItems.Enqueue(txtSearch.Text);
                    if (ClientSettings.SearchItems.Count > 8)
                    {
                        ClientSettings.SearchItems.Dequeue();
                    }
                    ClientSettings.SaveSettings();
                }
            }
            if (ClientSettings.UseGPS)
            {
                Locator.StopGPS();
            }
            StringBuilder b = new StringBuilder();
            if (!string.IsNullOrEmpty(txtSearch.Text))
            {

                if (!txtSearch.Text.StartsWith("q="))
                {
                    b.Append("q=");
                    b.Append(System.Web.HttpUtility.UrlEncode(txtSearch.Text));
                }
                else
                {
                    // term is an advanced search
                    b.Append(txtSearch.Text);
                }
            }
            if (cmbLocation.Text != PockeTwit.Localization.XmlBasedResourceManager.GetString("Anywhere"))
            {
                if (!string.IsNullOrEmpty(cmbLocation.Text))
                {
                    Coordinate c = Geocode.GetCoordinates(cmbLocation.Text);
                    this.GPSLocation = System.Web.HttpUtility.UrlEncode(c.ToString());
                    if (!string.IsNullOrEmpty(cmbDistance.Text) && !string.IsNullOrEmpty(cmbMeasurement.Text))
                    {
                        if (b.Length > 0)
                        {
                            b.Append("&");
                        }
                        b.Append("geocode=" + this.GPSLocation);
                        b.Append("," + cmbDistance.Text);
                        string miles = PockeTwit.Localization.XmlBasedResourceManager.GetString("Miles");
                        if (cmbMeasurement.Text == miles)
                            b.Append("mi");
                        else                           
                            b.Append("km");
                    }
                }
            }
            this.SearchText = b.ToString();

            if (chkSaveSearch.Checked && !string.IsNullOrEmpty(txtGroupName.Text))
            {
                if (SureTheyWantToSave())
                {
                    SavedSearch = new SavedSearchTimeLine();
                    SavedSearch.name = txtGroupName.Text;
                    SavedSearch.SearchPhrase = this.SearchText;
                    SavedSearch.autoUpdate = chkAutoUpdate.Checked;
                    SpecialTimeLinesRepository.Add(SavedSearch);
                    SpecialTimeLinesRepository.Save();
                }
                else
                {
                    chkSaveSearch.Checked = false;
                    return;
                }

            }

            this.DialogResult = DialogResult.OK;
        }

        private bool SureTheyWantToSave()
        {
            //We don't need to warn them about searches without auto-update
            if (!chkAutoUpdate.Checked)
            {
                return true;
            }
            return PockeTwit.Localization.LocalizedMessageBox.Show("Saved searches can affect battery life. Are you sure you want to add this?",
                                   "Saved Search", MessageBoxButtons.YesNo, MessageBoxIcon.Question,
                                   MessageBoxDefaultButton.Button2) == DialogResult.Yes;
        }


        #endregion Methods

        private void cmbMeasurement_SelectedValueChanged(object sender, EventArgs e)
        {
            ClientSettings.DistancePreference = (string)cmbMeasurement.SelectedValue;
            ClientSettings.SaveSettings();
        }

        private void SetupLocationBox()
        {
            if (DetectDevice.DeviceType == DeviceType.Professional)
            {
                cmbLocation.DropDownStyle = ComboBoxStyle.DropDown;
            }
            string item = PockeTwit.Localization.XmlBasedResourceManager.GetString("Anywhere");
            cmbLocation.Items.Add(item);
            cmbLocation.Text = item;
            if (ClientSettings.UseGPS)
            {
                cmbLocation.Items.Add(PockeTwit.Localization.XmlBasedResourceManager.GetString("Seeking GPS...Please Wait"));
            }
            cmbLocation.Tag = "AutoLocalize";
        }

        private void SetUpSearchBox()
        {

            if (DetectDevice.DeviceType == DeviceType.Professional)
            {
                this.txtSearch = new System.Windows.Forms.ComboBox();
                ComboBox txtSearchBox = (ComboBox)txtSearch;
                txtSearchBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDown;
                foreach (string Item in ClientSettings.SearchItems)
                {
                    txtSearchBox.Items.Add(Item);
                }
            }
            else
            {
                this.txtSearch = new System.Windows.Forms.TextBox();
            }

            this.txtSearch.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.txtSearch.Location = new System.Drawing.Point(txtDummy.Left, txtDummy.Top);
            this.txtSearch.Name = "txtSearch";
            this.txtSearch.Height = this.txtDummy.Size.Height;
            this.txtSearch.Width = this.txtDummy.Size.Width;
            txtSearch.BringToFront();
            this.txtSearch.TabIndex = 8;

            this.Controls.Add(this.txtSearch);
            this.ResumeLayout(false);

        }

        void btnAdvancedSearch_Click(object sender, EventArgs e)
        {
            if (PockeTwit.DetectDevice.DeviceType == PockeTwit.DeviceType.Standard)
            {
                using (AdvancedSearchSmartphoneForm f = new AdvancedSearchSmartphoneForm())
                {
                    if (f.ShowDialog() == DialogResult.Cancel)
                    {
                        f.Close();
                        return;
                    }
                    txtSearch.Text = f.Query;
                    f.Close();
                }
            }
            else
            {
                using (AdvancedSearchForm f = new AdvancedSearchForm())
                {
                    if (f.ShowDialog() == DialogResult.Cancel)
                    {
                        f.Close();
                        return;
                    }
                    txtSearch.Text = f.Query;
                    f.Close();
                }
            }
        }

        private void chkSaveSearch_Click(object sender, EventArgs e)
        {
            txtGroupName.Enabled = chkSaveSearch.Checked;
            chkAutoUpdate.Enabled = chkSaveSearch.Checked;
        }

        private void chkSaveSearch_CheckStateChanged(object sender, EventArgs e)
        {

        }
    }
}