using System.Windows;
using System.Windows.Controls;

namespace MPF.UserControls
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

        public static readonly DependencyProperty VerticalContentAlignmentValueProperty =
            DependencyProperty.Register("VerticalContentAlignmentValue", typeof(VerticalAlignment), typeof(UserInput));

        public static readonly DependencyProperty ScrollBarVisibilityProperty =
            DependencyProperty.Register("ScrollBarVisibility", typeof(ScrollBarVisibility), typeof(UserInput));
        
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

        public VerticalAlignment VerticalContentAlignmentValue
        {
            get => (VerticalAlignment)GetValue(VerticalContentAlignmentValueProperty);
            set => SetValue(VerticalContentAlignmentValueProperty, value);
        }

        public ScrollBarVisibility ScrollBarVisibility
        {
            get => (ScrollBarVisibility)GetValue(ScrollBarVisibilityProperty);
            set => SetValue(ScrollBarVisibilityProperty, value);
        }

        #endregion


        public UserInput()
        {
            // Set default values
            TextHeight = "22";
            Tab = false;
            Enter = false;
            TextWrapping = TextWrapping.NoWrap;
            VerticalContentAlignmentValue = VerticalAlignment.Center;
            ScrollBarVisibility = ScrollBarVisibility.Auto;

            InitializeComponent();
        }
    }
}
