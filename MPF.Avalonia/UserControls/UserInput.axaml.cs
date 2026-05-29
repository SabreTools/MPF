using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;

namespace MPF.Avalonia.UserControls
{
    /// <summary>
    /// Interaction logic for UserInput.axaml
    /// </summary>
    public partial class UserInput : UserControl
    {
        #region Styled Properties

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

        #endregion

        #region Properties

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

        #endregion

        public UserInput()
        {
            InitializeComponent();
            UpdateTextAlignment();
        }

        /// <summary>
        /// Re-evaluate the text alignment whenever the Enter property changes
        /// </summary>
        protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
        {
            base.OnPropertyChanged(change);

            if (change.Property == EnterProperty)
                UpdateTextAlignment();
        }

        /// <summary>
        /// Align the input text to the top for multi-line (Enter) inputs, otherwise center it
        /// </summary>
        private void UpdateTextAlignment()
            => InputTextBox.VerticalContentAlignment = Enter ? VerticalAlignment.Top : VerticalAlignment.Center;
    }
}
