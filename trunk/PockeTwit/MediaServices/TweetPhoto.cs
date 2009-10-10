using System;
using System.IO;
using System.Net;
using System.Xml;
using System.Text;

namespace PockeTwit.MediaServices
{
    public class TweetPhoto : PictureServiceBase
    {
        #region private properties

        private static volatile TweetPhoto _instance;
        private static readonly object SyncRoot = new Object();

        private const string API_UPLOAD = "http://tweetphotoapi.com/api/tpapi.svc/upload2";
        private const string API_UPLOAD_POST = "http://tweetphotoapi.com/api/tpapi.svc/upload2";
        
        private const string API_SHOW_FORMAT = "http://www.tweetphoto.com/show/medium/{0}";  //The extra / for directly sticking the image-id on.

        private const string API_ERROR_UPLOAD = "Unable to upload to TweetPhoto";
        private const string API_ERROR_DOWNLOAD = "Unable to download from TweetPhoto";

        private const string API_KEY = "cd6fa2df805addb613d06a91f24bdf01";

        #endregion

        #region private objects

        private System.Threading.Thread _workerThread;
        private PicturePostObject _workerPpo;

        #endregion

        #region constructor
        /// <summary>
        /// Private constructor for usage in singleton.
        /// </summary>
        private TweetPhoto()
        {
            API_SAVE_TO_PATH = "\\ArtCache\\www.tweetphoto.com\\";
            API_SERVICE_NAME = "TweetPhoto";
            API_CAN_UPLOAD_GPS = true;
            API_CAN_UPLOAD_MESSAGE = true;
            API_CAN_UPLOAD = true;

            API_URLLENGTH = 28;

            API_FILETYPES.Add(new MediaType("jpg", "image/jpeg", MediaTypeGroup.PICTURE));
            API_FILETYPES.Add(new MediaType("jpeg", "image/jpeg", MediaTypeGroup.PICTURE));
            API_FILETYPES.Add(new MediaType("gif", "image/gif", MediaTypeGroup.PICTURE));
            API_FILETYPES.Add(new MediaType("png", "image/png", MediaTypeGroup.PICTURE));

        }

        /// <summary>
        /// Singleton constructor
        /// </summary>
        /// <returns></returns>
        public static TweetPhoto Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (SyncRoot)
                    {
                        if (_instance == null)
                        {
                            _instance = new TweetPhoto {HasEventHandlersSet = false};
                        }
                    }
                }
                return _instance;
            }
        }


        #endregion

        #region IPictureService Members

        /// <summary>
        /// Post a picture
        /// </summary>
        /// <param name="postData"></param>
        public override void PostPicture(PicturePostObject postData)
        {
            #region Argument check

            //Check for empty path
            if (string.IsNullOrEmpty(postData.Filename))
            {
                OnErrorOccured(new PictureServiceEventArgs(PictureServiceErrorLevel.Failed, string.Empty, API_ERROR_UPLOAD));
            }

            //Check for empty credentials
            if (string.IsNullOrEmpty(postData.Username) ||
                string.IsNullOrEmpty(postData.Password))
            {
                OnErrorOccured(new PictureServiceEventArgs(PictureServiceErrorLevel.Failed, string.Empty, API_ERROR_UPLOAD));
            }

            #endregion

            using (var file = new FileStream(postData.Filename, FileMode.Open, FileAccess.Read))
            {
                try
                {
                    //Load the picture data
                    var incoming = new byte[file.Length];
                    file.Read(incoming, 0, incoming.Length);

                    if (postData.UseAsync)
                    {
                        _workerPpo = (PicturePostObject)postData.Clone();
                        _workerPpo.PictureData = incoming;

                        if (_workerThread == null)
                        {
                            _workerThread = new System.Threading.Thread(ProcessUpload){Name = "PictureUpload"};
                            _workerThread.Start();
                        }
                        else
                        {
                            OnErrorOccured(new PictureServiceEventArgs(PictureServiceErrorLevel.NotReady, string.Empty, "A request is already running."));
                        }
                    }
                    else
                    {
                        //use sync.
                        postData.PictureData = incoming;
                        XmlDocument uploadResult = UploadPictureMessage(API_UPLOAD, postData);

                        if (uploadResult == null)
                        {
                            return;
                        }
                        var nm = new XmlNamespaceManager(uploadResult.NameTable);
                        nm.AddNamespace("tweetPhoto", "http://tweetphotoapi.com");
                        if (uploadResult.SelectSingleNode("//tweetPhoto:Status",nm).InnerText!= "OK")
                        {
                            string errorText = uploadResult.SelectSingleNode("//err").Attributes["msg"].Value;
                            OnErrorOccured(new PictureServiceEventArgs(PictureServiceErrorLevel.Failed, string.Empty, errorText));
                        }
                        else
                        {
                            string url = uploadResult.SelectSingleNode("//MediaUrl").InnerText;
                            OnUploadFinish(new PictureServiceEventArgs(PictureServiceErrorLevel.OK, url, string.Empty,
                                                                       postData.Filename));
                        }
                    }
                }
                catch
                {
                    OnErrorOccured(new PictureServiceEventArgs(PictureServiceErrorLevel.Failed, string.Empty, API_ERROR_UPLOAD));
                }
            }
        }

        /// <summary>
        /// Fetch a picture
        /// </summary>
        /// <param name="pictureURL"></param>
        public override void FetchPicture(string pictureURL)
        {
            #region Argument check

            //Need a url to read from.
            if (string.IsNullOrEmpty(pictureURL))
            {
                OnErrorOccured(new PictureServiceEventArgs(PictureServiceErrorLevel.Failed, "", API_ERROR_DOWNLOAD));
            }

            #endregion

            try
            {
                _workerPpo = new PicturePostObject {Message = pictureURL};

                if (_workerThread == null)
                {
                    _workerThread = new System.Threading.Thread(ProcessDownload){Name = "PictureUpload"};
                    _workerThread.Start();
                }
                else
                {
                    OnErrorOccured(new PictureServiceEventArgs(PictureServiceErrorLevel.NotReady, "", "A request is already running."));
                }
            }
            catch
            {
                OnErrorOccured(new PictureServiceEventArgs(PictureServiceErrorLevel.Failed, "", API_ERROR_DOWNLOAD));
            }
        }

        /// <summary>
        /// Post a picture including a message to the media service.
        /// </summary>
        /// <param name="postData"></param>
        /// <returns></returns>
        public override bool PostPictureMessage(PicturePostObject postData)
        {
            #region Argument check

            //Check for empty path
            if (string.IsNullOrEmpty(postData.Filename))
            {
                OnErrorOccured(new PictureServiceEventArgs(PictureServiceErrorLevel.Failed, string.Empty, API_ERROR_UPLOAD));
                return false;
            }

            //Check for empty credentials
            if (string.IsNullOrEmpty(postData.Username) ||
                string.IsNullOrEmpty(postData.Password))
            {
                OnErrorOccured(new PictureServiceEventArgs(PictureServiceErrorLevel.Failed, string.Empty, API_ERROR_UPLOAD));
                return false;
            }

            #endregion

            using (var file = new FileStream(postData.Filename, FileMode.Open, FileAccess.Read))
            {
                try
                {
                    //Load the picture data
                    var incoming = new byte[file.Length];
                    file.Read(incoming, 0, incoming.Length);

                    postData.PictureData = incoming;
                    XmlDocument uploadResult = UploadPictureMessage(API_UPLOAD_POST, postData);
                    if (uploadResult == null)
                    {
                        //event allready thrown in upload.
                        return false;
                    }
                    var nm = new XmlNamespaceManager(uploadResult.NameTable);
                    nm.AddNamespace("tweetPhoto", "http://tweetphotoapi.com");
                    if (uploadResult.SelectSingleNode("//tweetPhoto:Status",nm).InnerText!= "OK")
                    {
                        string errorText = uploadResult.SelectSingleNode("//err").Attributes["msg"].Value;
                        OnErrorOccured(new PictureServiceEventArgs(PictureServiceErrorLevel.Failed, string.Empty, errorText));
                        return false;
                    }
                }
                catch
                {
                    OnErrorOccured(new PictureServiceEventArgs(PictureServiceErrorLevel.Failed, string.Empty, API_ERROR_UPLOAD));
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// Test whether this service can fetch a picture.
        /// </summary>
        /// <param name="URL"></param>
        /// <returns></returns>
        public override bool CanFetchUrl(string URL)
        {
            
            if(IsTweetPhoto(URL))
            {
                return true;
            }
            //Makes an extra request for looking into every picture.
            //Don't want that, it is a risk for other services.
            //if (IsRedirect(URL))
            //{
            //    if (requestedUrl != URL)
            //    {
            //        requestedUrl = URL;
            //        string redirectedUrl = GetRedirectUrl(URL);
            //        if (!string.IsNullOrEmpty(redirectedUrl))
            //        {
            //            //If string is not null, and imageId is found.
            //            redirectedUrlIsPictureUrl = true;
            //        }
            //    }
            //    return redirectedUrlIsPictureUrl;
            //}
            return false;
        }

        #endregion

        #region thread implementation

        private void ProcessDownload()
        {
            try
            {
                string pictureURL = _workerPpo.Message;
                string imageID;
                if (IsRedirect(pictureURL))
                {
                    imageID = GetRedirectUrl(pictureURL);
                }
                else
                {
                    int imageIdStartIndex = pictureURL.LastIndexOf('/') + 1;
                    imageID = pictureURL.Substring(imageIdStartIndex, pictureURL.Length - imageIdStartIndex);
                }

                string resultFileName = RetrievePicture(imageID);

                if (!string.IsNullOrEmpty(resultFileName))
                {
                    OnDownloadFinish(new PictureServiceEventArgs(PictureServiceErrorLevel.Failed, resultFileName, string.Empty, pictureURL));
                }
            }
            catch
            {
                OnErrorOccured(new PictureServiceEventArgs(PictureServiceErrorLevel.Failed, string.Empty, API_ERROR_DOWNLOAD));
            }
            _workerThread = null;
        }

        private void ProcessUpload()
        {
            try
            {
                XmlDocument uploadResult = UploadPictureMessage(API_UPLOAD, _workerPpo);

                var nm = new XmlNamespaceManager(uploadResult.NameTable);
                nm.AddNamespace("tweetPhoto", "http://tweetphotoapi.com");
                if (uploadResult.SelectSingleNode("//tweetPhoto:Status", nm).InnerText != "OK")
                {
                    string errorText = uploadResult.SelectSingleNode("//err").Attributes["msg"].Value;
                    OnErrorOccured(new PictureServiceEventArgs(PictureServiceErrorLevel.Failed, string.Empty, errorText));
                }
                else
                {
                    string url = uploadResult.SelectSingleNode("//MediaURL").InnerText;
                    OnUploadFinish(new PictureServiceEventArgs(PictureServiceErrorLevel.OK, url, string.Empty, _workerPpo.Filename));
                }
            }
            catch(Exception ex)
            {
                OnErrorOccured(new PictureServiceEventArgs(PictureServiceErrorLevel.Failed, ex.Message, API_ERROR_UPLOAD));
            }
            _workerThread = null;
        }

        #endregion

        #region private methods

        /// <summary>
        /// because tweetphoto uses a shortener service (their own)
        /// look into that too.
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        private static bool IsRedirect(string toCheckUrl)
        {
            const string siteMarker = "pic.gd";
            string url = toCheckUrl.ToLower();

            return (url.IndexOf(siteMarker) >= 0);
        }

        private static bool IsTweetPhoto(string toCheckUrl)
        {
            const string siteMarker = "tweetphoto";
            string url = toCheckUrl.ToLower();

            return (url.IndexOf(siteMarker) >= 0);
        }

        private static string GetRedirectUrl(string url)
        {
            HttpWebRequest myRequest = WebRequestFactory.CreateHttpRequest(url);
            string responseUri;
            using (var response = (HttpWebResponse)myRequest.GetResponse())
            {
                responseUri = response.ResponseUri.ToString();
                response.Close();
            }
            string imageID = string.Empty;
            if (!string.IsNullOrEmpty(responseUri) && IsTweetPhoto(responseUri))
            {
                int imageIdStartIndex = responseUri.LastIndexOf('/') + 1;
                imageID = responseUri.Substring(imageIdStartIndex, responseUri.Length - imageIdStartIndex);
            }
            return imageID;
        }

        /// <summary>
        /// Use a imageId to retrieve and save a thumbnail to the device.
        /// </summary>
        /// <param name="imageId">Id for the image</param>
        /// <returns></returns>
        private string RetrievePicture(string imageId)
        {
            try
            {
                HttpWebRequest myRequest = WebRequestFactory.CreateHttpRequest(string.Format(API_SHOW_FORMAT, imageId));
                myRequest.Method = "GET";
                String pictureFileName;

                using (var response = (HttpWebResponse)myRequest.GetResponse())
                {
                    using (Stream dataStream = response.GetResponseStream())
                    {
                        var totalResponseSize = (int)response.ContentLength;
                        var readBuffer = new byte[PT_READ_BUFFER_SIZE];
                        pictureFileName = GetPicturePath(imageId);

                        int responseSize = dataStream.Read(readBuffer, 0, PT_READ_BUFFER_SIZE);
                        int totalSize = responseSize;
                        OnDownloadPart(new PictureServiceEventArgs(responseSize, totalSize, totalResponseSize));
                        while (responseSize > 0)
                        {
                            SavePicture(pictureFileName, readBuffer, responseSize);
                            try
                            {
                                totalSize += responseSize;
                                responseSize = dataStream.Read(readBuffer, 0, PT_READ_BUFFER_SIZE);
                                OnDownloadPart(new PictureServiceEventArgs(responseSize, totalSize, totalResponseSize));
                            }
                            catch
                            {
                                responseSize = 0;
                            }
                            System.Threading.Thread.Sleep(100);
                        }
                        dataStream.Close();
                    }
                    response.Close();
                }

                return pictureFileName;
            }
            catch
            {
                OnErrorOccured(new PictureServiceEventArgs(PictureServiceErrorLevel.Failed, string.Empty, API_ERROR_DOWNLOAD));
                return string.Empty;
            }
        }

        /// <summary>
        /// Upload the picture
        /// </summary>
        /// <param name="url">URL to upload picture to</param>
        /// <param name="ppo">Postdata</param>
        /// <returns></returns>
        
        /// <summary>
        /// Upload the picture
        /// </summary>
        /// <param name="url">URL to upload picture to</param>
        /// <param name="ppo">Postdata</param>
        /// <returns></returns>
        private XmlDocument UploadPictureMessage(string url, PicturePostObject ppo)
        {
            try
            {
                HttpWebRequest request = WebRequestFactory.CreateHttpRequest(url);
                request.ContentType = string.Format("application/x-www-form-urlencoded");
                request.Headers.Add("Authorization", String.Format("Basic {0}",
                    Convert.ToBase64String(
                        Encoding.Default.GetBytes(
                            String.Format("{0}:{1}", ppo.Username, ppo.Password)
                            )
                        )));
                request.Method = "POST";
                request.Headers.Add("TPUTF8", Boolean.TrueString);
                if (!string.IsNullOrEmpty(ppo.Message))
                {
                    request.Headers.Add("TPMSG", GetUTF8EncodedHeaderString(ppo.Message));
                }
                request.Headers.Add("TPAPIKEY", API_KEY);
                request.Headers.Add("TPMIMETYPE", "image/"+Path.GetExtension(ppo.Filename).ToLower().Substring(1));
                request.Headers.Add("TPPOST", (!string.IsNullOrEmpty(ppo.Message)).ToString());

                if (!string.IsNullOrEmpty(ppo.Lat) && !string.IsNullOrEmpty(ppo.Lon))
                {
                    request.Headers.Add("longitude", ppo.Lon);
                    request.Headers.Add("latitude", ppo.Lat);
                }

                //Create the form message to send in bytes
                //byte[] message = Encoding.UTF8.GetBytes(ppo.PictureData);
                request.ContentLength = ppo.PictureData.Length;
                request.Timeout = 1000 * 60 * 3;  //3 minute time out
                using (Stream requestStream = request.GetRequestStream())
                {
                    const int len = 1024 * 64;
                    for (int i = 0; i < ppo.PictureData.Length; i += len)
                    {
                        int writeLen = len;
                        if ((i + len) > ppo.PictureData.Length)
                        {
                            writeLen = ppo.PictureData.Length - i;
                        }
                        requestStream.Write(ppo.PictureData, i, writeLen);
                    }
                    requestStream.Flush();
                    requestStream.Close();

                    using (WebResponse response = request.GetResponse())
                    {
                        using (var reader = new StreamReader(response.GetResponseStream()))
                        {
                            var responseXML = new XmlDocument();
                            string rsp = reader.ReadToEnd();
                            responseXML.LoadXml(rsp);
                            return responseXML;
                        }
                    }
                }

            }
            catch (Exception ex)
            {
                //Socket exception 10054 could occur when sending large files.
                OnErrorOccured(new PictureServiceEventArgs(PictureServiceErrorLevel.Failed, ex.Message, API_ERROR_UPLOAD));
                return null;
            }

        }
        
        #endregion

        #region helper mehtods

        static string GetUTF8EncodedHeaderString(string str)
        {
            return Convert.ToBase64String(Encoding.UTF8.GetBytes(str));
        }

        #endregion
    }
}
