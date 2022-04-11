using System.Windows;
using System.Windows.Controls;

namespace MPF.UI.Core.UserControls
{
    /// <summary>
    /// Interaction logic for UserInput.xaml
    /// </summary>
    public partial class UserInput : UserControl
    {
        #region Dependency Properties

        public static readonly DependencyProperty LabelProperty =
            DependencyProperty.Register("Label", typeof(string), typeof(UserInput));

        public static readonly DependencyProperty TextProperty =
            DependencyProperty.Register("Text", typeof(string), typeof(UserInput));

        public static readonly DependencyProperty TextHeightProperty =
            DependencyProperty.Register("TextHeight", typeof(string), typeof(UserInput));

        public static readonly DependencyProperty TabProperty =
            DependencyProperty.Register("Tab", typeof(bool), typeof(UserInput));

        public static readonly DependencyProperty EnterProperty =
            DependencyProperty.Register("Enter", typeof(bool), typeof(UserInput));

        public static readonly DependencyProperty TextWrappingProperty =
            DependencyProperty.Register("TextWrapping", typeof(TextWrapping), typeof(UserInput));

        public static readonly DependencyProperty IsReadOnlyProperty =
            DependencyProperty.Register("IsReadOnly", typeof(bool), typeof(UserInput));

        public static readonly DependencyProperty VerticalContentAlignmentValueProperty =
            DependencyProperty.Register("VerticalContentAlignmentValue", typeof(VerticalAlignment), typeof(UserInput));

        public static readonly DependencyProperty HorizontalScrollBarVisibilityProperty =
            DependencyProperty.Register("HorizontalScrollBarVisibility", typeof(ScrollBarVisibility), typeof(UserInput));

        public static readonly DependencyProperty VerticalScrollBarVisibilityProperty =
            DependencyProperty.Register("VerticalScrollBarVisibility", typeof(ScrollBarVisibility), typeof(UserInput));
        
        #endregion

        #region Properties

        public string Label
        {
            get => (string)GetValue(LabelProperty);
            set => SetValue(LabelProperty, value);
        }

        public string Text
        {
            get => (string)GetValue(TextProperty);
            set => SetValue(TextProperty, value);
        }

        public string TextHeight
        {
            get => (string)GetValue(TextHeightProperty);
            set => SetValue(TextHeightProperty, value);
        }

        public bool Tab
        {
            get => (bool)GetValue(TabProperty);
            set => SetValue(TabProperty, value);
        }

        public bool Enter
        {
            get => (bool)GetValue(EnterProperty);
            set => SetValue(EnterProperty, value);
        }

        public TextWrapping TextWrapping
        {
            get => (TextWrapping)GetValue(TextWrappingProperty);
            set => SetValue(TextWrappingProperty, value);
        }

        public bool IsReadOnly
        {
            get => (bool)GetValue(IsReadOnlyProperty);
            set => SetValue(IsReadOnlyProperty, value);
        }

        public VerticalAlignment VerticalContentAlignmentValue
        {
            get => (VerticalAlignment)GetValue(VerticalContentAlignmentValueProperty);
            set => SetValue(VerticalContentAlignmentValueProperty, value);
        }

        public ScrollBarVisibility HorizontalScrollBarVisibility
        {
            get => (ScrollBarVisibility)GetValue(HorizontalScrollBarVisibilityProperty);
            set => SetValue(HorizontalScrollBarVisibilityProperty, value);
        }

        public ScrollBarVisibility VerticalScrollBarVisibility
        {
            get => (ScrollBarVisibility)GetValue(VerticalScrollBarVisibilityProperty);
            set => SetValue(VerticalScrollBarVisibilityProperty, value);
        }

        #endregion

        public UserInput()
        {
            // Set default values
            TextHeight = "22";
            Tab = false;
            Enter = false;
            TextWrapping = TextWrapping.NoWrap;
            IsReadOnly = false;
            VerticalContentAlignmentValue = VerticalAlignment.Center;
            HorizontalScrollBarVisibility = ScrollBarVisibility.Hidden;
            VerticalScrollBarVisibility = ScrollBarVisibility.Auto;

            InitializeComponent();
        }
    }
}
