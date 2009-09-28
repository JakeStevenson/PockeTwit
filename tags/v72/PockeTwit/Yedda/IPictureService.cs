using System;
using System.IO;
using System.Net;
using System.Xml;
using System.Web;
using System.Collections.Generic;
using System.Text;

namespace Yedda
{
    
    public class PictureServiceEventArgs : EventArgs
    {
        private PictureServiceErrorLevel _pictureServiceErrorLevel = PictureServiceErrorLevel.OK; 
        private string _returnMessage = string.Empty;
        private string _errorMessage = string.Empty;
        private string _pictureFileName = string.Empty;
        private int _totalBytesDownloaded = -1;
        private int _totalBytesToDownload = -1;
        private int _bytesDownloaded = -1;

        #region  Constructors

        /// <summary>
        /// Constructor for returning error/messaga data
        /// </summary>
        /// <param name="pictureServiceErrorLevel"></param>
        /// <param name="returnMessage"></param>
        /// <param name="errorMessage"></param>
        public PictureServiceEventArgs(PictureServiceErrorLevel pictureServiceErrorLevel, string returnMessage, string errorMessage)
        {
            _pictureServiceErrorLevel = pictureServiceErrorLevel;
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
        public PictureServiceEventArgs(PictureServiceErrorLevel pictureServiceErrorLevel, string returnMessage, string errorMessage, string pictureFileName)
        {
            _pictureServiceErrorLevel = pictureServiceErrorLevel;
            _returnMessage = returnMessage;
            _errorMessage = errorMessage;
            _pictureFileName = pictureFileName;
        }

        /// <summary>
        /// Constructor for returning download stats.
        /// </summary>
        /// <param name="bytesDownloaded"></param>
        /// <param name="totalBytesDownloaded"></param>
        /// <param name="totalBytesToDownload"></param>
        public PictureServiceEventArgs(int bytesDownloaded, int totalBytesDownloaded, int totalBytesToDownload)
        {
            _bytesDownloaded = bytesDownloaded;
            _totalBytesDownloaded = totalBytesDownloaded;
            _totalBytesToDownload = totalBytesToDownload;
        }
        #endregion

        #region getters

        public PictureServiceErrorLevel ErrorLevel
        {
            get { return _pictureServiceErrorLevel; }
        }
        public string ReturnMessage
        {
            get { return _returnMessage; }
        }
        public string ErrorMessage
        {
            get { return _errorMessage; }
        }
        public string PictureFileName
        {
            get { return _pictureFileName; }
        }
        public int TotalBytesDownloaded
        {
            get { return _totalBytesDownloaded; }
        }
        public int TotalBytesToDownload
        {
            get { return _totalBytesToDownload; }
        }
        public int BytesDownloaded
        {
            get { return _bytesDownloaded; }
        }

        #endregion
    }

    /// <summary>
    /// Errorlevel list
    /// </summary>
    public enum PictureServiceErrorLevel
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

    #region picture services delegates
    public delegate void UploadFinishEventHandler(object sender, PictureServiceEventArgs eventArgs);
    public delegate void DownloadFinishEventHandler(object sender, PictureServiceEventArgs eventArgs);
    public delegate void ErrorOccuredEventHandler(object sender, PictureServiceEventArgs eventArgs);
    public delegate void MessageReadyEventHandler(object sender, PictureServiceEventArgs eventArgs);
    public delegate void DownloadPartEventHandler(object sender, PictureServiceEventArgs eventArgs);
    #endregion


    /// <summary>
    /// Interface for multiple picture services
    /// </summary>
    public interface IPictureService
    {
        event UploadFinishEventHandler UploadFinish;
        event DownloadFinishEventHandler DownloadFinish;
        event ErrorOccuredEventHandler ErrorOccured;
        event MessageReadyEventHandler MessageReady;
        event DownloadPartEventHandler DownloadPart;

        /// <summary>
        /// Send a picture to a twitter picture framework
        /// </summary>
        /// <param name="postData">Postdata</param>
        /// <returns>Returned URL from server</returns>
        void PostPicture(PicturePostObject postData);

        /// <summary>
        /// Send a picture to a twitter picture framework without the use of the finish event
        /// </summary>
        /// <param name="postData">Postdata</param>
        /// <returns>Returned URL from server</returns>
        bool PostPictureMessage(PicturePostObject postData);

        /// <summary>
        /// Retrieve a picture from a picture service. 
        /// </summary>
        /// <param name="pictureURL">pictureURL</param>
        /// <returns>Local path for downloaded picture.</returns>
        void FetchPicture(string pictureURL);

        /// <summary>
        /// Check for possibility of getting the picture with current service.
        /// </summary>
        /// <param name="URL"></param>
        /// <returns></returns>
        bool CanFetchUrl(string URL);



        /// <summary>
        /// Methods for getting and setting properties
        /// </summary>
        bool HasEventHandlersSet { get; set; }
        bool UseDefaultFileName { set; get; }
        string DefaultFileName { set; get; }
        bool UseDefaultFilePath { set; get; }
        string DefaultFilePath { set; get; }
        string RootPath {  set; get; }
        int ReadBufferSize { set; get; }
        string ServiceName { get; }
        bool CanUpload { get; }
        bool CanUploadMessage { get; }
        bool CanUploadGPS { get; }
        int UrlLength { get;  }
    }

    /// <summary>
    /// State object for downloading in a-sync mode.
    /// </summary>
    public class AsyncStateData
    {
        internal int totalBytesToDownload = -1;
        internal int bytesRead = 0;
        internal int totalBytesRead = 0;

        internal byte[] dataHolder;
        internal string fileName;
        internal Stream dataStream;
        internal WebResponse response;
    }
}