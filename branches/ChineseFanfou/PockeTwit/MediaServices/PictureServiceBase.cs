using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Text.RegularExpressions;

namespace PockeTwit.MediaServices
{
    public abstract class PictureServiceBase: IPictureService
    {
        #region private variables

        /// <summary>
        /// Settings for outside.
        /// </summary>
        protected string PT_DEFAULT_FILENAME = string.Empty;
        protected int PT_READ_BUFFER_SIZE = 512;
        protected bool PT_USE_DEFAULT_FILENAME = true;
        protected bool PT_USE_DEFAULT_PATH = true;
        protected string PT_DEFAULT_PATH = string.Empty;
        protected string PT_ROOT_PATH = string.Empty;
       
        protected string API_SAVE_TO_PATH { get; set; }
        protected string API_SERVICE_NAME = string.Empty;
        protected bool API_CAN_UPLOAD = true;
        protected bool API_CAN_UPLOAD_MESSAGE = false;
        protected bool API_CAN_UPLOAD_GPS = false;
        protected bool API_CAN_UPLOAD_MOREMEDIA = false;
        protected int API_URLLENGTH = 0;

        protected List<MediaType> API_FILETYPES = new List<MediaType>();

        #endregion

        #region IPictureService Members

        /// <summary>
        /// PostPicture method that must be overridden.
        /// </summary>
        /// <param name="postData">Data to post</param>
        public abstract void PostPicture(PicturePostObject postData);

        /// <summary>
        /// FetchPicture method that must be overridden.
        /// </summary>
        /// <param name="pictureURL">URL to fetch</param>
        public abstract void FetchPicture(string pictureURL);

        /// <summary>
        /// Send a picture to a twitter picture framework without the use of the finish event
        /// </summary>
        /// <param name="postData">Postdata</param>
        /// <returns>Returned URL from server</returns>
        public abstract bool PostPictureMessage(PicturePostObject postData);


        #region getters and setters

        /// <summary>
        /// Must be implemented to set fetching yes/no
        /// </summary>
        /// <param name="URL"></param>
        /// <returns></returns>
        public abstract bool CanFetchUrl(string URL);

        public bool HasEventHandlersSet { get; set; }

        public bool UseDefaultFileName
        {
            set
            {
                PT_USE_DEFAULT_FILENAME = value;
            }
            get
            {
                return PT_USE_DEFAULT_FILENAME;
            }
        }

        public string DefaultFileName
        {
            set
            {
                PT_DEFAULT_FILENAME = value;
            }
            get
            {
                return PT_DEFAULT_FILENAME;
            }
        }

        public bool UseDefaultFilePath
        {
            set
            {
                PT_USE_DEFAULT_PATH = value;
            }
            get
            {
                return PT_USE_DEFAULT_PATH;
            }
        }

        public string DefaultFilePath
        {
            set
            {
                PT_DEFAULT_PATH = value;
            }
            get
            {
                return PT_DEFAULT_PATH;
            }
        }

        public string RootPath
        {
            set
            {
                PT_ROOT_PATH = value;
            }
            get
            {
                return PT_ROOT_PATH;
            }
        }

        public int ReadBufferSize
        {
            set
            {
                PT_READ_BUFFER_SIZE = value;
            }
            get
            {
                return PT_READ_BUFFER_SIZE;
            }
        }

        public string ServiceName
        {
            get
            {
                return API_SERVICE_NAME;
            }
        }

        public bool CanUpload
        {
            get
            {
                return API_CAN_UPLOAD;
            }
        }

        /// <summary>
        /// Whether the service can upload a message
        /// </summary>
        public bool CanUploadMessage
        {
            get
            {
                return API_CAN_UPLOAD_MESSAGE;
            }
        }

        /// <summary>
        /// Whether the service can upload a gps position.
        /// </summary>
        public bool CanUploadGPS
        {
            get
            {
                return API_CAN_UPLOAD_GPS;
            }
        }

        public bool CanUploadOtherMedia
        {
            get
            {
                return API_CAN_UPLOAD_MOREMEDIA;
            }
        }

        public int UrlLength
        {
            get
            {
                return API_URLLENGTH;
            }
        }

        public string FileFilter(MediaTypeGroup mediaGroup)
        {

            bool first = true;
            string filterFormat = "{0} files (*.{0})|*.{0}";
            StringBuilder sb = new StringBuilder();
            foreach (MediaType type in API_FILETYPES)
            {
                if (type.MediaGroup != mediaGroup && mediaGroup != MediaTypeGroup.ALL)
                {
                    continue;
                }
                
                if (first)
                {
                   
                    first = false;
                }
                else
                {
                    sb.Append("|");
                }
                sb.Append(string.Format(filterFormat, type.Extension));

            }
            return sb.ToString();
            
        }

        public List<MediaType> FileTypes 
        {
            get
            {
                return API_FILETYPES;
            }
        }

        #endregion



        #endregion

        #region private methods

        /// <summary>
        /// Lookup the path and filename intended for the image. When it does not exist, create it.
        /// </summary>
        /// <param name="imageId">Image ID</param>
        /// <returns>Path to save the picture in.</returns>
        protected string GetPicturePath(string pictureURL)
        {
            #region argument check

            if (string.IsNullOrEmpty(pictureURL))
            {

            }

            #endregion

            String picturePath = String.Empty;

            int imageIdStartIndex = pictureURL.LastIndexOf('?') + 1;
            string imageId = pictureURL.Substring(imageIdStartIndex, pictureURL.Length - imageIdStartIndex);

            string rootpath = string.Empty;
            if (PT_USE_DEFAULT_PATH)
            {
                rootpath = Path.Combine(PT_ROOT_PATH, PT_DEFAULT_PATH);
            }
            else
            {
                rootpath = Path.Combine(PT_ROOT_PATH, API_SAVE_TO_PATH);
            }
            if (!Directory.Exists(rootpath))
            {
                Directory.CreateDirectory(rootpath);
            }

            if (PT_USE_DEFAULT_FILENAME)
            {
                picturePath = rootpath + "\\" + PT_DEFAULT_FILENAME;
                if (File.Exists(picturePath))
                {
                    File.Delete(picturePath);
                }
            }
            else
            {
                string firstChar = imageId.Substring(0, 1);
                picturePath = Path.Combine(rootpath, firstChar);
                if (!System.IO.Directory.Exists(picturePath))
                {
                    System.IO.Directory.CreateDirectory(picturePath);
                }
                picturePath = picturePath + "\\" + imageId + ".jpg";
            }
            return picturePath;
        }

        /// <summary>
        /// Save the picture data to disk.
        /// </summary>
        /// <param name="picturePath">Path to save picture to, with filename</param>
        /// <param name="pictureData">Data to save</param>
        /// <param name="bufferSize">Length of data in buffer.</param>
        /// <returns>Succes or failure</returns>
        protected bool SavePicture(String picturePath, byte[] pictureData, int bufferSize)
        {
            #region argument check
            if (String.IsNullOrEmpty(picturePath))
            {
                return false;
            }

            if (pictureData == null)
            {
                return false;
            }
            if (pictureData.Length == 0)
            {
                return false;
            }
            #endregion

            try
            {
                if (!File.Exists(picturePath))
                {
                    using (FileStream pictureFile = File.Create(picturePath))
                    {
                        pictureFile.Write(pictureData, 0, bufferSize);
                        pictureFile.Close();
                    }
                }
                else
                {
                    using (FileStream pictureFile = File.Open(picturePath, FileMode.Append, FileAccess.Write))
                    {
                        pictureFile.Write(pictureData, 0, bufferSize);
                        pictureFile.Close();
                    }
                }
                return true;
            }
            catch
            {
                return false;
            }
        }

        #endregion

        #region events

        public event UploadFinishEventHandler UploadFinish;
        public event DownloadFinishEventHandler DownloadFinish;
        public event ErrorOccuredEventHandler ErrorOccured;
        public event MessageReadyEventHandler MessageReady;
        public event DownloadPartEventHandler DownloadPart;

        protected virtual void OnDownloadFinish(PictureServiceEventArgs e)
        {
            if (DownloadFinish != null)
            {
                try
                {
                    DownloadFinish(this, e);
                }
                catch (Exception)
                {
                    //Always continue after a missed event
                }
            }
        }

        protected virtual void OnUploadFinish(PictureServiceEventArgs e)
        {
            if (UploadFinish != null)
            {
                try
                {
                    UploadFinish(this, e);
                }
                catch (Exception)
                {
                    //Always continue after a missed event
                }
            }
        }

        protected virtual void OnErrorOccured(PictureServiceEventArgs e)
        {
            if (ErrorOccured != null)
            {
                try
                {
                    ErrorOccured(this, e);
                }
                catch (Exception)
                {
                    //Always continue after a missed event
                }
            }
        }

        protected virtual void OnMessageReady(PictureServiceEventArgs e)
        {
            if (MessageReady != null)
            {
                try
                {
                    MessageReady(this, e);
                }
                catch (Exception)
                {
                    //Always continue after a missed event
                }
            }
        }

        protected virtual void OnDownloadPart(PictureServiceEventArgs e)
        {
            if (DownloadPart != null)
            {
                try
                {
                    DownloadPart(this, e);
                }
                catch (Exception)
                {
                    //Always continue after a missed event
                }
            }
        }


        #endregion

        #region helper methods

        /// <summary>
        /// Find all hashtags in a message and return them in a tagstring.
        /// </summary>
        /// <param name="message">Full message</param>
        /// <param name="seperator">Seperator between tags</param>
        /// <returns>String filled with tags seperated by seperator.</returns>
        protected string FindHashTags(string message, string seperator, int maxLength)
        {
            Regex hashTags = new Regex(@"([#][\w_]+)");
            MatchCollection hashTagsFound = hashTags.Matches(message);
            string result = string.Empty;
            bool first = true;

            foreach (Match match in hashTagsFound)
            {
                if (first)
                {
                    result = match.ToString();
                    first = false;
                }
                else
                {
                    if ((result.Length + seperator.Length + match.Length) > maxLength)
                    {
                        return result;
                    }
                    result += seperator;
                    result += match.ToString();
                }
            }
            return result;
        }

        protected bool CanUploadFileType(string filename)
        {
            int extensionStartIndex = filename.LastIndexOf('.') + 1;
            string fileExtension = filename.Substring(extensionStartIndex, filename.Length - extensionStartIndex);

            foreach (MediaType mediatype in API_FILETYPES)
            {
                if (fileExtension.ToUpper().CompareTo(mediatype.Extension.ToUpper()) == 0)
                {
                    return true;
                }
            }
            return false;
        }

      

        #endregion
    }


}
