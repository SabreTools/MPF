using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using BurnOutSharp.ProtectionType;
using Compress.ThreadReaders;
using MPF.Hashing;
using MPF.Utilities;
using RedumpLib.Data;

namespace MPF.Data
{
    public abstract class BaseParameters
    {
        #region Event Handlers

        /// <summary>
        /// Geneeic way of reporting a message
        /// </summary>
        /// <param name="message">String value to report</param>
        public EventHandler<string> ReportStatus;

        #endregion

        #region Generic Dumping Information

        /// <summary>
        /// Base command to run
        /// </summary>
        public string BaseCommand { get; set; }

        /// <summary>
        /// Set of flags to pass to the executable
        /// </summary>
        protected Dictionary<string, bool?> flags = new Dictionary<string, bool?>();
        protected internal IEnumerable<string> Keys => flags.Keys;

        /// <summary>
        /// Safe access to currently set flags
        /// </summary>
        public bool? this[string key]
        {
            get
            {
                if (flags.ContainsKey(key))
                    return flags[key];

                return null;
            }
            set
            {
                flags[key] = value;
            }
        }

        /// <summary>
        /// Process to track external program
        /// </summary>
        private Process process;

        #endregion

        #region Virtual Dumping Information

        /// <summary>
        /// Command to flag support mappings
        /// </summary>
        public Dictionary<string, List<string>> CommandSupport => GetCommandSupport();

        /// <summary>
        /// Input path for operations
        /// </summary>
        public virtual string InputPath => null;

        /// <summary>
        /// Output path for operations
        /// </summary>
        /// <returns>String representing the path, null on error</returns>
        public virtual string OutputPath => null;

        /// <summary>
        /// Get the processing speed from the implementation
        /// </summary>
        public virtual int? Speed { get; set; } = null;

        #endregion

        #region Metadata

        /// <summary>
        /// Path to the executable
        /// </summary>
        public string ExecutablePath { get; set; }

        /// <summary>
        /// Program that this set of parameters represents
        /// </summary>
        public virtual InternalProgram InternalProgram { get; }

        /// <summary>
        /// Currently represented system
        /// </summary>
        public KnownSystem? System { get; set; }

        /// <summary>
        /// Currently represented media type
        /// </summary>
        public MediaType? Type { get; set; }

        #endregion

        /// <summary>
        /// Populate a Parameters object from a param string
        /// </summary>
        /// <param name="parameters">String possibly representing a set of parameters</param>
        public BaseParameters(string parameters)
        {
            // If any parameters are not valid, wipe out everything
            if (!ValidateAndSetParameters(parameters))
                ResetValues();
        }

        /// <summary>
        /// Generate parameters based on a set of known inputs
        /// </summary>
        /// <param name="system">KnownSystem value to use</param>
        /// <param name="type">MediaType value to use</param>
        /// <param name="driveLetter">Drive letter to use</param>
        /// <param name="filename">Filename to use</param>
        /// <param name="driveSpeed">Drive speed to use</param>
        /// <param name="options">Options object containing all settings that may be used for setting parameters</param>
        public BaseParameters(KnownSystem? system, MediaType? type, char driveLetter, string filename, int? driveSpeed, Options options)
        {
            this.System = system;
            this.Type = type;
            SetDefaultParameters(driveLetter, filename, driveSpeed, options);
        }

        #region Abstract Methods

        /// <summary>
        /// Validate if all required output files exist
        /// </summary>
        /// <param name="basePath">Base filename and path to use for checking</param>
        /// <param name="preCheck">True if this is a check done before a dump, false if done after</param>
        /// <returns>Tuple of true if all required files exist, false otherwise and a list representing missing files</returns>
        public abstract (bool, List<string>) CheckAllOutputFilesExist(string basePath, bool preCheck);

        /// <summary>
        /// Generate a SubmissionInfo for the output files
        /// </summary>
        /// <param name="submissionInfo">Base submission info to fill in specifics for</param>
        /// <param name="basePath">Base filename and path to use for checking</param>
        /// <param name="drive">Drive representing the disc to get information from</param>
        /// <param name="includeArtifacts">True to include output files as encoded artifacts, false otherwise</param>
        public abstract void GenerateSubmissionInfo(SubmissionInfo submissionInfo, string basePath, Drive drive, bool includeArtifacts);

        #endregion

        #region Virtual Methods

        /// <summary>
        /// Get all commands mapped to the supported flags
        /// </summary>
        /// <returns>Mappings from command to supported flags</returns>
        public virtual Dictionary<string, List<string>> GetCommandSupport() => null;

        /// <summary>
        /// Blindly generate a parameter string based on the inputs
        /// </summary>
        /// <returns>Parameter string for invocation, null on error</returns>
        public virtual string GenerateParameters() => null;

        /// <summary>
        /// Get the default extension for a given media type
        /// </summary>
        /// <param name="mediaType">MediaType value to check</param>
        /// <returns>String representing the media type, null on error</returns>
        public virtual string GetDefaultExtension(MediaType? mediaType) => null;

        /// <summary>
        /// Generate a list of all log files generated
        /// </summary>
        /// <param name="basePath">Base filename and path to use for checking</param>
        /// <returns>List of all log file paths, empty otherwise</returns>
        public virtual List<string> GetLogFilePaths(string basePath) => new List<string>();

        /// <summary>
        /// Get the MediaType from the current set of parameters
        /// </summary>
        /// <returns>MediaType value if successful, null on error</returns>
        public virtual MediaType? GetMediaType() => null;

        /// <summary>
        /// Gets if the current command is considered a dumping command or not
        /// </summary>
        /// <returns>True if it's a dumping command, false otherwise</returns>
        public virtual bool IsDumpingCommand() => true;

        /// <summary>
        /// Gets if the flag is supported by the current command
        /// </summary>
        /// <param name="flag">Flag value to check</param>
        /// <returns>True if the flag value is supported, false otherwise</returns>
        public virtual bool IsFlagSupported(string flag)
        {
            if (CommandSupport == null)
                return false;
            if (this.BaseCommand == null)
                return false;
            if (!CommandSupport.ContainsKey(this.BaseCommand))
                return false;
            return CommandSupport[this.BaseCommand].Contains(flag);
        }

        /// <summary>
        /// Returns if the current Parameter object is valid
        /// </summary>
        /// <returns></returns>
        public bool IsValid() => GenerateParameters() != null;

        /// <summary>
        /// Reset all special variables to have default values
        /// </summary>
        protected virtual void ResetValues() { }

        /// <summary>
        /// Set default parameters for a given system and media type
        /// </summary>
        /// <param name="driveLetter">Drive letter to use</param>
        /// <param name="filename">Filename to use</param>
        /// <param name="driveSpeed">Drive speed to use</param>
        /// <param name="options">Options object containing all settings that may be used for setting parameters</param>
        protected virtual void SetDefaultParameters(char driveLetter, string filename, int? driveSpeed, Options options) { }

        /// <summary>
        /// Scan a possible parameter string and populate whatever possible
        /// </summary>
        /// <param name="parameters">String possibly representing parameters</param>
        /// <returns>True if the parameters were set correctly, false otherwise</returns>
        protected virtual bool ValidateAndSetParameters(string parameters) => true;

        #endregion

        #region Execution

        /// <summary>
        /// Run internal program
        /// </summary>
        /// <param name="separateWindow">True to show in separate window, false otherwise</param>
        public void ExecuteInternalProgram(bool separateWindow)
        {
            // Create the start info
            var startInfo = new ProcessStartInfo()
            {
                FileName = ExecutablePath,
                Arguments = GenerateParameters() ?? "",
                CreateNoWindow = !separateWindow,
                UseShellExecute = separateWindow,
                RedirectStandardOutput = !separateWindow,
                RedirectStandardError = !separateWindow,
            };

            // Create the new process
            process = new Process() { StartInfo = startInfo };

            // Start the process
            process.Start();

            // Start processing tasks, if necessary
            if (!separateWindow)
            {
                Logging.OutputToLog(process.StandardOutput, this, ReportStatus);
                Logging.OutputToLog(process.StandardError, this, ReportStatus);
            }

            process.WaitForExit();
            process.Close();
        }

        /// <summary>
        /// Cancel an in-progress dumping process
        /// </summary>
        public void KillInternalProgram()
        {
            try
            {
                while (process != null && !process.HasExited)
                {
                    process.Kill();
                }
            }
            catch
            { }
        }

        #endregion

        #region Parameter Parsing

        /// <summary>
        /// Returns whether or not the selected item exists
        /// </summary>
        /// <param name="parameters">List of parameters to check against</param>
        /// <param name="index">Current index</param>
        /// <returns>True if the next item exists, false otherwise</returns>
        protected static bool DoesExist(List<string> parameters, int index)
        {
            if (index >= parameters.Count)
                return false;

            return true;
        }

        /// <summary>
        /// Get the Base64 representation of a string
        /// </summary>
        /// <param name="content">String content to encode</param>
        /// <returns>Base64-encoded contents, if possible</returns>
        protected static string GetBase64(string content)
        {
            if (string.IsNullOrEmpty(content))
                return null;

            byte[] temp = Encoding.UTF8.GetBytes(content);
            return Convert.ToBase64String(temp);
        }

        /// <summary>
        /// Get the full lines from the input file, if possible
        /// </summary>
        /// <param name="filename">file location</param>
        /// <param name="binary">True if should read as binary, false otherwise (default)</param>
        /// <returns>Full text of the file, null on error</returns>
        protected static string GetFullFile(string filename, bool binary = false)
        {
            // If the file doesn't exist, we can't get info from it
            if (!File.Exists(filename))
                return null;

            // If we're reading as binary
            if (binary)
            {
                byte[] bytes = File.ReadAllBytes(filename);
                return BitConverter.ToString(bytes).Replace("-", string.Empty);
            }

            return string.Join("\n", File.ReadAllLines(filename));
        }

        /// <summary>
        /// Returns whether a string is a valid drive letter
        /// </summary>
        /// <param name="parameter">String value to check</param>
        /// <returns>True if it's a valid drive letter, false otherwise</returns>
        protected static bool IsValidDriveLetter(string parameter)
        {
            if (!Regex.IsMatch(parameter, @"^[A-Z]:?\\?$"))
                return false;

            return true;
        }

        /// <summary>
        /// Returns whether a string is a valid bool
        /// </summary>
        /// <param name="parameter">String value to check</param>
        /// <returns>True if it's a valid bool, false otherwise</returns>
        protected static bool IsValidBool(string parameter)
        {
            return bool.TryParse(parameter, out bool _);
        }

        /// <summary>
        /// Returns whether a string is a valid byte
        /// </summary>
        /// <param name="parameter">String value to check</param>
        /// <param name="lowerBound">Lower bound (>=)</param>
        /// <param name="upperBound">Upper bound (<=)</param>
        /// <returns>True if it's a valid byte, false otherwise</returns>
        protected static bool IsValidInt8(string parameter, sbyte lowerBound = -1, sbyte upperBound = -1)
        {
            (string value, long _) = ExtractFactorFromValue(parameter);
            if (!sbyte.TryParse(value, out sbyte temp))
                return false;
            else if (lowerBound != -1 && temp < lowerBound)
                return false;
            else if (upperBound != -1 && temp > upperBound)
                return false;

            return true;
        }

        /// <summary>
        /// Returns whether a string is a valid Int16
        /// </summary>
        /// <param name="parameter">String value to check</param>
        /// <param name="lowerBound">Lower bound (>=)</param>
        /// <param name="upperBound">Upper bound (<=)</param>
        /// <returns>True if it's a valid Int16, false otherwise</returns>
        protected static bool IsValidInt16(string parameter, short lowerBound = -1, short upperBound = -1)
        {
            (string value, long _) = ExtractFactorFromValue(parameter);
            if (!short.TryParse(value, out short temp))
                return false;
            else if (lowerBound != -1 && temp < lowerBound)
                return false;
            else if (upperBound != -1 && temp > upperBound)
                return false;

            return true;
        }

        /// <summary>
        /// Returns whether a string is a valid Int32
        /// </summary>
        /// <param name="parameter">String value to check</param>
        /// <param name="lowerBound">Lower bound (>=)</param>
        /// <param name="upperBound">Upper bound (<=)</param>
        /// <returns>True if it's a valid Int32, false otherwise</returns>
        protected static bool IsValidInt32(string parameter, int lowerBound = -1, int upperBound = -1)
        {
            (string value, long _) = ExtractFactorFromValue(parameter);
            if (!int.TryParse(value, out int temp))
                return false;
            else if (lowerBound != -1 && temp < lowerBound)
                return false;
            else if (upperBound != -1 && temp > upperBound)
                return false;

            return true;
        }

        /// <summary>
        /// Returns whether a string is a valid Int64
        /// </summary>
        /// <param name="parameter">String value to check</param>
        /// <param name="lowerBound">Lower bound (>=)</param>
        /// <param name="upperBound">Upper bound (<=)</param>
        /// <returns>True if it's a valid Int64, false otherwise</returns>
        protected static bool IsValidInt64(string parameter, long lowerBound = -1, long upperBound = -1)
        {
            (string value, long _) = ExtractFactorFromValue(parameter);
            if (!long.TryParse(value, out long temp))
                return false;
            else if (lowerBound != -1 && temp < lowerBound)
                return false;
            else if (upperBound != -1 && temp > upperBound)
                return false;

            return true;
        }

        /// <summary>
        /// Process a flag parameter
        /// </summary>
        /// <param name="parts">List of parts to be referenced</param>
        /// <param name="flagString">Flag string, if available</param>
        /// <param name="i">Reference to the position in the parts</param>
        /// <returns>True if the parameter was processed successfully or skipped, false otherwise</returns>
        protected bool ProcessFlagParameter(List<string> parts, string flagString, ref int i)
        {
            return ProcessFlagParameter(parts, null, flagString, ref i);
        }

        /// <summary>
        /// Process a flag parameter
        /// </summary>
        /// <param name="parts">List of parts to be referenced</param>
        /// <param name="shortFlagString">Short flag string, if available</param>
        /// <param name="longFlagString">Long flag string, if available</param>
        /// <param name="i">Reference to the position in the parts</param>
        /// <returns>True if the parameter was processed successfully or skipped, false otherwise</returns>
        protected bool ProcessFlagParameter(List<string> parts, string shortFlagString, string longFlagString, ref int i)
        {
            if (parts == null)
                return false;

            if (parts[i] == shortFlagString || parts[i] == longFlagString)
            {
                if (!IsFlagSupported(longFlagString))
                    return false;

                this[longFlagString] = true;
            }

            return true;
        }

        /// <summary>
        /// Process a boolean parameter
        /// </summary>
        /// <param name="parts">List of parts to be referenced</param>
        /// <param name="flagString">Flag string, if available</param>
        /// <param name="i">Reference to the position in the parts</param>
        /// <param name="missingAllowed">True if missing values are allowed, false otherwise</param>
        /// <returns>True if the parameter was processed successfully or skipped, false otherwise</returns>
        protected bool ProcessBooleanParameter(List<string> parts, string flagString, ref int i, bool missingAllowed = false)
        {
            return ProcessBooleanParameter(parts, null, flagString, ref i, missingAllowed);
        }

        /// <summary>
        /// Process a boolean parameter
        /// </summary>
        /// <param name="parts">List of parts to be referenced</param>
        /// <param name="shortFlagString">Short flag string, if available</param>
        /// <param name="longFlagString">Long flag string, if available</param>
        /// <param name="i">Reference to the position in the parts</param>
        /// <param name="missingAllowed">True if missing values are allowed, false otherwise</param>
        /// <returns>True if the parameter was processed successfully or skipped, false otherwise</returns>
        protected bool ProcessBooleanParameter(List<string> parts, string shortFlagString, string longFlagString, ref int i, bool missingAllowed = false)
        {
            if (parts == null)
                return false;

            if (parts[i] == shortFlagString || parts[i] == longFlagString)
            {
                if (!IsFlagSupported(longFlagString))
                {
                    return false;
                }
                else if (!DoesExist(parts, i + 1))
                {
                    if (missingAllowed)
                    {
                        this[longFlagString] = true;
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
                else if (IsFlagSupported(parts[i + 1]))
                {
                    if (missingAllowed)
                    {
                        this[longFlagString] = true;
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
                else if (!IsValidBool(parts[i + 1]))
                {
                    if (missingAllowed)
                    {
                        this[longFlagString] = true;
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }

                this[longFlagString] = bool.Parse(parts[i + 1]);
                i++;
            }

            return true;
        }

        /// <summary>
        /// Process a sbyte parameter
        /// </summary>
        /// <param name="parts">List of parts to be referenced</param>
        /// <param name="flagString">Flag string, if available</param>
        /// <param name="i">Reference to the position in the parts</param>
        /// <param name="missingAllowed">True if missing values are allowed, false otherwise</param>
        /// <returns>SByte value if success, SByte.MinValue if skipped, null on error/returns>
        protected sbyte? ProcessInt8Parameter(List<string> parts, string flagString, ref int i, bool missingAllowed = false)
        {
            return ProcessInt8Parameter(parts, null, flagString, ref i, missingAllowed);
        }

        /// <summary>
        /// Process an sbyte parameter
        /// </summary>
        /// <param name="parts">List of parts to be referenced</param>
        /// <param name="shortFlagString">Short flag string, if available</param>
        /// <param name="longFlagString">Long flag string, if available</param>
        /// <param name="i">Reference to the position in the parts</param>
        /// <param name="missingAllowed">True if missing values are allowed, false otherwise</param>
        /// <returns>SByte value if success, SByte.MinValue if skipped, null on error/returns>
        protected sbyte? ProcessInt8Parameter(List<string> parts, string shortFlagString, string longFlagString, ref int i, bool missingAllowed = false)
        {
            if (parts == null)
                return null;

            if (parts[i] == shortFlagString || parts[i] == longFlagString)
            {
                if (!IsFlagSupported(longFlagString))
                {
                    return null;
                }
                else if (!DoesExist(parts, i + 1))
                {
                    if (missingAllowed)
                        this[longFlagString] = true;

                    return null;
                }
                else if (IsFlagSupported(parts[i + 1]))
                {
                    if (missingAllowed)
                        this[longFlagString] = true;

                    return null;
                }
                else if (!IsValidInt8(parts[i + 1]))
                {
                    if (missingAllowed)
                        this[longFlagString] = true;

                    return null;
                }

                this[longFlagString] = true;
                i++;

                (string value, long factor) = ExtractFactorFromValue(parts[i]);
                return (sbyte)(sbyte.Parse(value) * factor);
            }
            else if (parts[i].StartsWith(shortFlagString + "=") || parts[i].StartsWith(longFlagString + "="))
            {
                if (!IsFlagSupported(longFlagString))
                    return null;

                string[] commandParts = parts[i].Split('=');
                if (commandParts.Length != 2)
                    return null;

                string valuePart = commandParts[1];

                (string value, long factor) = ExtractFactorFromValue(valuePart);
                return (sbyte)(sbyte.Parse(value) * factor);
            }

            return SByte.MinValue;
        }

        /// <summary>
        /// Process an Int16 parameter
        /// </summary>
        /// <param name="parts">List of parts to be referenced</param>
        /// <param name="flagString">Flag string, if available</param>
        /// <param name="i">Reference to the position in the parts</param>
        /// <param name="missingAllowed">True if missing values are allowed, false otherwise</param>
        /// <returns>Int16 value if success, Int16.MinValue if skipped, null on error/returns>
        protected short? ProcessInt16Parameter(List<string> parts, string flagString, ref int i, bool missingAllowed = false)
        {
            return ProcessInt16Parameter(parts, null, flagString, ref i, missingAllowed);
        }

        /// <summary>
        /// Process an Int16 parameter
        /// </summary>
        /// <param name="parts">List of parts to be referenced</param>
        /// <param name="shortFlagString">Short flag string, if available</param>
        /// <param name="longFlagString">Long flag string, if available</param>
        /// <param name="i">Reference to the position in the parts</param>
        /// <param name="missingAllowed">True if missing values are allowed, false otherwise</param>
        /// <returns>Int16 value if success, Int16.MinValue if skipped, null on error/returns>
        protected short? ProcessInt16Parameter(List<string> parts, string shortFlagString, string longFlagString, ref int i, bool missingAllowed = false)
        {
            if (parts == null)
                return null;

            if (parts[i] == shortFlagString || parts[i] == longFlagString)
            {
                if (!IsFlagSupported(longFlagString))
                {
                    return null;
                }
                else if (!DoesExist(parts, i + 1))
                {
                    if (missingAllowed)
                        this[longFlagString] = true;

                    return null;
                }
                else if (IsFlagSupported(parts[i + 1]))
                {
                    if (missingAllowed)
                        this[longFlagString] = true;

                    return null;
                }
                else if (!IsValidInt16(parts[i + 1]))
                {
                    if (missingAllowed)
                        this[longFlagString] = true;

                    return null;
                }

                this[longFlagString] = true;
                i++;
                (string value, long factor) = ExtractFactorFromValue(parts[i]);
                return (short)(short.Parse(value) * factor);
            }
            else if (parts[i].StartsWith(shortFlagString + "=") || parts[i].StartsWith(longFlagString + "="))
            {
                if (!IsFlagSupported(longFlagString))
                    return null;

                string[] commandParts = parts[i].Split('=');
                if (commandParts.Length != 2)
                    return null;

                string valuePart = commandParts[1];

                (string value, long factor) = ExtractFactorFromValue(valuePart);
                return (short)(short.Parse(value) * factor);
            }

            return Int16.MinValue;
        }

        /// <summary>
        /// Process an Int32 parameter
        /// </summary>
        /// <param name="parts">List of parts to be referenced</param>
        /// <param name="flagString">Flag string, if available</param>
        /// <param name="i">Reference to the position in the parts</param>
        /// <param name="missingAllowed">True if missing values are allowed, false otherwise</param>
        /// <returns>Int32 value if success, Int32.MinValue if skipped, null on error/returns>
        protected int? ProcessInt32Parameter(List<string> parts, string flagString, ref int i, bool missingAllowed = false)
        {
            return ProcessInt32Parameter(parts, null, flagString, ref i, missingAllowed);
        }

        /// <summary>
        /// Process an Int32 parameter
        /// </summary>
        /// <param name="parts">List of parts to be referenced</param>
        /// <param name="shortFlagString">Short flag string, if available</param>
        /// <param name="longFlagString">Long flag string, if available</param>
        /// <param name="i">Reference to the position in the parts</param>
        /// <param name="missingAllowed">True if missing values are allowed, false otherwise</param>
        /// <returns>Int32 value if success, Int32.MinValue if skipped, null on error/returns>
        protected int? ProcessInt32Parameter(List<string> parts, string shortFlagString, string longFlagString, ref int i, bool missingAllowed = false)
        {
            if (parts == null)
                return null;

            if (parts[i] == shortFlagString || parts[i] == longFlagString)
            {
                if (!IsFlagSupported(longFlagString))
                {
                    return null;
                }
                else if (!DoesExist(parts, i + 1))
                {
                    if (missingAllowed)
                        this[longFlagString] = true;

                    return null;
                }
                else if (IsFlagSupported(parts[i + 1]))
                {
                    if (missingAllowed)
                        this[longFlagString] = true;

                    return null;
                }
                else if (!IsValidInt32(parts[i + 1]))
                {
                    if (missingAllowed)
                        this[longFlagString] = true;

                    return null;
                }

                this[longFlagString] = true;
                i++;
                (string value, long factor) = ExtractFactorFromValue(parts[i]);
                return (int)(int.Parse(value) * factor);
            }
            else if (parts[i].StartsWith(shortFlagString + "=") || parts[i].StartsWith(longFlagString + "="))
            {
                if (!IsFlagSupported(longFlagString))
                    return null;

                string[] commandParts = parts[i].Split('=');
                if (commandParts.Length != 2)
                    return null;

                string valuePart = commandParts[1];

                (string value, long factor) = ExtractFactorFromValue(valuePart);
                return (int)(int.Parse(value) * factor);
            }

            return Int32.MinValue;
        }

        /// <summary>
        /// Process an Int64 parameter
        /// </summary>
        /// <param name="parts">List of parts to be referenced</param>
        /// <param name="flagString">Flag string, if available</param>
        /// <param name="i">Reference to the position in the parts</param>
        /// <param name="missingAllowed">True if missing values are allowed, false otherwise</param>
        /// <returns>Int64 value if success, Int64.MinValue if skipped, null on error/returns>
        protected long? ProcessInt64Parameter(List<string> parts, string flagString, ref int i, bool missingAllowed = false)
        {
            return ProcessInt64Parameter(parts, null, flagString, ref i, missingAllowed);
        }

        /// <summary>
        /// Process an Int64 parameter
        /// </summary>
        /// <param name="parts">List of parts to be referenced</param>
        /// <param name="shortFlagString">Short flag string, if available</param>
        /// <param name="longFlagString">Long flag string, if available</param>
        /// <param name="i">Reference to the position in the parts</param>
        /// <param name="missingAllowed">True if missing values are allowed, false otherwise</param>
        /// <returns>Int64 value if success, Int64.MinValue if skipped, null on error/returns>
        protected long? ProcessInt64Parameter(List<string> parts, string shortFlagString, string longFlagString, ref int i, bool missingAllowed = false)
        {
            if (parts == null)
                return null;

            if (parts[i] == shortFlagString || parts[i] == longFlagString)
            {
                if (!IsFlagSupported(longFlagString))
                {
                    return null;
                }
                else if (!DoesExist(parts, i + 1))
                {
                    if (missingAllowed)
                        this[longFlagString] = true;

                    return null;
                }
                else if (IsFlagSupported(parts[i + 1]))
                {
                    if (missingAllowed)
                        this[longFlagString] = true;

                    return null;
                }
                else if (!IsValidInt64(parts[i + 1]))
                {
                    if (missingAllowed)
                        this[longFlagString] = true;

                    return null;
                }

                this[longFlagString] = true;
                i++;
                (string value, long factor) = ExtractFactorFromValue(parts[i]);
                return long.Parse(value) * factor;
            }
            else if (parts[i].StartsWith(shortFlagString + "=") || parts[i].StartsWith(longFlagString + "="))
            {
                if (!IsFlagSupported(longFlagString))
                    return null;

                string[] commandParts = parts[i].Split('=');
                if (commandParts.Length != 2)
                    return null;

                string valuePart = commandParts[1];

                (string value, long factor) = ExtractFactorFromValue(valuePart);
                return long.Parse(value) * factor;
            }

            return Int64.MinValue;
        }

        /// <summary>
        /// Process an Int64 parameter
        /// </summary>
        /// <param name="parts">List of parts to be referenced</param>
        /// <param name="flagString">Flag string, if available</param>
        /// <param name="i">Reference to the position in the parts</param>
        /// <param name="missingAllowed">True if missing values are allowed, false otherwise</param>
        /// <returns>String value if possible, string.Empty on missing, null on error</returns>
        protected string ProcessStringParameter(List<string> parts, string flagString, ref int i, bool missingAllowed = false)
        {
            return ProcessStringParameter(parts, null, flagString, ref i, missingAllowed);
        }

        /// <summary>
        /// Process a string parameter
        /// </summary>
        /// <param name="parts">List of parts to be referenced</param>
        /// <param name="shortFlagString">Short flag string, if available</param>
        /// <param name="longFlagString">Long flag string, if available</param>
        /// <param name="i">Reference to the position in the parts</param>
        /// <param name="missingAllowed">True if missing values are allowed, false otherwise</param>
        /// <returns>String value if possible, string.Empty on missing, null on error</returns>
        protected string ProcessStringParameter(List<string> parts, string shortFlagString, string longFlagString, ref int i, bool missingAllowed = false)
        {
            if (parts == null)
                return null;

            if (parts[i] == shortFlagString || parts[i] == longFlagString)
            {
                if (!IsFlagSupported(longFlagString))
                {
                    return null;
                }
                else if (!DoesExist(parts, i + 1))
                {
                    if (missingAllowed)
                        this[longFlagString] = true;

                    return null;
                }
                else if (IsFlagSupported(parts[i + 1]))
                {
                    if (missingAllowed)
                        this[longFlagString] = true;

                    return null;
                }
                else if (string.IsNullOrWhiteSpace(parts[i + 1]))
                {
                    if (missingAllowed)
                        this[longFlagString] = true;

                    return null;
                }

                this[longFlagString] = true;
                i++;
                return parts[i].Trim('"');
            }
            else if (parts[i].StartsWith(shortFlagString + "=") || parts[i].StartsWith(longFlagString + "="))
            {
                if (!IsFlagSupported(longFlagString))
                    return null;

                string[] commandParts = parts[i].Split('=');
                if (commandParts.Length != 2)
                    return null;

                string valuePart = commandParts[1];

                this[longFlagString] = true;
                return valuePart.Trim('"');
            }

            return string.Empty;
        }

        /// <summary>
        /// Get yhe trimmed value and multiplication factor from a value
        /// </summary>
        /// <param name="value">String value to treat as suffixed number</param>
        /// <returns>Trimmed value and multiplication factor</returns>
        private static (string trimmed, long factor) ExtractFactorFromValue(string value)
        {
            value = value.Trim('"');
            long factor = 1;

            // Characters
            if (value.EndsWith("c", StringComparison.Ordinal))
            {
                factor = 1;
                value = value.TrimEnd('c');
            }

            // Words
            else if (value.EndsWith("w", StringComparison.Ordinal))
            {
                factor = 2;
                value = value.TrimEnd('w');
            }

            // Double Words
            else if (value.EndsWith("d", StringComparison.Ordinal))
            {
                factor = 4;
                value = value.TrimEnd('d');
            }

            // Quad Words
            else if (value.EndsWith("q", StringComparison.Ordinal))
            {
                factor = 8;
                value = value.TrimEnd('q');
            }

            // Kilobytes
            else if (value.EndsWith("k", StringComparison.Ordinal))
            {
                factor = 1024;
                value = value.TrimEnd('k');
            }

            // Megabytes
            else if (value.EndsWith("M", StringComparison.Ordinal))
            {
                factor = 1024 * 1024;
                value = value.TrimEnd('M');
            }

            // Gigabytes
            else if (value.EndsWith("G", StringComparison.Ordinal))
            {
                factor = 1024 * 1024 * 1024;
                value = value.TrimEnd('G');
            }

            return (value, factor);
        }

        #endregion

        #region Common Information Extraction

        /// <summary>
        /// Get hashes from an input file path
        /// </summary>
        /// <param name="filename">Path to the input file</param>
        /// <returns>True if hashing was successful, false otherwise</returns>
        protected static bool GetFileHashes(string filename, out long size, out string crc32, out string md5, out string sha1)
        {
            // Set all initial values
            size = -1; crc32 = null; md5 = null; sha1 = null;

            // If the file doesn't exist, we can't do anything
            if (!File.Exists(filename))
                return false;

            // Set the file size
            size = new FileInfo(filename).Length;

            // Open the input file
            var input = File.OpenRead(filename);

            try
            {
                // Get a list of hashers to run over the buffer
                List<Hasher> hashers = new List<Hasher>
                {
                    new Hasher(Hash.CRC),
                    new Hasher(Hash.MD5),
                    new Hasher(Hash.SHA1),
                    new Hasher(Hash.SHA256),
                    new Hasher(Hash.SHA384),
                    new Hasher(Hash.SHA512),
                };

                // Initialize the hashing helpers
                var loadBuffer = new ThreadLoadBuffer(input);
                int buffersize = 3 * 1024 * 1024;
                byte[] buffer0 = new byte[buffersize];
                byte[] buffer1 = new byte[buffersize];

                /*
                Please note that some of the following code is adapted from
                RomVault. This is a modified version of how RomVault does
                threaded hashing. As such, some of the terminology and code
                is the same, though variable names and comments may have
                been tweaked to better fit this code base.
                */

                // Pre load the first buffer
                long refsize = size;
                int next = refsize > buffersize ? buffersize : (int)refsize;
                input.Read(buffer0, 0, next);
                int current = next;
                refsize -= next;
                bool bufferSelect = true;

                while (current > 0)
                {
                    // Trigger the buffer load on the second buffer
                    next = refsize > buffersize ? buffersize : (int)refsize;
                    if (next > 0)
                        loadBuffer.Trigger(bufferSelect ? buffer1 : buffer0, next);

                    byte[] buffer = bufferSelect ? buffer0 : buffer1;

                    // Run hashes in parallel
                    Parallel.ForEach(hashers, h => h.Process(buffer, current));

                    // Wait for the load buffer worker, if needed
                    if (next > 0)
                        loadBuffer.Wait();

                    // Setup for the next hashing step
                    current = next;
                    refsize -= next;
                    bufferSelect = !bufferSelect;
                }

                // Finalize all hashing helpers
                loadBuffer.Finish();
                Parallel.ForEach(hashers, h => h.Terminate());

                // Get the results
                crc32 = hashers.First(h => h.HashType == Hash.CRC).GetHashString();
                md5 = hashers.First(h => h.HashType == Hash.MD5).GetHashString();
                sha1 = hashers.First(h => h.HashType == Hash.SHA1).GetHashString();
                //sha256 = hashers.First(h => h.HashType == Hash.SHA256).GetHashString();
                //sha384 = hashers.First(h => h.HashType == Hash.SHA384).GetHashString();
                //sha512 = hashers.First(h => h.HashType == Hash.SHA512).GetHashString();

                // Dispose of the hashers
                loadBuffer.Dispose();
                hashers.ForEach(h => h.Dispose());

                return true;
            }
            catch (IOException ex)
            {
                return false;
            }
            finally
            {
                input.Dispose();
            }

            return false;
        }

        /// <summary>
        /// Get the split values for ISO-based media
        /// </summary>
        /// <param name="hashData">String representing the combined hash data</param>
        /// <returns>True if extraction was successful, false otherwise</returns>
        protected static bool GetISOHashValues(string hashData, out long size, out string crc32, out string md5, out string sha1)
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
        /// Get the existance of an anti-modchip string from a PlayStation disc, if possible
        /// </summary>
        /// <param name="driveLetter">Drive letter to use to check</param>
        /// <returns>Anti-modchip existance if possible, false on error</returns>
        protected static bool GetPlayStationAntiModchipDetected(char? driveLetter)
        {
            // If there's no drive letter, we can't do this part
            if (driveLetter == null)
                return false;

            // If the folder no longer exists, we can't do this part
            string drivePath = driveLetter + ":\\";
            if (!Directory.Exists(drivePath))
                return false;

            // Scan through each file to check for the anti-modchip strings
            var antiModchip = new PSXAntiModchip();
            foreach (string path in Directory.EnumerateFiles(drivePath, "*", SearchOption.AllDirectories))
            {
                try
                {
                    byte[] fileContent = File.ReadAllBytes(path);
                    string protection = antiModchip.CheckContents(path, fileContent, includePosition: false);
                    if (!string.IsNullOrWhiteSpace(protection))
                        return true;
                }
                catch
                {
                    // No-op, we don't care what the error was
                }
            }

            return false;
        }

        /// <summary>
        /// Get the EXE date from a PlayStation disc, if possible
        /// </summary>
        /// <param name="driveLetter">Drive letter to use to check</param>
        /// <param name="serial">Internal disc serial, if possible</param>
        /// <param name="region">Output region, if possible</param>
        /// <param name="date">Output EXE date in "yyyy-mm-dd" format if possible, null on error</param>
        /// <returns></returns>
        protected static bool GetPlayStationExecutableInfo(char? driveLetter, out string serial, out Region? region, out string date)
        {
            serial = null; region = null; date = null;

            // If there's no drive letter, we can't do this part
            if (driveLetter == null)
                return false;

            // If the folder no longer exists, we can't do this part
            string drivePath = driveLetter + ":\\";
            if (!Directory.Exists(drivePath))
                return false;

            // Get the two paths that we will need to check
            string psxExePath = Path.Combine(drivePath, "PSX.EXE");
            string systemCnfPath = Path.Combine(drivePath, "SYSTEM.CNF");

            // Try both of the common paths that contain information
            string exeName = null;

            // Read the CNF file as an INI file
            var systemCnf = new IniFile(systemCnfPath);
            string bootValue = string.Empty;

            // PlayStation uses "BOOT" as the key
            if (systemCnf.ContainsKey("BOOT"))
                bootValue = systemCnf["BOOT"];

            // PlayStation 2 uses "BOOT2" as the key
            if (systemCnf.ContainsKey("BOOT2"))
                bootValue = systemCnf["BOOT2"];

            // If we had any boot value, parse it and get the executable name
            if (!string.IsNullOrEmpty(bootValue))
            {
                var match = Regex.Match(bootValue, @"cdrom.?:\\?(.*)");
                if (match != null && match.Groups.Count > 1)
                {
                    exeName = match.Groups[1].Value;
                    exeName = exeName.Split(';')[0];
                    serial = exeName.Replace('_', '-').Replace(".", string.Empty);
                }
            }

            // If the SYSTEM.CNF value can't be found, try PSX.EXE
            if (string.IsNullOrWhiteSpace(exeName) && File.Exists(psxExePath))
                exeName = "PSX.EXE";

            // If neither can be found, we return false
            if (string.IsNullOrWhiteSpace(exeName))
                return false;

            // Get the region, if possible
            region = GetPlayStationRegion(exeName);

            // Now that we have the EXE name, try to get the fileinfo for it
            string exePath = Path.Combine(drivePath, exeName);
            if (!File.Exists(exePath))
                return false;

            // Fix the Y2K timestamp issue
            FileInfo fi = new FileInfo(exePath);
            DateTime dt = new DateTime(fi.LastWriteTimeUtc.Year >= 1900 && fi.LastWriteTimeUtc.Year < 1920 ? 2000 + fi.LastWriteTimeUtc.Year % 100 : fi.LastWriteTimeUtc.Year,
                fi.LastWriteTimeUtc.Month, fi.LastWriteTimeUtc.Day);
            date = dt.ToString("yyyy-MM-dd");

            return true;
        }

        /// <summary>
        /// Get the version from a PlayStation 2 disc, if possible
        /// </summary>
        /// <param name="driveLetter">Drive letter to use to check</param>
        /// <returns>Game version if possible, null on error</returns>
        protected static string GetPlayStation2Version(char? driveLetter)
        {
            // If there's no drive letter, we can't do this part
            if (driveLetter == null)
                return null;

            // If the folder no longer exists, we can't do this part
            string drivePath = driveLetter + ":\\";
            if (!Directory.Exists(drivePath))
                return null;

            // Get the SYSTEM.CNF path to check
            string systemCnfPath = Path.Combine(drivePath, "SYSTEM.CNF");

            // Try to parse the SYSTEM.CNF file
            var systemCnf = new IniFile(systemCnfPath);
            if (systemCnf.ContainsKey("VER"))
                return systemCnf["VER"];

            // If "VER" can't be found, we can't do much
            return null;
        }

        /// <summary>
        /// Get the version from a PlayStation 4 disc, if possible
        /// </summary>
        /// <param name="driveLetter">Drive letter to use to check</param>
        /// <returns>Game version if possible, null on error</returns>
        protected static string GetPlayStation4Version(char? driveLetter)
        {
            // If there's no drive letter, we can't do this part
            if (driveLetter == null)
                return null;

            // If the folder no longer exists, we can't do this part
            string drivePath = driveLetter + ":\\";
            if (!Directory.Exists(drivePath))
                return null;

            // If we can't find param.sfo, we don't have a PlayStation 4 disc
            string paramSfoPath = Path.Combine(drivePath, "bd", "param.sfo");
            if (!File.Exists(paramSfoPath))
                return null;

            // Let's try reading param.sfo to find the version at the end of the file
            try
            {
                using (BinaryReader br = new BinaryReader(File.OpenRead(paramSfoPath)))
                {
                    br.BaseStream.Seek(-0x08, SeekOrigin.End);
                    return new string(br.ReadChars(5));
                }
            }
            catch
            {
                // We don't care what the error was
                return null;
            }
        }

        /// <summary>
        /// Get the version from a PlayStation 5 disc, if possible
        /// </summary>
        /// <param name="driveLetter">Drive letter to use to check</param>
        /// <returns>Game version if possible, null on error</returns>
        protected static string GetPlayStation5Version(char? driveLetter)
        {
            // If there's no drive letter, we can't do this part
            if (driveLetter == null)
                return null;

            // If the folder no longer exists, we can't do this part
            string drivePath = driveLetter + ":\\";
            if (!Directory.Exists(drivePath))
                return null;

            // If we can't find param.json, we don't have a PlayStation 5 disc
            string paramJsonPath = Path.Combine(drivePath, "bd", "param.json");
            if (!File.Exists(paramJsonPath))
                return null;

            // Let's try reading param.json to find the version in the unencrypted JSON
            try
            {
                using (BinaryReader br = new BinaryReader(File.OpenRead(paramJsonPath)))
                {
                    br.BaseStream.Seek(0x89E, SeekOrigin.Begin);
                    return new string(br.ReadChars(5));
                }
            }
            catch
            {
                // We don't care what the error was
                return null;
            }
        }

        #endregion

        #region Category Extraction

        /// <summary>
        /// Determine the category based on the UMDImageCreator string
        /// </summary>
        /// <param name="region">String representing the category</param>
        /// <returns>Category, if possible</returns>
        protected static DiscCategory? GetUMDCategory(string category)
        {
            switch (category)
            {
                case "GAME":
                    return DiscCategory.Games;
                case "VIDEO":
                    return DiscCategory.Video;
                case "AUDIO":
                    return DiscCategory.Audio;
                default:
                    return null;
            }
        }

        #endregion

        #region Region Extraction

        /// <summary>
        /// Determine the region based on the PlayStation serial code
        /// </summary>
        /// <param name="serial">PlayStation serial code</param>
        /// <returns>Region mapped from name, if possible</returns>
        protected static Region? GetPlayStationRegion(string serial)
        {
            // Standardized "S" serials
            if (serial.StartsWith("S"))
            {
                // string publisher = serial[0] + serial[1];
                // char secondRegion = serial[3];
                switch (serial[2])
                {
                    case 'A':
                        return Region.Asia;
                    case 'C':
                        return Region.China;
                    case 'E':
                        return Region.Europe;
                    case 'J':
                        return Region.JapanKorea;
                    case 'K':
                        return Region.Korea;
                    case 'P':
                        return Region.Japan;
                    case 'U':
                        return Region.USA;
                }
            }

            // Japan-only special serial
            else if (serial.StartsWith("PAPX"))
                return Region.Japan;

            // Region appears entirely random
            else if (serial.StartsWith("PABX"))
                return null;

            // Japan-only special serial
            else if (serial.StartsWith("PCBX"))
                return Region.Japan;

            // Single disc known, Japan
            else if (serial.StartsWith("PDBX"))
                return Region.Japan;

            // Single disc known, Europe
            else if (serial.StartsWith("PEBX"))
                return Region.Europe;

            return null;
        }

        /// <summary>
        /// Determine the region based on the XGD serial character
        /// </summary>
        /// <param name="region">Character denoting the region</param>
        /// <returns>Region, if possible</returns>
        protected static Region? GetXgdRegion(char region)
        {
            switch (region)
            {
                case 'W':
                    return Region.World;
                case 'A':
                    return Region.USA;
                case 'J':
                    return Region.JapanAsia;
                case 'E':
                    return Region.Europe;
                case 'K':
                    return Region.USAJapan;
                case 'L':
                    return Region.USAEurope;
                case 'H':
                    return Region.JapanEurope;
                default:
                    return null;
            }
        }

        #endregion
    }
}