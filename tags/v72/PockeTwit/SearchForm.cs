using System;

using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace PockeTwit
{
    public partial class SearchForm : Form
    {
        private delegate void delEnableDistance();

        public string GPSLocation = null;
        private LocationManager Locator = new LocationManager();

        private string _providedLocation;
        public string providedLocation {
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
                cmbMeasurement.Text = "Miles";
            }
        }

		#region Constructors (1) 

        public SearchForm()
        {
            Locator.LocationReady += new LocationManager.delLocationReady(Locator_LocationReady);
            InitializeComponent();
            SetUpSearchBox();
            SetupLocationBox();
            this.ResumeLayout(false);
            PockeTwit.Themes.FormColors.SetColors(this);
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
                cmbMeasurement.Text = "Miles";
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
                    cmbLocation.Items.Add("Anywhere");
                    if (!string.IsNullOrEmpty(_providedLocation))
                    {
                        cmbLocation.Items.Add(_providedLocation);
                        cmbLocation.SelectedItem = _providedLocation;
                    }
                    cmbLocation.Items.Add("Current GPS Position");
                    cmbLocation.Items.Add(Yedda.GoogleGeocoder.Geocode.GetAddress(this.GPSLocation).Replace("\r\n",""));
                    Locator.StopGPS();
                }
            }
        }
        
		#endregion Constructors 

		#region Properties (1) 

        public string SearchText{get;set;}

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
            if(!string.IsNullOrEmpty(txtSearch.Text))
            {
                b.Append("q=");
                b.Append(System.Web.HttpUtility.UrlEncode(txtSearch.Text));
                
            }
            if (cmbLocation.Text != "Anywhere")
            {
                if (!string.IsNullOrEmpty(cmbLocation.Text))
                {
                    Yedda.GoogleGeocoder.Coordinate c = Yedda.GoogleGeocoder.Geocode.GetCoordinates(cmbLocation.Text);
                    this.GPSLocation = System.Web.HttpUtility.UrlEncode(c.ToString());
                    if (!string.IsNullOrEmpty(cmbDistance.Text) && !string.IsNullOrEmpty(cmbMeasurement.Text))
                    {
                        if (b.Length > 0)
                        {
                            b.Append("&");
                        }
                        b.Append("geocode=" + this.GPSLocation);
                        b.Append("," + cmbDistance.Text);
                        switch (cmbMeasurement.Text)
                        {
                            case "Miles":
                                b.Append("mi");
                                break;
                            case "Kilometers":
                                b.Append("km");
                                break;
                        }
                    }
                }
            }
            this.SearchText = b.ToString();
            
            this.DialogResult = DialogResult.OK;
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
            cmbLocation.Items.Add("Anywhere");
            cmbLocation.Text = "Anywhere";
            if (ClientSettings.UseGPS)
            {
                cmbLocation.Items.Add("Seeking GPS...Please Wait");
            }
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
            this.txtSearch.Location = new System.Drawing.Point(57, 89);
            this.txtSearch.Name = "txtSearch";
            this.txtSearch.Size = this.cmbLocation.Size;
            txtSearch.BringToFront();
            this.txtSearch.TabIndex = 8;

            
            this.Controls.Add(this.txtSearch);
            this.ResumeLayout(false);

        }
    }
}