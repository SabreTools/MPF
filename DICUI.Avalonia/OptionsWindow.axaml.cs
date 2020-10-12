using System;
using System.Collections.Generic;
using System.IO;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using DICUI.Data;
using DICUI.Utilities;
using DICUI.Web;

namespace DICUI.Avalonia
{
    public class OptionsWindow : Window
    {
        #region Fields

        /// <summary>
        /// Current UI options
        /// </summary>
        public UIOptions UIOptions { get; set; }

        /// <summary>
        /// List of available internal programs
        /// </summary>
        public List<InternalProgramComboBoxItem> InternalPrograms { get; private set; }

        #endregion

        public OptionsWindow()
        {
            this.InitializeComponent();
#if DEBUG
            this.AttachDevTools();
#endif

            PopulateInternalPrograms();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }

        #region Helpers

        /// <summary>
        /// Get a complete list of internal programs and fill the combo box
        /// </summary>
        private void PopulateInternalPrograms()
        {
            // We only support certain programs for dumping
            var internalPrograms = new List<InternalProgram> { InternalProgram.DiscImageCreator, InternalProgram.Aaru, InternalProgram.DD };
            ViewModels.LoggerViewModel.VerboseLogLn("Populating internal programs, {0} internal programs found.", internalPrograms.Count);

            InternalPrograms = new List<InternalProgramComboBoxItem>();
            foreach (var internalProgram in internalPrograms)
            {
                InternalPrograms.Add(new InternalProgramComboBoxItem(internalProgram));
            }

            this.Find<ComboBox>("InternalProgramComboBox").Items = InternalPrograms;
            this.Find<ComboBox>("InternalProgramComboBox").SelectedIndex = 0;
        }

        /// <summary>
        /// Refresh any options-related elements
        /// </summary>
        public void Refresh()
        {
            // Handle non-bindable fields
            this.Find<ComboBox>("InternalProgramComboBox").SelectedIndex = InternalPrograms.FindIndex(r => r == Converters.ToInternalProgram(UIOptions.Options.InternalProgram));
        }

        /// <summary>
        /// Find a TextBox by setting name
        /// </summary>
        /// <param name="name">Setting name to find</param>
        /// <returns>TextBox for that setting</returns>
        private TextBox TextBoxForPathSetting(string name)
        {
            return this.Find<TextBox>(name + "TextBox");
        }

        #endregion

        #region Event Handlers

        /// <summary>
        /// Handler for generic Click event
        /// </summary>
        private async void BrowseForPathClick(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;

            // strips button prefix to obtain the setting name
            string pathSettingName = button.Name.Substring(0, button.Name.IndexOf("Button"));

            // TODO: hack for now, then we'll see
            bool shouldBrowseForPath = pathSettingName == "DefaultOutputPath";

            // Directory
            if (shouldBrowseForPath)
            {
                OpenFolderDialog dialog = new OpenFolderDialog();
                string result = await dialog.ShowAsync(this);
                if (!string.IsNullOrWhiteSpace(result))
                {
                    if (Directory.Exists(result))
                        TextBoxForPathSetting(pathSettingName).Text = result;
                    else
                        Console.WriteLine($"Path '{result}' cannot be found!");
                }
            }

            // File
            else
            {
                OpenFileDialog dialog = new OpenFileDialog();
                string[] result = await dialog.ShowAsync(this);
                if (result != null && result.Length > 0 && !string.IsNullOrWhiteSpace(result[0]))
                {
                    if (File.Exists(result[0]))
                        TextBoxForPathSetting(pathSettingName).Text = result[0];
                    else
                        Console.WriteLine($"Path '{result[0]}' cannot be found!");
                }
            }            
        }

        /// <summary>
        /// Handler for AcceptButton Click event
        /// </summary>
        private void OnAcceptClick(object sender, RoutedEventArgs e)
        {
            // Handle non-bindable fields
            UIOptions.Options.InternalProgram = (this.Find<ComboBox>("InternalProgramComboBox").SelectedItem as InternalProgramComboBoxItem)?.Name ?? InternalProgram.DiscImageCreator.ToString();

            UIOptions.Save();
            Hide();
        }

        /// <summary>
        /// Handler for CancelButton Click event
        /// </summary>
        private void OnCancelClick(object sender, RoutedEventArgs e)
        {
            Hide();
        }

        /// <summary>
        /// Handler for RedumpLoginTestButton Click event
        /// </summary>
        private void OnRedumpTestClick(object sender, RoutedEventArgs e)
        {
            using (RedumpWebClient wc = new RedumpWebClient())
            {
                if (wc.Login(this.Find<TextBox>("RedumpUsernameTextBox").Text, this.Find<TextBox>("RedumpPasswordTextBox").Text))
                {
                    new Window()
                    {
                        Title = "Success",
                        Content = "Redump login credentials accepted!",
                        SizeToContent = SizeToContent.WidthAndHeight,
                        WindowStartupLocation = WindowStartupLocation.CenterScreen,
                    }.ShowDialog(this);
                }
                else
                {
                    new Window()
                    {
                        Title = "Failure",
                        Content = "Redump login credentials denied!",
                        SizeToContent = SizeToContent.WidthAndHeight,
                        WindowStartupLocation = WindowStartupLocation.CenterScreen,
                    }.ShowDialog(this);
                }
            }
        }

        #endregion
    }
}
