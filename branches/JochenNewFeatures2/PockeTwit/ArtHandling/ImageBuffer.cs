/*
using System;
using System.Drawing;
using System.Collections.Generic;
using System.Text;

namespace PockeTwit
{
    public class ImageBuffer
    {
        class ImageInfo
        {
            public Image Image;
            public DateTime LastRequested;
            public String ID;
        }
		#region Fields (3) 

        public static Bitmap FavoriteImage;
        private SafeDictionary<string, ImageInfo> ImageDictionary = new SafeDictionary<string, ImageInfo>();
        public static Bitmap UnknownArt;
        private AsyncArtGrabber Grabber = new AsyncArtGrabber();
        //private static System.Threading.Timer timerUpdate;

		#endregion Fields 

		#region Constructors (1) 

        public ImageBuffer()
        {
            FavoriteImage = new Bitmap(ClientSettings.AppPath + "\\asterisk_yellow.png");
            Bitmap DiskUnknown = new Bitmap(ClientSettings.AppPath + "\\unknownart-small.jpg");
            UnknownArt = new Bitmap(ClientSettings.SmallArtSize, ClientSettings.SmallArtSize);
            Graphics g = Graphics.FromImage(UnknownArt);
            g.DrawImage(DiskUnknown, new Rectangle(0, 0, ClientSettings.SmallArtSize, ClientSettings.SmallArtSize), new Rectangle(0, 0, DiskUnknown.Width, DiskUnknown.Height), GraphicsUnit.Pixel);
            g.Dispose();
            Grabber.NewArtWasDownloaded += new AsyncArtGrabber.ArtIsReady(AsyncArtGrabber_NewArtWasDownloaded);
        }


		#endregion Constructors 

		#region Delegates and Events (2) 


		// Delegates (1) 

        public delegate void ArtWasUpdated(string User);

        public delegate void delAvatarHasChanged(string User, string NewURL);

		// Events (1) 
        
        public event ArtWasUpdated Updated;
        public event delAvatarHasChanged AvatarHasChanged;

		#endregion Delegates and Events 

		#region Methods (6) 


		// Public Methods (3) 

        public void Clear()
        {
            ImageDictionary.Clear();
        }

        public Image GetArt(string User)
        {
            User = User.ToLower();
            lock (ImageDictionary)
            {
                if (!ImageDictionary.ContainsKey(User))
                {
                    string ArtPath = Grabber.DetermineCacheFileName(User, "");
                    if (System.IO.File.Exists(ArtPath))
                    {
                        LoadArt(User);
                        ImageDictionary[User].LastRequested = DateTime.Now;
                        return ImageDictionary[User].Image;
                    }
                    //How do we find art for a user by name alone?
                    return UnknownArt;
                }
                ImageDictionary[User].LastRequested = DateTime.Now;
                return ImageDictionary[User].Image;    
            }
        }

        public Image GetArt(string User, string URL)
        {
            User = User.ToLower();
            if (User == null) { return null; }
            if (string.IsNullOrEmpty(URL))
            {
                return GetArt(User);
            }
            if (!ImageDictionary.ContainsKey(User))
            {
                if (!LoadArt(User, URL))
                {
                    if (string.IsNullOrEmpty(URL))
                    {
                        System.Diagnostics.Debug.WriteLine("Falling back to load user from screename");
                        Library.User newUser = null;
                        foreach (Yedda.Twitter.Account Account in ClientSettings.AccountsList)
                        {
                            newUser = Library.User.FromId(User, Account);
                            if (newUser != null) { break; }
                        }
                        if (newUser == null) { return UnknownArt; }
                        URL = newUser.profile_image_url;
                        LoadArt(User, URL);
                    }
                    return UnknownArt;
                }
            }

            ImageDictionary[User].LastRequested = DateTime.Now;
            return ImageDictionary[User].Image;
        }

        public bool HasArt(string User)
        {
            User = User.ToLower();
            if (ImageDictionary.ContainsKey(User))
            {
                return true;
            }
            string ArtPath = Grabber.DetermineCacheFileName(User,"");
            if (System.IO.File.Exists(ArtPath))
            {
                LoadArt(User);
                return true;
            }
            return false;
        }

        public void Trim()
        {
            DateTime runTime = DateTime.Now;
            lock (ImageDictionary)
            {
                List<string> Keys = new List<string>(ImageDictionary.Keys);
                foreach (string infoKey in Keys)
                {
                    ImageInfo info = ImageDictionary[infoKey];
                    if (runTime.Ticks - info.LastRequested.Ticks > 1000)
                    {
                        System.Diagnostics.Debug.WriteLine("Removing " + infoKey + " from imagebuffer");
                        ImageDictionary.Remove(infoKey);
                    }
                }
                Keys.Clear();
                Keys.TrimExcess(); 
            }
        }

		// Private Methods (3) 

        private void AsyncArtGrabber_NewArtWasDownloaded(string User, string Filename)
        {
            User = User.ToLower();
            if (System.IO.File.Exists(Filename))
            {
                try
                {
                    Bitmap NewArt = new Bitmap(Filename);
                    ImageInfo newInfo = new ImageInfo();
                    newInfo.Image = NewArt;
                    newInfo.LastRequested = DateTime.Now;
                    newInfo.ID = ImageDictionary[User].ID;
                    ImageDictionary[User] = newInfo;

                    System.IO.FileStream fs = new System.IO.FileStream(Filename + ".ID", System.IO.FileMode.OpenOrCreate, System.IO.FileAccess.Write);
                    using (System.IO.StreamWriter sw = new System.IO.StreamWriter(fs))
                    {
                        sw.Write(newInfo.ID);
                        sw.Flush();
                        sw.Close();
                    }

                    //WHY IS NOTHING CATCHING THESE?
                    if (Updated != null)
                    {
                        Updated(User);
                    }
                    else
                    {
                        GlobalEventHandler.CallArtWasUpdated(User);
                    }
                }
                catch
                {
                    //Try again next time.
                    System.IO.File.Delete(Filename);
                    System.IO.File.Delete(Filename + ".ID");
                }
            }
        }

        private bool LoadArt(string User)
        {
            User = User.ToLower();
            string ArtPath = Grabber.DetermineCacheFileName(User,"");
            
            Bitmap NewArt;
            try
            {
                NewArt = new Bitmap(ArtPath);
            }
            catch (System.IO.IOException)
            {
                return false;
            }
            ImageInfo newInfo = new ImageInfo();
            newInfo.LastRequested = DateTime.Now;
            newInfo.Image = NewArt;
            ImageDictionary.Add(User, newInfo);
            return true;
        }

        private bool LoadArt(string User, string URL)
        {
            User = User.ToLower();
            string ArtPath = Grabber.CopyTempFile(User, URL);
            Bitmap NewArt;
            string ID,ID2;
            bool bFound = false;
            try
            {
                if (ArtPath != null)
                {
                    NewArt = new Bitmap(ArtPath);

                    ID = URL;

                    if (System.IO.File.Exists(ArtPath + ".ID"))
                    {
                        using (System.IO.StreamReader reader = new System.IO.StreamReader(ArtPath + ".ID"))
                        {
                            ID2 = reader.ReadToEnd();
                        }

                        // Hack to download the latest image
                        if (ID != ID2)
                        {
                            Grabber.GetArt(User, URL);
                            WriteID(ArtPath, ID);
                            if (AvatarHasChanged != null)
                            {
                                AvatarHasChanged(User, URL);
                            }
                            else
                            {
                                GlobalEventHandler.CallAvatarHasChanged(User, URL);
                            }
                            
                        }
                    }
                    else
                    {
                        WriteID(ArtPath, ID);
                    }

                    bFound = true;
                }
                else
                {
                    NewArt = UnknownArt;

                    ID = URL;

                }
                if (ImageDictionary.ContainsKey(User))
                {
                    ImageDictionary.Remove(User);
                }

                ImageInfo newInfo = new ImageInfo();
                newInfo.LastRequested = DateTime.Now;
                newInfo.Image = NewArt;
                newInfo.ID = ID;
                ImageDictionary.Add(User, newInfo);
                return bFound;
            }
            catch
            {
                return false;
            }
        }

        private void WriteID(string ArtPath, string ID)
        {
            System.IO.FileStream fs = new System.IO.FileStream(ArtPath + ".ID", System.IO.FileMode.Create, System.IO.FileAccess.Write);
            using (System.IO.StreamWriter sw = new System.IO.StreamWriter(fs))
            {
                sw.Write(ID);
                sw.Flush();
                sw.Close();
            }
        }

		#endregion Methods 

    }
}
*/