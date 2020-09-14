using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using DICUI.Web;

namespace DICUI.Windows
{
    /// <summary>
    /// Interaction logic for DiscInformationWindow.xaml
    /// </summary>
    public partial class DiscInformationWindow : Window
    {
        #region Fields

        /// <summary>
        /// List of available disc categories
        /// </summary>
        public List<CategoryComboBoxItem> Categories { get; private set; }

        /// <summary>
        /// SubmissionInfo object to fill and save
        /// </summary>
        public SubmissionInfo SubmissionInfo { get; set; }

        /// <summary>
        /// List of available regions
        /// </summary>
        public List<RegionComboBoxItem> Regions { get; private set; }

        /// <summary>
        /// List of available languages
        /// </summary>
        public List<LanguageComboBoxItem> Languages { get; private set; }

        #endregion

        public DiscInformationWindow()
        {
            InitializeComponent();

            PopulateCategories();
            PopulateRegions();
            PopulateLanguages();
            DisableFieldsIfNeeded();
        }

        /// <summary>
        /// Disable fields that aren't applicable to the current disc
        /// </summary>
        private void DisableFieldsIfNeeded()
        {
            // Only disable for single-layer discs
            if (SubmissionInfo.SizeAndChecksums.Layerbreak == default(long))
            {
                L1MasteringRingTextBox.IsEnabled = false;
                L1MasteringRingTextBox.Background = Brushes.Gray;

                L1MasteringSIDTextBox.IsEnabled = false;
                L1MasteringSIDTextBox.Background = Brushes.Gray;

                L1ToolstampTextBox.IsEnabled = false;
                L1ToolstampTextBox.Background = Brushes.Gray;

                L1MouldSIDTextBox.IsEnabled = false;
                L1MouldSIDTextBox.Background = Brushes.Gray;

                L1AdditionalMouldTextBox.IsEnabled = false;
                L1AdditionalMouldTextBox.Background = Brushes.Gray;
            }
        }

        /// <summary>
        /// Load the current contents of the base SubmissionInfo to the UI
        /// </summary>
        public void Load()
        {
            TitleTextBox.Text = SubmissionInfo.CommonDiscInfo.Title ?? "";
            ForeignTitleTextBox.Text = SubmissionInfo.CommonDiscInfo.ForeignTitleNonLatin ?? "";
            DiscNumberLetterTextBox.Text = SubmissionInfo.CommonDiscInfo.DiscNumberLetter ?? "";
            DiscTitleTextBox.Text = SubmissionInfo.CommonDiscInfo.DiscTitle ?? "";
            CategoryComboBox.SelectedIndex = Categories.FindIndex(r => r == SubmissionInfo.CommonDiscInfo.Category);
            RegionComboBox.SelectedIndex = Regions.FindIndex(r => r == SubmissionInfo.CommonDiscInfo.Region);
            if (SubmissionInfo.CommonDiscInfo.Languages != null)
            {
                foreach (var language in SubmissionInfo.CommonDiscInfo.Languages)
                    Languages.Find(l => l == language).IsChecked = true;
            }
            SerialTextBox.Text = SubmissionInfo.CommonDiscInfo.Serial ?? "";
            L0MasteringRingTextBox.Text = SubmissionInfo.CommonDiscInfo.MasteringRingFirstLayerDataSide ?? "";
            L0MasteringSIDTextBox.Text = SubmissionInfo.CommonDiscInfo.MasteringSIDCodeFirstLayerDataSide ?? "";
            L0ToolstampTextBox.Text = SubmissionInfo.CommonDiscInfo.ToolstampMasteringCodeFirstLayerDataSide ?? "";
            L0MouldSIDTextBox.Text = SubmissionInfo.CommonDiscInfo.MouldSIDCodeFirstLayerDataSide ?? "";
            L0AdditionalMouldTextBox.Text = SubmissionInfo.CommonDiscInfo.AdditionalMouldFirstLayerDataSide ?? "";
            L1MasteringRingTextBox.Text = SubmissionInfo.CommonDiscInfo.MasteringRingSecondLayerLabelSide ?? "";
            L1MasteringSIDTextBox.Text = SubmissionInfo.CommonDiscInfo.MasteringSIDCodeSecondLayerLabelSide ?? "";
            L1ToolstampTextBox.Text = SubmissionInfo.CommonDiscInfo.ToolstampMasteringCodeSecondLayerLabelSide ?? "";
            L1MouldSIDTextBox.Text = SubmissionInfo.CommonDiscInfo.MouldSIDCodeSecondLayerLabelSide ?? "";
            L1AdditionalMouldTextBox.Text = SubmissionInfo.CommonDiscInfo.AdditionalMouldSecondLayerLabelSide ?? "";
            BarcodeTextBox.Text = SubmissionInfo.CommonDiscInfo.Barcode ?? "";
            CommentsTextBox.Text = SubmissionInfo.CommonDiscInfo.Comments ?? "";
            ContentsTextBox.Text = SubmissionInfo.CommonDiscInfo.Contents ?? "";

            VersionTextBox.Text = SubmissionInfo.VersionAndEditions.Version ?? "";
            EditionTextBox.Text = SubmissionInfo.VersionAndEditions.OtherEditions ?? "";
        }

        /// <summary>
        /// Get a complete list of categories and fill the combo box
        /// </summary>
        private void PopulateCategories()
        {
            var categories = Enum.GetValues(typeof(DiscCategory)).OfType<DiscCategory?>().ToList();

            ViewModels.LoggerViewModel.VerboseLogLn("Populating categories, {0} categories found.", categories.Count);

            Categories = new List<CategoryComboBoxItem>();
            foreach (var category in categories)
            {
                Categories.Add(new CategoryComboBoxItem(category));
            }

            CategoryComboBox.ItemsSource = Categories;
            CategoryComboBox.SelectedIndex = 0;
        }

        /// <summary>
        /// Get a complete list of languages and fill the combo box
        /// </summary>
        private void PopulateLanguages()
        {
            var languages = Enum.GetValues(typeof(Language)).OfType<Language?>().ToList();

            ViewModels.LoggerViewModel.VerboseLogLn("Populating languages, {0} languages found.", languages.Count);

            Languages = new List<LanguageComboBoxItem>();
            foreach (var language in languages)
            {
                Languages.Add(new LanguageComboBoxItem(language));
            }

            LanguagesComboBox.ItemsSource = Languages;
            LanguagesComboBox.SelectedIndex = 0;
        }

        /// <summary>
        /// Get a complete list of regions and fill the combo box
        /// </summary>
        private void PopulateRegions()
        {
            var regions = Enum.GetValues(typeof(Region)).OfType<Region?>().ToList();

            ViewModels.LoggerViewModel.VerboseLogLn("Populating regions, {0} regions found.", regions.Count);

            Regions = new List<RegionComboBoxItem>();
            foreach (var region in regions)
            {
                Regions.Add(new RegionComboBoxItem(region));
            }

            RegionComboBox.ItemsSource = Regions;
            RegionComboBox.SelectedIndex = 0;
        }

        /// <summary>
        /// Save the current contents of the UI to the base SubmissionInfo
        /// </summary>
        private void Save()
        {
            SubmissionInfo.CommonDiscInfo.Title = TitleTextBox.Text ?? "";
            SubmissionInfo.CommonDiscInfo.ForeignTitleNonLatin = ForeignTitleTextBox.Text ?? "";
            SubmissionInfo.CommonDiscInfo.DiscNumberLetter = DiscNumberLetterTextBox.Text ?? "";
            SubmissionInfo.CommonDiscInfo.DiscTitle = DiscTitleTextBox.Text ?? "";
            SubmissionInfo.CommonDiscInfo.Category = (CategoryComboBox.SelectedItem as CategoryComboBoxItem)?.Value ?? DiscCategory.Games;
            SubmissionInfo.CommonDiscInfo.Region = (RegionComboBox.SelectedItem as RegionComboBoxItem)?.Value ?? Region.World;
            var languages = new List<Language?>();
            foreach (var language in Languages)
            {
                if (language.IsChecked)
                    languages.Add(language.Value);
            }
            if (languages.Count == 0)
                languages.Add(null);
            SubmissionInfo.CommonDiscInfo.Languages = languages.ToArray();
            SubmissionInfo.CommonDiscInfo.Serial = SerialTextBox.Text ?? "";
            SubmissionInfo.CommonDiscInfo.MasteringRingFirstLayerDataSide = L0MasteringRingTextBox.Text ?? "";
            SubmissionInfo.CommonDiscInfo.MasteringSIDCodeFirstLayerDataSide = L0MasteringSIDTextBox.Text ?? "";
            SubmissionInfo.CommonDiscInfo.ToolstampMasteringCodeFirstLayerDataSide = L0ToolstampTextBox.Text ?? "";
            SubmissionInfo.CommonDiscInfo.MouldSIDCodeFirstLayerDataSide = L0MouldSIDTextBox.Text ?? "";
            SubmissionInfo.CommonDiscInfo.AdditionalMouldFirstLayerDataSide = L0AdditionalMouldTextBox.Text ?? "";
            SubmissionInfo.CommonDiscInfo.MasteringRingSecondLayerLabelSide = L1MasteringRingTextBox.Text ?? "";
            SubmissionInfo.CommonDiscInfo.MasteringSIDCodeSecondLayerLabelSide = L1MasteringSIDTextBox.Text ?? "";
            SubmissionInfo.CommonDiscInfo.ToolstampMasteringCodeSecondLayerLabelSide = L1ToolstampTextBox.Text ?? "";
            SubmissionInfo.CommonDiscInfo.MouldSIDCodeSecondLayerLabelSide = L1MouldSIDTextBox.Text ?? "";
            SubmissionInfo.CommonDiscInfo.AdditionalMouldSecondLayerLabelSide = L1AdditionalMouldTextBox.Text ?? "";
            SubmissionInfo.CommonDiscInfo.Barcode = BarcodeTextBox.Text ?? "";
            SubmissionInfo.CommonDiscInfo.Comments = CommentsTextBox.Text ?? "";
            SubmissionInfo.CommonDiscInfo.Contents = ContentsTextBox.Text ?? "";

            SubmissionInfo.VersionAndEditions.Version = VersionTextBox.Text ?? "";
            SubmissionInfo.VersionAndEditions.OtherEditions = EditionTextBox.Text ?? "";
        }

        #region Event Handlers

        /// <summary>
        /// Handler for AcceptButton Click event
        /// </summary>
        private void OnAcceptClick(object sender, RoutedEventArgs e)
        {
            Save();
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
        /// Handler for DiscInformationWindow Closed event
        /// </summary>
        private void OnClosed(object sender, EventArgs e)
        {
            Hide();
        }

        #endregion
    }
}
