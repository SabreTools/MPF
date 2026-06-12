using System;
using System.Linq;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Threading;
using Avalonia.VisualTree;
using MPF.Avalonia.Services;

namespace MPF.Avalonia.Windows
{
    /// <summary>
    /// Base window that applies shared chrome and platform layout and exposes user message helpers
    /// </summary>
    public class WindowBase : Window
    {
        public WindowBase()
        {
            Opened += (_, _) =>
            {
                MacOSWindowChromeService.Apply(this);
                ThemeService.ApplyWindowTitleBarTheme(this);
                ApplyPlatformLayout();
            };
        }

        /// <summary>
        /// Apply non-Windows layout tweaks, nudging tab controls down to clear the title bar
        /// </summary>
        private void ApplyPlatformLayout()
        {
            if (OperatingSystem.IsWindows())
                return;

            foreach (TabControl tabControl in this.GetVisualDescendants().OfType<TabControl>())
            {
                Thickness margin = tabControl.Margin;
                tabControl.Margin = new Thickness(margin.Left, margin.Top + 4, margin.Right, margin.Bottom);
            }
        }

        /// <summary>
        /// Look up a localized string resource by key, falling back to the given default
        /// </summary>
        protected string StringResource(string key, string fallback)
        {
            // If there is no current application, the resource cannot be resolved
            if (global::Avalonia.Application.Current is not { } application)
                return fallback;

            // If the key cannot be found in the application resources
            if (!application.TryFindResource(key, out object? value))
                return fallback;

            return value?.ToString() ?? fallback;
        }

        /// <summary>
        /// Display a user message using a MessageBoxWindow
        /// </summary>
        /// <param name="title">Title to display to the user</param>
        /// <param name="message">Message to display to the user</param>
        /// <param name="optionCount">Number of options to display</param>
        /// <param name="flag">true for inquiry, false otherwise</param>
        /// <returns>true for positive, false for negative, null for neutral</returns>
        protected async Task<bool?> DisplayUserMessageAsync(string title, string message, int optionCount, bool flag)
        {
            if (Dispatcher.UIThread.CheckAccess())
                return await MessageBoxWindow.ShowAsyncResult(this, title, message, optionCount, flag);

            return await Dispatcher.UIThread.InvokeAsync(() => MessageBoxWindow.ShowAsyncResult(this, title, message, optionCount, flag));
        }

        /// <summary>
        /// Display a user message using a MessageBoxWindow, blocking until the dialog is dismissed
        /// </summary>
        /// <param name="title">Title to display to the user</param>
        /// <param name="message">Message to display to the user</param>
        /// <param name="optionCount">Number of options to display</param>
        /// <param name="flag">true for inquiry, false otherwise</param>
        /// <returns>true for positive, false for negative, null for neutral</returns>
        protected bool? DisplayUserMessage(string title, string message, int optionCount, bool flag)
        {
            if (Dispatcher.UIThread.CheckAccess())
            {
                Task<bool?> dialogTask = MessageBoxWindow.ShowAsyncResult(this, title, message, optionCount, flag);
                var frame = new DispatcherFrame();
                dialogTask.ContinueWith(_ => Dispatcher.UIThread.Post(() => frame.Continue = false));
                Dispatcher.UIThread.PushFrame(frame);
                return dialogTask.GetAwaiter().GetResult();
            }

            return Dispatcher.UIThread
                .InvokeAsync(() => MessageBoxWindow.ShowAsyncResult(this, title, message, optionCount, flag))
                .GetAwaiter()
                .GetResult();
        }
    }
}
