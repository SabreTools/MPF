using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml;
using BinaryObjectScanner;
using MPF.Core.Data;
using MPF.Core.Modules;
using Newtonsoft.Json;
using SabreTools.RedumpLib.Data;
using SabreTools.RedumpLib.Web;

namespace MPF.Core
{
    /// <summary>
    /// Class to hold all SubmissionInfo-related methods
    /// </summary>
    internal static class SubmissionInfoTool
    {
        #region Creation

        /// <summary>
        /// Create a SubmissionInfo from a JSON file path
        /// </summary>
        /// <param name="path">Path to the SubmissionInfo JSON</param>
        /// <returns>Filled SubmissionInfo on success, null on error</returns>
#if NET48
        public static SubmissionInfo CreateFromFile(string path)
#else
        public static SubmissionInfo? CreateFromFile(string? path)
#endif
        {
            // If the path is invalid
            if (string.IsNullOrWhiteSpace(path))
                return null;

            // If the file doesn't exist
            if (!File.Exists(path))
                return null;

            // Try to open and deserialize the file
            try
            {
                byte[] data = File.ReadAllBytes(path);
                string dataString = Encoding.UTF8.GetString(data);
                return JsonConvert.DeserializeObject<SubmissionInfo>(dataString);
            }
            catch
            {
                // We don't care what the exception was
                return null;
            }
        }

        /// <summary>
        /// Create a new SubmissionInfo object from a disc page
        /// </summary>
        /// <param name="discData">String containing the HTML disc data</param>
        /// <returns>Filled SubmissionInfo object on success, null on error</returns>
        /// <remarks>Not currently working</remarks>
#if NET48
        public static SubmissionInfo CreateFromID(string discData)
#else
        private static SubmissionInfo? CreateFromID(string discData)
#endif
        {
            var info = new SubmissionInfo()
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
                var redumpPage = new XmlDocument() { PreserveWhitespace = true };
                redumpPage.LoadXml(discData);

                // If the current page isn't valid, we can't parse it
                if (!redumpPage.HasChildNodes)
                    return null;

                // Get the body node, if possible
                var bodyNode = redumpPage["html"]?["body"];
                if (bodyNode == null || !bodyNode.HasChildNodes)
                    return null;

                // Loop through and get the main node, if possible
#if NET48
                XmlNode mainNode = null;
#else
                XmlNode? mainNode = null;
#endif
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

                                        var gameInfoNodeHeader = gameInfoNode["th"];
                                        var gameInfoNodeData = gameInfoNode["td"];

                                        if (gameInfoNodeHeader == null || gameInfoNodeData == null)
                                        {
                                            // No-op for invalid data
                                        }
                                        else if (string.Equals(gameInfoNodeHeader.InnerText, "System", StringComparison.OrdinalIgnoreCase))
                                        {
                                            info.CommonDiscInfo.System = Extensions.ToRedumpSystem(gameInfoNodeData["a"]?.InnerText ?? string.Empty);
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
        /// Ensure all required sections in a submission info exist
        /// </summary>
        /// <param name="info">SubmissionInfo object to verify</param>
#if NET48
        public static SubmissionInfo EnsureAllSections(SubmissionInfo info)
        {
            // If there's no info, create one
            if (info == null) info = new SubmissionInfo();

            // Ensure all sections
            if (info.CommonDiscInfo == null) info.CommonDiscInfo = new CommonDiscInfoSection();
            if (info.VersionAndEditions == null) info.VersionAndEditions = new VersionAndEditionsSection();
            if (info.EDC == null) info.EDC = new EDCSection();
            if (info.Extras == null) info.Extras = new ExtrasSection();
            if (info.CopyProtection == null) info.CopyProtection = new CopyProtectionSection();
            if (info.TracksAndWriteOffsets == null) info.TracksAndWriteOffsets = new TracksAndWriteOffsetsSection();
            if (info.SizeAndChecksums == null) info.SizeAndChecksums = new SizeAndChecksumsSection();
            if (info.DumpingInfo == null) info.DumpingInfo = new DumpingInfoSection();

            // Ensure special dictionaries
            if (info.CommonDiscInfo.CommentsSpecialFields == null) info.CommonDiscInfo.CommentsSpecialFields = new Dictionary<SiteCode?, string>();
            if (info.CommonDiscInfo.ContentsSpecialFields == null) info.CommonDiscInfo.ContentsSpecialFields = new Dictionary<SiteCode?, string>();

            return info;
        }
#else
        public static SubmissionInfo EnsureAllSections(SubmissionInfo? info)
        {
            // If there's no info, create one
            info ??= new SubmissionInfo();

            // Ensure all sections
            info.CommonDiscInfo ??= new CommonDiscInfoSection();
            info.VersionAndEditions ??= new VersionAndEditionsSection();
            info.EDC ??= new EDCSection();
            info.ParentCloneRelationship ??= new ParentCloneRelationshipSection();
            info.Extras ??= new ExtrasSection();
            info.CopyProtection ??= new CopyProtectionSection();
            info.DumpersAndStatus ??= new DumpersAndStatusSection();
            info.TracksAndWriteOffsets ??= new TracksAndWriteOffsetsSection();
            info.SizeAndChecksums ??= new SizeAndChecksumsSection();
            info.DumpingInfo ??= new DumpingInfoSection();

            // Ensure special dictionaries
            info.CommonDiscInfo.CommentsSpecialFields ??= new Dictionary<SiteCode, string>();
            info.CommonDiscInfo.ContentsSpecialFields ??= new Dictionary<SiteCode, string>();

            return info;
        }
#endif

        #endregion

        #region Extraction and Filling

        /// <summary>
        /// Extract all of the possible information from a given input combination
        /// </summary>
        /// <param name="outputPath">Output path to write to</param>
        /// <param name="drive">Drive object representing the current drive</param>
        /// <param name="system">Currently selected system</param>
        /// <param name="mediaType">Currently selected media type</param>
        /// <param name="options">Options object representing user-defined options</param>
        /// <param name="parameters">Parameters object representing what to send to the internal program</param>
        /// <param name="resultProgress">Optional result progress callback</param>
        /// <param name="protectionProgress">Optional protection progress callback</param>
        /// <returns>SubmissionInfo populated based on outputs, null on error</returns>
#if NET48
        public static async Task<SubmissionInfo> ExtractOutputInformation(
#else
        public static async Task<SubmissionInfo?> ExtractOutputInformation(
#endif
            string outputPath,
#if NET48
            Drive drive,
#else
            Drive? drive,
#endif
            RedumpSystem? system,
            MediaType? mediaType,
            Data.Options options,
#if NET48
            BaseParameters parameters,
            IProgress<Result> resultProgress = null,
            IProgress<ProtectionProgress> protectionProgress = null)
#else
            BaseParameters? parameters,
            IProgress<Result>? resultProgress = null,
            IProgress<ProtectionProgress>? protectionProgress = null)
#endif
        {
            // Ensure the current disc combination should exist
            if (!system.MediaTypes().Contains(mediaType))
                return null;

            // Split the output path for easier use
            var outputDirectory = Path.GetDirectoryName(outputPath);
            string outputFilename = Path.GetFileName(outputPath);

            // Check that all of the relevant files are there
            (bool foundFiles, List<string> missingFiles) = InfoTool.FoundAllFiles(outputDirectory, outputFilename, parameters, false);
            if (!foundFiles)
            {
                resultProgress?.Report(Result.Failure($"There were files missing from the output:\n{string.Join("\n", missingFiles)}"));
                resultProgress?.Report(Result.Failure($"This may indicate an issue with the hardware or media, including unsupported devices.\nPlease see dumping program documentation for more details."));
                return null;
            }

            // Sanitize the output filename to strip off any potential extension
            outputFilename = Path.GetFileNameWithoutExtension(outputFilename);

            // Create the SubmissionInfo object with all user-inputted values by default
            string combinedBase;
            if (string.IsNullOrWhiteSpace(outputDirectory))
                combinedBase = outputFilename;
            else
                combinedBase = Path.Combine(outputDirectory, outputFilename);

            var info = new SubmissionInfo()
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
                },
                VersionAndEditions = new VersionAndEditionsSection()
                {
                    Version = options.AddPlaceholders ? Template.RequiredIfExistsValue : string.Empty,
                    OtherEditions = options.AddPlaceholders ? "(VERIFY THIS) Original" : string.Empty,
                },
            };

            // Ensure that required sections exist
            info = SubmissionInfoTool.EnsureAllSections(info);

            // Get specific tool output handling
            parameters?.GenerateSubmissionInfo(info, options, combinedBase, drive, options.IncludeArtifacts);

            // Get a list of matching IDs for each line in the DAT
#if NET48
            if (!string.IsNullOrEmpty(info.TracksAndWriteOffsets.ClrMameProData) && options.HasRedumpLogin)
                SubmissionInfoTool.FillFromRedump(options, info, resultProgress);
#else
            if (!string.IsNullOrEmpty(info.TracksAndWriteOffsets!.ClrMameProData) && options.HasRedumpLogin)
                _ = await SubmissionInfoTool.FillFromRedump(options, info, resultProgress);
#endif

            // If we have both ClrMamePro and Size and Checksums data, remove the ClrMamePro
            if (!string.IsNullOrWhiteSpace(info.SizeAndChecksums?.CRC32))
                info.TracksAndWriteOffsets.ClrMameProData = null;

            // Add the volume label to comments, if possible or necessary
            if (drive?.VolumeLabel != null && drive.GetRedumpSystemFromVolumeLabel() == null)
#if NET48
                info.CommonDiscInfo.CommentsSpecialFields[SiteCode.VolumeLabel] = drive.VolumeLabel;
#else
                info.CommonDiscInfo!.CommentsSpecialFields![SiteCode.VolumeLabel] = drive.VolumeLabel;
#endif

            // Extract info based generically on MediaType
            switch (mediaType)
            {
                case MediaType.CDROM:
                case MediaType.GDROM:
#if NET48
                    info.CommonDiscInfo.Layer0MasteringRing = options.AddPlaceholders ? Template.RequiredIfExistsValue : string.Empty;
#else
                    info.CommonDiscInfo!.Layer0MasteringRing = options.AddPlaceholders ? Template.RequiredIfExistsValue : string.Empty;
#endif
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
#if NET48
                    if (info.SizeAndChecksums.Layerbreak == default)
#else
                    if (info.SizeAndChecksums!.Layerbreak == default)
#endif
                    {
#if NET48
                        info.CommonDiscInfo.Layer0MasteringRing = options.AddPlaceholders ? Template.RequiredIfExistsValue : string.Empty;
#else
                        info.CommonDiscInfo!.Layer0MasteringRing = options.AddPlaceholders ? Template.RequiredIfExistsValue : string.Empty;
#endif
                        info.CommonDiscInfo.Layer0MasteringSID = options.AddPlaceholders ? Template.RequiredIfExistsValue : string.Empty;
                        info.CommonDiscInfo.Layer0ToolstampMasteringCode = options.AddPlaceholders ? Template.RequiredIfExistsValue : string.Empty;
                        info.CommonDiscInfo.Layer0MouldSID = options.AddPlaceholders ? Template.RequiredIfExistsValue : string.Empty;
                        info.CommonDiscInfo.Layer1MouldSID = options.AddPlaceholders ? Template.RequiredIfExistsValue : string.Empty;
                        info.CommonDiscInfo.Layer0AdditionalMould = options.AddPlaceholders ? Template.RequiredIfExistsValue : string.Empty;
                    }
                    // If we have a dual-layer disc
                    else
                    {
#if NET48
                        info.CommonDiscInfo.Layer0MasteringRing = options.AddPlaceholders ? Template.RequiredIfExistsValue : string.Empty;
#else
                        info.CommonDiscInfo!.Layer0MasteringRing = options.AddPlaceholders ? Template.RequiredIfExistsValue : string.Empty;
#endif
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
#if NET48
                    info.CommonDiscInfo.Layer0MasteringRing = options.AddPlaceholders ? Template.RequiredIfExistsValue : string.Empty;
#else
                    info.CommonDiscInfo!.Layer0MasteringRing = options.AddPlaceholders ? Template.RequiredIfExistsValue : string.Empty;
#endif
                    info.CommonDiscInfo.Layer0MasteringSID = options.AddPlaceholders ? Template.RequiredIfExistsValue : string.Empty;
                    info.CommonDiscInfo.Layer0ToolstampMasteringCode = options.AddPlaceholders ? Template.RequiredIfExistsValue : string.Empty;
                    info.CommonDiscInfo.Layer0MouldSID = options.AddPlaceholders ? Template.RequiredIfExistsValue : string.Empty;
                    info.CommonDiscInfo.Layer1MouldSID = options.AddPlaceholders ? Template.RequiredIfExistsValue : string.Empty;
                    info.CommonDiscInfo.Layer0AdditionalMould = options.AddPlaceholders ? Template.RequiredIfExistsValue : string.Empty;
#if NET48
                    info.Extras.BCA = info.Extras.BCA ?? (options.AddPlaceholders ? Template.RequiredValue : string.Empty);
#else
                    info.Extras!.BCA ??= (options.AddPlaceholders ? Template.RequiredValue : string.Empty);
#endif
                    break;

                case MediaType.NintendoWiiOpticalDisc:

                    // If we have a single-layer disc
#if NET48
                    if (info.SizeAndChecksums.Layerbreak == default)
#else
                    if (info.SizeAndChecksums!.Layerbreak == default)
#endif
                    {
#if NET48
                        info.CommonDiscInfo.Layer0MasteringRing = options.AddPlaceholders ? Template.RequiredIfExistsValue : string.Empty;
#else
                        info.CommonDiscInfo!.Layer0MasteringRing = options.AddPlaceholders ? Template.RequiredIfExistsValue : string.Empty;
#endif
                        info.CommonDiscInfo.Layer0MasteringSID = options.AddPlaceholders ? Template.RequiredIfExistsValue : string.Empty;
                        info.CommonDiscInfo.Layer0ToolstampMasteringCode = options.AddPlaceholders ? Template.RequiredIfExistsValue : string.Empty;
                        info.CommonDiscInfo.Layer0MouldSID = options.AddPlaceholders ? Template.RequiredIfExistsValue : string.Empty;
                        info.CommonDiscInfo.Layer1MouldSID = options.AddPlaceholders ? Template.RequiredIfExistsValue : string.Empty;
                        info.CommonDiscInfo.Layer0AdditionalMould = options.AddPlaceholders ? Template.RequiredIfExistsValue : string.Empty;
                    }
                    // If we have a dual-layer disc
                    else
                    {
#if NET48
                        info.CommonDiscInfo.Layer0MasteringRing = options.AddPlaceholders ? Template.RequiredIfExistsValue : string.Empty;
#else
                        info.CommonDiscInfo!.Layer0MasteringRing = options.AddPlaceholders ? Template.RequiredIfExistsValue : string.Empty;
#endif
                        info.CommonDiscInfo.Layer0MasteringSID = options.AddPlaceholders ? Template.RequiredIfExistsValue : string.Empty;
                        info.CommonDiscInfo.Layer0ToolstampMasteringCode = options.AddPlaceholders ? Template.RequiredIfExistsValue : string.Empty;
                        info.CommonDiscInfo.Layer0MouldSID = options.AddPlaceholders ? Template.RequiredIfExistsValue : string.Empty;
                        info.CommonDiscInfo.Layer0AdditionalMould = options.AddPlaceholders ? Template.RequiredIfExistsValue : string.Empty;

                        info.CommonDiscInfo.Layer1MasteringRing = options.AddPlaceholders ? Template.RequiredIfExistsValue : string.Empty;
                        info.CommonDiscInfo.Layer1MasteringSID = options.AddPlaceholders ? Template.RequiredIfExistsValue : string.Empty;
                        info.CommonDiscInfo.Layer1ToolstampMasteringCode = options.AddPlaceholders ? Template.RequiredIfExistsValue : string.Empty;
                        info.CommonDiscInfo.Layer1MouldSID = options.AddPlaceholders ? Template.RequiredIfExistsValue : string.Empty;
                    }

#if NET48
                    info.Extras.DiscKey = options.AddPlaceholders ? Template.RequiredValue : string.Empty;
#else
                    info.Extras!.DiscKey = options.AddPlaceholders ? Template.RequiredValue : string.Empty;
#endif
                    info.Extras.BCA = info.Extras.BCA ?? (options.AddPlaceholders ? Template.RequiredValue : string.Empty);

                    break;

                case MediaType.UMD:
                    // Both single- and dual-layer discs have two "layers" for the ring
#if NET48
                    info.CommonDiscInfo.Layer0MasteringRing = options.AddPlaceholders ? Template.RequiredIfExistsValue : string.Empty;
#else
                    info.CommonDiscInfo!.Layer0MasteringRing = options.AddPlaceholders ? Template.RequiredIfExistsValue : string.Empty;
#endif
                    info.CommonDiscInfo.Layer0MasteringSID = options.AddPlaceholders ? Template.RequiredIfExistsValue : string.Empty;
                    info.CommonDiscInfo.Layer0ToolstampMasteringCode = options.AddPlaceholders ? Template.RequiredIfExistsValue : string.Empty;
                    info.CommonDiscInfo.Layer0MouldSID = options.AddPlaceholders ? Template.RequiredIfExistsValue : string.Empty;

                    info.CommonDiscInfo.Layer1MasteringRing = options.AddPlaceholders ? Template.RequiredIfExistsValue : string.Empty;
                    info.CommonDiscInfo.Layer1MasteringSID = options.AddPlaceholders ? Template.RequiredIfExistsValue : string.Empty;
                    info.CommonDiscInfo.Layer1ToolstampMasteringCode = options.AddPlaceholders ? Template.RequiredIfExistsValue : string.Empty;

#if NET48
                    info.SizeAndChecksums.CRC32 = info.SizeAndChecksums.CRC32 ?? (options.AddPlaceholders ? Template.RequiredValue + " [Not automatically generated for UMD]" : string.Empty);
                    info.SizeAndChecksums.MD5 = info.SizeAndChecksums.MD5 ?? (options.AddPlaceholders ? Template.RequiredValue + " [Not automatically generated for UMD]" : string.Empty);
                    info.SizeAndChecksums.SHA1 = info.SizeAndChecksums.SHA1 ?? (options.AddPlaceholders ? Template.RequiredValue + " [Not automatically generated for UMD]" : string.Empty);
#else
                    info.SizeAndChecksums!.CRC32 ??= (options.AddPlaceholders ? Template.RequiredValue + " [Not automatically generated for UMD]" : string.Empty);
                    info.SizeAndChecksums.MD5 ??= (options.AddPlaceholders ? Template.RequiredValue + " [Not automatically generated for UMD]" : string.Empty);
                    info.SizeAndChecksums.SHA1 ??= (options.AddPlaceholders ? Template.RequiredValue + " [Not automatically generated for UMD]" : string.Empty);
#endif
                    info.TracksAndWriteOffsets.ClrMameProData = null;
                    break;
            }

            // Extract info based specifically on RedumpSystem
            switch (system)
            {
                case RedumpSystem.AcornArchimedes:
#if NET48
                    info.CommonDiscInfo.Region = info.CommonDiscInfo.Region ?? Region.UnitedKingdom;
#else
                    info.CommonDiscInfo!.Region ??= Region.UnitedKingdom;
#endif
                    break;

                case RedumpSystem.AppleMacintosh:
                case RedumpSystem.EnhancedCD:
                case RedumpSystem.IBMPCcompatible:
                case RedumpSystem.PalmOS:
                case RedumpSystem.PocketPC:
                case RedumpSystem.RainbowDisc:
                case RedumpSystem.SonyElectronicBook:
                    resultProgress?.Report(Result.Success("Running copy protection scan... this might take a while!"));
                    var (protectionString, fullProtections) = await InfoTool.GetCopyProtection(drive, options, protectionProgress);

#if NET48
                    info.CopyProtection.Protection = protectionString;
                    info.CopyProtection.FullProtections = fullProtections ?? new Dictionary<string, List<string>>();
#else
                    info.CopyProtection!.Protection = protectionString;
                    info.CopyProtection.FullProtections = fullProtections as Dictionary<string, List<string>?> ?? new Dictionary<string, List<string>?>();
#endif
                    resultProgress?.Report(Result.Success("Copy protection scan complete!"));

                    break;

                case RedumpSystem.AudioCD:
                case RedumpSystem.DVDAudio:
                case RedumpSystem.SuperAudioCD:
#if NET48
                    info.CommonDiscInfo.Category = info.CommonDiscInfo.Category ?? DiscCategory.Audio;
#else
                    info.CommonDiscInfo!.Category ??= DiscCategory.Audio;
#endif
                    break;

                case RedumpSystem.BandaiPlaydiaQuickInteractiveSystem:
#if NET48
                    info.CommonDiscInfo.EXEDateBuildDate = options.AddPlaceholders ? Template.RequiredValue : string.Empty;
#else
                    info.CommonDiscInfo!.EXEDateBuildDate = options.AddPlaceholders ? Template.RequiredValue : string.Empty;
#endif
                    info.CommonDiscInfo.Region = info.CommonDiscInfo.Region ?? Region.Japan;
                    break;

                case RedumpSystem.BDVideo:
#if NET48
                    info.CommonDiscInfo.Category = info.CommonDiscInfo.Category ?? DiscCategory.BonusDiscs;
                    info.CopyProtection.Protection = options.AddPlaceholders ? Template.RequiredIfExistsValue : string.Empty;
#else
                    info.CommonDiscInfo!.Category ??= DiscCategory.BonusDiscs;
                    info.CopyProtection!.Protection = options.AddPlaceholders ? Template.RequiredIfExistsValue : string.Empty;
#endif
                    break;

                case RedumpSystem.CommodoreAmigaCD:
#if NET48
                    info.CommonDiscInfo.EXEDateBuildDate = options.AddPlaceholders ? Template.RequiredValue : string.Empty;
#else
                    info.CommonDiscInfo!.EXEDateBuildDate = options.AddPlaceholders ? Template.RequiredValue : string.Empty;
#endif
                    break;

                case RedumpSystem.CommodoreAmigaCD32:
#if NET48
                    info.CommonDiscInfo.EXEDateBuildDate = options.AddPlaceholders ? Template.RequiredValue : string.Empty;
                    info.CommonDiscInfo.Region = info.CommonDiscInfo.Region ?? Region.Europe;
#else
                    info.CommonDiscInfo!.EXEDateBuildDate = options.AddPlaceholders ? Template.RequiredValue : string.Empty;
                    info.CommonDiscInfo.Region ??= Region.Europe;
#endif
                    break;

                case RedumpSystem.CommodoreAmigaCDTV:
#if NET48
                    info.CommonDiscInfo.EXEDateBuildDate = options.AddPlaceholders ? Template.RequiredValue : string.Empty;
                    info.CommonDiscInfo.Region = info.CommonDiscInfo.Region ?? Region.Europe;
#else
                    info.CommonDiscInfo!.EXEDateBuildDate = options.AddPlaceholders ? Template.RequiredValue : string.Empty;
                    info.CommonDiscInfo.Region ??= Region.Europe;
#endif
                    break;

                case RedumpSystem.DVDVideo:
#if NET48
                    info.CommonDiscInfo.Category = info.CommonDiscInfo.Category ?? DiscCategory.BonusDiscs;
#else
                    info.CommonDiscInfo!.Category ??= DiscCategory.BonusDiscs;
#endif
                    break;

                case RedumpSystem.FujitsuFMTownsseries:
#if NET48
                    info.CommonDiscInfo.EXEDateBuildDate = options.AddPlaceholders ? Template.RequiredValue : string.Empty;
#else
                    info.CommonDiscInfo!.EXEDateBuildDate = options.AddPlaceholders ? Template.RequiredValue : string.Empty;
#endif
                    info.CommonDiscInfo.Region = info.CommonDiscInfo.Region ?? Region.Japan;
                    break;

                case RedumpSystem.FujitsuFMTownsMarty:
#if NET48
                    info.CommonDiscInfo.Region = info.CommonDiscInfo.Region ?? Region.Japan;
#else
                    info.CommonDiscInfo!.Region ??= Region.Japan;
#endif
                    break;

                case RedumpSystem.IncredibleTechnologiesEagle:
#if NET48
                    info.CommonDiscInfo.EXEDateBuildDate = options.AddPlaceholders ? Template.RequiredValue : string.Empty;
#else
                    info.CommonDiscInfo!.EXEDateBuildDate = options.AddPlaceholders ? Template.RequiredValue : string.Empty;
#endif
                    break;

                case RedumpSystem.KonamieAmusement:
#if NET48
                    info.CommonDiscInfo.EXEDateBuildDate = options.AddPlaceholders ? Template.RequiredValue : string.Empty;
#else
                    info.CommonDiscInfo!.EXEDateBuildDate = options.AddPlaceholders ? Template.RequiredValue : string.Empty;
#endif
                    break;

                case RedumpSystem.KonamiFireBeat:
#if NET48
                    info.CommonDiscInfo.EXEDateBuildDate = options.AddPlaceholders ? Template.RequiredValue : string.Empty;
#else
                    info.CommonDiscInfo!.EXEDateBuildDate = options.AddPlaceholders ? Template.RequiredValue : string.Empty;
#endif
                    break;

                case RedumpSystem.KonamiSystemGV:
#if NET48
                    info.CommonDiscInfo.EXEDateBuildDate = options.AddPlaceholders ? Template.RequiredValue : string.Empty;
#else
                    info.CommonDiscInfo!.EXEDateBuildDate = options.AddPlaceholders ? Template.RequiredValue : string.Empty;
#endif
                    break;

                case RedumpSystem.KonamiSystem573:
#if NET48
                    info.CommonDiscInfo.EXEDateBuildDate = options.AddPlaceholders ? Template.RequiredValue : string.Empty;
#else
                    info.CommonDiscInfo!.EXEDateBuildDate = options.AddPlaceholders ? Template.RequiredValue : string.Empty;
#endif
                    break;

                case RedumpSystem.KonamiTwinkle:
#if NET48
                    info.CommonDiscInfo.EXEDateBuildDate = options.AddPlaceholders ? Template.RequiredValue : string.Empty;
#else
                    info.CommonDiscInfo!.EXEDateBuildDate = options.AddPlaceholders ? Template.RequiredValue : string.Empty;
#endif
                    break;

                case RedumpSystem.MattelHyperScan:
#if NET48
                    info.CommonDiscInfo.EXEDateBuildDate = options.AddPlaceholders ? Template.RequiredValue : string.Empty;
#else
                    info.CommonDiscInfo!.EXEDateBuildDate = options.AddPlaceholders ? Template.RequiredValue : string.Empty;
#endif
                    break;

                case RedumpSystem.MicrosoftXboxOne:
                    if (drive?.Name != null)
                    {
                        string xboxOneMsxcPath = Path.Combine(drive.Name, "MSXC");
                        if (drive != null && Directory.Exists(xboxOneMsxcPath))
                        {
#if NET48
                            info.CommonDiscInfo.CommentsSpecialFields[SiteCode.Filename] = string.Join("\n",
#else
                            info.CommonDiscInfo!.CommentsSpecialFields![SiteCode.Filename] = string.Join("\n",
#endif
                                Directory.GetFiles(xboxOneMsxcPath, "*", SearchOption.TopDirectoryOnly).Select(Path.GetFileName));
                        }
                    }

                    break;

                case RedumpSystem.MicrosoftXboxSeriesXS:
                    if (drive?.Name != null)
                    {
                        string xboxSeriesXMsxcPath = Path.Combine(drive.Name, "MSXC");
                        if (drive != null && Directory.Exists(xboxSeriesXMsxcPath))
                        {
#if NET48
                            info.CommonDiscInfo.CommentsSpecialFields[SiteCode.Filename] = string.Join("\n",
#else
                            info.CommonDiscInfo!.CommentsSpecialFields![SiteCode.Filename] = string.Join("\n",
#endif
                                Directory.GetFiles(xboxSeriesXMsxcPath, "*", SearchOption.TopDirectoryOnly).Select(Path.GetFileName));
                        }
                    }

                    break;

                case RedumpSystem.NamcoSegaNintendoTriforce:
#if NET48
                    info.CommonDiscInfo.EXEDateBuildDate = options.AddPlaceholders ? Template.RequiredValue : string.Empty;
#else
                    info.CommonDiscInfo!.EXEDateBuildDate = options.AddPlaceholders ? Template.RequiredValue : string.Empty;
#endif
                    break;

                case RedumpSystem.NavisoftNaviken21:
#if NET48
                    info.CommonDiscInfo.EXEDateBuildDate = options.AddPlaceholders ? Template.RequiredValue : string.Empty;
                    info.CommonDiscInfo.Region = info.CommonDiscInfo.Region ?? Region.Japan;
#else
                    info.CommonDiscInfo!.EXEDateBuildDate = options.AddPlaceholders ? Template.RequiredValue : string.Empty;
                    info.CommonDiscInfo.Region ??= Region.Japan;
#endif
                    break;

                case RedumpSystem.NECPC88series:
#if NET48
                    info.CommonDiscInfo.Region = info.CommonDiscInfo.Region ?? Region.Japan;
#else
                    info.CommonDiscInfo!.Region ??= Region.Japan;
#endif
                    break;

                case RedumpSystem.NECPC98series:
#if NET48
                    info.CommonDiscInfo.EXEDateBuildDate = options.AddPlaceholders ? Template.RequiredValue : string.Empty;
                    info.CommonDiscInfo.Region = info.CommonDiscInfo.Region ?? Region.Japan;
#else
                    info.CommonDiscInfo!.EXEDateBuildDate = options.AddPlaceholders ? Template.RequiredValue : string.Empty;
                    info.CommonDiscInfo!.Region ??= Region.Japan;
#endif
                    break;

                case RedumpSystem.NECPCFXPCFXGA:
#if NET48
                    info.CommonDiscInfo.Region = info.CommonDiscInfo.Region ?? Region.Japan;
#else
                    info.CommonDiscInfo!.Region ??= Region.Japan;
#endif
                    break;

                case RedumpSystem.SegaChihiro:
#if NET48
                    info.CommonDiscInfo.EXEDateBuildDate = options.AddPlaceholders ? Template.RequiredValue : string.Empty;
#else
                    info.CommonDiscInfo!.EXEDateBuildDate = options.AddPlaceholders ? Template.RequiredValue : string.Empty;
#endif
                    break;

                case RedumpSystem.SegaDreamcast:
#if NET48
                    info.CommonDiscInfo.EXEDateBuildDate = options.AddPlaceholders ? Template.RequiredValue : string.Empty;
#else
                    info.CommonDiscInfo!.EXEDateBuildDate = options.AddPlaceholders ? Template.RequiredValue : string.Empty;
#endif
                    break;

                case RedumpSystem.SegaNaomi:
#if NET48
                    info.CommonDiscInfo.EXEDateBuildDate = options.AddPlaceholders ? Template.RequiredValue : string.Empty;
#else
                    info.CommonDiscInfo!.EXEDateBuildDate = options.AddPlaceholders ? Template.RequiredValue : string.Empty;
#endif
                    break;

                case RedumpSystem.SegaNaomi2:
#if NET48
                    info.CommonDiscInfo.EXEDateBuildDate = options.AddPlaceholders ? Template.RequiredValue : string.Empty;
#else
                    info.CommonDiscInfo!.EXEDateBuildDate = options.AddPlaceholders ? Template.RequiredValue : string.Empty;
#endif
                    break;

                case RedumpSystem.SegaTitanVideo:
#if NET48
                    info.CommonDiscInfo.EXEDateBuildDate = options.AddPlaceholders ? Template.RequiredValue : string.Empty;
#else
                    info.CommonDiscInfo!.EXEDateBuildDate = options.AddPlaceholders ? Template.RequiredValue : string.Empty;
#endif
                    break;

                case RedumpSystem.SharpX68000:
#if NET48
                    info.CommonDiscInfo.Region = info.CommonDiscInfo.Region ?? Region.Japan;
#else
                    info.CommonDiscInfo!.Region ??= Region.Japan;
#endif
                    break;

                case RedumpSystem.SNKNeoGeoCD:
#if NET48
                    info.CommonDiscInfo.EXEDateBuildDate = options.AddPlaceholders ? Template.RequiredValue : string.Empty;
#else
                    info.CommonDiscInfo!.EXEDateBuildDate = options.AddPlaceholders ? Template.RequiredValue : string.Empty;
#endif
                    break;

                case RedumpSystem.SonyPlayStation:
                    // Only check the disc if the dumping program couldn't detect
#if NET48
                    if (drive != null && info.CopyProtection.AntiModchip == YesNo.NULL)
#else
                    if (drive != null && info.CopyProtection!.AntiModchip == YesNo.NULL)
#endif
                    {
                        resultProgress?.Report(Result.Success("Checking for anti-modchip strings... this might take a while!"));
                        info.CopyProtection.AntiModchip = await InfoTool.GetAntiModchipDetected(drive) ? YesNo.Yes : YesNo.No;
                        resultProgress?.Report(Result.Success("Anti-modchip string scan complete!"));
                    }

                    // Special case for DIC only
                    if (parameters?.InternalProgram == InternalProgram.DiscImageCreator)
                    {
                        resultProgress?.Report(Result.Success("Checking for LibCrypt status... this might take a while!"));
                        InfoTool.GetLibCryptDetected(info, combinedBase);
                        resultProgress?.Report(Result.Success("LibCrypt status checking complete!"));
                    }

                    break;

                case RedumpSystem.SonyPlayStation2:
#if NET48
                    info.CommonDiscInfo.LanguageSelection = new LanguageSelection?[] { LanguageSelection.BiosSettings, LanguageSelection.LanguageSelector, LanguageSelection.OptionsMenu };
#else
                    info.CommonDiscInfo!.LanguageSelection = new LanguageSelection?[] { LanguageSelection.BiosSettings, LanguageSelection.LanguageSelector, LanguageSelection.OptionsMenu };
#endif
                    break;

                case RedumpSystem.SonyPlayStation3:
#if NET48
                    info.Extras.DiscKey = options.AddPlaceholders ? Template.RequiredValue : string.Empty;
#else
                    info.Extras!.DiscKey = options.AddPlaceholders ? Template.RequiredValue : string.Empty;
#endif
                    info.Extras.DiscID = options.AddPlaceholders ? Template.RequiredValue : string.Empty;
                    break;

                case RedumpSystem.TomyKissSite:
#if NET48
                    info.CommonDiscInfo.Region = info.CommonDiscInfo.Region ?? Region.Japan;
#else
                    info.CommonDiscInfo!.Region ??= Region.Japan;
#endif
                    break;

                case RedumpSystem.ZAPiTGamesGameWaveFamilyEntertainmentSystem:
#if NET48
                    info.CopyProtection.Protection = options.AddPlaceholders ? Template.RequiredIfExistsValue : string.Empty;
#else
                    info.CopyProtection!.Protection = options.AddPlaceholders ? Template.RequiredIfExistsValue : string.Empty;
#endif
                    break;
            }

            // Set the category if it's not overriden
#if NET48
            info.CommonDiscInfo.Category = info.CommonDiscInfo.Category ?? DiscCategory.Games;
#else
            info.CommonDiscInfo!.Category ??= DiscCategory.Games;
#endif

            // Comments and contents have odd handling
            if (string.IsNullOrEmpty(info.CommonDiscInfo.Comments))
                info.CommonDiscInfo.Comments = options.AddPlaceholders ? Template.OptionalValue : string.Empty;
            if (string.IsNullOrEmpty(info.CommonDiscInfo.Contents))
                info.CommonDiscInfo.Contents = options.AddPlaceholders ? Template.OptionalValue : string.Empty;

            // Normalize the disc type with all current information
            InfoTool.NormalizeDiscType(info);

            return info;
        }

        /// <summary>
        /// Fill out an existing SubmissionInfo object based on a disc page
        /// </summary>
        /// <param name="wc">RedumpWebClient for making the connection</param>
        /// <param name="info">Existing SubmissionInfo object to fill</param>
        /// <param name="id">Redump disc ID to retrieve</param>
        /// <param name="includeAllData">True to include all pullable information, false to do bare minimum</param>
#if NET48
        public static bool FillFromId(RedumpWebClient wc, SubmissionInfo info, int id, bool includeAllData)
        {
            // Ensure that required sections exist
            info = EnsureAllSections(info);

            string discData = wc.DownloadSingleSiteID(id);
            if (string.IsNullOrEmpty(discData))
                return false;
#else
        public async static Task<bool> FillFromId(RedumpHttpClient wc, SubmissionInfo info, int id, bool includeAllData)
        {
            // Ensure that required sections exist
            info = EnsureAllSections(info);

            var discData = await wc.DownloadSingleSiteID(id);
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
#if NET48
                    info.CommonDiscInfo.Title = title.Substring(0, firstParenLocation);
#else
                    info.CommonDiscInfo!.Title = title[..firstParenLocation];
#endif
                    var subMatches = Constants.DiscNumberLetterRegex.Matches(title);
                    foreach (Match subMatch in subMatches.Cast<Match>())
                    {
                        var subMatchValue = subMatch.Groups[1].Value;

                        // Disc number or letter
                        if (subMatchValue.StartsWith("Disc"))
                            info.CommonDiscInfo.DiscNumberLetter = subMatchValue.Remove(0, "Disc ".Length);

                        // Issue number
                        else if (subMatchValue.All(c => char.IsNumber(c)))
                            info.CommonDiscInfo.Title += $" ({subMatchValue})";

                        // Disc title
                        else
                            info.CommonDiscInfo.DiscTitle = subMatchValue;
                    }
                }
                // Otherwise, leave the title as-is
                else
                {
#if NET48
                    info.CommonDiscInfo.Title = title;
#else
                    info.CommonDiscInfo!.Title = title;
#endif
                }
            }

            // Foreign Title
            match = Constants.ForeignTitleRegex.Match(discData);
            if (match.Success)
#if NET48
                info.CommonDiscInfo.ForeignTitleNonLatin = WebUtility.HtmlDecode(match.Groups[1].Value);
#else
                info.CommonDiscInfo!.ForeignTitleNonLatin = WebUtility.HtmlDecode(match.Groups[1].Value);
#endif
            else
#if NET48
                info.CommonDiscInfo.ForeignTitleNonLatin = null;
#else
                info.CommonDiscInfo!.ForeignTitleNonLatin = null;
#endif

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
                var tempLanguages = new List<Language?>();
                foreach (Match submatch in matches.Cast<Match>())
                {
                    tempLanguages.Add(Extensions.ToLanguage(submatch.Groups[1].Value));
                }

                info.CommonDiscInfo.Languages = tempLanguages.Where(l => l != null).ToArray();
            }

            // Serial
            if (includeAllData)
            {
                // TODO: Re-enable if there's a way of verifying against a disc
                //match = Constants.SerialRegex.Match(discData);
                //if (match.Success)
                //    info.CommonDiscInfo.Serial = $"(VERIFY THIS) {WebUtility.HtmlDecode(match.Groups[1].Value)}";
            }

            // Error count
            if (string.IsNullOrEmpty(info.CommonDiscInfo.ErrorsCount))
            {
                match = Constants.ErrorCountRegex.Match(discData);
                if (match.Success)
                    info.CommonDiscInfo.ErrorsCount = match.Groups[1].Value;
            }

            // Version
#if NET48
            if (info.VersionAndEditions.Version == null)
#else
            if (info.VersionAndEditions!.Version == null)
#endif
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
                var tempDumpers = new List<string>();
#if NET48
                if (info.DumpersAndStatus.Dumpers != null && info.DumpersAndStatus.Dumpers.Length > 0)
#else
                if (info.DumpersAndStatus!.Dumpers != null && info.DumpersAndStatus.Dumpers.Length > 0)
#endif
                {
                    foreach (string dumper in info.DumpersAndStatus.Dumpers)
                        tempDumpers.Add(dumper);
                }

                foreach (Match submatch in matches.Cast<Match>())
                {
                    tempDumpers.Add(WebUtility.HtmlDecode(submatch.Groups[1].Value));
                }

                info.DumpersAndStatus.Dumpers = tempDumpers.ToArray();
            }

            // TODO: Unify handling of fields that can include site codes (Comments/Contents)

            // Comments
            if (includeAllData)
            {
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
                            // If we have a null site code, just skip
                            if (siteCode == null)
                                continue;

                            // If the line doesn't contain this tag, just skip
                            var shortName = siteCode.ShortName();
                            if (shortName == null || !commentLine.Contains(shortName))
                                continue;

                            // Mark as having found a tag
                            foundTag = true;

                            // Cache the current site code
                            lastSiteCode = siteCode;

                            // A subset of tags can be multiline
                            addToLast = IsMultiLine(siteCode);

                            // Skip certain site codes because of data issues
                            switch (siteCode)
                            {
                                // Multiple
                                case SiteCode.InternalSerialName:
                                case SiteCode.Multisession:
                                case SiteCode.VolumeLabel:
                                    continue;

                                // Audio CD
                                case SiteCode.RingNonZeroDataStart:
                                case SiteCode.UniversalHash:
                                    continue;

                                // Microsoft Xbox and Xbox 360
                                case SiteCode.DMIHash:
                                case SiteCode.PFIHash:
                                case SiteCode.SSHash:
                                case SiteCode.SSVersion:
                                case SiteCode.XMID:
                                case SiteCode.XeMID:
                                    continue;

                                // Microsoft Xbox One and Series X/S
                                case SiteCode.Filename:
                                    continue;

                                // Nintendo Gamecube
                                case SiteCode.InternalName:
                                    continue;
                            }

                            // If we don't already have this site code, add it to the dictionary
#if NET48
                            if (!info.CommonDiscInfo.CommentsSpecialFields.ContainsKey(siteCode.Value))
#else
                            if (!info.CommonDiscInfo.CommentsSpecialFields!.ContainsKey(siteCode.Value))
#endif
                                info.CommonDiscInfo.CommentsSpecialFields[siteCode.Value] = $"(VERIFY THIS) {commentLine.Replace(shortName, string.Empty).Trim()}";

                            // Otherwise, append the value to the existing key
                            else
                                info.CommonDiscInfo.CommentsSpecialFields[siteCode.Value] += $", {commentLine.Replace(shortName, string.Empty).Trim()}";

                            break;
                        }

                        // If we didn't find a known tag, just add the line, just in case
                        if (!foundTag)
                        {
                            if (addToLast && lastSiteCode != null)
                            {
#if NET48
                                if (!string.IsNullOrWhiteSpace(info.CommonDiscInfo.CommentsSpecialFields[lastSiteCode.Value]))
#else
                                if (!string.IsNullOrWhiteSpace(info.CommonDiscInfo.CommentsSpecialFields![lastSiteCode.Value]))
#endif
                                    info.CommonDiscInfo.CommentsSpecialFields[lastSiteCode.Value] += "\n";

                                info.CommonDiscInfo.CommentsSpecialFields[lastSiteCode.Value] += commentLine;
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
            }

            // Contents
            if (includeAllData)
            {
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
                            // If we have a null site code, just skip
                            if (siteCode == null)
                                continue;

                            // If the line doesn't contain this tag, just skip
                            var shortName = siteCode.ShortName();
                            if (shortName == null || !contentLine.Contains(shortName))
                                continue;

                            // Cache the current site code
                            lastSiteCode = siteCode;

                            // If we don't already have this site code, add it to the dictionary
#if NET48
                            if (!info.CommonDiscInfo.ContentsSpecialFields.ContainsKey(siteCode.Value))
#else
                            if (!info.CommonDiscInfo.ContentsSpecialFields!.ContainsKey(siteCode.Value))
#endif
                                info.CommonDiscInfo.ContentsSpecialFields[siteCode.Value] = $"(VERIFY THIS) {contentLine.Replace(shortName, string.Empty).Trim()}";

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
#if NET48
                                if (!string.IsNullOrWhiteSpace(info.CommonDiscInfo.ContentsSpecialFields[lastSiteCode.Value]))
#else
                                if (!string.IsNullOrWhiteSpace(info.CommonDiscInfo.ContentsSpecialFields![lastSiteCode.Value]))
#endif
                                    info.CommonDiscInfo.ContentsSpecialFields[lastSiteCode.Value] += "\n";

                                info.CommonDiscInfo.ContentsSpecialFields[lastSiteCode.Value] += contentLine;
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
#if NET48
        public static bool FillFromRedump(Data.Options options, SubmissionInfo info, IProgress<Result> resultProgress = null)
#else
        public async static Task<bool> FillFromRedump(Data.Options options, SubmissionInfo info, IProgress<Result>? resultProgress = null)
#endif
        {
            // If no username is provided
            if (string.IsNullOrWhiteSpace(options.RedumpUsername) || string.IsNullOrWhiteSpace(options.RedumpPassword))
                return false;

            // Set the current dumper based on username
#if NET48
            if (info.DumpersAndStatus == null) info.DumpersAndStatus = new DumpersAndStatusSection();
#else
            info.DumpersAndStatus ??= new DumpersAndStatusSection();
#endif
            info.DumpersAndStatus.Dumpers = new string[] { options.RedumpUsername };
            info.PartiallyMatchedIDs = new List<int>();

#if NET48
            using (var wc = new RedumpWebClient())
#else
            using (var wc = new RedumpHttpClient())
#endif
            {
                // Login to Redump
#if NET48
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
#if NET48
                List<int> fullyMatchedIDs = null;
#else
                List<int>? fullyMatchedIDs = null;
#endif

                // Loop through all of the hashdata to find matching IDs
                resultProgress?.Report(Result.Success("Finding disc matches on Redump..."));
                var splitData = info.TracksAndWriteOffsets?.ClrMameProData?.TrimEnd('\n')?.Split('\n');
                int trackCount = splitData?.Length ?? 0;
                foreach (string hashData in splitData ?? Array.Empty<string>())
                {
                    // Catch any errant blank lines
                    if (string.IsNullOrWhiteSpace(hashData))
                    {
                        trackCount--;
                        resultProgress?.Report(Result.Success("Blank line found, skipping!"));
                        continue;
                    }

                    // If the line ends in a known extra track names, skip them for checking
                    if (hashData.Contains("(Track 0).bin")
                        || hashData.Contains("(Track 0.2).bin")
                        || hashData.Contains("(Track 00).bin")
                        || hashData.Contains("(Track 00.2).bin")
                        || hashData.Contains("(Track A).bin")
                        || hashData.Contains("(Track AA).bin"))
                    {
                        trackCount--;
                        resultProgress?.Report(Result.Success("Extra track found, skipping!"));
                        continue;
                    }

#if NET48
                    (bool singleFound, List<int> foundIds) = ValidateSingleTrack(wc, info, hashData, resultProgress);
#else
                    (bool singleFound, var foundIds) = await ValidateSingleTrack(wc, info, hashData, resultProgress);
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

                // If we don't have any matches but we have a universal hash
                if (!info.PartiallyMatchedIDs.Any() && info.CommonDiscInfo?.CommentsSpecialFields?.ContainsKey(SiteCode.UniversalHash) == true)
                {
#if NET48
                    (bool singleFound, List<int> foundIds) = ValidateUniversalHash(wc, info, resultProgress);
#else
                    (bool singleFound, var foundIds) = await ValidateUniversalHash(wc, info, resultProgress);
#endif

                    // Ensure that the hash is found
                    allFound = singleFound;

                    // If we found a track, only keep track of distinct found tracks
                    if (singleFound && foundIds != null)
                    {
                        fullyMatchedIDs = foundIds;
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

                resultProgress?.Report(Result.Success("Match finding complete! " + (fullyMatchedIDs != null && fullyMatchedIDs.Count > 0
                    ? "Fully Matched IDs: " + string.Join(",", fullyMatchedIDs)
                    : "No matches found")));

                // Exit early if one failed or there are no matched IDs
                if (!allFound || fullyMatchedIDs == null || fullyMatchedIDs.Count == 0)
                    return false;

                // Find the first matched ID where the track count matches, we can grab a bunch of info from it
                int totalMatchedIDsCount = fullyMatchedIDs.Count;
                for (int i = 0; i < totalMatchedIDsCount; i++)
                {
                    // Skip if the track count doesn't match
#if NET48
                    if (!ValidateTrackCount(wc, fullyMatchedIDs[i], trackCount))
                        continue;
#else
                    if (!await ValidateTrackCount(wc, fullyMatchedIDs[i], trackCount))
                        continue;
#endif

                    // Fill in the fields from the existing ID
                    resultProgress?.Report(Result.Success($"Filling fields from existing ID {fullyMatchedIDs[i]}..."));
#if NET48
                    FillFromId(wc, info, fullyMatchedIDs[i], options.PullAllInformation);
#else
                    _ = await FillFromId(wc, info, fullyMatchedIDs[i], options.PullAllInformation);
#endif
                    resultProgress?.Report(Result.Success("Information filling complete!"));

                    // Set the fully matched ID to the current
                    info.FullyMatchedID = fullyMatchedIDs[i];
                    break;
                }

                // Clear out fully matched IDs from the partial list
                if (info.FullyMatchedID.HasValue)
                {
                    if (info.PartiallyMatchedIDs.Count == 1)
                        info.PartiallyMatchedIDs = null;
                    else
                        info.PartiallyMatchedIDs.Remove(info.FullyMatchedID.Value);
                }
            }

            return true;
        }

        /// <summary>
        /// Inject information from a seed SubmissionInfo into the existing one
        /// </summary>
        /// <param name="info">Existing submission information</param>
        /// <param name="seed">User-supplied submission information</param>
#if NET48
        public static void InjectSubmissionInformation(SubmissionInfo info, SubmissionInfo seed)
#else
        public static void InjectSubmissionInformation(SubmissionInfo? info, SubmissionInfo? seed)
#endif
        {
            // If we have any invalid info
            if (seed == null)
                return;

            // Ensure that required sections exist
            info = SubmissionInfoTool.EnsureAllSections(info);

            // Otherwise, inject information as necessary
            if (info.CommonDiscInfo != null && seed.CommonDiscInfo != null)
            {
                // Info that only overwrites if supplied
                if (!string.IsNullOrWhiteSpace(seed.CommonDiscInfo.Title)) info.CommonDiscInfo.Title = seed.CommonDiscInfo.Title;
                if (!string.IsNullOrWhiteSpace(seed.CommonDiscInfo.ForeignTitleNonLatin)) info.CommonDiscInfo.ForeignTitleNonLatin = seed.CommonDiscInfo.ForeignTitleNonLatin;
                if (!string.IsNullOrWhiteSpace(seed.CommonDiscInfo.DiscNumberLetter)) info.CommonDiscInfo.DiscNumberLetter = seed.CommonDiscInfo.DiscNumberLetter;
                if (!string.IsNullOrWhiteSpace(seed.CommonDiscInfo.DiscTitle)) info.CommonDiscInfo.DiscTitle = seed.CommonDiscInfo.DiscTitle;
                if (seed.CommonDiscInfo.Category != null) info.CommonDiscInfo.Category = seed.CommonDiscInfo.Category;
                if (seed.CommonDiscInfo.Region != null) info.CommonDiscInfo.Region = seed.CommonDiscInfo.Region;
                if (seed.CommonDiscInfo.Languages != null) info.CommonDiscInfo.Languages = seed.CommonDiscInfo.Languages;
                if (seed.CommonDiscInfo.LanguageSelection != null) info.CommonDiscInfo.LanguageSelection = seed.CommonDiscInfo.LanguageSelection;
                if (!string.IsNullOrWhiteSpace(seed.CommonDiscInfo.Serial)) info.CommonDiscInfo.Serial = seed.CommonDiscInfo.Serial;
                if (!string.IsNullOrWhiteSpace(seed.CommonDiscInfo.Barcode)) info.CommonDiscInfo.Barcode = seed.CommonDiscInfo.Barcode;
                if (!string.IsNullOrWhiteSpace(seed.CommonDiscInfo.Comments)) info.CommonDiscInfo.Comments = seed.CommonDiscInfo.Comments;
                if (seed.CommonDiscInfo.CommentsSpecialFields != null) info.CommonDiscInfo.CommentsSpecialFields = seed.CommonDiscInfo.CommentsSpecialFields;
                if (!string.IsNullOrWhiteSpace(seed.CommonDiscInfo.Contents)) info.CommonDiscInfo.Contents = seed.CommonDiscInfo.Contents;
                if (seed.CommonDiscInfo.ContentsSpecialFields != null) info.CommonDiscInfo.ContentsSpecialFields = seed.CommonDiscInfo.ContentsSpecialFields;

                // Info that always overwrites
                info.CommonDiscInfo.Layer0MasteringRing = seed.CommonDiscInfo.Layer0MasteringRing;
                info.CommonDiscInfo.Layer0MasteringSID = seed.CommonDiscInfo.Layer0MasteringSID;
                info.CommonDiscInfo.Layer0ToolstampMasteringCode = seed.CommonDiscInfo.Layer0ToolstampMasteringCode;
                info.CommonDiscInfo.Layer0MouldSID = seed.CommonDiscInfo.Layer0MouldSID;
                info.CommonDiscInfo.Layer0AdditionalMould = seed.CommonDiscInfo.Layer0AdditionalMould;

                info.CommonDiscInfo.Layer1MasteringRing = seed.CommonDiscInfo.Layer1MasteringRing;
                info.CommonDiscInfo.Layer1MasteringSID = seed.CommonDiscInfo.Layer1MasteringSID;
                info.CommonDiscInfo.Layer1ToolstampMasteringCode = seed.CommonDiscInfo.Layer1ToolstampMasteringCode;
                info.CommonDiscInfo.Layer1MouldSID = seed.CommonDiscInfo.Layer1MouldSID;
                info.CommonDiscInfo.Layer1AdditionalMould = seed.CommonDiscInfo.Layer1AdditionalMould;

                info.CommonDiscInfo.Layer2MasteringRing = seed.CommonDiscInfo.Layer2MasteringRing;
                info.CommonDiscInfo.Layer2MasteringSID = seed.CommonDiscInfo.Layer2MasteringSID;
                info.CommonDiscInfo.Layer2ToolstampMasteringCode = seed.CommonDiscInfo.Layer2ToolstampMasteringCode;

                info.CommonDiscInfo.Layer3MasteringRing = seed.CommonDiscInfo.Layer3MasteringRing;
                info.CommonDiscInfo.Layer3MasteringSID = seed.CommonDiscInfo.Layer3MasteringSID;
                info.CommonDiscInfo.Layer3ToolstampMasteringCode = seed.CommonDiscInfo.Layer3ToolstampMasteringCode;
            }

            if (info.VersionAndEditions != null && seed.VersionAndEditions != null)
            {
                // Info that only overwrites if supplied
                if (!string.IsNullOrWhiteSpace(seed.VersionAndEditions.Version)) info.VersionAndEditions.Version = seed.VersionAndEditions.Version;
                if (!string.IsNullOrWhiteSpace(seed.VersionAndEditions.OtherEditions)) info.VersionAndEditions.OtherEditions = seed.VersionAndEditions.OtherEditions;
            }

            if (info.CopyProtection != null && seed.CopyProtection != null)
            {
                // Info that only overwrites if supplied
                if (!string.IsNullOrWhiteSpace(seed.CopyProtection.Protection)) info.CopyProtection.Protection = seed.CopyProtection.Protection;
            }
        }

        #endregion

        #region Helpers

        /// <summary>
        /// Check if a site code is multi-line or not
        /// </summary>
        /// <param name="siteCode">SiteCode to check</param>
        /// <returns>True if the code field is multiline by default, false otherwise</returns>
        /// <remarks>TODO: This should move to Extensions at some point</remarks>
        public static bool IsMultiLine(SiteCode? siteCode)
        {
#if NET48
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
#else
            return siteCode switch
            {
                SiteCode.Extras => true,
                SiteCode.Filename => true,
                SiteCode.Games => true,
                SiteCode.GameFootage => true,
                SiteCode.Multisession => true,
                SiteCode.NetYarozeGames => true,
                SiteCode.Patches => true,
                SiteCode.PlayableDemos => true,
                SiteCode.RollingDemos => true,
                SiteCode.Savegames => true,
                SiteCode.TechDemos => true,
                SiteCode.Videos => true,
                _ => false,
            };
#endif
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
                var longname = siteCode.LongName();
                if (!string.IsNullOrEmpty(longname))
                    text = text.Replace(longname, siteCode.ShortName());
            }

            // For some outdated tags, we need to use alternate names
            text = text.Replace("<b>Demos</b>:", ((SiteCode?)SiteCode.PlayableDemos).ShortName());
            text = text.Replace("DMI:", ((SiteCode?)SiteCode.DMIHash).ShortName());
            text = text.Replace("<b>LucasArts ID</b>:", ((SiteCode?)SiteCode.LucasArtsID).ShortName());
            text = text.Replace("PFI:", ((SiteCode?)SiteCode.PFIHash).ShortName());
            text = text.Replace("SS:", ((SiteCode?)SiteCode.SSHash).ShortName());
            text = text.Replace("SSv1:", ((SiteCode?)SiteCode.SSHash).ShortName());
            text = text.Replace("<b>SSv1</b>:", ((SiteCode?)SiteCode.SSHash).ShortName());
            text = text.Replace("SSv2:", ((SiteCode?)SiteCode.SSHash).ShortName());
            text = text.Replace("<b>SSv2</b>:", ((SiteCode?)SiteCode.SSHash).ShortName());
            text = text.Replace("SS version:", ((SiteCode?)SiteCode.SSVersion).ShortName());
            text = text.Replace("Universal Hash (SHA-1):", ((SiteCode?)SiteCode.UniversalHash).ShortName());
            text = text.Replace("XeMID:", ((SiteCode?)SiteCode.XeMID).ShortName());
            text = text.Replace("XMID:", ((SiteCode?)SiteCode.XMID).ShortName());

            return text;
        }

        /// <summary>
        /// List the disc IDs associated with a given quicksearch query
        /// </summary>
        /// <param name="wc">RedumpWebClient for making the connection</param>
        /// <param name="query">Query string to attempt to search for</param>
        /// <param name="filterForwardSlashes">True to filter forward slashes, false otherwise</param>
        /// <returns>All disc IDs for the given query, null on error</returns>
#if NET48
        private static List<int> ListSearchResults(RedumpWebClient wc, string query, bool filterForwardSlashes = true)
#else
        private async static Task<List<int>?> ListSearchResults(RedumpHttpClient wc, string? query, bool filterForwardSlashes = true)
#endif
        {
            // If there is an invalid query
            if (string.IsNullOrWhiteSpace(query))
                return null;

            var ids = new List<int>();

            // Strip quotes
            query = query.Trim('"', '\'');

            // Special characters become dashes
            query = query.Replace(' ', '-');
            if (filterForwardSlashes)
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
#if NET48
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
#if NET48
        private static (bool, List<int>) ValidateSingleTrack(RedumpWebClient wc, SubmissionInfo info, string hashData, IProgress<Result> resultProgress = null)
#else
        private async static Task<(bool, List<int>?)> ValidateSingleTrack(RedumpHttpClient wc, SubmissionInfo info, string hashData, IProgress<Result>? resultProgress = null)
#endif
        {
            // If the line isn't parseable, we can't validate
            if (!InfoTool.GetISOHashValues(hashData, out long _, out var _, out var _, out var sha1))
            {
                resultProgress?.Report(Result.Failure("Line could not be parsed for hash data"));
                return (false, null);
            }

            // Get all matching IDs for the track
#if NET48
            List<int> newIds = ListSearchResults(wc, sha1);
#else
            var newIds = await ListSearchResults(wc, sha1);
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
            if (info.PartiallyMatchedIDs != null && info.PartiallyMatchedIDs.Any())
                info.PartiallyMatchedIDs.AddRange(newIds);
            else
                info.PartiallyMatchedIDs = newIds;

            return (true, newIds);
        }

        /// <summary>
        /// Validate a universal hash against Redump, if possible
        /// </summary>
        /// <param name="wc">RedumpWebClient for making the connection</param>
        /// <param name="info">Existing SubmissionInfo object to fill</param>
        /// <param name="resultProgress">Optional result progress callback</param>
        /// <returns>True if the track was found, false otherwise; List of found values, if possible</returns>
#if NET48
        private static (bool, List<int>) ValidateUniversalHash(RedumpWebClient wc, SubmissionInfo info, IProgress<Result> resultProgress = null)
#else
        private async static Task<(bool, List<int>?)> ValidateUniversalHash(RedumpHttpClient wc, SubmissionInfo info, IProgress<Result>? resultProgress = null)
#endif
        {
            // If we don't have special fields
            if (info.CommonDiscInfo?.CommentsSpecialFields == null)
            {
                resultProgress?.Report(Result.Failure("Universal hash was missing"));
                return (false, null);
            }

            // If we don't have a universal hash
            var universalHash = info.CommonDiscInfo.CommentsSpecialFields[SiteCode.UniversalHash];
            if (string.IsNullOrEmpty(universalHash))
            {
                resultProgress?.Report(Result.Failure("Universal hash was missing"));
                return (false, null);
            }

            // Format the universal hash for finding within the comments
#if NET48
            universalHash = $"{universalHash.Substring(0, universalHash.Length - 1)}/comments/only";
#else
            universalHash = $"{universalHash[..^1]}/comments/only";
#endif

            // Get all matching IDs for the hash
#if NET48
            List<int> newIds = ListSearchResults(wc, universalHash, filterForwardSlashes: false);
#else
            var newIds = await ListSearchResults(wc, universalHash, filterForwardSlashes: false);
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
            if (info.PartiallyMatchedIDs != null && info.PartiallyMatchedIDs.Any())
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
#if NET48
        private static bool ValidateTrackCount(RedumpWebClient wc, int id, int localCount)
#else
        private async static Task<bool> ValidateTrackCount(RedumpHttpClient wc, int id, int localCount)
#endif
        {
            // If we can't pull the remote data, we can't match
#if NET48
            string discData = wc.DownloadSingleSiteID(id);
#else
            string? discData = await wc.DownloadSingleSiteID(id);
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
    }
}
