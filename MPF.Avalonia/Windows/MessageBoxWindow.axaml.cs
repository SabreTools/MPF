using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Layout;
using MPF.Avalonia.Services;

namespace MPF.Avalonia.Windows
{
    /// <summary>
    /// Simple modal message box with one or two buttons, returning the user's choice
    /// </summary>
    public partial class MessageBoxWindow : Window
    {
        public MessageBoxWindow()
        {
            InitializeComponent();
            Opened += (_, _) => WindowChromeService.Apply(this, hideMinimizeButton: true);
        }

        /// <summary>
        /// Populate the window title, message text, and the appropriate set of buttons
        /// </summary>
        private void Configure(string title, string message, int optionCount)
        {
            Title = title;
            this.FindControl<TextBlock>("MessageTextBlock")!.Text = message;
            StackPanel buttonPanel = this.FindControl<StackPanel>("ButtonPanel")!;
            buttonPanel.Children.Clear();

            buttonPanel.HorizontalAlignment = HorizontalAlignment.Center;

            Button button = CreateButton(optionCount > 1 ? "Yes" : "OK", true);
            buttonPanel.Children.Add(button);

            if (optionCount > 1)
                buttonPanel.Children.Add(CreateButton("No", false));
        }

        /// <summary>
        /// Create, configure, and show the message box as a modal dialog
        /// </summary>
        public static Task ShowAsync(Window owner, string title, string message, int optionCount, bool flag)
        {
            var window = new MessageBoxWindow();
            window.Configure(title, message, optionCount);
            return window.ShowDialog(owner);
        }

        /// <summary>
        /// Create, configure, and show the message box as a modal dialog, returning the user's choice
        /// </summary>
        /// <returns>true for positive, false for negative, null for neutral</returns>
        public static Task<bool?> ShowAsyncResult(Window owner, string title, string message, int optionCount, bool flag)
        {
            var window = new MessageBoxWindow();
            window.Configure(title, message, optionCount);
            return window.ShowDialog<bool?>(owner);
        }

        /// <summary>
        /// Create a button that closes the dialog with the given result when clicked
        /// </summary>
        private Button CreateButton(string content, bool result)
        {
            var button = new Button
            {
                Content = content,
                MinWidth = 96,
                HorizontalAlignment = HorizontalAlignment.Center,
                HorizontalContentAlignment = global::Avalonia.Layout.HorizontalAlignment.Center,
                VerticalContentAlignment = global::Avalonia.Layout.VerticalAlignment.Center,
            };
            button.Click += (_, _) => Close(result);
            return button;
        }
    }
}
