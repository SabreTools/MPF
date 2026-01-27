using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Text.RegularExpressions;
using SabreTools.RedumpLib.Data;

namespace MPF.ExecutionContexts
{
    public abstract class BaseExecutionContext
    {
        #region Generic Dumping Information

        /// <summary>
        /// Base command to run
        /// </summary>
        public string? BaseCommand { get; set; }

        /// <summary>
        /// Set of flags to pass to the executable
        /// </summary>
        protected Dictionary<string, bool?> flags = [];
        protected internal List<string> Keys => [.. flags.Keys];

        /// <summary>
        /// Safe access to currently set flags
        /// </summary>
        public bool? this[string key]
        {
            get
            {
                if (flags.TryGetValue(key, out bool? val))
                    return val;

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
        private Process? process;

        #endregion

        #region Virtual Dumping Information

        /// <summary>
        /// Command to flag support mappings
        /// </summary>
        public Dictionary<string, List<string>>? CommandSupport => GetCommandSupport();

        /// <summary>
        /// Input path for operations
        /// </summary>
        public virtual string? InputPath => null;

        /// <summary>
        /// Output path for operations
        /// </summary>
        /// <returns>String representing the path, null on error</returns>
        public virtual string? OutputPath => null;

        /// <summary>
        /// Get the processing speed from the implementation
        /// </summary>
        public virtual int? Speed { get; set; } = null;

        #endregion

        #region Metadata

        /// <summary>
        /// Path to the executable
        /// </summary>
        public string? ExecutablePath { get; set; }

        /// <summary>
        /// Currently represented system
        /// </summary>
        public RedumpSystem? RedumpSystem { get; set; }

        /// <summary>
        /// Currently represented media type
        /// </summary>
        public MediaType? MediaType { get; set; }

        #endregion

        /// <summary>
        /// Populate a Parameters object from a param string
        /// </summary>
        /// <param name="parameters">String possibly representing a set of parameters</param>
        public BaseExecutionContext(string? parameters)
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
        /// <param name="drivePath">Drive path to use</param>
        /// <param name="filename">Filename to use</param>
        /// <param name="driveSpeed">Drive speed to use</param>
        /// <param name="options">Dictionary object containing all settings that may be used for setting parameters</param>
        public BaseExecutionContext(RedumpSystem? system,
            MediaType? type,
            string? drivePath,
            string filename,
            int? driveSpeed,
            Dictionary<string, string?> options)
        {
            RedumpSystem = system;
            MediaType = type;
            SetDefaultParameters(drivePath, filename, driveSpeed, options);
        }

        #region Abstract Methods

        /// <summary>
        /// Get all commands mapped to the supported flags
        /// </summary>
        /// <returns>Mappings from command to supported flags</returns>
        public abstract Dictionary<string, List<string>>? GetCommandSupport();

        /// <summary>
        /// Blindly generate a parameter string based on the inputs
        /// </summary>
        /// <returns>Parameter string for invocation, null on error</returns>
        public abstract string? GenerateParameters();

        /// <summary>
        /// Get the default extension for a given media type
        /// </summary>
        /// <param name="mediaType">MediaType value to check</param>
        /// <returns>String representing the media type, null on error</returns>
        public abstract string? GetDefaultExtension(MediaType? mediaType);

        /// <summary>
        /// Get the MediaType from the current set of parameters
        /// </summary>
        /// <returns>MediaType value if successful, null on error</returns>
        public abstract MediaType? GetMediaType();

        /// <summary>
        /// Gets if the current command is considered a dumping command or not
        /// </summary>
        /// <returns>True if it's a dumping command, false otherwise</returns>
        public abstract bool IsDumpingCommand();

        /// <summary>
        /// Returns if the current Parameter object is valid
        /// </summary>
        /// <returns></returns>
        public bool IsValid() => GenerateParameters() is not null;

        /// <summary>
        /// Reset all special variables to have default values
        /// </summary>
        protected abstract void ResetValues();

        /// <summary>
        /// Set default parameters for a given system and media type
        /// </summary>
        /// <param name="drivePath">Drive path to use</param>
        /// <param name="filename">Filename to use</param>
        /// <param name="driveSpeed">Drive speed to use</param>
        /// <param name="options">Dictionary containing all settings that may be used for setting parameters</param>
        protected abstract void SetDefaultParameters(string? drivePath,
            string filename,
            int? driveSpeed,
            Dictionary<string, string?> options);

        /// <summary>
        /// Scan a possible parameter string and populate whatever possible
        /// </summary>
        /// <param name="parameters">String possibly representing parameters</param>
        /// <returns>True if the parameters were set correctly, false otherwise</returns>
        protected abstract bool ValidateAndSetParameters(string? parameters);

        #endregion

        #region Execution

        /// <summary>
        /// Run internal program
        /// </summary>
        public void ExecuteInternalProgram()
        {
            // Create the start info
            var startInfo = new ProcessStartInfo()
            {
                FileName = ExecutablePath!,
                Arguments = GenerateParameters() ?? "",
                CreateNoWindow = false,
                UseShellExecute = true,
                RedirectStandardOutput = false,
                RedirectStandardError = false,
            };

            // Create the new process
            process = new Process() { StartInfo = startInfo };

            // Start the process
            process.Start();
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
                while (process is not null && !process.HasExited)
                {
                    process.Kill();
                }
            }
            catch
            { }
        }

        #endregion

        #region Option Processing

        /// <summary>
        /// Get a Boolean setting from a settings, dictionary
        /// </summary>
        /// <param name="settings">Dictionary representing the settings</param>
        /// <param name="key">Setting key to get a value for</param>
        /// <param name="defaultValue">Default value to return if no value is found</param>
        /// <returns>Setting value if possible, default value otherwise</returns>
        internal static bool GetBooleanSetting(Dictionary<string, string?> settings, string key, bool defaultValue)
        {
            if (settings.ContainsKey(key))
            {
                if (bool.TryParse(settings[key], out bool value))
                    return value;
                else
                    return defaultValue;
            }
            else
            {
                return defaultValue;
            }
        }

        /// <summary>
        /// Get an Int32 setting from a settings, dictionary
        /// </summary>
        /// <param name="settings">Dictionary representing the settings</param>
        /// <param name="key">Setting key to get a value for</param>
        /// <param name="defaultValue">Default value to return if no value is found</param>
        /// <returns>Setting value if possible, default value otherwise</returns>
        internal static int GetInt32Setting(Dictionary<string, string?> settings, string key, int defaultValue)
        {
            if (settings.ContainsKey(key))
            {
                if (int.TryParse(settings[key], out int value))
                    return value;
                else
                    return defaultValue;
            }
            else
            {
                return defaultValue;
            }
        }

        /// <summary>
        /// Get a String setting from a settings, dictionary
        /// </summary>
        /// <param name="settings">Dictionary representing the settings</param>
        /// <param name="key">Setting key to get a value for</param>
        /// <param name="defaultValue">Default value to return if no value is found</param>
        /// <returns>Setting value if possible, default value otherwise</returns>
        internal static string? GetStringSetting(Dictionary<string, string?> settings, string key, string? defaultValue)
        {
            if (settings.ContainsKey(key))
                return settings[key];
            else
                return defaultValue;
        }

        /// <summary>
        /// Get an UInt8 setting from a settings, dictionary
        /// </summary>
        /// <param name="settings">Dictionary representing the settings</param>
        /// <param name="key">Setting key to get a value for</param>
        /// <param name="defaultValue">Default value to return if no value is found</param>
        /// <returns>Setting value if possible, default value otherwise</returns>
        internal static byte GetUInt8Setting(Dictionary<string, string?> settings, string key, byte defaultValue)
        {
            if (settings.ContainsKey(key))
            {
                if (byte.TryParse(settings[key], out byte value))
                    return value;
                else
                    return defaultValue;
            }
            else
            {
                return defaultValue;
            }
        }

        #endregion

        #region Parameter Parsing

        /// <summary>
        /// Split a parameters string into a list while taking quotes into account
        /// </summary>
        internal static string[] SplitParameterString(string parameters)
        {
            // Ensure the parameter string is trimmed
            parameters = parameters.Trim();

            // Split the string using Regex
            var matches = Regex.Matches(parameters, @"([a-zA-Z0-9\-]*=)?[\""].+?[\""]|[^ ]+", RegexOptions.Compiled);

            // Get just the values from the matches
            var matchArr = new Match[matches.Count];
            matches.CopyTo(matchArr, 0);
            return Array.ConvertAll(matchArr, m => m.Value);
        }

        /// <summary>
        /// Returns whether or not the selected item exists
        /// </summary>
        /// <param name="parts">Parts array to be referenced</param>
        /// <param name="index">Current index</param>
        /// <returns>True if the next item exists, false otherwise</returns>
        internal static bool DoesExist(string[] parts, int index)
            => index >= 0 && index < parts.Length;

        /// <summary>
        /// Gets if the flag is supported by the current command
        /// </summary>
        /// <param name="flag">Flag value to check</param>
        /// <returns>True if the flag value is supported, false otherwise</returns>
        protected bool IsFlagSupported(string flag)
        {
            if (CommandSupport is null)
                return false;
            if (BaseCommand is null)
                return false;
            if (!CommandSupport.TryGetValue(BaseCommand, out var supported))
                return false;
            return supported.Contains(flag);
        }

        /// <summary>
        /// Returns whether a string is a valid bool
        /// </summary>
        /// <param name="parameter">String value to check</param>
        /// <returns>True if it's a valid bool, false otherwise</returns>
        internal static bool IsValidBool(string parameter)
            => bool.TryParse(parameter, out bool _);

        /// <summary>
        /// Returns whether a string is a valid byte
        /// </summary>
        /// <param name="parameter">String value to check</param>
        /// <param name="lowerBound">Lower bound (>=)</param>
        /// <param name="upperBound">Upper bound (<=)</param>
        /// <returns>True if it's a valid byte, false otherwise</returns>
        internal static bool IsValidInt8(string parameter, sbyte? lowerBound = null, sbyte? upperBound = null)
        {
            string value = ExtractFactorFromValue(parameter, out _);
            if (!sbyte.TryParse(value, out sbyte temp))
                return false;
            else if (lowerBound is not null && temp < lowerBound)
                return false;
            else if (upperBound is not null && temp > upperBound)
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
        internal static bool IsValidInt16(string parameter, short? lowerBound = null, short? upperBound = null)
        {
            string value = ExtractFactorFromValue(parameter, out _);
            if (!short.TryParse(value, out short temp))
                return false;
            else if (lowerBound is not null && temp < lowerBound)
                return false;
            else if (upperBound is not null && temp > upperBound)
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
        internal static bool IsValidInt32(string parameter, int? lowerBound = null, int? upperBound = null)
        {
            string value = ExtractFactorFromValue(parameter, out _);
            if (!int.TryParse(value, out int temp))
                return false;
            else if (lowerBound is not null && temp < lowerBound)
                return false;
            else if (upperBound is not null && temp > upperBound)
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
        internal static bool IsValidInt64(string parameter, long? lowerBound = null, long? upperBound = null)
        {
            string value = ExtractFactorFromValue(parameter, out _);
            if (!long.TryParse(value, out long temp))
                return false;
            else if (lowerBound is not null && temp < lowerBound)
                return false;
            else if (upperBound is not null && temp > upperBound)
                return false;

            return true;
        }

        /// <summary>
        /// Process a flag parameter
        /// </summary>
        /// <param name="parts">Parts array to be referenced</param>
        /// <param name="flagString">Flag string, if available</param>
        /// <param name="i">Reference to the position in the parts</param>
        /// <returns>True if the parameter was processed successfully or skipped, false otherwise</returns>
        protected bool ProcessFlagParameter(string[] parts, string flagString, ref int i)
            => ProcessFlagParameter(parts, null, flagString, ref i);

        /// <summary>
        /// Process a flag parameter
        /// </summary>
        /// <param name="parts">Parts array to be referenced</param>
        /// <param name="shortFlagString">Short flag string, if available</param>
        /// <param name="longFlagString">Long flag string, if available</param>
        /// <param name="i">Reference to the position in the parts</param>
        /// <returns>True if the parameter was processed successfully or skipped, false otherwise</returns>
        protected bool ProcessFlagParameter(string[] parts, string? shortFlagString, string longFlagString, ref int i)
        {
            if (parts is null)
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
        /// <param name="parts">Parts array to be referenced</param>
        /// <param name="flagString">Flag string, if available</param>
        /// <param name="i">Reference to the position in the parts</param>
        /// <param name="missingAllowed">True if missing values are allowed, false otherwise</param>
        /// <returns>True if the parameter was processed successfully or skipped, false otherwise</returns>
        protected bool ProcessBooleanParameter(string[] parts, string flagString, ref int i, bool missingAllowed = false)
            => ProcessBooleanParameter(parts, null, flagString, ref i, missingAllowed);

        /// <summary>
        /// Process a boolean parameter
        /// </summary>
        /// <param name="parts">Parts array to be referenced</param>
        /// <param name="shortFlagString">Short flag string, if available</param>
        /// <param name="longFlagString">Long flag string, if available</param>
        /// <param name="i">Reference to the position in the parts</param>
        /// <param name="missingAllowed">True if missing values are allowed, false otherwise</param>
        /// <returns>True if the parameter was processed successfully or skipped, false otherwise</returns>
        protected bool ProcessBooleanParameter(string[] parts, string? shortFlagString, string longFlagString, ref int i, bool missingAllowed = false)
        {
            if (parts is null)
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
        /// <param name="parts">Parts array to be referenced</param>
        /// <param name="flagString">Flag string, if available</param>
        /// <param name="i">Reference to the position in the parts</param>
        /// <param name="missingAllowed">True if missing values are allowed, false otherwise</param>
        /// <returns>SByte value if success, SByte.MinValue if skipped, null on error/returns>
        protected sbyte? ProcessInt8Parameter(string[] parts, string flagString, ref int i, bool missingAllowed = false)
            => ProcessInt8Parameter(parts, null, flagString, ref i, missingAllowed);

        /// <summary>
        /// Process an sbyte parameter
        /// </summary>
        /// <param name="parts">Parts array to be referenced</param>
        /// <param name="shortFlagString">Short flag string, if available</param>
        /// <param name="longFlagString">Long flag string, if available</param>
        /// <param name="i">Reference to the position in the parts</param>
        /// <param name="missingAllowed">True if missing values are allowed, false otherwise</param>
        /// <returns>SByte value if success, SByte.MinValue if skipped, null on error/returns>
        protected sbyte? ProcessInt8Parameter(string[] parts, string? shortFlagString, string longFlagString, ref int i, bool missingAllowed = false)
        {
            if (parts is null)
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

                string value = ExtractFactorFromValue(parts[i], out long factor);
                if (sbyte.TryParse(value, out sbyte sByteValue))
                    return (sbyte)(sByteValue * factor);
                string hexValue = RemoveHexIdentifier(value);
                if (sbyte.TryParse(hexValue, NumberStyles.AllowHexSpecifier, CultureInfo.InvariantCulture, out sbyte sByteHexValue))
                    return (sbyte)(sByteHexValue * factor);
                return null;
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
                string value = ExtractFactorFromValue(valuePart, out long factor);
                if (sbyte.TryParse(value, out sbyte sByteValue))
                    return (sbyte)(sByteValue * factor);
                string hexValue = RemoveHexIdentifier(value);
                if (sbyte.TryParse(hexValue, NumberStyles.AllowHexSpecifier, CultureInfo.InvariantCulture, out sbyte sByteHexValue))
                    return (sbyte)(sByteHexValue * factor);
                return null;
            }

            return sbyte.MinValue;
        }

        /// <summary>
        /// Process an Int16 parameter
        /// </summary>
        /// <param name="parts">Parts array to be referenced</param>
        /// <param name="flagString">Flag string, if available</param>
        /// <param name="i">Reference to the position in the parts</param>
        /// <param name="missingAllowed">True if missing values are allowed, false otherwise</param>
        /// <returns>Int16 value if success, Int16.MinValue if skipped, null on error/returns>
        protected short? ProcessInt16Parameter(string[] parts, string flagString, ref int i, bool missingAllowed = false)
            => ProcessInt16Parameter(parts, null, flagString, ref i, missingAllowed);

        /// <summary>
        /// Process an Int16 parameter
        /// </summary>
        /// <param name="parts">Parts array to be referenced</param>
        /// <param name="shortFlagString">Short flag string, if available</param>
        /// <param name="longFlagString">Long flag string, if available</param>
        /// <param name="i">Reference to the position in the parts</param>
        /// <param name="missingAllowed">True if missing values are allowed, false otherwise</param>
        /// <returns>Int16 value if success, Int16.MinValue if skipped, null on error/returns>
        protected short? ProcessInt16Parameter(string[] parts, string? shortFlagString, string longFlagString, ref int i, bool missingAllowed = false)
        {
            if (parts is null)
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
                string value = ExtractFactorFromValue(parts[i], out long factor);
                if (short.TryParse(value, out short shortValue))
                    return (short)(shortValue * factor);
                string hexValue = RemoveHexIdentifier(value);
                if (short.TryParse(hexValue, NumberStyles.AllowHexSpecifier, CultureInfo.InvariantCulture, out short shortHexValue))
                    return (short)(shortHexValue * factor);
                return null;
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
                string value = ExtractFactorFromValue(valuePart, out long factor);
                if (short.TryParse(value, out short shortValue))
                    return (short)(shortValue * factor);
                string hexValue = RemoveHexIdentifier(value);
                if (short.TryParse(hexValue, NumberStyles.AllowHexSpecifier, CultureInfo.InvariantCulture, out short shortHexValue))
                    return (short)(shortHexValue * factor);
                return null;
            }

            return short.MinValue;
        }

        /// <summary>
        /// Process an Int32 parameter
        /// </summary>
        /// <param name="parts">Parts array to be referenced</param>
        /// <param name="flagString">Flag string, if available</param>
        /// <param name="i">Reference to the position in the parts</param>
        /// <param name="missingAllowed">True if missing values are allowed, false otherwise</param>
        /// <returns>Int32 value if success, Int32.MinValue if skipped, null on error/returns>
        protected int? ProcessInt32Parameter(string[] parts, string flagString, ref int i, bool missingAllowed = false)
            => ProcessInt32Parameter(parts, null, flagString, ref i, missingAllowed);

        /// <summary>
        /// Process an Int32 parameter
        /// </summary>
        /// <param name="parts">Parts array to be referenced</param>
        /// <param name="shortFlagString">Short flag string, if available</param>
        /// <param name="longFlagString">Long flag string, if available</param>
        /// <param name="i">Reference to the position in the parts</param>
        /// <param name="missingAllowed">True if missing values are allowed, false otherwise</param>
        /// <returns>Int32 value if success, Int32.MinValue if skipped, null on error/returns>
        protected int? ProcessInt32Parameter(string[] parts, string? shortFlagString, string longFlagString, ref int i, bool missingAllowed = false)
        {
            if (parts is null)
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
                string value = ExtractFactorFromValue(parts[i], out long factor);
                if (int.TryParse(value, out int intValue))
                    return (int)(intValue * factor);
                string hexValue = RemoveHexIdentifier(value);
                if (int.TryParse(hexValue, NumberStyles.AllowHexSpecifier, CultureInfo.InvariantCulture, out int intHexValue))
                    return (int)(intHexValue * factor);
                return null;
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
                string value = ExtractFactorFromValue(valuePart, out long factor);
                if (int.TryParse(value, out int intValue))
                    return (int)(intValue * factor);
                string hexValue = RemoveHexIdentifier(value);
                if (int.TryParse(hexValue, NumberStyles.AllowHexSpecifier, CultureInfo.InvariantCulture, out int intHexValue))
                    return (int)(intHexValue * factor);
                return null;
            }

            return int.MinValue;
        }

        /// <summary>
        /// Process an Int64 parameter
        /// </summary>
        /// <param name="parts">Parts array to be referenced</param>
        /// <param name="flagString">Flag string, if available</param>
        /// <param name="i">Reference to the position in the parts</param>
        /// <param name="missingAllowed">True if missing values are allowed, false otherwise</param>
        /// <returns>Int64 value if success, Int64.MinValue if skipped, null on error/returns>
        protected long? ProcessInt64Parameter(string[] parts, string flagString, ref int i, bool missingAllowed = false)
            => ProcessInt64Parameter(parts, null, flagString, ref i, missingAllowed);

        /// <summary>
        /// Process an Int64 parameter
        /// </summary>
        /// <param name="parts">Parts array to be referenced</param>
        /// <param name="shortFlagString">Short flag string, if available</param>
        /// <param name="longFlagString">Long flag string, if available</param>
        /// <param name="i">Reference to the position in the parts</param>
        /// <param name="missingAllowed">True if missing values are allowed, false otherwise</param>
        /// <returns>Int64 value if success, Int64.MinValue if skipped, null on error/returns>
        protected long? ProcessInt64Parameter(string[] parts, string? shortFlagString, string longFlagString, ref int i, bool missingAllowed = false)
        {
            if (parts is null)
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
                string value = ExtractFactorFromValue(parts[i], out long factor);
                if (long.TryParse(value, out long longValue))
                    return longValue * factor;
                string hexValue = RemoveHexIdentifier(value);
                if (long.TryParse(hexValue, NumberStyles.AllowHexSpecifier, CultureInfo.InvariantCulture, out long longHexValue))
                    return longHexValue * factor;
                return null;
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
                string value = ExtractFactorFromValue(valuePart, out long factor);
                if (long.TryParse(value, out long longValue))
                    return longValue * factor;
                string hexValue = RemoveHexIdentifier(value);
                if (long.TryParse(hexValue, NumberStyles.AllowHexSpecifier, CultureInfo.InvariantCulture, out long longHexValue))
                    return longHexValue * factor;
                return null;
            }

            return long.MinValue;
        }

        /// <summary>
        /// Process an string parameter
        /// </summary>
        /// <param name="parts">Parts array to be referenced</param>
        /// <param name="flagString">Flag string, if available</param>
        /// <param name="i">Reference to the position in the parts</param>
        /// <param name="missingAllowed">True if missing values are allowed, false otherwise</param>
        /// <returns>String value if possible, string.Empty on missing, null on error</returns>
        protected string? ProcessStringParameter(string[] parts, string flagString, ref int i, bool missingAllowed = false)
            => ProcessStringParameter(parts, null, flagString, ref i, missingAllowed);

        /// <summary>
        /// Process a string parameter
        /// </summary>
        /// <param name="parts">Parts array to be referenced</param>
        /// <param name="shortFlagString">Short flag string, if available</param>
        /// <param name="longFlagString">Long flag string, if available</param>
        /// <param name="i">Reference to the position in the parts</param>
        /// <param name="missingAllowed">True if missing values are allowed, false otherwise</param>
        /// <returns>String value if possible, string.Empty on missing, null on error</returns>
        protected string? ProcessStringParameter(string[] parts, string? shortFlagString, string longFlagString, ref int i, bool missingAllowed = false)
        {
            if (parts is null)
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
                else if (string.IsNullOrEmpty(parts[i + 1]))
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

                int loc = parts[i].IndexOf('=');

                string valuePart = parts[i].Substring(loc + 1);

                this[longFlagString] = true;
                return valuePart.Trim('"');
            }

            return string.Empty;
        }

        /// <summary>
        /// Process a byte parameter
        /// </summary>
        /// <param name="parts">Parts array to be referenced</param>
        /// <param name="flagString">Flag string, if available</param>
        /// <param name="i">Reference to the position in the parts</param>
        /// <param name="missingAllowed">True if missing values are allowed, false otherwise</param>
        /// <returns>Byte value if success, Byte.MinValue if skipped, null on error/returns>
        protected byte? ProcessUInt8Parameter(string[] parts, string flagString, ref int i, bool missingAllowed = false)
            => ProcessUInt8Parameter(parts, null, flagString, ref i, missingAllowed);

        /// <summary>
        /// Process a byte parameter
        /// </summary>
        /// <param name="parts">Parts array to be referenced</param>
        /// <param name="shortFlagString">Short flag string, if available</param>
        /// <param name="longFlagString">Long flag string, if available</param>
        /// <param name="i">Reference to the position in the parts</param>
        /// <param name="missingAllowed">True if missing values are allowed, false otherwise</param>
        /// <returns>Byte value if success, Byte.MinValue if skipped, null on error/returns>
        protected byte? ProcessUInt8Parameter(string[] parts, string? shortFlagString, string longFlagString, ref int i, bool missingAllowed = false)
        {
            if (parts is null)
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

                string value = ExtractFactorFromValue(parts[i], out long factor);
                if (byte.TryParse(value, out byte byteValue))
                    return (byte)(byteValue * factor);
                string hexValue = RemoveHexIdentifier(value);
                if (byte.TryParse(hexValue, NumberStyles.AllowHexSpecifier, CultureInfo.InvariantCulture, out byte byteHexValue))
                    return (byte)(byteHexValue * factor);
                return null;
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
                string value = ExtractFactorFromValue(valuePart, out long factor);
                if (byte.TryParse(value, out byte byteValue))
                    return (byte)(byteValue * factor);
                string hexValue = RemoveHexIdentifier(value);
                if (byte.TryParse(hexValue, NumberStyles.AllowHexSpecifier, CultureInfo.InvariantCulture, out byte byteHexValue))
                    return (byte)(byteHexValue * factor);
                return null;
            }

            return byte.MinValue;
        }

        /// <summary>
        /// Get the trimmed value and multiplication factor from a value
        /// </summary>
        /// <param name="value">String value to treat as suffixed number</param>
        /// <returns>Trimmed value and multiplication factor</returns>
        internal static string ExtractFactorFromValue(string value, out long factor)
        {
            value = value.Trim('"');
            factor = 1;

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

            return value;
        }

        /// <summary>
        /// Removes a leading 0x if it exists, case insensitive
        /// </summary>
        /// <param name="value">String with removed leading 0x</param>
        /// <returns></returns>
        internal static string RemoveHexIdentifier(string value)
        {
            if (value.Length <= 2)
                return value;
            if (value[0] != '0')
                return value;
            if (value[1] != 'x' && value[1] != 'X')
                return value;

            return value.Substring(2);
        }

        #endregion
    }
}
