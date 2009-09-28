using System;

using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Net;
using System.Xml;
using System.Web;

namespace PockeTwit
{
    public class UpgradeChecker
    {

		#region Fields (4) 

        public static double currentVersion = .71;
        public static bool devBuild = true;

        private string UpgradeURL = "http://pocketwit.googlecode.com/svn/LatestRelease/Release.xml";
        private UpgradeInfo WebVersion;
        private string XMLResponse;

		#endregion Fields 

		#region Constructors (2) 

        public UpgradeChecker(bool Auto)
        {
            if (Auto)
            {
                CheckForUpgrade();
            }
        }

        public UpgradeChecker()
        {
            if (ClientSettings.CheckVersion)
            {
                CheckForUpgrade();
            }
        }

		#endregion Constructors 

		#region Delegates and Events (3) 


		// Delegates (1) 

        public delegate void delUpgradeFound(UpgradeInfo Info);


		// Events (2) 

        public event delUpgradeFound CurrentVersion;

        public event delUpgradeFound UpgradeFound;


		#endregion Delegates and Events 

		#region Methods (2) 


		// Public Methods (1) 

        public void CheckForUpgrade()
        {
            if (!devBuild)
            {
                System.Threading.ThreadPool.QueueUserWorkItem(new System.Threading.WaitCallback(GetWebResponse));
            }
        }



		// Private Methods (1) 

        private void GetWebResponse(object o)
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(UpgradeURL);
            try
            {
                HttpWebResponse httpResponse = (HttpWebResponse)request.GetResponse();
                using (Stream stream = httpResponse.GetResponseStream())
                {
                    using (StreamReader reader = new StreamReader(stream))
                    {
                        XMLResponse = reader.ReadToEnd();
                    }
                }
            }
            catch{}
            XmlDocument UpgradeInfoDoc = new XmlDocument();
            try
            {
                if (XMLResponse != null)
                {
                    UpgradeInfoDoc.LoadXml(XMLResponse);
                    WebVersion = new UpgradeInfo();
                    WebVersion.webVersion = double.Parse(UpgradeInfoDoc.SelectSingleNode("//version").InnerText,System.Globalization.CultureInfo.InvariantCulture.NumberFormat);
                    WebVersion.DownloadURL = UpgradeInfoDoc.SelectSingleNode("//url").InnerText;
                    WebVersion.UpgradeNotes = UpgradeInfoDoc.SelectSingleNode("//notes").InnerText;

                    if (WebVersion.webVersion > currentVersion)
                    {
                        if (UpgradeFound != null)
                        {
                            UpgradeFound(WebVersion);
                        }
                    }
                    else
                    {
                        if (CurrentVersion != null)
                        {
                            CurrentVersion(WebVersion);
                        }
                    }
                }
            }
            catch
            {
            }
        }


		#endregion Methods 
        public struct UpgradeInfo
        {
            public double webVersion;
            public string DownloadURL;
            public string UpgradeNotes;
        }

    }
}
