using System.Windows;

namespace MPF
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    /// <remarks>
    /// This application is not fully MVVM. The following steps are needed to get there:
    /// - Use commands instead of event handlers, where at all possible
    /// - Reduce the amount of manual UI adjustments needed, instead binding to the view models
    /// </remarks>
    public partial class App : Application
    {
    }
}
