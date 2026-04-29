using Avalonia.Controls;
using Avalonia.Threading;

namespace MPF.Avalonia.Windows
{
    public class WindowBase : Window
    {
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
