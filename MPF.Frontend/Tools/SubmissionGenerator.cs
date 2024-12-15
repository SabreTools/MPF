﻿using System;
using System.Collections.Generic;
using System.IO;
#if NET35_OR_GREATER || NETCOREAPP
using System.Linq;
#endif
using System.Text;
using System.Threading.Tasks;
using BinaryObjectScanner;
using MPF.Processors;
using SabreTools.RedumpLib;
using SabreTools.RedumpLib.Data;
using SabreTools.RedumpLib.Web;

namespace MPF.Frontend.Tools
{
    /// <summary>
    /// Generator for SubmissionInfo objects
    /// </summary>
    internal static class SubmissionGenerator
    {
        #region Constants

        private const string RequiredValue = "(REQUIRED)";
        private const string RequiredIfExistsValue = "(REQUIRED, IF EXISTS)";
        private const string OptionalValue = "(OPTIONAL)";

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
        /// <param name="processor">Processor object representing how to process the outputs</param>
        /// <param name="resultProgress">Optional result progress callback</param>
        /// <param name="protectionProgress">Optional protection progress callback</param>
        /// <returns>SubmissionInfo populated based on outputs, null on error</returns>
        public static async Task<SubmissionInfo?> ExtractOutputInformation(
            string outputPath,
            Drive? drive,
            RedumpSystem? system,
            MediaType? mediaType,
            Options options,
            BaseProcessor processor,
            IProgress<ResultEventArgs>? resultProgress = null,
            IProgress<ProtectionProgress>? protectionProgress = null)
        {
            // Ensure the current disc combination should exist
            if (!system.MediaTypes().Contains(mediaType))
                return null;

            // Split the output path for easier use
            var outputDirectory = Path.GetDirectoryName(outputPath);
            string outputFilename = Path.GetFileName(outputPath);

            // Check that all of the relevant files are there
            List<string> missingFiles = processor.FoundAllFiles(outputDirectory, outputFilename);
            if (missingFiles.Count > 0)
            {
                resultProgress?.Report(ResultEventArgs.Failure($"There were files missing from the output:\n{string.Join("\n", [.. missingFiles])}"));
                resultProgress?.Report(ResultEventArgs.Failure($"This may indicate an issue with the hardware or media, including unsupported devices.\nPlease see dumping program documentation for more details."));
                return null;
            }

            // Sanitize the output filename to strip off any potential extension
            outputFilename = Path.GetFileNameWithoutExtension(outputFilename);

            // Create the SubmissionInfo object with all user-inputted values by default
            string combinedBase;
            if (string.IsNullOrEmpty(outputDirectory))
                combinedBase = outputFilename;
            else
                combinedBase = Path.Combine(outputDirectory, outputFilename);

            // Create the default submission info
            SubmissionInfo info = CreateDefaultSubmissionInfo(processor, system, mediaType, options.AddPlaceholders);

            // Get specific tool output handling
            processor?.GenerateSubmissionInfo(info, combinedBase, options.EnableRedumpCompatibility);
            if (options.IncludeArtifacts)
                info.Artifacts = processor?.GenerateArtifacts(combinedBase);

            // Get a list of matching IDs for each line in the DAT
            if (!string.IsNullOrEmpty(info.TracksAndWriteOffsets!.ClrMameProData) && options.HasRedumpLogin)
                _ = await FillFromRedump(options, info, resultProgress);

            // If we have both ClrMamePro and Size and Checksums data, remove the ClrMamePro
            if (!string.IsNullOrEmpty(info.SizeAndChecksums?.CRC32))
                info.TracksAndWriteOffsets.ClrMameProData = null;

            // Add the volume label to comments, if possible or necessary
            string? volLabels = FormatVolumeLabels(drive?.VolumeLabel, processor?.VolumeLabels);
            if (volLabels != null)
                info.CommonDiscInfo!.CommentsSpecialFields![SiteCode.VolumeLabel] = volLabels;

            // Extract info based generically on MediaType
            ProcessMediaType(info, mediaType, options.AddPlaceholders);

            // Extract info based specifically on RedumpSystem
            ProcessSystem(info, system, drive, options.AddPlaceholders, processor is DiscImageCreator, combinedBase);

            // Run anti-modchip check, if necessary
            if (drive != null && system.SupportsAntiModchipScans() && info.CopyProtection!.AntiModchip == YesNo.NULL)
            {
                resultProgress?.Report(ResultEventArgs.Success("Checking for anti-modchip strings... this might take a while!"));
                info.CopyProtection.AntiModchip = await ProtectionTool.GetPlayStationAntiModchipDetected(drive?.Name) ? YesNo.Yes : YesNo.No;
                resultProgress?.Report(ResultEventArgs.Success("Anti-modchip string scan complete!"));
            }

            // Run copy protection, if possible or necessary
            if (system.SupportsCopyProtectionScans())
            {
                resultProgress?.Report(ResultEventArgs.Success("Running copy protection scan... this might take a while!"));

                ProtectionDictionary? protections = null;
                try
                {
                    if (options.ScanForProtection && drive?.Name != null)
                        protections = await ProtectionTool.RunProtectionScanOnPath(drive.Name, options, protectionProgress);

                    var protectionString = ProtectionTool.FormatProtections(protections);

                    info.CopyProtection!.Protection += protectionString;
                    info.CopyProtection.FullProtections = ReformatProtectionDictionary(protections);
                    resultProgress?.Report(ResultEventArgs.Success("Copy protection scan complete!"));
                }
                catch (Exception ex)
                {
                    resultProgress?.Report(ResultEventArgs.Failure(ex.ToString()));
                }
            }

            // Set fields that may have automatic filling otherwise
            info.CommonDiscInfo!.Category ??= DiscCategory.Games;
            info.VersionAndEditions!.Version ??= options.AddPlaceholders ? RequiredIfExistsValue : string.Empty;

            // Comments and contents have odd handling
            if (string.IsNullOrEmpty(info.CommonDiscInfo.Comments))
                info.CommonDiscInfo.Comments = options.AddPlaceholders ? OptionalValue : string.Empty;
            if (string.IsNullOrEmpty(info.CommonDiscInfo.Contents))
                info.CommonDiscInfo.Contents = options.AddPlaceholders ? OptionalValue : string.Empty;

            // Normalize the disc type with all current information
            Validator.NormalizeDiscType(info);

            return info;
        }

        /// <summary>
        /// Fill in a SubmissionInfo object from Redump, if possible
        /// </summary>
        /// <param name="options">Options object representing user-defined options</param>
        /// <param name="info">Existing SubmissionInfo object to fill</param>
        /// <param name="resultProgress">Optional result progress callback</param>
        public async static Task<bool> FillFromRedump(Options options,
            SubmissionInfo info,
            IProgress<ResultEventArgs>? resultProgress = null)
        {
            // If no username is provided
            if (string.IsNullOrEmpty(options.RedumpUsername) || string.IsNullOrEmpty(options.RedumpPassword))
                return false;

            // Set the current dumper based on username
            info.DumpersAndStatus ??= new DumpersAndStatusSection();
            info.DumpersAndStatus.Dumpers = [options.RedumpUsername!];
            info.PartiallyMatchedIDs = [];

            // Login to Redump
            var wc = new RedumpClient();
            bool? loggedIn = await wc.Login(options.RedumpUsername ?? string.Empty, options.RedumpPassword ?? string.Empty);
            if (loggedIn == null)
            {
                resultProgress?.Report(ResultEventArgs.Failure("There was an unknown error connecting to Redump"));
                return false;
            }
            else if (loggedIn == false)
            {
                // Don't log the as a failure or error
                return false;
            }

            // Setup the checks
            bool allFound = true;
            List<int[]> foundIdSets = [];

            // Loop through all of the hashdata to find matching IDs
            resultProgress?.Report(ResultEventArgs.Success("Finding disc matches on Redump..."));
            var splitData = info.TracksAndWriteOffsets?.ClrMameProData?.TrimEnd('\n')?.Split('\n');
            int trackCount = splitData?.Length ?? 0;
            foreach (string hashData in splitData ?? [])
            {
                // Catch any errant blank lines
                if (string.IsNullOrEmpty(hashData))
                {
                    trackCount--;
                    resultProgress?.Report(ResultEventArgs.Success("Blank line found, skipping!"));
                    continue;
                }

                // If the line ends in a known extra track names, skip them for checking
                if (hashData.Contains("(Track 0).bin")
                    || hashData.Contains("(Track 0.2).bin")
                    || hashData.Contains("(Track 00).bin")
                    || hashData.Contains("(Track 00.2).bin")
                    || hashData.Contains("(Track A).bin")
                    || hashData.Contains("(Track A.2).bin")
                    || hashData.Contains("(Track AA).bin")
                    || hashData.Contains("(Track AA.2).bin"))
                {
                    trackCount--;
                    resultProgress?.Report(ResultEventArgs.Success("Extra track found, skipping!"));
                    continue;
                }

                // Get the SHA-1 hash
                if (!ProcessingTool.GetISOHashValues(hashData, out _, out _, out _, out string? sha1))
                {
                    resultProgress?.Report(ResultEventArgs.Failure($"Line could not be parsed: {hashData}"));
                    continue;
                }

                var foundIds = await Validator.ValidateSingleTrack(wc, info, sha1);
                if (foundIds != null && foundIds.Count == 1)
                    resultProgress?.Report(ResultEventArgs.Success($"Single match found for {sha1}"));
                else if (foundIds != null && foundIds.Count != 1)
                    resultProgress?.Report(ResultEventArgs.Success($"Multiple matches found for {sha1}"));
                else
                    resultProgress?.Report(ResultEventArgs.Failure($"No matches found for {sha1}"));

                // Add the found IDs to the map
                foundIdSets.Add(foundIds?.ToArray() ?? []);

                // Ensure that all tracks are found
                allFound &= (foundIds != null && foundIds.Count >= 1);
            }

            // If all tracks were found, check if there are any fully-matched IDs
            HashSet<int>? fullyMatchedIdsSet = null;
            if (allFound)
            {
                fullyMatchedIdsSet = null;
                foreach (var set in foundIdSets)
                {
                    // First track is always all IDs
                    if (fullyMatchedIdsSet == null)
                    {
                        fullyMatchedIdsSet = [.. set];
                        continue;
                    }

                    // Try to intersect with all known IDs
                    fullyMatchedIdsSet.IntersectWith(set);
                    if (fullyMatchedIdsSet.Count == 0)
                        break;
                }
            }

            // If we don't have any matches but we have a universal hash
            if (info.PartiallyMatchedIDs.Count == 0 && info.CommonDiscInfo?.CommentsSpecialFields?.ContainsKey(SiteCode.UniversalHash) == true)
            {
                string sha1 = info.CommonDiscInfo.CommentsSpecialFields[SiteCode.UniversalHash];
                var foundIds = await Validator.ValidateUniversalHash(wc, info);
                if (foundIds != null && foundIds.Count == 1)
                    resultProgress?.Report(ResultEventArgs.Success($"Single match found for universal hash {sha1}"));
                else if (foundIds != null && foundIds.Count != 1)
                    resultProgress?.Report(ResultEventArgs.Success($"Multiple matches found for universal hash {sha1}"));
                else
                    resultProgress?.Report(ResultEventArgs.Failure($"No matches found for universal hash {sha1}"));

                // Ensure that the hash is found
                allFound = (foundIds != null && foundIds.Count == 1);

                // If we found a match, then the disc is a match
                if (foundIds != null && foundIds.Count == 1)
                    fullyMatchedIdsSet = [.. foundIds];
                else
                    fullyMatchedIdsSet = [];
            }

            // Get a list version of the fully matched IDs
            List<int> fullyMatchedIdsList = fullyMatchedIdsSet != null ? [.. fullyMatchedIdsSet] : [];

            // Make sure we only have unique IDs
            var partiallyMatchedIds = new HashSet<int>();
            partiallyMatchedIds.IntersectWith(info.PartiallyMatchedIDs);
            info.PartiallyMatchedIDs = [.. partiallyMatchedIds];
            info.PartiallyMatchedIDs.Sort();

            resultProgress?.Report(ResultEventArgs.Success("Match finding complete! " + (fullyMatchedIdsList != null && fullyMatchedIdsList.Count > 0
                ? "Fully Matched IDs: " + string.Join(",", [.. fullyMatchedIdsList.ConvertAll(i => i.ToString())])
                : "No matches found")));

            // Exit early if one failed or there are no matched IDs
            if (!allFound || fullyMatchedIdsList == null || fullyMatchedIdsList.Count == 0)
                return false;

            // Find the first matched ID where the track count matches, we can grab a bunch of info from it
            int totalMatchedIDsCount = fullyMatchedIdsList.Count;
            for (int i = 0; i < totalMatchedIDsCount; i++)
            {
                // Skip if the track count doesn't match
                if (!await Validator.ValidateTrackCount(wc, fullyMatchedIdsList[i], trackCount))
                    continue;

                // Fill in the fields from the existing ID
                resultProgress?.Report(ResultEventArgs.Success($"Filling fields from existing ID {fullyMatchedIdsList[i]}..."));
                _ = await Builder.FillFromId(wc, info, fullyMatchedIdsList[i], options.PullAllInformation);
                resultProgress?.Report(ResultEventArgs.Success("Information filling complete!"));

                // Set the fully matched ID to the current
                info.FullyMatchedID = fullyMatchedIdsList[i];
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

            return true;
        }

        #endregion

        #region Helper Functions

        /// <summary>
        /// Creates a default SubmissionInfo object based on the current system and media type
        /// </summary>
        private static SubmissionInfo CreateDefaultSubmissionInfo(BaseProcessor processor, RedumpSystem? system, MediaType? mediaType, bool addPlaceholders)
        {
            // Create the template object
            var info = new SubmissionInfo()
            {
                CommonDiscInfo = new CommonDiscInfoSection()
                {
                    System = system,
                    Media = mediaType.ToDiscType(),
                    Title = addPlaceholders ? RequiredValue : string.Empty,
                    ForeignTitleNonLatin = addPlaceholders ? OptionalValue : string.Empty,
                    DiscNumberLetter = addPlaceholders ? OptionalValue : string.Empty,
                    DiscTitle = addPlaceholders ? OptionalValue : string.Empty,
                    Category = null,
                    Region = null,
                    Languages = null,
                    Serial = addPlaceholders ? RequiredIfExistsValue : string.Empty,
                    Barcode = addPlaceholders ? OptionalValue : string.Empty,
                    Contents = string.Empty,
                },
                VersionAndEditions = new VersionAndEditionsSection()
                {
                    OtherEditions = addPlaceholders ? "(VERIFY THIS) Original" : string.Empty,
                },
                DumpingInfo = new DumpingInfoSection()
                {
                    FrontendVersion = FrontendTool.GetCurrentVersion(),
                    DumpingProgram = GetDumpingProgramFromProcessor(processor),
                },
            };

            // Ensure that required sections exist
            info = Builder.EnsureAllSections(info);
            return info;
        }

        /// <summary>
        /// Get the dumping program name from the processor
        /// </summary>
        /// <param name="processor"></param>
        /// <returns></returns>
        private static string? GetDumpingProgramFromProcessor(BaseProcessor processor)
        {
            // Map to the internal program
            InternalProgram? internalProgram = processor switch
            {
                Processors.Aaru => InternalProgram.Aaru,
                CleanRip => InternalProgram.CleanRip,
                DiscImageCreator => InternalProgram.DiscImageCreator,
                PS3CFW => InternalProgram.PS3CFW,
                Redumper => InternalProgram.Redumper,
                UmdImageCreator => InternalProgram.UmdImageCreator,
                XboxBackupCreator => InternalProgram.XboxBackupCreator,
                _ => null,
            };

            // Use the internal program to map to the name
            return internalProgram.LongName();
        }

        /// <summary>
        /// Simplifies a volume label into uppercase alphanumeric only string
        /// </summary>
        /// <param name="labels">Volume label to simplify</param>
        /// <returns>Simplified volume label</returns>
        private static string? SimplifyVolumeLabel(string? label)
        {
            if (label == null || label.Length == 0)
                return null;
            
            var labelBuilder = new StringBuilder();
            foreach (char c in label)
            {
                if ((c >= 'A' && c <= 'Z') || (c >= 'a' && c <= 'z') || (c >= '0' && c <= '9'))
                    labelBuilder.Append(char.ToUpper(c));
            }
            string? simpleLabel = labelBuilder.ToString();

            if (simpleLabel == null || simpleLabel.Length == 0)
                return null;
            else
                return simpleLabel;
        }

        /// <summary>
        /// Formats a list of volume labels and their corresponding filesystems
        /// </summary>
        /// <param name="labels">Dictionary of volume labels and their filesystems</param>
        /// <returns>Formatted string of volume labels and their filesystems</returns>
        private static string? FormatVolumeLabels(string? driveLabel, Dictionary<string, List<string>>? labels)
        {
            // Treat empty label as null
            if (driveLabel != null && driveLabel.Length == 0)
                driveLabel = null;
            
            // Must have at least one label to format
            if (driveLabel == null && (labels == null || labels.Count == 0))
                return null;

            // If no labels given, use drive label
            if (labels == null || labels.Count == 0)
            {
                // Ignore common volume labels
                if (FrontendTool.GetRedumpSystemFromVolumeLabel(driveLabel) != null)
                    return null;

                return driveLabel;
            }

            // Get the default label to compare against
            // TODO: Full pairwise comparison of all labels, not just comparing against drive/UDF label.
            string? defaultLabel = null;
            if (driveLabel != null && driveLabel.Length != 0)
                defaultLabel = SimplifyVolumeLabel(driveLabel);
#if NET35_OR_GREATER || NETCOREAPP
            else
                defaultLabel = labels.Where(label => label.Value.Contains("UDF")).Select(label => label.Key).FirstOrDefault();
#endif

            // Remove duplicate/useless volume labels
            if (defaultLabel != null && defaultLabel.Length != 0)
            {
                List<string> keysToRemove = new List<string>();
                foreach (KeyValuePair<string, List<string>> label in labels)
                {
                    string? tempLabel = SimplifyVolumeLabel(label.Key);
                    // Remove duplicate volume labels and remove "DVD_ROM" / "CD_ROM" labels
                    if (defaultLabel == tempLabel || label.Key == "DVD_ROM" || label.Key == "CD_ROM")
                        keysToRemove.Add(label.Key);
                }
                foreach (string key in keysToRemove)
                    labels.Remove(key);
            }

            // If only one unique label left, don't mention fs
#if NET20
            string[] keyArr = new string[labels.Count];
            labels.Keys.CopyTo(keyArr, 0);
            string firstLabel = keyArr[0];
#else
            string firstLabel = labels.First().Key;
#endif
            if (labels.Count == 1 && (firstLabel == driveLabel || driveLabel == null))
            {
                // Ignore common volume labels
                if (FrontendTool.GetRedumpSystemFromVolumeLabel(firstLabel) != null)
                    return null;

                return firstLabel;
            }

            // Otherwise, state filesystem for each label
            List<string> volLabels = [];

            // Begin formatted output with the label from Windows, if it is unique and not a common volume label
            if (driveLabel != null && !labels.TryGetValue(driveLabel, out List<string>? value) && FrontendTool.GetRedumpSystemFromVolumeLabel(driveLabel) == null)
                volLabels.Add(driveLabel);

            // Add remaining labels with their corresponding filesystems
            foreach (KeyValuePair<string, List<string>> label in labels)
            {
                // Ignore common volume labels
                if (FrontendTool.GetRedumpSystemFromVolumeLabel(label.Key) == null)
                    volLabels.Add($"{label.Key} ({string.Join(", ", [.. label.Value])})");
            }

            // Ensure that no labels are empty
            volLabels = volLabels.FindAll(label => !string.IsNullOrEmpty(label?.Trim()));

            // Print each label separated by a comma and a space
            if (volLabels.Count == 0)
                return null;

            return string.Join(", ", [.. volLabels]);
        }

        /// <summary>
        /// Processes default data based on media type
        /// </summary>
        private static bool ProcessMediaType(SubmissionInfo info, MediaType? mediaType, bool addPlaceholders)
        {
            switch (mediaType)
            {
                case MediaType.CDROM:
                case MediaType.GDROM:
                    info.CommonDiscInfo!.Layer0MasteringRing = addPlaceholders ? RequiredIfExistsValue : string.Empty;
                    info.CommonDiscInfo.Layer0MasteringSID = addPlaceholders ? RequiredIfExistsValue : string.Empty;
                    info.CommonDiscInfo.Layer0ToolstampMasteringCode = addPlaceholders ? RequiredIfExistsValue : string.Empty;
                    info.CommonDiscInfo.Layer0MouldSID = addPlaceholders ? RequiredIfExistsValue : string.Empty;
                    info.CommonDiscInfo.Layer1MouldSID = addPlaceholders ? RequiredIfExistsValue : string.Empty;
                    info.CommonDiscInfo.Layer0AdditionalMould = addPlaceholders ? RequiredIfExistsValue : string.Empty;
                    break;

                case MediaType.DVD:
                case MediaType.HDDVD:
                case MediaType.BluRay:

                    // If we have a single-layer disc
                    if (info.SizeAndChecksums!.Layerbreak == default)
                    {
                        info.CommonDiscInfo!.Layer0MasteringRing = addPlaceholders ? RequiredIfExistsValue : string.Empty;
                        info.CommonDiscInfo.Layer0MasteringSID = addPlaceholders ? RequiredIfExistsValue : string.Empty;
                        info.CommonDiscInfo.Layer0ToolstampMasteringCode = addPlaceholders ? RequiredIfExistsValue : string.Empty;
                        info.CommonDiscInfo.Layer0MouldSID = addPlaceholders ? RequiredIfExistsValue : string.Empty;
                        info.CommonDiscInfo.Layer1MouldSID = addPlaceholders ? RequiredIfExistsValue : string.Empty;
                        info.CommonDiscInfo.Layer0AdditionalMould = addPlaceholders ? RequiredIfExistsValue : string.Empty;
                    }
                    // If we have a dual-layer disc
                    else if (info.SizeAndChecksums!.Layerbreak2 == default)
                    {
                        info.CommonDiscInfo!.Layer0MasteringRing = addPlaceholders ? RequiredIfExistsValue : string.Empty;
                        info.CommonDiscInfo.Layer0MasteringSID = addPlaceholders ? RequiredIfExistsValue : string.Empty;
                        info.CommonDiscInfo.Layer0ToolstampMasteringCode = addPlaceholders ? RequiredIfExistsValue : string.Empty;
                        info.CommonDiscInfo.Layer0MouldSID = addPlaceholders ? RequiredIfExistsValue : string.Empty;
                        info.CommonDiscInfo.Layer0AdditionalMould = addPlaceholders ? RequiredIfExistsValue : string.Empty;

                        info.CommonDiscInfo.Layer1MasteringRing = addPlaceholders ? RequiredIfExistsValue : string.Empty;
                        info.CommonDiscInfo.Layer1MasteringSID = addPlaceholders ? RequiredIfExistsValue : string.Empty;
                        info.CommonDiscInfo.Layer1ToolstampMasteringCode = addPlaceholders ? RequiredIfExistsValue : string.Empty;
                        info.CommonDiscInfo.Layer1MouldSID = addPlaceholders ? RequiredIfExistsValue : string.Empty;
                    }
                    // If we have a triple-layer disc
                    else if (info.SizeAndChecksums!.Layerbreak3 == default)
                    {
                        info.CommonDiscInfo!.Layer0MasteringRing = addPlaceholders ? RequiredIfExistsValue : string.Empty;
                        info.CommonDiscInfo.Layer0MasteringSID = addPlaceholders ? RequiredIfExistsValue : string.Empty;
                        info.CommonDiscInfo.Layer0ToolstampMasteringCode = addPlaceholders ? RequiredIfExistsValue : string.Empty;
                        info.CommonDiscInfo.Layer0MouldSID = addPlaceholders ? RequiredIfExistsValue : string.Empty;
                        info.CommonDiscInfo.Layer0AdditionalMould = addPlaceholders ? RequiredIfExistsValue : string.Empty;

                        info.CommonDiscInfo.Layer1MasteringRing = addPlaceholders ? RequiredIfExistsValue : string.Empty;
                        info.CommonDiscInfo.Layer1MasteringSID = addPlaceholders ? RequiredIfExistsValue : string.Empty;
                        info.CommonDiscInfo.Layer1ToolstampMasteringCode = addPlaceholders ? RequiredIfExistsValue : string.Empty;
                        info.CommonDiscInfo.Layer1MouldSID = addPlaceholders ? RequiredIfExistsValue : string.Empty;

                        info.CommonDiscInfo.Layer2MasteringRing = addPlaceholders ? RequiredIfExistsValue : string.Empty;
                        info.CommonDiscInfo.Layer2MasteringSID = addPlaceholders ? RequiredIfExistsValue : string.Empty;
                        info.CommonDiscInfo.Layer2ToolstampMasteringCode = addPlaceholders ? RequiredIfExistsValue : string.Empty;
                    }
                    // If we have a quad-layer disc
                    else
                    {
                        info.CommonDiscInfo!.Layer0MasteringRing = addPlaceholders ? RequiredIfExistsValue : string.Empty;
                        info.CommonDiscInfo.Layer0MasteringSID = addPlaceholders ? RequiredIfExistsValue : string.Empty;
                        info.CommonDiscInfo.Layer0ToolstampMasteringCode = addPlaceholders ? RequiredIfExistsValue : string.Empty;
                        info.CommonDiscInfo.Layer0MouldSID = addPlaceholders ? RequiredIfExistsValue : string.Empty;
                        info.CommonDiscInfo.Layer0AdditionalMould = addPlaceholders ? RequiredIfExistsValue : string.Empty;

                        info.CommonDiscInfo.Layer1MasteringRing = addPlaceholders ? RequiredIfExistsValue : string.Empty;
                        info.CommonDiscInfo.Layer1MasteringSID = addPlaceholders ? RequiredIfExistsValue : string.Empty;
                        info.CommonDiscInfo.Layer1ToolstampMasteringCode = addPlaceholders ? RequiredIfExistsValue : string.Empty;
                        info.CommonDiscInfo.Layer1MouldSID = addPlaceholders ? RequiredIfExistsValue : string.Empty;

                        info.CommonDiscInfo.Layer2MasteringRing = addPlaceholders ? RequiredIfExistsValue : string.Empty;
                        info.CommonDiscInfo.Layer2MasteringSID = addPlaceholders ? RequiredIfExistsValue : string.Empty;
                        info.CommonDiscInfo.Layer2ToolstampMasteringCode = addPlaceholders ? RequiredIfExistsValue : string.Empty;

                        info.CommonDiscInfo.Layer3MasteringRing = addPlaceholders ? RequiredIfExistsValue : string.Empty;
                        info.CommonDiscInfo.Layer3MasteringSID = addPlaceholders ? RequiredIfExistsValue : string.Empty;
                        info.CommonDiscInfo.Layer3ToolstampMasteringCode = addPlaceholders ? RequiredIfExistsValue : string.Empty;
                    }

                    break;

                case MediaType.NintendoGameCubeGameDisc:
                    info.CommonDiscInfo!.Layer0MasteringRing = addPlaceholders ? RequiredIfExistsValue : string.Empty;
                    info.CommonDiscInfo.Layer0MasteringSID = addPlaceholders ? RequiredIfExistsValue : string.Empty;
                    info.CommonDiscInfo.Layer0ToolstampMasteringCode = addPlaceholders ? RequiredIfExistsValue : string.Empty;
                    info.CommonDiscInfo.Layer0MouldSID = addPlaceholders ? RequiredIfExistsValue : string.Empty;
                    info.CommonDiscInfo.Layer1MouldSID = addPlaceholders ? RequiredIfExistsValue : string.Empty;
                    info.CommonDiscInfo.Layer0AdditionalMould = addPlaceholders ? RequiredIfExistsValue : string.Empty;
                    info.Extras!.BCA ??= addPlaceholders ? RequiredValue : string.Empty;
                    break;

                case MediaType.NintendoWiiOpticalDisc:
                case MediaType.NintendoWiiUOpticalDisc:

                    // If we have a single-layer disc
                    if (info.SizeAndChecksums!.Layerbreak == default)
                    {
                        info.CommonDiscInfo!.Layer0MasteringRing = addPlaceholders ? RequiredIfExistsValue : string.Empty;
                        info.CommonDiscInfo.Layer0MasteringSID = addPlaceholders ? RequiredIfExistsValue : string.Empty;
                        info.CommonDiscInfo.Layer0ToolstampMasteringCode = addPlaceholders ? RequiredIfExistsValue : string.Empty;
                        info.CommonDiscInfo.Layer0MouldSID = addPlaceholders ? RequiredIfExistsValue : string.Empty;
                        info.CommonDiscInfo.Layer1MouldSID = addPlaceholders ? RequiredIfExistsValue : string.Empty;
                        info.CommonDiscInfo.Layer0AdditionalMould = addPlaceholders ? RequiredIfExistsValue : string.Empty;
                    }
                    // If we have a dual-layer disc
                    else
                    {
                        info.CommonDiscInfo!.Layer0MasteringRing = addPlaceholders ? RequiredIfExistsValue : string.Empty;
                        info.CommonDiscInfo.Layer0MasteringSID = addPlaceholders ? RequiredIfExistsValue : string.Empty;
                        info.CommonDiscInfo.Layer0ToolstampMasteringCode = addPlaceholders ? RequiredIfExistsValue : string.Empty;
                        info.CommonDiscInfo.Layer0MouldSID = addPlaceholders ? RequiredIfExistsValue : string.Empty;
                        info.CommonDiscInfo.Layer0AdditionalMould = addPlaceholders ? RequiredIfExistsValue : string.Empty;

                        info.CommonDiscInfo.Layer1MasteringRing = addPlaceholders ? RequiredIfExistsValue : string.Empty;
                        info.CommonDiscInfo.Layer1MasteringSID = addPlaceholders ? RequiredIfExistsValue : string.Empty;
                        info.CommonDiscInfo.Layer1ToolstampMasteringCode = addPlaceholders ? RequiredIfExistsValue : string.Empty;
                        info.CommonDiscInfo.Layer1MouldSID = addPlaceholders ? RequiredIfExistsValue : string.Empty;
                    }

                    info.Extras!.DiscKey = addPlaceholders ? RequiredValue : string.Empty;
                    info.Extras.BCA ??= addPlaceholders ? RequiredValue : string.Empty;

                    break;

                case MediaType.UMD:
                    // Both single- and dual-layer discs have two "layers" for the ring
                    info.CommonDiscInfo!.Layer0MasteringRing = addPlaceholders ? RequiredIfExistsValue : string.Empty;
                    info.CommonDiscInfo.Layer0MasteringSID = addPlaceholders ? RequiredIfExistsValue : string.Empty;
                    info.CommonDiscInfo.Layer0ToolstampMasteringCode = addPlaceholders ? RequiredIfExistsValue : string.Empty;
                    info.CommonDiscInfo.Layer0MouldSID = addPlaceholders ? RequiredIfExistsValue : string.Empty;

                    info.CommonDiscInfo.Layer1MasteringRing = addPlaceholders ? RequiredIfExistsValue : string.Empty;
                    info.CommonDiscInfo.Layer1MasteringSID = addPlaceholders ? RequiredIfExistsValue : string.Empty;
                    info.CommonDiscInfo.Layer1ToolstampMasteringCode = addPlaceholders ? RequiredIfExistsValue : string.Empty;

                    info.SizeAndChecksums!.CRC32 ??= (addPlaceholders ? RequiredValue + " [Not automatically generated for UMD]" : string.Empty);
                    info.SizeAndChecksums.MD5 ??= (addPlaceholders ? RequiredValue + " [Not automatically generated for UMD]" : string.Empty);
                    info.SizeAndChecksums.SHA1 ??= (addPlaceholders ? RequiredValue + " [Not automatically generated for UMD]" : string.Empty);
                    info.TracksAndWriteOffsets!.ClrMameProData = null;
                    break;
            }

            return true;
        }

        /// <summary>
        /// Processes default data based on system type
        /// </summary>
        private static bool ProcessSystem(SubmissionInfo info, RedumpSystem? system, Drive? drive, bool addPlaceholders, bool isDiscImageCreator, string basePath)
        {
            // Extract info based specifically on RedumpSystem
            switch (system)
            {
                case RedumpSystem.AcornArchimedes:
                    info.CommonDiscInfo!.Region ??= Region.UnitedKingdom;
                    break;

                case RedumpSystem.AudioCD:
                case RedumpSystem.DVDAudio:
                case RedumpSystem.EnhancedCD:
                case RedumpSystem.SuperAudioCD:
                    info.CommonDiscInfo!.Category ??= DiscCategory.Audio;
                    break;

                case RedumpSystem.BandaiPlaydiaQuickInteractiveSystem:
                    info.CommonDiscInfo!.EXEDateBuildDate ??= addPlaceholders ? RequiredValue : string.Empty;
                    info.CommonDiscInfo.Region ??= info.CommonDiscInfo.Region ?? Region.Japan;
                    break;

                case RedumpSystem.BDVideo:
                    info.CommonDiscInfo!.Category ??= DiscCategory.Video;
                    bool bee = PhysicalTool.GetBusEncryptionEnabled(drive);
                    if (bee && string.IsNullOrEmpty(info.CopyProtection!.Protection))
                        info.CopyProtection.Protection = "Bus encryption enabled flag set";
                    else if (bee)
                        info.CopyProtection!.Protection += "\nBus encryption enabled flag set";
                    else
                        info.CopyProtection!.Protection ??= addPlaceholders ? RequiredIfExistsValue : string.Empty;

                    break;

                case RedumpSystem.DVDVideo:
                case RedumpSystem.HDDVDVideo:
                    info.CommonDiscInfo!.Category ??= DiscCategory.Video;
                    info.CopyProtection!.Protection ??= addPlaceholders ? RequiredIfExistsValue : string.Empty;
                    break;

                case RedumpSystem.CommodoreAmigaCD:
                    info.CommonDiscInfo!.EXEDateBuildDate ??= addPlaceholders ? RequiredValue : string.Empty;
                    break;

                case RedumpSystem.CommodoreAmigaCD32:
                    info.CommonDiscInfo!.EXEDateBuildDate ??= addPlaceholders ? RequiredValue : string.Empty;
                    info.CommonDiscInfo.Region ??= Region.Europe;
                    break;

                case RedumpSystem.CommodoreAmigaCDTV:
                    info.CommonDiscInfo!.EXEDateBuildDate ??= addPlaceholders ? RequiredValue : string.Empty;
                    info.CommonDiscInfo.Region ??= Region.Europe;
                    break;

                case RedumpSystem.FujitsuFMTownsseries:
                    info.CommonDiscInfo!.EXEDateBuildDate ??= addPlaceholders ? RequiredValue : string.Empty;
                    info.CommonDiscInfo.Region ??= Region.Japan;
                    break;

                case RedumpSystem.FujitsuFMTownsMarty:
                    info.CommonDiscInfo!.Region ??= Region.Japan;
                    break;

                case RedumpSystem.HasbroVideoNow:
                case RedumpSystem.HasbroVideoNowColor:
                case RedumpSystem.HasbroVideoNowJr:
                case RedumpSystem.VideoCD:
                    info.CommonDiscInfo!.Category ??= DiscCategory.Video;
                    break;

                case RedumpSystem.HasbroVideoNowXP:
                case RedumpSystem.PhotoCD:
                case RedumpSystem.SonyElectronicBook:
                    info.CommonDiscInfo!.Category ??= DiscCategory.Multimedia;
                    break;

                case RedumpSystem.IncredibleTechnologiesEagle:
                    info.CommonDiscInfo!.EXEDateBuildDate ??= addPlaceholders ? RequiredValue : string.Empty;
                    break;

                case RedumpSystem.KonamieAmusement:
                    info.CommonDiscInfo!.EXEDateBuildDate ??= addPlaceholders ? RequiredValue : string.Empty;
                    break;

                case RedumpSystem.KonamiFireBeat:
                    info.CommonDiscInfo!.EXEDateBuildDate ??= addPlaceholders ? RequiredValue : string.Empty;
                    break;

                case RedumpSystem.KonamiPython2:
                    string? kp2Exe = PhysicalTool.GetPlayStationExecutableName(drive);

                    // TODO: Remove this hack when DIC supports build date output
                    if (isDiscImageCreator)
                        info.CommonDiscInfo!.EXEDateBuildDate = DiscImageCreator.GetPlayStationEXEDate($"{basePath}_volDesc.txt", kp2Exe);

                    SetCommentFieldIfNotExists(info, SiteCode.InternalSerialName, drive, PhysicalTool.GetPlayStationSerial);
                    info.CommonDiscInfo!.EXEDateBuildDate ??= PhysicalTool.GetFileDate(drive, kp2Exe, fixTwoDigitYear: true);

                    if (CommentFieldExists(info, SiteCode.InternalSerialName, out kp2Exe))
                        info.CommonDiscInfo!.Region = ProcessingTool.GetPlayStationRegion(kp2Exe);

                    SetVersionIfNotExists(info, drive, PhysicalTool.GetPlayStation2Version);
                    break;

                case RedumpSystem.KonamiSystemGV:
                    info.CommonDiscInfo!.EXEDateBuildDate ??= addPlaceholders ? RequiredValue : string.Empty;
                    break;

                case RedumpSystem.KonamiSystem573:
                    info.CommonDiscInfo!.EXEDateBuildDate ??= addPlaceholders ? RequiredValue : string.Empty;
                    break;

                case RedumpSystem.KonamiTwinkle:
                    info.CommonDiscInfo!.EXEDateBuildDate ??= addPlaceholders ? RequiredValue : string.Empty;
                    break;

                case RedumpSystem.MattelHyperScan:
                    info.CommonDiscInfo!.EXEDateBuildDate ??= addPlaceholders ? RequiredValue : string.Empty;
                    break;

                case RedumpSystem.MicrosoftXboxOne:
                    info.CommonDiscInfo!.CommentsSpecialFields![SiteCode.Filename] = PhysicalTool.GetXboxFilenames(drive) ?? string.Empty;
                    break;

                case RedumpSystem.MicrosoftXboxSeriesXS:
                    info.CommonDiscInfo!.CommentsSpecialFields![SiteCode.Filename] = PhysicalTool.GetXboxFilenames(drive) ?? string.Empty;
                    break;

                case RedumpSystem.NamcoSegaNintendoTriforce:
                    info.CommonDiscInfo!.EXEDateBuildDate ??= addPlaceholders ? RequiredValue : string.Empty;
                    break;

                case RedumpSystem.NavisoftNaviken21:
                    info.CommonDiscInfo!.EXEDateBuildDate = addPlaceholders ? RequiredValue : string.Empty;
                    info.CommonDiscInfo.Region ??= Region.Japan;
                    break;

                case RedumpSystem.NECPC88series:
                    info.CommonDiscInfo!.Region ??= Region.Japan;
                    break;

                case RedumpSystem.NECPC98series:
                    info.CommonDiscInfo!.EXEDateBuildDate = addPlaceholders ? RequiredValue : string.Empty;
                    info.CommonDiscInfo!.Region ??= Region.Japan;
                    break;

                case RedumpSystem.NECPCFXPCFXGA:
                    info.CommonDiscInfo!.Region ??= Region.Japan;
                    break;

                case RedumpSystem.SegaChihiro:
                    info.CommonDiscInfo!.EXEDateBuildDate ??= addPlaceholders ? RequiredValue : string.Empty;
                    break;

                case RedumpSystem.SegaDreamcast:
                    info.CommonDiscInfo!.EXEDateBuildDate ??= addPlaceholders ? RequiredValue : string.Empty;
                    break;

                case RedumpSystem.SegaNaomi:
                    info.CommonDiscInfo!.EXEDateBuildDate ??= addPlaceholders ? RequiredValue : string.Empty;
                    break;

                case RedumpSystem.SegaNaomi2:
                    info.CommonDiscInfo!.EXEDateBuildDate ??= addPlaceholders ? RequiredValue : string.Empty;
                    break;

                case RedumpSystem.SegaTitanVideo:
                    info.CommonDiscInfo!.EXEDateBuildDate ??= addPlaceholders ? RequiredValue : string.Empty;
                    break;

                case RedumpSystem.SharpX68000:
                    info.CommonDiscInfo!.Region ??= Region.Japan;
                    break;

                case RedumpSystem.SNKNeoGeoCD:
                    info.CommonDiscInfo!.EXEDateBuildDate ??= addPlaceholders ? RequiredValue : string.Empty;
                    break;

                case RedumpSystem.SonyPlayStation:
                    string? ps1Exe = PhysicalTool.GetPlayStationExecutableName(drive);

                    // TODO: Remove this hack when DIC supports build date output
                    if (isDiscImageCreator)
                        info.CommonDiscInfo!.EXEDateBuildDate = DiscImageCreator.GetPlayStationEXEDate($"{basePath}_volDesc.txt", ps1Exe, psx: true);

                    SetCommentFieldIfNotExists(info, SiteCode.InternalSerialName, drive, PhysicalTool.GetPlayStationSerial);
                    info.CommonDiscInfo!.EXEDateBuildDate ??= PhysicalTool.GetFileDate(drive, ps1Exe, fixTwoDigitYear: true);

                    if (CommentFieldExists(info, SiteCode.InternalSerialName, out ps1Exe))
                        info.CommonDiscInfo!.Region = ProcessingTool.GetPlayStationRegion(ps1Exe);

                    break;

                case RedumpSystem.SonyPlayStation2:
                    info.CommonDiscInfo!.LanguageSelection ??= [];
                    string? ps2Exe = PhysicalTool.GetPlayStationExecutableName(drive);

                    // TODO: Remove this hack when DIC supports build date output
                    if (isDiscImageCreator)
                        info.CommonDiscInfo!.EXEDateBuildDate = DiscImageCreator.GetPlayStationEXEDate($"{basePath}_volDesc.txt", ps2Exe);

                    SetCommentFieldIfNotExists(info, SiteCode.InternalSerialName, drive, PhysicalTool.GetPlayStationSerial);
                    info.CommonDiscInfo!.EXEDateBuildDate ??= PhysicalTool.GetFileDate(drive, ps2Exe, fixTwoDigitYear: true);

                    if (CommentFieldExists(info, SiteCode.InternalSerialName, out ps2Exe))
                        info.CommonDiscInfo.Region = ProcessingTool.GetPlayStationRegion(ps2Exe);

                    SetVersionIfNotExists(info, drive, PhysicalTool.GetPlayStation2Version);
                    break;

                case RedumpSystem.SonyPlayStation3:
                    info.Extras!.DiscKey ??= addPlaceholders ? RequiredValue : string.Empty;
                    info.Extras.DiscID ??= addPlaceholders ? RequiredValue : string.Empty;

                    SetCommentFieldIfNotExists(info, SiteCode.InternalSerialName, drive, PhysicalTool.GetPlayStation3Serial);
                    SetVersionIfNotExists(info, drive, PhysicalTool.GetPlayStation3Version);
                    SetContentFieldIfNotExists(info, SiteCode.Patches, drive, FormatPlayStation3FirmwareVersion);
                    break;

                case RedumpSystem.SonyPlayStation4:
                    SetCommentFieldIfNotExists(info, SiteCode.InternalSerialName, drive, PhysicalTool.GetPlayStation4Serial);
                    SetVersionIfNotExists(info, drive, PhysicalTool.GetPlayStation4Version);
                    break;

                case RedumpSystem.SonyPlayStation5:
                    SetCommentFieldIfNotExists(info, SiteCode.InternalSerialName, drive, PhysicalTool.GetPlayStation5Serial);
                    SetVersionIfNotExists(info, drive, PhysicalTool.GetPlayStation5Version);
                    break;

                case RedumpSystem.TomyKissSite:
                    info.CommonDiscInfo!.Category ??= DiscCategory.Video;
                    info.CommonDiscInfo!.Region ??= Region.Japan;
                    break;

                case RedumpSystem.ZAPiTGamesGameWaveFamilyEntertainmentSystem:
                    info.CopyProtection!.Protection ??= addPlaceholders ? RequiredIfExistsValue : string.Empty;
                    break;
            }

            return true;
        }

        /// <summary>
        /// Get a preformatted string for the PS3 firmware version, if possible
        /// </summary>
        private static string? FormatPlayStation3FirmwareVersion(Drive? drive)
        {
            string? firmwareVersion = PhysicalTool.GetPlayStation3FirmwareVersion(drive);
            if (string.IsNullOrEmpty(firmwareVersion))
                return string.Empty;

            return $"PS3 Firmware {firmwareVersion}";
        }

        /// <summary>
        /// Determine if a comment field exists based on key
        /// </summary>
        private static bool CommentFieldExists(SubmissionInfo info, SiteCode key, out string? value)
        {
            // Ensure the comments fields exist
            if (info.CommonDiscInfo!.CommentsSpecialFields == null)
                info.CommonDiscInfo.CommentsSpecialFields = [];

            // Check if the field exists
            if (!info.CommonDiscInfo.CommentsSpecialFields.TryGetValue(key, out value))
                return false;
            if (string.IsNullOrEmpty(value))
                return false;

            // The value is valid
            return true;
        }

        /// <summary>
        /// Set a comment field if it doesn't already have a value
        /// </summary>
        private static void SetCommentFieldIfNotExists(SubmissionInfo info, SiteCode key, Drive? drive, Func<Drive?, string?> valueFunc)
        {
            // If the field has a valid value, skip
            if (CommentFieldExists(info, key, out _))
                return;

            // Set the value
            info.CommonDiscInfo!.CommentsSpecialFields![key] = valueFunc(drive) ?? string.Empty;
        }

        /// <summary>
        /// Determine if a content field exists based on key
        /// </summary>
        private static bool ContentFieldExists(SubmissionInfo info, SiteCode key, out string? value)
        {
            // Ensure the contents fields exist
            if (info.CommonDiscInfo!.ContentsSpecialFields == null)
                info.CommonDiscInfo.ContentsSpecialFields = [];

            // Check if the field exists
            if (!info.CommonDiscInfo.ContentsSpecialFields.TryGetValue(key, out value))
                return false;
            if (string.IsNullOrEmpty(value))
                return false;

            // The value is valid
            return true;
        }

        /// <summary>
        /// Set a content field if it doesn't already have a value
        /// </summary>
        private static void SetContentFieldIfNotExists(SubmissionInfo info, SiteCode key, Drive? drive, Func<Drive?, string?> valueFunc)
        {
            // If the field has a valid value, skip
            if (ContentFieldExists(info, key, out _))
                return;

            // Set the value
            info.CommonDiscInfo!.ContentsSpecialFields![key] = valueFunc(drive) ?? string.Empty;
        }

        /// <summary>
        /// Set the version if it doesn't already have a value
        /// </summary>
        private static void SetVersionIfNotExists(SubmissionInfo info, Drive? drive, Func<Drive?, string?> valueFunc)
        {
            // If the version already exists, skip
            if (!string.IsNullOrEmpty(info.VersionAndEditions!.Version))
                return;

            // Set the version
            info.VersionAndEditions.Version = valueFunc(drive) ?? string.Empty;
        }

        /// <summary>
        /// Reformat a protection dictionary for submission info
        /// </summary>
        /// <param name="oldDict">ProtectionDictionary to format</param>
        /// <returns>Reformatted dictionary on success, empty on error</returns>
        private static Dictionary<string, List<string>?> ReformatProtectionDictionary(ProtectionDictionary? oldDict)
        {
            // Null or empty protections return empty
            if (oldDict == null || oldDict.Count == 0)
                return [];

            // Reformat each set into a List
            var newDict = new Dictionary<string, List<string>?>();
            foreach (string key in oldDict.Keys)
            {
                newDict[key] = [.. oldDict[key]];
            }

            return newDict;
        }

        #endregion
    }
}
