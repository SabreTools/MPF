using System;
using System.Windows;
using MPF.Core.Data;
using MPF.UI.Core.ViewModels;
using WPFCustomMessageBox;

namespace MPF.UI.Core.Windows
{
    /// <summary>
    /// Interaction logic for OptionsWindow.xaml
    /// </summary>
    public partial class OptionsWindow : WindowBase
    {
        /// <summary>
        /// Read-only access to the current options view model
        /// </summary>
        public OptionsViewModel OptionsViewModel => DataContext as OptionsViewModel;

        /// <summary>
        /// Constructor
        /// </summary>
        public OptionsWindow(Options options)
        {
            InitializeComponent();
            DataContext = new OptionsViewModel(options);

            // Set initial value for binding
            RedumpPasswordBox.Password = options.RedumpPassword;

            // Add handlers
            AaruPathButton.Click += BrowseForPathClick;
            DiscImageCreatorPathButton.Click += BrowseForPathClick;
            DefaultOutputPathButton.Click += BrowseForPathClick;

            AcceptButton.Click += OnAcceptClick;
            CancelButton.Click += OnCancelClick;
            RedumpPasswordBox.PasswordChanged += OnPasswordChanged;
            RedumpLoginTestButton.Click += OnRedumpTestClick;
        }

        #region Event Handlers

        /// <summary>
        /// Handler for generic Click event
        /// </summary>
        private void BrowseForPathClick(object sender, EventArgs e) =>
            OptionsViewModel.BrowseForPath(this, sender as System.Windows.Controls.Button);

        /// <summary>
        /// Handler for AcceptButton Click event
        /// </summary>
        private void OnAcceptClick(object sender, EventArgs e)
        {
            OptionsViewModel.SavedSettings = true;
            Close();
        }

        /// <summary>
        /// Handler for CancelButtom Click event
        /// </summary>
        private void OnCancelClick(object sender, EventArgs e)
        {
            OptionsViewModel.SavedSettings = false;
            Close();
        }

        /// <summary>
        /// Handler for 
        /// </summary>
        private void OnPasswordChanged(object sender, EventArgs e)
        {
            OptionsViewModel.Options.RedumpPassword = RedumpPasswordBox.Password;
        }

        /// <summary>
        /// Test Redump credentials for validity
        /// </summary>
#if NET48
        private void OnRedumpTestClick(object sender, EventArgs e)
#else
        private async void OnRedumpTestClick(object sender, EventArgs e)
#endif
        {
#if NET48
            (bool? success, string message) = OptionsViewModel.TestRedumpLogin(this, RedumpUsernameTextBox.Text, RedumpPasswordBox.Password);
#else
            (bool? success, string message) = await OptionsViewModel.TestRedumpLogin(this, RedumpUsernameTextBox.Text, RedumpPasswordBox.Password);
#endif

            if (success == true)
                CustomMessageBox.Show(this, message, "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            else if (success == false)
                CustomMessageBox.Show(this, message, "Failure", MessageBoxButton.OK, MessageBoxImage.Error);
            else
                CustomMessageBox.Show(this, message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }

        #endregion
    }
}
