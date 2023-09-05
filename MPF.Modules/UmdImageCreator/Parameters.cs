using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using MPF.Core.Converters;
using MPF.Core.Data;
using SabreTools.RedumpLib.Data;

namespace MPF.Modules.UmdImageCreator
{
    /// <summary>
    /// Represents a generic set of UmdImageCreator parameters
    /// </summary>
    public class Parameters : BaseParameters
    {
        #region Metadata

        /// <inheritdoc/>
        public override InternalProgram InternalProgram => InternalProgram.UmdImageCreator;

        #endregion

        /// <inheritdoc/>
        public Parameters(string parameters) : base(parameters) { }

        /// <inheritdoc/>
        public Parameters(RedumpSystem? system, MediaType? type, char driveLetter, string filename, int? driveSpeed, Options options)
            : base(system, type, driveLetter, filename, driveSpeed, options)
        {
        }

        #region BaseParameters Implementations

        /// <inheritdoc/>
        public override (bool, List<string>) CheckAllOutputFilesExist(string basePath, bool preCheck)
        {
            List<string> missingFiles = new List<string>();
            switch (this.Type)
            {
                case MediaType.UMD:
                    if (!File.Exists($"{basePath}_logs.zip") || !preCheck)
                    {
                        if (!File.Exists($"{basePath}_disc.txt"))
                            missingFiles.Add($"{basePath}_disc.txt");
                        if (!File.Exists($"{basePath}_mainError.txt"))
                            missingFiles.Add($"{basePath}_mainError.txt");
                        if (!File.Exists($"{basePath}_mainInfo.txt"))
                            missingFiles.Add($"{basePath}_mainInfo.txt");
                        if (!File.Exists($"{basePath}_volDesc.txt"))
                            missingFiles.Add($"{basePath}_volDesc.txt");
                    }

                    break;

                default:
                    missingFiles.Add("Media and system combination not supported for UmdImageCreator");
                    break;
            }

            return (!missingFiles.Any(), missingFiles);
        }

        /// <inheritdoc/>
        public override void GenerateSubmissionInfo(SubmissionInfo info, Options options, string basePath, Drive drive, bool includeArtifacts)
        {
            // TODO: Determine if there's a UMDImageCreator version anywhere
            info.DumpingInfo.DumpingProgram = EnumConverter.LongName(this.InternalProgram);
            info.DumpingInfo.DumpingDate = GetFileModifiedDate(basePath + "_disc.txt")?.ToString("yyyy-MM-dd hh:mm:ss");

            // Extract info based generically on MediaType
            switch (this.Type)
            {
                case MediaType.UMD:
                    info.Extras.PVD = GetPVD(basePath + "_mainInfo.txt") ?? string.Empty;

                    if (GetFileHashes(basePath + ".iso", out long filesize, out string crc32, out string md5, out string sha1))
                    {
                        info.SizeAndChecksums.Size = filesize;
                        info.SizeAndChecksums.CRC32 = crc32;
                        info.SizeAndChecksums.MD5 = md5;
                        info.SizeAndChecksums.SHA1 = sha1;
                    }

                    if (GetUMDAuxInfo(basePath + "_disc.txt", out string title, out DiscCategory? umdcat, out string umdversion, out string umdlayer, out long umdsize))
                    {
                        info.CommonDiscInfo.Title = title ?? string.Empty;
                        info.CommonDiscInfo.Category = umdcat ?? DiscCategory.Games;
                        info.VersionAndEditions.Version = umdversion ?? string.Empty;
                        info.SizeAndChecksums.Size = umdsize;

                        if (!string.IsNullOrWhiteSpace(umdlayer))
                            info.SizeAndChecksums.Layerbreak = Int64.Parse(umdlayer ?? "-1");
                    }

                    break;
            }

            // Fill in any artifacts that exist, Base64-encoded, if we need to
            if (includeArtifacts)
            {
                if (File.Exists(basePath + "_disc.txt"))
                    info.Artifacts["disc"] = GetBase64(GetFullFile(basePath + "_disc.txt"));
                if (File.Exists(basePath + "_drive.txt"))
                    info.Artifacts["drive"] = GetBase64(GetFullFile(basePath + "_drive.txt"));
                if (File.Exists(basePath + "_mainError.txt"))
                    info.Artifacts["mainError"] = GetBase64(GetFullFile(basePath + "_mainError.txt"));
                if (File.Exists(basePath + "_mainInfo.txt"))
                    info.Artifacts["mainInfo"] = GetBase64(GetFullFile(basePath + "_mainInfo.txt"));
                if (File.Exists(basePath + "_volDesc.txt"))
                    info.Artifacts["volDesc"] = GetBase64(GetFullFile(basePath + "_volDesc.txt"));
            }
        }

        /// <inheritdoc/>
        public override List<string> GetLogFilePaths(string basePath)
        {
            List<string> logFiles = new List<string>();
            switch (this.Type)
            {
                case MediaType.UMD:
                    if (File.Exists($"{basePath}_disc.txt"))
                        logFiles.Add($"{basePath}_disc.txt");
                    if (File.Exists($"{basePath}_drive.txt"))
                        logFiles.Add($"{basePath}_drive.txt");
                    if (File.Exists($"{basePath}_mainError.txt"))
                        logFiles.Add($"{basePath}_mainError.txt");
                    if (File.Exists($"{basePath}_mainInfo.txt"))
                        logFiles.Add($"{basePath}_mainInfo.txt");
                    if (File.Exists($"{basePath}_volDesc.txt"))
                        logFiles.Add($"{basePath}_volDesc.txt");

                    break;
            }

            return logFiles;
        }

        #endregion

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
                    while (!sr.ReadLine().StartsWith("========== LBA[000016, 0x0000010]: Main Channel ==========")) ;

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
