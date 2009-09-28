using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using PockeTwit.FingerUI.Menu;

namespace PockeTwit.FingerUI
{
    public class StatusItem : IDisposable, IComparable, IDisplayItem
    {

        public static char[] IgnoredAtChars = new[] { ':', ',', '-', '.', '!', '?', '~','=','&','*','>',')', '(' };

        private static readonly System.Text.RegularExpressions.Regex GetClickables =
            new System.Text.RegularExpressions.Regex(@"(http://([a-zA-Z0-9\~\!\@\#\$\%\^\&amp;\*\(\)_\-\=\+\\\/\?\.\:\;\'\,]*)?)|(@\w+)|(#\w+)(http://([a-zA-Z0-9\~\!\@\#\$\%\^\&amp;\*\(\)_\-\=\+\\\/\?\.\:\;\'\,]*)?)|(@\w+)|(#\w+)", 
                                                     System.Text.RegularExpressions.RegexOptions.Compiled | System.Text.RegularExpressions.RegexOptions.IgnoreCase);

        #region Fields (15) 

        private Graphics _parentGraphics;
        private Library.status _tweet;
        private Rectangle _currentOffset;
        private Rectangle _mBounds;
        private bool _hasFavoriteStar;
        private KListControl _mParent;
        private bool _mSelected;
        private readonly StringFormat _mStringFormat = new StringFormat();
        private int _mX = -1;
        private int _mY = -1;
        #endregion Fields 

        #region Constructors (2) 

        /// <summary>
        /// Initializes a new instance of the <see cref="KListItem"/> class.
        /// </summary>
        /// <param name="parent">The parent.</param>
        /// <param name="text">The text.</param>
        /// <param name="value">The value.</param>
        public StatusItem(KListControl parent, string text, object value)
        {
            _mParent = parent;
            Text = text;
            Value = value;
        }

        public StatusItem()
        {
        }

        #endregion Constructors 

        #region Properties (12) 

        public Rectangle CurrentOffset
        {
            get { return _currentOffset; }
        }


        public void ResetTexts()
        {
            Tweet.SplitLines = new List<string>();
            Tweet.Clickables = new List<Clickable>();
        }

        public bool Highlighted { get { return _hasFavoriteStar; } set { _hasFavoriteStar = value; } }

        /// <summary>
        /// Gets or sets the Y.
        /// </summary>
        /// <value>The Y.</value>
        public int Index { get { return _mY; } set { _mY = value; } }

        public bool IsFavorite
        {
            get 
            {
                if(string.IsNullOrEmpty(Tweet.favorited))
                {
                    return false;
                }
                return bool.Parse(Tweet.favorited);
            }
            set
            {
                Tweet.favorited = value.ToString();
                Highlighted = value;
            }
        }


        /// <summary>
        /// Gets or sets the text.
        /// </summary>
        /// <value>The text.</value>
        public string Text { get; set; }

        public Library.status Tweet 
        {
            get { return _tweet; }
            set
            {
                _tweet = value;
                _hasFavoriteStar = !string.IsNullOrEmpty(value.favorited) && bool.Parse(value.favorited);
                if (Tweet.Clickables == null)
                {
                    Tweet.Clickables = new List<Clickable>();
                }
                if (Tweet.SplitLines == null)
                {
                    Tweet.SplitLines = new List<string>();
                }
                
            }

        }

        /// <summary>
        /// Gets or sets the value.
        /// </summary>
        /// <value>The value.</value>
        public object Value { get; set; }

        /// <summary>
        /// Gets or sets the X.
        /// </summary>
        /// <value>The X.</value>
        public int XIndex { get { return _mX; } set { _mX = value; } }

        
        #endregion Properties 

        #region Delegates and Events (1) 


        // Delegates (1) 

        public delegate void ClickedWordDelegate(string textClicked);

        #endregion Delegates and Events 

        #region Methods (4) 


        // Public Methods (2) 

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public virtual void Dispose()
        {

            _mParent = null;
        }


        public void RenderAvatarArea(Graphics g, Rectangle bounds)
        {
            var imageLocation = new Point(bounds.X + ClientSettings.Margin, bounds.Y + ClientSettings.Margin);
            if (ClientSettings.ShowAvatars)
            {
                string artURL = Tweet.user.profile_image_url;
                if (!ClientSettings.HighQualityAvatars)
                {
                    artURL = Tweet.user.profile_image_url;
                }
                try
                {
                    using (Image userImage = ThrottledArtGrabber.GetArt(artURL))
                    {
                        //g.DrawImage(UserImage, ImageLocation.X, ImageLocation.Y,);
                        g.DrawImage(userImage, new Rectangle(imageLocation.X, imageLocation.Y, ClientSettings.SmallArtSize, ClientSettings.SmallArtSize), new Rectangle(0, 0, userImage.Width, userImage.Height), GraphicsUnit.Pixel);

                    }
                }
                catch(Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine(ex.Message);
                }

                //Only occasionally is an item "starred", but we draw one on there if it is.
                if (_hasFavoriteStar)
                {
                    var ia = new System.Drawing.Imaging.ImageAttributes();
                    ia.SetColorKey(ThrottledArtGrabber.FavoriteImage.GetPixel(0, 0), ThrottledArtGrabber.FavoriteImage.GetPixel(0, 0));
                    g.DrawImage(ThrottledArtGrabber.FavoriteImage,
                                new Rectangle(bounds.X + 5, bounds.Y + 5, 7, 7), 0, 0, 7, 7, GraphicsUnit.Pixel, ia);
                }

                //If it's a reply or direct message, overlay that on the avatar
                string overlay;

                if ((Tweet.TypeofMessage & Library.StatusTypes.Reply) != 0)
                    overlay = "@";
                else if ((Tweet.TypeofMessage & Library.StatusTypes.Direct) != 0)
                    overlay = "D";
                else
                    overlay = String.Empty;

                if (overlay.Length != 0)
                {
                    using (Brush sBrush = new SolidBrush(ClientSettings.SelectedForeColor))
                    {

                        var imageRect = new Rectangle(imageLocation.X, imageLocation.Y, ClientSettings.SmallArtSize, ClientSettings.SmallArtSize);
                        SizeF overlaySize = g.MeasureString(overlay, ClientSettings.SmallFont);
                        using (Brush bBrush = new SolidBrush(ClientSettings.SelectedBackColor))
                        {
                            g.FillRectangle(bBrush, new Rectangle(imageRect.Right - (int)overlaySize.Width - 2, imageRect.Top, (int)overlaySize.Width + 2, (int)overlaySize.Height + 2));
                        }
                        g.DrawString(overlay, ClientSettings.SmallFont, sBrush, new Rectangle(imageRect.Right - (int)overlaySize.Width + 1, imageRect.Top , (int)overlaySize.Width, (int)overlaySize.Height));
                    }
                }
            }
        }

        public override string ToString()
        {
            return Tweet.text;
        }

        // Private Methods (2) 

        //texbounds is the area we're allowed to draw within
        //lineOffset is how many lines we've already drawn in these bounds
        private void MakeClickable(Graphics g, Rectangle textBounds)
        {
            var lPen = new Pen(ClientSettings.LinkColor);
            var hPen = new Pen(ClientSettings.HashLinkColor);

            if(Selected)
            {
                lPen.Color = ClientSettings.SelectedLinkColor;
                hPen.Color = ClientSettings.SelectedHashLinkColor;
            }
            foreach (Clickable c in Tweet.Clickables)
            {
                Pen uPen = lPen;
                if (c.Text.StartsWith("#")) { uPen = hPen; }
                g.DrawLine(uPen, (int)c.Location.Left + textBounds.Left, (int)c.Location.Bottom + textBounds.Top,
                           (int)c.Location.Right + textBounds.Left, (int)c.Location.Bottom + textBounds.Top);
            }
            lPen.Dispose();
            hPen.Dispose();

        }


        #endregion Methods 

        #region Nested Classes (1) 


        [Serializable]
        public class Clickable
        {

            #region Fields (2) 

            public RectangleF Location;
            public string Text;
            public int Id;

            #endregion Fields 

            #region Methods (2) 


            // Public Methods (2) 

            public override bool Equals(object obj)
            {
                var otherClick = (Clickable)obj;
                if (otherClick.Location.Top == Location.Top &&
                    otherClick.Location.Left == Location.Left)
                {
                    return true;
                }
                return false;
            }

            public override int GetHashCode()
            {
                return base.GetHashCode();
            }


            #endregion Methods 

        }
        #endregion Nested Classes 


        #region Parsing Routines

        private void BreakUpTheText(Graphics g, Rectangle textBounds)
        {
            if (!ClientSettings.UseClickables) { FirstClickableRun(Tweet.DisplayText); return; }

            int lineOffset = 1;
            if (Tweet.SplitLines == null || Tweet.SplitLines.Count == 0)
            {
                Tweet.SplitLines = new List<string>();
                string textToDisplay = System.Web.HttpUtility.HtmlDecode(Tweet.DisplayText).Replace('\n', ' ');
                FirstClickableRun(Tweet.DisplayText);
                SizeF size = g.MeasureString(textToDisplay, ClientSettings.TextFont);
                while (size.Width > textBounds.Width - ClientSettings.Margin)
                {
                    {
                        int totalChars = textToDisplay.Length;
                        int estimatedChars = Math.Min((((textBounds.Width-ClientSettings.Margin) * totalChars) / (int)size.Width)+2, textToDisplay.Length-1);
                        bool spaceBreak = true;
                        string line = textToDisplay;
                        int endOfLine = estimatedChars;
                        while (size.Width > textBounds.Width - ClientSettings.Margin)
                        {
                            endOfLine = textToDisplay.LastIndexOf(' ', estimatedChars);
                            if (endOfLine < 0) { endOfLine = estimatedChars; spaceBreak = false; }
                            line = textToDisplay.Substring(0, endOfLine);
                            size = g.MeasureString(line, ClientSettings.TextFont);
                            estimatedChars = endOfLine-1;
                        }
                        Tweet.SplitLines.Add(line);
                        textToDisplay = spaceBreak ? textToDisplay.Substring(endOfLine + 1) : textToDisplay.Substring(endOfLine);
                        size = g.MeasureString(textToDisplay, ClientSettings.TextFont);
                        FindClickables(line, g, lineOffset - 1);
                        lineOffset++;
                    }
                }
                Tweet.SplitLines.Add(textToDisplay);
                FindClickables(textToDisplay, g, lineOffset - 1);
            }
            return;
        }

        

        private void FirstClickableRun(string text)
        {
            Tweet.Clickables = new List<Clickable>();
            Tweet.ClickablesToDo = new List<int>();
            int id = 0;
            System.Text.RegularExpressions.MatchCollection m = GetClickables.Matches(text);
            foreach (System.Text.RegularExpressions.Match match in m)
            {
                var c = new Clickable {Text = match.Value.Trim(IgnoredAtChars), Id = id};
                Tweet.ClickablesToDo.Add(id);
                Tweet.Clickables.Add(c);
                id++;
            }
        }

        private void FindClickables(string line, Graphics g, int lineOffSet)
        {

            //Still need to handle "wrapped" links
            if (!ClientSettings.UseClickables) { return; }
            if (Tweet.ClickablesToDo.Count == 0) { return; }
            float position = ((lineOffSet * (ClientSettings.TextSize)));
            Clickable wrappedClick = null;
            foreach (Clickable c in Tweet.Clickables)
            {
                if (!Tweet.ClickablesToDo.Contains(c.Id))
                    continue;

                int i = line.IndexOf(c.Text);
                float startpos = 0;
                if (i >= 0)
                {
                    if (i > 0)
                    {
                        string LineBeforeThisWord = line.Substring(0, i);
                        startpos = g.MeasureString(LineBeforeThisWord, ClientSettings.TextFont).Width;
                    }
                    SizeF wordSize = g.MeasureString(c.Text, ClientSettings.TextFont);
                    c.Location = new RectangleF(startpos, position, wordSize.Width, wordSize.Height);
                    Tweet.ClickablesToDo.Remove(c.Id);
                }
                else{
                    //Check to see if clickable got wrapped
                    string lastWord = line;
                    string lineBeforeThisWord = "";
                    bool containsSpace = false;
                    if (line.IndexOf(" ") > 0)
                    {
                        lastWord = line.Substring(line.LastIndexOf(" "));
                        containsSpace = true;
                    }

                    if (c.Text.StartsWith(lastWord))
                    {
                        if (containsSpace)
                            lineBeforeThisWord = line.Substring(0, line.LastIndexOf(" "));
                        startpos = g.MeasureString(lineBeforeThisWord, ClientSettings.TextFont).Width;
                        SizeF wordSize = g.MeasureString(lastWord, ClientSettings.TextFont);
                        c.Location = new RectangleF(startpos, position, wordSize.Width, wordSize.Height);
                        //Find the rest of the word on the next line
                        if (lastWord.Length < c.Text.Length)
                        {
                            string secondPart = c.Text.Substring(lastWord.Length);
                            var wrapClick = new Clickable {Text = c.Text};
                            //Find the size of the word
                            wordSize = g.MeasureString(secondPart, ClientSettings.TextFont);
                            //A structure containing info we need to know about the word.
                            float nextPosition = (((lineOffSet + 1) * (ClientSettings.TextSize)));
                            wrapClick.Location = new RectangleF(0F, nextPosition, wordSize.Width, wordSize.Height);
                            wrappedClick = wrapClick;
                        }
                        Tweet.ClickablesToDo.Remove(c.Id);
                    }
                }
            }
            if (wrappedClick != null)
            {
                Tweet.Clickables.Add(wrappedClick);
            }
        }
        #endregion

        #region IComparable Members

        public int CompareTo(object obj)
        {
            var otherItem = (StatusItem)obj;
            return otherItem.Tweet.CompareTo(Tweet);
        }

        #endregion

        #region IDisplayItem Members 
        public void OnMouseClick(Point p)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Renders to the specified graphics.
        /// </summary>
        /// <param name="g">The graphics.</param>
        /// <param name="bounds">The bounds.</param>
        public virtual void Render(Graphics g, Rectangle bounds)
        {
            try
            {
                g.Clip = new Region(bounds);
                _currentOffset = bounds;
                var foreBrush = new SolidBrush(ClientSettings.ForeColor);
                Rectangle textBounds;
                //Shrink the text area to accomidate avatars if appropriate
                if (ClientSettings.ShowAvatars)
                {
                    textBounds = new Rectangle(bounds.X + (ClientSettings.SmallArtSize + ClientSettings.Margin), bounds.Y, bounds.Width - (ClientSettings.SmallArtSize + (ClientSettings.Margin * 2)), bounds.Height);
                }
                else
                {
                    textBounds = new Rectangle(bounds.X + ClientSettings.Margin, bounds.Y, bounds.Width - (ClientSettings.Margin * 2), bounds.Height);
                }

                var innerBounds = new Rectangle(bounds.Left, bounds.Top, bounds.Width, bounds.Height);
                innerBounds.Offset(1, 1);
                innerBounds.Width--; innerBounds.Height--;
                DisplayItemDrawingHelper.DrawItemBackground(g, bounds, Selected);
                //Add the timestamp if the settings call for it.

                if (ClientSettings.ShowExtra)
                {
                    Color smallColor = ClientSettings.SmallTextColor;
                    if (Selected) { smallColor = ClientSettings.SelectedSmallTextColor; }
                    using (Brush dateBrush = new SolidBrush(smallColor))
                    {
                        g.DrawString(Tweet.TimeStamp, ClientSettings.SmallFont, dateBrush, bounds.Left + ClientSettings.Margin, ClientSettings.SmallArtSize + ClientSettings.Margin + bounds.Top, _mStringFormat);
                    }
                }

                //Get and draw the avatar area.
                RenderAvatarArea(g, bounds);


                textBounds.Offset(ClientSettings.Margin, 1);
                textBounds.Height--;

                BreakUpTheText(g, textBounds);
                int lineOffset = 0;

                if (!ClientSettings.UseClickables)
                {
                    g.DrawString(Tweet.DisplayText, ClientSettings.TextFont, foreBrush, new RectangleF(textBounds.Left, textBounds.Top, textBounds.Width, textBounds.Height));
                    //g.DrawString(Tweet.DisplayText, TextFont, ForeBrush, textBounds.Left, textBounds.Top, _mStringFormat);
                }
                else
                {

                    for (int i = 0; i < Tweet.SplitLines.Count; i++)
                    {
                        if (i >= ClientSettings.LinesOfText)
                        {
                            break;
                        }
                        float position = ((lineOffset * (ClientSettings.TextSize)) + textBounds.Top);

                        g.DrawString(Tweet.SplitLines[i], ClientSettings.TextFont, foreBrush, textBounds.Left, position, _mStringFormat);
                        lineOffset++;
                    }
                    MakeClickable(g, textBounds);
                    foreBrush.Dispose();
                }
                g.Clip = new Region();
                Tweet.SplitLines = null;
            }
            catch (ObjectDisposedException)
            {
            }
        }

        /// <summary>
        /// Gets or sets the parent.
        /// </summary>
        /// <value>The parent.</value>
        public KListControl Parent
        {
            get { return _mParent; }
            set
            {
                _mParent = value;
            }
        }

        public Graphics ParentGraphics
        {
            set
            {
                _parentGraphics = value;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="KListItem"/> is selected.
        /// </summary>
        /// <value><c>true</c> if selected; otherwise, <c>false</c>.</value>
        public bool Selected
        {
            get
            {
                return _mSelected;
            }
            set
            {
                _mSelected = value;
            }
        }

        /// <summary>
        /// The unscrolled bounds for this item.
        /// </summary>
        public Rectangle Bounds
        {
            get { return _mBounds; }
            set
            {
                if (_mBounds.Width != 0 && value.Width != _mBounds.Width)
                {
                    ResetTexts();
                }
                _mBounds = value;
                Rectangle textBounds;
                if (ClientSettings.ShowAvatars)
                {
                    textBounds = new Rectangle(ClientSettings.SmallArtSize + ClientSettings.Margin, 0, _mBounds.Width - (ClientSettings.SmallArtSize + (ClientSettings.Margin * 2)), _mBounds.Height);
                }
                else
                {
                    textBounds = new Rectangle(ClientSettings.Margin, 0, _mBounds.Width - (ClientSettings.Margin * 2), _mBounds.Height);
                }
                BreakUpTheText(_parentGraphics, textBounds);
            }
        }

        public void CreateRightMenu(SideMenu menu)
        {
            throw new NotImplementedException();
        }

        public void UpdateRightMenu(SideMenu menu)
        {
            throw new NotImplementedException();
        }

        public void OnMouseDblClick()
        {
            throw new NotImplementedException();
        }

        #endregion


    }
}