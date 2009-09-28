using System;

using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Xml.Serialization;
using PockeTwit.FingerUI;
using PockeTwit.FingerUI.Menu;
using PockeTwit.Library;
using PockeTwit.MediaServices;
using PockeTwit.NotificationsCode;
using PockeTwit.OtherServices;
using PockeTwit.SpecialTimelines;
using PockeTwit.TimeLines;
using Microsoft.WindowsCE.Forms;
using Yedda;


namespace PockeTwit
{
    public partial class TweetList : Form
    {
        private class HistoryItem
        {
            public string Argument;
            public Yedda.Twitter.ActionType Action;
            public Yedda.Twitter.Account Account;
            public int SelectedItemIndex = -1;
            public int itemsOffset = -1;
            public object ItemInfo = null;
        }

        private Stack<HistoryItem> History = new Stack<HistoryItem>();

        #region�Fields�(12)�
        private MsgWindow MsgWin;

        private UpgradeChecker Checker;

        private Yedda.Twitter.Account CurrentlySelectedAccount;
        private List<Yedda.Twitter> TwitterConnections = new List<Yedda.Twitter>();
        private Dictionary<Yedda.Twitter, Following> FollowingDictionary = new Dictionary<Yedda.Twitter, Following>();
        private TimelineManagement Manager;
        private NotificationHandler Notifyer;
        private bool IsLoaded;
        private string ShowUserID;
        private bool StartBackground = false;
        private ISpecialTimeLine currentSpecialTimeLine = null;

        #region MenuItems
        #region LeftMenu
        FingerUI.Menu.SideMenuItem BackMenuItem;

        FingerUI.Menu.SideMenuItem FriendsTimeLineMenuItem;
        FingerUI.Menu.SideMenuItem RefreshFriendsTimeLineMenuItem;
        FingerUI.Menu.SideMenuItem MessagesMenuItem;
        FingerUI.Menu.SideMenuItem RefreshMessagesMenuItem;
        
        FingerUI.Menu.SideMenuItem PublicMenuItem;
        FingerUI.Menu.SideMenuItem SearchMenuItem;
        FingerUI.Menu.SideMenuItem ViewFavoritesMenuItem;
        FingerUI.Menu.SideMenuItem GroupsMenuItem;
        
        FingerUI.Menu.SideMenuItem OtherGlobalMenuItem;

        FingerUI.Menu.SideMenuItem PostUpdateMenuItem;
        FingerUI.Menu.SideMenuItem SettingsMenuItem;
        FingerUI.Menu.SideMenuItem FollowUserMenuItem;
        FingerUI.Menu.SideMenuItem AccountsSettingsMenuItem;
        FingerUI.Menu.SideMenuItem AdvancedSettingsMenuItem;
        FingerUI.Menu.SideMenuItem AvatarSettingsMenuItem;
        FingerUI.Menu.SideMenuItem GroupSettingsMenuItem;
        FingerUI.Menu.SideMenuItem NotificationSettingsMenuItem;
        FingerUI.Menu.SideMenuItem MediaServiceSettingsMenuItem;
        FingerUI.Menu.SideMenuItem OtherSettingsMenuItem;
        FingerUI.Menu.SideMenuItem UISettingsMenuItem;
        
        FingerUI.Menu.SideMenuItem AboutMenuItem;

        FingerUI.Menu.SideMenuItem WindowMenuItem;
        FingerUI.Menu.SideMenuItem FullScreenMenuItem;
        FingerUI.Menu.SideMenuItem MinimizeMenuItem;
        FingerUI.Menu.SideMenuItem ExitMenuItem;
        #endregion
        #region RightMenu
        FingerUI.Menu.SideMenuItem ConversationMenuItem;
        FingerUI.Menu.SideMenuItem DeleteStatusMenuItem;
        FingerUI.Menu.SideMenuItem ResponsesMenuItem;

        FingerUI.Menu.SideMenuItem ReplyMenuItem;
        FingerUI.Menu.SideMenuItem DirectMenuItem;

        FingerUI.Menu.SideMenuItem EmailMenuItem;
        FingerUI.Menu.SideMenuItem QuoteMenuItem;
        FingerUI.Menu.SideMenuItem ToggleFavoriteMenuItem;
        FingerUI.Menu.SideMenuItem UserTimelineMenuItem;
        FingerUI.Menu.SideMenuItem ProfilePageMenuItem;
        FingerUI.Menu.SideMenuItem FollowMenuItem;
        FingerUI.Menu.SideMenuItem MoveToGroupMenuItem;
        FingerUI.Menu.SideMenuItem CopyToGroupMenuItem;
        private FingerUI.Menu.SideMenuItem CopyNewGroupMenuItem;
        private FingerUI.Menu.SideMenuItem MoveNewGroupMenuItem;
        #endregion
        #endregion MenuItems

        #endregion�Fields�

        #region�Constructors�(1)�
        public TweetList(bool InBackGround, string[] args)
        {
            //throw new Exception("Bam!");
            StartBackground = InBackGround;
            Microsoft.WindowsCE.Forms.MobileDevice.Hibernate += new EventHandler(MobileDevice_Hibernate);
            if (InBackGround)
            {
                this.Hide();
            }
            else
            {
                if (ClientSettings.IsMaximized)
                {
                    this.WindowState = FormWindowState.Maximized;
                    this.Menu = null;
                }
                else
                {
                    AddMainMenuItems();
                }
            }
            InitializeComponent();

            this.MsgWin = new MsgWindow(this);

            progressBar1.Visible = false;
            lblProgress.Visible = true;
            lblProgress.Text = PockeTwit.Localization.XmlBasedResourceManager.GetString("Initializing");
            if (DetectDevice.DeviceType == DeviceType.Professional)
            {
                inputPanel1 = new Microsoft.WindowsCE.Forms.InputPanel();
                TodayScreenRegistrySetup.CheckTodayScreenInstalled();
            }
            if (UpgradeChecker.devBuild)
            {
                this.lblTitle.Text = PockeTwit.Localization.XmlBasedResourceManager.GetString("Launching PockeTwit Dev");
            }
            
            SizeF currentScreen = this.CurrentAutoScaleDimensions;
            if (currentScreen.Height == 192)
            {
                statList.MaxVelocity = 45;
            }
            else
            {
                statList.MaxVelocity = 45;
            }
            ClientSettings.TextHeight = currentScreen.Height;

            
            this.Visible = !InBackGround;
            statList.Visible = false;
            PockeTwit.Themes.FormColors.SetColors(this);
            PockeTwit.Localization.XmlBasedResourceManager.LocalizeForm(this);

            this.Refresh();
            
            Application.DoEvents();
            LocalStorage.DataBaseUtility.CheckDBSchema();

            statList.Progress += new KListControl.delProgress(statList_Progress);
            SpecialTimeLinesRepository.Load();

            //First run means no settings exist
            if (ClientSettings.AccountsList.Count == 0)
            {
                ClearSettings(); 
            }

            if (StartBackground) { this.Hide(); }
            CreateLeftMenu();
            CreateRightMenu();
            if (!SetEverythingUp())
            {
                if (Notifyer != null) { Notifyer.ShutDown(); }
                Application.Exit();
                ThrottledArtGrabber.running = false;
                return;
            }
            SwitchToDone();

            ProcessArgs(args);

            if(!string.IsNullOrEmpty(ClientSettings.PreviousMediaService))
            {
                var switcher = new SwitchToTweetPhoto();
                switcher.ShowDialog();
            }
        }

        void statList_Progress(int itemnumber, int totalnumber)
        {
            if (itemnumber >= totalnumber)
            {
                lblTitle.Visible = false;
                progressBar1.Visible = false;
                return;
            }
            lblTitle.Visible = true;
            progressBar1.Visible = true;
            progressBar1.Maximum = totalnumber;
            progressBar1.Minimum = 0;
            progressBar1.Value = itemnumber;
            if (lblProgress.Visible)
            {
                lblProgress.Visible = false;
                this.Refresh();
            }
            
        }

        private void AddMainMenuItems()
        {
            if(this.Menu==null)
            {
                this.Menu = mainMenu1;
            }
        }

        void MobileDevice_Hibernate(object sender, EventArgs e)
        {
            //MessageBox.Show("The device is running low on memory. You may want to close PockeTwit or other applications.");
        }

        #endregion�Constructors�

        #region�Delegates�and�Events�(2)�


        //�Delegates�(2)�
        private delegate void delSetWindowState(FormWindowState state);
        private delegate void delAddStatuses(Library.status[] arrayOfStats, bool KeepPosition);
        private delegate void delChangeCursor(Cursor CursorToset);
        private delegate void delNotify(int Count);
        private delegate bool delBool();
        private delegate void delNone();

        #endregion�Delegates�and�Events�

        #region�Methods�(38)�


        //�Private�Methods�(38)�


        public bool IsFocused()
        {
            if (InvokeRequired)
            {
                delBool d = new delBool(IsFocused);
                bool invokeRetured = (bool)this.Invoke(d, null);
                return invokeRetured;
            }
            else
            {
                try
                {
                    return this.Focused | statList.Focused;
                }
                catch (ObjectDisposedException)
                {
                    return false;
                }
            }

        }

        private Yedda.Twitter GetMatchingConnection(Yedda.Twitter.Account AccountInfo)
        {
            if (AccountInfo == null)
            {
                return TwitterConnections[0];
            }
            foreach (Yedda.Twitter conn in TwitterConnections)
            {
                if (conn.AccountInfo.Equals(AccountInfo))
                {
                    return conn;
                }
            }
            return TwitterConnections[0];
        }

        private void AddStatusesToList(Library.status[] mergedstatuses)
        {
            AddStatusesToList(mergedstatuses, false);
        }
        private void AddStatusesToList(Library.status[] mergedstatuses, bool KeepPosition)
        {
            if(mergedstatuses==null){return;} //Why would this turn up null? Comm error?
            if (mergedstatuses.Length == 0) { return; }
            if (InvokeRequired)
            {
                delAddStatuses d = AddStatusesToList;
                this.Invoke(d, new object[] { mergedstatuses, KeepPosition });
            }
            else
            {
                int newItems = 0;
                if(KeepPosition)
                {
                    IDisplayItem selectedItem = statList.SelectedItem;
                    if (selectedItem != null && selectedItem is StatusItem)
                    {
                        TimelineManagement.TimeLineType t = TimelineManagement.TimeLineType.Friends;
                        if (statList.CurrentList() == "Messages_TimeLine")
                        {
                            t = TimelineManagement.TimeLineType.Messages;
                        }
                        string constraints = "";
                        if (currentSpecialTimeLine != null)
                        {
                            constraints = currentSpecialTimeLine.GetConstraints();
                        }
                        newItems = LocalStorage.DataBaseUtility.CountItemsNewerThan(t, (selectedItem as StatusItem).Tweet.id, constraints);
                    }
                }
            
                int oldOffset = 0;
                int oldIndex = 0;
                
                oldIndex = statList.SelectedIndex;
                oldOffset = statList.YOffset - (ClientSettings.ItemHeight * oldIndex);

                statList.Clear();
                foreach (Library.status stat in mergedstatuses)
                {
                    if (stat == null || stat.user == null || stat.user.screen_name == null) continue;
                    StatusItem item = new StatusItem {Tweet = stat};
                    statList.AddItem(item);
                }
                IDisplayItem currentItem = null;
                if (!ClientSettings.AutoScrollToTop)
                {
                    if (oldIndex >= 0 && KeepPosition && newItems < ClientSettings.MaxTweets)
                    {
                        try
                        {
                            statList.SelectedItem = statList[newItems];
                            currentItem = statList.SelectedItem;
                            statList.YOffset = oldOffset + (newItems * ClientSettings.ItemHeight);
                        }
                        catch (KeyNotFoundException)
                        {
                            //What to do?
                        }
                    }
                    else
                    {
                        statList.JumpToLastSelected();
                        currentItem = statList[0];
                    }
                }
                else
                {
                    statList.SelectedItem = statList[0];
                    currentItem = statList.SelectedItem;
                    statList.YOffset = 0;
                }

                if (currentItem != null)
                {
                    if (currentItem is StatusItem)
                        CurrentlySelectedAccount = (currentItem as StatusItem).Tweet.Account;
                    UpdateRightMenu();
                }
                statList.Redraw();
                statList.RerenderPortal();
                statList.Repaint();
            }
        }

        private void ChangeCursor(Cursor CursorToset)
        {
            if (InvokeRequired)
            {
                delChangeCursor d = new delChangeCursor(ChangeCursor);
                this.Invoke(d, new object[] { CursorToset });
            }
            else
            {
                Cursor.Current = CursorToset;
            }
        }

        private void ChangeSettings(BaseSettingsForm f)
        {
            this.statList.Visible = false;
            IsLoaded = false;
            if(f.ShowDialog()==DialogResult.Cancel)
            {
                this.statList.Visible = true;
                IsLoaded = true;
                return;
            }
            if(f.NeedsReRender)
            {
                statList.BackColor = ClientSettings.BackColor;
                statList.ForeColor = ClientSettings.ForeColor;
                CreateRightMenu();
                CreateLeftMenu();
                PockeTwit.Themes.FormColors.SetColors(this);
                PockeTwit.Localization.XmlBasedResourceManager.LocalizeForm(this);

                ReloadTimeLine();
                statList.ResetFullScreenColors();
                statList.RerenderBySize();
            }
            if(f.NeedsReset)
            {
                PockeTwit.Localization.LocalizedMessageBox.Show("Your settings changes require that you restart the application.");
                ExitApplication();
            }
            this.statList.Visible = true;
            statList.Redraw();
            f.Close();
            f.Dispose();
        }

        private void ToggleFavorite()
        {
            StatusItem selectedItem = (StatusItem)statList.SelectedItem;
            if (selectedItem == null) { return; }
            if (selectedItem.IsFavorite)
            {
                DestroyFavorite();
                ToggleFavoriteMenuItem.Text = PockeTwit.Localization.XmlBasedResourceManager.GetString("Make Favorite");
            }
            else
            {
                CreateFavoriteAsync();
                ToggleFavoriteMenuItem.Text = PockeTwit.Localization.XmlBasedResourceManager.GetString("Remove Favorite");
            }
        }

        private void CreateFavorite(string ID, Yedda.Twitter.Account AccountInfo)
        {
            GetMatchingConnection(AccountInfo).SetFavorite(ID);
            UpdateRightMenu();
            statList.Repaint();
            ChangeCursor(Cursors.Default);
        }



        private void CreateFavoriteAsync()
        {
            StatusItem selectedItem = (StatusItem)statList.SelectedItem;
            if (selectedItem == null) { return; }
            Yedda.Twitter.Account ChosenAccount = selectedItem.Tweet.Account;

            ChangeCursor(Cursors.WaitCursor);
            selectedItem.IsFavorite = true;

            string ID = selectedItem.Tweet.id;
            System.Threading.ThreadStart ts = delegate { CreateFavorite(ID, ChosenAccount); };
            System.Threading.Thread t = new System.Threading.Thread(ts);
            t.Name = "CreateFavorite";
            t.Start();
        }

        private void DestroyFavorite()
        {
            ChangeCursor(Cursors.WaitCursor);
            StatusItem selectedItem = (StatusItem)statList.SelectedItem;
            string ID = selectedItem.Tweet.id;
            GetMatchingConnection(selectedItem.Tweet.Account).DestroyFavorite(ID);
            selectedItem.IsFavorite = false;
            UpdateRightMenu();
            statList.Repaint();
            ChangeCursor(Cursors.Default);
        }

        private void CreateNewGroup(bool Exclusive)
        {
            StatusItem selectedItem = (StatusItem)statList.SelectedItem;
            if (selectedItem == null) { return; }
            if (selectedItem.Tweet.user == null) { return; }

            using (DefineGroup d = new DefineGroup())
            {
                if (d.ShowDialog() == DialogResult.OK)
                {
                    UserGroupTimeLine t = new UserGroupTimeLine();
                    t.name = d.GroupName;

                    if(AddUserToGroup(t, Exclusive, false))
                    {
                        SpecialTimeLinesRepository.Add(t);

                        AddGroupSelectMenuItem(t);

                        AddAddUserToGroupMenuItem(t);
                        SpecialTimeLinesRepository.Save();
                        if(Exclusive)
                        {
                            Cursor.Current = Cursors.WaitCursor;
                            ReloadTimeLine();
                            Cursor.Current = Cursors.Default;
                        }
                    }
                }
            }
        }
       
        internal void ShowSpecialTimeLine(ISpecialTimeLine t, Yedda.Twitter.PagingMode pagingMode)
        {
            UpdateHistoryPosition();
            currentSpecialTimeLine = t;
            ChangeCursor(Cursors.WaitCursor);
            HistoryItem i = new HistoryItem();
            i.Action = Yedda.Twitter.ActionType.Search;
            i.Argument = t.name;
            i.ItemInfo = t;
            
            History.Push(i);

            SwitchToList(t.ListName);
            statList.ClearVisible();
            AddStatusesToList(Manager.GetGroupedTimeLine(t, pagingMode), false);
            if (t.Timelinetype == SpecialTimeLinesRepository.TimeLineType.SavedSearch)
                statList.AddItem(new MoreResultsItem(this,t));
            ChangeCursor(Cursors.Default);
        }

        private bool AddUserToGroup(UserGroupTimeLine t, bool Exclusive)
        {
            return AddUserToGroup(t, Exclusive, true);
        }
        private bool AddUserToGroup(UserGroupTimeLine t, bool Exclusive, bool ReloadImmediately)
        {
            StatusItem selectedItem = (StatusItem)statList.SelectedItem;
            if (selectedItem == null) { return false; }
            if (selectedItem.Tweet.user == null) { return false; }

            string Message = "";
            switch (Exclusive)
            {
                case true:
                    Message="This will move {0} out of the Friends timeline and into the {1} group.\n\nAre you sure you want to proceed?";
                    break;
                case false:
                    Message = "This will copy {0} into the {1} group and still show them in the Friends timeline.\n\nAre you sure you want to proceed?";

                    break;

            }
            if (PockeTwit.Localization.LocalizedMessageBox.Show(Message, "Group User", MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2, selectedItem.Tweet.user.screen_name, t.name) == DialogResult.Yes)
            {
                t.AddItem(selectedItem.Tweet.user.id, selectedItem.Tweet.user.screen_name, Exclusive);
                SpecialTimeLinesRepository.Save();
            }
            else
            {
                return false;
            }
            if(Exclusive && ReloadImmediately)
            {
                Cursor.Current = Cursors.WaitCursor;
                ReloadTimeLine();
                Cursor.Current = Cursors.Default;
            }
            return true;
        }

        private void ReloadTimeLine()
        {
            if(statList.CurrentList()=="Friends_TimeLine")
            {
                AddStatusesToList(Manager.GetFriendsImmediately(), true);
            }
            else
            {
                if (statList.CurrentList().StartsWith("Grouped") || statList.CurrentList().StartsWith("SavedSearch_TimeLine_"))
                {
                    AddStatusesToList(Manager.GetGroupedTimeLine(currentSpecialTimeLine, Yedda.Twitter.PagingMode.None), true);
                    if (currentSpecialTimeLine.Timelinetype == SpecialTimeLinesRepository.TimeLineType.SavedSearch)
                        statList.AddItem(new MoreResultsItem(this, currentSpecialTimeLine));

                }
            }
            
        }
        private delegate void delFollowUser(Yedda.Twitter.Account a);
        
        private void SetFollowMenu()
        {
            if(ClientSettings.AccountsList.Count==1)
            {
                return;
            }
            StatusItem selectedItem = (StatusItem)statList.SelectedItem;
            if (selectedItem == null) { return; }
            if (selectedItem.Tweet.user == null) { return; }
            Yedda.Twitter Conn = GetMatchingConnection(selectedItem.Tweet.Account);
            
            FollowMenuItem.SubMenuItems.Clear();
            List<Yedda.Twitter.Account> MatchingAccounts = new List<Twitter.Account>();

            foreach (var account in ClientSettings.AccountsList)
            {
                if(account.Server==Conn.AccountInfo.Server)
                {
                    MatchingAccounts.Add(account);
                }
            }

            if (MatchingAccounts.Count > 1)
            {
                FollowMenuItem.Text = PockeTwit.Localization.XmlBasedResourceManager.GetString("Follow...");
                FollowMenuItem.ClickedMethod = null;
                foreach (var account in MatchingAccounts)
                {
                    var account1 = account;
                    delMenuClicked d = () => FollowUser(account1);
                    var item = new SideMenuItem(d, account.ToString(), statList.RightMenu);
                    FollowMenuItem.SubMenuItems.Add(item);
                }
                return;
            }

            delMenuClicked  followClicked = () => FollowUser(MatchingAccounts[0]);
            FollowMenuItem.Text = PockeTwit.Localization.XmlBasedResourceManager.GetString("Follow");
            FollowMenuItem.ClickedMethod = followClicked;

        }

        private void ToggleFollow()
        {
            StatusItem selectedItem = (StatusItem)statList.SelectedItem;
            if (selectedItem == null) { return; }
            if (selectedItem.Tweet.user == null) { return; }
            Yedda.Twitter Conn = GetMatchingConnection(selectedItem.Tweet.Account);
            if (FollowingDictionary[Conn].IsFollowing(selectedItem.Tweet.user))
            {
                StopFollowingUser();
                FollowMenuItem.Text = PockeTwit.Localization.XmlBasedResourceManager.GetString("Follow");
            }
            else
            {
                FollowUser();
                FollowMenuItem.Text = PockeTwit.Localization.XmlBasedResourceManager.GetString("Stop Following");
            }
        }
        private void FollowUser()
        {
            StatusItem selectedItem = (StatusItem)statList.SelectedItem;
            if (selectedItem == null) { return; }
            if (selectedItem.Tweet.user == null) { return; }
            Yedda.Twitter Conn = GetMatchingConnection(selectedItem.Tweet.Account);
            FollowUser(Conn.AccountInfo);
        }
        private void FollowUser(Yedda.Twitter.Account account)
        {
            StatusItem selectedItem = (StatusItem)statList.SelectedItem;
            if (selectedItem == null) { return; }
            if (selectedItem.Tweet.user == null) { return; }
            ChangeCursor(Cursors.WaitCursor);
            Yedda.Twitter Conn = GetMatchingConnection(selectedItem.Tweet.Account);
            Conn.FollowUser(selectedItem.Tweet.user.screen_name);
            FollowingDictionary[Conn].AddUser(selectedItem.Tweet.user);
            UpdateRightMenu();
            ChangeCursor(Cursors.Default);
        }
        private void StopFollowingUser()
        {
            if (statList.SelectedItem == null) { return; }
            StatusItem selectedItem = (StatusItem)statList.SelectedItem;
            Yedda.Twitter Conn = GetMatchingConnection(selectedItem.Tweet.Account);
            if (PockeTwit.Localization.LocalizedMessageBox.Show("Are you sure you want to stop following {0}?", "Stop Following", MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2, selectedItem.Tweet.user.screen_name) == DialogResult.Yes)
            {
                ChangeCursor(Cursors.WaitCursor);
                Conn.StopFollowingUser(selectedItem.Tweet.user.screen_name);
                FollowingDictionary[Conn].StopFollowing(selectedItem.Tweet.user);
                UpdateRightMenu();
                ChangeCursor(Cursors.Default);
            }

        }


        private void SendDirectMessage()
        {
            if (statList.SelectedItem == null) { return; }
            StatusItem selectedItem = (StatusItem)statList.SelectedItem;
            string User = selectedItem.Tweet.user.screen_name;
            SetStatus("d " + User, selectedItem.Tweet.id);
        }

        private void SendReply()
        {
            if (statList.SelectedItem == null) { return; }
            StatusItem selectedItem = (StatusItem)statList.SelectedItem;
            string User = selectedItem.Tweet.user.screen_name;
            if (selectedItem.Tweet.isDirect)
            {
                if (PockeTwit.Localization.LocalizedMessageBox.Show("Are you sure you want to reply to a Direct Message?", "Reply?", MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation, MessageBoxDefaultButton.Button2) == DialogResult.No)
                {
                    SendDirectMessage();
                    return;
                }
            }
            SetStatus("@" + User, selectedItem.Tweet.id);
        }

        private void CreateLeftMenu()
        {
            BackMenuItem = new FingerUI.Menu.SideMenuItem(this.GoBackInHistory, "Back", statList.LeftMenu);
            BackMenuItem.CanHide = true;

            AboutMenuItem = new FingerUI.Menu.SideMenuItem(this.ShowAbout, "About/Feedback", statList.LeftMenu);

            FriendsTimeLineMenuItem = new FingerUI.Menu.SideMenuItem(this.ShowFriendsTimeLine, "Friends Timeline", statList.LeftMenu, "Friends_TimeLine");
            RefreshFriendsTimeLineMenuItem = new FingerUI.Menu.SideMenuItem(this.RefreshFriendsTimeLine, "Refresh Friends", statList.LeftMenu, "Friends_TimeLine");
            MessagesMenuItem = new FingerUI.Menu.SideMenuItem(this.ShowMessagesTimeLine, "Messages", statList.LeftMenu, "Messages_TimeLine");
            RefreshMessagesMenuItem = new FingerUI.Menu.SideMenuItem(this.RefreshMessagesTimeLine, "Refresh Messages", statList.LeftMenu, "Messages_TimeLine");
            PublicMenuItem = new FingerUI.Menu.SideMenuItem(this.ShowPublicTimeLine, "Public Timeline", statList.LeftMenu);
            SearchMenuItem = new FingerUI.Menu.SideMenuItem(this.TwitterSearch, "Search/Local", statList.LeftMenu);
            ViewFavoritesMenuItem = new FingerUI.Menu.SideMenuItem(this.ShowFavorites, "View Favorites", statList.LeftMenu);
            FollowUserMenuItem = new SideMenuItem(this.FollowUserClicked, "Follow User", statList.LeftMenu);

            OtherGlobalMenuItem = new FingerUI.Menu.SideMenuItem(null, "Other ...", statList.LeftMenu);
            OtherGlobalMenuItem.SubMenuItems.Add(SearchMenuItem);
            OtherGlobalMenuItem.SubMenuItems.Add(PublicMenuItem);
            OtherGlobalMenuItem.SubMenuItems.Add(ViewFavoritesMenuItem);
            OtherGlobalMenuItem.SubMenuItems.Add(FollowUserMenuItem);
            OtherGlobalMenuItem.SubMenuItems.Add(AboutMenuItem);

            

            GroupsMenuItem = new FingerUI.Menu.SideMenuItem(null, "Groups ...", statList.LeftMenu);
            GroupsMenuItem.Visible = false;
            //TimeLinesMenuItem.SubMenuItems.Add(GroupsMenuItem);
            
            PostUpdateMenuItem = new FingerUI.Menu.SideMenuItem(this.SetStatus, "Post Update", statList.LeftMenu);
            
            //MapMenuItem = new FingerUI.Menu.SideMenuItem(this.MapList, "Map These", statList.LeftMenu);

            delMenuClicked showAccounts = () => this.ChangeSettings(new AccountsForm());
            delMenuClicked showAdvanced = () => this.ChangeSettings(new SettingsHandler.AdvancedForm());
            delMenuClicked showAvatar = () => this.ChangeSettings(new AvatarSettings());
            delMenuClicked showNotification = () => this.ChangeSettings(new SettingsHandler.NotificationSettings());
            delMenuClicked showOther = () => this.ChangeSettings(new OtherSettings());
            delMenuClicked showUISettings = () => this.ChangeSettings(new UISettings());
            delMenuClicked showGroupSettings = () => this.ChangeSettings(new SettingsHandler.GroupManagement());
            delMenuClicked showMediaServiceSettings = () => this.ChangeSettings(new MediaService());

            WindowMenuItem = new FingerUI.Menu.SideMenuItem(null, "Window ...", statList.LeftMenu);

            FullScreenMenuItem = new FingerUI.Menu.SideMenuItem(ToggleFullScreen, "Toggle FullScreen", statList.LeftMenu);
            MinimizeMenuItem = new FingerUI.Menu.SideMenuItem(this.Minimize, "Minimize", statList.LeftMenu);
            ExitMenuItem = new FingerUI.Menu.SideMenuItem(this.ExitApplication, "Exit", statList.LeftMenu);

            WindowMenuItem.SubMenuItems.Add(FullScreenMenuItem);
            WindowMenuItem.SubMenuItems.Add(MinimizeMenuItem);
            
            //SettingsMenuItem = new FingerUI.Menu.SideMenuItem(this.ChangeSettings, "Settings", statList.LeftMenu);
            SettingsMenuItem = new FingerUI.Menu.SideMenuItem(null, "Settings...", statList.LeftMenu);
            AccountsSettingsMenuItem = new FingerUI.Menu.SideMenuItem(showAccounts, "Accounts", statList.LeftMenu);
            AdvancedSettingsMenuItem = new FingerUI.Menu.SideMenuItem(showAdvanced, "Advanced", statList.LeftMenu);
            AvatarSettingsMenuItem = new FingerUI.Menu.SideMenuItem(showAvatar, "Avatar", statList.LeftMenu);
            GroupSettingsMenuItem = new FingerUI.Menu.SideMenuItem(showGroupSettings, "Manage Groups", statList.LeftMenu);
            MediaServiceSettingsMenuItem = new FingerUI.Menu.SideMenuItem(showMediaServiceSettings, "Media Service", statList.LeftMenu);
            NotificationSettingsMenuItem = new FingerUI.Menu.SideMenuItem(showNotification, "Notifications", statList.LeftMenu);
            OtherSettingsMenuItem = new FingerUI.Menu.SideMenuItem(showOther, "Other", statList.LeftMenu);
            UISettingsMenuItem = new FingerUI.Menu.SideMenuItem(showUISettings, "UI", statList.LeftMenu);
            SettingsMenuItem.SubMenuItems.Add(AccountsSettingsMenuItem);
            SettingsMenuItem.SubMenuItems.Add(AvatarSettingsMenuItem);
            SettingsMenuItem.SubMenuItems.Add(GroupSettingsMenuItem);
            SettingsMenuItem.SubMenuItems.Add(MediaServiceSettingsMenuItem);
            SettingsMenuItem.SubMenuItems.Add(NotificationSettingsMenuItem);
            SettingsMenuItem.SubMenuItems.Add(UISettingsMenuItem);
            SettingsMenuItem.SubMenuItems.Add(OtherSettingsMenuItem);
            SettingsMenuItem.SubMenuItems.Add(AdvancedSettingsMenuItem);
            
            

            foreach (ISpecialTimeLine t in SpecialTimeLinesRepository.GetList())
            {
                AddGroupSelectMenuItem(t);
            }


            statList.LeftMenu.ResetMenu(new FingerUI.Menu.SideMenuItem[]{BackMenuItem, FriendsTimeLineMenuItem, 
                RefreshFriendsTimeLineMenuItem, MessagesMenuItem, RefreshMessagesMenuItem, GroupsMenuItem, 
                OtherGlobalMenuItem, PostUpdateMenuItem, SettingsMenuItem,
                WindowMenuItem, ExitMenuItem});
        }

        private void AddGroupSelectMenuItem(ISpecialTimeLine t)
        {
            delMenuClicked showItemClicked = delegate()
            {
                ShowSpecialTimeLine(t, Yedda.Twitter.PagingMode.None);
            };

            GroupsMenuItem.Visible = true;
            FingerUI.Menu.SideMenuItem item = new FingerUI.Menu.SideMenuItem(showItemClicked, t.name, statList.LeftMenu, t.ListName);
            GroupsMenuItem.SubMenuItems.Add(item);
        }


        private void CreateRightMenu()
        {
            if ((statList != null) && (statList.SelectedItem != null) && statList.SelectedItem.GetType() != typeof(StatusItem))
            {
                statList.RightMenu.ResetMenu(null);
                statList.SelectedItem.CreateRightMenu(statList.RightMenu);
                if (statList.RightMenu.Count == 0)
                    this.specificMenu.Enabled = false;
                else
                    this.specificMenu.Enabled = true;
                return;
            }

            // "Show Conversation", "Reply @User", "Direct @User", "Quote", 
            //   "Make Favorite", "@User TimeLine", "Profile Page", "Stop Following",
            // "Minimize" 
            this.specificMenu.Enabled = true;
            ConversationMenuItem = new FingerUI.Menu.SideMenuItem(GetConversation, "Show Conversation", statList.RightMenu);
            ConversationMenuItem.CanHide = true;

            DeleteStatusMenuItem = new FingerUI.Menu.SideMenuItem(DeleteStatus, "Delete Tweet", statList.RightMenu);
            DeleteStatusMenuItem.CanHide = true;

            ResponsesMenuItem = new FingerUI.Menu.SideMenuItem(null, "Respond to @User...", statList.RightMenu);
            ResponsesMenuItem.CanHide = true;

            ReplyMenuItem = new FingerUI.Menu.SideMenuItem(SendReply, "Reply @User", statList.RightMenu);
            DirectMenuItem = new FingerUI.Menu.SideMenuItem(SendDirectMessage, "Direct @User", statList.RightMenu);

            ResponsesMenuItem.SubMenuItems.Add(ReplyMenuItem);
            ResponsesMenuItem.SubMenuItems.Add(DirectMenuItem);

            EmailMenuItem = new FingerUI.Menu.SideMenuItem(EmailThisItem, "Email Status", statList.RightMenu);
            QuoteMenuItem = new FingerUI.Menu.SideMenuItem(this.Quote, "Quote", statList.RightMenu);
            ToggleFavoriteMenuItem = new FingerUI.Menu.SideMenuItem(ToggleFavorite, "Make Favorite", statList.RightMenu);
            UserTimelineMenuItem = new FingerUI.Menu.SideMenuItem(ShowUserTimeLine, "@User Timeline", statList.RightMenu);
            ProfilePageMenuItem = new FingerUI.Menu.SideMenuItem(ShowProfile, "@User Profile", statList.RightMenu);
            FollowMenuItem = new FingerUI.Menu.SideMenuItem(ToggleFollow, "Follow @User", statList.RightMenu);

            MoveToGroupMenuItem = new FingerUI.Menu.SideMenuItem(null, "Move to Group...", statList.RightMenu);
            CopyToGroupMenuItem = new FingerUI.Menu.SideMenuItem(null, "Copy to Group...", statList.RightMenu);

            delMenuClicked copyItemClicked = () => CreateNewGroup(false);

            delMenuClicked moveItemClicked = () => CreateNewGroup(true);

            CopyNewGroupMenuItem = new FingerUI.Menu.SideMenuItem(copyItemClicked, "New Group", statList.RightMenu);
            MoveNewGroupMenuItem = new FingerUI.Menu.SideMenuItem(moveItemClicked, "New Group", statList.RightMenu);
            MoveToGroupMenuItem.SubMenuItems.Add(MoveNewGroupMenuItem);
            CopyToGroupMenuItem.SubMenuItems.Add(CopyNewGroupMenuItem);
            foreach (UserGroupTimeLine t in SpecialTimeLinesRepository.GetList(SpecialTimeLinesRepository.TimeLineType.UserGroup))
            {
                AddAddUserToGroupMenuItem(t);
            }

            statList.RightMenu.ResetMenu(new FingerUI.Menu.SideMenuItem[]{ConversationMenuItem, DeleteStatusMenuItem, ResponsesMenuItem, QuoteMenuItem, EmailMenuItem, ToggleFavoriteMenuItem, 
                UserTimelineMenuItem, ProfilePageMenuItem, FollowMenuItem, MoveToGroupMenuItem, CopyToGroupMenuItem});
        }

        private void AddAddUserToGroupMenuItem(UserGroupTimeLine t)
        {
            delMenuClicked copyItemClicked = () => AddUserToGroup(t, false);

            delMenuClicked moveItemClicked = () => AddUserToGroup(t, true);

            FingerUI.Menu.SideMenuItem copyitem = new FingerUI.Menu.SideMenuItem(copyItemClicked, t.name, statList.RightMenu);
            FingerUI.Menu.SideMenuItem moveitem = new FingerUI.Menu.SideMenuItem(moveItemClicked, t.name, statList.RightMenu);
            MoveToGroupMenuItem.SubMenuItems.Add(moveitem);
            CopyToGroupMenuItem.SubMenuItems.Add(copyitem);

        }


        private void SetLeftMenu()
        {
            BackMenuItem.Visible = History.Count > 1;
            FriendsTimeLineMenuItem.Visible = statList.CurrentList() != "Friends_TimeLine";
            RefreshFriendsTimeLineMenuItem.Visible = statList.CurrentList() == "Friends_TimeLine";

            MessagesMenuItem.Visible = statList.CurrentList() != "Messages_TimeLine";
            RefreshMessagesMenuItem.Visible = statList.CurrentList() == "Messages_TimeLine";
        }
        private void UpdateRightMenu()
        {
            IDisplayItem selectedItem = statList.SelectedItem;
            if (selectedItem == null) { return; }

            if (selectedItem is StatusItem)
            {
                StatusItem item = selectedItem as StatusItem;
                Yedda.Twitter conn = GetMatchingConnection(item.Tweet.Account);
                if (selectedItem != null)
                {
                    statList.SetRightMenuUser();
                    if (string.IsNullOrEmpty(item.Tweet.in_reply_to_status_id))
                    {
                        ConversationMenuItem.Visible = false;
                    }
                    else
                    {
                        ConversationMenuItem.Visible = true;
                    }

                    if (ClientSettings.GetAcountForUser(item.Tweet.user.screen_name) != null)
                    {
                        DeleteStatusMenuItem.Visible = true;
                        ResponsesMenuItem.Visible = false;
                    }
                    else
                    {
                        DeleteStatusMenuItem.Visible = false;
                        ResponsesMenuItem.Visible = true;
                    }


                    if (conn.FavoritesWork)
                    {
                        if (item.IsFavorite)
                        {
                            ToggleFavoriteMenuItem.Text = PockeTwit.Localization.XmlBasedResourceManager.GetString("Remove Favorite");
                        }
                        else
                        {
                            ToggleFavoriteMenuItem.Text = PockeTwit.Localization.XmlBasedResourceManager.GetString("Make Favorite");
                        }
                    }




                    FollowMenuItem.Visible = true;
                    if (FollowingDictionary[conn].IsFollowing(item.Tweet.user))
                    {
                        FollowMenuItem.Text = "Stop Following";
                        delMenuClicked followClicked = StopFollowingUser;
                        FollowMenuItem.SubMenuItems.Clear();

                        MoveToGroupMenuItem.Visible = true;
                        CopyToGroupMenuItem.Visible = true;
                    }
                    else
                    {
                        FollowMenuItem.Text = "Follow";
                        SetFollowMenu();

                        MoveToGroupMenuItem.Visible = false;
                        CopyToGroupMenuItem.Visible = false;
                    }
                }
            }
            else
            {
                selectedItem.UpdateRightMenu(statList.RightMenu);
            }
        }

        private void SetConnectedMenus()
        {
            if (TwitterConnections.Count > 0)
            {
                SetConnectedMenus(TwitterConnections[0], null);
            }
        }

        private void SetConnectedMenus(Yedda.Twitter t, StatusItem item)
        {
            SetLeftMenu();
            UpdateRightMenu();
        }

        private bool SetEverythingUp()
        {
            HistoryItem firstItem = new HistoryItem();
            firstItem.Action = Yedda.Twitter.ActionType.Friends_Timeline;
            History.Push(firstItem);
            if (System.IO.File.Exists(ClientSettings.AppPath + "\\crash.txt"))
            {
                using (CrashReport errorForm = new CrashReport())
                {
                    errorForm.ShowDialog();
                }
            }
            if (!StartBackground)
            {
                this.Show();
            }
            bool ret = true;

            if (ClientSettings.CheckVersion)
            {
                Checker = new UpgradeChecker();
                Checker.UpgradeFound += new UpgradeChecker.delUpgradeFound(UpdateChecker_UpdateFound);
            }
            SetUpListControl();

            try
            {
                ResetDictionaries();
            }
            catch (OutOfMemoryException)
            {
                PockeTwit.Localization.LocalizedMessageBox.Show("There's not enough memory to run PockeTwit. You may want to close some applications and try again.");
                if (Manager != null)
                {
                    Manager.ShutDown();
                }
                this.Close();
            }
            catch (Exception ex)
            {
                PockeTwit.Localization.LocalizedMessageBox.Show("Corrupt settings - {0}\r\nPlease reconfigure.", ex.Message);
                ClearSettings();
                ResetDictionaries();
            }

            CurrentlySelectedAccount = ClientSettings.DefaultAccount;

            Notifyer = new NotificationHandler();
            Notifyer.MessagesNotificationClicked += new NotificationHandler.DelNotificationClicked(Notifyer_MessagesNotificationClicked);

            return ret;

        }

        private void ClearSettings()
        {
            if (System.IO.File.Exists(ClientSettings.AppPath + "\\app.config"))
            {
                System.IO.File.Delete(ClientSettings.AppPath + "\\app.config");
            }
            ClientSettings.AccountsList.Clear();
            using (AccountsForm f = new AccountsForm())
            {
                f.ShowDialog();
                if (ClientSettings.AccountsList.Count == 0)
                {
                    if (Manager != null)
                    {
                        Manager.ShutDown();
                    }
                    this.Close();
                }
            }
        }

        void Notifyer_MessagesNotificationClicked()
        {
            this.Show();
        }

        private void ResetDictionaries()
        {
            FollowingDictionary.Clear();
            TwitterConnections.Clear();
            foreach (Yedda.Twitter.Account a in ClientSettings.AccountsList)
            {
                Yedda.Twitter TwitterConn = new Yedda.Twitter();
                TwitterConn.AccountInfo.ServerURL = a.ServerURL;
                TwitterConn.AccountInfo.UserName = a.UserName;
                TwitterConn.AccountInfo.Password = a.Password;
                TwitterConn.AccountInfo.Enabled = a.Enabled;
                TwitterConnections.Add(TwitterConn);
                Following f = new Following(TwitterConn);
                FollowingDictionary.Add(TwitterConn, f);
            }
            SetConnectedMenus();
            Manager = new TimelineManagement();
            Manager.Progress += Manager_Progress;
            Manager.CompleteLoaded += Manager_CompleteLoaded;
            Manager.Startup(TwitterConnections);
            Manager.FriendsUpdated += Manager_FriendsUpdated;
            Manager.MessagesUpdated += Manager_MessagesUpdated;
            Manager.SearchesUpdated += Manager_SearchesUpdated;

            foreach (Following f in FollowingDictionary.Values)
            {
                f.LoadFromTwitter();
            }
        }

        void Manager_SearchesUpdated()
        {
            if (currentSpecialTimeLine != null && statList.CurrentList()== currentSpecialTimeLine.ListName)
            {
                AddStatusesToList(Manager.GetGroupedTimeLine(currentSpecialTimeLine, Yedda.Twitter.PagingMode.None), true);
                if (currentSpecialTimeLine.Timelinetype == SpecialTimeLinesRepository.TimeLineType.SavedSearch)
                    statList.AddItem(new MoreResultsItem(this, currentSpecialTimeLine));

            }
            LastSelectedItems.UpdateUnreadCounts();
            Notifyer.NewItems();
        }


        void Manager_CompleteLoaded()
        {
            if (InvokeRequired)
            {
                delNone d = new delNone(Manager_CompleteLoaded);
                this.Invoke(d);
            }
            else
            {
                Application.DoEvents();
                statList.SwitchTolist("Friends_TimeLine");

                AddStatusesToList(Manager.GetFriendsImmediately());
                statList.Startup = false;
                if (!StartBackground)
                {
                    statList.Visible = true;
                }
                //statList.SetSelectedIndexToZero();
                progressBar1.Visible = false;
                lblTitle.Visible = false;
                Application.DoEvents();
                statList.Repaint();
                
            }
        }

        void Manager_Progress(int percentage, string Status)
        {
            lblProgress.Visible = true;
            lblProgress.Text = Status;
            progressBar1.Visible = false;
            Application.DoEvents();
        }

        
        delegate void delText(string Text);
        private void setCaption(string text)
        {
            if (InvokeRequired)
            {
                delText d = new delText(setCaption);
                this.BeginInvoke(d, text);
            }
            else
            {
                this.Text = "PockeTwit" + text;
            }
        }

        void Manager_FriendsUpdated()
        {
            if (statList.CurrentList() == "Friends_TimeLine")
            {
                AddStatusesToList(Manager.GetFriendsImmediately(), true);
            }
            else
            {
                if(currentSpecialTimeLine!=null && statList.CurrentList() == currentSpecialTimeLine.ListName)
                {
                    AddStatusesToList(Manager.GetGroupedTimeLine(currentSpecialTimeLine, Yedda.Twitter.PagingMode.None), true);
                    if (currentSpecialTimeLine.Timelinetype == SpecialTimeLinesRepository.TimeLineType.SavedSearch)
                        statList.AddItem(new MoreResultsItem(this, currentSpecialTimeLine));

                }
            }
            LastSelectedItems.UpdateUnreadCounts();
            Notifyer.NewItems();
        }
        void Manager_MessagesUpdated()
        {
            if (statList.CurrentList() == "Messages_TimeLine")
            {
                AddStatusesToList(Manager.GetMessagesImmediately(), true);
            }
            Notifyer.NewItems();
            LastSelectedItems.UpdateUnreadCounts();
        }

        //private void MapList()
        //{
        //    Cursor.Current = Cursors.WaitCursor;
        //    Application.DoEvents();
        //    using (ProfileMap m = new ProfileMap())
        //    {
        //        List<Library.User> users = new List<Library.User>();
        //        for (int i = 0; i < statList.m_items.Count; i++)
        //        {
        //            Library.User thisUser = statList.m_items[i].Tweet.user;
        //            if (thisUser.needsFetching)
        //            {
        //                thisUser = Library.User.FromId(thisUser.screen_name, statList.m_items[i].Tweet.Account);
        //                thisUser.needsFetching = false;
        //            }
        //            users.Add(thisUser);
        //        }
        //        m.Users = users;
        //        m.ShowDialog();
        //        if (m.Range > 0)
        //        {

        //            SearchForm f = new SearchForm();
        //            f.providedDistnce = m.Range.ToString();
        //            string secondLoc = Geocode.GetAddress(m.CenterLocation.ToString());
        //            if (string.IsNullOrEmpty(secondLoc))
        //            {
        //                secondLoc = m.CenterLocation.ToString();
        //            }

        //            f.providedLocation = secondLoc;

        //            this.statList.Visible = false;
        //            IsLoaded = false;
        //            if (f.ShowDialog() == DialogResult.Cancel)
        //            {
        //                IsLoaded = true;
        //                this.statList.Visible = true;
        //                f.Close();
        //                return;
        //            }

        //            IsLoaded = true;
        //            this.statList.Visible = true;
        //            f.Hide();
        //            string SearchString = f.SearchText;
        //            f.Close();
        //            this.statList.Visible = true;

        //            ShowSearchResults(SearchString);
        //        }
        //        m.Close();
        //    }
        //}

        private void ToggleFullScreen()
        {
            ClientSettings.IsMaximized = !ClientSettings.IsMaximized;
            ClientSettings.SaveSettings();
            if (ClientSettings.IsMaximized)
            {
                this.WindowState = FormWindowState.Maximized;
                this.Menu = null;
            }
            else
            {
                this.WindowState = FormWindowState.Normal;
                AddMainMenuItems();
            }
        }

        private void SetStatus()
        {
            SetStatus("", "");
        }

        private void SetStatus(string Text, string in_reply_to_status_id)
        {
            using (PostUpdate StatusForm = new PostUpdate(false))
            {
                if (!string.IsNullOrEmpty(Text))
                {
                    StatusForm.AccountToSet = CurrentlySelectedAccount;
                }
                else
                {
                    StatusForm.AccountToSet = ClientSettings.DefaultAccount;
                }
                this.statList.Visible = false;
                if (!string.IsNullOrEmpty(Text))
                {
                    StatusForm.StatusText = Text + " ";
                }
                IsLoaded = false;
                StatusForm.in_reply_to_status_id = in_reply_to_status_id;
                Manager.Pause();
                if (StatusForm.ShowDialog() == DialogResult.OK)
                {
                    this.statList.Visible = true;
                    StatusForm.Hide();
                    IsLoaded = false;
                    //Manager.RefreshFriendsTimeLine();
                }
                else
                {
                    this.statList.Visible = true;
                    StatusForm.Hide();
                    IsLoaded = false;
                }
                Manager.Start();
                this.Visible = true;
                IsLoaded = true;
                this.statList.Redraw();
                this.statList.Visible = true;
                StatusForm.Close();
            }
        }

        private void SetUpListControl()
        {
            statList.IsMaximized = ClientSettings.IsMaximized;
            statList.WordClicked += new StatusItem.ClickedWordDelegate(statusList_WordClicked);
            statList.SelectedItemChanged += new EventHandler(statusList_SelectedItemChanged);
            statList.HookKey();
        }

        private void ShowAbout()
        {
            using (AboutForm a = new AboutForm())
            {
                IsLoaded = false;
                statList.Visible = false;
                a.ShowDialog();

                this.Visible = true;

                statList.Visible = true;
                IsLoaded = true;
                string ReqedUser = a.AskedToSeeUser;
                a.Close();

                if (!string.IsNullOrEmpty(ReqedUser))
                {
                    statList.IgnoreMouse = true;
                    SwitchToUserTimeLine(a.AskedToSeeUser);
                }
            }
        }
        private void EmailThisItem()
        {
            if ((statList.SelectedItem == null) || (statList.SelectedItem as StatusItem == null)) { return; }
            StatusItem selectedItem = statList.SelectedItem as StatusItem;
            Microsoft.WindowsMobile.PocketOutlook.OutlookSession sess = new Microsoft.WindowsMobile.PocketOutlook.OutlookSession();
            Microsoft.WindowsMobile.PocketOutlook.EmailAccountCollection accounts = sess.EmailAccounts;

            if (accounts.Count == 0)
            {
                PockeTwit.Localization.LocalizedMessageBox.Show("You don't have any email accounts set up on this phone.");
                return;
            }
            else if (accounts.Count>1)
            {
                using (EmailStatusForm f = new EmailStatusForm(selectedItem.Tweet.text))
                {
                    f.ShowDialog();
                    f.Close();
                }
                return;
            }

            Microsoft.WindowsMobile.PocketOutlook.EmailMessage m = new Microsoft.WindowsMobile.PocketOutlook.EmailMessage();
            m.BodyText = selectedItem.Tweet.text;
            Microsoft.WindowsMobile.PocketOutlook.MessagingApplication.DisplayComposeForm(accounts[0], m);
        }
        private void ShowProfile()
        {
            if (statList.SelectedItem == null) { return; }
            StatusItem selectedItem = (StatusItem)statList.SelectedItem;

            using (ProfileView v = new ProfileView(selectedItem.Tweet.user))
            {
                v.ShowDialog();
            }
        }

        private void ExitApplication()
        {
            Cursor.Current = Cursors.WaitCursor;
            if (Notifyer != null) { Notifyer.ShutDown(); }
            if (Manager != null)
            {
                Manager.ShutDown();
            }
            ThrottledArtGrabber.running = false;
            Cursor.Current = Cursors.Default;
            this.Close();
        }

        private void GoBackInHistory()
        {
            
            if (History.Count > 0)
            {
                HistoryItem prev = null;
                try
                {
                    HistoryItem current = History.Pop();
                    prev = History.Pop();
                }
                catch
                {
                    return;
                }
                switch (prev.Action)
                {
                    case Yedda.Twitter.ActionType.Conversation:
                        GetConversation(prev);
                        break;
                    case Yedda.Twitter.ActionType.Friends_Timeline:
                        ShowFriendsTimeLine();
                        statList.SetSelectedMenu(RefreshFriendsTimeLineMenuItem);
                        break;
                    case Yedda.Twitter.ActionType.Replies:
                        statList.SetSelectedMenu(RefreshMessagesMenuItem);
                        ShowMessagesTimeLine();
                        break;
                    case Yedda.Twitter.ActionType.Search:
                        if (prev.ItemInfo == null)
                        {
                            statList.SetSelectedMenu(SearchMenuItem);
                            if (CurrentList == "Search_Timeline" && LastSearchTerm == prev.Argument)
                            {
                                ShowSearchResults(prev.Argument, false, Twitter.PagingMode.Back);
                            }
                            else
                            {
                                ShowSearchResults(prev.Argument, false, Twitter.PagingMode.Neutral);
                            }
                        }
                        else
                        {
                            if (CurrentList.StartsWith("SavedSearch_TimeLine_") && currentSpecialTimeLine != null && prev.Argument == currentSpecialTimeLine.name)
                            {
                                ShowSpecialTimeLine(prev.ItemInfo as ISpecialTimeLine, Yedda.Twitter.PagingMode.Back);
                            }
                            else
                            {
                                ShowSpecialTimeLine(prev.ItemInfo as ISpecialTimeLine, Yedda.Twitter.PagingMode.Neutral);
                            }
                        }
                        break;
                    case Yedda.Twitter.ActionType.User_Timeline:
                        statList.SetSelectedMenu(UserTimelineMenuItem);
                        SwitchToUserTimeLine(prev.Argument);
                        break;
                }
                if (prev.SelectedItemIndex >= 0)
                {
                    try
                    {
                        statList.SelectedItem = statList[prev.SelectedItemIndex];
                    }
                    catch (KeyNotFoundException) { }
                }
                else
                {
                    statList.SelectedItem = statList[0];
                }
                if (prev.itemsOffset >= 0)
                {
                    statList.YOffset = prev.itemsOffset;
                    statList.Redraw();
                    statList.RerenderPortal();
                    statList.Repaint();
                }
            }
        }
        private void ShowFavorites()
        {
            currentSpecialTimeLine = null;
            ChangeCursor(Cursors.WaitCursor);
            
            SwitchToList("Favorites_TimeLine");
            HistoryItem i = new HistoryItem();
            i.Action = Yedda.Twitter.ActionType.Favorites;
            History.Push(i);
            statList.SetSelectedMenu(ViewFavoritesMenuItem);
            AddStatusesToList(Manager.GetFavorites());
            ChangeCursor(Cursors.Default);
        }
        private void ShowPublicTimeLine()
        {
            currentSpecialTimeLine = null;
            ChangeCursor(Cursors.WaitCursor);
            
            SwitchToList("Public_TimeLine");
            HistoryItem i = new HistoryItem();
            i.Action = Yedda.Twitter.ActionType.Public_Timeline;
            History.Push(i);
            statList.SetSelectedMenu(PublicMenuItem);
            AddStatusesToList(Manager.GetPublicTimeLine());
            ChangeCursor(Cursors.Default);
        }

        private void RefreshFriendsTimeLine()
        {
            Manager.RefreshFriendsTimeLine();
            statList.SetSelectedMenu(RefreshFriendsTimeLineMenuItem);
        }

        private void ShowFriendsTimeLine()
        {
            currentSpecialTimeLine = null;
            ChangeCursor(Cursors.WaitCursor);
            bool Redraw = statList.CurrentList() != "Friends_TimeLine";
            SwitchToList("Friends_TimeLine");
            History.Clear();
            HistoryItem i = new HistoryItem();
            i.Action = Yedda.Twitter.ActionType.Friends_Timeline;
            History.Push(i);
            statList.SetSelectedMenu(RefreshFriendsTimeLineMenuItem);
            AddStatusesToList(Manager.GetFriendsImmediately());
            ChangeCursor(Cursors.Default);
        }

        private void RefreshMessagesTimeLine()
        {
            Manager.RefreshMessagesTimeLine();
            statList.SetSelectedMenu(RefreshMessagesMenuItem);
        }

        private void ShowMessagesTimeLine()
        {
            currentSpecialTimeLine = null;
            ChangeCursor(Cursors.WaitCursor);
            SwitchToList("Messages_TimeLine");
            History.Clear();
            HistoryItem i = new HistoryItem();
            i.Action = Yedda.Twitter.ActionType.Replies;
            History.Push(i);
            statList.SetSelectedMenu(RefreshMessagesMenuItem);
            AddStatusesToList(Manager.GetMessagesImmediately());
            ChangeCursor(Cursors.Default);
        }

        private void ShowUserTimeLine()
        {
            currentSpecialTimeLine = null;
            UpdateHistoryPosition();
            ChangeCursor(Cursors.WaitCursor);
            StatusItem statItem = (StatusItem)statList.SelectedItem;
            if (statItem == null) { return; }
            ShowUserID = statItem.Tweet.user.screen_name;
            CurrentlySelectedAccount = statItem.Tweet.Account;
            Yedda.Twitter Conn = GetMatchingConnection(CurrentlySelectedAccount);
            SwitchToList("@User_TimeLine");
            HistoryItem i = new HistoryItem();
            i.Action = Yedda.Twitter.ActionType.User_Timeline;
            i.Account = CurrentlySelectedAccount;
            i.Argument = ShowUserID;
            History.Push(i);
            AddStatusesToList(Manager.GetUserTimeLine(Conn, ShowUserID));
            ChangeCursor(Cursors.Default);

            return;
        }

        private void DeleteStatus()
        {
            StatusItem s = (StatusItem)statList.SelectedItem;
            if (s.Tweet.Delete())
            {
                // refresh
                AddStatusesToList(Manager.GetFriendsImmediately());
            }
            else
            {
                GlobalEventHandler.CallShowErrorMessage("Could not delete.");
            }
        }

        private void GetConversation()
        {
            GetConversation(null);
        }

        private void GetConversation(HistoryItem history)
        {
            currentSpecialTimeLine = null;
            UpdateHistoryPosition();
            HistoryItem i = new HistoryItem();
            Library.status lastStatus;
            Yedda.Twitter Conn;

            if (history == null)
            {
                if (statList.SelectedItem == null) { return; }
                StatusItem selectedItem = (StatusItem)statList.SelectedItem;
                if (string.IsNullOrEmpty(selectedItem.Tweet.in_reply_to_status_id)) { return; }
                Conn = GetMatchingConnection(selectedItem.Tweet.Account);
                lastStatus = selectedItem.Tweet;

                i.Account = selectedItem.Tweet.Account;
                i.Action = Yedda.Twitter.ActionType.Conversation;
                i.Argument = lastStatus.id;
            }
            else
            {
                i = history;
                Conn = GetMatchingConnection(history.Account);
                try
                {
                    lastStatus = Library.status.DeserializeSingle(Conn.ShowSingleStatus(i.Argument), i.Account);
                }
                catch
                {
                    return;
                }
            }
            ChangeCursor(Cursors.WaitCursor);

            //List<Library.status> Conversation = GetConversationFROMTHEFUTURE(lastStatus);
            List<Library.status> Conversation = new List<PockeTwit.Library.status>();
            History.Push(i);

            while (!string.IsNullOrEmpty(lastStatus.in_reply_to_status_id))
            {
                Conversation.Add(lastStatus);
                try
                {
                    lastStatus = Library.status.DeserializeSingle(Conn.ShowSingleStatus(lastStatus.in_reply_to_status_id), Conn.AccountInfo);
                }
                catch
                {
                    lastStatus = null;
                    break;
                }
            }
            if (lastStatus != null)
            {
                Conversation.Add(lastStatus);
            }
            statList.SwitchTolist("Conversation");
            statList.ClearVisible();
            AddStatusesToList(Conversation.ToArray());
            ChangeCursor(Cursors.Default);
            this.SetLeftMenu();
        }

        private List<PockeTwit.Library.status> GetConversationFROMTHEFUTURE(PockeTwit.Library.status lastStatus)
        {
            Yedda.Twitter Conn = GetMatchingConnection(lastStatus.Account);
            Library.status[] SearchResults = Manager.SearchTwitter(Conn, "@" + lastStatus.user.screen_name, Yedda.Twitter.PagingMode.None);
            List<Library.status> Results = new List<PockeTwit.Library.status>();
            foreach (Library.status s in SearchResults)
            {
                if (s.in_reply_to_status_id == lastStatus.id)
                {
                    Results.Add(s);
                }
            }

            if (Results.Count == 1)
            {
                Results.AddRange(GetConversationFROMTHEFUTURE(Results[0]));
            }
            return Results;
        }


        private void Quote()
        {
            if (statList.SelectedItem == null) { return; }
            StatusItem selectedItem = (StatusItem)statList.SelectedItem;
            string quote = "RT @" + selectedItem.Tweet.user.screen_name + ": \"" + selectedItem.Tweet.text + "\"";
            SetStatus(quote, "");
        }

        Type _lastSelectedItemType = typeof(StatusItem);

        void statusList_SelectedItemChanged(object sender, EventArgs e)
        {
            Type selectedType = statList.SelectedItem.GetType();
            if (selectedType != _lastSelectedItemType)
            {
                // item type has changed, update menus
                CreateRightMenu();
                UpdateRightMenu();
                _lastSelectedItemType = selectedType;
            }

            StatusItem statItem = statList.SelectedItem as StatusItem;
            if (statItem == null) { return; }
            CurrentlySelectedAccount = statItem.Tweet.Account;
            SetConnectedMenus(GetMatchingConnection(CurrentlySelectedAccount), statItem);
            //UpdateRightMenu(); -- THIS IS DONE IN SETCONNECTEDMENUS
            UpdateHistoryPosition();
            int clickedNumber = statItem.Index + 1;
            SetLeftMenu();            
            LastSelectedItems.SetLastSelected(statList.CurrentList(), statItem.Tweet, currentSpecialTimeLine);
        }

        private void UpdateHistoryPosition()
        {
            if (History.Count > 0)
            {
                HistoryItem i = History.Peek();
                i.SelectedItemIndex = statList.SelectedIndex;
                i.itemsOffset = statList.YOffset;
            }
        }


        void statusList_WordClicked(string TextClicked)
        {
            if (TextClicked.StartsWith("http"))
            {
                if (PockeTwit.MediaServices.PictureServiceFactory.Instance.FetchServiceAvailable(TextClicked))
                {
                    Cursor.Current = Cursors.WaitCursor;
                    PockeTwit.MediaServices.IPictureService p = PockeTwit.MediaServices.PictureServiceFactory.Instance.LocateFetchService(TextClicked);
                    p.FetchPicture(TextClicked);
                    p.DownloadFinish += new PockeTwit.MediaServices.DownloadFinishEventHandler(p_DownloadFinish);
                    p.ErrorOccured += new PockeTwit.MediaServices.ErrorOccuredEventHandler(p_ErrorOccured);
                    return;
                }
                

                System.Diagnostics.ProcessStartInfo pi = new System.Diagnostics.ProcessStartInfo();
                if (ClientSettings.UseSkweezer)
                {
                    pi.FileName = Skweezer.GetSkweezerURL(TextClicked);
                }
                else
                {
                    pi.FileName = TextClicked;
                }
                try
                {
                    pi.UseShellExecute = true;
                    System.Diagnostics.Process p = System.Diagnostics.Process.Start(pi);
                }
                catch
                {
                    PockeTwit.Localization.LocalizedMessageBox.Show("There is no default web browser defined for the OS.");
                }
            }
            else if (TextClicked.StartsWith("#"))
            {
                ShowSearchResults("q=" + System.Web.HttpUtility.UrlEncode(TextClicked));
            }
            else if (TextClicked.StartsWith("@"))
            {
                SwitchToUserTimeLine(TextClicked);
            }
        }

        void p_ErrorOccured(object sender, PockeTwit.MediaServices.PictureServiceEventArgs eventArgs)
        {
            if (InvokeRequired)
            {
                delPictureDone d = new delPictureDone(p_ErrorOccured);
                this.Invoke(d, sender, eventArgs);
            }
            else
            {
                PockeTwit.MediaServices.IPictureService p = (PockeTwit.MediaServices.IPictureService)sender;
                p.DownloadFinish -= new PockeTwit.MediaServices.DownloadFinishEventHandler(p_DownloadFinish);
                p.ErrorOccured -= new PockeTwit.MediaServices.ErrorOccuredEventHandler(p_ErrorOccured);
                Cursor.Current = Cursors.Default;
                PockeTwit.Localization.LocalizedMessageBox.Show("Unable to fetch picture. You may want to try again.");
            }
        }

        delegate void delPictureDone(object sender, PockeTwit.MediaServices.PictureServiceEventArgs eventArgs);
        void p_DownloadFinish(object sender, PockeTwit.MediaServices.PictureServiceEventArgs eventArgs)
        {
            if (InvokeRequired)
            {
                delPictureDone d = new delPictureDone(p_DownloadFinish);
                this.Invoke(d, sender, eventArgs);
            }
            else
            {


                PockeTwit.MediaServices.IPictureService p = (PockeTwit.MediaServices.IPictureService)sender;
                p.DownloadFinish -= new PockeTwit.MediaServices.DownloadFinishEventHandler(p_DownloadFinish);
                p.ErrorOccured -= new PockeTwit.MediaServices.ErrorOccuredEventHandler(p_ErrorOccured);

                Cursor.Current = Cursors.Default;

                using (ImagePreview ip = new ImagePreview(eventArgs.ReturnMessage, eventArgs.PictureFileName))
                {
                    ip.ShowDialog();
                }
            }
        }

        private void SwitchToUserTimeLine(string TextClicked)
        {
            UpdateHistoryPosition();
            ShowUserID = TextClicked.Replace("@", "");
            StatusItem statItem = (StatusItem)statList.SelectedItem;
            if (statItem == null) { return; }
            ChangeCursor(Cursors.WaitCursor);
            HistoryItem i = new HistoryItem();
            i.Argument = ShowUserID;
            i.Account = statItem.Tweet.Account;
            i.Action = Yedda.Twitter.ActionType.User_Timeline;
            History.Push(i);
            CurrentlySelectedAccount = statItem.Tweet.Account;
            Yedda.Twitter Conn = GetMatchingConnection(CurrentlySelectedAccount);
            SwitchToList("@User_TimeLine");
            AddStatusesToList(Manager.GetUserTimeLine(Conn, ShowUserID));
            ChangeCursor(Cursors.Default);
            return;
        }

        private void SwitchToDone()
        {

            //lblTitle.Visible = false;

            GlobalEventHandler.setPid();

            if (!StartBackground)
            {
                statList.Visible = true;
            }
            statList.BringToFront();
            SwitchToList("Friends_TimeLine");
            IsLoaded = true;
            lblTitle.Text = "PockeTwit";
            lblProgress.Visible = false;
            this.Refresh();
            StartBackground = false;
        }
        private string CurrentList
        {
            get;
            set;
        }
        private void SwitchToList(string ListName)
        {
            if (statList.CurrentList() != ListName)
            {
                statList.SwitchTolist(ListName);
                CurrentList = ListName;
            }
            SetLeftMenu();
        }

        private void FollowUserClicked()
        {
            using (FollowUserForm f = new FollowUserForm())
            {
                if (f.ShowDialog() == DialogResult.Cancel)
                {
                    f.Close();
                    return;
                }
                ChangeCursor(Cursors.WaitCursor);
                try
                {
                    Yedda.Twitter conn = GetMatchingConnection(f.Account);
                    string response = conn.FollowUser(f.UserName);

                    if (string.IsNullOrEmpty(response))
                    {
                        GlobalEventHandler.CallShowErrorMessage("User not found.");
                    }
                    else
                    {
                        FollowingDictionary[conn].AddUser(Library.User.FromId(f.UserName, f.Account));
                        UpdateRightMenu();
                    }
                }
                finally
                {
                    ChangeCursor(Cursors.Default);
                }
                f.Close();
            }
        }

        
        private void TwitterSearch()
        {

            using (SearchForm f = new SearchForm())
            {
                this.statList.Visible = false;
                IsLoaded = false;
                SavedSearchTimeLine SavedSearch;
                if (f.ShowDialog() == DialogResult.Cancel)
                {
                    IsLoaded = true;
                    this.statList.Visible = true;
                    f.Close();
                    return;
                }

                IsLoaded = true;
                this.statList.Visible = true;
                f.Hide();
                string SearchString = f.SearchText;
                SavedSearch = f.SavedSearch;
                if(SavedSearch!=null)
                {
                    AddGroupSelectMenuItem(SavedSearch);
                }
                f.Close();

                statList.Visible = true;

                ShowSearchResults(SearchString, (SavedSearch!=null && SavedSearch.autoUpdate));
            }
        }

        private void ShowSearchResults(string SearchString)
        {
            ShowSearchResults(SearchString,false);
        }

        private void ShowSearchResults(string SearchString, bool saveThem)
        {
            ShowSearchResults(SearchString, saveThem, Yedda.Twitter.PagingMode.None);
        }

        internal string LastSearchTerm
        {
            get;
            set;
        }

        internal void ShowSearchResults(string SearchString, bool saveThem, Yedda.Twitter.PagingMode pagingMode)
        {
            LastSearchTerm = SearchString;
            UpdateHistoryPosition();
            ChangeCursor(Cursors.WaitCursor);
            statList.SetSelectedMenu(SearchMenuItem);
            HistoryItem i = new HistoryItem();
            i.Action = Yedda.Twitter.ActionType.Search;
            i.Argument = SearchString;
            History.Push(i);

            Yedda.Twitter Conn = GetMatchingConnection(CurrentlySelectedAccount);
            SwitchToList("Search_TimeLine");
            statList.ClearVisible();
            Library.status[] stats = Manager.SearchTwitter(Conn, SearchString, pagingMode);

            if (stats != null)
            {
                List<Library.status> searchResults = new List<status>(stats);
                if (saveThem)
                {
                    LocalStorage.DataBaseUtility.SaveItems(searchResults);
                }
                AddStatusesToList(searchResults.ToArray());

                statList.AddItem(new MoreResultsItem(this, SearchString, saveThem));
            }
            ChangeCursor(Cursors.Default);
        }

        void UpdateChecker_UpdateFound(UpgradeChecker.UpgradeInfo Info)
        {
            using (UpgradeForm uf = new UpgradeForm())
            {
                uf.NewVersion = Info;
                uf.ShowDialog();
            }
        }

        private void SetWindowState(FormWindowState State)
        {
            if (InvokeRequired)
            {
                delSetWindowState d = new delSetWindowState(SetWindowState);
                this.Invoke(d, new object[] { State });
            }
            else
            {
                this.WindowState = State;
            }
        }

        private bool isChangingingWindowState = false;

        protected override void OnActivated(EventArgs e)
        {
            if (DetectDevice.DeviceType == DeviceType.Professional)
            {
                inputPanel1.Enabled = false;
                if (Notifyer != null)
                {
                    Notifyer.DismissBubbler();
                }
            }
            if (isChangingingWindowState) { return; }
            isChangingingWindowState = true;
            
            
            GlobalEventHandler.setPid();

           
            // JohnB2007: changed this in order to avoid unused warning for IsLoaded.
            // Will result in the same MSIL due to compiler optimization anyway and allows
            // to reuse the infrastructure if once needed.
            if (!IsLoaded)
            {
            //    isChangingingWindowState = false;
            //    return;
            }
            


            if (ClientSettings.IsMaximized)
            {
                SetWindowState(FormWindowState.Maximized);
                this.Menu = null;
            }
            else
            {
                SetWindowState(FormWindowState.Normal);
                AddMainMenuItems();
            }
            //How do we tell if it was launched in background and is not being activated by user?
            // This is occuring after the initial run/setup is complete.
            statList.Visible = true;
            if (DetectDevice.DeviceType == DeviceType.Standard)
            {
                statList.Focus();
            }
            isChangingingWindowState = false;
           
            SendToForground();

            this.Invalidate();
        }

        [System.Runtime.InteropServices.DllImport("coredll.dll")]
        static extern int ShowWindow(IntPtr hWnd, int nCmdShow);

        const int SW_MINIMIZED = 6;

        void Minimize()
        {
            isChangingingWindowState = true;
            // The Taskbar must be enabled to be able to do a Smart Minimize
            statList.Visible = false;
            //this.FormBorderStyle = FormBorderStyle.FixedDialog;
            //statList.Visible = false;
            //this.WindowState = FormWindowState.Normal;
            //this.ControlBox = true;
            //this.MinimizeBox = true;
            //this.MaximizeBox = true;

            // Since there is no WindowState.Minimize, we have to P/Invoke ShowWindow
            /*
            statList.Clear();
             */
            ShowWindow(this.Handle, SW_MINIMIZED);
            isChangingingWindowState = false;
            GC.Collect();
            GC.WaitForPendingFinalizers();

        }

        [System.Runtime.InteropServices.DllImport("coredll.dll")]
        private static extern bool SetForegroundWindow(IntPtr hWnd);

        void SendToForground()
        {
            SetForegroundWindow(this.Handle);
        }

        protected override void OnClosed(EventArgs e)
        {
            Program.IgnoreDisposed = true;
            if (Manager != null)
            {
                Manager.ShutDown();
            }
            if (Notifyer != null)
            {
                Notifyer.ShutDown();
            }
            ThrottledArtGrabber.running = false;
            base.OnClosed(e);
        }

        #endregion�Methods�

        private void TweetList_LostFocus(object sender, EventArgs e)
        {
            if (!statList.Focused)
            {
                statList.Visible = false;
            }
        }

        private void globalMenu_Click(object sender, EventArgs e)
        {
            statList.OpenLeftMenu();
        }

        private void specificMenu_Click(object sender, EventArgs e)
        {
            statList.OpenRightMenu();
        }

        //handle requests from outside the UI -- either CLI args or custom messages
        public void ProcessArgs(string[] args)
        {
            BringToFront();
            if (args.Length > 0)
            {
                // QuickPost called
                string Arg = args[0];

                if (Arg == "/QuickPost")
                {
                    SetStatus();
                }

                // the today plugin passes /Group=XXXXX as argument
                if (Arg.StartsWith("/Group="))
                {
                    string GroupName = Arg.Substring(Arg.IndexOf('=') + 1);
                    ISpecialTimeLine t = SpecialTimeLinesRepository.GetFromReadableName(GroupName);
                    if (t != null)
                    {
                        ShowSpecialTimeLine(t, Yedda.Twitter.PagingMode.None);
                        
                    }
                    if (GroupName == "Friends TimeLine")
                    {
                        ShowFriendsTimeLine();
                    }
                    if (GroupName == "Messages TimeLine")
                    {
                        ShowMessagesTimeLine();
                    }
                    
                    
                }
            }


        }
    }
}
