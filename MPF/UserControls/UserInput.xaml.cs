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
        public bool Tab { get; set; } = false;

        public UserInput()
        {
            InitializeComponent();
            DataContext = this;
        }
    }
}
