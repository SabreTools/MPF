using System;

namespace DICUI.External.Unshield
{
    public class UnshieldFileGroup
    {
        public string Name;
        public uint FirstFile;
        public uint LastFile;

        /// <summary>
        /// Create a new UnshieldFileGroup from a header and data offset
        /// </summary>
        public static UnshieldFileGroup Create(Header header, uint offset)
        {
            UnshieldFileGroup self = new UnshieldFileGroup();
            int pPointer = header.GetDataOffset(offset);

            // unshield_trace("File group descriptor offset: %08x", offset);

            self.Name = header.GetString(BitConverter.ToUInt32(header.Data, pPointer)); pPointer += 4;

            if (header.MajorVersion <= 5)
                pPointer += 0x48;
            else
                pPointer += 0x12;

            self.FirstFile = BitConverter.ToUInt32(header.Data, pPointer); pPointer += 4;
            self.LastFile = BitConverter.ToUInt32(header.Data, pPointer); pPointer += 4;

            // unshield_trace("File group %08x first file = %i, last file = %i", offset, self->first_file, self->last_file);

            return self;
        }
    }
}
