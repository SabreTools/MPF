using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace MPF.UI.Avalonia.Views
{
    /// <summary>
    /// The set of button combinations that can be shown in a MessageBoxWindow.
    /// </summary>
    public enum MessageBoxButtons
    {
        /// <summary>Single OK button — returns true on click.</summary>
        Ok,

        /// <summary>OK and Cancel buttons — OK returns true, Cancel returns null.</summary>
        OkCancel,

        /// <summary>Yes and No buttons — Yes returns true, No returns false.</summary>
        YesNo,
    }

    /// <summary>
    /// A simple cross-platform replacement for WPF's CustomMessageBox / MessageBox.
    /// </summary>
    public partial class MessageBoxWindow : Window
    {
        private bool? _result;

        public MessageBoxWindow()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Show a modal message box and return the user's choice.
        /// </summary>
        /// <param name="owner">The owning window (required for modal ShowDialog).</param>
        /// <param name="title">Window title.</param>
        /// <param name="message">Body text to display.</param>
        /// <param name="buttons">Which button set to display.</param>
        /// <returns>true for positive (OK/Yes), false for negative (No), null for neutral (Cancel/close).</returns>
        public static async Task<bool?> ShowAsync(Window owner, string title, string message, MessageBoxButtons buttons)
        {
            var win = new MessageBoxWindow
            {
                Title = title,
            };

            var msgText = win.FindControl<TextBlock>("MessageText")!;
            msgText.Text = message;

            var panel = win.FindControl<StackPanel>("ButtonPanel")!;

            switch (buttons)
            {
                case MessageBoxButtons.Ok:
                    panel.Children.Add(MakeButton("OK", win, true));
                    break;

                case MessageBoxButtons.OkCancel:
                    panel.Children.Add(MakeButton("OK", win, true));
                    panel.Children.Add(MakeButton("Cancel", win, null));
                    break;

                case MessageBoxButtons.YesNo:
                    panel.Children.Add(MakeButton("Yes", win, true));
                    panel.Children.Add(MakeButton("No", win, false));
                    break;
            }

            await win.ShowDialog(owner);
            return win._result;
        }

        /// <summary>
        /// Create a button that sets <paramref name="win"/>._result and closes the window.
        /// </summary>
        private static Button MakeButton(string label, MessageBoxWindow win, bool? result)
        {
            var btn = new Button { Content = label };
            btn.Click += (_, _) =>
            {
                win._result = result;
                win.Close();
            };
            return btn;
        }
    }
}
