using System;
using System.IO.Hashing;
using System.Linq;
using System.Security.Cryptography;
using MPF.Core.Data;

namespace MPF.Core.Hashing
{
    /// <summary>
    /// Async hashing class wraper
    /// </summary>
    public class Hasher : IDisposable
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
