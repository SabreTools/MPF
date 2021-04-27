using System.Windows;

namespace MPF.Windows
{
    /// <summary>
    /// Interaction logic for RingCodeGuideWindow.xaml
    /// </summary>
    public partial class RingCodeGuideWindow : Window
    {
        public RingCodeGuideWindow()
        {
            InitializeComponent();
        }

        #region Event Handlers

        /// <summary>
        /// Handler for CloseButton Click event
        /// </summary>
        private void CloseButtonClick(object sender, RoutedEventArgs e)
        {
            Close();
        }

        /// <summary>
        /// Handler for MinimizeButton Click event
        /// </summary>
        private void MinimizeButtonClick(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }

        #endregion
    }
}
