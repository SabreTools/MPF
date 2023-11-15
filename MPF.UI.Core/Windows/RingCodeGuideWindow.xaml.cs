namespace MPF.UI.Core.Windows
{
    /// <summary>
    /// Interaction logic for RingCodeGuideWindow.xaml
    /// </summary>
    public partial class RingCodeGuideWindow : WindowBase
    {
        public RingCodeGuideWindow()
        {
#if NET452_OR_GREATER
            var chrome = new System.Windows.Shell.WindowChrome
            {
                CaptionHeight = 0,
                ResizeBorderThickness = new System.Windows.Thickness(0),
            };
            System.Windows.Shell.WindowChrome.SetWindowChrome(this, chrome);
#endif

            InitializeComponent();
        }
    }
}
