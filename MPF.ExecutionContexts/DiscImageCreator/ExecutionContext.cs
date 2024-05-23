using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using MPF.Core;
using SabreTools.RedumpLib.Data;

namespace MPF.ExecutionContexts.DiscImageCreator
{
    /// <summary>
    /// Represents a generic set of DiscImageCreator parameters
    /// </summary>
    public sealed class ExecutionContext : BaseExecutionContext
    {
        #region Generic Dumping Information

        /// <inheritdoc/>
        public override string? InputPath => DrivePath;

        /// <inheritdoc/>
        public override string? OutputPath => Filename;

        /// <inheritdoc/>
        /// <inheritdoc/>
        public override int? Speed
        {
            get { return DriveSpeed; }
            set { DriveSpeed = (sbyte?)value; }
        }

        #endregion

        #region Common Input Values

        /// <summary>
        /// Drive letter or path to pass to DiscImageCreator
        /// </summary>
        public string? DrivePath { get; set; }

        /// <summary>
        /// Drive speed to set, if applicable
        /// </summary>
        public int? DriveSpeed { get; set; }

        /// <summary>
        /// Destination filename for DiscImageCreator output
        /// </summary>
        public string? Filename { get; set; }

        /// <summary>
        /// Optiarc drive output filename for merging
        /// </summary>
        public string? OptiarcFilename { get; set; }

        /// <summary>
        /// Start LBA value for dumping specific sectors
        /// </summary>
        public int? StartLBAValue { get; set; }

        /// <summary>
        /// End LBA value for dumping specific sectors
        /// </summary>
        public int? EndLBAValue { get; set; }

        #endregion

        #region Flag Values

        /// <summary>
        /// Manual offset for Audio CD (default 0)
        /// </summary>
        public int? AddOffsetValue { get; set; }

        /// <summary>
        /// 0xbe opcode value for dumping
        /// Possible values: raw (default), pack
        /// </summary>
        /// TODO: Make this an enum
        public string? BEOpcodeValue { get; set; }

        /// <summary>
        /// C2 reread options for dumping [CD only]
        /// [0] - Reread value (default 4000)
        /// [1] - Reading speed when fixing the C2 error (default: same as the <DriveSpeed(0-72)>)
        /// [2] - C2 offset (default: 0)
        /// [3] - 0 reread issue sector (default), 1 reread all
        /// [4] - First LBA to reread (default 0)
        /// [5] - Last LBA to reread (default EOS)
        /// </summary>
        public int?[] C2OpcodeValue { get; set; } = new int?[6];

        /// <summary>
        /// C2 reread options for dumping [DVD/HD-DVD/BD only] (default 10)
        /// </summary>
        public int? DVDRereadValue { get; set; }

        /// <summary>
        /// End LBA for fixing
        /// </summary>
        public int? FixValue { get; set; }

        /// <summary>
        /// Set the force unit access flag value (default 1)
        /// </summary>
        public int? ForceUnitAccessValue { get; set; }

        /// <summary>
        /// Set the multi-sector read flag value (default 50)
        /// </summary>
        public int? MultiSectorReadValue { get; set; }

        /// <summary>
        /// Set the no skip security sector flag value (default 100)
        /// </summary>
        public int? NoSkipSecuritySectorValue { get; set; }

        /// <summary>
        /// Set the pad sector flag value (default 0)
        /// </summary>
        public byte? PadSectorValue { get; set; }

        /// <summary>
        /// Set the range End LBA value (required for DVD)
        /// </summary>
        public int? RangeEndLBAValue { get; set; }

        /// <summary>
        /// Set the range Start LBA value (required for DVD)
        /// </summary>
        public int? RangeStartLBAValue { get; set; }

        /// <summary>
        /// Set the reverse End LBA value (required for DVD)
        /// </summary>
        public int? ReverseEndLBAValue { get; set; }

        /// <summary>
        /// Set the reverse Start LBA value (required for DVD)
        /// </summary>
        public int? ReverseStartLBAValue { get; set; }

        /// <summary>
        /// Set scan file timeout value (default 60)
        /// </summary>
        public int? ScanFileProtectValue { get; set; }

        /// <summary>
        /// Beginning and ending sectors to skip for physical protection (both default 0)
        /// </summary>
        public int?[] SkipSectorValue { get; set; } = new int?[2];

        /// <summary>
        /// Set the subchanel read level
        /// Possible values: 0 no next sub, 1 next sub (default), 2 next and next next
        /// </summary>
        public int? SubchannelReadLevelValue { get; set; }

        /// <summary>
        /// Set number of empty bytes to insert at the head of first track for VideoNow (default 0)
        /// </summary>
        public int? VideoNowValue { get; set; }

        #endregion

        /// <inheritdoc/>
        public ExecutionContext(string? parameters) : base(parameters) { }

        /// <inheritdoc/>
        public ExecutionContext(RedumpSystem? system, MediaType? type, string? drivePath, string filename, int? driveSpeed, Options options)
            : base(system, type, drivePath, filename, driveSpeed, options)
        {
        }

        #region BaseExecutionContext Implementations

        /// <inheritdoc/>
        public override Dictionary<string, List<string>> GetCommandSupport()
        {
            return new Dictionary<string, List<string>>()
            {
                [CommandStrings.Audio] =
                [
                    FlagStrings.BEOpcode,
                    FlagStrings.C2Opcode,
                    FlagStrings.D8Opcode,
                    FlagStrings.DatExpand,
                    FlagStrings.DisableBeep,
                    FlagStrings.ForceUnitAccess,
                    FlagStrings.NoFixSubP,
                    FlagStrings.NoFixSubQ,
                    FlagStrings.NoFixSubRtoW,
                    FlagStrings.Reverse,
                    FlagStrings.ScanAntiMod,
                    FlagStrings.ScanFileProtect,
                    FlagStrings.ScanSectorProtect,
                    FlagStrings.SkipSector,
                    FlagStrings.SubchannelReadLevel,
                    FlagStrings.Tages,
                ],

                [CommandStrings.BluRay] =
                [
                    FlagStrings.DatExpand,
                    FlagStrings.DisableBeep,
                    FlagStrings.DVDReread,
                    FlagStrings.ForceUnitAccess,
                    FlagStrings.UseAnchorVolumeDescriptorPointer,
                ],

                [CommandStrings.Close] = [],

                [CommandStrings.CompactDisc] =
                [
                    FlagStrings.AddOffset,
                    FlagStrings.AMSF,
                    FlagStrings.AtariJaguar,
                    FlagStrings.BEOpcode,
                    FlagStrings.C2Opcode,
                    FlagStrings.D8Opcode,
                    FlagStrings.DatExpand,
                    FlagStrings.DisableBeep,
                    FlagStrings.ExtractMicroSoftCabFile,
                    FlagStrings.ForceUnitAccess,
                    FlagStrings.MultiSectorRead,
                    FlagStrings.NoFixSubP,
                    FlagStrings.NoFixSubQ,
                    FlagStrings.NoFixSubQLibCrypt,
                    FlagStrings.NoFixSubQSecuROM,
                    FlagStrings.NoFixSubRtoW,
                    FlagStrings.ScanAntiMod,
                    FlagStrings.ScanFileProtect,
                    FlagStrings.ScanSectorProtect,
                    FlagStrings.SeventyFour,
                    FlagStrings.SubchannelReadLevel,
                    FlagStrings.VideoNow,
                    FlagStrings.VideoNowColor,
                    FlagStrings.VideoNowXP,
                ],

                [CommandStrings.Data] =
                [
                    FlagStrings.BEOpcode,
                    FlagStrings.C2Opcode,
                    FlagStrings.D8Opcode,
                    FlagStrings.DatExpand,
                    FlagStrings.DisableBeep,
                    FlagStrings.ForceUnitAccess,
                    FlagStrings.NoFixSubP,
                    FlagStrings.NoFixSubQ,
                    FlagStrings.NoFixSubRtoW,
                    FlagStrings.Reverse,
                    FlagStrings.ScanAntiMod,
                    FlagStrings.ScanFileProtect,
                    FlagStrings.ScanSectorProtect,
                    FlagStrings.SkipSector,
                    FlagStrings.SubchannelReadLevel,
                    FlagStrings.Tages,
                ],

                [CommandStrings.DigitalVideoDisc] =
                [
                    FlagStrings.CopyrightManagementInformation,
                    FlagStrings.DatExpand,
                    FlagStrings.DisableBeep,
                    FlagStrings.DVDReread,
                    FlagStrings.Fix,
                    FlagStrings.ForceUnitAccess,
                    FlagStrings.PadSector,
                    FlagStrings.Range,
                    FlagStrings.Raw,
                    FlagStrings.Resume,
                    FlagStrings.Reverse,
                    FlagStrings.ScanFileProtect,
                    FlagStrings.SkipSector,
                    FlagStrings.UseAnchorVolumeDescriptorPointer,
                ],

                [CommandStrings.Disk] =
                [
                    FlagStrings.DatExpand,
                ],

                [CommandStrings.DriveSpeed] = [],

                [CommandStrings.Eject] = [],

                [CommandStrings.Floppy] =
                [
                    FlagStrings.DatExpand,
                ],

                [CommandStrings.GDROM] =
                [
                    FlagStrings.BEOpcode,
                    FlagStrings.C2Opcode,
                    FlagStrings.D8Opcode,
                    FlagStrings.DatExpand,
                    FlagStrings.DisableBeep,
                    FlagStrings.ForceUnitAccess,
                    FlagStrings.NoFixSubP,
                    FlagStrings.NoFixSubQ,
                    FlagStrings.NoFixSubRtoW,
                    FlagStrings.SubchannelReadLevel,
                ],

                [CommandStrings.MDS] = [],

                [CommandStrings.Merge] = [],

                [CommandStrings.Reset] = [],

                [CommandStrings.SACD] =
                [
                    FlagStrings.DatExpand,
                    FlagStrings.DisableBeep,
                ],

                [CommandStrings.Start] = [],

                [CommandStrings.Stop] = [],

                [CommandStrings.Sub] = [],

                [CommandStrings.Swap] =
                [
                    FlagStrings.AddOffset,
                    FlagStrings.BEOpcode,
                    FlagStrings.C2Opcode,
                    FlagStrings.D8Opcode,
                    FlagStrings.DatExpand,
                    FlagStrings.DisableBeep,
                    FlagStrings.ForceUnitAccess,
                    FlagStrings.NoFixSubP,
                    FlagStrings.NoFixSubQ,
                    FlagStrings.NoFixSubQLibCrypt,
                    FlagStrings.NoFixSubQSecuROM,
                    FlagStrings.NoFixSubRtoW,
                    FlagStrings.ScanAntiMod,
                    FlagStrings.ScanFileProtect,
                    FlagStrings.ScanSectorProtect,
                    FlagStrings.SeventyFour,
                    FlagStrings.SubchannelReadLevel,
                    FlagStrings.VideoNow,
                    FlagStrings.VideoNowColor,
                    FlagStrings.VideoNowXP,
                ],

                [CommandStrings.Tape] = [],

                [CommandStrings.Version] = [],

                [CommandStrings.XBOX] =
                [
                    FlagStrings.DatExpand,
                    FlagStrings.DisableBeep,
                    FlagStrings.DVDReread,
                    FlagStrings.ForceUnitAccess,
                    FlagStrings.NoSkipSS,
                ],

                [CommandStrings.XBOXSwap] =
                [
                    FlagStrings.DatExpand,
                    FlagStrings.DisableBeep,
                    FlagStrings.ForceUnitAccess,
                    FlagStrings.NoSkipSS,
                ],

                [CommandStrings.XGD2Swap] =
                [
                    FlagStrings.DatExpand,
                    FlagStrings.DisableBeep,
                    FlagStrings.ForceUnitAccess,
                    FlagStrings.NoSkipSS,
                ],

                [CommandStrings.XGD3Swap] =
                [
                    FlagStrings.DatExpand,
                    FlagStrings.DisableBeep,
                    FlagStrings.ForceUnitAccess,
                    FlagStrings.NoSkipSS,
                ],
            };
        }

        /// <inheritdoc/>
        public override string? GenerateParameters()
        {
            var parameters = new List<string>();

            BaseCommand ??= CommandStrings.NONE;

            if (!string.IsNullOrEmpty(BaseCommand))
                parameters.Add(BaseCommand);
            else
                return null;

            // Drive Letter
            if (BaseCommand == CommandStrings.Audio
                || BaseCommand == CommandStrings.BluRay
                || BaseCommand == CommandStrings.Close
                || BaseCommand == CommandStrings.CompactDisc
                || BaseCommand == CommandStrings.Data
                || BaseCommand == CommandStrings.DigitalVideoDisc
                || BaseCommand == CommandStrings.Disk
                || BaseCommand == CommandStrings.DriveSpeed
                || BaseCommand == CommandStrings.Eject
                || BaseCommand == CommandStrings.Floppy
                || BaseCommand == CommandStrings.GDROM
                || BaseCommand == CommandStrings.Reset
                || BaseCommand == CommandStrings.SACD
                || BaseCommand == CommandStrings.Start
                || BaseCommand == CommandStrings.Stop
                || BaseCommand == CommandStrings.Swap
                || BaseCommand == CommandStrings.XBOX
                || BaseCommand == CommandStrings.XBOXSwap
                || BaseCommand == CommandStrings.XGD2Swap
                || BaseCommand == CommandStrings.XGD3Swap)
            {
                if (DrivePath != null)
                    parameters.Add(DrivePath);
                else
                    return null;
            }

            // Filename
            if (BaseCommand == CommandStrings.Audio
                || BaseCommand == CommandStrings.BluRay
                || BaseCommand == CommandStrings.CompactDisc
                || BaseCommand == CommandStrings.Data
                || BaseCommand == CommandStrings.DigitalVideoDisc
                || BaseCommand == CommandStrings.Disk
                || BaseCommand == CommandStrings.Floppy
                || BaseCommand == CommandStrings.GDROM
                || BaseCommand == CommandStrings.MDS
                || BaseCommand == CommandStrings.Merge
                || BaseCommand == CommandStrings.SACD
                || BaseCommand == CommandStrings.Swap
                || BaseCommand == CommandStrings.Sub
                || BaseCommand == CommandStrings.Tape
                || BaseCommand == CommandStrings.XBOX
                || BaseCommand == CommandStrings.XBOXSwap
                || BaseCommand == CommandStrings.XGD2Swap
                || BaseCommand == CommandStrings.XGD3Swap)
            {
                if (Filename != null)
                    parameters.Add("\"" + Filename.Trim('"') + "\"");
                else
                    return null;
            }

            // Optiarc Filename
            if (BaseCommand == CommandStrings.Merge)
            {
                if (OptiarcFilename != null)
                    parameters.Add("\"" + OptiarcFilename.Trim('"') + "\"");
                else
                    return null;
            }

            // Drive Speed
            if (BaseCommand == CommandStrings.Audio
                || BaseCommand == CommandStrings.BluRay
                || BaseCommand == CommandStrings.CompactDisc
                || BaseCommand == CommandStrings.Data
                || BaseCommand == CommandStrings.DigitalVideoDisc
                || BaseCommand == CommandStrings.GDROM
                || BaseCommand == CommandStrings.SACD
                || BaseCommand == CommandStrings.Swap
                || BaseCommand == CommandStrings.XBOX
                || BaseCommand == CommandStrings.XBOXSwap
                || BaseCommand == CommandStrings.XGD2Swap
                || BaseCommand == CommandStrings.XGD3Swap)
            {
                if (DriveSpeed != null)
                    parameters.Add(DriveSpeed.ToString() ?? string.Empty);
                else
                    return null;
            }

            // LBA Markers
            if (BaseCommand == CommandStrings.Audio
                || BaseCommand == CommandStrings.Data)
            {
                if (StartLBAValue != null && EndLBAValue != null)
                {
                    parameters.Add(StartLBAValue.ToString() ?? string.Empty);
                    parameters.Add(EndLBAValue.ToString() ?? string.Empty);
                }
                else
                    return null;
            }

            // Add Offset
            if (IsFlagSupported(FlagStrings.AddOffset))
            {
                if (this[FlagStrings.AddOffset] == true)
                {
                    parameters.Add(FlagStrings.AddOffset);
                    if (AddOffsetValue != null)
                        parameters.Add(AddOffsetValue.ToString() ?? string.Empty);
                }
            }

            // AMSF Dumping
            if (IsFlagSupported(FlagStrings.AMSF))
            {
                if (this[FlagStrings.AMSF] == true)
                    parameters.Add(FlagStrings.AMSF);
            }

            // Atari Jaguar CD
            if (IsFlagSupported(FlagStrings.AtariJaguar))
            {
                if (this[FlagStrings.AtariJaguar] == true)
                    parameters.Add(FlagStrings.AtariJaguar);
            }

            // BE Opcode
            if (IsFlagSupported(FlagStrings.BEOpcode))
            {
                if (this[FlagStrings.BEOpcode] == true && this[FlagStrings.D8Opcode] != true)
                {
                    parameters.Add(FlagStrings.BEOpcode);
                    if (BEOpcodeValue != null
                        && (BEOpcodeValue == "raw" || BEOpcodeValue == "pack"))
                        parameters.Add(BEOpcodeValue);
                }
            }

            // C2 Opcode
            if (IsFlagSupported(FlagStrings.C2Opcode))
            {
                if (this[FlagStrings.C2Opcode] == true)
                {
                    parameters.Add(FlagStrings.C2Opcode);
                    if (C2OpcodeValue[0] != null)
                    {
                        if (C2OpcodeValue[0] > 0)
                            parameters.Add(C2OpcodeValue[0].ToString() ?? string.Empty);
                        else
                            return null;
                    }
                    if (C2OpcodeValue[1] != null)
                    {
                        parameters.Add(C2OpcodeValue[1].ToString() ?? string.Empty);
                    }
                    if (C2OpcodeValue[2] != null)
                    {
                        parameters.Add(C2OpcodeValue[2].ToString() ?? string.Empty);
                    }
                    if (C2OpcodeValue[3] != null)
                    {
                        if (C2OpcodeValue[3] == 0)
                        {
                            parameters.Add(C2OpcodeValue[3].ToString() ?? string.Empty);
                        }
                        else if (C2OpcodeValue[3] == 1)
                        {
                            parameters.Add(C2OpcodeValue[3].ToString() ?? string.Empty);
                            if (C2OpcodeValue[4] != null && C2OpcodeValue[5] != null)
                            {
                                if (C2OpcodeValue[4] > 0 && C2OpcodeValue[5] > 0)
                                {
                                    parameters.Add(C2OpcodeValue[4].ToString() ?? string.Empty);
                                    parameters.Add(C2OpcodeValue[5].ToString() ?? string.Empty);
                                }
                                else
                                {
                                    return null;
                                }
                            }
                        }
                        else
                        {
                            return null;
                        }
                    }
                }
            }

            // Copyright Management Information
            if (IsFlagSupported(FlagStrings.CopyrightManagementInformation))
            {
                if (this[FlagStrings.CopyrightManagementInformation] == true)
                    parameters.Add(FlagStrings.CopyrightManagementInformation);
            }

            // D8 Opcode
            if (IsFlagSupported(FlagStrings.D8Opcode))
            {
                if (this[FlagStrings.D8Opcode] == true)
                    parameters.Add(FlagStrings.D8Opcode);
            }

            // DAT Expand
            if (IsFlagSupported(FlagStrings.DatExpand))
            {
                if (this[FlagStrings.DatExpand] == true)
                    parameters.Add(FlagStrings.DatExpand);
            }

            // Disable Beep
            if (IsFlagSupported(FlagStrings.DisableBeep))
            {
                if (this[FlagStrings.DisableBeep] == true)
                    parameters.Add(FlagStrings.DisableBeep);
            }

            // DVD/HD-DVD/BD Reread
            if (IsFlagSupported(FlagStrings.DVDReread))
            {
                if (this[FlagStrings.DVDReread] == true)
                {
                    parameters.Add(FlagStrings.DVDReread);
                    if (DVDRereadValue != null)
                        parameters.Add(DVDRereadValue.ToString() ?? string.Empty);
                }
            }

            // Extract MicroSoftCabFile
            if (IsFlagSupported(FlagStrings.ExtractMicroSoftCabFile))
            {
                if (this[FlagStrings.ExtractMicroSoftCabFile] == true)
                    parameters.Add(FlagStrings.ExtractMicroSoftCabFile);
            }

            // Fix
            if (IsFlagSupported(FlagStrings.Fix))
            {
                if (this[FlagStrings.Fix] == true)
                {
                    parameters.Add(FlagStrings.Fix);
                    if (FixValue != null)
                        parameters.Add(FixValue.ToString() ?? string.Empty);
                    else
                        return null;
                }
            }

            // Force Unit Access
            if (IsFlagSupported(FlagStrings.ForceUnitAccess))
            {
                if (this[FlagStrings.ForceUnitAccess] == true)
                {
                    parameters.Add(FlagStrings.ForceUnitAccess);
                    if (ForceUnitAccessValue != null)
                        parameters.Add(ForceUnitAccessValue.ToString() ?? string.Empty);
                }
            }

            // Multi-Sector Read
            if (IsFlagSupported(FlagStrings.MultiSectorRead))
            {
                if (this[FlagStrings.MultiSectorRead] == true)
                {
                    parameters.Add(FlagStrings.MultiSectorRead);
                    if (MultiSectorReadValue != null)
                        parameters.Add(MultiSectorReadValue.ToString() ?? string.Empty);
                }
            }

            // Not fix SubP
            if (IsFlagSupported(FlagStrings.NoFixSubP))
            {
                if (this[FlagStrings.NoFixSubP] == true)
                    parameters.Add(FlagStrings.NoFixSubP);
            }

            // Not fix SubQ
            if (IsFlagSupported(FlagStrings.NoFixSubQ))
            {
                if (this[FlagStrings.NoFixSubQ] == true)
                    parameters.Add(FlagStrings.NoFixSubQ);
            }

            // Not fix SubQ (PlayStation LibCrypt)
            if (IsFlagSupported(FlagStrings.NoFixSubQLibCrypt))
            {
                if (this[FlagStrings.NoFixSubQLibCrypt] == true)
                    parameters.Add(FlagStrings.NoFixSubQLibCrypt);
            }

            // Not fix SubQ (SecuROM)
            if (IsFlagSupported(FlagStrings.NoFixSubQSecuROM))
            {
                if (this[FlagStrings.NoFixSubQSecuROM] == true)
                    parameters.Add(FlagStrings.NoFixSubQSecuROM);
            }

            // Not fix SubRtoW
            if (IsFlagSupported(FlagStrings.NoFixSubRtoW))
            {
                if (this[FlagStrings.NoFixSubRtoW] == true)
                    parameters.Add(FlagStrings.NoFixSubRtoW);
            }

            // Not skip security sectors
            if (IsFlagSupported(FlagStrings.NoSkipSS))
            {
                if (this[FlagStrings.NoSkipSS] == true)
                {
                    parameters.Add(FlagStrings.NoSkipSS);
                    if (NoSkipSecuritySectorValue != null)
                        parameters.Add(NoSkipSecuritySectorValue.ToString() ?? string.Empty);
                }
            }

            // Pad sectors
            if (IsFlagSupported(FlagStrings.PadSector))
            {
                if (this[FlagStrings.PadSector] == true)
                {
                    parameters.Add(FlagStrings.PadSector);
                    if (PadSectorValue != null)
                        parameters.Add(PadSectorValue.ToString() ?? string.Empty);
                }
            }

            // Range
            if (IsFlagSupported(FlagStrings.Range))
            {
                if (RangeStartLBAValue == null || RangeEndLBAValue == null)
                    return null;

                if (this[FlagStrings.Range] == true)
                {
                    parameters.Add(FlagStrings.Range);
                    parameters.Add(RangeStartLBAValue.ToString() ?? string.Empty);
                    parameters.Add(RangeEndLBAValue.ToString() ?? string.Empty);
                }
            }

            // Raw read (2064 byte/sector)
            if (IsFlagSupported(FlagStrings.Raw))
            {
                if (this[FlagStrings.Raw] == true)
                    parameters.Add(FlagStrings.Raw);
            }

            // Resume
            if (IsFlagSupported(FlagStrings.Resume))
            {
                if (this[FlagStrings.Resume] == true)
                    parameters.Add(FlagStrings.Resume);
            }

            // Reverse read
            if (IsFlagSupported(FlagStrings.Reverse))
            {
                if (this[FlagStrings.Reverse] == true)
                {
                    parameters.Add(FlagStrings.Reverse);

                    if (BaseCommand == CommandStrings.DigitalVideoDisc)
                    {
                        if (ReverseStartLBAValue == null || ReverseEndLBAValue == null)
                            return null;

                        parameters.Add(ReverseStartLBAValue.ToString() ?? string.Empty);
                        parameters.Add(ReverseEndLBAValue.ToString() ?? string.Empty);
                    }
                }
            }

            // Scan PlayStation anti-mod strings
            if (IsFlagSupported(FlagStrings.ScanAntiMod))
            {
                if (this[FlagStrings.ScanAntiMod] == true)
                    parameters.Add(FlagStrings.ScanAntiMod);
            }

            // Scan file to detect protect
            if (IsFlagSupported(FlagStrings.ScanFileProtect))
            {
                if (this[FlagStrings.ScanFileProtect] == true)
                {
                    parameters.Add(FlagStrings.ScanFileProtect);
                    if (ScanFileProtectValue != null)
                    {
                        if (ScanFileProtectValue > 0)
                            parameters.Add(ScanFileProtectValue.ToString() ?? string.Empty);
                        else
                            return null;
                    }
                }
            }

            // Scan file to detect protect
            if (IsFlagSupported(FlagStrings.ScanSectorProtect))
            {
                if (this[FlagStrings.ScanSectorProtect] == true)
                    parameters.Add(FlagStrings.ScanSectorProtect);
            }

            // Scan 74:00:00 (Saturn)
            if (IsFlagSupported(FlagStrings.SeventyFour))
            {
                if (this[FlagStrings.SeventyFour] == true)
                    parameters.Add(FlagStrings.SeventyFour);
            }

            // Skip sectors
            if (IsFlagSupported(FlagStrings.SkipSector))
            {
                if (this[FlagStrings.SkipSector] == true)
                {
                    parameters.Add(FlagStrings.SkipSector);
                    if (SkipSectorValue[0] != null)
                    {
                        if (SkipSectorValue[0] > 0)
                            parameters.Add(SkipSectorValue[0].ToString() ?? string.Empty);
                        else
                            return null;
                    }
                    if (SkipSectorValue[1] != null)
                    {
                        if (SkipSectorValue[1] == 0)
                            parameters.Add(SkipSectorValue[1].ToString() ?? string.Empty);
                    }
                }
            }

            // Set Subchannel read level
            if (IsFlagSupported(FlagStrings.SubchannelReadLevel))
            {
                if (this[FlagStrings.SubchannelReadLevel] == true)
                {
                    parameters.Add(FlagStrings.SubchannelReadLevel);
                    if (SubchannelReadLevelValue != null)
                    {
                        if (SubchannelReadLevelValue >= 0 && SubchannelReadLevelValue <= 2)
                            parameters.Add(SubchannelReadLevelValue.ToString() ?? string.Empty);
                        else
                            return null;
                    }
                }
            }

            // Tages
            if (IsFlagSupported(FlagStrings.Tages))
            {
                if (this[FlagStrings.Tages] == true)
                    parameters.Add(FlagStrings.Tages);
            }

            // Use Anchor Volume Descriptor Pointer
            if (IsFlagSupported(FlagStrings.UseAnchorVolumeDescriptorPointer))
            {
                if (this[FlagStrings.UseAnchorVolumeDescriptorPointer] == true)
                    parameters.Add(FlagStrings.UseAnchorVolumeDescriptorPointer);
            }

            // VideoNow
            if (IsFlagSupported(FlagStrings.VideoNow))
            {
                if (this[FlagStrings.VideoNow] == true)
                {
                    parameters.Add(FlagStrings.VideoNow);
                    if (VideoNowValue != null)
                    {
                        if (VideoNowValue >= 0)
                            parameters.Add(VideoNowValue.ToString() ?? string.Empty);
                        else
                            return null;
                    }
                }
            }

            // VideoNow Color
            if (IsFlagSupported(FlagStrings.VideoNowColor))
            {
                if (this[FlagStrings.VideoNowColor] == true)
                    parameters.Add(FlagStrings.VideoNowColor);
            }

            // VideoNowXP
            if (IsFlagSupported(FlagStrings.VideoNowXP))
            {
                if (this[FlagStrings.VideoNowXP] == true)
                    parameters.Add(FlagStrings.VideoNowXP);
            }

            return string.Join(" ", [.. parameters]);
        }

        /// <inheritdoc/>
        public override string? GetDefaultExtension(MediaType? mediaType) => Converters.Extension(mediaType);

        /// <inheritdoc/>
        public override MediaType? GetMediaType() => Converters.ToMediaType(BaseCommand);

        /// <inheritdoc/>
        public override bool IsDumpingCommand()
        {
            return BaseCommand switch
            {
                CommandStrings.Audio
                    or CommandStrings.BluRay
                    or CommandStrings.CompactDisc
                    or CommandStrings.Data
                    or CommandStrings.DigitalVideoDisc
                    or CommandStrings.Disk
                    or CommandStrings.Floppy
                    or CommandStrings.GDROM
                    or CommandStrings.SACD
                    or CommandStrings.Swap
                    or CommandStrings.Tape
                    or CommandStrings.XBOX
                    or CommandStrings.XBOXSwap
                    or CommandStrings.XGD2Swap
                    or CommandStrings.XGD3Swap => true,
                _ => false,
            };
        }

        /// <inheritdoc/>
        protected override void ResetValues()
        {
            BaseCommand = CommandStrings.NONE;

            DrivePath = null;
            DriveSpeed = null;

            Filename = null;

            StartLBAValue = null;
            EndLBAValue = null;

            flags = [];

            AddOffsetValue = null;
            BEOpcodeValue = null;
            C2OpcodeValue = new int?[6];
            DVDRereadValue = null;
            FixValue = null;
            ForceUnitAccessValue = null;
            NoSkipSecuritySectorValue = null;
            ScanFileProtectValue = null;
            SubchannelReadLevelValue = null;
            VideoNowValue = null;
        }

        /// <inheritdoc/>
        protected override void SetDefaultParameters(string? drivePath, string filename, int? driveSpeed, Options options)
        {
            SetBaseCommand(this.System, this.Type);

            DrivePath = drivePath;
            DriveSpeed = driveSpeed;
            Filename = filename;

            // First check to see if the combination of system and MediaType is valid
            var validTypes = this.System.MediaTypes();
            if (!validTypes.Contains(this.Type))
                return;

            // Set disable beep flag, if needed
            if (options.DICQuietMode)
                this[FlagStrings.DisableBeep] = true;

            // Set the C2 reread count
            C2OpcodeValue[0] = options.DICRereadCount switch
            {
                -1 => null,
                0 => 20,
                _ => options.DICRereadCount,
            };

            // Set the DVD/HD-DVD/BD reread count
            DVDRereadValue = options.DICDVDRereadCount switch
            {
                -1 => null,
                0 => 10,
                _ => options.DICDVDRereadCount,
            };

            // Now sort based on disc type
            switch (this.Type)
            {
                case MediaType.CDROM:
                    this[FlagStrings.C2Opcode] = true;
                    this[FlagStrings.MultiSectorRead] = options.DICMultiSectorRead;
                    if (options.DICMultiSectorRead)
                        this.MultiSectorReadValue = options.DICMultiSectorReadValue;

                    switch (this.System)
                    {
                        case RedumpSystem.AppleMacintosh:
                        case RedumpSystem.IBMPCcompatible:
                            this[FlagStrings.NoFixSubQSecuROM] = true;
                            this[FlagStrings.ScanFileProtect] = true;
                            this[FlagStrings.ScanSectorProtect] = options.DICParanoidMode;
                            this[FlagStrings.SubchannelReadLevel] = options.DICParanoidMode;
                            if (this[FlagStrings.SubchannelReadLevel] == true)
                                SubchannelReadLevelValue = 2;

                            break;
                        case RedumpSystem.AtariJaguarCDInteractiveMultimediaSystem:
                            this[FlagStrings.AtariJaguar] = true;
                            break;
                        case RedumpSystem.HasbroVideoNow:
                        case RedumpSystem.HasbroVideoNowColor:
                        case RedumpSystem.HasbroVideoNowJr:
                        case RedumpSystem.HasbroVideoNowXP:
                            this[FlagStrings.AddOffset] = true;
                            this.AddOffsetValue = 0; // Value needed for first run and placeholder after
                            break;
                        case RedumpSystem.SonyPlayStation:
                            this[FlagStrings.ScanAntiMod] = true;
                            this[FlagStrings.NoFixSubQLibCrypt] = true;
                            break;
                    }
                    break;
                case MediaType.DVD:
                    this[FlagStrings.CopyrightManagementInformation] = options.DICUseCMIFlag;
                    this[FlagStrings.ScanFileProtect] = options.DICParanoidMode;
                    this[FlagStrings.DVDReread] = true;
                    break;
                case MediaType.GDROM:
                    this[FlagStrings.C2Opcode] = true;
                    break;
                case MediaType.HDDVD:
                    this[FlagStrings.CopyrightManagementInformation] = options.DICUseCMIFlag;
                    this[FlagStrings.DVDReread] = true;
                    break;
                case MediaType.BluRay:
                    this[FlagStrings.DVDReread] = true;
                    break;

                // Special Formats
                case MediaType.NintendoGameCubeGameDisc:
                    this[FlagStrings.Raw] = true;
                    break;
                case MediaType.NintendoWiiOpticalDisc:
                    this[FlagStrings.Raw] = true;
                    break;

                // Non-optical
                case MediaType.FloppyDisk:
                    // Currently no defaults set
                    break;
            }
        }

        /// <inheritdoc/>
        protected override bool ValidateAndSetParameters(string? parameters)
        {
            BaseCommand = CommandStrings.NONE;

            // The string has to be valid by itself first
            if (string.IsNullOrEmpty(parameters))
                return false;

            // Now split the string into parts for easier validation
            // https://stackoverflow.com/questions/14655023/split-a-string-that-has-white-spaces-unless-they-are-enclosed-within-quotes
            parameters = parameters!.Trim();
            List<string> parts = Regex.Matches(parameters, @"[\""].+?[\""]|[^ ]+", RegexOptions.Compiled)
                .Cast<Match>()
                .Select(m => m.Value)
                .ToList();

            // Determine what the commandline should look like given the first item
            BaseCommand = parts[0];

            // Loop through ordered command-specific flags
            int index = -1;
            switch (BaseCommand)
            {
                case CommandStrings.Audio:
                    if (parts.Count < 6)
                        return false;

                    if (!IsValidDriveLetter(parts[1]))
                        return false;
                    else
                        DrivePath = parts[1];

                    if (IsFlagSupported(parts[2]))
                        return false;
                    else
                        Filename = parts[2];

                    if (!IsValidInt32(parts[3], lowerBound: 0, upperBound: 72))
                        return false;
                    else
                        DriveSpeed = Int32.Parse(parts[3]);

                    if (!IsValidInt32(parts[4]))
                        return false;
                    else
                        StartLBAValue = Int32.Parse(parts[4]);

                    if (!IsValidInt32(parts[5]))
                        return false;
                    else
                        EndLBAValue = Int32.Parse(parts[5]);

                    index = 6;
                    break;

                case CommandStrings.BluRay:
                    if (parts.Count < 4)
                        return false;

                    if (!IsValidDriveLetter(parts[1]))
                        return false;
                    else
                        DrivePath = parts[1];

                    if (IsFlagSupported(parts[2]))
                        return false;
                    else
                        Filename = parts[2];

                    if (!IsValidInt32(parts[3], lowerBound: 0, upperBound: 72))
                        return false;
                    else
                        DriveSpeed = Int32.Parse(parts[3]);

                    index = 4;
                    break;

                case CommandStrings.Close:
                    if (parts.Count != 2)
                        return false;

                    if (!IsValidDriveLetter(parts[1]))
                        return false;
                    else
                        DrivePath = parts[1];

                    break;

                case CommandStrings.CompactDisc:
                    if (parts.Count < 4)
                        return false;

                    if (!IsValidDriveLetter(parts[1]))
                        return false;
                    else
                        DrivePath = parts[1];

                    if (IsFlagSupported(parts[2]))
                        return false;
                    else
                        Filename = parts[2];

                    if (!IsValidInt32(parts[3], lowerBound: 0, upperBound: 72))
                        return false;
                    else
                        DriveSpeed = Int32.Parse(parts[3]);

                    index = 4;
                    break;

                case CommandStrings.Data:
                    if (parts.Count < 6)
                        return false;

                    if (!IsValidDriveLetter(parts[1]))
                        return false;
                    else
                        DrivePath = parts[1];

                    if (IsFlagSupported(parts[2]))
                        return false;
                    else
                        Filename = parts[2];

                    if (!IsValidInt32(parts[3], lowerBound: 0, upperBound: 72))
                        return false;
                    else
                        DriveSpeed = Int32.Parse(parts[3]);

                    if (!IsValidInt32(parts[4]))
                        return false;
                    else
                        StartLBAValue = Int32.Parse(parts[4]);

                    if (!IsValidInt32(parts[5]))
                        return false;
                    else
                        EndLBAValue = Int32.Parse(parts[5]);

                    index = 6;
                    break;

                case CommandStrings.DigitalVideoDisc:
                    if (parts.Count < 4)
                        return false;

                    if (!IsValidDriveLetter(parts[1]))
                        return false;
                    else
                        DrivePath = parts[1];

                    if (IsFlagSupported(parts[2]))
                        return false;
                    else
                        Filename = parts[2];

                    if (!IsValidInt32(parts[3], lowerBound: 0, upperBound: 24)) // Officially 0-16
                        return false;
                    else
                        DriveSpeed = Int32.Parse(parts[3]);

                    index = 4;
                    break;

                case CommandStrings.Disk:
                    if (parts.Count != 3)
                        return false;

                    if (!IsValidDriveLetter(parts[1]))
                        return false;
                    else
                        DrivePath = parts[1];

                    if (IsFlagSupported(parts[2]))
                        return false;
                    else
                        Filename = parts[2];

                    break;

                case CommandStrings.DriveSpeed:
                    if (parts.Count != 2)
                        return false;

                    if (!IsValidDriveLetter(parts[1]))
                        return false;
                    else
                        DrivePath = parts[1];

                    break;

                case CommandStrings.Eject:
                    if (parts.Count != 2)
                        return false;

                    if (!IsValidDriveLetter(parts[1]))
                        return false;
                    else
                        DrivePath = parts[1];

                    break;

                case CommandStrings.Floppy:
                    if (parts.Count != 3)
                        return false;

                    if (!IsValidDriveLetter(parts[1]))
                        return false;
                    else
                        DrivePath = parts[1];

                    if (IsFlagSupported(parts[2]))
                        return false;
                    else
                        Filename = parts[2];

                    break;

                case CommandStrings.GDROM:
                    if (parts.Count < 4)
                        return false;

                    if (!IsValidDriveLetter(parts[1]))
                        return false;
                    else
                        DrivePath = parts[1];

                    if (IsFlagSupported(parts[2]))
                        return false;
                    else
                        Filename = parts[2];

                    if (!IsValidInt32(parts[3], lowerBound: 0, upperBound: 72))
                        return false;
                    else
                        DriveSpeed = Int32.Parse(parts[3]);

                    index = 4;
                    break;

                case CommandStrings.MDS:
                    if (parts.Count != 2)
                        return false;

                    if (IsFlagSupported(parts[1]) || !File.Exists(parts[1]))
                        return false;
                    else
                        Filename = parts[1];

                    break;

                case CommandStrings.Merge:
                    if (parts.Count != 3)
                        return false;

                    if (IsFlagSupported(parts[1]) || !File.Exists(parts[1]))
                        return false;
                    else
                        Filename = parts[1];

                    if (IsFlagSupported(parts[2]) || !File.Exists(parts[2]))
                        return false;
                    else
                        OptiarcFilename = parts[2];

                    break;

                case CommandStrings.Reset:
                    if (parts.Count != 2)
                        return false;

                    if (!IsValidDriveLetter(parts[1]))
                        return false;
                    else
                        DrivePath = parts[1];

                    break;

                case CommandStrings.SACD:
                    if (parts.Count < 4)
                        return false;

                    if (!IsValidDriveLetter(parts[1]))
                        return false;
                    else
                        DrivePath = parts[1];

                    if (IsFlagSupported(parts[2]))
                        return false;
                    else
                        Filename = parts[2];

                    if (!IsValidInt32(parts[3], lowerBound: 0, upperBound: 16))
                        return false;
                    else
                        DriveSpeed = Int32.Parse(parts[3]);

                    index = 4;
                    break;

                case CommandStrings.Start:
                    if (parts.Count != 2)
                        return false;

                    if (!IsValidDriveLetter(parts[1]))
                        return false;
                    else
                        DrivePath = parts[1];

                    break;

                case CommandStrings.Stop:
                    if (parts.Count != 2)
                        return false;

                    if (!IsValidDriveLetter(parts[1]))
                        return false;
                    else
                        DrivePath = parts[1];

                    break;

                case CommandStrings.Sub:
                    if (parts.Count != 2)
                        return false;

                    if (IsFlagSupported(parts[1]) || !File.Exists(parts[1]))
                        return false;
                    else
                        Filename = parts[1];

                    break;

                case CommandStrings.Swap:
                    if (parts.Count < 4)
                        return false;

                    if (!IsValidDriveLetter(parts[1]))
                        return false;
                    else
                        DrivePath = parts[1];

                    if (IsFlagSupported(parts[2]))
                        return false;
                    else
                        Filename = parts[2];

                    if (!IsValidInt32(parts[3], lowerBound: 0, upperBound: 72))
                        return false;
                    else
                        DriveSpeed = Int32.Parse(parts[3]);

                    index = 4;
                    break;

                case CommandStrings.Tape:
                    if (parts.Count != 2)
                        return false;

                    if (IsFlagSupported(parts[1]) || !File.Exists(parts[1]))
                        return false;
                    else
                        Filename = parts[1];

                    break;

                case CommandStrings.Version:
                    if (parts.Count != 1)
                        return false;

                    break;

                case CommandStrings.XBOX:
                    if (parts.Count < 4)
                        return false;

                    if (!IsValidDriveLetter(parts[1]))
                        return false;
                    else
                        DrivePath = parts[1];

                    if (IsFlagSupported(parts[2]))
                        return false;
                    else
                        Filename = parts[2];

                    if (!IsValidInt32(parts[3], lowerBound: 0, upperBound: 72))
                        return false;
                    else
                        DriveSpeed = Int32.Parse(parts[3]);

                    index = 4;
                    break;

                case CommandStrings.XBOXSwap:
                case CommandStrings.XGD2Swap:
                case CommandStrings.XGD3Swap:
                    if (parts.Count < 4)
                        return false;

                    if (!IsValidDriveLetter(parts[1]))
                        return false;
                    else
                        DrivePath = parts[1];

                    if (IsFlagSupported(parts[2]))
                        return false;
                    else
                        Filename = parts[2];

                    if (!IsValidInt32(parts[3], lowerBound: 0, upperBound: 72))
                        return false;
                    else
                        DriveSpeed = Int32.Parse(parts[3]);

                    for (int i = 4; i < parts.Count; i++)
                    {
                        if (!Int64.TryParse(parts[i], out long temp))
                            return false;
                    }

                    break;
                default:
                    return false;
            }

            // Loop through all auxiliary flags, if necessary
            if (index > 0)
            {
                for (int i = index; i < parts.Count; i++)
                {
                    // Flag read-out values
                    byte? byteValue = null;
                    int? intValue = null;
                    string? stringValue = null;

                    // Add Offset
                    intValue = ProcessInt32Parameter(parts, FlagStrings.AddOffset, ref i, missingAllowed: true);
                    if (intValue != null && intValue != Int32.MinValue)
                        AddOffsetValue = intValue;

                    // AMSF
                    ProcessFlagParameter(parts, FlagStrings.AMSF, ref i);

                    // Atari Jaguar
                    ProcessFlagParameter(parts, FlagStrings.AtariJaguar, ref i);

                    // BE Opcode
                    stringValue = ProcessStringParameter(parts, FlagStrings.BEOpcode, ref i);
                    if (!string.IsNullOrEmpty(stringValue))
                    {
                        if (string.Equals(stringValue, "raw") || string.Equals(stringValue, "pack"))
                            BEOpcodeValue = stringValue;
                        else
                            i--;
                    }

                    // C2 Opcode
                    if (parts[i] == FlagStrings.C2Opcode && IsFlagSupported(FlagStrings.C2Opcode))
                    {
                        this[FlagStrings.C2Opcode] = true;
                        for (int j = 0; j < C2OpcodeValue.Length; j++)
                        {
                            if (!DoesExist(parts, i + 1))
                            {
                                break;
                            }
                            else if (IsFlagSupported(parts[i + 1]))
                            {
                                break;
                            }
                            else if (!IsValidInt32(parts[i + 1], lowerBound: 0))
                            {
                                return false;
                            }
                            else
                            {
                                C2OpcodeValue[j] = Int32.Parse(parts[i + 1]);
                                i++;
                            }
                        }
                    }

                    // Copyright Management Information
                    ProcessFlagParameter(parts, FlagStrings.CopyrightManagementInformation, ref i);

                    // D8 Opcode
                    ProcessFlagParameter(parts, FlagStrings.D8Opcode, ref i);

                    // DAT Expand
                    ProcessFlagParameter(parts, FlagStrings.DatExpand, ref i);

                    // Disable Beep
                    ProcessFlagParameter(parts, FlagStrings.DisableBeep, ref i);

                    // DVD/HD-DVD/BD Reread
                    intValue = ProcessInt32Parameter(parts, FlagStrings.DVDReread, ref i, missingAllowed: true);
                    if (intValue != null && intValue != Int32.MinValue)
                        DVDRereadValue = intValue;

                    // Extract MS-CAB
                    ProcessFlagParameter(parts, FlagStrings.ExtractMicroSoftCabFile, ref i);

                    // Fix
                    intValue = ProcessInt32Parameter(parts, FlagStrings.Fix, ref i);
                    if (intValue != null && intValue != Int32.MinValue)
                        FixValue = intValue;

                    // Force Unit Access
                    intValue = ProcessInt32Parameter(parts, FlagStrings.ForceUnitAccess, ref i, missingAllowed: true);
                    if (intValue != null && intValue != Int32.MinValue && intValue >= 0)
                        ForceUnitAccessValue = intValue;

                    // Multi-Sector Read
                    intValue = ProcessInt32Parameter(parts, FlagStrings.MultiSectorRead, ref i, missingAllowed: true);
                    if (intValue != null && intValue != Int32.MinValue && intValue >= 0)
                        MultiSectorReadValue = intValue;

                    // NoFixSubP
                    ProcessFlagParameter(parts, FlagStrings.NoFixSubP, ref i);

                    // NoFixSubQ
                    ProcessFlagParameter(parts, FlagStrings.NoFixSubQ, ref i);

                    // NoFixSubQLibCrypt
                    ProcessFlagParameter(parts, FlagStrings.NoFixSubQLibCrypt, ref i);

                    // NoFixSubQSecuROM
                    ProcessFlagParameter(parts, FlagStrings.NoFixSubQSecuROM, ref i);

                    // NoFixSubRtoW
                    ProcessFlagParameter(parts, FlagStrings.NoFixSubRtoW, ref i);

                    // NoSkipSS
                    intValue = ProcessInt32Parameter(parts, FlagStrings.NoSkipSS, ref i, missingAllowed: true);
                    if (intValue != null && intValue != Int32.MinValue && intValue >= 0)
                        NoSkipSecuritySectorValue = intValue;

                    // PadSector
                    byteValue = ProcessUInt8Parameter(parts, FlagStrings.PadSector, ref i, missingAllowed: true);
                    if (byteValue != null)
                        PadSectorValue = byteValue;

                    // Range
                    if (parts[i] == FlagStrings.Range && IsFlagSupported(FlagStrings.Range))
                    {
                        // DVD specifically requires StartLBA and EndLBA
                        if (BaseCommand == CommandStrings.DigitalVideoDisc)
                        {
                            if (!DoesExist(parts, i + 1) || !DoesExist(parts, i + 2))
                                return false;
                            else if (!IsValidInt32(parts[i + 1], lowerBound: 0) || !IsValidInt32(parts[i + 2], lowerBound: 0))
                                return false;

                            RangeStartLBAValue = Int32.Parse(parts[i + 1]);
                            RangeEndLBAValue = Int32.Parse(parts[i + 2]);
                            i += 2;
                        }

                        this[FlagStrings.Reverse] = true;
                    }

                    // Raw
                    ProcessFlagParameter(parts, FlagStrings.Raw, ref i);

                    // Resume
                    ProcessFlagParameter(parts, FlagStrings.Resume, ref i);

                    // Reverse
                    if (parts[i] == FlagStrings.Reverse && IsFlagSupported(FlagStrings.Reverse))
                    {
                        // DVD specifically requires StartLBA and EndLBA
                        if (BaseCommand == CommandStrings.DigitalVideoDisc)
                        {
                            if (!DoesExist(parts, i + 1) || !DoesExist(parts, i + 2))
                                return false;
                            else if (!IsValidInt32(parts[i + 1], lowerBound: 0) || !IsValidInt32(parts[i + 2], lowerBound: 0))
                                return false;

                            ReverseStartLBAValue = Int32.Parse(parts[i + 1]);
                            ReverseEndLBAValue = Int32.Parse(parts[i + 2]);
                            i += 2;
                        }

                        this[FlagStrings.Reverse] = true;
                    }

                    // ScanAntiMod
                    ProcessFlagParameter(parts, FlagStrings.ScanAntiMod, ref i);

                    // ScanFileProtect
                    intValue = ProcessInt32Parameter(parts, FlagStrings.ScanFileProtect, ref i, missingAllowed: true);
                    if (intValue != null && intValue != Int32.MinValue && intValue >= 0)
                        ScanFileProtectValue = intValue;

                    // ScanSectorProtect
                    ProcessFlagParameter(parts, FlagStrings.ScanSectorProtect, ref i);

                    // SeventyFour
                    ProcessFlagParameter(parts, FlagStrings.SeventyFour, ref i);

                    // SkipSector
                    if (parts[i] == FlagStrings.SkipSector && IsFlagSupported(FlagStrings.SkipSector))
                    {
                        bool stillValid = true;
                        for (int j = 0; j < 2; j++)
                        {
                            if (!DoesExist(parts, i + 1))
                            {
                                break;
                            }
                            else if (IsFlagSupported(parts[i + 1]))
                            {
                                break;
                            }
                            else if (!IsValidInt32(parts[i + 1], lowerBound: 0))
                            {
                                stillValid = false;
                                break;
                            }
                            else
                            {
                                SkipSectorValue[j] = Int32.Parse(parts[i + 1]);
                                i++;
                            }
                        }

                        if (stillValid)
                            this[FlagStrings.SkipSector] = true;
                    }

                    // SubchannelReadLevel
                    intValue = ProcessInt32Parameter(parts, FlagStrings.SubchannelReadLevel, ref i, missingAllowed: true);
                    if (intValue != null && intValue != Int32.MinValue && intValue >= 0 && intValue <= 2)
                        SubchannelReadLevelValue = intValue;

                    // SeventyFour
                    ProcessFlagParameter(parts, FlagStrings.UseAnchorVolumeDescriptorPointer, ref i);

                    // VideoNow
                    intValue = ProcessInt32Parameter(parts, FlagStrings.VideoNow, ref i, missingAllowed: true);
                    if (intValue != null && intValue != Int32.MinValue && intValue >= 0)
                        VideoNowValue = intValue;

                    // VideoNowColor
                    ProcessFlagParameter(parts, FlagStrings.VideoNowColor, ref i);

                    // VideoNowXP
                    ProcessFlagParameter(parts, FlagStrings.VideoNowXP, ref i);
                }
            }

            return true;
        }

        #endregion

        #region Private Extra Methods

        /// <summary>
        /// Set the DIC command to be used for a given system and media type
        /// </summary>
        /// <param name="system">RedumpSystem value to check</param>
        /// <param name="type">MediaType value to check</param>
        private void SetBaseCommand(RedumpSystem? system, MediaType? type)
        {
            // If we have an invalid combination, we should BaseCommand = null
            if (!system.MediaTypes().Contains(type))
            {
                BaseCommand = null;
                return;
            }

            switch (type)
            {
                case MediaType.CDROM:
                    if (system == RedumpSystem.SuperAudioCD)
                        BaseCommand = CommandStrings.SACD;
                    else
                        BaseCommand = CommandStrings.CompactDisc;
                    return;
                case MediaType.DVD:
                    if (system == RedumpSystem.MicrosoftXbox
                        || system == RedumpSystem.MicrosoftXbox360)
                    {
                        BaseCommand = CommandStrings.XBOX;
                        return;
                    }
                    BaseCommand = CommandStrings.DigitalVideoDisc;
                    return;
                case MediaType.GDROM:
                    BaseCommand = CommandStrings.GDROM;
                    return;
                case MediaType.HDDVD:
                    BaseCommand = CommandStrings.DigitalVideoDisc;
                    return;
                case MediaType.BluRay:
                    BaseCommand = CommandStrings.BluRay;
                    return;
                case MediaType.NintendoGameCubeGameDisc:
                    BaseCommand = CommandStrings.DigitalVideoDisc;
                    return;
                case MediaType.NintendoWiiOpticalDisc:
                    BaseCommand = CommandStrings.DigitalVideoDisc;
                    return;
                case MediaType.FloppyDisk:
                    BaseCommand = CommandStrings.Floppy;
                    return;
                case MediaType.HardDisk:
                    BaseCommand = CommandStrings.Disk;
                    return;
                case MediaType.DataCartridge:
                    BaseCommand = CommandStrings.Tape;
                    return;

                default:
                    BaseCommand = null;
                    return;
            }
        }

        #endregion
    }
}
