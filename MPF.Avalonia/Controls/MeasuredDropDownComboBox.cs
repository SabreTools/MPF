using System;
using System.Collections;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;

namespace MPF.Avalonia.Controls
{
    public class MeasuredDropDownComboBox : ComboBox
    {
        private const double DropDownChromeWidth = 72;

        public static readonly StyledProperty<bool> SizeToWidestItemProperty =
            AvaloniaProperty.Register<MeasuredDropDownComboBox, bool>(nameof(SizeToWidestItem));

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

        public bool IgnoreHeaderItemsInMeasurement
        {
            get => GetValue(IgnoreHeaderItemsInMeasurementProperty);
            set => SetValue(IgnoreHeaderItemsInMeasurementProperty, value);
        }

        public MeasuredDropDownComboBox()
        {
            DropDownOpened += OnDropDownOpened;
            AttachedToVisualTree += (_, _) => UpdateMeasuredWidth();
        }

        protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
        {
            base.OnApplyTemplate(e);
            _popup = e.NameScope.Find<Popup>("PART_Popup");
        }

        protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
        {
            base.OnPropertyChanged(change);

            if (change.Property == ItemsSourceProperty || change.Property == FontFamilyProperty
                || change.Property == FontSizeProperty || change.Property == FontStyleProperty
                || change.Property == FontWeightProperty || change.Property == SizeToWidestItemProperty
                || change.Property == IgnoreHeaderItemsInMeasurementProperty)
            {
                _measuredDropDownWidth = 0;
                UpdateMeasuredWidth();
            }
        }

        private void OnDropDownOpened(object? sender, EventArgs e)
        {
            if (_popup?.Child is not Control child)
                return;

            child.MinWidth = Math.Max(child.MinWidth, GetMeasuredDropDownWidth());
        }

        private void UpdateMeasuredWidth()
        {
            if (!SizeToWidestItem)
                return;

            double measuredWidth = GetMeasuredDropDownWidth();
            if (measuredWidth > 0)
                Width = measuredWidth;
        }

        private double GetMeasuredDropDownWidth()
        {
            if (_measuredDropDownWidth > 0)
                return _measuredDropDownWidth;

            double maxWidth = SizeToWidestItem ? 0 : Bounds.Width;
            IEnumerable? items = ItemsSource as IEnumerable ?? Items;
            if (items is null)
                return maxWidth;

            foreach (object? item in items)
            {
                if (ShouldSkipItem(item))
                    continue;

                maxWidth = Math.Max(maxWidth, MeasureItemWidth(item, DropDownChromeWidth));
            }

            _measuredDropDownWidth = Math.Ceiling(maxWidth);
            return _measuredDropDownWidth;
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
