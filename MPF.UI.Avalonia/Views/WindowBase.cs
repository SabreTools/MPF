using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;

namespace MPF.UI.Avalonia.Views
{
    /// <summary>
    /// Shared base window providing the custom-chrome behaviour the WPF MainWindow relied on:
    /// title-bar drag, minimize, and close.
    /// </summary>
    /// <remarks>
    /// The WPF <c>WindowBase</c> also exposed a synchronous <c>DisplayUserMessage</c> helper backed
    /// by <c>WPFCustomMessageBox</c>. In Avalonia that responsibility lives in
    /// <see cref="MPF.UI.Avalonia.Services.DialogService"/> (which bridges the synchronous
    /// <c>Func&lt;string,string,int,bool,bool?&gt;</c> delegate to the async message box), so it is
    /// intentionally NOT duplicated here. WPF's <c>DialogResult</c> property has no Avalonia
    /// equivalent on a non-modal window, so <see cref="CloseButtonClick"/> simply closes.
    /// </remarks>
    public class WindowBase : Window
    {
        #region Event Handlers

        /// <summary>
        /// Handler for CloseButton Click event
        /// </summary>
        public void CloseButtonClick(object? sender, RoutedEventArgs e)
            => Close();

        /// <summary>
        /// Handler for MinimizeButton Click event
        /// </summary>
        public void MinimizeButtonClick(object? sender, RoutedEventArgs e)
            => WindowState = WindowState.Minimized;

        /// <summary>
        /// Handler for Title pointer-press event (replaces WPF TitleMouseDown / DragMove)
        /// </summary>
        public void TitleMouseDown(object? sender, PointerPressedEventArgs e)
        {
            if (e.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
                BeginMoveDrag(e);
        }

        #endregion
    }
}
