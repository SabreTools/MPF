using System;
using System.Runtime.InteropServices;
using Avalonia.Controls;
using Avalonia.Threading;

namespace MPF.Avalonia.Services
{
    /// <summary>
    /// Applies platform-specific window chrome adjustments, such as hiding unavailable
    /// macOS title bar buttons and theming the title bar
    /// </summary>
    internal static class WindowChromeService
    {
        private const ulong NSWindowStyleMaskMiniaturizable = 1UL << 2;
        private const ulong NSWindowStyleMaskResizable = 1UL << 3;
        private const long NSWindowMiniaturizeButton = 1;
        private const long NSWindowZoomButton = 2;

        /// <summary>
        /// Apply window chrome to the given window, both immediately and once again after the
        /// window has loaded (since the native handle may not be ready on the first pass)
        /// </summary>
        public static void Apply(Window window, bool hideMinimizeButton = false)
        {
            HideMacOSUnavailableWindowButtons(window, hideMinimizeButton);
            Dispatcher.UIThread.Post(() => HideMacOSUnavailableWindowButtons(window, hideMinimizeButton), DispatcherPriority.Loaded);
            ThemeService.ApplyWindowTitleBarTheme(window);
        }

        #region macOS Window Buttons

        /// <summary>
        /// Adjust the native macOS window style mask to remove the minimize and/or resize
        /// affordances, then hide the corresponding traffic-light buttons
        /// </summary>
        private static void HideMacOSUnavailableWindowButtons(Window window, bool hideMinimizeButton)
        {
            if (!OperatingSystem.IsMacOS())
                return;

            IntPtr nsWindow = window.TryGetPlatformHandle()?.Handle ?? IntPtr.Zero;
            if (nsWindow == IntPtr.Zero)
                return;

            ulong styleMask = objc_msgSend_ulong(nsWindow, sel_registerName("styleMask"));
            if (hideMinimizeButton)
                styleMask &= ~NSWindowStyleMaskMiniaturizable;

            if (!window.CanResize)
                styleMask &= ~NSWindowStyleMaskResizable;

            objc_msgSend_ulong(nsWindow, sel_registerName("setStyleMask:"), styleMask);

            if (hideMinimizeButton)
                HideMacOSWindowButton(nsWindow, NSWindowMiniaturizeButton);

            if (!window.CanResize)
                HideMacOSWindowButton(nsWindow, NSWindowZoomButton);
        }

        /// <summary>
        /// Hide and disable a single standard macOS window button (traffic-light control)
        /// </summary>
        private static void HideMacOSWindowButton(IntPtr nsWindow, long button)
        {
            IntPtr windowButton = objc_msgSend_IntPtr_Int64(nsWindow, sel_registerName("standardWindowButton:"), button);
            if (windowButton == IntPtr.Zero)
                return;

            objc_msgSend_double(windowButton, sel_registerName("setAlphaValue:"), 0);
            objc_msgSend_bool(windowButton, sel_registerName("setHidden:"), true);
            objc_msgSend_bool(windowButton, sel_registerName("setEnabled:"), false);
        }

        #endregion

        #region Native Interop

        [DllImport("/usr/lib/libobjc.A.dylib")]
        private static extern IntPtr sel_registerName(string name);

        [DllImport("/usr/lib/libobjc.A.dylib", EntryPoint = "objc_msgSend")]
        private static extern IntPtr objc_msgSend_IntPtr_Int64(IntPtr receiver, IntPtr selector, long argument);

        [DllImport("/usr/lib/libobjc.A.dylib", EntryPoint = "objc_msgSend")]
        private static extern void objc_msgSend_bool(IntPtr receiver, IntPtr selector, bool argument);

        [DllImport("/usr/lib/libobjc.A.dylib", EntryPoint = "objc_msgSend")]
        private static extern void objc_msgSend_double(IntPtr receiver, IntPtr selector, double argument);

        [DllImport("/usr/lib/libobjc.A.dylib", EntryPoint = "objc_msgSend")]
        private static extern ulong objc_msgSend_ulong(IntPtr receiver, IntPtr selector);

        [DllImport("/usr/lib/libobjc.A.dylib", EntryPoint = "objc_msgSend")]
        private static extern void objc_msgSend_ulong(IntPtr receiver, IntPtr selector, ulong argument);

        #endregion
    }
}
