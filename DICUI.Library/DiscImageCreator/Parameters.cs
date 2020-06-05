using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using BurnOutSharp.External.psxt001z;
using DICUI.Data;
using DICUI.Utilities;

namespace DICUI.DiscImageCreator
{
    /// <summary>
    /// Represents a generic set of DiscImageCreator parameters
    /// </summary>
    public class Parameters : BaseParameters
    {
        /// <summary>
        /// Base command to run
        /// </summary>
        public Command BaseCommand { get; set; }

        /// <summary>
        /// Set of flags to pass to the executable
        /// </summary>
        protected Dictionary<Flag, bool?> _flags = new Dictionary<Flag, bool?>();
        public bool? this[Flag key]
        {
            get
            {
                if (_flags.ContainsKey(key))
                    return _flags[key];

                return null;
            }
            set
            {
                _flags[key] = value;
            }
        }
        protected internal IEnumerable<Flag> Keys => _flags.Keys;

        #region Common Input Values

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

        #endregion

        #region Flag Values

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
        /// Populate a Parameters object from a param string
        /// </summary>
        /// <param name="parameters">String possibly representing a set of parameters</param>
        public Parameters(string parameters)
            : base(parameters)
        {
            this.InternalProgram = InternalProgram.DiscImageCreator;
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
        /// <param name="quietMode">Enable quiet mode (no beeps)</param>
        /// <param name="retryCount">User-defined reread count</param>
        public Parameters(KnownSystem? system, MediaType? type, char driveLetter, string filename, int? driveSpeed, bool paranoid, bool quietMode, int retryCount)
            : base(system, type, driveLetter, filename, driveSpeed, paranoid, quietMode, retryCount)
        {
            if (quietMode)
                this[Flag.DisableBeep] = true;
        }

        /// <summary>
        /// Blindly generate a parameter string based on the inputs
        /// </summary>
        /// <returns>Correctly formatted parameter string, null on error</returns>
        public override string GenerateParameters()
        {
            List<string> parameters = new List<string>();

            if (BaseCommand != Command.NONE)
                parameters.Add(Converters.LongName(BaseCommand));
            else
                return null;

            // Drive Letter
            if (BaseCommand == Command.Audio
                || BaseCommand == Command.BluRay
                || BaseCommand == Command.Close
                || BaseCommand == Command.CompactDisc
                || BaseCommand == Command.Data
                || BaseCommand == Command.DigitalVideoDisc
                || BaseCommand == Command.Disk
                || BaseCommand == Command.DriveSpeed
                || BaseCommand == Command.Eject
                || BaseCommand == Command.Floppy
                || BaseCommand == Command.GDROM
                || BaseCommand == Command.Reset
                || BaseCommand == Command.SACD
                || BaseCommand == Command.Start
                || BaseCommand == Command.Stop
                || BaseCommand == Command.Swap
                || BaseCommand == Command.XBOX
                || BaseCommand == Command.XBOXSwap
                || BaseCommand == Command.XGD2Swap
                || BaseCommand == Command.XGD3Swap)
            {
                if (DriveLetter != null)
                    parameters.Add(DriveLetter);
                else
                    return null;
            }

            // Filename
            if (BaseCommand == Command.Audio
                || BaseCommand == Command.BluRay
                || BaseCommand == Command.CompactDisc
                || BaseCommand == Command.Data
                || BaseCommand == Command.DigitalVideoDisc
                || BaseCommand == Command.Disk
                || BaseCommand == Command.Floppy
                || BaseCommand == Command.GDROM
                || BaseCommand == Command.MDS
                || BaseCommand == Command.Merge
                || BaseCommand == Command.SACD
                || BaseCommand == Command.Swap
                || BaseCommand == Command.Sub
                || BaseCommand == Command.XBOX
                || BaseCommand == Command.XBOXSwap
                || BaseCommand == Command.XGD2Swap
                || BaseCommand == Command.XGD3Swap)
            {
                if (Filename != null)
                    parameters.Add("\"" + Filename.Trim('"') + "\"");
                else
                    return null;
            }

            // Optiarc Filename
            if (BaseCommand == Command.Merge)
            {
                if (OptiarcFilename != null)
                    parameters.Add("\"" + OptiarcFilename.Trim('"') + "\"");
                else
                    return null;
            }

            // Drive Speed
            if (BaseCommand == Command.Audio
                || BaseCommand == Command.BluRay
                || BaseCommand == Command.CompactDisc
                || BaseCommand == Command.Data
                || BaseCommand == Command.DigitalVideoDisc
                || BaseCommand == Command.GDROM
                || BaseCommand == Command.SACD
                || BaseCommand == Command.Swap
                || BaseCommand == Command.XBOX
                || BaseCommand == Command.XBOXSwap
                || BaseCommand == Command.XGD2Swap
                || BaseCommand == Command.XGD3Swap)
            {
                if (DriveSpeed != null)
                    parameters.Add(DriveSpeed.ToString());
                else
                    return null;
            }

            // LBA Markers
            if (BaseCommand == Command.Audio
                || BaseCommand == Command.Data)
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
            if (GetSupportedCommands(Flag.AddOffset).Contains(BaseCommand))
            {
                if (this[Flag.AddOffset] == true)
                {
                    parameters.Add(Converters.LongName(Flag.AddOffset));
                    if (AddOffsetValue != null)
                        parameters.Add(AddOffsetValue.ToString());
                    else
                        return null;
                }
            }

            // AMSF Dumping
            if (GetSupportedCommands(Flag.AMSF).Contains(BaseCommand))
            {
                if (this[Flag.AMSF] == true)
                    parameters.Add(Converters.LongName(Flag.AMSF));
            }

            // Atari Jaguar CD
            if (GetSupportedCommands(Flag.AtariJaguar).Contains(BaseCommand))
            {
                if (this[Flag.AtariJaguar] == true)
                    parameters.Add(Converters.LongName(Flag.AtariJaguar));
            }

            // BE Opcode
            if (GetSupportedCommands(Flag.BEOpcode).Contains(BaseCommand))
            {
                if (this[Flag.BEOpcode] == true && this[Flag.D8Opcode] != true)
                {
                    parameters.Add(Converters.LongName(Flag.BEOpcode));
                    if (BEOpcodeValue != null
                        && (BEOpcodeValue == "raw" || BEOpcodeValue == "pack"))
                        parameters.Add(BEOpcodeValue);
                }
            }

            // C2 Opcode
            if (GetSupportedCommands(Flag.C2Opcode).Contains(BaseCommand))
            {
                if (this[Flag.C2Opcode] == true)
                {
                    parameters.Add(Converters.LongName(Flag.C2Opcode));
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
            if (GetSupportedCommands(Flag.CopyrightManagementInformation).Contains(BaseCommand))
            {
                if (this[Flag.CopyrightManagementInformation] == true)
                    parameters.Add(Converters.LongName(Flag.CopyrightManagementInformation));
            }

            // D8 Opcode
            if (GetSupportedCommands(Flag.D8Opcode).Contains(BaseCommand))
            {
                if (this[Flag.D8Opcode] == true)
                    parameters.Add(Converters.LongName(Flag.D8Opcode));
            }

            // Disable Beep
            if (GetSupportedCommands(Flag.DisableBeep).Contains(BaseCommand))
            {
                if (this[Flag.DisableBeep] == true)
                    parameters.Add(Converters.LongName(Flag.DisableBeep));
            }

            // Force Unit Access
            if (GetSupportedCommands(Flag.ForceUnitAccess).Contains(BaseCommand))
            {
                if (this[Flag.ForceUnitAccess] == true)
                {
                    parameters.Add(Converters.LongName(Flag.ForceUnitAccess));
                    if (ForceUnitAccessValue != null)
                        parameters.Add(ForceUnitAccessValue.ToString());
                }
            }

            // Multi-Session
            if (GetSupportedCommands(Flag.MultiSession).Contains(BaseCommand))
            {
                if (this[Flag.MultiSession] == true)
                    parameters.Add(Converters.LongName(Flag.MultiSession));
            }

            // Not fix SubP
            if (GetSupportedCommands(Flag.NoFixSubP).Contains(BaseCommand))
            {
                if (this[Flag.NoFixSubP] == true)
                    parameters.Add(Converters.LongName(Flag.NoFixSubP));
            }

            // Not fix SubQ
            if (GetSupportedCommands(Flag.NoFixSubQ).Contains(BaseCommand))
            {
                if (this[Flag.NoFixSubQ] == true)
                    parameters.Add(Converters.LongName(Flag.NoFixSubQ));
            }

            // Not fix SubQ (PlayStation LibCrypt)
            if (GetSupportedCommands(Flag.NoFixSubQLibCrypt).Contains(BaseCommand))
            {
                if (this[Flag.NoFixSubQLibCrypt] == true)
                    parameters.Add(Converters.LongName(Flag.NoFixSubQLibCrypt));
            }

            // Not fix SubQ (SecuROM)
            if (GetSupportedCommands(Flag.NoFixSubQSecuROM).Contains(BaseCommand))
            {
                if (this[Flag.NoFixSubQSecuROM] == true)
                    parameters.Add(Converters.LongName(Flag.NoFixSubQSecuROM));
            }

            // Not fix SubRtoW
            if (GetSupportedCommands(Flag.NoFixSubRtoW).Contains(BaseCommand))
            {
                if (this[Flag.NoFixSubRtoW] == true)
                    parameters.Add(Converters.LongName(Flag.NoFixSubRtoW));
            }

            // Not skip security sectors
            if (GetSupportedCommands(Flag.NoSkipSS).Contains(BaseCommand))
            {
                if (this[Flag.NoSkipSS] == true)
                {
                    parameters.Add(Converters.LongName(Flag.NoSkipSS));
                    if (NoSkipSecuritySectorValue != null)
                        parameters.Add(NoSkipSecuritySectorValue.ToString());
                }
            }

            // Raw read (2064 byte/sector)
            if (GetSupportedCommands(Flag.Raw).Contains(BaseCommand))
            {
                if (this[Flag.Raw] == true)
                    parameters.Add(Converters.LongName(Flag.Raw));
            }

            // Reverse read
            if (GetSupportedCommands(Flag.Reverse).Contains(BaseCommand))
            {
                if (this[Flag.Reverse] == true)
                    parameters.Add(Converters.LongName(Flag.Reverse));
            }

            // Scan PlayStation anti-mod strings
            if (GetSupportedCommands(Flag.ScanAntiMod).Contains(BaseCommand))
            {
                if (this[Flag.ScanAntiMod] == true)
                    parameters.Add(Converters.LongName(Flag.ScanAntiMod));
            }

            // Scan file to detect protect
            if (GetSupportedCommands(Flag.ScanFileProtect).Contains(BaseCommand))
            {
                if (this[Flag.ScanFileProtect] == true)
                {
                    parameters.Add(Converters.LongName(Flag.ScanFileProtect));
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
            if (GetSupportedCommands(Flag.ScanSectorProtect).Contains(BaseCommand))
            {
                if (this[Flag.ScanSectorProtect] == true)
                    parameters.Add(Converters.LongName(Flag.ScanSectorProtect));
            }

            // Scan 74:00:00 (Saturn)
            if (GetSupportedCommands(Flag.SeventyFour).Contains(BaseCommand))
            {
                if (this[Flag.SeventyFour] == true)
                    parameters.Add(Converters.LongName(Flag.SeventyFour));
            }

            // Skip sectors
            if (GetSupportedCommands(Flag.SkipSector).Contains(BaseCommand))
            {
                if (this[Flag.SkipSector] == true)
                {
                    if (SkipSectorValue[0] != null && SkipSectorValue[1] != null)
                    {
                        parameters.Add(Converters.LongName(Flag.SkipSector));
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
            if (GetSupportedCommands(Flag.SubchannelReadLevel).Contains(BaseCommand))
            {
                if (this[Flag.SubchannelReadLevel] == true)
                {
                    parameters.Add(Converters.LongName(Flag.SubchannelReadLevel));
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
            if (GetSupportedCommands(Flag.VideoNow).Contains(BaseCommand))
            {
                if (this[Flag.VideoNow] == true)
                {
                    parameters.Add(Converters.LongName(Flag.VideoNow));
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
            if (GetSupportedCommands(Flag.VideoNowColor).Contains(BaseCommand))
            {
                if (this[Flag.VideoNowColor] == true)
                    parameters.Add(Converters.LongName(Flag.VideoNowColor));
            }

            // VideoNowXP
            if (GetSupportedCommands(Flag.VideoNowXP).Contains(BaseCommand))
            {
                if (this[Flag.VideoNowXP] == true)
                    parameters.Add(Converters.LongName(Flag.VideoNowXP));
            }

            return string.Join(" ", parameters);
        }

        /// <summary>
        /// Get the input path from the implementation
        /// </summary>
        /// <returns>String representing the path, null on error</returns>
        public override string InputPath() => DriveLetter;

        /// <summary>
        /// Get the output path from the implementation
        /// </summary>
        /// <returns>String representing the path, null on error</returns>
        public override string OutputPath() => Filename;

        /// <summary>
        /// Get the processing speed from the implementation
        /// </summary>
        /// <returns>int? representing the speed, null on error</returns>
        public override int? GetSpeed() => DriveSpeed;

        /// <summary>
        /// Set the processing speed int the implementation
        /// </summary>
        /// <param name="speed">int? representing the speed</param>
        public override void SetSpeed(int? speed) => DriveSpeed = speed;

        /// <summary>
        /// Get the MediaType from the current set of parameters
        /// </summary>
        /// <returns>MediaType value if successful, null on error</returns>
        public override MediaType? GetMediaType() => Converters.ToMediaType(BaseCommand);

        /// <summary>
        /// Gets if the current command is considered a dumping command or not
        /// </summary>
        /// <returns>True if it's a dumping command, false otherwise</returns>
        public override bool IsDumpingCommand()
        {
            switch (BaseCommand)
            {
                case Command.Audio:
                case Command.BluRay:
                case Command.CompactDisc:
                case Command.Data:
                case Command.DigitalVideoDisc:
                case Command.Disk:
                case Command.Floppy:
                case Command.GDROM:
                case Command.SACD:
                case Command.Swap:
                case Command.XBOX:
                case Command.XBOXSwap:
                case Command.XGD2Swap:
                case Command.XGD3Swap:
                    return true;

                default:
                    return false;
            }
        }

        /// <summary>
        /// Reset all special variables to have default values
        /// </summary>
        protected override void ResetValues()
        {
            BaseCommand = Command.NONE;

            DriveLetter = null;
            DriveSpeed = null;

            Filename = null;

            StartLBAValue = null;
            EndLBAValue = null;

            _flags = new Dictionary<Flag, bool?>();

            AddOffsetValue = null;
            BEOpcodeValue = null;
            C2OpcodeValue = new int?[4];
            ForceUnitAccessValue = null;
            NoSkipSecuritySectorValue = null;
            ScanFileProtectValue = null;
            SubchannelReadLevelValue = null;
            VideoNowValue = null;
        }

        /// <summary>
        /// Set default parameters for a given system and media type
        /// </summary>
        /// <param name="system">KnownSystem value to use</param>
        /// <param name="type">MediaType value to use</param>
        /// <param name="driveLetter">Drive letter to use</param>
        /// <param name="filename">Filename to use</param>
        /// <param name="driveSpeed">Drive speed to use</param>
        /// <param name="paranoid">Enable paranoid mode (safer dumping)</param>
        /// <param name="retryCount">User-defined reread count</param>
        protected override void SetDefaultParameters(
            KnownSystem? system,
            MediaType? type,
            char driveLetter,
            string filename,
            int? driveSpeed,
            bool paranoid,
            int retryCount)
        {
            SetBaseCommand(system, type);

            DriveLetter = driveLetter.ToString();
            DriveSpeed = driveSpeed;
            Filename = filename;

            // First check to see if the combination of system and MediaType is valid
            var validTypes = Utilities.Validators.GetValidMediaTypes(system);
            if (!validTypes.Contains(type))
                return;

            // Set the C2 reread count
            switch (retryCount)
            {
                case -1:
                    C2OpcodeValue[0] = null;
                    break;
                case 0:
                    C2OpcodeValue[0] = 20;
                    break;
                default:
                    C2OpcodeValue[0] = retryCount;
                    break;
            }

            // Now sort based on disc type
            switch (type)
            {
                case MediaType.CDROM:
                    this[Flag.C2Opcode] = true;

                    switch (system)
                    {
                        case KnownSystem.AppleMacintosh:
                        case KnownSystem.IBMPCCompatible:
                            this[Flag.NoFixSubQSecuROM] = true;
                            this[Flag.ScanFileProtect] = true;

                            if (paranoid)
                            {
                                this[Flag.ScanSectorProtect] = true;
                                this[Flag.SubchannelReadLevel] = true;
                                SubchannelReadLevelValue = 2;
                            }
                            break;
                        case KnownSystem.AtariJaguarCD:
                            this[Flag.AtariJaguar] = true;
                            break;
                        case KnownSystem.HasbroVideoNow:
                        case KnownSystem.HasbroVideoNowJr:
                            this[Flag.VideoNow] = true;
                            this.VideoNowValue = 18032;
                            break;
                        case KnownSystem.HasbroVideoNowColor:
                            this[Flag.VideoNowColor] = true;
                            break;
                        case KnownSystem.HasbroVideoNowXP:
                            this[Flag.VideoNowXP] = true;
                            break;
                        case KnownSystem.SonyPlayStation:
                            this[Flag.ScanAntiMod] = true;
                            this[Flag.NoFixSubQLibCrypt] = true;
                            break;
                    }
                    break;
                case MediaType.DVD:
                    if (paranoid)
                    {
                        this[Flag.CopyrightManagementInformation] = true;
                        this[Flag.ScanFileProtect] = true;
                    }
                    break;
                case MediaType.GDROM:
                    this[Flag.C2Opcode] = true;
                    break;
                case MediaType.HDDVD:
                    if (paranoid)
                        this[Flag.CopyrightManagementInformation] = true;
                    break;
                case MediaType.BluRay:
                    // Currently no defaults set
                    break;

                // Special Formats
                case MediaType.NintendoGameCubeGameDisc:
                    this[Flag.Raw] = true;
                    break;
                case MediaType.NintendoWiiOpticalDisc:
                    this[Flag.Raw] = true;
                    break;

                // Non-optical
                case MediaType.FloppyDisk:
                    // Currently no defaults set
                    break;
            }
        }

        /// <summary>
        /// Scan a possible parameter string and populate whatever possible
        /// </summary>
        /// <param name="parameters">String possibly representing parameters</param>
        /// <returns></returns>
        protected override bool ValidateAndSetParameters(string parameters)
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
            BaseCommand = Converters.StringToCommand(parts[0]);
            if (BaseCommand == Command.NONE)
                return false;

            // Loop through ordered command-specific flags
            int index = -1;
            switch (BaseCommand)
            {
                case Command.Audio:
                    if (!DoesExist(parts, 1) || !IsValidDriveLetter(parts[1]))
                        return false;
                    else
                        DriveLetter = parts[1];

                    if (!DoesExist(parts, 2) || IsFlag(parts[2]))
                        return false;
                    else
                        Filename = parts[2];

                    if (!DoesExist(parts, 3) || !IsValidInt32(parts[3], lowerBound: 0, upperBound: 72))
                        return false;
                    else
                        DriveSpeed = Int32.Parse(parts[3]);

                    if (!DoesExist(parts, 4) || !IsValidInt32(parts[4], lowerBound: 0))
                        return false;
                    else
                        StartLBAValue = Int32.Parse(parts[4]);

                    if (!DoesExist(parts, 5) || !IsValidInt32(parts[5], lowerBound: 0))
                        return false;
                    else
                        EndLBAValue = Int32.Parse(parts[5]);

                    index = 6;
                    break;

                case Command.BluRay:
                    if (!DoesExist(parts, 1) || !IsValidDriveLetter(parts[1]))
                        return false;
                    else
                        DriveLetter = parts[1];

                    if (!DoesExist(parts, 2) || IsFlag(parts[2]))
                        return false;
                    else
                        Filename = parts[2];

                    if (!DoesExist(parts, 3) || !IsValidInt32(parts[3], lowerBound: 0, upperBound: 72))
                        return false;
                    else
                        DriveSpeed = Int32.Parse(parts[3]);

                    index = 4;
                    break;

                case Command.Close:
                    if (!DoesExist(parts, 1) || !IsValidDriveLetter(parts[1]))
                        return false;
                    else
                        DriveLetter = parts[1];

                    if (parts.Count > 2)
                        return false;

                    break;

                case Command.CompactDisc:
                    if (!DoesExist(parts, 1) || !IsValidDriveLetter(parts[1]))
                        return false;
                    else
                        DriveLetter = parts[1];

                    if (!DoesExist(parts, 2) || IsFlag(parts[2]))
                        return false;
                    else
                        Filename = parts[2];

                    if (!DoesExist(parts, 3) || !IsValidInt32(parts[3], lowerBound: 0, upperBound: 72))
                        return false;
                    else
                        DriveSpeed = Int32.Parse(parts[3]);

                    index = 4;
                    break;

                case Command.Data:
                    if (!DoesExist(parts, 1) || !IsValidDriveLetter(parts[1]))
                        return false;
                    else
                        DriveLetter = parts[1];

                    if (!DoesExist(parts, 2) || IsFlag(parts[2]))
                        return false;
                    else
                        Filename = parts[2];

                    if (!DoesExist(parts, 3) || !IsValidInt32(parts[3], lowerBound: 0, upperBound: 72))
                        return false;
                    else
                        DriveSpeed = Int32.Parse(parts[3]);

                    if (!DoesExist(parts, 4) || !IsValidInt32(parts[4], lowerBound: 0))
                        return false;
                    else
                        StartLBAValue = Int32.Parse(parts[4]);

                    if (!DoesExist(parts, 5) || !IsValidInt32(parts[5], lowerBound: 0))
                        return false;
                    else
                        EndLBAValue = Int32.Parse(parts[5]);

                    index = 6;
                    break;

                case Command.DigitalVideoDisc:
                    if (!DoesExist(parts, 1) || !IsValidDriveLetter(parts[1]))
                        return false;
                    else
                        DriveLetter = parts[1];

                    if (!DoesExist(parts, 2) || IsFlag(parts[2]))
                        return false;
                    else
                        Filename = parts[2];

                    if (!DoesExist(parts, 3) || !IsValidInt32(parts[3], lowerBound: 0, upperBound: 24)) // Officially 0-16
                        return false;
                    else
                        DriveSpeed = Int32.Parse(parts[3]);

                    index = 4;
                    break;

                case Command.Disk:
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

                    break;

                case Command.DriveSpeed:
                    if (!DoesExist(parts, 1) || !IsValidDriveLetter(parts[1]))
                        return false;
                    else
                        DriveLetter = parts[1];

                    if (parts.Count > 2)
                        return false;

                    break;

                case Command.Eject:
                    if (!DoesExist(parts, 1) || !IsValidDriveLetter(parts[1]))
                        return false;
                    else
                        DriveLetter = parts[1];

                    if (parts.Count > 2)
                        return false;

                    break;

                case Command.Floppy:
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

                    break;

                case Command.GDROM:
                    if (!DoesExist(parts, 1) || !IsValidDriveLetter(parts[1]))
                        return false;
                    else
                        DriveLetter = parts[1];

                    if (!DoesExist(parts, 2) || IsFlag(parts[2]))
                        return false;
                    else
                        Filename = parts[2];

                    if (!DoesExist(parts, 3) || !IsValidInt32(parts[3], lowerBound: 0, upperBound: 72))
                        return false;
                    else
                        DriveSpeed = Int32.Parse(parts[3]);

                    index = 4;
                    break;

                case Command.MDS:
                    if (!DoesExist(parts, 1) || IsFlag(parts[1]) || !File.Exists(parts[1]))
                        return false;
                    else
                        Filename = parts[1];

                    if (parts.Count > 2)
                        return false;

                    break;

                case Command.Merge:
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

                    break;

                case Command.Reset:
                    if (!DoesExist(parts, 1) || !IsValidDriveLetter(parts[1]))
                        return false;
                    else
                        DriveLetter = parts[1];

                    if (parts.Count > 2)
                        return false;

                    break;

                case Command.SACD:
                    if (!DoesExist(parts, 1) || !IsValidDriveLetter(parts[1]))
                        return false;
                    else
                        DriveLetter = parts[1];

                    if (!DoesExist(parts, 2) || IsFlag(parts[2]))
                        return false;
                    else
                        Filename = parts[2];

                    if (!DoesExist(parts, 3) || !IsValidInt32(parts[3], lowerBound: 0, upperBound: 16))
                        return false;
                    else
                        DriveSpeed = Int32.Parse(parts[3]);

                    if (parts.Count > 4)
                        return false;

                    break;

                case Command.Start:
                    if (!DoesExist(parts, 1) || !IsValidDriveLetter(parts[1]))
                        return false;
                    else
                        DriveLetter = parts[1];

                    if (parts.Count > 2)
                        return false;

                    break;

                case Command.Stop:
                    if (!DoesExist(parts, 1) || !IsValidDriveLetter(parts[1]))
                        return false;
                    else
                        DriveLetter = parts[1];

                    if (parts.Count > 2)
                        return false;

                    break;

                case Command.Sub:
                    if (!DoesExist(parts, 1) || IsFlag(parts[1]) || !File.Exists(parts[1]))
                        return false;
                    else
                        Filename = parts[1];

                    if (parts.Count > 2)
                        return false;

                    break;

                case Command.Swap:
                    if (!DoesExist(parts, 1) || !IsValidDriveLetter(parts[1]))
                        return false;
                    else
                        DriveLetter = parts[1];

                    if (!DoesExist(parts, 2) || IsFlag(parts[2]))
                        return false;
                    else
                        Filename = parts[2];

                    if (!DoesExist(parts, 3) || !IsValidInt32(parts[3], lowerBound: 0, upperBound: 72))
                        return false;
                    else
                        DriveSpeed = Int32.Parse(parts[3]);

                    index = 4;
                    break;

                case Command.XBOX:
                    if (!DoesExist(parts, 1) || !IsValidDriveLetter(parts[1]))
                        return false;
                    else
                        DriveLetter = parts[1];

                    if (!DoesExist(parts, 2) || IsFlag(parts[2]))
                        return false;
                    else
                        Filename = parts[2];

                    if (!DoesExist(parts, 3) || !IsValidInt32(parts[3], lowerBound: 0, upperBound: 72))
                        return false;
                    else
                        DriveSpeed = Int32.Parse(parts[3]);

                    index = 4;
                    break;

                case Command.XBOXSwap:
                case Command.XGD2Swap:
                case Command.XGD3Swap:
                    if (!DoesExist(parts, 1) || !IsValidDriveLetter(parts[1]))
                        return false;
                    else
                        DriveLetter = parts[1];

                    if (!DoesExist(parts, 2) || IsFlag(parts[2]))
                        return false;
                    else
                        Filename = parts[2];

                    if (!DoesExist(parts, 3) || !IsValidInt32(parts[3], lowerBound: 0, upperBound: 72))
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
                        case FlagStrings.AddOffset:
                            if (!GetSupportedCommands(Flag.AddOffset).Contains(BaseCommand))
                                return false;
                            else if (!DoesExist(parts, i + 1))
                                return false;
                            else if (!IsValidInt32(parts[i + 1]))
                                return false;

                            this[Flag.AddOffset] = true;
                            AddOffsetValue = Int32.Parse(parts[i + 1]);
                            i++;
                            break;

                        case FlagStrings.AMSF:
                            if (!GetSupportedCommands(Flag.AMSF).Contains(BaseCommand))
                                return false;

                            this[Flag.AMSF] = true;
                            break;

                        case FlagStrings.AtariJaguar:
                            if (!GetSupportedCommands(Flag.AtariJaguar).Contains(BaseCommand))
                                return false;

                            this[Flag.AtariJaguar] = true;
                            break;

                        case FlagStrings.BEOpcode:
                            if (!GetSupportedCommands(Flag.BEOpcode).Contains(BaseCommand))
                                return false;
                            else if (!DoesExist(parts, i + 1))
                            {
                                this[Flag.BEOpcode] = true;
                                break;
                            }
                            else if (IsFlag(parts[i + 1]))
                            {
                                this[Flag.BEOpcode] = true;
                                break;
                            }
                            else if (parts[i + 1] != "raw" && (parts[i + 1] != "pack"))
                                return false;

                            this[Flag.BEOpcode] = true;
                            BEOpcodeValue = parts[i + 1];
                            i++;
                            break;

                        case FlagStrings.C2Opcode:
                            if (!GetSupportedCommands(Flag.C2Opcode).Contains(BaseCommand))
                                return false;

                            this[Flag.C2Opcode] = true;
                            for (int j = 0; j < 4; j++)
                            {
                                if (!DoesExist(parts, i + 1))
                                    break;
                                else if (IsFlag(parts[i + 1]))
                                    break;
                                else if (!IsValidInt32(parts[i + 1], lowerBound: 0))
                                    return false;
                                else
                                {
                                    C2OpcodeValue[j] = Int32.Parse(parts[i + 1]);
                                    i++;
                                }
                            }

                            break;

                        case FlagStrings.CopyrightManagementInformation:
                            if (!GetSupportedCommands(Flag.CopyrightManagementInformation).Contains(BaseCommand))
                                return false;

                            this[Flag.CopyrightManagementInformation] = true;
                            break;

                        case FlagStrings.D8Opcode:
                            if (!GetSupportedCommands(Flag.D8Opcode).Contains(BaseCommand))
                                return false;

                            this[Flag.D8Opcode] = true;
                            break;

                        case FlagStrings.DisableBeep:
                            if (!GetSupportedCommands(Flag.DisableBeep).Contains(BaseCommand))
                                return false;

                            this[Flag.DisableBeep] = true;
                            break;

                        case FlagStrings.ForceUnitAccess:
                            if (!GetSupportedCommands(Flag.ForceUnitAccess).Contains(BaseCommand))
                                return false;
                            else if (!DoesExist(parts, i + 1))
                            {
                                this[Flag.ForceUnitAccess] = true;
                                break;
                            }
                            else if (IsFlag(parts[i + 1]))
                            {
                                this[Flag.ForceUnitAccess] = true;
                                break;
                            }
                            else if (!IsValidInt32(parts[i + 1], lowerBound: 0))
                                return false;

                            this[Flag.ForceUnitAccess] = true;
                            ForceUnitAccessValue = Int32.Parse(parts[i + 1]);
                            i++;
                            break;

                        case FlagStrings.MultiSession:
                            if (!GetSupportedCommands(Flag.MultiSession).Contains(BaseCommand))
                                return false;

                            this[Flag.MultiSession] = true;
                            break;

                        case FlagStrings.NoFixSubP:
                            if (!GetSupportedCommands(Flag.NoFixSubP).Contains(BaseCommand))
                                return false;

                            this[Flag.NoFixSubP] = true;
                            break;

                        case FlagStrings.NoFixSubQ:
                            if (!GetSupportedCommands(Flag.NoFixSubQ).Contains(BaseCommand))
                                return false;

                            this[Flag.NoFixSubQ] = true;
                            break;

                        case FlagStrings.NoFixSubQLibCrypt:
                            if (!GetSupportedCommands(Flag.NoFixSubQLibCrypt).Contains(BaseCommand))
                                return false;

                            this[Flag.NoFixSubQLibCrypt] = true;
                            break;

                        case FlagStrings.NoFixSubRtoW:
                            if (!GetSupportedCommands(Flag.NoFixSubRtoW).Contains(BaseCommand))
                                return false;

                            this[Flag.NoFixSubRtoW] = true;
                            break;

                        case FlagStrings.NoFixSubQSecuROM:
                            if (!GetSupportedCommands(Flag.NoFixSubQSecuROM).Contains(BaseCommand))
                                return false;

                            this[Flag.NoFixSubQSecuROM] = true;
                            break;

                        case FlagStrings.NoSkipSS:
                            if (!GetSupportedCommands(Flag.NoSkipSS).Contains(BaseCommand))
                                return false;
                            else if (!DoesExist(parts, i + 1))
                            {
                                this[Flag.NoSkipSS] = true;
                                break;
                            }
                            else if (IsFlag(parts[i + 1]))
                            {
                                this[Flag.NoSkipSS] = true;
                                break;
                            }
                            else if (!IsValidInt32(parts[i + 1], lowerBound: 0))
                                return false;

                            this[Flag.NoSkipSS] = true;
                            ForceUnitAccessValue = Int32.Parse(parts[i + 1]);
                            i++;
                            break;

                        case FlagStrings.Raw:
                            if (!GetSupportedCommands(Flag.Raw).Contains(BaseCommand))
                                return false;

                            this[Flag.Raw] = true;
                            break;

                        case FlagStrings.Reverse:
                            if (!GetSupportedCommands(Flag.Reverse).Contains(BaseCommand))
                                return false;

                            this[Flag.Reverse] = true;
                            break;

                        case FlagStrings.ScanAntiMod:
                            if (!GetSupportedCommands(Flag.ScanAntiMod).Contains(BaseCommand))
                                return false;

                            this[Flag.ScanAntiMod] = true;
                            break;

                        case FlagStrings.ScanFileProtect:
                            if (!GetSupportedCommands(Flag.ScanFileProtect).Contains(BaseCommand))
                                return false;
                            else if (!DoesExist(parts, i + 1))
                            {
                                this[Flag.ScanFileProtect] = true;
                                break;
                            }
                            else if (IsFlag(parts[i + 1]))
                            {
                                this[Flag.ScanFileProtect] = true;
                                break;
                            }
                            else if (!IsValidInt32(parts[i + 1], lowerBound: 0))
                                return false;

                            this[Flag.ScanFileProtect] = true;
                            ScanFileProtectValue = Int32.Parse(parts[i + 1]);
                            i++;
                            break;

                        case FlagStrings.ScanSectorProtect:
                            if (!GetSupportedCommands(Flag.ScanSectorProtect).Contains(BaseCommand))
                                return false;

                            this[Flag.ScanSectorProtect] = true;
                            break;

                        case FlagStrings.SeventyFour:
                            if (!GetSupportedCommands(Flag.SeventyFour).Contains(BaseCommand))
                                return false;

                            this[Flag.SeventyFour] = true;
                            break;

                        case FlagStrings.SkipSector:
                            if (!GetSupportedCommands(Flag.SkipSector).Contains(BaseCommand))
                                return false;
                            else if (!DoesExist(parts, i + 1) || !DoesExist(parts, i + 2))
                                return false;
                            else if (IsFlag(parts[i + 1]) || IsFlag(parts[i + 2]))
                                return false;
                            else if (!IsValidInt32(parts[i + 1], lowerBound: 0) || !IsValidInt32(parts[i + 2], lowerBound: 0))
                                return false;

                            this[Flag.SkipSector] = true;
                            SkipSectorValue[0] = Int32.Parse(parts[i + 1]);
                            SkipSectorValue[1] = Int32.Parse(parts[i + 2]);
                            i += 2;
                            break;

                        case FlagStrings.SubchannelReadLevel:
                            if (!GetSupportedCommands(Flag.SubchannelReadLevel).Contains(BaseCommand))
                                return false;
                            else if (!DoesExist(parts, i + 1))
                            {
                                this[Flag.SubchannelReadLevel] = true;
                                break;
                            }
                            else if (IsFlag(parts[i + 1]))
                            {
                                this[Flag.SubchannelReadLevel] = true;
                                break;
                            }
                            else if (!IsValidInt32(parts[i + 1], lowerBound: 0, upperBound: 2))
                                return false;

                            this[Flag.SubchannelReadLevel] = true;
                            SubchannelReadLevelValue = Int32.Parse(parts[i + 1]);
                            i++;
                            break;

                        case FlagStrings.VideoNow:
                            if (!GetSupportedCommands(Flag.VideoNow).Contains(BaseCommand))
                                return false;
                            else if (!DoesExist(parts, i + 1))
                            {
                                this[Flag.VideoNow] = true;
                                break;
                            }
                            else if (IsFlag(parts[i + 1]))
                            {
                                this[Flag.VideoNow] = true;
                                break;
                            }
                            else if (!IsValidInt32(parts[i + 1], lowerBound: 0))
                                return false;

                            this[Flag.VideoNow] = true;
                            VideoNowValue = Int32.Parse(parts[i + 1]);
                            i++;
                            break;

                        case FlagStrings.VideoNowColor:
                            if (!GetSupportedCommands(Flag.VideoNowColor).Contains(BaseCommand))
                                return false;

                            this[Flag.VideoNowColor] = true;
                            break;

                        case FlagStrings.VideoNowXP:
                            if (!GetSupportedCommands(Flag.VideoNowXP).Contains(BaseCommand))
                                return false;

                            this[Flag.VideoNowXP] = true;
                            break;

                        default:
                            return false;
                    }
                }
            }

            return true;
        }

        /// <summary>
        /// Validate if all required output files exist
        /// </summary>
        /// <param name="basePath">Base filename and path to use for checking</param>
        /// <param name="system">KnownSystem type representing the media</param>
        /// <param name="type">MediaType type representing the media</param>
        /// <returns></returns>
        public override bool CheckAllOutputFilesExist(string basePath, KnownSystem? system, MediaType? type)
        {
            // Some disc types are audio-only
            bool audioOnly = (system == KnownSystem.AtariJaguarCD)
                || (system == KnownSystem.AudioCD)
                || (system == KnownSystem.SuperAudioCD);

            switch (type)
            {
                case MediaType.CDROM:
                case MediaType.GDROM: // TODO: Verify GD-ROM outputs this
                    // return File.Exists(basePath + ".c2") // Doesn't output on Linux
                    return File.Exists(basePath + ".ccd")
                        && File.Exists(basePath + ".cue")
                        && File.Exists(basePath + ".dat")
                        && File.Exists(basePath + ".img")
                        && (audioOnly || File.Exists(basePath + ".img_EdcEcc.txt") || File.Exists(basePath + ".img_EccEdc.txt"))
                        && (audioOnly || File.Exists(basePath + ".scm"))
                        && File.Exists(basePath + ".sub")
                        // && File.Exists(basePath + "_c2Error.txt") // Doesn't output on Linux
                        && File.Exists(basePath + "_cmd.txt")
                        && File.Exists(basePath + "_disc.txt")
                        && File.Exists(basePath + "_drive.txt")
                        && File.Exists(basePath + "_img.cue")
                        && File.Exists(basePath + "_mainError.txt")
                        && File.Exists(basePath + "_mainInfo.txt")
                        && File.Exists(basePath + "_subError.txt")
                        && File.Exists(basePath + "_subInfo.txt")
                        // && File.Exists(basePath + "_subIntention.txt") // Not guaranteed output
                        && (File.Exists(basePath + "_subReadable.txt") || File.Exists(basePath + "_sub.txt"))
                        && File.Exists(basePath + "_volDesc.txt");

                case MediaType.DVD:
                case MediaType.HDDVD:
                case MediaType.BluRay:
                case MediaType.NintendoGameCubeGameDisc:
                case MediaType.NintendoWiiOpticalDisc:
                    bool dicDump = File.Exists(basePath + ".dat")
                        && File.Exists(basePath + "_cmd.txt")
                        && File.Exists(basePath + "_disc.txt")
                        && File.Exists(basePath + "_drive.txt")
                        && File.Exists(basePath + "_mainError.txt")
                        && File.Exists(basePath + "_mainInfo.txt")
                        && File.Exists(basePath + "_volDesc.txt");
                    return dicDump;

                case MediaType.FloppyDisk:
                case MediaType.HardDisk:
                    // TODO: Determine what outputs come out from a HDD, SD, etc.
                    return File.Exists(basePath + ".dat")
                        && File.Exists(basePath + "_cmd.txt")
                       && File.Exists(basePath + "_disc.txt");

                case MediaType.UMD:
                    return File.Exists(basePath + "_disc.txt")
                        || File.Exists(basePath + "_mainError.txt")
                        || File.Exists(basePath + "_mainInfo.txt")
                        || File.Exists(basePath + "_volDesc.txt");

                default:
                    // Non-dumping commands will usually produce no output, so this is irrelevant
                    return true;
            }
        }

        /// <summary>
        /// Generate a SubmissionInfo for the output files
        /// </summary>
        /// <param name="info">Base submission info to fill in specifics for</param>
        /// <param name="basePath">Base filename and path to use for checking</param>
        /// <param name="system">KnownSystem type representing the media</param>
        /// <param name="type">MediaType type representing the media</param>
        /// <param name="drive">Drive representing the disc to get information from</param>
        /// <returns></returns>
        public override void GenerateSubmissionInfo(SubmissionInfo info, string basePath, KnownSystem? system, MediaType? type, Drive drive)
        {
            string outputDirectory = Path.GetDirectoryName(basePath);

            // Fill in the hash data
            info.TracksAndWriteOffsets.ClrMameProData = GetDatfile(basePath + ".dat");

            // Extract info based generically on MediaType
            switch (type)
            {
                case MediaType.CDROM:
                case MediaType.GDROM: // TODO: Verify GD-ROM outputs this
                    info.Extras.PVD = GetPVD(basePath + "_mainInfo.txt") ?? "Disc has no PVD"; ;

                    long errorCount = -1;
                    if (File.Exists(basePath + ".img_EdcEcc.txt"))
                        errorCount = GetErrorCount(basePath + ".img_EdcEcc.txt");
                    else if (File.Exists(basePath + ".img_EccEdc.txt"))
                        errorCount = GetErrorCount(basePath + ".img_EccEdc.txt");

                    info.CommonDiscInfo.ErrorsCount = (errorCount == -1 ? "Error retrieving error count" : errorCount.ToString());
                    info.TracksAndWriteOffsets.Cuesheet = GetFullFile(basePath + ".cue") ?? "";

                    string cdWriteOffset = GetWriteOffset(basePath + "_disc.txt") ?? "";
                    info.CommonDiscInfo.RingWriteOffset = cdWriteOffset;
                    info.TracksAndWriteOffsets.OtherWriteOffsets = cdWriteOffset;

                    break;

                case MediaType.DVD:
                case MediaType.HDDVD:
                case MediaType.BluRay:
                    bool xgd = (system == KnownSystem.MicrosoftXBOX || system == KnownSystem.MicrosoftXBOX360);

                    // Get the individual hash data, as per internal
                    if (GetISOHashValues(info.TracksAndWriteOffsets.ClrMameProData, out long size, out string crc32, out string md5, out string sha1))
                    {
                        info.SizeAndChecksums.Size = size;
                        info.SizeAndChecksums.CRC32 = crc32;
                        info.SizeAndChecksums.MD5 = md5;
                        info.SizeAndChecksums.SHA1 = sha1;
                        info.TracksAndWriteOffsets.ClrMameProData = null;
                    }

                    // Deal with the layerbreak
                    string layerbreak = null;
                    if (type == MediaType.DVD)
                        layerbreak = GetLayerbreak(basePath + "_disc.txt", xgd) ?? "";
                    else if (type == MediaType.BluRay)
                        layerbreak = (info.SizeAndChecksums.Size > 25025314816 ? "25025314816" : null);

                    // If we have a single-layer disc
                    if (string.IsNullOrWhiteSpace(layerbreak))
                    {
                        info.Extras.PVD = GetPVD(basePath + "_mainInfo.txt") ?? "";
                    }
                    // If we have a dual-layer disc
                    else
                    {
                        info.Extras.PVD = GetPVD(basePath + "_mainInfo.txt") ?? "";
                        info.SizeAndChecksums.Layerbreak = Int64.Parse(layerbreak);
                    }

                    // Bluray-specific options
                    if (type == MediaType.BluRay)
                        info.Extras.PIC = GetPIC(Path.Combine(outputDirectory, "PIC.bin")) ?? "";

                    break;

                case MediaType.UMD:
                    info.Extras.PVD = GetPVD(basePath + "_mainInfo.txt") ?? "";

                    if (GetUMDAuxInfo(basePath + "_disc.txt", out string title, out Category? umdcat, out string umdversion, out string umdlayer, out long umdsize))
                    {
                        info.CommonDiscInfo.Title = title ?? "";
                        info.CommonDiscInfo.Category = umdcat ?? Category.Games;
                        info.VersionAndEditions.Version = umdversion ?? "";
                        info.SizeAndChecksums.Size = umdsize;

                        if (!string.IsNullOrWhiteSpace(umdlayer))
                            info.SizeAndChecksums.Layerbreak = Int64.Parse(umdlayer ?? "-1");
                    }

                    break;
            }

            // Extract info based specifically on KnownSystem
            switch (system)
            {
                case KnownSystem.AppleMacintosh:
                case KnownSystem.EnhancedCD:
                case KnownSystem.IBMPCCompatible:
                case KnownSystem.RainbowDisc:
                    if (File.Exists(basePath + "_subIntention.txt"))
                    {
                        FileInfo fi = new FileInfo(basePath + "_subIntention.txt");
                        if (fi.Length > 0)
                            info.CopyProtection.SecuROMData = GetFullFile(basePath + "_subIntention.txt") ?? "";
                    }

                    break;

                case KnownSystem.DVDVideo:
                    info.CopyProtection.Protection = GetDVDProtection(basePath + "_CSSKey.txt", basePath + "_disc.txt") ?? "";
                    break;

                case KnownSystem.KonamiPython2:
                    if (GetPlayStationExecutableInfo(drive?.Letter, out Region? pythonTwoRegion, out string pythonTwoDate))
                    {
                        info.CommonDiscInfo.Region = info.CommonDiscInfo.Region ?? pythonTwoRegion;
                        info.CommonDiscInfo.EXEDateBuildDate = pythonTwoDate;
                    }

                    info.VersionAndEditions.Version = GetPlayStation2Version(drive?.Letter) ?? "";
                    break;

                case KnownSystem.MicrosoftXBOX:
                    if (GetXgdAuxInfo(basePath + "_disc.txt", out string dmihash, out string pfihash, out string sshash, out string ss, out string ssver))
                    {
                        info.CommonDiscInfo.Comments += $"{Template.XBOXDMIHash}: {dmihash ?? ""}\n" +
                            $"{Template.XBOXPFIHash}: {pfihash ?? ""}\n" +
                            $"{Template.XBOXSSHash}: {sshash ?? ""}\n" +
                            $"{Template.XBOXSSVersion}: {ssver ?? ""}\n";
                        info.Extras.SecuritySectorRanges = ss ?? "";
                    }

                    if (GetXboxDMIInfo(Path.Combine(outputDirectory, "DMI.bin"), out string serial, out string version, out Region? region))
                    {
                        info.CommonDiscInfo.Serial = serial ?? "";
                        info.VersionAndEditions.Version = version ?? "";
                        info.CommonDiscInfo.Region = region;
                    }

                    break;

                case KnownSystem.MicrosoftXBOX360:
                    if (GetXgdAuxInfo(basePath + "_disc.txt", out string dmi360hash, out string pfi360hash, out string ss360hash, out string ss360, out string ssver360))
                    {
                        info.CommonDiscInfo.Comments += $"{Template.XBOXDMIHash}: {dmi360hash ?? ""}\n" +
                            $"{Template.XBOXPFIHash}: {pfi360hash ?? ""}\n" +
                            $"{Template.XBOXSSHash}: {ss360hash ?? ""}\n" +
                            $"{Template.XBOXSSVersion}: {ssver360 ?? ""}\n";
                        info.Extras.SecuritySectorRanges = ss360 ?? "";
                    }

                    if (GetXbox360DMIInfo(Path.Combine(outputDirectory, "DMI.bin"), out string serial360, out string version360, out Region? region360))
                    {
                        info.CommonDiscInfo.Serial = serial360 ?? "";
                        info.VersionAndEditions.Version = version360 ?? "";
                        info.CommonDiscInfo.Region = region360;
                    }
                    break;

                case KnownSystem.NamcoSegaNintendoTriforce:
                    if (type == MediaType.CDROM)
                        info.Extras.Header = GetSegaHeader(basePath + "_mainInfo.txt") ?? "";

                    break;

                case KnownSystem.SegaCDMegaCD:
                    info.Extras.Header = GetSegaHeader(basePath + "_mainInfo.txt") ?? "";

                    // Take only the last 16 lines for Sega CD
                    if (!string.IsNullOrEmpty(info.Extras.Header))
                        info.Extras.Header = string.Join("\n", info.Extras.Header.Split('\n').Skip(16));

                    if (GetSegaCDBuildInfo(info.Extras.Header, out string scdSerial, out string fixedDate))
                    {
                        info.CommonDiscInfo.Serial = scdSerial ?? "";
                        info.CommonDiscInfo.EXEDateBuildDate = fixedDate ?? "";
                    }

                    break;

                case KnownSystem.SegaChihiro:
                    if (type == MediaType.CDROM)
                        info.Extras.Header = GetSegaHeader(basePath + "_mainInfo.txt") ?? "";

                    break;

                case KnownSystem.SegaDreamcast:
                    if (type == MediaType.CDROM)
                        info.Extras.Header = GetSegaHeader(basePath + "_mainInfo.txt") ?? "";

                    break;

                case KnownSystem.SegaNaomi:
                    if (type == MediaType.CDROM)
                        info.Extras.Header = GetSegaHeader(basePath + "_mainInfo.txt") ?? "";

                    break;

                case KnownSystem.SegaNaomi2:
                    if (type == MediaType.CDROM)
                        info.Extras.Header = GetSegaHeader(basePath + "_mainInfo.txt") ?? "";

                    break;

                case KnownSystem.SegaSaturn:
                    info.Extras.Header = GetSegaHeader(basePath + "_mainInfo.txt") ?? "";

                    // Take only the first 16 lines for Saturn
                    if (!string.IsNullOrEmpty(info.Extras.Header))
                        info.Extras.Header = string.Join("\n", info.Extras.Header.Split('\n').Take(16));

                    if (GetSaturnBuildInfo(info.Extras.Header, out string saturnSerial, out string saturnVersion, out string buildDate))
                    {
                        info.CommonDiscInfo.Serial = saturnSerial ?? "";
                        info.VersionAndEditions.Version = saturnVersion ?? "";
                        info.CommonDiscInfo.EXEDateBuildDate = buildDate ?? "";
                    }

                    break;

                case KnownSystem.SonyPlayStation:
                    if (GetPlayStationExecutableInfo(drive?.Letter, out Region? playstationRegion, out string playstationDate))
                    {
                        info.CommonDiscInfo.Region = info.CommonDiscInfo.Region ?? playstationRegion;
                        info.CommonDiscInfo.EXEDateBuildDate = playstationDate;
                    }

                    bool? psEdcStatus = GetPlayStationEDCStatus(basePath + ".img_EdcEcc.txt");
                    if (psEdcStatus == true)
                        info.EDC.EDC = YesNo.Yes;
                    else if (psEdcStatus == false)
                        info.EDC.EDC = YesNo.No;
                    else
                        info.EDC.EDC = YesNo.NULL;

                    info.CopyProtection.AntiModchip = GetPlayStationAntiModchipDetected(basePath + "_disc.txt") ? YesNo.Yes : YesNo.No;

                    bool? psLibCryptStatus = GetLibCryptDetected(basePath + ".sub");
                    if (psLibCryptStatus == true)
                    {
                        info.CopyProtection.LibCrypt = YesNo.Yes;
                        if (File.Exists(basePath + "_subIntention.txt"))
                            info.CopyProtection.LibCryptData = GetFullFile(basePath + "_subIntention.txt") ?? "";
                        else
                            info.CopyProtection.LibCryptData = "LibCrypt detected but no subIntention data found!";
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

                    break;

                case KnownSystem.SonyPlayStation2:
                    if (GetPlayStationExecutableInfo(drive?.Letter, out Region? playstationTwoRegion, out string playstationTwoDate))
                    {
                        info.CommonDiscInfo.Region = info.CommonDiscInfo.Region ?? playstationTwoRegion;
                        info.CommonDiscInfo.EXEDateBuildDate = playstationTwoDate;
                    }

                    info.VersionAndEditions.Version = GetPlayStation2Version(drive?.Letter) ?? "";
                    break;

                case KnownSystem.SonyPlayStation4:
                    info.VersionAndEditions.Version = GetPlayStation4Version(drive?.Letter) ?? "";
                    break;
            }
        }

        /// <summary>
        /// Get the list of commands that use a given flag
        /// </summary>
        /// <param name="flag">Flag value to get commands for</param>
        /// <returns>List of DiscImageCreator.Commands, if possible</returns>
        private static List<Command> GetSupportedCommands(Flag flag)
        {
            var commands = new List<Command>();
            switch (flag)
            {
                case Flag.AMSF:
                    commands.Add(Command.CompactDisc);
                    break;
                case Flag.AtariJaguar:
                    commands.Add(Command.CompactDisc);
                    break;
                case Flag.BEOpcode:
                    commands.Add(Command.Audio);
                    commands.Add(Command.CompactDisc);
                    commands.Add(Command.Data);
                    commands.Add(Command.GDROM);
                    commands.Add(Command.Swap);
                    break;
                case Flag.C2Opcode:
                    commands.Add(Command.Audio);
                    commands.Add(Command.CompactDisc);
                    commands.Add(Command.Data);
                    commands.Add(Command.GDROM);
                    commands.Add(Command.Swap);
                    break;
                case Flag.CopyrightManagementInformation:
                    commands.Add(Command.DigitalVideoDisc);
                    break;
                case Flag.D8Opcode:
                    commands.Add(Command.Audio);
                    commands.Add(Command.CompactDisc);
                    commands.Add(Command.Data);
                    commands.Add(Command.GDROM);
                    commands.Add(Command.Swap);
                    break;
                case Flag.DisableBeep:
                    commands.Add(Command.Audio);
                    commands.Add(Command.BluRay);
                    commands.Add(Command.CompactDisc);
                    commands.Add(Command.Data);
                    commands.Add(Command.DigitalVideoDisc);
                    commands.Add(Command.GDROM);
                    commands.Add(Command.Swap);
                    commands.Add(Command.XBOX);
                    break;
                case Flag.ForceUnitAccess:
                    commands.Add(Command.Audio);
                    commands.Add(Command.BluRay);
                    commands.Add(Command.CompactDisc);
                    commands.Add(Command.Data);
                    commands.Add(Command.DigitalVideoDisc);
                    commands.Add(Command.Swap);
                    commands.Add(Command.XBOX);
                    break;
                case Flag.MultiSession:
                    commands.Add(Command.CompactDisc);
                    break;
                case Flag.NoFixSubP:
                    commands.Add(Command.Audio);
                    commands.Add(Command.CompactDisc);
                    commands.Add(Command.Data);
                    commands.Add(Command.GDROM);
                    commands.Add(Command.Swap);
                    break;
                case Flag.NoFixSubQ:
                    commands.Add(Command.Audio);
                    commands.Add(Command.CompactDisc);
                    commands.Add(Command.Data);
                    commands.Add(Command.GDROM);
                    commands.Add(Command.Swap);
                    break;
                case Flag.NoFixSubQLibCrypt:
                    commands.Add(Command.Audio);
                    commands.Add(Command.CompactDisc);
                    commands.Add(Command.Data);
                    commands.Add(Command.GDROM);
                    commands.Add(Command.Swap);
                    break;
                case Flag.NoFixSubQSecuROM:
                    commands.Add(Command.Audio);
                    commands.Add(Command.CompactDisc);
                    commands.Add(Command.Data);
                    commands.Add(Command.GDROM);
                    commands.Add(Command.Swap);
                    break;
                case Flag.NoFixSubRtoW:
                    commands.Add(Command.Audio);
                    commands.Add(Command.CompactDisc);
                    commands.Add(Command.Data);
                    commands.Add(Command.GDROM);
                    commands.Add(Command.Swap);
                    break;
                case Flag.NoSkipSS:
                    commands.Add(Command.XBOX);
                    commands.Add(Command.XBOXSwap);
                    commands.Add(Command.XGD2Swap);
                    commands.Add(Command.XGD3Swap);
                    break;
                case Flag.Raw:
                    commands.Add(Command.DigitalVideoDisc);
                    break;
                case Flag.Reverse:
                    commands.Add(Command.CompactDisc);
                    commands.Add(Command.Data);
                    commands.Add(Command.DigitalVideoDisc);
                    break;
                case Flag.ScanAntiMod:
                    commands.Add(Command.CompactDisc);
                    commands.Add(Command.Data);
                    break;
                case Flag.ScanFileProtect:
                    commands.Add(Command.Audio);
                    commands.Add(Command.CompactDisc);
                    commands.Add(Command.Data);
                    commands.Add(Command.DigitalVideoDisc);
                    commands.Add(Command.Swap);
                    break;
                case Flag.ScanSectorProtect:
                    commands.Add(Command.CompactDisc);
                    commands.Add(Command.Data);
                    commands.Add(Command.Swap);
                    break;
                case Flag.SeventyFour:
                    commands.Add(Command.Swap);
                    break;
                case Flag.SkipSector:
                    commands.Add(Command.Data);
                    break;
                case Flag.SubchannelReadLevel:
                    commands.Add(Command.Audio);
                    commands.Add(Command.CompactDisc);
                    commands.Add(Command.Data);
                    commands.Add(Command.GDROM);
                    commands.Add(Command.Swap);
                    break;
                case Flag.VideoNow:
                    commands.Add(Command.CompactDisc);
                    break;
                case Flag.VideoNowColor:
                    commands.Add(Command.CompactDisc);
                    break;
                case Flag.VideoNowXP:
                    commands.Add(Command.CompactDisc);
                    break;

                case Flag.NONE:
                default:
                    return commands;
            }

            return commands;
        }

        /// <summary>
        /// Set the DIC command to be used for a given system and media type
        /// </summary>
        /// <param name="system">KnownSystem value to check</param>
        /// <param name="type">MediaType value to check</param>
        private void SetBaseCommand(KnownSystem? system, MediaType? type)
        {
            // If we have an invalid combination, we should BaseCommand = null
            if (!Validators.GetValidMediaTypes(system).Contains(type))
            {
                BaseCommand = Command.NONE;
                return;
            }

            switch (type)
            {
                case MediaType.CDROM:
                    if (system == KnownSystem.SuperAudioCD)
                        BaseCommand = Command.SACD;
                    else
                        BaseCommand = Command.CompactDisc;
                    return;
                case MediaType.DVD:
                    if (system == KnownSystem.MicrosoftXBOX
                        || system == KnownSystem.MicrosoftXBOX360)
                    {
                        BaseCommand = Command.XBOX;
                        return;
                    }
                    BaseCommand = Command.DigitalVideoDisc;
                    return;
                case MediaType.GDROM:
                    BaseCommand = Command.GDROM;
                    return;
                case MediaType.HDDVD:
                    BaseCommand = Command.DigitalVideoDisc;
                    return;
                case MediaType.BluRay:
                    BaseCommand = Command.BluRay;
                    return;
                case MediaType.NintendoGameCubeGameDisc:
                    BaseCommand = Command.DigitalVideoDisc;
                    return;
                case MediaType.NintendoWiiOpticalDisc:
                    BaseCommand = Command.DigitalVideoDisc;
                    return;
                case MediaType.FloppyDisk:
                    BaseCommand = Command.Floppy;
                    return;
                case MediaType.HardDisk:
                    BaseCommand = Command.Disk;
                    return;

                default:
                    BaseCommand = Command.NONE;
                    return;
            }
        }

        #region Information Extraction Methods

        /// <summary>
        /// Get the proper datfile from the input file, if possible
        /// </summary>
        /// <param name="dat">.dat file location</param>
        /// <returns>Relevant pieces of the datfile, null on error</returns>
        private static string GetDatfile(string dat)
        {
            // If the file doesn't exist, we can't get info from it
            if (!File.Exists(dat))
                return null;

            using (StreamReader sr = File.OpenText(dat))
            {
                try
                {
                    // Make sure this file is a .dat
                    if (sr.ReadLine() != "<?xml version=\"1.0\" encoding=\"UTF-8\"?>")
                        return null;
                    if (sr.ReadLine() != "<!DOCTYPE datafile PUBLIC \"-//Logiqx//DTD ROM Management Datafile//EN\" \"http://www.logiqx.com/Dats/datafile.dtd\">")
                        return null;

                    // Fast forward to the rom lines
                    while (!sr.ReadLine().TrimStart().StartsWith("<game")) ;
                    sr.ReadLine(); // <category>Games</category>
                    sr.ReadLine(); // <description>Plextor</description>

                    // Now that we're at the relevant entries, read each line in and concatenate
                    string pvd = "", line = sr.ReadLine().Trim();
                    while (line.StartsWith("<rom"))
                    {
                        pvd += line + "\n";
                        line = sr.ReadLine().Trim();
                    }

                    return pvd.TrimEnd('\n');
                }
                catch
                {
                    // We don't care what the exception is right now
                    return null;
                }
            }
        }

        /// <summary>
        /// Get the DVD protection information, if possible
        /// </summary>
        /// <param name="cssKey">_CSSKey.txt file location</param>
        /// <param name="disc">_disc.txt file location</param>
        /// <returns>Formatted string representing the DVD protection, null on error</returns>
        private static string GetDVDProtection(string cssKey, string disc)
        {
            // If one of the files doesn't exist, we can't get info from them
            if (!File.Exists(disc))
                return null;

            // Setup all of the individual pieces
            string region = null, rceProtection = null, copyrightProtectionSystemType = null, encryptedDiscKey = null, playerKey = null, decryptedDiscKey = null;

            // Get everything from _disc.txt first
            using (StreamReader sr = File.OpenText(disc))
            {
                try
                {
                    // Fast forward to the copyright information
                    while (!sr.ReadLine().Trim().StartsWith("========== CopyrightInformation ==========")) ;

                    // Now read until we hit the manufacturing information
                    string line = sr.ReadLine().Trim();
                    while (!line.StartsWith("========== ManufacturingInformation =========="))
                    {
                        if (line.StartsWith("CopyrightProtectionType"))
                            copyrightProtectionSystemType = line.Substring("CopyrightProtectionType: ".Length);
                        else if (line.StartsWith("RegionManagementInformation"))
                            region = line.Substring("RegionManagementInformation: ".Length);

                        line = sr.ReadLine().Trim();
                    }
                }
                catch { }
            }

            // Get everything from _CSSKey.txt next, if it exists
            if (File.Exists(cssKey))
            {
                using (StreamReader sr = File.OpenText(cssKey))
                {
                    try
                    {
                        // Read until the end
                        while (!sr.EndOfStream)
                        {
                            string line = sr.ReadLine().Trim();

                            if (line.StartsWith("[001]"))
                                encryptedDiscKey = line.Substring("[001]: ".Length);
                            else if (line.StartsWith("PlayerKey"))
                                playerKey = line.Substring("PlayerKey[1]: ".Length);
                            else if (line.StartsWith("DecryptedDiscKey"))
                                decryptedDiscKey = line.Substring("DecryptedDiscKey[020]: ".Length);
                        }
                    }
                    catch { }
                }
            }

            // Now we format everything we can
            string protection = "";
            if (!string.IsNullOrEmpty(region))
                protection += $"Region: {region}\n";
            if (!string.IsNullOrEmpty(rceProtection))
                protection += $"RCE Protection: {rceProtection}\n";
            if (!string.IsNullOrEmpty(copyrightProtectionSystemType))
                protection += $"Copyright Protection System Type: {copyrightProtectionSystemType}\n";
            if (!string.IsNullOrEmpty(encryptedDiscKey))
                protection += $"Encrypted Disc Key: {encryptedDiscKey}\n";
            if (!string.IsNullOrEmpty(playerKey))
                protection += $"Player Key: {playerKey}\n";
            if (!string.IsNullOrEmpty(decryptedDiscKey))
                protection += $"Decrypted Disc Key: {decryptedDiscKey}\n";

            return protection;
        }

        /// <summary>
        /// Get the detected error count from the input files, if possible
        /// </summary>
        /// <param name="edcecc">.img_EdcEcc.txt/.img_EccEdc.txt file location</param>
        /// <returns>Error count if possible, -1 on error</returns>
        private static long GetErrorCount(string edcecc)
        {
            // TODO: Better usage of _mainInfo and _c2Error for uncorrectable errors

            // If the file doesn't exist, we can't get info from it
            if (!File.Exists(edcecc))
                return -1;

            // First line of defense is the EdcEcc error file
            using (StreamReader sr = File.OpenText(edcecc))
            {
                try
                {
                    // Read in the error count whenever we find it
                    while (!sr.EndOfStream)
                    {
                        string line = sr.ReadLine().Trim();

                        if (line.StartsWith("[NO ERROR]"))
                        {
                            return 0;
                        }
                        else if (line.StartsWith("Total errors"))
                        {
                            if (Int64.TryParse(line.Substring("Total errors: ".Length).Trim(), out long te))
                                return te;
                            else
                                return Int64.MinValue;
                        }
                    }

                    // If we haven't found anything, return -1
                    return -1;
                }
                catch
                {
                    // We don't care what the exception is right now
                    return Int64.MaxValue;
                }
            }
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
        /// Get the layerbreak from the input file, if possible
        /// </summary>
        /// <param name="disc">_disc.txt file location</param>
        /// <param name="xgd">True if XGD layerbreak info should be used, false otherwise</param>
        /// <returns>Layerbreak if possible, null on error</returns>
        private static string GetLayerbreak(string disc, bool xgd)
        {
            // If the file doesn't exist, we can't get info from it
            if (!File.Exists(disc))
                return null;

            using (StreamReader sr = File.OpenText(disc))
            {
                try
                {
                    string line = sr.ReadLine();
                    while (line != null)
                    {
                        // Trim the line for later use
                        line = line.Trim();

                        // Single-layer discs have no layerbreak
                        if (line.Contains("NumberOfLayers: Single Layer"))
                        {
                            return null;
                        }

                        // Xbox discs have a special layerbreaks
                        else if (xgd && line.StartsWith("LayerBreak"))
                        {
                            // LayerBreak: <size> (L0 Video: <size>, L0 Middle: <size>, L0 Game: <size>)
                            return line.Split(' ')[1];
                        }

                        // Dual-layer discs have a regular layerbreak
                        else if (!xgd && line.StartsWith("LayerZeroSector"))
                        {
                            // LayerZeroSector: <size> (<hex>)
                            return line.Split(' ')[1];
                        }

                        line = sr.ReadLine();
                    }

                    // If we get to the end, there's an issue
                    return null;
                }
                catch
                {
                    // We don't care what the exception is right now
                    return null;
                }
            }
        }

        /// <summary>
        /// Get if LibCrypt data is detected in the subchannel file, if possible
        /// </summary>
        /// <param name="sub">.sub file location</param>
        /// <returns>Status of the LibCrypt data, if possible</returns>
        private static bool? GetLibCryptDetected(string sub)
        {
            // If the file doesn't exist, we can't get info from it
            if (!File.Exists(sub))
                return null;

            return LibCrypt.CheckSubfile(sub);
        }

        /// <summary>
        /// Get the hex contents of the PIC file
        /// </summary>
        /// <param name="picPath">Path to the PIC.bin file associated with the dump</param>
        /// <returns>PIC data as a hex string if possible, null on error</returns>
        /// <remarks>https://stackoverflow.com/questions/9932096/add-separator-to-string-at-every-n-characters</remarks>
        private static string GetPIC(string picPath)
        {
            // If the file doesn't exist, we can't get the info
            if (!File.Exists(picPath))
                return null;

            try
            {
                using (BinaryReader br = new BinaryReader(File.OpenRead(picPath)))
                {
                    string hex = BitConverter.ToString(br.ReadBytes(140)).Replace("-", string.Empty);
                    return Regex.Replace(hex, ".{32}", "$0\n");
                }
            }
            catch
            {
                // We don't care what the error was right now
                return null;
            }
        }

        /// <summary>
        /// Get the existance of an anti-modchip string from the input file, if possible
        /// </summary>
        /// <param name="disc">_disc.txt file location</param>
        /// <returns>Anti-modchip existance if possible, false on error</returns>
        private static bool GetPlayStationAntiModchipDetected(string disc)
        {
            // If the file doesn't exist, we can't get info from it
            if (!File.Exists(disc))
                return false;

            using (StreamReader sr = File.OpenText(disc))
            {
                try
                {
                    // Check for either antimod string
                    string line = sr.ReadLine().Trim();
                    while (!sr.EndOfStream)
                    {
                        if (line.StartsWith("Detected anti-mod string"))
                            return true;
                        else if (line.StartsWith("No anti-mod string"))
                            return false;

                        line = sr.ReadLine().Trim();
                    }

                    return false;
                }
                catch
                {
                    // We don't care what the exception is right now
                    return false;
                }
            }
        }

        /// <summary>
        /// Get the detected missing EDC count from the input files, if possible
        /// </summary>
        /// <param name="edcecc">.img_EdcEcc.txt file location</param>
        /// <returns>Status of PS1 EDC, if possible</returns>
        private static bool? GetPlayStationEDCStatus(string edcecc)
        {
            // If one of the files doesn't exist, we can't get info from them
            if (!File.Exists(edcecc))
                return null;

            // First line of defense is the EdcEcc error file
            int modeTwoNoEdc = 0;
            int modeTwoFormTwo = 0;
            using (StreamReader sr = File.OpenText(edcecc))
            {
                try
                {
                    while (!sr.EndOfStream)
                    {
                        string line = sr.ReadLine();
                        if (line.Contains("mode 2 form 2"))
                            modeTwoFormTwo++;
                        else if (line.Contains("mode 2 no edc"))
                            modeTwoNoEdc++;
                    }

                    // This shouldn't happen
                    if (modeTwoNoEdc == 0 && modeTwoFormTwo == 0)
                        return null;

                    // EDC exists
                    else if (modeTwoNoEdc == 0 && modeTwoFormTwo != 0)
                        return true;

                    // EDC doesn't exist
                    else if (modeTwoNoEdc != 0 && modeTwoFormTwo == 0)
                        return false;

                    // This shouldn't happen
                    else if (modeTwoNoEdc != 0 && modeTwoFormTwo != 0)
                        return null;

                    // No idea how it would fall through
                    return null;
                }
                catch
                {
                    // We don't care what the exception is right now
                    return null;
                }
            }
        }

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
                    while (!sr.ReadLine().StartsWith("========== LBA[000016, 0x00010]: Main Channel ==========")) ;

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
        /// Get the build info from a Saturn disc, if possible
        /// </summary>
        /// <<param name="segaHeader">String representing a formatter variant of the Saturn header</param>
        /// <returns>True on successful extraction of info, false otherwise</returns>
        private static bool GetSaturnBuildInfo(string segaHeader, out string serial, out string version, out string date)
        {
            serial = null; version = null; date = null;

            // If the input header is null, we can't do a thing
            if (string.IsNullOrWhiteSpace(segaHeader))
                return false;

            // Now read it in cutting it into lines for easier parsing
            try
            {
                string[] header = segaHeader.Split('\n');
                string serialVersionLine = header[2].Substring(58);
                string dateLine = header[3].Substring(58);
                serial = serialVersionLine.Substring(0, 8);
                version = serialVersionLine.Substring(10, 6);
                date = dateLine.Substring(0, 8);
                return true;
            }
            catch
            {
                // We don't care what the error is
                return false;
            }
        }

        /// <summary>
        /// Get the build info from a Sega CD disc, if possible
        /// </summary>
        /// <<param name="segaHeader">String representing a formatter variant of the  Sega CD header</param>
        /// <returns>True on successful extraction of info, false otherwise</returns>
        /// <remarks>Note that this works for MOST headers, except ones where the copyright stretches > 1 line</remarks>
        private static bool GetSegaCDBuildInfo(string segaHeader, out string serial, out string date)
        {
            serial = null; date = null;

            // If the input header is null, we can't do a thing
            if (string.IsNullOrWhiteSpace(segaHeader))
                return false;

            // Now read it in cutting it into lines for easier parsing
            try
            {
                string[] header = segaHeader.Split('\n');
                string serialVersionLine = header[8].Substring(58);
                string dateLine = header[1].Substring(58);
                serial = serialVersionLine.Substring(3, 7);
                date = dateLine.Substring(8).Trim();

                // Properly format the date string, if possible
                string[] dateSplit = date.Split('.');

                if (dateSplit.Length == 1)
                    dateSplit = new string[] { date.Substring(0, 4), date.Substring(4) };

                string month = dateSplit[1];
                switch (month)
                {
                    case "JAN":
                        dateSplit[1] = "01";
                        break;
                    case "FEB":
                        dateSplit[1] = "02";
                        break;
                    case "MAR":
                        dateSplit[1] = "03";
                        break;
                    case "APR":
                        dateSplit[1] = "04";
                        break;
                    case "MAY":
                        dateSplit[1] = "05";
                        break;
                    case "JUN":
                        dateSplit[1] = "06";
                        break;
                    case "JUL":
                        dateSplit[1] = "07";
                        break;
                    case "AUG":
                        dateSplit[1] = "08";
                        break;
                    case "SEP":
                        dateSplit[1] = "09";
                        break;
                    case "OCT":
                        dateSplit[1] = "10";
                        break;
                    case "NOV":
                        dateSplit[1] = "11";
                        break;
                    case "DEC":
                        dateSplit[1] = "12";
                        break;
                    default:
                        dateSplit[1] = "00";
                        break;
                }

                date = string.Join("-", dateSplit);

                return true;
            }
            catch
            {
                // We don't care what the error is
                return false;
            }
        }

        /// <summary>
        /// Get the header from a Sega CD / Mega CD, Saturn, or Dreamcast Low-Density region, if possible
        /// </summary>
        /// <param name="mainInfo">_mainInfo.txt file location</param>
        /// <returns>Header as a byte array if possible, null on error</returns>
        private static string GetSegaHeader(string mainInfo)
        {
            // If the file doesn't exist, we can't get info from it
            if (!File.Exists(mainInfo))
                return null;

            using (StreamReader sr = File.OpenText(mainInfo))
            {
                try
                {
                    // Make sure we're in the right sector
                    while (!sr.ReadLine().StartsWith("========== LBA[000000, 0000000]: Main Channel ==========")) ;

                    // Fast forward to the header
                    while (!sr.ReadLine().Trim().StartsWith("+0 +1 +2 +3 +4 +5 +6 +7  +8 +9 +A +B +C +D +E +F")) ;

                    // Now that we're at the Header, read each line in and concatenate
                    string header = "";
                    for (int i = 0; i < 32; i++)
                        header += sr.ReadLine() + "\n"; // 0000-01F0

                    return header;
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
        private static bool GetUMDAuxInfo(string disc, out string title, out Category? umdcat, out string umdversion, out string umdlayer, out long umdsize)
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

        /// <summary>
        /// Get the write offset from the input file, if possible
        /// </summary>
        /// <param name="disc">_disc.txt file location</param>
        /// <returns>Sample write offset if possible, null on error</returns>
        private static string GetWriteOffset(string disc)
        {
            // If the file doesn't exist, we can't get info from it
            if (!File.Exists(disc))
                return null;

            using (StreamReader sr = File.OpenText(disc))
            {
                try
                {
                    // Fast forward to the offsets
                    while (!sr.ReadLine().Trim().StartsWith("========== Offset")) ;
                    sr.ReadLine(); // Combined Offset
                    sr.ReadLine(); // Drive Offset
                    sr.ReadLine(); // Separator line

                    // Now that we're at the offsets, attempt to get the sample offset
                    return sr.ReadLine().Split(' ').LastOrDefault();
                }
                catch
                {
                    // We don't care what the exception is right now
                    return null;
                }
            }
        }

        /// <summary>
        /// Get the XGD auxiliary info from the outputted files, if possible
        /// </summary>
        /// <param name="disc">_disc.txt file location</param>
        /// <returns>True on successful extraction of info, false otherwise</returns>
        private static bool GetXgdAuxInfo(string disc, out string dmihash, out string pfihash, out string sshash, out string ss, out string ssver)
        {
            dmihash = null; pfihash = null; sshash = null; ss = null; ssver = null;

            // If the file doesn't exist, we can't get info from it
            if (!File.Exists(disc))
                return false;

            using (StreamReader sr = File.OpenText(disc))
            {
                try
                {
                    while(!sr.EndOfStream)
                    {
                        string line = sr.ReadLine().Trim();

                        // Security Sector version
                        if (line.StartsWith("Version of challenge table"))
                        {
                            ssver = line.Split(' ')[4]; // "Version of challenge table: <VER>"
                        }

                        // Security Sector ranges
                        else if (line.StartsWith("Number of security sector ranges:"))
                        {
                            Regex layerRegex = new Regex(@"Layer [01].*, startLBA-endLBA:\s*(\d+)-\s*(\d+)");

                            line = sr.ReadLine().Trim();
                            while (!line.StartsWith("========== Unlock 2 state(wxripper) =========="))
                            {
                                // If we have a recognized line format, parse it
                                if (line.StartsWith("Layer "))
                                {
                                    var match = layerRegex.Match(line);
                                    ss += $"{match.Groups[1]}-{match.Groups[2]}\n";
                                }

                                line = sr.ReadLine().Trim();
                            }
                        }

                        // Special File Hashes
                        else if (line.StartsWith("<rom"))
                        {
                            if (line.Contains("SS.bin"))
                                sshash = line;
                            else if (line.Contains("PFI.bin"))
                                pfihash = line;
                            else if (line.Contains("DMI.bin"))
                                dmihash = line;
                        }
                    }

                    return true;
                }
                catch
                {
                    // We don't care what the exception is right now
                    return false;
                }
            }
        }

        /// <summary>
        /// Get the Xbox serial info from the DMI.bin file, if possible
        /// </summary>
        /// <param name="dmi">DMI.bin file location</param>
        /// <returns>True on successful extraction of info, false otherwise</returns>
        private static bool GetXboxDMIInfo(string dmi, out string serial, out string version, out Region? region)
        {
            serial = null; version = null; region = Region.World;

            if (!File.Exists(dmi))
                return false;

            using (BinaryReader br = new BinaryReader(File.OpenRead(dmi)))
            {
                try
                {
                    br.BaseStream.Seek(8, SeekOrigin.Begin);
                    char[] str = br.ReadChars(8);

                    serial = $"{str[0]}{str[1]}-{str[2]}{str[3]}{str[4]}";
                    version = $"1.{str[5]}{str[6]}";
                    region = GetXgdRegion(str[7]);
                    return true;
                }
                catch
                {
                    return false;
                }
            }
        }

        /// <summary>
        /// Get the Xbox 360 serial info from the DMI.bin file, if possible
        /// </summary>
        /// <param name="dmi">DMI.bin file location</param>
        /// <returns>True on successful extraction of info, false otherwise</returns>
        private static bool GetXbox360DMIInfo(string dmi, out string serial, out string version, out Region? region)
        {
            serial = null; version = null; region = Region.World;

            if (!File.Exists(dmi))
                return false;

            using (BinaryReader br = new BinaryReader(File.OpenRead(dmi)))
            {
                try
                {
                    br.BaseStream.Seek(64, SeekOrigin.Begin);
                    char[] str = br.ReadChars(14);

                    serial = $"{str[0]}{str[1]}-{str[2]}{str[3]}{str[4]}{str[5]}";
                    version = $"1.{str[6]}{str[7]}";
                    region = GetXgdRegion(str[8]);
                    // str[9], str[10], str[11] - unknown purpose
                    // str[12], str[13] - disc <12> of <13>
                    return true;
                }
                catch
                {
                    return false;
                }
            }
        }

        #endregion
    }
}
