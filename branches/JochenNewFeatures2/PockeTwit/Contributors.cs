using System;
using System.IO;
using System.Net;
using System.Web;
using System.Collections.Generic;
using System.Text;

namespace PockeTwit
{
    class Contributors
    {
        public delegate void delContributorsReady();
        public event delContributorsReady ContributorsReady = delegate { };
        public struct Contributor
        {
            public string Name;
            public string Contribution;
        }

        public Contributors()
        {
            System.Threading.ThreadPool.QueueUserWorkItem(new System.Threading.WaitCallback(GetWebResponse));
        }

        private const string ContributorURL = "http://pocketwit.googlecode.com/svn/LatestRelease/Contributors.txt";
        public List<Contributor> ContributorsList = new List<Contributor>();
        private void GetWebResponse(object o)
        {

            HttpWebRequest request = WebRequestFactory.CreateHttpRequest(ContributorURL);
            try
            {
                HttpWebResponse httpResponse = (HttpWebResponse)request.GetResponse();
                using (Stream stream = httpResponse.GetResponseStream())
                {
                    using (StreamReader reader = new StreamReader(stream))
                    {
                        while(!reader.EndOfStream)
                        {
                            string Line = reader.ReadLine();
                            Contributor s = new Contributor();
                            string[] split =Line.Split(new char[]{':'});
                            s.Name = split[0];
                            s.Contribution = split[1];
                            ContributorsList.Add(s);
                        }
                    }
                }
                ContributorsReady();
            }
            catch { }
            
        }
    }
}
