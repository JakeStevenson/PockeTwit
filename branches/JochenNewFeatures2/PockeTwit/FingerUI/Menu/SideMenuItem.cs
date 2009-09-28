using System.Collections.Generic;
using PockeTwit.TimeLines;

namespace PockeTwit.FingerUI.Menu
{
    public delegate void delMenuClicked();

    public class SideMenuItem
    {
        #region Delegates

        public delegate void delItemExpanded(SideMenuItem sender, bool Opened);

        #endregion

        public delMenuClicked ClickedMethod;

        private SideMenu ParentMenu;
        private string _TextTemplate;
        private bool _Visible = true;
        public bool CanHide;
        public List<SideMenuItem> SubMenuItems = new List<SideMenuItem>();

        public SideMenuItem(delMenuClicked Callback, string TextTemplate, SideMenu Parent)
        {
            Initialize(TextTemplate, Callback, Parent, null);
        }

        public SideMenuItem(delMenuClicked Callback, string TextTemplate, SideMenu Parent, string List)
        {
            Initialize(TextTemplate, Callback, Parent, List);
        }

        private void Initialize(string TextTemplate, delMenuClicked Callback, SideMenu Parent, string List)
        {
            _TextTemplate = PockeTwit.Localization.XmlBasedResourceManager.GetString(TextTemplate); 
            ClickedMethod = Callback;
            ParentMenu = Parent;
            CorrespondingList = List;
        }

        public bool HasChildren
        {
            get { return SubMenuItems.Count > 0; }
        }

        public bool Expanded { get; set; }

        public bool Visible
        {
            get { return _Visible; }
            set
            {
                if (value != _Visible)
                {
                    _Visible = value;
                    if(!value)
                    {
                        Expanded = false;
                    }
                    ParentMenu.IsDirty = true;
                    ParentMenu.SetMenuHeight();
                }
                
            }
        }

        private string _CorrespondingList;
        public string CorrespondingList
        {
            get
            {
                return _CorrespondingList;
            }
            set
            {
                _CorrespondingList = value;
                if(!string.IsNullOrEmpty(_CorrespondingList))
                {
                    LastSelectedItems.UnreadCountChanged += new LastSelectedItems.delUnreadCountChanged(LastSelectedItems_UnreadCountChanged);
                }
                else
                {
                    LastSelectedItems.UnreadCountChanged -= new LastSelectedItems.delUnreadCountChanged(LastSelectedItems_UnreadCountChanged);
                }
            }
        }

        void LastSelectedItems_UnreadCountChanged(string TimeLine, int Count)
        {
            if(TimeLine==_CorrespondingList)
            {
                this.ParentMenu.IsDirty = true;
            }
        }

        public string Text
        {
            get
            {
                if(!string.IsNullOrEmpty(CorrespondingList))
                {
                    return _TextTemplate + newItemsText(LastSelectedItems.GetUnreadItems(CorrespondingList));
                }
                return _TextTemplate;
            }
            set
            {
                if (value != _TextTemplate)
                {
                    Expanded = false;
                    MenuExpandedOrCollapsed(null, Expanded);
                    ParentMenu.IsDirty = true;
                    _TextTemplate = PockeTwit.Localization.XmlBasedResourceManager.GetString(value);

                }
            }
        }

        private static string newItemsText(int count)
        {
            if (count > 0)
            {
                return " (" + count + ")";
            }
            return "";
        }

        public void ClickMe()
        {
            if (SubMenuItems.Count > 0)
            {
                Expanded = !Expanded;
                if (Expanded)
                {
                    MenuExpandedOrCollapsed(this, Expanded);
                }
                else
                {
                    MenuExpandedOrCollapsed(null, Expanded);
                }
            }
            else
            {
                if (ClickedMethod != null)
                {
                    ClickedMethod();
                }
                DoneWithClick();
            }
        }

        public event delMenuClicked DoneWithClick = delegate { };

        public event delItemExpanded MenuExpandedOrCollapsed = delegate { };
    }
}