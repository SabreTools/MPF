using System;
using System.Net;

namespace DICUI.Web
{
    // https://stackoverflow.com/questions/1777221/using-cookiecontainer-with-webclient-class
    public class CookieAwareWebClient : WebClient
    {
        private readonly CookieContainer m_container = new CookieContainer();

        protected override WebRequest GetWebRequest(Uri address)
        {
            WebRequest request = base.GetWebRequest(address);
            HttpWebRequest webRequest = request as HttpWebRequest;
            if (webRequest != null)
            {
                webRequest.CookieContainer = m_container;
            }

            return request;
        }

        /// <summary>
        /// Get the last downloaded filename, if possible
        /// </summary>
        /// <returns></returns>
        public string GetLastFilename()
        {
            // Try to extract the filename from the Content-Disposition header
            if (!String.IsNullOrEmpty(this.ResponseHeaders["Content-Disposition"]))
                return this.ResponseHeaders["Content-Disposition"].Substring(this.ResponseHeaders["Content-Disposition"].IndexOf("filename=") + 9).Replace("\"", "");

            return null;
        }
    }
}
