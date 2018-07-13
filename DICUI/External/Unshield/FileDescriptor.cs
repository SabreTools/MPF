namespace DICUI.External.Unshield
{
    public class FileDescriptor
    {
        public uint NameOffset;
        public uint DirectoryIndex;
        public ushort Flags;
        public uint ExpandedSize;
        public uint CompressedSize;
        public uint DataOffset;
        public byte[] Md5 = new byte[16];
        public ushort Volume;
        public uint LinkPrevious;
        public uint LinkNext;
        public byte LinkFlags;
    }
}
