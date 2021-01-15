using System;
using System.IO;
using MPF.Data;
using MPF.Utilities;
using MPF.Web;

namespace MPF.UmdImageCreator
{
    /// <summary>
    /// Represents a generic set of UmdImageCreator parameters
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
            this.InternalProgram = InternalProgram.UmdImageCreator;
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
        /// <param name="driveLetter">Drive letter to use</param>
        /// <param name="filename">Filename to use</param>
        /// <param name="driveSpeed">Drive speed to use</param>
        /// <param name="paranoid">Enable paranoid mode (safer dumping)</param>
        /// <param name="retryCount">User-defined reread count</param>
        protected override void SetDefaultParameters(
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
        /// <param name="progress">Optional result progress callback</param>
        /// <returns></returns>
        public override bool CheckAllOutputFilesExist(string basePath, IProgress<Result> progress = null)
        {
            string missingFiles = string.Empty;
            switch (this.Type)
            {
                case MediaType.UMD:
                    if (!File.Exists($"{basePath}_disc.txt"))
                        missingFiles += $";{basePath}_disc.txt";
                    if (!File.Exists($"{basePath}_mainError.txt"))
                        missingFiles += $";{basePath}_mainError.txt";
                    if (!File.Exists($"{basePath}_mainInfo.txt"))
                        missingFiles += $";{basePath}_mainInfo.txt";
                    if (!File.Exists($"{basePath}_volDesc.txt"))
                        missingFiles += $";{basePath}_volDesc.txt";

                    break;

                default:
                    // Non-dumping commands will usually produce no output, so this is irrelevant
                    return true;
            }

            // Use the missing files list as an indicator
            if (string.IsNullOrEmpty(missingFiles))
            {
                return true;
            }
            else
            {
                progress?.Report(Result.Failure($"The following files were missing: {missingFiles.TrimStart(';')}"));
                return false;
            }
        }

        /// <summary>
        /// Generate a SubmissionInfo for the output files
        /// </summary>
        /// <param name="info">Base submission info to fill in specifics for</param>
        /// <param name="basePath">Base filename and path to use for checking</param>
        /// <param name="drive">Drive representing the disc to get information from</param>
        /// <returns></returns>
        public override void GenerateSubmissionInfo(SubmissionInfo info, string basePath, Drive drive)
        {
            // Extract info based generically on MediaType
            switch (this.Type)
            {
                case MediaType.UMD:
                    info.Extras.PVD = GetPVD(basePath + "_mainInfo.txt") ?? "";

                    if (GetFileHashes(basePath + ".iso", out long filesize, out string crc32, out string md5, out string sha1))
                    {
                        info.SizeAndChecksums.Size = filesize;
                        info.SizeAndChecksums.CRC32 = crc32;
                        info.SizeAndChecksums.MD5 = md5;
                        info.SizeAndChecksums.SHA1 = sha1;
                    }

                    if (GetUMDAuxInfo(basePath + "_disc.txt", out string title, out DiscCategory? umdcat, out string umdversion, out string umdlayer, out long umdsize))
                    {
                        info.CommonDiscInfo.Title = title ?? "";
                        info.CommonDiscInfo.Category = umdcat ?? DiscCategory.Games;
                        info.VersionAndEditions.Version = umdversion ?? "";
                        info.SizeAndChecksums.Size = umdsize;

                        if (!string.IsNullOrWhiteSpace(umdlayer))
                            info.SizeAndChecksums.Layerbreak = Int64.Parse(umdlayer ?? "-1");
                    }

                    break;
            }
        }

        #region Information Extraction Methods

        /// <summary>
        /// Get the PVD from the input file, if possible
        /// </summary>
        /// <param name="mainInfo">_mainInfo.txt file location</param>
        /// <returns>Newline-deliminated PVD if possible, null on error</returns>
        private static string GetPVD(string mainInfo)
        {
            // If the file doesn't exist, we can't get info from it
            if (!File.Exists(mainInfo))
                return null;

            using (StreamReader sr = File.OpenText(mainInfo))
            {
                try
                {
                    // Make sure we're in the right sector
                    while (!sr.ReadLine().StartsWith("========== LBA[000016, 0x00010]: Main Channel ==========")) ;

                    // Fast forward to the PVD
                    while (!sr.ReadLine().StartsWith("0310")) ;

                    // Now that we're at the PVD, read each line in and concatenate
                    string pvd = "";
                    for (int i = 0; i < 6; i++)
                        pvd += sr.ReadLine() + "\n"; // 320-370

                    return pvd;
                }
                catch
                {
                    // We don't care what the exception is right now
                    return null;
                }
            }
        }

        /// <summary>
        /// Get the UMD auxiliary info from the outputted files, if possible
        /// </summary>
        /// <param name="disc">_disc.txt file location</param>
        /// <returns>True on successful extraction of info, false otherwise</returns>
        private static bool GetUMDAuxInfo(string disc, out string title, out DiscCategory? umdcat, out string umdversion, out string umdlayer, out long umdsize)
        {
            title = null; umdcat = null; umdversion = null; umdlayer = null; umdsize = -1;

            // If the file doesn't exist, we can't get info from it
            if (!File.Exists(disc))
                return false;

            using (StreamReader sr = File.OpenText(disc))
            {
                try
                {
                    // Loop through everything to get the first instance of each required field
                    string line = string.Empty;
                    while (!sr.EndOfStream)
                    {
                        line = sr.ReadLine().Trim();

                        if (line.StartsWith("TITLE") && title == null)
                            title = line.Substring("TITLE: ".Length);
                        else if (line.StartsWith("DISC_VERSION") && umdversion == null)
                            umdversion = line.Split(' ')[1];
                        else if (line.StartsWith("pspUmdTypes"))
                            umdcat = GetUMDCategory(line.Split(' ')[1]);
                        else if (line.StartsWith("L0 length"))
                            umdlayer = line.Split(' ')[2];
                        else if (line.StartsWith("FileSize:"))
                            umdsize = Int64.Parse(line.Split(' ')[1]);
                    }

                    // If the L0 length is the size of the full disc, there's no layerbreak
                    if (Int64.Parse(umdlayer) * 2048 == umdsize)
                        umdlayer = null;

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
