using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using DICUI.Data;

namespace DICUI.Utilities
{
    /// <summary>
    /// Represents a generic set of DiscImageCreator parameters
    /// </summary>
    public class CreatorParameters
    {
        /// <summary>
        /// Base DiscImageCreator command to run
        /// </summary>
        public CreatorCommand Command { get; set; }

        /// <summary>
        /// Drive letter or path to pass to DiscImageCreator
        /// </summary>
        public string DriveLetter { get; set; }

        /// <summary>
        /// Drive speed to set, if applicable
        /// </summary>
        public int? DriveSpeed { get; set; }

        /// <summary>
        /// Destination filename for DiscImageCreator output
        /// </summary>
        public string Filename { get; set; }

        /// <summary>
        /// Optiarc drive output filename for merging
        /// </summary>
        public string OptiarcFilename { get; set; }

        /// <summary>
        /// Start LBA value for dumping specific sectors
        /// </summary>
        public int? StartLBAValue { get; set; }

        /// <summary>
        /// End LBA value for dumping specific sectors
        /// </summary>
        public int? EndLBAValue { get; set; }

        /// <summary>
        /// Set of flags to pass to DiscImageCreator
        /// </summary>
        private Dictionary<CreatorFlag, bool> _flags = new Dictionary<CreatorFlag, bool>();
        public bool this[CreatorFlag key]
        {
            get
            {
                if (_flags.ContainsKey(key))
                    return _flags[key];
                return false;
            }
            set
            {
                _flags[key] = value;
            }
        }
        internal IEnumerable<CreatorFlag> Keys => _flags.Keys;

        #region DiscImageCreator Flag Values

        /// <summary>
        /// Manual offset for Audio CD
        /// </summary>
        public int? AddOffsetValue { get; set; }

        /// <summary>
        /// 0xbe opcode value for dumping
        /// Possible values: raw (default), pack
        /// </summary>
        public string BEOpcodeValue { get; set; }

        /// <summary>
        /// C2 reread options for dumping
        /// [0] - Reread value
        /// [1] - 0 reread issue sector (default), 1 reread all
        /// [2] - First LBA to reread (default 0)
        /// [3] - Last LBA to reread (default EOS)
        /// </summary>
        public int?[] C2OpcodeValue { get; set; } = new int?[4];

        /// <summary>
        /// Set the force unit access flag value (default 1)
        /// </summary>
        public int? ForceUnitAccessValue { get; set; }

        /// <summary>
        /// Set the no skip security sector flag value (default 100)
        /// </summary>
        public int? NoSkipSecuritySectorValue { get; set; }

        /// <summary>
        /// Set scan file timeout value (default 60)
        /// </summary>
        public int? ScanFileProtectValue { get; set; }

        /// <summary>
        /// Beginning and ending sectors to skip for physical protection
        /// </summary>
        public int?[] SkipSectorValue { get; set; } = new int?[2];

        /// <summary>
        /// Set the subchanel read level
        /// Possible values: 0 no next sub, 1 next sub (default), 2 next and next next
        /// </summary>
        public int? SubchannelReadLevelValue { get; set; }

        /// <summary>
        /// Set number of empty bytes to insert at the head of first track for VideoNow
        /// </summary>
        public int? VideoNowValue { get; set; }

        #endregion

        /// <summary>
        /// Populate a CreatorParameters object from a param string
        /// </summary>
        /// <param name="parameters">String possibly representing a set of parameters</param>
        public CreatorParameters(string parameters)
        {
            // If any parameters are not valid, wipe out everything
            if (!ValidateAndSetParameters(parameters))
            {
                Command = CreatorCommand.NONE;

                DriveLetter = null;
                DriveSpeed = null;

                Filename = null;

                StartLBAValue = null;
                EndLBAValue = null;

                _flags = new Dictionary<CreatorFlag, bool>();

                AddOffsetValue = null;
                BEOpcodeValue = null;
                C2OpcodeValue = new int?[4];
                ForceUnitAccessValue = null;
                NoSkipSecuritySectorValue = null;
                ScanFileProtectValue = null;
                SubchannelReadLevelValue = null;
                VideoNowValue = null;
            }
        }

        /// <summary>
        /// Generate parameters based on a set of known inputs
        /// </summary>
        /// <param name="system">KnownSystem value to use</param>
        /// <param name="type">MediaType value to use</param>
        /// <param name="driveLetter">Drive letter to use</param>
        /// <param name="filename">Filename to use</param>
        /// <param name="driveSpeed">Drive speed to use</param>
        /// <param name="paranoid">Enable paranoid mode (safer dumping)</param>
        /// <param name="rereadCount">User-defined reread count</param>
        public CreatorParameters(KnownSystem? system, MediaType? type, char driveLetter, string filename, int? driveSpeed, bool paranoid, int rereadCount)
        {
            SetBaseCommand(system, type);
            DriveLetter = driveLetter.ToString();
            DriveSpeed = driveSpeed;
            Filename = filename;
            SetDefaultParameters(system, type, paranoid, rereadCount);
        }

        /// <summary>
        /// Determine the base flags to use for checking a commandline
        /// </summary>
        /// <param name="type">Output nullable MediaType containing the found MediaType, if possible</param>
        /// <param name="system">Output nullable KnownSystem containing the found KnownSystem, if possible</param>
        /// <param name="letter">Output string containing the found drive letter</param>
        /// <param name="path">Output string containing the found path</param>
        /// <returns>False on error (and all outputs set to null), true otherwise</returns>
        public bool DetermineFlags(out MediaType? type, out KnownSystem? system, out string letter, out string path)
        {
            // Populate all output variables with null
            type = null; system = null; letter = null; path = null;

            // If we're not already valid, output false
            if (!IsValid())
                return false;

            // Set the default outputs
            type = Converters.ToMediaType(Command);
            system = Converters.ToKnownSystem(Command);
            letter = DriveLetter;
            path = Filename;

            // Determine what the commandline should look like given the first item
            switch (Command)
            {
                case CreatorCommand.Audio:
                case CreatorCommand.CompactDisc:
                case CreatorCommand.Data:
                case CreatorCommand.DigitalVideoDisc:
                case CreatorCommand.GDROM:
                case CreatorCommand.Swap:
                    // GameCube and Wii
                    if (this[CreatorFlag.Raw])
                    {
                        type = MediaType.NintendoGameCubeGameDisc;
                        system = KnownSystem.NintendoGameCube;
                    }

                    // PlayStation
                    else if (this[CreatorFlag.NoFixSubQLibCrypt]
                        || this[CreatorFlag.ScanAntiMod])
                    {
                        type = MediaType.CDROM;
                        system = KnownSystem.SonyPlayStation;
                    }

                    // Saturn
                    else if (this[CreatorFlag.SeventyFour])
                    {
                        type = MediaType.CDROM;
                        system = KnownSystem.SegaSaturn;
                    }

                    break;
            }

            return true;
        }

        /// <summary>
        /// Blindly generate a parameter string based on the inputs
        /// </summary>
        /// <returns>Correctly formatted parameter string, null on error</returns>
        public string GenerateParameters()
        {
            List<string> parameters = new List<string>();

            if (Command != CreatorCommand.NONE)
                parameters.Add(Command.LongName());
            else
                return null;

            // Drive Letter
            if (Command == CreatorCommand.Audio
                || Command == CreatorCommand.BluRay
                || Command == CreatorCommand.Close
                || Command == CreatorCommand.CompactDisc
                || Command == CreatorCommand.Data
                || Command == CreatorCommand.DigitalVideoDisc
                || Command == CreatorCommand.Disk
                || Command == CreatorCommand.DriveSpeed
                || Command == CreatorCommand.Eject
                || Command == CreatorCommand.Floppy
                || Command == CreatorCommand.GDROM
                || Command == CreatorCommand.Reset
                || Command == CreatorCommand.SACD
                || Command == CreatorCommand.Start
                || Command == CreatorCommand.Stop
                || Command == CreatorCommand.Swap
                || Command == CreatorCommand.XBOX
                || Command == CreatorCommand.XBOXSwap
                || Command == CreatorCommand.XGD2Swap
                || Command == CreatorCommand.XGD3Swap)
            {
                if (DriveLetter != null)
                    parameters.Add(DriveLetter);
                else
                    return null;
            }

            // Filename
            if (Command == CreatorCommand.Audio
                || Command == CreatorCommand.BluRay
                || Command == CreatorCommand.CompactDisc
                || Command == CreatorCommand.Data
                || Command == CreatorCommand.DigitalVideoDisc
                || Command == CreatorCommand.Disk
                || Command == CreatorCommand.Floppy
                || Command == CreatorCommand.GDROM
                || Command == CreatorCommand.MDS
                || Command == CreatorCommand.Merge
                || Command == CreatorCommand.SACD
                || Command == CreatorCommand.Swap
                || Command == CreatorCommand.Sub
                || Command == CreatorCommand.XBOX
                || Command == CreatorCommand.XBOXSwap
                || Command == CreatorCommand.XGD2Swap
                || Command == CreatorCommand.XGD3Swap)
            {
                if (Filename != null)
                    parameters.Add("\"" + Filename.Trim('"') + "\"");
                else
                    return null;
            }

            // Optiarc Filename
            if (Command == CreatorCommand.Merge)
            {
                if (OptiarcFilename != null)
                    parameters.Add("\"" + OptiarcFilename.Trim('"') + "\"");
                else
                    return null;
            }

            // Drive Speed
            if (Command == CreatorCommand.Audio
                || Command == CreatorCommand.BluRay
                || Command == CreatorCommand.CompactDisc
                || Command == CreatorCommand.Data
                || Command == CreatorCommand.DigitalVideoDisc
                || Command == CreatorCommand.GDROM
                || Command == CreatorCommand.SACD
                || Command == CreatorCommand.Swap
                || Command == CreatorCommand.XBOX
                || Command == CreatorCommand.XBOXSwap
                || Command == CreatorCommand.XGD2Swap
                || Command == CreatorCommand.XGD3Swap)
            {
                if (DriveSpeed != null)
                    parameters.Add(DriveSpeed.ToString());
                else
                    return null;
            }

            // LBA Markers
            if (Command == CreatorCommand.Audio
                || Command == CreatorCommand.Data)
            {
                if (StartLBAValue != null && StartLBAValue > 0
                    && EndLBAValue != null && EndLBAValue > 0)
                {
                    parameters.Add(StartLBAValue.ToString());
                    parameters.Add(EndLBAValue.ToString());
                }
                else
                    return null;
            }

            // Add Offset
            if (Command == CreatorCommand.Audio
                || Command == CreatorCommand.CompactDisc)
            {
                if (this[CreatorFlag.AddOffset])
                {
                    parameters.Add(CreatorFlag.AddOffset.LongName());
                    if (AddOffsetValue != null)
                        parameters.Add(AddOffsetValue.ToString());
                    else
                        return null;
                }
            }

            // AMSF Dumping
            if (Command == CreatorCommand.CompactDisc)
            {
                if (this[CreatorFlag.AMSF])
                    parameters.Add(CreatorFlag.AMSF.LongName());
            }

            // Atari Jaguar CD
            if (Command == CreatorCommand.CompactDisc)
            {
                if (this[CreatorFlag.AtariJaguar])
                    parameters.Add(CreatorFlag.AtariJaguar.LongName());
            }

            // BE Opcode
            if (Command == CreatorCommand.Audio
               || Command == CreatorCommand.CompactDisc
               || Command == CreatorCommand.Data
               || Command == CreatorCommand.GDROM
               || Command == CreatorCommand.Swap)
            {
                if (this[CreatorFlag.BEOpcode] && !this[CreatorFlag.D8Opcode])
                {
                    parameters.Add(CreatorFlag.BEOpcode.LongName());
                    if (BEOpcodeValue != null
                        && (BEOpcodeValue == "raw" || BEOpcodeValue == "pack"))
                        parameters.Add(BEOpcodeValue);
                }
            }

            // C2 Opcode
            if (Command == CreatorCommand.Audio
               || Command == CreatorCommand.CompactDisc
               || Command == CreatorCommand.Data
               || Command == CreatorCommand.GDROM
               || Command == CreatorCommand.Swap)
            {
                if (this[CreatorFlag.C2Opcode])
                {
                    parameters.Add(CreatorFlag.C2Opcode.LongName());
                    if (C2OpcodeValue[0] != null)
                    {
                        if (C2OpcodeValue[0] > 0)
                            parameters.Add(C2OpcodeValue[0].ToString());
                        else
                            return null;
                    }
                    if (C2OpcodeValue[1] != null)
                    {
                        if (C2OpcodeValue[1] == 0)
                            parameters.Add(C2OpcodeValue[1].ToString());
                        else if (C2OpcodeValue[1] == 1)
                        {
                            parameters.Add(C2OpcodeValue[1].ToString());
                            if (C2OpcodeValue[2] != null && C2OpcodeValue[3] != null)
                            {
                                if (C2OpcodeValue[2] > 0 && C2OpcodeValue[3] > 0)
                                {
                                    parameters.Add(C2OpcodeValue[2].ToString());
                                    parameters.Add(C2OpcodeValue[3].ToString());
                                }
                                else
                                    return null;
                            }
                        }
                        else
                            return null;
                    }
                }
            }

            // Copyright Management Information
            if (Command == CreatorCommand.DigitalVideoDisc)
            {
                if (this[CreatorFlag.CopyrightManagementInformation])
                    parameters.Add(CreatorFlag.CopyrightManagementInformation.LongName());
            }

            // D8 Opcode
            if (Command == CreatorCommand.Audio
               || Command == CreatorCommand.CompactDisc
               || Command == CreatorCommand.Data
               || Command == CreatorCommand.GDROM
               || Command == CreatorCommand.Swap)
            {
                if (this[CreatorFlag.D8Opcode])
                    parameters.Add(CreatorFlag.D8Opcode.LongName());
            }

            // Disable Beep
            if (Command == CreatorCommand.Audio
               || Command == CreatorCommand.BluRay
               || Command == CreatorCommand.CompactDisc
               || Command == CreatorCommand.Data
               || Command == CreatorCommand.DigitalVideoDisc
               || Command == CreatorCommand.GDROM
               || Command == CreatorCommand.Swap
               || Command == CreatorCommand.XBOX)
            {
                if (this[CreatorFlag.DisableBeep])
                    parameters.Add(CreatorFlag.DisableBeep.LongName());
            }

            // Force Unit Access
            if (Command == CreatorCommand.Audio
               || Command == CreatorCommand.BluRay
               || Command == CreatorCommand.CompactDisc
               || Command == CreatorCommand.Data
               || Command == CreatorCommand.DigitalVideoDisc
               || Command == CreatorCommand.Swap
               || Command == CreatorCommand.XBOX)
            {
                if (this[CreatorFlag.ForceUnitAccess])
                {
                    parameters.Add(CreatorFlag.ForceUnitAccess.LongName());
                    if (ForceUnitAccessValue != null)
                        parameters.Add(ForceUnitAccessValue.ToString());
                }
            }

            // MCN
            if (Command == CreatorCommand.CompactDisc)
            {
                if (this[CreatorFlag.MCN])
                    parameters.Add(CreatorFlag.MCN.LongName());
            }

            // Multi-Session
            if (Command == CreatorCommand.CompactDisc)
            {
                if (this[CreatorFlag.MultiSession])
                    parameters.Add(CreatorFlag.MultiSession.LongName());
            }

            // Not fix SubP
            if (Command == CreatorCommand.Audio
               || Command == CreatorCommand.CompactDisc
               || Command == CreatorCommand.Data
               || Command == CreatorCommand.GDROM
               || Command == CreatorCommand.Swap)
            {
                if (this[CreatorFlag.NoFixSubP])
                    parameters.Add(CreatorFlag.NoFixSubP.LongName());
            }

            // Not fix SubQ
            if (Command == CreatorCommand.Audio
               || Command == CreatorCommand.CompactDisc
               || Command == CreatorCommand.Data
               || Command == CreatorCommand.GDROM
               || Command == CreatorCommand.Swap)
            {
                if (this[CreatorFlag.NoFixSubQ])
                    parameters.Add(CreatorFlag.NoFixSubQ.LongName());
            }

            // Not fix SubQ (PlayStation LibCrypt)
            if (Command == CreatorCommand.Audio
               || Command == CreatorCommand.CompactDisc
               || Command == CreatorCommand.Data
               || Command == CreatorCommand.GDROM
               || Command == CreatorCommand.Swap)
            {
                if (this[CreatorFlag.NoFixSubQLibCrypt])
                    parameters.Add(CreatorFlag.NoFixSubQLibCrypt.LongName());
            }
            
            // Not fix SubQ (SecuROM)
            if (Command == CreatorCommand.Audio
               || Command == CreatorCommand.CompactDisc
               || Command == CreatorCommand.Data
               || Command == CreatorCommand.GDROM
               || Command == CreatorCommand.Swap)
            {
                if (this[CreatorFlag.NoFixSubQSecuROM])
                    parameters.Add(CreatorFlag.NoFixSubQSecuROM.LongName());
            }

            // Not fix SubRtoW
            if (Command == CreatorCommand.Audio
               || Command == CreatorCommand.CompactDisc
               || Command == CreatorCommand.Data
               || Command == CreatorCommand.GDROM
               || Command == CreatorCommand.Swap)
            {
                if (this[CreatorFlag.NoFixSubRtoW])
                    parameters.Add(CreatorFlag.NoFixSubRtoW.LongName());
            }

            // Not skip security sectors
            if (Command == CreatorCommand.XBOX
                || Command == CreatorCommand.XBOXSwap
                || Command == CreatorCommand.XGD2Swap
                || Command == CreatorCommand.XGD3Swap)
            {
                if (this[CreatorFlag.NoSkipSS])
                {
                    parameters.Add(CreatorFlag.NoSkipSS.LongName());
                    if (NoSkipSecuritySectorValue != null)
                        parameters.Add(NoSkipSecuritySectorValue.ToString());
                }
            }

            // Raw read (2064 byte/sector)
            if (Command == CreatorCommand.DigitalVideoDisc)
            {
                if (this[CreatorFlag.Raw])
                    parameters.Add(CreatorFlag.Raw.LongName());
            }

            // Reverse read
            if (Command == CreatorCommand.CompactDisc
               || Command == CreatorCommand.Data
               || Command == CreatorCommand.DigitalVideoDisc)
            {
                if (this[CreatorFlag.Reverse])
                    parameters.Add(CreatorFlag.Reverse.LongName());
            }

            // Scan PlayStation anti-mod strings
            if (Command == CreatorCommand.CompactDisc
               || Command == CreatorCommand.Data)
            {
                if (this[CreatorFlag.ScanAntiMod])
                    parameters.Add(CreatorFlag.ScanAntiMod.LongName());
            }

            // Scan file to detect protect
            if (Command == CreatorCommand.Audio
               || Command == CreatorCommand.CompactDisc
               || Command == CreatorCommand.Data
               || Command == CreatorCommand.DigitalVideoDisc
               || Command == CreatorCommand.Swap)
            {
                if (this[CreatorFlag.ScanFileProtect])
                {
                    parameters.Add(CreatorFlag.ScanFileProtect.LongName());
                    if (ScanFileProtectValue != null)
                    {
                        if (ScanFileProtectValue > 0)
                            parameters.Add(ScanFileProtectValue.ToString());
                        else
                            return null;
                    }
                }
            }

            // Scan file to detect protect
            if (Command == CreatorCommand.CompactDisc
               || Command == CreatorCommand.Data
               || Command == CreatorCommand.Swap)
            {
                if (this[CreatorFlag.ScanSectorProtect])
                    parameters.Add(CreatorFlag.ScanSectorProtect.LongName());
            }

            // Scan 74:00:00 (Saturn)
            if (Command == CreatorCommand.Swap)
            {
                if (this[CreatorFlag.SeventyFour])
                    parameters.Add(CreatorFlag.SeventyFour.LongName());
            }

            // Skip sectors
            if (Command == CreatorCommand.Data)
            {
                if (this[CreatorFlag.SkipSector])
                {
                    if (SkipSectorValue[0] != null && SkipSectorValue[1] != null)
                    {
                        parameters.Add(CreatorFlag.SkipSector.LongName());
                        if (SkipSectorValue[0] >= 0 && SkipSectorValue[1] >= 0)
                        {
                            parameters.Add(SkipSectorValue[0].ToString());
                            parameters.Add(SkipSectorValue[1].ToString());
                        }
                        else
                            return null;
                    }
                    else
                        return null;
                }
            }

            // Set Subchannel read level
            if (Command == CreatorCommand.Audio
               || Command == CreatorCommand.CompactDisc
               || Command == CreatorCommand.Data
               || Command == CreatorCommand.GDROM
               || Command == CreatorCommand.Swap)
            {
                if (this[CreatorFlag.SubchannelReadLevel])
                {
                    parameters.Add(CreatorFlag.SubchannelReadLevel.LongName());
                    if (SubchannelReadLevelValue != null)
                    {
                        if (SubchannelReadLevelValue >= 0 && SubchannelReadLevelValue <= 2)
                            parameters.Add(SubchannelReadLevelValue.ToString());
                        else
                            return null;
                    }
                }
            }

            // VideoNow
            if (Command == CreatorCommand.CompactDisc)
            {
                if (this[CreatorFlag.VideoNow])
                {
                    parameters.Add(CreatorFlag.VideoNow.LongName());
                    if (VideoNowValue != null)
                    {
                        if (VideoNowValue >= 0)
                            parameters.Add(VideoNowValue.ToString());
                        else
                            return null;
                    }
                }
            }

            // VideoNow Color
            if (Command == CreatorCommand.CompactDisc)
            {
                if (this[CreatorFlag.VideoNowColor])
                    parameters.Add(CreatorFlag.VideoNowColor.LongName());
            }

            return string.Join(" ", parameters);
        }

        /// <summary>
        /// Returns if the current Parameter object is valid
        /// </summary>
        /// <returns></returns>
        public bool IsValid()
        {
            return GenerateParameters() != null;
        }

        /// <summary>
        /// Scan a possible parameter string and populate whatever possible
        /// </summary>
        /// <param name="parameters">String possibly representing parameters</param>
        /// <returns></returns>
        private bool ValidateAndSetParameters(string parameters)
        {
            // The string has to be valid by itself first
            if (string.IsNullOrWhiteSpace(parameters))
                return false;

            // Now split the string into parts for easier validation
            // https://stackoverflow.com/questions/14655023/split-a-string-that-has-white-spaces-unless-they-are-enclosed-within-quotes
            parameters = parameters.Trim();
            List<string> parts = Regex.Matches(parameters, @"[\""].+?[\""]|[^ ]+")
                .Cast<Match>()
                .Select(m => m.Value)
                .ToList();

            // Determine what the commandline should look like given the first item
            int index = -1;
            switch (parts[0])
            {
                case CreatorCommandStrings.Audio:
                    if (!DoesExist(parts, 1) || !IsValidDriveLetter(parts[1]))
                        return false;
                    else
                        DriveLetter = parts[1];

                    if (!DoesExist(parts, 2) || IsFlag(parts[2]))
                        return false;
                    else
                        Filename = parts[2];

                    if (!DoesExist(parts, 3) || !IsValidNumber(parts[3], lowerBound: 0, upperBound: 72))
                        return false;
                    else
                        DriveSpeed = Int32.Parse(parts[3]);

                    if (!DoesExist(parts, 4) || !IsValidNumber(parts[4], lowerBound: 0))
                        return false;
                    else
                        StartLBAValue = Int32.Parse(parts[4]);

                    if (!DoesExist(parts, 5) || !IsValidNumber(parts[5], lowerBound: 0))
                        return false;
                    else
                        EndLBAValue = Int32.Parse(parts[5]);

                    Command = CreatorCommand.Audio;
                    index = 6;
                    break;

                case CreatorCommandStrings.BluRay:
                    if (!DoesExist(parts, 1) || !IsValidDriveLetter(parts[1]))
                        return false;
                    else
                        DriveLetter = parts[1];

                    if (!DoesExist(parts, 2) || IsFlag(parts[2]))
                        return false;
                    else
                        Filename = parts[2];

                    if (!DoesExist(parts, 3) || !IsValidNumber(parts[3], lowerBound: 0, upperBound: 72))
                        return false;
                    else
                        DriveSpeed = Int32.Parse(parts[3]);

                    Command = CreatorCommand.BluRay;
                    index = 4;
                    break;

                case CreatorCommandStrings.Close:
                    if (!DoesExist(parts, 1) || !IsValidDriveLetter(parts[1]))
                        return false;
                    else
                        DriveLetter = parts[1];

                    if (parts.Count > 2)
                        return false;

                    Command = CreatorCommand.Close;
                    break;

                case CreatorCommandStrings.CompactDisc:
                    if (!DoesExist(parts, 1) || !IsValidDriveLetter(parts[1]))
                        return false;
                    else
                        DriveLetter = parts[1];

                    if (!DoesExist(parts, 2) || IsFlag(parts[2]))
                        return false;
                    else
                        Filename = parts[2];

                    if (!DoesExist(parts, 3) || !IsValidNumber(parts[3], lowerBound: 0, upperBound: 72))
                        return false;
                    else
                        DriveSpeed = Int32.Parse(parts[3]);

                    Command = CreatorCommand.CompactDisc;
                    index = 4;
                    break;

                case CreatorCommandStrings.Data:
                    if (!DoesExist(parts, 1) || !IsValidDriveLetter(parts[1]))
                        return false;
                    else
                        DriveLetter = parts[1];

                    if (!DoesExist(parts, 2) || IsFlag(parts[2]))
                        return false;
                    else
                        Filename = parts[2];

                    if (!DoesExist(parts, 3) || !IsValidNumber(parts[3], lowerBound: 0, upperBound: 72))
                        return false;
                    else
                        DriveSpeed = Int32.Parse(parts[3]);

                    if (!DoesExist(parts, 4) || !IsValidNumber(parts[4], lowerBound: 0))
                        return false;
                    else
                        StartLBAValue = Int32.Parse(parts[4]);

                    if (!DoesExist(parts, 5) || !IsValidNumber(parts[5], lowerBound: 0))
                        return false;
                    else
                        EndLBAValue = Int32.Parse(parts[5]);

                    Command = CreatorCommand.Data;
                    index = 6;
                    break;

                case CreatorCommandStrings.DigitalVideoDisc:
                    if (!DoesExist(parts, 1) || !IsValidDriveLetter(parts[1]))
                        return false;
                    else
                        DriveLetter = parts[1];

                    if (!DoesExist(parts, 2) || IsFlag(parts[2]))
                        return false;
                    else
                        Filename = parts[2];

                    if (!DoesExist(parts, 3) || !IsValidNumber(parts[3], lowerBound: 0, upperBound: 24)) // Officially 0-16
                        return false;
                    else
                        DriveSpeed = Int32.Parse(parts[3]);

                    Command = CreatorCommand.DigitalVideoDisc;
                    index = 4;
                    break;

                case CreatorCommandStrings.Disk:
                    if (!DoesExist(parts, 1) || !IsValidDriveLetter(parts[1]))
                        return false;
                    else
                        DriveLetter = parts[1];

                    if (!DoesExist(parts, 2) || IsFlag(parts[2]))
                        return false;
                    else
                        Filename = parts[2];

                    if (parts.Count > 3)
                        return false;

                    Command = CreatorCommand.Disk;
                    break;

                case CreatorCommandStrings.DriveSpeed:
                    if (!DoesExist(parts, 1) || !IsValidDriveLetter(parts[1]))
                        return false;
                    else
                        DriveLetter = parts[1];

                    if (parts.Count > 2)
                        return false;

                    Command = CreatorCommand.DriveSpeed;
                    break;

                case CreatorCommandStrings.Eject:
                    if (!DoesExist(parts, 1) || !IsValidDriveLetter(parts[1]))
                        return false;
                    else
                        DriveLetter = parts[1];

                    if (parts.Count > 2)
                        return false;

                    Command = CreatorCommand.Eject;
                    break;

                case CreatorCommandStrings.Floppy:
                    if (!DoesExist(parts, 1) || !IsValidDriveLetter(parts[1]))
                        return false;
                    else
                        DriveLetter = parts[1];

                    if (!DoesExist(parts, 2) || IsFlag(parts[2]))
                        return false;
                    else
                        Filename = parts[2];

                    if (parts.Count > 3)
                        return false;

                    Command = CreatorCommand.Floppy;
                    break;

                case CreatorCommandStrings.GDROM:
                    if (!DoesExist(parts, 1) || !IsValidDriveLetter(parts[1]))
                        return false;
                    else
                        DriveLetter = parts[1];

                    if (!DoesExist(parts, 2) || IsFlag(parts[2]))
                        return false;
                    else
                        Filename = parts[2];

                    if (!DoesExist(parts, 3) || !IsValidNumber(parts[3], lowerBound: 0, upperBound: 72))
                        return false;
                    else
                        DriveSpeed = Int32.Parse(parts[3]);

                    Command = CreatorCommand.GDROM;
                    index = 4;
                    break;

                case CreatorCommandStrings.MDS:
                    if (!DoesExist(parts, 1) || IsFlag(parts[1]) || !File.Exists(parts[1]))
                        return false;
                    else
                        Filename = parts[1];

                    if (parts.Count > 2)
                        return false;

                    Command = CreatorCommand.MDS;
                    break;

                case CreatorCommandStrings.Merge:
                    if (!DoesExist(parts, 1) || IsFlag(parts[1]) || !File.Exists(parts[1]))
                        return false;
                    else
                        Filename = parts[1];

                    if (!DoesExist(parts, 2) || IsFlag(parts[2]) || !File.Exists(parts[2]))
                        return false;
                    else
                        OptiarcFilename = parts[2];

                    if (parts.Count > 3)
                        return false;

                    Command = CreatorCommand.Merge;
                    break;

                case CreatorCommandStrings.Reset:
                    if (!DoesExist(parts, 1) || !IsValidDriveLetter(parts[1]))
                        return false;
                    else
                        DriveLetter = parts[1];

                    if (parts.Count > 2)
                        return false;

                    Command = CreatorCommand.Reset;
                    break;

                case CreatorCommandStrings.SACD:
                    if (!DoesExist(parts, 1) || !IsValidDriveLetter(parts[1]))
                        return false;
                    else
                        DriveLetter = parts[1];

                    if (!DoesExist(parts, 2) || IsFlag(parts[2]))
                        return false;
                    else
                        Filename = parts[2];

                    if (!DoesExist(parts, 3) || !IsValidNumber(parts[3], lowerBound: 0, upperBound: 16))
                        return false;
                    else
                        DriveSpeed = Int32.Parse(parts[3]);

                    if (parts.Count > 4)
                        return false;

                    Command = CreatorCommand.SACD;
                    break;

                case CreatorCommandStrings.Start:
                    if (!DoesExist(parts, 1) || !IsValidDriveLetter(parts[1]))
                        return false;
                    else
                        DriveLetter = parts[1];

                    if (parts.Count > 2)
                        return false;

                    Command = CreatorCommand.Start;
                    break;

                case CreatorCommandStrings.Stop:
                    if (!DoesExist(parts, 1) || !IsValidDriveLetter(parts[1]))
                        return false;
                    else
                        DriveLetter = parts[1];

                    if (parts.Count > 2)
                        return false;

                    Command = CreatorCommand.Stop;
                    break;

                case CreatorCommandStrings.Sub:
                    if (!DoesExist(parts, 1) || IsFlag(parts[1]) || !File.Exists(parts[1]))
                        return false;
                    else
                        Filename = parts[1];

                    if (parts.Count > 2)
                        return false;

                    Command = CreatorCommand.Sub;
                    break;

                case CreatorCommandStrings.Swap:
                    if (!DoesExist(parts, 1) || !IsValidDriveLetter(parts[1]))
                        return false;
                    else
                        DriveLetter = parts[1];

                    if (!DoesExist(parts, 2) || IsFlag(parts[2]))
                        return false;
                    else
                        Filename = parts[2];

                    if (!DoesExist(parts, 3) || !IsValidNumber(parts[3], lowerBound: 0, upperBound: 72))
                        return false;
                    else
                        DriveSpeed = Int32.Parse(parts[3]);

                    Command = CreatorCommand.Swap;
                    index = 4;
                    break;

                case CreatorCommandStrings.XBOX:
                    if (!DoesExist(parts, 1) || !IsValidDriveLetter(parts[1]))
                        return false;
                    else
                        DriveLetter = parts[1];

                    if (!DoesExist(parts, 2) || IsFlag(parts[2]))
                        return false;
                    else
                        Filename = parts[2];

                    if (!DoesExist(parts, 3) || !IsValidNumber(parts[3], lowerBound: 0, upperBound: 72))
                        return false;
                    else
                        DriveSpeed = Int32.Parse(parts[3]);

                    Command = CreatorCommand.XBOX;
                    index = 4;
                    break;

                case CreatorCommandStrings.XBOXSwap:
                case CreatorCommandStrings.XGD2Swap:
                case CreatorCommandStrings.XGD3Swap:
                    if (!DoesExist(parts, 1) || !IsValidDriveLetter(parts[1]))
                        return false;
                    else
                        DriveLetter = parts[1];

                    if (!DoesExist(parts, 2) || IsFlag(parts[2]))
                        return false;
                    else
                        Filename = parts[2];

                    if (!DoesExist(parts, 3) || !IsValidNumber(parts[3], lowerBound: 0, upperBound: 72))
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

            // Loop through all auxilary flags, if necessary
            if (index > 0)
            {
                for (int i = index; i < parts.Count; i++)
                {
                    switch (parts[i])
                    {
                        case CreatorFlagStrings.AddOffset:
                            if (parts[0] != CreatorCommandStrings.Audio
                                && parts[0] != CreatorCommandStrings.CompactDisc)
                                return false;
                            else if (!DoesExist(parts, i + 1))
                                return false;
                            else if (!IsValidNumber(parts[i + 1]))
                                return false;

                            this[CreatorFlag.AddOffset] = true;
                            AddOffsetValue = Int32.Parse(parts[i + 1]);
                            i++;
                            break;

                        case CreatorFlagStrings.AMSF:
                            if (parts[0] != CreatorCommandStrings.CompactDisc)
                                return false;

                            this[CreatorFlag.AMSF] = true;
                            break;

                        case CreatorFlagStrings.AtariJaguar:
                            if (parts[0] != CreatorCommandStrings.CompactDisc)
                                return false;

                            this[CreatorFlag.AtariJaguar] = true;
                            break;

                        case CreatorFlagStrings.BEOpcode:
                            if (parts[0] != CreatorCommandStrings.Audio
                                && parts[0] != CreatorCommandStrings.CompactDisc
                                && parts[0] != CreatorCommandStrings.Data
                                && parts[0] != CreatorCommandStrings.GDROM)
                                return false;
                            else if (!DoesExist(parts, i + 1))
                            {
                                this[CreatorFlag.BEOpcode] = true;
                                break;
                            }
                            else if (IsFlag(parts[i + 1]))
                            {
                                this[CreatorFlag.BEOpcode] = true;
                                break;
                            }
                            else if (parts[i + 1] != "raw" && (parts[i + 1] != "pack"))
                                return false;

                            this[CreatorFlag.BEOpcode] = true;
                            BEOpcodeValue = parts[i + 1];
                            i++;
                            break;

                        case CreatorFlagStrings.C2Opcode:
                            if (parts[0] != CreatorCommandStrings.Audio
                                && parts[0] != CreatorCommandStrings.CompactDisc
                                && parts[0] != CreatorCommandStrings.Data
                                && parts[0] != CreatorCommandStrings.GDROM)
                                return false;

                            this[CreatorFlag.C2Opcode] = true;
                            for (int j = 0; j < 4; j++)
                            {
                                if (!DoesExist(parts, i + 1))
                                    break;
                                else if (IsFlag(parts[i + 1]))
                                    break;
                                else if (!IsValidNumber(parts[i + 1], lowerBound: 0))
                                    return false;
                                else
                                {
                                    C2OpcodeValue[j] = Int32.Parse(parts[i + 1]);
                                    i++;
                                }
                            }

                            break;

                        case CreatorFlagStrings.CopyrightManagementInformation:
                            if (parts[0] != CreatorCommandStrings.DigitalVideoDisc)
                                return false;

                            this[CreatorFlag.CopyrightManagementInformation] = true;
                            break;

                        case CreatorFlagStrings.D8Opcode:
                            if (parts[0] != CreatorCommandStrings.Audio
                                && parts[0] != CreatorCommandStrings.CompactDisc
                                && parts[0] != CreatorCommandStrings.Data
                                && parts[0] != CreatorCommandStrings.GDROM)
                                return false;

                            this[CreatorFlag.D8Opcode] = true;
                            break;

                        case CreatorFlagStrings.DisableBeep:
                            if (parts[0] != CreatorCommandStrings.Audio
                                && parts[0] != CreatorCommandStrings.BluRay
                                && parts[0] != CreatorCommandStrings.CompactDisc
                                && parts[0] != CreatorCommandStrings.Data
                                && parts[0] != CreatorCommandStrings.DigitalVideoDisc
                                && parts[0] != CreatorCommandStrings.GDROM
                                && parts[0] != CreatorCommandStrings.XBOX)
                                return false;

                            this[CreatorFlag.DisableBeep] = true;
                            break;

                        case CreatorFlagStrings.ForceUnitAccess:
                            if (parts[0] != CreatorCommandStrings.Audio
                               && parts[0] != CreatorCommandStrings.BluRay
                               && parts[0] != CreatorCommandStrings.CompactDisc
                               && parts[0] != CreatorCommandStrings.DigitalVideoDisc
                               && parts[0] != CreatorCommandStrings.Data
                               && parts[0] != CreatorCommandStrings.GDROM
                               && parts[0] != CreatorCommandStrings.XBOX)
                                return false;
                            else if (!DoesExist(parts, i + 1))
                            {
                                this[CreatorFlag.ForceUnitAccess] = true;
                                break;
                            }
                            else if (IsFlag(parts[i + 1]))
                            {
                                this[CreatorFlag.ForceUnitAccess] = true;
                                break;
                            }
                            else if (!IsValidNumber(parts[i + 1], lowerBound: 0))
                                return false;

                            this[CreatorFlag.ForceUnitAccess] = true;
                            ForceUnitAccessValue = Int32.Parse(parts[i + 1]);
                            i++;
                            break;

                        case CreatorFlagStrings.MCN:
                            if (parts[0] != CreatorCommandStrings.CompactDisc)
                                return false;

                            this[CreatorFlag.MCN] = true;
                            break;

                        case CreatorFlagStrings.MultiSession:
                            if (parts[0] != CreatorCommandStrings.CompactDisc)
                                return false;

                            this[CreatorFlag.MultiSession] = true;
                            break;

                        case CreatorFlagStrings.NoFixSubP:
                            if (parts[0] != CreatorCommandStrings.Audio
                                && parts[0] != CreatorCommandStrings.CompactDisc
                                && parts[0] != CreatorCommandStrings.Data
                                && parts[0] != CreatorCommandStrings.GDROM)
                                return false;

                            this[CreatorFlag.NoFixSubP] = true;
                            break;

                        case CreatorFlagStrings.NoFixSubQ:
                            if (parts[0] != CreatorCommandStrings.Audio
                                && parts[0] != CreatorCommandStrings.CompactDisc
                                && parts[0] != CreatorCommandStrings.Data
                                && parts[0] != CreatorCommandStrings.GDROM)
                                return false;

                            this[CreatorFlag.NoFixSubQ] = true;
                            break;

                        case CreatorFlagStrings.NoFixSubQLibCrypt:
                            if (parts[0] != CreatorCommandStrings.CompactDisc)
                                return false;

                            this[CreatorFlag.NoFixSubQLibCrypt] = true;
                            break;

                        case CreatorFlagStrings.NoFixSubRtoW:
                            if (parts[0] != CreatorCommandStrings.Audio
                                && parts[0] != CreatorCommandStrings.CompactDisc
                                && parts[0] != CreatorCommandStrings.Data
                                && parts[0] != CreatorCommandStrings.GDROM)
                                return false;

                            this[CreatorFlag.NoFixSubRtoW] = true;
                            break;

                        case CreatorFlagStrings.NoFixSubQSecuROM:
                            if (parts[0] != CreatorCommandStrings.Audio
                                && parts[0] != CreatorCommandStrings.CompactDisc
                                && parts[0] != CreatorCommandStrings.Data
                                && parts[0] != CreatorCommandStrings.GDROM)
                                return false;

                            this[CreatorFlag.NoFixSubQSecuROM] = true;
                            break;

                        case CreatorFlagStrings.NoSkipSS:
                            if (parts[0] != CreatorCommandStrings.XBOX
                                && parts[0] != CreatorCommandStrings.XBOXSwap
                                && parts[0] != CreatorCommandStrings.XGD2Swap
                                && parts[0] != CreatorCommandStrings.XGD3Swap)
                                return false;
                            else if (!DoesExist(parts, i + 1))
                            {
                                this[CreatorFlag.NoSkipSS] = true;
                                break;
                            }
                            else if (IsFlag(parts[i + 1]))
                            {
                                this[CreatorFlag.NoSkipSS] = true;
                                break;
                            }
                            else if (!IsValidNumber(parts[i + 1], lowerBound: 0))
                                return false;

                            this[CreatorFlag.NoSkipSS] = true;
                            ForceUnitAccessValue = Int32.Parse(parts[i + 1]);
                            i++;
                            break;

                        case CreatorFlagStrings.Raw:
                            if (parts[0] != CreatorCommandStrings.DigitalVideoDisc)
                                return false;

                            this[CreatorFlag.Raw] = true;
                            break;

                        case CreatorFlagStrings.Reverse:
                            if (parts[0] != CreatorCommandStrings.CompactDisc
                                && parts[0] != CreatorCommandStrings.Data
                                && parts[0] != CreatorCommandStrings.DigitalVideoDisc)
                                return false;

                            this[CreatorFlag.Reverse] = true;
                            break;

                        case CreatorFlagStrings.ScanAntiMod:
                            if (parts[0] != CreatorCommandStrings.CompactDisc)
                                return false;

                            this[CreatorFlag.ScanAntiMod] = true;
                            break;

                        case CreatorFlagStrings.ScanFileProtect:
                            if (parts[0] != CreatorCommandStrings.CompactDisc
                                && parts[0] != CreatorCommandStrings.Data
                                && parts[0] != CreatorCommandStrings.DigitalVideoDisc)
                                return false;
                            else if (!DoesExist(parts, i + 1))
                            {
                                this[CreatorFlag.ScanFileProtect] = true;
                                break;
                            }
                            else if (IsFlag(parts[i + 1]))
                            {
                                this[CreatorFlag.ScanFileProtect] = true;
                                break;
                            }
                            else if (!IsValidNumber(parts[i + 1], lowerBound: 0))
                                return false;

                            this[CreatorFlag.ScanFileProtect] = true;
                            ScanFileProtectValue = Int32.Parse(parts[i + 1]);
                            i++;
                            break;

                        case CreatorFlagStrings.ScanSectorProtect:
                            if (parts[0] != CreatorCommandStrings.CompactDisc
                                && parts[0] != CreatorCommandStrings.Data)
                                return false;

                            this[CreatorFlag.ScanSectorProtect] = true;
                            break;

                        case CreatorFlagStrings.SeventyFour:
                            if (parts[0] != CreatorCommandStrings.Swap)
                                return false;

                            this[CreatorFlag.SeventyFour] = true;
                            break;

                        case CreatorFlagStrings.SkipSector:
                            if (parts[0] != CreatorCommandStrings.Data)
                                return false;
                            else if (!DoesExist(parts, i + 1) || !DoesExist(parts, i + 2))
                                return false;
                            else if (IsFlag(parts[i + 1]) || IsFlag(parts[i + 2]))
                                return false;
                            else if (!IsValidNumber(parts[i + 1], lowerBound: 0) || !IsValidNumber(parts[i + 2], lowerBound: 0))
                                return false;

                            this[CreatorFlag.SkipSector] = true;
                            SkipSectorValue[0] = Int32.Parse(parts[i + 1]);
                            SkipSectorValue[1] = Int32.Parse(parts[i + 2]);
                            i += 2;
                            break;

                        case CreatorFlagStrings.SubchannelReadLevel:
                            if (parts[0] != CreatorCommandStrings.Audio
                                && parts[0] != CreatorCommandStrings.CompactDisc
                                && parts[0] != CreatorCommandStrings.Data
                                && parts[0] != CreatorCommandStrings.GDROM)
                                return false;
                            else if (!DoesExist(parts, i + 1))
                            {
                                this[CreatorFlag.SubchannelReadLevel] = true;
                                break;
                            }
                            else if (IsFlag(parts[i + 1]))
                            {
                                this[CreatorFlag.SubchannelReadLevel] = true;
                                break;
                            }
                            else if (!IsValidNumber(parts[i + 1], lowerBound: 0, upperBound: 2))
                                return false;

                            this[CreatorFlag.SubchannelReadLevel] = true;
                            SubchannelReadLevelValue = Int32.Parse(parts[i + 1]);
                            i++;
                            break;

                        case CreatorFlagStrings.VideoNow:
                            if (parts[0] != CreatorCommandStrings.CompactDisc)
                                return false;
                            else if (!DoesExist(parts, i + 1))
                            {
                                this[CreatorFlag.VideoNow] = true;
                                break;
                            }
                            else if (IsFlag(parts[i + 1]))
                            {
                                this[CreatorFlag.VideoNow] = true;
                                break;
                            }
                            else if (!IsValidNumber(parts[i + 1], lowerBound: 0))
                                return false;

                            this[CreatorFlag.VideoNow] = true;
                            VideoNowValue = Int32.Parse(parts[i + 1]);
                            i++;
                            break;

                        case CreatorFlagStrings.VideoNowColor:
                            if (parts[0] != CreatorCommandStrings.CompactDisc)
                                return false;

                            this[CreatorFlag.VideoNowColor] = true;
                            break;

                        default:
                            return false;
                    }
                }
            }

            return true;
        }

        /// <summary>
        /// Returns whether a string is a valid drive letter
        /// </summary>
        /// <param name="parameter">String value to check</param>
        /// <returns>True if it's a valid drive letter, false otherwise</returns>
        private bool IsValidDriveLetter(string parameter)
        {
            if (!Regex.IsMatch(parameter, @"^[A-Z]:?\\?$"))
                return false;

            return true;
        }

        /// <summary>
        /// Returns whether a string is a flag (starts with '/')
        /// </summary>
        /// <param name="parameter">String value to check</param>
        /// <returns>True if it's a flag, false otherwise</returns>
        private bool IsFlag(string parameter)
        {
            if (parameter.Trim('\"').StartsWith("/"))
                return true;

            return false;
        }

        /// <summary>
        /// Returns whether or not the selected item exists
        /// </summary>
        /// <param name="parameters">List of parameters to check against</param>
        /// <param name="index">Current index</param>
        /// <returns>True if the next item exists, false otherwise</returns>
        private bool DoesExist(List<string> parameters, int index)
        {
            if (index >= parameters.Count)
                return false;

            return true;
        }

        /// <summary>
        /// Returns whether a string is a valid number
        /// </summary>
        /// <param name="parameter">String value to check</param>
        /// <param name="lowerBound">Lower bound (>=)</param>
        /// <param name="upperBound">Upper bound (<=)</param>
        /// <returns>True if it's a valid number, false otherwise</returns>
        private bool IsValidNumber(string parameter, int lowerBound = -1, int upperBound = -1)
        {
            if (!Int32.TryParse(parameter, out int temp))
                return false;
            else if (lowerBound != -1 && temp < lowerBound)
                return false;
            else if (upperBound != -1 && temp > upperBound)
                return false;

            return true;
        }

        /// <summary>
        /// Set the DIC command to be used for a given system and media type
        /// </summary>
        /// <param name="system">KnownSystem value to check</param>
        /// <param name="type">MediaType value to check</param>
        private void SetBaseCommand(KnownSystem? system, MediaType? type)
        {
            // If we have an invalid combination, we should Command = null
            if (!Validators.GetValidMediaTypes(system).Contains(type))
            {
                Command = CreatorCommand.NONE;
                return;
            }

            switch (type)
            {
                case MediaType.CDROM:
                    if (system == KnownSystem.SuperAudioCD)
                        Command = CreatorCommand.SACD;
                    else
                        Command = CreatorCommand.CompactDisc;
                    return;
                case MediaType.DVD:
                    if (system == KnownSystem.MicrosoftXBOX
                        || system == KnownSystem.MicrosoftXBOX360)
                    {
                        Command = CreatorCommand.XBOX;
                        return;
                    }
                    Command = CreatorCommand.DigitalVideoDisc;
                    return;
                case MediaType.GDROM:
                    Command = CreatorCommand.GDROM;
                    return;
                case MediaType.HDDVD:
                    Command = CreatorCommand.DigitalVideoDisc;
                    return;
                case MediaType.BluRay:
                    Command = CreatorCommand.BluRay;
                    return;
                case MediaType.NintendoGameCubeGameDisc:
                    Command = CreatorCommand.DigitalVideoDisc;
                    return;
                case MediaType.NintendoWiiOpticalDisc:
                    Command = CreatorCommand.DigitalVideoDisc;
                    return;
                case MediaType.FloppyDisk:
                    Command = CreatorCommand.Floppy;
                    return;
                case MediaType.HardDisk:
                    Command = CreatorCommand.Disk;
                    return;

                default:
                    Command = CreatorCommand.NONE;
                    return;
            }
        }

        /// <summary>
        /// Set default parameters for a given system and media type
        /// </summary>
        /// <param name="system">KnownSystem value to check</param>
        /// <param name="type">MediaType value to check</param>
        /// <param name="paranoid">Enable paranoid mode (safer dumping)</param>
        /// <param name="rereadCount">User-defined reread count</param>
        private void SetDefaultParameters(KnownSystem? system, MediaType? type, bool paranoid, int rereadCount)
        {
            // First check to see if the combination of system and MediaType is valid
            var validTypes = Validators.GetValidMediaTypes(system);
            if (!validTypes.Contains(type))
                return;

            // Set the C2 reread count
            switch (rereadCount)
            {
                case -1:
                    C2OpcodeValue[0] = null;
                    break;
                case 0:
                    C2OpcodeValue[0] = 20;
                    break;
                default:
                    C2OpcodeValue[0] = rereadCount;
                    break;
            }

            // Now sort based on disc type
            switch (type)
            {
                case MediaType.CDROM:
                    this[CreatorFlag.C2Opcode] = true;

                    switch (system)
                    {
                        case KnownSystem.AppleMacintosh:
                        case KnownSystem.IBMPCCompatible:
                            this[CreatorFlag.NoFixSubQSecuROM] = true;
                            this[CreatorFlag.ScanFileProtect] = true;

                            if (paranoid)
                            {
                                this[CreatorFlag.ScanSectorProtect] = true;
                                this[CreatorFlag.SubchannelReadLevel] = true;
                                SubchannelReadLevelValue = 2;
                            }
                            break;
                        case KnownSystem.AtariJaguarCD:
                            this[CreatorFlag.AtariJaguar] = true;
                            break;
                        case KnownSystem.HasbroVideoNow:
                        case KnownSystem.HasbroVideoNowJr:
                            this[CreatorFlag.VideoNow] = true;
                            this.VideoNowValue = 18032;
                            break;
                        case KnownSystem.HasbroVideoNowColor:
                            this[CreatorFlag.VideoNowColor] = true;
                            break;
                        case KnownSystem.HasbroVideoNowXP:
                            this[CreatorFlag.VideoNow] = true;
                            this.VideoNowValue = 20832;
                            break;
                        case KnownSystem.NECPCEngineTurboGrafxCD:
                            this[CreatorFlag.MCN] = true;
                            break;
                        case KnownSystem.SonyPlayStation:
                            this[CreatorFlag.ScanAntiMod] = true;
                            this[CreatorFlag.NoFixSubQLibCrypt] = true;
                            break;
                    }
                    break;
                case MediaType.DVD:
                    if (paranoid)
                    {
                        this[CreatorFlag.CopyrightManagementInformation] = true;
                        this[CreatorFlag.ScanFileProtect] = true;
                    }
                    break;
                case MediaType.GDROM:
                    this[CreatorFlag.C2Opcode] = true;
                    break;
                case MediaType.HDDVD:
                    if (paranoid)
                        this[CreatorFlag.CopyrightManagementInformation] = true;
                    break;
                case MediaType.BluRay:
                    // Currently no defaults set
                    break;

                // Special Formats
                case MediaType.NintendoGameCubeGameDisc:
                    this[CreatorFlag.Raw] = true;
                    break;
                case MediaType.NintendoWiiOpticalDisc:
                    this[CreatorFlag.Raw] = true;
                    break;

                // Non-optical
                case MediaType.FloppyDisk:
                    // Currently no defaults set
                    break;
            }
        }
    }
}
