using System;
using System.IO;
using System.Drawing;
using System.Collections.Generic;
using System.Text;
using System.Web;
using System.Net;
using PockeTwit;
using PockeTwit.OtherServices;

namespace PockeTwit.OtherServices
{
    public interface ISpatialCoordinate
    {
        decimal Latitude { get; set; }
        decimal Longitude { get; set; }
    }

    /// <summary>
    /// Coordiate structure. Holds Latitude and Longitude.
    /// </summary>
    public struct Coordinate : ISpatialCoordinate
    {
        private decimal _latitude;
        private decimal _longitude;

        public Coordinate(decimal latitude, decimal longitude)
        {
            _latitude = latitude;
            _longitude = longitude;
        }

        #region ISpatialCoordinate Members

        public decimal Latitude
        {
            get
            {
                return _latitude;
            }
            set
            {
                this._latitude = value;
            }
        }

        public decimal Longitude
        {
            get
            {
                return _longitude;
            }
            set
            {
                this._longitude = value;
            }
        }

        public static bool tryParse(string original, out Coordinate Parsed)
        {
            IFormatProvider format = new System.Globalization.CultureInfo(1033);
            try
            {
                string[] set = original.Split(',');
                decimal lat = decimal.Parse(set[0], format);
                decimal longi = decimal.Parse(set[1], format);
                Parsed = new Coordinate(lat, longi);
                return true;
            }
            catch
            {
            }
            Parsed = new Coordinate();
            return false;
        }

        public override string ToString()
        {
            return _latitude + "," + _longitude;
        }
        #endregion
    }

    public class Geocode
    {
        private const string _googleUri = "http://maps.google.com/maps/geo?q=";
        private const string _googleKey = "ABQIAAAAavzfoRRgKKZOdkiw4CKkpxRtiFeQlxP7WphYAvKAZsrpIYWXFxQO2taB7kukKt1wbl4XqdE1oSb8yg";
        private const string _outputType = "csv"; // Available options: csv, xml, kml, json

        private static Uri GetGeocodeUri(string address)
        {
            address = HttpUtility.UrlEncode(address);
            return new Uri(String.Format("{0}{1}&output={2}&key={3}", _googleUri, address, _outputType, _googleKey));
        }

        /// <summary>
        /// Gets a Coordinate from a address.
        /// </summary>
        /// <param name="address">An address.
        /// <remarks>
        /// <example>1600 Amphitheatre Parkway Mountain View, CA 94043</example>
        /// </remarks>
        /// </param>
        /// <returns>A spatial coordinate that contains the latitude and longitude of the address.</returns>
        public static Coordinate GetCoordinates(string address)
        {
            Uri uri = GetGeocodeUri(address);
            System.Net.HttpWebRequest client = WebRequestFactory.CreateHttpRequest(uri);

            IFormatProvider format = new System.Globalization.CultureInfo(1033);

            /* The first number is the status code, 
                * the second is the accuracy, 
                * the third is the latitude, 
                * the fourth one is the longitude.
                */
                
            try
            {
                using (HttpWebResponse httpResponse = (HttpWebResponse)client.GetResponse())
                {
                    using (Stream stream = httpResponse.GetResponseStream())
                    {
                        using (StreamReader reader = new StreamReader(stream))
                        {
                            string response = reader.ReadToEnd();
                            string[] geocodeInfo = response.Split(',');

                            return new Coordinate(Convert.ToDecimal(geocodeInfo[2],format), Convert.ToDecimal(geocodeInfo[3],format));
                        }
                    }
                }
            }
            catch (WebException ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.Message);
            }
            return new Coordinate();   
        }
        public static string GetAddress(string CoordinateString)
        {
            Uri uri = GetGeocodeUri(CoordinateString);
            System.Net.HttpWebRequest client = WebRequestFactory.CreateHttpRequest(uri);



            /* The first number is the status code, 
                * the second is the accuracy, 
                * the third is the latitude, 
                * the fourth one is the longitude.
                */

            try
            {
                using (HttpWebResponse httpResponse = (HttpWebResponse)client.GetResponse())
                {
                    using (Stream stream = httpResponse.GetResponseStream())
                    {
                        using (StreamReader reader = new StreamReader(stream))
                        {
                            string response = reader.ReadToEnd();
                            string[] geocodeInfo = response.Split(',');
                            return geocodeInfo[2].TrimStart('"') + ",\r\n" + geocodeInfo[3].Trim(' ') + ", " + geocodeInfo[4].Trim(' ').TrimEnd('"');
                        }
                    }
                }
            }
            catch 
            {
            }
            return "";
        }
    }
}

namespace Yedda
{
    //API KEY ABQIAAAAavzfoRRgKKZOdkiw4CKkpxRtiFeQlxP7WphYAvKAZsrpIYWXFxQO2taB7kukKt1wbl4XqdE1oSb8yg
    class GoogleMaps
    {
        private const string staticMapURL = "http://maps.google.com/staticmap?center={0}&zoom={3}&size={1}x{2}&maptype=mobile&markers={0},red&key=ABQIAAAAavzfoRRgKKZOdkiw4CKkpxRtiFeQlxP7WphYAvKAZsrpIYWXFxQO2taB7kukKt1wbl4XqdE1oSb8yg&sensor=false";


        public static string GetGeoCode(string originalLocation)
        {
            return originalLocation;
        }
        public static System.Drawing.Bitmap GetMultiMap(string[] locations, int Zoom, int Height, int Width)
        {
            for(int i=0;i<locations.Length;i++)
            {
                Coordinate c;
                bool GetCoords = Coordinate.tryParse(locations[i], out c);
                if (!GetCoords)
                {
                    c = Geocode.GetCoordinates(locations[i]);
                    locations[i] = c.ToString();
                }   
            }
            string markers = string.Join("|", locations);
            string URL = string.Format(staticMapURL, markers, Width, Height, Zoom);
            HttpWebRequest client = WebRequestFactory.CreateHttpRequest(URL);
            using (HttpWebResponse httpResponse = (HttpWebResponse)client.GetResponse())
            {
                try
                {
                    return new Bitmap(httpResponse.GetResponseStream());
                }
                catch
                {
                }
            }
            return null;
        }
        public static System.Drawing.Bitmap GetMap(string location, int Zoom, int Height, int Width)
        {
            return GetMultiMap(new string[] { location }, Zoom, Height, Width);
        }
    }
}

