using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using RedumpLib.Data;

namespace RedumpLib.Web
{
    // https://stackoverflow.com/questions/1777221/using-cookiecontainer-with-webclient-class
    public class RedumpWebClient : WebClient
    {
        private readonly CookieContainer m_container = new CookieContainer();

        /// <summary>
        /// Determines if user is logged into Redump
        /// </summary>
        public bool LoggedIn { get; private set; } = false;

        /// <summary>
        /// Determines if the user is a staff member
        /// </summary>
        public bool IsStaff { get; private set; } = false;

        /// <summary>
        /// Get the last downloaded filename, if possible
        /// </summary>
        /// <returns></returns>
        public string GetLastFilename()
        {
            // If the response headers are null or empty
            if (ResponseHeaders == null || ResponseHeaders.Count == 0)
                return null;
            
            // If we don't have the response header we care about
            string headerValue = ResponseHeaders.Get("Content-Disposition");
            if (string.IsNullOrWhiteSpace(headerValue))
                return null;

            // Extract the filename from the value
            return headerValue.Substring(headerValue.IndexOf("filename=") + 9).Replace("\"", "");
        }

        /// <inheritdoc/>
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
        /// Login to Redump, if possible
        /// </summary>
        /// <param name="username">Redump username</param>
        /// <param name="password">Redump password</param>
        /// <returns>True if the user could be logged in, false otherwise, null on error</returns>
        public bool? Login(string username, string password)
        {
            // Credentials verification
            if (!string.IsNullOrWhiteSpace(username) && !string.IsNullOrWhiteSpace(password))
            {
                Console.WriteLine("Credentials entered, will attempt Redump login...");
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

            // HTTP encode the password
            password = WebUtility.UrlEncode(password);

            // Attempt to login up to 3 times
            for (int i = 0; i < 3; i++)
            {
                try
                {
                    // Get the current token from the login page
                    var loginPage = DownloadString(Constants.LoginUrl);
                    string token = Constants.TokenRegex.Match(loginPage).Groups[1].Value;

                    // Construct the login request
                    Headers[HttpRequestHeader.ContentType] = "application/x-www-form-urlencoded";
                    Encoding = Encoding.UTF8;
                    var response = UploadString(Constants.LoginUrl, $"form_sent=1&redirect_url=&csrf_token={token}&req_username={username}&req_password={password}&save_pass=0");

                    if (response.Contains("Incorrect username and/or password."))
                    {
                        Console.WriteLine("Invalid credentials entered, continuing without logging in...");
                        return false;
                    }

                    // The user was able to be logged in
                    Console.WriteLine("Credentials accepted! Logged into Redump...");
                    LoggedIn = true;

                    // If the user is a moderator or staff, set accordingly
                    if (response.Contains("http://forum.redump.org/forum/9/staff/"))
                        IsStaff = true;

                    return true;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"An exception occurred while trying to log in on attempt {i}: {ex}");
                }
            }

            Console.WriteLine("Could not login to Redump in 3 attempts, continuing without logging in...");
            return false;
        }

        #region Single Page Helpers

        /// <summary>
        /// Process a Redump site page as a list of possible IDs or disc page
        /// </summary>
        /// <param name="url">Base URL to download using</param>
        /// <returns>List of IDs from the page, empty on error</returns>
        public List<int> CheckSingleSitePage(string url)
        {
            List<int> ids = new List<int>();
            string dumpsPage = string.Empty;

            // Try up to 3 times to retrieve the data
            for (int i = 0; i < 3; i++)
            {
                try
                {
                    dumpsPage = DownloadString(url);
                    break;
                }
                catch { }
            }

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
            var matches = Constants.DiscRegex.Matches(dumpsPage);
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
        public bool CheckSingleSitePage(string url, string outDir, bool failOnSingle)
        {
            string dumpsPage = string.Empty;

            // Try up to 3 times to retrieve the data
            for (int i = 0; i < 3; i++)
            {
                try
                {
                    dumpsPage = DownloadString(url);
                    break;
                }
                catch { }
            }

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
            var matches = Constants.DiscRegex.Matches(dumpsPage);
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
        public List<int> CheckSingleWIPPage(string url)
        {
            List<int> ids = new List<int>();
            string dumpsPage = string.Empty;

            // Try up to 3 times to retrieve the data
            for (int i = 0; i < 3; i++)
            {
                try
                {
                    dumpsPage = DownloadString(url);
                    break;
                }
                catch { }
            }

            // If we have no dumps left
            if (dumpsPage.Contains("No discs found."))
                return ids;

            // Otherwise, traverse each dump on the page
            var matches = Constants.NewDiscRegex.Matches(dumpsPage);
            foreach (Match match in matches)
            {
                try
                {
                    if (int.TryParse(match.Groups[2].Value, out int value))
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
        public bool CheckSingleWIPPage(string url, string outDir, bool failOnSingle)
        {
            string dumpsPage = string.Empty;

            // Try up to 3 times to retrieve the data
            for (int i = 0; i < 3; i++)
            {
                try
                {
                    dumpsPage = DownloadString(url);
                    break;
                }
                catch { }
            }

            // If we have no dumps left
            if (dumpsPage.Contains("No discs found."))
                return false;

            // Otherwise, traverse each dump on the page
            var matches = Constants.NewDiscRegex.Matches(dumpsPage);
            foreach (Match match in matches)
            {
                try
                {
                    if (int.TryParse(match.Groups[2].Value, out int value))
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
        public byte[] DownloadSinglePack(string url, RedumpSystem? system)
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
        public void DownloadSinglePack(string url, RedumpSystem? system, string outDir, string subfolder)
        {
            try
            {
                // If no output directory is defined, use the current directory instead
                if (string.IsNullOrWhiteSpace(outDir))
                    outDir = Environment.CurrentDirectory;

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
        public string DownloadSingleSiteID(int id)
        {
            string paddedId = id.ToString().PadLeft(5, '0');
            Console.WriteLine($"Processing ID: {paddedId}");
            try
            {
                string discPage = string.Empty;

                // Try up to 3 times to retrieve the data
                for (int i = 0; i < 3; i++)
                {
                    try
                    {
                        discPage = DownloadString(string.Format(Constants.DiscPageUrl, +id));
                        break;
                    }
                    catch { }
                }

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
        public bool DownloadSingleSiteID(int id, string outDir, bool rename)
        {
            // If no output directory is defined, use the current directory instead
            if (string.IsNullOrWhiteSpace(outDir))
                outDir = Environment.CurrentDirectory;

            string paddedId = id.ToString().PadLeft(5, '0');
            string paddedIdDir = Path.Combine(outDir, paddedId);
            Console.WriteLine($"Processing ID: {paddedId}");
            try
            {
                string discPage = string.Empty;

                // Try up to 3 times to retrieve the data
                for (int i = 0; i < 3; i++)
                {
                    try
                    {
                        discPage = DownloadString(string.Format(Constants.DiscPageUrl, +id));
                        break;
                    }
                    catch { }
                }

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
                    var oldResult = Constants.LastModifiedRegex.Match(oldDiscPage);
                    var newResult = Constants.LastModifiedRegex.Match(discPage);

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

                // View Edit History
                if (discPage.Contains($"<a href=\"/disc/{id}/changes/\""))
                    DownloadFile(string.Format(Constants.DiscPageUrl, +id) + Constants.ChangesExt, Path.Combine(paddedIdDir, "changes.html"));
                
                // CUE
                if (discPage.Contains($"<a href=\"/disc/{id}/cue/\""))
                    DownloadFile(string.Format(Constants.DiscPageUrl, +id) + Constants.CueExt, Path.Combine(paddedIdDir, paddedId + ".cue"));

                // Edit disc
                if (discPage.Contains($"<a href=\"/disc/{id}/edit/\""))
                    DownloadFile(string.Format(Constants.DiscPageUrl, +id) + Constants.EditExt, Path.Combine(paddedIdDir, "edit.html"));

                // GDI
                if (discPage.Contains($"<a href=\"/disc/{id}/gdi/\""))
                    DownloadFile(string.Format(Constants.DiscPageUrl, +id) + Constants.GdiExt, Path.Combine(paddedIdDir, paddedId + ".gdi"));

                // KEYS
                if (discPage.Contains($"<a href=\"/disc/{id}/key/\""))
                    DownloadFile(string.Format(Constants.DiscPageUrl, +id) + Constants.KeyExt, Path.Combine(paddedIdDir, paddedId + ".key"));

                // LSD
                if (discPage.Contains($"<a href=\"/disc/{id}/lsd/\""))
                    DownloadFile(string.Format(Constants.DiscPageUrl, +id) + Constants.LsdExt, Path.Combine(paddedIdDir, paddedId + ".lsd"));

                // MD5
                if (discPage.Contains($"<a href=\"/disc/{id}/md5/\""))
                    DownloadFile(string.Format(Constants.DiscPageUrl, +id) + Constants.Md5Ext, Path.Combine(paddedIdDir, paddedId + ".md5"));

                // Review WIP entry
                if (Constants.NewDiscRegex.IsMatch(discPage))
                {
                    var match = Constants.NewDiscRegex.Match(discPage);
                    DownloadFile(string.Format(Constants.WipDiscPageUrl, match.Groups[2].Value), Path.Combine(paddedIdDir, "newdisc.html"));
                }

                // SBI
                if (discPage.Contains($"<a href=\"/disc/{id}/sbi/\""))
                    DownloadFile(string.Format(Constants.DiscPageUrl, +id) + Constants.SbiExt, Path.Combine(paddedIdDir, paddedId + ".sbi"));

                // SFV
                if (discPage.Contains($"<a href=\"/disc/{id}/sfv/\""))
                    DownloadFile(string.Format(Constants.DiscPageUrl, +id) + Constants.SfvExt, Path.Combine(paddedIdDir, paddedId + ".sfv"));

                // SHA1
                if (discPage.Contains($"<a href=\"/disc/{id}/sha1/\""))
                    DownloadFile(string.Format(Constants.DiscPageUrl, +id) + Constants.Sha1Ext, Path.Combine(paddedIdDir, paddedId + ".sha1"));

                // HTML (Last in case of errors)
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

        /// <summary>
        /// Download an individual WIP ID data, if possible
        /// </summary>
        /// <param name="id">Redump WIP disc ID to retrieve</param>
        /// <returns>String containing the page contents if successful, null on error</returns>
        public string DownloadSingleWIPID(int id)
        {
            string paddedId = id.ToString().PadLeft(5, '0');
            Console.WriteLine($"Processing ID: {paddedId}");
            try
            {
                string discPage = string.Empty;

                // Try up to 3 times to retrieve the data
                for (int i = 0; i < 3; i++)
                {
                    try
                    {
                        discPage = DownloadString(string.Format(Constants.WipDiscPageUrl, +id));
                        break;
                    }
                    catch { }
                }

                if (discPage.Contains($"WIP disc with ID \"{id}\" doesn't exist"))
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
        public bool DownloadSingleWIPID(int id, string outDir, bool rename)
        {
            // If no output directory is defined, use the current directory instead
            if (string.IsNullOrWhiteSpace(outDir))
                outDir = Environment.CurrentDirectory;

            string paddedId = id.ToString().PadLeft(5, '0');
            string paddedIdDir = Path.Combine(outDir, paddedId);
            Console.WriteLine($"Processing ID: {paddedId}");
            try
            {
                string discPage = string.Empty;

                // Try up to 3 times to retrieve the data
                for (int i = 0; i < 3; i++)
                {
                    try
                    {
                        discPage = DownloadString(string.Format(Constants.WipDiscPageUrl, +id));
                        break;
                    }
                    catch { }
                }

                if (discPage.Contains($"WIP disc with ID \"{id}\" doesn't exist"))
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
                    var oldResult = Constants.FullMatchRegex.Match(oldDiscPage);
                    var newResult = Constants.FullMatchRegex.Match(discPage);

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

        #region Helpers

        /// <summary>
        /// Download a set of packs
        /// </summary>
        /// <param name="url">Base URL to download using</param>
        /// <param name="system">Systems to download packs for</param>
        /// <param name="title">Name of the pack that is downloading</param>
        public Dictionary<RedumpSystem, byte[]> DownloadPacks(string url, RedumpSystem?[] systems, string title)
        {
            var packsDictionary = new Dictionary<RedumpSystem, byte[]>();

            Console.WriteLine($"Downloading {title}");
            foreach (var system in systems)
            {
                // If the system is invalid, we can't do anything
                if (system == null || !system.IsAvailable())
                    continue;

                // If we didn't have credentials
                if (!LoggedIn && system.IsBanned())
                    continue;
                
                // If the system is unknown, we can't do anything
                string longName = system.LongName();
                if (string.IsNullOrWhiteSpace(longName))
                    continue;

                Console.Write($"\r{longName}{new string(' ', Console.BufferWidth - longName.Length - 1)}");
                byte[] pack = DownloadSinglePack(url, system);
                if (pack != null)
                    packsDictionary.Add(system.Value, pack);
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
        public void DownloadPacks(string url, RedumpSystem?[] systems, string title, string outDir, string subfolder)
        {
            Console.WriteLine($"Downloading {title}");
            foreach (var system in systems)
            {
                // If the system is invalid, we can't do anything
                if (system == null || !system.IsAvailable())
                    continue;

                // If we didn't have credentials
                if (!LoggedIn && system.IsBanned())
                    continue;

                // If the system is unknown, we can't do anything
                string longName = system.LongName();
                if (string.IsNullOrWhiteSpace(longName))
                    continue;

                Console.Write($"\r{longName}{new string(' ', Console.BufferWidth - longName.Length - 1)}");
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
