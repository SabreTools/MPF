using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Layout;
using Avalonia.Markup.Xaml;
using Avalonia.Media;

namespace MPF.UI.Avalonia.Controls
{
    /// <summary>
    /// Reusable label + text-input control.
    /// Avalonia port of MPF.UI.UserControls.UserInput.
    /// </summary>
    public partial class UserInput : UserControl
    {
        #region Styled Properties

        public static readonly StyledProperty<string?> LabelProperty =
            AvaloniaProperty.Register<UserInput, string?>(nameof(Label));

        public static readonly StyledProperty<string?> TextProperty =
            AvaloniaProperty.Register<UserInput, string?>(nameof(Text));

        /// <summary>
        /// Height of the inner TextBox expressed as a string (e.g. "22").
        /// Kept as <see cref="string"/> to preserve API parity with the WPF source.
        /// The XAML binding uses <c>StringToDoubleConverter</c> to convert to the
        /// Avalonia <see cref="double"/> Height property.
        /// </summary>
        public static readonly StyledProperty<string?> TextHeightProperty =
            AvaloniaProperty.Register<UserInput, string?>(nameof(TextHeight), defaultValue: "22");

        public static readonly StyledProperty<bool> TabProperty =
            AvaloniaProperty.Register<UserInput, bool>(nameof(Tab), defaultValue: false);

        public static readonly StyledProperty<bool> EnterProperty =
            AvaloniaProperty.Register<UserInput, bool>(nameof(Enter), defaultValue: false);

        public static readonly StyledProperty<TextWrapping> TextWrappingProperty =
            AvaloniaProperty.Register<UserInput, TextWrapping>(nameof(TextWrapping), defaultValue: TextWrapping.NoWrap);

        public static readonly StyledProperty<bool> IsReadOnlyProperty =
            AvaloniaProperty.Register<UserInput, bool>(nameof(IsReadOnly), defaultValue: false);

        public static readonly StyledProperty<VerticalAlignment> VerticalContentAlignmentValueProperty =
            AvaloniaProperty.Register<UserInput, VerticalAlignment>(nameof(VerticalContentAlignmentValue), defaultValue: VerticalAlignment.Center);

        public static readonly StyledProperty<ScrollBarVisibility> HorizontalScrollBarVisibilityProperty =
            AvaloniaProperty.Register<UserInput, ScrollBarVisibility>(nameof(HorizontalScrollBarVisibility), defaultValue: ScrollBarVisibility.Hidden);

        public static readonly StyledProperty<ScrollBarVisibility> VerticalScrollBarVisibilityProperty =
            AvaloniaProperty.Register<UserInput, ScrollBarVisibility>(nameof(VerticalScrollBarVisibility), defaultValue: ScrollBarVisibility.Auto);

        #endregion

        #region CLR Property Wrappers

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

        public string? TextHeight
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

        public VerticalAlignment VerticalContentAlignmentValue
        {
            get => GetValue(VerticalContentAlignmentValueProperty);
            set => SetValue(VerticalContentAlignmentValueProperty, value);
        }

        public ScrollBarVisibility HorizontalScrollBarVisibility
        {
            get => GetValue(HorizontalScrollBarVisibilityProperty);
            set => SetValue(HorizontalScrollBarVisibilityProperty, value);
        }

        public ScrollBarVisibility VerticalScrollBarVisibility
        {
            get => GetValue(VerticalScrollBarVisibilityProperty);
            set => SetValue(VerticalScrollBarVisibilityProperty, value);
        }

        #endregion

        public UserInput()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
