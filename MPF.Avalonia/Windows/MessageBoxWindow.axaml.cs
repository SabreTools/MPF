using System.Threading.Tasks;
using Avalonia.Controls;

namespace MPF.Avalonia.Windows
{
    public partial class MessageBoxWindow : Window
    {
        public MessageBoxWindow()
            => InitializeComponent();

        private void Configure(string title, string message, int optionCount)
        {
            Title = title;
            this.FindControl<TextBlock>("MessageTextBlock")!.Text = message;
            this.FindControl<StackPanel>("ButtonPanel")!.Children.Clear();

            Button button = CreateButton("OK", true);
            this.FindControl<StackPanel>("ButtonPanel")!.Children.Add(button);

            if (optionCount > 1)
                this.FindControl<StackPanel>("ButtonPanel")!.Children.Add(CreateButton("Cancel", false));
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
            var button = new Button { Content = content, MinWidth = 80 };
            button.Click += (_, _) => Close(result);
            return button;
        }
    }
}
