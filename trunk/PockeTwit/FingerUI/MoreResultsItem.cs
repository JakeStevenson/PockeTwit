using System;

using System.Collections.Generic;
using System.Text;
using System.Drawing;
using PockeTwit.FingerUI.Menu;
using System.Windows.Forms;

namespace PockeTwit.FingerUI
{
    class MoreResultsItem : IDisplayItem
    {
        TweetList _list;
        string _searchString;
        bool _saveResults;

        public MoreResultsItem(TweetList list, string searchString, bool saveResults)
        {
            _list = list;
            _searchString = searchString;
            _saveResults = saveResults;
            Value = this.GetType().ToString();

        }
        ISpecialTimeLine _timeLine;
        public MoreResultsItem(TweetList list, ISpecialTimeLine timeLine)
        {
            _list = list;
            _timeLine = timeLine;
            Value = this.GetType().ToString();
        }

        #region IDisplayItem Members

        Graphics _parentGraphics;
        public System.Drawing.Graphics ParentGraphics
        {
            set 
            {
                _parentGraphics = value;
            }
        }

        public KListControl Parent { get; set; }
        public int Index { get; set; }

        public void OnMouseClick(Point p)
        {
            OnMouseDblClick();
        }

        public void OnMouseDblClick()
        {
            if (_timeLine == null) // direct Search
                _list.ShowSearchResults(_searchString, _saveResults, Yedda.Twitter.PagingMode.Forward);
            else
                _list.ShowSpecialTimeLine(_timeLine, Yedda.Twitter.PagingMode.Forward);
        }


        public void Render(System.Drawing.Graphics g, System.Drawing.Rectangle bounds)
        {
            try
            {
                g.Clip = new Region(bounds);
          
                Rectangle textBounds;
                textBounds = new Rectangle(bounds.X + ClientSettings.Margin, bounds.Y, bounds.Width - (ClientSettings.Margin * 2), bounds.Height);

                DisplayItemDrawingHelper.DrawItemBackground(g, bounds, Selected);

                SizeF textSize = g.MeasureString("more", ClientSettings.MenuFont);
                Point startPoint = new Point((int)(bounds.Left + (bounds.Width - textSize.Width) / 2),(int)(bounds.Top + (bounds.Height - textSize.Height) / 2));
                
                Color drawColor = ClientSettings.MenuTextColor;
                using (Brush drawBrush = new SolidBrush(drawColor))
                {
                    g.DrawString("more", ClientSettings.MenuFont, drawBrush, startPoint.X, startPoint.Y);
                }
            }
            catch (ObjectDisposedException)
            {
            }
        }

        public System.Drawing.Rectangle Bounds
        {
            get;
            set;
        }

        public bool Selected
        {
            get;
            set;
        }
        /*
        private void Test()
        {
            MessageBox.Show("Works!");
        }
        */

        public void CreateRightMenu(SideMenu menu)
        {
            /* Test Code for menu creation, left here as template
            FingerUI.Menu.SideMenuItem TestMenuItem = new FingerUI.Menu.SideMenuItem(Test, "Test", menu);
            menu.ResetMenu(new FingerUI.Menu.SideMenuItem[] { TestMenuItem });*/
            return;
        }

        public void UpdateRightMenu(SideMenu menu)
        {
            return;
        }

        public object Value { get; set; }

        #endregion
    }
}
