using System;
using System.IO;
using System.Net;
using System.Xml;
using System.Web;
using System.Collections.Generic;
using System.Text;

namespace PockeTwit.MediaServices
{
    class FanfouPhoto : PictureServiceBase
    {
        #region private properties
        private static volatile FanfouPhoto _instance;
        private static object syncRoot = new Object();
        private const string API_ERROR_UPLOAD = "无法上传照片";
        private const string API_ERROR_DOWNLOAD = "无法下载照片";
        private const string API_UPLOAD_POST = @"http://api.fanfou.com/photos/upload.xml";
        #endregion

        #region private objects

        private System.Threading.Thread workerThread;
        private PicturePostObject workerPPO;

        #endregion

        #region constructor
        public FanfouPhoto()
        {
            API_SAVE_TO_PATH = "\\ArtCache\\fanfou.com\\";
            API_SERVICE_NAME = "Fanfou";
            API_CAN_UPLOAD_GPS = false;
            API_CAN_UPLOAD_MESSAGE = true;
            API_CAN_UPLOAD = false;

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
        public static FanfouPhoto Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (syncRoot)
                    {
                        if (_instance == null)
                        {
                            _instance = new FanfouPhoto();
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
            throw new NotImplementedException();
        }

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

                    XmlNode node = uploadResult.SelectSingleNode("hash");
                    if (node != null)
                    {
                        if (node.SelectSingleNode("error") != null)
                        {
                            string ErrorText = node.SelectSingleNode("error").Value;
                            OnErrorOccured(new PictureServiceEventArgs(PictureServiceErrorLevel.Failed, string.Empty, ErrorText));
                            return false;
                        }
                        OnErrorOccured(new PictureServiceEventArgs(PictureServiceErrorLevel.Failed, string.Empty, API_ERROR_UPLOAD));
                        return false;

                    }
                    else
                    {
                        if (uploadResult.SelectSingleNode("status/id") != null)
                            return true;
                        OnErrorOccured(new PictureServiceEventArgs(PictureServiceErrorLevel.Failed, string.Empty, API_ERROR_UPLOAD));
                        return false;
                    }
                }
                catch (Exception)
                {
                    OnErrorOccured(new PictureServiceEventArgs(PictureServiceErrorLevel.Failed, string.Empty, API_ERROR_UPLOAD));
                    return false;
                }
            }
        }

        public override bool CanFetchUrl(string URL)
        {
            if (URL.StartsWith("http://fanfou.com/photo/"))
                return true;
            else
                return false;
        }
        #endregion

        #region thread implementation
        /// <summary>
        /// Use a imageId to retrieve and save a thumbnail to the device.
        /// </summary>
        /// <param name="imageId">Id for the image</param>
        /// <returns></returns>
        private string RetrievePicture(string url)
        {
            try
            {
                HttpWebRequest myRequest = WebRequestFactory.CreateHttpRequest(url);
                myRequest.Method = "GET";
                String pictureFileName = String.Empty;

                using (HttpWebResponse response = (HttpWebResponse)myRequest.GetResponse())
                {
                    using (Stream dataStream = response.GetResponseStream())
                    {
                        int totalSize = 0;
                        int totalResponseSize = (int)response.ContentLength;
                        byte[] readBuffer = new byte[PT_READ_BUFFER_SIZE];
                        pictureFileName = GetPicturePath(url);

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
        private void ProcessDownload()
        {
            try
            {
                string picturePageURL = workerPPO.Message;

                string url = GetPictureUrl(picturePageURL);
                if (url != null)
                {
                    string resultFileName = RetrievePicture(url);

                    if (!string.IsNullOrEmpty(resultFileName))
                    {
                        OnDownloadFinish(new PictureServiceEventArgs(PictureServiceErrorLevel.Failed, resultFileName, string.Empty, url));
                    }
                }
                else
                {
                    OnErrorOccured(new PictureServiceEventArgs(PictureServiceErrorLevel.Failed, string.Empty, API_ERROR_DOWNLOAD));
                }
            }
            catch
            {
                OnErrorOccured(new PictureServiceEventArgs(PictureServiceErrorLevel.Failed, string.Empty, API_ERROR_DOWNLOAD));
            }
            workerThread = null;
        }

        private string GetPictureUrl(string url)
        {
            HttpWebRequest myRequest = WebRequestFactory.CreateHttpRequest(url);
            using (HttpWebResponse response = (HttpWebResponse)myRequest.GetResponse())
            {
                using (StreamReader reader = new StreamReader(response.GetResponseStream()))
                {
                    string rsp = reader.ReadToEnd();
                    int start = rsp.IndexOf("http://photo.fanfou.com/");
                    if (start != -1)
                    {
                        int end = rsp.IndexOf('\"', start);
                        if (end != -1)
                            return rsp.Substring(start, end - start);
                    }
                }
            }
            return null;
        }
        #endregion

        #region private method

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
                request.AllowWriteStreamBuffering = true;
                request.Credentials = new NetworkCredential(ppo.Username, ppo.Password);
                request.Headers.Add("Accept-Language", "cs,en-us;q=0.7,en;q=0.3");
                request.PreAuthenticate = true;
                request.ContentType = string.Format("multipart/form-data;boundary={0}", boundary);

                request.Method = "POST";
                request.Timeout = 20000;
                string header = string.Format("--{0}", boundary);
                string ender = "\r\n" + header + "\r\n";

                StringBuilder contents = new StringBuilder();

                //contents.Append(CreateContentPartString(header, "username", ppo.Username));
                //contents.Append(CreateContentPartString(header, "password", ppo.Password));
                contents.Append(CreateContentPartString(header, "status", ppo.Message));
                contents.Append(CreateContentPartString(header, "source", "pocketwit"));

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
            contents.Append(string.Format("content-disposition:form-data; name=\"photo\";filename=\"{0}\"\r\n", filename));
            contents.Append("Content-Type: image/jpeg\r\n");
            contents.Append("\r\n");

            return contents.ToString();
        }


        #endregion
    }
}
