using System.Runtime.InteropServices;

namespace MPF.Frontend.Tools
{
    /// <summary>
    /// Windows-specific functionality
    /// </summary>
    public static partial class FrontendTool
    {
        /// <summary>
        /// Get the free space available to the current user on the volume a directory is on
        /// </summary>
        /// <param name="directory">Existing directory, including network paths and folders a volume is mounted on</param>
        /// <returns>Free space in bytes, null if it could not be found</returns>
        private static long? GetWindowsAvailableFreeSpace(string directory)
        {
            if (!GetDiskFreeSpaceEx(directory, out ulong freeBytesAvailable, out _, out _))
                return null;

            return (long)freeBytesAvailable;
        }

        #region Native Interop

        [DllImport("kernel32.dll", CharSet = CharSet.Unicode)]
        private static extern bool GetDiskFreeSpaceEx(string lpDirectoryName, out ulong lpFreeBytesAvailable, out ulong lpTotalNumberOfBytes, out ulong lpTotalNumberOfFreeBytes);

        #endregion
    }
}
