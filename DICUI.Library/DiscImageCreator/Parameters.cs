using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using DICUI.Data;

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
                    // return File.Exists(combinedBase + ".c2") // Doesn't output on Linux
                    return File.Exists(basePath + ".ccd")
                        && File.Exists(basePath + ".cue")
                        && File.Exists(basePath + ".dat")
                        && File.Exists(basePath + ".img")
                        && (audioOnly || File.Exists(basePath + ".img_EdcEcc.txt") || File.Exists(basePath + ".img_EccEdc.txt"))
                        && (audioOnly || File.Exists(basePath + ".scm"))
                        && File.Exists(basePath + ".sub")
                        // && File.Exists(combinedBase + "_c2Error.txt") // Doesn't output on Linux
                        && File.Exists(basePath + "_cmd.txt")
                        && File.Exists(basePath + "_disc.txt")
                        && File.Exists(basePath + "_drive.txt")
                        && File.Exists(basePath + "_img.cue")
                        && File.Exists(basePath + "_mainError.txt")
                        && File.Exists(basePath + "_mainInfo.txt")
                        && File.Exists(basePath + "_subError.txt")
                        && File.Exists(basePath + "_subInfo.txt")
                        // && File.Exists(combinedBase + "_subIntention.txt") // Not guaranteed output
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
                    bool cleanRipDump = File.Exists(basePath + "-dumpinfo.txt")
                        && File.Exists(basePath + ".bca");
                    return dicDump | cleanRipDump;

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
        /// Get the list of commands that use a given flag
        /// </summary>
        /// <param name="flag">Flag value to get commands for</param>
        /// <returns>List of DiscImageCreator.Commands, if possible</returns>
        private List<Command> GetSupportedCommands(Flag flag)
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
            if (!Utilities.Validators.GetValidMediaTypes(system).Contains(type))
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
    }
}
