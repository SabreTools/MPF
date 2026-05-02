using System;
using System.Runtime.InteropServices;
using Avalonia.Controls;

namespace MPF.Avalonia.Services
{
    internal static class WindowChromeService
    {
        public static void Apply(Window window)
        {
            HideMacOSZoomButton(window);
            ThemeService.ApplyWindowTitleBarTheme(window);
        }

        private static void HideMacOSZoomButton(Window window)
        {
            if (!OperatingSystem.IsMacOS())
                return;

            IntPtr nsWindow = window.TryGetPlatformHandle()?.Handle ?? IntPtr.Zero;
            if (nsWindow == IntPtr.Zero)
                return;

            IntPtr zoomButton = objc_msgSend_IntPtr_Int64(nsWindow, sel_registerName("standardWindowButton:"), NSWindowZoomButton);
            if (zoomButton == IntPtr.Zero)
                return;

            objc_msgSend_bool(zoomButton, sel_registerName("setEnabled:"), false);
            objc_msgSend_bool(zoomButton, sel_registerName("setHidden:"), true);
        }

        private const long NSWindowZoomButton = 2;

        [DllImport("/usr/lib/libobjc.A.dylib")]
        private static extern IntPtr sel_registerName(string name);

        [DllImport("/usr/lib/libobjc.A.dylib", EntryPoint = "objc_msgSend")]
        private static extern IntPtr objc_msgSend_IntPtr_Int64(IntPtr receiver, IntPtr selector, long argument);

        [DllImport("/usr/lib/libobjc.A.dylib", EntryPoint = "objc_msgSend")]
        private static extern void objc_msgSend_bool(IntPtr receiver, IntPtr selector, bool argument);
    }
}
