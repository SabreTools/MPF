using System;
using System.IO;
using System.Threading.Tasks;
using MPF.Processors;

namespace MPF.Frontend.Tools
{
    internal static class IRDTool
    {
        /// <summary>
        /// Create an IRD and write it next to the input ISO path
        /// </summary>
        /// <param name="isoPath">Path to the original ISO</param>
        /// <param name="discKeyString">Required hexadecimal disc key as a string</param>
        /// <param name="discIDString">Optional hexadecimal disc ID as a string</param>
        /// <param name="picString">Optional string representation of the PIC information</param>
        /// <param name="layerbreak">Optional disc layerbreak value</param>
        /// <param name="crc32">Optional ISO CRC-32 value for the Unique ID field</param>
        /// <returns>True on success, false on error</returns>
        public static async Task<bool> WriteIRD(string isoPath,
            string? discKeyString,
            string? discIDString,
            string? picString,
            long? layerbreak,
            string? crc32)
        {
            try
            {
                // Parse string values into required formats
                byte[]? discKey = ProcessingTool.ParseHexKey(discKeyString);
                byte[]? discID = ProcessingTool.ParseDiscID(discIDString);
                byte[]? pic = ProcessingTool.ParsePIC(picString);
                uint? uid = ProcessingTool.ParseCRC32(crc32);

                return await WriteIRD(isoPath, discKey, discID, pic, layerbreak, uid);
            }
            catch (Exception)
            {
                // Absorb the exception
                return false;
            }
        }

        /// <summary>
        /// Create an IRD and write it next to the input ISO path
        /// </summary>
        /// <param name="isoPath">Path to the original ISO</param>
        /// <param name="discKey">Required hexadecimal disc key as a byte array</param>
        /// <param name="discID">Optional hexadecimal disc ID as a byte array</param>
        /// <param name="pic">Optional byte array representation of the PIC information</param>
        /// <param name="layerbreak">Optional disc layerbreak value</param>
        /// <param name="uid">Optional ISO CRC-32 value for the Unique ID field</param>
        /// <returns>True on success, false on error</returns>
        public static async Task<bool> WriteIRD(string isoPath,
            byte[]? discKey,
            byte[]? discID,
            byte[]? pic,
            long? layerbreak,
            uint? uid)
        {
            try
            {
                // Fail on a missing disc key
                if (discKey == null)
                    return false;

                // Output IRD file path
                string irdPath = Path.ChangeExtension(isoPath, ".ird");

                // Ensure layerbreak value is valid
                layerbreak = ProcessingTool.ParseLayerbreak(layerbreak);

                // Create Redump-style reproducible IRD
#if NET40
                LibIRD.ReIRD ird = await Task.Factory.StartNew(() =>
#else
                LibIRD.ReIRD ird = await Task.Run(() =>
#endif
                    new LibIRD.ReIRD(isoPath, discKey, layerbreak, uid));

                // Set optional fields if valid
                if (pic != null)
                    ird.PIC = pic;
                if (discID != null && ird.DiscID[15] != 0x00)
                    ird.DiscID = discID;

                // Write IRD to file
                ird.Write(irdPath);

                return true;
            }
            catch (Exception)
            {
                // Absorb the exception
                return false;
            }
        }
    }
}
