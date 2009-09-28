using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Net;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading;
using PockeTwit.LocalStorage;
using TiledMaps;

namespace PockeTwit
{
    internal static class ThrottledArtGrabber
    {
        #region Delegates

        public delegate void ArtIsReady(string Argument);

        #endregion

        public static event ArtIsReady NewArtWasDownloaded;

        private static readonly List<string> BadURLs = new List<string>();
        private static readonly Queue<string> Requests = new Queue<string>();
        private static readonly CacheDictionary<string, Bitmap> MemCache = new CacheDictionary<string, Bitmap>(35,5);

        public static Bitmap DefaultArt;
        public static Bitmap FavoriteImage;
        public static WinCEImagingBitmap mapMarkerImage;
        private static bool _running = true;
        public static bool running
        {
            get { return _running; }
            set
            {
                if (!value)
                {
                    _running = false;
                    ClearMem();
                }
            }
        }
        public static Bitmap UnknownArt;
        private static Thread WorkerThread;

        static ThrottledArtGrabber()
        {
            Setup();
            mapMarkerImage =
                new WinCEImagingBitmap(Assembly.GetExecutingAssembly().GetManifestResourceStream("PockeTwit.Marker.png"));
            FavoriteImage = new Bitmap(ClientSettings.AppPath + "\\asterisk_yellow.png");
        }

        private static void Setup()
        {
            UnknownArt = new Bitmap(ClientSettings.SmallArtSize, ClientSettings.SmallArtSize);
            DefaultArt = new Bitmap(ClientSettings.SmallArtSize, ClientSettings.SmallArtSize);


            var DiskUnknown = new Bitmap(ClientSettings.AppPath + "\\unknownart-small.jpg");
            var DiskDefault = new Bitmap(ClientSettings.AppPath + "\\default_profile_bigger.png");
            using (Graphics g = Graphics.FromImage(UnknownArt))
            {
                g.DrawImage(DiskUnknown, new Rectangle(0, 0, ClientSettings.SmallArtSize, ClientSettings.SmallArtSize),
                            new Rectangle(0, 0, DiskUnknown.Width, DiskUnknown.Height), GraphicsUnit.Pixel);
            }

            using (Graphics g = Graphics.FromImage(DefaultArt))
            {
                g.DrawImage(DiskDefault, new Rectangle(0, 0, ClientSettings.SmallArtSize, ClientSettings.SmallArtSize),
                            new Rectangle(0, 0, DiskDefault.Width, DiskDefault.Height), GraphicsUnit.Pixel);
            }
            DiskUnknown.Dispose();
            DiskDefault.Dispose();
        }

        public static void ClearMem()
        {
            lock (MemCache)
            {
                MemCache.Clear();
            }
        }

        public static Image GetArt(string url)
        {
            if (string.IsNullOrEmpty(url) | string.IsNullOrEmpty((url)))
            {
                //Don't re-queue -- we won't be able to get it for now.
                return new Bitmap(UnknownArt);
            }
            if (url == "http://static.twitter.com/images/default_profile_normal.png")
            {
                return new Bitmap(DefaultArt);
            }
            Bitmap result = null;
            if (MemCache.TryGetValue(url, out result))
            {
                return (Bitmap)result.Clone();
            }
            lock (BadURLs)
            {
                if (BadURLs.Contains(url))
                {
                    return new Bitmap(UnknownArt);
                }
            }


            try
            {
                if (!HasArt(url))
                {
                    QueueRequest(url);
                    return new Bitmap(UnknownArt);
                }
                return GetBitmapFromDB(url);
            }
            catch
            {
                return new Bitmap(UnknownArt);
            }
        }

        private static Image GetBitmapFromDB(string url)
        {
            using (SQLiteConnection conn = DataBaseUtility.GetConnection())
            {
                conn.Open();
                var s = new MemoryStream();
                using (var comm = new SQLiteCommand(conn))
                {
                    comm.CommandText =
                        "SELECT avatar FROM avatarCache WHERE url=@url;";
                    comm.Parameters.Add(new SQLiteParameter("@url", url));
                    
                    byte[] imageData = (byte[])comm.ExecuteScalar();
                    MemoryStream stream = new MemoryStream(imageData);
                    Bitmap b = new Bitmap(stream);
                    MemCache.Add(url, (Bitmap)b.Clone());
                    return b;
                }
            }
        }

        private static void QueueRequest(string request)
        {
            lock (Requests)
            {
                if (!Requests.Contains(request))
                {
                    Requests.Enqueue(request);
                }
            }
            if (WorkerThread != null) return;
            WorkerThread = new Thread(ProcessQueue) {Name = "AvatarFetcher"};
            WorkerThread.Start();
        }

        public static bool HasArt(string url)
        {
            using (SQLiteConnection conn = DataBaseUtility.GetConnection())
            {
                conn.Open();
                using (var comm = new SQLiteCommand(conn))
                {
                    comm.CommandText =
                        "SELECT url FROM avatarCache WHERE url=@url;";
                    comm.Parameters.Add(new SQLiteParameter("@url", url));

                    return comm.ExecuteScalar() != null;
                }
            }
        }

        private static void ProcessQueue()
        {
            while (Requests.Count > 0 && running)
            {
                string request;
                lock (Requests)
                {
                    request = Requests.Peek();
                }
                FetchRequest(request);
                lock (Requests)
                {
                    Requests.Dequeue();
                }
                if (NewArtWasDownloaded != null)
                {
                    NewArtWasDownloaded.Invoke(request);
                }
            }
            WorkerThread = null;
        }

        private static void AddBadURL(string URL)
        {
            if (URL == "http://static.twitter.com/images/default_profile_normal.png")
            {
                return;
            }
            lock (BadURLs)
            {
                BadURLs.Add(URL);
            }
        }

        private static void FetchRequest(string request)
        {
            if (string.IsNullOrEmpty(request))
            {
                return;
            }
            HttpWebResponse ArtResponse = null;
            try
            {
                var GetArt = WebRequestFactory.CreateHttpRequest(request);
                GetArt.Timeout = 20000;
                ArtResponse = (HttpWebResponse) GetArt.GetResponse();
            }
            catch (Exception)
            {
                lock (BadURLs)
                {
                    AddBadURL(request);
                    return;
                }
            }
            if (ArtResponse == null)
            {
                lock (BadURLs)
                {
                    AddBadURL(request);
                    return;
                }
            }
            Stream responseStream = null;
            var ArtWriter = new MemoryStream();
            try
            {
                responseStream = ArtResponse.GetResponseStream();

                int count = 0;
                var buffer = new byte[8192];
                do
                {
                    count = responseStream.Read(buffer, 0, buffer.Length);
                    if (count != 0)
                    {
                        ArtWriter.Write(buffer, 0, count);
                    }
                } while (count != 0);
                responseStream.Close();

                ArtWriter.Seek(0, SeekOrigin.Begin);
                using (var original = new Bitmap(ArtWriter))
                {
                    using (var resized = new Bitmap(ClientSettings.SmallArtSize, ClientSettings.SmallArtSize))
                    {
                        Graphics g = Graphics.FromImage(resized);
                        g.DrawImage(original,
                                    new Rectangle(0, 0, ClientSettings.SmallArtSize, ClientSettings.SmallArtSize),
                                    new Rectangle(0, 0, original.Width, original.Height), GraphicsUnit.Pixel);
                        g.Dispose();

                        byte[] blobdata = BmpToBytes_MemStream(resized);
                        using (SQLiteConnection conn = DataBaseUtility.GetConnection())
                        {
                            conn.Open();
                            using (SQLiteTransaction t = conn.BeginTransaction())
                            {
                                using (var comm = new SQLiteCommand(conn))
                                {
                                    comm.CommandText =
                                        "INSERT INTO avatarCache (avatar, url) VALUES (@avatar, @url);";
                                    comm.Parameters.Add(new SQLiteParameter("@avatar", blobdata));
                                    comm.Parameters.Add(new SQLiteParameter("@url", request));
                                    try
                                    {
                                        comm.ExecuteNonQuery();
                                    }
                                    catch (SQLiteException)
                                    {
                                    }
                                }

                                t.Commit();
                            }
                        }
                    }
                }
            }
            catch (SQLiteException)
            {
            }
            catch (Exception)
            {
                lock (BadURLs)
                {
                    AddBadURL(request);
                    return;
                }
            }
            finally
            {
                ArtWriter.Close();
                NewArtWasDownloaded(request);
            }
        }

        private static byte[] BmpToBytes_MemStream(Bitmap bmp)
        {
            var ms = new MemoryStream();
            // Save to memory using the Jpeg format
            bmp.Save(ms, ImageFormat.Jpeg);

            // read to end
            byte[] bmpBytes = ms.GetBuffer();
            bmp.Dispose();
            ms.Close();

            return bmpBytes;
        }


        public static void ClearAvatars()
        {
            GlobalEventHandler.PauseFetches();
            using (SQLiteConnection conn = DataBaseUtility.GetConnection())
            {
                conn.Open();
                using (SQLiteTransaction t = conn.BeginTransaction())
                {
                    using (var comm = new SQLiteCommand(conn))
                    {
                        comm.CommandText = "DELETE FROM avatarCache";
                        comm.ExecuteNonQuery();
                    }
                    t.Commit();
                }
                DataBaseUtility.VacuumDB();
            }
            GlobalEventHandler.ResumeFetches();
        }
        public static void ClearUnlinkedAvatars()
        {
            using (SQLiteConnection conn = DataBaseUtility.GetConnection())
            {
                conn.Open();
                using (SQLiteTransaction t = conn.BeginTransaction())
                {
                    using (var comm = new SQLiteCommand(conn))
                    {
                        comm.CommandText =
                            @"DELETE FROM avatarCache WHERE url NOT IN (
                                                SELECT DISTINCT avatarURL FROM users
                                                );";
                        int total = comm.ExecuteNonQuery();
                    }
                    t.Commit();
                }
                //LocalStorage.DataBaseUtility.VacuumDB();
            }
        }
    }
}