using System;

using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.IO;
using System.Net;

namespace PockeTwit.Blog
{
    public class TwitPic: BlogServiceBase
    {
        private static volatile TwitPic _instance;
        private static object syncRoot = new Object();

        private const string API_UPLOAD_POST = "http://twitpic.com/api/uploadAndPost";

        private const string ERROR_UPLOAD_FAILED = "Failed to upload picture to TwitPic.";

        #region constructor
        /// <summary>
        /// Private constructor for usage in singleton.
        /// </summary>
        private TwitPic()
        {
            SetupService();
        }

        /// <summary>
        /// Singleton constructor
        /// </summary>
        /// <returns></returns>
        public static TwitPic Instance
        {
           get
           {
               if (_instance == null)
               {
                   lock (syncRoot)
                   {
                       if (_instance == null)
                       {
                           _instance = new TwitPic();
                       }
                   }
               }
               return _instance;
           }
        }

        #endregion

        protected override void SetupService()
        {
            _canSendGPS = false;
            _canSendPicture = true;
            _hasTimeLine = false;
            _maxCharacters = 140;
            _serviceName = "TwitPic";
        }

        public override void PostBlogMessage(BlogPostObject blogPostObject)
        {
            #region Argument check

            //Check for empty path, mandatory for twitpic
            if (string.IsNullOrEmpty(blogPostObject.Filename))
            {
                OnErrorOccured(new BlogServiceEventArgs(BlogServiceErrorLevel.Failed, ERROR_UPLOAD_FAILED, ""));
            }

            //Check for empty credentials
            if (string.IsNullOrEmpty(blogPostObject.Username) ||
                string.IsNullOrEmpty(blogPostObject.Password))
            {
                OnErrorOccured(new BlogServiceEventArgs(BlogServiceErrorLevel.Failed, ERROR_UPLOAD_FAILED, ""));
            }

            //Message is mandatory for a blog post
            if (string.IsNullOrEmpty(blogPostObject.Message))
            {
                OnErrorOccured(new BlogServiceEventArgs(BlogServiceErrorLevel.Failed, ERROR_UPLOAD_FAILED, ""));
            }

            #endregion

            using (FileStream file = new FileStream(blogPostObject.Filename, FileMode.Open, FileAccess.Read))
            {
                try
                {
                    //Load the picture data
                    byte[] incoming = new byte[file.Length];
                    file.Read(incoming, 0, incoming.Length);

                    blogPostObject.MediaData = incoming;
                    XmlDocument uploadResult = UploadPicture(API_UPLOAD_POST, blogPostObject);

                    if (uploadResult.SelectSingleNode("rsp").Attributes["stat"].Value == "fail")
                    {
                        string ErrorText = uploadResult.SelectSingleNode("//err").Attributes["msg"].Value;
                        OnErrorOccured(new BlogServiceEventArgs(BlogServiceErrorLevel.Failed, ErrorText, ""));
                    }
                    else
                    {
                        string URL = uploadResult.SelectSingleNode("//mediaurl").InnerText;
                        OnUploadFinish(new BlogServiceEventArgs(BlogServiceErrorLevel.OK, URL, "", blogPostObject.Filename));
                    }
                    
                }
                catch (Exception e)
                {
                    OnErrorOccured(new BlogServiceEventArgs(BlogServiceErrorLevel.Failed, "", "Failed to upload picture to TwitPic."));
                }
            }
        }


        #region private methodes

        private XmlDocument UploadPicture(string url, BlogPostObject bpo)
        {
            try
            {
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);

                string boundary = System.Guid.NewGuid().ToString();
                request.Credentials = new NetworkCredential(bpo.Username, bpo.Password);
                request.Headers.Add("Accept-Language", "cs,en-us;q=0.7,en;q=0.3");
                request.PreAuthenticate = true;
                request.ContentType = string.Format("multipart/form-data;boundary={0}", boundary);

                //request.ContentType = "application/x-www-form-urlencoded";
                request.Method = "POST";
                request.Timeout = 20000;
                string header = string.Format("--{0}", boundary);
                string ender = "\r\n" + header + "\r\n";

                StringBuilder contents = new StringBuilder();

                contents.Append(CreateContentPartString(header, "username", bpo.Username));
                contents.Append(CreateContentPartString(header, "password", bpo.Password));
                contents.Append(CreateContentPartString(header, "source", "pocketwit"));
                contents.Append(CreateContentPartString(header, "message", bpo.Message));

                contents.Append(CreateContentPartMedia(header, "media", MediaIdentifier.JPG));

                //Create the form message to send in bytes
                byte[] message = Encoding.UTF8.GetBytes(contents.ToString());
                byte[] footer = Encoding.UTF8.GetBytes(ender);
                request.ContentLength = message.Length + bpo.MediaData.Length + footer.Length;
                using (Stream requestStream = request.GetRequestStream())
                {
                    requestStream.Write(message, 0, message.Length);
                    requestStream.Write(bpo.MediaData, 0, bpo.MediaData.Length);
                    requestStream.Write(footer, 0, footer.Length);
                    requestStream.Flush();
                    requestStream.Close();

                    using (WebResponse response = request.GetResponse())
                    {
                        using (StreamReader reader = new StreamReader(response.GetResponseStream()))
                        {
                            XmlDocument responseXML = new XmlDocument();
                            responseXML.LoadXml(reader.ReadToEnd());
                            return responseXML;
                        }
                    }
                }

            }
            catch (Exception e)
            {
                //Socket exception 10054 could occur when sending large files.
                OnErrorOccured(new BlogServiceEventArgs(BlogServiceErrorLevel.Failed, "", "Unable to upload picture."));
                return null;
            }
        }

        #endregion
    }
}
