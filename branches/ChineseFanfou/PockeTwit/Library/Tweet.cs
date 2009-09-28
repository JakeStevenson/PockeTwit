using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using PockeTwit.FingerUI;
using Yedda;
using System.Windows.Forms;

namespace PockeTwit.Library
{
    [Flags]
    public enum StatusTypes
    {
        Normal = 0x1,
        Reply = 0x2,
        Direct = 0x4,
        SearchResult = 0x8
    }

    [Serializable]
    public class status : IComparable
    {
        private static readonly IFormatProvider format = new CultureInfo(1033);
        private static XmlSerializer statusSerializer = new XmlSerializer(typeof (status[]));
        private static XmlSerializer singleSerializer = new XmlSerializer(typeof (status));

        public StatusTypes TypeofMessage = StatusTypes.Normal;

        #region Properties (7) 

        [XmlIgnore] public bool Clipped = false;

        [XmlIgnore]
        public List<string> SplitLines { get; set; }

        [XmlIgnore]
        public List<StatusItem.Clickable> Clickables { get; set; }
        [XmlIgnore]
        public List<int> ClickablesToDo { get; set; }

        public StatusTypes type { get; set; }

        public string in_reply_to_status_id { get; set; }

        public string favorited { get; set; }

        [XmlIgnore]
        public string SearchTerm { get; set; }

        [XmlIgnore]
        public string TimeStamp
        {
            get
            {
                TimeSpan Difference = DateTime.Now - createdAt;
                double Diff;
                string Span;
                if (Difference.TotalDays > 1)
                {
                    Diff = Math.Round(Difference.TotalDays);
                    if (Diff > 1)
                    {
                        Span = PockeTwit.Localization.XmlBasedResourceManager.GetString("days");
                    }
                    else
                    {
                        Span = PockeTwit.Localization.XmlBasedResourceManager.GetString("day");
                    }
                }
                else if (Difference.TotalHours > 1)
                {
                    Diff = Math.Round(Difference.TotalHours);
                    if (Diff > 1)
                    {
                        Span = PockeTwit.Localization.XmlBasedResourceManager.GetString("hours");
                    }
                    else
                    {
                        Span = PockeTwit.Localization.XmlBasedResourceManager.GetString("hour");
                    }
                }
                else
                {
                    Diff = Math.Round(Difference.TotalMinutes);
                    Span = PockeTwit.Localization.XmlBasedResourceManager.GetString("min");
                }
                return String.Format(PockeTwit.Localization.XmlBasedResourceManager.GetString("about {0} {1} ago."), Diff.ToString(), Span);
            }
        }

        public DateTime createdAt;
        private string _created_at;

        public string created_at
        {
            get { return _created_at; }
            set
            {
                _created_at = value;
                try
                {
                    createdAt = DateTime.ParseExact(created_at, "ddd MMM dd H:mm:ss K yyyy", format,
                                                    DateTimeStyles.AssumeUniversal);
                }
                catch
                {
                    //Search results come in a different format :(
                    try
                    {
                        createdAt = DateTime.Parse(created_at, format, DateTimeStyles.AssumeUniversal);
                    }
                    catch
                    {
                        createdAt = new DateTime(2000, 1, 1);
                    }
                }
            }
        }

        public string id { get; set; }

        public string source { get; set; }
        //public bool truncated { get; set; }
        //public string in_reply_to_status_id { get; set; }
        public string in_reply_to_user_id { get; set; }
        public bool isDirect { get; set; }

        public string location { get; set; }
        public string text { get; set; }

        [XmlIgnore]
        public string DisplayText
        {
            get
            {
                if (ClientSettings.IncludeUserName)
                {
                    return user.screen_name + ": " + text;
                }
                else
                {
                    return text;
                }
            }
        }

        public User user { get; set; }


        public string AccountSummary
        {
            get
            {
                if (_Account != null)
                {
                    return _Account.Summary;
                }
                return ClientSettings.DefaultAccount.Summary;
            }
            set { _Account = Twitter.Account.fromSummary(value); }
        }

        [XmlIgnore] private Twitter.Account _Account;

        [XmlIgnore]
        public Twitter.Account Account
        {
            get
            {
                if (_Account == null)
                {
                    _Account = ClientSettings.DefaultAccount;
                }
                return _Account;
            }
            set { _Account = value; }
        }

        #endregion Properties 

        #region Methods (3) 

        // Public Methods (3) 

        public static status[] Deserialize(string response)
        {
            return Deserialize(response, null, StatusTypes.Normal);
        }

        public static status[] Deserialize(string response, Twitter.Account Account)
        {
            return Deserialize(response, Account, StatusTypes.Normal);
        }

        public static status DeserializeSingle(string response, Twitter.Account Account)
        {
            status s = null;
            if (string.IsNullOrEmpty(response))
            {
                return new status();
            }
            if (Account == null ||
                (Account.ServerURL.ServerType != Twitter.TwitterServer.brightkite &&
                 Account.ServerURL.ServerType != Twitter.TwitterServer.pingfm))
            {
                using (var r = new StringReader(response))
                {
                    s = (status) singleSerializer.Deserialize(r);
                }
                if (s.text == null)
                {
                    throw new Exception("Unable to deserialize the response");
                }
            }
            return s;
        }

        public static status[] Deserialize(string response, Twitter.Account Account, StatusTypes TypeOfMessage)
        {
            status[] statuses = null;

            try
            {
                if (string.IsNullOrEmpty(response))
                {
                    statuses = new status[0];
                }
                else
                {
                    if (Account == null || Account.ServerURL.ServerType != Twitter.TwitterServer.brightkite)
                    {
                        using (var r = new StringReader(response))
                        {
                            statuses = (status[]) statusSerializer.Deserialize(r);
                        }
                    }
                    else if (Account.ServerURL.ServerType == Twitter.TwitterServer.brightkite)
                    {
                        statuses = FromBrightKite(response);
                    }
                }
                if (Account != null)
                {
                    foreach (status stat in statuses)
                    {
                        stat.Account = Account;
                        stat.TypeofMessage = TypeOfMessage;
                    }
                }
            }
            catch{}
            return statuses;
        }

        public static status[] DeserializeArrayFromJSON(string response, Twitter.Account Accournt, StatusTypes TypeOfMessage)
        {
            var ret = new List<status>();
            var List = (System.Collections.Hashtable)JSON.JsonDecode(response);
            if (List == null) { return null; }
            var results = (System.Collections.ArrayList)List["results"];
            foreach (var result in results)
            {
                var resultHash = (System.Collections.Hashtable) result;
                ret.Add(DeserializeSingleJSONStatus(resultHash, TypeOfMessage));   
            }
            return ret.ToArray();
            
        }
        private static status DeserializeSingleJSONStatus(System.Collections.Hashtable jsonTable, StatusTypes TypeOfMessage)
        {
            
            var u = new User
                        {
                            id = jsonTable["from_user_id"].ToString(),
                            needsFetching = true,
                            screen_name = (string) jsonTable["from_user"],
                            profile_image_url = (string) jsonTable["profile_image_url"],
                            //Search results don't contain this info!
                            name = "",
                            description = ""
                        };

            var ret = new status
                          {
                              user = u,
                              text = (string) jsonTable["text"],
                              id = jsonTable["id"].ToString(),
                              created_at = (string) jsonTable["created_at"],
                              source = (string) jsonTable["source"],
                              TypeofMessage = TypeOfMessage,
                              //Search results don't contain this info!
                              in_reply_to_status_id = "",
                              favorited = ""
                          };
            
            return ret;
        }

        public static status[] DeserializeFromAtom(string response)
        {
            return DeserializeFromAtom(response, null);
        }

        public static status[] DeserializeFromAtom(string response, Twitter.Account Account)
        {
            var resultList = new List<status>();

            var results = new XmlDocument();

            results.LoadXml(response);
            var nm = new XmlNamespaceManager(results.NameTable);
            nm.AddNamespace("google", "http://base.google.com/ns/1.0");
            nm.AddNamespace("openSearch", "http://a9.com/-/spec/opensearch/1.1/");
            nm.AddNamespace("s", "http://www.w3.org/2005/Atom");
            XmlNodeList entries = results.SelectNodes("//s:entry", nm);
            Debug.WriteLine(entries.Count);
            try
            {
                foreach (XmlNode entry in entries)
                {
                    var newStat = new status();
                    newStat.text = entry.SelectSingleNode("s:title", nm).InnerText;
                    newStat.id = entry.SelectSingleNode("s:id", nm).InnerText;
                    newStat.created_at = entry.SelectSingleNode("s:published", nm).InnerText;
                    string userName = entry.SelectSingleNode("s:author/s:name", nm).InnerText;
                    newStat.created_at = entry.SelectSingleNode("s:published", nm).InnerText;
                    string userscreenName = userName.Split(new char[] {' '})[0];
                    newStat.user = new User();
                    newStat.user.screen_name = userscreenName;
                    newStat.user.profile_image_url =
                        entry.SelectSingleNode("s:link[@type=\"image/png\"]", nm).Attributes["href"].Value;
                    newStat.user.needsFetching = true;
                    resultList.Add(newStat);
                }
            }
            catch
            {
            }
            foreach (status stat in resultList)
            {
                stat.TypeofMessage = StatusTypes.SearchResult;
                stat.Account = Account;
            }
            return resultList.ToArray();
        }

        public static status[] FromDirectReplies(string response, Twitter.Account Account)
        {
            var resultList = new List<status>();

            var results = new XmlDocument();

            results.LoadXml(response);
            XmlNodeList entries = results.SelectNodes("//direct_message");
            foreach (XmlNode entry in entries)
            {
                var newStat = new status();
                newStat.text = entry.SelectSingleNode("text").InnerText;
                newStat.id = entry.SelectSingleNode("id").InnerText;
                newStat.created_at = entry.SelectSingleNode("created_at").InnerText;
                newStat.user = new User();
                newStat.user.screen_name = entry.SelectSingleNode("sender/screen_name").InnerText;
                newStat.user.id = entry.SelectSingleNode("sender/id").InnerText;
                newStat.user.profile_image_url = entry.SelectSingleNode("sender/profile_image_url").InnerText;
                newStat.user.location = entry.SelectSingleNode("sender/location").InnerText;
                newStat.user.name = entry.SelectSingleNode("sender/name").InnerText;
                newStat.user.description = entry.SelectSingleNode("sender/description").InnerText;
                newStat.favorited = "false";
                newStat.source = "";
                newStat.in_reply_to_status_id = "";
                resultList.Add(newStat);
            }
            foreach (status stat in resultList)
            {
                stat.TypeofMessage = StatusTypes.Direct;
                stat.Account = Account;
            }
            return resultList.ToArray();
        }

        public static status[] FromBrightKite(string response)
        {
            var resultList = new List<status>();

            var results = new XmlDocument();

            results.LoadXml(response);
            XmlNodeList entries = results.SelectNodes("//note");
            foreach (XmlNode entry in entries)
            {
                var newStat = new status();
                newStat.text = entry.SelectSingleNode("body").InnerText;
                newStat.id = entry.SelectSingleNode("id").InnerText;
                newStat.created_at = entry.SelectSingleNode("created_at").InnerText;
                newStat.location = entry.SelectSingleNode("place/display_location").InnerText;
                string userName = entry.SelectSingleNode("creator/login").InnerText;
                string avURL = entry.SelectSingleNode("creator/small_avatar_url").InnerText;
                newStat.user = new User();
                newStat.user.screen_name = userName;
                newStat.user.profile_image_url = "http://brightkite.com/" + avURL;
                resultList.Add(newStat);
            }
            return resultList.ToArray();
        }

        public bool Delete()
        {
            Yedda.Twitter Twitter = new Yedda.Twitter();
            Yedda.Twitter.Account account = ClientSettings.GetAcountForUser(user.screen_name);
            if (account == null)
                return false;
            Twitter.AccountInfo = account;

            Cursor.Current = Cursors.WaitCursor;
            try
            {
                string response = Twitter.Destroy_Status(id, Twitter.OutputFormatType.XML);
                if (response != null)
                {
                    LocalStorage.DataBaseUtility.DeleteStatus(id);
                    return true;
                }
                else
                {
                    return false;
                }
            }
            finally
            {
                Cursor.Current = Cursors.Default;
            }
        }

        public static string Serialize(status[] List)
        {
            if (List.Length == 0)
            {
                return null;
            }
            var sb = new StringBuilder();
            using (var w = new StringWriter(sb))
            {
                statusSerializer.Serialize(w, List);
            }
            return sb.ToString();
        }

        #endregion Methods 

        private const string SQLSave =
            @"INSERT INTO statuses (id, fulltext, userid, timestamp, in_reply_to_id, favorited, clientSource, accountSummary, statustypes, SearchTerm)
                                          VALUES
                                        (@id, @fulltext, @userid, @timestamp, @in_reply_to_id, @favorited, @clientSource, @accountSummary, @statustypes, @SearchTerm);";

        private const string SQLCheck = @"SELECT COUNT(id) from statuses WHERE id=@id;";
        private const string SQLUpdateTypes = @"UPDATE statuses SET statustypes=@type WHERE id=@id";

        public void Save(SQLiteConnection conn)
        {
            using (var comm = new SQLiteCommand(SQLCheck, conn))
            {
                comm.Parameters.Add(new SQLiteParameter("@id", id));

                object ret = comm.ExecuteScalar();
                if (ret != null && (long) ret != 0)
                {
                    if (TypeofMessage == StatusTypes.Reply)
                    {
                        //Already exists in the DB as a Friends update -- we need to make it an @mention too
                        TypeofMessage = StatusTypes.Normal | StatusTypes.Reply;
                        comm.CommandText = SQLUpdateTypes;
                        comm.Parameters.Add(new SQLiteParameter("@type", TypeofMessage));
                        comm.ExecuteNonQuery();
                    }
                    return;
                }
            }
            using (var comm = new SQLiteCommand(SQLSave, conn))
            {
                comm.Parameters.Add(new SQLiteParameter("@id", id));
                comm.Parameters.Add(new SQLiteParameter("@fulltext", text));
                comm.Parameters.Add(new SQLiteParameter("@userid", user.id));
                comm.Parameters.Add(new SQLiteParameter("@timestamp", createdAt));
                comm.Parameters.Add(new SQLiteParameter("@in_reply_to_id", in_reply_to_status_id));
                comm.Parameters.Add(new SQLiteParameter("@favorited", favorited));
                comm.Parameters.Add(new SQLiteParameter("@clientSource", source));
                comm.Parameters.Add(new SQLiteParameter("@accountSummary", AccountSummary));
                comm.Parameters.Add(new SQLiteParameter("@SearchTerm", SearchTerm));
                comm.Parameters.Add(new SQLiteParameter("@statustypes", TypeofMessage));

                try
                {
                    comm.ExecuteNonQuery();
                }
                catch (Exception)
                {
                }
            }
            user.Save(conn);
        }

        public override bool Equals(object obj)
        {
            try
            {
                var otherStat = (status) obj;
                return (otherStat.id.Equals(id) && otherStat.Account.Equals(Account));
            }
            catch
            {
                return false;
            }
        }

        public override int GetHashCode()
        {
            return id.GetHashCode() ^ Account.GetHashCode();
        }

        #region IComparable Members

        public int CompareTo(object obj)
        {
            var otherStat = (status) obj;
            return otherStat.createdAt.CompareTo(createdAt);
        }

        #endregion
    }

    [Serializable]
    public class User
    {
        #region Properties
        private bool _needsFetching = true;
        public bool needsFetching 
        {
            get
            {
                return _needsFetching;
            }
            set
            {
                _needsFetching = value;
            }
        }

        public string location { get; set; }
        public string description { get; set; }
        private string _profile_image_url;

        public string profile_image_url
        {
            get { return _profile_image_url; }
            set
            {
                _profile_image_url = value;
                if (ClientSettings.HighQualityAvatars)
                {
                    if (_profile_image_url.IndexOf("s3.amazonaws.com/twitter_production") > 0 &&
                        _profile_image_url.IndexOf("_bigger") == -1)
                    {
                        _profile_image_url = profile_image_url.Replace("_normal", "_bigger");
                    }
                }
            }
        }

        public string id { get; set; }
        public string name { get; set; }
        private string _screen_name;

        public string screen_name
        {
            get { return _screen_name; }
            set { _screen_name = value; }
        }

        public string followers_count { get; set; }

        #endregion Properties 

        #region Methods (1) 

        private const string SQLSave =
            @"INSERT INTO users (screenname, fullname, description, avatarURL, id)
                                          VALUES
                                        (@screenname, @fullname, @description, @avatarURL, @id);";

        private const string SQLCheck = @"SELECT COUNT(id) from users WHERE id=@id;";

        public void Save(SQLiteConnection conn)
        {
            using (var comm = new SQLiteCommand(SQLSave, conn))
            {
                comm.Parameters.Add(new SQLiteParameter("@screenname", screen_name));
                comm.Parameters.Add(new SQLiteParameter("@fullname", name));
                comm.Parameters.Add(new SQLiteParameter("@description", description));
                comm.Parameters.Add(new SQLiteParameter("@avatarURL", profile_image_url));
                comm.Parameters.Add(new SQLiteParameter("@id", id));

                try
                {
                    comm.ExecuteNonQuery();
                }
                catch (Exception)
                {
                }
            }
        }

        public static User FromId(string ID, Twitter.Account Account)
        {
            var t = new Twitter {AccountInfo = Account};
            string response = null;
            try
            {
                response = t.Show(ID, Twitter.OutputFormatType.XML);
            }
            catch
            {
                var toReturn = new User {screen_name = "PockeTwitUnknownUser"};
                return toReturn;
            }

            try
            {
                var s = new XmlSerializer(typeof (User));
                if (string.IsNullOrEmpty(response))
                {
                    var toReturn = new User {screen_name = "PockeTwitUnknownUser"};
                    return toReturn;
                }
                using (var r = new StringReader(response))
                {
                    User result = (User)s.Deserialize(r);
                    result._needsFetching = false;
                    return result;
                }
            }
            catch
            {
                var toReturn = new User {screen_name = "PockeTwitUnknownUser"};
                return toReturn;
            }
        }

        #endregion Methods 
    }
}
