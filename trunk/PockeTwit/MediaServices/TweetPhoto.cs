using System;
using System.IO;
using System.Net;
using System.Xml;
using System.Web;
using System.Collections.Generic;
using System.Text;

namespace PockeTwit.MediaServices
{
    public class TweetPhoto : PictureServiceBase
    {
        #region private properties

        private static volatile TweetPhoto _instance;
        private static object syncRoot = new Object();

        private const string API_UPLOAD = "http://www.tweetphoto.com/uploadapiwithkey.php";
        private const string API_UPLOAD_POST = "http://www.tweetphoto.com/uploadandpostapiwithkey.php";
        
        private const string API_SHOW_FORMAT = "http://www.tweetphoto.com/show/medium/{0}";  //The extra / for directly sticking the image-id on.

        private const string API_ERROR_UPLOAD = "Unable to upload to TweetPhoto";
        private const string API_ERROR_DOWNLOAD = "Unable to download from TweetPhoto";

        private const string API_KEY = "cd6fa2df805addb613d06a91f24bdf01";

        private string requestedUrl = string.Empty;

        #endregion

        #region private objects

        private System.Threading.Thread workerThread;
        private PicturePostObject workerPPO;

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
                    lock (syncRoot)
                    {
                        if (_instance == null)
                        {
                            _instance = new TweetPhoto();
                            _instance.HasEventHandlersSet = false;
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

            using (System.IO.FileStream file = new FileStream(postData.Filename, FileMode.Open, FileAccess.Read))
            {
                try
                {
                    //Load the picture data
                    byte[] incoming = new byte[file.Length];
                    file.Read(incoming, 0, incoming.Length);

                    if (postData.UseAsync)
                    {
                        workerPPO = (PicturePostObject)postData.Clone();
                        workerPPO.PictureData = incoming;

                        if (workerThread == null)
                        {
                            workerThread = new System.Threading.Thread(new System.Threading.ThreadStart(ProcessUpload));
                            workerThread.Name = "PictureUpload";
                            workerThread.Start();
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
                        XmlDocument uploadResult = UploadPicture(API_UPLOAD, postData);

                        if (uploadResult == null)
                        {
                            return;
                        }

                        if (uploadResult.SelectSingleNode("rsp").Attributes["status"].Value == "fail")
                        {
                            string ErrorText = uploadResult.SelectSingleNode("//err").Attributes["msg"].Value;
                            OnErrorOccured(new PictureServiceEventArgs(PictureServiceErrorLevel.Failed, string.Empty, ErrorText));
                        }
                        else
                        {
                            string URL = uploadResult.SelectSingleNode("//mediaurl").InnerText;
                            OnUploadFinish(new PictureServiceEventArgs(PictureServiceErrorLevel.OK, URL, string.Empty, postData.Filename));
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
                workerPPO = new PicturePostObject();
                workerPPO.Message = pictureURL;

                if (workerThread == null)
                {
                    workerThread = new System.Threading.Thread(new System.Threading.ThreadStart(ProcessDownload));
                    workerThread.Name = "PictureUpload";
                    workerThread.Start();
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

            using (System.IO.FileStream file = new FileStream(postData.Filename, FileMode.Open, FileAccess.Read))
            {
                try
                {
                    //Load the picture data
                    byte[] incoming = new byte[file.Length];
                    file.Read(incoming, 0, incoming.Length);

                    postData.PictureData = incoming;
                    XmlDocument uploadResult = UploadPictureMessage(API_UPLOAD_POST, postData);

                    if (uploadResult == null)
                    {
                        //event allready thrown in upload.
                        return false;
                    }

                    if (uploadResult.SelectSingleNode("rsp").Attributes["status"].Value == "fail")
                    {
                        string ErrorText = uploadResult.SelectSingleNode("//err").Attributes["msg"].Value;
                        OnErrorOccured(new PictureServiceEventArgs(PictureServiceErrorLevel.Failed, string.Empty, ErrorText));
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
                string pictureURL = workerPPO.Message;
                string imageID = string.Empty;
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
            workerThread = null;
        }

        private void ProcessUpload()
        {
            try
            {
                XmlDocument uploadResult = UploadPicture(API_UPLOAD, workerPPO);

                if (uploadResult.SelectSingleNode("rsp").Attributes["status"].Value == "fail")
                {
                    string ErrorText = uploadResult.SelectSingleNode("//err").Attributes["msg"].Value;
                    OnErrorOccured(new PictureServiceEventArgs(PictureServiceErrorLevel.Failed, string.Empty, ErrorText));
                }
                else
                {
                    string URL = uploadResult.SelectSingleNode("//mediaurl").InnerText;
                    OnUploadFinish(new PictureServiceEventArgs(PictureServiceErrorLevel.OK, URL, string.Empty, workerPPO.Filename));
                }
            }
            catch
            {
                OnErrorOccured(new PictureServiceEventArgs(PictureServiceErrorLevel.Failed, string.Empty, API_ERROR_UPLOAD));
            }
            workerThread = null;
        }

        #endregion

        #region private methods

        /// <summary>
        /// because tweetphoto uses a shortener service (their own)
        /// look into that too.
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        private bool IsRedirect(string toCheckUrl)
        {
            const string siteMarker = "pic.gd";
            string url = toCheckUrl.ToLower();

            return (url.IndexOf(siteMarker) >= 0);
        }

        private bool IsTweetPhoto(string toCheckUrl)
        {
            const string siteMarker = "tweetphoto";
            string url = toCheckUrl.ToLower();

            return (url.IndexOf(siteMarker) >= 0);
        }

        private string GetRedirectUrl(string url)
        {
            HttpWebRequest myRequest = WebRequestFactory.CreateHttpRequest(url);
            string responseUri = string.Empty;
            using (HttpWebResponse response = (HttpWebResponse)myRequest.GetResponse())
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
                String pictureFileName = String.Empty;

                using (HttpWebResponse response = (HttpWebResponse)myRequest.GetResponse())
                {
                    using (Stream dataStream = response.GetResponseStream())
                    {
                        int totalSize = 0;
                        int totalResponseSize = (int)response.ContentLength;
                        byte[] readBuffer = new byte[PT_READ_BUFFER_SIZE];
                        pictureFileName = GetPicturePath(imageId);

                        int responseSize = dataStream.Read(readBuffer, 0, PT_READ_BUFFER_SIZE);
                        totalSize = responseSize;
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
        private XmlDocument UploadPicture(string url, PicturePostObject ppo)
        {
            try
            {
                HttpWebRequest request = WebRequestFactory.CreateHttpRequest(url);

                string boundary = System.Guid.NewGuid().ToString();
                request.Credentials = new NetworkCredential(ppo.Username, ppo.Password);
                request.Headers.Add("Accept-Language", "cs,en-us;q=0.7,en;q=0.3");
                request.PreAuthenticate = true;
                request.ContentType = string.Format("multipart/form-data;boundary={0}", boundary);

                //request.ContentType = "application/x-www-form-urlencoded";
                request.Method = "POST";
                request.Timeout = 20000;
                string header = string.Format("--{0}", boundary);
                string ender = "\r\n" + header + "\r\n";

                StringBuilder contents = new StringBuilder();

                contents.Append(CreateContentPartString(header, "username", ppo.Username));
                contents.Append(CreateContentPartString(header, "password", ppo.Password));
                contents.Append(CreateContentPartString(header, "api_key", API_KEY));

                int imageIdStartIndex = ppo.Filename.LastIndexOf('\\') + 1;
                string filename = ppo.Filename.Substring(imageIdStartIndex, ppo.Filename.Length - imageIdStartIndex);
                contents.Append(CreateContentPartPicture(header, filename));

                //Create the form message to send in bytes
                byte[] message = Encoding.UTF8.GetBytes(contents.ToString());
                byte[] footer = Encoding.UTF8.GetBytes(ender);
                request.ContentLength = message.Length + ppo.PictureData.Length + footer.Length;
                using (Stream requestStream = request.GetRequestStream())
                {
                    requestStream.Write(message, 0, message.Length);
                    requestStream.Write(ppo.PictureData, 0, ppo.PictureData.Length);
                    requestStream.Write(footer, 0, footer.Length);
                    requestStream.Flush();
                    requestStream.Close();

                    using (WebResponse response = request.GetResponse())
                    {
                        using (StreamReader reader = new StreamReader(response.GetResponseStream()))
                        {
                            XmlDocument responseXML = new XmlDocument();
                            string rsp = reader.ReadToEnd();
                            responseXML.LoadXml(rsp);
                            return responseXML;
                        }
                    }
                }

            }
            catch
            {
                //Socket exception 10054 could occur when sending large files.

                OnErrorOccured(new PictureServiceEventArgs(PictureServiceErrorLevel.Failed, string.Empty, API_ERROR_UPLOAD));
                return null;
            }
        }


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

                string boundary = System.Guid.NewGuid().ToString();
                request.Credentials = new NetworkCredential(ppo.Username, ppo.Password);
                request.Headers.Add("Accept-Language", "cs,en-us;q=0.7,en;q=0.3");
                request.PreAuthenticate = true;
                request.ContentType = string.Format("multipart/form-data;boundary={0}", boundary);

                request.Method = "POST";
                request.Timeout = 20000;
                string header = string.Format("--{0}", boundary);
                string ender = "\r\n" + header + "\r\n";

                StringBuilder contents = new StringBuilder();

                contents.Append(CreateContentPartString(header, "username", ppo.Username));
                contents.Append(CreateContentPartString(header, "password", ppo.Password));
                contents.Append(CreateContentPartString(header, "message", ppo.Message));
                contents.Append(CreateContentPartString(header, "api_key", API_KEY));

                if (!string.IsNullOrEmpty(ppo.Lat) && !string.IsNullOrEmpty(ppo.Lon))
                {
                    contents.Append(CreateContentPartString(header, "longitude", ppo.Lon));
                    contents.Append(CreateContentPartString(header, "latitude", ppo.Lat));
                }

                int imageIdStartIndex = ppo.Filename.LastIndexOf('\\') + 1;
                string filename = ppo.Filename.Substring(imageIdStartIndex, ppo.Filename.Length - imageIdStartIndex);
                contents.Append(CreateContentPartPicture(header, filename));

                //Create the form message to send in bytes
                byte[] message = Encoding.UTF8.GetBytes(contents.ToString());
                byte[] footer = Encoding.UTF8.GetBytes(ender);
                request.ContentLength = message.Length + ppo.PictureData.Length + footer.Length;
                using (Stream requestStream = request.GetRequestStream())
                {
                    requestStream.Write(message, 0, message.Length);
                    requestStream.Write(ppo.PictureData, 0, ppo.PictureData.Length);
                    requestStream.Write(footer, 0, footer.Length);
                    requestStream.Flush();
                    requestStream.Close();

                    using (WebResponse response = request.GetResponse())
                    {
                        using (StreamReader reader = new StreamReader(response.GetResponseStream()))
                        {
                            XmlDocument responseXML = new XmlDocument();
                            string rsp = reader.ReadToEnd();
                            responseXML.LoadXml(rsp);
                            return responseXML;
                        }
                    }
                }

            }
            catch (Exception)
            {
                //Socket exception 10054 could occur when sending large files.
                OnErrorOccured(new PictureServiceEventArgs(PictureServiceErrorLevel.Failed, string.Empty, API_ERROR_UPLOAD));
                return null;
            }

        }
        
        #endregion

        #region helper mehtods

        private string CreateContentPartString(string header, string dispositionName, string valueToSend)
        {
            StringBuilder contents = new StringBuilder();

            contents.Append(header);
            contents.Append("\r\n");
            contents.Append(String.Format("Content-Disposition: form-data;name=\"{0}\"\r\n", dispositionName));
            contents.Append("\r\n");
            contents.Append(valueToSend);
            contents.Append("\r\n");

            return contents.ToString();
        }

        private string CreateContentPartPicture(string header, string filename)
        {
            StringBuilder contents = new StringBuilder();

            contents.Append(header);
            contents.Append("\r\n");
            contents.Append(string.Format("content-disposition:form-data; name=\"media\";filename=\"{0}\"\r\n", filename));
            contents.Append("Content-Type: image/jpeg\r\n");
            contents.Append("\r\n");

            return contents.ToString();
        }


        #endregion
    }
}
