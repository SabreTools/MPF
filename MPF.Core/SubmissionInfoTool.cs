using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using MPF.Core.Data;
using MPF.Core.Modules;
using SabreTools.RedumpLib;
using SabreTools.RedumpLib.Data;
using SabreTools.RedumpLib.Web;

namespace MPF.Core
{
    /// <summary>
    /// Class to hold all SubmissionInfo-related methods
    /// </summary>
    internal static class SubmissionInfoTool
    {
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
        public static async Task<SubmissionInfo?> ExtractOutputInformation(
            string outputPath,
            Drive? drive,
            RedumpSystem? system,
            MediaType? mediaType,
            Options options,
            BaseParameters? parameters,
            IProgress<Result>? resultProgress = null,
            IProgress<BinaryObjectScanner.ProtectionProgress>? protectionProgress = null)
        {
            // Ensure the current disc combination should exist
            if (!system.MediaTypes().Contains(mediaType))
                return null;

            // Split the output path for easier use
            var outputDirectory = Path.GetDirectoryName(outputPath);
            string outputFilename = Path.GetFileName(outputPath);

            // Check that all of the relevant files are there
            (bool foundFiles, List<string> missingFiles) = parameters.FoundAllFiles(outputDirectory, outputFilename, false);
            if (!foundFiles)
            {
                resultProgress?.Report(Result.Failure($"There were files missing from the output:\n{string.Join("\n", [.. missingFiles])}"));
                resultProgress?.Report(Result.Failure($"This may indicate an issue with the hardware or media, including unsupported devices.\nPlease see dumping program documentation for more details."));
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
            info = Builder.EnsureAllSections(info);

            // Get specific tool output handling
            parameters?.GenerateSubmissionInfo(info, options, combinedBase, drive, options.IncludeArtifacts);

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
            string? volLabels = FormatVolumeLabels(drive?.VolumeLabel, parameters?.VolumeLabels);
            if (volLabels != null)
                info.CommonDiscInfo!.CommentsSpecialFields![SiteCode.VolumeLabel] = volLabels;

            // Extract info based generically on MediaType
            switch (mediaType)
            {
                case MediaType.CDROM:
                case MediaType.GDROM:
                    info.CommonDiscInfo!.Layer0MasteringRing = options.AddPlaceholders ? Template.RequiredIfExistsValue : string.Empty;
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
                    if (info.SizeAndChecksums!.Layerbreak == default)
                    {
                        info.CommonDiscInfo!.Layer0MasteringRing = options.AddPlaceholders ? Template.RequiredIfExistsValue : string.Empty;
                        info.CommonDiscInfo.Layer0MasteringSID = options.AddPlaceholders ? Template.RequiredIfExistsValue : string.Empty;
                        info.CommonDiscInfo.Layer0ToolstampMasteringCode = options.AddPlaceholders ? Template.RequiredIfExistsValue : string.Empty;
                        info.CommonDiscInfo.Layer0MouldSID = options.AddPlaceholders ? Template.RequiredIfExistsValue : string.Empty;
                        info.CommonDiscInfo.Layer1MouldSID = options.AddPlaceholders ? Template.RequiredIfExistsValue : string.Empty;
                        info.CommonDiscInfo.Layer0AdditionalMould = options.AddPlaceholders ? Template.RequiredIfExistsValue : string.Empty;
                    }
                    // If we have a dual-layer disc
                    else if (info.SizeAndChecksums!.Layerbreak2 == default)
                    {
                        info.CommonDiscInfo!.Layer0MasteringRing = options.AddPlaceholders ? Template.RequiredIfExistsValue : string.Empty;
                        info.CommonDiscInfo.Layer0MasteringSID = options.AddPlaceholders ? Template.RequiredIfExistsValue : string.Empty;
                        info.CommonDiscInfo.Layer0ToolstampMasteringCode = options.AddPlaceholders ? Template.RequiredIfExistsValue : string.Empty;
                        info.CommonDiscInfo.Layer0MouldSID = options.AddPlaceholders ? Template.RequiredIfExistsValue : string.Empty;
                        info.CommonDiscInfo.Layer0AdditionalMould = options.AddPlaceholders ? Template.RequiredIfExistsValue : string.Empty;

                        info.CommonDiscInfo.Layer1MasteringRing = options.AddPlaceholders ? Template.RequiredIfExistsValue : string.Empty;
                        info.CommonDiscInfo.Layer1MasteringSID = options.AddPlaceholders ? Template.RequiredIfExistsValue : string.Empty;
                        info.CommonDiscInfo.Layer1ToolstampMasteringCode = options.AddPlaceholders ? Template.RequiredIfExistsValue : string.Empty;
                        info.CommonDiscInfo.Layer1MouldSID = options.AddPlaceholders ? Template.RequiredIfExistsValue : string.Empty;
                    }
                    // If we have a triple-layer disc
                    else if (info.SizeAndChecksums!.Layerbreak3 == default)
                    {
                        info.CommonDiscInfo!.Layer0MasteringRing = options.AddPlaceholders ? Template.RequiredIfExistsValue : string.Empty;
                        info.CommonDiscInfo.Layer0MasteringSID = options.AddPlaceholders ? Template.RequiredIfExistsValue : string.Empty;
                        info.CommonDiscInfo.Layer0ToolstampMasteringCode = options.AddPlaceholders ? Template.RequiredIfExistsValue : string.Empty;
                        info.CommonDiscInfo.Layer0MouldSID = options.AddPlaceholders ? Template.RequiredIfExistsValue : string.Empty;
                        info.CommonDiscInfo.Layer0AdditionalMould = options.AddPlaceholders ? Template.RequiredIfExistsValue : string.Empty;

                        info.CommonDiscInfo.Layer1MasteringRing = options.AddPlaceholders ? Template.RequiredIfExistsValue : string.Empty;
                        info.CommonDiscInfo.Layer1MasteringSID = options.AddPlaceholders ? Template.RequiredIfExistsValue : string.Empty;
                        info.CommonDiscInfo.Layer1ToolstampMasteringCode = options.AddPlaceholders ? Template.RequiredIfExistsValue : string.Empty;
                        info.CommonDiscInfo.Layer1MouldSID = options.AddPlaceholders ? Template.RequiredIfExistsValue : string.Empty;

                        info.CommonDiscInfo.Layer2MasteringRing = options.AddPlaceholders ? Template.RequiredIfExistsValue : string.Empty;
                        info.CommonDiscInfo.Layer2MasteringSID = options.AddPlaceholders ? Template.RequiredIfExistsValue : string.Empty;
                        info.CommonDiscInfo.Layer2ToolstampMasteringCode = options.AddPlaceholders ? Template.RequiredIfExistsValue : string.Empty;
                    }
                    // If we have a quad-layer disc
                    else
                    {
                        info.CommonDiscInfo!.Layer0MasteringRing = options.AddPlaceholders ? Template.RequiredIfExistsValue : string.Empty;
                        info.CommonDiscInfo.Layer0MasteringSID = options.AddPlaceholders ? Template.RequiredIfExistsValue : string.Empty;
                        info.CommonDiscInfo.Layer0ToolstampMasteringCode = options.AddPlaceholders ? Template.RequiredIfExistsValue : string.Empty;
                        info.CommonDiscInfo.Layer0MouldSID = options.AddPlaceholders ? Template.RequiredIfExistsValue : string.Empty;
                        info.CommonDiscInfo.Layer0AdditionalMould = options.AddPlaceholders ? Template.RequiredIfExistsValue : string.Empty;

                        info.CommonDiscInfo.Layer1MasteringRing = options.AddPlaceholders ? Template.RequiredIfExistsValue : string.Empty;
                        info.CommonDiscInfo.Layer1MasteringSID = options.AddPlaceholders ? Template.RequiredIfExistsValue : string.Empty;
                        info.CommonDiscInfo.Layer1ToolstampMasteringCode = options.AddPlaceholders ? Template.RequiredIfExistsValue : string.Empty;
                        info.CommonDiscInfo.Layer1MouldSID = options.AddPlaceholders ? Template.RequiredIfExistsValue : string.Empty;

                        info.CommonDiscInfo.Layer2MasteringRing = options.AddPlaceholders ? Template.RequiredIfExistsValue : string.Empty;
                        info.CommonDiscInfo.Layer2MasteringSID = options.AddPlaceholders ? Template.RequiredIfExistsValue : string.Empty;
                        info.CommonDiscInfo.Layer2ToolstampMasteringCode = options.AddPlaceholders ? Template.RequiredIfExistsValue : string.Empty;

                        info.CommonDiscInfo.Layer3MasteringRing = options.AddPlaceholders ? Template.RequiredIfExistsValue : string.Empty;
                        info.CommonDiscInfo.Layer3MasteringSID = options.AddPlaceholders ? Template.RequiredIfExistsValue : string.Empty;
                        info.CommonDiscInfo.Layer3ToolstampMasteringCode = options.AddPlaceholders ? Template.RequiredIfExistsValue : string.Empty;
                    }

                    break;

                case MediaType.NintendoGameCubeGameDisc:
                    info.CommonDiscInfo!.Layer0MasteringRing = options.AddPlaceholders ? Template.RequiredIfExistsValue : string.Empty;
                    info.CommonDiscInfo.Layer0MasteringSID = options.AddPlaceholders ? Template.RequiredIfExistsValue : string.Empty;
                    info.CommonDiscInfo.Layer0ToolstampMasteringCode = options.AddPlaceholders ? Template.RequiredIfExistsValue : string.Empty;
                    info.CommonDiscInfo.Layer0MouldSID = options.AddPlaceholders ? Template.RequiredIfExistsValue : string.Empty;
                    info.CommonDiscInfo.Layer1MouldSID = options.AddPlaceholders ? Template.RequiredIfExistsValue : string.Empty;
                    info.CommonDiscInfo.Layer0AdditionalMould = options.AddPlaceholders ? Template.RequiredIfExistsValue : string.Empty;
                    info.Extras!.BCA ??= (options.AddPlaceholders ? Template.RequiredValue : string.Empty);
                    break;

                case MediaType.NintendoWiiOpticalDisc:

                    // If we have a single-layer disc
                    if (info.SizeAndChecksums!.Layerbreak == default)
                    {
                        info.CommonDiscInfo!.Layer0MasteringRing = options.AddPlaceholders ? Template.RequiredIfExistsValue : string.Empty;
                        info.CommonDiscInfo.Layer0MasteringSID = options.AddPlaceholders ? Template.RequiredIfExistsValue : string.Empty;
                        info.CommonDiscInfo.Layer0ToolstampMasteringCode = options.AddPlaceholders ? Template.RequiredIfExistsValue : string.Empty;
                        info.CommonDiscInfo.Layer0MouldSID = options.AddPlaceholders ? Template.RequiredIfExistsValue : string.Empty;
                        info.CommonDiscInfo.Layer1MouldSID = options.AddPlaceholders ? Template.RequiredIfExistsValue : string.Empty;
                        info.CommonDiscInfo.Layer0AdditionalMould = options.AddPlaceholders ? Template.RequiredIfExistsValue : string.Empty;
                    }
                    // If we have a dual-layer disc
                    else
                    {
                        info.CommonDiscInfo!.Layer0MasteringRing = options.AddPlaceholders ? Template.RequiredIfExistsValue : string.Empty;
                        info.CommonDiscInfo.Layer0MasteringSID = options.AddPlaceholders ? Template.RequiredIfExistsValue : string.Empty;
                        info.CommonDiscInfo.Layer0ToolstampMasteringCode = options.AddPlaceholders ? Template.RequiredIfExistsValue : string.Empty;
                        info.CommonDiscInfo.Layer0MouldSID = options.AddPlaceholders ? Template.RequiredIfExistsValue : string.Empty;
                        info.CommonDiscInfo.Layer0AdditionalMould = options.AddPlaceholders ? Template.RequiredIfExistsValue : string.Empty;

                        info.CommonDiscInfo.Layer1MasteringRing = options.AddPlaceholders ? Template.RequiredIfExistsValue : string.Empty;
                        info.CommonDiscInfo.Layer1MasteringSID = options.AddPlaceholders ? Template.RequiredIfExistsValue : string.Empty;
                        info.CommonDiscInfo.Layer1ToolstampMasteringCode = options.AddPlaceholders ? Template.RequiredIfExistsValue : string.Empty;
                        info.CommonDiscInfo.Layer1MouldSID = options.AddPlaceholders ? Template.RequiredIfExistsValue : string.Empty;
                    }

                    info.Extras!.DiscKey = options.AddPlaceholders ? Template.RequiredValue : string.Empty;
                    info.Extras.BCA = info.Extras.BCA ?? (options.AddPlaceholders ? Template.RequiredValue : string.Empty);

                    break;

                case MediaType.UMD:
                    // Both single- and dual-layer discs have two "layers" for the ring
                    info.CommonDiscInfo!.Layer0MasteringRing = options.AddPlaceholders ? Template.RequiredIfExistsValue : string.Empty;
                    info.CommonDiscInfo.Layer0MasteringSID = options.AddPlaceholders ? Template.RequiredIfExistsValue : string.Empty;
                    info.CommonDiscInfo.Layer0ToolstampMasteringCode = options.AddPlaceholders ? Template.RequiredIfExistsValue : string.Empty;
                    info.CommonDiscInfo.Layer0MouldSID = options.AddPlaceholders ? Template.RequiredIfExistsValue : string.Empty;

                    info.CommonDiscInfo.Layer1MasteringRing = options.AddPlaceholders ? Template.RequiredIfExistsValue : string.Empty;
                    info.CommonDiscInfo.Layer1MasteringSID = options.AddPlaceholders ? Template.RequiredIfExistsValue : string.Empty;
                    info.CommonDiscInfo.Layer1ToolstampMasteringCode = options.AddPlaceholders ? Template.RequiredIfExistsValue : string.Empty;

                    info.SizeAndChecksums!.CRC32 ??= (options.AddPlaceholders ? Template.RequiredValue + " [Not automatically generated for UMD]" : string.Empty);
                    info.SizeAndChecksums.MD5 ??= (options.AddPlaceholders ? Template.RequiredValue + " [Not automatically generated for UMD]" : string.Empty);
                    info.SizeAndChecksums.SHA1 ??= (options.AddPlaceholders ? Template.RequiredValue + " [Not automatically generated for UMD]" : string.Empty);
                    info.TracksAndWriteOffsets.ClrMameProData = null;
                    break;
            }

            // Extract info based specifically on RedumpSystem
            switch (system)
            {
                case RedumpSystem.AcornArchimedes:
                    info.CommonDiscInfo!.Region ??= Region.UnitedKingdom;
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

                    info.CopyProtection!.Protection += protectionString;
                    info.CopyProtection.FullProtections = fullProtections as Dictionary<string, List<string>?> ?? [];
                    resultProgress?.Report(Result.Success("Copy protection scan complete!"));

                    break;

                case RedumpSystem.AudioCD:
                case RedumpSystem.DVDAudio:
                case RedumpSystem.SuperAudioCD:
                    info.CommonDiscInfo!.Category ??= DiscCategory.Audio;
                    break;

                case RedumpSystem.BandaiPlaydiaQuickInteractiveSystem:
                    info.CommonDiscInfo!.EXEDateBuildDate = options.AddPlaceholders ? Template.RequiredValue : string.Empty;
                    info.CommonDiscInfo.Region = info.CommonDiscInfo.Region ?? Region.Japan;
                    break;

                case RedumpSystem.BDVideo:
                    info.CommonDiscInfo!.Category ??= DiscCategory.BonusDiscs;
                    info.CopyProtection!.Protection = options.AddPlaceholders ? Template.RequiredIfExistsValue : string.Empty;
                    break;

                case RedumpSystem.CommodoreAmigaCD:
                    info.CommonDiscInfo!.EXEDateBuildDate = options.AddPlaceholders ? Template.RequiredValue : string.Empty;
                    break;

                case RedumpSystem.CommodoreAmigaCD32:
                    info.CommonDiscInfo!.EXEDateBuildDate = options.AddPlaceholders ? Template.RequiredValue : string.Empty;
                    info.CommonDiscInfo.Region ??= Region.Europe;
                    break;

                case RedumpSystem.CommodoreAmigaCDTV:
                    info.CommonDiscInfo!.EXEDateBuildDate = options.AddPlaceholders ? Template.RequiredValue : string.Empty;
                    info.CommonDiscInfo.Region ??= Region.Europe;
                    break;

                case RedumpSystem.DVDVideo:
                    info.CommonDiscInfo!.Category ??= DiscCategory.BonusDiscs;
                    break;

                case RedumpSystem.FujitsuFMTownsseries:
                    info.CommonDiscInfo!.EXEDateBuildDate = options.AddPlaceholders ? Template.RequiredValue : string.Empty;
                    info.CommonDiscInfo.Region = info.CommonDiscInfo.Region ?? Region.Japan;
                    break;

                case RedumpSystem.FujitsuFMTownsMarty:
                    info.CommonDiscInfo!.Region ??= Region.Japan;
                    break;

                case RedumpSystem.IncredibleTechnologiesEagle:
                    info.CommonDiscInfo!.EXEDateBuildDate = options.AddPlaceholders ? Template.RequiredValue : string.Empty;
                    break;

                case RedumpSystem.KonamieAmusement:
                    info.CommonDiscInfo!.EXEDateBuildDate = options.AddPlaceholders ? Template.RequiredValue : string.Empty;
                    break;

                case RedumpSystem.KonamiFireBeat:
                    info.CommonDiscInfo!.EXEDateBuildDate = options.AddPlaceholders ? Template.RequiredValue : string.Empty;
                    break;

                case RedumpSystem.KonamiSystemGV:
                    info.CommonDiscInfo!.EXEDateBuildDate = options.AddPlaceholders ? Template.RequiredValue : string.Empty;
                    break;

                case RedumpSystem.KonamiSystem573:
                    info.CommonDiscInfo!.EXEDateBuildDate = options.AddPlaceholders ? Template.RequiredValue : string.Empty;
                    break;

                case RedumpSystem.KonamiTwinkle:
                    info.CommonDiscInfo!.EXEDateBuildDate = options.AddPlaceholders ? Template.RequiredValue : string.Empty;
                    break;

                case RedumpSystem.MattelHyperScan:
                    info.CommonDiscInfo!.EXEDateBuildDate = options.AddPlaceholders ? Template.RequiredValue : string.Empty;
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
                    info.CommonDiscInfo!.EXEDateBuildDate = options.AddPlaceholders ? Template.RequiredValue : string.Empty;
                    break;

                case RedumpSystem.NavisoftNaviken21:
                    info.CommonDiscInfo!.EXEDateBuildDate = options.AddPlaceholders ? Template.RequiredValue : string.Empty;
                    info.CommonDiscInfo.Region ??= Region.Japan;
                    break;

                case RedumpSystem.NECPC88series:
                    info.CommonDiscInfo!.Region ??= Region.Japan;
                    break;

                case RedumpSystem.NECPC98series:
                    info.CommonDiscInfo!.EXEDateBuildDate = options.AddPlaceholders ? Template.RequiredValue : string.Empty;
                    info.CommonDiscInfo!.Region ??= Region.Japan;
                    break;

                case RedumpSystem.NECPCFXPCFXGA:
                    info.CommonDiscInfo!.Region ??= Region.Japan;
                    break;

                case RedumpSystem.SegaChihiro:
                    info.CommonDiscInfo!.EXEDateBuildDate = options.AddPlaceholders ? Template.RequiredValue : string.Empty;
                    break;

                case RedumpSystem.SegaDreamcast:
                    info.CommonDiscInfo!.EXEDateBuildDate = options.AddPlaceholders ? Template.RequiredValue : string.Empty;
                    break;

                case RedumpSystem.SegaNaomi:
                    info.CommonDiscInfo!.EXEDateBuildDate = options.AddPlaceholders ? Template.RequiredValue : string.Empty;
                    break;

                case RedumpSystem.SegaNaomi2:
                    info.CommonDiscInfo!.EXEDateBuildDate = options.AddPlaceholders ? Template.RequiredValue : string.Empty;
                    break;

                case RedumpSystem.SegaTitanVideo:
                    info.CommonDiscInfo!.EXEDateBuildDate = options.AddPlaceholders ? Template.RequiredValue : string.Empty;
                    break;

                case RedumpSystem.SharpX68000:
                    info.CommonDiscInfo!.Region ??= Region.Japan;
                    break;

                case RedumpSystem.SNKNeoGeoCD:
                    info.CommonDiscInfo!.EXEDateBuildDate = options.AddPlaceholders ? Template.RequiredValue : string.Empty;
                    break;

                case RedumpSystem.SonyPlayStation:
                    // Only check the disc if the dumping program couldn't detect
                    if (drive != null && info.CopyProtection!.AntiModchip == YesNo.NULL)
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
                    info.CommonDiscInfo!.LanguageSelection = [LanguageSelection.BiosSettings, LanguageSelection.LanguageSelector, LanguageSelection.OptionsMenu];
                    break;

                case RedumpSystem.SonyPlayStation3:
                    info.Extras!.DiscKey = options.AddPlaceholders ? Template.RequiredValue : string.Empty;
                    info.Extras.DiscID = options.AddPlaceholders ? Template.RequiredValue : string.Empty;
                    break;

                case RedumpSystem.TomyKissSite:
                    info.CommonDiscInfo!.Region ??= Region.Japan;
                    break;

                case RedumpSystem.ZAPiTGamesGameWaveFamilyEntertainmentSystem:
                    info.CopyProtection!.Protection = options.AddPlaceholders ? Template.RequiredIfExistsValue : string.Empty;
                    break;
            }

            // Set the category if it's not overriden
            info.CommonDiscInfo!.Category ??= DiscCategory.Games;

            // Comments and contents have odd handling
            if (string.IsNullOrEmpty(info.CommonDiscInfo.Comments))
                info.CommonDiscInfo.Comments = options.AddPlaceholders ? Template.OptionalValue : string.Empty;
            if (string.IsNullOrEmpty(info.CommonDiscInfo.Contents))
                info.CommonDiscInfo.Contents = options.AddPlaceholders ? Template.OptionalValue : string.Empty;

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
        public static bool FillFromRedump(Options options, SubmissionInfo info, IProgress<Result>? resultProgress = null)
#else
        public async static Task<bool> FillFromRedump(Options options, SubmissionInfo info, IProgress<Result>? resultProgress = null)
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
            List<int>? fullyMatchedIDs = null;

            // Loop through all of the hashdata to find matching IDs
            resultProgress?.Report(Result.Success("Finding disc matches on Redump..."));
            var splitData = info.TracksAndWriteOffsets?.ClrMameProData?.TrimEnd('\n')?.Split('\n');
            int trackCount = splitData?.Length ?? 0;
            foreach (string hashData in splitData ?? [])
            {
                // Catch any errant blank lines
                if (string.IsNullOrEmpty(hashData))
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

                // Get the SHA-1 hash
                if (!InfoTool.GetISOHashValues(hashData, out _, out _, out _, out string? sha1))
                {
                    resultProgress?.Report(Result.Failure($"Line could not be parsed: {hashData}"));
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
                    resultProgress?.Report(Result.Success(result));
                else
                    resultProgress?.Report(Result.Failure(result));

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
                    fullyMatchedIDs = [];
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
                    resultProgress?.Report(Result.Success(result));
                else
                    resultProgress?.Report(Result.Failure(result));

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
                    fullyMatchedIDs = [];
                }
            }

            // Make sure we only have unique IDs
            info.PartiallyMatchedIDs = [.. info.PartiallyMatchedIDs.Distinct().OrderBy(id => id)];

            resultProgress?.Report(Result.Success("Match finding complete! " + (fullyMatchedIDs != null && fullyMatchedIDs.Count > 0
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
                resultProgress?.Report(Result.Success($"Filling fields from existing ID {fullyMatchedIDs[i]}..."));
#if NET40
                var fillTask = Task.Factory.StartNew(() => Builder.FillFromId(wc, info, fullyMatchedIDs[i], options.PullAllInformation));
                fillTask.Wait();
                _ = fillTask.Result;
#else
                _ = await Builder.FillFromId(wc, info, fullyMatchedIDs[i], options.PullAllInformation);
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

            return true;
        }

        #endregion

        #region Helper Functions

        /// <summary>
        /// Formats a list of volume labels and their corresponding filesystems
        /// </summary>
        /// <param name="labels">Dictionary of volume labels and their filesystems</param>
        /// <returns>Formatted string of volume labels and their filesystems</returns>
        private static string? FormatVolumeLabels(string? driveLabel, Dictionary<string, List<string>>? labels)
        {

            // Must have at least one label to format
            if (driveLabel == null && (labels == null || labels.Count <= 0))
                return null;

            // If no labels given, use drive label
            if (labels == null || labels.Count <= 0)
            {
                // Ignore common volume labels
                if (Drive.GetRedumpSystemFromVolumeLabel(driveLabel) != null)
                    return null;

                return driveLabel;
            }

            // If only one label, don't mention fs
            string firstLabel = labels.FirstOrDefault().Key;
            if (labels.Count == 1 && (firstLabel == driveLabel || driveLabel == null))
            {
                // Ignore common volume labels
                if (Drive.GetRedumpSystemFromVolumeLabel(firstLabel) != null)
                    return null;

                return firstLabel;
            }

            // Otherwise, state filesystem for each label
            List<string> volLabels = [];
            
            // Begin formatted output with the label from Windows, if it is unique and not a common volume label
            if (driveLabel != null && !labels.TryGetValue(driveLabel, out List<string>? value) && Drive.GetRedumpSystemFromVolumeLabel(driveLabel) == null)
                volLabels.Add(driveLabel);

            // Add remaining labels with their corresponding filesystems
            foreach (KeyValuePair<string, List<string>> label in labels)
            {
                // Ignore common volume labels
                if (Drive.GetRedumpSystemFromVolumeLabel(label.Key) == null)
                    volLabels.Add($"{label.Key} ({string.Join(", ", [.. label.Value])})");
            }

            // Print each label separated by a comma and a space
            if (volLabels == null)
                return null;

            return string.Join(", ", [.. volLabels]);
        }

        #endregion
    }
}
