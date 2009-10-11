using System;
using System.Net;
using System.Web;
using System.Collections.Generic;
using System.Text;

namespace PockeTwit
{
    class isgd
    {

		#region Methods (1) 


		// Public Methods (1) 

        public static string ShortenURL(string URL)
        {
            try
            {
                string TotalURL = "http://is.gd/api.php?longurl=" + URL;
                HttpWebRequest request = WebRequestFactory.CreateHttpRequest(TotalURL);
                using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
                {
                    using (System.IO.StreamReader reader = new System.IO.StreamReader(response.GetResponseStream()))
                    {
                        return reader.ReadToEnd();
                    }
                }
            }
            catch
            {
                return null;
            }
        }


		#endregion Methods 

    }
}
