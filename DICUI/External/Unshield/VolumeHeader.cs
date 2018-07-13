namespace DICUI.External.Unshield
{
    public class VolumeHeader
    {
        public uint DataOffset;
        public uint DataOffsetHigh;
        public uint FirstFileIndex;
        public uint LastFileIndex;
        public uint FirstFileOffset;
        public uint FirstFileOffsetHigh;
        public uint FirstFileSizeExpanded;
        public uint FirstFileSizeExpandedHigh;
        public uint FirstFileSizeCompressed;
        public uint FirstFileSizeCompressedHigh;
        public uint LastFileOffset;
        public uint LastFileOffsetHigh;
        public uint LastFileSizeExpanded;
        public uint LastFileSizeExpandedHigh;
        public uint LastFileSizeCompressed;
        public uint LastFileSizeCompressedHigh;
    }
}
