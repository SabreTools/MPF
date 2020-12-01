using System;
using System.Collections.Generic;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using MPF.Data;
using MPF.Web;

namespace MPF.Avalonia
{
    public class DiscInformationWindow : Window
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
            : this(new SubmissionInfo())
        {
        }

        public DiscInformationWindow(SubmissionInfo submissionInfo)
        {
            this.SubmissionInfo = submissionInfo;
            this.InitializeComponent();
#if DEBUG
            this.AttachDevTools();
#endif
        }

            private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);

            PopulateCategories();
            PopulateRegions();
            PopulateLanguages();
            ManipulateFields();
        }

        #region Helpers

        /// <summary>
        /// Manipulate fields based on the current disc
        /// </summary>
        private void ManipulateFields()
        {
            // Different media types mean different fields available
            switch (SubmissionInfo?.CommonDiscInfo.Media)
            {
                case MediaType.CDROM:
                case MediaType.GDROM:
                    this.Find<TextBlock>("L0MasteringRingTextBlock").Text = "Data-Side Mastering Ring";
                    this.Find<TextBlock>("L0MasteringSIDTextBlock").Text = "Data-Side Mastering SID";
                    this.Find<TextBlock>("L0ToolstampTextBlock").Text = "Data-Side Toolstamp/Mastering Code";
                    this.Find<TextBlock>("L0MouldSIDTextBlock").Text = "Data-Side Mould SID";
                    this.Find<TextBlock>("L0AdditionalMouldTextBlock").Text = "Data-Side Additional Mould";
                    this.Find<TextBlock>("L1MasteringRingTextBlock").Text = "Label-Side Mastering Ring";
                    this.Find<TextBlock>("L1MasteringSIDTextBlock").Text = "Label-Side Mastering SID";
                    this.Find<TextBlock>("L1ToolstampTextBlock").Text = "Label-Side Toolstamp/Mastering Code";
                    this.Find<TextBlock>("L1MouldSIDTextBlock").Text = "Label-Side Mould SID";
                    this.Find<TextBlock>("L1AdditionalMouldTextBlock").Text = "Label-Side Additional Mould";
                    break;

                case MediaType.DVD:
                case MediaType.HDDVD:
                case MediaType.BluRay:
                case MediaType.NintendoGameCubeGameDisc:
                case MediaType.NintendoWiiOpticalDisc:
                case MediaType.NintendoWiiUOpticalDisc:
                    // Single-layer discs should read the same as CDs
                    if (SubmissionInfo?.SizeAndChecksums?.Layerbreak == default(long))
                    {
                        this.Find<TextBlock>("L0MasteringRingTextBlock").Text = "Data-Side Mastering Ring";
                        this.Find<TextBlock>("L0MasteringSIDTextBlock").Text = "Data-Side Mastering SID";
                        this.Find<TextBlock>("L0ToolstampTextBlock").Text = "Data-Side Toolstamp/Mastering Code";
                        this.Find<TextBlock>("L0MouldSIDTextBlock").Text = "Data-Side Mould SID";
                        this.Find<TextBlock>("L0AdditionalMouldTextBlock").Text = "Data-Side Additional Mould";
                        this.Find<TextBlock>("L1MasteringRingTextBlock").Text = "Label-Side Mastering Ring";
                        this.Find<TextBlock>("L1MasteringSIDTextBlock").Text = "Label-Side Mastering SID";
                        this.Find<TextBlock>("L1ToolstampTextBlock").Text = "Label-Side Toolstamp/Mastering Code";
                        this.Find<TextBlock>("L1MouldSIDTextBlock").Text = "Label-Side Mould SID";
                        this.Find<TextBlock>("L1AdditionalMouldTextBlock").Text = "Label-Side Additional Mould";
                    }
                    // Double-layer discs should have different naming
                    else
                    {
                        this.Find<TextBlock>("L0MasteringRingTextBlock").Text = "Layer 0 Mastering Ring";
                        this.Find<TextBlock>("L0MasteringSIDTextBlock").Text = "Layer 0 Mastering SID";
                        this.Find<TextBlock>("L0ToolstampTextBlock").Text = "Layer 0 Toolstamp/Mastering Code";
                        this.Find<TextBlock>("L0MouldSIDTextBlock").Text = "Data-Side Mould SID";
                        this.Find<TextBlock>("L0AdditionalMouldTextBlock").Text = "Data-Side Additional Mould";
                        this.Find<TextBlock>("L1MasteringRingTextBlock").Text = "Layer 1 Mastering Ring";
                        this.Find<TextBlock>("L1MasteringSIDTextBlock").Text = "Layer 1 Mastering SID";
                        this.Find<TextBlock>("L1ToolstampTextBlock").Text = "Layer 1 Toolstamp/Mastering Code";
                        this.Find<TextBlock>("L1MouldSIDTextBlock").Text = "Label-Side Mould SID";
                        this.Find<TextBlock>("L1AdditionalMouldTextBlock").Text = "Label-Side Additional Mould";
                    }

                    break;
            }
        }

        /// <summary>
        /// Load the current contents of the base SubmissionInfo to the UI
        /// </summary>
        public void Load()
        {
            // Common Disc Info
            if (SubmissionInfo.CommonDiscInfo == null)
                SubmissionInfo.CommonDiscInfo = new CommonDiscInfoSection();

            this.Find<TextBox>("TitleTextBox").Text = SubmissionInfo.CommonDiscInfo.Title ?? "";
            this.Find<TextBox>("ForeignTitleTextBox").Text = SubmissionInfo.CommonDiscInfo.ForeignTitleNonLatin ?? "";
            this.Find<TextBox>("DiscNumberLetterTextBox").Text = SubmissionInfo.CommonDiscInfo.DiscNumberLetter ?? "";
            this.Find<TextBox>("DiscTitleTextBox").Text = SubmissionInfo.CommonDiscInfo.DiscTitle ?? "";
            this.Find<ComboBox>("CategoryComboBox").SelectedIndex = Categories.FindIndex(r => r == SubmissionInfo.CommonDiscInfo.Category);
            this.Find<ComboBox>("RegionComboBox").SelectedIndex = Regions.FindIndex(r => r == SubmissionInfo.CommonDiscInfo.Region);
            if (SubmissionInfo.CommonDiscInfo.Languages != null)
            {
                foreach (var language in SubmissionInfo.CommonDiscInfo.Languages)
                    Languages.Find(l => l == language).IsChecked = true;
            }

            this.Find<TextBox>("SerialTextBox").Text = SubmissionInfo.CommonDiscInfo.Serial ?? "";
            this.Find<TextBox>("L0MasteringRingTextBox").Text = SubmissionInfo.CommonDiscInfo.MasteringRingFirstLayerDataSide ?? "";
            this.Find<TextBox>("L0MasteringSIDTextBox").Text = SubmissionInfo.CommonDiscInfo.MasteringSIDCodeFirstLayerDataSide ?? "";
            this.Find<TextBox>("L0ToolstampTextBox").Text = SubmissionInfo.CommonDiscInfo.ToolstampMasteringCodeFirstLayerDataSide ?? "";
            this.Find<TextBox>("L0MouldSIDTextBox").Text = SubmissionInfo.CommonDiscInfo.MouldSIDCodeFirstLayerDataSide ?? "";
            this.Find<TextBox>("L0AdditionalMouldTextBox").Text = SubmissionInfo.CommonDiscInfo.AdditionalMouldFirstLayerDataSide ?? "";
            this.Find<TextBox>("L1MasteringRingTextBox").Text = SubmissionInfo.CommonDiscInfo.MasteringRingSecondLayerLabelSide ?? "";
            this.Find<TextBox>("L1MasteringSIDTextBox").Text = SubmissionInfo.CommonDiscInfo.MasteringSIDCodeSecondLayerLabelSide ?? "";
            this.Find<TextBox>("L1ToolstampTextBox").Text = SubmissionInfo.CommonDiscInfo.ToolstampMasteringCodeSecondLayerLabelSide ?? "";
            this.Find<TextBox>("L1MouldSIDTextBox").Text = SubmissionInfo.CommonDiscInfo.MouldSIDCodeSecondLayerLabelSide ?? "";
            this.Find<TextBox>("L1AdditionalMouldTextBox").Text = SubmissionInfo.CommonDiscInfo.AdditionalMouldSecondLayerLabelSide ?? "";
            this.Find<TextBox>("BarcodeTextBox").Text = SubmissionInfo.CommonDiscInfo.Barcode ?? "";
            this.Find<TextBox>("CommentsTextBox").Text = SubmissionInfo.CommonDiscInfo.Comments ?? "";
            this.Find<TextBox>("ContentsTextBox").Text = SubmissionInfo.CommonDiscInfo.Contents ?? "";
            
            // Version and Editions
            if (SubmissionInfo.VersionAndEditions == null)
                SubmissionInfo.VersionAndEditions = new VersionAndEditionsSection();

            this.Find<TextBox>("VersionTextBox").Text = SubmissionInfo.VersionAndEditions.Version ?? "";
            this.Find<TextBox>("EditionTextBox").Text = SubmissionInfo.VersionAndEditions.OtherEditions ?? "";
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

            this.Find<ComboBox>("CategoryComboBox").Items = Categories;
            this.Find<ComboBox>("CategoryComboBox").SelectedIndex = 0;
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

            this.Find<ComboBox>("LanguagesComboBox").Items = Languages;
            this.Find<ComboBox>("LanguagesComboBox").SelectedIndex = 0;
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

            this.Find<ComboBox>("RegionComboBox").Items = Regions;
            this.Find<ComboBox>("RegionComboBox").SelectedIndex = 0;
        }

        /// <summary>
        /// Save the current contents of the UI to the base SubmissionInfo
        /// </summary>
        private void Save()
        {
            // Common Disc Info
            if (SubmissionInfo.CommonDiscInfo == null)
                SubmissionInfo.CommonDiscInfo = new CommonDiscInfoSection();

            SubmissionInfo.CommonDiscInfo.Title = this.Find<TextBox>("TitleTextBox").Text ?? "";
            SubmissionInfo.CommonDiscInfo.ForeignTitleNonLatin = this.Find<TextBox>("ForeignTitleTextBox").Text ?? "";
            SubmissionInfo.CommonDiscInfo.DiscNumberLetter = this.Find<TextBox>(" DiscNumberLetterTextBox").Text ?? "";
            SubmissionInfo.CommonDiscInfo.DiscTitle = this.Find<TextBox>("DiscTitleTextBox").Text ?? "";
            SubmissionInfo.CommonDiscInfo.Category = (this.Find<ComboBox>("CategoryComboBox").SelectedItem as CategoryComboBoxItem)?.Value ?? DiscCategory.Games;
            SubmissionInfo.CommonDiscInfo.Region = (this.Find<ComboBox>("RegionComboBox").SelectedItem as RegionComboBoxItem)?.Value ?? Region.World;
            var languages = new List<Language?>();
            foreach (var language in Languages)
            {
                if (language.IsChecked)
                    languages.Add(language.Value);
            }
            if (languages.Count == 0)
                languages.Add(null);

            SubmissionInfo.CommonDiscInfo.Languages = languages.ToArray();
            SubmissionInfo.CommonDiscInfo.Serial = this.Find<TextBox>("SerialTextBox").Text ?? "";
            SubmissionInfo.CommonDiscInfo.MasteringRingFirstLayerDataSide = this.Find<TextBox>("L0MasteringRingTextBox").Text ?? "";
            SubmissionInfo.CommonDiscInfo.MasteringSIDCodeFirstLayerDataSide = this.Find<TextBox>("L0MasteringSIDTextBox").Text ?? "";
            SubmissionInfo.CommonDiscInfo.ToolstampMasteringCodeFirstLayerDataSide = this.Find<TextBox>("L0ToolstampTextBox").Text ?? "";
            SubmissionInfo.CommonDiscInfo.MouldSIDCodeFirstLayerDataSide = this.Find<TextBox>("L0MouldSIDTextBox").Text ?? "";
            SubmissionInfo.CommonDiscInfo.AdditionalMouldFirstLayerDataSide = this.Find<TextBox>("L0AdditionalMouldTextBox").Text ?? "";
            SubmissionInfo.CommonDiscInfo.MasteringRingSecondLayerLabelSide = this.Find<TextBox>("L1MasteringRingTextBox").Text ?? "";
            SubmissionInfo.CommonDiscInfo.MasteringSIDCodeSecondLayerLabelSide = this.Find<TextBox>("L1MasteringSIDTextBox").Text ?? "";
            SubmissionInfo.CommonDiscInfo.ToolstampMasteringCodeSecondLayerLabelSide = this.Find<TextBox>("L1ToolstampTextBox").Text ?? "";
            SubmissionInfo.CommonDiscInfo.MouldSIDCodeSecondLayerLabelSide = this.Find<TextBox>("L1MouldSIDTextBox").Text ?? "";
            SubmissionInfo.CommonDiscInfo.AdditionalMouldSecondLayerLabelSide = this.Find<TextBox>("L1AdditionalMouldTextBox").Text ?? "";
            SubmissionInfo.CommonDiscInfo.Barcode = this.Find<TextBox>("BarcodeTextBox").Text ?? "";
            SubmissionInfo.CommonDiscInfo.Comments = this.Find<TextBox>("CommentsTextBox").Text ?? "";
            SubmissionInfo.CommonDiscInfo.Contents = this.Find<TextBox>("ContentsTextBox").Text ?? "";

            // Version and Editions
            if (SubmissionInfo.VersionAndEditions == null)
                SubmissionInfo.VersionAndEditions = new VersionAndEditionsSection();

            SubmissionInfo.VersionAndEditions.Version = this.Find<TextBox>("VersionTextBox").Text ?? "";
            SubmissionInfo.VersionAndEditions.OtherEditions = this.Find<TextBox>("EditionTextBox").Text ?? "";
        }

        #endregion

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
