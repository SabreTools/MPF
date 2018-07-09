using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using DICUI.Data;

namespace DICUI.Utilities
{
    /// <summary>
    ///  Represents a generic set of DIC parameters
    /// </summary>
    public class Parameters
    {
        // DIC Command
        public DICCommand Command;

        // Drive Information
        public string DriveLetter;
        int? DriveSpeed;

        // Path Information
        public string Filename;

        // Sector Information
        public int? StartLBAValue;
        public int? EndLBAValue;

        // DIC Flags
        private Dictionary<DICFlag, bool> _flags;
        public bool this[DICFlag key]
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

        // DIC Flag Values
        public int? AddOffsetValue;
        public string BEOpcodeValue; // raw (default), pack
        public int?[] C2OpcodeValue = new int?[4];    // Reread Value;
        //public int? C2OpcodeValue2;   // 0 reread issue sector (default), 1 reread all
        //public int? C2OpcodeValue3;   // First LBA to reread (default 0)
        //public int? C2OpcodeValue4;   // Last LBA to reread (default EOS)
        public int? ForceUnitAccessValue; // Delete per specified (default 1)
        public int? ScanFileProtectValue; // Timeout value (default 60)
        public int? SubchannelReadLevelValue; // 0 no next sub, 1 next sub (default), 2 next and next next

        /// <summary>
        /// Generic empty constructor for adding things individually
        /// </summary>
        public Parameters()
        {
        }

        /// <summary>
        /// Populate a Parameters object from a param string
        /// </summary>
        /// <param name="parameters">String possibly representing a set of parameters</param>
        public Parameters(string parameters)
        {
            // The string has to be valid by itself first
            if (String.IsNullOrWhiteSpace(parameters))
            {
                return;
            }

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
                case DICCommandStrings.Audio:
                    if (!DoesExist(parts, 1) || !IsValidDriveLetter(parts[1]))
                        return;
                    else
                        DriveLetter = parts[1];

                    if (!DoesExist(parts, 2) || IsFlag(parts[2]))
                        return;
                    else
                        Filename = parts[2];

                    if (!DoesExist(parts, 3) || !IsValidNumber(parts[3], lowerBound: 0, upperBound: 72))
                        return;
                    else
                        DriveSpeed = Int32.Parse(parts[3]);

                    if (!DoesExist(parts, 4) || !IsValidNumber(parts[4], lowerBound: 0))
                        return;
                    else
                        StartLBAValue = Int32.Parse(parts[4]);

                    if (!DoesExist(parts, 5) || !IsValidNumber(parts[5], lowerBound: 0))
                        return;
                    else
                        EndLBAValue = Int32.Parse(parts[5]);

                    Command = DICCommand.Audio;
                    index = 6;
                    break;

                case DICCommandStrings.BluRay:
                    if (!DoesExist(parts, 1) || !IsValidDriveLetter(parts[1]))
                        return;
                    else
                        DriveLetter = parts[1];

                    if (!DoesExist(parts, 2) || IsFlag(parts[2]))
                        return;
                    else
                        Filename = parts[2];

                    Command = DICCommand.BluRay;
                    index = 3;
                    break;

                case DICCommandStrings.Close:
                    if (!DoesExist(parts, 1) || !IsValidDriveLetter(parts[1]))
                        return;
                    else
                        DriveLetter = parts[1];

                    if (parts.Count > 2)
                        return;

                    Command = DICCommand.Close;
                    break;

                case DICCommandStrings.CompactDisc:
                    if (!DoesExist(parts, 1) || !IsValidDriveLetter(parts[1]))
                        return;
                    else
                        DriveLetter = parts[1];

                    if (!DoesExist(parts, 2) || IsFlag(parts[2]))
                        return;
                    else
                        Filename = parts[2];

                    if (!DoesExist(parts, 3) || !IsValidNumber(parts[3], lowerBound: 0, upperBound: 72))
                        return;
                    else
                        DriveSpeed = Int32.Parse(parts[3]);

                    Command = DICCommand.CompactDisc;
                    index = 4;
                    break;

                case DICCommandStrings.Data:
                    if (!DoesExist(parts, 1) || !IsValidDriveLetter(parts[1]))
                        return;
                    else
                        DriveLetter = parts[1];

                    if (!DoesExist(parts, 2) || IsFlag(parts[2]))
                        return;
                    else
                        Filename = parts[2];

                    if (!DoesExist(parts, 3) || !IsValidNumber(parts[3], lowerBound: 0, upperBound: 72))
                        return;
                    else
                        DriveSpeed = Int32.Parse(parts[3]);

                    if (!DoesExist(parts, 4) || !IsValidNumber(parts[4], lowerBound: 0))
                        return;
                    else
                        StartLBAValue = Int32.Parse(parts[4]);

                    if (!DoesExist(parts, 5) || !IsValidNumber(parts[5], lowerBound: 0))
                        return;
                    else
                        EndLBAValue = Int32.Parse(parts[5]);

                    Command = DICCommand.Data;
                    index = 6;
                    break;

                case DICCommandStrings.DigitalVideoDisc:
                    if (!DoesExist(parts, 1) || !IsValidDriveLetter(parts[1]))
                        return;
                    else
                        DriveLetter = parts[1];

                    if (!DoesExist(parts, 2) || IsFlag(parts[2]))
                        return;
                    else
                        Filename = parts[2];

                    if (!DoesExist(parts, 3) || !IsValidNumber(parts[3], lowerBound: 0, upperBound: 24)) // Officially 0-16
                        return;
                    else
                        DriveSpeed = Int32.Parse(parts[3]);

                    Command = DICCommand.DigitalVideoDisc;
                    index = 4;
                    break;

                case DICCommandStrings.DriveSpeed:
                    if (!DoesExist(parts, 1) || !IsValidDriveLetter(parts[1]))
                        return;
                    else
                        DriveLetter = parts[1];

                    if (parts.Count > 2)
                        return;

                    Command = DICCommand.DriveSpeed;
                    break;

                case DICCommandStrings.Eject:
                    if (!DoesExist(parts, 1) || !IsValidDriveLetter(parts[1]))
                        return;
                    else
                        DriveLetter = parts[1];

                    if (parts.Count > 2)
                        return;

                    Command = DICCommand.Eject;
                    break;

                case DICCommandStrings.Floppy:
                    if (!DoesExist(parts, 1) || !IsValidDriveLetter(parts[1]))
                        return;
                    else
                        DriveLetter = parts[1];

                    if (!DoesExist(parts, 2) || IsFlag(parts[2]))
                        return;
                    else
                        Filename = parts[2];

                    if (parts.Count > 3)
                        return;

                    Command = DICCommand.Floppy;
                    break;

                case DICCommandStrings.GDROM:
                    if (!DoesExist(parts, 1) || !IsValidDriveLetter(parts[1]))
                        return;
                    else
                        DriveLetter = parts[1];

                    if (!DoesExist(parts, 2) || IsFlag(parts[2]))
                        return;
                    else
                        Filename = parts[2];

                    if (!DoesExist(parts, 3) || !IsValidNumber(parts[3], lowerBound: 0, upperBound: 72))
                        return;
                    else
                        DriveSpeed = Int32.Parse(parts[3]);

                    Command = DICCommand.GDROM;
                    index = 4;
                    break;

                case DICCommandStrings.MDS:
                    if (!DoesExist(parts, 1) || IsFlag(parts[1]) || !File.Exists(parts[1]))
                        return;
                    else
                        Filename = parts[1];

                    if (parts.Count > 2)
                        return;

                    Command = DICCommand.MDS;
                    break;

                case DICCommandStrings.Reset:
                    if (!DoesExist(parts, 1) || !IsValidDriveLetter(parts[1]))
                        return;
                    else
                        DriveLetter = parts[1];

                    if (parts.Count > 2)
                        return;

                    Command = DICCommand.Reset;
                    break;

                case DICCommandStrings.Start:
                    if (!DoesExist(parts, 1) || !IsValidDriveLetter(parts[1]))
                        return;
                    else
                        DriveLetter = parts[1];

                    if (parts.Count > 2)
                        return;

                    Command = DICCommand.Start;
                    break;

                case DICCommandStrings.Stop:
                    if (!DoesExist(parts, 1) || !IsValidDriveLetter(parts[1]))
                        return;
                    else
                        DriveLetter = parts[1];

                    if (parts.Count > 2)
                        return;

                    Command = DICCommand.Stop;
                    break;

                case DICCommandStrings.Sub:
                    if (!DoesExist(parts, 1) || IsFlag(parts[1]) || !File.Exists(parts[1]))
                        return;
                    else
                        Filename = parts[1];

                    if (parts.Count > 2)
                        return;

                    Command = DICCommand.Sub;
                    break;

                case DICCommandStrings.Swap:
                    if (!DoesExist(parts, 1) || !IsValidDriveLetter(parts[1]))
                        return;
                    else
                        DriveLetter = parts[1];

                    if (!DoesExist(parts, 2) || IsFlag(parts[2]))
                        return;
                    else
                        Filename = parts[2];

                    if (!DoesExist(parts, 3) || !IsValidNumber(parts[3], lowerBound: 0, upperBound: 72))
                        return;
                    else
                        DriveSpeed = Int32.Parse(parts[3]);

                    Command = DICCommand.Swap;
                    index = 4;
                    break;

                case DICCommandStrings.XBOX:
                    if (!DoesExist(parts, 1) || !IsValidDriveLetter(parts[1]))
                        return;
                    else
                        DriveLetter = parts[1];

                    if (!DoesExist(parts, 2) || IsFlag(parts[2]))
                        return;
                    else
                        Filename = parts[2];

                    Command = DICCommand.XBOX;
                    index = 3;
                    break;

                default:
                    return;
            }

            // Loop through all auxilary flags, if necessary
            if (index > 0)
            {
                for (int i = index; i < parts.Count; i++)
                {
                    switch (parts[i])
                    {
                        case DICFlagStrings.AddOffset:
                            if (parts[0] != DICCommandStrings.Audio
                                && parts[0] != DICCommandStrings.CompactDisc)
                                return;
                            else if (!DoesExist(parts, i + 1))
                                return;
                            else if (!IsValidNumber(parts[i + 1]))
                                return;

                            this[DICFlag.AddOffset] = true;
                            AddOffsetValue = Int32.Parse(parts[i + 1]);
                            i++;
                            break;

                        case DICFlagStrings.AMSF:
                            if (parts[0] != DICCommandStrings.CompactDisc)
                                return;

                            this[DICFlag.AMSF] = true;
                            break;

                        case DICFlagStrings.BEOpcode:
                            if (parts[0] != DICCommandStrings.Audio
                                && parts[0] != DICCommandStrings.CompactDisc
                                && parts[0] != DICCommandStrings.Data
                                && parts[0] != DICCommandStrings.GDROM)
                                return;
                            else if (!DoesExist(parts, i + 1))
                            {
                                this[DICFlag.BEOpcode] = true;
                                break;
                            }
                            else if (IsFlag(parts[i + 1]))
                            {
                                this[DICFlag.BEOpcode] = true;
                                break;
                            }
                            else if (parts[i + 1] != "raw" && (parts[i + 1] != "pack"))
                                return;

                            this[DICFlag.BEOpcode] = true;
                            BEOpcodeValue = parts[i + 1];
                            i++;
                            break;

                        case DICFlagStrings.C2Opcode:
                            if (parts[0] != DICCommandStrings.Audio
                                && parts[0] != DICCommandStrings.CompactDisc
                                && parts[0] != DICCommandStrings.Data
                                && parts[0] != DICCommandStrings.GDROM)
                                return;

                            this[DICFlag.C2Opcode] = true;
                            for (int j = 0; j < 4; j++)
                            {
                                if (!DoesExist(parts, i + 1))
                                    break;
                                else if (IsFlag(parts[i + 1]))
                                    break;
                                else if (!IsValidNumber(parts[i + 1], lowerBound: 0))
                                    return;
                                else
                                {
                                    C2OpcodeValue[j] = Int32.Parse(parts[i + 1]);
                                    i++;
                                }
                            }

                            break;

                        case DICFlagStrings.CopyrightManagementInformation:
                            if (parts[0] != DICCommandStrings.DigitalVideoDisc)
                                return;

                            this[DICFlag.CopyrightManagementInformation] = true;
                            break;

                        case DICFlagStrings.D8Opcode:
                            if (parts[0] != DICCommandStrings.Audio
                                && parts[0] != DICCommandStrings.CompactDisc
                                && parts[0] != DICCommandStrings.Data
                                && parts[0] != DICCommandStrings.GDROM)
                                return;

                            this[DICFlag.D8Opcode] = true;
                            break;

                        case DICFlagStrings.DisableBeep:
                            if (parts[0] != DICCommandStrings.Audio
                                && parts[0] != DICCommandStrings.BluRay
                                && parts[0] != DICCommandStrings.CompactDisc
                                && parts[0] != DICCommandStrings.Data
                                && parts[0] != DICCommandStrings.DigitalVideoDisc
                                && parts[0] != DICCommandStrings.GDROM
                                && parts[0] != DICCommandStrings.XBOX)
                                return;

                            this[DICFlag.DisableBeep] = true;
                            break;

                        case DICFlagStrings.ForceUnitAccess:
                            if (parts[0] != DICCommandStrings.Audio
                                && parts[0] != DICCommandStrings.BluRay
                                && parts[0] != DICCommandStrings.CompactDisc
                                && parts[0] != DICCommandStrings.DigitalVideoDisc
                                && parts[0] != DICCommandStrings.Data
                                && parts[0] != DICCommandStrings.GDROM
                                && parts[0] != DICCommandStrings.XBOX)
                                return;
                            else if (!DoesExist(parts, i + 1))
                            {
                                this[DICFlag.ForceUnitAccess] = true;
                                break;
                            }
                            else if (IsFlag(parts[i + 1]))
                            {
                                this[DICFlag.ForceUnitAccess] = true;
                                break;
                            }
                            else if (!IsValidNumber(parts[i + 1], lowerBound: 0))
                                return;

                            this[DICFlag.ForceUnitAccess] = true;
                            ForceUnitAccessValue = Int32.Parse(parts[i + 1]);
                            i++;
                            break;

                        case DICFlagStrings.MCN:
                            if (parts[0] != DICCommandStrings.CompactDisc)
                                return;

                            this[DICFlag.MCN] = true;
                            break;

                        case DICFlagStrings.MultiSession:
                            if (parts[0] != DICCommandStrings.CompactDisc)
                                return;

                            this[DICFlag.MultiSession] = true;
                            break;

                        case DICFlagStrings.NoFixSubP:
                            if (parts[0] != DICCommandStrings.Audio
                                && parts[0] != DICCommandStrings.CompactDisc
                                && parts[0] != DICCommandStrings.Data
                                && parts[0] != DICCommandStrings.GDROM)
                                return;

                            this[DICFlag.NoFixSubP] = true;
                            break;

                        case DICFlagStrings.NoFixSubQ:
                            if (parts[0] != DICCommandStrings.Audio
                                && parts[0] != DICCommandStrings.CompactDisc
                                && parts[0] != DICCommandStrings.Data
                                && parts[0] != DICCommandStrings.GDROM)
                                return;

                            this[DICFlag.NoFixSubQ] = true;
                            break;

                        case DICFlagStrings.NoFixSubQLibCrypt:
                            if (parts[0] != DICCommandStrings.CompactDisc)
                                return;

                            this[DICFlag.NoFixSubQLibCrypt] = true;
                            break;

                        case DICFlagStrings.NoFixSubQSecuROM:
                            if (parts[0] != DICCommandStrings.Audio
                                && parts[0] != DICCommandStrings.CompactDisc
                                && parts[0] != DICCommandStrings.Data
                                && parts[0] != DICCommandStrings.GDROM)
                                return;

                            this[DICFlag.NoFixSubQSecuROM] = true;
                            break;

                        case DICFlagStrings.NoFixSubRtoW:
                            if (parts[0] != DICCommandStrings.Audio
                                && parts[0] != DICCommandStrings.CompactDisc
                                && parts[0] != DICCommandStrings.Data
                                && parts[0] != DICCommandStrings.GDROM)
                                return;

                            this[DICFlag.NoFixSubRtoW] = true;
                            break;

                        case DICFlagStrings.Raw:
                            if (parts[0] != DICCommandStrings.DigitalVideoDisc)
                                return;

                            this[DICFlag.Raw] = true;
                            break;

                        case DICFlagStrings.Reverse:
                            if (parts[0] != DICCommandStrings.CompactDisc
                                && parts[0] != DICCommandStrings.Data)
                                return;

                            this[DICFlag.Reverse] = true;
                            break;

                        case DICFlagStrings.ScanAntiMod:
                            if (parts[0] != DICCommandStrings.CompactDisc)
                                return;

                            this[DICFlag.ScanAntiMod] = true;
                            break;

                        case DICFlagStrings.ScanFileProtect:
                            if (parts[0] != DICCommandStrings.CompactDisc
                                && parts[0] != DICCommandStrings.Data)
                                return;
                            else if (!DoesExist(parts, i + 1))
                            {
                                this[DICFlag.ScanFileProtect] = true;
                                break;
                            }
                            else if (IsFlag(parts[i + 1]))
                            {
                                this[DICFlag.ScanFileProtect] = true;
                                break;
                            }
                            else if (!IsValidNumber(parts[i + 1], lowerBound: 0))
                                return;

                            this[DICFlag.ScanFileProtect] = true;
                            ScanFileProtectValue = Int32.Parse(parts[i + 1]);
                            i++;
                            break;

                        case DICFlagStrings.ScanSectorProtect:
                            if (parts[0] != DICCommandStrings.CompactDisc
                                && parts[0] != DICCommandStrings.Data)
                                return;

                            this[DICFlag.ScanSectorProtect] = true;
                            break;

                        case DICFlagStrings.SeventyFour:
                            if (parts[0] != DICCommandStrings.Swap)
                                return;

                            this[DICFlag.SeventyFour] = true;
                            break;

                        case DICFlagStrings.SubchannelReadLevel:
                            if (parts[0] != DICCommandStrings.Audio
                                && parts[0] != DICCommandStrings.CompactDisc
                                && parts[0] != DICCommandStrings.Data
                                && parts[0] != DICCommandStrings.GDROM)
                                return;
                            else if (DoesExist(parts, i + 1))
                            {
                                this[DICFlag.SubchannelReadLevel] = true;
                                break;
                            }
                            else if (IsFlag(parts[i + 1]))
                            {
                                this[DICFlag.SubchannelReadLevel] = true;
                                break;
                            }
                            else if (!IsValidNumber(parts[i + 1], lowerBound: 0, upperBound: 2))
                                return;

                            this[DICFlag.SubchannelReadLevel] = true;
                            SubchannelReadLevelValue = Int32.Parse(parts[i + 1]);
                            i++;
                            break;

                        default:
                            return;
                    }
                }
            }
        }

        /// <summary>
        /// Blindly generate a parameter string based on the inputs
        /// </summary>
        /// <returns>Correctly formatted parameter string, null on error</returns>
        public string GenerateParameters()
        {
            List<string> parameters = new List<string>();

            parameters.Add(Command.Name());

            // Drive Letter
            if (Command == DICCommand.Audio
                || Command == DICCommand.BluRay
                || Command == DICCommand.Close
                || Command == DICCommand.CompactDisc
                || Command == DICCommand.Data
                || Command == DICCommand.DigitalVideoDisc
                || Command == DICCommand.DriveSpeed
                || Command == DICCommand.Eject
                || Command == DICCommand.Floppy
                || Command == DICCommand.GDROM
                || Command == DICCommand.Reset
                || Command == DICCommand.Start
                || Command == DICCommand.Stop
                || Command == DICCommand.Swap
                || Command == DICCommand.XBOX)
            {
                if (DriveLetter != null)
                    parameters.Add(DriveLetter);
                else
                    return null;
            }

            // Filename
            if (Command == DICCommand.Audio
                || Command == DICCommand.BluRay
                || Command == DICCommand.CompactDisc
                || Command == DICCommand.Data
                || Command == DICCommand.DigitalVideoDisc
                || Command == DICCommand.Floppy
                || Command == DICCommand.GDROM
                || Command == DICCommand.MDS
                || Command == DICCommand.Swap
                || Command == DICCommand.Sub
                || Command == DICCommand.XBOX)
            {
                if (Filename != null)
                    parameters.Add(Filename);
                else
                    return null;
            }

            // Drive Speed
            if (Command == DICCommand.Audio
                || Command == DICCommand.CompactDisc
                || Command == DICCommand.Data
                || Command == DICCommand.DigitalVideoDisc
                || Command == DICCommand.GDROM
                || Command == DICCommand.Swap)
            {
                if (DriveSpeed != null)
                    parameters.Add(DriveSpeed.ToString());
                else
                    return null;
            }

            // LBA Markers
            if (Command == DICCommand.Audio
                || Command == DICCommand.Data)
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
            if (Command == DICCommand.Audio
                || Command == DICCommand.CompactDisc)
            {
                if (this[DICFlag.AddOffset])
                {
                    parameters.Add(DICFlagStrings.AddOffset);
                    if (AddOffsetValue != null)
                        parameters.Add(AddOffsetValue.ToString());
                    else
                        return null;
                }
            }

            // AMSF Dumping
            if (Command == DICCommand.CompactDisc)
            {
                if (this[DICFlag.AMSF])
                    parameters.Add(DICFlagStrings.AMSF);
            }

            // BE Opcode
            if (Command == DICCommand.Audio
               || Command == DICCommand.CompactDisc
               || Command == DICCommand.Data
               || Command == DICCommand.GDROM
               || Command == DICCommand.Swap)
            {
                if (this[DICFlag.BEOpcode] && !this[DICFlag.D8Opcode])
                {
                    parameters.Add(DICFlagStrings.BEOpcode);
                    if (BEOpcodeValue != null
                        && (BEOpcodeValue == "raw" || BEOpcodeValue == "pack"))
                        parameters.Add(BEOpcodeValue);
                }
            }

            // C2 Opcode
            if (Command == DICCommand.Audio
               || Command == DICCommand.CompactDisc
               || Command == DICCommand.Data
               || Command == DICCommand.GDROM
               || Command == DICCommand.Swap)
            {
                if (this[DICFlag.C2Opcode])
                {
                    parameters.Add(DICFlagStrings.C2Opcode);
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
            if (Command == DICCommand.DigitalVideoDisc)
            {
                if (this[DICFlag.CopyrightManagementInformation])
                    parameters.Add(DICFlagStrings.CopyrightManagementInformation);
            }

            // D8 Opcode
            if (Command == DICCommand.Audio
               || Command == DICCommand.CompactDisc
               || Command == DICCommand.Data
               || Command == DICCommand.GDROM
               || Command == DICCommand.Swap)
            {
                if (this[DICFlag.D8Opcode])
                    parameters.Add(DICFlagStrings.D8Opcode);
            }

            // Disable Beep
            if (Command == DICCommand.Audio
               || Command == DICCommand.BluRay
               || Command == DICCommand.CompactDisc
               || Command == DICCommand.Data
               || Command == DICCommand.DigitalVideoDisc
               || Command == DICCommand.GDROM
               || Command == DICCommand.Swap
               || Command == DICCommand.XBOX)
            {
                if (this[DICFlag.DisableBeep])
                    parameters.Add(DICFlagStrings.DisableBeep);
            }

            // Force Unit Access
            if (Command == DICCommand.BluRay
               || Command == DICCommand.CompactDisc
               || Command == DICCommand.DigitalVideoDisc
               || Command == DICCommand.Swap
               || Command == DICCommand.XBOX)
            {
                if (this[DICFlag.ForceUnitAccess])
                {
                    parameters.Add(DICFlagStrings.ForceUnitAccess);
                    if (ForceUnitAccessValue != null)
                        parameters.Add(ForceUnitAccessValue.ToString());
                }
            }

            // MCN
            if (Command == DICCommand.CompactDisc)
            {
                if (this[DICFlag.MCN])
                    parameters.Add(DICFlagStrings.MCN);
            }

            // Multi-Session
            if (Command == DICCommand.CompactDisc)
            {
                if (this[DICFlag.MultiSession])
                    parameters.Add(DICFlagStrings.MultiSession);
            }

            // Not fix SubP
            if (Command == DICCommand.Audio
               || Command == DICCommand.CompactDisc
               || Command == DICCommand.Data
               || Command == DICCommand.GDROM
               || Command == DICCommand.Swap)
            {
                if (this[DICFlag.NoFixSubP])
                    parameters.Add(DICFlagStrings.NoFixSubP);
            }

            // Not fix SubQ
            if (Command == DICCommand.Audio
               || Command == DICCommand.CompactDisc
               || Command == DICCommand.Data
               || Command == DICCommand.GDROM
               || Command == DICCommand.Swap)
            {
                if (this[DICFlag.NoFixSubQ])
                    parameters.Add(DICFlagStrings.NoFixSubQ);
            }

            // Not fix SubQ (PlayStation LibCrypt)
            if (Command == DICCommand.Audio
               || Command == DICCommand.CompactDisc
               || Command == DICCommand.Data
               || Command == DICCommand.GDROM
               || Command == DICCommand.Swap)
            {
                if (this[DICFlag.NoFixSubQLibCrypt])
                    parameters.Add(DICFlagStrings.NoFixSubQLibCrypt);
            }
            
            // Not fix SubQ (SecuROM)
            if (Command == DICCommand.Audio
               || Command == DICCommand.CompactDisc
               || Command == DICCommand.Data
               || Command == DICCommand.GDROM
               || Command == DICCommand.Swap)
            {
                if (this[DICFlag.NoFixSubQSecuROM])
                    parameters.Add(DICFlagStrings.NoFixSubQSecuROM);
            }

            // Not fix SubRtoW
            if (Command == DICCommand.Audio
               || Command == DICCommand.CompactDisc
               || Command == DICCommand.Data
               || Command == DICCommand.GDROM
               || Command == DICCommand.Swap)
            {
                if (this[DICFlag.NoFixSubRtoW])
                    parameters.Add(DICFlagStrings.NoFixSubRtoW);
            }

            // Raw read (2064 byte/sector)
            if (Command == DICCommand.DigitalVideoDisc)
            {
                if (this[DICFlag.Raw])
                    parameters.Add(DICFlagStrings.Raw);
            }

            // Reverse read
            if (Command == DICCommand.CompactDisc
               || Command == DICCommand.Data)
            {
                if (this[DICFlag.Reverse])
                    parameters.Add(DICFlagStrings.Reverse);
            }

            // Scan PlayStation anti-mod strings
            if (Command == DICCommand.CompactDisc
               || Command == DICCommand.Data)
            {
                if (this[DICFlag.ScanAntiMod])
                    parameters.Add(DICFlagStrings.ScanAntiMod);
            }

            // Scan file to detect protect
            if (Command == DICCommand.Audio
               || Command == DICCommand.CompactDisc
               || Command == DICCommand.Data
               || Command == DICCommand.Swap)
            {
                if (this[DICFlag.ScanFileProtect])
                {
                    parameters.Add(DICFlagStrings.ScanFileProtect);
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
            if (Command == DICCommand.CompactDisc
               || Command == DICCommand.Data
               || Command == DICCommand.Swap)
            {
                if (this[DICFlag.ScanSectorProtect])
                    parameters.Add(DICFlagStrings.ScanSectorProtect);
            }

            // Scan 74:00:00 (Saturn)
            if (Command == DICCommand.Swap)
            {
                if (this[DICFlag.SeventyFour])
                    parameters.Add(DICFlagStrings.SeventyFour);
            }

            // Set Subchannel read level
            if (Command == DICCommand.Audio
               || Command == DICCommand.CompactDisc
               || Command == DICCommand.Data
               || Command == DICCommand.GDROM
               || Command == DICCommand.Swap)
            {
                if (this[DICFlag.SubchannelReadLevel])
                {
                    parameters.Add(DICFlagStrings.SubchannelReadLevel);
                    if (SubchannelReadLevelValue != null)
                    {
                        if (SubchannelReadLevelValue >= 0 && SubchannelReadLevelValue <= 2)
                            parameters.Add(SubchannelReadLevelValue.ToString());
                        else
                            return null;
                    }
                }
            }

            return string.Join(" ", parameters);
        }

        /// <summary>
        /// Returns whether a string is a valid drive letter
        /// </summary>
        /// <param name="parameter">String value to check</param>
        /// <returns>True if it's a valid drive letter, false otherwise</returns>
        private static bool IsValidDriveLetter(string parameter)
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
        private static bool IsFlag(string parameter)
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
        private static bool DoesExist(List<string> parameters, int index)
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
        private static bool IsValidNumber(string parameter, int lowerBound = -1, int upperBound = -1)
        {
            if (!Int32.TryParse(parameter, out int temp))
                return false;
            else if (lowerBound != -1 && temp < lowerBound)
                return false;
            else if (upperBound != -1 && temp > upperBound)
                return false;

            return true;
        }
    }
}
