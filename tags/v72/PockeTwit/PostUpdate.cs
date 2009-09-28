using System;

using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

using Yedda;

namespace PockeTwit
{
    public partial class PostUpdate : Form
    {
        private System.Windows.Forms.ContextMenu copyPasteMenu;
        private System.Windows.Forms.MenuItem PasteItem;
        private System.Windows.Forms.MenuItem CopyItem;
		
        public string GPSLocation = null;
        private LocationManager LocationFinder = new LocationManager();
        private bool _StandAlone;
        private delegate void delUpdateText(string text);

        private IPictureService pictureService;
        private string uploadedPictureOrigin = string.Empty;
        private string uploadedPictureURL = string.Empty;
        private bool uploadingPicture = false;
        private bool pictureUsed = true;
        private bool localPictureEventsSet = false;
        private string picturePath = string.Empty;

        private int availableChars = 140; //should retrieve this from a blogservice.

        public delegate void delAddPicture(string ImageFile, PictureBox BoxToUpdate);
        public delegate void delUpdatePictureData(string pictureUrl, bool uploadingPicture);

        #region Properties
        private Yedda.Twitter.Account _AccountToSet = ClientSettings.DefaultAccount;
        public Yedda.Twitter.Account AccountToSet
        {
            get
            {
                return _AccountToSet;
            }
            set
            {
                _AccountToSet = value;
                cmbAccount.SelectedItem = _AccountToSet;
                Yedda.Twitter t = new Yedda.Twitter();
                t.AccountInfo = _AccountToSet;
                AllowTwitPic = t.AllowTwitPic;
            }
        }

        public bool AllowTwitPic
        {
            set
            {
                pictureFromCamers.Visible = value;
                pictureFromStorage.Visible = value;
            }
        }

        public string StatusText
        {
            get
            {
                return txtStatusUpdate.Text;
            }
            set
            {
                txtStatusUpdate.Text = value;
                this.txtStatusUpdate.SelectionStart = txtStatusUpdate.Text.Length;
            }
        }

        public string in_reply_to_status_id { get; set; }

        #endregion


        public PostUpdate(bool Standalone)
        {
            _StandAlone = Standalone;
            InitializeComponent();
            SetImages();
            userListControl1.HookTextBoxKeyPress(txtStatusUpdate);
            PockeTwit.Themes.FormColors.SetColors(this);
            if (ClientSettings.IsMaximized)
            {
                this.WindowState = FormWindowState.Maximized;
            }

            lblCharsLeft.Text = "140";
            PopulateAccountList();
            if (DetectDevice.DeviceType == DeviceType.Standard)
            {
                SmartPhoneMenu();
            }
            else
            {
                SetupTouchScreen();
            }
            this.mainMenu1.MenuItems.Add(this.menuCancel);
            SizeF currentScreen = this.CurrentAutoScaleDimensions;
            
            this.ResumeLayout(false);

            LocationFinder.LocationReady += new LocationManager.delLocationReady(l_LocationReady);

            this.txtStatusUpdate.Focus();
            userListControl1.ItemChosen += new userListControl.delItemChose(userListControl1_ItemChosen);
        }

        private void SetupTouchScreen()
        {
            this.mainMenu1.MenuItems.Add(this.menuSubmit);
            this.pictureFromCamers.Click += new EventHandler(pictureFromCamers_Click);
            this.pictureFromStorage.Click += new EventHandler(pictureFromStorage_Click);
            this.pictureURL.Click += new EventHandler(pictureURL_Click);
            this.pictureLocation.Click += new EventHandler(pictureLocation_Click);

            this.picAddressBook.Click += new EventHandler(picAddressBook_Click);

            this.copyPasteMenu = new System.Windows.Forms.ContextMenu();

            this.PasteItem = new System.Windows.Forms.MenuItem();
            PasteItem.Text = "Paste";

            this.CopyItem = new MenuItem();
            CopyItem.Text = "Copy";

            copyPasteMenu.MenuItems.Add(CopyItem);
            copyPasteMenu.MenuItems.Add(PasteItem);
            this.txtStatusUpdate.ContextMenu = copyPasteMenu;

            CopyItem.Click += new EventHandler(CopyItem_Click);
            PasteItem.Click += new EventHandler(PasteItem_Click);

        }

        void picAddressBook_Click(object sender, EventArgs e)
        {
            txtStatusUpdate.Text = txtStatusUpdate.Text + "@";
            userListControl1.Visible = true;
            userListControl1.Focus();
        }

        void userListControl1_ItemChosen(string itemText)
        {
            txtStatusUpdate.Text = txtStatusUpdate.Text + itemText;
            txtStatusUpdate.SelectionStart = txtStatusUpdate.Text.Length;
            txtStatusUpdate.Focus();
        }

        void PasteItem_Click(object sender, EventArgs e)
        {
            IDataObject iData = Clipboard.GetDataObject();
            if (iData.GetDataPresent(DataFormats.Text))
            {
                this.txtStatusUpdate.SelectedText = (string)iData.GetData(DataFormats.Text);
            }
        }
        void CopyItem_Click(object sender, EventArgs e)
        {
            string selText = this.txtStatusUpdate.SelectedText;
            if (!string.IsNullOrEmpty(selText))
            {
                Clipboard.SetDataObject(selText);
            }
        }

        
        void l_LocationReady(string Location)
        {
            try
            {
                if (InvokeRequired)
                {
                    delUpdateText d = new delUpdateText(l_LocationReady);
                    this.BeginInvoke(d, Location);
                }
                else
                {
                    if (!string.IsNullOrEmpty(Location))
                    {
                        LocationFinder.StopGPS();
                        this.GPSLocation = Location;
                        lblGPS.Text = "Location Found";
                        if (DetectDevice.DeviceType == DeviceType.Standard)
                        {
                            // just enable the menuItem
                            if (null != menuGPSInsert)
                            {
                                menuGPSInsert.Enabled = true;
                            }
                        }
                        else
                        {
                            // hide the label, add a new button
                            lblGPS.Visible = false;
                            LinkLabel llGPS = new LinkLabel();
                            llGPS.Text = "Ins. GPS Link";
                            llGPS.ForeColor = Color.White;
                            llGPS.Left = lblGPS.Left;
                            llGPS.Top = lblGPS.Top;
                            llGPS.Height = lblGPS.Height;
                            llGPS.Width = lblGPS.Width;
                            llGPS.Click += new EventHandler(llGPS_Click);
                            this.Controls.Add(llGPS);
                        }
                    }
                }
            }
            catch (ObjectDisposedException)
            {
            }
        }

        void llGPS_Click(object sender, EventArgs e)
        {
            InsertGpsLocation();
        }

        private void StartLocating()
        {   
            //StartAnimation
            LocationFinder.StartGPS();
            pictureLocation.Visible = false;
            lblGPS.Visible = true;
        }

        

        private System.Windows.Forms.MenuItem menuExist;
        private System.Windows.Forms.MenuItem menuCamera;
        private System.Windows.Forms.MenuItem menuURL;
        private System.Windows.Forms.MenuItem menuGPS;
        private System.Windows.Forms.MenuItem menuGPSInsert;
        private System.Windows.Forms.MenuItem menuAddressBook;
        private System.Windows.Forms.MenuItem menuItem1;
        private void SmartPhoneMenu()
        {
            lblGPS.Left = 5;
            pictureFromCamers.Visible = false;
            pictureFromStorage.Visible = false;
            pictureLocation.Visible = false;
            pictureURL.Visible = false;
            picAddressBook.Visible = false;
            this.menuExist = new MenuItem();
            menuExist.Text = "Existing Picture";
            menuExist.Click += new EventHandler(menuExist_Click);

            this.menuCamera = new MenuItem();
            menuCamera.Text = "Take Picture";
            menuCamera.Click += new EventHandler(menuCamera_Click);
            
            this.menuURL = new MenuItem();
            menuURL.Text = "URL...";
            menuURL.Click += new EventHandler(menuURL_Click);

            this.menuGPS = new MenuItem();
            menuGPS.Text = "Update Location";
            menuGPS.Click += new EventHandler(menuGPS_Click);

            this.menuGPSInsert = new MenuItem();
            menuGPSInsert.Text = "Insert GPS Location";
            menuGPSInsert.Click += new EventHandler(menuGPSInsert_Click);
            menuGPSInsert.Enabled = false;

            this.menuAddressBook = new MenuItem();
            menuAddressBook.Text = "Address Book";
            menuAddressBook.Click += new EventHandler(menuAddressBook_Click);
            menuAddressBook.Enabled = true;

            this.PasteItem = new MenuItem();
            this.PasteItem.Text = "Paste";
            PasteItem.Click += new EventHandler(PasteItem_Click);

            this.menuItem1 = new System.Windows.Forms.MenuItem();
            this.menuItem1.Text = "Action";

            this.menuItem1.MenuItems.Add(this.menuSubmit);
            this.menuItem1.MenuItems.Add(menuAddressBook);
            this.menuItem1.MenuItems.Add(PasteItem);
            this.menuItem1.MenuItems.Add(menuURL);
            this.menuItem1.MenuItems.Add(menuExist);
            this.menuItem1.MenuItems.Add(menuCamera);
            this.menuItem1.MenuItems.Add(menuGPS);
            this.menuItem1.MenuItems.Add(menuGPSInsert);
            this.mainMenu1.MenuItems.Add(menuItem1);
        }

        void menuAddressBook_Click(object sender, EventArgs e)
        {
            txtStatusUpdate.Text = txtStatusUpdate.Text + "@";
            userListControl1.Visible = true;
            userListControl1.Focus();
        }

        void menuGPSInsert_Click(object sender, EventArgs e)
        {
            InsertGpsLocation();
        }

        private void InsertGpsLocation()
        {
            // google maps url format
            // http://maps.google.com/maps?f=q&source=s_q&hl=en&geocode=&q=41.4043197631836,-1.28760504722595
            Cursor.Current = Cursors.WaitCursor;
            string sUrl = string.Format(@"http://maps.google.com/maps?q={0}", this.GPSLocation);
            string gpsUrl = isgd.ShortenURL(sUrl);
            if (string.IsNullOrEmpty(gpsUrl))
            {
                Cursor.Current = Cursors.Default;
                MessageBox.Show("A communication error occured shortening the URL. Please try again later.");
                return;
            }
            txtStatusUpdate.Text = txtStatusUpdate.Text + " " + gpsUrl;
            Cursor.Current = Cursors.Default;
        }

        void menuGPS_Click(object sender, EventArgs e)
        {
            StartLocating();
        }

        void menuURL_Click(object sender, EventArgs e)
        {
            InsertURL();
        }

        void menuCamera_Click(object sender, EventArgs e)
        {
            InsertPictureFromCamera();
        }

        void menuExist_Click(object sender, EventArgs e)
        {
            InsertPictureFromFile();
        }

        private void SetImages()
        {
            this.pictureFromCamers.Image = PockeTwit.Themes.FormColors.GetThemeIcon("takepicture.png");
            this.pictureFromStorage.Image = PockeTwit.Themes.FormColors.GetThemeIcon("existingimage.png");
            this.pictureURL.Image = PockeTwit.Themes.FormColors.GetThemeIcon("url.png");
            this.pictureLocation.Image = PockeTwit.Themes.FormColors.GetThemeIcon("map.png");
            this.picAddressBook.Image = PockeTwit.Themes.FormColors.GetThemeIcon("address.png");
        }

        private void PopulateAccountList()
        {
            foreach (Yedda.Twitter.Account Account in ClientSettings.AccountsList)
            {
                cmbAccount.Items.Add(Account);
            }
        }

        #region Methods
        private void InsertURL()
        {
            using (URLForm f = new URLForm())
            {
                if (f.ShowDialog() == DialogResult.OK)
                {
                    txtStatusUpdate.Text = txtStatusUpdate.Text + " " + f.URL;
                }
                this.Show();
                f.Close();
            }
        }

        private void InsertPictureFromCamera()
        {
            if (!uploadingPicture)
            {
                using (Microsoft.WindowsMobile.Forms.CameraCaptureDialog c = new Microsoft.WindowsMobile.Forms.CameraCaptureDialog())
                {
                    String pictureUrl = string.Empty;
                    try
                    {
                        if (c.ShowDialog() == DialogResult.OK)
                        {
                            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(PostUpdate));
                            this.pictureFromStorage.Image = PockeTwit.Themes.FormColors.GetThemeIcon("existingimage.png");
                            if (DetectDevice.DeviceType == DeviceType.Standard)
                            {
                                this.pictureFromStorage.Visible = false;
                            }
                            uploadedPictureOrigin = "camera";
                           
                            
                            pictureService = GetMediaService();
                            if (pictureService.CanUploadMessage && ClientSettings.SendMessageToMediaService)
                            {
                                AddPictureToForm(c.FileName, pictureFromCamers);
                                picturePath = c.FileName;
                                //Reduce length of message 140-pictureService.UrlLength
                                
                                pictureUsed = true;
                            }
                            else
                            {
                                AddPictureToForm(ClientSettings.IconsFolder() + "wait.png", pictureFromCamers);
                                uploadingPicture = true;
                                using (PicturePostObject ppo = new PicturePostObject())
                                {
                                    ppo.Filename = c.FileName;
                                    ppo.Username = AccountToSet.UserName;
                                    ppo.Password = AccountToSet.Password;
                                    ppo.UseAsync = false;
                                    Cursor.Current = Cursors.WaitCursor;
                                    pictureService.PostPicture(ppo);
                                }
                            }
                        }
                    }
                    catch
                    {
                        MessageBox.Show("The camera is not available.", "PockeTwit");
                    }
                }
            }
            else
            {
                MessageBox.Show("Uploading picture...");
            }
        }

        private void InsertPictureFromFile()
        {
            if (!uploadingPicture)
            {
                String pictureUrl = string.Empty;
                using (Microsoft.WindowsMobile.Forms.SelectPictureDialog s = new Microsoft.WindowsMobile.Forms.SelectPictureDialog())
                {
                    if (s.ShowDialog() == DialogResult.OK)
                    {
                        System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(PostUpdate));
                        this.pictureFromCamers.Image = PockeTwit.Themes.FormColors.GetThemeIcon("takepicture.png");
                        if (DetectDevice.DeviceType == DeviceType.Standard)
                        {
                            this.pictureFromCamers.Visible = false;
                        }
                        uploadedPictureOrigin = "file";

                        pictureService = GetMediaService();
                        if (pictureService.CanUploadMessage && ClientSettings.SendMessageToMediaService)
                        {
                            AddPictureToForm(s.FileName, pictureFromStorage);
                            picturePath = s.FileName;
                            //Reduce length of message 140-pictureService.UrlLength
                            pictureUsed = true;
                        }
                        else
                        {
                            uploadingPicture = true;
                            AddPictureToForm(ClientSettings.IconsFolder() + "wait.png", pictureFromStorage);
                            using (PicturePostObject ppo = new PicturePostObject())
                            {
                                ppo.Filename = s.FileName;
                                ppo.Username = AccountToSet.UserName;
                                ppo.Password = AccountToSet.Password;
                                ppo.UseAsync = false;
                                Cursor.Current = Cursors.WaitCursor;
                                pictureService.PostPicture(ppo);
                            }
                        }
                    }
                }
            }
            else
            {
                MessageBox.Show("Uploading picture...");
            }
        }

        /// <summary>
        /// Set all the event handlers for the chosen picture service.
        /// Aparently after posting, event set are lost so these have to be set again.
        /// </summary>
        /// <param name="pictureService">Picture service on which the event handlers should be set.</param>
        private void SetPictureEventHandlers(IPictureService pictureService, bool addEvents)
        {
            if (!localPictureEventsSet && addEvents)
            {
                //No need to set finish upload event when posting to it
                if (!pictureService.CanUploadMessage || !ClientSettings.SendMessageToMediaService)
                {
                    pictureService.UploadFinish += new UploadFinishEventHandler(pictureService_UploadFinish);
                }
                pictureService.MessageReady += new MessageReadyEventHandler(pictureService_MessageReady);
                pictureService.ErrorOccured += new ErrorOccuredEventHandler(pictureService_ErrorOccured);
                localPictureEventsSet = true;
            }
            else if (localPictureEventsSet && !addEvents)
            {
                //No need to remove finish upload event when posting to it 
                if (!pictureService.CanUploadMessage || !ClientSettings.SendMessageToMediaService)
                {
                    pictureService.UploadFinish -= new UploadFinishEventHandler(pictureService_UploadFinish);
                }   
                pictureService.MessageReady -= new MessageReadyEventHandler(pictureService_MessageReady);
                pictureService.ErrorOccured -= new ErrorOccuredEventHandler(pictureService_ErrorOccured);
                localPictureEventsSet = false;
            }
        }

        private void pictureService_ErrorOccured(object sender, PictureServiceEventArgs eventArgs)
        {
            //Show the error message
            UpdatePictureData(string.Empty, false);
            
            if (uploadedPictureOrigin == "file")
            {
                AddPictureToForm(ClientSettings.IconsFolder() + "existingimage.png", pictureFromStorage);
            }
            else //camera
            {
                AddPictureToForm(ClientSettings.IconsFolder() + "takepicture.png", pictureFromCamers);
            }
            MessageBox.Show(eventArgs.ErrorMessage);
        }

        private void pictureService_MessageReady(object sender, PictureServiceEventArgs eventArgs)
        {
            //Show the message
            MessageBox.Show(eventArgs.ReturnMessage);

            if (uploadedPictureOrigin == "file")
            {
                AddPictureToForm(ClientSettings.IconsFolder() + "existingimage.png", pictureFromStorage);
            }
            else //camera
            {
                AddPictureToForm(ClientSettings.IconsFolder() + "takepicture.png", pictureFromCamers);
            }
            UpdatePictureData(string.Empty, false);

        }

        /// <summary>
        /// Event handling for when the upload is finished
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="eventArgs"></param>
        private void pictureService_UploadFinish(object sender, PictureServiceEventArgs eventArgs)
        {
            if (uploadedPictureOrigin == "file")
            {
                AddPictureToForm(eventArgs.PictureFileName, pictureFromStorage);
            }
            else //camera
            {
                AddPictureToForm(eventArgs.PictureFileName, pictureFromCamers);
            }
            UpdatePictureData(eventArgs.ReturnMessage, false);
        }

        private IPictureService GetMediaService()
        {
            IPictureService service;
            
            service = PictureServiceFactory.Instance.GetServiceByName(ClientSettings.MediaService);

            SetPictureEventHandlers(service, true);

            return service;
        }

        /// <summary>
        /// Put the picture in the form.
        /// </summary>
        /// <param name="ImageFile"></param>
        /// <param name="BoxToUpdate"></param>
        private void AddPictureToForm(string ImageFile, PictureBox BoxToUpdate)
        {
            if (InvokeRequired)
            {
                delAddPicture d = new delAddPicture(AddPictureToForm);
                this.BeginInvoke(d, ImageFile, BoxToUpdate);
            }
            else
            {
                try
                {
                    BoxToUpdate.Image = new System.Drawing.Bitmap(ImageFile);
                    if (DetectDevice.DeviceType == DeviceType.Standard && lblGPS.Visible)
                    {
                        BoxToUpdate.Left = lblGPS.Right + 5;
                    }
                    BoxToUpdate.Visible = true;
                    txtStatusUpdate_TextChanged(null, new EventArgs());
                }
                catch (OutOfMemoryException)
                {
                    BoxToUpdate.Image = PockeTwit.Themes.FormColors.GetThemeIcon("insertlink.png");
                }
            }
        }

        private void UpdatePictureData(string pictureURL, bool uploadingPicture)
        {
            if (InvokeRequired)
            {
                delUpdatePictureData d = new delUpdatePictureData(UpdatePictureData);
                this.BeginInvoke(d, pictureURL, uploadingPicture);
            }
            else
            {
                try
                {
                    Cursor.Current = Cursors.Default;
                    pictureUsed = uploadingPicture;
                    //if (DetectDevice.DeviceType == DeviceType.Standard && !string.IsNullOrEmpty(pictureURL))
                    //{
                        if (txtStatusUpdate.Text.Length > 0)
                        {
                            txtStatusUpdate.Text = txtStatusUpdate.Text + ' ' + pictureURL;
                        }
                        else
                        {
                            txtStatusUpdate.Text = txtStatusUpdate.Text + pictureURL;
                        }
                        txtStatusUpdate.SelectionStart = txtStatusUpdate.Text.Length;
                        pictureUsed = true;
                    //}

                    uploadedPictureURL = pictureURL;
                    this.uploadingPicture = uploadingPicture;
                }
                catch (OutOfMemoryException)
                {
                }
            }
        }

        private string TrimTo140(string Original)
        {
            if (Original.Length > 140)
            {
                //Truncate the text to the last available space, the add the URL.
                string URL = Yedda.ShortText.shorten(Original);
                if (string.IsNullOrEmpty(URL))
                {
                    return null;
                }
                int trimLength = 5;
                
                string NewText = Original.Substring(0, Original.LastIndexOf(" ", 140 - (URL.Length + trimLength)));
                return NewText + " ... " + URL;
            }
            return Original;
        }

        private bool PostTheUpdate()
        {
            LocationFinder.StopGPS();
            if (!string.IsNullOrEmpty(StatusText))
            {
                Cursor.Current = Cursors.WaitCursor;
                string UpdateText = TrimTo140(StatusText);

                if (string.IsNullOrEmpty(UpdateText))
                {
                    MessageBox.Show("There was an error shortening the text. Please shorten the message or try again later.");
                    return false;
                }


                if (!string.IsNullOrEmpty(picturePath) && pictureService.CanUploadMessage && ClientSettings.SendMessageToMediaService )
                {
                    PicturePostObject ppo = new PicturePostObject();
                    ppo.Filename = picturePath;
                    ppo.Username = AccountToSet.UserName;
                    ppo.Password = AccountToSet.Password;
                    ppo.Message = StatusText;

                    if (pictureService.CanUploadGPS && this.GPSLocation != null)
                    {
                        try
                        {
                            ppo.Lat = GPSLocation.Split(',')[0];
                            ppo.Lon = GPSLocation.Split(',')[1];
                        }
                        catch { }
                    }

                    return pictureService.PostPictureMessage(ppo);
                }
                else
                {


                    Yedda.Twitter TwitterConn = new Yedda.Twitter();
                    TwitterConn.AccountInfo = this.AccountToSet;

                    try
                    {
                        if (this.GPSLocation != null)
                        {
                            TwitterConn.SetLocation(this.GPSLocation);
                        }
                    }
                    catch { }


                    string retValue = TwitterConn.Update(UpdateText, in_reply_to_status_id, Yedda.Twitter.OutputFormatType.XML);

                    uploadedPictureURL = string.Empty;
                    uploadingPicture = false;

                    if (string.IsNullOrEmpty(retValue))
                    {
                        MessageBox.Show("Error posting status -- empty response.  You may want to try again later.");
                        return false;
                    }
                    try
                    {
                        Library.status.DeserializeSingle(retValue, AccountToSet);
                    }
                    catch
                    {
                        MessageBox.Show("Error posting status -- bad response.  You may want to try again later.");
                        return false;
                    }

                    return true;
                }
            }
            return true;
        }


        #endregion

        #region Events
        private void txtStatusUpdate_TextChanged(object sender, EventArgs e)
        {
            int charsAvail = 140;
            
            int lengthLeft = charsAvail - txtStatusUpdate.Text.Length;
            lblCharsLeft.Text = lengthLeft.ToString();
        }
        private void menuCancel_Click(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(this.txtStatusUpdate.Text))
            {
                if (MessageBox.Show("Are you sure you want to cancel the update?", "Cancel", MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button1) == DialogResult.No)
                {
                    return;
                }
            }

            SetPictureEventHandlers(pictureService, false);
            UpdatePictureData(string.Empty, false);

            if (_StandAlone)
            {
                this.Close();
            }
            this.DialogResult = DialogResult.Cancel;
        }
        void pictureLocation_Click(object sender, EventArgs e)
        {
            StartLocating();
        }
        void pictureURL_Click(object sender, EventArgs e)
        {
            InsertURL();
        }

        void pictureFromStorage_Click(object sender, EventArgs e)
        {
            if (String.IsNullOrEmpty(uploadedPictureURL))
            {
                pictureUsed = false;
                InsertPictureFromFile();
            }
            else
            {
                if (MessageBox.Show("Paste URL in message?", "", MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2) == DialogResult.Yes)
                {
                    txtStatusUpdate.Text += uploadedPictureURL;
                    this.txtStatusUpdate.SelectionStart = txtStatusUpdate.Text.Length;
                    pictureUsed = true;
                }
                else
                {
                    if (MessageBox.Show("Load a new picture?", "", MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2) == DialogResult.Yes)
                    {
                        uploadedPictureURL = string.Empty;
                        pictureUsed = false;
                        uploadingPicture = false;
                        InsertPictureFromFile();
                    }
                }
            }
            
        }
        void pictureFromCamers_Click(object sender, EventArgs e)
        {
            if (String.IsNullOrEmpty(uploadedPictureURL))
            {
                pictureUsed = false;
                InsertPictureFromCamera();
            }
            else
            {
                if (MessageBox.Show("Paste URL in message?", "", MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2) == DialogResult.Yes)
                {
                    txtStatusUpdate.Text += uploadedPictureURL;
                    this.txtStatusUpdate.SelectionStart = txtStatusUpdate.Text.Length;
                    pictureUsed = true;
                }
                else
                {
                    if (MessageBox.Show("Take a new picture?", "", MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2) == DialogResult.Yes)
                    {
                        uploadedPictureURL = string.Empty;
                        pictureUsed = false;
                        uploadingPicture = false;
                        InsertPictureFromCamera();
                    }
                }
            }
        }
        
        private void menuSubmit_Click(object sender, EventArgs e)
        {
            if (!pictureUsed)
            {
                if (MessageBox.Show("Uploaded picture not used, are you sure?", "PockeTwit", MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button1) == DialogResult.No)
                {
                    return;
                }
            }
            bool Success = PostTheUpdate();

            Cursor.Current = Cursors.Default;
            if (Success)
            {
                UpdatePictureData(string.Empty, false);
                SetPictureEventHandlers(pictureService, false);
                if (_StandAlone)
                {
                    this.Close();
                }
                this.DialogResult = DialogResult.OK;
            }
        }
        private void cmbAccount_SelectedIndexChanged(object sender, EventArgs e)
        {
            this.AccountToSet = (Yedda.Twitter.Account)cmbAccount.SelectedItem;
        }

        #endregion



        protected override void OnClosed(EventArgs e)
        {
            userListControl1.UnHookTextBoxKeyPress();
            base.OnClosed(e);
        }
    }
}
