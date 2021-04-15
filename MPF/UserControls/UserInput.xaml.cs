using System.Windows;
using System.Windows.Controls;

namespace MPF.UserControls
{
    /// <summary>
    /// Interaction logic for UserInput.xaml
    /// </summary>
    public partial class UserInput : UserControl
    {
        public string Label { get; set; }
        public string Text { get; set; }
        public string TextHeight { get; set; } = "22";
        public bool Tab { get; set; } = false;
        public bool Enter { get; set; } = false;
        public TextWrapping TextWrapping { get; set; } = TextWrapping.NoWrap;
        public VerticalAlignment VerticalContentAlignmentValue { get; set; } = VerticalAlignment.Center;
        public ScrollBarVisibility ScrollBarVisibility { get; set; } = ScrollBarVisibility.Auto;

        public UserInput()
        {
            InitializeComponent();
            DataContext = this;
        }
    }
}
