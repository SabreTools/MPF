using System;
using System.Collections.Generic;
using System.IO;
#if NET35_OR_GREATER || NETCOREAPP
using System.Linq;
#endif
using System.Text;
using System.Threading.Tasks;
using BinaryObjectScanner;
using MPF.Processors;
using SabreTools.RedumpLib.Data;
using SabreTools.RedumpLib.Data.Sections;
using SabreTools.RedumpLib.Tools;
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
        private const string RequiredIfCFW = "(REQUIRED, IF CFW)";
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
            PhysicalSystem? system,
            PhysicalMediaType? mediaType,
            Options options,
            BaseProcessor processor,
            IProgress<ResultEventArgs>? resultProgress = null,
            IProgress<ProtectionProgress>? protectionProgress = null)
        {
            // Split the output path for easier use
            var outputDirectory = Path.GetDirectoryName(outputPath);
            string outputFilename = Path.GetFileName(outputPath);

            // Assemble a base path
            string basePath = Path.GetFileNameWithoutExtension(outputFilename);
            if (!string.IsNullOrEmpty(outputDirectory))
                basePath = Path.Combine(outputDirectory, basePath);

            // If a standard log zip was provided, replace the suffix with ".tmp" for easier processing
            if (outputFilename.EndsWith("_logs.zip", StringComparison.OrdinalIgnoreCase))
            {
                int zipSuffixIndex = outputFilename.LastIndexOf("_logs.zip", StringComparison.OrdinalIgnoreCase);
#if NETCOREAPP || NETSTANDARD2_1_OR_GREATER
                outputFilename = outputFilename[..zipSuffixIndex] + ".tmp";
#else
                outputFilename = outputFilename.Substring(0, zipSuffixIndex) + ".tmp";
#endif
            }

            // Check that all of the relevant files are there
            List<string> missingFiles = processor.FoundAllFiles(mediaType, outputDirectory, outputFilename);
            if (missingFiles.Count > 0)
            {
                resultProgress?.Report(ResultEventArgs.Failure($"There were files missing from the output:\n{string.Join("\n", [.. missingFiles])}\nThis may indicate an issue with the hardware or media, including unsupported devices.\nPlease see dumping program documentation for more details."));
                return null;
            }

            // Extract files from existing log archive, if it exists
#if NET462_OR_GREATER || NETCOREAPP
            processor.ExtractFromLogs(mediaType, outputDirectory, outputFilename);
#endif

            // Create the default submission info
            SubmissionInfo info = CreateDefaultSubmissionInfo(processor, system, mediaType, options.Processing.MediaInformation.AddPlaceholders);

            // Get specific tool output handling
            processor.GenerateSubmissionInfo(info, mediaType, basePath, options.Processing.MediaInformation.EnableRedumpCompatibility);
            if (options.Processing.IncludeArtifacts)
                info.Artifacts = processor.GenerateArtifacts(mediaType, outputDirectory, outputFilename);

            // Get a list of matching IDs for each line in the DAT
            if (!string.IsNullOrEmpty(info.DumpMetadata.Dat))
                _ = await FillFromRedump(options, info, resultProgress);

            // TEMPORARY HACK -- Add newline after Dat section until RedumpLib is updated
            if (!string.IsNullOrEmpty(info.DumpMetadata.Dat))
                info.DumpMetadata.Dat += "\n";

            // Add the volume label to comments, if possible or necessary
            string? volLabels = FormatVolumeLabels(drive?.VolumeLabel, processor.VolumeLabels);
            if (volLabels is not null)
                info.DumpMetadata.CommentsSpecialFields[SiteCode.VolumeLabel] = volLabels;

            // Extract info based generically on PhysicalMediaType
            ProcessPhysicalMediaType(info, mediaType, options.Processing.MediaInformation.AddPlaceholders);

            // Extract info based specifically on PhysicalSystem
            ProcessSystem(info, system, drive, options.Processing.MediaInformation.AddPlaceholders, processor is DiscImageCreator, basePath);

            // Run anti-modchip check, if necessary
            // TODO: Reenable when anti-modchip documentation is updated
            // if (drive is not null && system.SupportsAntiModchipScans() && info.CopyProtection.AntiModchip == YesNo.NULL)
            // {
            //     // TODO: This goes into protection now
            //     resultProgress?.Report(ResultEventArgs.Neutral("Checking for anti-modchip strings... this might take a while!"));
            //     info.CopyProtection.AntiModchip = await ProtectionTool.GetPlayStationAntiModchipDetected(drive?.Name) ? YesNo.Yes : YesNo.No;
            //     resultProgress?.Report(ResultEventArgs.Success("Anti-modchip string scan complete!"));
            // }

            // Run copy protection, if possible or necessary
            if (system.SupportsCopyProtectionScans())
            {
                resultProgress?.Report(ResultEventArgs.Neutral("Running copy protection scan... this might take a while!"));

                try
                {
                    Dictionary<string, List<string>>? protections = null;
                    if (options.Processing.ProtectionScanning.ScanForProtection)
                    {
                        // Explicitly note missing/invalid device paths
                        if (drive?.Name is null)
                            resultProgress?.Report(ResultEventArgs.Success("No mounted device path found, protection outputs may be incomplete!"));

                        protections = await ProtectionTool.RunCombinedProtectionScans(basePath, drive, options, protectionProgress);
                    }

                    var protectionString = ProtectionTool.FormatProtections(protections, drive);

                    info.DumpMetadata.Protection += protectionString;
                    info.DumpMetadata.FullProtections = ReformatProtectionDictionary(protections);
                    resultProgress?.Report(ResultEventArgs.Success("Copy protection scan complete!"));
                }
                catch (Exception ex)
                {
                    resultProgress?.Report(ResultEventArgs.Failure(ex.ToString()));
                }
            }

            // Set fields that may have automatic filling otherwise
            info.DiscIdentity.Category ??= DiscCategory.Games;
            info.DiscIdentifiers.Version ??= options.Processing.MediaInformation.AddPlaceholders ? RequiredIfExistsValue : string.Empty;

            // Comments and contents have odd handling
            if (string.IsNullOrEmpty(info.DumpMetadata.Comments))
                info.DumpMetadata.Comments = options.Processing.MediaInformation.AddPlaceholders ? OptionalValue : string.Empty;
            if (string.IsNullOrEmpty(info.DumpMetadata.Contents))
                info.DumpMetadata.Contents = options.Processing.MediaInformation.AddPlaceholders ? OptionalValue : string.Empty;

            // Normalize the disc type with all current information
            info.NormalizeDiscType();

            return info;
        }

        /// <summary>
        /// Fill in a SubmissionInfo object from redump.info, if possible
        /// </summary>
        /// <param name="options">Options object representing user-defined options</param>
        /// <param name="info">Existing SubmissionInfo object to fill</param>
        /// <param name="resultProgress">Optional result progress callback</param>
        public static async Task<bool> FillFromRedump(Options options,
            SubmissionInfo info,
            IProgress<ResultEventArgs>? resultProgress = null)
        {
            // If information should not be pulled at all
            if (!options.Processing.Login.RetrieveMatchInformation)
                return false;

            // Reset the partially matched IDs
            info.PartiallyMatchedIDs = [];

            // Login to redump.info, if possible
            int attemptCount = options.Processing.Login.AttemptCount;
            int timeoutSeconds = options.Processing.Login.TimeoutSeconds;
            var wc = new Client
            {
                AttemptCount = attemptCount > 0 ? attemptCount : 3,
                Timeout = TimeSpan.FromSeconds(timeoutSeconds > 0 ? timeoutSeconds : 30),
            };

            // Setup the checks
            bool allFound = true;
            List<int[]> foundIdSets = [];

            // Loop through all of the hashdata to find matching IDs
            resultProgress?.Report(ResultEventArgs.Neutral("Finding disc matches on redump.info, this might take a while..."));
            var splitData = info.DumpMetadata.Dat?.TrimEnd('\n')?.Split('\n');
            int trackCount = splitData?.Length ?? 0;
            foreach (string hashData in splitData ?? [])
            {
                // Catch any errant blank lines
                if (string.IsNullOrEmpty(hashData))
                {
                    trackCount--;
                    resultProgress?.Report(ResultEventArgs.Neutral("Blank line found, skipping!"));
                    continue;
                }

                // If the line ends in a known extra track names, skip them for checking
                // TODO: Smarter method to ignore all tracks that start with 0. 00. A. or AA.
                if (hashData.Contains(".dmi")
                    || hashData.Contains(".pfi")
                    || hashData.Contains(".ss")
                    || hashData.Contains("(Track 0).bin")
                    || hashData.Contains("(Track 0.1).bin")
                    || hashData.Contains("(Track 0.2).bin")
                    || hashData.Contains("(Track 0.3).bin")
                    || hashData.Contains("(Track 0.4).bin")
                    || hashData.Contains("(Track 0.5).bin")
                    || hashData.Contains("(Track 00).bin")
                    || hashData.Contains("(Track 00.1).bin")
                    || hashData.Contains("(Track 00.2).bin")
                    || hashData.Contains("(Track 00.3).bin")
                    || hashData.Contains("(Track 00.4).bin")
                    || hashData.Contains("(Track 00.5).bin")
                    || hashData.Contains("(Track A).bin")
                    || hashData.Contains("(Track A.1).bin")
                    || hashData.Contains("(Track A.2).bin")
                    || hashData.Contains("(Track A.3).bin")
                    || hashData.Contains("(Track A.4).bin")
                    || hashData.Contains("(Track A.5).bin")
                    || hashData.Contains("(Track AA).bin")
                    || hashData.Contains("(Track AA.1).bin")
                    || hashData.Contains("(Track AA.2).bin")
                    || hashData.Contains("(Track AA.3).bin")
                    || hashData.Contains("(Track AA.4).bin")
                    || hashData.Contains("(Track AA.5).bin"))
                {
                    trackCount--;
                    resultProgress?.Report(ResultEventArgs.Neutral("Extra track found, skipping!"));
                    continue;
                }

                // Get the SHA-1 hash
                if (!ProcessingTool.GetISOHashValues(hashData, out _, out _, out _, out string? sha1))
                {
                    resultProgress?.Report(ResultEventArgs.Failure($"Line could not be parsed: {hashData}"));
                    continue;
                }

                var foundIds = await Validator.ValidateSingleTrack(wc, info, sha1);
                if (foundIds is null)
                {
                    resultProgress?.Report(ResultEventArgs.Failure("Error accessing redump.info"));
                    return false;
                }
                else if (foundIds.Count == 0)
                {
                    resultProgress?.Report(ResultEventArgs.Failure($"No matches found for {sha1}"));
                }
                else if (foundIds.Count == 1)
                {
                    resultProgress?.Report(ResultEventArgs.Success($"Single match found for {sha1}"));
                }
                else
                {
                    resultProgress?.Report(ResultEventArgs.Success($"Multiple matches found for {sha1}"));
                }

                // Add the found IDs to the map
                foundIdSets.Add(foundIds?.ToArray() ?? []);

                // Ensure that all tracks are found
                allFound &= foundIds is not null && foundIds.Count >= 1;
            }

            // If all tracks were found, check if there are any fully-matched IDs
            HashSet<int>? fullyMatchedIdsSet = null;
            if (allFound)
            {
                fullyMatchedIdsSet = null;
                foreach (var set in foundIdSets)
                {
                    // First track is always all IDs
                    if (fullyMatchedIdsSet is null)
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
            if (info.PartiallyMatchedIDs.Count == 0 && info.DiscIdentifiers?.UniversalHash is not null)
            {
                string sha1 = info.DiscIdentifiers.UniversalHash;
                var foundIds = await Validator.ValidateUniversalHash(wc, info);
                if (foundIds is null)
                    resultProgress?.Report(ResultEventArgs.Failure("Error accessing redump.info"));
                else if (foundIds.Count == 0)
                    resultProgress?.Report(ResultEventArgs.Failure($"No matches found for universal hash {sha1}"));
                else if (foundIds.Count == 1)
                    resultProgress?.Report(ResultEventArgs.Success($"Single match found for universal hash {sha1}"));
                else
                    resultProgress?.Report(ResultEventArgs.Success($"Multiple matches found for universal hash {sha1}"));

                // Ensure that the hash is found
                allFound = foundIds is not null && foundIds.Count == 1;

                // If we found a match, then the disc is a match
                if (foundIds is not null && foundIds.Count == 1)
                    fullyMatchedIdsSet = [.. foundIds];
                else
                    fullyMatchedIdsSet = [];
            }

            // Get a list version of the fully matched IDs
            List<int> fullyMatchedIdsList = fullyMatchedIdsSet is not null ? [.. fullyMatchedIdsSet] : [];

            // Make sure we only have unique IDs
            var partiallyMatchedIds = new HashSet<int>();
            partiallyMatchedIds.IntersectWith(info.PartiallyMatchedIDs);
            info.PartiallyMatchedIDs = [.. partiallyMatchedIds];
            info.PartiallyMatchedIDs.Sort();

            resultProgress?.Report(ResultEventArgs.Neutral("Match finding complete! " + (fullyMatchedIdsList is not null && fullyMatchedIdsList.Count > 0
                ? "Fully Matched IDs: " + string.Join(",", [.. fullyMatchedIdsList.ConvertAll(i => i.ToString())])
                : "No matches found")));

            // Exit early if one failed or there are no matched IDs
            if (!allFound || fullyMatchedIdsList is null || fullyMatchedIdsList.Count == 0)
                return false;

            // If there are more than one fully-matched IDs, add to partial list and return
            if (fullyMatchedIdsList.Count != 1)
            {
                var fullyMatchedIds = new HashSet<int>();

                fullyMatchedIds.IntersectWith(fullyMatchedIdsList);
                fullyMatchedIds.IntersectWith(info.PartiallyMatchedIDs);
                info.PartiallyMatchedIDs = [.. fullyMatchedIds];
                info.PartiallyMatchedIDs.Sort();

                return true;
            }

            // Use the fully-matched ID to grab a bunch of info from it
            // Fill in the fields from the existing ID
            resultProgress?.Report(ResultEventArgs.Neutral($"Filling fields from existing ID {fullyMatchedIdsList[0]}..."));
            _ = await Builder.FillFromId(wc, info, fullyMatchedIdsList[0], options.Processing.Login.PullAllInformation);
            resultProgress?.Report(ResultEventArgs.Success("Information filling complete!"));

            // Set the fully matched ID to the current
            info.FullyMatchedIDs = fullyMatchedIdsList;

            // Clear out fully matched IDs from the partial list
            if (info.FullyMatchedIDs.Count > 0)
            {
                info.PartiallyMatchedIDs.RemoveAll(id => info.FullyMatchedIDs.Contains(id));
                if (info.PartiallyMatchedIDs.Count == 0)
                    info.PartiallyMatchedIDs = null;
            }

            return true;
        }

        #endregion

        #region Helper Functions

        /// <summary>
        /// Creates a default SubmissionInfo object based on the current system and media type
        /// </summary>
        private static SubmissionInfo CreateDefaultSubmissionInfo(BaseProcessor processor, PhysicalSystem? system, PhysicalMediaType? mediaType, bool addPlaceholders)
        {
            // Create the template object
            var info = new SubmissionInfo()
            {
                DiscIdentity = new DiscIdentitySection()
                {
                    System = system,
                    Media = mediaType.ToMediaType(),
                    Category = null,
                    Title = addPlaceholders ? RequiredValue : string.Empty,
                    ForeignTitle = addPlaceholders ? OptionalValue : string.Empty,
                    DiscNumber = addPlaceholders ? OptionalValue : string.Empty,
                    DiscTitle = addPlaceholders ? OptionalValue : string.Empty,
                },
                RegionsAndLanguages = new RegionsAndLanguagesSection()
                {
                    Regions = null,
                    Languages = null,
                },
                DiscIdentifiers = new DiscIdentifiersSection()
                {
                    DiscSerials = addPlaceholders ? RequiredIfExistsValue : string.Empty,
                    Editions = addPlaceholders ? "(VERIFY THIS) Original" : string.Empty,
                    Barcodes = addPlaceholders ? OptionalValue : string.Empty,
                },
                DumpMetadata = new DumpMetadataSection()
                {
                    Contents = string.Empty,
                },
                SubmissionControls = new SubmissionControlsSection()
                {
                    LogsArchiveURL = addPlaceholders ? RequiredValue : string.Empty,
                    SubmissionComment = addPlaceholders ? OptionalValue : string.Empty,
                },
                DumpingInfo = new DumpingInfoSection()
                {
                    FrontendVersion = FrontendTool.GetCurrentVersion(),
                    DumpingProgram = GetDumpingProgramFromProcessor(processor),
                },
            };

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
                Aaru => InternalProgram.Aaru,
                CleanRip => InternalProgram.CleanRip,
                DiscImageCreator => InternalProgram.DiscImageCreator,
                // Dreamdump => InternalProgram.Dreamdump,
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
        /// <returns>Simplified volume label, null if empty or invalid</returns>
        private static string? SimplifyVolumeLabel(string? label)
        {
            // Ignore empty labels
            if (label is null || label.Length == 0)
                return null;

            // Take only ASCII alphanumeric characters
            var labelBuilder = new StringBuilder();
            foreach (char c in label)
            {
                if (c >= '0' && c <= '9')
                    labelBuilder.Append(c);
                else if (c >= 'A' && c <= 'Z')
                    labelBuilder.Append(c);
                else if (c >= 'a' && c <= 'z')
                    labelBuilder.Append(char.ToUpper(c));
            }

            // Ignore non-ASCII labels
            string? simpleLabel = labelBuilder.ToString();
            if (simpleLabel is null || simpleLabel.Length == 0)
                return null;

            return simpleLabel;
        }

        /// <summary>
        /// Formats a list of volume labels and their corresponding filesystems
        /// </summary>
        /// <param name="labels">Dictionary of volume labels and their filesystems</param>
        /// <returns>Formatted string of volume labels and their filesystems</returns>
        internal static string? FormatVolumeLabels(string? driveLabel, Dictionary<string, List<string>>? labels)
        {
            // Treat empty label as null
            if (driveLabel is not null && driveLabel.Length == 0)
                driveLabel = null;

            // Treat "path" labels as null -- Indicates a mounted path
            // This can over-match if a label contains a directory separator somehow
            if (driveLabel is not null && (driveLabel.Contains("/") || driveLabel.Contains("\\")))
                driveLabel = null;

            // Must have at least one label to format
            if (driveLabel is null && (labels is null || labels.Count == 0))
                return null;

            // If no labels given, use drive label
            if (labels is null || labels.Count == 0)
            {
                // Ignore common volume labels
                if (FrontendTool.GetPhysicalSystemFromVolumeLabel(driveLabel) is not null)
                    return null;

                // Ignore "DVD_ROM" / "CD_ROM" labels
                if (driveLabel == "DVD_ROM" || driveLabel == "CD_ROM")
                    return null;

                return driveLabel;
            }

            // Get the default label to compare against
            // TODO: Full pairwise comparison of all labels, not just comparing against drive/UDF label.
            string? defaultLabel = null;
            if (driveLabel is not null && driveLabel.Length != 0)
            {
                defaultLabel = SimplifyVolumeLabel(driveLabel);
            }
#if NET35_OR_GREATER || NETCOREAPP
            else
            {
                defaultLabel = labels
                    .Where(label => label.Value.Contains("UDF"))
                    .Select(label => label.Key)
                    .FirstOrDefault();
            }
#endif

            // Remove duplicate/useless volume labels
            if (defaultLabel is not null && defaultLabel.Length != 0)
            {
                List<string> keys = [.. labels.Keys];
                foreach (var label in keys)
                {
                    // Always remove "DVD_ROM" / "CD_ROM" labels
                    if (label == "DVD_ROM" || label == "CD_ROM")
                    {
                        labels.Remove(label);
                        continue;
                    }

                    // Get upper-case ASCII variant of the label
                    string? tempLabel = SimplifyVolumeLabel(label);
                    if (tempLabel is null)
                        continue;

                    // Remove duplicate volume labels
                    if (defaultLabel == tempLabel)
                        labels.Remove(label);
                }
            }

            // If no labels are left, use drive label
            if (labels is null || labels.Count == 0)
            {
                // Ignore common volume labels
                if (FrontendTool.GetPhysicalSystemFromVolumeLabel(driveLabel) is not null)
                    return null;

                // Ignore "DVD_ROM" / "CD_ROM" labels
                if (driveLabel == "DVD_ROM" || driveLabel == "CD_ROM")
                    return null;

                return driveLabel;
            }

            // If only one unique label left, don't mention fs
#if NET20
            string[] keyArr = [.. labels.Keys];
            string firstLabel = keyArr[0];
#else
            string firstLabel = labels.First().Key;
#endif
            if (labels.Count == 1 && (firstLabel == driveLabel || driveLabel is null))
            {
                // Ignore common volume labels
                if (FrontendTool.GetPhysicalSystemFromVolumeLabel(firstLabel) is not null)
                    return null;

                // Ignore "DVD_ROM" / "CD_ROM" labels
                if (driveLabel == "DVD_ROM" || driveLabel == "CD_ROM")
                    return null;

                return firstLabel;
            }

            // Otherwise, state filesystem for each label
            List<string> volLabels = [];

            // Begin formatted output with the label from Windows, if it is unique and not a common volume label
            if (driveLabel is not null && !labels.TryGetValue(driveLabel, out List<string>? value) && FrontendTool.GetPhysicalSystemFromVolumeLabel(driveLabel) is null)
                volLabels.Add(driveLabel);

            // Add remaining labels with their corresponding filesystems
            foreach (var kvp in labels)
            {
                // Ignore common volume labels
                if (FrontendTool.GetPhysicalSystemFromVolumeLabel(kvp.Key) is null)
                    volLabels.Add($"{kvp.Key} ({string.Join(", ", [.. kvp.Value])})");
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
        private static bool ProcessPhysicalMediaType(SubmissionInfo info, PhysicalMediaType? mediaType, bool addPlaceholders)
        {
#pragma warning disable IDE0010
            switch (mediaType)
            {
                case PhysicalMediaType.CDROM:
                case PhysicalMediaType.GDROM:
                    info.RingCodes.Layer0MasteringCode = addPlaceholders ? RequiredIfExistsValue : string.Empty;
                    info.RingCodes.Layer0MasteringSID = addPlaceholders ? RequiredIfExistsValue : string.Empty;
                    info.RingCodes.Layer0Toolstamps = addPlaceholders ? RequiredIfExistsValue : string.Empty;
                    info.RingCodes.Layer0MouldSIDs = addPlaceholders ? RequiredIfExistsValue : string.Empty;
                    info.RingCodes.Layer0AdditionalMoulds = addPlaceholders ? RequiredIfExistsValue : string.Empty;

                    info.RingCodes.LabelSideMasteringCode = addPlaceholders ? RequiredIfExistsValue : string.Empty;
                    info.RingCodes.LabelSideMasteringSID = addPlaceholders ? RequiredIfExistsValue : string.Empty;
                    info.RingCodes.LabelSideToolstamps = addPlaceholders ? RequiredIfExistsValue : string.Empty;
                    info.RingCodes.LabelSideMouldSIDs = addPlaceholders ? RequiredIfExistsValue : string.Empty;
                    info.RingCodes.LabelSideAdditionalMoulds = addPlaceholders ? RequiredIfExistsValue : string.Empty;
                    break;

                case PhysicalMediaType.DVD:
                case PhysicalMediaType.HDDVD:
                case PhysicalMediaType.BluRay:

                    // If we have a single-layer disc
                    if (info.DiscIdentifiers.Layerbreak == default)
                    {
                        info.RingCodes.Layer0MasteringCode = addPlaceholders ? RequiredIfExistsValue : string.Empty;
                        info.RingCodes.Layer0MasteringSID = addPlaceholders ? RequiredIfExistsValue : string.Empty;
                        info.RingCodes.Layer0Toolstamps = addPlaceholders ? RequiredIfExistsValue : string.Empty;
                        info.RingCodes.Layer0MouldSIDs = addPlaceholders ? RequiredIfExistsValue : string.Empty;
                        info.RingCodes.Layer0AdditionalMoulds = addPlaceholders ? RequiredIfExistsValue : string.Empty;
                    }
                    // If we have a dual-layer disc
                    else if (info.DiscIdentifiers.Layerbreak2 == default)
                    {
                        info.RingCodes.Layer0MasteringCode = addPlaceholders ? RequiredIfExistsValue : string.Empty;
                        info.RingCodes.Layer0MasteringSID = addPlaceholders ? RequiredIfExistsValue : string.Empty;
                        info.RingCodes.Layer0Toolstamps = addPlaceholders ? RequiredIfExistsValue : string.Empty;
                        info.RingCodes.Layer0MouldSIDs = addPlaceholders ? RequiredIfExistsValue : string.Empty;
                        info.RingCodes.Layer0AdditionalMoulds = addPlaceholders ? RequiredIfExistsValue : string.Empty;

                        info.RingCodes.Layer1MasteringCode = addPlaceholders ? RequiredIfExistsValue : string.Empty;
                        info.RingCodes.Layer1MasteringSID = addPlaceholders ? RequiredIfExistsValue : string.Empty;
                        info.RingCodes.Layer1Toolstamps = addPlaceholders ? RequiredIfExistsValue : string.Empty;
                        info.RingCodes.Layer1MouldSIDs = addPlaceholders ? RequiredIfExistsValue : string.Empty;
                        info.RingCodes.Layer1AdditionalMoulds = addPlaceholders ? RequiredIfExistsValue : string.Empty;
                    }
                    // If we have a triple-layer disc
                    else if (info.DiscIdentifiers.Layerbreak3 == default)
                    {
                        info.RingCodes.Layer0MasteringCode = addPlaceholders ? RequiredIfExistsValue : string.Empty;
                        info.RingCodes.Layer0MasteringSID = addPlaceholders ? RequiredIfExistsValue : string.Empty;
                        info.RingCodes.Layer0Toolstamps = addPlaceholders ? RequiredIfExistsValue : string.Empty;
                        info.RingCodes.Layer0MouldSIDs = addPlaceholders ? RequiredIfExistsValue : string.Empty;
                        info.RingCodes.Layer0AdditionalMoulds = addPlaceholders ? RequiredIfExistsValue : string.Empty;

                        info.RingCodes.Layer1MasteringCode = addPlaceholders ? RequiredIfExistsValue : string.Empty;
                        info.RingCodes.Layer1MasteringSID = addPlaceholders ? RequiredIfExistsValue : string.Empty;
                        info.RingCodes.Layer1Toolstamps = addPlaceholders ? RequiredIfExistsValue : string.Empty;
                        info.RingCodes.Layer1MouldSIDs = addPlaceholders ? RequiredIfExistsValue : string.Empty;
                        info.RingCodes.Layer1AdditionalMoulds = addPlaceholders ? RequiredIfExistsValue : string.Empty;

                        info.RingCodes.Layer2MasteringCode = addPlaceholders ? RequiredIfExistsValue : string.Empty;
                        info.RingCodes.Layer2MasteringSID = addPlaceholders ? RequiredIfExistsValue : string.Empty;
                        info.RingCodes.Layer2Toolstamps = addPlaceholders ? RequiredIfExistsValue : string.Empty;
                        info.RingCodes.Layer2MouldSIDs = addPlaceholders ? RequiredIfExistsValue : string.Empty;
                        info.RingCodes.Layer2AdditionalMoulds = addPlaceholders ? RequiredIfExistsValue : string.Empty;
                    }
                    // If we have a quad-layer disc
                    else
                    {
                        info.RingCodes.Layer0MasteringCode = addPlaceholders ? RequiredIfExistsValue : string.Empty;
                        info.RingCodes.Layer0MasteringSID = addPlaceholders ? RequiredIfExistsValue : string.Empty;
                        info.RingCodes.Layer0Toolstamps = addPlaceholders ? RequiredIfExistsValue : string.Empty;
                        info.RingCodes.Layer0MouldSIDs = addPlaceholders ? RequiredIfExistsValue : string.Empty;
                        info.RingCodes.Layer0AdditionalMoulds = addPlaceholders ? RequiredIfExistsValue : string.Empty;

                        info.RingCodes.Layer1MasteringCode = addPlaceholders ? RequiredIfExistsValue : string.Empty;
                        info.RingCodes.Layer1MasteringSID = addPlaceholders ? RequiredIfExistsValue : string.Empty;
                        info.RingCodes.Layer1Toolstamps = addPlaceholders ? RequiredIfExistsValue : string.Empty;
                        info.RingCodes.Layer1MouldSIDs = addPlaceholders ? RequiredIfExistsValue : string.Empty;
                        info.RingCodes.Layer1AdditionalMoulds = addPlaceholders ? RequiredIfExistsValue : string.Empty;

                        info.RingCodes.Layer2MasteringCode = addPlaceholders ? RequiredIfExistsValue : string.Empty;
                        info.RingCodes.Layer2MasteringSID = addPlaceholders ? RequiredIfExistsValue : string.Empty;
                        info.RingCodes.Layer2Toolstamps = addPlaceholders ? RequiredIfExistsValue : string.Empty;
                        info.RingCodes.Layer2MouldSIDs = addPlaceholders ? RequiredIfExistsValue : string.Empty;
                        info.RingCodes.Layer2AdditionalMoulds = addPlaceholders ? RequiredIfExistsValue : string.Empty;

                        info.RingCodes.Layer3MasteringCode = addPlaceholders ? RequiredIfExistsValue : string.Empty;
                        info.RingCodes.Layer3MasteringSID = addPlaceholders ? RequiredIfExistsValue : string.Empty;
                        info.RingCodes.Layer3Toolstamps = addPlaceholders ? RequiredIfExistsValue : string.Empty;
                        info.RingCodes.Layer3MouldSIDs = addPlaceholders ? RequiredIfExistsValue : string.Empty;
                        info.RingCodes.Layer3AdditionalMoulds = addPlaceholders ? RequiredIfExistsValue : string.Empty;
                    }

                    info.RingCodes.LabelSideMasteringCode = addPlaceholders ? RequiredIfExistsValue : string.Empty;
                    info.RingCodes.LabelSideMasteringSID = addPlaceholders ? RequiredIfExistsValue : string.Empty;
                    info.RingCodes.LabelSideToolstamps = addPlaceholders ? RequiredIfExistsValue : string.Empty;
                    info.RingCodes.LabelSideMouldSIDs = addPlaceholders ? RequiredIfExistsValue : string.Empty;
                    info.RingCodes.LabelSideAdditionalMoulds = addPlaceholders ? RequiredIfExistsValue : string.Empty;

                    break;

                case PhysicalMediaType.NintendoGameCubeGameDisc:
                    info.RingCodes.Layer0MasteringCode = addPlaceholders ? RequiredIfExistsValue : string.Empty;
                    info.RingCodes.Layer0MasteringSID = addPlaceholders ? RequiredIfExistsValue : string.Empty;
                    info.RingCodes.Layer0Toolstamps = addPlaceholders ? RequiredIfExistsValue : string.Empty;
                    info.RingCodes.Layer0MouldSIDs = addPlaceholders ? RequiredIfExistsValue : string.Empty;
                    info.RingCodes.Layer0AdditionalMoulds = addPlaceholders ? RequiredIfExistsValue : string.Empty;

                    info.RingCodes.LabelSideMasteringCode = addPlaceholders ? RequiredIfExistsValue : string.Empty;
                    info.RingCodes.LabelSideMasteringSID = addPlaceholders ? RequiredIfExistsValue : string.Empty;
                    info.RingCodes.LabelSideToolstamps = addPlaceholders ? RequiredIfExistsValue : string.Empty;
                    info.RingCodes.LabelSideMouldSIDs = addPlaceholders ? RequiredIfExistsValue : string.Empty;
                    info.RingCodes.LabelSideAdditionalMoulds = addPlaceholders ? RequiredIfExistsValue : string.Empty;

                    info.DumpMetadata.BCA ??= addPlaceholders ? RequiredValue : string.Empty;
                    break;

                case PhysicalMediaType.NintendoWiiOpticalDisc:

                    // If we have a single-layer disc
                    if (info.DiscIdentifiers.Layerbreak == default)
                    {
                        info.RingCodes.Layer0MasteringCode = addPlaceholders ? RequiredIfExistsValue : string.Empty;
                        info.RingCodes.Layer0MasteringSID = addPlaceholders ? RequiredIfExistsValue : string.Empty;
                        info.RingCodes.Layer0Toolstamps = addPlaceholders ? RequiredIfExistsValue : string.Empty;
                        info.RingCodes.Layer0MouldSIDs = addPlaceholders ? RequiredIfExistsValue : string.Empty;
                        info.RingCodes.Layer0AdditionalMoulds = addPlaceholders ? RequiredIfExistsValue : string.Empty;
                    }
                    // If we have a dual-layer disc
                    else
                    {
                        info.RingCodes.Layer0MasteringCode = addPlaceholders ? RequiredIfExistsValue : string.Empty;
                        info.RingCodes.Layer0MasteringSID = addPlaceholders ? RequiredIfExistsValue : string.Empty;
                        info.RingCodes.Layer0Toolstamps = addPlaceholders ? RequiredIfExistsValue : string.Empty;
                        info.RingCodes.Layer0MouldSIDs = addPlaceholders ? RequiredIfExistsValue : string.Empty;
                        info.RingCodes.Layer0AdditionalMoulds = addPlaceholders ? RequiredIfExistsValue : string.Empty;

                        info.RingCodes.Layer1MasteringCode = addPlaceholders ? RequiredIfExistsValue : string.Empty;
                        info.RingCodes.Layer1MasteringSID = addPlaceholders ? RequiredIfExistsValue : string.Empty;
                        info.RingCodes.Layer1Toolstamps = addPlaceholders ? RequiredIfExistsValue : string.Empty;
                        info.RingCodes.Layer1MouldSIDs = addPlaceholders ? RequiredIfExistsValue : string.Empty;
                        info.RingCodes.Layer1AdditionalMoulds = addPlaceholders ? RequiredIfExistsValue : string.Empty;
                    }

                    info.DumpMetadata.BCA ??= addPlaceholders ? RequiredValue : string.Empty;

                    break;

                case PhysicalMediaType.NintendoWiiUOpticalDisc:

                    // If we have a single-layer disc
                    if (info.DiscIdentifiers.Layerbreak == default)
                    {
                        info.RingCodes.Layer0MasteringCode = addPlaceholders ? RequiredIfExistsValue : string.Empty;
                        info.RingCodes.Layer0MasteringSID = addPlaceholders ? RequiredIfExistsValue : string.Empty;
                        info.RingCodes.Layer0Toolstamps = addPlaceholders ? RequiredIfExistsValue : string.Empty;
                        info.RingCodes.Layer0MouldSIDs = addPlaceholders ? RequiredIfExistsValue : string.Empty;
                        info.RingCodes.Layer0AdditionalMoulds = addPlaceholders ? RequiredIfExistsValue : string.Empty;
                    }
                    // If we have a dual-layer disc
                    else
                    {
                        info.RingCodes.Layer0MasteringCode = addPlaceholders ? RequiredIfExistsValue : string.Empty;
                        info.RingCodes.Layer0MasteringSID = addPlaceholders ? RequiredIfExistsValue : string.Empty;
                        info.RingCodes.Layer0Toolstamps = addPlaceholders ? RequiredIfExistsValue : string.Empty;
                        info.RingCodes.Layer0MouldSIDs = addPlaceholders ? RequiredIfExistsValue : string.Empty;
                        info.RingCodes.Layer0AdditionalMoulds = addPlaceholders ? RequiredIfExistsValue : string.Empty;

                        info.RingCodes.Layer1MasteringCode = addPlaceholders ? RequiredIfExistsValue : string.Empty;
                        info.RingCodes.Layer1MasteringSID = addPlaceholders ? RequiredIfExistsValue : string.Empty;
                        info.RingCodes.Layer1Toolstamps = addPlaceholders ? RequiredIfExistsValue : string.Empty;
                        info.RingCodes.Layer1MouldSIDs = addPlaceholders ? RequiredIfExistsValue : string.Empty;
                        info.RingCodes.Layer1AdditionalMoulds = addPlaceholders ? RequiredIfExistsValue : string.Empty;
                    }

                    info.RingCodes.LabelSideMasteringCode = addPlaceholders ? RequiredIfExistsValue : string.Empty;
                    info.RingCodes.LabelSideMasteringSID = addPlaceholders ? RequiredIfExistsValue : string.Empty;
                    info.RingCodes.LabelSideToolstamps = addPlaceholders ? RequiredIfExistsValue : string.Empty;
                    info.RingCodes.LabelSideMouldSIDs = addPlaceholders ? RequiredIfExistsValue : string.Empty;
                    info.RingCodes.LabelSideAdditionalMoulds = addPlaceholders ? RequiredIfExistsValue : string.Empty;

                    info.DiscIdentifiers.DiscKey = addPlaceholders ? RequiredValue : string.Empty;

                    break;

                case PhysicalMediaType.UMD:
                    // Both single- and dual-layer discs have two "layers" for the ring
                    info.RingCodes.Layer0MasteringCode = addPlaceholders ? RequiredIfExistsValue : string.Empty;
                    info.RingCodes.Layer0MasteringSID = addPlaceholders ? RequiredIfExistsValue : string.Empty;
                    info.RingCodes.Layer0Toolstamps = addPlaceholders ? RequiredIfExistsValue : string.Empty;
                    info.RingCodes.Layer0MouldSIDs = addPlaceholders ? RequiredIfExistsValue : string.Empty;
                    info.RingCodes.Layer0AdditionalMoulds = addPlaceholders ? RequiredIfExistsValue : string.Empty;

                    info.RingCodes.Layer1MasteringCode = addPlaceholders ? RequiredIfExistsValue : string.Empty;
                    info.RingCodes.Layer1MasteringSID = addPlaceholders ? RequiredIfExistsValue : string.Empty;
                    info.RingCodes.Layer1Toolstamps = addPlaceholders ? RequiredIfExistsValue : string.Empty;
                    info.RingCodes.Layer1MouldSIDs = addPlaceholders ? RequiredIfExistsValue : string.Empty;
                    info.RingCodes.Layer1AdditionalMoulds = addPlaceholders ? RequiredIfExistsValue : string.Empty;

                    info.RingCodes.LabelSideMasteringCode = addPlaceholders ? RequiredIfExistsValue : string.Empty;
                    info.RingCodes.LabelSideMasteringSID = addPlaceholders ? RequiredIfExistsValue : string.Empty;
                    info.RingCodes.LabelSideToolstamps = addPlaceholders ? RequiredIfExistsValue : string.Empty;
                    info.RingCodes.LabelSideMouldSIDs = addPlaceholders ? RequiredIfExistsValue : string.Empty;
                    info.RingCodes.LabelSideAdditionalMoulds = addPlaceholders ? RequiredIfExistsValue : string.Empty;
                    break;
            }
#pragma warning restore IDE0010

            return true;
        }

        /// <summary>
        /// Processes default data based on system type
        /// </summary>
        private static bool ProcessSystem(SubmissionInfo info, PhysicalSystem? system, Drive? drive, bool addPlaceholders, bool isDiscImageCreator, string basePath)
        {
#pragma warning disable IDE0010
            // Extract info based specifically on PhysicalSystem
            switch (system)
            {
                case PhysicalSystem.AcornArchimedesAndRiscPC:
                    info.RegionsAndLanguages.Regions ??= [Region.UnitedKingdom];
                    break;

                case PhysicalSystem.AudioCD:
                case PhysicalSystem.DVDAudio:
                case PhysicalSystem.EnhancedCD:
                case PhysicalSystem.SuperAudioCD:
                    info.DiscIdentity.Category ??= DiscCategory.Audio;
                    break;

                case PhysicalSystem.BandaiPlaydiaQuickInteractiveSystem:
                    info.DiscIdentifiers.EXEDate ??= addPlaceholders ? RequiredValue : string.Empty;
                    info.RegionsAndLanguages.Regions ??= [Region.Japan];
                    break;

                case PhysicalSystem.BDVideo:
                    info.DiscIdentity.Category ??= DiscCategory.Video;

                    // General protection info
                    string? bdProtection = PhysicalTool.GetBluRayProtection(drive);
                    if (bdProtection is not null && string.IsNullOrEmpty(info.DumpMetadata.Protection))
                        info.DumpMetadata.Protection = bdProtection;
                    else if (bdProtection is not null)
                        info.DumpMetadata.Protection += $"\n{bdProtection}";
                    else
                        info.DumpMetadata.Protection ??= addPlaceholders ? RequiredIfExistsValue : string.Empty;

                    // Determine if BEE is set
                    bool busEncryptionEnabled = PhysicalTool.GetBusEncryptionEnabled(drive);
                    if (busEncryptionEnabled && !info.DumpMetadata.CommentsSpecialFields.ContainsKey(SiteCode.Protection))
                        info.DumpMetadata.CommentsSpecialFields[SiteCode.Protection] = "Bus Encryption Enabled";
                    else if (busEncryptionEnabled && info.DumpMetadata.CommentsSpecialFields.ContainsKey(SiteCode.Protection))
                        info.DumpMetadata.Protection += $"\nBus Encryption Enabled";

                    break;

                case PhysicalSystem.DVDVideo:
                case PhysicalSystem.HDDVDVideo:
                    info.DiscIdentity.Category ??= DiscCategory.Video;
                    info.DumpMetadata.Protection ??= addPlaceholders ? RequiredIfExistsValue : string.Empty;
                    break;

                case PhysicalSystem.CommodoreAmigaCD:
                    info.DiscIdentifiers.EXEDate ??= addPlaceholders ? RequiredValue : string.Empty;
                    break;

                case PhysicalSystem.CommodoreAmigaCD32:
                    info.DiscIdentifiers.EXEDate ??= addPlaceholders ? RequiredValue : string.Empty;
                    info.RegionsAndLanguages.Regions ??= [Region.Europe];
                    break;

                case PhysicalSystem.CommodoreAmigaCDTV:
                    info.DiscIdentifiers.EXEDate ??= addPlaceholders ? RequiredValue : string.Empty;
                    info.RegionsAndLanguages.Regions ??= [Region.Europe];
                    break;

                case PhysicalSystem.FujitsuFMTownsSeries:
                    info.DiscIdentifiers.EXEDate ??= addPlaceholders ? RequiredValue : string.Empty;
                    info.RegionsAndLanguages.Regions ??= [Region.Japan];
                    break;

                case PhysicalSystem.FujitsuFMTownsMarty:
                    info.RegionsAndLanguages.Regions ??= [Region.Japan];
                    break;

                case PhysicalSystem.HasbroVideoNow:
                case PhysicalSystem.HasbroVideoNowColor:
                case PhysicalSystem.HasbroVideoNowJr:
                case PhysicalSystem.VideoCD:
                    info.DiscIdentity.Category ??= DiscCategory.Video;
                    break;

                case PhysicalSystem.HasbroVideoNowXP:
                case PhysicalSystem.PhotoCD:
                case PhysicalSystem.SonyElectronicBook:
                    info.DiscIdentity.Category ??= DiscCategory.Multimedia;
                    break;

                case PhysicalSystem.IBMPCcompatible:
                case PhysicalSystem.AppleMacintosh:
                    info.DumpMetadata.CommentsSpecialFields[SiteCode.SteamAppID] =
                        PhysicalTool.GetSteamAppInfo(drive) ?? string.Empty;
                    info.DumpMetadata.ContentsSpecialFields[SiteCode.SteamSimSidDepotID] =
                        PhysicalTool.GetSteamSimSidInfo(drive) ?? string.Empty;
                    info.DumpMetadata.ContentsSpecialFields[SiteCode.SteamCsmCsdDepotID] =
                        PhysicalTool.GetSteamCsmCsdInfo(drive) ?? string.Empty;
                    break;

                case PhysicalSystem.IncredibleTechnologiesEagle:
                    info.DiscIdentifiers.EXEDate ??= addPlaceholders ? RequiredValue : string.Empty;
                    break;

                case PhysicalSystem.KonamieAmusement:
                    info.DiscIdentifiers.EXEDate ??= addPlaceholders ? RequiredValue : string.Empty;
                    break;

                case PhysicalSystem.KonamiFireBeat:
                    info.DiscIdentifiers.EXEDate ??= addPlaceholders ? RequiredValue : string.Empty;
                    break;

                case PhysicalSystem.KonamiPython2:
                    string? kp2Exe = PhysicalTool.GetPlayStationExecutableName(drive);

                    // TODO: Remove this hack when DIC supports build date output
                    if (isDiscImageCreator)
                        info.DiscIdentifiers.EXEDate = DiscImageCreator.GetPlayStationEXEDate($"{basePath}_volDesc.txt", kp2Exe);

                    SetCommentFieldIfNotExists(info, SiteCode.InternalSerialName, drive, PhysicalTool.GetPlayStationSerial);
                    info.DiscIdentifiers.EXEDate ??= PhysicalTool.GetFileDate(drive, kp2Exe, fixTwoDigitYear: true);

                    if (CommentFieldExists(info, SiteCode.InternalSerialName, out kp2Exe))
                        info.RegionsAndLanguages.Regions = [ProcessingTool.GetPlayStationRegion(kp2Exe)];

                    SetVersionIfNotExists(info, drive, PhysicalTool.GetPlayStation2Version);
                    break;

                case PhysicalSystem.KonamiSystemGV:
                    info.DiscIdentifiers.EXEDate ??= addPlaceholders ? RequiredValue : string.Empty;
                    break;

                case PhysicalSystem.KonamiSystem573:
                    info.DiscIdentifiers.EXEDate ??= addPlaceholders ? RequiredValue : string.Empty;
                    break;

                case PhysicalSystem.KonamiTwinkle:
                    info.DiscIdentifiers.EXEDate ??= addPlaceholders ? RequiredValue : string.Empty;
                    break;

                case PhysicalSystem.MattelHyperScan:
                    info.DiscIdentifiers.EXEDate ??= addPlaceholders ? RequiredValue : string.Empty;
                    break;

                case PhysicalSystem.MicrosoftXbox:
                case PhysicalSystem.MicrosoftXbox360:
                    if (!info.DumpMetadata.CommentsSpecialFields.ContainsKey(SiteCode.DiscHologramID))
                        info.DumpMetadata.CommentsSpecialFields[SiteCode.DiscHologramID] = addPlaceholders ? RequiredIfExistsValue : string.Empty;
                    break;

                case PhysicalSystem.MicrosoftXboxOne:
                    if (!info.DumpMetadata.CommentsSpecialFields.ContainsKey(SiteCode.DiscHologramID))
                        info.DumpMetadata.CommentsSpecialFields[SiteCode.DiscHologramID] = addPlaceholders ? RequiredIfExistsValue : string.Empty;
                    info.DumpMetadata.CommentsSpecialFields[SiteCode.Filename] = PhysicalTool.GetXboxFilenames(drive) ?? string.Empty;
                    info.DumpMetadata.CommentsSpecialFields[SiteCode.TitleID] = PhysicalTool.GetXboxTitleID(drive) ?? string.Empty;
                    break;

                case PhysicalSystem.MicrosoftXboxSeriesXS:
                    if (!info.DumpMetadata.CommentsSpecialFields.ContainsKey(SiteCode.DiscHologramID))
                        info.DumpMetadata.CommentsSpecialFields[SiteCode.DiscHologramID] = addPlaceholders ? RequiredIfExistsValue : string.Empty;
                    info.DumpMetadata.CommentsSpecialFields[SiteCode.Filename] = PhysicalTool.GetXboxFilenames(drive) ?? string.Empty;
                    info.DumpMetadata.CommentsSpecialFields[SiteCode.TitleID] = PhysicalTool.GetXboxTitleID(drive) ?? string.Empty;
                    break;

                case PhysicalSystem.NamcoSegaNintendoTriforce:
                    info.DiscIdentifiers.EXEDate ??= addPlaceholders ? RequiredValue : string.Empty;
                    break;

                case PhysicalSystem.NavisoftNaviken:
                    info.DiscIdentifiers.EXEDate = addPlaceholders ? RequiredValue : string.Empty;
                    info.RegionsAndLanguages.Regions ??= [Region.Japan];
                    break;

                case PhysicalSystem.NECPC88Series:
                    info.RegionsAndLanguages.Regions ??= [Region.Japan];
                    break;

                case PhysicalSystem.NECPC98Series:
                    info.DiscIdentifiers.EXEDate = addPlaceholders ? RequiredValue : string.Empty;
                    info.RegionsAndLanguages.Regions ??= [Region.Japan];
                    break;

                case PhysicalSystem.NECPCFXPCFXGA:
                    info.RegionsAndLanguages.Regions ??= [Region.Japan];
                    break;

                case PhysicalSystem.NintendoGameCube:
                case PhysicalSystem.NintendoWii:
                    if (!info.DumpMetadata.CommentsSpecialFields.ContainsKey(SiteCode.CoverID))
                        info.DumpMetadata.CommentsSpecialFields[SiteCode.CoverID] = addPlaceholders ? RequiredIfExistsValue : string.Empty;

                    break;

                case PhysicalSystem.SegaChihiro:
                    info.DiscIdentifiers.EXEDate ??= addPlaceholders ? RequiredValue : string.Empty;
                    break;

                case PhysicalSystem.SegaDreamcast:
                    info.DiscIdentifiers.EXEDate ??= addPlaceholders ? RequiredValue : string.Empty;
                    break;

                case PhysicalSystem.SegaNaomi:
                    info.DiscIdentifiers.EXEDate ??= addPlaceholders ? RequiredValue : string.Empty;
                    break;

                case PhysicalSystem.SegaNaomi2:
                    info.DiscIdentifiers.EXEDate ??= addPlaceholders ? RequiredValue : string.Empty;
                    break;

                case PhysicalSystem.SegaTitanVideo:
                    info.DiscIdentifiers.EXEDate ??= addPlaceholders ? RequiredValue : string.Empty;
                    break;

                case PhysicalSystem.SharpX68000:
                    info.RegionsAndLanguages.Regions ??= [Region.Japan];
                    break;

                case PhysicalSystem.SNKNeoGeoCD:
                    info.DiscIdentifiers.EXEDate ??= addPlaceholders ? RequiredValue : string.Empty;
                    break;

                case PhysicalSystem.SonyPlayStation:
                    string? ps1Exe = PhysicalTool.GetPlayStationExecutableName(drive);

                    // TODO: Remove this hack when DIC supports build date output
                    if (isDiscImageCreator)
                        info.DiscIdentifiers.EXEDate = DiscImageCreator.GetPlayStationEXEDate($"{basePath}_volDesc.txt", ps1Exe, psx: true);

                    SetCommentFieldIfNotExists(info, SiteCode.InternalSerialName, drive, PhysicalTool.GetPlayStationSerial);
                    info.DiscIdentifiers.EXEDate ??= PhysicalTool.GetFileDate(drive, ps1Exe, fixTwoDigitYear: true);

                    if (CommentFieldExists(info, SiteCode.InternalSerialName, out ps1Exe))
                        info.RegionsAndLanguages.Regions = [ProcessingTool.GetPlayStationRegion(ps1Exe)];

                    break;

                case PhysicalSystem.SonyPlayStation2:
                    string? ps2Exe = PhysicalTool.GetPlayStationExecutableName(drive);

                    // TODO: Remove this hack when DIC supports build date output
                    if (isDiscImageCreator)
                        info.DiscIdentifiers.EXEDate = DiscImageCreator.GetPlayStationEXEDate($"{basePath}_volDesc.txt", ps2Exe);

                    SetCommentFieldIfNotExists(info, SiteCode.InternalSerialName, drive, PhysicalTool.GetPlayStationSerial);
                    info.DiscIdentifiers.EXEDate ??= PhysicalTool.GetFileDate(drive, ps2Exe, fixTwoDigitYear: true);

                    if (CommentFieldExists(info, SiteCode.InternalSerialName, out ps2Exe))
                        info.RegionsAndLanguages.Regions = [ProcessingTool.GetPlayStationRegion(ps2Exe)];

                    SetVersionIfNotExists(info, drive, PhysicalTool.GetPlayStation2Version);
                    break;

                case PhysicalSystem.SonyPlayStation3:
                    info.DiscIdentifiers.DiscKey ??= addPlaceholders ? RequiredIfCFW : string.Empty;
                    info.DiscIdentifiers.DiscID ??= addPlaceholders ? RequiredIfCFW : string.Empty;

                    SetCommentFieldIfNotExists(info, SiteCode.InternalSerialName, drive, PhysicalTool.GetPlayStation3Serial);
                    SetVersionIfNotExists(info, drive, PhysicalTool.GetPlayStation3Version);
                    SetContentFieldIfNotExists(info, SiteCode.Patches, drive, FormatPlayStation3FirmwareVersion);
                    break;

                case PhysicalSystem.SonyPlayStation4:
                    SetCommentFieldIfNotExists(info, SiteCode.InternalSerialName, drive, PhysicalTool.GetPlayStation4Serial);
                    SetVersionIfNotExists(info, drive, PhysicalTool.GetPlayStation4Version);
                    SetContentFieldIfNotExists(info, SiteCode.Games, drive, PhysicalTool.GetPlayStation4PkgInfo);
                    break;

                case PhysicalSystem.SonyPlayStation5:
                    SetCommentFieldIfNotExists(info, SiteCode.InternalSerialName, drive, PhysicalTool.GetPlayStation5Serial);
                    SetVersionIfNotExists(info, drive, PhysicalTool.GetPlayStation5Version);
                    SetContentFieldIfNotExists(info, SiteCode.Games, drive, PhysicalTool.GetPlayStation5PkgInfo);
                    break;

                case PhysicalSystem.TomyKissSite:
                    info.DiscIdentity.Category ??= DiscCategory.Video;
                    info.RegionsAndLanguages.Regions ??= [Region.Japan];
                    break;

                case PhysicalSystem.ZAPiTGamesGameWaveFamilyEntertainmentSystem:
                    info.DumpMetadata.Protection ??= addPlaceholders ? RequiredIfExistsValue : string.Empty;
                    break;
            }
#pragma warning restore IDE0010

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
            if (info.DumpMetadata.CommentsSpecialFields is null)
                info.DumpMetadata.CommentsSpecialFields = [];

            // Check if the field exists
            if (!info.DumpMetadata.CommentsSpecialFields.TryGetValue(key, out value))
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
            string? value = valueFunc(drive);
            if (value is not null)
                info.DumpMetadata.CommentsSpecialFields[key] = value;
        }

        /// <summary>
        /// Determine if a content field exists based on key
        /// </summary>
        private static bool ContentFieldExists(SubmissionInfo info, SiteCode key, out string? value)
        {
            // Ensure the contents fields exist
            if (info.DumpMetadata.ContentsSpecialFields is null)
                info.DumpMetadata.ContentsSpecialFields = [];

            // Check if the field exists
            if (!info.DumpMetadata.ContentsSpecialFields.TryGetValue(key, out value))
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
            string? value = valueFunc(drive);
            if (value is not null)
                info.DumpMetadata.ContentsSpecialFields![key] = value;
        }

        /// <summary>
        /// Set the version if it doesn't already have a value
        /// </summary>
        private static void SetVersionIfNotExists(SubmissionInfo info, Drive? drive, Func<Drive?, string?> valueFunc)
        {
            // If the version already exists, skip
            if (!string.IsNullOrEmpty(info.DiscIdentifiers.Version))
                return;

            // Set the version
            info.DiscIdentifiers.Version = valueFunc(drive) ?? string.Empty;
        }

        /// <summary>
        /// Reformat a protection dictionary for submission info
        /// </summary>
        /// <param name="oldDict">ProtectionDictionary to format</param>
        /// <returns>Reformatted dictionary on success, empty on error</returns>
        private static Dictionary<string, List<string>?> ReformatProtectionDictionary(Dictionary<string, List<string>>? oldDict)
        {
            // Null or empty protections return empty
            if (oldDict is null || oldDict.Count == 0)
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
