#if NETFRAMEWORK || NETCOREAPP3_1 || NET5_0

/*
 
 Copyright (c) 2012-2015 Eugene Larchenko (spct@mail.ru)
 Permission is hereby granted, free of charge, to any person obtaining a copy
 of this software and associated documentation files (the "Software"), to deal
 in the Software without restriction, including without limitation the rights
 to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
 copies of the Software, and to permit persons to whom the Software is
 furnished to do so, subject to the following conditions:
 The above copyright notice and this permission notice shall be included in
 all copies or substantial portions of the Software.
 THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
 AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
 LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
 OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
 THE SOFTWARE.
 
*/

using System;

//namespace OptimizedCRC
namespace MPF.Core.Hashing
{
    /// <summary>
    /// Shell class to trick older versions into using CRC-32 properly
    /// </summary>
    internal abstract class NonCryptographicHashAlgorithm
    {
#if NET20 || NET35 || NET40
        /// <summary>
        /// When overridden in a derived class, appends the contents of source to
        /// the data already processed for the current hash computation.
        /// </summary>
        /// <param name="source">The data to process.</param>
        public abstract void Append(byte[] source);
#else
        /// <summary>
        /// When overridden in a derived class, appends the contents of source to
        /// the data already processed for the current hash computation.
        /// </summary>
        /// <param name="source">The data to process.</param>
        public abstract void Append(ReadOnlySpan<byte> source);
#endif

        /// <summary>
        /// Gets the current computed hash value without modifying accumulated state.
        /// </summary>
        /// <returns>The hash value for the data already provided.</returns>
        public abstract byte[] GetCurrentHash();
    }

    /// <remarks>
    /// Some changes have been made to this code to make it more similar to the System.IO.Hashing implementations
    /// </remarks>
    internal class Crc32 : NonCryptographicHashAlgorithm, IDisposable
    {
        private const uint kCrcPoly = 0xEDB88320;
        private const uint kInitial = 0xFFFFFFFF;
        private const int CRC_NUM_TABLES = 8;
        private static readonly uint[] Table;

        static Crc32()
        {
            unchecked
            {
                Table = new uint[256 * CRC_NUM_TABLES];
                int i;
                for (i = 0; i < 256; i++)
                {
                    uint r = (uint)i;
                    for (int j = 0; j < 8; j++)
                    {
                        r = (r >> 1) ^ (kCrcPoly & ~((r & 1) - 1));
                    }
                    Table[i] = r;
                }
                for (; i < 256 * CRC_NUM_TABLES; i++)
                {
                    uint r = Table[i - 256];
                    Table[i] = Table[r & 0xFF] ^ (r >> 8);
                }
            }
        }

        public uint UnsignedValue;

        public Crc32()
        {
            Init();
        }

        /// <summary>
        /// Reset CRC
        /// </summary>
        public void Init()
        {
            UnsignedValue = kInitial;
        }

        /// <inheritdoc/>
        public override byte[] GetCurrentHash()
        {
            return BitConverter.GetBytes(~UnsignedValue);
        }

        /// <inheritdoc/>
#if NET20 || NET35 || NET40
        public override void Append(byte[] source)
        {
            Update(source, 0, source.Length);
        }
#else
        public override void Append(ReadOnlySpan<byte> source)
        {
            byte[] sourceBytes = source.ToArray();
            Update(sourceBytes, 0, sourceBytes.Length);
        }
#endif

        private void Update(byte[] data, int offset, int count)
        {
            _ = new ArraySegment<byte>(data, offset, count);     // check arguments
            if (count == 0)
            {
                return;
            }

            var table = Table;

            uint crc = UnsignedValue;

            for (; (offset & 7) != 0 && count != 0; count--)
            {
                crc = (crc >> 8) ^ table[(byte)crc ^ data[offset++]];
            }

            if (count >= 8)
            {
                /*
                 * Idea from 7-zip project sources (http://7-zip.org/sdk.html)
                 */

                int end = (count - 8) & ~7;
                count -= end;
                end += offset;

                while (offset != end)
                {
                    crc ^= (uint)(data[offset] + (data[offset + 1] << 8) + (data[offset + 2] << 16) + (data[offset + 3] << 24));
                    uint high = (uint)(data[offset + 4] + (data[offset + 5] << 8) + (data[offset + 6] << 16) + (data[offset + 7] << 24));
                    offset += 8;

                    crc = table[(byte)crc + 0x700]
                        ^ table[(byte)(crc >>= 8) + 0x600]
                        ^ table[(byte)(crc >>= 8) + 0x500]
                        ^ table[/*(byte)*/(crc >> 8) + 0x400]
                        ^ table[(byte)(high) + 0x300]
                        ^ table[(byte)(high >>= 8) + 0x200]
                        ^ table[(byte)(high >>= 8) + 0x100]
                        ^ table[/*(byte)*/(high >> 8) + 0x000];
                }
            }

            while (count-- != 0)
            {
                crc = (crc >> 8) ^ table[(byte)crc ^ data[offset++]];
            }

            UnsignedValue = crc;
        }

        public void Dispose()
        {
            UnsignedValue = 0;
        }
    }
}

#endif
