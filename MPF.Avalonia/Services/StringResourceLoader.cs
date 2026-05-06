using System;
using System.Collections.Generic;
using Avalonia.Controls;
using Avalonia.Markup.Xaml.Styling;
using MPF.Frontend;

namespace MPF.Avalonia.Services
{
    internal static class StringResourceLoader
    {
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

        public static void Load(IResourceDictionary resources, InterfaceLanguage language)
        {
            resources.MergedDictionaries.Clear();

            AddResource(resources, "Strings.xaml");

            if (language == InterfaceLanguage.AutoDetect)
                language = FromCurrentCulture();

            if (language != InterfaceLanguage.English && LanguageFiles.TryGetValue(language, out string? fileName))
                AddResource(resources, fileName);
        }

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
