using System.Xml.Serialization;

namespace MPF.Core.Modules
{
    [XmlRoot("datafile")]
    public class Datafile
    {
        [XmlElement("header")]
#if NET48
        public Header Header;
#else
        public Header? Header;
#endif

        [XmlElement("game")]
#if NET48
        public Game[] Games;
#else
        public Game[]? Games;
#endif
    }

    public class Header
    {
        [XmlElement("name")]
#if NET48
        public string Name;
#else
        public string? Name;
#endif

        [XmlElement("description")]
#if NET48
        public string Description;
#else
        public string? Description;
#endif

        [XmlElement("version")]
#if NET48
        public string Version;
#else
        public string? Version;
#endif

        [XmlElement("date")]
#if NET48
        public string Date;
#else
        public string? Date;
#endif

        [XmlElement("author")]
#if NET48
        public string Author;
#else
        public string? Author;
#endif

        [XmlElement("homepage")]
#if NET48
        public string Homepage;
#else
        public string? Homepage;
#endif

        [XmlElement("url")]
#if NET48
        public string Url;
#else
        public string? Url;
#endif
    }

    public class Game
    {
        [XmlAttribute("name")]
#if NET48
        public string Name;
#else
        public string? Name;
#endif

        [XmlElement("category")]
#if NET48
        public string Category;
#else
        public string? Category;
#endif

        [XmlElement("description")]
#if NET48
        public string Description;
#else
        public string? Description;
#endif

        [XmlElement("rom")]
#if NET48
        public Rom[] Roms;
#else
        public Rom[]? Roms;
#endif
    }

    public class Rom
    {
        [XmlAttribute("name")]
#if NET48
        public string Name;
#else
        public string? Name;
#endif

        [XmlAttribute("size")]
#if NET48
        public string Size;
#else
        public string? Size;
#endif

        [XmlAttribute("crc")]
#if NET48
        public string Crc;
#else
        public string? Crc;
#endif

        [XmlAttribute("md5")]
#if NET48
        public string Md5;
#else
        public string? Md5;
#endif

        [XmlAttribute("sha1")]
#if NET48
        public string Sha1;
#else
        public string? Sha1;
#endif

        // TODO: Add extended hashes here
    }
}
