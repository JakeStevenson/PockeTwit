using System;

using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;

namespace PockeTwit
{
    public  class Following
    {

        private static XmlSerializer userSerializer = new XmlSerializer(typeof(Library.User[]));
        public Yedda.Twitter TwitterConnection { get; set; }
        public delegate void delFollowers(Yedda.Twitter ConnectionDone);

		#region Fields (2) 

        private  List<string> FollowedUsers = new List<string>();
        private  bool OnceLoaded = false;

		#endregion Fields 

		#region Constructors (1) 

        public Following(Yedda.Twitter Connection )
        {
            TwitterConnection = Connection;
            System.Threading.ThreadPool.QueueUserWorkItem(new System.Threading.WaitCallback(GetCachedFollowers));
            //GetCachedFollowers();
        }

		#endregion Constructors 

		#region Methods (9) 


		// Public Methods (5) 

        public void AddUser(Library.User userToAdd)
        {
            if (!FollowedUsers.Contains(userToAdd.id))
            {
                FollowedUsers.Add(userToAdd.id);
                SaveUsers();
            }
        }

        public bool IsFollowing(Library.User userToCheck)
        {
            bool bFound = false;
            foreach (string User in FollowedUsers)
            {
                if (User == userToCheck.id)
                {
                    bFound = true;
                    break;
                }
            }
            return bFound;
        }

        public void LoadFromTwitter()
        {
            System.Threading.ThreadPool.QueueUserWorkItem(new System.Threading.WaitCallback(GetFollowersFromTwitter));
        }

        public  void Reset()
        {
            if (OnceLoaded)
            {
                System.Threading.ThreadPool.QueueUserWorkItem(new System.Threading.WaitCallback(GetFollowersFromTwitter));
            }
        }

        public void StopFollowing(Library.User usertoStop)
        {
            foreach(string User in FollowedUsers)
            {
                if(User == usertoStop.id)
                {
                    FollowedUsers.Remove(User);
                    break;
                }
            }
            SaveUsers();
        }



		// Private Methods (4) 

        private void GetCachedFollowers(object o)
        {
            string location = ClientSettings.AppPath + "\\Following" + TwitterConnection.AccountInfo.UserName + TwitterConnection.AccountInfo.ServerURL.Name + ".xml";
            try
            {
                if (System.IO.File.Exists(location))
                {
                    using (System.IO.StreamReader r = new System.IO.StreamReader(location))
                    {
                        string Followers = r.ReadToEnd();
                        InterpretUsers(Followers);
                    }
                }
            }
            catch
            {
                if (!string.IsNullOrEmpty(location))
                {
                    if (System.IO.File.Exists(location))
                    {
                        System.IO.File.Delete(location);
                    }
                }
            }
            
        }

        private  void GetFollowersFromTwitter(object o)
        {
            try
            {
                string response = this.TwitterConnection.GetFriendsIDs();
                InterpretUsers(response);
                SaveUsers();
            }
            catch 
            {
            }
        }

        private  void InterpretUsers(string response)
        {
            if (!string.IsNullOrEmpty(response))
            {
                System.Xml.XmlDocument d = new System.Xml.XmlDocument();
                d.LoadXml(response);
                
                System.Xml.XmlNodeList l = d.SelectNodes("//id");
                foreach (System.Xml.XmlNode n in l)
                {
                    if (!FollowedUsers.Contains(n.InnerText))
                    {
                        FollowedUsers.Add(n.InnerText);
                    }
                }
            }
        }

        private  void SaveUsers()
        {
            string location = ClientSettings.AppPath + "\\Following" + TwitterConnection.AccountInfo.UserName + TwitterConnection.AccountInfo.ServerURL.Name + ".xml";

            System.Xml.XmlDocument d = new System.Xml.XmlDocument();
            System.Xml.XmlElement root = d.CreateElement("ids");
            d.AppendChild(root);

            foreach (string ID in FollowedUsers)
            {
                System.Xml.XmlElement idElement = d.CreateElement("id");
                idElement.InnerText = ID;
                root.AppendChild(idElement);
            }
            
            d.Save(location);
        }


		#endregion Methods 

    }
}
