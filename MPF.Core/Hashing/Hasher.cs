using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Hashing;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;
using MPF.Core.Data;

namespace MPF.Core.Hashing
{
    public sealed class Hasher : IDisposable
    {
        #region Properties

        /// <summary>
        /// Hash type associated with the current state
        /// </summary>
#if NET48
        public Hash HashType { get; private set; }
#else
        public Hash HashType { get; init; }
#endif

        /// <summary>
        /// Current hash in bytes
        /// </summary>
#if NET48
        public byte[] CurrentHashBytes
#else
        public byte[]? CurrentHashBytes
#endif
        {
            get
            {
#if NET48
                switch (_hasher)
                {
                    case HashAlgorithm ha:
                        return ha.Hash;
                    case NonCryptographicHashAlgorithm ncha:
                        return ncha.GetCurrentHash().Reverse().ToArray();
                    default:
                        return null;
                }
#else
                return (_hasher) switch
                {
                    HashAlgorithm ha => ha.Hash,
                    NonCryptographicHashAlgorithm ncha => ncha.GetCurrentHash().Reverse().ToArray(),
                    _ => null,
                };
#endif
            }
        }

        /// <summary>
        /// Current hash as a string
        /// </summary>
#if NET48
        public string CurrentHashString => ByteArrayToString(CurrentHashBytes);
#else
        public string? CurrentHashString => ByteArrayToString(CurrentHashBytes);
#endif

        #endregion

        #region Private Fields

        /// <summary>
        /// Internal hasher being used for processing
        /// </summary>
        /// <remarks>May be either a HashAlgorithm or NonCryptographicHashAlgorithm</remarks>
#if NET48
        private object _hasher;
#else
        private object? _hasher;
#endif

        #endregion

        #region Constructors

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="hashType">Hash type to instantiate</param>
        public Hasher(Hash hashType)
        {
            this.HashType = hashType;
            GetHasher();
        }

        /// <summary>
        /// Generate the correct hashing class based on the hash type
        /// </summary>
        private void GetHasher()
        {
#if NET48
            switch (HashType)
            {
                case Hash.CRC32:
                    _hasher = new Crc32();
                    break;
                case Hash.CRC64:
                    _hasher = new Crc64();
                    break;
                case Hash.MD5:
                    _hasher = MD5.Create();
                    break;
                case Hash.SHA1:
                    _hasher = SHA1.Create();
                    break;
                case Hash.SHA256:
                    _hasher = SHA256.Create();
                    break;
                case Hash.SHA384:
                    _hasher = SHA384.Create();
                    break;
                case Hash.SHA512:
                    _hasher = SHA512.Create();
                    break;
                case Hash.XxHash32:
                    _hasher = new XxHash32();
                    break;
                case Hash.XxHash64:
                    _hasher = new XxHash64();
                    break;
            }
#else
            _hasher = HashType switch
            {
                Hash.CRC32 => new Crc32(),
                Hash.CRC64 => new Crc64(),
                Hash.MD5 => MD5.Create(),
                Hash.SHA1 => SHA1.Create(),
                Hash.SHA256 => SHA256.Create(),
                Hash.SHA384 => SHA384.Create(),
                Hash.SHA512 => SHA512.Create(),
                Hash.XxHash32 => new XxHash32(),
                Hash.XxHash64 => new XxHash64(),
                _ => null,
            };
#endif
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            if (_hasher is IDisposable disposable)
                disposable.Dispose();
        }

        #endregion

        #region Static Hashing

        /// <summary>
        /// Get hashes from an input file path
        /// </summary>
        /// <param name="filename">Path to the input file</param>
        /// <returns>True if hashing was successful, false otherwise</returns>
#if NET48
        public static bool GetFileHashes(string filename, out long size, out string crc32, out string md5, out string sha1)
#else
        public static bool GetFileHashes(string filename, out long size, out string? crc32, out string? md5, out string? sha1)
#endif
        {
            // Set all initial values
            crc32 = null; md5 = null; sha1 = null;

            // Get all file hashes
            var fileHashes = GetFileHashes(filename, out size);
            if (fileHashes == null)
                return false;

            // Assign the file hashes and return
            crc32 = fileHashes[Hash.CRC32];
            md5 = fileHashes[Hash.MD5];
            sha1 = fileHashes[Hash.SHA1];
            return true;
        }

        /// <summary>
        /// Get hashes from an input file path
        /// </summary>
        /// <param name="filename">Path to the input file</param>
        /// <returns>Dictionary containing hashes on success, null on error</returns>
#if NET48
        public static Dictionary<Hash, string> GetFileHashes(string filename, out long size)
#else
        public static Dictionary<Hash, string?>? GetFileHashes(string filename, out long size)
#endif
        {
            // If the file doesn't exist, we can't do anything
            if (!File.Exists(filename))
            {
                size = -1;
                return null;
            }

            // Set the file size
            size = new FileInfo(filename).Length;

            // Create the output dictionary
#if NET48
            var hashDict = new Dictionary<Hash, string>();
#else
            var hashDict = new Dictionary<Hash, string?>();
#endif

            // Open the input file
            var input = File.OpenRead(filename);

            // Return the hashes from the stream
            return GetStreamHashes(input);
        }

        /// <summary>
        /// Get hashes from an input Stream
        /// </summary>
        /// <param name="input">Stream to hash</param>
        /// <returns>Dictionary containing hashes on success, null on error</returns>
#if NET48
        public static Dictionary<Hash, string> GetStreamHashes(Stream input)
#else
        public static Dictionary<Hash, string?>? GetStreamHashes(Stream input)
#endif
        {
            // Create the output dictionary
#if NET48
            var hashDict = new Dictionary<Hash, string>();
#else
            var hashDict = new Dictionary<Hash, string?>();
#endif

            try
            {
                // Get a list of hashers to run over the buffer
                var hashers = new Dictionary<Hash, Hasher>
                {
                    { Hash.CRC32, new Hasher(Hash.CRC32) },
                    { Hash.CRC64, new Hasher(Hash.CRC64) },
                    { Hash.MD5, new Hasher(Hash.MD5) },
                    { Hash.SHA1, new Hasher(Hash.SHA1) },
                    { Hash.SHA256, new Hasher(Hash.SHA256) },
                    { Hash.SHA384, new Hasher(Hash.SHA384) },
                    { Hash.SHA512, new Hasher(Hash.SHA512) },
                    { Hash.XxHash32, new Hasher(Hash.XxHash32) },
                    { Hash.XxHash64, new Hasher(Hash.XxHash64) },
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
                long refsize = input.Length;
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
                    Parallel.ForEach(hashers, h => h.Value.Process(buffer, current));

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
                Parallel.ForEach(hashers, h => h.Value.Terminate());

                // Get the results
                hashDict[Hash.CRC32] = hashers[Hash.CRC32].CurrentHashString;
                hashDict[Hash.CRC64] = hashers[Hash.CRC64].CurrentHashString;
                hashDict[Hash.MD5] = hashers[Hash.MD5].CurrentHashString;
                hashDict[Hash.SHA1] = hashers[Hash.SHA1].CurrentHashString;
                hashDict[Hash.SHA256] = hashers[Hash.SHA256].CurrentHashString;
                hashDict[Hash.SHA384] = hashers[Hash.SHA384].CurrentHashString;
                hashDict[Hash.SHA512] = hashers[Hash.SHA512].CurrentHashString;
                hashDict[Hash.XxHash32] = hashers[Hash.XxHash32].CurrentHashString;
                hashDict[Hash.XxHash64] = hashers[Hash.XxHash64].CurrentHashString;
                hashDict[Hash.CRC64] = hashers[Hash.CRC64].CurrentHashString;

                // Dispose of the hashers
                loadBuffer.Dispose();
                foreach (var hasher in hashers.Values)
                {
                    hasher.Dispose();
                }

                return hashDict;
            }
            catch (IOException)
            {
                return null;
            }
            finally
            {
                input.Dispose();
            }
        }

        #endregion

        #region Hashing

        /// <summary>
        /// Process a buffer of some length with the internal hash algorithm
        /// </summary>
        public void Process(byte[] buffer, int size)
        {
            switch (_hasher)
            {
                case HashAlgorithm ha:
                    ha.TransformBlock(buffer, 0, size, null, 0);
                    break;
                case NonCryptographicHashAlgorithm ncha:
                    var bufferSpan = new ReadOnlySpan<byte>(buffer, 0, size);
                    ncha.Append(bufferSpan);
                    break;
            }
        }

        /// <summary>
        /// Finalize the internal hash algorigthm
        /// </summary>
        /// <remarks>NonCryptographicHashAlgorithm implementations do not need finalization</remarks>
        public void Terminate()
        {
            byte[] emptyBuffer = Array.Empty<byte>();
            switch (_hasher)
            {
                case HashAlgorithm ha:
                    ha.TransformFinalBlock(emptyBuffer, 0, 0);
                    break;
            }
        }

        #endregion

        #region Helpers

        /// <summary>
        /// Convert a byte array to a hex string
        /// </summary>
        /// <param name="bytes">Byte array to convert</param>
        /// <returns>Hex string representing the byte array</returns>
        /// <link>http://stackoverflow.com/questions/311165/how-do-you-convert-byte-array-to-hexadecimal-string-and-vice-versa</link>
#if NET48
        private static string ByteArrayToString(byte[] bytes)
#else
        private static string? ByteArrayToString(byte[]? bytes)
#endif
        {
            // If we get null in, we send null out
            if (bytes == null)
                return null;

            try
            {
                string hex = BitConverter.ToString(bytes);
                return hex.Replace("-", string.Empty).ToLowerInvariant();
            }
            catch
            {
                return null;
            }
        }

        #endregion
    }
}
