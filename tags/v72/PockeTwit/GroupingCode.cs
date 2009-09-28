using System;

using System.Data.SQLite;

using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml.Serialization;

namespace PockeTwit
{
    [Serializable]
    public class SpecialTimeLine
    {
        [Serializable]
        public class groupTerm
        {
            public string Term;
            public string Name;
            public bool Exclusive;
        }
        
        public string ListName
        {
            get
            {
                return "Grouped_TimeLine_" + name;
            }
        }

        public string name { get; set; }
        public groupTerm[] Terms { get; set; }

        
        public void AddItem(string Term, string ScreenName, bool Exclusive)
        {
            groupTerm newTerm = new groupTerm() { Term = Term, Name = ScreenName, Exclusive = Exclusive };
            if (Terms != null && Terms.Length > 0)
            {
                List<groupTerm> items = new List<groupTerm>(Terms);
                if (!items.Contains(newTerm))
                {
                    items.Add(newTerm);
                }
                Terms = items.ToArray();
            }
            else
            {
                Terms = new groupTerm[] { newTerm };
            }
        }
        public void RemoveItem(string Term)
        {
            List<groupTerm> items = new List<groupTerm>(Terms);
            groupTerm toRemove = new groupTerm();
            foreach (groupTerm t in items)
            {
                if (t.Term == Term)
                {
                    toRemove = t;
                }
            }
            if (items.Contains(toRemove))
            {
                items.Remove(toRemove);
            }
            Terms = items.ToArray();
            if(Terms.Length==0)
            {
                SpecialTimeLines.Remove(this);
            }
            SpecialTimeLines.Save();
        }
        

        public string GetConstraints()
        {
            if (Terms == null) 
            {
                SpecialTimeLines.Load();
            }
            if (Terms == null) 
            {
                return "";
            }
            string ret = "";
            List<string> UserList = new List<string>();
            foreach (groupTerm t in Terms)
            {
                UserList.Add("'"+t.Term+"'");
                
            }
            if (UserList.Count > 0)
            {
                ret = " AND statuses.userid IN(" + string.Join(",", UserList.ToArray()) + ") ";
            }

            return ret;
        }

        public override string ToString()
        {
            return name;
        }


    }

    [Serializable]
    public static class SpecialTimeLines
    {
        private static string configPath = ClientSettings.AppPath + "\\savedTimelines.xml";
        private static Dictionary<string, SpecialTimeLine> _Items = new Dictionary<string, SpecialTimeLine>();

        public static SpecialTimeLine[] GetList()
        {
            List<SpecialTimeLine> s = new List<SpecialTimeLine>();
            lock (_Items)
            {
                foreach (SpecialTimeLine item in _Items.Values)
                {
                    s.Add(item);
                }
            }

            return s.ToArray();
        }
        public static void Add(SpecialTimeLine newLine)
        {
            lock (_Items)
            {
                if (!_Items.ContainsKey(newLine.name))
                {
                    _Items.Add(newLine.name, newLine);
                    NotificationHandler.AddSpecialTimeLineNotifications(newLine);
                }
            }
        }
        public static void Remove(SpecialTimeLine oldLine)
        {
            lock (_Items)
            {
                if(_Items.ContainsKey(oldLine.name))
                {
                    _Items.Remove(oldLine.name);
                    NotificationHandler.RemoveSpecialTimeLineNotifications(oldLine);
                }
            }
            using (SQLiteConnection conn = LocalStorage.DataBaseUtility.GetConnection())
            {
                conn.Open();
                using (SQLiteTransaction t = conn.BeginTransaction())
                {
                    using (SQLiteCommand comm = new SQLiteCommand(conn))
                    {
                        comm.CommandText = "DELETE FROM usersInGroups WHERE groupname=@groupname;";
                        comm.Parameters.Add(new SQLiteParameter("@groupname", oldLine.name));
                        comm.ExecuteNonQuery();

                        comm.CommandText = "DELETE FROM groups WHERE groupname=@groupname;";
                        comm.ExecuteNonQuery();
                    }
                    t.Commit();
                }
            }
        }
        public static void Clear()
        {
            lock (_Items)
            {
                _Items.Clear();
            }
            using (SQLiteConnection conn = LocalStorage.DataBaseUtility.GetConnection())
            {
                conn.Open();
                using (SQLiteTransaction t = conn.BeginTransaction())
                {
                    using (SQLiteCommand comm = new SQLiteCommand(conn))
                    {
                        comm.CommandText = "DELETE FROM usersInGroups;";
                        comm.ExecuteNonQuery();

                        comm.CommandText = "DELETE FROM groups;";
                        comm.ExecuteNonQuery();
                    }
                    t.Commit();
                }
            }

        }

        public static void Load()
        {
            
            using (SQLiteConnection conn = LocalStorage.DataBaseUtility.GetConnection())
            {
                conn.Open();
                using (SQLiteCommand comm = new SQLiteCommand(conn))
                {
                    comm.CommandText = "SELECT groupname, userid, exclusive, users.screenname FROM usersInGroups INNER JOIN users ON usersInGroups.userid = users.id";
                    using (SQLiteDataReader r = comm.ExecuteReader())
                    {
                        while (r.Read())
                        {
                            string groupName = r.GetString(0);
                            string userID = r.GetString(1);
                            bool exclusive = r.GetBoolean(2);
                            string screenName = r.GetString(3);
                            SpecialTimeLine thisLine = new SpecialTimeLine();
                            if (_Items.ContainsKey(groupName))
                            {
                                thisLine = _Items[groupName];
                            }
                            else
                            {
                                thisLine.name = groupName;
                                Add(thisLine);
                            }
                            thisLine.AddItem(userID,screenName, exclusive);
                        }
                    }
                }
            }
        }
        public static void Save()
        {
            
            if (_Items.Count > 0)
            {
                using (SQLiteConnection conn = LocalStorage.DataBaseUtility.GetConnection())
                {
                    lock (_Items)
                    {
                        conn.Open();
                        using (SQLiteTransaction t = conn.BeginTransaction())
                        {
                            foreach (SpecialTimeLine group in _Items.Values)
                            {
                                using (SQLiteCommand comm = new SQLiteCommand(conn))
                                {
                                    comm.CommandText = "INSERT INTO groups (groupname) VALUES (@name);";
                                    comm.Parameters.Add(new SQLiteParameter("@name", group.name));

                                    comm.ExecuteNonQuery();

                                    comm.CommandText = "DELETE FROM usersInGroups WHERE groupname=@groupname";
                                    comm.Parameters.Add(new SQLiteParameter("@groupname", group.name));
                                    comm.ExecuteNonQuery();
                                    comm.Parameters.Clear();

                                    foreach (SpecialTimeLine.groupTerm groupItem in group.Terms)
                                    {
                                        comm.Parameters.Clear();
                                        comm.CommandText = "INSERT INTO usersInGroups (id, groupname, userid, exclusive) VALUES (@pairid, @name, @userid, @exclusive)";
                                        comm.Parameters.Add(new SQLiteParameter("@pairid", group.name + groupItem.Term));
                                        comm.Parameters.Add(new SQLiteParameter("@name", group.name));
                                        comm.Parameters.Add(new SQLiteParameter("@userid", groupItem.Term));
                                        comm.Parameters.Add(new SQLiteParameter("@exclusive", groupItem.Exclusive));
                                        comm.ExecuteNonQuery();

                                    }
                                }
                            }
                            t.Commit();
                        }
                    }
                }
            }
        }

        public static bool UserIsExcluded(string term)
        {
            lock (_Items)
            {
                foreach (SpecialTimeLine t in _Items.Values)
                {
                    foreach (SpecialTimeLine.groupTerm groupterm in t.Terms)
                    {
                        if (groupterm.Term == term && groupterm.Exclusive)
                        {
                            return true;
                        }
                    }
                }
            }
            return false;
        }

        internal static SpecialTimeLine GetFromName(string ListName)
        {
            SpecialTimeLine ret = null;
            foreach (SpecialTimeLine t in GetList())
            {
                if (t.ListName == ListName)
                {
                    ret = t;
                }
            }
            return ret;
        }


        public static void Export(string FileName)
        {
            
            lock (_Items)
            {
                List<SpecialTimeLine> l = new List<SpecialTimeLine>();
                foreach (var item in _Items.Values)
                {
                    l.Add(item);
                }
                System.Xml.Serialization.XmlSerializer s = new XmlSerializer(typeof(SpecialTimeLine[]));
                StringBuilder b = new StringBuilder();
                using (System.IO.StreamWriter w = new StreamWriter(FileName))
                {
                    s.Serialize(w, l.ToArray());
                }
            }
        }

        public static void Import(string FileName)
        {
            if (!System.IO.File.Exists(FileName)) return;
            SpecialTimeLine[] Input;
            System.Xml.Serialization.XmlSerializer s = new XmlSerializer(typeof (SpecialTimeLine[]));

            using (System.IO.StreamReader r = new StreamReader(FileName))
            {
                Input = (SpecialTimeLine[]) s.Deserialize(r);
            }

            lock (_Items)
            {
                _Items.Clear();
                foreach (var line in Input)
                {
                    _Items.Add(line.name, line);
                }
            }
            Save();
        }
    }
}
