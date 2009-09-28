using System;

using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using PockeTwit.FingerUI;

namespace FingerUI
{
    public class ClickablesMenu
    {
        
        #region Fields (2)

        private int _CurrentlyFocused = 0;
        private List<string> TextItems = new List<string>(new string[] { PockeTwit.Localization.XmlBasedResourceManager.GetString("Close Menu") });

        #endregion Fields

        #region Constructors (1)

        public ClickablesMenu()
        {

        }

        #endregion Constructors

        #region Properties (7)

        public int Height { get; set; }

        public List<StatusItem.Clickable> Items
        {
            set
            {
                TextItems = new List<string>();
                TextItems.Add(PockeTwit.Localization.XmlBasedResourceManager.GetString("Detailed View"));
                foreach (StatusItem.Clickable c in value)
                {
                    if (!TextItems.Contains(c.Text))
                    {
                        TextItems.Add(c.Text);
                    }
                }
                TextItems.Add(PockeTwit.Localization.XmlBasedResourceManager.GetString("Close Menu"));
            }
        }

        public int Left { get; set; }


        public int Top { get; set; }

        public bool Visible { get; set; }

        public int Width { get; set; }

        #endregion Properties

        #region Delegates and Events
        public delegate void delNoArgs();
        public event delNoArgs NeedRedraw = delegate { };
        public event delNoArgs Dismissed = delegate { };
        public event StatusItem.ClickedWordDelegate WordClicked = delegate { };
        

        #endregion Delegates and Events

        #region Methods

        public void SelectAtPoint(Point p)
        {
            try
            {
                int ItemHeight = (ClientSettings.TextSize * 2);
                int TopOfItem = ((this.Height / 2) - ((TextItems.Count * ItemHeight) / 2));
                int i = 0;
                foreach (string Item in TextItems)
                {
                    Rectangle r = new Rectangle(this.Left, TopOfItem, this.Width, ItemHeight);
                    if (r.Contains(p))
                    {
                        _CurrentlyFocused = i;
                        NeedRedraw();
                    }
                    TopOfItem = TopOfItem + ItemHeight;
                    i++;
                }
            }
            catch (KeyNotFoundException)
            {
            }
        }

        public bool CheckForClicks(Point p)
        {
            try
            {
                int ItemHeight = (ClientSettings.TextSize * 2);
                int TopOfItem = ((this.Height / 2) - ((TextItems.Count * ItemHeight) / 2));
                foreach (string Item in TextItems)
                {
                    Rectangle r = new Rectangle(this.Left, TopOfItem, this.Width, ItemHeight);
                    if (r.Contains(p))
                    {
                        if (TextItems[_CurrentlyFocused] != PockeTwit.Localization.XmlBasedResourceManager.GetString("Close Menu"))
                        {
                            WordClicked(Item);
                        }
                        Dismissed();
                        _CurrentlyFocused = 0;
                        return true;
                    }
                    TopOfItem = TopOfItem + ItemHeight;
                }
            }
            catch (KeyNotFoundException)
            {
            }
            return false;
        }

        public void KeyDown(KeyEventArgs e)
        {
            lock (TextItems)
            {
                if (e.KeyCode == Keys.Down)
                {
                    if (_CurrentlyFocused < TextItems.Count - 1)
                    {
                        _CurrentlyFocused++;
                    }
                    else
                    {
                        _CurrentlyFocused = 0;
                    }
                }
                if (e.KeyCode == Keys.Up)
                {
                    if (_CurrentlyFocused > 0)
                    {
                        _CurrentlyFocused--;
                    }
                    else
                    {
                        _CurrentlyFocused = TextItems.Count - 1;
                    }
                }
                if (e.KeyCode == Keys.Enter)
                {
                    if (TextItems[_CurrentlyFocused] != PockeTwit.Localization.XmlBasedResourceManager.GetString("Close Menu"))
                    {
                        if (WordClicked != null)
                        {
                            WordClicked(TextItems[_CurrentlyFocused]);
                        }
                    }
                    _CurrentlyFocused = 0;
                    Dismissed();
                }
            }
        }

        public void Render(Graphics g)
        {
            int ItemHeight = (ClientSettings.TextSize * 2);
            int TopOfItem = ((this.Height / 2) - ((TextItems.Count * ItemHeight) / 2));

            Region originalClip = g.Clip;
            Rectangle DrawingArea = new Rectangle(this.Left, TopOfItem, this.Width+1, (ItemHeight * TextItems.Count)+1);
            g.Clip = new Region(DrawingArea);

            int i = 0;
            using (Pen whitePen = new Pen(ClientSettings.LineColor))
            {
                foreach (string Item in TextItems)
                {
                    Rectangle r = new Rectangle(this.Left, TopOfItem, this.Width, ItemHeight);
                    int TextTop = ((r.Bottom - r.Top) / 2) + r.Top;
                    Color TextColor = ClientSettings.MenuTextColor;
                    if (i == _CurrentlyFocused)
                    {
                        Gradient.GradientFill.Fill(g, r, ClientSettings.SelectedBackColor, ClientSettings.SelectedBackGradColor, Gradient.GradientFill.FillDirection.TopToBottom);
                        TextColor = ClientSettings.MenuSelectedTextColor;
                    }
                    else
                    {
                        using (Brush b = new SolidBrush(ClientSettings.BackColor))
                        {
                            g.FillRectangle(b, r);
                        }
                    }
                    g.DrawRectangle(whitePen, r);
                    StringFormat sFormat = new StringFormat();
                    sFormat.LineAlignment = StringAlignment.Center;
                    
                    using (Brush c = new SolidBrush(TextColor))
                    {
                        g.DrawString(Item, new Font(FontFamily.GenericSansSerif, 9, FontStyle.Bold), c, r.Left + 4, TextTop, sFormat);
                    }
                    TopOfItem = TopOfItem + ItemHeight;
                    i++;
                }
            }
            
            g.Clip = originalClip;
        }

        #endregion Methods

    }
}
