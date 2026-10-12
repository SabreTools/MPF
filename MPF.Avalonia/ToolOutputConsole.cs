using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Platform;
using MPF.Avalonia.Windows;
using MPF.Frontend;
using MPF.Frontend.Tools;

namespace MPF.Avalonia
{
    /// <summary>
    /// Bridges the frontend's <see cref="IToolOutputConsole"/> seam to an Avalonia
    /// <see cref="ToolOutputWindow"/>.
    /// </summary>
    public sealed class ToolOutputConsole : IToolOutputConsole
    {
        #region Constants

        /// <summary>
        /// Gap between the main window and the tool-output window, in device-independent pixels
        /// </summary>
        private const double PlacementGap = 8;

        #endregion

        #region Private Fields

        /// <summary>
        /// Console window parent
        /// </summary>
        private readonly Window _owner;

        /// <summary>
        /// Frontend options for output
        /// </summary>
        private readonly Options _options;

        /// <summary>
        /// Generated tool window, if available
        /// </summary>
        private ToolOutputWindow? _window;

        #endregion

        public ToolOutputConsole(Window owner, Options options)
        {
            _owner = owner;
            _options = options;
        }

        /// <inheritdoc/>
        public void Open()
        {
            // Discard any leftover console from a previous run (kept open because the user
            // turned off auto-close) so each dump starts with a clean console.
            _window?.Close();
            _window = null;

            var window = new ToolOutputWindow { AutoClose = _options.GUI.ToolConsoleAutoClose };
            window.AutoCloseChanged += OnAutoCloseChanged;
            window.Closed += (_, _) =>
            {
                if (ReferenceEquals(_window, window))
                    _window = null;
            };
            _window = window;

            TryPositionBesideOwner(window);
            window.Show(_owner);
        }

        /// <inheritdoc/>
        public void Append(string chunk) => _window?.Append(chunk);

        /// <inheritdoc/>
        public void NotifyToolExited() => _window?.NotifyToolExited();

        #region Event Handlers

        /// <summary>
        /// Persist the user's auto-close preference whenever the checkbox is toggled
        /// </summary>
        private void OnAutoCloseChanged(bool autoClose)
        {
            _options.GUI.ToolConsoleAutoClose = autoClose;
            OptionsLoader.SaveToConfig(_options);
        }

        #endregion

        #region Helpers

        /// <summary>
        /// Place the console beside the main window on whichever side has room, falling back
        /// to overlaying the main window; always clamped fully inside the monitor work area.
        /// </summary>
        private void TryPositionBesideOwner(ToolOutputWindow window)
        {
            try
            {
                Screen? screen = _owner.Screens.ScreenFromWindow(_owner) ?? _owner.Screens.Primary;
                if (screen is null)
                {
                    window.WindowStartupLocation = WindowStartupLocation.CenterOwner;
                    return;
                }

                PixelRect area = screen.WorkingArea;

                // Owner frame and the new window, converted to physical pixels.
                Size ownerSize = _owner.FrameSize ?? _owner.Bounds.Size;
                PixelPoint ownerPos = _owner.Position;
                int ownerW = (int)Math.Round(ownerSize.Width * screen.Scaling);
                int winW = (int)Math.Round(window.Width * screen.Scaling);
                int winH = (int)Math.Round(window.Height * screen.Scaling);
                int gap = (int)Math.Round(PlacementGap * screen.Scaling);

                int rightRoom = area.X + area.Width - (ownerPos.X + ownerW);
                int leftRoom = ownerPos.X - area.X;

                int x;

                // Beside, to the right
                if (rightRoom >= winW + gap)
                    x = ownerPos.X + ownerW + gap;

                // Beside, to the left
                else if (leftRoom >= winW + gap)
                    x = ownerPos.X - gap - winW;

                // Overlay, centered on the owner
                else
                    x = ownerPos.X + ((ownerW - winW) / 2);

                // Align tops
                int y = ownerPos.Y;

                // Clamp the whole window inside the work area so it never lands off-screen.
                int maxX = area.X + area.Width - winW;
                int maxY = area.Y + area.Height - winH;
                x = Math.Max(area.X, Math.Min(x, maxX));
                y = Math.Max(area.Y, Math.Min(y, maxY));

                window.WindowStartupLocation = WindowStartupLocation.Manual;
                window.Position = new PixelPoint(x, y);
            }
            catch
            {
                // Any platform quirk in the geometry math -> just center on the owner.
                window.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            }
        }

        #endregion
    }
}
