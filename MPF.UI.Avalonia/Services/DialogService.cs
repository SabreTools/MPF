// MPF cross-platform (Avalonia) UI — contributed by Knutwurst (https://github.com/knutwurst)
using System;
using Avalonia.Controls;
using Avalonia.Threading;
using MPF.UI.Avalonia.Views;

namespace MPF.UI.Avalonia.Services
{
    /// <summary>
    /// Bridges the synchronous <c>Func&lt;string,string,int,bool,bool?&gt;</c> delegate
    /// expected by <c>MainViewModel.Init</c> and the async Avalonia dialog API.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The ViewModel calls <c>_displayUserMessage</c> from a background (dumping) thread,
    /// which is the SAFE path: <c>Dispatcher.UIThread.InvokeAsync</c> queues work on the UI
    /// thread, and <c>.GetAwaiter().GetResult()</c> blocks the calling background thread until
    /// the user responds.
    /// </para>
    /// <para>
    /// KNOWN CAVEAT: if <c>DisplayUserMessage</c> is ever called from the UI thread itself,
    /// <c>.GetAwaiter().GetResult()</c> will deadlock because the UI thread is blocked waiting
    /// for a task that must itself run on the UI thread. This scenario does not arise in the
    /// current call path: all invocations from MainViewModel happen during a dump, i.e. from
    /// a background thread (verified when MainWindow was wired up). Any future caller that
    /// invokes this on the UI thread must use an async path instead.
    /// </para>
    /// </remarks>
    public sealed class DialogService(Func<Window?> ownerProvider)
    {
        /// <summary>
        /// Display a modal message box synchronously from any thread.
        /// </summary>
        /// <param name="title">Window title.</param>
        /// <param name="message">Body text.</param>
        /// <param name="optionCount">Number of default options (1 = OK, 2 = OK+Cancel or Yes+No).</param>
        /// <param name="isInquiry">true for inquiry-style (Yes/No); false for notification-style.</param>
        /// <returns>true for positive, false for negative, null for neutral/cancel.</returns>
        public bool? DisplayUserMessage(string title, string message, int optionCount, bool isInquiry)
        {
            var buttons = isInquiry
                ? MessageBoxButtons.YesNo
                : optionCount >= 2 ? MessageBoxButtons.OkCancel : MessageBoxButtons.Ok;

            return Dispatcher.UIThread.InvokeAsync(async () =>
            {
                var owner = ownerProvider();
                if (owner is null)
                    return (bool?)null;
                return await MessageBoxWindow.ShowAsync(owner, title, message, buttons);
            }).GetAwaiter().GetResult();
        }
    }
}
