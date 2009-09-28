using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.IO;
using System.Xml.Serialization;
using PockeTwit.LocalStorage;
using PockeTwit.NotificationsCode;

namespace PockeTwit.SpecialTimelines
{
    [Serializable]
    public static class SpecialTimeLinesRepository
    {
        #region TimeLineType enum
        public enum TimeLineType
        {
            UserGroup,
            SavedSearch
        }
        #endregion

        public static TimelineManagement.TimeLineType GetTimelineTypeFromSpecialType(string timeLineName)
        {
            if (timeLineName.StartsWith("Saved"))
            {
                return TimelineManagement.TimeLineType.Searches;
            }
            if (timeLineName == "Messages_TimeLine")
            {
                return TimelineManagement.TimeLineType.Messages;
            }
            return TimelineManagement.TimeLineType.Friends;
        }

        private static readonly Dictionary<string, ISpecialTimeLine> Items =
            new Dictionary<string, ISpecialTimeLine>();

        public static ISpecialTimeLine[] GetList(TimeLineType timeLineType)
        {
            var s = new List<ISpecialTimeLine>();
            lock (Items)
            {
                foreach (var item in Items.Values)
                {
                    if (item.Timelinetype == timeLineType)
                    {
                        s.Add(item);
                    }
                }
            }

            return s.ToArray();
        }

        public static ISpecialTimeLine[] GetList()
        {
            var s = new List<ISpecialTimeLine>();
            lock (Items)
            {
                foreach (var item in Items.Values)
                {
                    s.Add(item);
                }
            }

            return s.ToArray();
        }

        public static void Add(ISpecialTimeLine newLine)
        {
            lock (Items)
            {
                if (!Items.ContainsKey(newLine.name))
                {
                    Items.Add(newLine.name, newLine);
                    NotificationHandler.AddSpecialTimeLineNotifications(newLine);
                }
            }
        }

        public static void Remove(ISpecialTimeLine oldLine)
        {
            lock (Items)
            {
                if (Items.ContainsKey(oldLine.name))
                {
                    Items.Remove(oldLine.name);
                    NotificationHandler.RemoveSpecialTimeLineNotifications(oldLine);
                }
            }
            using (var conn = DataBaseUtility.GetConnection())
            {
                conn.Open();
                using (var t = conn.BeginTransaction())
                {
                    using (var comm = new SQLiteCommand(conn))
                    {
                        if (oldLine is UserGroupTimeLine)
                        {
                            var line = (UserGroupTimeLine) oldLine;
                            comm.CommandText = "DELETE FROM usersInGroups WHERE groupname=@groupname;";
                            comm.Parameters.Add(new SQLiteParameter("@groupname", line.name));
                            comm.ExecuteNonQuery();

                            comm.CommandText = "DELETE FROM groups WHERE groupname=@groupname;";
                            comm.ExecuteNonQuery();
                        }
                        else
                        {
                            var line = (SavedSearchTimeLine) oldLine;
                            comm.CommandText = "DELETE FROM savedSearches WHERE searchName=@searchName;";
                            comm.Parameters.Add(new SQLiteParameter("@searchName", line.name));
                            comm.ExecuteNonQuery();
                        }
                    }
                    t.Commit();
                }
            }
        }

        public static void Clear()
        {
            lock (Items)
            {
                Items.Clear();
            }
            using (var conn = DataBaseUtility.GetConnection())
            {
                conn.Open();
                using (var t = conn.BeginTransaction())
                {
                    using (var comm = new SQLiteCommand(conn))
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
            using (var conn = DataBaseUtility.GetConnection())
            {
                conn.Open();
                using (var comm = new SQLiteCommand(conn))
                {
                    comm.CommandText =
                        "SELECT groupname, userid, exclusive, users.screenname FROM usersInGroups INNER JOIN users ON usersInGroups.userid = users.id";
                    using (var r = comm.ExecuteReader())
                    {
                        while (r.Read())
                        {
                            var groupName = r.GetString(0);
                            var userID = r.GetString(1);
                            var exclusive = r.GetBoolean(2);
                            var screenName = r.GetString(3);
                            var thisLine = new UserGroupTimeLine();
                            if (Items.ContainsKey(groupName))
                            {
                                thisLine = (UserGroupTimeLine) Items[groupName];
                            }
                            else
                            {
                                thisLine.name = groupName;
                                Add(thisLine);
                            }
                            thisLine.AddItem(userID, screenName, exclusive);
                        }
                    }


                    comm.CommandText =
                        "SELECT searchName, searchTerm, autoUpdate from savedSearches;";

                    try
                    {

                        using (var r = comm.ExecuteReader())
                        {
                            while (r.Read())
                            {
                                var searchName = r.GetString(0);
                                var searchTerm = r.GetString(1);
                                var autoUpdate = r.GetBoolean(2);

                                var savedLine = new SavedSearchTimeLine
                                                    {
                                                        name = searchName,
                                                        SearchPhrase = searchTerm,
                                                        autoUpdate = autoUpdate
                                                    };
                                Add(savedLine);
                            }
                        }
                    }
                    //Ignore it when we have a mismatched save search table.
                    catch{}
                }
            }
        }

        public static void Save()
        {
            if (Items.Count <= 0) return;
            using (var conn = DataBaseUtility.GetConnection())
            {
                lock (Items)
                {
                    conn.Open();
                    using (var t = conn.BeginTransaction())
                    {
                        foreach (var item in Items.Values)
                        {
                            
                            using (var comm = new SQLiteCommand(conn))
                            {
                                //UserGroup
                                if (item.Timelinetype == TimeLineType.UserGroup)
                                {
                                    var group = (UserGroupTimeLine)item;

                                    comm.CommandText = "INSERT INTO groups (groupname) VALUES (@name);";
                                    comm.Parameters.Add(new SQLiteParameter("@name", group.name));

                                    comm.ExecuteNonQuery();

                                    comm.CommandText = "DELETE FROM usersInGroups WHERE groupname=@groupname";
                                    comm.Parameters.Add(new SQLiteParameter("@groupname", group.name));
                                    comm.ExecuteNonQuery();
                                    comm.Parameters.Clear();

                                    if (group.Terms != null)
                                    {
                                        foreach (var groupItem in group.Terms)
                                        {
                                            comm.Parameters.Clear();
                                            comm.CommandText =
                                                "INSERT INTO usersInGroups (id, groupname, userid, exclusive) VALUES (@pairid, @name, @userid, @exclusive)";
                                            comm.Parameters.Add(new SQLiteParameter("@pairid",
                                                                                    group.name + groupItem.Term));
                                            comm.Parameters.Add(new SQLiteParameter("@name", group.name));
                                            comm.Parameters.Add(new SQLiteParameter("@userid", groupItem.Term));
                                            comm.Parameters.Add(new SQLiteParameter("@exclusive",
                                                                                    groupItem.Exclusive));
                                            comm.ExecuteNonQuery();
                                            
                                        }
                                    }
                                }
                                //SavedSearch
                                else
                                {
                                    var searchLine = (SavedSearchTimeLine) item;
                                    comm.CommandText =
                                        "INSERT INTO savedSearches (searchName, searchTerm, autoUpdate) VALUES (@searchName, @searchTerm, @autoUpdate);";
                                    comm.Parameters.Clear();
                                    comm.Parameters.Add(new SQLiteParameter("@searchName", searchLine.name));
                                    comm.Parameters.Add(new SQLiteParameter("@searchTerm", searchLine.SearchPhrase));
                                    comm.Parameters.Add(new SQLiteParameter("@autoUpdate", searchLine.autoUpdate));
                                    comm.ExecuteNonQuery();
                                }
                            }
                        }
                        t.Commit();
                    }
                }
            }
        }

        public static bool UserIsExcluded(string term)
        {
            lock (Items)
            {
                var list = GetList(TimeLineType.UserGroup);
                foreach (UserGroupTimeLine t in list)
                {
                    foreach (var groupterm in t.Terms)
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

        internal static ISpecialTimeLine GetFromListName(string listName)
        {
            ISpecialTimeLine ret = null;
            foreach (var t in GetList())
            {
                if (t.ListName == listName)
                {
                    ret = t;
                }
            }
            return ret;
        }

        internal static ISpecialTimeLine GetFromReadableName(string name)
        {
            ISpecialTimeLine ret = null;
            foreach (var t in GetList())
            {
                if (t.name == name)
                {
                    ret = t;
                }
            }
            return ret;
        }

        public static void Export()
        {
            var fileName = ClientSettings.CacheDir + "\\GroupBackup.xml";
            lock (Items)
            {
                var l = new List<ISpecialTimeLine>();
                foreach (var item in Items.Values)
                {
                    l.Add(item);
                }
                var s = new XmlSerializer(typeof(SpecialTimeLineSerializationHelper));
                SpecialTimeLineSerializationHelper helper = new SpecialTimeLineSerializationHelper(l);
                using (var w = new StreamWriter(fileName))
                {
                    s.Serialize(w, helper);
                }
            }
        }

        public static void Import()
        {
            var fileName = ClientSettings.CacheDir + "\\GroupBackup.xml";
            if (!File.Exists(fileName)) return;
            ISpecialTimeLine[] input;
            var s = new XmlSerializer(typeof(SpecialTimeLineSerializationHelper));

            using (var r = new StreamReader(fileName))
            {
                input = ((SpecialTimeLineSerializationHelper)s.Deserialize(r)).Items;
            }

            lock (Items)
            {
                Items.Clear();
                foreach (var line in input)
                {
                    Items.Add(line.name, line);
                }
            }
            Save();
        }
    }
}