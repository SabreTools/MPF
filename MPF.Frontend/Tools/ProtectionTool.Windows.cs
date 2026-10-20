using System;
using System.IO;
using System.Runtime.InteropServices;
using Microsoft.Win32.SafeHandles;

namespace MPF.Frontend.Tools
{
    public static partial class ProtectionTool
    {
        /// <summary>
        /// Control code to mark a file as sparse
        /// </summary>
        private const uint FSCTL_SET_SPARSE = 0x000900C4;

        /// <summary>
        /// Mark a file as sparse on Windows, so the parts of it that are not written take no space
        /// </summary>
        /// <param name="file">File to mark</param>
        /// <remarks>On a file system without sparse files, the file stays as it is</remarks>
        private static void MarkWindowsSparseFile(FileStream file)
        {
            DeviceIoControl(file.SafeFileHandle, FSCTL_SET_SPARSE, IntPtr.Zero, 0, IntPtr.Zero, 0, out _, IntPtr.Zero);
        }

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool DeviceIoControl(SafeFileHandle hDevice,
            uint dwIoControlCode,
            IntPtr lpInBuffer,
            uint nInBufferSize,
            IntPtr lpOutBuffer,
            uint nOutBufferSize,
            out uint lpBytesReturned,
            IntPtr lpOverlapped);
    }
}
