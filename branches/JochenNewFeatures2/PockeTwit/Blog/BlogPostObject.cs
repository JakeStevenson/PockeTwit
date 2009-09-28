using System;

using System.Collections.Generic;
using System.Text;

namespace PockeTwit.Blog
{
    public class BlogPostObject : IDisposable, ICloneable
    {
        #region private properties

        private string _username = string.Empty;
        private string _password = string.Empty;
        private string _filename = string.Empty;
        private string _message = string.Empty;
        private GPSObject _gps = null;
        private byte[] _mediaData = null;
        private BlogPostMediaType _mediaType = BlogPostMediaType.ImageJpg;

        #endregion

        #region constructor/deconstructor

        public BlogPostObject()
        {

        }

        ~BlogPostObject()
        {
            _mediaData = null;
            _gps = null;
        }

        #region IDisposable Members

        public void Dispose()
        {
            _mediaData = null;
        }

        #endregion

        #endregion

        #region getters and setters

        /// <summary>
        /// The media type of the media data.
        /// </summary>
        public BlogPostMediaType MediaType
        {
            get
            {
                return _mediaType;
            }
            set
            {
                _mediaType = value;
            }
        }

        /// <summary>
        /// Byte data from a picture
        /// </summary>
        public byte[] MediaData
        {
            get
            {
                return _mediaData;
            }
            set
            {
                _mediaData = value;
            }
        }

        /// <summary>
        /// Get or set the User name
        /// </summary>
        public string Username
        {
            get
            {
                return _username;
            }
            set
            {
                if (string.IsNullOrEmpty(value))
                {
                    _username = string.Empty;
                }
                else
                {
                    _username = value;
                }
            }
        }

        /// <summary>
        /// Get or set the Password
        /// </summary>
        public string Password
        {
            get
            {
                return _password;
            }
            set
            {
                if (string.IsNullOrEmpty(value))
                {
                    _password = string.Empty;
                }
                else
                {
                    _password = value;
                }
            }
        }

        /// <summary>
        /// Get or set the Filename
        /// </summary>
        public string Filename
        {
            get
            {
                return _filename;
            }
            set
            {
                if (string.IsNullOrEmpty(value))
                {
                    _filename = string.Empty;
                }
                else
                {
                    _filename = value;
                }
            }
        }

        /// <summary>
        /// Get or set the Message
        /// </summary>
        public string Message
        {
            get
            {
                return _message;
            }
            set
            {
                if (string.IsNullOrEmpty(value))
                {
                    _message = string.Empty;
                }
                else
                {
                    _message = value;
                }
            }
        }

        /// <summary>
        /// Get or set the GPS object
        /// </summary>
        public GPSObject Gps
        {
            get
            {
                return _gps;
            }
            set
            {
                _gps = value;
            }
        }

        #endregion

        #region ICloneable Members

        public object Clone()
        {
            //shallow clone, this won't copy objects
            BlogPostObject clone = (BlogPostObject)this.MemberwiseClone();

            //Copy the inner objects
            clone.Gps = (GPSObject) _gps.Clone();

            //space for cloning objects.

            return clone;
        }

        #endregion
    }
}
