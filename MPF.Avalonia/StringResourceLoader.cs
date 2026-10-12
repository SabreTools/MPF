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
                var baselineDictionary = new ResourceInclude(engUri) { Source = engUri };
                resources.MergedDictionaries.Add(baselineDictionary);
            }

            // Determine the XAML path and load
            string xamlPath = FrontendTool.GetLanguageXamlFilename(lang);
            var uri = new Uri($"avares://{assemblyName}/Assets/{xamlPath}");
            var dictionary = new ResourceInclude(uri) { Source = uri };

            // Merge the new language dictionary
            resources.MergedDictionaries.Add(dictionary);
        }
    }
}
