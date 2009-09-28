using System;
using System.Windows.Forms;
using System.Collections.Generic;
using System.Text;
using Microsoft.WindowsMobile.Status;
using OpenNETCF.WindowsCE;
using PockeTwit.Library;
using PockeTwit.SpecialTimelines;
using Yedda;

namespace PockeTwit
{
    public class TimelineManagement
    {
        #region Events

        public delegate void delUpdated();
        public delegate void delProgress(int percentage, string Status);
        public delegate void delComplete();
        public delegate void delNullReturnedByAccount(Yedda.Twitter.Account t, Yedda.Twitter.ActionType Action);
        public event delUpdated FriendsUpdated;
        public event delUpdated MessagesUpdated;
        public event delUpdated SearchesUpdated;
        public event delProgress Progress;
        public event delComplete CompleteLoaded;
        public event delNullReturnedByAccount NoData = delegate{};
        public event delNullReturnedByAccount ErrorCleared = delegate { };

        #endregion
        public bool RunInBackground = true;
        
        [Flags]
        public enum TimeLineType
        {
            Friends=0,
            Replies=1,
            Direct=2,
            Messages=3,
            Searches=4
        }
        private LargeIntervalTimer updateTimer = new LargeIntervalTimer();
        private List<Yedda.Twitter> TwitterConnections;
        private int HoldNewMessages = 0;
        private int HoldNewFriends = 0;
        public DateTime NextUpdate;

        //private Microsoft.WindowsMobile.Status.SystemState PowerState = new Microsoft.WindowsMobile.Status.SystemState(Microsoft.WindowsMobile.Status.SystemProperty.PowerBatteryState);

        public TimelineManagement()
        {
            //Not working out so well on my device.  Will investigate more later.
            //PowerState.Changed += new Microsoft.WindowsMobile.Status.ChangeEventHandler(s_Changed);    
            GlobalEventHandler.PauseConnections += new GlobalEventHandler.delEmpty(GlobalEventHandler_PauseConnections);
            GlobalEventHandler.ResumeConnections += new GlobalEventHandler.delEmpty(GlobalEventHandler_ResumeConnections);
        }

        void GlobalEventHandler_ResumeConnections()
        {
            updateTimer.Enabled = true;
        }

        void GlobalEventHandler_PauseConnections()
        {
            updateTimer.Enabled = false;
        }

        void s_Changed(object sender, Microsoft.WindowsMobile.Status.ChangeEventArgs args)
        {
            BatteryLevel level = (BatteryLevel)SystemState.GetValue(SystemProperty.PowerBatteryStrength);
            if (ClientSettings.UpdateMinutes > 0)
            {
                if (level <= BatteryLevel.VeryLow)
                {
                    updateTimer.Enabled = false;
                    PockeTwit.Localization.LocalizedMessageBox.Show("Battery low - disabling auto-updates.\n{0}", "PockeTwit", level.ToString());
                }
                else
                {
                    if (!updateTimer.Enabled)
                    {
                        if (level >= BatteryLevel.Low)
                        {
                            updateTimer.Enabled = false;
                            PockeTwit.Localization.LocalizedMessageBox.Show("Battery charged - re-enabling auto-updates.\n{0}", "PockeTwit", level.ToString());
                        }
                    }
                }
            }
        }

        public void Startup(List<Yedda.Twitter> TwitterConnectionsToFollow)
        {
            
            TwitterConnections = TwitterConnectionsToFollow;
            
            if (LocalStorage.DataBaseUtility.GetList(TimeLineType.Friends, ClientSettings.MaxTweets).Count > 0)
            {
                CompleteLoaded();
            }
            else
            {
                Progress(0, "The first run takes a while.");
                GetFriendsTimeLine();
                Progress(0, "Just a bit longer.");
                GetMessagesTimeLine();
                GetSavedSearches();
                CompleteLoaded();
            } 
            if (ClientSettings.UpdateMinutes > 0)
            {
                if (updateTimer != null)
                {
                    updateTimer.FirstEventTime = DateTime.Now.Add(new TimeSpan(0, ClientSettings.UpdateMinutes, 0));
                    updateTimer.Interval = new TimeSpan(0, ClientSettings.UpdateMinutes, 0);
                    updateTimer.OneShot = false;
                    updateTimer.Tick += new EventHandler(updateTimer_Tick);
                    updateTimer_Tick(null, null);
                }
                NextUpdate = DateTime.Now.Add(new TimeSpan(0, ClientSettings.UpdateMinutes, 0));
            }
        }
        ~TimelineManagement()
        {
            ShutDown();
        }
        public void ShutDown()
        {
            if (updateTimer != null)
            {
                updateTimer.Enabled = false;
                updateTimer = null;
            }
        }

        public void Pause()
        {
            if (updateTimer != null)
            {
                updateTimer.Enabled = false;
            }
        }
        public void Start()
        {
            if (updateTimer != null)
            {
                updateTimer.Enabled = true;
            }
        }

        public void updateTimer_Tick(object sender, EventArgs e)
        {
            System.Threading.ThreadPool.QueueUserWorkItem(new System.Threading.WaitCallback(BackgroundUpdateBoth));   
        }

        private void BackgroundUpdateBoth(object o)
        {
            GetFriendsTimeLine(true);
            GetMessagesTimeLine(true);
            GetSavedSearches();
        }

        private void BackgroundMessagesUpdate(object o)
        {
            GetMessagesTimeLine(true);
        }


        public Library.status[] GetFriendsImmediately()
        {
            return LocalStorage.DataBaseUtility.GetList(TimeLineType.Friends, ClientSettings.MaxTweets).ToArray();
        }

        public Library.status[] GetMessagesImmediately()
        {
            return LocalStorage.DataBaseUtility.GetList(TimeLineType.Messages, ClientSettings.MaxTweets).ToArray();
        }

        private void BackgroundFriendsUpdate(object o)
        {
            GetFriendsTimeLine(true);
        }

        public void RefreshFriendsTimeLine()
        {
            if (!GlobalEventHandler.FriendsUpdating)
            {
                System.Threading.ThreadPool.QueueUserWorkItem(new System.Threading.WaitCallback(GetFriendsTimeLine));
            }
         }

        public void RefreshMessagesTimeLine()
        {
            if (!GlobalEventHandler.MessagesUpdating)
            {
                System.Threading.ThreadPool.QueueUserWorkItem(new System.Threading.WaitCallback(GetMessagesTimeLine));
            }
        }

        private int _currentSearchPageNo = 0;
        private string _firstSearchHitId = String.Empty;
        private string _lastSearchTerm = String.Empty;
 
        public Library.status[] SearchTwitter(Yedda.Twitter t, string SearchString, Yedda.Twitter.PagingMode pagingMode)
        {
            if (pagingMode == Twitter.PagingMode.None)
            {
                _lastSearchTerm = SearchString;
                _currentSearchPageNo = 1;
            }
            else
            {
                if (_lastSearchTerm != SearchString)
                    _currentSearchPageNo = 1;

                _lastSearchTerm = SearchString;
                          
                switch (pagingMode)
                {
                    case Twitter.PagingMode.Forward:
                        _currentSearchPageNo++;
                        break;
                    case Twitter.PagingMode.Back:
                        _currentSearchPageNo--;
                        break;
                    case Twitter.PagingMode.Neutral:
                        break;
                }

                if (_currentSearchPageNo != 1)
                {
                    SearchString = String.Format("max_id={0}&page={1}&{2}", _firstSearchHitId, _currentSearchPageNo, SearchString);
                }
            }

            string response = FetchSpecificFromTwitter(t, Yedda.Twitter.ActionType.Search, SearchString);
            if (string.IsNullOrEmpty(response))
            {
                NoData(t.AccountInfo, Yedda.Twitter.ActionType.Search);
                GlobalEventHandler.CallShowErrorMessage("Communications Error");
                return null;
            }
            else
            {
                ErrorCleared(t.AccountInfo, Yedda.Twitter.ActionType.Search);
            }
            var Items = Library.status.DeserializeArrayFromJSON(response, t.AccountInfo, StatusTypes.SearchResult);
            if (Items == null) return null;
            if (Items.Length > 0)
            {
                foreach (status item in Items)
                {
                    item.SearchTerm = SearchString;
                }
                if (_currentSearchPageNo == 1)
                {
                    _firstSearchHitId = Items[0].id;
                }
            }
            return Items;
        }

        public PockeTwit.Library.status[] GetFavorites()
        {
            List<Library.status> TempLine = new List<PockeTwit.Library.status>();
            foreach (Yedda.Twitter t in TwitterConnections)
            {
                if (t.AccountInfo.Enabled && t.FavoritesWork)
                {
                    try
                    {
                        string response = FetchSpecificFromTwitter(t, Yedda.Twitter.ActionType.Favorites);
                        Library.status[] NewStats = Library.status.Deserialize(response, t.AccountInfo, PockeTwit.Library.StatusTypes.Normal);
                        TempLine.AddRange(NewStats);
                        ErrorCleared(t.AccountInfo, Yedda.Twitter.ActionType.Favorites);
                    }
                    catch
                    {
                        NoData(t.AccountInfo, Yedda.Twitter.ActionType.Favorites);
                        GlobalEventHandler.CallShowErrorMessage("Communications Error");
                    }
                }
            }
            TempLine.Sort();
            return TempLine.ToArray();
        }
        public PockeTwit.Library.status[] GetPublicTimeLine()
        {
            bool twitterDone = false;
            List<Library.status> TempLine = new List<PockeTwit.Library.status>();
            foreach (Yedda.Twitter t in TwitterConnections)
            {
                if (!(twitterDone && t.AccountInfo.Server == Yedda.Twitter.TwitterServer.twitter))
                {
                    if (t.AccountInfo.Enabled)
                    {
                        try
                        {
                            string response = FetchSpecificFromTwitter(t, Yedda.Twitter.ActionType.Public_Timeline, null);
                            if (!string.IsNullOrEmpty(response))
                            {
                                Library.status[] NewStats = Library.status.Deserialize(response, t.AccountInfo, PockeTwit.Library.StatusTypes.Normal);
                                TempLine.AddRange(NewStats);
                            }
                        }
                        catch
                        {
                        }
                    }
                    if (t.AccountInfo.Server == Yedda.Twitter.TwitterServer.twitter)
                    {
                        twitterDone = true;
                    }
                }
            }
            TempLine.Sort();
            return TempLine.ToArray();
        }
        public Library.status[] GetUserTimeLine(Yedda.Twitter t, string UserID)
        {
            string response = FetchSpecificFromTwitter(t, Yedda.Twitter.ActionType.Show, UserID);
            if (string.IsNullOrEmpty(response))
            {
                NoData(t.AccountInfo, Yedda.Twitter.ActionType.Show);
                GlobalEventHandler.CallShowErrorMessage("Communications Error");
                return null;
            }
            else
            {
                ErrorCleared(t.AccountInfo, Yedda.Twitter.ActionType.Show);
            }
            return Library.status.Deserialize(response, t.AccountInfo);
        }

        public Library.status[] GetGroupedTimeLine(ISpecialTimeLine t, Yedda.Twitter.PagingMode pagingMode)
        {
            TimeLineType timeLineType = TimeLineType.Friends;
            if(t is SavedSearchTimeLine)
            {
                var savedLine = (SavedSearchTimeLine) t;
                if(!savedLine.autoUpdate)
                {
                    return GetRegularSavedSearchTimeLine(savedLine, pagingMode);
                }
                timeLineType = TimeLineType.Searches;
            }
            return LocalStorage.DataBaseUtility.GetList(timeLineType, ClientSettings.MaxTweets, t.GetConstraints()).ToArray();
        }

        private Library.status[] GetRegularSavedSearchTimeLine(SavedSearchTimeLine searchLine, Yedda.Twitter.PagingMode pagingMode)
        {
            var TwitterConn = new Twitter
            {
                AccountInfo =
                {
                    ServerURL = ClientSettings.DefaultAccount.ServerURL,
                    UserName = ClientSettings.DefaultAccount.UserName,
                    Password = ClientSettings.DefaultAccount.Password,
                    Enabled = ClientSettings.DefaultAccount.Enabled
                }
            };
            status[] items = SearchTwitter(TwitterConn, searchLine.SearchPhrase, pagingMode);
            
            return items;
        }

        private void GetMessagesTimeLine(object o)
        {
            GetMessagesTimeLine(true);
        }
        private void GetMessagesTimeLine()
        {
            GetMessagesTimeLine(true);
        }
        private void GetMessagesTimeLine(bool Notify)
        {
            if (!GlobalEventHandler.MessagesUpdating)
            {
                try
                {
                    updateTimer.Enabled = false;
                    GlobalEventHandler.NotifyTimeLineFetching(TimeLineType.Messages);
                    List<Library.status> TempLine = new List<PockeTwit.Library.status>();
                    GetMessagesList(TempLine);
                    LocalStorage.DataBaseUtility.SaveItems(TempLine);
                    if (MessagesUpdated != null && TempLine.Count > 0)
                    {
                        if (Notify)
                        {
                            MessagesUpdated();
                        }
                        else
                        {
                            HoldNewMessages = TempLine.Count;
                        }
                    }
                    TempLine.Clear();
                    TempLine.TrimExcess();
                }
                catch (NullReferenceException)
                {
                }
                finally
                {
                    GlobalEventHandler.NotifyTimeLineDone(TimeLineType.Messages);
                    if (ClientSettings.UpdateMinutes > 0 && updateTimer!=null)
                    {
                        updateTimer.Enabled = true;
                    }
                }
            }
        }

        private void GetMessagesList(List<Library.status> TempLine)
        {
            lock (TwitterConnections)
            {
                foreach (Yedda.Twitter t in TwitterConnections)
                {
                    if (t.AccountInfo.Enabled && t.AccountInfo.ServerURL.ServerType != Yedda.Twitter.TwitterServer.pingfm)
                    {
                        string response = FetchSpecificFromTwitter(t, Yedda.Twitter.ActionType.Replies);
                        if (!string.IsNullOrEmpty(response))
                        {
                            try
                            {
                                Library.status[] NewStats = Library.status.Deserialize(response, t.AccountInfo, PockeTwit.Library.StatusTypes.Reply);
                                foreach (Library.status s in NewStats)
                                {
                                    s.TypeofMessage = PockeTwit.Library.StatusTypes.Reply;
                                    if(DateTime.Now.Subtract(s.createdAt)<new TimeSpan(10,0,0,0,0))
                                    {
                                        TempLine.Add(s);
                                    }
                                }
                                //TempLine.AddRange(NewStats);
                                ErrorCleared(t.AccountInfo, Yedda.Twitter.ActionType.Replies);
                            }
                            catch
                            {
                                NoData(t.AccountInfo, Yedda.Twitter.ActionType.Replies);
                                GlobalEventHandler.CallShowErrorMessage("Communications Error");
                            }
                        }
                        else
                        {
                            NoData(t.AccountInfo, Yedda.Twitter.ActionType.Replies);
                            GlobalEventHandler.CallShowErrorMessage("Communications Error");
                        }
                        ////I HATE DIRECT MESSAGES

                        if (t.DirectMessagesWork)
                        {
                            response = FetchSpecificFromTwitter(t, Yedda.Twitter.ActionType.Direct_Messages);
                            if (!string.IsNullOrEmpty(response))
                            {
                                try
                                {
                                    Library.status[] NewStats = Library.status.FromDirectReplies(response, t.AccountInfo);
                                    foreach (Library.status s in NewStats)
                                    {
                                        s.TypeofMessage = PockeTwit.Library.StatusTypes.Direct;
                                        if (DateTime.Now.Subtract(s.createdAt) < new TimeSpan(10,0,0,0,0))
                                        {
                                            TempLine.Add(s);
                                        }
                                    }
                                    //TempLine.AddRange(NewStats);
                                    ErrorCleared(t.AccountInfo, Yedda.Twitter.ActionType.Direct_Messages);
                                }
                                catch
                                {
                                    NoData(t.AccountInfo, Yedda.Twitter.ActionType.Direct_Messages);
                                    GlobalEventHandler.CallShowErrorMessage("Communications Error");
                                }
                            }
                            else
                            {
                                NoData(t.AccountInfo, Yedda.Twitter.ActionType.Direct_Messages);
                                GlobalEventHandler.CallShowErrorMessage("Communications Error");
                            }
                        }
                    }
                }
            }
        }
        private void GetFriendsTimeLine(object o)
        {
            GetFriendsTimeLine(true);
        }
        private void GetFriendsTimeLine()
        {
            GetFriendsTimeLine(true);
        }
        private void GetFriendsTimeLine(bool Notify)
        {
            try
            {
                if (!GlobalEventHandler.FriendsUpdating)
                {
                    if (updateTimer != null)
                    {
                        updateTimer.Enabled = false;
                    }
                    GlobalEventHandler.NotifyTimeLineFetching(TimeLineType.Friends);
                    List<Library.status> TempLine = new List<PockeTwit.Library.status>();
                    foreach (Yedda.Twitter t in TwitterConnections)
                    {
                        if (t.AccountInfo.Enabled && t.AccountInfo.ServerURL.ServerType != Yedda.Twitter.TwitterServer.pingfm)
                        {
                            string response = FetchSpecificFromTwitter(t, Yedda.Twitter.ActionType.Friends_Timeline);

                            if (!string.IsNullOrEmpty(response))
                            {
                                try
                                {
                                    Library.status[] NewStats = Library.status.Deserialize(response, t.AccountInfo);
                                    TempLine.AddRange(NewStats);
                                    ErrorCleared(t.AccountInfo, Yedda.Twitter.ActionType.Friends_Timeline);
                                }
                                catch
                                {
                                    NoData(t.AccountInfo, Yedda.Twitter.ActionType.Friends_Timeline);
                                    GlobalEventHandler.CallShowErrorMessage("Communications Error");
                                }
                            }
                            else
                            {
                                NoData(t.AccountInfo, Yedda.Twitter.ActionType.Friends_Timeline);
                                GlobalEventHandler.CallShowErrorMessage("Communications Error");
                            }
                        }
                    }
                    int NewItems = 0;
                    if (TempLine.Count > 0)
                    {
                        
                        NewItems = TempLine.Count;
                        //Don't count items that were excluded from main friends timeline.
                        foreach (Library.status s in TempLine)
                        {
                            if (SpecialTimeLinesRepository.UserIsExcluded(s.user.id)) { NewItems--; }
                        }
                        int ItemsFromCache = ClientSettings.MaxTweets - NewItems;

                        if (ItemsFromCache > 0)
                        {
                            List<Library.status> OldItems = LocalStorage.DataBaseUtility.GetList(TimeLineType.Friends, ItemsFromCache);
                            LocalStorage.DataBaseUtility.SaveItems(TempLine);
                            TempLine.AddRange(OldItems);
                        }
                        else
                        {
                            LocalStorage.DataBaseUtility.SaveItems(TempLine);
                        }
                        TempLine.Sort();
                       
                    }
                    if (FriendsUpdated != null && NewItems > 0)
                    {
                        if (Notify)
                        {
                            FriendsUpdated();
                        }
                        else
                        {
                            HoldNewFriends = NewItems;
                        }
                    }
                    TempLine.Clear();
                    TempLine.TrimExcess();

                    
                }
            }
            catch (NullReferenceException)
            {

            }
            finally
            {
                GlobalEventHandler.NotifyTimeLineDone(TimeLineType.Friends);
                if (ClientSettings.UpdateMinutes > 0)
                {
                    if (updateTimer != null)
                    {
                        updateTimer.Enabled = true;
                    }
                }   
            }
        }

        public void GetSavedSearches()
        {
            //return if no saved searches defined
            ISpecialTimeLine[] specialTimeLines = SpecialTimeLinesRepository.GetList(SpecialTimeLinesRepository.TimeLineType.SavedSearch);
            if(specialTimeLines.Length==0){ return; }
            if (GlobalEventHandler.SearchesUpdating) { return; }

            var TwitterConn = new Twitter
                                  {
                                      AccountInfo =
                                          {
                                              ServerURL = ClientSettings.DefaultAccount.ServerURL,
                                              UserName = ClientSettings.DefaultAccount.UserName,
                                              Password = ClientSettings.DefaultAccount.Password,
                                              Enabled = ClientSettings.DefaultAccount.Enabled
                                          }
                                  };

            updateTimer.Enabled = false;
            GlobalEventHandler.NotifyTimeLineFetching(TimeLineType.Searches);
            var tempLine = new List<Library.status>();

            foreach (var specialTimeLine in specialTimeLines)
            {
                SavedSearchTimeLine searchLine = (SavedSearchTimeLine) specialTimeLine;
                if (searchLine.autoUpdate)
                {
                    //Need a way to specify "since_id" here too.
                    status[] Items = SearchTwitter(TwitterConn, searchLine.SearchPhrase, Yedda.Twitter.PagingMode.None);
                    if (Items != null)
                    {
                        tempLine.AddRange(Items);
                    }
                }
            }

            if (tempLine.Count > 0)
            {
                LocalStorage.DataBaseUtility.SaveItems(tempLine);
                SearchesUpdated();
            }
            GlobalEventHandler.NotifyTimeLineDone(TimeLineType.Searches);
            tempLine.Clear();
            tempLine.TrimExcess();
            updateTimer.Enabled = true;
        }

        

        private string FetchSpecificFromTwitter(Yedda.Twitter t, Yedda.Twitter.ActionType TimelineType)
        {
            return FetchSpecificFromTwitter(t, TimelineType, null);
        }
        private string FetchSpecificFromTwitter(Yedda.Twitter t, Yedda.Twitter.ActionType TimelineType, string AdditionalParameter)
        {
            string response = "";
            try
            {
                switch (TimelineType)
                {
                        
                    case Yedda.Twitter.ActionType.Direct_Messages:
                        string LastDirectID = LocalStorage.DataBaseUtility.GetLatestItem(t.AccountInfo, TimeLineType.Direct);
                        if (string.IsNullOrEmpty(LastDirectID))
                        {
                            response = t.GetDirectTimeLineSince(null);
                        }
                        else
                        {
                            response = t.GetDirectTimeLineSince(LastDirectID);
                        }
                        break;
                    case Yedda.Twitter.ActionType.Friends_Timeline:
                        if (!t.BigTimeLines)
                        {
                            response = t.GetFriendsTimeline(Yedda.Twitter.OutputFormatType.XML);
                        }
else
                        {
                            string LastStatusID = LocalStorage.DataBaseUtility.GetLatestItem(t.AccountInfo, TimeLineType.Friends);
                            if (string.IsNullOrEmpty(LastStatusID))
                            {
                                response = t.GetFriendsTimeLineMax(Yedda.Twitter.OutputFormatType.XML);
                            }
                            else
                            {
                                response = t.GetFriendsTimeLineSince(Yedda.Twitter.OutputFormatType.XML, LastStatusID);
                            }
                        }
                        break;
                    case Yedda.Twitter.ActionType.Public_Timeline:
                        response = t.GetPublicTimeline(Yedda.Twitter.OutputFormatType.XML);
                        break;
                    case Yedda.Twitter.ActionType.Replies:
                        if (!t.BigTimeLines)
                        {
                            response = t.GetRepliesTimeLine(Yedda.Twitter.OutputFormatType.XML);
                        }
                        else
                        {
                            string LastReplyID = LocalStorage.DataBaseUtility.GetLatestItem(t.AccountInfo, TimeLineType.Replies);
                            if (string.IsNullOrEmpty(LastReplyID))
                            {
                                response = t.GetRepliesTimeLine(Yedda.Twitter.OutputFormatType.XML);
                            }
                            else
                            {
                                response = t.GetRepliesTimeLineSince(Yedda.Twitter.OutputFormatType.XML, LastReplyID);
                            }
                        }
                        break;
                    case Yedda.Twitter.ActionType.User_Timeline:
                        response = t.GetUserTimeline(AdditionalParameter, Yedda.Twitter.OutputFormatType.XML);
                        break;
                    case Yedda.Twitter.ActionType.Show:
                        response = t.GetUserTimeline(AdditionalParameter, Yedda.Twitter.OutputFormatType.XML);
                        break;
                    case Yedda.Twitter.ActionType.Favorites:
                        response = t.GetFavorites();
                        break;
                    case Yedda.Twitter.ActionType.Search:
                        string LastSearchID = LocalStorage.DataBaseUtility.GetLatestItem(t.AccountInfo,
                                                                                         TimeLineType.Searches, " statuses.SearchTerm='" + AdditionalParameter + "' AND ");
                        if (string.IsNullOrEmpty(LastSearchID))
                        {
                            return t.SearchFor(AdditionalParameter);
                        }
                        return t.SearchFor(AdditionalParameter, LastSearchID);
                }
            }
            catch (Exception)
            {
                
            }
            return response;
        }
    }
}
