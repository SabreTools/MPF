using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Linq;
using Avalonia.Controls;
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

        public static void LoadEnglish(IResourceDictionary resources)
            => Load(resources, InterfaceLanguage.English);

        public static void Load(IResourceDictionary resources, InterfaceLanguage language)
        {
            LoadFile(resources, "Strings.xaml");

            if (language == InterfaceLanguage.AutoDetect)
                language = FromCurrentCulture();

            if (language != InterfaceLanguage.English && LanguageFiles.TryGetValue(language, out string? fileName))
                LoadFile(resources, fileName);
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

        private static void LoadFile(IResourceDictionary resources, string fileName)
        {
            string baseDirectory = AppContext.BaseDirectory;
            string resourcePath = Path.Combine(baseDirectory, "Resources", fileName);
            if (!File.Exists(resourcePath))
                return;

            XDocument document = XDocument.Load(resourcePath);
            XNamespace xNamespace = "http://schemas.microsoft.com/winfx/2006/xaml";

            foreach (XElement element in document.Root?.Elements() ?? Array.Empty<XElement>())
            {
                XAttribute? keyAttribute = element.Attribute(xNamespace + "Key");
                if (keyAttribute is null)
                    continue;

                resources[keyAttribute.Value] = element.Value;
            }
        }
    }
}
