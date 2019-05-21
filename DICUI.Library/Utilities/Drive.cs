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
        /// Represents if it is a floppy drive
        /// </summary>
        public bool IsFloppy { get; private set; }

        /// <summary>
        /// Media label as read by Windows
        /// </summary>
        public string VolumeLabel { get; private set; }

        /// <summary>
        /// Represents if Windows has marked the drive as active
        /// </summary>
        public bool MarkedActive { get; private set; }

        private Drive(char letter, string volumeLabel, bool isFloppy, bool markedActive)
        {
            this.Letter = letter;
            this.IsFloppy = isFloppy;
            this.VolumeLabel = volumeLabel;
            this.MarkedActive = markedActive;
        }

        /// <summary>
        /// Create a new Floppy drive instance
        /// </summary>
        /// <param name="letter">Drive letter to use</param>
        /// <returns>Drive object for a Floppy drive</returns>
        public static Drive Floppy(char letter) => new Drive(letter, null, true, true);

        /// <summary>
        /// Create a new Optical drive instance
        /// </summary>
        /// <param name="letter">Drive letter to use</param>
        /// <param name="volumeLabel">Media label, if it exists</param>
        /// <param name="active">True if the drive is marked active, false otherwise</param>
        /// <returns>Drive object for an Optical drive</returns>
        public static Drive Optical(char letter, string volumeLabel, bool active) => new Drive(letter, volumeLabel, false, active);
    }
}
