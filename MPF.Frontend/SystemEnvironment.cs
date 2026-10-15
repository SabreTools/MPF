#if NET5_0_OR_GREATER
using System;
#endif
#if NETCOREAPP3_1
using System.Runtime.InteropServices;
#endif

namespace MPF.Frontend
{
    /// <summary>
    /// Helpers for the system environment
    /// </summary>
    /// TODO: Remove this when IO is updated
    public static class SystemEnvironment
    {
        /// <summary>
        /// Determine if the current system is MacOS
        /// </summary>
        public static bool IsLinux()
        {
#if NETFRAMEWORK
            // .NET Framework can never be Linux
            return false;
#elif NETCOREAPP3_1
            return RuntimeInformation.IsOSPlatform(OSPlatform.Linux);
#else
            return OperatingSystem.IsLinux();
#endif
        }

        /// <summary>
        /// Determine if the current system is MacOS
        /// </summary>
        public static bool IsMacOS()
        {
#if NETFRAMEWORK
            // .NET Framework can never be MacOS
            return false;
#elif NETCOREAPP3_1
            return RuntimeInformation.IsOSPlatform(OSPlatform.OSX);
#else
            return OperatingSystem.IsMacOS();
#endif
        }

        /// <summary>
        /// Determine if the current system is Windows
        /// </summary>
        public static bool IsWindows()
        {
#if NETFRAMEWORK
            // .NET Framework is always Windows
            return true;
#elif NETCOREAPP3_1
            return RuntimeInformation.IsOSPlatform(OSPlatform.Windows);
#else
            return OperatingSystem.IsWindows();
#endif
        }
    }
}
