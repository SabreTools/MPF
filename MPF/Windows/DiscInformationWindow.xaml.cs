using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using MPF.Data;
using MPF.Web;

namespace MPF.Windows
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

        public DiscInformationWindow(SubmissionInfo submissionInfo)
        {
            this.SubmissionInfo = submissionInfo;
            InitializeComponent();

            PopulateCategories();
            PopulateRegions();
            PopulateLanguages();
            ManipulateFields();
        }

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
                    L0MasteringRing.Label = "Data-Side Mastering Ring";
                    L0MasteringSID.Label = "Data-Side Mastering SID";
                    L0Toolstamp.Label = "Data-Side Toolstamp/Mastering Code";
                    L0MouldSID.Label = "Data-Side Mould SID";
                    L0AdditionalMould.Label = "Data-Side Additional Mould";

                    L1MasteringRing.Label = "Label-Side Mastering Ring";
                    L1MasteringSID.Label = "Label-Side Mastering SID";
                    L1Toolstamp.Label = "Label-Side Toolstamp/Mastering Code";
                    L1MouldSID.Label = "Label-Side Mould SID";
                    L1AdditionalMould.Label = "Label-Side Additional Mould";
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
                        L0MasteringRing.Label = "Data-Side Mastering Ring";
                        L0MasteringSID.Label = "Data-Side Mastering SID";
                        L0Toolstamp.Label = "Data-Side Toolstamp/Mastering Code";
                        L0MouldSID.Label = "Data-Side Mould SID";
                        L0AdditionalMould.Label = "Data-Side Additional Mould";

                        L1MasteringRing.Label = "Label-Side Mastering Ring";
                        L1MasteringSID.Label = "Label-Side Mastering SID";
                        L1Toolstamp.Label = "Label-Side Toolstamp/Mastering Code";
                        L1MouldSID.Label = "Label-Side Mould SID";
                        L1AdditionalMould.Label = "Label-Side Additional Mould";
                    }
                    // Double-layer discs should have different naming
                    else
                    {
                        L0MasteringRing.Label = "Layer 0 Mastering Ring";
                        L0MasteringSID.Label = "Layer 0 Mastering SID";
                        L0Toolstamp.Label = "Layer 0 Toolstamp/Mastering Code";
                        L0MouldSID.Label = "Data-Side Mould SID";
                        L0AdditionalMould.Label = "Data-Side Additional Mould";

                        L1MasteringRing.Label = "Layer 1 Mastering Ring";
                        L1MasteringSID.Label = "Layer 1 Mastering SID";
                        L1Toolstamp.Label = "Layer 1 Toolstamp/Mastering Code";
                        L1MouldSID.Label = "Label-Side Mould SID";
                        L1AdditionalMould.Label = "Label-Side Additional Mould";
                    }

                    break;
            }
        }

        /// <summary>
        /// Load the current contents of the base SubmissionInfo to the UI
        /// </summary>
        public void Load()
        {
            Title.Text = SubmissionInfo.CommonDiscInfo.Title ?? "";
            ForeignTitle.Text = SubmissionInfo.CommonDiscInfo.ForeignTitleNonLatin ?? "";
            DiscNumberLetter.Text = SubmissionInfo.CommonDiscInfo.DiscNumberLetter ?? "";
            DiscTitle.Text = SubmissionInfo.CommonDiscInfo.DiscTitle ?? "";
            CategoryComboBox.SelectedIndex = Categories.FindIndex(r => r == SubmissionInfo.CommonDiscInfo.Category);
            RegionComboBox.SelectedIndex = Regions.FindIndex(r => r == SubmissionInfo.CommonDiscInfo.Region);
            if (SubmissionInfo.CommonDiscInfo.Languages != null)
            {
                foreach (var language in SubmissionInfo.CommonDiscInfo.Languages)
                    Languages.Find(l => l == language).IsChecked = true;
            }
            Serial.Text = SubmissionInfo.CommonDiscInfo.Serial ?? "";

            L0MasteringRing.Text = SubmissionInfo.CommonDiscInfo.Layer0MasteringRing ?? "";
            L0MasteringSID.Text = SubmissionInfo.CommonDiscInfo.Layer0MasteringSID ?? "";
            L0Toolstamp.Text = SubmissionInfo.CommonDiscInfo.Layer0ToolstampMasteringCode ?? "";
            L0MouldSID.Text = SubmissionInfo.CommonDiscInfo.Layer0MouldSID ?? "";
            L0AdditionalMould.Text = SubmissionInfo.CommonDiscInfo.Layer0AdditionalMould ?? "";

            L1MasteringRing.Text = SubmissionInfo.CommonDiscInfo.Layer1MasteringRing ?? "";
            L1MasteringSID.Text = SubmissionInfo.CommonDiscInfo.Layer1MasteringSID ?? "";
            L1Toolstamp.Text = SubmissionInfo.CommonDiscInfo.Layer1ToolstampMasteringCode ?? "";
            L1MouldSID.Text = SubmissionInfo.CommonDiscInfo.Layer1MouldSID ?? "";
            L1AdditionalMould.Text = SubmissionInfo.CommonDiscInfo.Layer1AdditionalMould ?? "";

            //L2MasteringRing.Text = SubmissionInfo.CommonDiscInfo.Layer2MasteringRing ?? "";
            //L2MasteringSID.Text = SubmissionInfo.CommonDiscInfo.Layer2MasteringSID ?? "";
            //L2Toolstamp.Text = SubmissionInfo.CommonDiscInfo.Layer2ToolstampMasteringCode ?? "";;

            //L3MasteringRing.Text = SubmissionInfo.CommonDiscInfo.Layer3MasteringRing ?? "";
            //L3MasteringSID.Text = SubmissionInfo.CommonDiscInfo.Layer3MasteringSID ?? "";
            //L3Toolstamp.Text = SubmissionInfo.CommonDiscInfo.Layer3ToolstampMasteringCode ?? "";

            Barcode.Text = SubmissionInfo.CommonDiscInfo.Barcode ?? "";
            Comments.Text = SubmissionInfo.CommonDiscInfo.Comments ?? "";
            Contents.Text = SubmissionInfo.CommonDiscInfo.Contents ?? "";

            Version.Text = SubmissionInfo.VersionAndEditions.Version ?? "";
            Edition.Text = SubmissionInfo.VersionAndEditions.OtherEditions ?? "";
        }

        /// <summary>
        /// Get a complete list of categories and fill the combo box
        /// </summary>
        private void PopulateCategories()
        {
            var categories = Enum.GetValues(typeof(DiscCategory)).OfType<DiscCategory?>().ToList();

            ViewModels.LoggerViewModel.VerboseLogLn($"Populating categories, {categories.Count} categories found.");

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

            ViewModels.LoggerViewModel.VerboseLogLn($"Populating languages, {languages.Count} languages found.");

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

            ViewModels.LoggerViewModel.VerboseLogLn($"Populating regions, {regions.Count} regions found.");

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
            SubmissionInfo.CommonDiscInfo.Title = Title.Text ?? "";
            SubmissionInfo.CommonDiscInfo.ForeignTitleNonLatin = ForeignTitle.Text ?? "";
            SubmissionInfo.CommonDiscInfo.DiscNumberLetter = DiscNumberLetter.Text ?? "";
            SubmissionInfo.CommonDiscInfo.DiscTitle = DiscTitle.Text ?? "";
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
            SubmissionInfo.CommonDiscInfo.Serial = Serial.Text ?? "";

            SubmissionInfo.CommonDiscInfo.Layer0MasteringRing = L0MasteringRing.Text ?? "";
            SubmissionInfo.CommonDiscInfo.Layer0MasteringSID = L0MasteringSID.Text ?? "";
            SubmissionInfo.CommonDiscInfo.Layer0ToolstampMasteringCode = L0Toolstamp.Text ?? "";
            SubmissionInfo.CommonDiscInfo.Layer0MouldSID = L0MouldSID.Text ?? "";
            SubmissionInfo.CommonDiscInfo.Layer0AdditionalMould = L0AdditionalMould.Text ?? "";

            SubmissionInfo.CommonDiscInfo.Layer1MasteringRing = L1MasteringRing.Text ?? "";
            SubmissionInfo.CommonDiscInfo.Layer1MasteringSID = L1MasteringSID.Text ?? "";
            SubmissionInfo.CommonDiscInfo.Layer1ToolstampMasteringCode = L1Toolstamp.Text ?? "";
            SubmissionInfo.CommonDiscInfo.Layer1MouldSID = L1MouldSID.Text ?? "";
            SubmissionInfo.CommonDiscInfo.Layer1AdditionalMould = L1AdditionalMould.Text ?? "";

            //SubmissionInfo.CommonDiscInfo.Layer2MasteringRing = L2MasteringRing.Text ?? "";
            //SubmissionInfo.CommonDiscInfo.Layer2MasteringSID = L2MasteringSID.Text ?? "";
            //SubmissionInfo.CommonDiscInfo.Layer2ToolstampMasteringCode = L2Toolstamp.Text ?? "";

            //SubmissionInfo.CommonDiscInfo.Layer3MasteringRing = L3MasteringRing.Text ?? "";
            //SubmissionInfo.CommonDiscInfo.Layer3MasteringSID = L3MasteringSID.Text ?? "";
            //SubmissionInfo.CommonDiscInfo.Layer3ToolstampMasteringCode = L3Toolstamp.Text ?? "";

            SubmissionInfo.CommonDiscInfo.Barcode = Barcode.Text ?? "";
            SubmissionInfo.CommonDiscInfo.Comments = Comments.Text ?? "";
            SubmissionInfo.CommonDiscInfo.Contents = Contents.Text ?? "";

            SubmissionInfo.VersionAndEditions.Version = this.Version.Text ?? "";
            SubmissionInfo.VersionAndEditions.OtherEditions = Edition.Text ?? "";
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
