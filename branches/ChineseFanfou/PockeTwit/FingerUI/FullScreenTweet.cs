using System;
using System.Drawing;
using System.Windows.Forms;
using System.Text.RegularExpressions;
using PockeTwit.OtherServices;
using PockeTwit.OtherServices.TextShrinkers;

namespace FingerUI
{
    public partial class FullScreenTweet : UserControl
    {
        const string HTMLTagPattern = "<.*?>";

        public PockeTwit.Library.status Status;
        private bool _visible;
        public new bool Visible
        {
            get
            {
                return _visible;
            }
            set
            {
                _visible = value;
                base.Visible = value;
                if (_visible)
                {
                    PockeTwit.ThrottledArtGrabber.NewArtWasDownloaded += ThrottledArtGrabberNewArtWasDownloaded;
                }
                else
                {
                    PockeTwit.ThrottledArtGrabber.NewArtWasDownloaded -= ThrottledArtGrabberNewArtWasDownloaded;
                    if (avatarBox.Image != null)
                    {
                        avatarBox.Image.Dispose();
                    }
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
            _fontSize = lblText.Font.Size;
            PockeTwit.Themes.FormColors.SetColors(this);
            PockeTwit.Localization.XmlBasedResourceManager.LocalizeForm(this);
            avatarBox.Width = ClientSettings.SmallArtSize;
            avatarBox.Height = ClientSettings.SmallArtSize;
        }

        public void ResetRendering()
        {
            PockeTwit.Themes.FormColors.SetColors(this);
            PockeTwit.Localization.XmlBasedResourceManager.LocalizeForm(this);
        }

        private delegate void DelUpdateArt(string argument);
        void ThrottledArtGrabberNewArtWasDownloaded(string argument)
        {
            
            if (Status != null)
            {
                if (argument == Status.user.profile_image_url)
                {
                    if (InvokeRequired)
                    {
                        var d = new DelUpdateArt(ThrottledArtGrabberNewArtWasDownloaded);
                        Invoke(d, argument);
                    }
                    else
                    {
                        try
                        {
                            avatarBox.Image = PockeTwit.ThrottledArtGrabber.GetArt(Status.user.profile_image_url);
                        }
                        catch(ObjectDisposedException){}
                    }
                }
            }
        }

        private float _fontSize;
        public float FontSize
        {
            get { return _fontSize; }
            set
            {
                if (value > 5)
                {
                    _fontSize = value;
                    using (var textFont = new Font(FontFamily.GenericSansSerif, value, FontStyle.Regular))
                    {
                        lblText.Font = textFont;
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
                lblTime.Text = Status.TimeStamp;
                if(string.IsNullOrEmpty(Status.source))
                {
                    lblSource.Text = "";
                }
                else
                {

                    lblSource.Text = "from " + StripHTML(System.Web.HttpUtility.HtmlDecode(Status.source));
                }
                string fullText;
                if (ShortText.IsShortTextURL(Status.text))
                {
                    string[] splitup = Status.text.Split(new[] { ' ' });
                    fullText = ShortText.GetFullText(splitup[splitup.Length - 1]);
                }
                else
                {
                    fullText = Status.text;
                }
                if (ClientSettings.AutoTranslate)
                {
                    fullText = GoogleTranslate.GetTranslation(fullText);
                }
                lblText.Text = System.Web.HttpUtility.HtmlDecode(fullText).Replace("&", "&&");
                Cursor.Current = Cursors.Default;
            }
        }

        private void lnkDismiss_Click(object sender, EventArgs e)
        {
            Visible = false;
        }
        static string StripHTML(string inputString)
        {
            if (string.IsNullOrEmpty(inputString)) { return null; }
            return Regex.Replace
              (inputString, HTMLTagPattern, string.Empty);
        }
    }
}
