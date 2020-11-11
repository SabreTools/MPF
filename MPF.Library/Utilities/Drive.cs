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
        /// DriveInfo object representing the drive, if possible
        /// </summary>
        public DriveInfo DriveInfo { get; private set; }

        /// <summary>
        /// Windows drive letter
        /// </summary>
        public char Letter { get { return DriveInfo?.Name[0] ?? '\0'; } }

        /// <summary>
        /// Media label as read by Windows
        /// </summary>
        public string VolumeLabel
        {
            get
            {
                string volumeLabel = Template.DiscNotDetected;
                if (DriveInfo.IsReady)
                {
                    if (string.IsNullOrWhiteSpace(DriveInfo.VolumeLabel))
                        volumeLabel = "track";
                    else
                        volumeLabel = DriveInfo.VolumeLabel;
                }

                foreach (char c in Path.GetInvalidFileNameChars())
                    volumeLabel = volumeLabel.Replace(c, '_');

                return volumeLabel;
            }
        }

        /// <summary>
        /// Drive partition format
        /// </summary>
        public string DriveFormat { get { return DriveInfo.DriveFormat; } }

        /// <summary>
        /// Represents if Windows has marked the drive as active
        /// </summary>
        public bool MarkedActive { get { return DriveInfo.IsReady; } }

        public Drive(InternalDriveType? driveType, DriveInfo driveInfo)
        {
            this.InternalDriveType = driveType;
            this.DriveInfo = driveInfo;
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
            if (string.IsNullOrEmpty(this.DriveInfo?.Name))
                return null;

            // We don't support negative sectors
            if (num < 0)
                return null;

            // Wrap the following in case of device access errors
            try
            {
                // Open the drive as a device
                var fs = File.OpenRead($"\\\\?\\{this.Letter}:");

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
        }
    }
}
