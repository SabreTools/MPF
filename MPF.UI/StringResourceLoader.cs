using System;
using System.Windows;
using MPF.Frontend;
using MPF.Frontend.Tools;

namespace MPF.UI
{
    /// <summary>
    /// Loads localized string resource dictionaries into the application based on interface language
    /// </summary>
    internal static class StringResourceLoader
    {
        /// <summary>
        /// Reset the merged dictionaries to the default English strings, then overlay the
        /// requested language (auto-detecting from the current culture when set to AutoDetect)
        /// </summary>
        public static void SetInterfaceLanguage(InterfaceLanguage lang)
        {
            // Reset the resources dictionary
            Application.Current.Resources.MergedDictionaries.Clear();

            // Auto detect language
            if (lang == InterfaceLanguage.AutoDetect)
                lang = FrontendTool.FromCurrentCulture();

            // Set baseline language (English), required as some translations may not translate all strings
            if (lang != InterfaceLanguage.English)
            {
                var baselineDictionary = new ResourceDictionary
                {
                    Source = new Uri("../Resources/Strings.xaml", UriKind.Relative)
                };
                Application.Current.Resources.MergedDictionaries.Add(baselineDictionary);
            }

            var dictionary = new ResourceDictionary
            {
#pragma warning disable IDE0072
                Source = lang switch
                {
                    InterfaceLanguage.English => new Uri("../Resources/Strings.xaml", UriKind.Relative),
                    InterfaceLanguage.French => new Uri("../Resources/Strings.fr.xaml", UriKind.Relative),
                    InterfaceLanguage.German => new Uri("../Resources/Strings.de.xaml", UriKind.Relative),
                    InterfaceLanguage.Italian => new Uri("../Resources/Strings.it.xaml", UriKind.Relative),
                    InterfaceLanguage.Japanese => new Uri("../Resources/Strings.ja.xaml", UriKind.Relative),
                    InterfaceLanguage.Korean => new Uri("../Resources/Strings.ko.xaml", UriKind.Relative),
                    InterfaceLanguage.Polish => new Uri("../Resources/Strings.pl.xaml", UriKind.Relative),
                    InterfaceLanguage.Portuguese => new Uri("../Resources/Strings.pt.xaml", UriKind.Relative),
                    InterfaceLanguage.Russian => new Uri("../Resources/Strings.ru.xaml", UriKind.Relative),
                    InterfaceLanguage.Spanish => new Uri("../Resources/Strings.es.xaml", UriKind.Relative),
                    InterfaceLanguage.Swedish => new Uri("../Resources/Strings.sv.xaml", UriKind.Relative),
                    InterfaceLanguage.Ukrainian => new Uri("../Resources/Strings.uk.xaml", UriKind.Relative),
                    InterfaceLanguage.L337 => new Uri("../Resources/Strings.37.xaml", UriKind.Relative),
                    _ => new Uri("../Resources/Strings.xaml", UriKind.Relative),
                }
#pragma warning restore IDE0072
            };
            Application.Current.Resources.MergedDictionaries.Add(dictionary);
        }
    }
}
