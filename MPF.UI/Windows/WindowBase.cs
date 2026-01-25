using System.Windows;
using System.Windows.Input;
using WPFCustomMessageBox;

namespace MPF.UI.Windows
{
    public class WindowBase : Window
    {
        #region Event Handlers

        /// <summary>
        /// Handler for CloseButton Click event
        /// </summary>
        public void CloseButtonClick(object sender, RoutedEventArgs e)
        {
            try
            {
                DialogResult = false;
            }
            catch { }

            Close();
        }

        /// <summary>
        /// Handler for MinimizeButton Click event
        /// </summary>
        public void MinimizeButtonClick(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }

        /// <summary>
        /// Handler for Title MouseDown event
        /// </summary>
        public void TitleMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
                DragMove();
        }

        #endregion

        /// <summary>
        /// Display a user message using a CustomMessageBox
        /// </summary>
        /// <param name="title">Title to display to the user</param>
        /// <param name="message">Message to display to the user</param>
        /// <param name="optionCount">Number of options to display</param>
        /// <param name="flag">true for inquiry, false otherwise</param>
        /// <returns>true for positive, false for negative, null for neutral</returns>
        public bool? DisplayUserMessage(string title, string message, int optionCount, bool flag)
        {
            // Set the correct button style
            var button = optionCount switch
            {
                1 => MessageBoxButton.OK,
                2 => MessageBoxButton.YesNo,

                // This should not happen, but default to "OK"
                _ => MessageBoxButton.OK,
            };

            // Set the correct icon
            MessageBoxImage image = flag ? MessageBoxImage.Question : MessageBoxImage.Exclamation;

            // Display and get the result
            MessageBoxResult result = CustomMessageBox.Show(this, message, title, button, image);

#pragma warning disable IDE0072
            return result switch
            {
                MessageBoxResult.OK or MessageBoxResult.Yes => true,
                MessageBoxResult.No => false,
                _ => null,
            };
#pragma warning restore IDE0072
        }
    }
}
