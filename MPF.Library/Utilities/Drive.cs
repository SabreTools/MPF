using System.IO;
using MPF.Data;

namespace MPF.Utilities
{
    /// <summary>
    /// Represents information for a single drive
    /// </summary>
    public class Drive
    {
        /// <summary>
        /// Represents drive type
        /// </summary>
        public InternalDriveType? InternalDriveType { get; set; }

        /// <summary>
        /// Drive partition format
        /// </summary>
        public string DriveFormat { get { return driveInfo.DriveFormat; } }

        /// <summary>
        /// Windows drive letter
        /// </summary>
        public char Letter { get { return driveInfo?.Name[0] ?? '\0'; } }

        /// <summary>
        /// Represents if Windows has marked the drive as active
        /// </summary>
        public bool MarkedActive { get { return driveInfo.IsReady; } }

        /// <summary>
        /// Media label as read by Windows
        /// </summary>
        public string VolumeLabel
        {
            get
            {
                string volumeLabel = Template.DiscNotDetected;
                if (driveInfo.IsReady)
                {
                    if (string.IsNullOrWhiteSpace(driveInfo.VolumeLabel))
                        volumeLabel = "track";
                    else
                        volumeLabel = driveInfo.VolumeLabel;
                }

                foreach (char c in Path.GetInvalidFileNameChars())
                    volumeLabel = volumeLabel.Replace(c, '_');

                return volumeLabel;
            }
        }

        /// <summary>
        /// DriveInfo object representing the drive, if possible
        /// </summary>
        private DriveInfo driveInfo = null;

        public Drive(InternalDriveType? driveType, DriveInfo driveInfo)
        {
            this.InternalDriveType = driveType;
            this.driveInfo = driveInfo;
        }

        /// <summary>
        /// Read a sector with a specified size from the drive
        /// </summary>
        /// <param name="num">Sector number, non-negative</param>
        /// <param name="size">Size of a sector in bytes</param>
        /// <returns>Byte array representing the sector, null on error</returns>
        public byte[] ReadSector(long num, int size = 2048)
        {
            // Missing drive leter is not supported
            if (string.IsNullOrEmpty(this.driveInfo?.Name))
                return null;

            // We don't support negative sectors
            if (num < 0)
                return null;

            // Wrap the following in case of device access errors
            Stream fs = null;
            try
            {
                // Open the drive as a device
                fs = File.OpenRead($"\\\\?\\{this.Letter}:");

                // Seek to the start of the sector, if possible
                long start = num * size;
                fs.Seek(start, SeekOrigin.Begin);

                // Read and return the sector
                byte[] buffer = new byte[size];
                fs.Read(buffer, 0, size);
                return buffer;
            }
            catch
            {
                return null;
            }
            finally
            {
                fs.Dispose();
            }
        }
    }
}
