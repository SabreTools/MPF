using System;
using System.Collections.Generic;
using System.IO;
#if NET6_0_OR_GREATER
using System.IO.Hashing;
#endif
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
#if NETFRAMEWORK || NETCOREAPP3_1
        public Hash HashType { get; private set; }
#else
        public Hash HashType { get; init; }
#endif

        /// <summary>
        /// Current hash in bytes
        /// </summary>
        public byte[]? CurrentHashBytes
        {
            get
            {
                return (_hasher) switch
                {
                    HashAlgorithm ha => ha.Hash,
                    NonCryptographicHashAlgorithm ncha => ncha.GetCurrentHash().Reverse().ToArray(),
                    _ => null,
                };
            }
        }

        /// <summary>
        /// Current hash as a string
        /// </summary>
        public string? CurrentHashString => ByteArrayToString(CurrentHashBytes);

        #endregion

        #region Private Fields

        /// <summary>
        /// Internal hasher being used for processing
        /// </summary>
        /// <remarks>May be either a HashAlgorithm or NonCryptographicHashAlgorithm</remarks>
        private object? _hasher;

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
            _hasher = HashType switch
            {
                Hash.CRC32 => new Crc32(),
#if NET6_0_OR_GREATER
                Hash.CRC64 => new Crc64(),
#endif
                Hash.MD5 => MD5.Create(),
                Hash.SHA1 => SHA1.Create(),
                Hash.SHA256 => SHA256.Create(),
                Hash.SHA384 => SHA384.Create(),
                Hash.SHA512 => SHA512.Create(),
#if NET6_0_OR_GREATER
                Hash.XxHash32 => new XxHash32(),
                Hash.XxHash64 => new XxHash64(),
#endif
                _ => null,
            };
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
        public static bool GetFileHashes(string filename, out long size, out string? crc32, out string? md5, out string? sha1)
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
        public static Dictionary<Hash, string?>? GetFileHashes(string filename, out long size)
        {
            // If the file doesn't exist, we can't do anything
            if (!File.Exists(filename))
            {
                size = -1;
                return null;
            }

            // Set the file size
            size = new FileInfo(filename).Length;

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
        public static Dictionary<Hash, string?>? GetStreamHashes(Stream input)
        {
            // Create the output dictionary
            var hashDict = new Dictionary<Hash, string?>();

            try
            {
                // Get a list of hashers to run over the buffer
                var hashers = new Dictionary<Hash, Hasher>
                {
                    { Hash.CRC32, new Hasher(Hash.CRC32) },
#if NET6_0_OR_GREATER
                    { Hash.CRC64, new Hasher(Hash.CRC64) },
#endif
                    { Hash.MD5, new Hasher(Hash.MD5) },
                    { Hash.SHA1, new Hasher(Hash.SHA1) },
                    { Hash.SHA256, new Hasher(Hash.SHA256) },
                    { Hash.SHA384, new Hasher(Hash.SHA384) },
                    { Hash.SHA512, new Hasher(Hash.SHA512) },
#if NET6_0_OR_GREATER
                    { Hash.XxHash32, new Hasher(Hash.XxHash32) },
                    { Hash.XxHash64, new Hasher(Hash.XxHash64) },
#endif
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
#if NET6_0_OR_GREATER
                hashDict[Hash.CRC64] = hashers[Hash.CRC64].CurrentHashString;
#endif
                hashDict[Hash.MD5] = hashers[Hash.MD5].CurrentHashString;
                hashDict[Hash.SHA1] = hashers[Hash.SHA1].CurrentHashString;
                hashDict[Hash.SHA256] = hashers[Hash.SHA256].CurrentHashString;
                hashDict[Hash.SHA384] = hashers[Hash.SHA384].CurrentHashString;
                hashDict[Hash.SHA512] = hashers[Hash.SHA512].CurrentHashString;
#if NET6_0_OR_GREATER
                hashDict[Hash.XxHash32] = hashers[Hash.XxHash32].CurrentHashString;
                hashDict[Hash.XxHash64] = hashers[Hash.XxHash64].CurrentHashString;
#endif

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
#if NET40
                    byte[] bufferSpan = new byte[size];
                    Array.Copy(buffer, bufferSpan, size);
#else
                    var bufferSpan = new ReadOnlySpan<byte>(buffer, 0, size);
#endif
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
            byte[] emptyBuffer = [];
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
        private static string? ByteArrayToString(byte[]? bytes)
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
