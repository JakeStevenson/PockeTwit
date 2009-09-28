using System;
using System.Collections.Generic;
using Microsoft.Win32;
using PockeTwit.Library;
using PockeTwit.SpecialTimelines;

namespace PockeTwit.TimeLines
{
    internal struct NewestSelectedInformation
    {
        public long CreatedAtTicks;
        public string id;
    }

    internal static class LastSelectedItems
    {
        public delegate void delUnreadCountChanged(string TimeLine, int Count);

        public static event delUnreadCountChanged UnreadCountChanged = delegate { };

        private const string LastSavedStoragePath = @"\Software\Apps\JustForFun PockeTwit\LastSaved\";
        private const string NewestSavedStoragePath = @"\Software\Apps\JustForFun PockeTwit\NewestSaved\";
        private const string UnreadCountRegistryPath = @"\Software\Apps\JustForFun PockeTwit\UnreadCount\";

        private static readonly Dictionary<string, string> LastSelectedItemsDictionary =
            new Dictionary<string, string>();

        private static Dictionary<string, NewestSelectedInformation> NewestSelectedItemsDictionary = new Dictionary<string, NewestSelectedInformation>();

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
            ISpecialTimeLine t = null;
            if (ListName.StartsWith("Grouped_TimeLine_"))
            {
                t = SpecialTimeLinesRepository.GetFromListName(ListName);
            }
            SetLastSelected(ListName,selectedStatus,t);
        }

        public static void SetLastSelected(string ListName, status selectedStatus, ISpecialTimeLine specialTime)
        {
            if (ListName == "Conversation" || ListName == "Search_TimeLine" || ListName == "@User_TimeLine" || ListName == "Favorites_TimeLine") { return; }
            if (!LastSelectedItemsDictionary.ContainsKey(ListName))
            {
                LastSelectedItemsDictionary.Add(ListName, "");
            }
            LastSelectedItemsDictionary[ListName] = selectedStatus.id;

            var newInfo = new NewestSelectedInformation
            {
                CreatedAtTicks = selectedStatus.createdAt.Ticks,
                id = selectedStatus.id
            };


            if (!NewestSelectedItemsDictionary.ContainsKey(ListName))
            {
                lock(NewestSelectedItemsDictionary)
                {
                    NewestSelectedItemsDictionary.Add(ListName, newInfo);
                    StoreNewestInfoInRegistry(ListName, newInfo);
                }
                SetUnreadCount(ListName, selectedStatus.id, specialTime);
            }
            else
            {
                if (NewestSelectedItemsDictionary[ListName].CreatedAtTicks <= newInfo.CreatedAtTicks)
                {
                    NewestSelectedItemsDictionary[ListName] = newInfo;
                    StoreNewestInfoInRegistry(ListName, newInfo);
                    SetUnreadCount(ListName, newInfo.id, specialTime);
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

                foreach (ISpecialTimeLine t in SpecialTimeLinesRepository.GetList())
                {
                    if (NewestSelectedItemsDictionary.ContainsKey(t.ListName))
                    {
                        SetUnreadCount(t.ListName, NewestSelectedItemsDictionary[t.ListName].id, t);
                    }
                }
            }
        }

        public static void SetUnreadCount(string ListName, string selectedStatus, ISpecialTimeLine specialTime)
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
            string DisplayName = ListName.Replace('_', ' ').Replace("Grouped TimeLine ", "").Replace("SavedSearch TimeLine ","");
            UnreadCountRoot.SetValue(DisplayName, updatedCount);
            UnreadCountRoot.SetValue("UnreadCountChanged", System.DateTime.Now.Ticks);
        }

        public static int GetUpdatedCount(string ListName, ISpecialTimeLine specialTime, string selectedStatus)
        {
            TimelineManagement.TimeLineType t = SpecialTimeLinesRepository.GetTimelineTypeFromSpecialType(ListName);
            
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
        private static void StoreNewestInfoInRegistry(string ListName, NewestSelectedInformation Item)
        {
            NewestSavedItemsRoot.SetValue(ListName, Item.CreatedAtTicks + "|" + Item.id);
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
                string[] storedItems = NewestSavedItemsRoot.GetValueNames();
                foreach (string storedItem in storedItems)
                {
                    var serializedItem = (string)NewestSavedItemsRoot.GetValue(storedItem);
                    NewestSelectedInformation newItem;
                    try
                    {
                        var splitItem = serializedItem.Split('|');
                        newItem = new NewestSelectedInformation
                                          {CreatedAtTicks = long.Parse(splitItem[1]), id = splitItem[1]};
                    }
                    catch
                    {
                        var deserializedStatus = status.DeserializeSingle(serializedItem, null);
                        newItem = new NewestSelectedInformation
                                      {CreatedAtTicks = deserializedStatus.createdAt.Ticks, id = deserializedStatus.id};
                    }
                    NewestSelectedItemsDictionary.Add(storedItem, newItem);
                }
            }
            UpdateUnreadCounts();
        }
    }
}