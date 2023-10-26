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
    /// <summary>
    /// Async hashing class wraper
    /// </summary>
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
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            if (_hasher is IDisposable disposable)
                disposable.Dispose();
        }

        #endregion

        #region Hashing

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
                var hashers = new List<Hasher>
                {
                    new Hasher(Hash.CRC32),
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
                crc32 = hashers.First(h => h.HashType == Hash.CRC32).CurrentHashString;
                //crc64 = hashers.First(h => h.HashType == Hash.CRC64).CurrentHashString;
                md5 = hashers.First(h => h.HashType == Hash.MD5).CurrentHashString;
                sha1 = hashers.First(h => h.HashType == Hash.SHA1).CurrentHashString;
                //sha256 = hashers.First(h => h.HashType == Hash.SHA256).CurrentHashString;
                //sha384 = hashers.First(h => h.HashType == Hash.SHA384).CurrentHashString;
                //sha512 = hashers.First(h => h.HashType == Hash.SHA512).CurrentHashString;
                //xxHash32 = hashers.First(h => h.HashType == Hash.XxHash32).CurrentHashString;
                //xxHash64 = hashers.First(h => h.HashType == Hash.XxHash64).CurrentHashString;

                // Dispose of the hashers
                loadBuffer.Dispose();
                hashers.ForEach(h => h.Dispose());

                return true;
            }
            catch (IOException)
            {
                return false;
            }
            finally
            {
                input.Dispose();
            }
        }

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
