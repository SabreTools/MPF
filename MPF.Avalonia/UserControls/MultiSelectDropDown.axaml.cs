using System.Collections;
using System.Collections.Specialized;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;

namespace MPF.Avalonia.UserControls
{
    public partial class MultiSelectDropDown : UserControl
    {
        public static readonly StyledProperty<string?> LabelProperty =
            AvaloniaProperty.Register<MultiSelectDropDown, string?>(nameof(Label));

        public static readonly StyledProperty<IEnumerable?> ItemsSourceProperty =
            AvaloniaProperty.Register<MultiSelectDropDown, IEnumerable?>(nameof(ItemsSource));

        public static readonly StyledProperty<bool> IsDropDownOpenProperty =
            AvaloniaProperty.Register<MultiSelectDropDown, bool>(nameof(IsDropDownOpen));

        public static readonly StyledProperty<string> SummaryTextProperty =
            AvaloniaProperty.Register<MultiSelectDropDown, string>(nameof(SummaryText), "None");

        public string? Label
        {
            get => GetValue(LabelProperty);
            set => SetValue(LabelProperty, value);
        }

        public IEnumerable? ItemsSource
        {
            get => GetValue(ItemsSourceProperty);
            set => SetValue(ItemsSourceProperty, value);
        }

        public bool IsDropDownOpen
        {
            get => GetValue(IsDropDownOpenProperty);
            set => SetValue(IsDropDownOpenProperty, value);
        }

        public string SummaryText
        {
            get => GetValue(SummaryTextProperty);
            private set => SetValue(SummaryTextProperty, value);
        }

        public MultiSelectDropDown()
        {
            InitializeComponent();
            UpdateItemsSource();
            UpdateSummaryText();
        }

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

        private void ItemsSourceCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            UpdateItemsSource();
            UpdateSummaryText();
        }

        private void SelectionCheckBoxClick(object? sender, RoutedEventArgs e)
            => UpdateSummaryText();

        private void UpdateItemsSource()
            => ItemsList.ItemsSource = ItemsSource;

        private void UpdateSummaryText()
        {
            if (ItemsSource is null)
            {
                SummaryText = "None";
                return;
            }

            string[] selectedItems = ItemsSource
                .Cast<object?>()
                .Where(IsChecked)
                .Select(item => item?.ToString() ?? string.Empty)
                .Where(text => !string.IsNullOrWhiteSpace(text))
                .ToArray();

            SummaryText = selectedItems.Length == 0 ? "None" : string.Join(", ", selectedItems);
        }

        private static bool IsChecked(object? item)
            => item?.GetType().GetProperty("IsChecked")?.GetValue(item) is true;
    }
}
