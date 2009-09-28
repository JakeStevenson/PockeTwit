using System;

using System.Collections.Generic;
using System.Text;

namespace Yedda
{
    /// <summary>
    /// Oject for holding GPS information.
    /// </summary>
    public class GPSObject
    {
        //Start with basic lat/lon
        //Later on, maybe more possibilities
        //Depends on what let's say BrightKite accepts.
        private string _latitude = string.Empty;
        private string _longitude = string.Empty;
        

        /// <summary>
        /// Get or set the latitude
        /// </summary>
        public string Latitude
        {
            get
            {
                return _latitude;
            }
            set
            {
                if (string.IsNullOrEmpty(value))
                {
                    _latitude = string.Empty;
                }
                else
                {
                    _latitude = value;
                }
            }
        }

        /// <summary>
        /// Get or set the longitude
        /// </summary>
        public string Longitude
        {
            get
            {
                return _longitude;
            }
            set
            {
                if (string.IsNullOrEmpty(value))
                {
                    _longitude = string.Empty;
                }
                else
                {
                    _longitude = value;
                }
            }
        }
       
    }
}
