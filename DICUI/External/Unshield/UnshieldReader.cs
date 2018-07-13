using System;
using System.IO;

namespace DICUI.External.Unshield
{
    public class UnshieldReader
    {
        public UnshieldCabinet Unshield;
        public uint Index;
        public FileDescriptor FileDescriptor;
        public int Volume;
        public FileStream VolumeFile;
        public VolumeHeader VolumeHeader;
        public uint VolumeBytesLeft;
        public uint ObfuscationOffset;

        /// <summary>
        /// Create a new UnshieldReader from an existing cabinet, index, and file descriptor
        /// </summary>
        public static UnshieldReader Create(UnshieldCabinet unshield, int index, FileDescriptor fileDescriptor)
        {
            UnshieldReader reader = new UnshieldReader();
            if (reader == null)
                return null;

            reader.Unshield = unshield;
            reader.Index = (uint)index;
            reader.FileDescriptor = fileDescriptor;

            for (; ; )
            {
                if (!reader.OpenVolume(fileDescriptor.Volume))
                {
                    // unshield_error("Failed to open volume %i", file_descriptor->volume);
                    return null;
                }

                // Start with the correct volume for IS5 cabinets
                if (reader.Unshield.HeaderList.MajorVersion <= 5 &&
                    index > (int)reader.VolumeHeader.LastFileIndex)
                {
                    // unshield_trace("Trying next volume...");
                    fileDescriptor.Volume++;
                    continue;
                }

                break;
            }

            return reader;
        }

        /// <summary>
        /// Dispose of the current object
        /// </summary>
        public void Dispose()
        {
            VolumeFile?.Close();
        }

        /// <summary>
        /// Open the volume at the inputted index
        /// </summary>
        public bool OpenVolume(int volume)
        {
            bool success = false;
            uint dataOffset = 0;
            uint volumeBytesLeftCompressed;
            uint volumeBytesLeftExpanded;
            CommonHeader commonHeader = new CommonHeader();

            // unshield_trace("Open volume %i", volume);

            this.VolumeFile?.Close();

            this.VolumeFile = this.Unshield.OpenFileForReading(volume, Constants.CABINET_SUFFIX);
            if (this.VolumeFile == null)
            {
                // unshield_error("Failed to open input cabinet file %i", volume);
                return success;
            }

            {
                byte[] tmp = new byte[Constants.COMMON_HEADER_SIZE];
                int p = 0;

                if (Constants.COMMON_HEADER_SIZE !=
                    this.VolumeFile.Read(tmp, 0, Constants.COMMON_HEADER_SIZE))
                    return success;

                if (!CommonHeader.ReadCommonHeader(ref tmp, p, commonHeader))
                    return success;
            }

            this.VolumeHeader = new VolumeHeader();

            switch (this.Unshield.HeaderList.MajorVersion)
            {
                case 0:
                case 5:
                    {
                        byte[] fiveHeader = new byte[Constants.VOLUME_HEADER_SIZE_V5];
                        int p = 0;

                        if (Constants.VOLUME_HEADER_SIZE_V5 !=
                            this.VolumeFile.Read(fiveHeader, 0, Constants.VOLUME_HEADER_SIZE_V5))
                            return success;

                        this.VolumeHeader.DataOffset = BitConverter.ToUInt32(fiveHeader, p); p += 4;

                        /*
                        if (READ_UINT32(p))
                            unshield_trace("Unknown = %08x", READ_UINT32(p));
                        */

                        /* unknown */
                        p += 4;
                        this.VolumeHeader.FirstFileIndex = BitConverter.ToUInt32(fiveHeader, p); p += 4;
                        this.VolumeHeader.LastFileIndex = BitConverter.ToUInt32(fiveHeader, p); p += 4;
                        this.VolumeHeader.FirstFileOffset = BitConverter.ToUInt32(fiveHeader, p); p += 4;
                        this.VolumeHeader.FirstFileSizeExpanded = BitConverter.ToUInt32(fiveHeader, p); p += 4;
                        this.VolumeHeader.FirstFileSizeCompressed = BitConverter.ToUInt32(fiveHeader, p); p += 4;
                        this.VolumeHeader.LastFileOffset = BitConverter.ToUInt32(fiveHeader, p); p += 4;
                        this.VolumeHeader.LastFileSizeExpanded = BitConverter.ToUInt32(fiveHeader, p); p += 4;
                        this.VolumeHeader.LastFileSizeCompressed = BitConverter.ToUInt32(fiveHeader, p); p += 4;

                        if (this.VolumeHeader.LastFileOffset == 0)
                            this.VolumeHeader.LastFileOffset = Int32.MaxValue;
                    }
                    break;

                case 6:
                case 7:
                case 8:
                case 9:
                case 10:
                case 11:
                case 12:
                case 13:
                default:
                    {
                        byte[] sixHeader = new byte[Constants.VOLUME_HEADER_SIZE_V6];
                        int p = 0;

                        if (Constants.VOLUME_HEADER_SIZE_V6 !=
                            this.VolumeFile.Read(sixHeader, 0, Constants.VOLUME_HEADER_SIZE_V6))
                            return success;

                        this.VolumeHeader.DataOffset = BitConverter.ToUInt32(sixHeader, p); p += 4;
                        this.VolumeHeader.DataOffsetHigh = BitConverter.ToUInt32(sixHeader, p); p += 4;
                        this.VolumeHeader.FirstFileIndex = BitConverter.ToUInt32(sixHeader, p); p += 4;
                        this.VolumeHeader.LastFileIndex = BitConverter.ToUInt32(sixHeader, p); p += 4;
                        this.VolumeHeader.FirstFileOffset = BitConverter.ToUInt32(sixHeader, p); p += 4;
                        this.VolumeHeader.FirstFileOffsetHigh = BitConverter.ToUInt32(sixHeader, p); p += 4;
                        this.VolumeHeader.FirstFileSizeExpanded = BitConverter.ToUInt32(sixHeader, p); p += 4;
                        this.VolumeHeader.FirstFileSizeExpandedHigh = BitConverter.ToUInt32(sixHeader, p); p += 4;
                        this.VolumeHeader.FirstFileSizeCompressed = BitConverter.ToUInt32(sixHeader, p); p += 4;
                        this.VolumeHeader.FirstFileSizeCompressedHigh = BitConverter.ToUInt32(sixHeader, p); p += 4;
                        this.VolumeHeader.LastFileOffset = BitConverter.ToUInt32(sixHeader, p); p += 4;
                        this.VolumeHeader.LastFileOffsetHigh = BitConverter.ToUInt32(sixHeader, p); p += 4;
                        this.VolumeHeader.LastFileSizeExpanded = BitConverter.ToUInt32(sixHeader, p); p += 4;
                        this.VolumeHeader.LastFileSizeExpandedHigh = BitConverter.ToUInt32(sixHeader, p); p += 4;
                        this.VolumeHeader.LastFileSizeCompressed = BitConverter.ToUInt32(sixHeader, p); p += 4;
                        this.VolumeHeader.LastFileSizeCompressedHigh = BitConverter.ToUInt32(sixHeader, p); p += 4;
                    }
                    break;
            }

            /*
            unshield_trace("First file index = %i, last file index = %i",
                reader->volume_header.first_file_index, reader->volume_header.last_file_index);
            unshield_trace("First file offset = %08x, last file offset = %08x",
                reader->volume_header.first_file_offset, reader->volume_header.last_file_offset);
            */

            // enable support for split archives for IS5
            if (this.Unshield.HeaderList.MajorVersion == 5)
            {
                if (this.Index < (this.Unshield.HeaderList.Cab.FileCount - 1) &&
                    this.Index == this.VolumeHeader.LastFileIndex &&
                    this.VolumeHeader.LastFileSizeCompressed != this.FileDescriptor.CompressedSize)
                {
                    // unshield_trace("IS5 split file last in volume");
                    this.FileDescriptor.Flags |= Constants.FILE_SPLIT;
                }
                else if (this.Index > 0 &&
                    this.Index == this.VolumeHeader.FirstFileIndex &&
                    this.VolumeHeader.FirstFileSizeCompressed != this.FileDescriptor.CompressedSize)
                {
                    // unshield_trace("IS5 split file first in volume");
                    this.FileDescriptor.Flags |= Constants.FILE_SPLIT;
                }
            }

            if ((this.FileDescriptor.Flags & Constants.FILE_SPLIT) != 0)
            {
                // unshield_trace(/*"Total bytes left = 0x08%x, "*/"previous data offset = 0x08%x", /*total_bytes_left, */ data_offset);

                if (this.Index == this.VolumeHeader.LastFileIndex && this.VolumeHeader.LastFileOffset != 0x7FFFFFFF)
                {
                    // can be first file too
                    // unshield_trace("Index %i is last file in cabinet file %i", reader->index, volume);

                    dataOffset = this.VolumeHeader.LastFileOffset;
                    volumeBytesLeftExpanded = this.VolumeHeader.LastFileSizeExpanded;
                    volumeBytesLeftCompressed = this.VolumeHeader.LastFileSizeCompressed;
                }
                else if (this.Index == this.VolumeHeader.FirstFileIndex)
                {
                    // unshield_trace("Index %i is first file in cabinet file %i", reader->index, volume);

                    dataOffset = this.VolumeHeader.FirstFileOffset;
                    volumeBytesLeftExpanded = this.VolumeHeader.FirstFileSizeExpanded;
                    volumeBytesLeftCompressed = this.VolumeHeader.FirstFileSizeCompressed;
                }
                else
                {
                    success = true;
                    return success;
                }

                // unshield_trace("Will read 0x%08x bytes from offset 0x%08x", volume_bytes_left_compressed, data_offset);
            }
            else
            {
                dataOffset = this.FileDescriptor.DataOffset;
                volumeBytesLeftExpanded = this.FileDescriptor.ExpandedSize;
                volumeBytesLeftCompressed = this.FileDescriptor.CompressedSize;
            }

            if ((this.FileDescriptor.Flags & Constants.FILE_COMPRESSED) != 0)
                this.VolumeBytesLeft = volumeBytesLeftCompressed;
            else
                this.VolumeBytesLeft = volumeBytesLeftExpanded;

            this.VolumeFile.Seek(dataOffset, SeekOrigin.Begin);

            this.Volume = volume;
            success = true;

            return success;
        }

        /// <summary>
        /// Deobfuscate a buffer
        /// </summary>
        public void Deobfuscate(ref byte[] buffer, ref int bufferPointer, int size)
        {
            this.Deobfuscate(ref buffer, ref bufferPointer, size, ref this.ObfuscationOffset);
        }

        /// <summary>
        /// Read a certain number of bytes from the current volume
        /// </summary>
        public bool Read(ref byte[] buffer, ref int bufferPointer, int size)
        {
            bool success = false;
            int p = bufferPointer;
            int bytesLeft = size;

            // unshield_trace("unshield_reader_read start: bytes_left = 0x%x, volume_bytes_left = 0x%x", bytes_left, reader->volume_bytes_left);

            for (; ; )
            {
                // Read as much as possible from this volume
                int bytesToRead = (int)Math.Min(bytesLeft, this.VolumeBytesLeft);

                // unshield_trace("Trying to read 0x%x bytes from offset %08x in volume %i", bytes_to_read, ftell(reader->volume_file), reader->volume);
                if (bytesToRead == 0)
                {
                    // unshield_error("bytes_to_read can't be zero");
                    return success;
                }

                if (bytesToRead != this.VolumeFile.Read(buffer, p, bytesToRead))
                {
                    // unshield_error("Failed to read 0x%08x bytes of file %i (%s) from volume %i. Current offset = 0x%08x", bytes_to_read, reader->index, unshield_file_name(reader->unshield, reader->index), reader->volume, ftell(reader->volume_file));
                    return success;
                }

                bytesLeft -= bytesToRead;
                this.VolumeBytesLeft -= (uint)bytesToRead;

                // unshield_trace("bytes_left = %i, volume_bytes_left = %i", bytes_left, reader->volume_bytes_left);

                if (bytesLeft == 0)
                    break;

                p += bytesToRead;

                // Open next volume
                if (!this.OpenVolume(this.Volume + 1))
                {
                    // unshield_error("Failed to open volume %i to read %i more bytes", reader->volume + 1, bytes_to_read);
                    return success;
                }
            }

            if ((this.FileDescriptor.Flags & Constants.FILE_OBFUSCATED) != 0)
                this.Deobfuscate(ref buffer, ref bufferPointer, size);

            success = true;
            return success;
        }

        /// <summary>
        /// Deobfuscate a buffer with a seed value
        /// </summary>
        /// <remarks>Seed is 0 at file start</remarks>
        private void Deobfuscate(ref byte[] buffer, ref int bufferPointer, int size, ref uint seed)
        {
            uint tmpSeed = seed;

            for (; size > 0; size--, bufferPointer++, tmpSeed++)
            {
                buffer[bufferPointer] = (byte)(ROR8(buffer[bufferPointer] ^ 0xd5, 2) - (tmpSeed % 0x47));
            }

            seed = tmpSeed;
        }

        /// <summary>
        /// Rotate Right 8
        /// </summary>
        private int ROR8(int x, int n) { return (((x) >> ((int)(n))) | ((x) << (8 - (int)(n)))); }
    }
}
