using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text.RegularExpressions;
using zlib;

namespace DICUI.External.Unshield
{
    public static class Constants
    {
        #region cabfile.h

        public const int OFFSET_COUNT = 0x47;
        public const int CAB_SIGNATURE = 0x28635349;

        public const int MSCF_SIGNATURE = 0x4643534d;

        public const int COMMON_HEADER_SIZE = 20;
        public const int VOLUME_HEADER_SIZE_V5 = 40;
        public const int VOLUME_HEADER_SIZE_V6 = 64;

        public const int MAX_FILE_GROUP_COUNT = 71;
        public const int MAX_COMPONENT_COUNT = 71;

        public const int FILE_SPLIT = 1;
        public const int FILE_OBFUSCATED = 2;
        public const int FILE_COMPRESSED = 4;
        public const int FILE_INVALID = 8;

        public const int LINK_NONE = 0;
        public const int LINK_PREV = 1;
        public const int LINK_NEXT = 2;
        public const int LINK_BOTH = 3;

        #endregion

        #region file.c

        public const int BUFFER_SIZE = 64 * 1024;

        #endregion

        #region internal.h

        public const string HEADER_SUFFIX = "hdr";
        public const string CABINET_SUFFIX = "cab";

        #endregion

        #region libunshield.h

        public const int UNSHIELD_LOG_LEVEL_LOWEST = 0;

        public const int UNSHIELD_LOG_LEVEL_ERROR = 1;
        public const int UNSHIELD_LOG_LEVEL_WARNING = 2;
        public const int UNSHIELD_LOG_LEVEL_TRACE = 3;

        public const int UNSHIELD_LOG_LEVEL_HIGHEST = 4;

        #endregion

        #region zconf.h

        public const int MAX_WBITS = 15;
        public const int Z_BLOCK = 5;

        #endregion
    }

    public class UnshieldCabinet
    {
        // Linked CAB headers
        public Header HeaderList { get; set; }

        // Internal CAB Counts
        public int ComponentCount { get { return this.HeaderList?.ComponentCount ?? 0; } }
        public int DirectoryCount { get { return (int)(this.HeaderList?.Cab?.DirectoryCount ?? 0); } } // XXX: multi-volume support...
        public int FileCount { get { return (int)(this.HeaderList?.Cab?.FileCount ?? 0); } } // XXX: multi-volume support...
        public int FileGroupCount { get { return this.HeaderList?.FileGroupCount ?? 0; } }

        // Unicode compatibility
        public bool IsUnicode { get { return (this.HeaderList == null ? false : this.HeaderList?.MajorVersion >= 17); } }

        // Base filename path for related CAB files
        private string filenamePattern;

        #region Open Cabinet

        /// <summary>
        /// Open a file as an InstallShield CAB
        /// </summary>
        public static UnshieldCabinet Open(string filename)
        {
            return OpenForceVersion(filename, -1);
        }

        /// <summary>
        /// Open a file as an InstallShield CAB, forcing a version
        /// </summary>
        public static UnshieldCabinet OpenForceVersion(string filename, int version)
        {
            UnshieldCabinet unshield = new UnshieldCabinet();
            if (!unshield.CreateFilenamePattern(filename))
            {
                // unshield_error("Failed to create filename pattern");
                return null;
            }

            if (!unshield.ReadHeaders(version))
            {
                // unshield_error("Failed to read header files");
                return null;
            }

            return unshield;
        }

        #endregion

        #region Name From Index

        /// <summary>
        /// Get the component name at an index
        /// </summary>
        public string ComponentName(int index)
        {
            if (index >= 0 && index < this.HeaderList.ComponentCount)
                return this.HeaderList.Components[index].Name;
            else
                return null;
        }

        /// <summary>
        /// Get the directory name at an index
        /// </summary>
        public string DirectoryName(int index)
        {
            if (index >= 0)
            {
                // XXX: multi-volume support...
                Header header = this.HeaderList;

                if (index < (int)header.Cab.DirectoryCount)
                    return header.GetUTF8String(header.Data,
                        (int)(header.Common.CabDescriptorOffset
                        + header.Cab.FileTableOffset
                        + header.FileTable[index]));
            }

            // unshield_warning("Failed to get directory name %i", index);
            return null;
        }

        /// <summary>
        /// Get the file name at an index
        /// </summary>
        public string FileName(int index)
        {
            FileDescriptor fd = this.GetFileDescriptor(index);

            if (fd != null)
            {
                // XXX: multi-volume support...
                Header header = this.HeaderList;

                return header.GetUTF8String(header.Data,
                    (int)(header.Common.CabDescriptorOffset
                    + header.Cab.FileTableOffset
                    + fd.NameOffset));
            }

            // unshield_warning("Failed to get file descriptor %i", index);
            return null;
        }

        /// <summary>
        /// Get the file group name at an index
        /// </summary>
        public string FileGroupName(int index)
        {
            Header header = this.HeaderList;

            if (index >= 0 && index < header.FileGroupCount)
                return header.FileGroups[index].Name;
            else
                return null;
        }

        #endregion

        #region File

        /// <summary>
        /// Returns if the file at a given index is marked as valid
        /// </summary>
        public bool FileIsValid(int index)
        {
            FileDescriptor fd;

            if (index < 0 || index > this.FileCount)
                return false;

            if ((fd = this.GetFileDescriptor(index)) == null)
                return false;

            if ((fd.Flags & Constants.FILE_INVALID) != 0)
                return false;

            if (fd.NameOffset == default(uint))
                return false;

            if (fd.DataOffset == default(uint))
                return false;

            return true;
        }

        /// <summary>
        /// Save the file at the given index to the filename specified
        /// </summary>
        public bool FileSave(int index, string filename)
        {
            FileStream output = null;
            byte[] inputBuffer = new byte[Constants.BUFFER_SIZE + 1];
            int inputBufferPointer = 0;
            byte[] outputBuffer = new byte[Constants.BUFFER_SIZE];
            int outputBufferPointer = 0;
            uint bytesLeft;
            ulong totalWritten = 0;
            UnshieldReader reader = null;
            FileDescriptor fileDescriptor;
            MD5 md5 = MD5.Create();

            md5.Initialize();

            if ((fileDescriptor = this.GetFileDescriptor(index)) == null)
            {
                // unshield_error("Failed to get file descriptor for file %i", index);
                return false;
            }

            if (((fileDescriptor.Flags & Constants.FILE_INVALID) != 0) || 0 == fileDescriptor.DataOffset)
            {
                // invalid file
                return false;
            }

            if ((fileDescriptor.LinkFlags & Constants.LINK_PREV) != 0)
            {
                return this.FileSave((int)fileDescriptor.LinkPrevious, filename);
            }

            reader = UnshieldReader.Create(this, index, fileDescriptor);
            if (reader == null)
            {
                // unshield_error("Failed to create data reader for file %i", index);
                reader?.Dispose();
                return false;
            }

            if (reader.VolumeFile.Length == (long)fileDescriptor.DataOffset)
            {
                // unshield_error("File %i is not inside the cabinet.", index);
                reader?.Dispose();
                return false;
            }

            if (!String.IsNullOrWhiteSpace(filename))
            {
                output = File.OpenWrite(filename);
                if (output == null)
                {
                    // unshield_error("Failed to open output file '%s'", filename);
                    reader?.Dispose();
                    output?.Close();
                    return false;
                }
            }

            if ((fileDescriptor.Flags & Constants.FILE_COMPRESSED) != 0)
                bytesLeft = fileDescriptor.CompressedSize;
            else
                bytesLeft = fileDescriptor.ExpandedSize;

            // unshield_trace("Bytes to read: %i", bytes_left);

            while (bytesLeft > 0)
            {
                ulong bytesToWrite = Constants.BUFFER_SIZE;
                int result;

                if ((fileDescriptor.Flags & Constants.FILE_COMPRESSED) != 0)
                {
                    ulong readBytes;
                    byte[] bytesToRead = new byte[sizeof(ushort)];
                    int bytesToReadPointer = 0;

                    if (!reader.Read(ref bytesToRead, ref bytesToReadPointer, bytesToRead.Length))
                    {
                        // unshield_error("Failed to read %i bytes of file %i (%s) from input cabinet file %i", izeof(bytes_to_read), index, unshield_file_name(unshield, index), file_descriptor->volume);
                        reader?.Dispose();
                        output?.Close();
                        return false;
                    }

                    // bytesToRead = letoh16(bytesToRead); // TODO: No-op?
                    if (BitConverter.ToUInt16(bytesToRead, 0) == 0)
                    {
                        // unshield_error("bytes_to_read can't be zero");
                        // unshield_error("HINT: Try unshield_file_save_old() or -O command line parameter!");
                        reader?.Dispose();
                        output?.Close();
                        return false;
                    }

                    if (!reader.Read(ref inputBuffer, ref inputBufferPointer, (int)BitConverter.ToUInt16(bytesToRead, 0)))
                    {
                        // unshield_error("Failed to read %i bytes of file %i (%s) from input cabinet file %i", ytes_to_read, index, unshield_file_name(unshield, index), file_descriptor->volume);
                        reader?.Dispose();
                        output?.Close();
                        return false;
                    }

                    // add a null byte to make inflate happy
                    inputBuffer[BitConverter.ToUInt16(bytesToRead, 0)] = 0;
                    readBytes = (ulong)(BitConverter.ToUInt16(bytesToRead, 0) + 1);
                    result = Uncompress(ref outputBuffer, ref bytesToWrite, ref inputBuffer, ref readBytes);

                    if (result != zlibConst.Z_OK)
                    {
                        //  unshield_error("Decompression failed with code %i. bytes_to_read=%i, volume_bytes_left=%i, volume=%i, read_bytes=%i", result, bytes_to_read, reader->volume_bytes_left, file_descriptor->volume, read_bytes)
                        if (result == zlibConst.Z_DATA_ERROR)
                        {
                            // unshield_error("HINT: Try unshield_file_save_old() or -O command line parameter!");
                        }
                        reader?.Dispose();
                        output?.Close();
                        return false;
                    }

                    // unshield_trace("read_bytes = %i", read_bytes);

                    bytesLeft -= 2;
                    bytesLeft -= BitConverter.ToUInt16(bytesToRead, 0);
                }
                else
                {
                    bytesToWrite = Math.Min(bytesLeft, Constants.BUFFER_SIZE);

                    if (!reader.Read(ref outputBuffer, ref outputBufferPointer, (int)bytesToWrite))
                    {
                        // unshield_error("Failed to read %i bytes from input cabinet file %i", bytes_to_write, file_descriptor->volume);
                        reader?.Dispose();
                        output?.Close();
                        return false;
                    }

                    bytesLeft -= (uint)bytesToWrite;
                }

                md5.TransformBlock(outputBuffer, 0, (int)bytesToWrite, outputBuffer, 0);

                if (output != null)
                {
                    output.Write(outputBuffer, 0, (int)bytesToWrite);
                }

                totalWritten += bytesToWrite;
            }

            if (fileDescriptor.ExpandedSize != totalWritten)
            {
                // unshield_error("Expanded size expected to be %i, but was %i", file_descriptor->expanded_size, total_written);
                reader?.Dispose();
                output?.Close();
                return false;
            }

            if (this.HeaderList.MajorVersion >= 6)
            {
                md5.TransformFinalBlock(outputBuffer, 0, 0);
                byte[] md5result = new byte[16];
                md5result = md5.Hash;

                if (!md5result.SequenceEqual(fileDescriptor.Md5))
                {
                    // unshield_error("MD5 checksum failure for file %i (%s)", index, unshield_file_name(unshield, index));
                    reader?.Dispose();
                    output?.Close();
                    return false;
                }
            }

            reader?.Dispose();
            output?.Close();
            return true;
        }

        /// <summary>
        /// Save the file at the given index to the filename specified (old version)
        /// </summary>
        public bool FileSaveOld(int index, string filename)
        {
            // XXX: Thou Shalt Not Cut & Paste
            FileStream output = null;
            long inputBufferSize = Constants.BUFFER_SIZE;
            byte[] inputBuffer = new byte[Constants.BUFFER_SIZE];
            int inputBufferPointer = 0;
            byte[] outputBuffer = new byte[Constants.BUFFER_SIZE];
            int outputBufferPointer = 0;
            uint bytesLeft;
            ulong totalWritten = 0;
            UnshieldReader reader = null;
            FileDescriptor fileDescriptor;

            if ((fileDescriptor = this.GetFileDescriptor(index)) == null)
            {
                // unshield_error("Failed to get file descriptor for file %i", index);
                reader?.Dispose();
                output.Close();
                return false;
            }

            if (((fileDescriptor.Flags & Constants.FILE_INVALID) != 0) || fileDescriptor.DataOffset == 0)
            {
                // invalid file
                reader?.Dispose();
                output.Close();
                return false;
            }

            if ((fileDescriptor.LinkFlags & Constants.LINK_PREV) != 0)
            {
                reader?.Dispose();
                output.Close();
                return FileSaveRaw((int)fileDescriptor.LinkPrevious, filename);
            }

            reader = UnshieldReader.Create(this, index, fileDescriptor);
            if (reader == null)
            {
                // unshield_error("Failed to create data reader for file %i", index);
                reader?.Dispose();
                output.Close();
                return false;
            }

            if (reader.VolumeFile.Length == (long)(fileDescriptor.DataOffset))
            {
                // unshield_error("File %i is not inside the cabinet.", index);
                reader?.Dispose();
                output.Close();
                return false;
            }

            if (!String.IsNullOrWhiteSpace(filename))
            {
                output = File.OpenWrite(filename);
                if (output == null)
                {
                    // unshield_error("Failed to open output file '%s'", filename);
                    reader?.Dispose();
                    output.Close();
                    return false;
                }
            }

            if ((fileDescriptor.Flags & Constants.FILE_COMPRESSED) != 0)
                bytesLeft = fileDescriptor.CompressedSize;
            else
                bytesLeft = fileDescriptor.ExpandedSize;

            // unshield_trace("Bytes to read: %i", bytes_left);

            while (bytesLeft > 0)
            {
                ulong bytesToWrite = 0;
                int result;

                if (reader.VolumeBytesLeft == 0 && !reader.OpenVolume(reader.Volume + 1))
                {
                    // unshield_error("Failed to open volume %i to read %i more bytes",  reader->volume + 1, bytes_left);
                    reader?.Dispose();
                    output.Close();
                    return false;
                }

                if ((fileDescriptor.Flags & Constants.FILE_COMPRESSED) != 0)
                {
                    byte[] END_OF_CHUNK = { 0x00, 0x00, 0xff, 0xff };
                    int eocPointer = 0;
                    ulong readBytes;
                    long inputSize = reader.VolumeBytesLeft;
                    byte[] chunkBuffer;
                    int chunkBufferPointer;

                    while (inputSize > inputBufferSize)
                    {
                        inputBufferSize *= 2;
                        // unshield_trace("increased input_buffer_size to 0x%x", input_buffer_size);

                        Array.Resize(ref inputBuffer, (int)inputBufferSize);
                        // assert(input_buffer)
                    }

                    if (!reader.Read(ref inputBuffer, ref inputBufferPointer, (int)inputSize))
                    {
                        // unshield_error("Failed to read 0x%x bytes of file %i (%s) from input cabinet file %i", input_size, index, unshield_file_name(unshield, index), file_descriptor->volume);
                        reader?.Dispose();
                        output.Close();
                        return false;
                    }

                    bytesLeft -= (uint)inputSize;

                    chunkBuffer = inputBuffer;
                    for (chunkBufferPointer = inputBufferPointer; inputSize > 0;)
                    {
                        long chunkSize;
                        int match = FindBytes(ref chunkBuffer, ref chunkBufferPointer, inputSize, ref END_OF_CHUNK, ref eocPointer, END_OF_CHUNK.Length);
                        if (match == -1)
                        {
                            // unshield_error("Could not find end of chunk for file %i (%s) from input cabinet file %i", index, unshield_file_name(unshield, index), file_descriptor->volume);
                            reader?.Dispose();
                            output.Close();
                            return false;
                        }

                        chunkSize = match - chunkBufferPointer;

                        /*
                        Detect when the chunk actually contains the end of chunk marker.

                        Needed by Qtime.smk from "The Feeble Files - spanish version".
           
                        The first bit of a compressed block is always zero, so we apply this
                        workaround if it's a one.

                        A possibly more proper fix for this would be to have
                        unshield_uncompress_old eat compressed data and discard chunk
                        markers inbetween.
                        */
                        while ((chunkSize + END_OF_CHUNK.Length) < inputSize &&
                           (chunkBuffer[chunkSize + END_OF_CHUNK.Length] & 1) != 0)
                        {
                            // unshield_warning("It seems like we have an end of chunk marker inside of a chunk.");
                            chunkSize += END_OF_CHUNK.Length;
                            int tempChunkPointer = (int)(chunkBufferPointer + chunkSize);
                            match = FindBytes(ref chunkBuffer, ref tempChunkPointer, inputSize - chunkSize, ref END_OF_CHUNK, ref eocPointer, END_OF_CHUNK.Length);
                            if (match == -1)
                            {
                                // unshield_error("Could not find end of chunk for file %i (%s) from input cabinet file %i", index, unshield_file_name(unshield, index), file_descriptor->volume);
                                reader?.Dispose();
                                output.Close();
                                return false;
                            }
                            chunkSize = match - chunkBufferPointer;
                        }

                        // unshield_trace("chunk_size = 0x%x", chunk_size);

                        // add a null byte to make inflate happy
                        chunkBuffer[chunkSize] = 0;

                        bytesToWrite = Constants.BUFFER_SIZE;
                        readBytes = (ulong)chunkSize;
                        result = UncompressOld(ref outputBuffer, ref bytesToWrite, ref chunkBuffer, ref readBytes);

                        if (result != zlibConst.Z_OK)
                        {
                            // unshield_error("Decompression failed with code %i. input_size=%i, volume_bytes_left=%i, volume=%i, read_bytes=%i", result, input_size, reader->volume_bytes_left, file_descriptor->volume, read_bytes);
                            reader?.Dispose();
                            output.Close();
                            return false;
                        }

                        // unshield_trace("read_bytes = 0x%x", read_bytes);

                        chunkBufferPointer += (int)chunkSize;
                        chunkBufferPointer += END_OF_CHUNK.Length;

                        inputSize -= chunkSize;
                        inputSize -= END_OF_CHUNK.Length;

                        if (output != null)
                        {
                            output.Write(outputBuffer, 0, (int)bytesToWrite);
                        }

                        totalWritten += bytesToWrite;
                    }
                }
                else
                {
                    bytesToWrite = Math.Min(bytesLeft, Constants.BUFFER_SIZE);

                    if (!reader.Read(ref outputBuffer, ref outputBufferPointer, (int)bytesToWrite))
                    {
                        // unshield_error("Failed to read %i bytes from input cabinet file %i", bytes_to_write, file_descriptor->volume);
                        reader?.Dispose();
                        output.Close();
                        return false;
                    }

                    bytesLeft -= (uint)bytesToWrite;

                    if (output != null)
                    {
                        output.Write(outputBuffer, 0, (int)bytesToWrite);
                    }

                    totalWritten += bytesToWrite;
                }
            }

            if (fileDescriptor.ExpandedSize != totalWritten)
            {
                // unshield_error("Expanded size expected to be %i, but was %i", file_descriptor->expanded_size, total_written);
                reader?.Dispose();
                output.Close();
                return false;
            }

            reader?.Dispose();
            output.Close();
            return true;
        }

        /// <summary>
        /// Save the file at the given index to the filename specified as raw
        /// </summary>
        public bool FileSaveRaw(int index, string filename)
        {
            // XXX: Thou Shalt Not Cut & Paste
            FileStream output = null;
            byte[] inputBuffer = new byte[Constants.BUFFER_SIZE];
            byte[] outputBuffer = new byte[Constants.BUFFER_SIZE];
            int outputBufferPointer = 0;
            uint bytesLeft;
            UnshieldReader reader = null;
            FileDescriptor fileDescriptor;

            if ((fileDescriptor = this.GetFileDescriptor(index)) == null)
            {
                // unshield_error("Failed to get file descriptor for file %i", index);
                reader?.Dispose();
                output.Close();
                return false;
            }

            if (((fileDescriptor.Flags & Constants.FILE_INVALID) != 0) || fileDescriptor.DataOffset == 0)
            {
                // invalid file
                reader?.Dispose();
                output.Close();
                return false;
            }

            if ((fileDescriptor.LinkFlags & Constants.LINK_PREV) != 0)
            {
                reader?.Dispose();
                output.Close();
                return FileSaveRaw((int)fileDescriptor.LinkPrevious, filename);
            }

            reader = UnshieldReader.Create(this, index, fileDescriptor);
            if (reader == null)
            {
                // unshield_error("Failed to create data reader for file %i", index);
                reader?.Dispose();
                output.Close();
                return false;
            }

            if (reader.VolumeFile.Length == (long)(fileDescriptor.DataOffset))
            {
                // unshield_error("File %i is not inside the cabinet.", index);
                reader?.Dispose();
                output.Close();
                return false;
            }

            if (!String.IsNullOrWhiteSpace(filename))
            {
                output = File.OpenWrite(filename);
                if (output == null)
                {
                    // unshield_error("Failed to open output file '%s'", filename);
                    reader?.Dispose();
                    output.Close();
                    return false;
                }
            }

            if ((fileDescriptor.Flags & Constants.FILE_COMPRESSED) != 0)
                bytesLeft = fileDescriptor.CompressedSize;
            else
                bytesLeft = fileDescriptor.ExpandedSize;

            // unshield_trace("Bytes to read: %i", bytes_left);

            while (bytesLeft > 0)
            {
                ulong bytesToWrite = Math.Min(bytesLeft, Constants.BUFFER_SIZE);

                if (!reader.Read(ref outputBuffer, ref outputBufferPointer, (int)bytesToWrite))
                {
                    // unshield_error("Failed to read %i bytes from input cabinet file %i", bytes_to_write, file_descriptor->volume);
                    reader?.Dispose();
                    output.Close();
                    return false;
                }

                bytesLeft -= (uint)bytesToWrite;

                output.Write(outputBuffer, 0, (int)bytesToWrite);
            }

            reader?.Dispose();
            output.Close();
            return true;
        }

        /// <summary>
        /// Get the directory index for the given file index
        /// </summary>
        public int FileDirectory(int index)
        {
            FileDescriptor fd = this.GetFileDescriptor(index);
            if (fd != null)
                return (int)fd.DirectoryIndex;
            else
                return -1;
        }

        /// <summary>
        /// Get the reported expanded file size for a given index
        /// </summary>
        public int FileSize(int index)
        {
            FileDescriptor fd = this.GetFileDescriptor(index);
            if (fd != null)
                return (int)fd.ExpandedSize;
            else
                return 0;
        }

        #endregion

        #region File Group

        /// <summary>
        /// Retrieve a file group based on index
        /// </summary>
        public UnshieldFileGroup FileGroupGet(int index)
        {
            Header header = this.HeaderList;

            if (index >= 0 && index < header.FileGroupCount)
                return header.FileGroups[index];
            else
                return null;
        }

        /// <summary>
        /// Retrieve a file group based on name
        /// </summary>
        public UnshieldFileGroup FileGroupFind(string name)
        {
            Header header = this.HeaderList;

            for (int i = 0; i < header.FileGroupCount; i++)
            {
                if (header.FileGroups[i].Name == name)
                    return header.FileGroups[i];
            }

            return null;
        }

        #endregion

        #region Uncompression

        /// <summary>
        /// Uncompress a source byte array to a destination
        /// </summary>
        public static int Uncompress(ref byte[] dest, ref ulong destLen, ref byte[] source, ref ulong sourceLen)
        {
            ZStream stream = new ZStream();
            int err;

            stream.next_in = source;
            stream.avail_in = (int)sourceLen;

            stream.next_out = dest;
            stream.avail_out = (int)destLen;

            //stream.zalloc = (alloc_func)0;
            //stream.zfree = (free_func)0;

            // make second parameter negative to disable checksum verification
            err = stream.inflateInit(-Constants.MAX_WBITS);
            if (err != zlibConst.Z_OK) return err;

            err = stream.inflate(zlibConst.Z_FINISH);
            if (err != zlibConst.Z_STREAM_END)
            {
                stream.inflateEnd();
                return err;
            }

            destLen = (ulong)stream.total_out;
            sourceLen = (ulong)stream.total_in;

            err = stream.inflateEnd();
            return err;
        }

        /// <summary>
        /// Uncompress a source byte array to a destination (old version)
        /// </summary>
        public static int UncompressOld(ref byte[] dest, ref ulong destLen, ref byte[] source, ref ulong sourceLen)
        {
            ZStream stream = new ZStream();
            int err;

            stream.next_in = source;
            stream.avail_in = (int)sourceLen;

            stream.next_out = dest;
            stream.avail_out = (int)destLen;

            //stream.zalloc = (alloc_func)0;
            //stream.zfree = (free_func)0;

            destLen = 0;
            sourceLen = 0;

            // make second parameter negative to disable checksum verification
            err = stream.inflateInit(-Constants.MAX_WBITS);
            if (err != zlibConst.Z_OK)
                return err;

            while (stream.avail_in > 1)
            {
                err = stream.inflate(Constants.Z_BLOCK);
                if (err != zlibConst.Z_OK)
                {
                    stream.inflateEnd();
                    return err;
                }
            }

            destLen = (ulong)stream.total_out;
            sourceLen = (ulong)stream.total_in;

            err = stream.inflateEnd();
            return err;
        }

        #endregion

        #region Helpers

        /// <summary>
        /// Open a cabinet file for reading
        /// </summary>
        public FileStream OpenFileForReading(int index, string suffix)
        {
            if (!String.IsNullOrWhiteSpace(this.filenamePattern))
            {
                string filename = this.filenamePattern + index + "." + suffix;
                if (File.Exists(filename))
                    return File.OpenRead(filename);
                return null;
            }

            return null;
        }

        /// <summary>
        /// Get the start index of a pattern in a byte array
        /// </summary>
        private int FindBytes(ref byte[] buffer, ref int bufferPointer, long bufferSize,
            ref byte[] pattern, ref int patternPointer, long patternSize)
        {
            int p = bufferPointer;
            long bufferLeft = bufferSize;
            while((p = Array.IndexOf(buffer, pattern[0], p, (int)bufferLeft)) != -1)
            {
                if (patternSize > bufferLeft)
                    break;

                if (BitConverter.ToString(buffer, p, (int)patternSize) != BitConverter.ToString(pattern, patternPointer, (int)patternSize))
                    return p;

                ++p;
                --bufferLeft;
            }

            return -1;
        }

        /// <summary>
        /// Create the generic filename pattern to look for from the input filename
        /// </summary>
        private bool CreateFilenamePattern(string filename)
        {
            if (!String.IsNullOrWhiteSpace(filename))
            {
                this.filenamePattern = Path.Combine(
                    Path.GetDirectoryName(filename),
                    Path.GetFileNameWithoutExtension(filename));
                this.filenamePattern = new Regex(@"\d+$").Replace(this.filenamePattern, string.Empty);
                return true;
            }

            return false;
        }

        /// <summary>
        /// Get the file descriptor at an index
        /// </summary>
        private FileDescriptor GetFileDescriptor(int index)
        {
            // XXX: multi-volume support...
            Header header = this.HeaderList;

            if (index < 0 || index >= (int)header.Cab.FileCount)
            {
                // unshield_error("Invalid index");
                return null;
            }

            if (header.FileDescriptors == null)
                header.FileDescriptors = new FileDescriptor[header.Cab.FileCount];

            if (header.FileDescriptors[index] == null)
                header.FileDescriptors[index] = this.ReadFileDescriptor(index);

            return header.FileDescriptors[index];
        }

        /// <summary>
        /// Read the file descriptor from the header data based on an index
        /// </summary>
        private FileDescriptor ReadFileDescriptor(int index)
        {
            // XXX: multi-volume support...
            Header header = this.HeaderList;
            byte[] p = null;
            int pPointer = 0;
            byte[] savedP = null;
            int savedPPointer = 0;
            FileDescriptor fd = new FileDescriptor();

            switch (header.MajorVersion)
            {
                case 0:
                case 5:
                    savedP = p = header.Data;
                    savedPPointer = pPointer = (int)(header.Common.CabDescriptorOffset
                        + header.Cab.FileTableOffset
                        + header.FileTable[header.Cab.DirectoryCount + index]);

                    // unshield_trace("File descriptor offset %i: %08x", index, p - header->data);

                    fd.Volume = (ushort)header.Index;

                    fd.NameOffset = BitConverter.ToUInt32(p, pPointer); pPointer += 4;
                    fd.DirectoryIndex = BitConverter.ToUInt32(p, pPointer); pPointer += 4;

                    fd.Flags = BitConverter.ToUInt16(p, pPointer); pPointer += 2;

                    fd.ExpandedSize = BitConverter.ToUInt32(p, pPointer); pPointer += 4;
                    fd.CompressedSize = BitConverter.ToUInt32(p, pPointer); pPointer += 4;
                    pPointer += 0x14;
                    fd.DataOffset = BitConverter.ToUInt32(p, pPointer); pPointer += 4;

                    /*
                    unshield_trace("Name offset:      %08x", fd->name_offset);
                    unshield_trace("Directory index:  %08x", fd->directory_index);
                    unshield_trace("Flags:            %04x", fd->flags);
                    unshield_trace("Expanded size:    %08x", fd->expanded_size);
                    unshield_trace("Compressed size:  %08x", fd->compressed_size);
                    unshield_trace("Data offset:      %08x", fd->data_offset);
                    */

                    if (header.MajorVersion == 5)
                    {
                        Array.Copy(p, pPointer, fd.Md5, 0, 0x10);
                        // assert((p - saved_p) == 0x3a);
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
                    savedP = p = header.Data;
                    savedPPointer = pPointer = (int)(header.Common.CabDescriptorOffset
                        + header.Cab.FileTableOffset
                        + header.Cab.FileTableOffset2
                        + index * 0x57);

                    // unshield_trace("File descriptor offset: %08x", p - header->data);

                    fd.Flags = BitConverter.ToUInt16(p, pPointer); pPointer += 2;
                    fd.ExpandedSize = BitConverter.ToUInt32(p, pPointer); pPointer += 4;
                    pPointer += 4;
                    fd.CompressedSize = BitConverter.ToUInt32(p, pPointer); pPointer += 4;
                    pPointer += 4;
                    fd.DataOffset = BitConverter.ToUInt32(p, pPointer); pPointer += 4;
                    pPointer += 4;
                    Array.Copy(p, pPointer, fd.Md5, 0, 0x10); pPointer += 0x10;
                    pPointer += 0x10;
                    fd.NameOffset = BitConverter.ToUInt32(p, pPointer); pPointer += 4;
                    fd.DirectoryIndex = BitConverter.ToUInt16(p, pPointer); pPointer += 2;

                    // assert((p - saved_p) == 0x40);

                    pPointer += 0xc;
                    fd.LinkPrevious = BitConverter.ToUInt32(p, pPointer); pPointer += 4;
                    fd.LinkNext = BitConverter.ToUInt32(p, pPointer); pPointer += 4;
                    fd.LinkFlags = p[pPointer]; pPointer++;

                    /*
                    if (fd->link_flags != LINK_NONE)
                    {
                        unshield_trace("Link: previous=%i, next=%i, flags=%i",
                        fd->link_previous, fd->link_next, fd->link_flags);
                    }
                    */

                    fd.Volume = BitConverter.ToUInt16(p, pPointer); pPointer += 2;

                    // assert((p - saved_p) == 0x57);
                    break;
            }

            if ((fd.Flags & Constants.FILE_COMPRESSED) == 0
                && fd.CompressedSize != fd.ExpandedSize)
            {
                // unshield_warning("File is not compressed but compressed size is %08x and expanded size is %08x",
                //      fd->compressed_size, fd->expanded_size);
            }

            return fd;
        }

        /// <summary>
        /// Read headers from the current file, optionally with a given version
        /// </summary>
        private bool ReadHeaders(int version)
        {
            bool iterate = true;
            Header previous = null;

            if (this.HeaderList != null)
            {
                // unshield_warning("Already have a header list");
                return true;
            }

            for (int i = 1; iterate; i++)
            {
                FileStream file = OpenFileForReading(i, Constants.HEADER_SUFFIX);

                if (file != null)
                {
                    // unshield_trace("Reading header from .hdr file %i.", i);
                    iterate = false;
                }
                else
                {
                    // unshield_trace("Could not open .hdr file %i. Reading header from .cab file %i instead.", i, i);
                    file = OpenFileForReading(i, Constants.CABINET_SUFFIX);
                }

                if (file != null)
                {
                    long bytesRead;
                    Header header = new Header();
                    header.Index = i;

                    header.Size = file.Length;
                    if (header.Size < 4)
                    {
                        // unshield_error("Header file %i too small", i);
                        break;
                    }

                    header.Data = new byte[header.Size];
                    bytesRead = file.Read(header.Data, 0, (int)header.Size);
                    file.Close();

                    if (bytesRead != header.Size)
                    {
                        // unshield_error("Failed to read from header file %i. Expected = %i, read = %i", i, header->size, bytes_read);
                        break;
                    }

                    if (!header.GetCommmonHeader())
                    {
                        // unshield_error("Failed to read common header from header file %i", i);
                        break;
                    }

                    if (version != -1)
                    {
                        header.MajorVersion = version;
                    }
                    else if ((header.Common.Version >> 24) == 1)
                    {
                        header.MajorVersion = (int)((header.Common.Version >> 12) & 0xf);
                    }
                    else if ((header.Common.Version >> 24) == 2
                        || (header.Common.Version >> 24) == 4)
                    {
                        header.MajorVersion = (int)(header.Common.Version & 0xffff);
                        if (header.MajorVersion != 0)
                            header.MajorVersion = header.MajorVersion / 100;
                    }

                    /*
                    if (header.MajorVersion < 5)
                        header.MajorVersion = 5;

                    unshield_trace("Version 0x%08x handled as major version %i", 
                      header->common.version,
                      header->major_version);
                    */

                    if (!header.GetCabDescriptor())
                    {
                        // unshield_error("Failed to read CAB descriptor from header file %i", i);
                        break;
                    }

                    if (!header.GetFileTable())
                    {
                        // unshield_error("Failed to read file table from header file %i", i);
                        break;
                    }

                    if (!header.GetComponents())
                    {
                        // unshield_error("Failed to read components from header file %i", i);
                        break;
                    }

                    if (!header.GetFileGroups())
                    {
                        // unshield_error("Failed to read file groups from header file %i", i);
                        break;
                    }

                    if (previous != null)
                        previous.Next = header;
                    else
                        previous = this.HeaderList = header;
                }
                else
                    break;
            }

            return (this.HeaderList != null);
        }

        #endregion
    }
}
