using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
            Frontend.Options options,
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
            (bool foundFiles, List<string> missingFiles) = processor.FoundAllFiles(outputDirectory, outputFilename, false);
            if (!foundFiles)
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
                processor?.GenerateArtifacts(info, combinedBase);

            // Get a list of matching IDs for each line in the DAT
            if (!string.IsNullOrEmpty(info.TracksAndWriteOffsets!.ClrMameProData) && options.HasRedumpLogin)
#if NET40
                _ = FillFromRedump(options, info, resultProgress);
#else
                _ = await FillFromRedump(options, info, resultProgress);
#endif

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
            if (drive != null && SupportsAntiModchipScans(system) && info.CopyProtection!.AntiModchip == YesNo.NULL)
            {
                resultProgress?.Report(ResultEventArgs.Success("Checking for anti-modchip strings... this might take a while!"));
                info.CopyProtection.AntiModchip = await ProtectionTool.GetPlayStationAntiModchipDetected(drive?.Name) ? YesNo.Yes : YesNo.No;
                resultProgress?.Report(ResultEventArgs.Success("Anti-modchip string scan complete!"));
            }

            // Run copy protection, if possible or necessary
            if (SupportsCopyProtectionScans(system))
            {
                resultProgress?.Report(ResultEventArgs.Success("Running copy protection scan... this might take a while!"));
                var (protectionString, fullProtections) = await ProtectionTool.GetCopyProtection(drive, options, protectionProgress);

                info.CopyProtection!.Protection += protectionString;
                info.CopyProtection.FullProtections = fullProtections as Dictionary<string, List<string>?> ?? [];
                resultProgress?.Report(ResultEventArgs.Success("Copy protection scan complete!"));
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
#if NET40
        public static bool FillFromRedump(Frontend.Options options, SubmissionInfo info, IProgress<ResultEventArgs>? resultProgress = null)
#else
        public async static Task<bool> FillFromRedump(Frontend.Options options, SubmissionInfo info, IProgress<ResultEventArgs>? resultProgress = null)
#endif
        {
            // If no username is provided
            if (string.IsNullOrEmpty(options.RedumpUsername) || string.IsNullOrEmpty(options.RedumpPassword))
                return false;

            // Set the current dumper based on username
            info.DumpersAndStatus ??= new DumpersAndStatusSection();
            info.DumpersAndStatus.Dumpers = [options.RedumpUsername!];
            info.PartiallyMatchedIDs = [];

            // Login to Redump
#if NETFRAMEWORK
            using var wc = new RedumpWebClient();
            bool? loggedIn = wc.Login(options.RedumpUsername!, options.RedumpPassword!);
#else
            using var wc = new RedumpHttpClient();
            bool? loggedIn = await wc.Login(options.RedumpUsername, options.RedumpPassword);
#endif
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

#if NET40
                var validateTask = Validator.ValidateSingleTrack(wc, info, sha1);
                validateTask.Wait();
                (bool singleFound, var foundIds, string? result) = validateTask.Result;
#else
                (bool singleFound, var foundIds, string? result) = await Validator.ValidateSingleTrack(wc, info, sha1);
#endif
                if (singleFound)
                    resultProgress?.Report(ResultEventArgs.Success(result));
                else
                    resultProgress?.Report(ResultEventArgs.Failure(result));

                // Add the found IDs to the map
                foundIdSets.Add(foundIds?.ToArray() ?? []);

                // Ensure that all tracks are found
                allFound &= singleFound;
            }

            // If all tracks were found, check if there are any fully-matched IDs
            List<int>? fullyMatchedIDs = null;
            if (allFound)
            {
                fullyMatchedIDs = null;
                foreach (var set in foundIdSets)
                {
                    // First track is always all IDs
                    if (fullyMatchedIDs == null)
                    {
                        fullyMatchedIDs = [.. set];
                        continue;
                    }
                    
                    // Try to intersect with all known IDs
                    fullyMatchedIDs = fullyMatchedIDs.Intersect(set).ToList();
                    if (!fullyMatchedIDs.Any())
                        break;
                }
            }

            // If we don't have any matches but we have a universal hash
            if (!info.PartiallyMatchedIDs.Any() && info.CommonDiscInfo?.CommentsSpecialFields?.ContainsKey(SiteCode.UniversalHash) == true)
            {
#if NET40
                var validateTask = Validator.ValidateUniversalHash(wc, info);
                validateTask.Wait();
                (bool singleFound, var foundIds, string? result) = validateTask.Result;
#else
                (bool singleFound, var foundIds, string? result) = await Validator.ValidateUniversalHash(wc, info);
#endif
                if (singleFound)
                    resultProgress?.Report(ResultEventArgs.Success(result));
                else
                    resultProgress?.Report(ResultEventArgs.Failure(result));

                // Ensure that the hash is found
                allFound = singleFound;

                // If we found a match, then the disc is a match
                if (singleFound && foundIds != null)
                    fullyMatchedIDs = foundIds;
                else
                    fullyMatchedIDs = [];
            }

            // Make sure we only have unique IDs
            info.PartiallyMatchedIDs = [.. info.PartiallyMatchedIDs.Distinct().OrderBy(id => id)];

            resultProgress?.Report(ResultEventArgs.Success("Match finding complete! " + (fullyMatchedIDs != null && fullyMatchedIDs.Count > 0
                ? "Fully Matched IDs: " + string.Join(",", fullyMatchedIDs.Select(i => i.ToString()).ToArray())
                : "No matches found")));

            // Exit early if one failed or there are no matched IDs
            if (!allFound || fullyMatchedIDs == null || fullyMatchedIDs.Count == 0)
                return false;

            // Find the first matched ID where the track count matches, we can grab a bunch of info from it
            int totalMatchedIDsCount = fullyMatchedIDs.Count;
            for (int i = 0; i < totalMatchedIDsCount; i++)
            {
                // Skip if the track count doesn't match
#if NET40
                var validateTask = Validator.ValidateTrackCount(wc, fullyMatchedIDs[i], trackCount);
                validateTask.Wait();
                if (!validateTask.Result)
#else
                if (!await Validator.ValidateTrackCount(wc, fullyMatchedIDs[i], trackCount))
#endif
                    continue;

                // Fill in the fields from the existing ID
                resultProgress?.Report(ResultEventArgs.Success($"Filling fields from existing ID {fullyMatchedIDs[i]}..."));
#if NET40
                var fillTask = Task.Factory.StartNew(() => Builder.FillFromId(wc, info, fullyMatchedIDs[i], options.PullAllInformation));
                fillTask.Wait();
                _ = fillTask.Result;
#else
                _ = await Builder.FillFromId(wc, info, fullyMatchedIDs[i], options.PullAllInformation);
#endif
                resultProgress?.Report(ResultEventArgs.Success("Information filling complete!"));

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
        /// Formats a list of volume labels and their corresponding filesystems
        /// </summary>
        /// <param name="labels">Dictionary of volume labels and their filesystems</param>
        /// <returns>Formatted string of volume labels and their filesystems</returns>
        private static string? FormatVolumeLabels(string? driveLabel, Dictionary<string, List<string>>? labels)
        {
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

            // If only one label, don't mention fs
            string firstLabel = labels.First().Key;
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
            volLabels = volLabels.Where(l => !string.IsNullOrEmpty(l?.Trim())).ToList();

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
                    info.Extras!.BCA ??= (addPlaceholders ? RequiredValue : string.Empty);
                    break;

                case MediaType.NintendoWiiOpticalDisc:

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
                    info.Extras.BCA = info.Extras.BCA ?? (addPlaceholders ? RequiredValue : string.Empty);

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
                    // TODO: Remove this hack when DIC supports build date output
                    if (isDiscImageCreator)
                        info.CommonDiscInfo!.EXEDateBuildDate = DiscImageCreator.GetPlayStationEXEDate($"{basePath}_volDesc.txt", drive?.GetPlayStationExecutableName());

                    if (!info.CommonDiscInfo!.CommentsSpecialFields!.TryGetValue(SiteCode.InternalSerialName, out string? kp2Exe) || string.IsNullOrEmpty(kp2Exe))
                        info.CommonDiscInfo.Region = Drive.GetPlayStationRegion(kp2Exe);

                    if (drive?.GetPlayStationExecutableInfo(out var kp2Serial, out Region? kp2Region, out var kp2Date) == true)
                    {
                        if (!info.CommonDiscInfo!.CommentsSpecialFields!.TryGetValue(SiteCode.InternalSerialName, out string? value) || string.IsNullOrEmpty(value))
                            info.CommonDiscInfo!.CommentsSpecialFields![SiteCode.InternalSerialName] = kp2Serial ?? string.Empty;

                        info.CommonDiscInfo.Region ??= kp2Region;
                        info.CommonDiscInfo.EXEDateBuildDate ??= kp2Date;
                    }

                    if (string.IsNullOrEmpty(info.VersionAndEditions!.Version))
                        info.VersionAndEditions!.Version = drive?.GetPlayStation2Version() ?? string.Empty;

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
                    if (drive?.Name != null)
                    {
                        string xboxOneMsxcPath = Path.Combine(drive.Name, "MSXC");
                        if (drive != null && Directory.Exists(xboxOneMsxcPath))
                        {
                            info.CommonDiscInfo!.CommentsSpecialFields![SiteCode.Filename] = string.Join("\n",
                                Directory.GetFiles(xboxOneMsxcPath, "*", SearchOption.TopDirectoryOnly).Select(Path.GetFileName).ToArray());
                        }
                    }

                    break;

                case RedumpSystem.MicrosoftXboxSeriesXS:
                    if (drive?.Name != null)
                    {
                        string xboxSeriesXMsxcPath = Path.Combine(drive.Name, "MSXC");
                        if (drive != null && Directory.Exists(xboxSeriesXMsxcPath))
                        {
                            info.CommonDiscInfo!.CommentsSpecialFields![SiteCode.Filename] = string.Join("\n",
                                Directory.GetFiles(xboxSeriesXMsxcPath, "*", SearchOption.TopDirectoryOnly).Select(Path.GetFileName).ToArray());
                        }
                    }

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
                    // TODO: Remove this hack when DIC supports build date output
                    if (isDiscImageCreator)
                        info.CommonDiscInfo!.EXEDateBuildDate = DiscImageCreator.GetPlayStationEXEDate($"{basePath}_volDesc.txt", drive?.GetPlayStationExecutableName(), psx: true);

                    if (!info.CommonDiscInfo!.CommentsSpecialFields!.TryGetValue(SiteCode.InternalSerialName, out string? psxExe) || string.IsNullOrEmpty(psxExe))
                        info.CommonDiscInfo.Region = Drive.GetPlayStationRegion(psxExe);

                    if (drive?.GetPlayStationExecutableInfo(out var psxSerial, out Region? psxRegion, out var psxDate) == true)
                    {
                        if (!info.CommonDiscInfo!.CommentsSpecialFields!.TryGetValue(SiteCode.InternalSerialName, out string? value) || string.IsNullOrEmpty(value))
                            info.CommonDiscInfo!.CommentsSpecialFields![SiteCode.InternalSerialName] = psxSerial ?? string.Empty;

                        info.CommonDiscInfo.Region ??= psxRegion;
                        info.CommonDiscInfo.EXEDateBuildDate ??= psxDate;
                    }

                    break;

                case RedumpSystem.SonyPlayStation2:
                    info.CommonDiscInfo!.LanguageSelection ??= [];

                    // TODO: Remove this hack when DIC supports build date output
                    if (isDiscImageCreator)
                        info.CommonDiscInfo!.EXEDateBuildDate = DiscImageCreator.GetPlayStationEXEDate($"{basePath}_volDesc.txt", drive?.GetPlayStationExecutableName());

                    if (!info.CommonDiscInfo!.CommentsSpecialFields!.TryGetValue(SiteCode.InternalSerialName, out string? ps2Exe) || string.IsNullOrEmpty(ps2Exe))
                        info.CommonDiscInfo.Region = Drive.GetPlayStationRegion(ps2Exe);

                    if (drive?.GetPlayStationExecutableInfo(out var ps2Serial, out Region? ps2Region, out var ps2Date) == true)
                    {
                        if (!info.CommonDiscInfo!.CommentsSpecialFields!.TryGetValue(SiteCode.InternalSerialName, out string? value) || string.IsNullOrEmpty(value))
                            info.CommonDiscInfo!.CommentsSpecialFields![SiteCode.InternalSerialName] = ps2Serial ?? string.Empty;

                        info.CommonDiscInfo.Region ??= ps2Region;
                        info.CommonDiscInfo.EXEDateBuildDate ??= ps2Date;
                    }

                    if (string.IsNullOrEmpty(info.VersionAndEditions!.Version))
                        info.VersionAndEditions!.Version = drive?.GetPlayStation2Version() ?? string.Empty;

                    break;

                case RedumpSystem.SonyPlayStation3:
                    info.Extras!.DiscKey ??= addPlaceholders ? RequiredValue : string.Empty;
                    info.Extras.DiscID ??= addPlaceholders ? RequiredValue : string.Empty;

                    if (!info.CommonDiscInfo!.CommentsSpecialFields!.TryGetValue(SiteCode.InternalSerialName, out string? ps3Serial) || string.IsNullOrEmpty(ps3Serial))
                        info.CommonDiscInfo!.CommentsSpecialFields![SiteCode.InternalSerialName] = drive?.GetPlayStation3Serial() ?? string.Empty;

                    if (string.IsNullOrEmpty(info.VersionAndEditions!.Version))
                        info.VersionAndEditions!.Version = drive?.GetPlayStation3Version() ?? string.Empty;

                    if (!info.CommonDiscInfo!.CommentsSpecialFields!.TryGetValue(SiteCode.Patches, out string? ps3Firmware) || string.IsNullOrEmpty(ps3Firmware))
                    {
                        string? firmwareVersion = drive?.GetPlayStation3FirmwareVersion();
                        if (firmwareVersion != null)
                            info.CommonDiscInfo!.ContentsSpecialFields![SiteCode.Patches] = $"PS3 Firmware {firmwareVersion}";
                    }

                    break;

                case RedumpSystem.SonyPlayStation4:
                    if (!info.CommonDiscInfo!.CommentsSpecialFields!.TryGetValue(SiteCode.InternalSerialName, out string? ps4Serial) || string.IsNullOrEmpty(ps4Serial))
                        info.CommonDiscInfo!.CommentsSpecialFields![SiteCode.InternalSerialName] = drive?.GetPlayStation4Serial() ?? string.Empty;

                    if (string.IsNullOrEmpty(info.VersionAndEditions!.Version))
                        info.VersionAndEditions!.Version = drive?.GetPlayStation4Version() ?? string.Empty;

                    break;

                case RedumpSystem.SonyPlayStation5:
                    if (!info.CommonDiscInfo!.CommentsSpecialFields!.TryGetValue(SiteCode.InternalSerialName, out string? ps5Serial) || string.IsNullOrEmpty(ps5Serial))
                        info.CommonDiscInfo!.CommentsSpecialFields![SiteCode.InternalSerialName] = drive?.GetPlayStation5Serial() ?? string.Empty;

                    if (string.IsNullOrEmpty(info.VersionAndEditions!.Version))
                        info.VersionAndEditions!.Version = drive?.GetPlayStation5Version() ?? string.Empty;

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
        /// Helper to determine if a system requires an anti-modchip scan
        /// </summary>
        private static bool SupportsAntiModchipScans(RedumpSystem? system)
        {
            return system switch
            {
                RedumpSystem.SonyPlayStation => true,
                _ => false,
            };
        }

        /// <summary>
        /// Helper to determine if a system requires a copy protection scan
        /// </summary>
        private static bool SupportsCopyProtectionScans(RedumpSystem? system)
        {
            return system switch
            {
                RedumpSystem.AppleMacintosh => true,
                RedumpSystem.EnhancedCD => true,
                RedumpSystem.IBMPCcompatible => true,
                RedumpSystem.PalmOS => true,
                RedumpSystem.PocketPC => true,
                RedumpSystem.RainbowDisc => true,
                RedumpSystem.SonyElectronicBook => true,
                _ => false,
            };
        }

        #endregion
    }
}
