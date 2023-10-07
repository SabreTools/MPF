using System.Xml.Serialization;

namespace MPF.Core.Modules
{
    [XmlRoot("datafile")]
    public class Datafile
    {
        [XmlElement("header")]
        public Header Header;

        [XmlElement("game")]
        public Game[] Games;
    }

    public class Header
    {
        [XmlElement("name")]
        public string Name;

        [XmlElement("description")]
        public string Description;

        [XmlElement("version")]
        public string Version;

        [XmlElement("date")]
        public string Date;

        [XmlElement("author")]
        public string Author;

        [XmlElement("homepage")]
        public string Homepage;

        [XmlElement("url")]
        public string Url;
    }

    public class Game
    {
        [XmlAttribute("name")]
        public string Name;

        [XmlElement("category")]
        public string Category;

        [XmlElement("description")]
        public string Description;

        [XmlElement("rom")]
        public Rom[] Roms;
    }

    public class Rom
    {
        [XmlAttribute("name")]
        public string Name;

        [XmlAttribute("size")]
        public string Size;

        [XmlAttribute("crc")]
        public string Crc;

        [XmlAttribute("md5")]
        public string Md5;

        [XmlAttribute("sha1")]
        public string Sha1;

        // TODO: Add extended hashes here
    }
}
