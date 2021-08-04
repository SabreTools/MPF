using System;
using MPF.GUI.ViewModels;

namespace MPF.Windows
{
    public partial class MainWindow : WindowBase
    {
        #region Fields

        /// <summary>
        /// Read-only access to the current main view model
        /// </summary>
        public MainViewModel MainViewModel => DataContext as MainViewModel;

        #endregion

        /// <summary>
        /// Constructor
        /// </summary>
        public MainWindow() => InitializeComponent();

        #region Event Handlers

        /// <summary>
        /// Handler for MainWindow OnContentRendered event
        /// </summary>
        protected override void OnContentRendered(EventArgs e)
        {
            base.OnContentRendered(e);
            MainViewModel.Init();
        }

        #endregion
    }
}
