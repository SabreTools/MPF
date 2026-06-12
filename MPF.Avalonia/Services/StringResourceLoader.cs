using System;
using System.Collections.Generic;
using Avalonia.Controls;
using Avalonia.Markup.Xaml.Styling;
using MPF.Frontend;

namespace MPF.Avalonia.Services
{
    /// <summary>
    /// Loads localized string resource dictionaries into the application based on interface language
    /// </summary>
    internal static class StringResourceLoader
    {
        /// <summary>
        /// Maps each supported interface language to its resource dictionary file name
        /// </summary>
        private static readonly Dictionary<InterfaceLanguage, string> LanguageFiles = new()
        {
            [InterfaceLanguage.English] = "Strings.xaml",
            [InterfaceLanguage.French] = "Strings.fr.xaml",
            [InterfaceLanguage.German] = "Strings.de.xaml",
            [InterfaceLanguage.Italian] = "Strings.it.xaml",
            [InterfaceLanguage.Japanese] = "Strings.ja.xaml",
            [InterfaceLanguage.Korean] = "Strings.ko.xaml",
            [InterfaceLanguage.Polish] = "Strings.pl.xaml",
            [InterfaceLanguage.Portuguese] = "Strings.pt.xaml",
            [InterfaceLanguage.Russian] = "Strings.ru.xaml",
            [InterfaceLanguage.Spanish] = "Strings.es.xaml",
            [InterfaceLanguage.Swedish] = "Strings.sv.xaml",
            [InterfaceLanguage.Ukrainian] = "Strings.uk.xaml",
            [InterfaceLanguage.L337] = "Strings.37.xaml",
        };

        /// <summary>
        /// Reset the merged dictionaries to the default English strings, then overlay the
        /// requested language (auto-detecting from the current culture when set to AutoDetect)
        /// </summary>
        public static void Load(IResourceDictionary resources, InterfaceLanguage language)
        {
            resources.MergedDictionaries.Clear();

            AddResource(resources, "Strings.xaml");

            if (language == InterfaceLanguage.AutoDetect)
                language = FromCurrentCulture();

            if (language != InterfaceLanguage.English && LanguageFiles.TryGetValue(language, out string? fileName))
                AddResource(resources, fileName);
        }

        /// <summary>
        /// Build the embedded resource URI for the given file and merge it into the dictionary
        /// </summary>
        private static void AddResource(IResourceDictionary resources, string fileName)
        {
            string assemblyName = typeof(StringResourceLoader).Assembly.GetName().Name ?? "MPF";
            var uri = new Uri($"avares://{assemblyName}/Assets/{fileName}");

            var resourceInclude = new ResourceInclude(uri)
            {
                Source = uri
            };

            resources.MergedDictionaries.Add(resourceInclude);
        }

        /// <summary>
        /// Map the current UI culture's two-letter ISO language name to a supported interface language
        /// </summary>
        private static InterfaceLanguage FromCurrentCulture()
        {
            return System.Globalization.CultureInfo.CurrentUICulture.TwoLetterISOLanguageName switch
            {
                "fr" => InterfaceLanguage.French,
                "de" => InterfaceLanguage.German,
                "it" => InterfaceLanguage.Italian,
                "ja" => InterfaceLanguage.Japanese,
                "ko" => InterfaceLanguage.Korean,
                "pl" => InterfaceLanguage.Polish,
                "pt" => InterfaceLanguage.Portuguese,
                "ru" => InterfaceLanguage.Russian,
                "es" => InterfaceLanguage.Spanish,
                "sv" => InterfaceLanguage.Swedish,
                "uk" => InterfaceLanguage.Ukrainian,
                _ => InterfaceLanguage.English,
            };
        }
    }
}
