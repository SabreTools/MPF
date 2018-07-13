using System;

namespace DICUI.External.Unshield
{
    public class CommonHeader
    {
        public uint Signature; // 00
        public uint Version;
        public uint VolumeInfo;
        public uint CabDescriptorOffset;
        public uint CabDescriptorSize; // 10

        /// <summary>
        /// Populate a CommonHeader from an input buffer
        /// </summary>
        public static bool ReadCommonHeader(ref byte[] buffer, int bufferPointer, CommonHeader common)
        {
            common.Signature = BitConverter.ToUInt32(buffer, bufferPointer); bufferPointer += 4;

            if (common.Signature != Constants.CAB_SIGNATURE)
            {
                // unshield_error("Invalid file signature");

                if (common.Signature == Constants.MSCF_SIGNATURE)
                {
                    // unshield_warning("Found Microsoft Cabinet header. Use cabextract (http://www.kyz.uklinux.net/cabextract.php) to unpack this file.");
                }

                return false;
            }

            common.Version = BitConverter.ToUInt32(buffer, bufferPointer); bufferPointer += 4;
            common.VolumeInfo = BitConverter.ToUInt32(buffer, bufferPointer); bufferPointer += 4;
            common.CabDescriptorOffset = BitConverter.ToUInt32(buffer, bufferPointer); bufferPointer += 4;
            common.CabDescriptorSize = BitConverter.ToUInt32(buffer, bufferPointer); bufferPointer += 4;

            /*
            unshield_trace("Common header: %08x %08x %08x %08x",
            common->version, 
            ommon->volume_info, 
            common->cab_descriptor_offset, 
            common->cab_descriptor_size);
            */

            return true;
        }
    }
}
