﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;
using SabreTools.Models.CueSheets;
using SabreTools.Models.Logiqx;
using SabreTools.RedumpLib;
using SabreTools.RedumpLib.Data;
using Schemas;

#pragma warning disable CS0618 // Ignore "Type or member is obsolete"
#pragma warning disable IDE0059 // Unnecessary assignment of a value

namespace MPF.Processors
{
    /// <summary>
    /// Represents processing Aaru outputs
    /// </summary>
    public sealed class Aaru : BaseProcessor
    {
        /// <inheritdoc/>
        public Aaru(RedumpSystem? system, MediaType? type) : base(system, type) { }

        #region BaseProcessor Implementations

        /// <inheritdoc/>
        public override void GenerateSubmissionInfo(SubmissionInfo info, string basePath, bool redumpCompat)
        {
            // TODO: Fill in submission info specifics for Aaru
            var outputDirectory = Path.GetDirectoryName(basePath);

            // Ensure that required sections exist
            info = Builder.EnsureAllSections(info);

            // TODO: Determine if there's an Aaru version anywhere
            info.DumpingInfo!.DumpingDate = ProcessingTool.GetFileModifiedDate(basePath + ".cicm.xml")?.ToString("yyyy-MM-dd HH:mm:ss");

            // Deserialize the sidecar, if possible
            var sidecar = GenerateSidecar(basePath + ".cicm.xml");

            // Fill in the hardware data
            if (GetHardwareInfo(sidecar, out var manufacturer, out var model, out var firmware))
            {
                info.DumpingInfo.Manufacturer = manufacturer;
                info.DumpingInfo.Model = model;
                info.DumpingInfo.Firmware = firmware;
            }

            // Fill in the disc type data
            if (GetDiscType(sidecar, out var discType, out var discSubType))
            {
                string fullDiscType = string.Empty;
                if (!string.IsNullOrEmpty(discType) && !string.IsNullOrEmpty(discSubType))
                    fullDiscType = $"{discType} ({discSubType})";
                else if (!string.IsNullOrEmpty(discType) && string.IsNullOrEmpty(discSubType))
                    fullDiscType = discType!;
                else if (string.IsNullOrEmpty(discType) && !string.IsNullOrEmpty(discSubType))
                    fullDiscType = discSubType!;

                info.DumpingInfo.ReportedDiscType = fullDiscType;
            }

            // Get the Datafile information
            var datafile = GenerateDatafile(sidecar, basePath);

            // Fill in the hash data
            info.TracksAndWriteOffsets!.ClrMameProData = ProcessingTool.GenerateDatfile(datafile);

            switch (Type)
            {
                // TODO: Can this do GD-ROM?
                case MediaType.CDROM:
                    // TODO: Re-enable once PVD generation / finding is fixed
                    // Generate / obtain the PVD
                    //info.Extras.PVD = GeneratePVD(sidecar) ?? "Disc has no PVD";

                    long errorCount = -1;
                    if (File.Exists(basePath + ".resume.xml"))
                        errorCount = GetErrorCount(basePath + ".resume.xml");

                    info.CommonDiscInfo!.ErrorsCount = (errorCount == -1 ? "Error retrieving error count" : errorCount.ToString());

                    info.TracksAndWriteOffsets.Cuesheet = GenerateCuesheet(sidecar, basePath) ?? string.Empty;

                    string cdWriteOffset = GetWriteOffset(sidecar) ?? string.Empty;
                    info.CommonDiscInfo.RingWriteOffset = cdWriteOffset;
                    info.TracksAndWriteOffsets.OtherWriteOffsets = cdWriteOffset;
                    break;

                case MediaType.DVD:
                case MediaType.HDDVD:
                case MediaType.BluRay:

                    // Get the individual hash data, as per internal
                    if (ProcessingTool.GetISOHashValues(datafile, out long size, out var crc32, out var md5, out var sha1))
                    {
                        info.SizeAndChecksums!.CRC32 = crc32;
                        info.SizeAndChecksums.CRC32 = crc32;
                        info.SizeAndChecksums.MD5 = md5;
                        info.SizeAndChecksums.SHA1 = sha1;
                    }

                    // TODO: Re-enable once PVD generation / finding is fixed
                    // Generate / obtain the PVD
                    //info.Extras.PVD = GeneratePVD(sidecar) ?? "Disc has no PVD";

                    // Deal with the layerbreak
                    string? layerbreak = null;
                    if (Type == MediaType.DVD)
                        layerbreak = GetLayerbreak(sidecar) ?? string.Empty;
                    else if (Type == MediaType.BluRay)
                        layerbreak = info.SizeAndChecksums!.Size > 25_025_314_816 ? "25025314816" : null;

                    // If we have a single-layer disc
                    if (string.IsNullOrEmpty(layerbreak))
                    {
                        // Currently no-op
                    }
                    // If we have a dual-layer disc
                    else
                    {
                        info.SizeAndChecksums!.Layerbreak = Int64.Parse(layerbreak);
                    }

                    // TODO: Investigate XGD disc outputs
                    // TODO: Investigate BD specifics like PIC

                    break;
            }

            switch (System)
            {
                // TODO: Can we get SecuROM data?
                // TODO: Can we get SS version/ranges?
                // TODO: Can we get DMI info?
                // TODO: Can we get Sega Header info?
                // TODO: Can we get PS1 EDC status?
                // TODO: Can we get PS1 LibCrypt status?

                case RedumpSystem.DVDAudio:
                case RedumpSystem.DVDVideo:
                    info.CopyProtection!.Protection = GetDVDProtection(sidecar) ?? string.Empty;
                    break;

                case RedumpSystem.MicrosoftXbox:
                    if (GetXgdAuxInfo(sidecar, out var xgd1DMIHash, out var xgd1PFIHash, out var xgd1SSHash, out var ss, out var xgd1SSVer))
                    {
                        info.CommonDiscInfo!.CommentsSpecialFields![SiteCode.DMIHash] = xgd1DMIHash ?? string.Empty;
                        info.CommonDiscInfo.CommentsSpecialFields[SiteCode.PFIHash] = xgd1PFIHash ?? string.Empty;
                        info.CommonDiscInfo.CommentsSpecialFields[SiteCode.SSHash] = xgd1SSHash ?? string.Empty;
                        info.CommonDiscInfo.CommentsSpecialFields[SiteCode.SSVersion] = xgd1SSVer ?? string.Empty;
                        info.Extras!.SecuritySectorRanges = ss ?? string.Empty;
                    }

                    if (GetXboxDMIInfo(sidecar, out var serial, out var version, out Region? region))
                    {
                        info.CommonDiscInfo!.Serial = serial ?? string.Empty;
                        info.VersionAndEditions!.Version = version ?? string.Empty;
                        info.CommonDiscInfo.Region = region;
                    }

                    break;

                case RedumpSystem.MicrosoftXbox360:
                    if (GetXgdAuxInfo(sidecar, out var xgd23DMIHash, out var xgd23PFIHash, out var xgd23SSHash, out var ss360, out var xgd23SSVer))
                    {
                        info.CommonDiscInfo!.CommentsSpecialFields![SiteCode.DMIHash] = xgd23DMIHash ?? string.Empty;
                        info.CommonDiscInfo.CommentsSpecialFields[SiteCode.PFIHash] = xgd23PFIHash ?? string.Empty;
                        info.CommonDiscInfo.CommentsSpecialFields[SiteCode.SSHash] = xgd23SSHash ?? string.Empty;
                        info.CommonDiscInfo.CommentsSpecialFields[SiteCode.SSVersion] = xgd23SSVer ?? string.Empty;
                        info.Extras!.SecuritySectorRanges = ss360 ?? string.Empty;
                    }

                    if (GetXbox360DMIInfo(sidecar, out var serial360, out var version360, out Region? region360))
                    {
                        info.CommonDiscInfo!.Serial = serial360 ?? string.Empty;
                        info.VersionAndEditions!.Version = version360 ?? string.Empty;
                        info.CommonDiscInfo.Region = region360;
                    }
                    break;
            }
        }

        /// <inheritdoc/>
        internal override List<OutputFile> GetOutputFiles(string? baseDirectory, string baseFilename)
        {
            switch (Type)
            {
                case MediaType.CDROM:
                    return [
                        new($"{baseFilename}.aaruf", OutputFileFlags.Required),
                        new($"{baseFilename}.cicm.xml", OutputFileFlags.Required
                            | OutputFileFlags.Artifact
                            | OutputFileFlags.Zippable,
                            "cicm"),
                        new($"{baseFilename}.error.log", OutputFileFlags.Artifact
                            | OutputFileFlags.Zippable,
                            "error_log"),
                        new($"{baseFilename}.ibg", OutputFileFlags.Required
                            | OutputFileFlags.Binary
                            | OutputFileFlags.Zippable,
                            "ibg"),
                        new($"{baseFilename}.log", OutputFileFlags.Required
                            | OutputFileFlags.Artifact
                            | OutputFileFlags.Zippable,
                            "log"),
                        new($"{baseFilename}.mhddlog.bin", OutputFileFlags.Required
                            | OutputFileFlags.Binary
                            | OutputFileFlags.Zippable,
                            "mhddlog"),
                        new($"{baseFilename}.resume.xml", OutputFileFlags.Required
                            | OutputFileFlags.Artifact
                            | OutputFileFlags.Zippable,
                            "resume"),
                        new($"{baseFilename}.sub.log", OutputFileFlags.Required
                            | OutputFileFlags.Artifact
                            | OutputFileFlags.Zippable,
                            "sub_log"),
                    ];

                case MediaType.DVD:
                case MediaType.HDDVD:
                case MediaType.BluRay:
                    return [
                        new($"{baseFilename}.aaruf", OutputFileFlags.Required),
                        new($"{baseFilename}.cicm.xml", OutputFileFlags.Required
                            | OutputFileFlags.Artifact
                            | OutputFileFlags.Zippable,
                            "cicm"),
                        new($"{baseFilename}.error.log", OutputFileFlags.Artifact
                            | OutputFileFlags.Zippable,
                            "error_log"),
                        new($"{baseFilename}.ibg", OutputFileFlags.Required
                            | OutputFileFlags.Binary
                            | OutputFileFlags.Zippable,
                            "ibg"),
                        new($"{baseFilename}.log", OutputFileFlags.Required
                            | OutputFileFlags.Artifact
                            | OutputFileFlags.Zippable,
                            "log"),
                        new($"{baseFilename}.mhddlog.bin", OutputFileFlags.Required
                            | OutputFileFlags.Binary
                            | OutputFileFlags.Zippable,
                            "mhddlog"),
                        new($"{baseFilename}.resume.xml", OutputFileFlags.Required
                            | OutputFileFlags.Artifact
                            | OutputFileFlags.Zippable,
                            "resume"),
                    ];
            }

            return [];
        }

        #endregion

        #region Information Extraction Methods

        /// <summary>
        /// Convert the TrackTypeTrackType value to a CueTrackDataType
        /// </summary>
        /// <param name="trackType">TrackTypeTrackType to convert</param>
        /// <param name="bytesPerSector">Sector size to help with specific subtypes</param>
        /// <returns>CueTrackDataType representing the input data</returns>
        private static CueTrackDataType ConvertToDataType(TrackTypeTrackType trackType, uint bytesPerSector)
        {
            switch (trackType)
            {
                case TrackTypeTrackType.audio:
                    return CueTrackDataType.AUDIO;

                case TrackTypeTrackType.mode1:
                    if (bytesPerSector == 2048)
                        return CueTrackDataType.MODE1_2048;
                    else
                        return CueTrackDataType.MODE1_2352;

                case TrackTypeTrackType.mode2:
                case TrackTypeTrackType.m2f1:
                case TrackTypeTrackType.m2f2:
                    if (bytesPerSector == 2336)
                        return CueTrackDataType.MODE2_2336;
                    else
                        return CueTrackDataType.MODE2_2352;

                default:
                    return CueTrackDataType.MODE1_2352;
            }
        }

        /// <summary>
        /// Convert the TrackFlagsType value to a CueTrackFlag
        /// </summary>
        /// <param name="trackFlagsType">TrackFlagsType containing flag data</param>
        /// <returns>CueTrackFlag representing the flags</returns>
        private static CueTrackFlag ConvertToTrackFlag(TrackFlagsType trackFlagsType)
        {
            if (trackFlagsType == null)
                return 0;

            CueTrackFlag flag = 0;

            if (trackFlagsType.CopyPermitted)
                flag |= CueTrackFlag.DCP;

            if (trackFlagsType.Quadraphonic)
                flag |= CueTrackFlag.FourCH;

            if (trackFlagsType.PreEmphasis)
                flag |= CueTrackFlag.PRE;

            return flag;
        }

        /// <summary>
        /// Generate a cuesheet string based on CICM sidecar data
        /// </summary>
        /// <param name="cicmSidecar">CICM Sidecar data generated by Aaru</param>
        /// <param name="basePath">Base path for determining file names</param>
        /// <returns>String containing the cuesheet, null on error</returns>
        private static string? GenerateCuesheet(CICMMetadataType? cicmSidecar, string basePath)
        {
            // If the object is null, we can't get information from it
            if (cicmSidecar == null)
                return null;

            // Required variables
            uint totalTracks = 0;
            var cueFiles = new List<CueFile>();
            var cueSheet = new CueSheet
            {
                Performer = string.Join(", ", cicmSidecar.Performer ?? []),
            };

            // Only care about OpticalDisc types
            if (cicmSidecar.OpticalDisc == null || cicmSidecar.OpticalDisc.Length == 0)
                return null;

            // Loop through each OpticalDisc in the metadata
            foreach (OpticalDiscType opticalDisc in cicmSidecar.OpticalDisc)
            {
                // Only capture the first total track count
                if (opticalDisc.Tracks != null && opticalDisc.Tracks.Length > 0)
                    totalTracks = opticalDisc.Tracks[0];

                // If there are no tracks, we can't get a cuesheet
                if (opticalDisc.Track == null || opticalDisc.Track.Length == 0)
                    continue;

                // Get cuesheet-level information
                cueSheet.Catalog = opticalDisc.MediaCatalogueNumber;

                // Loop through each track
                foreach (TrackType track in opticalDisc.Track)
                {
                    // Create cue track entry
                    var cueTrack = new CueTrack
                    {
                        Number = (int)(track.Sequence?.TrackNumber ?? 0),
                        DataType = ConvertToDataType(track.TrackType1, track.BytesPerSector),
                        Flags = ConvertToTrackFlag(track.Flags),
                        ISRC = track.ISRC,
                    };

                    // Create cue file entry
                    var cueFile = new CueFile
                    {
                        FileName = GenerateTrackName(basePath, (int)totalTracks, cueTrack.Number, opticalDisc.DiscType),
                        FileType = CueFileType.BINARY,
                    };

                    // Add index data
                    var cueTracks = new List<CueTrack>();
                    if (track.Indexes != null && track.Indexes.Length > 0)
                    {
                        var cueIndicies = new List<CueIndex>();

                        // Loop through each index
                        foreach (TrackIndexType trackIndex in track.Indexes)
                        {
                            // Get timestamp from frame count
                            int absoluteLength = Math.Abs(trackIndex.Value);
                            int frames = absoluteLength % 75;
                            int seconds = (absoluteLength / 75) % 60;
                            int minutes = (absoluteLength / 75 / 60);
                            string timeString = $"{minutes:D2}:{seconds:D2}:{frames:D2}";

                            // Pregap information
                            if (trackIndex.Value < 0)
                            {
                                string[] timeStringSplit = timeString.Split(':');
                                cueTrack.PreGap = new PreGap
                                {
                                    Minutes = int.Parse(timeStringSplit[0]),
                                    Seconds = int.Parse(timeStringSplit[1]),
                                    Frames = int.Parse(timeStringSplit[2]),
                                };
                            }

                            // Individual indexes
                            else
                            {
                                string[] timeStringSplit = timeString.Split(':');
                                cueIndicies.Add(new CueIndex
                                {
                                    Index = trackIndex.index,
                                    Minutes = int.Parse(timeStringSplit[0]),
                                    Seconds = int.Parse(timeStringSplit[1]),
                                    Frames = int.Parse(timeStringSplit[2]),
                                });
                            }
                        }

                        cueTrack.Indices = [.. cueIndicies];
                    }
                    else
                    {
                        // Default if index data missing from sidecar
                        cueTrack.Indices = new CueIndex[]
                        {
                            new()
                            {
                                Index = 1,
                                Minutes = 0,
                                Seconds = 0,
                                Frames = 0,
                            },
                        };
                    }

                    // Add the track to the file
                    cueTracks.Add(cueTrack);
                    cueFile.Tracks = [.. cueTracks];

                    // Add the file to the cuesheet
                    cueFiles.Add(cueFile);
                }
            }

            // If we have a cuesheet to write out, do so
            cueSheet.Files = [.. cueFiles];
            if (cueSheet != null && cueSheet != default)
            {
                var ms = SabreTools.Serialization.Serializers.CueSheet.SerializeStream(cueSheet);
                if (ms == null)
                    return null;

                using var sr = new StreamReader(ms);
                return sr.ReadToEnd();
            }

            return null;
        }

        /// <summary>
        /// Generate a CMP XML datfile string based on CICM sidecar data
        /// </summary>
        /// <param name="cicmSidecar">CICM Sidecar data generated by Aaru</param>
        /// <param name="basePath">Base path for determining file names</param>
        /// <returns>Datafile containing the hash information, null on error</returns>
        private static Datafile? GenerateDatafile(CICMMetadataType? cicmSidecar, string basePath)
        {
            // If the object is null, we can't get information from it
            if (cicmSidecar == null)
                return null;

            // Required variables
            var datafile = new Datafile();
            var roms = new List<Rom>();

            // Process OpticalDisc, if possible
            if (cicmSidecar.OpticalDisc != null && cicmSidecar.OpticalDisc.Length > 0)
            {
                // Loop through each OpticalDisc in the metadata
                foreach (OpticalDiscType opticalDisc in cicmSidecar.OpticalDisc)
                {
                    // Only capture the first total track count
                    uint totalTracks = 0;
                    if (opticalDisc.Tracks != null && opticalDisc.Tracks.Length > 0)
                        totalTracks = opticalDisc.Tracks[0];

                    // If there are no tracks, we can't get a datfile
                    if (opticalDisc.Track == null || opticalDisc.Track.Length == 0)
                        continue;

                    // Loop through each track
                    foreach (TrackType track in opticalDisc.Track)
                    {
                        uint trackNumber = track.Sequence?.TrackNumber ?? 0;
                        ulong size = track.Size;
                        string crc32 = string.Empty;
                        string md5 = string.Empty;
                        string sha1 = string.Empty;

                        // If we don't have any checksums, we can't get a DAT for this track
                        if (track.Checksums == null || track.Checksums.Length == 0)
                            continue;

                        // Extract only relevant checksums
                        foreach (ChecksumType checksum in track.Checksums)
                        {
                            switch (checksum.type)
                            {
                                case ChecksumTypeType.crc32:
                                    crc32 = checksum.Value;
                                    break;
                                case ChecksumTypeType.md5:
                                    md5 = checksum.Value;
                                    break;
                                case ChecksumTypeType.sha1:
                                    sha1 = checksum.Value;
                                    break;
                            }
                        }

                        // Build the track datfile data and append
                        string trackName = GenerateTrackName(basePath, (int)totalTracks, (int)trackNumber, opticalDisc.DiscType);
                        roms.Add(new Rom { Name = trackName, Size = size.ToString(), CRC = crc32, MD5 = md5, SHA1 = sha1 });
                    }
                }
            }

            // Process BlockMedia, if possible
            if (cicmSidecar.BlockMedia != null && cicmSidecar.BlockMedia.Length > 0)
            {
                // Loop through each BlockMedia in the metadata
                foreach (BlockMediaType blockMedia in cicmSidecar.BlockMedia)
                {
                    ulong size = blockMedia.Size;
                    string crc32 = string.Empty;
                    string md5 = string.Empty;
                    string sha1 = string.Empty;

                    // If we don't have any checksums, we can't get a DAT for this track
                    if (blockMedia.Checksums == null || blockMedia.Checksums.Length == 0)
                        continue;

                    // Extract only relevant checksums
                    foreach (ChecksumType checksum in blockMedia.Checksums)
                    {
                        switch (checksum.type)
                        {
                            case ChecksumTypeType.crc32:
                                crc32 = checksum.Value;
                                break;
                            case ChecksumTypeType.md5:
                                md5 = checksum.Value;
                                break;
                            case ChecksumTypeType.sha1:
                                sha1 = checksum.Value;
                                break;
                        }
                    }

                    // Build the track datfile data and append
                    string trackName = $"{basePath}.bin";
                    roms.Add(new Rom { Name = trackName, Size = size.ToString(), CRC = crc32, MD5 = md5, SHA1 = sha1 });
                }
            }

            // Assign the roms to a new game
            datafile.Game = new Game[1];
            datafile.Game[0] = new Game { Rom = [.. roms] };

            return datafile;
        }

        /// <summary>
        /// Generate a track name based on current path and tracks
        /// </summary>
        /// <param name="basePath">Base path for determining file names</param>
        /// <param name="totalTracks">Total number of tracks in the media</param>
        /// <param name="trackNumber">Current track index</param>
        /// <param name="discType">Current disc type, used for determining extension</param>
        /// <returns>Formatted string representing the track name according to Redump standards</returns>
        private static string GenerateTrackName(string basePath, int totalTracks, int trackNumber, string discType)
        {
            string extension = "bin";
            if (discType.Contains("BD") || discType.Contains("DVD"))
                extension = "iso";

            string trackName = Path.GetFileNameWithoutExtension(basePath);
            if (totalTracks == 1)
                trackName = $"{trackName}.{extension}";
            else if (totalTracks > 1 && totalTracks < 10)
                trackName = $"{trackName} (Track {trackNumber}).{extension}";
            else
                trackName = $"{trackName} (Track {trackNumber:D2}).{extension}";

            return trackName;
        }

        /// <summary>
        /// Generate a Redump-compatible PVD block based on CICM sidecar file
        /// </summary>
        /// <param name="cicmSidecar">CICM Sidecar data generated by Aaru</param>
        /// <returns>String containing the PVD, null on error</returns>
        private static string? GeneratePVD(CICMMetadataType? cicmSidecar)
        {
            // If the object is null, we can't get information from it
            if (cicmSidecar == null)
                return null;

            // Process OpticalDisc, if possible
            if (cicmSidecar.OpticalDisc != null && cicmSidecar.OpticalDisc.Length > 0)
            {
                // Loop through each OpticalDisc in the metadata
                foreach (OpticalDiscType opticalDisc in cicmSidecar.OpticalDisc)
                {
                    var pvdData = GeneratePVDData(opticalDisc);

                    // If we got a null value, we skip this disc
                    if (pvdData == null)
                        continue;

                    // Build each row in consecutive order
                    string pvd = string.Empty;
#if NET20 || NET35 || NET40 || NET452
                    byte[] pvdLine = new byte[16];
                    Array.Copy(pvdData, 0, pvdLine, 0, 16);
                    pvd += GenerateSectorOutputLine("0320", pvdLine);
                    Array.Copy(pvdData, 16, pvdLine, 0, 16);
                    pvd += GenerateSectorOutputLine("0330", pvdLine);
                    Array.Copy(pvdData, 32, pvdLine, 0, 16);
                    pvd += GenerateSectorOutputLine("0340", pvdLine);
                    Array.Copy(pvdData, 48, pvdLine, 0, 16);
                    pvd += GenerateSectorOutputLine("0350", pvdLine);
                    Array.Copy(pvdData, 64, pvdLine, 0, 16);
                    pvd += GenerateSectorOutputLine("0360", pvdLine);
                    Array.Copy(pvdData, 80, pvdLine, 0, 16);
                    pvd += GenerateSectorOutputLine("0370", pvdLine);
#else
                    pvd += GenerateSectorOutputLine("0320", new ReadOnlySpan<byte>(pvdData, 0, 16).ToArray());
                    pvd += GenerateSectorOutputLine("0330", new ReadOnlySpan<byte>(pvdData, 16, 16).ToArray());
                    pvd += GenerateSectorOutputLine("0340", new ReadOnlySpan<byte>(pvdData, 32, 16).ToArray());
                    pvd += GenerateSectorOutputLine("0350", new ReadOnlySpan<byte>(pvdData, 48, 16).ToArray());
                    pvd += GenerateSectorOutputLine("0360", new ReadOnlySpan<byte>(pvdData, 64, 16).ToArray());
                    pvd += GenerateSectorOutputLine("0370", new ReadOnlySpan<byte>(pvdData, 80, 16).ToArray());
#endif

                    return pvd;
                }
            }

            return null;
        }

        /// <summary>
        /// Generate the byte array representing the current PVD information
        /// </summary>
        /// <param name="opticalDisc">OpticalDisc type from CICM Sidecar data</param>
        /// <returns>Byte array representing the PVD, null on error</returns>
        private static byte[]? GeneratePVDData(OpticalDiscType? opticalDisc)
        {
            // Required variables
            DateTime creation = DateTime.MinValue;
            DateTime modification = DateTime.MinValue;
            DateTime expiration = DateTime.MinValue;
            DateTime effective = DateTime.MinValue;

            // If there are no tracks, we can't get a PVD
            if (opticalDisc?.Track == null || opticalDisc.Track.Length == 0)
                return null;

            // Take the first track only
            TrackType track = opticalDisc.Track[0];

            // If there are no partitions, we can't get a PVD
            if (track.FileSystemInformation == null || track.FileSystemInformation.Length == 0)
                return null;

            // Loop through each Partition
            foreach (PartitionType partition in track.FileSystemInformation)
            {
                // If the partition has no file systems, we can't get a PVD
                if (partition.FileSystems == null || partition.FileSystems.Length == 0)
                    continue;

                // Loop through each FileSystem until we find a PVD
                foreach (FileSystemType fileSystem in partition.FileSystems)
                {
                    // If we don't have a PVD-able filesystem, we can't get a PVD
                    if (!fileSystem.CreationDateSpecified
                        && !fileSystem.ModificationDateSpecified
                        && !fileSystem.ExpirationDateSpecified
                        && !fileSystem.EffectiveDateSpecified)
                    {
                        continue;
                    }

                    // Creation Date
                    if (fileSystem.CreationDateSpecified)
                        creation = fileSystem.CreationDate;

                    // Modification Date
                    if (fileSystem.ModificationDateSpecified)
                        modification = fileSystem.ModificationDate;

                    // Expiration Date
                    if (fileSystem.ExpirationDateSpecified)
                        expiration = fileSystem.ExpirationDate;

                    // Effective Date
                    if (fileSystem.EffectiveDateSpecified)
                        effective = fileSystem.EffectiveDate;

                    break;
                }

                // If we found a Partition with PVD data, we break
                if (creation != DateTime.MinValue
                    || modification != DateTime.MinValue
                    || expiration != DateTime.MinValue
                    || effective != DateTime.MinValue)
                {
                    break;
                }
            }

            // If we found no partitions, we return null
            if (creation == DateTime.MinValue
                && modification == DateTime.MinValue
                && expiration == DateTime.MinValue
                && effective == DateTime.MinValue)
            {
                return null;
            }

            // Now generate the byte array data
            var pvdData = new List<byte>();
            pvdData.AddRange(Array.ConvertAll(new string(' ', 13).ToCharArray(), c => (byte)c));
            pvdData.AddRange(GeneratePVDDateTimeBytes(creation));
            pvdData.AddRange(GeneratePVDDateTimeBytes(modification));
            pvdData.AddRange(GeneratePVDDateTimeBytes(expiration));
            pvdData.AddRange(GeneratePVDDateTimeBytes(effective));
            pvdData.Add(0x01);
            pvdData.AddRange(Array.ConvertAll(new string((char)0, 14).ToCharArray(), c => (byte)c));

            // Return the filled array
            return [.. pvdData];
        }

        /// <summary>
        /// Generate the required bytes from a DateTime object
        /// </summary>
        /// <param name="dateTime">DateTime to get representation of</param>
        /// <returns>Byte array representing the DateTime</returns>
        private static byte[] GeneratePVDDateTimeBytes(DateTime dateTime)
        {
            string emptyTime = "0000000000000000";
            string dateTimeString = emptyTime;
            byte timeZoneNumber = 0;

            // If we don't have default values, set the proper string
            if (dateTime != DateTime.MinValue)
            {
                dateTimeString = dateTime.ToString("yyyyMMddHHmmssff");

                // Get timezone offset (0 == GMT, up and down in 15-minute increments)
                string timeZoneString;
                try
                {
                    timeZoneString = dateTime.ToString("zzz");
                }
                catch
                {
                    timeZoneString = "00:00";
                }

                // Format is hh:mm
                string[] splitTimeZoneString = timeZoneString.Split(':');
                if (int.TryParse(splitTimeZoneString[0], out int hours))
                    timeZoneNumber += (byte)(hours * 4);
                if (int.TryParse(splitTimeZoneString[1], out int minutes))
                    timeZoneNumber += (byte)(minutes / 15);
            }

            // Get and return the byte array
            List<byte> dateTimeList = [.. Array.ConvertAll(dateTimeString.ToCharArray(), c => (byte)c)];
            dateTimeList.Add(timeZoneNumber);
            return [.. dateTimeList];
        }

        /// <summary>
        /// Generate a single 16-byte sector line from a byte array
        /// </summary>
        /// <param name="row">Row ID for outputting</param>
        /// <param name="bytes">Bytes representing the data to write</param>
        /// <returns>Formatted string representing the sector line</returns>
        private static string? GenerateSectorOutputLine(string row, byte[] bytes)
        {
            // If the data isn't correct, return null
            if (bytes == null || bytes.Length != 16)
                return null;

            string pvdLine = $"{row} : ";
            pvdLine += BitConverter.ToString(bytes, 0, 8).Replace("-", " ");
            pvdLine += "  ";
            pvdLine += BitConverter.ToString(bytes, 8, 8).Replace("-", " ");
            pvdLine += "   ";
            pvdLine += Encoding.ASCII.GetString(bytes).Replace((char)0, '.').Replace('?', '.');
            pvdLine += "\n";

            return pvdLine;
        }

        /// <summary>
        /// Read the CICM Sidecar as an object
        /// </summary>
        /// <param name="cicmSidecar">CICM Sidecar data generated by Aaru</param>
        /// <returns>Object containing the data, null on error</returns>
        private static CICMMetadataType? GenerateSidecar(string cicmSidecar)
        {
            // If the file doesn't exist, we can't get info from it
            if (!File.Exists(cicmSidecar))
                return null;

            // Open and read in the XML file
            XmlReader xtr = XmlReader.Create(cicmSidecar, new XmlReaderSettings
            {
                CheckCharacters = false,
#if NET40_OR_GREATER || NETCOREAPP
                DtdProcessing = DtdProcessing.Ignore,
#endif
                IgnoreComments = true,
                IgnoreWhitespace = true,
                ValidationFlags = XmlSchemaValidationFlags.None,
                ValidationType = ValidationType.None,
            });

            // If the reader is null for some reason, we can't do anything
            if (xtr == null)
                return null;

            var serializer = new XmlSerializer(typeof(CICMMetadataType));
            return serializer.Deserialize(xtr) as CICMMetadataType;
        }

        /// <summary>
        /// Get reported disc type information, if possible
        /// </summary>
        /// <param name="cicmSidecar">CICM Sidecar data generated by Aaru</param>
        /// <returns>True if disc type info was set, false otherwise</returns>
        private static bool GetDiscType(CICMMetadataType? cicmSidecar, out string? discType, out string? discSubType)
        {
            // Set the default values
            discType = null; discSubType = null;

            // If the object is null, we can't get information from it
            if (cicmSidecar == null)
                return false;

            // Only care about OpticalDisc types
            if (cicmSidecar.OpticalDisc == null || cicmSidecar.OpticalDisc.Length == 0)
                return false;

            // Find and return the hardware info, if possible
            foreach (OpticalDiscType opticalDisc in cicmSidecar.OpticalDisc)
            {
                // Store the first instance of each value
                if (string.IsNullOrEmpty(discType) && !string.IsNullOrEmpty(opticalDisc.DiscType))
                    discType = opticalDisc.DiscType;
                if (string.IsNullOrEmpty(discSubType) && !string.IsNullOrEmpty(opticalDisc.DiscSubType))
                    discSubType = opticalDisc.DiscSubType;
            }

            return !string.IsNullOrEmpty(discType) || !string.IsNullOrEmpty(discSubType);
        }

        /// <summary>
        /// Get the DVD protection information, if possible
        /// </summary>
        /// <param name="cicmSidecar">CICM Sidecar data generated by Aaru</param>
        /// <returns>Formatted string representing the DVD protection, null on error</returns>
        private static string? GetDVDProtection(CICMMetadataType? cicmSidecar)
        {
            // If the object is null, we can't get information from it
            if (cicmSidecar == null)
                return null;

            // Only care about OpticalDisc types
            if (cicmSidecar.OpticalDisc == null || cicmSidecar.OpticalDisc.Length == 0)
                return null;

            // Get an output for the copyright protection
            string copyrightProtectionSystemType = string.Empty;

            // Loop through each OpticalDisc in the metadata
            foreach (OpticalDiscType opticalDisc in cicmSidecar.OpticalDisc)
            {
                if (!string.IsNullOrEmpty(opticalDisc.CopyProtection))
                    copyrightProtectionSystemType += $", {opticalDisc.CopyProtection}";
            }

            // Trim the values
            copyrightProtectionSystemType = copyrightProtectionSystemType.TrimStart(',').Trim();

            // TODO: Note- Most of the below values are not currently captured by Aaru.
            // At the time of writing, there are open issues to capture more of this
            // information and store it in the output. For now, only the copyright
            // protection system can be retrieved.

            // Now we format everything we can
            string protection = string.Empty;
            //if (!string.IsNullOrEmpty(region))
            //    protection += $"Region: {region}\n";
            //if (!string.IsNullOrEmpty(rceProtection))
            //    protection += $"RCE Protection: {rceProtection}\n";
            if (!string.IsNullOrEmpty(copyrightProtectionSystemType))
                protection += $"Copyright Protection System Type: {copyrightProtectionSystemType}\n";
            //if (!string.IsNullOrEmpty(vobKeys))
            //    protection += vobKeys;
            //if (!string.IsNullOrEmpty(decryptedDiscKey))
            //    protection += $"Decrypted Disc Key: {decryptedDiscKey}\n";

            return protection;
        }

        /// <summary>
        /// Get the detected error count from the input files, if possible
        /// </summary>
        /// <param name="resume">.resume.xml file location</param>
        /// <returns>Error count if possible, -1 on error</returns>
        private static long GetErrorCount(string resume)
        {
            // If the file doesn't exist, we can't get info from it
            if (!File.Exists(resume))
                return -1;

            // Get a total error count for after
            long? totalErrors = null;

            // Parse the resume XML file
            try
            {
                // Read in the error count whenever we find it
                using var sr = File.OpenText(resume);
                while (!sr.EndOfStream)
                {
                    var line = sr.ReadLine()?.Trim();

                    // Initialize on seeing the open tag
                    if (string.IsNullOrEmpty(line))
                        continue;
                    else if (line!.StartsWith("<BadBlocks>"))
                        totalErrors = 0;
                    else if (line.StartsWith("</BadBlocks>"))
                        return totalErrors ?? -1;
                    else if (line.StartsWith("<Block>") && totalErrors != null)
                        totalErrors++;
                }

                // If we haven't found anything, return -1
                return totalErrors ?? -1;
            }
            catch
            {
                // We don't care what the exception is right now
                return Int64.MaxValue;
            }
        }

        /// <summary>
        /// Get hardware information, if possible
        /// </summary>
        /// <param name="cicmSidecar">CICM Sidecar data generated by Aaru</param>
        /// <returns>True if hardware info was set, false otherwise</returns>
        private static bool GetHardwareInfo(CICMMetadataType? cicmSidecar, out string? manufacturer, out string? model, out string? firmware)
        {
            // Set the default values
            manufacturer = null; model = null; firmware = null;

            // If the object is null, we can't get information from it
            if (cicmSidecar == null)
                return false;

            // Only care about OpticalDisc types
            if (cicmSidecar.OpticalDisc == null || cicmSidecar.OpticalDisc.Length == 0)
                return false;

            // Find and return the hardware info, if possible
            foreach (OpticalDiscType opticalDisc in cicmSidecar.OpticalDisc)
            {
                // If there's no hardware information, skip
                if (opticalDisc.DumpHardwareArray == null || opticalDisc.DumpHardwareArray.Length == 0)
                    continue;

                foreach (DumpHardwareType hardware in opticalDisc.DumpHardwareArray)
                {
                    // If the hardware information is invalid, skip
                    if (hardware == null)
                        continue;

                    // Store the first instance of each value
                    if (string.IsNullOrEmpty(manufacturer) && !string.IsNullOrEmpty(hardware.Manufacturer))
                        manufacturer = hardware.Manufacturer;
                    if (string.IsNullOrEmpty(model) && !string.IsNullOrEmpty(hardware.Model))
                        model = hardware.Model;
                    if (string.IsNullOrEmpty(firmware) && !string.IsNullOrEmpty(hardware.Firmware))
                        firmware = hardware.Firmware;
                }
            }

            return !string.IsNullOrEmpty(manufacturer) || !string.IsNullOrEmpty(model) || !string.IsNullOrEmpty(firmware);
        }

        /// <summary>
        /// Get the layerbreak from the input file, if possible
        /// </summary>
        /// <param name="cicmSidecar">CICM Sidecar data generated by Aaru</param>
        /// <returns>Layerbreak if possible, null on error</returns>
        private static string? GetLayerbreak(CICMMetadataType? cicmSidecar)
        {
            // If the object is null, we can't get information from it
            if (cicmSidecar == null)
                return null;

            // Only care about OpticalDisc types
            if (cicmSidecar.OpticalDisc == null || cicmSidecar.OpticalDisc.Length == 0)
                return null;

            // Setup the layerbreak
            string? layerbreak = null;

            // Find and return the layerbreak, if possible
            foreach (OpticalDiscType opticalDisc in cicmSidecar.OpticalDisc)
            {
                // If there's no layer information, skip
                if (opticalDisc.Layers == null)
                    continue;

                // TODO: Determine how to find the layerbreak from the CICM or other outputs
            }

            return layerbreak;
        }

        /// <summary>
        /// Get the write offset from the CICM Sidecar file, if possible
        /// </summary>
        /// <param name="cicmSidecar">CICM Sidecar data generated by Aaru</param>
        /// <returns>Sample write offset if possible, null on error</returns>
        private static string? GetWriteOffset(CICMMetadataType? cicmSidecar)
        {
            // If the object is null, we can't get information from it
            if (cicmSidecar == null)
                return null;

            // Only care about OpticalDisc types
            if (cicmSidecar.OpticalDisc == null || cicmSidecar.OpticalDisc.Length == 0)
                return null;

            // Loop through each OpticalDisc in the metadata
            foreach (OpticalDiscType opticalDisc in cicmSidecar.OpticalDisc)
            {
                // If the disc doesn't have an offset specified, we skip it;
                if (!opticalDisc.OffsetSpecified)
                    continue;

                return opticalDisc.Offset.ToString();
            }

            return null;
        }

        /// <summary>
        /// Get the XGD auxiliary info from the CICM Sidecar file, if possible
        /// </summary>
        /// <param name="cicmSidecar">CICM Sidecar data generated by Aaru</param>
        /// <returns>True on successful extraction of info, false otherwise</returns>
        private static bool GetXgdAuxInfo(CICMMetadataType? cicmSidecar, out string? dmihash, out string? pfihash, out string? sshash, out string? ss, out string? ssver)
        {
            dmihash = null; pfihash = null; sshash = null; ss = null; ssver = null;

            // If the object is null, we can't get information from it
            if (cicmSidecar == null)
                return false;

            // Only care about OpticalDisc types
            if (cicmSidecar.OpticalDisc == null || cicmSidecar.OpticalDisc.Length == 0)
                return false;

            // Loop through each OpticalDisc in the metadata
            foreach (OpticalDiscType opticalDisc in cicmSidecar.OpticalDisc)
            {
                // If the Xbox type isn't set, we can't extract information
                if (opticalDisc.Xbox == null)
                    continue;

                // Get the Xbox information
                XboxType xbox = opticalDisc.Xbox;

                // DMI
                if (xbox.DMI != null)
                {
                    DumpType dmi = xbox.DMI;
                    if (dmi.Checksums != null && dmi.Checksums.Length != 0)
                    {
                        foreach (ChecksumType checksum in dmi.Checksums)
                        {
                            // We only care about the CRC32
                            if (checksum.type == ChecksumTypeType.crc32)
                            {
                                dmihash = checksum.Value;
                                break;
                            }
                        }
                    }
                }

                // PFI
                if (xbox.PFI != null)
                {
                    DumpType pfi = xbox.PFI;
                    if (pfi.Checksums != null && pfi.Checksums.Length != 0)
                    {
                        foreach (ChecksumType checksum in pfi.Checksums)
                        {
                            // We only care about the CRC32
                            if (checksum.type == ChecksumTypeType.crc32)
                            {
                                pfihash = checksum.Value;
                                break;
                            }
                        }
                    }
                }

                // SS
                if (xbox.SecuritySectors != null && xbox.SecuritySectors.Length > 0)
                {
                    foreach (XboxSecuritySectorsType securitySector in xbox.SecuritySectors)
                    {
                        DumpType security = securitySector.SecuritySectors;
                        if (security.Checksums != null && security.Checksums.Length != 0)
                        {
                            foreach (ChecksumType checksum in security.Checksums)
                            {
                                // We only care about the CRC32
                                if (checksum.type == ChecksumTypeType.crc32)
                                {
                                    // TODO: Validate correctness for all 3 fields
                                    ss = security.Image;
                                    ssver = securitySector.RequestVersion.ToString();
                                    sshash = checksum.Value;
                                    break;
                                }
                            }
                        }

                        // If we got a hash, we can break
                        if (sshash != null)
                            break;
                    }
                }
            }

            return false;
        }

        /// <summary>
        /// Get the Xbox serial info from the CICM Sidecar file, if possible
        /// </summary>
        /// <param name="cicmSidecar">CICM Sidecar data generated by Aaru</param>
        /// <returns>True on successful extraction of info, false otherwise</returns>
        private static bool GetXboxDMIInfo(CICMMetadataType? cicmSidecar, out string? serial, out string? version, out Region? region)
        {
            serial = null; version = null; region = Region.World;

            // If the object is null, we can't get information from it
            if (cicmSidecar == null)
                return false;

            // Only care about OpticalDisc types
            if (cicmSidecar.OpticalDisc == null || cicmSidecar.OpticalDisc.Length == 0)
                return false;

            // Loop through each OpticalDisc in the metadata
            foreach (OpticalDiscType opticalDisc in cicmSidecar.OpticalDisc)
            {
                // If the Xbox type isn't set, we can't extract information
                if (opticalDisc.Xbox == null)
                    continue;

                // Get the Xbox information
                XboxType xbox = opticalDisc.Xbox;

                // DMI
                if (xbox.DMI != null)
                {
                    DumpType dmi = xbox.DMI;
                    string image = dmi.Image;

                    // TODO: Figure out if `image` is the right thing here
                    // TODO: Figure out how to extract info from `image`
                    //br.BaseStream.Seek(8, SeekOrigin.Begin);
                    //char[] str = br.ReadChars(8);

                    //serial = $"{str[0]}{str[1]}-{str[2]}{str[3]}{str[4]}";
                    //version = $"1.{str[5]}{str[6]}";
                    //region = GetXgdRegion(str[7]);
                    //return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Get the Xbox 360 serial info from the CICM Sidecar file, if possible
        /// </summary>
        /// <param name="cicmSidecar">CICM Sidecar data generated by Aaru</param>
        /// <returns>True on successful extraction of info, false otherwise</returns>
        private static bool GetXbox360DMIInfo(CICMMetadataType? cicmSidecar, out string? serial, out string? version, out Region? region)
        {
            serial = null; version = null; region = Region.World;

            // If the object is null, we can't get information from it
            if (cicmSidecar == null)
                return false;

            // Only care about OpticalDisc types
            if (cicmSidecar.OpticalDisc == null || cicmSidecar.OpticalDisc.Length == 0)
                return false;

            // Loop through each OpticalDisc in the metadata
            foreach (OpticalDiscType opticalDisc in cicmSidecar.OpticalDisc)
            {
                // If the Xbox type isn't set, we can't extract information
                if (opticalDisc.Xbox == null)
                    continue;

                // Get the Xbox information
                XboxType xbox = opticalDisc.Xbox;

                // DMI
                if (xbox.DMI != null)
                {
                    DumpType dmi = xbox.DMI;
                    string image = dmi.Image;

                    // TODO: Figure out if `image` is the right thing here
                    // TODO: Figure out how to extract info from `image`
                    //br.BaseStream.Seek(64, SeekOrigin.Begin);
                    //char[] str = br.ReadChars(14);

                    //serial = $"{str[0]}{str[1]}-{str[2]}{str[3]}{str[4]}{str[5]}";
                    //version = $"1.{str[6]}{str[7]}";
                    //region = GetXgdRegion(str[8]);
                    // str[9], str[10], str[11] - unknown purpose
                    // str[12], str[13] - disc <12> of <13>
                    //return true;
                }
            }

            return false;
        }

        #endregion
    }
}
