//
// Yedda Twitter C# Library (or more of an API wrapper) v0.1
// Written by Eran Sandler (eran AT yedda.com)
// http://devblog.yedda.com/index.php/twitter-c-library/
//
// The library is provided on a "AS IS" basis. Yedda is not repsonsible in any way 
// for whatever usage you do with it.
//
// Giving credit would be nice though :-)
//
// Get more cool dev information and other stuff at the Yedda Dev Blog:
// http://devblog.yedda.com
//
// Got a question about this library? About programming? C#? .NET? About anything else?
// Ask about it at Yedda (http://yedda.com) and get answers from real people.
//
using System;
using System.IO;
using System.Net;
using System.Xml;
using System.Web;
using System.Collections.Generic;
using System.Text;

namespace Yedda
{
    public static class Servers
    {
        public static Dictionary<string, Twitter.ServerURL> ServerList = new Dictionary<string, Twitter.ServerURL>();
        static Servers()
        {
            System.Net.ServicePointManager.Expect100Continue = false;   
            string appPath = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().GetName().CodeBase);
            using (System.IO.StreamReader r = new StreamReader(appPath + "\\laconicaservers.txt"))
            {
                Twitter.ServerURL tServer = new Twitter.ServerURL();
                tServer.Name = "twitter";
                tServer.URL = "http://twitter.com/";
                ServerList.Add(tServer.Name, tServer);

                Twitter.ServerURL pServer = new Twitter.ServerURL();
                pServer.Name = "ping.fm";
                pServer.URL = "http://api.ping.fm/";
                ServerList.Add(pServer.Name, pServer);

                /*
                Twitter.ServerURL bServer = new Twitter.ServerURL();
                bServer.Name = "brightkite";
                bServer.URL = "http://brightkite.com/";
                ServerList.Add(bServer.Name, bServer);
                */

                while (!r.EndOfStream)
                {
                    string URL = r.ReadLine();
                    Twitter.ServerURL Pair = new Twitter.ServerURL();
                    Pair.URL = URL + "api/";
                    Pair.Name = URL.Replace("http://","").Replace(".","").Replace("/","");
                    ServerList.Add(Pair.Name, Pair);
                }

            }
        }
    }

    public class Twitter
    {
        public static Dictionary<Twitter.Account, Dictionary<Twitter.ActionType, int>> Failures = new Dictionary<Account, Dictionary<ActionType, int>>();
        [Serializable]
        public class Account
        {
            
            public static Account fromSummary(string Summary)
            {
                string[] parts = Summary.Split(',');
                foreach (Account a in ClientSettings.AccountsList)
                {
                    if (a.UserName == parts[0] && a.ServerURL.Name == parts[1])
                    {
                        return a;
                    }
                }
                return ClientSettings.DefaultAccount;
            }

            private string _Summary;
            [System.Xml.Serialization.XmlIgnore]
            public string Summary
            {
                get
                {
                    if(string.IsNullOrEmpty(_Summary))
                    {
                        _Summary = UserName + "," + ServerURL.Name;
                    }
                    return _Summary;
                    
                }
            }

            public string UserName { get; set; }
            [System.Xml.Serialization.XmlIgnore]
            public string Password { get; set; }

            private Yedda.Twitter.TwitterServer _Server;
            public Yedda.Twitter.TwitterServer Server
            {
                set
                {
                    if (value == TwitterServer.twitter)
                    {
                        _ServerURL = new ServerURL();
                        _ServerURL.URL = "http://twitter.com/";
                        _ServerURL.Name = "twitter";
                    }
                    else
                    {
                        _ServerURL = new ServerURL();
                        _ServerURL.URL = "http://identi.ca/api/";
                        _ServerURL.Name = "identica";
                    }
                    _Server = value;
                }
                get
                {
                    return _Server;
                }
            }
            private Yedda.Twitter.ServerURL _ServerURL;
            public Yedda.Twitter.ServerURL ServerURL { 
                get
                {
                    if (_ServerURL == null)
                    {
                        if (this.Server == TwitterServer.twitter)
                        {
                            _ServerURL = new ServerURL();
                            _ServerURL.URL = "http://twitter.com/";
                            _ServerURL.Name = "twitter";
                        }
                        else
                        {
                            _ServerURL = new ServerURL();
                            _ServerURL.URL = "http://identi.ca/api/";
                            _ServerURL.Name = "identica";
                        }
                    }
                    return _ServerURL;
                }
                set
                {
                    _ServerURL = value;
                    _Server = value.ServerType;
                }
            }
            private bool _Enabled = true;
            public bool Enabled 
            {
                get
                {
                    return _Enabled;
                }
                set
                {
                    _Enabled = value;
                }
            }
            private bool _IsDefault = false;
            public bool IsDefault
            {
                get { return _IsDefault; }
                set { _IsDefault = value; }
            }

            public override bool Equals(object obj)
            {
                Account otherAccount = (Account)obj;
                return (otherAccount.UserName == this.UserName && otherAccount.ServerURL.URL== this.ServerURL.URL);
            }
            public override int GetHashCode()
            {
                string hashString = this.UserName + this.ServerURL.URL;
                return hashString.GetHashCode();
            }
            public override string ToString()
            {
                if (ServerURL.ServerType == TwitterServer.pingfm)
                {
                    return "+ ping.fm";
                }
                else
                {
                    return UserName + " (" + ServerURL.Name + ")";
                }
            }
        }

        public class ServerURL
        {
            public string URL { get; set; }
            public string Name { get; set; }
            public Yedda.Twitter.TwitterServer ServerType
            {
                get
                {
                    if (URL == "http://twitter.com/")
                    {
                        return TwitterServer.twitter;
                    }
                    else if (URL == "http://brightkite.com/")
                    {
                        return TwitterServer.brightkite;
                    }
                    else if (URL == "http://api.ping.fm/")
                    {
                        return TwitterServer.pingfm;
                    }
                    return TwitterServer.identica;
                }
            }
            public override bool Equals(object obj)
            {
                ServerURL otherURL = (ServerURL)obj;
                return this.URL.Equals(otherURL.URL);
            }
        }

        public enum TwitterServer
        {
            brightkite,
            pingfm,
            twitter,
            identica
        }

        /// <summary>
        /// The output formats supported by Twitter. Not all of them can be used with all of the functions.
        /// For more information about the output formats and the supported functions Check the 
        /// Twitter documentation at: http://groups.google.com/group/twitter-development-talk/web/api-documentation
        /// </summary>
        public enum OutputFormatType
        {
            JSON,
            XML,
            RSS,
            Atom
        }

        /// <summary>
        /// The various object types supported at Twitter.
        /// </summary>
        public enum ObjectType
        {
            Statuses,
            Account,
            Users,
            Notifications,
            Friendships
        }

        /// <summary>
        /// The various actions used at Twitter. Not all actions works on all object types.
        /// For more information about the actions types and the supported functions Check the 
        /// Twitter documentation at: http://groups.google.com/group/twitter-development-talk/web/api-documentation
        /// </summary>
        public enum ActionType
        {
            Direct_Messages,
            Search,
            Public_Timeline,
            User_Timeline,
            Friends_Timeline,
            Friends,
            Replies,
            Followers,
            Update,
            Account_Settings,
            Featured,
            Show,
            New,
            Favorites,
            Create,
            Destroy,
            Follow,
            Leave,
            Verify_Credentials,
            Update_Location,
            Conversation
        }


        private string PlaceID = null;
        private string source = "pocketwit";

        private string twitterClient = "pocketwit";
        private string twitterClientVersion = "0.9";
        private string twitterClientUrl = "http://code.google.com/p/pocketwit";
        
        public int MaxTweets = 50;

        private Account _AccountInfo = new Account();
        public Account AccountInfo 
        {
            get { return _AccountInfo; }
            set { _AccountInfo = value; }
        }

        public bool BigTimeLines
        {
            get
            {
                return AccountInfo.ServerURL.ServerType == Yedda.Twitter.TwitterServer.twitter || AccountInfo.ServerURL.ServerType== TwitterServer.identica; 
            }
        }
        public bool FavoritesWork
        {
            get
            {
                return AccountInfo.ServerURL.ServerType == Yedda.Twitter.TwitterServer.twitter; 
            }
        }
        public bool DirectMessagesWork
        {
            get
            {
                return AccountInfo.ServerURL.ServerType == Yedda.Twitter.TwitterServer.twitter || AccountInfo.ServerURL.ServerType == Yedda.Twitter.TwitterServer.identica;
            }

        }
        public bool AllowTwitPic
        {
            get
            {
                return AccountInfo.ServerURL.ServerType == Yedda.Twitter.TwitterServer.twitter;
            }

        }


        /// <summary>
        /// Source is an additional parameters that will be used to fill the "From" field.
        /// Currently you must talk to the developers of Twitter at:
        /// http://groups.google.com/group/twitter-development-talk/
        /// Otherwise, Twitter will simply ignore this parameter and set the "From" field to "web".
        /// </summary>
        public string Source
        {
            get { return source; }
            set { source = value; }
        }

        /// <summary>
        /// Sets the name of the Twitter client.
        /// According to the Twitter Fan Wiki at http://twitter.pbwiki.com/API-Docs and supported by
        /// the Twitter developers, this will be used in the future (hopefully near) to set more information
        /// in Twitter about the client posting the information as well as future usage in a clients directory.
        /// </summary>
        public string TwitterClient
        {
            get { return twitterClient; }
            set { twitterClient = value; }
        }

        /// <summary>
        /// Sets the version of the Twitter client.
        /// According to the Twitter Fan Wiki at http://twitter.pbwiki.com/API-Docs and supported by
        /// the Twitter developers, this will be used in the future (hopefully near) to set more information
        /// in Twitter about the client posting the information as well as future usage in a clients directory.
        /// </summary>
        public string TwitterClientVersion
        {
            get { return twitterClientVersion; }
            set { twitterClientVersion = value; }
        }

        /// <summary>
        /// Sets the URL of the Twitter client.
        /// Must be in the XML format documented in the "Request Headers" section at:
        /// http://twitter.pbwiki.com/API-Docs.
        /// According to the Twitter Fan Wiki at http://twitter.pbwiki.com/API-Docs and supported by
        /// the Twitter developers, this will be used in the future (hopefully near) to set more information
        /// in Twitter about the client posting the information as well as future usage in a clients directory.		
        /// </summary>
        public string TwitterClientUrl
        {
            get { return twitterClientUrl; }
            set { twitterClientUrl = value; }
        }

        protected const string TwitterBaseUrlFormat = "{3}{0}/{1}.{2}";
        protected const string TwitterSimpleURLFormat = "{1}/{0}.xml";
        protected const string TwitterFavoritesUrlFormat = "{3}/{0}/{1}/{2}.xml";
        protected const string TwitterSearchUrlFormat = "http://search.twitter.com/search.atom?{0}";
        protected const string TwitterConversationUrlFormat = "http://search.twitter.com/search/thread/{0}";

        public string GetProfileURL(string User)
        {
            return AccountInfo.ServerURL.URL.Replace("api/", "") + User;
        }

        protected string GetServerString()
        {
            return AccountInfo.ServerURL.Name;
            
        }

        protected string GetObjectTypeString(ObjectType objectType)
        {
            return objectType.ToString().ToLower();
        }

        protected string GetActionTypeString(ActionType actionType)
        {
            return actionType.ToString().ToLower();
        }

        protected string GetFormatTypeString(OutputFormatType format)
        {
            return format.ToString().ToLower();
        }


        protected string ExecuteAnonymousGetCommand(string url)
        {
            HttpWebRequest client = (HttpWebRequest)WebRequest.Create(url);
            client.Timeout = 20000;
            
            try
            {
                using (HttpWebResponse httpResponse = (HttpWebResponse)client.GetResponse())
                {
                    using (Stream stream = httpResponse.GetResponseStream())
                    {
                        using (StreamReader reader = new StreamReader(stream))
                        {
                            return reader.ReadToEnd();
                        }
                    }
                }
            }
            catch (WebException ex)
            {
                //
                // Handle HTTP 404 errors gracefully and return a null string to indicate there is no content.
                //
                PockeTwit.GlobalEventHandler.LogCommError(ex);
                if (ex.Response is HttpWebResponse)
                {
                    try
                    {
                        if ((ex.Response as HttpWebResponse).StatusCode == HttpStatusCode.NotFound)
                        {
                            return null;
                        }

                        HttpWebResponse errorResponse = (HttpWebResponse)ex.Response;
                        if (errorResponse.StatusCode == HttpStatusCode.Unauthorized)
                        {
                            return null;
                        }
                        string ErrorText;
                        using (Stream stream = errorResponse.GetResponseStream())
                        {
                            using (StreamReader reader = new StreamReader(stream))
                            {
                                ErrorText = reader.ReadToEnd();
                                XmlDocument doc = new XmlDocument();
                                doc.LoadXml(ErrorText);

                                if (doc.SelectSingleNode("//error").InnerText.StartsWith("Rate limit exceeded"))
                                {
                                    DateTime NewTime = GetTimeOutTime();
                                    errorResponse.Close();
                                    PockeTwit.GlobalEventHandler.CallShowErrorMessage("Timeout until " + NewTime.ToString());
                                    throw new Exception("Timeout until " + NewTime.ToString());
                                }

                            }
                        }
                    }
                    catch
                    {
                        ex.Response.Close();

                    }


                }

            }
            return null;
        }

        protected string ExecuteGetCommand(string url)
        {
            HttpWebRequest client = (HttpWebRequest)WebRequest.Create(url);
            client.Timeout = 20000;
            if (!string.IsNullOrEmpty(AccountInfo.UserName) &&
                !string.IsNullOrEmpty(AccountInfo.Password))
            {
                client.Credentials = new NetworkCredential(AccountInfo.UserName, AccountInfo.Password);
            }
            client.PreAuthenticate = true;

            try
            {
                using (HttpWebResponse httpResponse = (HttpWebResponse)client.GetResponse())
                {
                    using (Stream stream = httpResponse.GetResponseStream())
                    {
                        using (StreamReader reader = new StreamReader(stream))
                        {
                            return reader.ReadToEnd();
                        }
                    }
                }
            }
            catch (WebException ex)
            {
                PockeTwit.GlobalEventHandler.LogCommError(ex);
                //
                // Handle HTTP 404 errors gracefully and return a null string to indicate there is no content.
                //
                if (ex.Response is HttpWebResponse)
                {
                    try
                    {
                        if ((ex.Response as HttpWebResponse).StatusCode == HttpStatusCode.NotFound)
                        {
                            return null;
                        }

                        HttpWebResponse errorResponse = (HttpWebResponse)ex.Response;
                        if (errorResponse.StatusCode == HttpStatusCode.Unauthorized)
                        {
                            return null;
                        }
                        string ErrorText;
                        using (Stream stream = errorResponse.GetResponseStream())
                        {
                            using (StreamReader reader = new StreamReader(stream))
                            {
                                ErrorText = reader.ReadToEnd();
                                XmlDocument doc = new XmlDocument();
                                doc.LoadXml(ErrorText);

                                if (doc.SelectSingleNode("//error").InnerText.StartsWith("Rate limit exceeded"))
                                {
                                    DateTime NewTime = GetTimeOutTime();
                                    errorResponse.Close();
                                    PockeTwit.GlobalEventHandler.CallShowErrorMessage("Timeout until " + NewTime.ToString());
                                    throw new Exception("Timeout until " + NewTime.ToString());
                                }
                                else
                                {
                                    Exception TwitterError = new Exception(doc.SelectSingleNode("//error").InnerText);
                                    PockeTwit.GlobalEventHandler.LogCommError(TwitterError);
                                    PockeTwit.GlobalEventHandler.CallShowErrorMessage(TwitterError.Message);
                                    throw TwitterError;
                                }
                            }
                        }
                    }
                    catch 
                    {
                        ex.Response.Close();
                    
                    }

                    
                }
                
            }
            return null;
        }

        public DateTime GetTimeOutTime()
        {
            string URL = "http://twitter.com/account/rate_limit_status.xml";
            string Response = ExecuteGetCommand(URL);
            XmlDocument doc = new XmlDocument();
            doc.LoadXml(Response);
            string strTime = doc.SelectSingleNode("//reset-time").InnerText;
            DateTime t = DateTime.Parse(strTime);
            return t;

        }

        /// <summary>
        /// Executes an HTTP POST command and retrives the information.		
        /// This function will automatically include a "source" parameter if the "Source" property is set.
        /// </summary>
        /// <param name="url">The URL to perform the POST operation</param>
        /// <param name="userName">The username to use with the request</param>
        /// <param name="password">The password to use with the request</param>
        /// <param name="data">The data to post</param> 
        /// <returns>The response of the request, or null if we got 404 or nothing.</returns>
        protected string ExecutePostCommand(string url, string data)
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            
            if (!string.IsNullOrEmpty(AccountInfo.UserName) && !string.IsNullOrEmpty(AccountInfo.Password))
            {
                request.Credentials = new NetworkCredential(AccountInfo.UserName, AccountInfo.Password);
                request.PreAuthenticate = true;
                request.ContentType = "application/x-www-form-urlencoded";
                request.Method = "POST";
                request.Timeout = 20000;
                
                request.Headers.Add("Authorization", "Basic " + Convert.ToBase64String(Encoding.ASCII.GetBytes(AccountInfo.UserName + ":" + AccountInfo.Password))); 

                if (!string.IsNullOrEmpty(TwitterClient))
                {
                    request.Headers.Add("X-Twitter-Client", TwitterClient);
                }

                if (!string.IsNullOrEmpty(TwitterClientVersion))
                {
                    request.Headers.Add("X-Twitter-Version", TwitterClientVersion);
                }

                if (!string.IsNullOrEmpty(TwitterClientUrl))
                {
                    request.Headers.Add("X-Twitter-URL", TwitterClientUrl);
                }


                if (!string.IsNullOrEmpty(Source))
                {
                    data += "&source=" + HttpUtility.UrlEncode(Source);
                }

                byte[] bytes = Encoding.UTF8.GetBytes(data);
                request.ContentLength = bytes.Length;
                try
                {
                    using (Stream requestStream = request.GetRequestStream())
                    {
                        requestStream.Write(bytes, 0, bytes.Length);
                        requestStream.Flush();
                    }
                }
                catch(Exception ex)
                {
                    PockeTwit.GlobalEventHandler.LogCommError(ex);
                    System.Diagnostics.Debug.WriteLine(ex.Message);
                }
                try
                {
                    using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
                    {
                        using (StreamReader reader = new StreamReader(response.GetResponseStream()))
                        {
                            return reader.ReadToEnd();
                        }
                    }
                }
                catch (WebException ex)
                {
                    //
                    // Handle HTTP 404 errors gracefully and return a null string to indicate there is no content.
                    //
                    if (ex.Response is HttpWebResponse)
                    {
                        PockeTwit.GlobalEventHandler.LogCommError(ex);
                        if ((ex.Response as HttpWebResponse).StatusCode == HttpStatusCode.NotFound)
                        {
                            return null;
                        }
                        try
                        {
                            HttpWebResponse errorResponse = (HttpWebResponse)ex.Response;
                            string ErrorText;
                            using (Stream stream = errorResponse.GetResponseStream())
                            {
                                using (StreamReader reader = new StreamReader(stream))
                                {
                                    ErrorText = reader.ReadToEnd();
                                    XmlDocument doc = new XmlDocument();
                                    doc.LoadXml(ErrorText);

                                    if (doc.SelectSingleNode("//error").InnerText.StartsWith("Rate limit exceeded"))
                                    {
                                        DateTime NewTime = GetTimeOutTime();
                                        PockeTwit.GlobalEventHandler.CallShowErrorMessage("Timeout until " + NewTime.ToString());
                                        throw new Exception("Timeout until " + NewTime.ToString());
                                    }

                                }
                            }
                        }
                        catch { }
                    }
                }
                catch { }
            }

            return null;
        }

        #region Public_Timeline

        public string GetPublicTimeline(OutputFormatType format)
        {
            string url = string.Format(TwitterBaseUrlFormat, GetObjectTypeString(ObjectType.Statuses), GetActionTypeString(ActionType.Public_Timeline), GetFormatTypeString(format),AccountInfo.ServerURL.URL);
            return ExecuteGetCommand(url);
        }

        public string GetPublicTimelineAsJSON()
        {
            return GetPublicTimeline(OutputFormatType.JSON);
        }

        public XmlDocument GetPublicTimelineAsXML(OutputFormatType format)
        {
            if (format == OutputFormatType.JSON)
            {
                throw new ArgumentException("GetPublicTimelineAsXml supports only XML based formats (XML, RSS, Atom)", "format");
            }

            string output = GetPublicTimeline(format);
            if (!string.IsNullOrEmpty(output))
            {
                XmlDocument xmlDocument = new XmlDocument();
                xmlDocument.LoadXml(output);

                return xmlDocument;
            }

            return null;
        }

        public XmlDocument GetPublicTimelineAsXML()
        {
            return GetPublicTimelineAsXML(OutputFormatType.XML);
        }

        public XmlDocument GetPublicTimelineAsRSS()
        {
            return GetPublicTimelineAsXML(OutputFormatType.RSS);
        }

        public XmlDocument GetPublicTimelineAsAtom()
        {
            return GetPublicTimelineAsXML(OutputFormatType.Atom);
        }

        #endregion

        #region User_Timeline

        public string GetUserTimeline(string IDorScreenName, OutputFormatType format)
        {
            string url = null;
            if (string.IsNullOrEmpty(IDorScreenName))
            {
                url = string.Format(TwitterBaseUrlFormat, GetObjectTypeString(ObjectType.Statuses), GetActionTypeString(ActionType.User_Timeline), GetFormatTypeString(format), AccountInfo.ServerURL.URL);
            }
            else
            {
                url = string.Format(TwitterBaseUrlFormat, GetObjectTypeString(ObjectType.Statuses), GetActionTypeString(ActionType.User_Timeline) + "/" + IDorScreenName, GetFormatTypeString(format), AccountInfo.ServerURL.URL);
            }

            return ExecuteGetCommand(url);
        }

        public string GetUserTimeline(OutputFormatType format)
        {
            return GetUserTimeline(null, format);
        }

        public string GetUserTimelineAsJSON()
        {
            return GetUserTimeline(OutputFormatType.JSON);
        }

        public string GetUserTimelineAsJSON(string IDorScreenName)
        {
            return GetUserTimeline(IDorScreenName, OutputFormatType.JSON);
        }

        public XmlDocument GetUserTimelineAsXML(string IDorScreenName, OutputFormatType format)
        {
            if (format == OutputFormatType.JSON)
            {
                throw new ArgumentException("GetUserTimelineAsXML supports only XML based formats (XML, RSS, Atom)", "format");
            }

            string output = GetUserTimeline(IDorScreenName, format);
            if (!string.IsNullOrEmpty(output))
            {
                XmlDocument xmlDocument = new XmlDocument();
                xmlDocument.LoadXml(output);

                return xmlDocument;
            }

            return null;
        }

        public XmlDocument GetUserTimelineAsXML(OutputFormatType format)
        {
            return GetUserTimelineAsXML(null, format);
        }

        public XmlDocument GetUserTimelineAsXML(string IDorScreenName)
        {
            return GetUserTimelineAsXML(IDorScreenName, OutputFormatType.XML);
        }

        public XmlDocument GetUserTimelineAsXML()
        {
            return GetUserTimelineAsXML(null);
        }

        public XmlDocument GetUserTimelineAsRSS(string IDorScreenName)
        {
            return GetUserTimelineAsXML(IDorScreenName, OutputFormatType.RSS);
        }

        public XmlDocument GetUserTimelineAsRSS()
        {
            return GetUserTimelineAsXML(OutputFormatType.RSS);
        }

        public XmlDocument GetUserTimelineAsAtom(string IDorScreenName)
        {
            return GetUserTimelineAsXML(IDorScreenName, OutputFormatType.Atom);
        }

        public XmlDocument GetUserTimelineAsAtom()
        {
            return GetUserTimelineAsXML(OutputFormatType.Atom);
        }
        #endregion

        #region Direct_Messages
        public string GetDirectTimeLineSince(string SinceID)
        {
            string url = string.Format(TwitterSimpleURLFormat, GetActionTypeString(ActionType.Direct_Messages),  AccountInfo.ServerURL.URL) + "?since_id=" + SinceID;
            return ExecuteGetCommand(url);
        }
        #endregion
        #region Friends_Timeline
        public string GetFriendsTimeLineMax(OutputFormatType format)
        {
            string url = string.Format(TwitterBaseUrlFormat, GetObjectTypeString(ObjectType.Statuses), GetActionTypeString(ActionType.Friends_Timeline), GetFormatTypeString(format), AccountInfo.ServerURL.URL)+"?count="+MaxTweets;
            return ExecuteGetCommand(url);
        }
        public string GetFriendsTimeLineSince(OutputFormatType format, string SinceID)
        {
            string url = string.Format(TwitterBaseUrlFormat, GetObjectTypeString(ObjectType.Statuses), GetActionTypeString(ActionType.Friends_Timeline), GetFormatTypeString(format), AccountInfo.ServerURL.URL) + "?since_id=" + SinceID + "&count=" + ClientSettings.MaxTweets;
            return ExecuteGetCommand(url);
        }

        public string GetFriendsTimeline(OutputFormatType format)
        {
            if (this.AccountInfo.ServerURL.ServerType == TwitterServer.brightkite)
            {
                string url = "http://brightkite.com/me/friendstream.xml";
                return ExecuteGetCommand(url);
            }
            else
            {
                string url = string.Format(TwitterBaseUrlFormat, GetObjectTypeString(ObjectType.Statuses), GetActionTypeString(ActionType.Friends_Timeline), GetFormatTypeString(format), AccountInfo.ServerURL.URL);
                return ExecuteGetCommand(url);
            }
        }

        public string GetFriendsTimelineAsJSON()
        {
            return GetFriendsTimeline(OutputFormatType.JSON);
        }

        public XmlDocument GetFriendsTimelineAsXML(OutputFormatType format)
        {
            if (format == OutputFormatType.JSON)
            {
                throw new ArgumentException("GetFriendsTimelineAsXML supports only XML based formats (XML, RSS, Atom)", "format");
            }

            string output = GetFriendsTimeline(format);
            if (!string.IsNullOrEmpty(output))
            {
                XmlDocument xmlDocument = new XmlDocument();
                xmlDocument.LoadXml(output);

                return xmlDocument;
            }

            return null;
        }

        public XmlDocument GetFriendsTimelineAsXML()
        {
            return GetFriendsTimelineAsXML(OutputFormatType.XML);
        }

        public XmlDocument GetFriendsTimelineAsRSS()
        {
            return GetFriendsTimelineAsXML(OutputFormatType.RSS);
        }

        public XmlDocument GetFriendsTimelineAsAtom()
        {
            return GetFriendsTimelineAsXML(OutputFormatType.Atom);
        }

        #endregion

        #region Replies
        public string GetRepliesTimeLineSince(OutputFormatType format, string SinceID)
        {
            string url = string.Format(TwitterBaseUrlFormat, GetObjectTypeString(ObjectType.Statuses), GetActionTypeString(ActionType.Replies), GetFormatTypeString(format), AccountInfo.ServerURL.URL) + "?since_id=" + SinceID;
            return ExecuteGetCommand(url);
        }
        public string GetRepliesTimeLine(OutputFormatType format)
        {
            string url = string.Format(TwitterBaseUrlFormat, GetObjectTypeString(ObjectType.Statuses), GetActionTypeString(ActionType.Replies), GetFormatTypeString(format), AccountInfo.ServerURL.URL);
            return ExecuteGetCommand(url);
        }

        #endregion

        #region Friends

        public string GetFriends(OutputFormatType format)
        {
            if (format != OutputFormatType.JSON && format != OutputFormatType.XML)
            {
                throw new ArgumentException("GetFriends support only XML and JSON output format", "format");
            }

            string url = string.Format(TwitterBaseUrlFormat, GetObjectTypeString(ObjectType.Statuses), GetActionTypeString(ActionType.Friends), GetFormatTypeString(format), AccountInfo.ServerURL.URL);
            return ExecutePostCommand(url, null);
        }

        public string GetFriends(string IDorScreenName, OutputFormatType format)
        {
            if (format != OutputFormatType.JSON && format != OutputFormatType.XML)
            {
                throw new ArgumentException("GetFriends support only XML and JSON output format", "format");
            }

            string url = null;
            if (string.IsNullOrEmpty(IDorScreenName))
            {
                url = string.Format(TwitterBaseUrlFormat, GetObjectTypeString(ObjectType.Statuses), GetActionTypeString(ActionType.Friends), GetFormatTypeString(format), AccountInfo.ServerURL.URL);
            }
            else
            {
                url = string.Format(TwitterBaseUrlFormat, GetObjectTypeString(ObjectType.Statuses), GetActionTypeString(ActionType.Friends) + "/" + IDorScreenName, GetFormatTypeString(format), AccountInfo.ServerURL.URL);
            }

            return ExecuteGetCommand(url);
        }

        public string GetFriendsAsJSON(string IDorScreenName)
        {
            return GetFriends(IDorScreenName, OutputFormatType.JSON);
        }

        public string GetFriendsAsJSON()
        {
            return GetFriendsAsJSON(null);
        }

        public XmlDocument GetFriendsAsXML(string IDorScreenName)
        {
            string output = GetFriends(IDorScreenName, OutputFormatType.XML);
            if (!string.IsNullOrEmpty(output))
            {
                XmlDocument xmlDocument = new XmlDocument();
                xmlDocument.LoadXml(output);

                return xmlDocument;
            }

            return null;
        }

        public XmlDocument GetFriendsAsXML()
        {
            return GetFriendsAsXML(null);
        }

        #endregion

        #region Followers

        public string GetFollowers(OutputFormatType format)
        {
            if (format != OutputFormatType.JSON && format != OutputFormatType.XML)
            {
                throw new ArgumentException("GetFollowers supports only XML and JSON output format", "format");
            }

            string url = string.Format(TwitterBaseUrlFormat, GetObjectTypeString(ObjectType.Statuses), GetActionTypeString(ActionType.Followers), GetFormatTypeString(format), AccountInfo.ServerURL.URL);
            return ExecuteGetCommand(url);
        }

        public string GetFollowersAsJSON()
        {
            return GetFollowers(OutputFormatType.JSON);
        }

        public XmlDocument GetFollowersAsXML()
        {
            string output = GetFollowers(OutputFormatType.XML);
            if (!string.IsNullOrEmpty(output))
            {
                XmlDocument xmlDocument = new XmlDocument();
                xmlDocument.LoadXml(output);

                return xmlDocument;
            }

            return null;
        }

        #endregion

        #region Update
        public string Update(string status, OutputFormatType format)
        {
            return Update(status, null, format);
        }

        public string Update(string status, string in_reply_to_status_id, OutputFormatType format)
        {
            
            if (this.AccountInfo.ServerURL.ServerType == TwitterServer.pingfm)
            {
                string url = "http://api.ping.fm/v1/user.post";
                //string data = string.Format("user_app_key={0}&api_key={1}&post_method=microblog&body={2}", this.AccountInfo.UserName,this.AccountInfo.Password,HttpUtility.UrlEncode(status));
                string data = string.Format("user_app_key={0}&api_key={1}&post_method=default&body={2}", this.AccountInfo.UserName, this.AccountInfo.Password, HttpUtility.UrlEncode(status));
                
                return ExecutePostCommand(url, data);
            }
            else if (this.AccountInfo.ServerURL.ServerType == TwitterServer.brightkite)
            {
                if (this.PlaceID != null)
                {
                    string url = string.Format("http://brightkite.com/places/{0}/notes", this.PlaceID);
                    string data = string.Format("note[body]={0}", HttpUtility.UrlEncode(status));
                    return ExecutePostCommand(url, data);
                }
                else
                {
                    return null;
                }
            }
            else
            {
                if (format != OutputFormatType.JSON && format != OutputFormatType.XML)
                {
                    throw new ArgumentException("Update support only XML and JSON output format", "format");
                }

                string url = string.Format(TwitterBaseUrlFormat, GetObjectTypeString(ObjectType.Statuses), GetActionTypeString(ActionType.Update), GetFormatTypeString(format), AccountInfo.ServerURL.URL);
                string data = string.Format("status={0}", HttpUtility.UrlEncode(status));
                if (!string.IsNullOrEmpty(in_reply_to_status_id))
                {
                    data = data + "&in_reply_to_status_id=" + in_reply_to_status_id;
                }
                return ExecutePostCommand(url, data);
            }
        }

        public string UpdateAsJSON(string text)
        {
            return Update(text, OutputFormatType.JSON);
        }

        public XmlDocument UpdateAsXML(string text)
        {
            string output = Update(text, OutputFormatType.XML);
            if (!string.IsNullOrEmpty(output))
            {
                XmlDocument xmlDocument = new XmlDocument();
                xmlDocument.LoadXml(output);

                return xmlDocument;
            }

            return null;
        }

        #endregion

        #region Featured

        public string GetFeatured(OutputFormatType format)
        {
            if (format != OutputFormatType.JSON && format != OutputFormatType.XML)
            {
                throw new ArgumentException("GetFeatured supports only XML and JSON output format", "format");
            }

            string url = string.Format(TwitterBaseUrlFormat, GetObjectTypeString(ObjectType.Statuses), GetActionTypeString(ActionType.Featured), GetFormatTypeString(format), AccountInfo.ServerURL.URL);
            return ExecuteGetCommand(url);
        }

        public string GetFeaturedAsJSON()
        {
            return GetFeatured(OutputFormatType.JSON);
        }

        public XmlDocument GetFeaturedAsXML()
        {
            string output = GetFeatured(OutputFormatType.XML);
            if (!string.IsNullOrEmpty(output))
            {
                XmlDocument xmlDocument = new XmlDocument();
                xmlDocument.LoadXml(output);

                return xmlDocument;
            }

            return null;
        }

        #endregion

        #region Show

        public string ShowSingleStatus(string statusID)
        {
            string url = string.Format(TwitterBaseUrlFormat, GetObjectTypeString(ObjectType.Statuses), GetActionTypeString(ActionType.Show) + "/" + statusID, GetFormatTypeString(OutputFormatType.XML), AccountInfo.ServerURL.URL);
            return ExecuteAnonymousGetCommand(url);
        }

        public string Show(string IDorScreenName, OutputFormatType format)
        {
            if (format != OutputFormatType.JSON && format != OutputFormatType.XML)
            {
                throw new ArgumentException("Show supports only XML and JSON output format", "format");
            }

            string url = string.Format(TwitterBaseUrlFormat, GetObjectTypeString(ObjectType.Users), GetActionTypeString(ActionType.Show) + "/" + IDorScreenName, GetFormatTypeString(format), AccountInfo.ServerURL.URL);
            return ExecuteAnonymousGetCommand(url);
        }

        public string ShowAsJSON(string IDorScreenName)
        {
            return Show(IDorScreenName, OutputFormatType.JSON);
        }

        public XmlDocument ShowAsXML(string IDorScreenName)
        {
            string output = Show(IDorScreenName, OutputFormatType.XML);
            if (!string.IsNullOrEmpty(output))
            {
                XmlDocument xmlDocument = new XmlDocument();
                xmlDocument.LoadXml(output);

                return xmlDocument;
            }

            return null;
        }

        public string ShowStatus(string ID)
        {
            string url = string.Format(TwitterBaseUrlFormat, GetObjectTypeString(ObjectType.Statuses), GetActionTypeString(ActionType.Show) + "/" + ID, GetFormatTypeString(OutputFormatType.XML), AccountInfo.ServerURL.URL);
            return ExecuteAnonymousGetCommand(url);
        }

        #endregion

        #region Favorites
        public string SetFavorite(string IDofMessage)
        {
            string url = string.Format(TwitterFavoritesUrlFormat, GetActionTypeString(ActionType.Favorites), GetActionTypeString(ActionType.Create), IDofMessage, AccountInfo.ServerURL.URL);
            return ExecutePostCommand(url, "");
        }
        public string DestroyFavorite(string IDofMessage)
        {
            string url = string.Format(TwitterFavoritesUrlFormat, GetActionTypeString(ActionType.Favorites), GetActionTypeString(ActionType.Destroy), IDofMessage,AccountInfo.ServerURL.URL);
            return ExecutePostCommand(url, "");
        }
        public string GetFavorites()
        {
            string url = string.Format(TwitterSimpleURLFormat, GetActionTypeString(ActionType.Favorites),AccountInfo.ServerURL.URL);
            return ExecuteGetCommand(url);
        }
        #endregion

        #region Follow
        public string FollowUser(string IDofUserToFollow)
        {
            string url = string.Format(TwitterFavoritesUrlFormat, GetObjectTypeString(ObjectType.Friendships), GetActionTypeString(ActionType.Create), IDofUserToFollow,AccountInfo.ServerURL.URL);
            return ExecutePostCommand(url, "");
        }
        public string StopFollowingUser(string IDofUserToFollow)
        {
            
            string url = string.Format(TwitterFavoritesUrlFormat, GetObjectTypeString(ObjectType.Friendships), GetActionTypeString(ActionType.Destroy), IDofUserToFollow,AccountInfo.ServerURL.URL);
            return ExecutePostCommand(url, "");
        }
        #endregion

        #region Verify
        public bool Verify()
        {
            string url;
            if (this.AccountInfo.ServerURL.ServerType == TwitterServer.pingfm)
            {
                return PingValidate();
            }
            else if (this.AccountInfo.ServerURL.ServerType == TwitterServer.brightkite)
            {
                return true;
                /*  For later development
                url = "http://brightkite.com/me";
                return !string.IsNullOrEmpty(ExecutePostCommand(url, ""));
                 */
            }
            else
            {
                url = string.Format(TwitterBaseUrlFormat, GetObjectTypeString(ObjectType.Account), GetActionTypeString(ActionType.Verify_Credentials), GetFormatTypeString(OutputFormatType.XML), AccountInfo.ServerURL.URL);
                string Response = ExecuteGetCommand(url);
                return (!string.IsNullOrEmpty(Response));
            }
        }

        private bool PingValidate()
        {
            //First, if mobile key use it to get real key
            string url;
            if (this.AccountInfo.UserName.Length < 6)
            {
                url = "http://api.ping.fm/v1/user.key";
                try
                {
                    string KeyResponse = ExecutePostCommand(url, string.Format("mobile_key={0}&api_key={1}", this.AccountInfo.UserName, this.AccountInfo.Password));
                    XmlDocument d = new XmlDocument();
                    d.LoadXml(KeyResponse);
                    this.AccountInfo.UserName = d.SelectSingleNode("//key").InnerText;
                }
                catch
                {
                    return false;
                }
            }

            url = "http://api.ping.fm/v1/user.validate";
            string Response = ExecutePostCommand(url, string.Format("user_app_key={0}&api_key={1}", this.AccountInfo.UserName, this.AccountInfo.Password));
            if (!string.IsNullOrEmpty(Response))
            {
                return Response.IndexOf("<rsp status=\"OK\">") > 0;
            }
            return false;
        }
        #endregion

        #region Search
        public string SearchFor(string textToSearch)
        {
            string url = string.Format(TwitterSearchUrlFormat, textToSearch);
            return ExecuteGetCommand(url);
        }
        #endregion

        #region Location
        public string SetLocation(string Location)
        {
            if (AccountInfo.ServerURL.ServerType == TwitterServer.twitter || AccountInfo.ServerURL.ServerType == TwitterServer.identica)
            {
                string url = string.Format(TwitterBaseUrlFormat, GetObjectTypeString(ObjectType.Account), GetActionTypeString(ActionType.Update_Location), GetFormatTypeString(OutputFormatType.XML), AccountInfo.ServerURL.URL);
                string data = string.Format("location={0}", HttpUtility.UrlEncode(Location));

                return ExecutePostCommand(url, data);
            }
            else
            {
                string url = string.Format("http://brightkite.com/places/search.xml?q={0}", HttpUtility.UrlEncode(Location));
                string placeRet = ExecuteGetCommand(url);
                try
                {
                    XmlDocument placeDoc = new XmlDocument();
                    placeDoc.LoadXml(placeRet);
                    this.PlaceID = placeDoc.SelectSingleNode("//id").InnerText;
                    return "";
                }
                catch
                {
                }
            }
            return "";
        }
        #endregion

        public string GetThread(string threadID)
        {
            if (AccountInfo.ServerURL.ServerType != TwitterServer.twitter)
            {
                return null;
            }
            string url = string.Format(TwitterConversationUrlFormat, threadID);
            return ExecuteGetCommand(url);
        }

        public string GetFriendsIDs()
        {
            string url = "http://twitter.com/friends/ids.xml";
            return ExecuteGetCommand(url);
        }
    }
}
