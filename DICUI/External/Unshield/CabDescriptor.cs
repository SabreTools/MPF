namespace DICUI.External.Unshield
{
    public class CabDescriptor
    {
        public uint FileTableOffset;             /* 0c */
        public uint FileTableSize;               /* 14 */
        public uint FileTableSize2;              /* 18 */
        public uint DirectoryCount;              /* 1c */
        public uint FileCount;                   /* 28 */
        public uint FileTableOffset2;            /* 2c */

        public uint[] FileGroupOffsets = new uint[Constants.MAX_FILE_GROUP_COUNT];  /* 0x3e  */
        public uint[] ComponentOffsets = new uint[Constants.MAX_COMPONENT_COUNT];   /* 0x15a */
    }
}
