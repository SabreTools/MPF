using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml.Schema;
using System.Xml;
using System.Xml.Serialization;
using MPF.Core.Data;
using MPF.Core.Hashing;
using MPF.Core.Utilities;
using RedumpLib.Data;

namespace MPF.Modules
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
        public RedumpSystem? System { get; set; }

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
        /// <param name="system">RedumpSystem value to use</param>
        /// <param name="type">MediaType value to use</param>
        /// <param name="driveLetter">Drive letter to use</param>
        /// <param name="filename">Filename to use</param>
        /// <param name="driveSpeed">Drive speed to use</param>
        /// <param name="options">Options object containing all settings that may be used for setting parameters</param>
        public BaseParameters(RedumpSystem? system, MediaType? type, char driveLetter, string filename, int? driveSpeed, Options options)
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
        /// <param name="options">Options object representing user-defined options</param>
        /// <param name="basePath">Base filename and path to use for checking</param>
        /// <param name="drive">Drive representing the disc to get information from</param>
        /// <param name="includeArtifacts">True to include output files as encoded artifacts, false otherwise</param>
        public abstract void GenerateSubmissionInfo(SubmissionInfo submissionInfo, Options options, string basePath, Drive drive, bool includeArtifacts);

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
        protected virtual bool ValidateAndSetParameters(string parameters) => !string.IsNullOrWhiteSpace(parameters);

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
            => index < parameters.Count;

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

            return File.ReadAllText(filename);
        }

        /// <summary>
        /// Returns whether a string is a valid drive letter
        /// </summary>
        /// <param name="parameter">String value to check</param>
        /// <returns>True if it's a valid drive letter, false otherwise</W>
        protected static bool IsValidDriveLetter(string parameter)
            => Regex.IsMatch(parameter, @"^[A-Z]:?\\?$");

        /// <summary>
        /// Returns whether a string is a valid bool
        /// </summary>
        /// <param name="parameter">String value to check</param>
        /// <returns>True if it's a valid bool, false otherwise</returns>
        protected static bool IsValidBool(string parameter)
            => bool.TryParse(parameter, out bool _);

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
            => ProcessFlagParameter(parts, null, flagString, ref i);

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
            => ProcessBooleanParameter(parts, null, flagString, ref i, missingAllowed);

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
            => ProcessInt8Parameter(parts, null, flagString, ref i, missingAllowed);

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

                this[longFlagString] = true;
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
            => ProcessInt16Parameter(parts, null, flagString, ref i, missingAllowed);

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

                this[longFlagString] = true;
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
            => ProcessInt32Parameter(parts, null, flagString, ref i, missingAllowed);

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

                this[longFlagString] = true;
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
            => ProcessInt64Parameter(parts, null, flagString, ref i, missingAllowed);

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

                this[longFlagString] = true;
                (string value, long factor) = ExtractFactorFromValue(valuePart);
                return long.Parse(value) * factor;
            }

            return Int64.MinValue;
        }

        /// <summary>
        /// Process an string parameter
        /// </summary>
        /// <param name="parts">List of parts to be referenced</param>
        /// <param name="flagString">Flag string, if available</param>
        /// <param name="i">Reference to the position in the parts</param>
        /// <param name="missingAllowed">True if missing values are allowed, false otherwise</param>
        /// <returns>String value if possible, string.Empty on missing, null on error</returns>
        protected string ProcessStringParameter(List<string> parts, string flagString, ref int i, bool missingAllowed = false)
            => ProcessStringParameter(parts, null, flagString, ref i, missingAllowed);

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
        /// Process a byte parameter
        /// </summary>
        /// <param name="parts">List of parts to be referenced</param>
        /// <param name="flagString">Flag string, if available</param>
        /// <param name="i">Reference to the position in the parts</param>
        /// <param name="missingAllowed">True if missing values are allowed, false otherwise</param>
        /// <returns>Byte value if success, Byte.MinValue if skipped, null on error/returns>
        protected byte? ProcessUInt8Parameter(List<string> parts, string flagString, ref int i, bool missingAllowed = false)
            => ProcessUInt8Parameter(parts, null, flagString, ref i, missingAllowed);

        /// <summary>
        /// Process a byte parameter
        /// </summary>
        /// <param name="parts">List of parts to be referenced</param>
        /// <param name="shortFlagString">Short flag string, if available</param>
        /// <param name="longFlagString">Long flag string, if available</param>
        /// <param name="i">Reference to the position in the parts</param>
        /// <param name="missingAllowed">True if missing values are allowed, false otherwise</param>
        /// <returns>Byte value if success, Byte.MinValue if skipped, null on error/returns>
        protected byte? ProcessUInt8Parameter(List<string> parts, string shortFlagString, string longFlagString, ref int i, bool missingAllowed = false)
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
                return (byte)(byte.Parse(value) * factor);
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
                (string value, long factor) = ExtractFactorFromValue(valuePart);
                return (byte)(byte.Parse(value) * factor);
            }

            return Byte.MinValue;
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
        /// Generate the proper datfile from the input Datafile, if possible
        /// </summary>
        /// <param name="datafile">.dat file location</param>
        /// <returns>Relevant pieces of the datfile, null on error</returns>
        protected static string GenerateDatfile(Datafile datafile)
        {
            // If we don't have a valid datafile, we can't do anything
            if (datafile?.Games == null || datafile.Games.Length == 0 || datafile.Games[0]?.Roms == null || datafile.Games[0].Roms.Length == 0)
                return null;

            // Otherwise, reconstruct the hash data with only the required info
            try
            {
                var roms = datafile.Games[0].Roms;

                string datString = string.Empty;
                for (int i = 0; i < roms.Length; i++)
                {
                    var rom = roms[i];
                    datString += $"<rom name=\"{rom.Name}\" size=\"{rom.Size}\" crc=\"{rom.Crc}\" md5=\"{rom.Md5}\" sha1=\"{rom.Sha1}\" />\n";
                }

                datString.TrimEnd('\n');
                return datString;
            }
            catch
            {
                // We don't care what the exception is right now
                return null;
            }
        }

        /// <summary>
        /// Get Datafile from a standard DAT
        /// </summary>
        /// <param name="dat">Path to the DAT file to parse</param>
        /// <returns>Filled Datafile on success, null on error</returns>
        protected static Datafile GetDatafile(string dat)
        {
            // If there's no path, we can't read the file
            if (string.IsNullOrWhiteSpace(dat))
                return null;

            // If the file doesn't exist, we can't read it
            if (!File.Exists(dat))
                return null;

            try
            {
                // Open and read in the XML file
                XmlReader xtr = XmlReader.Create(dat, new XmlReaderSettings
                {
                    CheckCharacters = false,
                    DtdProcessing = DtdProcessing.Ignore,
                    IgnoreComments = true,
                    IgnoreWhitespace = true,
                    ValidationFlags = XmlSchemaValidationFlags.None,
                    ValidationType = ValidationType.None,
                });

                // If the reader is null for some reason, we can't do anything
                if (xtr == null)
                    return null;

                XmlSerializer serializer = new XmlSerializer(typeof(Datafile));
                Datafile obj = serializer.Deserialize(xtr) as Datafile;

                return obj;
            }
            catch
            {
                // We don't care what the exception is right now
                return null;
            }
        }

        /// <summary>
        /// Gets disc information from a PIC file
        /// </summary>
        /// <param name="pic">Path to a PIC.bin file</param>
        /// <returns>Filled PICDiscInformation on success, null on error</returns>
        /// <remarks>This omits the emergency brake information, if it exists</remarks>
        protected static PICDiscInformation GetDiscInformation(string pic)
        {
            try
            {
                using (BinaryReader br = new BinaryReader(File.OpenRead(pic)))
                {
                    var di = new PICDiscInformation();

                    // Read the initial disc information
                    di.DataStructureLength = br.ReadUInt16BigEndian();
                    di.Reserved0 = br.ReadByte();
                    di.Reserved1 = br.ReadByte();

                    // Create a list for the units
                    var diUnits = new List<PICDiscInformationUnit>();

                    // Loop and read all available units
                    for (int i = 0; i < 32; i++)
                    {
                        var unit = new PICDiscInformationUnit();

                        // We only accept Disc Information units, not Emergency Brake or other
                        unit.DiscInformationIdentifier = Encoding.ASCII.GetString(br.ReadBytes(2));
                        if (unit.DiscInformationIdentifier != "DI")
                            break;

                        unit.DiscInformationFormat = br.ReadByte();
                        unit.NumberOfUnitsInBlock = br.ReadByte();
                        unit.Reserved0 = br.ReadByte();
                        unit.SequenceNumber = br.ReadByte();
                        unit.BytesInUse = br.ReadByte();
                        unit.Reserved1 = br.ReadByte();

                        unit.DiscTypeIdentifier = Encoding.ASCII.GetString(br.ReadBytes(3));
                        unit.DiscSizeClassVersion = br.ReadByte();
                        switch (unit.DiscTypeIdentifier)
                        {
                            case PICDiscInformationUnit.DiscTypeIdentifierROM:
                            case PICDiscInformationUnit.DiscTypeIdentifierROMUltra:
                                unit.FormatDependentContents = br.ReadBytes(52);
                                break;
                            case PICDiscInformationUnit.DiscTypeIdentifierReWritable:
                            case PICDiscInformationUnit.DiscTypeIdentifierRecordable:
                                unit.FormatDependentContents = br.ReadBytes(100);
                                unit.DiscManufacturerID = br.ReadBytes(6);
                                unit.MediaTypeID = br.ReadBytes(3);
                                unit.TimeStamp = br.ReadUInt16();
                                unit.ProductRevisionNumber = br.ReadByte();
                                break;
                        }

                        diUnits.Add(unit);
                    }

                    // Assign the units and return
                    di.Units = diUnits.ToArray();
                    return di;
                }
            }
            catch
            {
                // We don't care what the error was
                return null;
            }
        }

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
        /// Get the last modified date from a file path, if possible
        /// </summary>
        /// <param name="filename">Path to the input file</param>
        /// <returns>Filled DateTime on success, null on failure</returns>
        protected static DateTime? GetFileModifiedDate(string filename, bool fallback = false)
        {
            if (string.IsNullOrWhiteSpace(filename))
                return fallback ? (DateTime?)DateTime.UtcNow : null;
            else if (!File.Exists(filename))
                return fallback ? (DateTime?)DateTime.UtcNow : null;

            var fi = new FileInfo(filename);
            return fi.LastWriteTimeUtc;
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

            // TODO: Use deserialization to Rom instead of Regex

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
        /// Get the split values for ISO-based media
        /// </summary>
        /// <param name="datafile">Datafile represenging the hash data</param>
        /// <returns>True if extraction was successful, false otherwise</returns>
        protected static bool GetISOHashValues(Datafile datafile, out long size, out string crc32, out string md5, out string sha1)
        {
            size = -1; crc32 = null; md5 = null; sha1 = null;

            if (datafile?.Games == null || datafile.Games.Length == 0 || datafile.Games[0].Roms.Length == 0)
                return false;

            var rom = datafile.Games[0].Roms[0];

            Int64.TryParse(rom.Size, out size);
            crc32 = rom.Crc;
            md5 = rom.Md5;
            sha1 = rom.Sha1;

            return true;
        }

        /// <summary>
        /// Get the layerbreak info associated from the disc information
        /// </summary>
        /// <param name="di">Disc information containing unformatted data</param>
        /// <returns>True if layerbreak info was set, false otherwise</returns>
        protected static bool GetLayerbreaks(PICDiscInformation di, out long? layerbreak1, out long? layerbreak2, out long? layerbreak3)
        {
            // Set the default values
            layerbreak1 = null; layerbreak2 = null; layerbreak3 = null;

            // If we don't have valid disc information, we can't do anything
            if (di?.Units == null || di.Units.Length <= 1)
                return false;

            int ReadFromArrayBigEndian(byte[] bytes, int offset)
            {
                var span = new ReadOnlySpan<byte>(bytes, offset, 0x04);
                byte[] rev = span.ToArray();
                Array.Reverse(rev);
                return BitConverter.ToInt32(rev, 0);
            }

            // Layerbreak 1 (2+ layers)
            if (di.Units.Length >= 2)
            {
                long offset = ReadFromArrayBigEndian(di.Units[0].FormatDependentContents, 0x0C);
                long value = ReadFromArrayBigEndian(di.Units[0].FormatDependentContents, 0x10);
                layerbreak1 = value - offset + 2;
            }

            // Layerbreak 2 (3+ layers)
            if (di.Units.Length >= 3)
            {
                long offset = ReadFromArrayBigEndian(di.Units[1].FormatDependentContents, 0x0C);
                long value = ReadFromArrayBigEndian(di.Units[1].FormatDependentContents, 0x10);
                layerbreak2 = layerbreak1 + value - offset + 2;
            }

            // Layerbreak 3 (4 layers)
            if (di.Units.Length >= 4)
            {
                long offset = ReadFromArrayBigEndian(di.Units[2].FormatDependentContents, 0x0C);
                long value = ReadFromArrayBigEndian(di.Units[2].FormatDependentContents, 0x10);
                layerbreak3 = layerbreak2 + value - offset + 2;
            }

            return true;
        }

        /// <summary>
        /// Get the PIC identifier from the first disc information unit, if possible
        /// </summary>
        /// <param name="di">Disc information containing the data</param>
        /// <returns>String representing the PIC identifier, null on error</returns>
        protected static string GetPICIdentifier(PICDiscInformation di)
        {
            // If we don't have valid disc information, we can't do anything
            if (di?.Units == null || di.Units.Length <= 1)
                return null;

            // We assume the identifier is consistent across all units
            return di.Units[0].DiscTypeIdentifier;
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
                if (match.Groups.Count > 1)
                {
                    // EXE name may have a trailing `;` after
                    // EXE name should always be in all caps
                    exeName = match.Groups[1].Value
                        .Split(';')[0]
                        .ToUpperInvariant();

                    // Serial is most of the EXE name normalized
                    serial = exeName
                        .Replace('_', '-')
                        .Replace(".", string.Empty);

                    // Some games may have the EXE in a subfolder
                    serial = Path.GetFileName(serial);
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
        /// Get the internal serial from a PlayStation 3 disc, if possible
        /// </summary>
        /// <param name="driveLetter">Drive letter to use to check</param>
        /// <returns>Internal disc serial if possible, null on error</returns>
        protected static string GetPlayStation3Serial(char? driveLetter)
        {
            // If there's no drive letter, we can't do this part
            if (driveLetter == null)
                return null;

            // If the folder no longer exists, we can't do this part
            string drivePath = driveLetter + ":\\";
            if (!Directory.Exists(drivePath))
                return null;

            // If we can't find PARAM.SFO, we don't have a PlayStation 3 disc
            string paramSfoPath = Path.Combine(drivePath, "PS3_GAME", "PARAM.SFO");
            if (!File.Exists(paramSfoPath))
                return null;

            // Let's try reading PARAM.SFO to find the serial at the end of the file
            try
            {
                using (BinaryReader br = new BinaryReader(File.OpenRead(paramSfoPath)))
                {
                    br.BaseStream.Seek(-0x18, SeekOrigin.End);
                    return new string(br.ReadChars(9));
                }
            }
            catch
            {
                // We don't care what the error was
                return null;
            }
        }

        /// <summary>
        /// Get the version from a PlayStation 3 disc, if possible
        /// </summary>
        /// <param name="driveLetter">Drive letter to use to check</param>
        /// <returns>Game version if possible, null on error</returns>
        protected static string GetPlayStation3Version(char? driveLetter)
        {
            // If there's no drive letter, we can't do this part
            if (driveLetter == null)
                return null;

            // If the folder no longer exists, we can't do this part
            string drivePath = driveLetter + ":\\";
            if (!Directory.Exists(drivePath))
                return null;

            // If we can't find PARAM.SFO, we don't have a PlayStation 3 disc
            string paramSfoPath = Path.Combine(drivePath, "PS3_GAME", "PARAM.SFO");
            if (!File.Exists(paramSfoPath))
                return null;

            // Let's try reading PARAM.SFO to find the version at the end of the file
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
        /// Get the internal serial from a PlayStation 4 disc, if possible
        /// </summary>
        /// <param name="driveLetter">Drive letter to use to check</param>
        /// <returns>Internal disc serial if possible, null on error</returns>
        protected static string GetPlayStation4Serial(char? driveLetter)
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

            // Let's try reading param.sfo to find the serial at the end of the file
            try
            {
                using (BinaryReader br = new BinaryReader(File.OpenRead(paramSfoPath)))
                {
                    br.BaseStream.Seek(-0x14, SeekOrigin.End);
                    return new string(br.ReadChars(9));
                }
            }
            catch
            {
                // We don't care what the error was
                return null;
            }
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
        /// Get the internal serial from a PlayStation 5 disc, if possible
        /// </summary>
        /// <param name="driveLetter">Drive letter to use to check</param>
        /// <returns>Internal disc serial if possible, null on error</returns>
        protected static string GetPlayStation5Serial(char? driveLetter)
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

            // Let's try reading param.json to find the serial in the unencrypted JSON
            try
            {
                using (BinaryReader br = new BinaryReader(File.OpenRead(paramJsonPath)))
                {
                    br.BaseStream.Seek(0x82E, SeekOrigin.Begin);
                    return new string(br.ReadChars(9));
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
                case "GAME": return DiscCategory.Games;
                case "VIDEO": return DiscCategory.Video;
                case "AUDIO": return DiscCategory.Audio;
                default: return null;
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
                    case 'A': return Region.Asia;
                    case 'C': return Region.China;
                    case 'E': return Region.Europe;
                    case 'K': return Region.SouthKorea;
                    case 'U': return Region.UnitedStatesOfAmerica;
                    case 'P': 
                        // Region of S_P_ serials may be Japan, Asia, or SouthKorea
                        switch (serial[3])
                        {
                            case 'S':
                                // Check first two digits of S_PS serial
                                switch (serial.Substring(5, 2))
                                {
                                    case "46": return Region.SouthKorea;
                                    case "56": return Region.SouthKorea;
                                    case "51": return Region.Asia;
                                    case "55": return Region.Asia;
                                    default: return Region.Japan;
                                }
                            case 'M':
                                // Check first three digits of S_PM serial
                                switch (serial.Substring(5, 3))
                                {
                                    case "645": return Region.SouthKorea;
                                    case "675": return Region.SouthKorea;
                                    case "885": return Region.SouthKorea;
                                    default: return Region.Japan; // Remaining S_PM serials may be Japan or Asia
                                }
                            default: return Region.Japan;
                        }
                }
            }

            // Japan-only special serial
            else if (serial.StartsWith("PAPX"))
                return Region.Japan;

            // Region appears entirely random
            else if (serial.StartsWith("PABX"))
                return null;

            // Region appears entirely random
            else if (serial.StartsWith("PBPX"))
                return null;

            // Japan-only special serial
            else if (serial.StartsWith("PCBX"))
                return Region.Japan;

            // Japan-only special serial
            else if (serial.StartsWith("PCXC"))
                return Region.Japan;

            // Single disc known, Japan
            else if (serial.StartsWith("PDBX"))
                return Region.Japan;

            // Single disc known, Europe
            else if (serial.StartsWith("PEBX"))
                return Region.Europe;

            // Single disc known, USA
            else if (serial.StartsWith("PUBX"))
                return Region.UnitedStatesOfAmerica;

            return null;
        }

        #endregion
    }
}
