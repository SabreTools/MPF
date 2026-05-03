using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Layout;
using MPF.Avalonia.Services;

namespace MPF.Avalonia.Windows
{
    public partial class MessageBoxWindow : Window
    {
        public MessageBoxWindow()
        {
            InitializeComponent();
            Opened += (_, _) => WindowChromeService.Apply(this, hideMinimizeButton: true);
        }

        private void Configure(string title, string message, int optionCount)
        {
            Title = title;
            this.FindControl<TextBlock>("MessageTextBlock")!.Text = message;
            StackPanel buttonPanel = this.FindControl<StackPanel>("ButtonPanel")!;
            buttonPanel.Children.Clear();

            buttonPanel.HorizontalAlignment = HorizontalAlignment.Center;

            Button button = CreateButton("OK", true);
            buttonPanel.Children.Add(button);

            if (optionCount > 1)
                buttonPanel.Children.Add(CreateButton("Cancel", false));
        }

        public static Task ShowAsync(Window owner, string title, string message, int optionCount, bool flag)
        {
            var window = new MessageBoxWindow();
            window.Configure(title, message, optionCount);
            return window.ShowDialog(owner);
        }

        public static Task<bool?> ShowAsyncResult(Window owner, string title, string message, int optionCount, bool flag)
        {
            var window = new MessageBoxWindow();
            window.Configure(title, message, optionCount);
            return window.ShowDialog<bool?>(owner);
        }

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
