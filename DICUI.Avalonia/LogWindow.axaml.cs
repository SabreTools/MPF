using System.IO;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Media;

namespace DICUI.Avalonia
{
    public class LogWindow : Window
    {
        public LogWindow()
        {
            this.InitializeComponent();
#if DEBUG
            this.AttachDevTools();
#endif
        }

        /// <summary>
        /// Adjust the position of the log window if the main window is moved
        /// </summary>
        public void AdjustPositionToMainWindow()
        {
            var owner = Owner as Window;
            Position = new PixelPoint(
                owner.Position.X,
                owner.Position.Y + (int)owner.Height + Constants.LogWindowMarginFromMainWindow);
        }

        /// <summary>
        /// Append rich text to the scrolling element
        /// </summary>
        /// <param name="text">Text to add, including newlines</param>
        /// <param name="color">Color to format the text</param>
        public void AppendToTextBox(string text, ISolidColorBrush color)
        {
            // TODO: Use brush color
            this.Find<TextBox>("Output").Text += $"{text}\n";
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }

        #region Event Handlers

        /// <summary>
        /// Handler for ClearButton Click event
        /// </summary>
        private void OnClearButton(object sender, RoutedEventArgs e)
        {
            this.Find<TextBox>("Output").Text = string.Empty;
        }

        /// <summary>
        /// Handler for HideButton Click event
        /// </summary>
        private void OnHideButton(object sender, RoutedEventArgs e)
        {
            ViewModels.LoggerViewModel.WindowVisible = false;

            //TODO: this should be bound directly to WindowVisible property in two way fashion
            // we need to study how to properly do it in XAML
            (Owner as MainWindow).Find<CheckBox>("ShowLogCheckBox").IsChecked = false;
        }

        /// <summary>
        /// Handler for SaveButton Click event
        /// </summary>
        private void OnSaveButton(object sender, RoutedEventArgs e)
        {
            using (StreamWriter tw = new StreamWriter(File.OpenWrite("console.log")))
            {
                tw.Write(this.Find<TextBox>("Output").Text);
            }
        }

        #endregion
    }
}
