using System;
using Avalonia.Controls;
using Avalonia.Markup.Xaml.Styling;
using MPF.Frontend;
using MPF.Frontend.Tools;

namespace MPF.Avalonia
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
        public static void SetInterfaceLanguage(IResourceDictionary resources, InterfaceLanguage lang)
        {
            // Reset the resources dictionary
            resources.MergedDictionaries.Clear();

            // Auto detect language
            if (lang == InterfaceLanguage.AutoDetect)
                lang = FrontendTool.FromCurrentCulture();

            // Get the executing assembly name for loading
            string assemblyName = typeof(StringResourceLoader).Assembly.GetName().Name ?? "MPF";

            // Set baseline language (English), required as some translations may not translate all strings
            if (lang != InterfaceLanguage.English)
            {
                var engUri = new Uri($"avares://{assemblyName}/Assets/Strings.xaml");
                var baselineDictionary = new ResourceInclude(engUri)
                {
                    Source = engUri
                };
                resources.MergedDictionaries.Add(baselineDictionary);
            }

#pragma warning disable IDE0072
            var uri = lang switch
            {
                InterfaceLanguage.English => new Uri($"avares://{assemblyName}/Assets/Strings.xaml"),
                InterfaceLanguage.French => new Uri($"avares://{assemblyName}/Assets/Strings.fr.xaml"),
                InterfaceLanguage.German => new Uri($"avares://{assemblyName}/Assets/Strings.de.xaml"),
                InterfaceLanguage.Italian => new Uri($"avares://{assemblyName}/Assets/Strings.it.xaml"),
                InterfaceLanguage.Japanese => new Uri($"avares://{assemblyName}/Assets/Strings.ja.xaml"),
                InterfaceLanguage.Korean => new Uri($"avares://{assemblyName}/Assets/Strings.ko.xaml"),
                InterfaceLanguage.Polish => new Uri($"avares://{assemblyName}/Assets/Strings.pl.xaml"),
                InterfaceLanguage.Portuguese => new Uri($"avares://{assemblyName}/Assets/Strings.pt.xaml"),
                InterfaceLanguage.Russian => new Uri($"avares://{assemblyName}/Assets/Strings.ru.xaml"),
                InterfaceLanguage.Spanish => new Uri($"avares://{assemblyName}/Assets/Strings.es.xaml"),
                InterfaceLanguage.Swedish => new Uri($"avares://{assemblyName}/Assets/Strings.sv.xaml"),
                InterfaceLanguage.Ukrainian => new Uri($"avares://{assemblyName}/Assets/Strings.uk.xaml"),
                InterfaceLanguage.L337 => new Uri($"avares://{assemblyName}/Assets/Strings.37.xaml"),
                _ => new Uri($"avares://{assemblyName}/Assets/Strings.xaml"),
            };
#pragma warning restore IDE0072

            var dictionary = new ResourceInclude(uri)
            {
                Source = uri
            };

            resources.MergedDictionaries.Add(dictionary);
        }
    }
}
