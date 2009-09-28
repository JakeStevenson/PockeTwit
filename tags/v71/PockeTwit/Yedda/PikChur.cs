using System;

using System.Collections.Generic;
using System.Text;
using Yedda;
using System.Xml;
using System.Net;
using System.IO;

namespace Yedda
{
    public class PikChur : PictureServiceBase
    {
        #region private properties

        private static volatile PikChur _instance;
        private static object syncRoot = new Object();

        private const string API_UPLOAD = "http://api.pikchur.com/post/xml";
        private const string API_AUTH = "http://api.pikchur.com/auth/xml";

        private const string API_SHOW_THUMB = "https://s3.amazonaws.com/pikchurimages/";  //The extra / for directly sticking the image-id on.

        private const string API_ERROR_UPLOAD = "Failed to upload picture to PikChur.";
        private const string API_ERROR_NOTREADY = "A request is already running.";
        private const string API_ERROR_DOWNLOAD = "Unable to download picture, try again later.";

        private string AUTH_KEY = string.Empty;
        private const string API_ORIGIN_ID = "MjUx";
        private const string API_KEY = "fzC/xJKgGySRN82+UPYvDA";

        #endregion

        #region private objects

        private System.Threading.Thread workerThread;
        private PicturePostObject workerPPO;

        #endregion

        #region constructor
        /// <summary>
        /// Private constructor for usage in singleton.
        /// </summary>
        private PikChur()
        {
            API_SAVE_TO_PATH = "\\ArtCache\\www.PikChur.com\\";
            API_SERVICE_NAME = "PikChur";
            API_CAN_UPLOAD_MESSAGE = true;
            API_CAN_UPLOAD_GPS = true;
            API_URLLENGTH = 23;
        }

        /// <summary>
        /// Singleton constructor
        /// </summary>
        /// <returns></returns>
        public static PikChur Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (syncRoot)
                    {
                        if (_instance == null)
                        {
                            _instance = new PikChur();
                            _instance.HasEventHandlersSet = false;
                        }
                    }
                }
                return _instance;
            }
        }

        #endregion

        #region IPictureService Members

        public override void PostPicture(PicturePostObject postData)
        {
            #region Argument check

            //Check for empty path
            if (string.IsNullOrEmpty(postData.Filename))
            {
                OnErrorOccured(new PictureServiceEventArgs(PictureServiceErrorLevel.Failed,string.Empty , API_ERROR_UPLOAD));
            }

            //Check for empty credentials
            if (string.IsNullOrEmpty(postData.Username) ||
                string.IsNullOrEmpty(postData.Password))
            {
                OnErrorOccured(new PictureServiceEventArgs(PictureServiceErrorLevel.Failed, string.Empty, API_ERROR_UPLOAD));
            }

            #endregion

            using (FileStream file = new FileStream(postData.Filename, FileMode.Open, FileAccess.Read))
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
                            OnErrorOccured(new PictureServiceEventArgs(PictureServiceErrorLevel.NotReady, string.Empty, API_ERROR_NOTREADY));
                        }
                    }
                    else
                    {
                        //use sync.
                        postData.PictureData = incoming;
                        XmlDocument uploadResult;
                        if (string.IsNullOrEmpty(postData.Message))
                        {
                            uploadResult = UploadPicture(postData);
                        }
                        else
                        {
                            uploadResult = UploadPictureAndPost(postData);
                        }

                        if (uploadResult.SelectSingleNode("pikchur/error") == null)
                        {
                            XmlNode UrlKeyNode = uploadResult.SelectSingleNode("pikchur/post/url");
                            string URL = UrlKeyNode.InnerText;
                            URL = URL.Replace("\n","");

                            OnUploadFinish(new PictureServiceEventArgs(PictureServiceErrorLevel.OK, URL, string.Empty, postData.Filename));
                        }
                        else
                        {
                            OnErrorOccured(new PictureServiceEventArgs(PictureServiceErrorLevel.Failed, string.Empty, API_ERROR_UPLOAD));
                        }
                    }
                }
                catch (Exception e)
                {
                    OnErrorOccured(new PictureServiceEventArgs(PictureServiceErrorLevel.Failed, string.Empty, API_ERROR_UPLOAD));
                }
            }
        }

        public override void FetchPicture(string pictureURL)
        {
            #region Argument check

            //Need a url to read from.
            if (string.IsNullOrEmpty(pictureURL))
            {
                OnErrorOccured(new PictureServiceEventArgs(PictureServiceErrorLevel.Failed, string.Empty, API_ERROR_UPLOAD));
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
                    OnErrorOccured(new PictureServiceEventArgs(PictureServiceErrorLevel.NotReady, string.Empty, API_ERROR_NOTREADY));
                }
            }
            catch (Exception e)
            {
                OnErrorOccured(new PictureServiceEventArgs(PictureServiceErrorLevel.Failed, string.Empty, API_ERROR_UPLOAD));
            }
        }

        public override bool CanFetchUrl(string URL)
        {
            //https://s3.amazonaws.com/pikchurimages/pic_r68_m.jpg
            //http://www.pikchur.com/t5o
            const string siteMarker = "pikchur";
            string url = URL.ToLower();

            return (url.IndexOf(siteMarker) >= 0);
        }

        /// <summary>
        /// Send a picture to a twitter picture framework without the use of the finish event
        /// </summary>
        /// <param name="postData">Postdata</param>
        /// <returns>Returned URL from server</returns>
        public override bool PostPictureMessage(PicturePostObject postData)
        {
            #region Argument check

            //Check for empty path
            if (string.IsNullOrEmpty(postData.Filename))
            {
                OnErrorOccured(new PictureServiceEventArgs(PictureServiceErrorLevel.Failed, string.Empty , API_ERROR_UPLOAD));
                return false;
            }

            //Check for empty credentials
            if (string.IsNullOrEmpty(postData.Username) ||
                string.IsNullOrEmpty(postData.Password))
            {
                OnErrorOccured(new PictureServiceEventArgs(PictureServiceErrorLevel.Failed,string.Empty , API_ERROR_UPLOAD));
                return false;
            }

            #endregion

            using (FileStream file = new FileStream(postData.Filename, FileMode.Open, FileAccess.Read))
            {
                try
                {
                    //Load the picture data
                    byte[] incoming = new byte[file.Length];
                    file.Read(incoming, 0, incoming.Length);

                    //use sync.
                    postData.PictureData = incoming;
                    XmlDocument uploadResult = UploadPictureAndPost(postData);

                    if (uploadResult.SelectSingleNode("pikchur/error") == null)
                    {
                        XmlNode UrlKeyNode = uploadResult.SelectSingleNode("pikchur/post/url");
                        string URL = UrlKeyNode.InnerText;
                        URL = URL.Replace("\n", "");
                    }
                    else
                    {
                        OnErrorOccured(new PictureServiceEventArgs(PictureServiceErrorLevel.Failed, string.Empty, API_ERROR_UPLOAD));
                    }
                }
                catch (Exception e)
                {
                    OnErrorOccured(new PictureServiceEventArgs(PictureServiceErrorLevel.Failed, string.Empty, API_ERROR_UPLOAD));
                    return false;
                }
            }
            return true;
        }


        #endregion

        #region thread implementation

        private void ProcessDownload()
        {
            try
            {
                string pictureURL = workerPPO.Message;
                int imageIdStartIndex = pictureURL.LastIndexOf('/') + 1;
                string imageID = pictureURL.Substring(imageIdStartIndex, pictureURL.Length - imageIdStartIndex);

                string resultFileName = RetrievePicture(imageID);

                if (!string.IsNullOrEmpty(resultFileName))
                {
                    OnDownloadFinish(new PictureServiceEventArgs(PictureServiceErrorLevel.OK, resultFileName, string.Empty, pictureURL));
                }
            }
            catch (Exception e)
            {
                OnErrorOccured(new PictureServiceEventArgs(PictureServiceErrorLevel.Failed, string.Empty, API_ERROR_DOWNLOAD));
            }
            workerThread = null;
        }

        private void ProcessUpload()
        {
            try
            {
                XmlDocument uploadResult = UploadPicture(workerPPO);

                if (uploadResult == null)
                {
                    workerThread = null;
                    return;
                }

                if (uploadResult.SelectSingleNode("pikchur/error") == null)
                {
                    XmlNode UrlKeyNode = uploadResult.SelectSingleNode("pikchur/post/url");
                    string URL = UrlKeyNode.InnerText;
                    URL = URL.Replace("\n", "");

                    OnUploadFinish(new PictureServiceEventArgs(PictureServiceErrorLevel.OK, URL, string.Empty, workerPPO.Filename));
                }
                else
                {
                    OnErrorOccured(new PictureServiceEventArgs(PictureServiceErrorLevel.Failed, string.Empty, API_ERROR_UPLOAD));
                }
            }
            catch (Exception e)
            {
                OnErrorOccured(new PictureServiceEventArgs(PictureServiceErrorLevel.Failed, string.Empty, API_ERROR_UPLOAD));
            }
            workerThread = null;
        }

        #endregion

        #region private methods

        /// <summary>
        /// Use a imageId to retrieve and save a thumbnail to the device.
        /// </summary>
        /// <param name="imageId">Id for the image</param>
        /// <returns></returns>
        private string RetrievePicture(string imageId)
        {
            try
            {
                string URL_FORMAT = "https://s3.amazonaws.com/pikchurimages/pic_{0}_m.jpg";

                HttpWebRequest myRequest = (HttpWebRequest)WebRequest.Create(string.Format(URL_FORMAT, imageId));
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
            catch (Exception e)
            {
                OnErrorOccured(new PictureServiceEventArgs(PictureServiceErrorLevel.Failed, string.Empty, API_ERROR_DOWNLOAD));
                return string.Empty;
            }
        }

        /// <summary>
        /// Request authorisation key for use in posting.
        /// </summary>
        /// <param name="ppo"></param>
        private void RequestAuthKey(PicturePostObject ppo)
        {
            try
            {
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(API_AUTH);

                string boundary = System.Guid.NewGuid().ToString();
                NetworkCredential myCred = new NetworkCredential(ppo.Username, ppo.Password);
                CredentialCache MyCrendentialCache = new CredentialCache();
                Uri uri = new Uri(API_AUTH);
                MyCrendentialCache.Add(uri, "Basic", myCred);
                request.Credentials = MyCrendentialCache;
                request.UserAgent = "Mozilla/4.0 (compatible; MSIE 7.0b; Windows NT 6.0)";
                request.Headers.Set("Pragma", "no-cache");
                request.ContentType = string.Format("multipart/form-data;boundary={0}", boundary);

                //request.ContentType = "application/x-www-form-urlencoded";
                request.Method = "POST";
                request.Timeout = 20000;

                string header = string.Format("--{0}", boundary);
                string ender = header + "\r\n";

                StringBuilder contents = new StringBuilder();

                contents.Append(CreateContentPartStringForm(header, "data[api][username]", ppo.Username, "application/octet-stream"));
                contents.Append(CreateContentPartStringForm(header, "data[api][password]", ppo.Password, "application/octet-stream"));
                contents.Append(CreateContentPartStringForm(header, "data[api][service]", "twitter", "application/octet-stream"));
                contents.Append(CreateContentPartStringForm(header, "data[api][key]", API_KEY, "application/octet-stream"));

                //Create the form message to send in bytes
                byte[] message = Encoding.UTF8.GetBytes(contents.ToString());

                request.ContentLength = message.Length;
                using (Stream requestStream = request.GetRequestStream())
                {
                    requestStream.Write(message, 0, message.Length);
                    requestStream.Flush();
                    requestStream.Close();

                    using (WebResponse response = request.GetResponse())
                    {
                        using (StreamReader reader = new StreamReader(response.GetResponseStream()))
                        {
                            XmlDocument responseXML = new XmlDocument();
                            string responseFromService = reader.ReadToEnd();
                            responseXML.LoadXml(responseFromService);
                            if (responseXML.SelectSingleNode("pikchur/error") == null)
                            {
                                XmlNode authKeyNode = responseXML.SelectSingleNode("pikchur/auth_key");
                                AUTH_KEY = authKeyNode.InnerText;
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                //OnErrorOccured(new PictureServiceEventArgs(PictureServiceErrorLevel.Failed, string.Empty, API_ERROR_DOWNLOAD));
            }
        }

        /// <summary>
        /// Upload picture
        /// </summary>
        /// <param name="ppo"></param>
        /// <returns></returns>
        private XmlDocument UploadPicture(PicturePostObject ppo)
        {
            if (string.IsNullOrEmpty(AUTH_KEY))
            {
                RequestAuthKey(ppo);
            }
            if ( string.IsNullOrEmpty( AUTH_KEY ))
            {
                //Authorisation has failed.
                OnErrorOccured(new PictureServiceEventArgs(PictureServiceErrorLevel.Failed, string.Empty, API_ERROR_UPLOAD));
                return null;
            }
            try
            {
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(API_UPLOAD);

                string boundary = Guid.NewGuid().ToString();
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

                contents.Append(CreateContentPartStringForm(header, "data[api][key]", API_KEY, "application/octet-stream"));
                contents.Append(CreateContentPartStringForm(header, "data[api][status]", "Uploaded with PockeTwit", "application/octet-stream"));
                contents.Append(CreateContentPartStringForm(header, "data[api][auth_key]", AUTH_KEY, "application/octet-stream"));
                contents.Append(CreateContentPartStringForm(header, "data[api][upload_only]", "TRUE", "application/octet-stream"));
                contents.Append(CreateContentPartStringForm(header, "data[api][origin]", API_ORIGIN_ID, "application/octet-stream"));

                //image
                contents.Append(CreateContentPartPicture(header,"dataAPIimage", "image.jpg"));

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
                            string responseFromService = reader.ReadToEnd();
                            responseXML.LoadXml(responseFromService);
                            return responseXML;
                        }
                    }
                }
            }
            catch (Exception e)
            {
                //Socket exception 10054 could occur when sending large files.
                OnErrorOccured(new PictureServiceEventArgs(PictureServiceErrorLevel.Failed, string.Empty, API_ERROR_UPLOAD));
                return null;
            }
        }

        /// <summary>
        /// Upload picture
        /// </summary>
        /// <param name="ppo"></param>
        /// <returns></returns>
        private XmlDocument UploadPictureAndPost(PicturePostObject ppo)
        {
            if (string.IsNullOrEmpty(AUTH_KEY))
            {
                RequestAuthKey(ppo);
            }
            if (string.IsNullOrEmpty(AUTH_KEY))
            {
                //Authorisation has failed.
                OnErrorOccured(new PictureServiceEventArgs(PictureServiceErrorLevel.Failed, string.Empty, API_ERROR_UPLOAD));
                return null;
            }
            try
            {
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(API_UPLOAD);

                string boundary = Guid.NewGuid().ToString();
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

                contents.Append(CreateContentPartStringForm(header, "data[api][key]", API_KEY, "application/octet-stream"));
                contents.Append(CreateContentPartStringForm(header, "data[api][status]", ppo.Message, "application/octet-stream"));
                contents.Append(CreateContentPartStringForm(header, "data[api][auth_key]", AUTH_KEY, "application/octet-stream"));
                contents.Append(CreateContentPartStringForm(header, "data[api][upload_only]", "FALSE", "application/octet-stream"));
                contents.Append(CreateContentPartStringForm(header, "data[api][origin]", API_ORIGIN_ID, "application/octet-stream"));

                if (!string.IsNullOrEmpty(ppo.Lat) && !string.IsNullOrEmpty(ppo.Lon))
                {
                    contents.Append(CreateContentPartStringForm(header, "data[api][geo][lat]", ppo.Lat, "application/octet-stream"));
                    contents.Append(CreateContentPartStringForm(header, "data[api][geo][lon]", ppo.Lon, "application/octet-stream"));
                }
                //image
                contents.Append(CreateContentPartPicture(header, "dataAPIimage", "image.jpg"));

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
                            string responseFromService = reader.ReadToEnd();
                            responseXML.LoadXml(responseFromService);
                            return responseXML;
                        }
                    }
                }
            }
            catch (Exception e)
            {
                //Socket exception 10054 could occur when sending large files.
                OnErrorOccured(new PictureServiceEventArgs(PictureServiceErrorLevel.Failed, string.Empty, API_ERROR_UPLOAD));
                return null;
            }
        }


        #endregion
    }
}
