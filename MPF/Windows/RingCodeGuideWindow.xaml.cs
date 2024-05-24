namespace MPF.UI.Windows
{
    /// <summary>
    /// Interaction logic for RingCodeGuideWindow.xaml
    /// </summary>
    public partial class RingCodeGuideWindow : WindowBase
    {
        public RingCodeGuideWindow()
        {
#if NET40_OR_GREATER || NETCOREAPP
            InitializeComponent();
#endif

#if NET452_OR_GREATER || NETCOREAPP
            var chrome = new System.Windows.Shell.WindowChrome
            {
                CaptionHeight = 0,
                ResizeBorderThickness = new System.Windows.Thickness(0),
            };
            System.Windows.Shell.WindowChrome.SetWindowChrome(this, chrome);
#endif
        }
    }
}
