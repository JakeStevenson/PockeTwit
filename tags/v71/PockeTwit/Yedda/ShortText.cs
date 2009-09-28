using System;
using System.IO;
using System.Net;
using System.Web;
using System.Collections.Generic;
using System.Text;

namespace Yedda
{
    public static class ShortText
    {
        private static string API = "http://shortText.com/api.aspx";
        private static string APIKey = "66810129-8F4D-45D8-B6B4-922E35817A48";

        private static System.Text.RegularExpressions.Regex matchURL = new System.Text.RegularExpressions.Regex("shorttext.com", System.Text.RegularExpressions.RegexOptions.IgnoreCase);


        public static bool isShortTextURL(string URLToCheck)
        {
            return matchURL.IsMatch(URLToCheck);
        }
        public static string shorten(string inputText)
        {
            string data = "shorttext=" + HttpUtility.UrlEncode(inputText);
            string shortenURL = ExecutePostCommand(API, data);
            return shortenURL;
        }

        public static string getFullText(string textURL)
        {
            textURL = textURL.Substring(textURL.LastIndexOf("/")+1);
            string data = "appkey=" + HttpUtility.UrlEncode(APIKey) + "&url=" + textURL;
            string ret = ExecutePostCommand(API, data);
            ret = ret.Substring(0, ret.IndexOf("<div "));
            ret = HttpUtility.HtmlDecode(ret);
            return ret;
            
        }



        private static string ExecutePostCommand(string url, string data)
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            
            
            request.ContentType = "application/x-www-form-urlencoded";
            request.Method = "POST";
            request.Timeout = 20000;
            
            byte[] bytes = Encoding.UTF8.GetBytes(data);

            request.ContentLength = bytes.Length;
            try
            {
                using (Stream requestStream = request.GetRequestStream())
                {
                    requestStream.Write(bytes, 0, bytes.Length);
                    requestStream.Flush();
                }
            }
            catch
            {

            }
            try
            {
                using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
                {
                    using (StreamReader reader = new StreamReader(response.GetResponseStream()))
                    {
                        return reader.ReadToEnd();
                    }
                }
            }
            catch (WebException ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.Message);
            }
            return null;
            }

            
        }
}
