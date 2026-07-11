namespace MPF.Frontend
{
    /// <summary>
    /// A macOS whole-disk device discovered via diskutil.
    /// </summary>
    internal sealed class MacOSBlockDevice
    {
        /// <summary>
        /// Device node path (e.g. "/dev/disk0")
        /// </summary>
        public string DevicePath { get; }

        /// <summary>
        /// Drive type derived from the diskutil "Removable Media" flag, media size, and
        /// optical characteristics
        /// </summary>
        public InternalDriveType DriveType { get; }

        /// <summary>
        /// Device size in bytes, or 0 when unknown
        /// </summary>
        public long TotalSize { get; }

        public MacOSBlockDevice(string devicePath, InternalDriveType driveType, long totalSize)
        {
            DevicePath = devicePath;
            DriveType = driveType;
            TotalSize = totalSize;
        }
    }
}
