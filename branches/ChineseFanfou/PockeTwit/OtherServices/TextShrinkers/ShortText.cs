using System;
using System.IO;
using System.Net;
using System.Web;
using System.Text;

namespace PockeTwit.OtherServices.TextShrinkers
{
    public class ShortText : ITextShrinker
    {
        private const string API = "http://shortText.com/api.aspx";
        private const string APIKey = "66810129-8F4D-45D8-B6B4-922E35817A48";

        private static readonly System.Text.RegularExpressions.Regex MatchURL = new System.Text.RegularExpressions.Regex("shorttext.com", System.Text.RegularExpressions.RegexOptions.IgnoreCase);


        public static bool IsShortTextURL(string urlToCheck)
        {
            return MatchURL.IsMatch(urlToCheck);
        }
        public string GetShortenedText(string inputText)
        {
            string data = "shorttext=" + HttpUtility.UrlEncode(inputText);
            const int trimLength = 5;
            string shortenURL = ExecutePostCommand(API, data);
            if (!string.IsNullOrEmpty(shortenURL))
            {
                string newText =
                    inputText.Substring(0, inputText.LastIndexOf(" ", 140 - (shortenURL.Length + trimLength))) + " " +
                    shortenURL;
                return newText;
            }
            return inputText;
        }

        public string ServiceName()
        {
            return "ShortText";
        }

        public string ServiceDescription()
        {
            return "Will add a link to your tweet that allows someone to view the full text.";
        }

        public static string GetFullText(string textURL)
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
            var request = WebRequestFactory.CreateHttpRequest(url);
            
            
            request.ContentType = "application/x-www-form-urlencoded";
            request.Method = "POST";
            request.Timeout = 20000;
            
            byte[] bytes = Encoding.UTF8.GetBytes(data);

            request.ContentLength = bytes.Length;
            try
            {
                using (var requestStream = request.GetRequestStream())
                {
                    requestStream.Write(bytes, 0, bytes.Length);
                    requestStream.Flush();
                }
            }
            catch (Exception)
            {

            }
            try
            {
                using (var response = (HttpWebResponse)request.GetResponse())
                {
                    using (var reader = new StreamReader(response.GetResponseStream()))
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