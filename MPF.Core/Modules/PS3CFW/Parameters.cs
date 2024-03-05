using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using MPF.Core.Converters;
using MPF.Core.Data;
using SabreTools.Hashing;
using SabreTools.RedumpLib;
using SabreTools.RedumpLib.Data;

namespace MPF.Core.Modules.PS3CFW
{
    /// <summary>
    /// Represents a generic set of PlayStation 3 Custom Firmware parameters
    /// </summary>
    public class Parameters : BaseParameters
    {
        #region Metadata

        /// <inheritdoc/>
        public override InternalProgram InternalProgram => InternalProgram.PS3CFW;

        #endregion

        /// <inheritdoc/>
        public Parameters(string? parameters) : base(parameters) { }

        /// <inheritdoc/>
        public Parameters(RedumpSystem? system, MediaType? type, string? drivePath, string filename, int? driveSpeed, Options options)
            : base(system, type, drivePath, filename, driveSpeed, options)
        {
        }

        #region BaseParameters Implementations

        /// <inheritdoc/>
        public override (bool, List<string>) CheckAllOutputFilesExist(string basePath, bool preCheck)
        {
            var missingFiles = new List<string>();
            
            if (this.Type != MediaType.BluRay || this.System != RedumpSystem.SonyPlayStation3)
            {
                missingFiles.Add("Media and system combination not supported for PS3 CFW");
            }
            else
            {
                string? getKeyBasePath = GetCFWBasePath(basePath);
                if (!File.Exists($"{getKeyBasePath}.getkey.log"))
                    missingFiles.Add($"{getKeyBasePath}.getkey.log");
                if (!File.Exists($"{getKeyBasePath}.disc.pic"))
                    missingFiles.Add($"{getKeyBasePath}.disc.pic");
            }

            return (missingFiles.Count == 0, missingFiles);
        }

        /// <inheritdoc/>
        public override void GenerateSubmissionInfo(SubmissionInfo info, Options options, string basePath, Drive? drive, bool includeArtifacts)
        {
            // Ensure that required sections exist
            info = Builder.EnsureAllSections(info);

            info.DumpingInfo!.DumpingProgram = EnumConverter.LongName(this.InternalProgram);

            // Get the Datafile information
            Datafile? datafile = GeneratePS3CFWDatafile(basePath + ".iso");

            // Fill in the hash data
            info.TracksAndWriteOffsets!.ClrMameProData = InfoTool.GenerateDatfile(datafile);

            // Get the individual hash data, as per internal
            if (InfoTool.GetISOHashValues(datafile, out long size, out var crc32, out var md5, out var sha1))
            {
                info.SizeAndChecksums!.Size = size;
                info.SizeAndChecksums.CRC32 = crc32;
                info.SizeAndChecksums.MD5 = md5;
                info.SizeAndChecksums.SHA1 = sha1;
            }

            // Get the PVD from the ISO
            if (GetPVD(basePath + ".iso", out string? pvd))
                info.Extras!.PVD = pvd;

            // Try get the serial, version, and firmware version if a drive is provided
            if (drive != null)
            {
                info.VersionAndEditions!.Version = InfoTool.GetPlayStation3Version(drive?.Name) ?? string.Empty;
                info.CommonDiscInfo!.CommentsSpecialFields![SiteCode.InternalSerialName] = InfoTool.GetPlayStation3Serial(drive?.Name) ?? string.Empty;
                string? firmwareVersion = InfoTool.GetPlayStation3FirmwareVersion(drive?.Name);
                if (firmwareVersion != null)
                    info.CommonDiscInfo!.ContentsSpecialFields![SiteCode.Patches] = $"PS3 Firmware {firmwareVersion}";
            }

            // Try to determine the name of the GetKey file(s)
            string? getKeyBasePath = GetCFWBasePath(basePath);

            // If GenerateSubmissionInfo is run, .getkey.log existence should already be checked
            if (!File.Exists(getKeyBasePath + ".getkey.log"))
                return;

            // Get dumping date from GetKey log date
            info.DumpingInfo.DumpingDate = InfoTool.GetFileModifiedDate(getKeyBasePath + ".getkey.log")?.ToString("yyyy-MM-dd HH:mm:ss");

            // TODO: Put info about abnormal PIC info beyond 132 bytes in comments?
            if (File.Exists(getKeyBasePath + ".disc.pic"))
                info.Extras!.PIC = GetPIC(getKeyBasePath + ".disc.pic", 264);

            // Parse Disc Key, Disc ID, and PIC from the .getkey.log file
            if (Utilities.Tools.ParseGetKeyLog(getKeyBasePath + ".getkey.log", out string? key, out string? id, out string? pic))
            {
                if (key != null)
                    info.Extras!.DiscKey = key.ToUpperInvariant();
                if (id != null)
                    info.Extras!.DiscID = id.ToUpperInvariant().Substring(0, 24) + "XXXXXXXX";
                if (string.IsNullOrEmpty(info.Extras!.PIC) && !string.IsNullOrEmpty(pic))
                {
                    pic = Regex.Replace(pic, ".{32}", "$0\n");
                    info.Extras.PIC = pic;
                }
            }

            // Fill in any artifacts that exist, Base64-encoded, if we need to
            if (includeArtifacts)
            {
                info.Artifacts ??= [];

                if (File.Exists(getKeyBasePath + ".disc.pic"))
                    info.Artifacts["discpic"] = GetBase64(GetFullFile(getKeyBasePath + ".disc.pic", binary: true)) ?? string.Empty;
                if (File.Exists(getKeyBasePath + ".getkey.log"))
                    info.Artifacts["getkeylog"] = GetBase64(GetFullFile(getKeyBasePath + ".getkey.log")) ?? string.Empty;
            }
        }

        /// <inheritdoc/>
        public override List<string> GetLogFilePaths(string basePath)
        {
            var logFiles = new List<string>();
            string? getKeyBasePath = GetCFWBasePath(basePath);

            if (this.System != RedumpSystem.SonyPlayStation3)
                return logFiles;

            switch (this.Type)
            {
                case MediaType.BluRay:
                    if (File.Exists($"{getKeyBasePath}.getkey.log"))
                        logFiles.Add($"{getKeyBasePath}.getkey.log");
                    if (File.Exists($"{getKeyBasePath}.disc.pic"))
                        logFiles.Add($"{getKeyBasePath}.disc.pic");

                    break;
            }

            return logFiles;
        }

        #endregion

        #region Information Extraction Methods

        /// <summary>
        /// Get a formatted datfile from the PS3 CFW output, if possible
        /// </summary>
        /// <param name="iso">Path to ISO file</param>
        /// <returns></returns>
        private static Datafile? GeneratePS3CFWDatafile(string iso)
        {
            // If the ISO file doesn't exist, we can't get info from it
            if (!File.Exists(iso))
                return null;

            try
            {
                if (HashTool.GetStandardHashes(iso, out long size, out string? crc, out string? md5, out string? sha1))
                {
                    return new Datafile
                    {
                        Games = [new Game { Roms = [new Rom { Name = Path.GetFileName(iso), Size = size.ToString(), Crc = crc, Md5 = md5, Sha1 = sha1, }] }]
                    };
                }
                return null;
            }
            catch
            {
                // We don't care what the exception is right now
                return null;
            }
        }

        // TODO: Don't hardcode 0x8320 and move this function to BaseParameters
        /// <summary>
        /// Get a isobuster-formatted PVD from a PS3 ISO, if possible
        /// </summary>
        /// <param name="isoPath">Path to ISO file</param>
        /// <param name="pvd">Formatted PVD string, otherwise null</param>
        /// <returns>True if PVD was successfully parsed, otherwise false</returns>
        private static bool GetPVD(string isoPath, out string? pvd)
        {
            pvd = null;
            try
            {
                // Get PVD bytes from ISO file
                var buf = new byte[96];
                using (FileStream iso = File.OpenRead(isoPath))
                {
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
                    pvd += $"{(0x0320+i):X4} : {buf[i]:X2} {buf[i+1]:X2} {buf[i+2]:X2} {buf[i+3]:X2} {buf[i+4]:X2} {buf[i+5]:X2} {buf[i+6]:X2} {buf[i+7]:X2}  " +
                        $"{buf[i+8]:X2} {buf[i+9]:X2} {buf[i+10]:X2} {buf[i+11]:X2} {buf[i+12]:X2} {buf[i+13]:X2} {buf[i+14]:X2} {buf[i+15]:X2}   {pvdASCII.Substring(i, 16)}\n";
                }

                return true;
            }
            catch
            {
                // We don't care what the error is
                return false;
            }
        }

        /// <summary>
        /// Get a formatted datfile from the PS3 CFW output, if possible
        /// </summary>
        /// <param name="iso">Path to ISO file</param>
        /// <returns>Formatted datfile, null if not valid</returns>
        private static string? GetPS3CFWDatfile(string iso)
        {
            // If the files don't exist, we can't get info from it
            if (!File.Exists(iso))
                return null;

            try
            {
                if (HashTool.GetStandardHashes(iso, out long size, out string? crc, out string? md5, out string? sha1))
                    return $"<rom name=\"{Path.GetFileName(iso)}\" size=\"{size}\" crc=\"{crc}\" md5=\"{md5}\" sha1=\"{sha1}\" />";
                return null;
            }
            catch
            {
                // We don't care what the exception is right now
                return null;
            }
        }

        #endregion

        #region Helper Functions

        /// <summary>
        /// Estimate the base filename of the .getkey.log file associated with the dump
        /// </summary>
        /// <param name="iso">Path to ISO file</param>
        /// <returns>Base filename, null if not found</returns>
        private string? GetCFWBasePath(string iso)
        {
            string? dir = Path.GetDirectoryName(iso);
            dir ??= ".";

            string[] files = Directory.GetFiles(dir, "*.getkey.log");

            if (files.Length != 1)
                return null;

            return files[0].Substring(0, files[0].Length - 11);
        }

        #endregion
    }
}
