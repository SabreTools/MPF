using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using DICUI.Data;
using DICUI.Utilities;

namespace DICUI.Web
{
    public class RedumpAccess
    {
        #region Base URLs

        private const string loginUrl = "http://forum.redump.org/login/";

        #endregion

        #region Disc URLs and Extensions

        private const string discPageUrl = @"http://redump.org/disc/{0}/";
        private const string wipDiscPageUrl = @"http://redump.org/newdisc/{0}/";

        private const string changesExt = "/changes/";
        private const string cueExt = "cue/";
        private const string gdiExt = "gdi/";
        private const string keyExt = "key/";
        private const string md5Ext = "md5/";
        private const string sbiExt = "sbi/";
        private const string sfvExt = "sfv/";
        private const string sha1Ext = "sha1/";

        #endregion

        #region List URLs

        private const string lastModifiedUrl = @"http://redump.org/discs/sort/modified/dir/desc?page={0}";
        private const string quickSearchUrl = @"http://redump.org/discs/quicksearch/{0}/?page={1}";
        private const string userDumpsUrl = @"http://redump.org/discs/dumper/{0}/?page={1}";
        private const string wipDumpsUrl = @"http://redump.org/discs-wip/";

        #endregion

        #region Pack URLs

        private const string packCuesUrl = @"http://redump.org/cues/{0}/";
        private const string packDatfileUrl = @"http://redump.org/datfile/{0}/";
        private const string packDkeysUrl = @"http://redump.org/dkeys/{0}/";
        private const string packGdiUrl = @"http://redump.org/gdi/{0}/";
        private const string packKeysUrl = @"http://redump.org/keys/{0}/";
        private const string packSbiUrl = @"http://redump.org/sbi/{0}/";

        #endregion

        #region Regexes

        private Regex discRegex = new Regex(@"<a href=""/disc/(\d+)/"">");
        private Regex newDiscRegex = new Regex(@"<a href=""/newdisc/(\d+)/"">");
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

        #region Process IDs and Pages

        /// <summary>
        /// Get the list of the last modified IDs, in order of appearance
        /// </summary>
        /// <param name="wc">CookieAwareWebClient to hold the login state</param>
        /// <param name="pageCount">Number of pages to grab until stopping; -1 means continue until end</param>
        /// <returns>A list of IDs in order of last modified</returns>
        private List<int> ProcessLastModified(CookieAwareWebClient wc, int pageCount = -1)
        {
            List<int> ids = new List<int>();

            // If we have a -1 page count, set the maximum page limit
            if (pageCount == -1)
                pageCount = Int32.MaxValue;

            // Keep getting last modified pages until there are none left
            int pageNumber = 1;
            while (pageNumber < pageCount)
            {
                List<int> pageIds = CheckSingleSitePage(wc, string.Format(lastModifiedUrl, pageNumber++));
                ids.AddRange(pageIds);
                if (pageIds.Count < 2)
                    break;
            }

            return ids;
        }

        /// <summary>
        /// Retrieve premade packs from Redump
        /// </summary>
        /// <param name="wc">CookieAwareWebClient to hold the login state</param>
        private void ProcessAllPacks(CookieAwareWebClient wc)
        {
            var cuesPacks = this.DownloadPacks(wc, packCuesUrl, Extensions.HasCues, "CUEs");
            var datPacks = this.DownloadPacks(wc, packDatfileUrl, Extensions.HasDat, "DATs");
            var dkeyPacks = this.DownloadPacks(wc, packDkeysUrl, Extensions.HasDkeys, "Decrypted KEYS");
            var gdiPacks = this.DownloadPacks(wc, packGdiUrl, Extensions.HasGdi, "GDIs");
            var keysPacks = this.DownloadPacks(wc, packKeysUrl, Extensions.HasKeys, "KEYS");
            var sbiPacks = this.DownloadPacks(wc, packSbiUrl, Extensions.HasSbi, "SBIs");
        }

        /// <summary>
        /// Retrieve premade packs from Redump
        /// </summary>
        /// <param name="wc">CookieAwareWebClient to hold the login state</param>
        /// <param name="system">RedumpSystem to get all possible packs for</param>
        private void ProcessPacksForSystem(CookieAwareWebClient wc, RedumpSystem system)
        {
            var packs = new Dictionary<string, byte[]>();

            if (Extensions.HasCues.Contains(system))
                packs.Add("cues", this.DownloadPack(wc, packCuesUrl, system, "CUEs"));

            if (Extensions.HasDat.Contains(system))
                packs.Add("dat", this.DownloadPack(wc, packDatfileUrl, system, "DATs"));

            if (Extensions.HasDkeys.Contains(system))
                packs.Add("dkeys", this.DownloadPack(wc, packDkeysUrl, system, "Decrypted KEYS"));

            if (Extensions.HasGdi.Contains(system))
                packs.Add("gdi", this.DownloadPack(wc, packGdiUrl, system, "GDIs"));

            if (Extensions.HasKeys.Contains(system))
                packs.Add("keys", this.DownloadPack(wc, packKeysUrl, system, "KEYS"));

            if (Extensions.HasSbi.Contains(system))
                packs.Add("sbi", this.DownloadPack(wc, packSbiUrl, system, "SBIs"));
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
        /// Get the list of IDs associated with the given user
        /// </summary>
        /// <param name="wc">CookieAwareWebClient to hold the login state</param>
        /// <param name="username">Redump username to get the list of IDs for</param>
        /// <returns>A list of IDs associated with that user</returns>
        private List<int> ProcessUser(CookieAwareWebClient wc, string username)
        {
            List<int> ids = new List<int>();

            // Keep getting user pages until there are none left
            int pageNumber = 1;
            while (true)
            {
                List<int> pageIds = CheckSingleSitePage(wc, string.Format(userDumpsUrl, username, pageNumber++));
                ids.AddRange(pageIds);
                if (pageIds.Count < 2)
                    break;
            }

            return ids;
        }

        #endregion

        #region Single item processing

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
        /// Process a Redump WIP page as a list of possible IDs or disc page
        /// </summary>
        /// <param name="wc">CookieAwareWebClient to access the pages</param>
        /// <param name="url">Page URL to check and parse</param>
        /// <returns>List of matching IDs</returns>
        private List<int> CheckSingleWIPPage(CookieAwareWebClient wc, string url)
        {
            List<int> ids = new List<int>();

            var wipDumpsPage = wc.DownloadString(url);

            // If we have no WIP dumps left
            if (wipDumpsPage.Contains("No WIP discs found."))
                return ids;

            // Otherwise, traverse each dump on the page
            var matches = newDiscRegex.Matches(wipDumpsPage);
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
        /// Download an individual pack
        /// </summary>
        /// <param name="wc">CookieAwareWebClient to access the packs</param>
        /// <param name="url">Base URL to download using</param>
        /// <param name="system">System to download packs for</param>
        /// <param name="title">Name of the pack that is downloading</param>
        private byte[] DownloadPack(CookieAwareWebClient wc, string url, RedumpSystem system, string title)
        {
            Console.WriteLine($"Downloading {title}");
            Console.Write($"\r{system.LongName()}{new string(' ', Console.BufferWidth - system.LongName().Length - 1)}");
            var pack = wc.DownloadData(string.Format(url, system.ShortName()));

            Console.Write($"\rComplete!{new string(' ', Console.BufferWidth - 10)}");
            Console.WriteLine();

            return pack;
        }

        /// <summary>
        /// Download a set of packs
        /// </summary>
        /// <param name="wc">CookieAwareWebClient to access the packs</param>
        /// <param name="url">Base URL to download using</param>
        /// <param name="systems">List of systems to download packs for</param>
        /// <param name="title">Name of the pack that is downloading</param>
        private Dictionary<RedumpSystem, byte[]> DownloadPacks(CookieAwareWebClient wc, string url, RedumpSystem[] systems, string title)
        {
            var dict = new Dictionary<RedumpSystem, byte[]>();

            Console.WriteLine($"Downloading {title}");
            foreach (var system in systems)
            {
                Console.Write($"\r{system.LongName()}{new string(' ', Console.BufferWidth - system.LongName().Length - 1)}");
                dict.Add(system, wc.DownloadData(string.Format(url, system.ShortName())));
            }

            Console.Write($"\rComplete!{new string(' ', Console.BufferWidth - 10)}");
            Console.WriteLine();

            return dict;
        }

        /// <summary>
        /// Download an individual site ID page as a string, if possible
        /// </summary>
        /// <param name="wc">CookieAwareWebClient to access the pages</param>
        /// <param name="id">Redump disc ID to retrieve</param>
        /// <returns>Disc page as a string, null on error</returns>
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

        /// <summary>
        /// Download an individual WIP ID page as a string, if possible
        /// </summary>
        /// <param name="wc">CookieAwareWebClient to access the pages</param>
        /// <param name="id">Redump disc ID to retrieve</param>
        /// <returns>WIP disc page as a string, null on error</returns>
        private string DownloadSingleWIPID(CookieAwareWebClient wc, int id)
        {
            string paddedId = id.ToString().PadLeft(5, '0');
            Console.WriteLine($"Processing WIP ID: {paddedId}");
            try
            {
                string discPage = wc.DownloadString(string.Format(wipDiscPageUrl, +id));
                if (discPage.Contains($"WIP Disc with ID \"{id}\" doesn't exist"))
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

        #endregion
    }
}
