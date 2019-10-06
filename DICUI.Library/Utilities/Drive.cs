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
        public InternalDriveType InternalDriveType { get; private set; }

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
                if (DriveInfo.IsReady)
                {
                    if (string.IsNullOrWhiteSpace(DriveInfo.VolumeLabel))
                        return "track";
                    else
                        return DriveInfo.VolumeLabel;
                }
                else
                {
                    return Template.DiscNotDetected;
                }
            }
        }

        /// <summary>
        /// Drive partition format
        /// </summary>
        public string Driveformat { get { return DriveInfo.DriveFormat; } }

        /// <summary>
        /// Represents if Windows has marked the drive as active
        /// </summary>
        public bool MarkedActive { get { return DriveInfo.IsReady; } }

        private Drive(InternalDriveType driveType, DriveInfo driveInfo)
        {
            this.InternalDriveType = driveType;
            this.DriveInfo = driveInfo;
        }

        /// <summary>
        /// Create a new floppy drive instance
        /// </summary>
        /// <param name="letter">Drive letter to use</param>
        /// <returns>Drive object for a floppy drive</returns>
        public static Drive Floppy(char letter) => new Drive(InternalDriveType.Floppy, new DriveInfo($"{letter}:\\"));

        /// <summary>
        /// generate a new hard disk drive instance
        /// </summary>
        /// <param name="driveInfo">Drive information object</param>
        /// <returns>Drive object for a hard disk drive</returns>
        public static Drive HardDisk(DriveInfo driveInfo) => new Drive(InternalDriveType.HardDisk, driveInfo);

        /// <summary>
        /// Create a new optical drive instance
        /// </summary>
        /// <param name="driveInfo">Drive information object</param>
        /// <returns>Drive object for an optical drive</returns>
        public static Drive Optical(DriveInfo driveInfo) => new Drive(InternalDriveType.Optical, driveInfo);

        /// <summary>
        /// Create a new removable drive instance
        /// </summary>
        /// <param name="driveInfo">Drive information object</param>
        /// <returns>Drive object for a removable drive</returns>
        public static Drive Removable(DriveInfo driveInfo) => new Drive(InternalDriveType.Removable, driveInfo);
    }
}
