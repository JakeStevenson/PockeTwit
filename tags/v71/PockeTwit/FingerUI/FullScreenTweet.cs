using System;

using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;
using System.Text.RegularExpressions;

namespace FingerUI
{
    public partial class FullScreenTweet : UserControl
    {
        const string HTML_TAG_PATTERN = "<.*?>";

        public PockeTwit.Library.status Status;
        private bool _Visible = false;
        public new bool Visible
        {
            get
            {
                return _Visible;
            }
            set
            {
                _Visible = value;
                base.Visible = value;
                if (_Visible)
                {
                    PockeTwit.ThrottledArtGrabber.NewArtWasDownloaded += new PockeTwit.ThrottledArtGrabber.ArtIsReady(ThrottledArtGrabber_NewArtWasDownloaded);
                }
                else
                {
                    if (avatarBox.Image != null)
                    {
                        avatarBox.Image.Dispose();
                    }
                    PockeTwit.ThrottledArtGrabber.NewArtWasDownloaded -= new PockeTwit.ThrottledArtGrabber.ArtIsReady(ThrottledArtGrabber_NewArtWasDownloaded);
                }
            }
        }
        public FullScreenTweet()
        {
            InitializeComponent();
            if (PockeTwit.DetectDevice.DeviceType == PockeTwit.DeviceType.Standard)
            {
                lnkDismiss.Visible = false;
            }
            _FontSize = lblText.Font.Size;
            PockeTwit.Themes.FormColors.SetColors(this);
            avatarBox.Width = ClientSettings.SmallArtSize;
            avatarBox.Height = ClientSettings.SmallArtSize;
        }

        public void ResetRendering()
        {
            PockeTwit.Themes.FormColors.SetColors(this);
        }

        private delegate void delUpdateArt(string Argument);
        void ThrottledArtGrabber_NewArtWasDownloaded(string Argument)
        {
            
            if (Status != null)
            {
                if (Argument == Status.user.profile_image_url)
                {
                    if (InvokeRequired)
                    {
                        delUpdateArt d = new delUpdateArt(ThrottledArtGrabber_NewArtWasDownloaded);
                        this.Invoke(d, Argument);
                    }
                    else
                    {
                        avatarBox.Image = PockeTwit.ThrottledArtGrabber.GetArt(Status.user.profile_image_url);
                    }
                }
            }
        }

        private float _FontSize;
        public float FontSize
        {
            get { return _FontSize; }
            set
            {
                if (value > 5)
                {
                    _FontSize = value;
                    using (Font TextFont = new Font(FontFamily.GenericSansSerif, value, FontStyle.Regular))
                    {
                        lblText.Font = TextFont;
                    }
                }
            }
        }
        public void Render()
        {
            if (Status != null)
            {
                Cursor.Current = Cursors.WaitCursor;
                avatarBox.Image = PockeTwit.ThrottledArtGrabber.GetArt(Status.user.profile_image_url);
                lblUserName.Text = Status.user.screen_name;
                lblTime.Text = Status.TimeStamp.ToString();
                if(string.IsNullOrEmpty(Status.source))
                {
                    lblSource.Text = "";
                }
                else
                {
                    
                    lblSource.Text = "from "+StripHTML(Status.source);
                }
                string fullText;
                if (Yedda.ShortText.isShortTextURL(Status.text))
                {
                    string[] splitup = Status.text.Split(new char[] { ' ' });
                    fullText = Yedda.ShortText.getFullText(splitup[splitup.Length - 1]);
                }
                else
                {
                    fullText = Status.text;
                }
                if (ClientSettings.AutoTranslate)
                {
                    fullText = Yedda.GoogleTranslate.GetTranslation(fullText);
                }
                lblText.Text = System.Web.HttpUtility.HtmlDecode(fullText).Replace("&", "&&");
                Cursor.Current = Cursors.Default;
            }
        }

        private void lnkDismiss_Click(object sender, EventArgs e)
        {
            this.Visible = false;
        }
        static string StripHTML(string inputString)
        {
            if (string.IsNullOrEmpty(inputString)) { return null; }
            return Regex.Replace
              (inputString, HTML_TAG_PATTERN, string.Empty);
        }
    }
}
