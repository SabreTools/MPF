using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace DICUI.Utilities
{
    public static class IniParse
    {
        /// <summary>
        /// Read an INI file based on the path
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static Dictionary<string, string> ParseIniFile(string path)
        {
            // If we don't have a file, we can't read it
            if (!File.Exists(path))
                return new Dictionary<string, string>();

            using (var fileStream = File.OpenRead(path))
            {
                return ParseIniFile(fileStream);
            }
        }

        /// <summary>
        /// Read an INI file from a stream
        /// </summary>
        /// <param name="stream"></param>
        /// <returns></returns>
        public static Dictionary<string, string> ParseIniFile(Stream stream)
        {
            // If the stream is invalid or unreadable, we can't process it
            if (stream == null || !stream.CanRead || stream.Position >= stream.Length - 1)
                return new Dictionary<string, string>();

            // Keys are case-insensitive by default
            var keyValuePairs = new Dictionary<string, string>();
            try
            {
                using (StreamReader sr = new StreamReader(stream))
                {
                    string section = string.Empty;
                    while (!sr.EndOfStream)
                    {
                        string line = sr.ReadLine().Trim();

                        // Comments start with ';'
                        if (line.StartsWith(";"))
                        {
                            // No-op, we don't process comments
                        }

                        // Section titles are surrounded by square brackets
                        else if (line.StartsWith("["))
                        {
                            section = line.TrimStart('[').TrimEnd(']');
                        }

                        // Valid INI lines are in the format key=value
                        else if (line.Contains("="))
                        {
                            // Split the line by '=' for key-value pairs
                            string[] data = line.Split('=');

                            // If the value field contains an '=', we need to put them back in
                            string key = data[0].Trim();
                            string value = string.Join("=", data.Skip(1)).Trim();

                            // Section names are prepended to the key with a '.' separating
                            if (!string.IsNullOrEmpty(section))
                                key = $"{section}.{key}";

                            // Set or overwrite keys in the returned dictionary
                            keyValuePairs[key.ToLowerInvariant()] = value;
                        }

                        // All other lines are ignored
                    }
                }
            }
            catch
            {
                // We don't care what the error was, just catch and return
                return new Dictionary<string, string>();
            }

            return keyValuePairs;
        }
    }
}
