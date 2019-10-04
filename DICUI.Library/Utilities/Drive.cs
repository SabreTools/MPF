using DICUI.Data;

namespace DICUI.Utilities
{
    /// <summary>
    /// Represents information for a single drive
    /// </summary>
    public class Drive
    {
        /// <summary>
        /// Windows drive letter
        /// </summary>
        public char Letter { get; private set; }

        /// <summary>
        /// Represents drive type
        /// </summary>
        public InternalDriveType DriveType { get; private set; }

        /// <summary>
        /// Media label as read by Windows
        /// </summary>
        public string VolumeLabel { get; private set; }

        /// <summary>
        /// Represents if Windows has marked the drive as active
        /// </summary>
        public bool MarkedActive { get; private set; }

        private Drive(char letter, string volumeLabel, InternalDriveType driveType, bool markedActive)
        {
            this.Letter = letter;
            this.DriveType = driveType;
            this.VolumeLabel = volumeLabel;
            this.MarkedActive = markedActive;
        }

        /// <summary>
        /// Create a new floppy drive instance
        /// </summary>
        /// <param name="letter">Drive letter to use</param>
        /// <returns>Drive object for a floppy drive</returns>
        public static Drive Floppy(char letter) => new Drive(letter, null, InternalDriveType.Floppy, true);

        /// <summary>
        /// generate a new hard disk drive instance
        /// </summary>
        /// <param name="letter">Drive letter to use</param>
        /// <param name="volumeLabel">Media label, if it exists</param>
        /// <returns>Drive object for a hard disk drive</returns>
        public static Drive HardDisk(char letter, string volumeLabel) => new Drive(letter, volumeLabel, InternalDriveType.HardDisk, true);

        /// <summary>
        /// Create a new optical drive instance
        /// </summary>
        /// <param name="letter">Drive letter to use</param>
        /// <param name="volumeLabel">Media label, if it exists</param>
        /// <param name="active">True if the drive is marked active, false otherwise</param>
        /// <returns>Drive object for an optical drive</returns>
        public static Drive Optical(char letter, string volumeLabel, bool active) => new Drive(letter, volumeLabel, InternalDriveType.Optical, active);

        /// <summary>
        /// Create a new removable drive instance
        /// </summary>
        /// <param name="letter">Drive letter to use</param>
        /// <param name="volumeLabel">Media label, if it exists</param>
        /// <param name="active">True if the drive is marked active, false otherwise</param>
        /// <returns>Drive object for a removable drive</returns>
        public static Drive Removable(char letter, string volumeLabel, bool active) => new Drive(letter, volumeLabel, InternalDriveType.Removable, active);
    }
}
