using Avalonia.Controls;
using Avalonia.Threading;
using MPF.Avalonia.Services;

namespace MPF.Avalonia.Windows
{
    public class WindowBase : Window
    {
        public WindowBase()
        {
            Opened += (_, _) => WindowChromeService.Apply(this);
        }

        protected string StringResource(string key, string fallback)
            => global::Avalonia.Application.Current?.TryFindResource(key, out object? value) == true ? value?.ToString() ?? fallback : fallback;

        protected bool? DisplayUserMessage(string title, string message, int optionCount, bool flag)
        {
            if (Dispatcher.UIThread.CheckAccess())
            {
                _ = MessageBoxWindow.ShowAsync(this, title, message, optionCount, flag);

                if (optionCount <= 1)
                    return true;

                return null;
            }

            return Dispatcher.UIThread
                .InvokeAsync(() => MessageBoxWindow.ShowAsyncResult(this, title, message, optionCount, flag))
                .GetAwaiter()
                .GetResult();
        }
    }
}
