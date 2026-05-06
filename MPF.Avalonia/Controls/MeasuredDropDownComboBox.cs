using System;
using System.Collections;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Layout;

namespace MPF.Avalonia.Controls
{
    public class MeasuredDropDownComboBox : ComboBox
    {
        private const double DropDownButtonWidth = 24;
        private const double ClosedComboBoxChromeWidth = 30;
        private const double ShortClosedComboBoxChromeWidth = 44;
        private const int ShortClosedTextThreshold = 24;
        private const double OpenComboBoxChromeWidth = 44;

        public static readonly StyledProperty<bool> SizeToWidestItemProperty =
            AvaloniaProperty.Register<MeasuredDropDownComboBox, bool>(nameof(SizeToWidestItem));

        public static readonly StyledProperty<bool> SizeToSelectedItemProperty =
            AvaloniaProperty.Register<MeasuredDropDownComboBox, bool>(nameof(SizeToSelectedItem));

        public static readonly StyledProperty<bool> IgnoreHeaderItemsInMeasurementProperty =
            AvaloniaProperty.Register<MeasuredDropDownComboBox, bool>(nameof(IgnoreHeaderItemsInMeasurement));

        private Popup? _popup;
        private double _measuredDropDownWidth;

        protected override Type StyleKeyOverride => typeof(ComboBox);

        public bool SizeToWidestItem
        {
            get => GetValue(SizeToWidestItemProperty);
            set => SetValue(SizeToWidestItemProperty, value);
        }

        public bool SizeToSelectedItem
        {
            get => GetValue(SizeToSelectedItemProperty);
            set => SetValue(SizeToSelectedItemProperty, value);
        }

        public bool IgnoreHeaderItemsInMeasurement
        {
            get => GetValue(IgnoreHeaderItemsInMeasurementProperty);
            set => SetValue(IgnoreHeaderItemsInMeasurementProperty, value);
        }

        public MeasuredDropDownComboBox()
        {
            DropDownOpened += OnDropDownOpened;
            DropDownClosed += OnDropDownClosed;
            AttachedToVisualTree += (_, _) => UpdateMeasuredWidth();
        }

        protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
        {
            base.OnApplyTemplate(e);
            _popup = e.NameScope.Find<Popup>("PART_Popup");

            Control? dropDownButton = e.NameScope.Find<Control>("DropDownButton")
                ?? e.NameScope.Find<Control>("PART_DropDownButton");

            if (dropDownButton is not null)
            {
                dropDownButton.Width = DropDownButtonWidth;
                dropDownButton.MinWidth = DropDownButtonWidth;
                dropDownButton.MaxWidth = DropDownButtonWidth;
                dropDownButton.HorizontalAlignment = HorizontalAlignment.Right;

                if (dropDownButton is Button button)
                    button.Padding = new Thickness(0);
            }
        }

        protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
        {
            base.OnPropertyChanged(change);

            if (change.Property == ItemsSourceProperty || change.Property == FontFamilyProperty
                || change.Property == FontSizeProperty || change.Property == FontStyleProperty
                || change.Property == FontWeightProperty || change.Property == SizeToWidestItemProperty
                || change.Property == SizeToSelectedItemProperty
                || change.Property == IgnoreHeaderItemsInMeasurementProperty)
            {
                _measuredDropDownWidth = 0;
                UpdateMeasuredWidth();
            }
            else if (change.Property == SelectedItemProperty)
            {
                UpdateMeasuredWidth();
            }
        }

        private void OnDropDownOpened(object? sender, EventArgs e)
        {
            double measuredWidth = GetMeasuredDropDownWidth();

            if (SizeToSelectedItem && measuredWidth > 0)
                Width = measuredWidth;

            if (_popup?.Child is not Control child)
                return;

            child.MinWidth = Math.Max(child.MinWidth, measuredWidth);
        }

        private void OnDropDownClosed(object? sender, EventArgs e)
        {
            UpdateMeasuredWidth();
        }

        private void UpdateMeasuredWidth()
        {
            if (!SizeToWidestItem && !SizeToSelectedItem)
                return;

            double measuredWidth = SizeToSelectedItem && !IsDropDownOpen
                ? MeasureSelectedItemWidth()
                : GetMeasuredDropDownWidth();

            if (measuredWidth > 0)
                Width = measuredWidth;
        }

        private double GetMeasuredDropDownWidth()
        {
            if (_measuredDropDownWidth > 0)
                return _measuredDropDownWidth;

            double maxWidth = 0;
            IEnumerable? items = ItemsSource as IEnumerable ?? Items;
            if (items is null)
                return maxWidth;

            foreach (object? item in items)
            {
                if (ShouldSkipItem(item))
                    continue;

                maxWidth = Math.Max(maxWidth, MeasureItemWidth(item, OpenComboBoxChromeWidth));
            }

            _measuredDropDownWidth = Math.Ceiling(maxWidth);
            return _measuredDropDownWidth;
        }

        private double MeasureSelectedItemWidth()
        {
            string selectedText = SelectedItem?.ToString() ?? string.Empty;
            double chromeWidth = selectedText.Length <= ShortClosedTextThreshold
                ? ShortClosedComboBoxChromeWidth
                : ClosedComboBoxChromeWidth;

            return MeasureItemWidth(SelectedItem, chromeWidth);
        }

        private double MeasureItemWidth(object? item, double chromeWidth)
        {
            if (item is null)
                return 0;

            var textBlock = new TextBlock
            {
                Text = item.ToString() ?? string.Empty,
                FontFamily = FontFamily,
                FontSize = FontSize,
                FontStyle = FontStyle,
                FontWeight = global::Avalonia.Media.FontWeight.SemiBold,
            };

            textBlock.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
            return Math.Ceiling(textBlock.DesiredSize.Width + chromeWidth);
        }

        private bool ShouldSkipItem(object? item)
        {
            if (!IgnoreHeaderItemsInMeasurement || item is null)
                return false;

            object? isHeader = item.GetType().GetProperty("IsHeader")?.GetValue(item);
            return isHeader is true;
        }
    }
}
