using System;
using System.Collections.Generic;
using System.IO;

/// <remarks>
/// Information sourced from http://web.archive.org/web/20070221154246/http://www.goldenhawk.com/download/cdrwin.pdf
/// </remarks>
namespace DICUI.CueSheets
{
    /// <summary>
    /// Represents a single cuesheet
    /// </summary>
    public class CueSheet
    {
        /// <summary>
        /// CATALOG
        /// </summary>
        public string Catalog { get; set; }

        /// <summary>
        /// CDTEXTFILE
        /// </summary>
        public string CdTextFile { get; set; }

        /// <summary>
        /// PERFORMER
        /// </summary>
        public string Performer { get; set; }

        /// <summary>
        /// SONGWRITER
        /// </summary>
        public string Songwriter { get; set; }

        /// <summary>
        /// TITLE
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// List of FILE in cuesheet
        /// </summary>
        public List<CueFile> Files { get; set; }

        /// <summary>
        /// Create an empty cuesheet
        /// </summary>
        public CueSheet()
        {
        }

        /// <summary>
        /// Create a cuesheet from a file, if possible
        /// </summary>
        /// <param name="filename"></param>
        public CueSheet(string filename)
        {
            // Check that the file exists
            if (!File.Exists(filename))
                return;

            // Check the extension
            string ext = Path.GetExtension(filename).TrimStart('.');
            if (!string.Equals(ext, "cue", StringComparison.OrdinalIgnoreCase)
                && !string.Equals(ext, "txt", StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            // Open the file and begin reading
            using (var sr = new StreamReader(filename))
            {
                while (!sr.EndOfStream)
                {
                    string line = sr.ReadLine().Trim();
                    string[] splitLine = line.Split(' ');

                    // If we have an empty line, we skip
                    if (string.IsNullOrWhiteSpace(line))
                        continue;

                    switch (splitLine[0])
                    {
                        // Read comments
                        case "REM":
                            // We ignore all comments for now
                            break;

                        // Read MCN
                        case "CATALOG":
                            if (splitLine.Length < 2)
                                continue; // TODO: Make this throw an exception

                            this.Catalog = splitLine[1];
                            break;

                        // Read external CD-Text file path
                        case "CDTEXTFILE":
                            if (splitLine.Length < 2)
                                continue; // TODO: Make this throw an exception

                            this.CdTextFile = splitLine[1];
                            break;

                        // Read CD-Text enhanced performer
                        case "PERFORMER":
                            if (splitLine.Length < 2)
                                continue; // TODO: Make this throw an exception

                            this.Performer = splitLine[1];
                            break;

                        // Read CD-Text enhanced songwriter
                        case "SONGWRITER":
                            if (splitLine.Length < 2)
                                continue; // TODO: Make this throw an exception

                            this.Songwriter = splitLine[1];
                            break;

                        // Read CD-Text enhanced title
                        case "TITLE":
                            if (splitLine.Length < 2)
                                continue; // TODO: Make this throw an exception

                            this.Title = splitLine[1];
                            break;

                        // Read file information
                        case "FILE":
                            if (splitLine.Length < 3)
                                continue; // TODO: Make this throw an exception

                            if (this.Files == null)
                                this.Files = new List<CueFile>();

                            var file = new CueFile(splitLine[1], splitLine[2], sr);
                            if (file == default)
                                continue; // TODO: Make this throw an exception

                            this.Files.Add(file);
                            break;
                    }
                }
            }
        }
    }
}
