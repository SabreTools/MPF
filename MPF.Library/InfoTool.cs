using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml;
using BurnOutSharp;
using MPF.Core.Data;
using MPF.Core.Utilities;
using MPF.Modules;
using Newtonsoft.Json;
using RedumpLib.Data;
using RedumpLib.Web;
using Formatting = Newtonsoft.Json.Formatting;

namespace MPF.Library
{
    public static class InfoTool
    {
        #region Information Extraction

        /// <summary>
        /// Extract all of the possible information from a given input combination
        /// </summary>
        /// <param name="outputDirectory">Output folder to write to</param>
        /// <param name="outputFilename">Output filename to use as the base path</param>
        /// <param name="drive">Drive object representing the current drive</param>
        /// <param name="system">Currently selected system</param>
        /// <param name="mediaType">Currently selected media type</param>
        /// <param name="options">Options object representing user-defined options</param>
        /// <param name="parameters">Parameters object representing what to send to the internal program</param>
        /// <param name="resultProgress">Optional result progress callback</param>
        /// <param name="protectionProgress">Optional protection progress callback</param>
        /// <returns>SubmissionInfo populated based on outputs, null on error</returns>
        public static async Task<SubmissionInfo> ExtractOutputInformation(
            string outputDirectory,
            string outputFilename,
            Drive drive,
            RedumpSystem? system,
            MediaType? mediaType,
            Options options,
            BaseParameters parameters,
            IProgress<Result> resultProgress = null,
            IProgress<ProtectionProgress> protectionProgress = null)
        {
            // Ensure the current disc combination should exist
            if (!system.MediaTypes().Contains(mediaType))
                return null;

            // Check that all of the relevant files are there
            (bool foundFiles, List<string> missingFiles) = FoundAllFiles(outputDirectory, outputFilename, parameters, false);
            if (!foundFiles)
            {
                resultProgress.Report(Result.Failure($"There were files missing from the output:\n{string.Join("\n", missingFiles)}"));
                return null;
            }

            // Sanitize the output filename to strip off any potential extension
            outputFilename = Path.GetFileNameWithoutExtension(outputFilename);

            // Create the SubmissionInfo object with all user-inputted values by default
            string combinedBase = Path.Combine(outputDirectory, outputFilename);
            SubmissionInfo info = new SubmissionInfo()
            {
                CommonDiscInfo = new CommonDiscInfoSection()
                {
                    System = system,
                    Media = mediaType.ToDiscType(),
                    Title = options.AddPlaceholders ? Template.RequiredValue : string.Empty,
                    ForeignTitleNonLatin = options.AddPlaceholders ? Template.OptionalValue : string.Empty,
                    DiscNumberLetter = options.AddPlaceholders ? Template.OptionalValue : string.Empty,
                    DiscTitle = options.AddPlaceholders ? Template.OptionalValue : string.Empty,
                    Category = null,
                    Region = null,
                    Languages = null,
                    Serial = options.AddPlaceholders ? Template.RequiredIfExistsValue : string.Empty,
                    Barcode = options.AddPlaceholders ? Template.OptionalValue : string.Empty,
                    Contents = string.Empty,
                    ContentsSpecialFields = new Dictionary<SiteCode?, string>(),
                    Comments = string.Empty,
                    CommentsSpecialFields = new Dictionary<SiteCode?, string>(),
                },
                VersionAndEditions = new VersionAndEditionsSection()
                {
                    Version = options.AddPlaceholders ? Template.RequiredIfExistsValue : string.Empty,
                    OtherEditions = options.AddPlaceholders ? "(VERIFY THIS) Original" : string.Empty,
                },
                TracksAndWriteOffsets = new TracksAndWriteOffsetsSection(),
            };

            // Get specific tool output handling
            parameters.GenerateSubmissionInfo(info, options, combinedBase, drive, options.IncludeArtifacts);

            // Get a list of matching IDs for each line in the DAT
            if (!string.IsNullOrEmpty(info.TracksAndWriteOffsets.ClrMameProData) && options.HasRedumpLogin)
#if NET48 || NETSTANDARD2_1
                FillFromRedump(options, info, resultProgress);
#else
                _ = await FillFromRedump(options, info, resultProgress);
#endif

            // If we have both ClrMamePro and Size and Checksums data, remove the ClrMamePro
            if (!string.IsNullOrWhiteSpace(info.SizeAndChecksums.CRC32))
                info.TracksAndWriteOffsets.ClrMameProData = null;

            // Add the volume label to comments, if possible or necessary
            if (drive != null && drive.GetRedumpSystemFromVolumeLabel() == null)
                info.CommonDiscInfo.CommentsSpecialFields[SiteCode.VolumeLabel] = drive.VolumeLabel;

            // Extract info based generically on MediaType
            switch (mediaType)
            {
                case MediaType.CDROM:
                case MediaType.GDROM:
                    info.CommonDiscInfo.Layer0MasteringRing = options.AddPlaceholders ? Template.RequiredIfExistsValue : string.Empty;
                    info.CommonDiscInfo.Layer0MasteringSID = options.AddPlaceholders ? Template.RequiredIfExistsValue : string.Empty;
                    info.CommonDiscInfo.Layer0ToolstampMasteringCode = options.AddPlaceholders ? Template.RequiredIfExistsValue : string.Empty;
                    info.CommonDiscInfo.Layer0MouldSID = options.AddPlaceholders ? Template.RequiredIfExistsValue : string.Empty;
                    info.CommonDiscInfo.Layer1MouldSID = options.AddPlaceholders ? Template.RequiredIfExistsValue : string.Empty;
                    info.CommonDiscInfo.Layer0AdditionalMould = options.AddPlaceholders ? Template.RequiredIfExistsValue : string.Empty;
                    break;

                case MediaType.DVD:
                case MediaType.HDDVD:
                case MediaType.BluRay:
                    // If we have a single-layer disc
                    if (info.SizeAndChecksums.Layerbreak == default)
                    {
                        info.CommonDiscInfo.Layer0MasteringRing = options.AddPlaceholders ? Template.RequiredIfExistsValue : string.Empty;
                        info.CommonDiscInfo.Layer0MasteringSID = options.AddPlaceholders ? Template.RequiredIfExistsValue : string.Empty;
                        info.CommonDiscInfo.Layer0ToolstampMasteringCode = options.AddPlaceholders ? Template.RequiredIfExistsValue : string.Empty;
                        info.CommonDiscInfo.Layer0MouldSID = options.AddPlaceholders ? Template.RequiredIfExistsValue : string.Empty;
                        info.CommonDiscInfo.Layer1MouldSID = options.AddPlaceholders ? Template.RequiredIfExistsValue : string.Empty;
                        info.CommonDiscInfo.Layer0AdditionalMould = options.AddPlaceholders ? Template.RequiredIfExistsValue : string.Empty;
                    }
                    // If we have a dual-layer disc
                    else
                    {
                        info.CommonDiscInfo.Layer0MasteringRing = options.AddPlaceholders ? Template.RequiredIfExistsValue : string.Empty;
                        info.CommonDiscInfo.Layer0MasteringSID = options.AddPlaceholders ? Template.RequiredIfExistsValue : string.Empty;
                        info.CommonDiscInfo.Layer0ToolstampMasteringCode = options.AddPlaceholders ? Template.RequiredIfExistsValue : string.Empty;
                        info.CommonDiscInfo.Layer0MouldSID = options.AddPlaceholders ? Template.RequiredIfExistsValue : string.Empty;
                        info.CommonDiscInfo.Layer0AdditionalMould = options.AddPlaceholders ? Template.RequiredIfExistsValue : string.Empty;

                        info.CommonDiscInfo.Layer1MasteringRing = options.AddPlaceholders ? Template.RequiredIfExistsValue : string.Empty;
                        info.CommonDiscInfo.Layer1MasteringSID = options.AddPlaceholders ? Template.RequiredIfExistsValue : string.Empty;
                        info.CommonDiscInfo.Layer1ToolstampMasteringCode = options.AddPlaceholders ? Template.RequiredIfExistsValue : string.Empty;
                        info.CommonDiscInfo.Layer1MouldSID = options.AddPlaceholders ? Template.RequiredIfExistsValue : string.Empty;
                    }

                    break;

                case MediaType.NintendoGameCubeGameDisc:
                    info.CommonDiscInfo.Layer0MasteringRing = options.AddPlaceholders ? Template.RequiredIfExistsValue : string.Empty;
                    info.CommonDiscInfo.Layer0MasteringSID = options.AddPlaceholders ? Template.RequiredIfExistsValue : string.Empty;
                    info.CommonDiscInfo.Layer0ToolstampMasteringCode = options.AddPlaceholders ? Template.RequiredIfExistsValue : string.Empty;
                    info.CommonDiscInfo.Layer0MouldSID = options.AddPlaceholders ? Template.RequiredIfExistsValue : string.Empty;
                    info.CommonDiscInfo.Layer1MouldSID = options.AddPlaceholders ? Template.RequiredIfExistsValue : string.Empty;
                    info.CommonDiscInfo.Layer0AdditionalMould = options.AddPlaceholders ? Template.RequiredIfExistsValue : string.Empty;
                    info.Extras.BCA = info.Extras.BCA ?? (options.AddPlaceholders ? Template.RequiredValue : string.Empty);
                    break;

                case MediaType.NintendoWiiOpticalDisc:
                    // If we have a single-layer disc
                    if (info.SizeAndChecksums.Layerbreak == default)
                    {
                        info.CommonDiscInfo.Layer0MasteringRing = options.AddPlaceholders ? Template.RequiredIfExistsValue : string.Empty;
                        info.CommonDiscInfo.Layer0MasteringSID = options.AddPlaceholders ? Template.RequiredIfExistsValue : string.Empty;
                        info.CommonDiscInfo.Layer0ToolstampMasteringCode = options.AddPlaceholders ? Template.RequiredIfExistsValue : string.Empty;
                        info.CommonDiscInfo.Layer0MouldSID = options.AddPlaceholders ? Template.RequiredIfExistsValue : string.Empty;
                        info.CommonDiscInfo.Layer1MouldSID = options.AddPlaceholders ? Template.RequiredIfExistsValue : string.Empty;
                        info.CommonDiscInfo.Layer0AdditionalMould = options.AddPlaceholders ? Template.RequiredIfExistsValue : string.Empty;
                    }
                    // If we have a dual-layer disc
                    else
                    {
                        info.CommonDiscInfo.Layer0MasteringRing = options.AddPlaceholders ? Template.RequiredIfExistsValue : string.Empty;
                        info.CommonDiscInfo.Layer0MasteringSID = options.AddPlaceholders ? Template.RequiredIfExistsValue : string.Empty;
                        info.CommonDiscInfo.Layer0ToolstampMasteringCode = options.AddPlaceholders ? Template.RequiredIfExistsValue : string.Empty;
                        info.CommonDiscInfo.Layer0MouldSID = options.AddPlaceholders ? Template.RequiredIfExistsValue : string.Empty;
                        info.CommonDiscInfo.Layer0AdditionalMould = options.AddPlaceholders ? Template.RequiredIfExistsValue : string.Empty;

                        info.CommonDiscInfo.Layer1MasteringRing = options.AddPlaceholders ? Template.RequiredIfExistsValue : string.Empty;
                        info.CommonDiscInfo.Layer1MasteringSID = options.AddPlaceholders ? Template.RequiredIfExistsValue : string.Empty;
                        info.CommonDiscInfo.Layer1ToolstampMasteringCode = options.AddPlaceholders ? Template.RequiredIfExistsValue : string.Empty;
                        info.CommonDiscInfo.Layer1MouldSID = options.AddPlaceholders ? Template.RequiredIfExistsValue : string.Empty;
                    }

                    info.Extras.DiscKey = options.AddPlaceholders ? Template.RequiredValue : string.Empty;
                    info.Extras.BCA = info.Extras.BCA ?? (options.AddPlaceholders ? Template.RequiredValue : string.Empty);

                    break;

                case MediaType.UMD:
                    // If we have a single-layer disc
                    if (info.SizeAndChecksums.Layerbreak == default)
                    {
                        info.CommonDiscInfo.Layer0MasteringRing = options.AddPlaceholders ? Template.RequiredIfExistsValue : string.Empty;
                        info.CommonDiscInfo.Layer0MasteringSID = options.AddPlaceholders ? Template.RequiredIfExistsValue : string.Empty;
                        info.CommonDiscInfo.Layer0ToolstampMasteringCode = options.AddPlaceholders ? Template.RequiredIfExistsValue : string.Empty;
                        info.CommonDiscInfo.Layer0MouldSID = options.AddPlaceholders ? Template.RequiredIfExistsValue : string.Empty;
                    }
                    // If we have a dual-layer disc
                    else
                    {
                        info.CommonDiscInfo.Layer0MasteringRing = options.AddPlaceholders ? Template.RequiredIfExistsValue : string.Empty;
                        info.CommonDiscInfo.Layer0MasteringSID = options.AddPlaceholders ? Template.RequiredIfExistsValue : string.Empty;
                        info.CommonDiscInfo.Layer0ToolstampMasteringCode = options.AddPlaceholders ? Template.RequiredIfExistsValue : string.Empty;
                        info.CommonDiscInfo.Layer0MouldSID = options.AddPlaceholders ? Template.RequiredIfExistsValue : string.Empty;

                        info.CommonDiscInfo.Layer1MasteringRing = options.AddPlaceholders ? Template.RequiredIfExistsValue : string.Empty;
                        info.CommonDiscInfo.Layer1MasteringSID = options.AddPlaceholders ? Template.RequiredIfExistsValue : string.Empty;
                        info.CommonDiscInfo.Layer1ToolstampMasteringCode = options.AddPlaceholders ? Template.RequiredIfExistsValue : string.Empty;
                    }

                    info.SizeAndChecksums.CRC32 = info.SizeAndChecksums.CRC32 ?? (options.AddPlaceholders ? Template.RequiredValue + " [Not automatically generated for UMD]" : string.Empty);
                    info.SizeAndChecksums.MD5 = info.SizeAndChecksums.MD5 ?? (options.AddPlaceholders ? Template.RequiredValue + " [Not automatically generated for UMD]" : string.Empty);
                    info.SizeAndChecksums.SHA1 = info.SizeAndChecksums.SHA1 ?? (options.AddPlaceholders ? Template.RequiredValue + " [Not automatically generated for UMD]" : string.Empty);
                    info.TracksAndWriteOffsets.ClrMameProData = null;
                    break;
            }

            // Extract info based specifically on RedumpSystem
            switch (system)
            {
                case RedumpSystem.AcornArchimedes:
                    info.CommonDiscInfo.Region = info.CommonDiscInfo.Region ?? Region.UnitedKingdom;
                    break;

                case RedumpSystem.AppleMacintosh:
                case RedumpSystem.EnhancedCD:
                case RedumpSystem.IBMPCcompatible:
                case RedumpSystem.PalmOS:
                case RedumpSystem.PocketPC:
                case RedumpSystem.RainbowDisc:
                case RedumpSystem.SonyElectronicBook:
                    resultProgress?.Report(Result.Success("Running copy protection scan... this might take a while!"));
                    (string protectionString, Dictionary<string, List<string>> fullProtections) = await GetCopyProtection(drive, options, protectionProgress);
                    info.CopyProtection.Protection = protectionString;
                    info.CopyProtection.FullProtections = fullProtections;
                    resultProgress?.Report(Result.Success("Copy protection scan complete!"));

                    break;

                case RedumpSystem.AudioCD:
                case RedumpSystem.DVDAudio:
                case RedumpSystem.SuperAudioCD:
                    info.CommonDiscInfo.Category = info.CommonDiscInfo.Category ?? DiscCategory.Audio;
                    break;

                case RedumpSystem.BandaiPlaydiaQuickInteractiveSystem:
                    info.CommonDiscInfo.EXEDateBuildDate = options.AddPlaceholders ? Template.RequiredValue : string.Empty;
                    info.CommonDiscInfo.Region = info.CommonDiscInfo.Region ?? Region.Japan;
                    break;

                case RedumpSystem.BDVideo:
                    info.CommonDiscInfo.Category = info.CommonDiscInfo.Category ?? DiscCategory.BonusDiscs;
                    info.CopyProtection.Protection = options.AddPlaceholders ? Template.RequiredIfExistsValue : string.Empty;
                    break;

                case RedumpSystem.CommodoreAmigaCD:
                    info.CommonDiscInfo.EXEDateBuildDate = options.AddPlaceholders ? Template.RequiredValue : string.Empty;
                    break;

                case RedumpSystem.CommodoreAmigaCD32:
                    info.CommonDiscInfo.EXEDateBuildDate = options.AddPlaceholders ? Template.RequiredValue : string.Empty;
                    info.CommonDiscInfo.Region = info.CommonDiscInfo.Region ?? Region.Europe;
                    break;

                case RedumpSystem.CommodoreAmigaCDTV:
                    info.CommonDiscInfo.EXEDateBuildDate = options.AddPlaceholders ? Template.RequiredValue : string.Empty;
                    info.CommonDiscInfo.Region = info.CommonDiscInfo.Region ?? Region.Europe;
                    break;

                case RedumpSystem.DVDVideo:
                    info.CommonDiscInfo.Category = info.CommonDiscInfo.Category ?? DiscCategory.BonusDiscs;
                    break;

                case RedumpSystem.FujitsuFMTownsseries:
                    info.CommonDiscInfo.EXEDateBuildDate = options.AddPlaceholders ? Template.RequiredValue : string.Empty;
                    info.CommonDiscInfo.Region = info.CommonDiscInfo.Region ?? Region.Japan;
                    break;

                case RedumpSystem.FujitsuFMTownsMarty:
                    info.CommonDiscInfo.Region = info.CommonDiscInfo.Region ?? Region.Japan;
                    break;

                case RedumpSystem.IncredibleTechnologiesEagle:
                    info.CommonDiscInfo.EXEDateBuildDate = options.AddPlaceholders ? Template.RequiredValue : string.Empty;
                    break;

                case RedumpSystem.KonamieAmusement:
                    info.CommonDiscInfo.EXEDateBuildDate = options.AddPlaceholders ? Template.RequiredValue : string.Empty;
                    break;

                case RedumpSystem.KonamiFireBeat:
                    info.CommonDiscInfo.EXEDateBuildDate = options.AddPlaceholders ? Template.RequiredValue : string.Empty;
                    break;

                case RedumpSystem.KonamiSystemGV:
                    info.CommonDiscInfo.EXEDateBuildDate = options.AddPlaceholders ? Template.RequiredValue : string.Empty;
                    break;

                case RedumpSystem.KonamiSystem573:
                    info.CommonDiscInfo.EXEDateBuildDate = options.AddPlaceholders ? Template.RequiredValue : string.Empty;
                    break;

                case RedumpSystem.KonamiTwinkle:
                    info.CommonDiscInfo.EXEDateBuildDate = options.AddPlaceholders ? Template.RequiredValue : string.Empty;
                    break;

                case RedumpSystem.MattelHyperScan:
                    info.CommonDiscInfo.EXEDateBuildDate = options.AddPlaceholders ? Template.RequiredValue : string.Empty;
                    break;

                case RedumpSystem.MicrosoftXboxOne:
                    string xboxOneMsxcPath = Path.Combine($"{drive.Letter}:\\", "MSXC");
                    if (drive != null && Directory.Exists(xboxOneMsxcPath))
                    {
                        info.CommonDiscInfo.CommentsSpecialFields[SiteCode.Filename] = string.Join("\n",
                            Directory.GetFiles(xboxOneMsxcPath, "*", SearchOption.TopDirectoryOnly).Select(Path.GetFileName));
                    }

                    break;

                case RedumpSystem.NamcoSegaNintendoTriforce:
                    info.CommonDiscInfo.EXEDateBuildDate = options.AddPlaceholders ? Template.RequiredValue : string.Empty;
                    break;

                case RedumpSystem.NavisoftNaviken21:
                    info.CommonDiscInfo.EXEDateBuildDate = options.AddPlaceholders ? Template.RequiredValue : string.Empty;
                    info.CommonDiscInfo.Region = info.CommonDiscInfo.Region ?? Region.Japan;
                    break;

                case RedumpSystem.NECPC88series:
                    info.CommonDiscInfo.Region = info.CommonDiscInfo.Region ?? Region.Japan;
                    break;

                case RedumpSystem.NECPC98series:
                    info.CommonDiscInfo.EXEDateBuildDate = options.AddPlaceholders ? Template.RequiredValue : string.Empty;
                    info.CommonDiscInfo.Region = info.CommonDiscInfo.Region ?? Region.Japan;
                    break;

                case RedumpSystem.NECPCFXPCFXGA:
                    info.CommonDiscInfo.Region = info.CommonDiscInfo.Region ?? Region.Japan;
                    break;

                case RedumpSystem.SegaChihiro:
                    info.CommonDiscInfo.EXEDateBuildDate = options.AddPlaceholders ? Template.RequiredValue : string.Empty;
                    break;

                case RedumpSystem.SegaDreamcast:
                    info.CommonDiscInfo.EXEDateBuildDate = options.AddPlaceholders ? Template.RequiredValue : string.Empty;
                    break;

                case RedumpSystem.SegaNaomi:
                    info.CommonDiscInfo.EXEDateBuildDate = options.AddPlaceholders ? Template.RequiredValue : string.Empty;
                    break;

                case RedumpSystem.SegaNaomi2:
                    info.CommonDiscInfo.EXEDateBuildDate = options.AddPlaceholders ? Template.RequiredValue : string.Empty;
                    break;

                case RedumpSystem.SegaTitanVideo:
                    info.CommonDiscInfo.EXEDateBuildDate = options.AddPlaceholders ? Template.RequiredValue : string.Empty;
                    break;

                case RedumpSystem.SharpX68000:
                    info.CommonDiscInfo.Region = info.CommonDiscInfo.Region ?? Region.Japan;
                    break;

                case RedumpSystem.SNKNeoGeoCD:
                    info.CommonDiscInfo.EXEDateBuildDate = options.AddPlaceholders ? Template.RequiredValue : string.Empty;
                    break;

                case RedumpSystem.SonyPlayStation:
                    // Only check the disc if the dumping program couldn't detect
                    if (drive != null && info.CopyProtection.AntiModchip == YesNo.NULL)
                    {
                        resultProgress?.Report(Result.Success("Checking for anti-modchip strings... this might take a while!"));
                        info.CopyProtection.AntiModchip = await GetAntiModchipDetected(drive) ? YesNo.Yes : YesNo.No;
                        resultProgress?.Report(Result.Success("Anti-modchip string scan complete!"));
                    }

                    // Special case for DIC only
                    if (parameters.InternalProgram == InternalProgram.DiscImageCreator)
                    {
                        resultProgress?.Report(Result.Success("Checking for LibCrypt status... this might take a while!"));
                        GetLibCryptDetected(info, combinedBase);
                        resultProgress?.Report(Result.Success("LibCrypt status checking complete!"));
                    }

                    break;

                case RedumpSystem.SonyPlayStation2:
                    info.CommonDiscInfo.LanguageSelection = new LanguageSelection?[] { LanguageSelection.BiosSettings, LanguageSelection.LanguageSelector, LanguageSelection.OptionsMenu };
                    break;

                case RedumpSystem.SonyPlayStation3:
                    info.Extras.DiscKey = options.AddPlaceholders ? Template.RequiredValue : string.Empty;
                    info.Extras.DiscID = options.AddPlaceholders ? Template.RequiredValue : string.Empty;
                    break;

                case RedumpSystem.TomyKissSite:
                    info.CommonDiscInfo.Region = info.CommonDiscInfo.Region ?? Region.Japan;
                    break;

                case RedumpSystem.ZAPiTGamesGameWaveFamilyEntertainmentSystem:
                    info.CopyProtection.Protection = options.AddPlaceholders ? Template.RequiredIfExistsValue : string.Empty;
                    break;
            }

            // Set the category if it's not overriden
            info.CommonDiscInfo.Category = info.CommonDiscInfo.Category ?? DiscCategory.Games;

            // Comments and contents have odd handling
            if (string.IsNullOrEmpty(info.CommonDiscInfo.Comments))
                info.CommonDiscInfo.Comments = options.AddPlaceholders ? Template.OptionalValue : string.Empty;
            if (string.IsNullOrEmpty(info.CommonDiscInfo.Contents))
                info.CommonDiscInfo.Contents = options.AddPlaceholders ? Template.OptionalValue : string.Empty;

            // Normalize the disc type with all current information
            NormalizeDiscType(info);

            return info;
        }

        /// <summary>
        /// Ensures that all required output files have been created
        /// </summary>
        /// <param name="outputDirectory">Output folder to write to</param>
        /// <param name="outputFilename">Output filename to use as the base path</param>
        /// <param name="parameters">Parameters object representing what to send to the internal program</param>
        /// <param name="preCheck">True if this is a check done before a dump, false if done after</param>
        /// <returns>Tuple of true if all required files exist, false otherwise and a list representing missing files</returns>
        public static (bool, List<string>) FoundAllFiles(string outputDirectory, string outputFilename, BaseParameters parameters, bool preCheck)
        {
            // First, sanitized the output filename to strip off any potential extension
            outputFilename = Path.GetFileNameWithoutExtension(outputFilename);

            // Then get the base path for all checking
            string basePath = Path.Combine(outputDirectory, outputFilename);

            // Finally, let the parameters say if all files exist
            return parameters.CheckAllOutputFilesExist(basePath, preCheck);
        }

        /// <summary>
        /// Get the existence of an anti-modchip string from a PlayStation disc, if possible
        /// </summary>
        /// <param name="drive">Drive object representing the current drive</param>
        /// <returns>Anti-modchip existence if possible, false on error</returns>
        private static async Task<bool> GetAntiModchipDetected(Drive drive)
            => await Protection.GetPlayStationAntiModchipDetected($"{drive.Letter}:\\");

        /// <summary>
        /// Get the current detected copy protection(s), if possible
        /// </summary>
        /// <param name="drive">Drive object representing the current drive</param>
        /// <param name="options">Options object that determines what to scan</param>
        /// <param name="progress">Optional progress callback</param>
        /// <returns>Detected copy protection(s) if possible, null on error</returns>
        private static async Task<(string, Dictionary<string, List<string>>)> GetCopyProtection(Drive drive, Options options, IProgress<ProtectionProgress> progress = null)
        {
            if (options.ScanForProtection && drive != null)
            {
                (var protection, string _) = await Protection.RunProtectionScanOnPath($"{drive.Letter}:\\", options, progress);
                return (Protection.FormatProtections(protection), protection);
            }

            return ("(CHECK WITH PROTECTIONID)", null);
        }

        /// <summary>
        /// Get the full lines from the input file, if possible
        /// </summary>
        /// <param name="filename">file location</param>
        /// <param name="binary">True if should read as binary, false otherwise (default)</param>
        /// <returns>Full text of the file, null on error</returns>
        private static string GetFullFile(string filename, bool binary = false)
        {
            // If the file doesn't exist, we can't get info from it
            if (!File.Exists(filename))
                return null;

            // If we're reading as binary
            if (binary)
            {
                byte[] bytes = File.ReadAllBytes(filename);
                return BitConverter.ToString(bytes).Replace("-", string.Empty);
            }

            return string.Join("\n", File.ReadAllLines(filename));
        }

        /// <summary>
        /// Get the split values for ISO-based media
        /// </summary>
        /// <param name="hashData">String representing the combined hash data</param>
        /// <returns>True if extraction was successful, false otherwise</returns>
        private static bool GetISOHashValues(string hashData, out long size, out string crc32, out string md5, out string sha1)
        {
            size = -1; crc32 = null; md5 = null; sha1 = null;

            if (string.IsNullOrWhiteSpace(hashData))
                return false;

            Regex hashreg = new Regex(@"<rom name="".*?"" size=""(.*?)"" crc=""(.*?)"" md5=""(.*?)"" sha1=""(.*?)""");
            Match m = hashreg.Match(hashData);
            if (m.Success)
            {
                Int64.TryParse(m.Groups[1].Value, out size);
                crc32 = m.Groups[2].Value;
                md5 = m.Groups[3].Value;
                sha1 = m.Groups[4].Value;
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Get if LibCrypt data is detected in the subchannel file, if possible
        /// </summary>
        /// <param name="info">Base submission info to fill in specifics for</param>
        /// <param name="basePath">Base filename and path to use for checking</param>
        /// <returns>Status of the LibCrypt data, if possible</returns>
        private static void GetLibCryptDetected(SubmissionInfo info, string basePath)
        {
            bool? psLibCryptStatus = Protection.GetLibCryptDetected(basePath + ".sub");
            if (psLibCryptStatus == true)
            {
                // Guard against false positives
                if (File.Exists(basePath + "_subIntention.txt"))
                {
                    string libCryptData = GetFullFile(basePath + "_subIntention.txt") ?? "";
                    if (string.IsNullOrEmpty(libCryptData))
                    {
                        info.CopyProtection.LibCrypt = YesNo.No;
                    }
                    else
                    {
                        info.CopyProtection.LibCrypt = YesNo.Yes;
                        info.CopyProtection.LibCryptData = libCryptData;
                    }
                }
                else
                {
                    info.CopyProtection.LibCrypt = YesNo.No;
                }
            }
            else if (psLibCryptStatus == false)
            {
                info.CopyProtection.LibCrypt = YesNo.No;
            }
            else
            {
                info.CopyProtection.LibCrypt = YesNo.NULL;
                info.CopyProtection.LibCryptData = "LibCrypt could not be detected because subchannel file is missing";
            }
        }

        #endregion

        #region Information Output

        /// <summary>
        /// Compress log files to save space
        /// </summary>
        /// <param name="outputDirectory">Output folder to write to</param>
        /// <param name="outputFilename">Output filename to use as the base path</param>
        /// <param name="parameters">Parameters object to use to derive log file paths</param>
        /// <returns>True if the process succeeded, false otherwise</returns>
        public static (bool, string) CompressLogFiles(string outputDirectory, string outputFilename, BaseParameters parameters)
        {
            // Prepare the necessary paths
            outputFilename = Path.GetFileNameWithoutExtension(outputFilename);
            string combinedBase = Path.Combine(outputDirectory, outputFilename);
            string archiveName = combinedBase + "_logs.zip";

            // Get the list of log files from the parameters object
            var files = parameters.GetLogFilePaths(combinedBase);

            // Add on generated log files if they exist
            var mpfFiles = GetGeneratedFilePaths(outputDirectory);
            files.AddRange(mpfFiles);

            if (!files.Any())
                return (true, "No files to compress!");

            // If the file already exists, we want to delete the old one
            try
            {
                if (File.Exists(archiveName))
                    File.Delete(archiveName);
            }
            catch
            {
                return (false, "Could not delete old archive!");
            }

            // Add the log files to the archive and delete the uncompressed file after
            ZipArchive zf = null;
            try
            {
                zf = ZipFile.Open(archiveName, ZipArchiveMode.Create);
                foreach (string file in files)
                {
                    string entryName = file.Substring(outputDirectory.Length).TrimStart(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
                    zf.CreateEntryFromFile(file, entryName);

                    // If the file is MPF-specific, don't delete
                    if (mpfFiles.Contains(file))
                        continue;

                    try
                    {
                        File.Delete(file);
                    }
                    catch { }
                }

                return (true, "Compression complete!");
            }
            catch (Exception ex)
            {
                return (false, $"Compression could not complete: {ex}");
            }
            finally
            {
                zf?.Dispose();
            }
        }

        /// <summary>
        /// Format the output data in a human readable way, separating each printed line into a new item in the list
        /// </summary>
        /// <param name="info">Information object that should contain normalized values</param>
        /// <returns>List of strings representing each line of an output file, null on error</returns>
        public static (List<string>, string) FormatOutputData(SubmissionInfo info)
        {
            // Check to see if the inputs are valid
            if (info == null)
                return (null, "Submission information was missing");

            try
            {
                // Sony-printed discs have layers in the opposite order
                var system = info.CommonDiscInfo.System;
                bool reverseOrder = system.HasReversedRingcodes();

                // Common Disc Info section
                List<string> output = new List<string> { "Common Disc Info:" };
                AddIfExists(output, Template.TitleField, info.CommonDiscInfo.Title, 1);
                AddIfExists(output, Template.ForeignTitleField, info.CommonDiscInfo.ForeignTitleNonLatin, 1);
                AddIfExists(output, Template.DiscNumberField, info.CommonDiscInfo.DiscNumberLetter, 1);
                AddIfExists(output, Template.DiscTitleField, info.CommonDiscInfo.DiscTitle, 1);
                AddIfExists(output, Template.SystemField, info.CommonDiscInfo.System.LongName(), 1);
                AddIfExists(output, Template.MediaTypeField, GetFixedMediaType(
                        info.CommonDiscInfo.Media.ToMediaType(),
                        info.SizeAndChecksums.Size,
                        info.SizeAndChecksums.Layerbreak,
                        info.SizeAndChecksums.Layerbreak2,
                        info.SizeAndChecksums.Layerbreak3),
                    1);
                AddIfExists(output, Template.CategoryField, info.CommonDiscInfo.Category.LongName(), 1);
                AddIfExists(output, Template.FullyMatchingIDField, info.FullyMatchedID?.ToString(), 1);
                AddIfExists(output, Template.PartiallyMatchingIDsField, info.PartiallyMatchedIDs, 1);
                AddIfExists(output, Template.RegionField, info.CommonDiscInfo.Region.LongName() ?? "SPACE! (CHANGE THIS)", 1);
                AddIfExists(output, Template.LanguagesField, (info.CommonDiscInfo.Languages ?? new Language?[] { null }).Select(l => l.LongName() ?? "SILENCE! (CHANGE THIS)").ToArray(), 1);
                AddIfExists(output, Template.PlaystationLanguageSelectionViaField, (info.CommonDiscInfo.LanguageSelection ?? new LanguageSelection?[] { }).Select(l => l.LongName()).ToArray(), 1);
                AddIfExists(output, Template.DiscSerialField, info.CommonDiscInfo.Serial, 1);

                // All ringcode information goes in an indented area
                output.Add(""); output.Add("\tRingcode Information:");

                // If we have a triple-layer disc
                if (info.SizeAndChecksums.Layerbreak3 != default)
                {
                    AddIfExists(output, (reverseOrder ? "Layer 0 (Outer) " : "Layer 0 (Inner) ") + Template.MasteringRingField, info.CommonDiscInfo.Layer0MasteringRing, 2);
                    AddIfExists(output, (reverseOrder ? "Layer 0 (Outer) " : "Layer 0 (Inner) ") + Template.MasteringSIDField, info.CommonDiscInfo.Layer0MasteringSID, 2);
                    AddIfExists(output, (reverseOrder ? "Layer 0 (Outer) " : "Layer 0 (Inner) ") + Template.ToolstampField, info.CommonDiscInfo.Layer0ToolstampMasteringCode, 2);
                    AddIfExists(output, "Data Side " + Template.MouldSIDField, info.CommonDiscInfo.Layer0MouldSID, 2);
                    AddIfExists(output, "Data Side " + Template.AdditionalMouldField, info.CommonDiscInfo.Layer0AdditionalMould, 2);

                    AddIfExists(output, "Layer 1 " + Template.MasteringRingField, info.CommonDiscInfo.Layer1MasteringRing, 2);
                    AddIfExists(output, "Layer 1 " + Template.MasteringSIDField, info.CommonDiscInfo.Layer1MasteringSID, 2);
                    AddIfExists(output, "Layer 1 " + Template.ToolstampField, info.CommonDiscInfo.Layer1ToolstampMasteringCode, 2);
                    AddIfExists(output, "Label Side " + Template.MouldSIDField, info.CommonDiscInfo.Layer1MouldSID, 2);
                    AddIfExists(output, "Label Side " + Template.AdditionalMouldField, info.CommonDiscInfo.Layer1AdditionalMould, 2);

                    AddIfExists(output, "Layer 2 " + Template.MasteringRingField, info.CommonDiscInfo.Layer2MasteringRing, 2);
                    AddIfExists(output, "Layer 2 " + Template.MasteringSIDField, info.CommonDiscInfo.Layer2MasteringSID, 2);
                    AddIfExists(output, "Layer 2 " + Template.ToolstampField, info.CommonDiscInfo.Layer2ToolstampMasteringCode, 2);

                    AddIfExists(output, (reverseOrder ? "Layer 3 (Inner) " : "Layer 3 (Outer) ") + Template.MasteringRingField, info.CommonDiscInfo.Layer3MasteringRing, 2);
                    AddIfExists(output, (reverseOrder ? "Layer 3 (Inner) " : "Layer 3 (Outer) ") + Template.MasteringSIDField, info.CommonDiscInfo.Layer3MasteringSID, 2);
                    AddIfExists(output, (reverseOrder ? "Layer 3 (Inner) " : "Layer 3 (Outer) ") + Template.ToolstampField, info.CommonDiscInfo.Layer3ToolstampMasteringCode, 2);
                }
                // If we have a triple-layer disc
                else if (info.SizeAndChecksums.Layerbreak2 != default)
                {
                    AddIfExists(output, (reverseOrder ? "Layer 0 (Outer) " : "Layer 0 (Inner) ") + Template.MasteringRingField, info.CommonDiscInfo.Layer0MasteringRing, 2);
                    AddIfExists(output, (reverseOrder ? "Layer 0 (Outer) " : "Layer 0 (Inner) ") + Template.MasteringSIDField, info.CommonDiscInfo.Layer0MasteringSID, 2);
                    AddIfExists(output, (reverseOrder ? "Layer 0 (Outer) " : "Layer 0 (Inner) ") + Template.ToolstampField, info.CommonDiscInfo.Layer0ToolstampMasteringCode, 2);
                    AddIfExists(output, "Data Side " + Template.MouldSIDField, info.CommonDiscInfo.Layer0MouldSID, 2);
                    AddIfExists(output, "Data Side " + Template.AdditionalMouldField, info.CommonDiscInfo.Layer0AdditionalMould, 2);

                    AddIfExists(output, "Layer 1 " + Template.MasteringRingField, info.CommonDiscInfo.Layer1MasteringRing, 2);
                    AddIfExists(output, "Layer 1 " + Template.MasteringSIDField, info.CommonDiscInfo.Layer1MasteringSID, 2);
                    AddIfExists(output, "Layer 1 " + Template.ToolstampField, info.CommonDiscInfo.Layer1ToolstampMasteringCode, 2);
                    AddIfExists(output, "Label Side " + Template.MouldSIDField, info.CommonDiscInfo.Layer1MouldSID, 2);
                    AddIfExists(output, "Label Side " + Template.AdditionalMouldField, info.CommonDiscInfo.Layer1AdditionalMould, 2);

                    AddIfExists(output, (reverseOrder ? "Layer 2 (Inner) " : "Layer 2 (Outer) ") + Template.MasteringRingField, info.CommonDiscInfo.Layer2MasteringRing, 2);
                    AddIfExists(output, (reverseOrder ? "Layer 2 (Inner) " : "Layer 2 (Outer) ") + Template.MasteringSIDField, info.CommonDiscInfo.Layer2MasteringSID, 2);
                    AddIfExists(output, (reverseOrder ? "Layer 2 (Inner) " : "Layer 2 (Outer) ") + Template.ToolstampField, info.CommonDiscInfo.Layer2ToolstampMasteringCode, 2);
                }
                // If we have a dual-layer disc
                else if (info.SizeAndChecksums.Layerbreak != default)
                {
                    AddIfExists(output, (reverseOrder ? "Layer 0 (Outer) " : "Layer 0 (Inner) ") + Template.MasteringRingField, info.CommonDiscInfo.Layer0MasteringRing, 2);
                    AddIfExists(output, (reverseOrder ? "Layer 0 (Outer) " : "Layer 0 (Inner) ") + Template.MasteringSIDField, info.CommonDiscInfo.Layer0MasteringSID, 2);
                    AddIfExists(output, (reverseOrder ? "Layer 0 (Outer) " : "Layer 0 (Inner) ") + Template.ToolstampField, info.CommonDiscInfo.Layer0ToolstampMasteringCode, 2);
                    AddIfExists(output, "Data Side " + Template.MouldSIDField, info.CommonDiscInfo.Layer0MouldSID, 2);
                    AddIfExists(output, "Data Side " + Template.AdditionalMouldField, info.CommonDiscInfo.Layer0AdditionalMould, 2);

                    AddIfExists(output, (reverseOrder ? "Layer 1 (Inner) " : "Layer 1 (Outer) ") + Template.MasteringRingField, info.CommonDiscInfo.Layer1MasteringRing, 2);
                    AddIfExists(output, (reverseOrder ? "Layer 1 (Inner) " : "Layer 1 (Outer) ") + Template.MasteringSIDField, info.CommonDiscInfo.Layer1MasteringSID, 2);
                    AddIfExists(output, (reverseOrder ? "Layer 1 (Inner) " : "Layer 1 (Outer) ") + Template.ToolstampField, info.CommonDiscInfo.Layer1ToolstampMasteringCode, 2);
                    AddIfExists(output, "Label Side " + Template.MouldSIDField, info.CommonDiscInfo.Layer1MouldSID, 2);
                    AddIfExists(output, "Label Side " + Template.AdditionalMouldField, info.CommonDiscInfo.Layer1AdditionalMould, 2);
                }
                // If we have a single-layer disc
                else
                {
                    AddIfExists(output, "Data Side " + Template.MasteringRingField, info.CommonDiscInfo.Layer0MasteringRing, 2);
                    AddIfExists(output, "Data Side " + Template.MasteringSIDField, info.CommonDiscInfo.Layer0MasteringSID, 2);
                    AddIfExists(output, "Data Side " + Template.ToolstampField, info.CommonDiscInfo.Layer0ToolstampMasteringCode, 2);
                    AddIfExists(output, "Data Side " + Template.MouldSIDField, info.CommonDiscInfo.Layer0MouldSID, 2);
                    AddIfExists(output, "Data Side " + Template.AdditionalMouldField, info.CommonDiscInfo.Layer0AdditionalMould, 2);

                    AddIfExists(output, "Label Side " + Template.MasteringRingField, info.CommonDiscInfo.Layer1MasteringRing, 2);
                    AddIfExists(output, "Label Side " + Template.MasteringSIDField, info.CommonDiscInfo.Layer1MasteringSID, 2);
                    AddIfExists(output, "Label Side " + Template.ToolstampField, info.CommonDiscInfo.Layer1ToolstampMasteringCode, 2);
                    AddIfExists(output, "Label Side " + Template.MouldSIDField, info.CommonDiscInfo.Layer1MouldSID, 2);
                    AddIfExists(output, "Label Side " + Template.AdditionalMouldField, info.CommonDiscInfo.Layer1AdditionalMould, 2);
                }

                AddIfExists(output, Template.BarcodeField, info.CommonDiscInfo.Barcode, 1);
                AddIfExists(output, Template.EXEDateBuildDate, info.CommonDiscInfo.EXEDateBuildDate, 1);
                AddIfExists(output, Template.ErrorCountField, info.CommonDiscInfo.ErrorsCount, 1);
                AddIfExists(output, Template.CommentsField, info.CommonDiscInfo.Comments.Trim(), 1);
                AddIfExists(output, Template.ContentsField, info.CommonDiscInfo.Contents.Trim(), 1);

                // Version and Editions section
                output.Add(""); output.Add("Version and Editions:");
                AddIfExists(output, Template.VersionField, info.VersionAndEditions.Version, 1);
                AddIfExists(output, Template.EditionField, info.VersionAndEditions.OtherEditions, 1);

                // EDC section
                if (info.CommonDiscInfo.System == RedumpSystem.SonyPlayStation)
                {
                    output.Add(""); output.Add("EDC:");
                    AddIfExists(output, Template.PlayStationEDCField, info.EDC.EDC.LongName(), 1);
                }

                // Parent/Clone Relationship section
                // output.Add(""); output.Add("Parent/Clone Relationship:");
                // AddIfExists(output, Template.ParentIDField, info.ParentID);
                // AddIfExists(output, Template.RegionalParentField, info.RegionalParent.ToString());

                // Extras section
                if (info.Extras.PVD != null || info.Extras.PIC != null || info.Extras.BCA != null)
                {
                    output.Add(""); output.Add("Extras:");
                    AddIfExists(output, Template.PVDField, info.Extras.PVD?.Trim(), 1);
                    AddIfExists(output, Template.PlayStation3WiiDiscKeyField, info.Extras.DiscKey, 1);
                    AddIfExists(output, Template.PlayStation3DiscIDField, info.Extras.DiscID, 1);
                    AddIfExists(output, Template.PICField, info.Extras.PIC, 1);
                    AddIfExists(output, Template.HeaderField, info.Extras.Header, 1);
                    AddIfExists(output, Template.GameCubeWiiBCAField, info.Extras.BCA, 1);
                    AddIfExists(output, Template.XBOXSSRanges, info.Extras.SecuritySectorRanges, 1);
                }

                // Copy Protection section
                if (!string.IsNullOrWhiteSpace(info.CopyProtection.Protection)
                    || (info.CopyProtection.AntiModchip != null && info.CopyProtection.AntiModchip != YesNo.NULL)
                    || (info.CopyProtection.LibCrypt != null && info.CopyProtection.LibCrypt != YesNo.NULL)
                    || !string.IsNullOrWhiteSpace(info.CopyProtection.LibCryptData)
                    || !string.IsNullOrWhiteSpace(info.CopyProtection.SecuROMData))
                {
                    output.Add(""); output.Add("Copy Protection:");
                    if (info.CommonDiscInfo.System == RedumpSystem.SonyPlayStation)
                    {
                        AddIfExists(output, Template.PlayStationAntiModchipField, info.CopyProtection.AntiModchip.LongName(), 1);
                        AddIfExists(output, Template.PlayStationLibCryptField, info.CopyProtection.LibCrypt.LongName(), 1);
                        AddIfExists(output, Template.SubIntentionField, info.CopyProtection.LibCryptData, 1);
                    }

                    AddIfExists(output, Template.CopyProtectionField, info.CopyProtection.Protection, 1);
                    AddIfExists(output, Template.SubIntentionField, info.CopyProtection.SecuROMData, 1);
                }

                // Dumpers and Status section
                // output.Add(""); output.Add("Dumpers and Status");
                // AddIfExists(output, Template.StatusField, info.Status.Name());
                // AddIfExists(output, Template.OtherDumpersField, info.OtherDumpers);

                // Tracks and Write Offsets section
                if (!string.IsNullOrWhiteSpace(info.TracksAndWriteOffsets.ClrMameProData))
                {
                    output.Add(""); output.Add("Tracks and Write Offsets:");
                    AddIfExists(output, Template.DATField, info.TracksAndWriteOffsets.ClrMameProData + "\n", 1);
                    AddIfExists(output, Template.CuesheetField, info.TracksAndWriteOffsets.Cuesheet, 1);
                    string offset = info.TracksAndWriteOffsets.OtherWriteOffsets;
                    if (offset?.StartsWith("-") == false)
                        offset = $"+{offset}";

                    AddIfExists(output, Template.WriteOffsetField, offset, 1);
                }
                // Size & Checksum section
                else
                {
                    output.Add(""); output.Add("Size & Checksum:");
                    AddIfExists(output, Template.LayerbreakField, (info.SizeAndChecksums.Layerbreak == default ? null : info.SizeAndChecksums.Layerbreak.ToString()), 1);
                    AddIfExists(output, Template.SizeField, info.SizeAndChecksums.Size.ToString(), 1);
                    AddIfExists(output, Template.CRC32Field, info.SizeAndChecksums.CRC32, 1);
                    AddIfExists(output, Template.MD5Field, info.SizeAndChecksums.MD5, 1);
                    AddIfExists(output, Template.SHA1Field, info.SizeAndChecksums.SHA1, 1);
                }

                // Dumping Info section
                output.Add(""); output.Add("Dumping Info:");
                AddIfExists(output, Template.DumpingProgramField, info.DumpingInfo.DumpingProgram, 1);
                AddIfExists(output, Template.DumpingDriveManufacturer, info.DumpingInfo.Manufacturer, 1);
                AddIfExists(output, Template.DumpingDriveModel, info.DumpingInfo.Model, 1);
                AddIfExists(output, Template.DumpingDriveFirmware, info.DumpingInfo.Firmware, 1);
                AddIfExists(output, Template.ReportedDiscType, info.DumpingInfo.ReportedDiscType, 1);

                // Make sure there aren't any instances of two blank lines in a row
                string last = null;
                for (int i = 0; i < output.Count;)
                {
                    if (output[i] == last && string.IsNullOrWhiteSpace(last))
                    {
                        output.RemoveAt(i);
                    }
                    else
                    {
                        last = output[i];
                        i++;
                    }
                }

                return (output, "Formatting complete!");
            }
            catch (Exception ex)
            {
                return (null, $"Error formatting submission info: {ex}");
            }
        }

        /// <summary>
        /// Get the adjusted name of the media based on layers, if applicable
        /// </summary>
        /// <param name="mediaType">MediaType to get the proper name for</param>
        /// <param name="size">Size of the current media</param>
        /// <param name="layerbreak">First layerbreak value, as applicable</param>
        /// <param name="layerbreak2">Second layerbreak value, as applicable</param>
        /// <param name="layerbreak3">Third layerbreak value, as applicable</param>
        /// <returns>String representation of the media, including layer specification</returns>
        public static string GetFixedMediaType(MediaType? mediaType, long size, long layerbreak, long layerbreak2, long layerbreak3)
        {
            switch (mediaType)
            {
                case MediaType.DVD:
                    if (layerbreak != default)
                        return $"{mediaType.LongName()}-9";
                    else
                        return $"{mediaType.LongName()}-5";

                case MediaType.BluRay:
                    if (layerbreak3 != default)
                        return $"{mediaType.LongName()}-128";
                    else if (layerbreak2 != default)
                        return $"{mediaType.LongName()}-100";
                    //else if (layerbreak != default && size > 53_687_063_712)
                    //    return $"{mediaType.LongName()}-66";
                    else if (layerbreak != default)
                        return $"{mediaType.LongName()}-50";
                    //else if (size > 26_843_531_856)
                    //    return $"{mediaType.LongName()}-33";
                    else
                        return $"{mediaType.LongName()}-25";

                case MediaType.UMD:
                    if (layerbreak != default)
                        return $"{mediaType.LongName()}-DL";
                    else
                        return $"{mediaType.LongName()}-SL";

                default:
                    return mediaType.LongName();
            }
        }

        /// <summary>
        /// Process any fields that have to be combined
        /// </summary>
        /// <param name="info">Information object to normalize</param>
        public static void ProcessSpecialFields(SubmissionInfo info)
        {
            // Process the comments field
            if (info.CommonDiscInfo?.CommentsSpecialFields != null && info.CommonDiscInfo.CommentsSpecialFields?.Any() == true)
            {
                // If the field is missing, add an empty one to fill in
                if (info.CommonDiscInfo.Comments == null)
                    info.CommonDiscInfo.Comments = string.Empty;

                // Add all special fields before any comments
                info.CommonDiscInfo.Comments = string.Join(
                    "\n", OrderCommentTags(info.CommonDiscInfo.CommentsSpecialFields)
                        .Where(kvp => !string.IsNullOrWhiteSpace(kvp.Value))
                        .Select(FormatSiteTag)
                        .Where(s => !string.IsNullOrEmpty(s))
                ) + "\n" + info.CommonDiscInfo.Comments;

                // Normalize newlines
                info.CommonDiscInfo.Comments = info.CommonDiscInfo.Comments.Replace("\r\n", "\n");

                // Trim the comments field
                info.CommonDiscInfo.Comments = info.CommonDiscInfo.Comments.Trim();

                // Wipe out the special fields dictionary
                info.CommonDiscInfo.CommentsSpecialFields = null;
            }

            // Process the contents field
            if (info.CommonDiscInfo?.ContentsSpecialFields != null && info.CommonDiscInfo.ContentsSpecialFields?.Any() == true)
            {
                // If the field is missing, add an empty one to fill in
                if (info.CommonDiscInfo.Contents == null)
                    info.CommonDiscInfo.Contents = string.Empty;

                // Add all special fields before any contents
                info.CommonDiscInfo.Contents = string.Join(
                    "\n", OrderContentTags(info.CommonDiscInfo.ContentsSpecialFields)
                        .Where(kvp => !string.IsNullOrWhiteSpace(kvp.Value))
                        .Select(FormatSiteTag)
                        .Where(s => !string.IsNullOrEmpty(s))
                ) + "\n" + info.CommonDiscInfo.Contents;

                // Normalize newlines
                info.CommonDiscInfo.Contents = info.CommonDiscInfo.Contents.Replace("\r\n", "\n");

                // Trim the contents field
                info.CommonDiscInfo.Contents = info.CommonDiscInfo.Contents.Trim();

                // Wipe out the special fields dictionary
                info.CommonDiscInfo.ContentsSpecialFields = null;
            }
        }

        /// <summary>
        /// Write the data to the output folder
        /// </summary>
        /// <param name="outputDirectory">Output folder to write to</param>
        /// <param name="lines">Preformatted list of lines to write out to the file</param>
        /// <returns>True on success, false on error</returns>
        public static (bool, string) WriteOutputData(string outputDirectory, List<string> lines)
        {
            // Check to see if the inputs are valid
            if (lines == null)
                return (false, "No formatted data found to write!");

            // Now write out to a generic file
            try
            {
                using (StreamWriter sw = new StreamWriter(File.Open(Path.Combine(outputDirectory, "!submissionInfo.txt"), FileMode.Create, FileAccess.Write)))
                {
                    foreach (string line in lines)
                    {
                        sw.WriteLine(line);
                    }
                }
            }
            catch (Exception ex)
            {
                return (false, $"Writing could not complete: {ex}");
            }

            return (true, "Writing complete!");
        }

        /// <summary>
        /// Write the data to the output folder
        /// </summary>
        /// <param name="outputDirectory">Output folder to write to</param>
        /// <param name="info">SubmissionInfo object representing the JSON to write out to the file</param>
        /// <param name="includedArtifacts">True if artifacts were included, false otherwise</param>
        /// <returns>True on success, false on error</returns>
        public static bool WriteOutputData(string outputDirectory, SubmissionInfo info, bool includedArtifacts)
        {
            // Check to see if the input is valid
            if (info == null)
                return false;

            try
            {
                // Serialize the JSON and get it writable
                string json = JsonConvert.SerializeObject(info, Formatting.Indented);
                byte[] jsonBytes = Encoding.UTF8.GetBytes(json);

                // If we included artifacts, write to a GZip-compressed file
                if (includedArtifacts)
                {
                    using (var fs = File.Create(Path.Combine(outputDirectory, "!submissionInfo.json.gz")))
                    using (var gs = new GZipStream(fs, CompressionMode.Compress))
                    {
                        gs.Write(jsonBytes, 0, jsonBytes.Length);
                    }
                }

                // Otherwise, write out to a normal JSON
                else
                {
                    using (var fs = File.Create(Path.Combine(outputDirectory, "!submissionInfo.json")))
                    {
                        fs.Write(jsonBytes, 0, jsonBytes.Length);
                    }
                }
            }
            catch (Exception ex)
            {
                // We don't care what the error is right now
                return false;
            }

            return true;
        }

        /// <summary>
        /// Write the protection data to the output folder
        /// </summary>
        /// <param name="outputDirectory">Output folder to write to</param>
        /// <param name="info">SubmissionInfo object containing the protection information</param>
        /// <returns>True on success, false on error</returns>
        public static bool WriteProtectionData(string outputDirectory, SubmissionInfo info)
        {
            // Check to see if the inputs are valid
            if (info?.CopyProtection?.FullProtections == null || !info.CopyProtection.FullProtections.Any())
                return true;

            // Now write out to a generic file
            try
            {
                using (StreamWriter sw = new StreamWriter(File.Open(Path.Combine(outputDirectory, "!protectionInfo.txt"), FileMode.Create, FileAccess.Write)))
                {
                    foreach (var kvp in info.CopyProtection.FullProtections)
                    {
                        sw.WriteLine($"{kvp.Key}: {string.Join(", ", kvp.Value)}");
                    }
                }
            }
            catch (Exception ex)
            {
                // We don't care what the error is right now
                return false;
            }

            return true;
        }

        /// <summary>
        /// Add the properly formatted key and value, if possible
        /// </summary>
        /// <param name="output">Output list</param>
        /// <param name="key">Name of the output key to write</param>
        /// <param name="value">Name of the output value to write</param>
        /// <param name="indent">Number of tabs to indent the line</param>
        private static void AddIfExists(List<string> output, string key, string value, int indent)
        {
            // If there's no valid value to write
            if (value == null)
                return;

            string prefix = "";
            for (int i = 0; i < indent; i++)
                prefix += "\t";

            // Skip fields that need to keep internal whitespace intact
            if (key != "Primary Volume Descriptor (PVD)"
                && key != "Header"
                && key != "Cuesheet")
            {
                // Convert to tabs
                value = value.Replace("<tab>", "\t");
                value = value.Replace("<TAB>", "\t");
                value = value.Replace("   ", "\t");

                // Sanitize whitespace around tabs
                value = Regex.Replace(value, @"\s*\t\s*", "\t");
            }

            // If the value contains a newline
            value = value.Replace("\r\n", "\n");
            if (value.Contains("\n"))
            {
                output.Add(prefix + key + ":"); output.Add("");
                string[] values = value.Split('\n');
                foreach (string val in values)
                    output.Add(val);

                output.Add("");
            }

            // For all regular values
            else
            {
                output.Add(prefix + key + ": " + value);
            }
        }

        /// <summary>
        /// Add the properly formatted key and value, if possible
        /// </summary>
        /// <param name="output">Output list</param>
        /// <param name="key">Name of the output key to write</param>
        /// <param name="value">Name of the output value to write</param>
        /// <param name="indent">Number of tabs to indent the line</param>
        private static void AddIfExists(List<string> output, string key, string[] value, int indent)
        {
            // If there's no valid value to write
            if (value == null || value.Length == 0)
                return;

            AddIfExists(output, key, string.Join(", ", value), indent);
        }

        /// <summary>
        /// Add the properly formatted key and value, if possible
        /// </summary>
        /// <param name="output">Output list</param>
        /// <param name="key">Name of the output key to write</param>
        /// <param name="value">Name of the output value to write</param>
        /// <param name="indent">Number of tabs to indent the line</param>
        private static void AddIfExists(List<string> output, string key, List<int> value, int indent)
        {
            // If there's no valid value to write
            if (value == null || value.Count() == 0)
                return;

            AddIfExists(output, key, string.Join(", ", value.Select(o => o.ToString())), indent);
        }

        /// <summary>
        /// Generate a list of all MPF-specific log files generated
        /// </summary>
        /// <param name="outputDirectory">Output folder to write to</param>
        /// <returns>List of all log file paths, empty otherwise</returns>
        private static List<string> GetGeneratedFilePaths(string outputDirectory)
        {
            List<string> files = new List<string>();

            if (File.Exists(Path.Combine(outputDirectory, "!submissionInfo.txt")))
                files.Add(Path.Combine(outputDirectory, "!submissionInfo.txt"));
            if (File.Exists(Path.Combine(outputDirectory, "!submissionInfo.json")))
                files.Add(Path.Combine(outputDirectory, "!submissionInfo.json"));
            if (File.Exists(Path.Combine(outputDirectory, "!submissionInfo.json.gz")))
                files.Add(Path.Combine(outputDirectory, "!submissionInfo.json.gz"));
            if (File.Exists(Path.Combine(outputDirectory, "!protectionInfo.txt")))
                files.Add(Path.Combine(outputDirectory, "!protectionInfo.txt"));

            return files;
        }

        #endregion

        #region Normalization

        /// <summary>
        /// Adjust a disc title so that it will be processed correctly by Redump
        /// </summary>
        /// <param name="title">Existing title to potentially reformat</param>
        /// <param name="languages">Array of languages to use for assuming articles</param>
        /// <returns>The reformatted title</returns>
        public static string NormalizeDiscTitle(string title, Language[] languages)
        {
            // If we have no set languages, then assume English
            if (languages == null || languages.Length == 0)
                languages = new Language[] { Language.English };

            // Loop through all of the given languages
            foreach (var language in languages)
            {
                // If the new title is different, assume it was normalized and return it
                string newTitle = NormalizeDiscTitle(title, language);
                if (newTitle == title)
                    return newTitle;
            }

            // If we didn't already try English, try it now
            if (!languages.Contains(Language.English))
                return NormalizeDiscTitle(title, Language.English);

            // If all fails, then the title didn't need normalization
            return title;
        }

        /// <summary>
        /// Adjust a disc title so that it will be processed correctly by Redump
        /// </summary>
        /// <param name="title">Existing title to potentially reformat</param>
        /// <param name="language">Language to use for assuming articles</param>
        /// <returns>The reformatted title</returns>
        /// <remarks>
        /// If the language of the title is unknown or if it's multilingual,
        /// pass in Language.English for standardized coverage.
        /// </remarks>
        public static string NormalizeDiscTitle(string title, Language language)
        {
            // If we have an invalid title, just return it as-is
            if (string.IsNullOrWhiteSpace(title))
                return title;

            // Get the title split into parts
            string[] splitTitle = title.Split(' ').Where(s => !string.IsNullOrWhiteSpace(s)).ToArray();

            // If we only have one part, we can't do anything
            if (splitTitle.Length <= 1)
                return title;

            // Determine if we have a definite or indefinite article as the first item
            string firstItem = splitTitle[0];
            switch (firstItem.ToLowerInvariant())
            {
                // Latin script articles
                case "'n"
                    when language is Language.Manx:
                case "a"
                    when language is Language.English
                        || language is Language.Hungarian
                        || language is Language.Portuguese
                        || language is Language.Scots:
                case "a'"
                    when language is Language.English
                        || language is Language.Hungarian
                        || language is Language.Irish
                        || language is Language.Gaelic:     // Scottish Gaelic
                case "al"
                    when language is Language.Breton:
                case "am"
                    when language is Language.Gaelic:       // Scottish Gaelic
                case "an"
                    when language is Language.Breton
                        || language is Language.Cornish
                        || language is Language.English
                        || language is Language.Irish
                        || language is Language.Gaelic:     // Scottish Gaelic
                case "anek"
                    when language is Language.Nepali:
                case "ar"
                    when language is Language.Breton:
                case "az"
                    when language is Language.Hungarian:
                case "ān"
                    when language is Language.Persian:
                case "as"
                    when language is Language.Portuguese:
                case "d'"
                    when language is Language.Luxembourgish:
                case "das"
                    when language is Language.German:
                case "dat"
                    when language is Language.Luxembourgish:
                case "de"
                    when language is Language.Dutch:
                case "déi"
                    when language is Language.Luxembourgish:
                case "dem"
                    when language is Language.German
                        || language is Language.Luxembourgish:
                case "den"
                    when language is Language.Dutch
                        || language is Language.German
                        || language is Language.Luxembourgish:
                case "der"
                    when language is Language.Dutch
                        || language is Language.German
                        || language is Language.Luxembourgish:
                case "des"
                    when language is Language.Dutch
                        || language is Language.French
                        || language is Language.German:
                case "die"
                    when language is Language.Afrikaans
                        || language is Language.German:
                case "e"
                    when language is Language.Papiamento:
                case "een"
                    when language is Language.Dutch:
                case "egy"
                    when language is Language.Hungarian:
                case "ei"
                    when language is Language.Norwegian:
                case "ein"
                    when language is Language.German
                        || language is Language.Norwegian:
                case "eine"
                    when language is Language.German:
                case "einem"
                    when language is Language.German:
                case "einen"
                    when language is Language.German:
                case "einer"
                    when language is Language.German:
                case "eines"
                    when language is Language.German:
                case "eit"
                    when language is Language.Norwegian:
                case "ek"
                    when language is Language.Nepali:
                case "el"
                    when language is Language.Arabic
                        || language is Language.Catalan
                        || language is Language.Spanish:
                case "els"
                    when language is Language.Catalan:
                case "en"
                    when language is Language.Danish
                        || language is Language.Luxembourgish
                        || language is Language.Norwegian
                        || language is Language.Swedish:
                case "eng"
                    when language is Language.Luxembourgish:
                case "engem"
                    when language is Language.Luxembourgish:
                case "enger"
                    when language is Language.Luxembourgish:
                case "es"
                    when language is Language.Catalan:
                case "et"
                    when language is Language.Danish
                        || language is Language.Norwegian:
                case "ett"
                    when language is Language.Swedish:
                case "euta"
                    when language is Language.Nepali:
                case "euti"
                    when language is Language.Nepali:
                case "gli"
                    when language is Language.Italian:
                case "he"
                    when language is Language.Hawaiian
                        || language is Language.Maori:
                case "het"
                    when language is Language.Dutch:
                case "i"
                    when language is Language.Italian
                        || language is Language.Khasi:
                case "il"
                    when language is Language.Italian:
                case "in"
                    when language is Language.Persian:
                case "ka"
                    when language is Language.Hawaiian
                        || language is Language.Khasi:
                case "ke"
                    when language is Language.Hawaiian:
                case "ki"
                    when language is Language.Khasi:
                case "kunai"
                    when language is Language.Nepali:
                case "l'"
                    when language is Language.Catalan
                        || language is Language.French
                        || language is Language.Italian:
                case "la"
                    when language is Language.Catalan
                        || language is Language.Esperanto
                        || language is Language.French
                        || language is Language.Italian
                        || language is Language.Spanish:
                case "las"
                    when language is Language.Spanish:
                case "le"
                    when language is Language.French
                        || language is Language.Interlingua
                        || language is Language.Italian:
                case "les"
                    when language is Language.Catalan
                        || language is Language.French:
                case "lo"
                    when language is Language.Catalan
                        || language is Language.Italian
                        || language is Language.Spanish:
                case "los"
                    when language is Language.Catalan
                        || language is Language.Spanish:
                case "na"
                    when language is Language.Irish
                        || language is Language.Gaelic:     // Scottish Gaelic
                case "nam"
                    when language is Language.Gaelic:       // Scottish Gaelic
                case "nan"
                    when language is Language.Gaelic:       // Scottish Gaelic
                case "nā"
                    when language is Language.Hawaiian:
                case "ngā"
                    when language is Language.Maori:
                case "niște"
                    when language is Language.Romanian:
                case "ny"
                    when language is Language.Manx:
                case "o"
                    when language is Language.Portuguese
                        || language is Language.Romanian:
                case "os"
                    when language is Language.Portuguese:
                case "sa"
                    when language is Language.Catalan:
                case "sang"
                    when language is Language.Malay:
                case "se"
                    when language is Language.Finnish:
                case "ses"
                    when language is Language.Catalan:
                case "si"
                    when language is Language.Malay:
                case "te"
                    when language is Language.Maori:
                case "the"
                    when language is Language.English
                        || language is Language.Scots:
                case "u"
                    when language is Language.Khasi:
                case "ul"
                    when language is Language.Breton:
                case "um"
                    when language is Language.Portuguese:
                case "uma"
                    when language is Language.Portuguese:
                case "umas"
                    when language is Language.Portuguese:
                case "un"
                    when language is Language.Breton
                        || language is Language.Catalan
                        || language is Language.French
                        || language is Language.Interlingua
                        || language is Language.Italian
                        || language is Language.Papiamento
                        || language is Language.Romanian
                        || language is Language.Spanish:
                case "un'"
                    when language is Language.Italian:
                case "una"
                    when language is Language.Catalan
                        || language is Language.Italian:
                case "unas"
                    when language is Language.Spanish:
                case "une"
                    when language is Language.French:
                case "uno"
                    when language is Language.Italian:
                case "unos"
                    when language is Language.Spanish:
                case "uns"
                    when language is Language.Catalan
                        || language is Language.Portuguese:
                case "unei"
                    when language is Language.Romanian:
                case "unes"
                    when language is Language.Catalan:
                case "unor"
                    when language is Language.Romanian:
                case "unui"
                    when language is Language.Romanian:
                case "ur"
                    when language is Language.Breton:
                case "y"
                    when language is Language.Manx
                        || language is Language.Welsh:
                case "ye"
                    when language is Language.Persian:
                case "yek"
                    when language is Language.Persian:
                case "yn"
                    when language is Language.Manx:
                case "yr"
                    when language is Language.Welsh:

                // Non-latin script articles
                case "ο"
                    when language is Language.Greek:
                case "η"
                    when language is Language.Greek:
                case "το"
                    when language is Language.Greek:
                case "οι"
                    when language is Language.Greek:
                case "τα"
                    when language is Language.Greek:
                case "ένας"
                    when language is Language.Greek:
                case "μια"
                    when language is Language.Greek:
                case "ένα"
                    when language is Language.Greek:
                case "еден"
                    when language is Language.Macedonian:
                case "една"
                    when language is Language.Macedonian:
                case "едно"
                    when language is Language.Macedonian:
                case "едни"
                    when language is Language.Macedonian:
                case "एउटा"
                    when language is Language.Nepali:
                case "एउटी"
                    when language is Language.Nepali:
                case "एक"
                    when language is Language.Nepali:
                case "अनेक"
                    when language is Language.Nepali:
                case "कुनै"
                    when language is Language.Nepali:
                case "דער"
                    when language is Language.Yiddish:
                case "די"
                    when language is Language.Yiddish:
                case "דאָס"
                    when language is Language.Yiddish:
                case "דעם"
                    when language is Language.Yiddish:
                case "אַ"
                    when language is Language.Yiddish:
                case "אַן"
                    when language is Language.Yiddish:

                // Seen by Redump, unknown origin
                case "du":
                    break;

                // Otherwise, just return it as-is
                default:
                    return title;
            }

            // Insert the first item if we have a `:` or `-`
            bool itemInserted = false;
            StringBuilder newTitleBuilder = new StringBuilder();
            for (int i = 1; i < splitTitle.Length; i++)
            {
                string segment = splitTitle[i];
                if (segment.EndsWith(":") || segment.EndsWith("-"))
                {
                    itemInserted = true;
                    newTitleBuilder.Append($"{segment}, {firstItem}");
                }
                else
                {
                    newTitleBuilder.Append($"{segment} ");
                }
            }

            // If we didn't insert the item yet, add it to the end
            string newTitle = newTitleBuilder.ToString().Trim();
            if (!itemInserted)
                newTitle = $"{newTitle}, {firstItem}";

            return newTitle;
        }

        /// <summary>
        /// Adjust the disc type based on size and layerbreak information
        /// </summary>
        /// <param name="info">Existing SubmissionInfo object to fill</param>
        /// <returns>Corrected disc type, if possible</returns>
        public static void NormalizeDiscType(SubmissionInfo info)
        {
            // If we have nothing valid, do nothing
            if (info?.CommonDiscInfo?.Media == null)
                return;

            switch (info.CommonDiscInfo.Media)
            {
                case DiscType.BD25:
                //case DiscType.BD33:
                case DiscType.BD50:
                //case DiscType.BD66:
                case DiscType.BD100:
                case DiscType.BD128:
                    if (info.SizeAndChecksums.Layerbreak3 != default)
                        info.CommonDiscInfo.Media = DiscType.BD128;
                    else if (info.SizeAndChecksums.Layerbreak2 != default)
                        info.CommonDiscInfo.Media = DiscType.BD100;
                    //else if (info.SizeAndChecksums.Layerbreak != default && info.SizeAndChecksums.Size > 53_687_063_712)
                    //    info.CommonDiscInfo.Media = DiscType.BD66;
                    else if (info.SizeAndChecksums.Layerbreak != default)
                        info.CommonDiscInfo.Media = DiscType.BD50;
                    //else if (info.SizeAndChecksums.Size > 26_843_531_856)
                    //    info.CommonDiscInfo.Media = DiscType.BD33;
                    else
                        info.CommonDiscInfo.Media = DiscType.BD25;
                    break;

                case DiscType.DVD5:
                case DiscType.DVD9:
                    if (info.SizeAndChecksums.Layerbreak != default)
                        info.CommonDiscInfo.Media = DiscType.DVD9;
                    else
                        info.CommonDiscInfo.Media = DiscType.DVD5;
                    break;

                case DiscType.HDDVDSL:
                case DiscType.HDDVDDL:
                    if (info.SizeAndChecksums.Layerbreak != default)
                        info.CommonDiscInfo.Media = DiscType.HDDVDDL;
                    else
                        info.CommonDiscInfo.Media = DiscType.HDDVDSL;
                    break;

                case DiscType.UMDSL:
                case DiscType.UMDDL:
                    if (info.SizeAndChecksums.Layerbreak != default)
                        info.CommonDiscInfo.Media = DiscType.UMDDL;
                    else
                        info.CommonDiscInfo.Media = DiscType.UMDSL;
                    break;

                // All other disc types are not processed
                default:
                    break;
            }
        }

        /// <summary>
        /// Normalize a split set of paths
        /// </summary>
        /// <param name="directory">Directory name to normalize</param>
        /// <param name="filename">Filename to normalize</param>
        public static (string, string) NormalizeOutputPaths(string directory, string filename)
        {
            try
            {
                // Cache if we had a directory separator or not
                bool endedWithDirectorySeparator = directory.EndsWith(Path.DirectorySeparatorChar.ToString())
                    || directory.EndsWith(Path.AltDirectorySeparatorChar.ToString());

                // Combine the path to make things separate easier
                string combinedPath = Path.Combine(directory, filename);

                // If we have have a blank path, just return
                if (string.IsNullOrWhiteSpace(combinedPath))
                    return (directory, filename);

                // Now get the normalized paths
                directory = Path.GetDirectoryName(combinedPath);
                filename = Path.GetFileName(combinedPath);

                // Take care of extra path characters
                directory = new StringBuilder(directory)
                    .Replace(':', '_', 0, directory.LastIndexOf(':') == -1 ? 0 : directory.LastIndexOf(':'))
                    .ToString();

                // Sanitize the directory path
                directory = directory.Replace('?', '_');
                foreach (char c in Path.GetInvalidPathChars())
                    directory = directory.Replace(c, '_');

                // Sanitize the filename
                filename = filename.Replace('?', '_');
                foreach (char c in Path.GetInvalidFileNameChars())
                    filename = filename.Replace(c, '_');

                // If we had a directory separator at the end before, add it again
                if (endedWithDirectorySeparator)
                    directory += Path.DirectorySeparatorChar;

                // If we have a root directory, sanitize
                if (Directory.Exists(directory))
                {
                    var possibleRootDir = new DirectoryInfo(directory);
                    if (possibleRootDir.Parent == null)
                        directory = directory.Replace($"{Path.DirectorySeparatorChar}{Path.DirectorySeparatorChar}", $"{Path.DirectorySeparatorChar}");
                }
            }
            catch { }

            return (directory, filename);
        }

        #endregion

        #region Web Calls

        /// <summary>
        /// Create a new SubmissionInfo object from a disc page
        /// </summary>
        /// <param name="discData">String containing the HTML disc data</param>
        /// <returns>Filled SubmissionInfo object on success, null on error</returns>
        /// <remarks>Not currently working</remarks>
        private static SubmissionInfo CreateFromID(string discData)
        {
            SubmissionInfo info = new SubmissionInfo()
            {
                CommonDiscInfo = new CommonDiscInfoSection(),
                VersionAndEditions = new VersionAndEditionsSection(),
            };

            // No disc data means we can't parse it
            if (string.IsNullOrWhiteSpace(discData))
                return null;

            try
            {
                // Load the current disc page into an XML document
                XmlDocument redumpPage = new XmlDocument() { PreserveWhitespace = true };
                redumpPage.LoadXml(discData);

                // If the current page isn't valid, we can't parse it
                if (!redumpPage.HasChildNodes)
                    return null;

                // Get the body node, if possible
                XmlNode bodyNode = redumpPage["html"]?["body"];
                if (bodyNode == null || !bodyNode.HasChildNodes)
                    return null;

                // Loop through and get the main node, if possible
                XmlNode mainNode = null;
                foreach (XmlNode tempNode in bodyNode.ChildNodes)
                {
                    // We only care about div elements
                    if (!string.Equals(tempNode.Name, "div", StringComparison.OrdinalIgnoreCase))
                        continue;

                    // We only care if it has attributes
                    if (tempNode.Attributes == null)
                        continue;

                    // The main node has a class of "main"
                    if (string.Equals(tempNode.Attributes["class"]?.Value, "main", StringComparison.OrdinalIgnoreCase))
                    {
                        mainNode = tempNode;
                        break;
                    }
                }

                // If the main node is invalid, we can't do anything
                if (mainNode == null || !mainNode.HasChildNodes)
                    return null;

                // Try to find elements as we're going
                foreach (XmlNode childNode in mainNode.ChildNodes)
                {
                    // The title is the only thing in h1 tags
                    if (string.Equals(childNode.Name, "h1", StringComparison.OrdinalIgnoreCase))
                        info.CommonDiscInfo.Title = childNode.InnerText;

                    // Most things are div elements but can be hard to parse out
                    else if (!string.Equals(childNode.Name, "div", StringComparison.OrdinalIgnoreCase))
                        continue;

                    // Only 2 of the internal divs have classes attached and one is not used here
                    if (childNode.Attributes != null && string.Equals(childNode.Attributes["class"]?.Value, "game",
                            StringComparison.OrdinalIgnoreCase))
                    {
                        // If we don't have children nodes, skip this one over
                        if (!childNode.HasChildNodes)
                            continue;

                        // The game node contains multiple other elements
                        foreach (XmlNode gameNode in childNode.ChildNodes)
                        {
                            // Table elements contain multiple other parts of information
                            if (string.Equals(gameNode.Name, "table", StringComparison.OrdinalIgnoreCase))
                            {
                                // All tables have some attribute we can use
                                if (gameNode.Attributes == null)
                                    continue;

                                // The gameinfo node contains most of the major information
                                if (string.Equals(gameNode.Attributes["class"]?.Value, "gameinfo",
                                        StringComparison.OrdinalIgnoreCase))
                                {
                                    // If we don't have children nodes, skip this one over
                                    if (!gameNode.HasChildNodes)
                                        continue;

                                    // Loop through each of the rows
                                    foreach (XmlNode gameInfoNode in gameNode.ChildNodes)
                                    {
                                        // If we run into anything not a row, ignore it
                                        if (!string.Equals(gameInfoNode.Name, "tr", StringComparison.OrdinalIgnoreCase))
                                            continue;

                                        // If we don't have the required nodes, ignore it
                                        if (gameInfoNode["th"] == null || gameInfoNode["td"] == null)
                                            continue;

                                        XmlNode gameInfoNodeHeader = gameInfoNode["th"];
                                        XmlNode gameInfoNodeData = gameInfoNode["td"];

                                        if (string.Equals(gameInfoNodeHeader.InnerText, "System", StringComparison.OrdinalIgnoreCase))
                                        {
                                            info.CommonDiscInfo.System = Extensions.ToRedumpSystem(gameInfoNodeData["a"]?.InnerText);
                                        }
                                        else if (string.Equals(gameInfoNodeHeader.InnerText, "Media", StringComparison.OrdinalIgnoreCase))
                                        {
                                            info.CommonDiscInfo.Media = Extensions.ToDiscType(gameInfoNodeData.InnerText);
                                        }
                                        else if (string.Equals(gameInfoNodeHeader.InnerText, "Category", StringComparison.OrdinalIgnoreCase))
                                        {
                                            info.CommonDiscInfo.Category = Extensions.ToDiscCategory(gameInfoNodeData.InnerText);
                                        }
                                        else if (string.Equals(gameInfoNodeHeader.InnerText, "Region", StringComparison.OrdinalIgnoreCase))
                                        {
                                            // TODO: COMPLETE
                                        }
                                        else if (string.Equals(gameInfoNodeHeader.InnerText, "Languages", StringComparison.OrdinalIgnoreCase))
                                        {
                                            // TODO: COMPLETE
                                        }
                                        else if (string.Equals(gameInfoNodeHeader.InnerText, "Edition", StringComparison.OrdinalIgnoreCase))
                                        {
                                            info.VersionAndEditions.OtherEditions = gameInfoNodeData.InnerText;
                                        }
                                        else if (string.Equals(gameInfoNodeHeader.InnerText, "Added", StringComparison.OrdinalIgnoreCase))
                                        {
                                            if (DateTime.TryParse(gameInfoNodeData.InnerText, out DateTime added))
                                                info.Added = added;
                                        }
                                        else if (string.Equals(gameInfoNodeHeader.InnerText, "Last modified", StringComparison.OrdinalIgnoreCase))
                                        {
                                            if (DateTime.TryParse(gameInfoNodeData.InnerText, out DateTime lastModified))
                                                info.LastModified = lastModified;
                                        }
                                    }
                                }

                                // The gamecomments node contains way more than it implies
                                if (string.Equals(gameNode.Attributes["class"]?.Value, "gamecomments", StringComparison.OrdinalIgnoreCase))
                                {
                                    // TODO: COMPLETE
                                }

                                // TODO: COMPLETE
                            }

                            // The only other supported elements are divs
                            else if (!string.Equals(gameNode.Name, "div", StringComparison.OrdinalIgnoreCase))
                            {
                                continue;
                            }

                            // Check the div for dumper info
                            // TODO: COMPLETE
                        }
                    }

                    // Figure out what the div contains, if possible
                    // TODO: COMPLETE
                }
            }
            catch
            {
                return null;
            }

            return info;
        }

        /// <summary>
        /// Fill out an existing SubmissionInfo object based on a disc page
        /// </summary>
        /// <param name="wc">RedumpWebClient for making the connection</param>
        /// <param name="info">Existing SubmissionInfo object to fill</param>
        /// <param name="id">Redump disc ID to retrieve</param>
#if NET48 || NETSTANDARD2_1
        private static bool FillFromId(RedumpWebClient wc, SubmissionInfo info, int id)
        {
            string discData = wc.DownloadSingleSiteID(id);
            if (string.IsNullOrEmpty(discData))
                return false;
#else
        private async static Task<bool> FillFromId(RedumpHttpClient wc, SubmissionInfo info, int id)
        {
            string discData = await wc.DownloadSingleSiteID(id);
            if (string.IsNullOrEmpty(discData))
                return false;
#endif

            // Title, Disc Number/Letter, Disc Title
            var match = Constants.TitleRegex.Match(discData);
            if (match.Success)
            {
                string title = WebUtility.HtmlDecode(match.Groups[1].Value);

                // If we have parenthesis, title is everything before the first one
                int firstParenLocation = title.IndexOf(" (");
                if (firstParenLocation >= 0)
                {
                    info.CommonDiscInfo.Title = title.Substring(0, firstParenLocation);
                    var subMatches = Constants.DiscNumberLetterRegex.Matches(title);
                    foreach (Match subMatch in subMatches)
                    {
                        var subMatchValue = subMatch.Groups[1].Value;

                        // Disc number or letter
                        if (subMatchValue.StartsWith("Disc"))
                            info.CommonDiscInfo.DiscNumberLetter = subMatchValue.Remove(0, "Disc ".Length);

                        // Disc title
                        else
                            info.CommonDiscInfo.DiscTitle = subMatchValue;
                    }
                }
                // Otherwise, leave the title as-is
                else
                {
                    info.CommonDiscInfo.Title = title;
                }
            }

            // Foreign Title
            match = Constants.ForeignTitleRegex.Match(discData);
            if (match.Success)
                info.CommonDiscInfo.ForeignTitleNonLatin = WebUtility.HtmlDecode(match.Groups[1].Value);
            else
                info.CommonDiscInfo.ForeignTitleNonLatin = null;

            // Category
            match = Constants.CategoryRegex.Match(discData);
            if (match.Success)
                info.CommonDiscInfo.Category = Extensions.ToDiscCategory(match.Groups[1].Value);
            else
                info.CommonDiscInfo.Category = DiscCategory.Games;

            // Region
            if (info.CommonDiscInfo.Region == null)
            {
                match = Constants.RegionRegex.Match(discData);
                if (match.Success)
                    info.CommonDiscInfo.Region = Extensions.ToRegion(match.Groups[1].Value);
            }

            // Languages
            var matches = Constants.LanguagesRegex.Matches(discData);
            if (matches.Count > 0)
            {
                List<Language?> tempLanguages = new List<Language?>();
                foreach (Match submatch in matches)
                    tempLanguages.Add(Extensions.ToLanguage(submatch.Groups[1].Value));

                info.CommonDiscInfo.Languages = tempLanguages.Where(l => l != null).ToArray();
            }

            // Serial
            // TODO: Re-enable if there's a way of verifying against a disc
            //match = Constants.SerialRegex.Match(discData);
            //if (match.Success)
            //    info.CommonDiscInfo.Serial = $"(VERIFY THIS) {WebUtility.HtmlDecode(match.Groups[1].Value)}";

            // Error count
            if (string.IsNullOrEmpty(info.CommonDiscInfo.ErrorsCount))
            {
                match = Constants.ErrorCountRegex.Match(discData);
                if (match.Success)
                    info.CommonDiscInfo.ErrorsCount = match.Groups[1].Value;
            }

            // Version
            if (info.VersionAndEditions.Version == null)
            {
                match = Constants.VersionRegex.Match(discData);
                if (match.Success)
                    info.VersionAndEditions.Version = $"(VERIFY THIS) {WebUtility.HtmlDecode(match.Groups[1].Value)}";
            }

            // Dumpers
            matches = Constants.DumpersRegex.Matches(discData);
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

            // TODO: Unify handling of fields that can include site codes (Comments/Contents)

            // Comments
            match = Constants.CommentsRegex.Match(discData);
            if (match.Success)
            {
                // Process the old comments block
                string oldComments = info.CommonDiscInfo.Comments
                    + (string.IsNullOrEmpty(info.CommonDiscInfo.Comments) ? string.Empty : "\n")
                    + WebUtility.HtmlDecode(match.Groups[1].Value)
                        .Replace("\r\n", "\n")
                        .Replace("<br />\n", "\n")
                        .Replace("<br />", string.Empty)
                        .Replace("</div>", string.Empty)
                        .Replace("[+]", string.Empty)
                        .ReplaceHtmlWithSiteCodes();
                oldComments = Regex.Replace(oldComments, @"<div .*?>", string.Empty);

                // Create state variables
                bool addToLast = false;
                SiteCode? lastSiteCode = null;
                string newComments = string.Empty;

                // Process the comments block line-by-line
                string[] commentsSeparated = oldComments.Split('\n');
                for (int i = 0; i < commentsSeparated.Length; i++)
                {
                    string commentLine = commentsSeparated[i].Trim();

                    // If we have an empty line, we want to treat this as intentional
                    if (string.IsNullOrWhiteSpace(commentLine))
                    {
                        addToLast = false;
                        lastSiteCode = null;
                        newComments += $"{commentLine}\n";
                        continue;
                    }

                    // Otherwise, we need to find what tag is in use
                    bool foundTag = false;
                    foreach (SiteCode? siteCode in Enum.GetValues(typeof(SiteCode)))
                    {
                        // If the line doesn't contain this tag, just skip
                        if (!commentLine.Contains(siteCode.ShortName()))
                            continue;

                        // Cache the current site code
                        lastSiteCode = siteCode;

                        // If we don't already have this site code, add it to the dictionary
                        if (!info.CommonDiscInfo.CommentsSpecialFields.ContainsKey(siteCode))
                            info.CommonDiscInfo.CommentsSpecialFields[siteCode] = $"(VERIFY THIS) {commentLine.Replace(siteCode.ShortName(), string.Empty).Trim()}";

                        // A subset of tags can be multiline
                        addToLast = IsMultiLine(siteCode);

                        // Mark as having found a tag
                        foundTag = true;
                        break;
                    }

                    // If we didn't find a known tag, just add the line, just in case
                    if (!foundTag)
                    {
                        if (addToLast && lastSiteCode != null)
                        {
                            if (!string.IsNullOrWhiteSpace(info.CommonDiscInfo.CommentsSpecialFields[lastSiteCode]))
                                info.CommonDiscInfo.CommentsSpecialFields[lastSiteCode] += "\n";

                            info.CommonDiscInfo.CommentsSpecialFields[lastSiteCode] += commentLine;
                        }
                        else
                        {
                            newComments += $"{commentLine}\n";
                        }
                    }
                }

                // Set the new comments field
                info.CommonDiscInfo.Comments = newComments;
            }

            // Contents
            match = Constants.ContentsRegex.Match(discData);
            if (match.Success)
            {
                // Process the old contents block
                string oldContents = info.CommonDiscInfo.Contents
                    + (string.IsNullOrEmpty(info.CommonDiscInfo.Contents) ? string.Empty : "\n")
                    + WebUtility.HtmlDecode(match.Groups[1].Value)
                        .Replace("\r\n", "\n")
                        .Replace("<br />\n", "\n")
                        .Replace("<br />", string.Empty)
                        .Replace("</div>", string.Empty)
                        .Replace("[+]", string.Empty)
                        .ReplaceHtmlWithSiteCodes();
                oldContents = Regex.Replace(oldContents, @"<div .*?>", string.Empty);

                // Create state variables
                bool addToLast = false;
                SiteCode? lastSiteCode = null;
                string newContents = string.Empty;

                // Process the contents block line-by-line
                string[] contentsSeparated = oldContents.Split('\n');
                for (int i = 0; i < contentsSeparated.Length; i++)
                {
                    string contentLine = contentsSeparated[i].Trim();

                    // If we have an empty line, we want to treat this as intentional
                    if (string.IsNullOrWhiteSpace(contentLine))
                    {
                        addToLast = false;
                        lastSiteCode = null;
                        newContents += $"{contentLine}\n";
                        continue;
                    }

                    // Otherwise, we need to find what tag is in use
                    bool foundTag = false;
                    foreach (SiteCode? siteCode in Enum.GetValues(typeof(SiteCode)))
                    {
                        // If the line doesn't contain this tag, just skip
                        if (!contentLine.Contains(siteCode.ShortName()))
                            continue;

                        // Cache the current site code
                        lastSiteCode = siteCode;

                        // If we don't already have this site code, add it to the dictionary
                        if (!info.CommonDiscInfo.ContentsSpecialFields.ContainsKey(siteCode))
                            info.CommonDiscInfo.ContentsSpecialFields[siteCode] = $"(VERIFY THIS) {contentLine.Replace(siteCode.ShortName(), string.Empty).Trim()}";

                        // A subset of tags can be multiline
                        addToLast = IsMultiLine(siteCode);

                        // Mark as having found a tag
                        foundTag = true;
                        break;
                    }

                    // If we didn't find a known tag, just add the line, just in case
                    if (!foundTag)
                    {
                        if (addToLast && lastSiteCode != null)
                        {
                            if (!string.IsNullOrWhiteSpace(info.CommonDiscInfo.ContentsSpecialFields[lastSiteCode]))
                                info.CommonDiscInfo.ContentsSpecialFields[lastSiteCode] += "\n";

                            info.CommonDiscInfo.ContentsSpecialFields[lastSiteCode] += contentLine;
                        }
                        else
                        {
                            newContents += $"{contentLine}\n";
                        }
                    }
                }

                // Set the new contents field
                info.CommonDiscInfo.Contents = newContents;
            }

            // Added
            match = Constants.AddedRegex.Match(discData);
            if (match.Success)
            {
                if (DateTime.TryParse(match.Groups[1].Value, out DateTime added))
                    info.Added = added;
                else
                    info.Added = null;
            }

            // Last Modified
            match = Constants.LastModifiedRegex.Match(discData);
            if (match.Success)
            {
                if (DateTime.TryParse(match.Groups[1].Value, out DateTime lastModified))
                    info.LastModified = lastModified;
                else
                    info.LastModified = null;
            }

            return true;
        }

        /// <summary>
        /// Fill in a SubmissionInfo object from Redump, if possible
        /// </summary>
        /// <param name="options">Options object representing user-defined options</param>
        /// <param name="info">Existing SubmissionInfo object to fill</param>
        /// <param name="resultProgress">Optional result progress callback</param>
#if NET48 || NETSTANDARD2_1
        private static bool FillFromRedump(Options options, SubmissionInfo info, IProgress<Result> resultProgress = null)
#else
        private async static Task<bool> FillFromRedump(Options options, SubmissionInfo info, IProgress<Result> resultProgress = null)
#endif
        {
            // Set the current dumper based on username
            info.DumpersAndStatus.Dumpers = new string[] { options.RedumpUsername };
            info.PartiallyMatchedIDs = new List<int>();

#if NET48 || NETSTANDARD2_1
            using (RedumpWebClient wc = new RedumpWebClient())
#else
            using (RedumpHttpClient wc = new RedumpHttpClient())
#endif
            {
                // Login to Redump
#if NET48 || NETSTANDARD2_1
                bool? loggedIn = wc.Login(options.RedumpUsername, options.RedumpPassword);
#else
                bool? loggedIn = await wc.Login(options.RedumpUsername, options.RedumpPassword);
#endif
                if (loggedIn == null)
                {
                    resultProgress?.Report(Result.Failure("There was an unknown error connecting to Redump"));
                    return false;
                }
                else if (loggedIn == false)
                {
                    // Don't log the as a failure or error
                    return false;
                }

                // Setup the full-track checks
                bool allFound = true;
                List<int> fullyMatchedIDs = null;

                // Loop through all of the hashdata to find matching IDs
                resultProgress?.Report(Result.Success("Finding disc matches on Redump..."));
                string[] splitData = info.TracksAndWriteOffsets.ClrMameProData.Split('\n');
                foreach (string hashData in splitData)
                {
#if NET48 || NETSTANDARD2_1
                    (bool singleFound, List<int> foundIds) = ValidateSingleTrack(wc, info, hashData, resultProgress);
#else
                    (bool singleFound, List<int> foundIds) = await ValidateSingleTrack(wc, info, hashData, resultProgress);
#endif

                    // Ensure that all tracks are found
                    allFound &= singleFound;

                    // If we found a track, only keep track of distinct found tracks
                    if (singleFound && foundIds != null)
                    {
                        if (fullyMatchedIDs == null)
                            fullyMatchedIDs = foundIds;
                        else
                            fullyMatchedIDs = fullyMatchedIDs.Intersect(foundIds).ToList();
                    }
                    // If no tracks were found, remove all fully matched IDs found so far
                    else
                    {
                        fullyMatchedIDs = new List<int>();
                    }
                }

                // Make sure we only have unique IDs
                info.PartiallyMatchedIDs = info.PartiallyMatchedIDs
                    .Distinct()
                    .OrderBy(id => id)
                    .ToList();

                resultProgress?.Report(Result.Success("Match finding complete! " + (fullyMatchedIDs.Count > 0
                    ? "Fully Matched IDs: " + string.Join(",", fullyMatchedIDs)
                    : "No matches found")));

                // Exit early if one failed or there are no matched IDs
                if (!allFound || fullyMatchedIDs.Count == 0)
                    return false;

                // Find the first matched ID where the track count matches, we can grab a bunch of info from it
                int totalMatchedIDsCount = fullyMatchedIDs.Count;
                for (int i = 0; i < totalMatchedIDsCount; i++)
                {
                    // Skip if the track count doesn't match
#if NET48 || NETSTANDARD2_1
                    if (!ValidateTrackCount(wc, fullyMatchedIDs[i], splitData.Length))
                        continue;
#else
                    if (!await ValidateTrackCount(wc, fullyMatchedIDs[i], splitData.Length))
                        continue;
#endif

                    // Fill in the fields from the existing ID
                    resultProgress?.Report(Result.Success($"Filling fields from existing ID {fullyMatchedIDs[i]}..."));
#if NET48 || NETSTANDARD2_1
                    FillFromId(wc, info, fullyMatchedIDs[i]);
#else
                    _ = await FillFromId(wc, info, fullyMatchedIDs[i]);
#endif
                    resultProgress?.Report(Result.Success("Information filling complete!"));

                    // Set the fully matched ID to the current
                    info.FullyMatchedID = fullyMatchedIDs[i];
                    break;
                }

                // Clear out fully matched IDs from the partial list
                if (info.FullyMatchedID.HasValue)
                {
                    if (info.PartiallyMatchedIDs.Count() == 1)
                        info.PartiallyMatchedIDs = null;
                    else
                        info.PartiallyMatchedIDs.Remove(info.FullyMatchedID.Value);
                }
            }

            return true;
        }

        /// <summary>
        /// Process a text block and replace with internal identifiers
        /// </summary>
        /// <param name="text">Text block to process</param>
        /// <returns>Processed text block, if possible</returns>
        private static string ReplaceHtmlWithSiteCodes(this string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return text;

            foreach (SiteCode? siteCode in Enum.GetValues(typeof(SiteCode)))
            {
                text = text.Replace(siteCode.LongName(), siteCode.ShortName());
            }

            // For some outdated tags, we need to use alternate names
            text = text.Replace("<b>Demos</b>:", ((SiteCode?)SiteCode.PlayableDemos).ShortName());
            text = text.Replace("DMI:", ((SiteCode?)SiteCode.DMIHash).ShortName());
            text = text.Replace("<b>LucasArts ID</b>:", ((SiteCode?)SiteCode.LucasArtsID).ShortName());
            text = text.Replace("PFI:", ((SiteCode?)SiteCode.PFIHash).ShortName());
            text = text.Replace("SS:", ((SiteCode?)SiteCode.SSHash).ShortName());
            text = text.Replace("SSv1:", ((SiteCode?)SiteCode.SSHash).ShortName());
            text = text.Replace("SSv2:", ((SiteCode?)SiteCode.SSHash).ShortName());
            text = text.Replace("SS version:", ((SiteCode?)SiteCode.SSVersion).ShortName());
            text = text.Replace("XeMID:", ((SiteCode?)SiteCode.XeMID).ShortName());
            text = text.Replace("XMID:", ((SiteCode?)SiteCode.XMID).ShortName());

            return text;
        }

        /// <summary>
        /// List the disc IDs associated with a given quicksearch query
        /// </summary>
        /// <param name="wc">RedumpWebClient for making the connection</param>
        /// <param name="query">Query string to attempt to search for</param>
        /// <returns>All disc IDs for the given query, null on error</returns>
#if NET48 || NETSTANDARD2_1
        private static List<int> ListSearchResults(RedumpWebClient wc, string query)
#else
        private async static Task<List<int>> ListSearchResults(RedumpHttpClient wc, string query)
#endif
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
            try
            {
                int pageNumber = 1;
                while (true)
                {
#if NET48 || NETSTANDARD2_1
                    List<int> pageIds = wc.CheckSingleSitePage(string.Format(Constants.QuickSearchUrl, query, pageNumber++));
#else
                    List<int> pageIds = await wc.CheckSingleSitePage(string.Format(Constants.QuickSearchUrl, query, pageNumber++));
#endif
                    ids.AddRange(pageIds);
                    if (pageIds.Count <= 1)
                        break;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An exception occurred while trying to log in: {ex}");
                return null;
            }

            return ids;
        }

        /// <summary>
        /// Validate a single track against Redump, if possible
        /// </summary>
        /// <param name="wc">RedumpWebClient for making the connection</param>
        /// <param name="info">Existing SubmissionInfo object to fill</param>
        /// <param name="hashData">DAT-formatted hash data to parse out</param>
        /// <param name="resultProgress">Optional result progress callback</param>
        /// <returns>True if the track was found, false otherwise; List of found values, if possible</returns>
#if NET48 || NETSTANDARD2_1
        private static (bool, List<int>) ValidateSingleTrack(RedumpWebClient wc, SubmissionInfo info, string hashData, IProgress<Result> resultProgress = null)
#else
        private async static Task<(bool, List<int>)> ValidateSingleTrack(RedumpHttpClient wc, SubmissionInfo info, string hashData, IProgress<Result> resultProgress = null)
#endif
        {
            // If the line isn't parseable, we can't validate
            if (!GetISOHashValues(hashData, out long _, out string _, out string _, out string sha1))
            {
                resultProgress?.Report(Result.Failure("Line could not be parsed for hash data"));
                return (false, null);
            }

            // Get all matching IDs for the track
#if NET48 || NETSTANDARD2_1
            List<int> newIds = ListSearchResults(wc, sha1);
#else
            List<int> newIds = await ListSearchResults(wc, sha1);
#endif

            // If we got null back, there was an error
            if (newIds == null)
            {
                resultProgress?.Report(Result.Failure("There was an unknown error retrieving information from Redump"));
                return (false, null);
            }

            // If no IDs match any track, just return
            if (!newIds.Any())
                return (false, null);

            // Join the list of found IDs to the existing list, if possible
            if (info.PartiallyMatchedIDs.Any())
                info.PartiallyMatchedIDs.AddRange(newIds);
            else
                info.PartiallyMatchedIDs = newIds;

            return (true, newIds);
        }

        /// <summary>
        /// Validate that the current track count and remote track count match
        /// </summary>
        /// <param name="wc">RedumpWebClient for making the connection</param>
        /// <param name="id">Redump disc ID to retrieve</param>
        /// <param name="localCount">Local count of tracks for the current disc</param>
        /// <returns>True if the track count matches, false otherwise</returns>
#if NET48 || NETSTANDARD2_1
        private static bool ValidateTrackCount(RedumpWebClient wc, int id, int localCount)
#else
        private async static Task<bool> ValidateTrackCount(RedumpHttpClient wc, int id, int localCount)
#endif
        {
            // If we can't pull the remote data, we can't match
#if NET48 || NETSTANDARD2_1
            string discData = wc.DownloadSingleSiteID(id);
#else
            string discData = await wc.DownloadSingleSiteID(id);
#endif
            if (string.IsNullOrEmpty(discData))
                return false;

            // Discs with only 1 track don't have a track count listed
            var match = Constants.TrackCountRegex.Match(discData);
            if (!match.Success && localCount == 1)
                return true;
            else if (!match.Success)
                return false;

            // If the count isn't parseable, we're not taking chances
            if (!Int32.TryParse(match.Groups[1].Value, out int remoteCount))
                return false;

            // Finally check to see if the counts match
            return localCount == remoteCount;
        }

        #endregion

        #region Helpers

        /// <summary>
        /// Format a single site tag to string
        /// </summary>
        /// <param name="kvp">KeyValuePair representing the site tag and value</param>
        /// <returns>String-formatted tag and value</returns>
        private static string FormatSiteTag(KeyValuePair<SiteCode?, string> kvp)
        {
            bool isMultiLine = IsMultiLine(kvp.Key);
            string line = $"{kvp.Key.ShortName()}{(isMultiLine ? "\n" : " ")}";

            // Special case for boolean fields
            if (IsBoolean(kvp.Key))
            {
                if (kvp.Value != true.ToString())
                    return string.Empty;

                return line.Trim();
            }

            return $"{line}{kvp.Value}{(isMultiLine ? "\n" : string.Empty)}";
        }

        /// <summary>
        /// Check if a site code is boolean or not
        /// </summary>
        /// <param name="siteCode">SiteCode to check</param>
        /// <returns>True if the code field is a flag with no value, false otherwise</returns>
        /// <remarks>TODO: This should move to Extensions at some point</remarks>
        private static bool IsBoolean(SiteCode? siteCode)
        {
            switch (siteCode)
            {
                case SiteCode.PostgapType:
                case SiteCode.VCD:
                    return true;
                default:
                    return false;
            }
        }

        /// <summary>
        /// Check if a site code is multi-line or not
        /// </summary>
        /// <param name="siteCode">SiteCode to check</param>
        /// <returns>True if the code field is multiline by default, false otherwise</returns>
        /// <remarks>TODO: This should move to Extensions at some point</remarks>
        private static bool IsMultiLine(SiteCode? siteCode)
        {
            switch (siteCode)
            {
                case SiteCode.Extras:
                case SiteCode.Filename:
                case SiteCode.Games:
                case SiteCode.GameFootage:
                case SiteCode.Multisession:
                case SiteCode.NetYarozeGames:
                case SiteCode.Patches:
                case SiteCode.PlayableDemos:
                case SiteCode.RollingDemos:
                case SiteCode.Savegames:
                case SiteCode.TechDemos:
                case SiteCode.Videos:
                    return true;
                default:
                    return false;
            }
        }

        /// <summary>
        /// Order comment code tags according to Redump requirements
        /// </summary>
        /// <returns>Ordered list of KeyValuePairs representing the tags and values</returns>
        private static List<KeyValuePair<SiteCode?, string>> OrderCommentTags(Dictionary<SiteCode?, string> tags)
        {
            var sorted = new List<KeyValuePair<SiteCode?, string>>();

            // If the input is invalid, just return an empty set
            if (tags == null || tags.Count == 0)
                return sorted;

            // Identifying Info
            if (tags.ContainsKey(SiteCode.AlternativeTitle))
                sorted.Add(new KeyValuePair<SiteCode?, string>(SiteCode.AlternativeTitle, tags[SiteCode.AlternativeTitle]));
            if (tags.ContainsKey(SiteCode.AlternativeForeignTitle))
                sorted.Add(new KeyValuePair<SiteCode?, string>(SiteCode.AlternativeForeignTitle, tags[SiteCode.AlternativeForeignTitle]));
            if (tags.ContainsKey(SiteCode.InternalName))
                sorted.Add(new KeyValuePair<SiteCode?, string>(SiteCode.InternalName, tags[SiteCode.InternalName]));
            if (tags.ContainsKey(SiteCode.InternalSerialName))
                sorted.Add(new KeyValuePair<SiteCode?, string>(SiteCode.InternalSerialName, tags[SiteCode.InternalSerialName]));
            if (tags.ContainsKey(SiteCode.VolumeLabel))
                sorted.Add(new KeyValuePair<SiteCode?, string>(SiteCode.VolumeLabel, tags[SiteCode.VolumeLabel]));
            if (tags.ContainsKey(SiteCode.Multisession))
                sorted.Add(new KeyValuePair<SiteCode?, string>(SiteCode.Multisession, tags[SiteCode.Multisession]));

            if (tags.ContainsKey(SiteCode.XMID))
                sorted.Add(new KeyValuePair<SiteCode?, string>(SiteCode.XMID, tags[SiteCode.XMID]));
            if (tags.ContainsKey(SiteCode.XeMID))
                sorted.Add(new KeyValuePair<SiteCode?, string>(SiteCode.XeMID, tags[SiteCode.XeMID]));
            if (tags.ContainsKey(SiteCode.DMIHash))
                sorted.Add(new KeyValuePair<SiteCode?, string>(SiteCode.DMIHash, tags[SiteCode.DMIHash]));
            if (tags.ContainsKey(SiteCode.PFIHash))
                sorted.Add(new KeyValuePair<SiteCode?, string>(SiteCode.PFIHash, tags[SiteCode.PFIHash]));
            if (tags.ContainsKey(SiteCode.SSHash))
                sorted.Add(new KeyValuePair<SiteCode?, string>(SiteCode.SSHash, tags[SiteCode.SSHash]));
            if (tags.ContainsKey(SiteCode.SSVersion))
                sorted.Add(new KeyValuePair<SiteCode?, string>(SiteCode.SSVersion, tags[SiteCode.SSVersion]));

            if (tags.ContainsKey(SiteCode.Filename))
                sorted.Add(new KeyValuePair<SiteCode?, string>(SiteCode.Filename, tags[SiteCode.Filename]));

            if (tags.ContainsKey(SiteCode.BBFCRegistrationNumber))
                sorted.Add(new KeyValuePair<SiteCode?, string>(SiteCode.BBFCRegistrationNumber, tags[SiteCode.BBFCRegistrationNumber]));
            if (tags.ContainsKey(SiteCode.DiscHologramID))
                sorted.Add(new KeyValuePair<SiteCode?, string>(SiteCode.DiscHologramID, tags[SiteCode.DiscHologramID]));
            if (tags.ContainsKey(SiteCode.DNASDiscID))
                sorted.Add(new KeyValuePair<SiteCode?, string>(SiteCode.DNASDiscID, tags[SiteCode.DNASDiscID]));
            if (tags.ContainsKey(SiteCode.ISBN))
                sorted.Add(new KeyValuePair<SiteCode?, string>(SiteCode.ISBN, tags[SiteCode.ISBN]));
            if (tags.ContainsKey(SiteCode.ISSN))
                sorted.Add(new KeyValuePair<SiteCode?, string>(SiteCode.ISSN, tags[SiteCode.ISSN]));
            if (tags.ContainsKey(SiteCode.PPN))
                sorted.Add(new KeyValuePair<SiteCode?, string>(SiteCode.PPN, tags[SiteCode.PPN]));
            if (tags.ContainsKey(SiteCode.VFCCode))
                sorted.Add(new KeyValuePair<SiteCode?, string>(SiteCode.VFCCode, tags[SiteCode.VFCCode]));

            if (tags.ContainsKey(SiteCode.Genre))
                sorted.Add(new KeyValuePair<SiteCode?, string>(SiteCode.Genre, tags[SiteCode.Genre]));
            if (tags.ContainsKey(SiteCode.Series))
                sorted.Add(new KeyValuePair<SiteCode?, string>(SiteCode.Series, tags[SiteCode.Series]));
            if (tags.ContainsKey(SiteCode.PostgapType))
                sorted.Add(new KeyValuePair<SiteCode?, string>(SiteCode.PostgapType, tags[SiteCode.PostgapType]));
            if (tags.ContainsKey(SiteCode.VCD))
                sorted.Add(new KeyValuePair<SiteCode?, string>(SiteCode.VCD, tags[SiteCode.VCD]));

            // Publisher / Company IDs
            if (tags.ContainsKey(SiteCode.AcclaimID))
                sorted.Add(new KeyValuePair<SiteCode?, string>(SiteCode.AcclaimID, tags[SiteCode.AcclaimID]));
            if (tags.ContainsKey(SiteCode.ActivisionID))
                sorted.Add(new KeyValuePair<SiteCode?, string>(SiteCode.ActivisionID, tags[SiteCode.ActivisionID]));
            if (tags.ContainsKey(SiteCode.BandaiID))
                sorted.Add(new KeyValuePair<SiteCode?, string>(SiteCode.BandaiID, tags[SiteCode.BandaiID]));
            if (tags.ContainsKey(SiteCode.ElectronicArtsID))
                sorted.Add(new KeyValuePair<SiteCode?, string>(SiteCode.ElectronicArtsID, tags[SiteCode.ElectronicArtsID]));
            if (tags.ContainsKey(SiteCode.FoxInteractiveID))
                sorted.Add(new KeyValuePair<SiteCode?, string>(SiteCode.FoxInteractiveID, tags[SiteCode.FoxInteractiveID]));
            if (tags.ContainsKey(SiteCode.GTInteractiveID))
                sorted.Add(new KeyValuePair<SiteCode?, string>(SiteCode.GTInteractiveID, tags[SiteCode.GTInteractiveID]));
            if (tags.ContainsKey(SiteCode.JASRACID))
                sorted.Add(new KeyValuePair<SiteCode?, string>(SiteCode.JASRACID, tags[SiteCode.JASRACID]));
            if (tags.ContainsKey(SiteCode.KingRecordsID))
                sorted.Add(new KeyValuePair<SiteCode?, string>(SiteCode.KingRecordsID, tags[SiteCode.KingRecordsID]));
            if (tags.ContainsKey(SiteCode.KoeiID))
                sorted.Add(new KeyValuePair<SiteCode?, string>(SiteCode.KoeiID, tags[SiteCode.KoeiID]));
            if (tags.ContainsKey(SiteCode.KonamiID))
                sorted.Add(new KeyValuePair<SiteCode?, string>(SiteCode.KonamiID, tags[SiteCode.KonamiID]));
            if (tags.ContainsKey(SiteCode.LucasArtsID))
                sorted.Add(new KeyValuePair<SiteCode?, string>(SiteCode.LucasArtsID, tags[SiteCode.LucasArtsID]));
            if (tags.ContainsKey(SiteCode.MicrosoftID))
                sorted.Add(new KeyValuePair<SiteCode?, string>(SiteCode.MicrosoftID, tags[SiteCode.MicrosoftID]));
            if (tags.ContainsKey(SiteCode.NaganoID))
                sorted.Add(new KeyValuePair<SiteCode?, string>(SiteCode.NaganoID, tags[SiteCode.NaganoID]));
            if (tags.ContainsKey(SiteCode.NamcoID))
                sorted.Add(new KeyValuePair<SiteCode?, string>(SiteCode.NamcoID, tags[SiteCode.NamcoID]));
            if (tags.ContainsKey(SiteCode.NipponIchiSoftwareID))
                sorted.Add(new KeyValuePair<SiteCode?, string>(SiteCode.NipponIchiSoftwareID, tags[SiteCode.NipponIchiSoftwareID]));
            if (tags.ContainsKey(SiteCode.OriginID))
                sorted.Add(new KeyValuePair<SiteCode?, string>(SiteCode.OriginID, tags[SiteCode.OriginID]));
            if (tags.ContainsKey(SiteCode.PonyCanyonID))
                sorted.Add(new KeyValuePair<SiteCode?, string>(SiteCode.PonyCanyonID, tags[SiteCode.PonyCanyonID]));
            if (tags.ContainsKey(SiteCode.SegaID))
                sorted.Add(new KeyValuePair<SiteCode?, string>(SiteCode.SegaID, tags[SiteCode.SegaID]));
            if (tags.ContainsKey(SiteCode.SelenID))
                sorted.Add(new KeyValuePair<SiteCode?, string>(SiteCode.SelenID, tags[SiteCode.SelenID]));
            if (tags.ContainsKey(SiteCode.SierraID))
                sorted.Add(new KeyValuePair<SiteCode?, string>(SiteCode.SierraID, tags[SiteCode.SierraID]));
            if (tags.ContainsKey(SiteCode.TaitoID))
                sorted.Add(new KeyValuePair<SiteCode?, string>(SiteCode.TaitoID, tags[SiteCode.TaitoID]));
            if (tags.ContainsKey(SiteCode.UbisoftID))
                sorted.Add(new KeyValuePair<SiteCode?, string>(SiteCode.UbisoftID, tags[SiteCode.UbisoftID]));
            if (tags.ContainsKey(SiteCode.ValveID))
                sorted.Add(new KeyValuePair<SiteCode?, string>(SiteCode.ValveID, tags[SiteCode.ValveID]));

            return sorted;
        }

        /// <summary>
        /// Order content code tags according to Redump requirements
        /// </summary>
        /// <returns>Ordered list of KeyValuePairs representing the tags and values</returns>
        private static List<KeyValuePair<SiteCode?, string>> OrderContentTags(Dictionary<SiteCode?, string> tags)
        {
            var sorted = new List<KeyValuePair<SiteCode?, string>>();

            // If the input is invalid, just return an empty set
            if (tags == null || tags.Count == 0)
                return sorted;

            // Games
            if (tags.ContainsKey(SiteCode.Games))
                sorted.Add(new KeyValuePair<SiteCode?, string>(SiteCode.Games, tags[SiteCode.Games]));
            if (tags.ContainsKey(SiteCode.NetYarozeGames))
                sorted.Add(new KeyValuePair<SiteCode?, string>(SiteCode.NetYarozeGames, tags[SiteCode.NetYarozeGames]));

            // Demos
            if (tags.ContainsKey(SiteCode.PlayableDemos))
                sorted.Add(new KeyValuePair<SiteCode?, string>(SiteCode.PlayableDemos, tags[SiteCode.PlayableDemos]));
            if (tags.ContainsKey(SiteCode.RollingDemos))
                sorted.Add(new KeyValuePair<SiteCode?, string>(SiteCode.RollingDemos, tags[SiteCode.RollingDemos]));
            if (tags.ContainsKey(SiteCode.TechDemos))
                sorted.Add(new KeyValuePair<SiteCode?, string>(SiteCode.TechDemos, tags[SiteCode.TechDemos]));

            // Video
            if (tags.ContainsKey(SiteCode.GameFootage))
                sorted.Add(new KeyValuePair<SiteCode?, string>(SiteCode.GameFootage, tags[SiteCode.GameFootage]));
            if (tags.ContainsKey(SiteCode.Videos))
                sorted.Add(new KeyValuePair<SiteCode?, string>(SiteCode.Videos, tags[SiteCode.Videos]));

            // Miscellaneous
            if (tags.ContainsKey(SiteCode.Patches))
                sorted.Add(new KeyValuePair<SiteCode?, string>(SiteCode.Patches, tags[SiteCode.Patches]));
            if (tags.ContainsKey(SiteCode.Savegames))
                sorted.Add(new KeyValuePair<SiteCode?, string>(SiteCode.Savegames, tags[SiteCode.Savegames]));
            if (tags.ContainsKey(SiteCode.Extras))
                sorted.Add(new KeyValuePair<SiteCode?, string>(SiteCode.Extras, tags[SiteCode.Extras]));

            return sorted;
        }

        #endregion
    }
}
