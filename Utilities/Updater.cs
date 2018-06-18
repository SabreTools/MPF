using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace DICUI.Utilities
{
    class Version
    {
        private readonly string identifier;
        private readonly GitHubRelease release;

        public Version(string identifier) : this(identifier, null)
        {

        }

        public Version(string identifier, GitHubRelease release)
        {
            this.identifier = identifier;
            this.release = release;
        }

        public bool IsNewerThan(Version reference)
        {
            return reference.identifier != this.identifier;
        }
    }

    class GitHubRelease
    {
        public string url;
        public string html_url;
        public string tag_name;
        public string zipball_url;
    }

    class Updater
    {
        private const string GITHUB_LAST_RELEASE_URL = "https://api.github.com/repos/reignstumble/DICUI/releases/latest";

        private static Version GetCurrentVersion()
        {
            return new Version("1.06");
        }

        private static Version GetLatestVersion()
        {
            using (WebClient wc = new WebClient())
            {
                var json = wc.DownloadString(GITHUB_LAST_RELEASE_URL);

                GitHubRelease latestRelease = JsonConvert.DeserializeObject<GitHubRelease>(json);

                Version latestVersion = new Version(latestRelease.tag_name);

                return latestVersion;
            }
        }

    }
}
