using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using MPF.Frontend.ComboBoxItems;

namespace MPF.Avalonia.UserControls
{
    /// <summary>
    /// Drop-down control that allows selecting multiple checkbox items and summarizes the selection
    /// </summary>
    public partial class MultiSelectDropDown : UserControl
    {
        #region Styled Properties

        public static readonly StyledProperty<string?> LabelProperty =
            AvaloniaProperty.Register<MultiSelectDropDown, string?>(nameof(Label));

        public static readonly StyledProperty<IEnumerable<IElement>?> ItemsSourceProperty =
            AvaloniaProperty.Register<MultiSelectDropDown, IEnumerable<IElement>?>(nameof(ItemsSource));

        public static readonly StyledProperty<bool> IsDropDownOpenProperty =
            AvaloniaProperty.Register<MultiSelectDropDown, bool>(nameof(IsDropDownOpen));

        public static readonly StyledProperty<string> SummaryTextProperty =
            AvaloniaProperty.Register<MultiSelectDropDown, string>(nameof(SummaryText), "None");

        #endregion

        #region Properties

        /// <summary>
        /// Label that is displayed to the left of the dropdown
        /// </summary>
        public string? Label
        {
            get => GetValue(LabelProperty);
            set => SetValue(LabelProperty, value);
        }

        /// <summary>
        /// Gets or sets a collection used to generate the content of the ItemsControl
        /// </summary>
        public IEnumerable<IElement>? ItemsSource
        {
            get => GetValue(ItemsSourceProperty);
            set => SetValue(ItemsSourceProperty, value);
        }

        /// <summary>
        /// Indicates if the dropdown panel is open
        /// </summary>
        public bool IsDropDownOpen
        {
            get => GetValue(IsDropDownOpenProperty);
            set => SetValue(IsDropDownOpenProperty, value);
        }

        /// <summary>
        /// Text displayed when the dropdown panel is closed
        /// </summary>
        public string SummaryText
        {
            get => GetValue(SummaryTextProperty);
            private set => SetValue(SummaryTextProperty, value);
        }

        #endregion

        public MultiSelectDropDown()
        {
            InitializeComponent();
            UpdateItemsSource();
            UpdateSummaryText();
        }

        /// <summary>
        /// Subscribe/unsubscribe to collection change notifications and refresh when the
        /// items source is replaced
        /// </summary>
        protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
        {
            base.OnPropertyChanged(change);

            if (change.Property == ItemsSourceProperty)
            {
                if (change.OldValue is INotifyCollectionChanged oldCollection)
                    oldCollection.CollectionChanged -= ItemsSourceCollectionChanged;

                if (change.NewValue is INotifyCollectionChanged newCollection)
                    newCollection.CollectionChanged += ItemsSourceCollectionChanged;

                UpdateItemsSource();
                UpdateSummaryText();
            }
        }

        /// <summary>
        /// Refresh the bound items and summary when the underlying collection changes
        /// </summary>
        private void ItemsSourceCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            UpdateItemsSource();
            UpdateSummaryText();
        }

        /// <summary>
        /// Update the summary text whenever a selection checkbox is toggled
        /// </summary>
        private void SelectionCheckBoxClick(object? sender, RoutedEventArgs e)
            => UpdateSummaryText();

        /// <summary>
        /// Bind the items list to the current items source
        /// </summary>
        private void UpdateItemsSource()
            => ItemsList.ItemsSource = ItemsSource;

        /// <summary>
        /// Recompute the comma-separated summary of the currently checked items, or "None"
        /// </summary>
        private void UpdateSummaryText()
        {
            if (ItemsSource is null)
            {
                SummaryText = "None";
                return;
            }

            string[] selectedItems = ItemsSource
                .Where(IsChecked)
                .Select(item => item?.ToString() ?? string.Empty)
                .Where(text => !string.IsNullOrWhiteSpace(text))
                .ToArray();

            SummaryText = selectedItems.Length == 0 ? "None" : string.Join(", ", selectedItems);
        }

        /// <summary>
        /// Reflectively determine whether an item exposes an IsChecked property set to true
        /// </summary>
        private static bool IsChecked(IElement? item)
            => item is not null && item.IsChecked;
    }
}
