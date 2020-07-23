using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using DICUI.Utilities;

namespace DICUI.Web
{
    // https://stackoverflow.com/questions/1777221/using-cookiecontainer-with-webclient-class
    public class RedumpWebClient : WebClient
    {
        #region Regular Expressions

        /// <summary>
        /// Regex matching the added field on a disc page
        /// </summary>
        private Regex addedRegex = new Regex(@"<tr><th>Added</th><td>(.*?)</td></tr>");

        /// <summary>
        /// Regex matching the barcode field on a disc page
        /// </summary>
        private Regex barcodeRegex = new Regex(@"<tr><th>Barcode</th></tr><tr><td>(.*?)</td></tr>");

        /// <summary>
        /// Regex matching the BCA field on a disc page
        /// </summary>
        private Regex bcaRegex = new Regex(@"<h3>BCA .*?/></h3></td><td .*?></td></tr>"
            + "<tr><th>Row</th><th>Contents</th><th>ASCII</th></tr>"
            + "<tr><td>(?<row1number>.*?)</td><td>(?<row1contents>.*?)</td><td>(?<row1ascii>.*?)</td></tr>"
            + "<tr><td>(?<row2number>.*?)</td><td>(?<row2contents>.*?)</td><td>(?<row2ascii>.*?)</td></tr>"
            + "<tr><td>(?<row3number>.*?)</td><td>(?<row3contents>.*?)</td><td>(?<row3ascii>.*?)</td></tr>"
            + "<tr><td>(?<row4number>.*?)</td><td>(?<row4contents>.*?)</td><td>(?<row4ascii>.*?)</td></tr>");

        /// <summary>
        /// Regex matching the category field on a disc page
        /// </summary>
        private Regex categoryRegex = new Regex(@"<tr><th>Category</th><td>(.*?)</td></tr>");

        /// <summary>
        /// Regex matching the comments field on a disc page
        /// </summary>
        private Regex commentsRegex = new Regex(@"<tr><th>Comments</th></tr><tr><td>(.*?)</td></tr>");

        /// <summary>
        /// Regex matching the contents field on a disc page
        /// </summary>
        private Regex contentsRegex = new Regex(@"<tr><th>Contents</th></tr><tr .*?><td>(.*?)</td></tr>");

        /// <summary>
        /// Regex matching individual disc links on a results page
        /// </summary>
        private Regex discRegex = new Regex(@"<a href=""/disc/(\d+)/"">");

        /// <summary>
        /// Regex matching the disc number or letter field on a disc page
        /// </summary>
        private Regex discNumberLetterRegex = new Regex(@"\((.*?)\)");

        /// <summary>
        /// Regex matching the dumpers on a disc page
        /// </summary>
        private Regex dumpersRegex = new Regex(@"<a href=""/discs/dumper/(.*?)/"">");

        /// <summary>
        /// Regex matching the edition field on a disc page
        /// </summary>
        private Regex editionRegex = new Regex(@"<tr><th>Edition</th><td>(.*?)</td></tr>");

        /// <summary>
        /// Regex matching the error count field on a disc page
        /// </summary>
        private Regex errorCountRegex = new Regex(@"<tr><th>Errors count</th><td>(.*?)</td></tr>");

        /// <summary>
        /// Regex matching the foreign title field on a disc page
        /// </summary>
        private Regex foreignTitleRegex = new Regex(@"<h2>(.*?)</h2>");

        /// <summary>
        /// Regex matching the "full match" ID list from a WIP disc page
        /// </summary>
        private Regex fullMatchRegex = new Regex(@"<td class=""static"">full match ids: (.*?)</td>");

        /// <summary>
        /// Regex matching the languages field on a disc page
        /// </summary>
        private Regex languagesRegex = new Regex(@"<img src=""/images/languages/(.*?)\.png"" alt="".*?"" title="".*?"" />\s*");

        /// <summary>
        /// Regex matching the last modified field on a disc page
        /// </summary>
        private Regex lastModifiedRegex = new Regex(@"<tr><th>Last modified</th><td>(.*?)</td></tr>");

        /// <summary>
        /// Regex matching the media field on a disc page
        /// </summary>
        private Regex mediaRegex = new Regex(@"<tr><th>Media</th><td>(.*?)</td></tr>");

        /// <summary>
        /// Regex matching individual WIP disc links on a results page
        /// </summary>
        private Regex newDiscRegex = new Regex(@"<a href=""/newdisc/(\d+)/"">");

        /// <summary>
        /// Regex matching the "partial match" ID list from a WIP disc page
        /// </summary>
        private Regex partialMatchRegex = new Regex(@"<td class=""static"">partial match ids: (.*?)</td>");

        /// <summary>
        /// Regex matching the PVD field on a disc page
        /// </summary>
        private Regex pvdRegex = new Regex(@"<h3>Primary Volume Descriptor (PVD) <img .*?/></h3></td><td .*?></td></tr>"
            + @"<tr><th>Record / Entry</th><th>Contents</th><th>Date</th><th>Time</th><th>GMT</th></tr>"
            + @"<tr><td>Creation</td><td>(?<creationbytes>.*?)</td><td>(?<creationdate>.*?)</td><td>(?<creationtime>.*?)</td><td>(?<creationtimezone>.*?)</td></tr>"
            + @"<tr><td>Modification</td><td>(?<modificationbytes>.*?)</td><td>(?<modificationdate>.*?)</td><td>(?<modificationtime>.*?)</td><td>(?<modificationtimezone>.*?)</td></tr>"
            + @"<tr><td>Expiration</td><td>(?<expirationbytes>.*?)</td><td>(?<expirationdate>.*?)</td><td>(?<expirationtime>.*?)</td><td>(?<expirationtimezone>.*?)</td></tr>"
            + @"<tr><td>Effective</td><td>(?<effectivebytes>.*?)</td><td>(?<effectivedate>.*?)</td><td>(?<effectivetime>.*?)</td><td>(?<effectivetimezone>.*?)</td></tr>");

        /// <summary>
        /// Regex matching the region field on a disc page
        /// </summary>
        private Regex regionRegex = new Regex(@"<tr><th>Region</th><td><a href=""/discs/region/(.*?)/"">");

        /// <summary>
        /// Regex matching a double-layer disc ringcode information
        /// </summary>
        private Regex ringCodeDoubleRegex = new Regex(@""); // Varies based on available fields, like Addtional Mould

        /// <summary>
        /// Regex matching a single-layer disc ringcode information
        /// </summary>
        private Regex ringCodeSingleRegex = new Regex(@""); // Varies based on available fields, like Addtional Mould

        /// <summary>
        /// Regex matching the serial field on a disc page
        /// </summary>
        private Regex serialRegex = new Regex(@"<tr><th>Serial</th><td>(.*?)</td></tr>");

        /// <summary>
        /// Regex matching the system field on a disc page
        /// </summary>
        private Regex systemRegex = new Regex(@"<tr><th>System</th><td><a href=""/discs/system/(.*?)/"">");

        /// <summary>
        /// Regex matching the title field on a disc page
        /// </summary>
        private Regex titleRegex = new Regex(@"<h1>(.*?)</h1>");

        /// <summary>
        /// Regex matching the current nonce token for login
        /// </summary>
        private Regex tokenRegex = new Regex(@"<input type=""hidden"" name=""csrf_token"" value=""(.*?)"" />");

        /// <summary>
        /// Regex matching a single track on a disc page
        /// </summary>
        private Regex trackRegex = new Regex(@"<tr><td>(?<number>.*?)</td><td>(?<type>.*?)</td><td>(?<pregap>.*?)</td><td>(?<length>.*?)</td><td>(?<sectors>.*?)</td><td>(?<size>.*?)</td><td>(?<crc32>.*?)</td><td>(?<md5>.*?)</td><td>(?<sha1>.*?)</td></tr>");

        /// <summary>
        /// Regex matching the track count on a disc page
        /// </summary>
        private Regex trackCountRegex = new Regex(@"<tr><th>Number of tracks</th><td>(.*?)</td></tr>");

        /// <summary>
        /// Regex matching the version field on a disc page
        /// </summary>
        private Regex versionRegex = new Regex(@"<tr><th>Version</th><td>(.*?)</td></tr>");

        /// <summary>
        /// Regex matching the write offset field on a disc page
        /// </summary>
        private Regex writeOffsetRegex = new Regex(@"<tr><th>Write offset</th><td>(.*?)</td></tr>");

        #endregion

        #region URLs

        /// <summary>
        /// Redump disc page URL template
        /// </summary>
        private const string discPageUrl = @"http://redump.org/disc/{0}/";

        /// <summary>
        /// Redump last modified search URL
        /// </summary>
        private const string lastModifiedUrl = @"http://redump.org/discs/sort/modified/dir/desc?page={0}";

        /// <summary>
        /// Redump login page URL
        /// </summary>
        private const string loginUrl = "http://forum.redump.org/login/";

        /// <summary>
        /// Redump CUE pack URL template
        /// </summary>
        private const string packCuesUrl = @"http://redump.org/cues/{0}/";

        /// <summary>
        /// Redump DAT pack URL template
        /// </summary>
        private const string packDatfileUrl = @"http://redump.org/datfile/{0}/";

        /// <summary>
        /// Redump DKEYS pack URL template
        /// </summary>
        private const string packDkeysUrl = @"http://redump.org/dkeys/{0}/";

        /// <summary>
        /// Redump GDI pack URL template
        /// </summary>
        private const string packGdiUrl = @"http://redump.org/gdi/{0}/";

        /// <summary>
        /// Redump KEYS pack URL template
        /// </summary>
        private const string packKeysUrl = @"http://redump.org/keys/{0}/";

        /// <summary>
        /// Redump LSD pack URL template
        /// </summary>
        private const string packLsdUrl = @"http://redump.org/lsd/{0}/";

        /// <summary>
        /// Redump SBI pack URL template
        /// </summary>
        private const string packSbiUrl = @"http://redump.org/sbi/{0}/";

        /// <summary>
        /// Redump quicksearch URL template
        /// </summary>
        private const string quickSearchUrl = @"http://redump.org/discs/quicksearch/{0}/?page={1}";

        /// <summary>
        /// Redump user dumps URL template
        /// </summary>
        private const string userDumpsUrl = @"http://redump.org/discs/dumper/{0}/?page={1}";

        /// <summary>
        /// Redump WIP disc page URL template
        /// </summary>
        private const string wipDiscPageUrl = @"http://redump.org/newdisc/{0}/";

        /// <summary>
        /// Redump WIP dumps queue URL
        /// </summary>
        private const string wipDumpsUrl = @"http://redump.org/discs-wip/";

        #endregion

        #region URL Extensions

        private const string changesExt = "/changes/";
        private const string cueExt = "cue/";
        private const string gdiExt = "gdi/";
        private const string keyExt = "key/";
        private const string lsdExt = "lsd/";
        private const string md5Ext = "md5/";
        private const string sbiExt = "sbi/";
        private const string sfvExt = "sfv/";
        private const string sha1Ext = "sha1/";

        #endregion

        private readonly CookieContainer m_container = new CookieContainer();

        /// <summary>
        /// Determines if user is logged into Redump
        /// </summary>
        public bool LoggedIn { get; set; } = false;

        /// <summary>
        /// Determines if the user is a staff member
        /// </summary>
        public bool IsStaff { get; set; } = false;

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

        #region Features

        /// <summary>
        /// Login to Redump, if possible
        /// </summary>
        /// <param name="username">Redump username</param>
        /// <param name="password">Redump password</param>
        /// <returns>True if the user could be logged in, false otherwise, false otherwise</returns>
        public bool Login(string username, string password)
        {
            // Credentials verification
            if (!string.IsNullOrWhiteSpace(username) && !string.IsNullOrWhiteSpace(password))
            {
                Console.WriteLine("Credentials entered, will attempt Redump login...");
                return false;
            }
            else if (!string.IsNullOrWhiteSpace(username) && string.IsNullOrWhiteSpace(password))
            {
                Console.WriteLine("Only a username was specified, will not attempt Redump login...");
                return false;
            }
            else if (string.IsNullOrWhiteSpace(username))
            {
                Console.WriteLine("No credentials entered, will not attempt Redump login...");
                return false;
            }

            // Get the current token from the login page
            var loginPage = DownloadString(loginUrl);
            string token = this.tokenRegex.Match(loginPage).Groups[1].Value;

            // Construct the login request
            Headers[HttpRequestHeader.ContentType] = "application/x-www-form-urlencoded";
            Encoding = Encoding.UTF8;
            var response = UploadString(loginUrl, $"form_sent=1&redirect_url=&csrf_token={token}&req_username={username}&req_password={password}&save_pass=0");

            if (response.Contains("Incorrect username and/or password."))
            {
                Console.WriteLine("Invalid credentials entered, continuing without logging in...");
                return false;
            }
            else
            {
                Console.WriteLine("Credentials accepted! Logged into Redump...");
            }

            // If the user is a moderator or staff, set accordingly
            if (response.Contains("http://forum.redump.org/forum/9/staff/"))
                IsStaff = true;

            return true;
        }

        /// <summary>
        /// Create a new SubmissionInfo object based on a disc page
        /// </summary>
        /// <param name="id">Redump disc ID to retrieve</param>
        /// <returns>Filled SubmissionInfo object on success, null on error</returns>
        public SubmissionInfo CreateFromId(int id)
        {
            string discData = DownloadSingleSiteID(id);
            if (string.IsNullOrEmpty(discData))
                return null;

            // Create the new object
            SubmissionInfo info = new SubmissionInfo();

            // Added
            var match = addedRegex.Match(discData);
            if (match.Success)
            {
                if (DateTime.TryParse(match.Groups[1].Value, out DateTime added))
                    info.Added = added;
                else
                    info.Added = null;
            }

            // Barcode
            match = barcodeRegex.Match(discData);
            if (match.Success)
                info.CommonDiscInfo.Barcode = WebUtility.HtmlDecode(match.Groups[1].Value);

            // BCA
            match = bcaRegex.Match(discData);
            if (match.Success)
            {
                info.Extras.BCA = WebUtility.HtmlDecode(match.Groups[1].Value)
                       .Replace("<br />", "\n")
                       .Replace("</div>", "");
                info.Extras.BCA = Regex.Replace(info.Extras.BCA, @"<div .*?>", "");
            }

            // Category
            match = categoryRegex.Match(discData);
            if (match.Success)
                info.CommonDiscInfo.Category = Extensions.ToCategory(match.Groups[1].Value);
            else
                info.CommonDiscInfo.Category = DiscCategory.Games;

            // Comments
            match = commentsRegex.Match(discData);
            if (match.Success)
            {
                info.CommonDiscInfo.Comments = WebUtility.HtmlDecode(match.Groups[1].Value)
                    .Replace("<br />", "\n")
                    .Replace("<b>ISBN</b>", "[T:ISBN]") + "\n";
            }

            // Contents
            match = contentsRegex.Match(discData);
            if (match.Success)
            {
                info.CommonDiscInfo.Contents = WebUtility.HtmlDecode(match.Groups[1].Value)
                       .Replace("<br />", "\n")
                       .Replace("</div>", "");
                info.CommonDiscInfo.Contents = Regex.Replace(info.CommonDiscInfo.Contents, @"<div .*?>", "");
            }

            // Dumpers
            var matches = dumpersRegex.Matches(discData);
            if (matches.Count > 0)
            {
                List<string> tempDumpers = new List<string>();
                foreach (Match submatch in matches)
                {
                    tempDumpers.Add(WebUtility.HtmlDecode(submatch.Groups[1].Value));
                }

                info.DumpersAndStatus.Dumpers = tempDumpers.ToArray();
            }

            // Edition
            match = editionRegex.Match(discData);
            if (match.Success)
                info.VersionAndEditions.OtherEditions = WebUtility.HtmlDecode(match.Groups[1].Value);

            // Error Count
            match = errorCountRegex.Match(discData);
            if (match.Success)
                info.CommonDiscInfo.ErrorsCount = match.Groups[1].Value;

            // Foreign Title
            match = foreignTitleRegex.Match(discData);
            if (match.Success)
                info.CommonDiscInfo.ForeignTitleNonLatin = WebUtility.HtmlDecode(match.Groups[1].Value);
            else
                info.CommonDiscInfo.ForeignTitleNonLatin = null;

            // Languages
            matches = languagesRegex.Matches(discData);
            if (matches.Count > 0)
            {
                List<Language?> tempLanguages = new List<Language?>();
                foreach (Match submatch in matches)
                {
                    tempLanguages.Add(Extensions.ToLanguage(submatch.Groups[1].Value));
                }

                info.CommonDiscInfo.Languages = tempLanguages.Where(l => l != null).ToArray();
            }

            // Last Modified
            match = lastModifiedRegex.Match(discData);
            if (match.Success)
            {
                if (DateTime.TryParse(match.Groups[1].Value, out DateTime lastModified))
                    info.LastModified = lastModified;
                else
                    info.LastModified = null;
            }

            // Media
            match = mediaRegex.Match(discData);
            if (match.Success)
                info.CommonDiscInfo.Media = Converters.ToMediaType(match.Groups[1].Value);

            // PVD
            match = pvdRegex.Match(discData);
            if (match.Success)
            {
                info.Extras.PVD = WebUtility.HtmlDecode(match.Groups[1].Value)
                       .Replace("<br />", "\n")
                       .Replace("</div>", "");
                info.Extras.PVD = Regex.Replace(info.Extras.PVD, @"<div .*?>", "");
            }

            // Region
            match = regionRegex.Match(discData);
            if (match.Success)
                info.CommonDiscInfo.Region = Extensions.ToRegion(match.Groups[1].Value);

            // Serial
            match = serialRegex.Match(discData);
            if (match.Success)
                info.CommonDiscInfo.Serial = WebUtility.HtmlDecode(match.Groups[1].Value);

            // System
            match = systemRegex.Match(discData);
            if (match.Success)
                info.CommonDiscInfo.System = Converters.ToKnownSystem(match.Groups[1].Value);

            // Title, Disc Number/Letter, Disc Title
            match = titleRegex.Match(discData);
            if (match.Success)
            {
                string title = WebUtility.HtmlDecode(match.Groups[1].Value);

                // If we have parenthesis, title is everything before the first one
                int firstParenLocation = title.IndexOf(" (");
                if (firstParenLocation >= 0)
                {
                    info.CommonDiscInfo.Title = title.Substring(0, firstParenLocation);
                    var subMatches = discNumberLetterRegex.Match(title);
                    for (int i = 1; i < subMatches.Groups.Count; i++)
                    {
                        string subMatch = subMatches.Groups[i].Value;

                        // Disc number or letter
                        if (subMatch.StartsWith("Disc"))
                            info.CommonDiscInfo.DiscNumberLetter = subMatch.Remove(0, "Disc ".Length);

                        // Disc title
                        else
                            info.CommonDiscInfo.DiscTitle = subMatch;
                    }
                }
                // Otherwise, leave the title as-is
                else
                {
                    info.CommonDiscInfo.Title = title;
                }
            }

            // Tracks
            matches = trackRegex.Matches(discData);
            if (matches.Count > 0)
            {
                List<string> tempTracks = new List<string>();
                foreach (Match submatch in matches)
                {
                    tempTracks.Add(submatch.Groups[1].Value);
                }

                info.TracksAndWriteOffsets.ClrMameProData = string.Join("\n", tempTracks);
            }

            // Track Count
            match = trackCountRegex.Match(discData);
            if (match.Success)
                info.TracksAndWriteOffsets.Cuesheet = match.Groups[1].Value;

            // Version
            match = versionRegex.Match(discData);
            if (match.Success)
                info.VersionAndEditions.Version = WebUtility.HtmlDecode(match.Groups[1].Value);

            // Write Offset
            match = writeOffsetRegex.Match(discData);
            if (match.Success)
                info.TracksAndWriteOffsets.OtherWriteOffsets = WebUtility.HtmlDecode(match.Groups[1].Value);                

            return info;
        }

        /// <summary>
        /// Download the last modified disc pages, until first failure
        /// </summary>
        /// <param name="outDir">Output directory to save data to</param>
        public void DownloadLastModified(string outDir)
        {
            // Keep getting last modified pages until there are none left
            int pageNumber = 1;
            while (true)
            {
                if (!CheckSingleSitePage(string.Format(lastModifiedUrl, pageNumber++), outDir, true))
                    break;
            }
        }

        /// <summary>
        /// Download the last submitted WIP disc pages
        /// </summary>
        /// <param name="outDir">Output directory to save data to</param>
        public void DownloadLastSubmitted(string outDir)
        {
            CheckSingleWIPPage(wipDumpsUrl, outDir, false);
        }

        /// <summary>
        /// Download premade packs
        /// </summary>
        /// <param name="outDir">Output directory to save data to</param>
        /// <param name="useSubfolders">True to use named subfolders to store downloads, false to store directly in the output directory</param>
        public void DownloadPacks(string outDir, bool useSubfolders)
        {
            this.DownloadPacks(packCuesUrl, Extensions.HasCues, "CUEs", outDir, useSubfolders ? "cue" : null);
            this.DownloadPacks(packDatfileUrl, Extensions.HasDat, "DATs", outDir, useSubfolders ? "dat" : null);
            this.DownloadPacks(packDkeysUrl, Extensions.HasDkeys, "Decrypted KEYS", outDir, useSubfolders ? "dkey" : null);
            this.DownloadPacks(packGdiUrl, Extensions.HasGdi, "GDIs", outDir, useSubfolders ? "gdi" : null);
            this.DownloadPacks(packKeysUrl, Extensions.HasKeys, "KEYS", outDir, useSubfolders ? "keys" : null);
            this.DownloadPacks(packLsdUrl, Extensions.HasKeys, "LSD", outDir, useSubfolders ? "lsd" : null);
            this.DownloadPacks(packSbiUrl, Extensions.HasSbi, "SBIs", outDir, useSubfolders ? "sbi" : null);
        }

        /// <summary>
        /// Download premade packs for an individual system
        /// </summary>
        /// <param name="system">RedumpSystem to get all possible packs for</param>
        /// <param name="outDir">Output directory to save data to</param>
        /// <param name="useSubfolders">True to use named subfolders to store downloads, false to store directly in the output directory</param>
        public void DownloadPacksForSystem(RedumpSystem system, string outDir, bool useSubfolders)
        {
            RedumpSystem?[] systemAsArray = new RedumpSystem?[] { system };

            if (Extensions.HasCues.Contains(system))
                this.DownloadPacks(packCuesUrl, systemAsArray, "CUEs", outDir, useSubfolders ? "cue" : null);

            if (Extensions.HasDat.Contains(system))
                this.DownloadPacks(packCuesUrl, Extensions.HasDat, "DATs", outDir, useSubfolders ? "dat" : null);

            if (Extensions.HasDkeys.Contains(system))
                this.DownloadPacks(packCuesUrl, Extensions.HasDkeys, "Decrypted KEYS", outDir, useSubfolders ? "dkey" : null);

            if (Extensions.HasGdi.Contains(system))
                this.DownloadPacks(packCuesUrl, Extensions.HasGdi, "GDIs", outDir, useSubfolders ? "gdi" : null);

            if (Extensions.HasKeys.Contains(system))
                this.DownloadPacks(packCuesUrl, Extensions.HasKeys, "KEYS", outDir, useSubfolders ? "keys" : null);

            if (Extensions.HasLsd.Contains(system))
                this.DownloadPacks(packCuesUrl, Extensions.HasKeys, "LSD", outDir, useSubfolders ? "lsd" : null);

            if (Extensions.HasSbi.Contains(system))
                this.DownloadPacks(packCuesUrl, Extensions.HasSbi, "SBIs", outDir, useSubfolders ? "sbi" : null);
        }

        /// <summary>
        /// Download the disc pages associated with a given quicksearch query
        /// </summary>
        /// <param name="query">Query string to attempt to search for</param>
        public Dictionary<int, string> DownloadSearchResults(string query)
        {
            Dictionary<int, string> resultPages = new Dictionary<int, string>();

            // Strip quotes
            query = query.Trim('"', '\'');

            // Special characters become dashes
            query = query.Replace(' ', '-');
            query = query.Replace('/', '-');
            query = query.Replace('\\', '/');

            // Lowercase is defined per language
            query = query.ToLowerInvariant();

            // Keep getting quicksearch pages until there are none left
            int pageNumber = 1;
            while (true)
            {
                List<int> pageIds = CheckSingleSitePage(string.Format(quickSearchUrl, query, pageNumber++));
                foreach (int pageId in pageIds)
                {
                    resultPages[pageId] = DownloadSingleSiteID(pageId);
                }

                if (pageIds.Count <= 1)
                    break;
            }

            return resultPages;
        }

        /// <summary>
        /// Download the disc pages associated with a given quicksearch query
        /// </summary>
        /// <param name="query">Query string to attempt to search for</param>
        /// <param name="outDir">Output directory to save data to</param>
        public void DownloadSearchResults(string query, string outDir)
        {
            // Strip quotes
            query = query.Trim('"', '\'');

            // Special characters become dashes
            query = query.Replace(' ', '-');
            query = query.Replace('/', '-');
            query = query.Replace('\\', '/');

            // Lowercase is defined per language
            query = query.ToLowerInvariant();

            // Keep getting quicksearch pages until there are none left
            int pageNumber = 1;
            while (true)
            {
                if (!CheckSingleSitePage(string.Format(quickSearchUrl, query, pageNumber++), outDir, false))
                    break;
            }
        }

        /// <summary>
        /// Download the specified range of site disc pages
        /// </summary>
        /// <param name="outDir">Output directory to save data to</param>
        /// <param name="minId">Starting ID for the range</param>
        /// <param name="maxId">Ending ID for the range (inclusive)</param>
        public void DownloadSiteRange(string outDir, int minId = 0, int maxId = 0)
        {
            if (!LoggedIn)
            {
                Console.WriteLine("Site download functionality is only available to Redump members");
                return;
            }

            for (int id = minId; id <= maxId; id++)
            {
                if (DownloadSingleSiteID(id, outDir, true))
                    Thread.Sleep(5 * 1000); // Intentional sleep here so we don't flood the server
            }
        }

        /// <summary>
        /// Download the disc pages associated with the given user
        /// </summary>
        /// <param name="username">Username to check discs for</param>
        /// <param name="outDir">Output directory to save data to</param>
        public void DownloadUser(string username, string outDir)
        {
            if (!LoggedIn)
            {
                Console.WriteLine("User download functionality is only available to Redump members");
                return;
            }

            // Keep getting user pages until there are none left
            int pageNumber = 1;
            while (true)
            {
                if (!CheckSingleSitePage(string.Format(userDumpsUrl, username, pageNumber++), outDir, false))
                    break;
            }
        }

        /// <summary>
        /// Download the specified range of WIP disc pages
        /// </summary>
        /// <param name="wc">RedumpWebClient for all access</param>
        /// <param name="outDir">Output directory to save data to</param>
        /// <param name="minId">Starting ID for the range</param>
        /// <param name="maxId">Ending ID for the range (inclusive)</param>
        public void DownloadWIPRange(string outDir, int minId = 0, int maxId = 0)
        {
            if (!LoggedIn || !IsStaff)
            {
                Console.WriteLine("WIP download functionality is only available to Redump moderators");
                return;
            }

            for (int id = minId; id <= maxId; id++)
            {
                if (DownloadSingleWIPID(id, outDir, true))
                    Thread.Sleep(5 * 1000); // Intentional sleep here so we don't flood the server
            }
        }

        /// <summary>
        /// Fill out an existing SubmissionInfo object based on a disc page
        /// </summary>
        /// <param name="info">Existing SubmissionInfo object to fill</param>
        /// <param name="id">Redump disc ID to retrieve</param>
        public void FillFromId(SubmissionInfo info, int id)
        {
            string discData = DownloadSingleSiteID(id);
            if (string.IsNullOrEmpty(discData))
                return;

            // Title, Disc Number/Letter, Disc Title
            var match = titleRegex.Match(discData);
            if (match.Success)
            {
                string title = WebUtility.HtmlDecode(match.Groups[1].Value);

                // If we have parenthesis, title is everything before the first one
                int firstParenLocation = title.IndexOf(" (");
                if (firstParenLocation >= 0)
                {
                    info.CommonDiscInfo.Title = title.Substring(0, firstParenLocation);
                    var subMatches = discNumberLetterRegex.Match(title);
                    for (int i = 1; i < subMatches.Groups.Count; i++)
                    {
                        string subMatch = subMatches.Groups[i].Value;

                        // Disc number or letter
                        if (subMatch.StartsWith("Disc"))
                            info.CommonDiscInfo.DiscNumberLetter = subMatch.Remove(0, "Disc ".Length);

                        // Disc title
                        else
                            info.CommonDiscInfo.DiscTitle = subMatch;
                    }
                }
                // Otherwise, leave the title as-is
                else
                {
                    info.CommonDiscInfo.Title = title;
                }
            }

            // Foreign Title
            match = foreignTitleRegex.Match(discData);
            if (match.Success)
                info.CommonDiscInfo.ForeignTitleNonLatin = WebUtility.HtmlDecode(match.Groups[1].Value);
            else
                info.CommonDiscInfo.ForeignTitleNonLatin = null;

            // Category
            match = categoryRegex.Match(discData);
            if (match.Success)
                info.CommonDiscInfo.Category = Extensions.ToCategory(match.Groups[1].Value);
            else
                info.CommonDiscInfo.Category = DiscCategory.Games;

            // Region
            match = regionRegex.Match(discData);
            if (match.Success)
                info.CommonDiscInfo.Region = Extensions.ToRegion(match.Groups[1].Value);

            // Languages
            var matches = languagesRegex.Matches(discData);
            if (matches.Count > 0)
            {
                List<Language?> tempLanguages = new List<Language?>();
                foreach (Match submatch in matches)
                    tempLanguages.Add(Extensions.ToLanguage(submatch.Groups[1].Value));

                info.CommonDiscInfo.Languages = tempLanguages.Where(l => l != null).ToArray();
            }

            // Serial
            match = serialRegex.Match(discData);
            if (match.Success)
                info.CommonDiscInfo.Serial = WebUtility.HtmlDecode(match.Groups[1].Value);

            // Error count
            match = errorCountRegex.Match(discData);
            if (match.Success)
            {
                // If the error counts don't match, then use the one from the disc page
                if (!string.IsNullOrEmpty(info.CommonDiscInfo.ErrorsCount) && match.Groups[1].Value != info.CommonDiscInfo.ErrorsCount)
                    info.CommonDiscInfo.ErrorsCount = match.Groups[1].Value;
            }

            // Version
            match = versionRegex.Match(discData);
            if (match.Success)
                info.VersionAndEditions.Version = WebUtility.HtmlDecode(match.Groups[1].Value);

            // Edition
            match = editionRegex.Match(discData);
            if (match.Success)
                info.VersionAndEditions.OtherEditions = WebUtility.HtmlDecode(match.Groups[1].Value);

            // Dumpers
            matches = dumpersRegex.Matches(discData);
            if (matches.Count > 0)
            {
                // Start with any currently listed dumpers
                List<string> tempDumpers = new List<string>();
                if (info.DumpersAndStatus.Dumpers.Length > 0)
                {
                    foreach (string dumper in info.DumpersAndStatus.Dumpers)
                        tempDumpers.Add(dumper);
                }

                foreach (Match submatch in matches)
                    tempDumpers.Add(WebUtility.HtmlDecode(submatch.Groups[1].Value));

                info.DumpersAndStatus.Dumpers = tempDumpers.ToArray();
            }

            // Barcode
            match = barcodeRegex.Match(discData);
            if (match.Success)
                info.CommonDiscInfo.Barcode = WebUtility.HtmlDecode(match.Groups[1].Value);

            // Comments
            match = commentsRegex.Match(discData);
            if (match.Success)
            {
                info.CommonDiscInfo.Comments = WebUtility.HtmlDecode(match.Groups[1].Value)
                    .Replace("<br />", "\n")
                    .Replace("<b>ISBN</b>", "[T:ISBN]") + "\n";
            }

            // Contents
            match = contentsRegex.Match(discData);
            if (match.Success)
            {
                info.CommonDiscInfo.Contents = WebUtility.HtmlDecode(match.Groups[1].Value)
                       .Replace("<br />", "\n")
                       .Replace("</div>", "");
                info.CommonDiscInfo.Contents = Regex.Replace(info.CommonDiscInfo.Contents, @"<div .*?>", "");
            }

            // Added
            match = addedRegex.Match(discData);
            if (match.Success)
            {
                if (DateTime.TryParse(match.Groups[1].Value, out DateTime added))
                    info.Added = added;
                else
                    info.Added = null;
            }

            // Last Modified
            match = lastModifiedRegex.Match(discData);
            if (match.Success)
            {
                if (DateTime.TryParse(match.Groups[1].Value, out DateTime lastModified))
                    info.LastModified = lastModified;
                else
                    info.LastModified = null;
            }
        }

        /// <summary>
        /// List the disc IDs associated with a given quicksearch query
        /// </summary>
        /// <param name="query">Query string to attempt to search for</param>
        /// <returns>All disc IDs for the given query, empty on error</returns>
        public List<int> ListSearchResults(string query)
        {
            List<int> ids = new List<int>();

            // Strip quotes
            query = query.Trim('"', '\'');

            // Special characters become dashes
            query = query.Replace(' ', '-');
            query = query.Replace('/', '-');
            query = query.Replace('\\', '/');

            // Lowercase is defined per language
            query = query.ToLowerInvariant();

            // Keep getting quicksearch pages until there are none left
            int pageNumber = 1;
            while (true)
            {
                List<int> pageIds = CheckSingleSitePage(string.Format(quickSearchUrl, query, pageNumber++));
                ids.AddRange(pageIds);
                if (pageIds.Count <= 1)
                    break;
            }

            return ids;
        }

        /// <summary>
        /// List the disc IDs associated with the given user
        /// </summary>
        /// <param name="username">Username to check discs for</param>
        /// <returns>All disc IDs for the given user, empty on error</returns>
        public List<int> ListUser(string username)
        {
            List<int> ids = new List<int>();

            if (!LoggedIn)
            {
                Console.WriteLine("User download functionality is only available to Redump members");
                return ids;
            }

            // Keep getting user pages until there are none left
            int pageNumber = 1;
            while (true)
            {
                List<int> pageIds = CheckSingleSitePage(string.Format(userDumpsUrl, username, pageNumber++));
                ids.AddRange(pageIds);
                if (pageIds.Count <= 1)
                    break;
            }

            return ids;
        }

        #endregion

        #region Single Page Helpers

        /// <summary>
        /// Process a Redump site page as a list of possible IDs or disc page
        /// </summary>
        /// <param name="url">Base URL to download using</param>
        /// <returns>List of IDs from the page, empty on error</returns>
        private List<int> CheckSingleSitePage(string url)
        {
            List<int> ids = new List<int>();
            var dumpsPage = DownloadString(url);

            // If we have no dumps left
            if (dumpsPage.Contains("No discs found."))
                return ids;

            // If we have a single disc page already
            if (dumpsPage.Contains("<b>Download:</b>"))
            {
                var value = Regex.Match(dumpsPage, @"/disc/(\d+)/sfv/").Groups[1].Value;
                if (int.TryParse(value, out int id))
                    ids.Add(id);

                return ids;
            }

            // Otherwise, traverse each dump on the page
            var matches = discRegex.Matches(dumpsPage);
            foreach (Match match in matches)
            {
                try
                {
                    if (int.TryParse(match.Groups[1].Value, out int value))
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
        /// Process a Redump site page as a list of possible IDs or disc page
        /// </summary>
        /// <param name="url">Base URL to download using</param>
        /// <param name="outDir">Output directory to save data to</param>
        /// <param name="failOnSingle">True to return on first error, false otherwise</param>
        /// <returns>True if the page could be downloaded, false otherwise</returns>
        private bool CheckSingleSitePage(string url, string outDir, bool failOnSingle)
        {
            var dumpsPage = DownloadString(url);

            // If we have no dumps left
            if (dumpsPage.Contains("No discs found."))
                return false;

            // If we have a single disc page already
            if (dumpsPage.Contains("<b>Download:</b>"))
            {
                var value = Regex.Match(dumpsPage, @"/disc/(\d+)/sfv/").Groups[1].Value;
                if (int.TryParse(value, out int id))
                {
                    bool downloaded = DownloadSingleSiteID(id, outDir, false);
                    if (!downloaded && failOnSingle)
                        return false;
                }

                return false;
            }

            // Otherwise, traverse each dump on the page
            var matches = discRegex.Matches(dumpsPage);
            foreach (Match match in matches)
            {
                try
                {
                    if (int.TryParse(match.Groups[1].Value, out int value))
                    {
                        bool downloaded = DownloadSingleSiteID(value, outDir, false);
                        if (!downloaded && failOnSingle)
                            return false;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"An exception has occurred: {ex}");
                    continue;
                }
            }

            return true;
        }

        /// <summary>
        /// Process a Redump WIP page as a list of possible IDs or disc page
        /// </summary>
        /// <param name="wc">RedumpWebClient to access the packs</param>
        /// <returns>List of IDs from the page, empty on error</returns>
        private List<int> CheckSingleWIPPage(string url)
        {
            List<int> ids = new List<int>();
            var dumpsPage = DownloadString(url);

            // If we have no dumps left
            if (dumpsPage.Contains("No discs found."))
                return ids;

            // Otherwise, traverse each dump on the page
            var matches = newDiscRegex.Matches(dumpsPage);
            foreach (Match match in matches)
            {
                try
                {
                    if (int.TryParse(match.Groups[1].Value, out int value))
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
        /// <param name="wc">RedumpWebClient to access the packs</param>
        /// <param name="outDir">Output directory to save data to</param>
        /// <param name="failOnSingle">True to return on first error, false otherwise</param>
        /// <returns>True if the page could be downloaded, false otherwise</returns>
        private bool CheckSingleWIPPage(string url, string outDir, bool failOnSingle)
        {
            var dumpsPage = DownloadString(url);

            // If we have no dumps left
            if (dumpsPage.Contains("No discs found."))
                return false;

            // Otherwise, traverse each dump on the page
            var matches = newDiscRegex.Matches(dumpsPage);
            foreach (Match match in matches)
            {
                try
                {
                    if (int.TryParse(match.Groups[1].Value, out int value))
                    {
                        bool downloaded = DownloadSingleWIPID(value, outDir, false);
                        if (!downloaded && failOnSingle)
                            return false;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"An exception has occurred: {ex}");
                    continue;
                }
            }

            return true;
        }

        #endregion

        #region Download Helpers

        /// <summary>
        /// Download a single pack
        /// </summary>
        /// <param name="url">Base URL to download using</param>
        /// <param name="system">System to download packs for</param>
        /// <returns>Byte array containing the downloaded pack, null on error</returns>
        private byte[] DownloadSinglePack(string url, RedumpSystem? system)
        {
            try
            {
                return DownloadData(string.Format(url, system.ShortName()));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An exception has occurred: {ex}");
                return null;
            }
        }

        /// <summary>
        /// Download a single pack
        /// </summary>
        /// <param name="url">Base URL to download using</param>
        /// <param name="system">System to download packs for</param>
        /// <param name="outDir">Output directory to save data to</param>
        /// <param name="subfolder">Named subfolder for the pack, used optionally</param>
        private void DownloadSinglePack(string url, RedumpSystem? system, string outDir, string subfolder)
        {
            try
            {
                string tempfile = Path.Combine(outDir, "tmp" + Guid.NewGuid().ToString());
                DownloadFile(string.Format(url, system.ShortName()), tempfile);
                MoveOrDelete(tempfile, GetLastFilename(), outDir, subfolder);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An exception has occurred: {ex}");
            }
        }

        /// <summary>
        /// Download an individual site ID data, if possible
        /// </summary>
        /// <param name="id">Redump disc ID to retrieve</param>
        /// <returns>String containing the page contents if successful, null on error</returns>
        private string DownloadSingleSiteID(int id)
        {
            string paddedId = id.ToString().PadLeft(5, '0');
            Console.WriteLine($"Processing ID: {paddedId}");
            try
            {
                string discPage = DownloadString(string.Format(discPageUrl, +id));
                if (discPage.Contains($"Disc with ID \"{id}\" doesn't exist"))
                {
                    Console.WriteLine($"ID {paddedId} could not be found!");
                    return null;
                }

                Console.WriteLine($"ID {paddedId} has been successfully downloaded");
                return discPage;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An exception has occurred: {ex}");
                return null;
            }
        }

        /// <summary>
        /// Download an individual site ID data, if possible
        /// </summary>
        /// <param name="id">Redump disc ID to retrieve</param>
        /// <param name="outDir">Output directory to save data to</param>
        /// <param name="rename">True to rename deleted entries, false otherwise</param>
        /// <returns>True if all data was downloaded, false otherwise</returns>
        private bool DownloadSingleSiteID(int id, string outDir, bool rename)
        {
            string paddedId = id.ToString().PadLeft(5, '0');
            string paddedIdDir = Path.Combine(outDir, paddedId);
            Console.WriteLine($"Processing ID: {paddedId}");
            try
            {
                string discPage = DownloadString(string.Format(discPageUrl, +id));
                if (discPage.Contains($"Disc with ID \"{id}\" doesn't exist"))
                {
                    try
                    {
                        if (rename)
                        {
                            if (Directory.Exists(paddedIdDir) && rename)
                                Directory.Move(paddedIdDir, paddedIdDir + "-deleted");
                            else
                                Directory.CreateDirectory(paddedIdDir + "-deleted");
                        }
                    }
                    catch { }

                    Console.WriteLine($"ID {paddedId} could not be found!");
                    return false;
                }

                // Check if the page has been updated since the last time it was downloaded, if possible
                if (File.Exists(Path.Combine(paddedIdDir, "disc.html")))
                {
                    // Read in the cached file
                    var oldDiscPage = File.ReadAllText(Path.Combine(paddedIdDir, "disc.html"));

                    // Check for the last modified date in both pages
                    var oldResult = lastModifiedRegex.Match(oldDiscPage);
                    var newResult = lastModifiedRegex.Match(discPage);

                    // If both pages contain the same modified date, skip it
                    if (oldResult.Success && newResult.Success && oldResult.Groups[1].Value == newResult.Groups[1].Value)
                    {
                        Console.WriteLine($"ID {paddedId} has not been changed since last download");
                        return false;
                    }

                    // If neither page contains a modified date, skip it
                    else if (!oldResult.Success && !newResult.Success)
                    {
                        Console.WriteLine($"ID {paddedId} has not been changed since last download");
                        return false;
                    }
                }

                // Create ID subdirectory
                Directory.CreateDirectory(paddedIdDir);

                // HTML
                using (var discStreamWriter = File.CreateText(Path.Combine(paddedIdDir, "disc.html")))
                {
                    discStreamWriter.Write(discPage);
                }

                DownloadFile(string.Format(discPageUrl, +id) + changesExt, Path.Combine(paddedIdDir, "changes.html"));

                // CUE
                if (discPage.Contains($"<a href=\"/disc/{id}/cue/\""))
                    DownloadFile(string.Format(discPageUrl, +id) + cueExt, Path.Combine(paddedIdDir, paddedId + ".cue"));

                // GDI
                if (discPage.Contains($"<a href=\"/disc/{id}/gdi/\""))
                    DownloadFile(string.Format(discPageUrl, +id) + gdiExt, Path.Combine(paddedIdDir, paddedId + ".gdi"));

                // KEYS
                if (discPage.Contains($"<a href=\"/disc/{id}/key/\""))
                    DownloadFile(string.Format(discPageUrl, +id) + keyExt, Path.Combine(paddedIdDir, paddedId + ".key"));

                // LSD
                if (discPage.Contains($"<a href=\"/disc/{id}/lsd/\""))
                    DownloadFile(string.Format(discPageUrl, +id) + lsdExt, Path.Combine(paddedIdDir, paddedId + ".lsd"));

                // MD5
                if (discPage.Contains($"<a href=\"/disc/{id}/md5/\""))
                    DownloadFile(string.Format(discPageUrl, +id) + md5Ext, Path.Combine(paddedIdDir, paddedId + ".md5"));

                // SBI
                if (discPage.Contains($"<a href=\"/disc/{id}/sbi/\""))
                    DownloadFile(string.Format(discPageUrl, +id) + sbiExt, Path.Combine(paddedIdDir, paddedId + ".sbi"));

                // SFV
                if (discPage.Contains($"<a href=\"/disc/{id}/sfv/\""))
                    DownloadFile(string.Format(discPageUrl, +id) + sfvExt, Path.Combine(paddedIdDir, paddedId + ".sfv"));

                // SHA1
                if (discPage.Contains($"<a href=\"/disc/{id}/sha1/\""))
                    DownloadFile(string.Format(discPageUrl, +id) + sha1Ext, Path.Combine(paddedIdDir, paddedId + ".sha1"));

                Console.WriteLine($"ID {paddedId} has been successfully downloaded");
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An exception has occurred: {ex}");
                return false;
            }
        }

        /// <summary>
        /// Download an individual WIP ID data, if possible
        /// </summary>
        /// <param name="id">Redump WIP disc ID to retrieve</param>
        /// <returns>String containing the page contents if successful, null on error</returns>
        private string DownloadSingleWIPID(int id)
        {
            string paddedId = id.ToString().PadLeft(5, '0');
            Console.WriteLine($"Processing ID: {paddedId}");
            try
            {
                string discPage = DownloadString(string.Format(wipDiscPageUrl, +id));
                if (discPage.Contains($"System \"{id}\" doesn't exist"))
                {
                    Console.WriteLine($"ID {paddedId} could not be found!");
                    return null;
                }

                Console.WriteLine($"ID {paddedId} has been successfully downloaded");
                return discPage;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An exception has occurred: {ex}");
                return null;
            }
        }

        /// <summary>
        /// Download an individual WIP ID data, if possible
        /// </summary>
        /// <param name="id">Redump WIP disc ID to retrieve</param>
        /// <param name="outDir">Output directory to save data to</param>
        /// <param name="rename">True to rename deleted entries, false otherwise</param>
        /// <returns>True if all data was downloaded, false otherwise</returns>
        private bool DownloadSingleWIPID(int id, string outDir, bool rename)
        {
            string paddedId = id.ToString().PadLeft(5, '0');
            string paddedIdDir = Path.Combine(outDir, paddedId);
            Console.WriteLine($"Processing ID: {paddedId}");
            try
            {
                string discPage = DownloadString(string.Format(wipDiscPageUrl, +id));
                if (discPage.Contains($"System \"{id}\" doesn't exist"))
                {
                    try
                    {
                        if (rename)
                        {
                            if (Directory.Exists(paddedIdDir) && rename)
                                Directory.Move(paddedIdDir, paddedIdDir + "-deleted");
                            else
                                Directory.CreateDirectory(paddedIdDir + "-deleted");
                        }
                    }
                    catch { }

                    Console.WriteLine($"ID {paddedId} could not be found!");
                    return false;
                }

                // Check if the page has been updated since the last time it was downloaded, if possible
                if (File.Exists(Path.Combine(paddedIdDir, "disc.html")))
                {
                    // Read in the cached file
                    var oldDiscPage = File.ReadAllText(Path.Combine(paddedIdDir, "disc.html"));

                    // Check for the full match ID in both pages
                    var oldResult = fullMatchRegex.Match(oldDiscPage);
                    var newResult = fullMatchRegex.Match(discPage);

                    // If both pages contain the same ID, skip it
                    if (oldResult.Success && newResult.Success && oldResult.Groups[1].Value == newResult.Groups[1].Value)
                    {
                        Console.WriteLine($"ID {paddedId} has not been changed since last download");
                        return false;
                    }

                    // If neither page contains an ID, skip it
                    else if (!oldResult.Success && !newResult.Success)
                    {
                        Console.WriteLine($"ID {paddedId} has not been changed since last download");
                        return false;
                    }
                }

                // Create ID subdirectory
                Directory.CreateDirectory(paddedIdDir);

                // HTML
                using (var discStreamWriter = File.CreateText(Path.Combine(paddedIdDir, "disc.html")))
                {
                    discStreamWriter.Write(discPage);
                }

                Console.WriteLine($"ID {paddedId} has been successfully downloaded");
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An exception has occurred: {ex}");
                return false;
            }
        }

        #endregion

        #region Internal Helpers

        /// <summary>
        /// Download a set of packs
        /// </summary>
        /// <param name="url">Base URL to download using</param>
        /// <param name="system">Systems to download packs for</param>
        /// <param name="title">Name of the pack that is downloading</param>
        private Dictionary<RedumpSystem?, byte[]> DownloadPacks(string url, RedumpSystem?[] systems, string title)
        {
            var packsDictionary = new Dictionary<RedumpSystem?, byte[]>();

            // If we didn't have credentials
            if (!LoggedIn)
                systems = systems.Where(s => !Extensions.BannedSystems.Contains(s)).ToArray();

            Console.WriteLine($"Downloading {title}");
            foreach (var system in systems)
            {
                Console.Write($"\r{system.LongName()}{new string(' ', Console.BufferWidth - system.LongName().Length - 1)}");
                byte[] pack = DownloadSinglePack(url, system);
                if (pack != null)
                    packsDictionary.Add(system, pack);
            }

            Console.Write($"\rComplete!{new string(' ', Console.BufferWidth - 10)}");
            Console.WriteLine();

            return packsDictionary;
        }

        /// <summary>
        /// Download a set of packs
        /// </summary>
        /// <param name="url">Base URL to download using</param>
        /// <param name="system">Systems to download packs for</param>
        /// <param name="title">Name of the pack that is downloading</param>
        /// <param name="outDir">Output directory to save data to</param>
        /// <param name="subfolder">Named subfolder for the pack, used optionally</param>
        private void DownloadPacks(string url, RedumpSystem?[] systems, string title, string outDir, string subfolder)
        {
            // If we didn't have credentials
            if (!LoggedIn)
                systems = systems.Where(s => !Extensions.BannedSystems.Contains(s)).ToArray();

            Console.WriteLine($"Downloading {title}");
            foreach (var system in systems)
            {
                Console.Write($"\r{system.LongName()}{new string(' ', Console.BufferWidth - system.LongName().Length - 1)}");
                DownloadSinglePack(url, system, outDir, subfolder);
            }

            Console.Write($"\rComplete!{new string(' ', Console.BufferWidth - 10)}");
            Console.WriteLine();
        }

        /// <summary>
        /// Move a tempfile to a new name unless it aleady exists, in which case, delete the tempfile
        /// </summary>
        /// <param name="tempfile">Path to existing temporary file</param>
        /// <param name="newfile">Path to new output file</param>
        /// <param name="outDir">Output directory to save data to</param>
        /// <param name="subfolder">Optional subfolder to append to the path</param>
        private void MoveOrDelete(string tempfile, string newfile, string outDir, string subfolder)
        {
            if (!string.IsNullOrWhiteSpace(newfile))
            {
                if (!string.IsNullOrWhiteSpace(subfolder))
                {
                    if (!Directory.Exists(Path.Combine(outDir, subfolder)))
                        Directory.CreateDirectory(Path.Combine(outDir, subfolder));

                    newfile = Path.Combine(subfolder, newfile);
                }

                if (File.Exists(Path.Combine(outDir, newfile)))
                    File.Delete(tempfile);
                else
                    File.Move(tempfile, Path.Combine(outDir, newfile));
            }
            else
                File.Delete(tempfile);
        }

        #endregion
    }
}
