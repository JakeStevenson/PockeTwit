using System;

using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using PockeTwit.OtherServices;
using TiledMaps;
using System.Reflection;
using System.Collections;
using Geocode=TiledMaps.Geocode;

namespace PockeTwit
{
    public partial class ProfileMap : Form
    {
        private int startCount = 0;
        
        private List<Library.User> _Users = new List<Library.User>();
        public List<Library.User> Users
        {
            get { return _Users; }
            set
            {
                _Users = value;
                SetMarkers();
                RefreshBitmap();
            }
        }

        Bitmap myBitmap;
        GraphicsRenderer myRenderer = new GraphicsRenderer();
        GoogleMapSession mySession = new GoogleMapSession();

        public Geocode CenterLocation { get; set; }
        public double Range { get; set; }
       

        public ProfileMap()
        {
            InitializeComponent();
            //marker = myRenderer.LoadBitmap(Assembly.GetExecutingAssembly().GetManifestResourceStream("PockeTwit.Marker.png"));
            PockeTwit.Themes.FormColors.SetColors(this);
            PockeTwit.Localization.XmlBasedResourceManager.LocalizeForm(this);
            if (ClientSettings.IsMaximized)
            {
                this.WindowState = FormWindowState.Maximized;
            }
            this.myPictureBox.Resize += new EventHandler(pictureBox1_Resize);
            if (DetectDevice.DeviceType == DeviceType.Professional)
            {
                myPictureBox.MouseDown += new MouseEventHandler(myPictureBox_MouseDown);
                myPictureBox.MouseMove += new MouseEventHandler(myPictureBox_MouseMove);
                myPictureBox.MouseUp += new MouseEventHandler(myPictureBox_MouseUp);
            }
            RefreshBitmap();
            Cursor.Current = Cursors.Default;
        }

        void myPictureBox_MouseUp(object sender, MouseEventArgs e)
        {
            Point p = new Point(e.X, e.Y);
            foreach (MapOverlay o in mySession.Overlays)
            {
                userMapDrawable marker = (userMapDrawable)o.Drawable;
                if (marker.Location.Contains(p))
                {
                    marker.IsOpened = !marker.IsOpened;
                    o.Offset = new Point(0, -marker.Height / 2);
                }
            }
            RefreshBitmap();
        }

        private void SetMarkers()
        {
            float fSize = 9;
            List<string> seenLocs = new List<string>();
            List<Geocode> codes = new List<Geocode>();
            SizeF currentScreen = this.CurrentAutoScaleDimensions;
            if (currentScreen.Height == 192)
            {
                fSize = 4;
            }
            foreach (Library.User user in _Users)
            {
                string location = user.location;
                if (!seenLocs.Contains(location))
                {
                    seenLocs.Add(location);
                    Coordinate c;
                    if (!Coordinate.tryParse(location, out c))
                    {
                        c = OtherServices.Geocode.GetCoordinates(location);
                    }
                    if (c.Latitude != 0 && c.Longitude != 0)
                    {
                        userMapDrawable marker = new userMapDrawable();
                        marker.userToDraw = user;
                        marker.fSize = fSize;
                        Geocode g = new Geocode((double)c.Latitude, (double)c.Longitude);
                        MapOverlay o = new MapOverlay(marker, g, new Point(0, -marker.Height / 2));
                        codes.Add(g);
                        mySession.Overlays.Add(o);
                    }
                }
            }
            mySession.FitPOIToDimensions(myPictureBox.Width, myPictureBox.Height, 8, codes.ToArray());
        }

        Point myLastPos = Point.Empty;
        private void myPictureBox_MouseMove(object sender, MouseEventArgs e)
        {
            mySession.Pan(MousePosition.X - myLastPos.X, MousePosition.Y - myLastPos.Y);
            myLastPos = MousePosition;
            RefreshBitmap();
        }

        private void myPictureBox_MouseDown(object sender, MouseEventArgs e)
        {
            myLastPos = MousePosition;
        }

        void pictureBox1_Resize(object sender, EventArgs e)
        {
            RefreshBitmap();
        }



        void RefreshBitmap()
        {
            // clear out tiles that haven't been used in 10 seconds, just to keep from running out of memory.
            mySession.ClearAgedTiles(10000);

            if (myBitmap == null || myBitmap.Width != myPictureBox.ClientSize.Width || myBitmap.Height != myPictureBox.ClientSize.Height)
            {
                myBitmap = new Bitmap(myPictureBox.ClientSize.Width, myPictureBox.ClientSize.Height, System.Drawing.Imaging.PixelFormat.Format16bppRgb565);
                myRenderer.Graphics = Graphics.FromImage(myBitmap);
                myPictureBox.Image = myBitmap;
            }

            List<IMapOverlay> visitems= mySession.VisibleItems(myPictureBox.ClientSize.Width, myPictureBox.ClientSize.Height);
            if (visitems.Count < 10)
            {
                for (int i = 0; i < visitems.Count;i++)
                {
                    userMapDrawable marker = (userMapDrawable)visitems[i].Drawable;
                    marker.charToUse = i;
                }
            }

            mySession.DrawMap(myRenderer, 0, 0, myBitmap.Width, myBitmap.Height, (o) =>
            {
                Invoke(new EventHandler((sender, args) =>
                {
                    RefreshBitmap();
                }));
            }, null);
            using (Brush b = new SolidBrush(Color.Black))
            {   
                float pos = (float)this.Height - ClientSettings.TextSize;
                myRenderer.Graphics.DrawString("Maps by Google", this.Font, b, 0, pos);
            }
            myPictureBox.Refresh();
        }

        
        protected override void OnKeyDown(KeyEventArgs e)
        {
            switch(e.KeyCode)
            {
                case (Keys.LButton | Keys.MButton | Keys.Back):
                    if (mySession.CanZoomIn)
                    {
                        mySession.ZoomIn();
                        RefreshBitmap();
                    }
                    break;
                case Keys.Right:
                    mySession.Pan(0 - ClientSettings.TextSize, 0);
                    RefreshBitmap();
                    break;
                case Keys.Left:
                    mySession.Pan(ClientSettings.TextSize, 0);
                    RefreshBitmap();
                    break;
                case Keys.Up:
                    mySession.Pan(0, ClientSettings.TextSize);
                    RefreshBitmap();
                    break;
                case Keys.Down:
                    mySession.Pan(0, 0 - ClientSettings.TextSize);
                    RefreshBitmap();
                    break;
                default:
                    foreach (MapOverlay o in mySession.Overlays)
                    {
                        userMapDrawable marker = (userMapDrawable)o.Drawable;
                        if(marker.charToUse == (e.KeyValue-48))
                        {
                            marker.IsOpened = !marker.IsOpened;
                            o.Offset = new Point(0, -marker.Height / 2);
                            RefreshBitmap();
                        }
                    }
                    break;
            }
        }
        private void menuItem1_Click(object sender, EventArgs e)
        {
            this.myPictureBox.Resize -= new EventHandler(pictureBox1_Resize);
            if (this.myPictureBox.Image != null)
            {
                this.myPictureBox.Image.Dispose();
            }
            this.Close();
        }

        
        private void menuItem4_Click(object sender, EventArgs e)
        {
            if (mySession.CanZoomOut)
            {
                mySession.ZoomOut();
                RefreshBitmap();
            }
        }

        private void menuNext_Click(object sender, EventArgs e)
        {
            if (startCount < Users.Count)
            {
                startCount = startCount + 5;
                SetMarkers();
                RefreshBitmap();
            }
        }

        private void menuZoomIn_Click(object sender, EventArgs e)
        {
            if (mySession.CanZoomIn)
            {
                mySession.ZoomIn();
                RefreshBitmap();
            }
        }
        private void menuItem4_Click_1(object sender, EventArgs e)
        {
            SearchNearHere();
        }

        
        private void SearchNearHere()
        {
            Point OutsidePoint =  new Point(0, this.Height / 2);
            Point CenterPoint = new Point(this.Width / 2, this.Height / 2);
            Geocode g = mySession.CenterRelativePointToGeocode(OutsidePoint);
            Geocode c = mySession.CenterRelativePointToGeocode(CenterPoint);
            double dist = distance(g.Latitude, g.Longitude, c.Latitude, c.Longitude, 'K');
            System.Diagnostics.Debug.WriteLine(dist);
            this.Range = dist;
            this.CenterLocation = c;
            this.Close();
        }


        #region distancestuff
        private double distance(double lat1, double lon1, double lat2, double lon2, char unit)
        {
            double theta = lon1 - lon2;
            double dist = Math.Sin(deg2rad(lat1)) * Math.Sin(deg2rad(lat2)) + Math.Cos(deg2rad(lat1)) * Math.Cos(deg2rad(lat2)) * Math.Cos(deg2rad(theta));
            dist = Math.Acos(dist);
            dist = rad2deg(dist);
            dist = dist * 60 * 1.1515;
            if (unit == 'K')
            {
                dist = dist * 1.609344;
            }
            else if (unit == 'N')
            {
                dist = dist * 0.8684;
            }
            return (dist);
        }

        //:::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::
        //::  This function converts decimal degrees to radians             :::
        //:::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::
        private double deg2rad(double deg)
        {
            return (deg * Math.PI / 180.0);
        }

        //:::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::
        //::  This function converts radians to decimal degrees             :::
        //:::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::
        private double rad2deg(double rad)
        {
            return (rad / Math.PI * 180.0);
        }

        #endregion

        bool _focusing = false;
        private void menuItem5_Click(object sender, EventArgs e)
        {
            pictureBoxJump.Visible = true;
            lblJump.Visible = true;
            txtJump.Visible = true;
            _focusing = true;
            txtJump.Focus();
        }

        private void txtJump_LostFocus(object sender, EventArgs e)
        {
            
        }

        private void txtJump_KeyUp(object sender, KeyEventArgs e)
        {
            if (_focusing) { _focusing = false; return; }
            if (e.KeyCode == Keys.Enter)
            {
                lblJump.Visible = false;
                txtJump.Visible = false;
                pictureBoxJump.Visible = false;
                myPictureBox.Focus();
                this.Focus();
                Coordinate c = OtherServices.Geocode.GetCoordinates(txtJump.Text);
                mySession.FitPOIToDimensions(myPictureBox.Width, myPictureBox.Height, 12, new Geocode[] { new Geocode((double)c.Latitude, (double)c.Longitude) });
                RefreshBitmap();
            }
        }

    }
}