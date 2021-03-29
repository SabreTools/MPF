using System;
using System.IO;

namespace MPF.Utilities
{
    /// <summary>
    /// Big endian reading overloads for BinaryReader
    /// </summary>
    internal static class BinaryReaderExtensions
    {
        /// <summary>
        /// Reads the specified number of bytes from the stream, starting from a specified point in the byte array.
        /// </summary>
        /// <param name="buffer">The buffer to read data into.</param>
        /// <param name="index">The starting point in the buffer at which to begin reading into the buffer.</param>
        /// <param name="count">The number of bytes to read.</param>
        /// <returns>The number of bytes read into buffer. This might be less than the number of bytes requested if that many bytes are not available, or it might be zero if the end of the stream is reached.</returns>
        public static int ReadBigEndian(this BinaryReader reader, byte[] buffer, int index, int count)
        {
            int retval = reader.Read(buffer, index, count);
            Array.Reverse(buffer);
            return retval;
        }

        /// <summary>
        /// Reads the specified number of characters from the stream, starting from a specified point in the character array.
        /// </summary>
        /// <param name="buffer">The buffer to read data into.</param>
        /// <param name="index">The starting point in the buffer at which to begin reading into the buffer.</param>
        /// <param name="count">The number of characters to read.</param>
        /// <returns>The total number of characters read into the buffer. This might be less than the number of characters requested if that many characters are not currently available, or it might be zero if the end of the stream is reached.</returns>
        public static int ReadBigEndian(this BinaryReader reader, char[] buffer, int index, int count)
        {
            int retval = reader.Read(buffer, index, count);
            Array.Reverse(buffer);
            return retval;
        }

        /// <summary>
        /// Reads the specified number of bytes from the current stream into a byte array and advances the current position by that number of bytes.
        /// </summary>
        /// <param name="count">The number of bytes to read. This value must be 0 or a non-negative number or an exception will occur.</param>
        /// <returns>A byte array containing data read from the underlying stream. This might be less than the number of bytes requested if the end of the stream is reached.</returns>
        public static byte[] ReadBytesBigEndian(this BinaryReader reader, int count)
        {
            byte[] retval = reader.ReadBytes(count);
            Array.Reverse(retval);
            return retval;
        }

        /// <summary>
        /// Reads the specified number of characters from the current stream, returns the data in a character array, and advances the current position in accordance with the Encoding used and the specific character being read from the stream.
        /// </summary>
        /// <param name="count">The number of characters to read. This value must be 0 or a non-negative number or an exception will occur.</param>
        /// <returns>A character array containing data read from the underlying stream. This might be less than the number of bytes requested if the end of the stream is reached.</returns>
        public static char[] ReadCharsBigEndian(this BinaryReader reader, int count)
        {
            char[] retval = reader.ReadChars(count);
            Array.Reverse(retval);
            return retval;
        }

        /// <summary>
        /// Reads a decimal value from the current stream and advances the current position of the stream by sixteen bytes.
        /// </summary>
        /// <returns>A decimal value read from the current stream.</returns>
        public static decimal ReadDecimalBigEndian(this BinaryReader reader)
        {
            byte[] retval = reader.ReadBytes(16);
            Array.Reverse(retval);

            int i1 = BitConverter.ToInt32(retval, 0);
            int i2 = BitConverter.ToInt32(retval, 4);
            int i3 = BitConverter.ToInt32(retval, 8);
            int i4 = BitConverter.ToInt32(retval, 12);

            return new decimal(new int[] { i1, i2, i3, i4 });
        }

        /// <summary>
        /// eads an 8-byte floating point value from the current stream and advances the current position of the stream by eight bytes.
        /// </summary>
        /// <returns>An 8-byte floating point value read from the current stream.</returns>
        public static double ReadDoubleBigEndian(this BinaryReader reader)
        {
            byte[] retval = reader.ReadBytes(8);
            Array.Reverse(retval);
            return BitConverter.ToDouble(retval, 0);
        }

        /// <summary>
        /// Reads a 2-byte signed integer from the current stream and advances the current position of the stream by two bytes.
        /// </summary>
        /// <returns>A 2-byte signed integer read from the current stream.</returns>
        public static short ReadInt16BigEndian(this BinaryReader reader)
        {
            byte[] retval = reader.ReadBytes(2);
            Array.Reverse(retval);
            return BitConverter.ToInt16(retval, 0);
        }

        /// <summary>
        /// Reads a 4-byte signed integer from the current stream and advances the current position of the stream by four bytes.
        /// </summary>
        /// <returns>A 4-byte signed integer read from the current stream.</returns>
        public static int ReadInt32BigEndian(this BinaryReader reader)
        {
            byte[] retval = reader.ReadBytes(4);
            Array.Reverse(retval);
            return BitConverter.ToInt32(retval, 0);
        }

        /// <summary>
        /// Reads an 8-byte signed integer from the current stream and advances the current position of the stream by eight bytes.
        /// </summary>
        /// <returns>An 8-byte signed integer read from the current stream.</returns>
        public static long ReadInt64BigEndian(this BinaryReader reader)
        {
            byte[] retval = reader.ReadBytes(8);
            Array.Reverse(retval);
            return BitConverter.ToInt64(retval, 0);
        }

        /// <summary>
        /// Reads a 4-byte floating point value from the current stream and advances the current position of the stream by four bytes.
        /// </summary>
        /// <returns>A 4-byte floating point value read from the current stream.</returns>
        public static float ReadSingleBigEndian(this BinaryReader reader)
        {
            byte[] retval = reader.ReadBytes(4);
            Array.Reverse(retval);
            return BitConverter.ToSingle(retval, 0);
        }

        /// <summary>
        /// Reads a 2-byte unsigned integer from the current stream using little-endian encoding and advances the position of the stream by two bytes.
        /// 
        /// This API is not CLS-compliant.
        /// </summary>
        /// <returns>A 2-byte unsigned integer read from this stream.</returns>
        public static ushort ReadUInt16BigEndian(this BinaryReader reader)
        {
            byte[] retval = reader.ReadBytes(2);
            Array.Reverse(retval);
            return BitConverter.ToUInt16(retval, 0);
        }

        /// <summary>
        /// Reads a 4-byte unsigned integer from the current stream and advances the position of the stream by four bytes.
        /// 
        /// This API is not CLS-compliant.
        /// </summary>
        /// <returns>A 4-byte unsigned integer read from this stream.</returns>
        public static uint ReadUInt32BigEndian(this BinaryReader reader)
        {
            byte[] retval = reader.ReadBytes(4);
            Array.Reverse(retval);
            return BitConverter.ToUInt32(retval, 0);
        }

        /// <summary>
        /// Reads an 8-byte unsigned integer from the current stream and advances the position of the stream by eight bytes.
        /// 
        /// This API is not CLS-compliant.
        /// </summary>
        /// <returns>An 8-byte unsigned integer read from this stream.</returns>
        public static ulong ReadUInt64BigEndian(this BinaryReader reader)
        {
            byte[] retval = reader.ReadBytes(8);
            Array.Reverse(retval);
            return BitConverter.ToUInt64(retval, 0);
        }
    }
}
