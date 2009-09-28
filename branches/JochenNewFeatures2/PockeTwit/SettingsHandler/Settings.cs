using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using System.Drawing;
public static class ClientSettings
{
    [System.Runtime.InteropServices.DllImport("coredll.dll")]
    private static extern int SHCreateShortcut(StringBuilder szShortcut, StringBuilder szTarget);

    #region DefaultColors
    public static System.Drawing.Color BackColor = System.Drawing.Color.Black;
    public static System.Drawing.Color BackGradColor = System.Drawing.Color.Black;
    public static System.Drawing.Color ErrorColor = System.Drawing.Color.Red;
    public static System.Drawing.Color ForeColor = System.Drawing.Color.LightGray;
    public static System.Drawing.Color LineColor = ForeColor;
    public static System.Drawing.Color SmallTextColor = System.Drawing.Color.Gray;
    public static System.Drawing.Color SelectedSmallTextColor = System.Drawing.Color.Gray;
    public static System.Drawing.Color SelectedBackColor = System.Drawing.Color.Gray;
    public static System.Drawing.Color SelectedBackGradColor = System.Drawing.Color.Black;
    public static System.Drawing.Color SelectedForeColor = System.Drawing.Color.White;
    public static System.Drawing.Color FieldBackColor = System.Drawing.Color.White;
    public static System.Drawing.Color FieldForeColor = System.Drawing.Color.Black;
    public static System.Drawing.Color LinkColor = System.Drawing.Color.LightBlue;
    public static System.Drawing.Color HashLinkColor = System.Drawing.Color.Sienna;
    public static System.Drawing.Color DimmedColor = Color.FromArgb(75,75,75);
    public static System.Drawing.Color DimmedLineColor = Color.FromArgb(75, 75, 75);
    public static System.Drawing.Color SelectedLinkColor = LinkColor;
    public static System.Drawing.Color SelectedHashLinkColor = HashLinkColor;
    public static System.Drawing.Color PointerColor = ForeColor;
    public static System.Drawing.Color MenuTextColor = ForeColor;
    public static System.Drawing.Color MenuSelectedTextColor = ForeColor;
    public static System.Drawing.Color PopUpBackgroundColor = BackColor;
    public static System.Drawing.Color PopUpTextColor = ForeColor;
    #endregion

    #region Fields (13)

    private static int _TextSize = 0;
    public static int AnimationInterval;
    public static string AppPath = System.IO.Path.GetDirectoryName(Assembly.GetExecutingAssembly().GetName().CodeBase);
   
    public static string PingApi = "07fcca78e725fa4d3b27ea552ef06b3b";


    public static int Margin = 5;
    public static int MaxTweets = 50;
    public static Font MenuFont;
    public static Font SmallFont;
    public static Font TextFont;
    public static int SmallArtSize = 65;
    private static int _ItemHeight = -1;
    public static int ItemHeight
    {
        get
        {
            if (_ItemHeight == -1)
            {
                _ItemHeight = (ClientSettings.TextSize * ClientSettings.LinesOfText) + 5;
            }
            return _ItemHeight;
        }
    }

    public static Yedda.Twitter.Account GetAcountForUser(string userScreenName)
    {
        foreach (Yedda.Twitter.Account a in AccountsList)
        {
            if (a.UserName.ToLower()==userScreenName.ToLower()) { return a; }
        }
        return null;
    }

    public static Yedda.Twitter.Account DefaultAccount
    {
        get
        {
            foreach (Yedda.Twitter.Account a in AccountsList)
            {
                if (a.IsDefault) { return a; }
            }
            if (AccountsList.Count > 0)
            {
                return AccountsList[0];
            }
            return null;
        }
        set
        {
            lock (AccountsList)
            {
                foreach (Yedda.Twitter.Account a in AccountsList)
                {
                    a.IsDefault = a == value;
                }
            }
        }
    }

    private static int _LinesOfText = -1;
    public static int LinesOfText
    {
        get
        {
            if (_LinesOfText == -1)
            {
                if (ShowExtra)
                {
                    _LinesOfText = 6;
                }
                else
                {
                    _LinesOfText = 5;
                }
            }
            return _LinesOfText;
        }
    }
		#endregion Fields 

    #region Constructors (1) 

    static ClientSettings()
    {
        LoadSettings();
        LoadColors();
    }

    #endregion Constructors 

    #region Properties (7) 
    public static bool AutoScrollToTop { get; set; }
    public static bool RunOnStartUp
    {
        get
        {
            return System.IO.File.Exists(@"\Windows\StartUp\PockeTwit.lnk");
        }
        set
        {
            if (value)
            {
                StringBuilder shortcut = new StringBuilder(@"\Windows\StartUp\PockeTwit.lnk");
                StringBuilder target;
                if (ClientSettings.AppPath.IndexOf(' ') > 0)
                {
                    target = new StringBuilder("\"" + ClientSettings.AppPath + "\\PockeTwit.exe" + "\"" + " /BackGround");
                }
                else
                {
                    target = new StringBuilder(ClientSettings.AppPath + "\\PockeTwit.exe /BackGround");
                }

                SHCreateShortcut(shortcut, target);
            }
            else
            {
                if (RunOnStartUp)
                {
                    System.IO.File.Delete(@"\Windows\StartUp\PockeTwit.lnk");
                }
            }
        }
    }
    private static int _FontSize = 9;
    public static int FontSize
    {
        get
        {
            return _FontSize;
        }
        set
        {
            _FontSize = value;
            GetTextSizes();
        }
    }
    public static bool UseDIB { get; set; }
    private static string _CacheDir;
    public static string CacheDir
    {
        get
        {
            return _CacheDir;
        }
        set
        {
            if(string.IsNullOrEmpty(value))
            {
                value = AppPath;
            }
            if(!string.IsNullOrEmpty(_CacheDir) && value!=_CacheDir)
            {
                PockeTwit.LocalStorage.DataBaseUtility.MoveDB(value);
            }
            _CacheDir = value;
        }
    }
    public static string SelectedMediaService { get; set; }
    public static string PreviousMediaService { get; set; }
    public static bool SendMessageToMediaService { get; set; }
    public static Queue<string> SearchItems { get; set; }
    public static bool AutoTranslate { get; set; }
    public static string TranslationLanguage { get; set; }
    public static int PortalSize { get; set; }
    public static int UpdateMinutes { get; set; }
    public static float TextHeight { get; set; }
    public static string ThemeName { get; set; }
    public static bool IncludeUserName { get; set; }
    public static bool HighQualityAvatars { get; set; }
    public static bool UseClickables { get; set; }
    public static bool UseSkweezer { get; set; }
    public static string ProxyServer { get; set; }
    public static int ProxyPort { get; set; }
    public static bool ShowAvatars { get; set; }
    public static bool UseGPS { get; set; }
    public static bool IsMaximized { get; set; }
    public static bool CheckVersion { get; set; }
    public static string DistancePreference { get; set; }
    public static bool _ShowExtra = true;
    public static bool ShowExtra
    {
        get { return _ShowExtra; }
        set 
        { 
            _ShowExtra = value;
            _LinesOfText = -1;
            _ItemHeight = -1;
        }
    }
    public static int SmallTextSize{ get; set; }
    public static int TextSize 
    {
        get
        {
            return _TextSize;
        }
        set
        {
            _TextSize = value;
            SmallArtSize = _TextSize * 5;
            _ItemHeight = -1;
        }
    }
    public static bool ZoomPreview { get; set; }
    public static bool AutoCompleteAddressBook { get; set; }
    public static List<Yedda.Twitter.Account> AccountsList { get; set; }
	#endregion Properties 

    #region Methods (4) 
    public static void LoadSettings()
    {
        ConfigurationSettings.LoadConfig();
        IFormatProvider format = new System.Globalization.CultureInfo(1033);
        AccountsList = new List<Yedda.Twitter.Account>();
        Yedda.Twitter.Account LegacySettingsAccount = new Yedda.Twitter.Account();
        try
        {
            if (!string.IsNullOrEmpty(ConfigurationSettings.AppSettings["AutoCompleteAddressBook"]))
            {
                AutoCompleteAddressBook = Boolean.Parse(ConfigurationSettings.AppSettings["AutoCompleteAddressBook"]);
            }
            else
            {
                AutoCompleteAddressBook = true;
            }
            if (!string.IsNullOrEmpty(ConfigurationSettings.AppSettings["UseDIB"]))
            {
                UseDIB = Boolean.Parse(ConfigurationSettings.AppSettings["UseDIB"]);
            }
            else
            {
                UseDIB = true;
            }
            if (!string.IsNullOrEmpty(ConfigurationSettings.AppSettings["ZoomPreview"]))
            {
                ZoomPreview = Boolean.Parse(ConfigurationSettings.AppSettings["ZoomPreview"]);
            }
            else
            {
                ZoomPreview = true;
            }
            if (!string.IsNullOrEmpty(ConfigurationSettings.AppSettings["AutoScrollToTop"]))
            {
                AutoScrollToTop = Boolean.Parse(ConfigurationSettings.AppSettings["AutoScrollToTop"]);
            }
            else
            {
                AutoScrollToTop = false;
            }
            if (!string.IsNullOrEmpty(ConfigurationSettings.AppSettings["CacheDir"]))
            {
                CacheDir = ConfigurationSettings.AppSettings["CacheDir"];
            }
            else
            {
                CacheDir = AppPath;
            }

            if (!string.IsNullOrEmpty(ConfigurationSettings.AppSettings["MediaService"]))
            {
                PreviousMediaService = ConfigurationSettings.AppSettings["MediaService"];
            }
            if (!string.IsNullOrEmpty(ConfigurationSettings.AppSettings["SelectedMediaService"]))
            {
                SelectedMediaService = ConfigurationSettings.AppSettings["SelectedMediaService"];
            }
            else
            {
                SelectedMediaService = "TweetPhoto";
            }


            if (!string.IsNullOrEmpty(ConfigurationSettings.AppSettings["SendMessageToMediaService"]))
            {
                SendMessageToMediaService = bool.Parse(ConfigurationSettings.AppSettings["SendMessageToMediaService"]);
            }
            else
            {
                SendMessageToMediaService = true;
            }

            if (!string.IsNullOrEmpty(ConfigurationSettings.AppSettings["FontSize"]))
            {
                FontSize = int.Parse(ConfigurationSettings.AppSettings["FontSize"]);
            }
            else
            {
                FontSize = 9;
            }

            if (!string.IsNullOrEmpty(ConfigurationSettings.AppSettings["SearchItems"]))
            {
                SearchItems = new Queue<string>(ConfigurationSettings.AppSettings["SearchItems"].Split('|'));
            }
            else
            {
                SearchItems = new Queue<string>();
            }
            if (!string.IsNullOrEmpty(ConfigurationSettings.AppSettings["AutoTranslate"]))
            {
                AutoTranslate = bool.Parse(ConfigurationSettings.AppSettings["AutoTranslate"]);
            }
            else
            {
                AutoTranslate = false;
            }
            if (!string.IsNullOrEmpty(ConfigurationSettings.AppSettings["UILanguage"]))
            {
                PockeTwit.Localization.XmlBasedResourceManager.CultureInfo = new System.Globalization.CultureInfo(ConfigurationSettings.AppSettings["UILanguage"]);
            }
            else
            {
                PockeTwit.Localization.XmlBasedResourceManager.CultureInfo = System.Globalization.CultureInfo.CurrentUICulture;
            }
            if (!string.IsNullOrEmpty(ConfigurationSettings.AppSettings["TranslationLanguage"]))
            {
                TranslationLanguage = ConfigurationSettings.AppSettings["TranslationLanguage"];
            }
            else
            {
                TranslationLanguage = System.Globalization.CultureInfo.CurrentUICulture.TwoLetterISOLanguageName.ToLower();
            }
            if (!string.IsNullOrEmpty(ConfigurationSettings.AppSettings["UpdateMinutes"]))
            {
                UpdateMinutes = int.Parse(ConfigurationSettings.AppSettings["UpdateMinutes"], format);
            }
            else
            {
                UpdateMinutes = 5;
            }
            if (!string.IsNullOrEmpty(ConfigurationSettings.AppSettings["ThemeName"]))
            {
                ThemeName = ConfigurationSettings.AppSettings["ThemeName"];
            }
            else
            {
                ThemeName = "Sunny";
            }
            if (!string.IsNullOrEmpty(ConfigurationSettings.AppSettings["HighQualityAvatars"]))
            {
                HighQualityAvatars = bool.Parse(ConfigurationSettings.AppSettings["HighQualityAvatars"]);
            }
            else
            {
                HighQualityAvatars = true;
            }
            if (!string.IsNullOrEmpty(ConfigurationSettings.AppSettings["UseClickables"]))
            {
                UseClickables = bool.Parse(ConfigurationSettings.AppSettings["UseClickables"]);
            }
            else
            {
                UseClickables = true;
            }
            if (!string.IsNullOrEmpty(ConfigurationSettings.AppSettings["ShowAvatars"]))
            {
                ShowAvatars = bool.Parse(ConfigurationSettings.AppSettings["ShowAvatars"]);
            }
            else
            {
                ShowAvatars = true;
            }
            if (!string.IsNullOrEmpty(ConfigurationSettings.AppSettings["DistancePreference"]))
            {
                DistancePreference = ConfigurationSettings.AppSettings["DistancePreference"];
            }
            else
            {
                DistancePreference = "Miles";
            }
            if (!string.IsNullOrEmpty(ConfigurationSettings.AppSettings["UseGPS"]))
            {
                UseGPS = bool.Parse(ConfigurationSettings.AppSettings["UseGPS"]);
            }
            else
            {
                UseGPS = true;
            }

            if (!string.IsNullOrEmpty(ConfigurationSettings.AppSettings["IsMaximized"]))
            {
                IsMaximized = bool.Parse(ConfigurationSettings.AppSettings["IsMaximized"]);
            }
            else
            {
                IsMaximized = true;
            }
            if (!string.IsNullOrEmpty(ConfigurationSettings.AppSettings["UserName"]))
            {
                LegacySettingsAccount.UserName = ConfigurationSettings.AppSettings["UserName"];
            }
            if (!string.IsNullOrEmpty(ConfigurationSettings.AppSettings["Password"]))
            {
                LegacySettingsAccount.Password = ConfigurationSettings.AppSettings["Password"];
            }
            if (!string.IsNullOrEmpty(ConfigurationSettings.AppSettings["CheckVersion"]))
            {
                CheckVersion = bool.Parse(ConfigurationSettings.AppSettings["CheckVersion"]);
            }
            else
            {
                CheckVersion = true;
            }

            if (!string.IsNullOrEmpty(ConfigurationSettings.AppSettings["ShowExtra"]))
            {
                ShowExtra = bool.Parse(ConfigurationSettings.AppSettings["ShowExtra"]);
            }

            if (!string.IsNullOrEmpty(ConfigurationSettings.AppSettings["Server"]))
            {
                LegacySettingsAccount.Server = (Yedda.Twitter.TwitterServer)Enum.Parse(typeof(Yedda.Twitter.TwitterServer), ConfigurationSettings.AppSettings["Server"], true);
            }

            if (!string.IsNullOrEmpty(ConfigurationSettings.AppSettings["AnimationInterval"]))
            {
                AnimationInterval = int.Parse(ConfigurationSettings.AppSettings["AnimationInterval"],format);
            }
            else
            {
                AnimationInterval = 15;
            }
            if (!string.IsNullOrEmpty(ConfigurationSettings.AppSettings["MaxTweets"]))
            {
                MaxTweets= int.Parse(ConfigurationSettings.AppSettings["MaxTweets"],format);
            }
            else
            {
                MaxTweets = 50;
            }
            if (!string.IsNullOrEmpty(LegacySettingsAccount.UserName))
            {
                AccountsList.Add(LegacySettingsAccount);
            }
            if (!string.IsNullOrEmpty(ConfigurationSettings.AppSettings["IncludeScreenName"]))
            {
                IncludeUserName = bool.Parse(ConfigurationSettings.AppSettings["IncludeScreenName"]);
            }
            if (!string.IsNullOrEmpty(ConfigurationSettings.AppSettings["UseSkweezer"]))
            {
                UseSkweezer = bool.Parse(ConfigurationSettings.AppSettings["UseSkweezer"]);
            }
            if (!string.IsNullOrEmpty(ConfigurationSettings.AppSettings["ProxyServer"]))
            {
                ProxyServer = ConfigurationSettings.AppSettings["ProxyServer"];
            }
            else
            {
                ProxyServer = string.Empty;
            }
            if (!string.IsNullOrEmpty(ConfigurationSettings.AppSettings["ProxyPort"]))
            {
                ProxyPort = int.Parse(ConfigurationSettings.AppSettings["ProxyPort"]);
            }
            else
            {
                ProxyPort = 0;
            }
            if (!string.IsNullOrEmpty(ConfigurationSettings.AppSettings["PortalSize"]))
            {
                PortalSize= int.Parse(ConfigurationSettings.AppSettings["PortalSize"]);
            }
            else
            {
                PortalSize = MaxTweets;
            }

            foreach (Yedda.Twitter.Account a in ConfigurationSettings.Accounts)
            {
                AccountsList.Add(a);
            }
        }
        catch(Exception ex)
        {
            System.Diagnostics.Debug.WriteLine(ex.Message);
        }
        
    }
    public static void SaveSettings()
    {
        ConfigurationSettings.AppSettings.Clear();
        ConfigurationSettings.AppSettings["AutoCompleteAddressBook"] = AutoCompleteAddressBook.ToString();
        ConfigurationSettings.AppSettings["UseDIB"] = UseDIB.ToString();
        ConfigurationSettings.AppSettings["ZoomPreview"] = ZoomPreview.ToString();
        ConfigurationSettings.AppSettings["CacheDir"] = CacheDir;
        ConfigurationSettings.AppSettings["SelectedMediaService"] = SelectedMediaService;
        ConfigurationSettings.AppSettings["SendMessageToMediaService"] = SendMessageToMediaService.ToString();
        ConfigurationSettings.AppSettings["FontSize"] = FontSize.ToString();
        ConfigurationSettings.AppSettings["SearchItems"] = string.Join("|", SearchItems.ToArray());
        ConfigurationSettings.AppSettings["AutoTranslate"] = AutoTranslate.ToString();
        ConfigurationSettings.AppSettings["ThemeName"] = ThemeName;
        ConfigurationSettings.AppSettings["ShowAvatars"] = ShowAvatars.ToString();
        ConfigurationSettings.AppSettings["UseGPS"] = UseGPS.ToString();
        ConfigurationSettings.AppSettings["IsMaximized"] = IsMaximized.ToString();
        ConfigurationSettings.AppSettings["CheckVersion"] = CheckVersion.ToString();
        ConfigurationSettings.AppSettings["AnimationInterval"] = AnimationInterval.ToString();
        ConfigurationSettings.AppSettings["MaxTweets"] = MaxTweets.ToString();
        ConfigurationSettings.AppSettings["DistancePreference"] = DistancePreference;
        ConfigurationSettings.AppSettings["UseClickables"] = UseClickables.ToString();
        ConfigurationSettings.AppSettings["IncludeScreenName"] = IncludeUserName.ToString();
        ConfigurationSettings.AppSettings["HighQualityAvatars"] = HighQualityAvatars.ToString();
        ConfigurationSettings.AppSettings["UseSkweezer"] = UseSkweezer.ToString();
        ConfigurationSettings.AppSettings["ProxyServer"] = ProxyServer.ToString();
        ConfigurationSettings.AppSettings["ProxyPort"] = ProxyPort.ToString();
        ConfigurationSettings.AppSettings["PortalSize"] = PortalSize.ToString();
        ConfigurationSettings.AppSettings["AutoScrollToTop"] = ClientSettings.AutoScrollToTop.ToString();
        if (ConfigurationSettings.AppSettings["UserName"] != null)
        {
            ConfigurationSettings.AppSettings.Remove("UserName");
        }
        if (ConfigurationSettings.AppSettings["Password"] != null)
        {
            ConfigurationSettings.AppSettings.Remove("Password");
        }
        ConfigurationSettings.AppSettings["ShowExtra"] = ShowExtra.ToString();
        ConfigurationSettings.AppSettings["UpdateMinutes"] = UpdateMinutes.ToString();
        ConfigurationSettings.AppSettings["UILanguage"] = PockeTwit.Localization.XmlBasedResourceManager.CultureInfo.Name;
        ConfigurationSettings.Accounts = AccountsList;
        ConfigurationSettings.SaveConfig();
    }


    private static void GetTextSizes()
    {
        TextFont = new System.Drawing.Font(System.Drawing.FontFamily.GenericSansSerif, FontSize, System.Drawing.FontStyle.Regular);
        MenuFont = new Font(FontFamily.GenericSansSerif, FontSize, FontStyle.Bold);
        int SmallFontSize = 6;
        if (FontSize > 4)
        {
            SmallFontSize = FontSize - 3;
        }
        SmallFont = new Font(FontFamily.GenericSansSerif, SmallFontSize, FontStyle.Regular);
    
        using (System.Drawing.Bitmap b = new System.Drawing.Bitmap(100, 100))
        {
            using (System.Drawing.Graphics g = System.Drawing.Graphics.FromImage(b))
            {
                TextSize = (int)(g.MeasureString("H", TextFont).Height - 1);
                SmallTextSize = (int)(g.MeasureString("H", SmallFont).Height - 1);
            }
        }
    }

    public static string IconsFolder()
    {
        return (AppPath + "\\Themes\\" + ThemeName + "\\");
    }
    private static void ResetColors()
    {
        BackColor = System.Drawing.Color.Black;
        BackGradColor = System.Drawing.Color.Black;
        ErrorColor = System.Drawing.Color.Red;
        ForeColor = System.Drawing.Color.LightGray;
        LineColor = ForeColor;
        SmallTextColor = System.Drawing.Color.Gray;
        SelectedSmallTextColor = System.Drawing.Color.Gray;
        SelectedBackColor = System.Drawing.Color.Gray;
        SelectedBackGradColor = System.Drawing.Color.Black;
        SelectedForeColor = System.Drawing.Color.White;
        FieldBackColor = System.Drawing.Color.White;
        FieldForeColor = System.Drawing.Color.Black;
        LinkColor = System.Drawing.Color.LightBlue;
        HashLinkColor = System.Drawing.Color.Sienna;
        DimmedColor = Color.FromArgb(75,75,75);
        DimmedLineColor = Color.FromArgb(75, 75, 75);
        SelectedLinkColor = LinkColor;
        SelectedHashLinkColor = HashLinkColor;
        PointerColor = ForeColor;
        MenuTextColor = ForeColor;
        MenuSelectedTextColor = ForeColor;
    }
    public static void LoadColors()
    {
        ResetColors();
        if (!System.IO.File.Exists(AppPath + "\\Themes\\" + ThemeName + "\\" + ThemeName + ".txt"))
        {
            ThemeName = "Original";
        }
        if (System.IO.File.Exists(AppPath + "\\Themes\\" + ThemeName + "\\" + ThemeName + ".txt"))
        {
            using (System.IO.StreamReader r = new System.IO.StreamReader(AppPath + "\\Themes\\" + ThemeName + "\\" + ThemeName + ".txt"))
            {
                while(!r.EndOfStream)
                {
                    string[] ColorPair = r.ReadLine().Split(new char[]{':'});
                    string ColorType = ColorPair[0];


                    System.Drawing.Color ColorChosen = System.Drawing.Color.FromArgb(int.Parse(ColorPair[1]), int.Parse(ColorPair[2]), int.Parse(ColorPair[3]));

                    FieldInfo fi = typeof(ClientSettings).GetField(ColorType);
                    try
                    {
                        fi.SetValue(null, ColorChosen);
                    }
                    catch { }
                }
            }

        }
    }
	#endregion Methods 

}
