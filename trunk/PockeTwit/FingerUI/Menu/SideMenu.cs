using System;
using System.Drawing;
using System.Collections.Generic;

namespace PockeTwit.FingerUI.Menu
{
    public class SideMenu : System.Windows.Forms.Control
    {
        public delegate void DelAnimateMe();
        public event DelAnimateMe AnimateMe = delegate { };
        public bool IsAnimating { get { return _animationStep >= 0; }}

        public SideMenu(KListControl.SideShown side)
        {
            _side = side;
            _height = System.Windows.Forms.Screen.PrimaryScreen.Bounds.Height;
            _width = System.Windows.Forms.Screen.PrimaryScreen.Bounds.Width;
        }
        public delegate void DelClearMe();
        public event DelClearMe ItemWasClicked = delegate { };
        public event DelClearMe NeedRedraw = delegate { };
        private int _topOfSubMenu;

        private SideMenuItem _expandedItem;
        private SideMenuItem ExpandedItem
        {
            get
            {
                return _expandedItem;
            }
            set
            {
                
                if (value == null)
                {
                    if (_expandedItem != null)
                    {
                        ExpandedItem.Expanded = false;
                        IsDirty = true;
                        SelectedItem = ExpandedItem;
                        NeedRedraw();

                        _animationTextColor = ClientSettings.DimmedColor;
                        _animationLineColor = ClientSettings.DimmedLineColor;
                        _isFading = false;
                        _animationStep = 6;
                        AnimateMe();
                    }
                    //SelectedItem = _expandedItem;
                }
                else
                {
                    if (_expandedItem != null)
                    {
                        _expandedItem.Expanded = false;
                        IsDirty = true;
                        NeedRedraw();
                    }

                    SelectedItem = value.SubMenuItems[0];
                    _animationStep = 6;
                    _isFading = true;
                    _animationTextColor = ClientSettings.MenuTextColor;
                    _animationLineColor = ClientSettings.LineColor;
                    AnimateMe();
                }
                _expandedItem = value;
            }
        }
        public bool IsExpanded
        {
            get
            {
                return ExpandedItem != null;
            }
        }
        private Bitmap _rendered;
        public Bitmap Rendered
        {
            get
            {
                if (IsDirty)
                {
                    DrawMenu();
                }
                return _rendered;
            }
        }

        public bool IsDirty = true;

        private readonly KListControl.SideShown _side;
        private readonly List<SideMenuItem> _items = new List<SideMenuItem>();
        private SideMenuItem _selectedItem;
        
        public SideMenuItem SelectedItem
        {
            get 
            {
                lock (_items)
                {
                    if (_items.Count == 0)
                    {
                        return null;
                    }
                    if(_selectedItem==null)
                    {
                        return _items[0];
                    }
                    return _selectedItem;
                }
            }
            set
            {
                _selectedItem = value;
                IsDirty = true;
            }
        }
        public int Count
        {
            get { return _items.Count; }
        }
        private int _itemHeight;
        private int _topOfMenu;
        public int ItemHeight { get { return _itemHeight; } }
        public int TopOfMenu { get { return _topOfMenu; } }

        private string _userName;
        public string UserName
        {
            get
            {
                return _userName;
            }
            set
            {
                _userName = value;
                IsDirty=true;
            }
        }

        private int _height;
        public new int Height
        {
            get { return _height; }
            set
            {
                if (value != _height)
                {
                    _height = value;
                    SetMenuHeight();
                    if (_width > 0 && _height > 0)
                    {
                        if (_rendered != null)
                        {
                            _rendered.Dispose();
                            GC.Collect();
                            GC.WaitForPendingFinalizers();
                        }
                        _rendered = new Bitmap(_width, _height);
                    }
                    IsDirty = true;
                }
            }
        }

        private int _width;
        public new int Width
        {
            get{return _width;}
            set
            {
                if (value != _width)
                {
                    _width = value;
                    if (_width > 0 && _height > 0)
                    {
                        if (_rendered != null)
                        {
                            _rendered.Dispose();
                            GC.Collect();
                            GC.WaitForPendingFinalizers();
                        }
                        _rendered = new Bitmap(_width, _height);
                    }
                    IsDirty = true;
                }
            }
        }

        public void ForceRerender()
        {
            IsDirty = true;
            _animationTextColor = ClientSettings.MenuTextColor;
            _animationLineColor = ClientSettings.LineColor;
            _animationStep = 0;
        }

        public void SetMenuHeight()
        {
            if (InvokeRequired)
            {
                DelClearMe d = SetMenuHeight;
                Invoke(d, null);
            }
            else
            {
                if (_items.Count > 0)
                {
                    try
                    {
                        int screenHeight = System.Windows.Forms.Screen.PrimaryScreen.WorkingArea.Height;
                        //int screenHeight = this.Height;
                        if (!ClientSettings.IsMaximized)
                        {
                            //subtract once the menu height. The working area allready subtracted it once.
                            screenHeight = screenHeight - (System.Windows.Forms.SystemInformation.MenuHeight);
                        }

                        int multiplyer = 4;
                        if (DetectDevice.DeviceType == DeviceType.Standard )
                        {
                            multiplyer = 3;
                        }
                        int i = 0;
                        foreach (SideMenuItem item in _items)
                        {
                            if (item.Visible)
                            {
                                i++;
                            }
                        }
                        _itemHeight = (screenHeight - (ClientSettings.TextSize * multiplyer)) / i;
                        _topOfMenu = ((screenHeight / 2) - ((i * ItemHeight) / 2));
                        if (ClientSettings.IsMaximized)
                        {
                            _topOfMenu = _topOfMenu + (_itemHeight/2);
                        }
                    }
                    catch (ObjectDisposedException) { }
                }
            }
        }

        public void InsertMenuItem(int index, SideMenuItem item)
        {
            lock (_items)
            {
                _items.Insert(index, item);
                SetMenuHeight();
            }
            IsDirty=true;
        }

        public void ResetMenu(IEnumerable<SideMenuItem> newItems)
        {
            lock (_items)
            {
                foreach (SideMenuItem item in _items)
                {
                    item.DoneWithClick -= ItemDoneWithClick;
                    item.MenuExpandedOrCollapsed -= ItemMenuExpandedOrCollapsed;
                    if (item.HasChildren)
                    {
                        foreach (SideMenuItem subItem in item.SubMenuItems)
                        {
                            subItem.DoneWithClick -= ItemDoneWithClick;
                            subItem.MenuExpandedOrCollapsed -= ItemMenuExpandedOrCollapsed;
                        }
                    }
                }
                _items.Clear();
                if (newItems != null)
                {
                    _items.AddRange(newItems);
                    SetMenuHeight();
                    foreach (SideMenuItem item in _items)
                    {
                        item.DoneWithClick += ItemDoneWithClick;
                        item.MenuExpandedOrCollapsed += ItemMenuExpandedOrCollapsed;
                        if (item.HasChildren)
                        {
                            foreach (SideMenuItem subItem in item.SubMenuItems)
                            {
                                subItem.DoneWithClick += ItemDoneWithClick;
                                subItem.MenuExpandedOrCollapsed += ItemMenuExpandedOrCollapsed;
                            }
                        }
                    }
                }
            }
            IsDirty=true;
        }

        void ItemMenuExpandedOrCollapsed(SideMenuItem sender, bool opened)
        {
            ExpandedItem = sender;
            IsDirty = true;
            NeedRedraw();
        }

        void ItemDoneWithClick()
        {
            ExpandedItem = null;
            ItemWasClicked();
        }

        public void AddItems(IEnumerable<SideMenuItem> newItems)
        {
            lock (_items)
            {
                foreach (SideMenuItem item in newItems)
                {
                    if (!_items.Contains(item))
                    {
                        _items.Add(item);
                    }
                }
                SetMenuHeight();
            }
            IsDirty=true;
        }
        public void RemoveItem(SideMenuItem oldItem)
        {
            lock (_items)
            {
                if (_items.Contains(oldItem))
                {
                    oldItem.DoneWithClick -= ItemDoneWithClick;
                    _items.Remove(oldItem);
                }
                SetMenuHeight();
            }
            IsDirty=true;
        }
        public SideMenuItem[] GetItems()
        {
            lock (_items)
            {
                return _items.ToArray();
            }
        }

        public bool Contains(SideMenuItem itemToSeek)
        {
            lock (_items)
            {
                return _items.Contains(itemToSeek);
            }
        }
        public bool Contains(string textOfMenuItem)
        {
            bool bFound = false;
            lock (_items)
            {
                foreach (SideMenuItem item in _items)
                {
                    if (item.Text == textOfMenuItem && item.Visible)
                    {
                        bFound = true;
                        break;
                    }
                }
            }
            return bFound;
        }

        public void SelectDown()
        {
            List<SideMenuItem> itemsToUse = _items;
            if (ExpandedItem != null)
            {
                itemsToUse = ExpandedItem.SubMenuItems;
            }
            int prevSelected = itemsToUse.IndexOf(SelectedItem);
            lock (itemsToUse)
            {
                _selectedItem = prevSelected < itemsToUse.Count - 1 ? itemsToUse[prevSelected + 1] : itemsToUse[0];
            }
            if (!_selectedItem.Visible)
            {
                SelectDown();
            }
            IsDirty=true;
        }
        public void SelectUp()
        {
            List<SideMenuItem> itemsToUse = _items;
            if (ExpandedItem != null)
            {
                itemsToUse = ExpandedItem.SubMenuItems;
            }
            lock (itemsToUse)
            {
                int prevSelected = itemsToUse.IndexOf(SelectedItem);
                _selectedItem = prevSelected > 0 ? itemsToUse[prevSelected - 1] : itemsToUse[itemsToUse.Count - 1];
            }
            if (!_selectedItem.Visible)
            {
                SelectUp();
            }
            IsDirty=true;
        }

        public void SelectByText(string itemToSelect)
        {
            List<SideMenuItem> itemsToUse = _items;
            if (ExpandedItem != null)
            {
                itemsToUse = ExpandedItem.SubMenuItems;
            }
            lock (itemsToUse)
            {
                foreach (SideMenuItem item in itemsToUse)
                {
                    if (item.Text == itemToSelect)
                    {
                        _selectedItem = item;
                        IsDirty = true;

                        break;
                    }
                }
            }
        }

        public void InvokeSelected()
        {
            SelectedItem.ClickMe();
        }

        public void InvokeByText(string itemToInvoke)
        {
            lock (_items)
            {
                foreach (SideMenuItem item in _items)
                {
                    if (item.Text == itemToInvoke)
                    {
                        item.ClickMe();
                        break;
                    }
                }
            }
        }

        public void SelectByNumber(int number)
        {
            List<SideMenuItem> itemsToUse = _items;
            if (ExpandedItem != null)
            {
                itemsToUse = ExpandedItem.SubMenuItems;
            }
            lock (itemsToUse)
            {
                int realNum = 0;
                if (itemsToUse[0].Visible)
                {
                    realNum = -1;
                }
                for (int i = 0; i < itemsToUse.Count; i++)
                {
                    if (itemsToUse[i].Visible)
                    {
                        realNum++;
                    }
                    if (realNum == number)
                    {
                        _selectedItem = itemsToUse[i];
                        break;
                    }
                }
            }
            IsDirty = true;
        }
        public void ReplaceItem(SideMenuItem original, SideMenuItem @new)
        {
            lock (_items)
            {
                for (int i = 0; i < _items.Count; i++)
                {
                    if (_items[i] == original)
                    {
                        _items[i] = @new;
                    }
                }
            }
            IsDirty=true;
        }

        public new void OnMouseMove(System.Windows.Forms.MouseEventArgs e)
        {
            SideMenuItem i = GetMenuItemForTransposedPoint(new Point(e.X, e.Y));
            if (i != null)
            {
                SelectedItem = i;
                IsDirty = true;
                NeedRedraw();
            }
        }

        public new void OnMouseUp(System.Windows.Forms.MouseEventArgs e)
        {
            SideMenuItem i = GetMenuItemForTransposedPoint(new Point(e.X, e.Y));
            if (i != null)
            {
                SelectedItem = i;
                CollapseExpandedMenu();
                i.ClickMe();
            }
            else
            {
                CollapseExpandedMenu();
            }
        }

        public void CollapseExpandedMenu()
        {
            if (ExpandedItem != null)
            {
                ExpandedItem.Expanded = false;
                ExpandedItem = null;
                IsDirty = true;
                NeedRedraw();
            }
        }

        private void DrawMenu()
        {
            var expandedRect = new Rectangle();
            lock (_items)
            {
                if (_items.Count == 0)
                    return;

                var menuTextColor = AnimationTextColor;
                var menuLineColor = AnimationLineColor;
                
                int i = 1;
                if (_items[0].CanHide && _items[0].Visible)
                {
                    i = 0;
                }
                if (_rendered == null)
                {
                    _rendered = new Bitmap(_width, _height);
                }
                using (Graphics mBackBuffer = Graphics.FromImage(_rendered))
                {
                    mBackBuffer.Clear(ClientSettings.BackColor);
                    int currentTop = TopOfMenu;

                    SideMenuItem[] itemList = GetItems();
                    foreach (SideMenuItem item in itemList)
                    {
                        if (item.Visible)
                        {
                            DrawSingleItem(i, mBackBuffer, 0, currentTop, item, menuLineColor, menuTextColor);

                            i++;

                            if (item.Expanded)
                            {
                                expandedRect = new Rectangle(0, currentTop, _width, ItemHeight);
                            }
                            currentTop = currentTop + ItemHeight;
                        }
                    }
                    if (ExpandedItem != null)
                    {
                        DrawSubMenu(ExpandedItem, expandedRect);
                    }
                    using (var whitePen = new Pen(ClientSettings.LineColor))
                    {
                        if (_side == KListControl.SideShown.Right)
                        {
                            mBackBuffer.DrawLine(whitePen, 0, 0, 0, Height);
                        }
                        else
                        {
                            mBackBuffer.DrawLine(whitePen, _width-1, 0, _width- 1, Height);
                        }
                    }
                    if (_animationStep == 0) 
                    {
                        _animationStep = -1; 
                    }
                    if (_animationStep > 0)
                    {
                        _animationStep = _animationStep - 2;
                        //IsDirty = true;
                    }
                }
                IsDirty = IsAnimating;
            }

        }

        private void DrawSingleItem(int i, Graphics mBackBuffer, int offset, int currentTop, SideMenuItem item, Color menuLineColor, Color menuTextColor)
        {
            int leftPos = 0;
            string displayItem = item.Text;
            
            if (DetectDevice.DeviceType == DeviceType.Standard)
            {
                displayItem = i + ". " + displayItem;
            }

            if (_side == KListControl.SideShown.Left)
            {
                int textWidth = (int)mBackBuffer.MeasureString(displayItem, ClientSettings.MenuFont).Width + ClientSettings.Margin;
                leftPos = _width - (textWidth + 5 + offset);
            }
            else
            {
                leftPos = leftPos + ClientSettings.Margin + 5 + offset;
            }
            using (var whitePen = new Pen(menuLineColor))
            {
                Rectangle menuRect;
                menuRect = _side == KListControl.SideShown.Left ? new Rectangle(0, currentTop, _width - offset, ItemHeight) : new Rectangle(offset, currentTop, _width, ItemHeight);

                Color BackColor;
                Color GradColor;
                Color TextColor;
                if (item == SelectedItem)
                {
                    BackColor = ClientSettings.SelectedBackColor;
                    GradColor = ClientSettings.SelectedBackGradColor;
                    TextColor = ClientSettings.MenuSelectedTextColor;
                }
                else
                {
                    BackColor = ClientSettings.BackColor;
                    GradColor = ClientSettings.BackGradColor;
                    TextColor = menuTextColor;
                }
                try
                {
                    Gradient.GradientFill.Fill(mBackBuffer, menuRect, BackColor, GradColor, Gradient.GradientFill.FillDirection.TopToBottom);
                }
                catch
                {
                    using (Brush backBrush = new SolidBrush(ClientSettings.SelectedBackColor))
                    {
                        mBackBuffer.FillRectangle(backBrush, menuRect);
                    }
                }
                mBackBuffer.DrawLine(whitePen, menuRect.Left, menuRect.Top, menuRect.Right, menuRect.Top);
                using (Brush sBrush = new SolidBrush(TextColor))
                {
                    var sFormat = new StringFormat {LineAlignment = StringAlignment.Center};
                    int textTop = ((menuRect.Bottom - menuRect.Top) / 2) + menuRect.Top;
                    displayItem = displayItem.Replace("@User", "@" + _userName);
                    mBackBuffer.DrawString(displayItem, ClientSettings.MenuFont, sBrush, leftPos, textTop, sFormat);
                }
                mBackBuffer.DrawLine(whitePen, menuRect.Left, menuRect.Bottom, menuRect.Right, menuRect.Bottom);
            }
        }

        

        private int _animationStep = -1;
        private static int _animationDelta
        {
            get
            {
                return ClientSettings.TextSize / 3;
            }
        }
        private static int rTextColorDelta
        {
            get
            {
                return (ClientSettings.DimmedColor.R - ClientSettings.MenuTextColor.R) / 3;
            }
        }
        private static int gTextColorDelta
        {
            get
            {
                return (ClientSettings.DimmedColor.G - ClientSettings.MenuTextColor.G) / 3;
            }
        }
        private static int bTextColorDelta
        {
            get
            {
                return (ClientSettings.DimmedColor.B - ClientSettings.MenuTextColor.B) / 3;
            }
        }

        private static int rLineColorDelta
        {
            get
            {
                return (ClientSettings.DimmedLineColor.R - ClientSettings.LineColor.R) / 3;
            }
        }
        private static int gLineColorDelta
        {
            get
            {
                return (ClientSettings.DimmedLineColor.G - ClientSettings.LineColor.G) / 3;
            }
        }
        private static int bLineColorDelta
        {
            get
            {
                return (ClientSettings.DimmedLineColor.B - ClientSettings.LineColor.B) / 3;
            }
        }

        private bool _isFading = true;
        private Color _animationLineColor = ClientSettings.LineColor;
        private Color AnimationLineColor
        {
            get
            {
                if (_animationStep > 0)
                {
                    Color newColor;
                    try
                    {
                        if (_isFading)
                        {
                            newColor = Color.FromArgb(_animationLineColor.R + rLineColorDelta,
                                                      _animationLineColor.G + gLineColorDelta,
                                                      _animationLineColor.B + bLineColorDelta);
                        }
                        else
                        {
                            newColor = Color.FromArgb(_animationLineColor.R - rLineColorDelta,
                                                      _animationLineColor.G - gLineColorDelta,
                                                      _animationLineColor.B - bLineColorDelta);
                        }
                    }
                    catch
                    {
                        newColor = ClientSettings.LineColor;
                    }
                    _animationLineColor = newColor;
                    return newColor;
                }
                return _animationLineColor;
            }
        }
        private Color _animationTextColor = ClientSettings.MenuTextColor;
        private Color AnimationTextColor
        {
            get
            {
                if (_animationStep > 0)
                {
                    Color newColor;
                    try
                    {
                        if (_isFading)
                        {
                            newColor = Color.FromArgb(_animationTextColor.R + rTextColorDelta,
                                                      _animationTextColor.G + gTextColorDelta,
                                                      _animationTextColor.B + bTextColorDelta);
                        }
                        else
                        {
                            newColor = Color.FromArgb(_animationTextColor.R - rTextColorDelta,
                                                      _animationTextColor.G - gTextColorDelta,
                                                      _animationTextColor.B - bTextColorDelta);
                        }
                    }
                    catch
                    {
                        newColor = ClientSettings.MenuTextColor;
                    }
                    _animationTextColor = newColor;
                    return newColor;
                }
                return _animationTextColor;
            }
        }
        
        private void DrawSubMenu(SideMenuItem item, Rectangle menuRect)
        {
            int i = 0;
            int offSet;
            if (_animationStep >= 0)
            {
                offSet = ClientSettings.TextSize - (_animationStep * _animationDelta);
            }
            else
            {
                offSet = ClientSettings.TextSize;
            }
            int itemsCount = item.SubMenuItems.Count;
            if (menuRect.Height > 0)
            {
                _topOfSubMenu = (((menuRect.Bottom - menuRect.Top) / 2) + menuRect.Top) - (itemsCount * ItemHeight / 2);
            }
            else
            {
                _topOfSubMenu = Height/2 - (itemsCount * ItemHeight / 2);
            }
            if (_topOfSubMenu < 0) { _topOfSubMenu = 0; }
            int backPedal = Height - (_topOfSubMenu + (item.SubMenuItems.Count * ItemHeight));
            if(backPedal<0)
            {
                _topOfSubMenu = _topOfSubMenu + backPedal;
            }
            int currentTop = _topOfSubMenu;
            int currentBottom = currentTop + ItemHeight;
            using (Graphics mBackBuffer = Graphics.FromImage(_rendered))
            {
                foreach (SideMenuItem subItem in item.SubMenuItems)
                {
                    
                    if (subItem.Visible)
                    {
                        DrawSingleItem(i, mBackBuffer, offSet, currentTop, subItem, ClientSettings.LineColor, ClientSettings.MenuTextColor);
                        i++;
                        currentBottom = currentTop + ItemHeight;
                        currentTop = currentTop + ItemHeight;
                        
                    }
                }
                using (var whitePen = new Pen(ClientSettings.LineColor))
                {
                    if (_side == KListControl.SideShown.Left)
                    {
                        mBackBuffer.DrawLine(whitePen, _width - offSet, _topOfSubMenu, _width - offSet, currentBottom);
                    }
                    else
                    {
                        mBackBuffer.DrawLine(whitePen, offSet, _topOfSubMenu, offSet, currentBottom);
                    }
                }
            }
        }

        public SideMenuItem GetMenuItemForTransposedPoint(Point x)
        {
            List<SideMenuItem> listOfItems = _items;
            int TopOfItem = TopOfMenu;
            int offset = 0;
            if (ExpandedItem != null)
            {
                listOfItems = ExpandedItem.SubMenuItems;
                TopOfItem = _topOfSubMenu;
                offset = ClientSettings.TextSize; 
            }
            

            foreach (SideMenuItem menuItem in listOfItems)
            {
                if (menuItem.Visible)
                {

                    Rectangle menuRect;
                    menuRect = _side == KListControl.SideShown.Left ? new Rectangle(0, TopOfItem, Width-offset, ItemHeight) : new Rectangle(offset, TopOfItem, Width, ItemHeight);
                    TopOfItem = TopOfItem + ItemHeight;
                    if (menuRect.Contains(x))
                    {
                        return menuItem;
                    }
                }
            }
            return null;
        }

        public SideMenuItem GetMenuItemForPoint(Point x, int leftOfItem)
        {
            int topOfItem = TopOfMenu;
            
            foreach (SideMenuItem menuItem in _items)
            {
                if (menuItem.Visible)
                {
                    var menuRect = new Rectangle(leftOfItem, topOfItem, Width, ItemHeight);
                    topOfItem = topOfItem + ItemHeight;
                    if (menuRect.Contains(x))
                    {
                        return menuItem;
                    }
                }
            }
            return null;
        }
    }
}