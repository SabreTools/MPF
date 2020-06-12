using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace DICUI.Utilities
{
    public class IniFile : IDictionary<string, string>
    {
        private Dictionary<string, string> _keyValuePairs = new Dictionary<string, string>();

        public string this[string key]
        {
            get
            {
                if (_keyValuePairs == null)
                    _keyValuePairs = new Dictionary<string, string>();

                key = key.ToLowerInvariant();
                if (_keyValuePairs.ContainsKey(key))
                    return _keyValuePairs[key];

                return null;
            }
            set
            {
                if (_keyValuePairs == null)
                    _keyValuePairs = new Dictionary<string, string>();

                key = key.ToLowerInvariant();
                _keyValuePairs[key] = value;
            }
        }

        /// <summary>
        /// Create an empty INI file
        /// </summary>
        public IniFile()
        {
        }

        /// <summary>
        /// Populate an INI file from path
        /// </summary>
        public IniFile(string path)
        {
            this.Parse(path);
        }

        /// <summary>
        /// Populate an INI file from stream
        /// </summary>
        public IniFile(Stream stream)
        {
            this.Parse(stream);
        }

        /// <summary>
        /// Add or update a key and value to the INI file
        /// </summary>
        public void AddOrUpdate(string key, string value)
        {
            _keyValuePairs[key.ToLowerInvariant()] = value;
        }

        /// <summary>
        /// Remove a key from the INI file
        /// </summary>
        public void Remove(string key)
        {
            _keyValuePairs.Remove(key.ToLowerInvariant());
        }

        /// <summary>
        /// Read an INI file based on the path
        /// </summary>
        public bool Parse(string path)
        {
            // If we don't have a file, we can't read it
            if (!File.Exists(path))
                return false;

            using (var fileStream = File.OpenRead(path))
            {
                return Parse(fileStream);
            }
        }

        /// <summary>
        /// Read an INI file from a stream
        /// </summary>
        public bool Parse(Stream stream)
        {
            // If the stream is invalid or unreadable, we can't process it
            if (stream == null || !stream.CanRead || stream.Position >= stream.Length - 1)
                return false;

            // Keys are case-insensitive by default
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
                            _keyValuePairs[key.ToLowerInvariant()] = value;
                        }

                        // All other lines are ignored
                    }
                }
            }
            catch
            {
                // We don't care what the error was, just catch and return
                return false;
            }

            return true;
        }

        /// <summary>
        /// Write an INI file to a path
        /// </summary>
        public bool Write(string path)
        {
            // If we don't have a valid dictionary with values, we can't write out
            if (_keyValuePairs == null || _keyValuePairs.Count == 0)
                return false;

            using (var fileStream = File.OpenWrite(path))
            {
                return Write(fileStream);
            }
        }

        /// <summary>
        /// Write an INI file to a stream
        /// </summary>
        public bool Write(Stream stream)
        {
            // If we don't have a valid dictionary with values, we can't write out
            if (_keyValuePairs == null || _keyValuePairs.Count == 0)
                return false;

            // If the stream is invalid or unwritable, we can't output to it
            if (stream == null || !stream.CanWrite || stream.Position >= stream.Length - 1)
                return false;

            try
            {
                using (StreamWriter sw = new StreamWriter(stream))
                {
                    // Order the dictionary by keys to link sections together
                    var orderedKeyValuePairs = _keyValuePairs.OrderBy(kvp => kvp.Key);

                    string section = string.Empty;
                    foreach (var keyValuePair in orderedKeyValuePairs)
                    {
                        // Extract the key and value
                        string key = keyValuePair.Key;
                        string value = keyValuePair.Value;

                        // We assume '.' is a section name separator
                        if (key.Contains('.'))
                        {
                            // Split the key by '.'
                            string[] data = keyValuePair.Key.Split('.');

                            // If the key contains an '.', we need to put them back in
                            string newSection = data[0].Trim();
                            key = string.Join(".", data.Skip(1)).Trim();

                            // If we have a new section, write it out
                            if (!string.Equals(newSection, section, StringComparison.OrdinalIgnoreCase))
                            {
                                sw.WriteLine($"[{newSection}]");
                                section = newSection;
                            }
                        }

                        // Now write out the key and value in a standardized way
                        sw.WriteLine($"{key}={value}");
                    }
                }
            }
            catch
            {
                // We don't care what the error was, just catch and return
                return false;
            }

            return true;
        }

        #region IDictionary Impelementations

        public ICollection<string> Keys => ((IDictionary<string, string>)_keyValuePairs).Keys;

        public ICollection<string> Values => ((IDictionary<string, string>)_keyValuePairs).Values;

        public int Count => ((ICollection<KeyValuePair<string, string>>)_keyValuePairs).Count;

        public bool IsReadOnly => ((ICollection<KeyValuePair<string, string>>)_keyValuePairs).IsReadOnly;

        public void Add(string key, string value)
        {
            ((IDictionary<string, string>)_keyValuePairs).Add(key.ToLowerInvariant(), value);
        }

        bool IDictionary<string, string>.Remove(string key)
        {
            return ((IDictionary<string, string>)_keyValuePairs).Remove(key.ToLowerInvariant());
        }

        public bool TryGetValue(string key, out string value)
        {
            return ((IDictionary<string, string>)_keyValuePairs).TryGetValue(key.ToLowerInvariant(), out value);
        }

        public void Add(KeyValuePair<string, string> item)
        {
            var newItem = new KeyValuePair<string, string>(item.Key.ToLowerInvariant(), item.Value);
            ((ICollection<KeyValuePair<string, string>>)_keyValuePairs).Add(newItem);
        }

        public void Clear()
        {
            ((ICollection<KeyValuePair<string, string>>)_keyValuePairs).Clear();
        }

        public bool Contains(KeyValuePair<string, string> item)
        {
            var newItem = new KeyValuePair<string, string>(item.Key.ToLowerInvariant(), item.Value);
            return ((ICollection<KeyValuePair<string, string>>)_keyValuePairs).Contains(newItem);
        }

        public bool ContainsKey(string key)
        {
            return _keyValuePairs.ContainsKey(key.ToLowerInvariant());
        }

        public void CopyTo(KeyValuePair<string, string>[] array, int arrayIndex)
        {
            ((ICollection<KeyValuePair<string, string>>)_keyValuePairs).CopyTo(array, arrayIndex);
        }

        public bool Remove(KeyValuePair<string, string> item)
        {
            var newItem = new KeyValuePair<string, string>(item.Key.ToLowerInvariant(), item.Value);
            return ((ICollection<KeyValuePair<string, string>>)_keyValuePairs).Remove(newItem);
        }

        public IEnumerator<KeyValuePair<string, string>> GetEnumerator()
        {
            return ((IEnumerable<KeyValuePair<string, string>>)_keyValuePairs).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable)_keyValuePairs).GetEnumerator();
        }

        #endregion
    }
}
