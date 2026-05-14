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
    public class WindowBase : Window
    {
        public WindowBase()
        {
            Opened += (_, _) =>
            {
                WindowChromeService.Apply(this);
                ApplyPlatformLayout();
            };
        }

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

        protected string StringResource(string key, string fallback)
            => global::Avalonia.Application.Current?.TryFindResource(key, out object? value) == true ? value?.ToString() ?? fallback : fallback;

        protected async Task<bool?> DisplayUserMessageAsync(string title, string message, int optionCount, bool flag)
        {
            if (Dispatcher.UIThread.CheckAccess())
                return await MessageBoxWindow.ShowAsyncResult(this, title, message, optionCount, flag);

            return await Dispatcher.UIThread.InvokeAsync(() => MessageBoxWindow.ShowAsyncResult(this, title, message, optionCount, flag));
        }

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
