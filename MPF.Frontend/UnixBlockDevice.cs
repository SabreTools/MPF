namespace MPF.Frontend
{
    /// <summary>
    /// A Linux fixed or removable block device discovered via sysfs.
    /// </summary>
    internal sealed class UnixBlockDevice
    {
        /// <summary>
        /// Device node path (e.g. "/dev/sda")
        /// </summary>
        public string DevicePath { get; }

        /// <summary>
        /// Drive type derived from the sysfs "removable" flag
        /// </summary>
        public InternalDriveType DriveType { get; }

        /// <summary>
        /// Device size in bytes, or 0 when unknown
        /// </summary>
        public long TotalSize { get; }

        public UnixBlockDevice(string devicePath, InternalDriveType driveType, long totalSize)
        {
            DevicePath = devicePath;
            DriveType = driveType;
            TotalSize = totalSize;
        }
    }
}
