using System;

using System.Collections.Generic;
using System.Text;

namespace PockeTwit
{
    class LocationManager
    {
        //Ummm, just in case :)
        //http://maps.google.com/staticmap?center=37.393891,-122.066517&markers=37.400465,-122.073003,red&path=rgba:0x0000FF80,weight:5|37.40489,-122.05261&zoom=13&size=500x300&key=ABQIAAAA-6_yG-9k0X8KgfnjWXnHpBRtiFeQlxP7WphYAvKAZsrpIYWXFxQBOiC3o3mV0Wc7L8i2JnuMvxLdRQ
        //private const GoogleAPIKey = "ABQIAAAA-6_yG-9k0X8KgfnjWXnHpBRtiFeQlxP7WphYAvKAZsrpIYWXFxQBOiC3o3mV0Wc7L8i2JnuMvxLdRQ";

        public delegate void delLocationReady(string Location);
        public event delLocationReady LocationReady;
        private GPS.GpsPosition position = null;
        private GPS.Gps gps = new PockeTwit.GPS.Gps();

        public void GetGPS()
        {
            if (ClientSettings.UseGPS)
            {
                StartGPS();
            }
        }
        public void StartPosition()
        {
            if (ClientSettings.UseGPS)
                StartGPS();
            else if (ClientSettings.UseCellIDPosition)
                GetCellIDPosition();
        }
        public void StopPosition()
        {
            StopGPS();
        }
        public void SwitchToCellID()
        {
            StopGPS();
            if (!GetCellIDPosition())
            {
                // Fallback, continue to try GPS locating
                if (ClientSettings.UseGPS)
                    StartGPS();
            }
        }
        private bool GetCellIDPosition()
        {
            CellTowerInformation cell = RILWrapper.GetCellTowerInfo();
            if ((cell.LAC != 0) && (cell.CellID != 0))
            {
                GeoPosition position = GoogleMapCellID.GetLatLng(cell);
                if (position != null)
                {
                    if (!Double.IsNaN(position.Longitude) && !Double.IsNaN(position.Latitude))
                    {
                        if (LocationReady != null)
                        {
                            IFormatProvider format = new System.Globalization.CultureInfo(1033);
                            LocationReady(position.Latitude.ToString(format) + "," + position.Longitude.ToString(format));
                            return true;
                        }
                    }
                }
            }
            return false;
        }
        private void StartGPS()
        {
            gps.LocationChanged += new PockeTwit.GPS.LocationChangedEventHandler(gps_LocationChanged);
            if (!gps.Opened)
            {
                gps.Open();
            }
        }
        private void StopGPS()
        {
            gps.LocationChanged -= new PockeTwit.GPS.LocationChangedEventHandler(gps_LocationChanged);
            if (gps.Opened)
            {
                gps.Close();
            }
        }

        void gps_LocationChanged(object sender, PockeTwit.GPS.LocationChangedEventArgs args)
        {
            IFormatProvider format = new System.Globalization.CultureInfo(1033);
            if (gps.Opened)
            {
                try
                {
                    if (args.Position == null) { return; }
                    if (args.Position.LatitudeValid && args.Position.LongitudeValid)
                    {
                        if (!Double.IsNaN(args.Position.Longitude) && !Double.IsNaN(args.Position.Latitude))
                        {
                            position = args.Position;
                            if (LocationReady != null)
                            {
                                LocationReady(position.Latitude.ToString(format) + "," + position.Longitude.ToString(format));
                            }
                        }
                    }
                }
                catch (DivideByZeroException)
                {
                }

            }

        }
    }
}
