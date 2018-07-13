using System;

namespace DICUI.External.Unshield
{
    public class Header
    {
        public Header Next;
        public int Index;
        public byte[] Data;
        public int DataPointer = 0;
        public long Size;
        public int MajorVersion;

        // Shortcuts
        public CommonHeader Common = new CommonHeader();
        public CabDescriptor Cab = new CabDescriptor();
        public uint[] FileTable;
        public int FileTablePointer;
        public FileDescriptor[] FileDescriptors;
        public int FileDescriptorsPointer;

        public int ComponentCount;
        public UnshieldComponent[] Components;
        public int ComponentsPointer;

        public int FileGroupCount;
        public UnshieldFileGroup[] FileGroups;
        public int FileGroupsCounter;

        public StringBuffer StringBuffer = new StringBuffer();

        public StringBuffer AddStringBuffer()
        {
            StringBuffer result = new StringBuffer();
            result.Next = this.StringBuffer;
            this.StringBuffer = result;
            return result;
        }

        public int GetBuffer(uint offset)
        {
            if (offset > 0)
                return (int)(this.Common.CabDescriptorOffset + offset);
            else
                return -1;
        }

        public bool GetCabDescriptor()
        {
            if (this.Common.CabDescriptorSize > 0)
            {
                int p = (int)(this.Common.CabDescriptorOffset);

                p += 0xc;
                this.Cab.FileTableOffset = BitConverter.ToUInt32(this.Data, p); p += 4;
                p += 4;
                this.Cab.FileTableSize = BitConverter.ToUInt32(this.Data, p); p += 4;
                this.Cab.FileTableSize2 = BitConverter.ToUInt32(this.Data, p); p += 4;
                this.Cab.DirectoryCount = BitConverter.ToUInt32(this.Data, p); p += 4;
                p += 8;
                this.Cab.FileCount = BitConverter.ToUInt32(this.Data, p); p += 4;
                this.Cab.FileTableOffset2 = BitConverter.ToUInt32(this.Data, p); p += 4;

                // assert((p - (header->data + header->common.cab_descriptor_offset)) == 0x30);

                if (this.Cab.FileTableSize != this.Cab.FileTableSize2)
                {
                    // unshield_warning("File table sizes do not match");
                }

                /*
                unshield_trace("Cabinet descriptor: %08x %08x %08x %08x",
                    header->cab.file_table_offset,
                    header->cab.file_table_size,
                    header->cab.file_table_size2,
                    header->cab.file_table_offset2
                    );

                unshield_trace("Directory count: %i", header->cab.directory_count);
                unshield_trace("File count: %i", header->cab.file_count);
                */

                p += 0xe;

                for (int i = 0; i < Constants.MAX_FILE_GROUP_COUNT; i++)
                {
                    this.Cab.FileGroupOffsets[i] = BitConverter.ToUInt32(this.Data, p); p += 4;
                }
                
                for (int i = 0; i < Constants.MAX_COMPONENT_COUNT; i++)
                {
                    this.Cab.ComponentOffsets[i] = this.Cab.FileGroupOffsets[i] = BitConverter.ToUInt32(this.Data, p); p += 4;
                }

                return true;
            }
            else
            {
                // unshield_error("No CAB descriptor available!");
                return false;
            }
        }

        public bool GetCommmonHeader()
        {
            return CommonHeader.ReadCommonHeader(ref this.Data, this.DataPointer, this.Common);
        }

        public bool GetComponents()
        {
            int count = 0;
            int available = 16;

            this.Components = new UnshieldComponent[available];

            for (int i = 0; i < Constants.MAX_COMPONENT_COUNT; i++)
            {
                if (this.Cab.ComponentOffsets[i] > 0)
                {
                    OffsetList list = new OffsetList();

                    list.NextOffset = this.Cab.ComponentOffsets[i];

                    while (list.NextOffset > 0)
                    {
                        int p = GetBuffer(list.NextOffset);

                        list.NameOffset = BitConverter.ToUInt32(this.Data, p); p += 4;
                        list.DescriptorOffset = BitConverter.ToUInt32(this.Data, p); p += 4;
                        list.NextOffset = BitConverter.ToUInt32(this.Data, p); p += 4;

                        if (count == available)
                        {
                            available <<= 1;
                            Array.Resize(ref this.Components, available);
                        }

                        this.Components[count++] = UnshieldComponent.Create(this, list.DescriptorOffset);
                    }
                }
            }

            this.ComponentCount = count;

            return true;
        }

        public bool GetFileGroups()
        {
            int count = 0;
            int available = 16;

            this.FileGroups = new UnshieldFileGroup[available];

            for (int i = 0; i < Constants.MAX_FILE_GROUP_COUNT; i++)
            {
                if (this.Cab.FileGroupOffsets[i] > 0)
                {
                    OffsetList list = new OffsetList();

                    list.NextOffset = this.Cab.FileGroupOffsets[i];

                    while (list.NextOffset > 0)
                    {
                        int p = GetBuffer(list.NextOffset);

                        list.NameOffset = BitConverter.ToUInt32(this.Data, p); p += 4;
                        list.DescriptorOffset = BitConverter.ToUInt32(this.Data, p); p += 4;
                        list.NextOffset = BitConverter.ToUInt32(this.Data, p); p += 4;

                        if (count == available)
                        {
                            available <<= 1;
                            Array.Resize(ref this.FileGroups, available);
                        }

                        this.FileGroups[count++] = UnshieldFileGroup.Create(this, list.DescriptorOffset);
                    }
                }
            }

            this.FileGroupCount = count;

            return true;
        }

        public bool GetFileTable()
        {
            int p = (int)(this.Common.CabDescriptorOffset +
                this.Cab.FileTableOffset);
            int count = (int)(this.Cab.DirectoryCount + this.Cab.FileCount);

            this.FileTable = new uint[count];

            for (int i = 0; i < count; i++)
            {
                this.FileTable[i] = BitConverter.ToUInt32(this.Data, p); p += 4;
            }

            return true;
        }

        public string GetString(uint offset)
        {
            return GetUTF8String(this.Data, GetBuffer(offset));
        }

        public string GetUTF8String(byte[] buffer, int bufferPointer)
        {
            return BitConverter.ToUInt32(buffer, bufferPointer).ToString("X8");
        }
    }
}
