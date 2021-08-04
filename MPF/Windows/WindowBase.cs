using System.Windows;
using System.Windows.Input;

namespace MPF.Windows
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
                this.DialogResult = false;
            }
            catch { }
            
            Close();
        }

        /// <summary>
        /// Handler for MinimizeButton Click event
        /// </summary>
        public void MinimizeButtonClick(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }

        /// <summary>
        /// Handler for Title MouseDown event
        /// </summary>
        public void TitleMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
                this.DragMove();
        }

        #endregion
    }
}
