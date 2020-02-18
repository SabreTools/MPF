using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using DICUI.Data;

namespace DICUI.Windows
{
    /// <summary>
    /// Interaction logic for DiscInformationWindow.xaml
    /// </summary>
    public partial class DiscInformationWindow : Window
    {
        private readonly MainWindow _mainWindow;
        private readonly SubmissionInfo _submissionInfo;

        private List<CategoryComboBoxItem> _categories;
        private List<RegionComboBoxItem> _regions;
        private List<LanguageComboBoxItem> _languages;

        public DiscInformationWindow(MainWindow mainWindow, SubmissionInfo submissionInfo)
        {
            InitializeComponent();
            _mainWindow = mainWindow;
            _submissionInfo = submissionInfo;

            PopulateCategories();
            PopulateRegions();
            PopulateLanguages();
            DisableFieldsIfNeeded();
        }

        public void Refresh()
        {
            TitleTextBox.Text = _submissionInfo.CommonDiscInfo.Title ?? "";
            ForeignTitleTextBox.Text = _submissionInfo.CommonDiscInfo.ForeignTitleNonLatin ?? "";
            DiscNumberLetterTextBox.Text = _submissionInfo.CommonDiscInfo.DiscNumberLetter ?? "";
            DiscTitleTextBox.Text = _submissionInfo.CommonDiscInfo.DiscTitle ?? "";
            CategoryComboBox.SelectedIndex = _categories.FindIndex(r => r == _submissionInfo.CommonDiscInfo.Category);
            RegionComboBox.SelectedIndex = _regions.FindIndex(r => r == _submissionInfo.CommonDiscInfo.Region);
            if (_submissionInfo.CommonDiscInfo.Languages != null)
            {
                foreach (var language in _submissionInfo.CommonDiscInfo.Languages)
                    _languages.Find(l => l == language).IsChecked = true;
            }
            SerialTextBox.Text = _submissionInfo.CommonDiscInfo.Serial ?? "";
            L0MasteringRingTextBox.Text = _submissionInfo.CommonDiscInfo.MasteringRingFirstLayerDataSide ?? "";
            L0MasteringSIDTextBox.Text = _submissionInfo.CommonDiscInfo.MasteringSIDCodeFirstLayerDataSide ?? "";
            L0ToolstampTextBox.Text = _submissionInfo.CommonDiscInfo.ToolstampMasteringCodeFirstLayerDataSide ?? "";
            L0MouldSIDTextBox.Text = _submissionInfo.CommonDiscInfo.MouldSIDCodeFirstLayerDataSide ?? "";
            L0AdditionalMouldTextBox.Text = _submissionInfo.CommonDiscInfo.AdditionalMouldFirstLayerDataSide ?? "";
            L1MasteringRingTextBox.Text = _submissionInfo.CommonDiscInfo.MasteringRingSecondLayerLabelSide ?? "";
            L1MasteringSIDTextBox.Text = _submissionInfo.CommonDiscInfo.MasteringSIDCodeSecondLayerLabelSide ?? "";
            L1ToolstampTextBox.Text = _submissionInfo.CommonDiscInfo.ToolstampMasteringCodeSecondLayerLabelSide ?? "";
            L1MouldSIDTextBox.Text = _submissionInfo.CommonDiscInfo.MouldSIDCodeSecondLayerLabelSide ?? "";
            L1AdditionalMouldTextBox.Text = _submissionInfo.CommonDiscInfo.AdditionalMouldSecondLayerLabelSide ?? "";
            BarcodeTextBox.Text = _submissionInfo.CommonDiscInfo.Barcode ?? "";
            CommentsTextBox.Text = _submissionInfo.CommonDiscInfo.Comments ?? "";
            ContentsTextBox.Text = _submissionInfo.CommonDiscInfo.Contents ?? "";

            VersionTextBox.Text = _submissionInfo.VersionAndEditions.Version ?? "";
            EditionTextBox.Text = _submissionInfo.VersionAndEditions.OtherEditions ?? "";
        }

        /// <summary>
        /// Get a complete list of categories and fill the combo box
        /// </summary>
        private void PopulateCategories()
        {
            var categories = Enum.GetValues(typeof(Category)).OfType<Category?>().ToList();

            ViewModels.LoggerViewModel.VerboseLogLn("Populating categories, {0} categories found.", categories.Count);

            _categories = new List<CategoryComboBoxItem>();
            foreach (var category in categories)
            {
                _categories.Add(new CategoryComboBoxItem(category));
            }

            CategoryComboBox.ItemsSource = _categories;
            CategoryComboBox.SelectedIndex = 0;
        }

        /// <summary>
        /// Get a complete list of regions and fill the combo box
        /// </summary>
        private void PopulateRegions()
        {
            var regions = Enum.GetValues(typeof(Region)).OfType<Region?>().ToList();

            ViewModels.LoggerViewModel.VerboseLogLn("Populating regions, {0} regions found.", regions.Count);

            _regions = new List<RegionComboBoxItem>();
            foreach (var region in regions)
            {
                _regions.Add(new RegionComboBoxItem(region));
            }

            RegionComboBox.ItemsSource = _regions;
            RegionComboBox.SelectedIndex = 0;
        }

        /// <summary>
        /// Get a complete list of languages and fill the combo box
        /// </summary>
        private void PopulateLanguages()
        {
            var languages = Enum.GetValues(typeof(Language)).OfType<Language?>().ToList();

            ViewModels.LoggerViewModel.VerboseLogLn("Populating languages, {0} languages found.", languages.Count);

            _languages = new List<LanguageComboBoxItem>();
            foreach (var language in languages)
            {
                _languages.Add(new LanguageComboBoxItem(language));
            }

            LanguagesComboBox.ItemsSource = _languages;
            LanguagesComboBox.SelectedIndex = 0;
        }

        /// <summary>
        /// Disable fields that aren't applicable to the current disc
        /// </summary>
        private void DisableFieldsIfNeeded()
        {
            // Only disable for single-layer discs
            if (_submissionInfo.SizeAndChecksums.Layerbreak == default(long))
            {
                L1MasteringRingTextBox.IsReadOnly = true;
                L1MasteringSIDTextBox.IsReadOnly = true;
                L1ToolstampTextBox.IsReadOnly = true;
                L1MouldSIDTextBox.IsReadOnly = true;
                L1AdditionalMouldTextBox.IsReadOnly = true;
            }
        }

        #region Event Handlers

        private void OnClosed(object sender, EventArgs e)
        {
            _submissionInfo.CommonDiscInfo.Title = TitleTextBox.Text ?? "";
            _submissionInfo.CommonDiscInfo.ForeignTitleNonLatin = ForeignTitleTextBox.Text ?? "";
            _submissionInfo.CommonDiscInfo.DiscNumberLetter = DiscNumberLetterTextBox.Text ?? "";
            _submissionInfo.CommonDiscInfo.DiscTitle = DiscTitleTextBox.Text ?? "";
            _submissionInfo.CommonDiscInfo.Category = (CategoryComboBox.SelectedItem as CategoryComboBoxItem)?.Value ?? Category.Games;
            _submissionInfo.CommonDiscInfo.Region = (RegionComboBox.SelectedItem as RegionComboBoxItem)?.Value ?? Region.World;
            var languages = new List<Language?>();
            foreach (var language in _languages)
            {
                if (language.IsChecked)
                    languages.Add(language.Value);
            }
            if (languages.Count == 0)
                languages.Add(null);
            _submissionInfo.CommonDiscInfo.Languages = languages.ToArray();
            _submissionInfo.CommonDiscInfo.Serial = SerialTextBox.Text ?? "";
            _submissionInfo.CommonDiscInfo.MasteringRingFirstLayerDataSide = L0MasteringRingTextBox.Text ?? "";
            _submissionInfo.CommonDiscInfo.MasteringSIDCodeFirstLayerDataSide = L0MasteringSIDTextBox.Text ?? "";
            _submissionInfo.CommonDiscInfo.ToolstampMasteringCodeFirstLayerDataSide = L0ToolstampTextBox.Text ?? "";
            _submissionInfo.CommonDiscInfo.MouldSIDCodeFirstLayerDataSide = L0MouldSIDTextBox.Text ?? "";
            _submissionInfo.CommonDiscInfo.AdditionalMouldFirstLayerDataSide = L0AdditionalMouldTextBox.Text ?? "";
            _submissionInfo.CommonDiscInfo.MasteringRingSecondLayerLabelSide = L1MasteringRingTextBox.Text ?? "";
            _submissionInfo.CommonDiscInfo.MasteringSIDCodeSecondLayerLabelSide = L1MasteringSIDTextBox.Text ?? "";
            _submissionInfo.CommonDiscInfo.ToolstampMasteringCodeSecondLayerLabelSide = L1ToolstampTextBox.Text ?? "";
            _submissionInfo.CommonDiscInfo.MouldSIDCodeSecondLayerLabelSide = L1MouldSIDTextBox.Text ?? "";
            _submissionInfo.CommonDiscInfo.AdditionalMouldSecondLayerLabelSide = L1AdditionalMouldTextBox.Text ?? "";
            _submissionInfo.CommonDiscInfo.Barcode = BarcodeTextBox.Text ?? "";
            _submissionInfo.CommonDiscInfo.Comments = CommentsTextBox.Text ?? "";
            _submissionInfo.CommonDiscInfo.Contents = ContentsTextBox.Text ?? "";

            _submissionInfo.VersionAndEditions.Version = VersionTextBox.Text ?? "";
            _submissionInfo.VersionAndEditions.OtherEditions = EditionTextBox.Text ?? "";

            Hide();
        }

        #endregion
    }
}
