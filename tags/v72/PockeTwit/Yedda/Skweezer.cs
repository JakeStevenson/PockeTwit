using System;

using System.Collections.Generic;
using System.Text;

namespace Yedda
{
    class Skweezer
    {
        public static string GetSkweezerURL(string originalURL)
        {
            string baseURL = originalURL.Replace("http://", "");
            return "http://www.skweezer.com/s.aspx?i=1&q=" + baseURL;
        }
    }
}
