using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using MPF.Core.Data;
using SabreTools.RedumpLib.Data;

namespace MPF.Core.Processors
{
    public abstract class BaseProcessor
    {
        /// <summary>
        /// All found volume labels and their corresponding file systems
        /// </summary>
        public Dictionary<string, List<string>>? VolumeLabels;

        #region Metadata

        /// <summary>
        /// Currently represented system
        /// </summary>
        public RedumpSystem? System { get; private set; }

        /// <summary>
        /// Currently represented media type
        /// </summary>
        public MediaType? Type { get; private set; }

        #endregion

        /// <summary>
        /// Generate processor for a system and media type combination
        /// </summary>
        /// <param name="system">RedumpSystem value to use</param>
        /// <param name="type">MediaType value to use</param>
        public BaseProcessor(RedumpSystem? system, MediaType? type)
        {
            System = system;
            Type = type;
        }

        #region Abstract Methods

        /// <summary>
        /// Validate if all required output files exist
        /// </summary>
        /// <param name="basePath">Base filename and path to use for checking</param>
        /// <param name="preCheck">True if this is a check done before a dump, false if done after</param>
        /// <returns>Tuple of true if all required files exist, false otherwise and a list representing missing files</returns>
        public abstract (bool, List<string>) CheckAllOutputFilesExist(string basePath, bool preCheck);

        /// <summary>
        /// Generate artifacts and add to the SubmissionInfo
        /// </summary>
        /// <param name="submissionInfo">Base submission info to fill in specifics for</param>
        /// <param name="basePath">Base filename and path to use for checking</param>
        public abstract void GenerateArtifacts(SubmissionInfo submissionInfo, string basePath);

        /// <summary>
        /// Generate a SubmissionInfo for the output files
        /// </summary>
        /// <param name="submissionInfo">Base submission info to fill in specifics for</param>
        /// <param name="options">Options object representing user-defined options</param>
        /// <param name="basePath">Base filename and path to use for checking</param>
        /// <param name="drive">Drive representing the disc to get information from</param>
        /// <param name="redumpCompat">Determines if outputs are processed according to Redump specifications</param>
        public abstract void GenerateSubmissionInfo(SubmissionInfo submissionInfo, string basePath, Drive? drive, bool redumpCompat);

        /// <summary>
        /// Generate a list of all log files generated
        /// </summary>
        /// <param name="basePath">Base filename and path to use for checking</param>
        /// <returns>List of all log file paths, empty otherwise</returns>
        public abstract List<string> GetLogFilePaths(string basePath);

        #endregion

        #region Virtual Methods

        /// <summary>
        /// Generate a list of all deleteable files generated
        /// </summary>
        /// <param name="basePath">Base filename and path to use for checking</param>
        /// <returns>List of all deleteable file paths, empty otherwise</returns>
        public virtual List<string> GetDeleteableFilePaths(string basePath) => [];

        #endregion

        #region Methods to Move

        /// <summary>
        /// Get the hex contents of the PIC file
        /// </summary>
        /// <param name="picPath">Path to the PIC.bin file associated with the dump</param>
        /// <param name="trimLength">Number of characters to trim the PIC to, if -1, ignored</param>
        /// <returns>PIC data as a hex string if possible, null on error</returns>
        /// <remarks>https://stackoverflow.com/questions/9932096/add-separator-to-string-at-every-n-characters</remarks>
        protected static string? GetPIC(string picPath, int trimLength = -1)
        {
            // If the file doesn't exist, we can't get the info
            if (!File.Exists(picPath))
                return null;

            try
            {
                var hex = InfoTool.GetFullFile(picPath, true);
                if (hex == null)
                    return null;

                if (trimLength > -1)
                    hex = hex.Substring(0, trimLength);

                // TODO: Check for non-zero values in discarded PIC

                return Regex.Replace(hex, ".{32}", "$0\n", RegexOptions.Compiled);
            }
            catch
            {
                // We don't care what the error was right now
                return null;
            }
        }

        /// <summary>
        /// Get a isobuster-formatted PVD from a 2048 byte-per-sector image, if possible
        /// </summary>
        /// <param name="isoPath">Path to ISO file</param>
        /// <param name="pvd">Formatted PVD string, otherwise null</param>
        /// <returns>True if PVD was successfully parsed, otherwise false</returns>
        protected static bool GetPVD(string isoPath, out string? pvd)
        {
            pvd = null;
            try
            {
                // Get PVD bytes from ISO file
                var buf = new byte[96];
                using (FileStream iso = File.OpenRead(isoPath))
                {
                    // TODO: Don't hardcode 0x8320
                    iso.Seek(0x8320, SeekOrigin.Begin);

                    int offset = 0;
                    while (offset < 96)
                    {
                        int read = iso.Read(buf, offset, buf.Length - offset);
                        if (read == 0)
                            throw new EndOfStreamException();
                        offset += read;
                    }
                }

                // Format PVD to isobuster standard
                char[] pvdCharArray = new char[96];
                for (int i = 0; i < 96; i++)
                {
                    if (buf[i] >= 0x20 && buf[i] <= 0x7E)
                        pvdCharArray[i] = (char)buf[i];
                    else
                        pvdCharArray[i] = '.';
                }
                string pvdASCII = new string(pvdCharArray, 0, 96);
                pvd = string.Empty;
                for (int i = 0; i < 96; i += 16)
                {
                    pvd += $"{(0x0320 + i):X4} : {buf[i]:X2} {buf[i + 1]:X2} {buf[i + 2]:X2} {buf[i + 3]:X2} {buf[i + 4]:X2} {buf[i + 5]:X2} {buf[i + 6]:X2} {buf[i + 7]:X2}  " +
                        $"{buf[i + 8]:X2} {buf[i + 9]:X2} {buf[i + 10]:X2} {buf[i + 11]:X2} {buf[i + 12]:X2} {buf[i + 13]:X2} {buf[i + 14]:X2} {buf[i + 15]:X2}   {pvdASCII.Substring(i, 16)}\n";
                }

                return true;
            }
            catch
            {
                // We don't care what the error is
                return false;
            }
        }

        #endregion
    }
}
