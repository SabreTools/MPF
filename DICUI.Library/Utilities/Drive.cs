using System.IO;
using DICUI.Data;

namespace DICUI.Utilities
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
    }
}
