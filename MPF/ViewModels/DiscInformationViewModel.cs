﻿using System.Collections.Generic;
using System.Linq;
using System.Windows;
using MPF.Data;
using MPF.Windows;

namespace MPF.GUI.ViewModels
{
    public class DiscInformationViewModel
    {
        #region Fields

        /// <summary>
        /// Parent DiscInformationWindow object
        /// </summary>
        public DiscInformationWindow Parent { get; private set; }

        /// <summary>
        /// SubmissionInfo object to fill and save
        /// </summary>
        public SubmissionInfo SubmissionInfo { get; private set; }

        #endregion

        #region Lists

        /// <summary>
        /// List of available disc categories
        /// </summary>
        public List<Element<RedumpDiscCategory>> Categories { get; private set; } = Element<RedumpDiscCategory>.GenerateElements().ToList();

        /// <summary>
        /// List of available regions
        /// </summary>
        public List<Element<RedumpRegion>> Regions { get; private set; } = Element<RedumpRegion>.GenerateElements().ToList();

        /// <summary>
        /// List of available languages
        /// </summary>
        public List<Element<RedumpLanguage>> Languages { get; private set; } = Element<RedumpLanguage>.GenerateElements().ToList();

        /// <summary>
        /// List of available languages
        /// </summary>
        public List<Element<RedumpLanguageSelection>> LanguageSelections { get; private set; } = Element<RedumpLanguageSelection>.GenerateElements().ToList();

        #endregion

        /// <summary>
        /// Constructor
        /// </summary>
        public DiscInformationViewModel(DiscInformationWindow parent, SubmissionInfo submissionInfo)
        {
            Parent = parent;
            SubmissionInfo = submissionInfo.Clone() as SubmissionInfo ?? new SubmissionInfo();

            // Add handlers
            Parent.AcceptButton.Click += OnAcceptClick;
            Parent.CancelButton.Click += OnCancelClick;
            Parent.RingCodeGuideButton.Click += OnRingCodeGuideClick;

            // Update UI with new values
            ManipulateFields();
            Load();
        }

        #region Helpers

        /// <summary>
        /// Manipulate fields based on the current disc
        /// </summary>
        private void ManipulateFields()
        {
            // Sony-printed discs have layers in the opposite order
            var system = SubmissionInfo?.CommonDiscInfo?.System;
            bool reverseOrder = system == KnownSystem.SonyPlayStation2
                || system == KnownSystem.SonyPlayStation3
                || system == KnownSystem.SonyPlayStation4
                || system == KnownSystem.SonyPlayStation5;

            // Different media types mean different fields available
            switch (SubmissionInfo?.CommonDiscInfo?.Media)
            {
                case MediaType.CDROM:
                case MediaType.GDROM:
                    Parent.L0Info.Header = "Data Side";
                    Parent.L0MasteringRing.Label = "Mastering Ring";
                    Parent.L0MasteringSID.Label = "Mastering SID";
                    Parent.L0Toolstamp.Label = "Toolstamp/Mastering Code";
                    Parent.L0MouldSID.Label = "Mould SID";
                    Parent.L0AdditionalMould.Label = "Additional Mould";

                    Parent.L1Info.Header = "Label Side";
                    Parent.L1MasteringRing.Visibility = Visibility.Collapsed;
                    Parent.L1MasteringSID.Visibility = Visibility.Collapsed;
                    Parent.L1Toolstamp.Visibility = Visibility.Collapsed;
                    Parent.L1MouldSID.Label = "Mould SID";
                    Parent.L1AdditionalMould.Label = "Additional Mould";
                    break;

                case MediaType.DVD:
                case MediaType.HDDVD:
                case MediaType.BluRay:
                case MediaType.NintendoGameCubeGameDisc:
                case MediaType.NintendoWiiOpticalDisc:
                case MediaType.NintendoWiiUOpticalDisc:
                    // Quad-layer discs
                    if (SubmissionInfo?.SizeAndChecksums?.Layerbreak3 != default(long))
                    {
                        Parent.L2Info.Visibility = Visibility.Visible;
                        Parent.L3Info.Visibility = Visibility.Visible;

                        Parent.L0Info.Header = reverseOrder ? "Layer 0 (Outer)" : "Layer 0 (Inner)";
                        Parent.L0MasteringRing.Label = "Mastering Ring";
                        Parent.L0MasteringSID.Label = "Mastering SID";
                        Parent.L0Toolstamp.Label = "Toolstamp/Mastering Code";
                        Parent.L0MouldSID.Label = "Data Side Mould SID";
                        Parent.L0AdditionalMould.Label = "Data Side Additional Mould";

                        Parent.L1Info.Header = "Layer 1";
                        Parent.L1MasteringRing.Label = "Mastering Ring";
                        Parent.L1MasteringSID.Label = "Mastering SID";
                        Parent.L1Toolstamp.Label = "Toolstamp/Mastering Code";
                        Parent.L1MouldSID.Label = "Label Side Mould SID";
                        Parent.L1AdditionalMould.Label = "Label Side Additional Mould";

                        Parent.L2Info.Header = "Layer 2";
                        Parent.L2MasteringRing.Label = "Mastering Ring";
                        Parent.L2MasteringSID.Label = "Mastering SID";
                        Parent.L2Toolstamp.Label = "Toolstamp/Mastering Code";

                        Parent.L3Info.Header = reverseOrder ? "Layer 3 (Inner)" : "Layer 3 (Outer)";
                        Parent.L3MasteringRing.Label = "Mastering Ring";
                        Parent.L3MasteringSID.Label = "Mastering SID";
                        Parent.L3Toolstamp.Label = "Toolstamp/Mastering Code";
                    }

                    // Triple-layer discs
                    else if (SubmissionInfo?.SizeAndChecksums?.Layerbreak2 != default(long))
                    {
                        Parent.L2Info.Visibility = Visibility.Visible;

                        Parent.L0Info.Header = reverseOrder ? "Layer 0 (Outer)" : "Layer 0 (Inner)";
                        Parent.L0MasteringRing.Label = "Mastering Ring";
                        Parent.L0MasteringSID.Label = "Mastering SID";
                        Parent.L0Toolstamp.Label = "Toolstamp/Mastering Code";
                        Parent.L0MouldSID.Label = "Data Side Mould SID";
                        Parent.L0AdditionalMould.Label = "Data Side Additional Mould";

                        Parent.L1Info.Header = "Layer 1";
                        Parent.L1MasteringRing.Label = "Mastering Ring";
                        Parent.L1MasteringSID.Label = "Mastering SID";
                        Parent.L1Toolstamp.Label = "Toolstamp/Mastering Code";
                        Parent.L1MouldSID.Label = "Label Side Mould SID";
                        Parent.L1AdditionalMould.Label = "Label Side Additional Mould";

                        Parent.L2Info.Header = reverseOrder ? "Layer 2 (Inner)" : "Layer 2 (Outer)";
                        Parent.L2MasteringRing.Label = "Mastering Ring";
                        Parent.L2MasteringSID.Label = "Mastering SID";
                        Parent.L2Toolstamp.Label = "Toolstamp/Mastering Code";
                    }

                    // Double-layer discs
                    else if (SubmissionInfo?.SizeAndChecksums?.Layerbreak != default(long))
                    {
                        Parent.L0Info.Header = reverseOrder ? "Layer 0 (Outer)" : "Layer 0 (Inner)";
                        Parent.L0MasteringRing.Label = "Mastering Ring";
                        Parent.L0MasteringSID.Label = "Mastering SID";
                        Parent.L0Toolstamp.Label = "Toolstamp/Mastering Code";
                        Parent.L0MouldSID.Label = "Data Side Mould SID";
                        Parent.L0AdditionalMould.Label = "Data Side Additional Mould";

                        Parent.L1Info.Header = reverseOrder ? "Layer 1 (Inner)" : "Layer 1 (Outer)";
                        Parent.L1MasteringRing.Label = "Mastering Ring";
                        Parent.L1MasteringSID.Label = "Mastering SID";
                        Parent.L1Toolstamp.Label = "Toolstamp/Mastering Code";
                        Parent.L1MouldSID.Label = "Label Side Mould SID";
                        Parent.L1AdditionalMould.Label = "Label Side Additional Mould";
                    }

                    // Single-layer discs
                    else
                    {
                        Parent.L0Info.Header = "Data Side";
                        Parent.L0MasteringRing.Label = "Mastering Ring";
                        Parent.L0MasteringSID.Label = "Mastering SID";
                        Parent.L0Toolstamp.Label = "Toolstamp/Mastering Code";
                        Parent.L0MouldSID.Label = "Mould SID";
                        Parent.L0AdditionalMould.Label = "Additional Mould";

                        Parent.L1Info.Header = "Label Side";
                        Parent.L1MasteringRing.Label = "Mastering Ring";
                        Parent.L1MasteringSID.Label = "Mastering SID";
                        Parent.L1Toolstamp.Label = "Toolstamp/Mastering Code";
                        Parent.L1MouldSID.Label = "Mould SID";
                        Parent.L1AdditionalMould.Label = "Additional Mould";
                    }

                    break;

                // All other media we assume to have no rings
                default:
                    Parent.L0Info.Visibility = Visibility.Collapsed;
                    Parent.L1Info.Visibility = Visibility.Collapsed;
                    Parent.L2Info.Visibility = Visibility.Collapsed;
                    Parent.L3Info.Visibility = Visibility.Collapsed;
                    break;
            }

            // Different systems mean different fields available
            switch (system)
            {
                case KnownSystem.SonyPlayStation2:
                    Parent.LanguageSelectionGrid.Visibility = Visibility.Visible;
                    break;
            }
        }

        /// <summary>
        /// Load the current contents of the base SubmissionInfo to the UI
        /// </summary>
        private void Load()
        {
            Parent.CategoryComboBox.SelectedIndex = Categories.FindIndex(r => r == SubmissionInfo.CommonDiscInfo.Category);
            Parent.RegionComboBox.SelectedIndex = Regions.FindIndex(r => r == SubmissionInfo.CommonDiscInfo.Region);
            if (SubmissionInfo.CommonDiscInfo.Languages != null)
                Languages.ForEach(l => l.IsChecked = SubmissionInfo.CommonDiscInfo.Languages.Contains(l));
            if (SubmissionInfo.CommonDiscInfo.LanguageSelection != null)
                LanguageSelections.ForEach(ls => ls.IsChecked = SubmissionInfo.CommonDiscInfo.LanguageSelection.Contains(ls));
        }

        /// <summary>
        /// Save the current contents of the UI to the base SubmissionInfo
        /// </summary>
        public void Save()
        {
            SubmissionInfo.CommonDiscInfo.Category = (Parent.CategoryComboBox.SelectedItem as Element<RedumpDiscCategory>)?.Value ?? RedumpDiscCategory.Games;
            SubmissionInfo.CommonDiscInfo.Region = (Parent.RegionComboBox.SelectedItem as Element<RedumpRegion>)?.Value ?? RedumpRegion.World;
            SubmissionInfo.CommonDiscInfo.Languages = Languages.Where(l => l.IsChecked).Select(l => l?.Value).ToArray();
            if (!SubmissionInfo.CommonDiscInfo.Languages.Any())
                SubmissionInfo.CommonDiscInfo.Languages = new RedumpLanguage?[] { null };
            SubmissionInfo.CommonDiscInfo.LanguageSelection = LanguageSelections.Where(ls => ls.IsChecked).Select(ls => ls?.Value).ToArray();
        }

        #endregion

        #region Event Handlers

        /// <summary>
        /// Handler for AcceptButton Click event
        /// </summary>
        private void OnAcceptClick(object sender, RoutedEventArgs e)
        {
            Save();
            Parent.DialogResult = true;
            Parent.Close();
        }

        /// <summary>
        /// Handler for CancelButton Click event
        /// </summary>
        private void OnCancelClick(object sender, RoutedEventArgs e)
        {
            Parent.DialogResult = false;
            Parent.Close();
        }

        /// <summary>
        /// Handler for RingCodeGuideButton Click event
        /// </summary>
        private void OnRingCodeGuideClick(object sender, RoutedEventArgs e)
        {
            var ringCodeGuideWindow = new RingCodeGuideWindow()
            {
                Owner = Parent,
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
            };
            ringCodeGuideWindow.Show();
        }

        #endregion
    }
}
