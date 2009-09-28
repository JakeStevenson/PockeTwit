using System;
using System.Collections.Generic;
using Microsoft.Win32;
using PockeTwit.Library;

namespace PockeTwit.TimeLines
{
    internal static class LastSelectedItems
    {
        public delegate void delUnreadCountChanged(string TimeLine, int Count);

        public static event delUnreadCountChanged UnreadCountChanged = delegate { };

        private const string LastSavedStoragePath = @"\Software\Apps\JustForFun PockeTwit\LastSaved\";
        private const string NewestSavedStoragePath = @"\Software\Apps\JustForFun PockeTwit\NewestSaved\";
        private const string UnreadCountRegistryPath = @"\Software\Apps\JustForFun PockeTwit\UnreadCount\";

        private static readonly Dictionary<string, string> LastSelectedItemsDictionary =
            new Dictionary<string, string>();

        private static readonly Dictionary<string, status> NewestSelectedItemsDictionary =
            new Dictionary<string, status>();

        private static readonly Dictionary<string, int> UnreadItemCount =
            new Dictionary<string, int>();

        private static RegistryKey LastSavedItemsRoot;
        private static RegistryKey NewestSavedItemsRoot;
        private static RegistryKey UnreadCountRoot;
        
        static LastSelectedItems()
        {
            LoadStoredItems();
        }

        public static void SetLastSelected(string ListName, status selectedStatus)
        {
            SpecialTimeLine t = null;
            if (ListName.StartsWith("Grouped_TimeLine_"))
            {
                t = SpecialTimeLines.GetFromName(ListName);
            }
            SetLastSelected(ListName,selectedStatus,t);
        }

        public static void SetLastSelected(string ListName, status selectedStatus, SpecialTimeLine specialTime)
        {
            if (ListName == "Conversation" || ListName == "Search_TimeLine") { return; }
            if (!LastSelectedItemsDictionary.ContainsKey(ListName))
            {
                LastSelectedItemsDictionary.Add(ListName, "");
            }
            LastSelectedItemsDictionary[ListName] = selectedStatus.id;
            
            if (!NewestSelectedItemsDictionary.ContainsKey(ListName))
            {
                lock(NewestSelectedItemsDictionary)
                {
                    NewestSelectedItemsDictionary.Add(ListName, selectedStatus);
                    StoreStatusInRegistry(ListName, selectedStatus);
                }
                SetUnreadCount(ListName, selectedStatus.id, specialTime);
            }
            else
            {
                if (NewestSelectedItemsDictionary[ListName].createdAt <= selectedStatus.createdAt)
                {
                    NewestSelectedItemsDictionary[ListName] = selectedStatus;
                    StoreStatusInRegistry(ListName, selectedStatus);
                    SetUnreadCount(ListName, selectedStatus.id, specialTime);
                }
            }
            
            StoreSelectedItem(ListName, selectedStatus.id);
        }

        public static int GetUnreadItems(string ListName)
        {
            if(UnreadItemCount.ContainsKey(ListName))
            {
                return UnreadItemCount[ListName];
            }
            return 0;
        }

        public static void UpdateUnreadCounts()
        {
            lock (NewestSelectedItemsDictionary)
            {
                if (NewestSelectedItemsDictionary.ContainsKey("Friends_TimeLine"))
                {
                    SetUnreadCount("Friends_TimeLine", NewestSelectedItemsDictionary["Friends_TimeLine"].id, null);
                }
                if (NewestSelectedItemsDictionary.ContainsKey("Messages_TimeLine"))
                {
                    SetUnreadCount("Messages_TimeLine", NewestSelectedItemsDictionary["Messages_TimeLine"].id, null);
                }

                foreach (SpecialTimeLine t in SpecialTimeLines.GetList())
                {
                    if (NewestSelectedItemsDictionary.ContainsKey(t.ListName))
                    {
                        SetUnreadCount(t.ListName, NewestSelectedItemsDictionary[t.ListName].id, t);
                    }
                }
            }
        }

        public static void SetUnreadCount(string ListName, string selectedStatus, SpecialTimeLine specialTime)
        {
            int updatedCount = GetUpdatedCount(ListName, specialTime, selectedStatus);
            if(!UnreadItemCount.ContainsKey(ListName))
            {
                UnreadItemCount.Add(ListName, updatedCount);
            }
            else
            {
                UnreadItemCount[ListName] = updatedCount;
            }
            SetUnreadInRegistry(ListName, updatedCount);
            UnreadCountChanged(ListName, updatedCount);
        }

        private static void SetUnreadInRegistry(string ListName, int updatedCount)
        {
            string DisplayName = ListName.Replace('_', ' ').Replace("Grouped TimeLine ", "");
            UnreadCountRoot.SetValue(DisplayName, updatedCount);
            UnreadCountRoot.SetValue("UnreadCountChanged", System.DateTime.Now.Ticks);
        }

        public static int GetUpdatedCount(string ListName, SpecialTimeLine specialTime, string selectedStatus)
        {
            TimelineManagement.TimeLineType t = TimelineManagement.TimeLineType.Friends;
            switch (ListName)
            {
                case "Messages_TimeLine":
                    t = TimelineManagement.TimeLineType.Messages;
                    break;
            }

            string Constraints = null;
            if (specialTime != null) 
            {
                Constraints = specialTime.GetConstraints();
            }
            return LocalStorage.DataBaseUtility.CountItemsNewerThan(t, selectedStatus, Constraints);
        }

        public static string GetLastSelected(string ListName)
        {
            if (!LastSelectedItemsDictionary.ContainsKey(ListName))
            {
                return null;
            }
            return LastSelectedItemsDictionary[ListName];
        }

        public static string GetNewestSelected(string ListName)
        {
            if (!NewestSelectedItemsDictionary.ContainsKey(ListName))
            {
                return null;
            }
            return NewestSelectedItemsDictionary[ListName].id;
        }

        private static void StoreSelectedItem(string ListName, string ID)
        {
            LastSavedItemsRoot.SetValue(ListName, ID, RegistryValueKind.String);
        }
        private static void StoreStatusInRegistry(string ListName, status Item)
        {
            NewestSavedItemsRoot.SetValue(ListName, Item.Serialized);
        }


        private static void LoadStoredItems()
        {
            LastSavedItemsRoot = Registry.LocalMachine.OpenSubKey(LastSavedStoragePath, true);
            NewestSavedItemsRoot = Registry.LocalMachine.OpenSubKey(NewestSavedStoragePath, true);
            UnreadCountRoot = Registry.LocalMachine.OpenSubKey(UnreadCountRegistryPath, true);

            if (UnreadCountRoot == null)
            {
                RegistryKey ParentKey = Registry.LocalMachine.OpenSubKey(@"\Software\Apps\", true);
                if (ParentKey != null) UnreadCountRoot = ParentKey.CreateSubKey("JustForFun PockeTwit\\UnreadCount");
            }

            if (LastSavedItemsRoot == null)
            {
                RegistryKey ParentKey = Registry.LocalMachine.OpenSubKey(@"\Software\Apps\", true);
                if (ParentKey != null) LastSavedItemsRoot = ParentKey.CreateSubKey("JustForFun PockeTwit\\LastSaved");
            }
            if (LastSavedItemsRoot != null)
            {
                string[] StoredItems = LastSavedItemsRoot.GetValueNames();
                foreach (string StoredItem in StoredItems)
                {
                    LastSelectedItemsDictionary.Add(StoredItem, (string) LastSavedItemsRoot.GetValue(StoredItem));
                }
            }

            if (NewestSavedItemsRoot == null)
            {
                RegistryKey ParentKey = Registry.LocalMachine.OpenSubKey(@"\Software\Apps\", true);
                if (ParentKey != null) NewestSavedItemsRoot = ParentKey.CreateSubKey("JustForFun PockeTwit\\NewestSaved");
            }
            if (NewestSavedItemsRoot != null)
            {
                string[] StoredItems = NewestSavedItemsRoot.GetValueNames();
                foreach (string StoredItem in StoredItems)
                {
                    string SerializedItem = (string)NewestSavedItemsRoot.GetValue(StoredItem);
                    status Deserialized = status.DeserializeSingle(SerializedItem, null);
                    NewestSelectedItemsDictionary.Add(StoredItem, Deserialized);
                }
            }
            UpdateUnreadCounts();
        }
    }
}