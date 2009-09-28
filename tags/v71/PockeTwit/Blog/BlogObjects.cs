using System;

using System.Collections.Generic;
using System.Text;

namespace PockeTwit.Blog
{

    /// <summary>
    /// Oject for holding GPS information.
    /// </summary>
    public class GPSObject : ICloneable
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


        #region ICloneable Members

        public object Clone()
        {
            //shallow clone, without the object data.
            //As long as no other objects are cloned no need to cast it to a PicturePostObject
            object clone = this.MemberwiseClone();

            //space for cloning objects.

            return clone;
        }

        #endregion
    }

    /// <summary>
    /// Errorlevel list
    /// </summary>
    public enum BlogServiceErrorLevel
    {
        /// <summary>
        /// everything OK
        /// </summary>
        OK = 0,
        /// <summary>
        /// Service not ready for down/upload
        /// </summary>
        NotReady = 10,
        /// <summary>
        /// Service not available for up/download
        /// </summary>
        UnAvailable = 20,
        /// <summary>
        /// Service failed to down/upload
        /// </summary>
        Failed = 99

        //Allways room te expand.
    }

    /// <summary>
    /// Event arguments for blog services.
    /// </summary>
    public class BlogServiceEventArgs : EventArgs
    {
        private BlogServiceErrorLevel _blogServiceErrorLevel = BlogServiceErrorLevel.OK;
        private string _returnMessage = string.Empty;
        private string _errorMessage = string.Empty;
        private string _mediaFileName = string.Empty;

        #region  Constructors

        /// <summary>
        /// Constructor for returning error/messaga data
        /// </summary>
        /// <param name="pictureServiceErrorLevel"></param>
        /// <param name="returnMessage"></param>
        /// <param name="errorMessage"></param>
        public BlogServiceEventArgs(BlogServiceErrorLevel blogServiceErrorLevel, string returnMessage, string errorMessage)
        {
            _blogServiceErrorLevel = blogServiceErrorLevel;
            _returnMessage = returnMessage;
            _errorMessage = errorMessage;
        }

        /// <summary>
        /// Constructor for returning upload finish data
        /// </summary>
        /// <param name="pictureServiceErrorLevel"></param>
        /// <param name="returnMessage"></param>
        /// <param name="errorMessage"></param>
        /// <param name="pictureFileName"></param>
        public BlogServiceEventArgs(BlogServiceErrorLevel blogServiceErrorLevel, string returnMessage, string errorMessage, string mediaFileName)
        {
            _blogServiceErrorLevel = blogServiceErrorLevel;
            _returnMessage = returnMessage;
            _errorMessage = errorMessage;
            _mediaFileName = mediaFileName;
        }

        #endregion

        #region getters

        public BlogServiceErrorLevel ErrorLevel
        {
            get { return _blogServiceErrorLevel; }
        }
        public string ReturnMessage
        {
            get { return _returnMessage; }
        }
        public string ErrorMessage
        {
            get { return _errorMessage; }
        }
        public string MediaFileName
        {
            get { return _mediaFileName; }
        }


        #endregion
    }

    /// <summary>
    /// Types for timelines to be fetched.
    /// </summary>
    public enum BlogTimeLineFetchType
    {
        /// <summary>
        /// Timeline for followed friend
        /// </summary>
        Friends = 0,

        /// <summary>
        /// Timeline for messages directed to you.
        /// </summary>
        Messages = 10,

        /// <summary>
        /// Timeline for favorite messages.
        /// </summary>
        Favorites = 20,

        /// <summary>
        /// The public timeline.
        /// </summary>
        Public = 30

        //Maybe more timelines?
    }

    /// <summary>
    /// Mediatypes for sending with blog postings.
    /// </summary>
    public enum BlogPostMediaType
    {
        /// <summary>
        /// MediaType Image, most commen as jpg.
        /// </summary>
        ImageJpg = 0,
        
        /// <summary>
        /// Mediatype movie
        /// </summary>
        Movie = 10,

        /// <summary>
        /// Mediatype audio, formatted as MP3
        /// </summary>
        AudioClipMP3 = 20,

        /// <summary>
        /// Mediatype audio, formatted as wave
        /// </summary>
        AudioClipWav = 30

        //Add other movietypes
    }

    /// <summary>
    /// Content types for form post parts
    /// </summary>
    public struct MediaIdentifier
    {
        /// <summary>
        /// Content type for jpg.
        /// </summary>
        public const string JPG = "image/jpeg";
    }
}
