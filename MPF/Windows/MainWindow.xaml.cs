using System;
using MPF.UI.Core.Windows;
using MPF.UI.ViewModels;

namespace MPF.Windows
{
    public partial class MainWindow : WindowBase
    {
        /// <summary>
        /// Read-only access to the current main view model
        /// </summary>
        public MainViewModel MainViewModel => DataContext as MainViewModel;

        /// <summary>
        /// Constructor
        /// </summary>
        public MainWindow() => InitializeComponent();

        /// <summary>
        /// Handler for MainWindow OnContentRendered event
        /// </summary>
        protected override void OnContentRendered(EventArgs e)
        {
            base.OnContentRendered(e);
            MainViewModel.Init(App.Instance, App.Logger, App.Options);
        }
    }
}
