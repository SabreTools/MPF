using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;

namespace DICUI.Web
{
    public class RedumpAccess
    {
        #region Base URLs

        private const string loginUrl = "http://forum.redump.org/login/";

        #endregion

        #region Disc URLs and Extensions

        private const string discPageUrl = @"http://redump.org/disc/{0}/";

        #endregion

        #region List URLs

        private const string quickSearchUrl = @"http://redump.org/discs/quicksearch/{0}/?page={1}";

        #endregion

        #region Regexes

        private Regex discRegex = new Regex(@"<a href=""/disc/(\d+)/"">");
        private Regex tokenRegex = new Regex(@"<input type=""hidden"" name=""csrf_token"" value=""(.*?)"" />");

        #endregion

        /// <summary>
        /// Login to Redump, if possible
        /// </summary>
        /// <param name="wc">CookieAwareWebClient to hold the login state</param>
        /// <param name="username">Redump username to log in for protected systems</param>
        /// <param name="password">Redump password to log in for protected systems</param>
        /// <returns>True login was successful, false otherwise</returns>
        public bool RedumpLogin(CookieAwareWebClient wc, string username, string password)
        {
            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
                return false;

            var loginPage = wc.DownloadString(loginUrl);
            string token = this.tokenRegex.Match(loginPage).Groups[1].Value;

            wc.Headers[HttpRequestHeader.ContentType] = "application/x-www-form-urlencoded";
            wc.Encoding = Encoding.UTF8;
            var response = wc.UploadString(loginUrl, $"form_sent=1&redirect_url=&csrf_token={token}&req_username={username}&req_password={password}&save_pass=0");

            if (response.Contains("Incorrect username and/or password."))
                return false;
            else
                return true;
        }

        /// <summary>
        /// Get the list of IDs that associate with a given string
        /// </summary>
        /// <param name="wc">CookieAwareWebClient to hold the login state</param>
        /// <param name="query">Value to search for in Redump</param>
        /// <returns>A list of IDs associated with that value</returns>
        public List<int> ProcessSearch(CookieAwareWebClient wc, string query)
        {
            List<int> ids = new List<int>();

            // Keep getting quicksearch pages until there are none left
            int pageNumber = 1;
            while (true)
            {
                List<int> pageIds = CheckSingleSitePage(wc, string.Format(quickSearchUrl, query, pageNumber++));
                ids.AddRange(pageIds);
                if (pageIds.Count < 2)
                    break;
            }

            return ids;
        }

        /// <summary>
        /// Process a Redump site page as a list of possible IDs or disc page
        /// </summary>
        /// <param name="wc">CookieAwareWebClient to access the pages</param>
        /// <param name="url">Page URL to check and parse</param>
        /// <returns>List of matching IDs</returns>
        private List<int> CheckSingleSitePage(CookieAwareWebClient wc, string url)
        {
            List<int> ids = new List<int>();

            var dumpsPage = wc.DownloadString(url);

            // If we have no dumps left
            if (dumpsPage.Contains("No discs found."))
                return ids;

            // If we have a single disc page already
            if (dumpsPage.Contains("<b>Download:</b>"))
            {
                var value = Regex.Match(dumpsPage, @"/disc/(\d+)/sfv/").Groups[1].Value;
                if (Int32.TryParse(value, out int id))
                    ids.Add(id);

                return ids;
            }

            // Otherwise, traverse each dump on the page
            var matches = discRegex.Matches(dumpsPage);
            foreach (Match match in matches)
            {
                try
                {
                    if (Int32.TryParse(match.Groups[1].Value, out int value))
                        ids.Add(value);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"An exception has occurred: {ex}");
                    continue;
                }
            }

            return ids;
        }

        /// <summary>
        /// Download an individual site ID page as a string, if possible
        /// </summary>
        /// <param name="wc">CookieAwareWebClient to access the pages</param>
        /// <param name="id">Redump disc ID to retrieve</param>
        /// <returns></returns>
        public string DownloadSingleSiteID(CookieAwareWebClient wc, int id)
        {
            string paddedId = id.ToString().PadLeft(5, '0');
            Console.WriteLine($"Processing ID: {paddedId}");
            try
            {
                string discPage = wc.DownloadString(string.Format(discPageUrl, +id));
                if (discPage.Contains($"Disc with ID \"{id}\" doesn't exist"))
                    return null;
                else
                    return discPage;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An exception has occurred: {ex}");
                return null;
            }
        }
    }
}
