using System;
using System.IO;
using System.Net;
using System.Xml;
using System.Web;
using System.Collections.Generic;
using System.Text;

namespace PockeTwit.MediaServices
{
    public class MobyPicture : PictureServiceBase
    {
        #region private properties
        private static volatile MobyPicture _instance;
        private static object syncRoot = new Object();

        private const string APPLICATION_NAME = "p0ck3tTTTTw";
        private const string API_URL = "http://api.mobypicture.com";

        private const string API_UPLOAD = "http://api.mobypicture.com/";
        private const string API_UPLOAD_POST = "http://api.mobypicture.com/";
        private const string API_GET_THUMB = "http://api.mobypicture.com/?s=small&format=plain&k="+APPLICATION_NAME;  //The extra / for directly sticking the image-id on.

        private const string API_ERROR_UPLOAD = "Failed to upload picture to MobyPicture.";
        private const string API_ERROR_DOWNLOAD = "Failed to download picture from MobyPicture.";

        private byte[] readBuffer;
        private Stream dataStream;
        private bool useAsyncCall = false;


        #endregion

        #region private objects

        private System.Threading.Thread workerThread;
        private PicturePostObject workerPPO;

        #endregion

        #region constructor
        /// <summary>
        /// Private constructor for usage in singleton.
        /// </summary>
        private MobyPicture()
        {
            
            API_SAVE_TO_PATH = "\\ArtCache\\www.mobypicture.com\\";
            API_SERVICE_NAME = "MobyPicture";
            API_CAN_UPLOAD_GPS = true;
            API_CAN_UPLOAD_MESSAGE = true;
            API_CAN_UPLOAD_MOREMEDIA = true;
            API_URLLENGTH = 31;

            API_FILETYPES.Add(new MediaType("jpg", "image/jpeg", MediaTypeGroup.PICTURE));
            API_FILETYPES.Add(new MediaType("jpeg", "image/jpeg", MediaTypeGroup.PICTURE));
            API_FILETYPES.Add(new MediaType("gif", "image/gif", MediaTypeGroup.PICTURE));
            API_FILETYPES.Add(new MediaType("png", "image/png", MediaTypeGroup.PICTURE));

            API_FILETYPES.Add(new MediaType("bmp", "image/bmp", MediaTypeGroup.PICTURE));
            API_FILETYPES.Add(new MediaType("flv", "video/x-flv", MediaTypeGroup.VIDEO));


            API_FILETYPES.Add(new MediaType("mpeg", "", MediaTypeGroup.VIDEO));
            API_FILETYPES.Add(new MediaType("mkv", "", MediaTypeGroup.VIDEO));
            API_FILETYPES.Add(new MediaType("wmv", "", MediaTypeGroup.VIDEO));
            API_FILETYPES.Add(new MediaType("mov", "", MediaTypeGroup.VIDEO));
            API_FILETYPES.Add(new MediaType("3gp", "", MediaTypeGroup.VIDEO));
            API_FILETYPES.Add(new MediaType("mp4", "", MediaTypeGroup.VIDEO));
            API_FILETYPES.Add(new MediaType("avi", "", MediaTypeGroup.VIDEO));
            API_FILETYPES.Add(new MediaType("mp3", "", MediaTypeGroup.AUDIO));
            API_FILETYPES.Add(new MediaType("wma", "", MediaTypeGroup.AUDIO));
            API_FILETYPES.Add(new MediaType("aac", "", MediaTypeGroup.AUDIO));
            API_FILETYPES.Add(new MediaType("aif", "", MediaTypeGroup.AUDIO));
            API_FILETYPES.Add(new MediaType("au", "", MediaTypeGroup.AUDIO));
            API_FILETYPES.Add(new MediaType("flac", "", MediaTypeGroup.AUDIO));
            API_FILETYPES.Add(new MediaType("ra", "", MediaTypeGroup.AUDIO));
            API_FILETYPES.Add(new MediaType("wav", "", MediaTypeGroup.AUDIO));
            API_FILETYPES.Add(new MediaType("ogg", "", MediaTypeGroup.AUDIO));


        }

        /// <summary>
        /// Singleton constructor
        /// </summary>
        /// <returns></returns>
        public static MobyPicture Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (syncRoot)
                    {
                        if (_instance == null)
                        {
                            _instance = new MobyPicture();
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
        /// Start posting a picture
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
                        string uploadResult = UploadPicture(API_UPLOAD, postData);

                        if (!string.IsNullOrEmpty(uploadResult))
                        {
                            OnUploadFinish(new PictureServiceEventArgs(PictureServiceErrorLevel.OK, uploadResult, string.Empty, postData.Filename));
                        }

                    }
                }
                catch (Exception)
                {
                    OnErrorOccured(new PictureServiceEventArgs(PictureServiceErrorLevel.Failed, string.Empty, API_ERROR_UPLOAD));
                }
            }
        }

        /// <summary>
        /// Fetch a picture.
        /// </summary>
        /// <param name="pictureURL"></param>
        public override void FetchPicture(string pictureURL)
        {
            #region Argument check

            //Need a url to read from.
            if (string.IsNullOrEmpty(pictureURL))
            {
                OnErrorOccured(new PictureServiceEventArgs(PictureServiceErrorLevel.Failed, string.Empty, API_ERROR_DOWNLOAD));
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
                    OnErrorOccured(new PictureServiceEventArgs(PictureServiceErrorLevel.NotReady, string.Empty, "A request is already running."));
                }
            }
            catch (Exception)
            {
                OnErrorOccured(new PictureServiceEventArgs(PictureServiceErrorLevel.Failed, string.Empty, API_ERROR_DOWNLOAD));
            } 
        }

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
                    string uploadResult = UploadPictureMessage(API_UPLOAD_POST, postData);

                    if (string.IsNullOrEmpty(uploadResult))
                    {
                        //OnErrorOccured(new PictureServiceEventArgs(PictureServiceErrorLevel.Failed, string.Empty, API_ERROR_UPLOAD));
                        return false;
                    }
                    

                }
                catch (Exception)
                {
                    OnErrorOccured(new PictureServiceEventArgs(PictureServiceErrorLevel.Failed, string.Empty, API_ERROR_UPLOAD));
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// Check whether the service can fetch an URL
        /// </summary>
        /// <param name="URL"></param>
        /// <returns></returns>
        public override bool CanFetchUrl(string URL)
        {
            const string siteMarker = "mobypicture";
            const string notAllowedInUrl = "user";

            string url = URL.ToLower();
            return (url.IndexOf(siteMarker) >= 0 && url.IndexOf(notAllowedInUrl) < 0);
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

                if (!useAsyncCall)
                {
                    string resultFileName = RetrievePicture(pictureURL);
                    if (!string.IsNullOrEmpty(resultFileName))
                    {
                        OnDownloadFinish(new PictureServiceEventArgs(PictureServiceErrorLevel.Failed, resultFileName, ""));
                    }
                }
                else
                {
                    RetrievePictureAsync(pictureURL);
                }
            }
            catch (Exception)
            {
                //No need to throw, postPicture throws event.        
                //OnErrorOccured(new PictureServiceEventArgs(PictureServiceErrorLevel.Failed, "", "Unable to download picture, try again later."));
            }
            workerThread = null;
        }

        private void ProcessUpload()
        {
            try
            {
                string uploadResult = UploadPicture(API_UPLOAD, workerPPO);

                if (!string.IsNullOrEmpty( uploadResult) )
                {
                    OnUploadFinish(new PictureServiceEventArgs(PictureServiceErrorLevel.OK, uploadResult, "", workerPPO.Filename));
                }
            }
            catch (Exception)
            {
                OnErrorOccured(new PictureServiceEventArgs(PictureServiceErrorLevel.Failed, string.Empty, API_ERROR_UPLOAD));
            }
            workerThread = null;
        }

        #endregion

        #region private methods

        /// <summary>
        /// Upload a picture
        /// </summary>
        /// <param name="url"></param>
        /// <param name="ppo"></param>
        /// <returns></returns>
        private string UploadPicture(string url, PicturePostObject ppo)
        {
            try
            {
                HttpWebRequest request = WebRequestFactory.CreateHttpRequest(url);

                string boundary = System.Guid.NewGuid().ToString();
                request.Credentials = new NetworkCredential(ppo.Username, ppo.Password);
                request.Headers.Add("Accept-Language", "cs,en-us;q=0.7,en;q=0.3");
                request.PreAuthenticate = true;
                request.ContentType = string.Format("multipart/form-data;boundary={0}", boundary);
                request.Headers.Add("Action", "postMediaUrl");

                //request.ContentType = "application/x-www-form-urlencoded";
                request.Method = "POST";
                request.Timeout = 20000;
                string header = string.Format("--{0}", boundary);
                string ender = "\r\n" + header + "\r\n";

                StringBuilder contents = new StringBuilder();

                contents.Append(CreateContentPartString(header, "u", ppo.Username));
                contents.Append(CreateContentPartString(header, "p", ppo.Password));
                contents.Append(CreateContentPartString(header, "k", APPLICATION_NAME));
                //Don't send the picture to twitter or any service just yet.
                contents.Append(CreateContentPartString(header, "s", "none"));

                string hashTags = FindHashTags(ppo.Message,",",32);
                if (!string.IsNullOrEmpty(hashTags)) 
                {
                    contents.Append(CreateContentPartString(header, "ht", hashTags));
                }

                if (!string.IsNullOrEmpty(ppo.Message))
                {
                    contents.Append(CreateContentPartString(header, "message", ppo.Message));
                    contents.Append(CreateContentPartString(header, "action", "postMedia"));
                }
                else
                {
                    contents.Append(CreateContentPartString(header, "action", "postMediaUrl"));
                }



                int imageIdStartIndex = ppo.Filename.LastIndexOf('\\') + 1;
                string filename = ppo.Filename.Substring(imageIdStartIndex, ppo.Filename.Length - imageIdStartIndex);
                contents.Append(CreateContentPartMedia(header, filename));

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
                            String receiverResponse = reader.ReadToEnd();
                            //should be 0 with a following URL for the picture.

                            return receiverResponse;
                        }
                    }
                }
            }
            catch (Exception)
            {
                return string.Empty;
                // JohnB2007 commented this out to avoid unreachable code warning.
                // not sure if order should be flipped actually?
                // OnErrorOccured(new PictureServiceEventArgs(PictureServiceErrorLevel.Failed, string.Empty, API_ERROR_UPLOAD));
            }
            //return string.Empty;
        }

        /// <summary>
        /// Upload a picture
        /// </summary>
        /// <param name="url"></param>
        /// <param name="ppo"></param>
        /// <returns></returns>
        private string UploadPictureMessage(string url, PicturePostObject ppo)
        {
            try
            {
                HttpWebRequest request = WebRequestFactory.CreateHttpRequest(url);

                string boundary = System.Guid.NewGuid().ToString();
                request.Credentials = new NetworkCredential(ppo.Username, ppo.Password);
                request.Headers.Add("Accept-Language", "cs,en-us;q=0.7,en;q=0.3");
                request.PreAuthenticate = true;
                request.ContentType = string.Format("multipart/form-data;boundary={0}", boundary);
                request.Headers.Add("action", "postMedia");

                //request.ContentType = "application/x-www-form-urlencoded";
                request.Method = "POST";
                request.Timeout = 20000;
                string header = string.Format("--{0}", boundary);
                string ender = "\r\n" + header + "\r\n";

                StringBuilder contents = new StringBuilder();

                contents.Append(CreateContentPartString(header, "u", ppo.Username));
                contents.Append(CreateContentPartString(header, "p", ppo.Password));
                contents.Append(CreateContentPartString(header, "k", APPLICATION_NAME));
                contents.Append(CreateContentPartString(header, "t", ppo.Message));
                contents.Append(CreateContentPartString(header, "d", ppo.Message));
                contents.Append(CreateContentPartString(header, "action", "postMedia"));

                if (!string.IsNullOrEmpty(ppo.Lat) && !string.IsNullOrEmpty(ppo.Lon))
                {
                    contents.Append(CreateContentPartString(header, "latlong", string.Format("{0},{1}",ppo.Lat,ppo.Lon) ));
                }

                string hashTags = FindHashTags(ppo.Message, ",", 32);
                if (!string.IsNullOrEmpty(hashTags))
                {
                    contents.Append(CreateContentPartString(header, "ht", hashTags));
                }


                int imageIdStartIndex = ppo.Filename.LastIndexOf('\\') + 1;
                string filename = ppo.Filename.Substring(imageIdStartIndex, ppo.Filename.Length - imageIdStartIndex);
                contents.Append(CreateContentPartMedia(header, filename));

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
                            String receiverResponse = reader.ReadToEnd();
                            //should be 0 with a following URL for the picture.

                            return receiverResponse;
                        }
                    }

                }
            }
            catch (Exception)
            {
                OnErrorOccured(new PictureServiceEventArgs(PictureServiceErrorLevel.Failed, string.Empty, API_ERROR_UPLOAD));
                return string.Empty;
            }
        }

        /// <summary>
        /// Use a imageId to retrieve and save a thumbnail to the device.
        /// </summary>
        /// <param name="imageId">Id for the image</param>
        /// <returns></returns>
        private string RetrievePicture(string pictureURL)
        {
            try
            {
                HttpWebRequest myRequest = WebRequestFactory.CreateHttpRequest(API_GET_THUMB + "&t=" + pictureURL);
                myRequest.Method = "GET";
                String pictureFileName = String.Empty;

                using (HttpWebResponse response = (HttpWebResponse)myRequest.GetResponse())
                {
                    using (dataStream = response.GetResponseStream())
                    {
                        int totalSize = 0;
                        readBuffer = new byte[PT_READ_BUFFER_SIZE];
                        pictureFileName = GetPicturePath(pictureURL);
                        int totalResponseSize = (int)response.ContentLength;

                        int responseSize = dataStream.Read(readBuffer, 0, PT_READ_BUFFER_SIZE);
                        totalSize = responseSize;
                        OnDownloadPart(new PictureServiceEventArgs(responseSize, responseSize, totalResponseSize));
                        while (responseSize > 0)
                        {
                            base.SavePicture(pictureFileName, readBuffer, responseSize);
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
            catch (Exception)
            {
                OnErrorOccured(new PictureServiceEventArgs(PictureServiceErrorLevel.Failed, string.Empty, API_ERROR_UPLOAD));
                return string.Empty;
            }
        }

        /// <summary>
        /// Use a imageId to retrieve and save a thumbnail to the device.
        /// </summary>
        /// <param name="imageId">Id for the image</param>
        /// <returns></returns>
        private string RetrievePictureAsync(string pictureURL)
        {
            try
            {
                HttpWebRequest myRequest = WebRequestFactory.CreateHttpRequest(API_GET_THUMB);
                myRequest.Method = "GET";
                String pictureFileName = String.Empty;

                using (HttpWebResponse response = (HttpWebResponse)myRequest.GetResponse())
                {
                    using (dataStream = response.GetResponseStream())
                    {
                        readBuffer = new byte[PT_READ_BUFFER_SIZE];
                        pictureFileName = GetPicturePath(pictureURL);

                        AsyncStateData state = new AsyncStateData();

                        state.dataHolder = readBuffer;
                        state.dataStream = dataStream;
                        state.fileName = pictureFileName;
                        state.totalBytesToDownload = (int)response.ContentLength;

                        dataStream.BeginRead(readBuffer, 0, PT_READ_BUFFER_SIZE, new System.AsyncCallback(DownloadPartFinished), state);

                    }
                    response.Close();
                }

                return pictureFileName;
            }
            catch (Exception)
            {            
                OnErrorOccured(new PictureServiceEventArgs(PictureServiceErrorLevel.Failed, string.Empty, API_ERROR_UPLOAD));
                return string.Empty;
            }
        }

        /// <summary>
        /// Asynchronised callback method
        /// </summary>
        /// <param name="ar"></param>
        private void DownloadPartFinished(IAsyncResult ar)
        {
            AsyncStateData state = (AsyncStateData)ar.AsyncState;

            try
            {
                int len = state.dataStream.EndRead(ar);
                
                state.bytesRead = len;
                state.totalBytesRead += state.bytesRead;

                bool saveSucces = SavePicture(state.fileName, state.dataHolder, PT_READ_BUFFER_SIZE);
                if (!saveSucces)
                {
                    OnErrorOccured(new PictureServiceEventArgs(PictureServiceErrorLevel.Failed, string.Empty, API_ERROR_DOWNLOAD));
                }

                OnDownloadPart(new PictureServiceEventArgs(state.bytesRead, state.totalBytesRead, state.totalBytesToDownload));

                if (ar.IsCompleted)
                {
                    //OnDownloadFinish(new PictureServiceEventArgs(PictureServiceErrorLevel.OK, state.fileName, ""));
                }

                dataStream.BeginRead(readBuffer, 0, PT_READ_BUFFER_SIZE, new System.AsyncCallback(DownloadPartFinished), state);
            }
            catch (Exception)
            {
                OnErrorOccured(new PictureServiceEventArgs(PictureServiceErrorLevel.Failed, string.Empty, API_ERROR_DOWNLOAD));
            }
        }


        #endregion


        #region helper functions

        private string CreateContentPartMedia(string header, string filename)
        {
            StringBuilder contents = new StringBuilder();

            contents.Append(header);
            contents.Append("\r\n");
            contents.Append(string.Format("Content-Disposition:form-data; name=\"i\";filename=\"{0}\"\r\n",filename));
            contents.Append("Content-Type: image/jpeg\r\n");
            contents.Append("\r\n");

            return contents.ToString();
        }

        protected string CreateContentPartString(string header, string dispositionName, string valueToSend)
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

        #endregion

    }
}
