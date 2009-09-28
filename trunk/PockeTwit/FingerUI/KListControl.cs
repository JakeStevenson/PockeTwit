using System;
using System.Collections.Generic;
using System.Collections;
using System.Text;
using System.Drawing;
using System.Windows.Forms;
using FingerUI;
using PockeTwit.FingerUI.Menu;
using PockeTwit.OtherServices.TextShrinkers;
using PockeTwit.TimeLines;

namespace PockeTwit.FingerUI
{
    public class KListControl : UserControl
    {
        private FullScreenTweet fsDisplay = new FullScreenTweet();
        private System.Threading.Timer animationTimer;
        private System.Threading.Timer errorTimer;
        public delegate void delProgress(int itemnumber, int totalnumber);
        public event delProgress Progress = delegate { };

        private delegate void delPaint();

        private class Velocity
        {
            public delegate void delStoppedMoving();
            public event delStoppedMoving StoppedMoving = delegate { };



            private int _X = 0;
            public int X
            {
                get
                {
                    return _X;
                }
                set
                {
                    _X = value;
                }
            }
            private int _Y = 0;
            public int Y
            {
                get
                {
                    return _Y;
                }
                set
                {
                    bool bRaiseStop = false;
                    bRaiseStop = (_Y != value) && (value == 0);
                        
                    _Y = value;
                    if (bRaiseStop)
                    {
                        StoppedMoving();
                    }
                }
            }
            

        }
        
        #region Fields (23) 
        private bool menuwasClicked = false;
        private Portal SlidingPortal = new Portal();
        private Popup NotificationArea = new Popup();
        private Popup ErrorPopup = new Popup();
        private ClickablesMenu ClickablesControl = new ClickablesMenu();
        private bool HasMoved = false;
        private bool InFocus = false;
        // Background drawing
        
        public ItemList m_items = new ItemList();
        Dictionary<string, ItemList> ItemLists = new Dictionary<string, ItemList>();
        
        int m_itemWidth = -1;
        // Properties
        int m_maxVelocity = 45;
        Point m_mouseDown = new Point(-1, -1);
        Point m_mousePrev = new Point(-1, -1);
        Point m_offset = new Point();
        
        bool m_scrollBarMove = false;

        int m_selectedIndex = 0;
        Timer m_timer = new Timer();
        public bool Startup = true;
        private Velocity m_velocity = new Velocity();
        
        
        public SideMenu LeftMenu = new SideMenu(SideShown.Left);
        public SideMenu RightMenu = new SideMenu(SideShown.Right);
        #endregion Fields 

        #region Enums (2) 

        enum XDirection
        {
            Left, Right
        }
        public enum SideShown
        {
            Left,
            Middle,
            Right
        }

        #endregion Enums 

        #region Constructors (1) 

        public KListControl()
        {

            LeftMenu.ItemWasClicked+=new SideMenu.DelClearMe(delegate{SnapBack();});
            RightMenu.ItemWasClicked += new SideMenu.DelClearMe(delegate { SnapBack(); });

            LeftMenu.AnimateMe += new SideMenu.DelAnimateMe(this.startAnimation);
            RightMenu.AnimateMe+=new SideMenu.DelAnimateMe(this.startAnimation);

            LeftMenu.NeedRedraw+=new SideMenu.DelClearMe(delegate{Repaint();});
            RightMenu.NeedRedraw += new SideMenu.DelClearMe(delegate { Repaint(); });

            ClickablesControl.NeedRedraw += new ClickablesMenu.delNoArgs(delegate { Repaint();});
            ClickablesControl.Dismissed += new ClickablesMenu.delNoArgs(delegate {HideClickablesControl();});

            animationTimer = new System.Threading.Timer(new System.Threading.TimerCallback(animationTimer_Tick), null, System.Threading.Timeout.Infinite, System.Threading.Timeout.Infinite);
            errorTimer = new System.Threading.Timer(new System.Threading.TimerCallback(errorTimer_Tick), null, System.Threading.Timeout.Infinite, System.Threading.Timeout.Infinite);
            NotificationArea.TextFont = ClientSettings.TextFont;
            NotificationArea.parentControl = this;

            ErrorPopup.TextFont = ClientSettings.TextFont;
            ErrorPopup.parentControl = this;
            ErrorPopup.AtTop = true;

            SelectedFont = ClientSettings.TextFont;
            m_timer.Interval = ClientSettings.AnimationInterval;
            m_timer.Tick += new EventHandler(m_timer_Tick);

            ClickablesControl.Visible = false;
            ClickablesControl.WordClicked += new StatusItem.ClickedWordDelegate(ClickablesControl_WordClicked);

            //Need to repaint when fetching state has changed.
            PockeTwit.GlobalEventHandler.TimeLineDone += new PockeTwit.GlobalEventHandler.delTimelineIsDone(GlobalEventHandler_TimeLineDone);
            PockeTwit.GlobalEventHandler.TimeLineFetching += new PockeTwit.GlobalEventHandler.delTimelineIsFetching(GlobalEventHandler_TimeLineFetching);

            SlidingPortal.NewImage += new Portal.delNewImage(SlidingPortal_NewImage);
            SlidingPortal.Progress += new Portal.delProgress(SlidingPortal_Progress);
            SlidingPortal.Panic += new Portal.delNewImage(SlidingPortal_Panic);
            m_velocity.StoppedMoving += new Velocity.delStoppedMoving(m_velocity_StoppedMoving);

            fsDisplay.Visible = false;
            fsDisplay.Dock = DockStyle.Fill;
            this.Controls.Add(fsDisplay);

            PockeTwit.GlobalEventHandler.ShowErrorMessage += new PockeTwit.GlobalEventHandler.delshowErrorMessage(GlobalEventHandler_ShowErrorMessage);
            this.LostFocus += new EventHandler(KListControl_LostFocus);
        }

        void SlidingPortal_Progress(int itemnumber, int totalnumber)
        {
            Progress(itemnumber, totalnumber);
        }

        void SlidingPortal_Panic()
        {
            foreach (IDisplayItem i in m_items.Values)
            {
                i.ParentGraphics = SlidingPortal._RenderedGraphics;
            }
        }

        

        void KListControl_LostFocus(object sender, EventArgs e)
        {
            if (!this.Parent.Focused)
            {
                this.Visible = false;
            }
        }

        void GlobalEventHandler_ShowErrorMessage(string Message)
        {
            ErrorPopup.ShowNotification(Message);
            errorTimer.Change(15000, System.Threading.Timeout.Infinite);
        }

        void SlidingPortal_NewImage()
        {
            SlidingPortalOffset = YOffset - (itemsBeforePortal * ClientSettings.ItemHeight);
            SlidingPortal.WindowOffset = SlidingPortalOffset;
            Repaint();
        }

        void m_velocity_StoppedMoving()
        {
            RerenderPortal();
        }

        int itemsBeforePortal = 0;
        int previousItemsBeforePortal = 0;

        
        public void RerenderPortal()
        {
            //Only rerender if we're not moving -- otherwise it makes scrolling choppy.
            if (!Capture && m_velocity.Y == 0 && m_velocity.X==0)
            {
                RecalculatePortalPosition();
            }
        }

        private void RecalculatePortalPosition()
        {
            int itemsBeforeScreen = YOffset / ClientSettings.ItemHeight;

            itemsBeforePortal = (itemsBeforeScreen +2 ) - (SlidingPortal.MaxItems / 2); // center to view
            int itemsAfterPortal = (m_items.Count - (itemsBeforeScreen +2)) - (SlidingPortal.MaxItems / 2) - 1;
            if (itemsAfterPortal < 0)
            {
                itemsBeforePortal = itemsBeforePortal + itemsAfterPortal;
            }
            if (itemsBeforePortal < 0) { itemsBeforePortal = 0; }
            List<IDisplayItem> NewSet = new List<IDisplayItem>();
            int MaxSize = Math.Min(itemsBeforePortal + SlidingPortal.MaxItems, m_items.Count);
            for (int i = itemsBeforePortal; i < MaxSize; i++)
            {
                try
                {
                    NewSet.Add(m_items[i]);
                }
                catch (KeyNotFoundException)
                {
                }
            }
            previousItemsBeforePortal = itemsBeforePortal;
            if (NewSet.Count > 0)
            {
                SlidingPortal.SetItemList(NewSet);
            }
        }
        public void ResetFullScreenColors()
        {
            fsDisplay.ResetRendering();
            RightMenu.ForceRerender();
            LeftMenu.ForceRerender();
        }

        void GlobalEventHandler_TimeLineFetching(PockeTwit.TimelineManagement.TimeLineType TType)
        {
            if (InvokeRequired)
            {
                delClearMe d = new delClearMe(this.Invalidate);
                this.BeginInvoke(d);
            }
            else
            {
                this.Invalidate();
            }
        }

        void GlobalEventHandler_TimeLineDone(PockeTwit.TimelineManagement.TimeLineType TType)
        {
            if (InvokeRequired)
            {
                delClearMe d = new delClearMe(this.Invalidate);
                this.BeginInvoke(d);
            }
            else
            {
                this.Invalidate();
            }
        }


        #endregion Constructors 

        #region Properties (19) 
        
        private Point OldSize;
        private bool _FirstView = true;
        private bool _Visible;
        public new bool Visible
        {
            get
            {
                return _Visible;
            }
            set
            {
                try
                {
                    if (!value)
                    {
                        fsDisplay.Visible = false;
                    }

                    if (value && !_Visible)
                    {

                        if (_FirstView)
                        {
                            _FirstView = false;
                        }
                            //after saving new account this is being disposed.
                            //should make a check and do something else than rerender.
                        else if (OldSize != new Point(this.Width, this.Height))
                        {
                            Application.DoEvents();
                            RerenderBySize();
                        }
                    }
                    OldSize = new Point(this.Width, this.Height);
                    _Visible = value;
                    base.Visible = value;
                }
                catch(ObjectDisposedException)
                {
                    return;
                }
            }
        }

        public int Count
        {
            get
            {
                return m_items.Count;
            }
        }

        private SideShown CurrentlyViewing
        {
            get
            {
                if (XOffset < 0)
                {
                    return SideShown.Left;
                }
                else if (XOffset> 0)
                {
                    return SideShown.Right;
                }
                return SideShown.Middle;
            }
        }

        
        public bool IsMaximized { get; set; }
        
        
        public int ItemWidth
        {
            get
            {
                // In vertical mode, we just use the full bounds, other modes use m_itemWidth.
                if (m_itemWidth < 0)
                {
                    m_itemWidth = this.Width;
                }
                return m_itemWidth;
            }
            set
            {
                m_itemWidth = value;
                //this.Redraw();
            }
        }

        public int MaxVelocity
        {
            get
            {
                return m_maxVelocity;
            }
            set
            {
                m_maxVelocity = value;
            }
        }

        private int MaxXOffset
        {
            get
            {
                //return Math.Max(((m_items.Count * ItemWidth)) - Bounds.Width, 0);
                return this.Width-50;
            }
        }

        private int MaxYOffset
        {
            get
            {
                return Math.Max(((m_items.Count * ClientSettings.ItemHeight)) - Bounds.Height, 0);
            }
        }

        private int MinXOffset
        {
            get
            {
                return 0 - (this.Width - 50);
            }
        }

        
        public Font SelectedFont { get; set; }

        public int SelectedIndex
        {
            get
            {
                return m_selectedIndex;
            }
        }
        public IDisplayItem SelectedItem
        {
            get
            {
                if (m_items.Count > 0)
                {
                    return m_items[m_selectedIndex];
                }
                
                return null;
            }
            set
            {
                m_items[m_selectedIndex].Selected = false;
                SlidingPortal.ReRenderItem(m_items[m_selectedIndex]);
                for(int i=0;i<m_items.Count;i++)
                {
                    IDisplayItem item = m_items[i];
                    if (item == value)
                    {
                        item.Selected = true;
                        m_selectedIndex = i;
                        //PockeTwit.LastSelectedItems.SetLastSelected(CurrentList(), item.Tweet);
                        SlidingPortal.ReRenderItem(m_items[m_selectedIndex]);
                    }
                    else
                    {
                        item.Selected = false;
                    }
                }
                if (SelectedItemChanged != null)
                {
                    SelectedItemChanged(this, new EventArgs());
                }
            }
        }

        public IDisplayItem this[int index]
        {
            get
            {
                if (m_items.Count <= index)
                {
                    return m_items[m_items.Count - 1];
                }
                return m_items[index];
            }
        }

        private int SlidingPortalOffset = 0;
        private int SlidingPortalCurrentEnd;

        public int YOffset
        {
            get
            {
                return m_offset.Y;
            }
            set
            {
                m_offset.Y = value;

                SlidingPortalOffset = YOffset - (itemsBeforePortal * ClientSettings.ItemHeight);
                SlidingPortal.WindowOffset = SlidingPortalOffset;
            }
        }
        public int XOffset
        {
            get
            {
                return m_offset.X;
            }
            set
            {
                m_offset.X = value;
            }
        }

        #endregion Properties 

        #region Delegates and Events (9) 


        // Delegates (4) 

        public delegate void delAddItem(IDisplayItem item);
        public delegate void delClearMe();
        
        // Events (5) 

        //public event delMenuItemSelected MenuItemSelected;

        public event EventHandler SelectedItemChanged;

        public event EventHandler SelectedItemClicked;

        public event StatusItem.ClickedWordDelegate WordClicked;


        #endregion Delegates and Events 

        #region Methods (49) 
        
        void animationTimer_Tick(object o)
        {
            if (!NotificationArea.isAnimating && !ErrorPopup.isAnimating && !LeftMenu.IsAnimating && !RightMenu.IsAnimating)
            {
                animationTimer.Change(System.Threading.Timeout.Infinite, System.Threading.Timeout.Infinite);
            }
            this.Repaint();
        }

        void errorTimer_Tick(object o)
        {
            errorTimer.Change(System.Threading.Timeout.Infinite, System.Threading.Timeout.Infinite);
            ErrorPopup.HideNotification();
            this.Repaint();
        }
        public void startAnimation()
        {
            animationTimer.Change(50, 50);
        }

        /*
        public void stopAnimation()
        {
            if (!NotificationArea.isAnimating && !ErrorPopup.isAnimating)
            {
                animationTimer.Enabled = false;
            }
        }
         */
        // Public Methods (16) 

        private string _CurrentList = null;
        public string CurrentList()
        {
            return _CurrentList;
        }
        public void SwitchTolist(string ListName)
        {
            string thisList = CurrentList();
            if (!string.IsNullOrEmpty(thisList))
            {
                if ((SelectedItem != null) && (SelectedItem is StatusItem))
                {
                    if (ListName != "Conversation" && ListName != "Search_TimeLine")
                    {
                        LastSelectedItems.SetLastSelected(thisList, (SelectedItem as StatusItem).Tweet);
                    }
                }
                if (ListName != thisList)
                {
                    PockeTwit.ThrottledArtGrabber.ClearMem();
                }
            }
            if (!ItemLists.ContainsKey(ListName))
            {
                ItemLists.Add(ListName, new ItemList());
            }
            _CurrentList = ListName;
            m_items = ItemLists[ListName];
            Reset();
        }

        public void JumpToLastSelected()
        {
            string jumpID = LastSelectedItems.GetLastSelected(CurrentList());
            if (!string.IsNullOrEmpty(jumpID))
            {
                for (int i = 0; i < m_items.Count; i++)
                {
                    if (!(m_items[i] is StatusItem))
                        continue;
                    if ((m_items[i] as StatusItem).Tweet.id == jumpID)
                    {
                        m_selectedIndex = i;
                        SelectAndJump();
                        return;
                    }
                }
            }
            SetSelectedIndexToZero();
        }

        public void AddItem(string text, object value)
        {
            
            StatusItem item = new StatusItem(this, text, value);
            item.Index = m_items.Count;
            AddItem(item);
        }

        public void AddItem(IDisplayItem item)
        {
            if (InvokeRequired)
            {
                delAddItem d = new delAddItem(AddItem);
                this.BeginInvoke(d, new object[] { item });
            }
            else
            {
                item.Parent = this;
                item.Index = m_items.Count;
                item.ParentGraphics = SlidingPortal._RenderedGraphics;
                item.Selected = false;
                item.Bounds = ItemBounds(0, item.Index);
                m_items.Add(item.Index, item);
            }
        }

        public void Clear()
        {
            if (InvokeRequired)
            {
                delClearMe d = new delClearMe(Clear);
                this.BeginInvoke(d, null);
            }
            else
            {
                foreach (ItemList items in this.ItemLists.Values)
                {
                    items.Clear();
                }
                Reset();
            }
        }
        public void ClearVisible()
        {
            if (InvokeRequired)
            {
                delClearMe d = new delClearMe(ClearVisible);
                this.BeginInvoke(d, null);
            }   
            else
            {
                m_items.Clear();
                Reset();
            }
        }

        public void HookKey()
        {
            this.Parent.KeyDown += new KeyEventHandler(OnKeyDown);
            this.Parent.KeyPress += new KeyPressEventHandler(OnKeyPress);
        }
        
        public void JumpToItem(object Value)
        {
            if (Value is IDisplayItem)
            {
                JumpToItem(Value as IDisplayItem);
            }
            for (int i = 0; i < this.Count; i++)
            {
                IDisplayItem item = this[i];
                if (!(item is StatusItem))
                    continue;
                if ((item as StatusItem).Value == null)
                    continue;
                if ((item as StatusItem).Value.ToString() == Value.ToString())
                {
                    JumpToItem(item);
                }
            }
        }

        public void JumpToItem(IDisplayItem item)
        {
            
            Rectangle VisibleBounds = new Rectangle(0, YOffset, 10, this.Height);
            Rectangle CheckAgainstBounds = new Rectangle(0, item.Bounds.Top, 10, item.Bounds.Height);
            int searchItems = 0;
            while (!VisibleBounds.Contains(CheckAgainstBounds))
            {
                if(item.Bounds.Top >= VisibleBounds.Top)
                {
                    YOffset  = YOffset + ClientSettings.ItemHeight;
                }
                else
                {
                    YOffset = YOffset - ClientSettings.ItemHeight;
                }

                if (YOffset < 0) 
                { 
                    YOffset = 0;
                    break; 
                }
                if (YOffset > (m_items.Values.Count - 1) * ClientSettings.ItemHeight) 
                { 
                    YOffset = m_items.Values.Count * ClientSettings.ItemHeight;
                    break;
                }

                VisibleBounds = new Rectangle(0, YOffset, 10, this.Height);
                searchItems++;
                if (searchItems > ClientSettings.MaxTweets)
                {
                    break;
                }
            }
            Invalidate();
        }

        public void Repaint()
        {
            if (InvokeRequired)
            {
                delClearMe d = new delClearMe(Repaint);
                try
                {
                    this.BeginInvoke(d, null);
                }
                catch (ObjectDisposedException)
                {
                    this.Dispose();
                }
            }
            else
            {
                this.Invalidate();
            }
        }

        public void Redraw()
        {
            if (!Startup)
            {
                if (InvokeRequired)
                {
                    delClearMe d = new delClearMe(Redraw);
                    this.BeginInvoke(d, null);
                }
                else
                {
                    FillBuffer();
                    this.Invalidate();
                }
            }
        }

        public void RemoveItem(StatusItem item)
        {
            if (m_items.ContainsKey(item.Index))
            {
                m_items.Remove(item.Index);
            }
            Reset();
        }

        public void ResetHoriz()
        {
            XOffset = 0;
        }

        public void SetSelectedIndexToZero()
        {
            if (InvokeRequired)
            {
                delClearMe d = new delClearMe(SetSelectedIndexToZero);
                this.BeginInvoke(d, null);
            }
            else
            {
                if (m_items != null && m_items.Count>0)
                {
                    m_items[m_selectedIndex].Selected = false;
                    SlidingPortal.ReRenderItem(m_items[m_selectedIndex]);
                    m_selectedIndex = m_items.Count - 1;
                    m_items[m_selectedIndex].Selected = true;
                    SlidingPortal.ReRenderItem(m_items[m_selectedIndex]);
                    StatusItem s = SelectedItem as StatusItem;
                    if (s != null)
                    {
                        RightMenu.UserName = s.Tweet.user.screen_name;
                        LastSelectedItems.SetLastSelected(CurrentList(), (m_items[m_selectedIndex] as StatusItem).Tweet);
                    }
                    //FillBuffer();
                }
            }
        }


        public void SetSelectedMenu(SideMenuItem RequestedMenuItem)
        {
            if (RightMenu.Contains(RequestedMenuItem))
            {
                RightMenu.SelectedItem = RequestedMenuItem;
            }
            if (LeftMenu.Contains(RequestedMenuItem))
            {
                LeftMenu.SelectedItem = RequestedMenuItem;
            }
        }

        public void UnHookKey()
        {
            this.Parent.KeyDown -= new KeyEventHandler(OnKeyDown);
            this.Parent.KeyPress -= new KeyPressEventHandler(OnKeyPress);
        }

        // Protected Methods (11) 

        protected override void Dispose(bool disposing)
        {
            try
            {
                if (m_timer != null)
                {
                    m_timer.Enabled = false;
                }
                base.Dispose(disposing);
            }
            catch(ObjectDisposedException){}
        }

        protected override void OnGotFocus(EventArgs e)
        {
            base.OnGotFocus(e);
            InFocus = true;
        }


        protected override void OnKeyPress(KeyPressEventArgs e)
        {
            OnKeyPress(null, e);
        }

        protected void OnKeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (Char)Keys.Escape)
            {
                if (LeftMenu.IsExpanded && CurrentlyViewing == SideShown.Left)
                {
                    LeftMenu.CollapseExpandedMenu();
                    e.Handled = true;
                }
                else if (RightMenu.IsExpanded && CurrentlyViewing == SideShown.Right)
                {
                    RightMenu.CollapseExpandedMenu();
                    e.Handled = true;
                }
                else if (LeftMenu.Contains("Back"))
                {
                    LeftMenu.InvokeByText("Back");
                    e.Handled = true;
                }
                else
                {
                    if (CurrentlyViewing != SideShown.Middle)
                    {
                        SnapBack();
                        e.Handled = true;
                    }
                }
            }
            
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            if (InFocus)
            {
                OnKeyDown(null, e);
            }
        }

        
        protected void OnKeyDown(object sender, KeyEventArgs e)
        {
            base.OnKeyDown(e);
            if (ClickablesControl.Visible)
            {
                ClickablesControl.KeyDown(e);
                this.Invalidate();
                return;
            }
            if (e.KeyCode == (Keys.LButton | Keys.MButton | Keys.Back))
            {
                switch (CurrentlyViewing)
                {
                    case SideShown.Left:
                        {
                            LeftMenu.InvokeSelected();
                            break;
                        }
                    case SideShown.Right:
                        {
                            RightMenu.InvokeSelected();
                            break;
                        }
                    case SideShown.Middle:
                        {
                            if (fsDisplay.Visible) { fsDisplay.Visible = false; }
                            else
                            {
                                ShowClickablesControl();
                            }
                            break;
                        }
                }
            }
            if (e.KeyCode == System.Windows.Forms.Keys.Up)
            {
                if (CurrentlyViewing == SideShown.Middle)
                {
                    try
                    {
                        if (m_selectedIndex > 0)
                        {
                            UnselectCurrentItem();
                            m_selectedIndex--;
                            SelectAndJump();
                            SetRightMenuUser();
                        }
                    }
                    catch
                    {
                    }
                }
                else
                {
                    if (CurrentlyViewing == SideShown.Right)
                    {
                        RightMenu.SelectUp();
                    }
                    if (CurrentlyViewing == SideShown.Left)
                    {
                        LeftMenu.SelectUp();
                    }
                }
            }
            if (e.KeyCode == System.Windows.Forms.Keys.Down)
            {
                if (CurrentlyViewing == SideShown.Middle)
                {
                    try
                    {
                        if (m_selectedIndex < m_items.Count - 1)
                        {
                            UnselectCurrentItem();
                            m_selectedIndex++;
                            SelectAndJump();
                            SetRightMenuUser();
                        }
                    }
                    catch
                    {
                    }
                }
                if (CurrentlyViewing == SideShown.Right)
                {
                    RightMenu.SelectDown();
                }
                if (CurrentlyViewing == SideShown.Left)
                {
                    LeftMenu.SelectDown();
                }
            }
            if (e.KeyCode == Keys.Right | e.KeyCode == Keys.F2) 
            {
                if (fsDisplay.Visible) 
                {
                    fsDisplay.FontSize++;
                    return; 
                }
                if (CurrentlyViewing != SideShown.Right)
                {
                    if (LeftMenu.IsExpanded)
                    {
                        LeftMenu.CollapseExpandedMenu();
                    }
                    else
                    {
                        OpenRightMenu();
                    }
                }
                else if (CurrentlyViewing == SideShown.Right)
                {
                    if (RightMenu.SelectedItem.HasChildren && !RightMenu.SelectedItem.Expanded)
                    {
                        RightMenu.SelectedItem.ClickMe();
                    }
                }
            }
            if (e.KeyCode == Keys.Left | e.KeyCode == Keys.F1)
            {
                if (fsDisplay.Visible) 
                {
                    fsDisplay.FontSize--;
                    return; 
                }
                if (CurrentlyViewing != SideShown.Left)
                {
                    if (RightMenu.IsExpanded)
                    {
                        RightMenu.CollapseExpandedMenu();
                    }
                    else
                    {
                        OpenLeftMenu();
                    }
                }
                else if (CurrentlyViewing == SideShown.Left)
                {
                    if (LeftMenu.SelectedItem.HasChildren && !LeftMenu.SelectedItem.Expanded)
                    {
                        LeftMenu.SelectedItem.ClickMe();
                    }
                }
            }
            if (PockeTwit.DetectDevice.DeviceType == PockeTwit.DeviceType.Standard)
            {
                int KeyToCheck = (e.KeyValue - 48);
                if (0 <= KeyToCheck && KeyToCheck <= 9)
                {
                    if (CurrentlyViewing == SideShown.Left)
                    {
                        LeftMenu.SelectByNumber(KeyToCheck);
                        LeftMenu.InvokeSelected();
                    }
                    else if (CurrentlyViewing == SideShown.Right)
                    {
                        RightMenu.SelectByNumber(KeyToCheck);
                        RightMenu.InvokeSelected();
                    }
                }
            }
            this.Refresh();
            
        }

        public void OpenRightMenu()
        {
            if (RightMenu.Count == 0)
                return;

            m_velocity.X = (this.Width / 2);
            XOffset = XOffset + 3;
            m_timer.Enabled = true;
        }

        public void OpenLeftMenu()
        {
            m_velocity.X = -(this.Width / 2);
            XOffset = XOffset - 3;
            m_timer.Enabled = true;
        }

        protected override void OnLostFocus(EventArgs e)
        {
            base.OnLostFocus(e);
            fsDisplay.Visible = false;
            InFocus = false;
        }

        private long ticks = 0;

        private bool movingItems = true;
        protected override void OnMouseDown(MouseEventArgs e)
        {
            System.Diagnostics.Debug.WriteLine("MouseDown!");
            HasMoved = false;
            //Fast scrolling on the right 10 pixels
            if (m_offset.X==0 && 
                e.X > this.Width - PointerSize - m_offset.X && 
                e.X < this.Width - m_offset.X)
            {
                m_scrollBarMove = true;
                movingItems = true;
                return;
            }
            
            base.OnMouseDown(e);

            
            Capture = true;

            m_mouseDown.X = e.X;
            m_mouseDown.Y = e.Y;
            m_mousePrev = m_mouseDown;

            if (XOffset < 0 && (0 - e.X) > XOffset)
            {
                movingItems = false;
                return;
            }
            if (XOffset > 0 && e.X > (ItemWidth-XOffset))
            {
                movingItems = false;
                return;
            }
            movingItems = true;

        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            if (ClickablesControl.Visible)
            {
                ClickablesControl.SelectAtPoint(new Point(e.X, e.Y));
                return;
            }
            if (!movingItems) 
            {
                
                int LeftOfItem = this.Width - XOffset;
                if (CurrentlyViewing == SideShown.Left)
                {
                    LeftMenu.OnMouseMove(e);
                }
                else
                {
                    RightMenu.OnMouseMove(e);
                }
                return; 
            }
            base.OnMouseMove(e);
            if (e.Button == MouseButtons.Left)
            {
                
                //Fast scroll
                if (m_scrollBarMove)
                {
                    float ScrollPos = (float)e.Y/this.Height;
                    int MoveToPos = (int)Math.Round(MaxYOffset * ScrollPos);
                    
                    float Percentage = (float)YOffset / MaxYOffset;

                    YOffset = MoveToPos;
                    
                    m_velocity.X = 0;
                    m_velocity.Y = 0;
                    Invalidate();
                    return;
                }
                Point currPos = new Point(e.X, e.Y);

                int distanceX = m_mousePrev.X - currPos.X;
                
                int distanceY = m_mousePrev.Y - currPos.Y;
                //if we're primarily moving vertically, ignore horizontal movement.
                //It makes it "stick" to the middle better!
                if (XOffset==0 & Math.Abs(distanceX) < Math.Abs(distanceY))
                {
                    distanceX = 0;
                }

                // if right menu is disabled, do not allow scroll
                if (RightMenu.Count == 0 && XOffset >= 0 && distanceX > 0)
                {
                    distanceX = 0;
                }

                m_velocity.X = distanceX / 2;
                m_velocity.Y = distanceY / 2;

                if (distanceX > 2 || distanceY > 2)
                {
                    HasMoved = true;
                }
                

                ClipVelocity();

                XOffset = XOffset + distanceX;
                YOffset = YOffset + distanceY;
                //m_offset.Offset(distanceX, distanceY);
                ClipScrollPosition();

                m_mousePrev = currPos;

                Invalidate();
            }
        }

        public bool IgnoreMouse = false;

        protected override void OnMouseUp(MouseEventArgs e)
        {
            if (IgnoreMouse)
            {
                IgnoreMouse = false;
                return;
            }
            if (!movingItems) 
            {
                
                if (CurrentlyViewing == SideShown.Left)
                {
                    LeftMenu.OnMouseUp(e);
                }
                else
                {
                    RightMenu.OnMouseUp(e);
                }
                return; 
            }
            base.OnMouseUp(e);

            if (ClickablesControl.Visible)
            {
                if (!ClickablesControl.CheckForClicks(new Point(e.X, e.Y)))
                {
                    HideClickablesControl();
                }
                Invalidate();
                return;
            }


            //If we're fast-scrolling. stop
            if (m_scrollBarMove)
            {
                m_scrollBarMove = false;
                RerenderPortal();
                return;
            }
            // Did the click end on the same item it started on?
            bool sameX = Math.Abs(e.X - m_mouseDown.X) < m_itemWidth;
            bool sameY = Math.Abs(e.Y - m_mouseDown.Y) < ClientSettings.ItemHeight;

            if (sameY)
            {
                // Yes, so select that item or menuiten
                if (Math.Abs(e.X - m_mouseDown.X) < MaxVelocity)
                {
                    int OldSelected = m_selectedIndex;
                    SelectItemOrMenu(e);
                    //Check for double-click!
                    if (m_selectedIndex == OldSelected)
                    {
                        long NowTicks = DateTime.Now.Ticks;
                        if ((NowTicks - ticks) < new TimeSpan(0, 0, 0, 0, 500).Ticks)
                        {
                            ShowClickablesControl();
                            return;
                        }
                        ticks = NowTicks;
                    }
                    Invalidate();
                    if (menuwasClicked)
                    {
                        menuwasClicked = false;
                        return;
                    }
                }
            }
            else
            {
                m_timer.Enabled = true;
            }

            try
            {
                //Check if we're half-way to menu
                if (XOffset > 0 && XOffset <= this.Width)
                {
                    m_timer.Enabled = true;
                    if (XOffset > (this.Width * .51))
                    {
                        //Scroll to other side
                        m_velocity.X = (this.Width / 10);
                    }
                    else
                    {
                        m_velocity.X = -(this.Width / 10);
                        //Scroll back
                    }
                }

                if (XOffset < 0 && XOffset >= 0 - this.Width)
                {
                    m_timer.Enabled = true;
                    if (XOffset < (0 - (this.Width * .6)))
                    {
                        //Scroll to other side
                        m_velocity.X = -(this.Width / 10);
                    }
                    else
                    {
                        m_velocity.X = (this.Width / 10);
                        //Scroll back
                    }
                }

                m_mouseDown.Y = -1;
                Capture = false;

                Invalidate();
            }
            catch (ObjectDisposedException)
            { }
            if (!HasMoved)
            {
                CheckForClicks(new Point(e.X, e.Y));
            }
            HasMoved = false;
            if (HasMoved)
            {
                RerenderPortal();
            }
        }
        

        public void SnapBack()
        {
            try
            {
                if (CurrentlyViewing == SideShown.Right)
                {
                    Capture = false;
                    m_timer.Enabled = true;
                    m_velocity.X = -(this.Width / 10);
                }
                else if (CurrentlyViewing == SideShown.Left)
                {
                    Capture = false;
                    m_timer.Enabled = true;
                    m_velocity.X = (this.Width / 10);
                }
            }
            catch (ObjectDisposedException) { }
        }

        private void FillBuffer()
        {
            if (m_items.Count > 0)
            {
                /*
                FillImmediateBuffer();
                //I want to do this async, but I'm running into race conditions right now.
                FillBackBuffer(null);
                 */
                SlidingPortal.SetItemList(new List<IDisplayItem>(m_items.Values));
                SlidingPortalCurrentEnd = SlidingPortal.MaxItems;
                //SlidingPortal.RenderImmediately();
            }
        }
        

        protected override void OnPaint(PaintEventArgs e)
        {
            using (Image flickerBuffer = new Bitmap(this.Width, this.Height))
            {
                using (Graphics flickerGraphics = Graphics.FromImage(flickerBuffer))
                {
                    flickerGraphics.Clear(ClientSettings.BackColor);

                    if (SlidingPortalOffset > SlidingPortal.BitmapHeight | SlidingPortalOffset < 0)
                    {
                        using (Brush sBrush = new SolidBrush(ClientSettings.ForeColor))
                        {
                            flickerGraphics.DrawString("Let me catch up...", ClientSettings.TextFont, sBrush, this.Bounds);
                        }
                    }
                    flickerGraphics.DrawImage(SlidingPortal.Rendered, 0 - XOffset, 0 - SlidingPortalOffset);
                    
                    if (XOffset > 0)
                    {
                        DrawMenu(flickerGraphics, SideShown.Right);
                    }
                    else if (XOffset < 0)
                    {
                        DrawMenu(flickerGraphics, SideShown.Left);
                    }
                    DrawPointer(flickerGraphics);
                    
                    if ((CurrentList() == "Friends_TimeLine" && PockeTwit.GlobalEventHandler.FriendsUpdating) || (CurrentList() == "Messages_TimeLine" && PockeTwit.GlobalEventHandler.MessagesUpdating))
                    {
                        NotificationArea.ShowNotification("Updating...");
                    }
                    else
                    {
                        NotificationArea.HideNotification();
                    }

                    NotificationArea.DrawNotification(flickerGraphics, this.Bottom, this.Width);
                    ErrorPopup.DrawNotification(flickerGraphics, this.Bottom, this.Width);

                    if (ClickablesControl.Visible)
                    {
                        AdjustBrightness((Bitmap)flickerBuffer, -60);
                        ClickablesControl.Render(flickerGraphics);
                    }
                    e.Graphics.DrawImage(flickerBuffer, 0, 0);
                }
            }
        }

        protected override void OnPaintBackground(PaintEventArgs e)
        {
            // Do nothing
        }


        private Size oldSize = new Size(0,0);
        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);


            if (this.Visible)
            {
                if (this.Width != oldSize.Width)
                {
                    RerenderBySize();
                }
                LeftMenu.Height = this.Height;
                RightMenu.Height = this.Height;
                LeftMenu.Width = this.Width;
                RightMenu.Width = this.Width;
            }
            oldSize = new Size(this.Width, this.Height);
        }

        public void RerenderBySize()
        {
            base.Visible = false;
            Application.DoEvents();
            LeftMenu.Height = this.Height;
            LeftMenu.Width = this.Width;
            
            RightMenu.Height = this.Height;
            RightMenu.Width = this.Width;

            ClickablesControl.Top = this.Top + 20;
            ClickablesControl.Left = this.Left + 20;
            ClickablesControl.Width = this.Width - 40;
            ClickablesControl.Height = this.Height - 40;

            this.ItemWidth = this.Width;

            foreach (IDisplayItem item in m_items.Values)
            {
                item.Bounds = ItemBounds(0, item.Index);
                if (item is StatusItem)
                {
                    (item as StatusItem).ResetTexts();
                }
            }
            

            SlidingPortal.Clear();
            FillBuffer();
            SelectAndJump();
            this.Repaint();
            base.Visible = true;
        }



        // Private Methods (22) 
        private void CheckForClicks(Point point)
        {
            if (m_items.Count == 0) { return; }
            try
            {
                int itemNumber = FindIndex(point.X, point.Y).Y;
                if (itemNumber > m_items.Count - 1) { return; }

                if (m_items[itemNumber] is StatusItem)
                {
                    StatusItem s = (StatusItem)m_items[itemNumber];

                    foreach (StatusItem.Clickable c in s.Tweet.Clickables)
                    {
                        Rectangle itemRect = s.Bounds;
                        itemRect.Offset(-XOffset, -YOffset);
                        Rectangle cRect = new Rectangle(((int)c.Location.X + itemRect.Left) + (ClientSettings.SmallArtSize + 10), (int)c.Location.Y + itemRect.Top, (int)c.Location.Width, (int)c.Location.Height);
                        if (cRect.Contains(point))
                        {
                            if (WordClicked != null)
                            {
                                WordClicked(c.Text);
                            }
                        }
                    }
                }
                else
                {
                    // call clicked handler from the interface
                    m_items[itemNumber].OnMouseClick(point);
                }
            }
            catch (ObjectDisposedException)
            {
                //Oops, we're closing shop.
            }
            catch (KeyNotFoundException)
            {
            }
        }


        void ClickablesControl_WordClicked(string TextClicked)
        {
            if (TextClicked == PockeTwit.Localization.XmlBasedResourceManager.GetString("Detailed View", "Detailed View") | ShortText.IsShortTextURL(TextClicked))
            {
                //Show the full tweet somehow.
                StatusItem s = (StatusItem)SelectedItem;

                fsDisplay.Status = s.Tweet;
                fsDisplay.Render();
                fsDisplay.Visible = true;
                HideClickablesControl();
                
                /*
                string fullText = null;
                if (Yedda.ShortText.IsShortTextURL(s.Tweet.text))
                {
                    string[] splitup = s.Tweet.text.Split(new char[] { ' ' });
                    fullText = Yedda.ShortText.GetFullText(splitup[splitup.Length - 1]);
                }
                else
                {
                    fullText = s.Tweet.text;   
                }
                MessageBox.Show(fullText, s.Tweet.user.screen_name);
                 */
            }
            else if (WordClicked != null)
            {
                WordClicked(TextClicked);
            }
        }

        private void ClipScrollPosition()
        {
            if (XOffset < MinXOffset)
            {
                XOffset = MinXOffset;
                m_velocity.X = 0;
            }
            else if (XOffset > MaxXOffset)
            {
                XOffset = MaxXOffset;
                m_velocity.X = 0;
            }
            if (YOffset < 0)
            {
                YOffset = 0;
                m_velocity.Y = 0;
            }
            else if (YOffset > MaxYOffset)
            {
                YOffset = MaxYOffset;
                m_velocity.Y = 0;
            }
        }

        private void ClipVelocity()
        {
            m_velocity.X = Math.Min(m_velocity.X, m_maxVelocity);
            m_velocity.X = Math.Max(m_velocity.X, -m_maxVelocity);

            m_velocity.Y = Math.Min(m_velocity.Y, m_maxVelocity);
            m_velocity.Y = Math.Max(m_velocity.Y, -m_maxVelocity);
        }

        private void CreateBackBuffer()
        {
            
            foreach (StatusItem item in m_items.Values)
            {
                if (item is StatusItem)
                {
                    StatusItem sItem = (StatusItem)item;
                    sItem.ParentGraphics = SlidingPortal._RenderedGraphics;
                }
            }
        }

        private void DrawMenu(Graphics m_backBuffer, SideShown Side)
        {
            Bitmap MenuMap;
            if (Side == SideShown.Left)
            {
                MenuMap = LeftMenu.Rendered;
                if (MenuMap != null)
                {
                    m_backBuffer.DrawImage(MenuMap, (0 - this.Width) + Math.Abs(XOffset), 0);
                }
            }
            else if (Side == SideShown.Right)
            {
                MenuMap = RightMenu.Rendered;
                if (MenuMap != null)
                {
                    m_backBuffer.DrawImage(MenuMap, this.Width - XOffset, 0);
                }
            }
            
        }

        
        int PointerSize = ClientSettings.TextSize;
        int PointerHalf = ClientSettings.TextSize / 2;

        private void DrawPointer(Graphics g)
        {
            int x = Width - m_offset.X;
            float Percentage = 0;
            if (YOffset > 0)
            {
                Percentage = (float)YOffset / MaxYOffset;
            }
            int Position = (int)Math.Round(Height * Percentage);
            using (SolidBrush SBrush = new SolidBrush(ClientSettings.PointerColor))
            {
                Point a = new Point(x - PointerSize, Position);
                Point b = new Point(x, Position - PointerHalf);
                Point c = new Point(x, Position + PointerHalf);
                Point[] Triangle = new Point[]{a,b,c};
                g.FillPolygon(SBrush, Triangle);
            }
            

        }


        private Point FindIndex(int x, int y)
        {
            Point index = new Point(0, 0);

            index.Y = ((y + YOffset - Bounds.Top) / (ClientSettings.ItemHeight));
            
            return index;
        }

        private global::PockeTwit.FingerUI.Menu.SideMenuItem GetMenuItemForPoint(MouseEventArgs e)
        {
            int LeftOfItem = this.Width - Math.Abs(XOffset);
            SideMenu MenuToCheck = null;
            if (XOffset > 0)
            {
                MenuToCheck = RightMenu;
            }
            else if (XOffset < 0)
            {
                MenuToCheck = LeftMenu;
            }
            return MenuToCheck.GetMenuItemForPoint(new Point(e.X, e.Y), LeftOfItem);
        }

        private Rectangle ItemBounds(int x, int y)
        {
            int itemY = Bounds.Top + (ClientSettings.ItemHeight * y);

            return new Rectangle(Bounds.Left, itemY, ItemWidth, ClientSettings.ItemHeight);
            
        }

        private void m_timer_Tick(object sender, EventArgs e)
        {
            
            if (!Capture && (m_velocity.Y != 0 || m_velocity.X != 0))
            {
                XDirection dir = m_velocity.X > 0 ? XDirection.Right : XDirection.Left;
                XDirection currentPos = XOffset > 0 ? XDirection.Right : XDirection.Left;

                //m_offset.Offset(m_velocity.X, m_velocity.Y);
                XOffset = XOffset + m_velocity.X;
                YOffset = YOffset + m_velocity.Y;

                if (currentPos == XDirection.Right & dir == XDirection.Left)
                {
                    if (XOffset <= 0)
                    {
                        XOffset = 0;
                        m_velocity.X = 0;
                    }
                }
                else if (currentPos == XDirection.Left & dir == XDirection.Right)
                {
                    if (XOffset >= 0)
                    {
                        XOffset = 0;
                        m_velocity.X = 0;
                    }
                }
                            
                ClipScrollPosition();
                
                // Slow down
                if (m_velocity.Y < 0)
                {
                    m_velocity.Y++;
                }
                else if (m_velocity.Y > 0)
                {
                    m_velocity.Y--;
                }
                if (m_velocity.Y == 0 && m_velocity.X == 0)
                {
                    m_timer.Enabled = false;
                    HasMoved = false;
                }

                Invalidate();
            }
        }

        private void Reset()
        {
            
            SlidingPortal._RenderedGraphics.Clear(ClientSettings.BackColor);
            string Message = "One moment please...";
            if (m_items.Count == 0) { Message = "There are no items to display"; }
            using (Brush sBrush = new SolidBrush(ClientSettings.ForeColor))
            {
                SlidingPortal._RenderedGraphics.DrawString(Message, ClientSettings.TextFont, sBrush, new RectangleF(0, 0, this.Width, this.Height));
            }
            m_timer.Enabled = false;
            if (m_items.Count > 0)
            {
                m_items[m_selectedIndex].Selected = false;
                SlidingPortal.ReRenderItem(m_items[m_selectedIndex]);
            }
            m_selectedIndex = 0;
            Capture = false;
            m_velocity.X = 0;
            m_velocity.Y = 0;
            YOffset = 0;
            FillBuffer();
            SetSelectedIndexToZero();
            Invalidate();
        }

        public void SetRightMenuUser()
        {
            StatusItem s = (StatusItem)m_items[m_selectedIndex];
            RightMenu.UserName = s.Tweet.user.screen_name;
        }

        private void SelectAndJump()
        {
            //m_items[m_selectedIndex].Render(m_backBuffer, m_items[m_selectedIndex].Bounds);
            IDisplayItem item = null;
            try
            {
                item = m_items[m_selectedIndex];
            }
            catch (KeyNotFoundException)
            {
                return;
            }
            item.Selected = true;
            
            SlidingPortal.ReRenderItem(m_items[m_selectedIndex]);
            if (SelectedItemChanged != null)
            {
                SelectedItemChanged(this, new EventArgs());
            }
            if (fsDisplay.Visible)
            {
                StatusItem s = m_items[m_selectedIndex] as StatusItem;
                if (s != null)
                {
                    fsDisplay.Status = s.Tweet;
                    fsDisplay.Render();
                }
            }
            JumpToItem(item);
            RerenderPortal();
        }

        private void SelectItemOrMenu(MouseEventArgs e)
        {
            if (e.X > this.Width-XOffset)
            {
                //MenuItem selected
                SideMenuItem Item = GetMenuItemForPoint(e);
                if (Item!=null)
                {
                    Item.ClickMe();
                    menuwasClicked = true;
                }
            }
            else if ((XOffset) < 0 && e.X<Math.Abs(XOffset))
            {
                SideMenuItem Item = GetMenuItemForPoint(e);
                if (Item != null) 
                {
                    Item.ClickMe();
                    menuwasClicked = true;
                }
            }
            else
            {
                Point selectedIndex = FindIndex(e.X, e.Y);
                if (selectedIndex.Y!= m_selectedIndex)
                {
                    if (m_items.ContainsKey(selectedIndex.Y))
                    {
                        m_items[m_selectedIndex].Selected = false;
                        SlidingPortal.ReRenderItem(m_items[m_selectedIndex]);
                        m_selectedIndex = selectedIndex.Y;
                        m_items[m_selectedIndex].Selected = true;
                        //PockeTwit.LastSelectedItems.SetLastSelected(CurrentList(), m_items[m_selectedIndex].Tweet);
                        SlidingPortal.ReRenderItem(m_items[m_selectedIndex]);
                        if (SelectedItemChanged != null)
                        {
                            SelectedItemChanged(this, new EventArgs());
                        }
                        if (fsDisplay.Visible && m_items[m_selectedIndex] is StatusItem)
                        {
                            StatusItem s = (StatusItem)m_items[m_selectedIndex];
                            fsDisplay.Status = s.Tweet;
                            fsDisplay.Render();
                        }
                        if (m_items[m_selectedIndex] is StatusItem)
                            SetRightMenuUser();
                        m_velocity.X = 0;
                        m_velocity.Y = 0;
                    }
                }
                else
                {
                    if (SelectedItemClicked != null)
                    {
                        SelectedItemClicked(this, new EventArgs());
                    }
                }
            }

        }


        private void HideClickablesControl()
        {
            ClickablesControl.Visible = false;
        }
        private void ShowClickablesControl()
        {

            StatusItem s = null;
            try
            {
                if (!(m_items[m_selectedIndex] is StatusItem))
                {
                    m_items[m_selectedIndex].OnMouseDblClick();
                    return;
                }
                s = (StatusItem)m_items[m_selectedIndex];
            }
            catch (KeyNotFoundException) 
            {
                return;
            }
            if (s == null) { return; }
            ClickablesControl.Items = s.Tweet.Clickables;
            ClickablesControl.Top = this.Top + 20;
            ClickablesControl.Left = this.Left + 20;
            ClickablesControl.Width = this.Width - 40;
            ClickablesControl.Height = this.Height - 40;
            ClickablesControl.Visible = true;
            Invalidate();
        }

        private void UnselectCurrentItem()
        {
            if (m_selectedIndex >= 0)
            {
                m_items[m_selectedIndex].Selected = false;
                SlidingPortal.ReRenderItem(m_items[m_selectedIndex]);
            }
        }

        public static void AdjustBrightness(Bitmap image, int brightness)
        {
            int offset = 0;
            brightness = (brightness * 255) / 100;
            // GDI+ still lies to us - the return format is BGR, NOT RGB.
            System.Drawing.Imaging.BitmapData bmData = image.LockBits(new Rectangle(0, 0, image.Width, image.Height), System.Drawing.Imaging.ImageLockMode.ReadWrite, System.Drawing.Imaging.PixelFormat.Format24bppRgb);

            int stride = bmData.Stride;
            IntPtr Scan0 = bmData.Scan0;

            int nVal = 0;
            int nOffset = stride - image.Width * 3;
            int nWidth = image.Width * 3;

            for (int y = 0; y < image.Height; ++y)
            {
                for (int x = 0; x < nWidth; ++x)
                {
                    nVal = System.Runtime.InteropServices.Marshal.ReadByte(Scan0, offset) + brightness;

                    if (nVal < 0)
                        nVal = 0;
                    if (nVal > 255)
                        nVal = 255;

                    System.Runtime.InteropServices.Marshal.WriteByte(Scan0, offset, (byte)nVal);
                    ++offset;
                }
                offset += nOffset;
            }
            image.UnlockBits(bmData);
        }


        #endregion Methods 

        #region Nested Classes (1) 
        public class ItemList : Dictionary<int, IDisplayItem>
        {
        }
        #endregion Nested Classes 
        
    }
}