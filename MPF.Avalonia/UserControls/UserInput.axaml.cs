using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;

namespace MPF.Avalonia.UserControls
{
    public partial class UserInput : UserControl
    {
        public static readonly StyledProperty<string?> LabelProperty =
            AvaloniaProperty.Register<UserInput, string?>(nameof(Label));

        public static readonly StyledProperty<string?> TextProperty =
            AvaloniaProperty.Register<UserInput, string?>(nameof(Text), defaultBindingMode: global::Avalonia.Data.BindingMode.TwoWay);

        public static readonly StyledProperty<double> TextHeightProperty =
            AvaloniaProperty.Register<UserInput, double>(nameof(TextHeight), 28.0);

        public static readonly StyledProperty<bool> TabProperty =
            AvaloniaProperty.Register<UserInput, bool>(nameof(Tab));

        public static readonly StyledProperty<bool> EnterProperty =
            AvaloniaProperty.Register<UserInput, bool>(nameof(Enter));

        public static readonly StyledProperty<TextWrapping> TextWrappingProperty =
            AvaloniaProperty.Register<UserInput, TextWrapping>(nameof(TextWrapping), TextWrapping.NoWrap);

        public static readonly StyledProperty<bool> IsReadOnlyProperty =
            AvaloniaProperty.Register<UserInput, bool>(nameof(IsReadOnly));

        public string? Label
        {
            get => GetValue(LabelProperty);
            set => SetValue(LabelProperty, value);
        }

        public string? Text
        {
            get => GetValue(TextProperty);
            set => SetValue(TextProperty, value);
        }

        public double TextHeight
        {
            get => GetValue(TextHeightProperty);
            set => SetValue(TextHeightProperty, value);
        }

        public bool Tab
        {
            get => GetValue(TabProperty);
            set => SetValue(TabProperty, value);
        }

        public bool Enter
        {
            get => GetValue(EnterProperty);
            set => SetValue(EnterProperty, value);
        }

        public TextWrapping TextWrapping
        {
            get => GetValue(TextWrappingProperty);
            set => SetValue(TextWrappingProperty, value);
        }

        public bool IsReadOnly
        {
            get => GetValue(IsReadOnlyProperty);
            set => SetValue(IsReadOnlyProperty, value);
        }

        public UserInput()
        {
            InitializeComponent();
            UpdateTextAlignment();
        }

        protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
        {
            base.OnPropertyChanged(change);

            if (change.Property == EnterProperty)
                UpdateTextAlignment();
        }

        private void UpdateTextAlignment()
            => InputTextBox.VerticalContentAlignment = Enter ? VerticalAlignment.Top : VerticalAlignment.Center;
    }
}
