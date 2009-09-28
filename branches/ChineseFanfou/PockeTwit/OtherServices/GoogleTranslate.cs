using System;
using System.Net;
using System.Xml;
using System.Web;
using System.IO;
using System.Collections.Generic;
using System.Text;

namespace PockeTwit.OtherServices
{
    class GoogleTranslate
    {
        private const string detectURL = @"http://ajax.googleapis.com/ajax/services/language/detect?v=1.0&q={0}";
        private const string apiURL = @"http://ajax.googleapis.com/ajax/services/language/translate?v=1.0&q={0}&langpair={1}%7C{2}&format=html";
        protected static string ExecuteGetCommand(string url)
        {
            HttpWebRequest client = WebRequestFactory.CreateHttpRequest(url);
            client.Timeout = 20000;
            
            try
            {
                using (HttpWebResponse httpResponse = (HttpWebResponse)client.GetResponse())
                {
                    using (Stream stream = httpResponse.GetResponseStream())
                    {
                        using (StreamReader reader = new StreamReader(stream))
                        {
                            return reader.ReadToEnd();
                        }
                    }
                }
            }
            catch (WebException ex)
            {
                //
                // Handle HTTP 404 errors gracefully and return a null string to indicate there is no content.
                //
                if (ex.Response is HttpWebResponse)
                {
                    try
                    {
                        if ((ex.Response as HttpWebResponse).StatusCode == HttpStatusCode.NotFound)
                        {
                            return null;
                        }

                        HttpWebResponse errorResponse = (HttpWebResponse)ex.Response;
                        if (errorResponse.StatusCode == HttpStatusCode.Unauthorized)
                        {
                            return null;
                        }
                        string ErrorText;
                        using (Stream stream = errorResponse.GetResponseStream())
                        {
                            using (StreamReader reader = new StreamReader(stream))
                            {
                                ErrorText = reader.ReadToEnd();
                                return ErrorText;
                            }
                        }
                    }
                    catch
                    {
                        ex.Response.Close();
                    }
                }

            }
            return null;
        }

        public static string GetLanguage(string Original)
        {
            string URL = string.Format(detectURL, HttpUtility.UrlEncode(Original));
            string response = ExecuteGetCommand(URL);
            if (string.IsNullOrEmpty(response))
            {
                return ClientSettings.TranslationLanguage;
            }
            return getJsonValue(response, "language");
        }

        public static string GetTranslation(string Original)
        {
            try
            {
                string origLanguage = GetLanguage(Original);
                if (origLanguage == ClientSettings.TranslationLanguage)
                {
                    return Original;
                }
                string URL = string.Format(apiURL, HttpUtility.UrlEncode(Original), origLanguage, ClientSettings.TranslationLanguage);
                string response = ExecuteGetCommand(URL);
                if (string.IsNullOrEmpty(response))
                {
                    return Original;
                }
                string newOutput = "Translated " + origLanguage + " to " + ClientSettings.TranslationLanguage + " by Google.\r\n\r\n" + HttpUtility.HtmlDecode(getJsonValue(response, "translatedText"));
                return newOutput;
            }
            catch
            {
                return Original;
            }
        }

        private static string getJsonValue(string json, string value)
        {
            int position = json.IndexOf("\""+value+"\":\"") + value.Length+4;
            int endPosition = json.IndexOf("\"", position+1);
            return json.Substring(position, endPosition-position);
        }
        
    }
}