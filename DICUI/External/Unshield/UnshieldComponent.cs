using System;

namespace DICUI.External.Unshield
{
    public class UnshieldComponent
    {
        public string Name;
        public uint FileGroupCount;
        public string[] FileGroupNames;
        public int FileGroupNamesPointer = 0;

        /// <summary>
        /// Create a new UnshieldComponent from a header and data offset
        /// </summary>
        public static UnshieldComponent Create(Header header, uint offset)
        {
            UnshieldComponent self = new UnshieldComponent();
            int bufferPointer = header.GetDataOffset(offset);
            uint fileGroupTableOffset;

            self.Name = header.GetString((uint)bufferPointer); bufferPointer += 4;

            switch (header.MajorVersion)
            {
                case 0:
                case 5:
                    bufferPointer += 0x6c;
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
                    bufferPointer += 0x6b;
                    break;
            }

            self.FileGroupCount = BitConverter.ToUInt16(header.Data, bufferPointer); bufferPointer += 2;
            if (self.FileGroupCount > Constants.MAX_FILE_GROUP_COUNT)
                return default(UnshieldComponent);

            self.FileGroupNames = new string[self.FileGroupCount];

            fileGroupTableOffset = BitConverter.ToUInt32(header.Data, bufferPointer); bufferPointer += 4;

            bufferPointer = header.GetDataOffset(fileGroupTableOffset);

            for (int i = 0; i < self.FileGroupCount; i++)
            {
                self.FileGroupNames[i] = header.GetString((uint)bufferPointer); bufferPointer += 4; // TODO: Verify GetString
            }

            return self;
        }
    }
}
