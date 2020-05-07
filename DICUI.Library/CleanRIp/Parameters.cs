using System.IO;
using DICUI.Data;
using DICUI.Utilities;

namespace DICUI.CleanRip
{
    /// <summary>
    /// Represents a generic set of CleanRip parameters
    /// </summary>
    public class Parameters : BaseParameters
    {
        /// <summary>
        /// Populate a Parameters object from a param string
        /// </summary>
        /// <param name="parameters">String possibly representing a set of parameters</param>
        public Parameters(string parameters)
            : base(parameters)
        {
            this.InternalProgram = InternalProgram.CleanRip;
        }

        /// <summary>
        /// Generate parameters based on a set of known inputs
        /// </summary>
        /// <param name="system">KnownSystem value to use</param>
        /// <param name="type">MediaType value to use</param>
        /// <param name="driveLetter">Drive letter to use</param>
        /// <param name="filename">Filename to use</param>
        /// <param name="driveSpeed">Drive speed to use</param>
        /// <param name="paranoid">Enable paranoid mode (safer dumping)</param>
        /// <param name="quietMode">Enable quiet mode (no beeps)</param>
        /// <param name="retryCount">User-defined reread count</param>
        public Parameters(KnownSystem? system, MediaType? type, char driveLetter, string filename, int? driveSpeed, bool paranoid, bool quietMode, int retryCount)
            : base(system, type, driveLetter, filename, driveSpeed, paranoid, quietMode, retryCount)
        {
        }

        /// <summary>
        /// Blindly generate a parameter string based on the inputs
        /// </summary>
        /// <returns>Correctly formatted parameter string, null on error</returns>
        public override string GenerateParameters() => null;

        /// <summary>
        /// Get the input path from the implementation
        /// </summary>
        /// <returns>String representing the path, null on error</returns>
        public override string InputPath() => null;

        /// <summary>
        /// Get the output path from the implementation
        /// </summary>
        /// <returns>String representing the path, null on error</returns>
        public override string OutputPath() => null;

        /// <summary>
        /// Get the processing speed from the implementation
        /// </summary>
        /// <returns>int? representing the speed, null on error</returns>
        public override int? GetSpeed() => null;

        /// <summary>
        /// Set the processing speed int the implementation
        /// </summary>
        /// <param name="speed">int? representing the speed</param>
        public override void SetSpeed(int? speed) { }

        /// <summary>
        /// Get the MediaType from the current set of parameters
        /// </summary>
        /// <returns>MediaType value if successful, null on error</returns>
        public override MediaType? GetMediaType() => null;

        /// <summary>
        /// Gets if the current command is considered a dumping command or not
        /// </summary>
        /// <returns>True if it's a dumping command, false otherwise</returns>
        public override bool IsDumpingCommand() => true;

        /// <summary>
        /// Reset all special variables to have default values
        /// </summary>
        protected override void ResetValues() { }

        /// <summary>
        /// Set default parameters for a given system and media type
        /// </summary>
        /// <param name="system">KnownSystem value to use</param>
        /// <param name="type">MediaType value to use</param>
        /// <param name="driveLetter">Drive letter to use</param>
        /// <param name="filename">Filename to use</param>
        /// <param name="driveSpeed">Drive speed to use</param>
        /// <param name="paranoid">Enable paranoid mode (safer dumping)</param>
        /// <param name="retryCount">User-defined reread count</param>
        protected override void SetDefaultParameters(
            KnownSystem? system,
            MediaType? type,
            char driveLetter,
            string filename,
            int? driveSpeed,
            bool paranoid,
            int retryCount)
        {
        }

        /// <summary>
        /// Scan a possible parameter string and populate whatever possible
        /// </summary>
        /// <param name="parameters">String possibly representing parameters</param>
        /// <returns></returns>
        protected override bool ValidateAndSetParameters(string parameters) => true;

        /// <summary>
        /// Validate if all required output files exist
        /// </summary>
        /// <param name="basePath">Base filename and path to use for checking</param>
        /// <param name="system">KnownSystem type representing the media</param>
        /// <param name="type">MediaType type representing the media</param>
        /// <returns></returns>
        public override bool CheckAllOutputFilesExist(string basePath, KnownSystem? system, MediaType? type)
        {
            switch (type)
            {
                case MediaType.NintendoGameCubeGameDisc:
                case MediaType.NintendoWiiOpticalDisc:
                    return File.Exists(basePath + "-dumpinfo.txt")
                        && File.Exists(basePath + ".bca");

                default:
                    return false;
            }
        }

        /// <summary>
        /// Generate a SubmissionInfo for the output files
        /// </summary>
        /// <param name="info">Base submission info to fill in specifics for</param>
        /// <param name="basePath">Base filename and path to use for checking</param>
        /// <param name="system">KnownSystem type representing the media</param>
        /// <param name="type">MediaType type representing the media</param>
        /// <param name="drive">Drive representing the disc to get information from</param>
        /// <returns></returns>
        public override void GenerateSubmissionInfo(SubmissionInfo info, string basePath, KnownSystem? system, MediaType? type, Drive drive)
        {
            info.TracksAndWriteOffsets.ClrMameProData = GetCleanripDatfile(basePath + ".iso", basePath + "-dumpinfo.txt");

            // Extract info based generically on MediaType
            switch (type)
            {
                case MediaType.NintendoGameCubeGameDisc:
                case MediaType.NintendoWiiOpticalDisc:
                    if (File.Exists(basePath + ".bca"))
                        info.Extras.BCA = GetFullFile(basePath + ".bca", true);

                    if (GetGameCubeWiiInformation(basePath + "-dumpinfo.txt", out Region? gcRegion, out string gcVersion))
                    {
                        info.CommonDiscInfo.Region = info.CommonDiscInfo.Region ?? gcRegion;
                        info.VersionAndEditions.Version = string.IsNullOrEmpty(info.VersionAndEditions.Version) ? gcVersion : info.VersionAndEditions.Version;
                    }

                    break;
            }
        }

        #region Information Extraction Methods

        /// <summary>
        /// Get a formatted datfile from the cleanrip output, if possible
        /// </summary>
        /// <param name="iso">Path to ISO file</param>
        /// <param name="dumpinfo">Path to discinfo file</param>
        /// <returns></returns>
        private string GetCleanripDatfile(string iso, string dumpinfo)
        {
            // If the file doesn't exist, we can't get info from it
            if (!File.Exists(dumpinfo))
                return null;

            using (StreamReader sr = File.OpenText(dumpinfo))
            {
                long size = new FileInfo(iso).Length;
                string crc = string.Empty;
                string md5 = string.Empty;
                string sha1 = string.Empty;

                try
                {
                    // Make sure this file is a dumpinfo
                    if (!sr.ReadLine().Contains("--File Generated by CleanRip"))
                        return null;

                    // Read all lines and gather dat information
                    while (!sr.EndOfStream)
                    {
                        string line = sr.ReadLine().Trim();
                        if (line.StartsWith("CRC32"))
                            crc = line.Substring(7).ToLowerInvariant();
                        else if (line.StartsWith("MD5"))
                            md5 = line.Substring(5);
                        else if (line.StartsWith("SHA-1"))
                            sha1 = line.Substring(7);
                    }

                    return $"<rom name=\"{Path.GetFileName(iso)}\" size=\"{size}\" crc=\"{crc}\" md5=\"{md5}\" sha1=\"{sha1}\" />";
                }
                catch
                {
                    // We don't care what the exception is right now
                    return null;
                }
            }
        }

        /// <summary>
        /// Get the extracted GC and Wii version
        /// </summary>
        /// <param name="dumpinfo">Path to discinfo file</param>
        /// <param name="region">Output region, if possible</param>
        /// <param name="version">Output internal version of the game</param>
        /// <returns></returns>
        private bool GetGameCubeWiiInformation(string dumpinfo, out Region? region, out string version)
        {
            region = null; version = null;

            // If the file doesn't exist, we can't get info from it
            if (!File.Exists(dumpinfo))
                return false;

            using (StreamReader sr = File.OpenText(dumpinfo))
            {
                try
                {
                    // Make sure this file is a dumpinfo
                    if (!sr.ReadLine().Contains("--File Generated by CleanRip"))
                        return false;

                    // Read all lines and gather dat information
                    while (!sr.EndOfStream)
                    {
                        string line = sr.ReadLine().Trim();
                        if (line.StartsWith("Version"))
                        {
                            version = line.Substring(9);
                        }
                        else if (line.StartsWith("Filename"))
                        {
                            string serial = line.Substring(10);

                            // char gameType = serial[0];
                            // string gameid = serial[1] + serial[2];
                            // string version = serial[4] + serial[5]

                            switch (serial[3])
                            {
                                case 'A':
                                    region = Region.World;
                                    break;
                                case 'D':
                                    region = Region.Germany;
                                    break;
                                case 'E':
                                    region = Region.USA;
                                    break;
                                case 'F':
                                    region = Region.France;
                                    break;
                                case 'I':
                                    region = Region.Italy;
                                    break;
                                case 'J':
                                    region = Region.Japan;
                                    break;
                                case 'K':
                                    region = Region.Korea;
                                    break;
                                case 'L':
                                    region = Region.Europe; // Japanese import to Europe
                                    break;
                                case 'M':
                                    region = Region.Europe; // American import to Europe
                                    break;
                                case 'N':
                                    region = Region.USA; // Japanese import to USA
                                    break;
                                case 'P':
                                    region = Region.Europe;
                                    break;
                                case 'R':
                                    region = Region.Russia;
                                    break;
                                case 'S':
                                    region = Region.Spain;
                                    break;
                                case 'Q':
                                    region = Region.Korea; // Korea with Japanese language
                                    break;
                                case 'T':
                                    region = Region.Korea; // Korea with English language
                                    break;
                                case 'X':
                                    region = null; // Not a real region code
                                    break;
                            }
                        }
                    }

                    return true;
                }
                catch
                {
                    // We don't care what the exception is right now
                    return false;
                }
            }
        }

        #endregion
    }
}
