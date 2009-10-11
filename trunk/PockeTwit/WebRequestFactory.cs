using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace PockeTwit
{
    class WebRequestFactory
    {
        public static HttpWebRequest CreateHttpRequest(string url)
        {
            return CreateHttpRequest(new Uri(url));
        }

        public static HttpWebRequest CreateHttpRequest(Uri uri)
        {
            var request = (HttpWebRequest)WebRequest.Create(uri);
            request.AllowAutoRedirect = true;
            if (!string.IsNullOrEmpty(ClientSettings.ProxyServer))
            {
                var proxy = new WebProxy(ClientSettings.ProxyServer, ClientSettings.ProxyPort);
                proxy.BypassProxyOnLocal = true;
                proxy.Credentials = CredentialCache.DefaultCredentials;
                request.Proxy = proxy;
            }
            return request;
        }
    }
}
