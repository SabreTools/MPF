using RedumpLib.Attributes;

namespace RedumpLib.Data
{
    /// <summary>
    /// List of all disc categories
    /// </summary>
    public enum DiscCategory
    {
        [HumanReadable(LongName = "Games")]
        Games = 1,

        [HumanReadable(LongName = "Demos")]
        Demos = 2,

        [HumanReadable(LongName = "Video")]
        Video = 3,

        [HumanReadable(LongName = "Audio")]
        Audio = 4,

        [HumanReadable(LongName = "Multimedia")]
        Multimedia = 5,

        [HumanReadable(LongName = "Applications")]
        Applications = 6,

        [HumanReadable(LongName = "Coverdiscs")]
        Coverdiscs = 7,

        [HumanReadable(LongName = "Educational")]
        Educational = 8,

        [HumanReadable(LongName = "Bonus Discs")]
        BonusDiscs = 9,

        [HumanReadable(LongName = "Preproduction")]
        Preproduction = 10,

        [HumanReadable(LongName = "Add-Ons")]
        AddOns = 11,
    }

    /// <summary>
    /// List of all disc types
    /// </summary>
    /// <remarks>
    /// All names here match Redump names for the types, not official
    /// naming. Some names had to be extrapolated due to no current support
    /// in the Redump site.
    /// </remarks>
    public enum DiscType
    {
        NONE = 0,

        [HumanReadable(LongName = "BD-25")]
        BD25,

        //[HumanReadable(LongName = "BD-33")]
        //BD33,

        [HumanReadable(LongName = "BD-50")]
        BD50,

        //[HumanReadable(LongName = "BD-66")]
        //BD66,

        [HumanReadable(LongName = "BD-100")]
        BD100,

        [HumanReadable(LongName = "BD-128")]
        BD128,

        [HumanReadable(LongName = "CD")]
        CD,

        [HumanReadable(LongName = "DVD-5")]
        DVD5,

        [HumanReadable(LongName = "DVD-9")]
        DVD9,

        [HumanReadable(LongName = "GD-ROM")]
        GDROM,

        [HumanReadable(LongName = "HD-DVD SL")]
        HDDVDSL,

        [HumanReadable(LongName = "HD-DVD DL")]
        HDDVDDL,

        [HumanReadable(LongName = "MIL-CD")]
        MILCD,

        [HumanReadable(LongName = "Nintendo GameCube Game Disc")]
        NintendoGameCubeGameDisc,

        [HumanReadable(LongName = "Nintendo Wii Optical Disc SL")]
        NintendoWiiOpticalDiscSL,

        [HumanReadable(LongName = "Nintendo Wii Optical Disc DL")]
        NintendoWiiOpticalDiscDL,

        [HumanReadable(LongName = "Nintendo Wii U Optical Disc SL")]
        NintendoWiiUOpticalDiscSL,

        [HumanReadable(LongName = "UMD SL")]
        UMDSL,

        [HumanReadable(LongName = "UMD DL")]
        UMDDL,
    }

    /// <summary>
    /// Dump status
    /// </summary>
    public enum DumpStatus
    {
        BadDumpRed = 2,
        PossibleBadDumpYellow = 3,
        OriginalMediaBlue = 4,
        TwoOrMoreGreen = 5,
    }

    /// <summary>
    /// Determines what download type to initate
    /// </summary>
    public enum Feature
    {
        NONE,
        Site,
        WIP,
        Packs,
        User,
        Quicksearch,
    }

    /// <summary>
    /// List of all disc langauges
    /// </summary>
    /// <remarks>https://www.loc.gov/standards/iso639-2/php/code_list.php</remarks>
    public enum Language
    {
        #region A

        [Language(LongName = "Abkhazian", TwoLetterCode = "ab", ThreeLetterCode = "abk")]
        Abkhazian,

        [Language(LongName = "Achinese", ThreeLetterCode = "ace")]
        Achinese,

        [Language(LongName = "Acoli", ThreeLetterCode = "ach")]
        Acoli,

        [Language(LongName = "Adangme", ThreeLetterCode = "ada")]
        Adangme,

        // Adyghe; Adygei
        [Language(LongName = "Adyghe", ThreeLetterCode = "ady")]
        Adyghe,

        [Language(LongName = "Afar", TwoLetterCode = "aa", ThreeLetterCode = "aar")]
        Afar,

        [Language(LongName = "Afrihili", ThreeLetterCode = "afh")]
        Afrihili,

        [Language(LongName = "Afrikaans", TwoLetterCode = "af", ThreeLetterCode = "afr")]
        Afrikaans,

        [Language(LongName = "Ainu", ThreeLetterCode = "ain")]
        Ainu,

        [Language(LongName = "Akan", TwoLetterCode = "ak", ThreeLetterCode = "aka")]
        Akan,

        [Language(LongName = "Akkadian", ThreeLetterCode = "akk")]
        Akkadian,

        [Language(LongName = "Albanian", TwoLetterCode = "sq", ThreeLetterCode = "alb", ThreeLetterCodeAlt = "sqi")]
        Albanian,

        [Language(LongName = "Aleut", ThreeLetterCode = "ale")]
        Aleut,

        [Language(LongName = "Amharic", TwoLetterCode = "am", ThreeLetterCode = "amh")]
        Amharic,

        [Language(LongName = "Angika", ThreeLetterCode = "anp")]
        Angika,

        [Language(LongName = "Arabic", TwoLetterCode = "ar", ThreeLetterCode = "ara")]
        Arabic,

        [Language(LongName = "Aragonese", TwoLetterCode = "an", ThreeLetterCode = "arg")]
        Aragonese,

        // Official Aramaic (700-300 BCE); Imperial Aramaic (700-300 BCE)
        [Language(LongName = "Aramaic", ThreeLetterCode = "arc")]
        Aramaic,

        [Language(LongName = "Armenian", TwoLetterCode = "hy", ThreeLetterCode = "arm", ThreeLetterCodeAlt = "hye")]
        Armenian,

        [Language(LongName = "Arapaho", ThreeLetterCode = "arp")]
        Arapaho,

        // Aromanian; Arumanian; Macedo-Romanian
        [Language(LongName = "Aromanian", ThreeLetterCode = "rup")]
        Aromanian,

        [Language(LongName = "Arawak", ThreeLetterCode = "arw")]
        Arawak,

        [Language(LongName = "Assamese", TwoLetterCode = "as", ThreeLetterCode = "asm")]
        Assamese,

        // Asturian; Bable; Leonese; Asturleonese
        [Language(LongName = "Asturian", ThreeLetterCode = "ast")]
        Asturian,

        [Language(LongName = "Athapascan", ThreeLetterCode = "den")]
        Athapascan,

        [Language(LongName = "Avaric", TwoLetterCode = "av", ThreeLetterCode = "ava")]
        Avaric,

        [Language(LongName = "Avestan", TwoLetterCode = "ae", ThreeLetterCode = "ave")]
        Avestan,

        [Language(LongName = "Awadhi", ThreeLetterCode = "awa")]
        Awadhi,

        [Language(LongName = "Aymara", TwoLetterCode = "ay", ThreeLetterCode = "aym")]
        Aymara,

        [Language(LongName = "Azerbaijani", TwoLetterCode = "az", ThreeLetterCode = "aze")]
        Azerbaijani,

        #endregion

        #region B

        [Language(LongName = "Balinese", ThreeLetterCode = "ban")]
        Balinese,

        [Language(LongName = "Baluchi", ThreeLetterCode = "bal")]
        Baluchi,

        [Language(LongName = "Bambara", TwoLetterCode = "bm", ThreeLetterCode = "bam")]
        Bambara,

        [Language(LongName = "Basa", ThreeLetterCode = "bas")]
        Basa,

        [Language(LongName = "Bashkir", TwoLetterCode = "ba", ThreeLetterCode = "bak")]
        Bashkir,

        [Language(LongName = "Basque", TwoLetterCode = "eu", ThreeLetterCode = "baq", ThreeLetterCodeAlt = "eus")]
        Basque,

        // Beja; Bedawiyet
        [Language(LongName = "Beja", ThreeLetterCode = "bej")]
        Beja,

        [Language(LongName = "Belarusian", TwoLetterCode = "be", ThreeLetterCode = "bel")]
        Belarusian,

        [Language(LongName = "Bemba", ThreeLetterCode = "bem")]
        Bemba,

        [Language(LongName = "Bengali", TwoLetterCode = "bn", ThreeLetterCode = "ben")]
        Bengali,

        [Language(LongName = "Bhojpuri", ThreeLetterCode = "bho")]
        Bhojpuri,

        [Language(LongName = "Bikol", ThreeLetterCode = "bik")]
        Bikol,

        [Language(LongName = "Bini; Edo", ThreeLetterCode = "bin")]
        Bini,

        [Language(LongName = "Bislama", TwoLetterCode = "bla", ThreeLetterCode = "bis")]
        Bislama,

        // Blin; Bilin
        [Language(LongName = "Blin", ThreeLetterCode = "byn")]
        Blin,

        // Blissymbols; Blissymbolics; Bliss
        [Language(LongName = "Blissymbols", ThreeLetterCode = "zbl")]
        Blissymbols,

        [Language(LongName = "Bosnian", TwoLetterCode = "bs", ThreeLetterCode = "bos")]
        Bosnian,

        [Language(LongName = "Braj", ThreeLetterCode = "bra")]
        Braj,

        [Language(LongName = "Breton", TwoLetterCode = "br", ThreeLetterCode = "bre")]
        Breton,

        [Language(LongName = "Buginese", ThreeLetterCode = "bug")]
        Buginese,

        [Language(LongName = "Bulgarian", TwoLetterCode = "bg", ThreeLetterCode = "bul")]
        Bulgarian,

        [Language(LongName = "Buriat", ThreeLetterCode = "bua")]
        Buriat,

        [Language(LongName = "Burmese", TwoLetterCode = "my", ThreeLetterCode = "bur", ThreeLetterCodeAlt = "mya")]
        Burmese,

        #endregion

        #region C

        [Language(LongName = "Caddo", ThreeLetterCode = "cad")]
        Caddo,

        // Catalan; Valencian
        [Language(LongName = "Catalan", TwoLetterCode = "ca", ThreeLetterCode = "cat")]
        Catalan,

        [Language(LongName = "Cebuano", ThreeLetterCode = "ceb")]
        Cebuano,

        [Language(LongName = "Central Khmer", TwoLetterCode = "km", ThreeLetterCode = "khm")]
        CentralKhmer,

        [Language(LongName = "Chagatai", ThreeLetterCode = "chg")]
        Chagatai,

        [Language(LongName = "Chamorro", TwoLetterCode = "ch", ThreeLetterCode = "cha")]
        Chamorro,

        [Language(LongName = "Chechen", TwoLetterCode = "ce", ThreeLetterCode = "che")]
        Chechen,

        [Language(LongName = "Cherokee", ThreeLetterCode = "chr")]
        Cherokee,

        [Language(LongName = "Cheyenne", ThreeLetterCode = "chy")]
        Cheyenne,

        [Language(LongName = "Chibcha", ThreeLetterCode = "chb")]
        Chibcha,

        // Chichewa; Chewa; Nyanja
        [Language(LongName = "Chichewa", TwoLetterCode = "ny", ThreeLetterCode = "nya")]
        Chichewa,

        [Language(LongName = "Chinese", TwoLetterCode = "zh", ThreeLetterCode = "chi", ThreeLetterCodeAlt = "zho")]
        Chinese,

        [Language(LongName = "Chinook jargon", ThreeLetterCode = "chn")]
        ChinookJargon,

        // Chipewyan; Dene Suline
        [Language(LongName = "Chipewyan", ThreeLetterCode = "chp")]
        Chipewyan,

        [Language(LongName = "Choctaw", ThreeLetterCode = "cho")]
        Choctaw,

        // Church Slavic; Old Slavonic; Church Slavonic; Old Bulgarian; Old Church Slavonic
        [Language(LongName = "Church Slavic", TwoLetterCode = "cu", ThreeLetterCode = "chu")]
        ChurchSlavic,

        [Language(LongName = "Chuukese", ThreeLetterCode = "chk")]
        Chuukese,

        [Language(LongName = "Chuvash", TwoLetterCode = "cv", ThreeLetterCode = "chv")]
        Chuvash,

        // Classical Newari; Old Newari; Classical Nepal Bhasa
        [Language(LongName = "Classical Newari", ThreeLetterCode = "nwc")]
        ClassicalNewari,

        [Language(LongName = "Coptic", ThreeLetterCode = "cop")]
        Coptic,

        [Language(LongName = "Cornish", TwoLetterCode = "kw", ThreeLetterCode = "cor")]
        Cornish,

        [Language(LongName = "Corsican", TwoLetterCode = "co", ThreeLetterCode = "cos")]
        Corsican,

        [Language(LongName = "Cree", TwoLetterCode = "cr", ThreeLetterCode = "cre")]
        Cree,

        [Language(LongName = "Creek", ThreeLetterCode = "mus")]
        Creek,

        [Language(LongName = "Creoles and pidgins", ThreeLetterCode = "crp")]
        CreolesAndPidgins,

        [Language(LongName = "Creoles and pidgins (English-based)", ThreeLetterCode = "cpe")]
        EnglishCreole,

        [Language(LongName = "Creoles and pidgins (French-based)", ThreeLetterCode = "cpf")]
        FrenchCreole,

        [Language(LongName = "Creoles and pidgins (Portuguese-based)", ThreeLetterCode = "cpp")]
        PortugueseCreole,

        // Crimean Tatar; Crimean Turkish
        [Language(LongName = "Crimean Tatar", ThreeLetterCode = "crh")]
        CrimeanTatar,

        [Language(LongName = "Croatian", TwoLetterCode = "hr", ThreeLetterCode = "hrv")]
        Croatian,

        [Language(LongName = "Czech", TwoLetterCode = "cs", ThreeLetterCode = "cze", ThreeLetterCodeAlt = "ces")]
        Czech,

        #endregion

        #region D

        [Language(LongName = "Dakota", ThreeLetterCode = "dak")]
        Dakota,

        [Language(LongName = "Danish", TwoLetterCode = "da", ThreeLetterCode = "dan")]
        Danish,

        [Language(LongName = "Dargwa", ThreeLetterCode = "dar")]
        Dargwa,

        [Language(LongName = "Delaware", ThreeLetterCode = "del")]
        Delaware,

        [Language(LongName = "Dinka", ThreeLetterCode = "din")]
        Dinka,

        // Divehi; Dhivehi; Maldivian
        [Language(LongName = "Divehi", TwoLetterCode = "dv", ThreeLetterCode = "div")]
        Divehi,

        [Language(LongName = "Dogrib", ThreeLetterCode = "dgr")]
        Dogrib,

        [Language(LongName = "Dogri", ThreeLetterCode = "doi")]
        Dogri,

        [Language(LongName = "Duala", ThreeLetterCode = "dua")]
        Duala,

        // Dutch; Flemish
        [Language(LongName = "Dutch", TwoLetterCode = "nl", ThreeLetterCode = "dut", ThreeLetterCodeAlt = "nld")]
        Dutch,

        [Language(LongName = "Dutch, Middle (ca.1050-1350)", ThreeLetterCode = "dum")]
        MiddleDutch,

        [Language(LongName = "Dyula", ThreeLetterCode = "dyu")]
        Dyula,

        [Language(LongName = "Dzongkha", TwoLetterCode = "dz", ThreeLetterCode = "dzo")]
        Dzongkha,

        #endregion

        #region E

        [Language(LongName = "Eastern Frisian", ThreeLetterCode = "frs")]
        EasternFrisian,

        [Language(LongName = "Efik", ThreeLetterCode = "efi")]
        Efik,

        [Language(LongName = "Egyptian (Ancient)", ThreeLetterCode = "egy")]
        AncientEgyptian,

        [Language(LongName = "Ekajuk", ThreeLetterCode = "eka")]
        Ekajuk,

        [Language(LongName = "Elamite", ThreeLetterCode = "elx")]
        Elamite,

        [Language(LongName = "English", TwoLetterCode = "en", ThreeLetterCode = "eng")]
        English,

        [Language(LongName = "English, Old (ca.450-1100)", ThreeLetterCode = "ang")]
        OldEnglish,

        [Language(LongName = "English, Middle (1100-1500)", ThreeLetterCode = "enm")]
        MiddleEnglish,

        [Language(LongName = "Erzya", ThreeLetterCode = "myv")]
        Erzya,

        [Language(LongName = "Esperanto", TwoLetterCode = "eo", ThreeLetterCode = "epo")]
        Esperanto,

        [Language(LongName = "Estonian", TwoLetterCode = "et", ThreeLetterCode = "est")]
        Estonian,

        [Language(LongName = "Ewe", TwoLetterCode = "ee", ThreeLetterCode = "ewe")]
        Ewe,

        [Language(LongName = "Ewondo", ThreeLetterCode = "ewo")]
        Ewondo,

        #endregion

        #region F

        [Language(LongName = "Fang", ThreeLetterCode = "fan")]
        Fang,

        [Language(LongName = "Faroese", TwoLetterCode = "fo", ThreeLetterCode = "fao")]
        Faroese,

        [Language(LongName = "Fanti", ThreeLetterCode = "fat")]
        Fanti,

        [Language(LongName = "Fijian", TwoLetterCode = "fj", ThreeLetterCode = "fij")]
        Fijian,

        // Filipino; Pilipino
        [Language(LongName = "Filipino", ThreeLetterCode = "fil")]
        Filipino,

        [Language(LongName = "Finnish", TwoLetterCode = "fi", ThreeLetterCode = "fin")]
        Finnish,

        [Language(LongName = "Fon", ThreeLetterCode = "fon")]
        Fon,

        [Language(LongName = "French", TwoLetterCode = "fr", ThreeLetterCode = "fre", ThreeLetterCodeAlt = "fra")]
        French,

        [Language(LongName = "French, Middle (ca.1400-1600)", ThreeLetterCode = "frm")]
        MiddleFrench,

        [Language(LongName = "French, Old (842-ca.1400)", ThreeLetterCode = "fro")]
        OldFrench,

        [Language(LongName = "Friulian", ThreeLetterCode = "fur")]
        Friulian,

        [Language(LongName = "Fulah", TwoLetterCode = "ff", ThreeLetterCode = "ful")]
        Fulah,

        #endregion

        #region G

        [Language(LongName = "Ga", ThreeLetterCode = "gaa")]
        Ga,

        // Gaelic; Scottish Gaelic
        [Language(LongName = "Gaelic", TwoLetterCode = "gd", ThreeLetterCode = "gla")]
        Gaelic,

        [Language(LongName = "Galibi Carib", ThreeLetterCode = "car")]
        GalibiCarib,

        [Language(LongName = "Galician", TwoLetterCode = "gl", ThreeLetterCode = "glg")]
        Galician,

        [Language(LongName = "Ganda", TwoLetterCode = "lg", ThreeLetterCode = "lug")]
        Ganda,

        [Language(LongName = "Gayo", ThreeLetterCode = "gay")]
        Gayo,

        [Language(LongName = "Gbaya", ThreeLetterCode = "gba")]
        Gbaya,

        [Language(LongName = "Geez", ThreeLetterCode = "gez")]
        Geez,

        [Language(LongName = "Georgian", TwoLetterCode = "ka", ThreeLetterCode = "geo", ThreeLetterCodeAlt = "kat")]
        Georgian,

        [Language(LongName = "German", TwoLetterCode = "de", ThreeLetterCode = "ger", ThreeLetterCodeAlt = "deu")]
        German,

        [Language(LongName = "German, Middle High (ca.1050-1500)", ThreeLetterCode = "gmh")]
        MiddleHighGerman,

        [Language(LongName = "German, Old High (ca.750-1050)", ThreeLetterCode = "goh")]
        OldHighGerman,

        [Language(LongName = "Gilbertese", ThreeLetterCode = "gil")]
        Gilbertese,

        [Language(LongName = "Gondi", ThreeLetterCode = "gon")]
        Gondi,

        [Language(LongName = "Gorontalo", ThreeLetterCode = "gor")]
        Gorontalo,

        [Language(LongName = "Gothic", ThreeLetterCode = "got")]
        Gothic,

        [Language(LongName = "Grebo", ThreeLetterCode = "grb")]
        Grebo,

        [Language(LongName = "Greek", TwoLetterCode = "el", ThreeLetterCode = "gre", ThreeLetterCodeAlt = "eli")]
        Greek,

        [Language(LongName = "Greek, Ancient (to 1453)", ThreeLetterCode = "grc")]
        AncientGreek,

        [Language(LongName = "Guarani", TwoLetterCode = "gn", ThreeLetterCode = "grn")]
        Guarani,

        [Language(LongName = "Gujarati", TwoLetterCode = "gu", ThreeLetterCode = "guj")]
        Gujarati,

        [Language(LongName = "Gwich'in", ThreeLetterCode = "gwi")]
        Gwichin,

        #endregion

        #region H

        [Language(LongName = "Haida", ThreeLetterCode = "hai")]
        Haida,

        // Haitian; Haitian Creole
        [Language(LongName = "Haitian", TwoLetterCode = "ht", ThreeLetterCode = "hat")]
        Haitian,

        [Language(LongName = "Hausa", TwoLetterCode = "ha", ThreeLetterCode = "gau")]
        Hausa,

        [Language(LongName = "Hawaiian", ThreeLetterCode = "haw")]
        Hawaiian,

        [Language(LongName = "Hebrew", TwoLetterCode = "he", ThreeLetterCode = "heb")]
        Hebrew,

        [Language(LongName = "Herero", TwoLetterCode = "hz", ThreeLetterCode = "her")]
        Herero,

        [Language(LongName = "Hiligaynon", ThreeLetterCode = "hil")]
        Hiligaynon,

        [Language(LongName = "Hindi", TwoLetterCode = "hi", ThreeLetterCode = "hin")]
        Hindi,

        [Language(LongName = "Hiri Motu", ThreeLetterCode = "hmo")]
        HiriMotu,

        [Language(LongName = "Hittite", ThreeLetterCode = "hit")]
        Hittite,

        // Hmong; Mong
        [Language(LongName = "Hmong", ThreeLetterCode = "hmn")]
        Hmong,

        [Language(LongName = "Hungarian", TwoLetterCode = "hu", ThreeLetterCode = "hun")]
        Hungarian,

        [Language(LongName = "Hupa", ThreeLetterCode = "hup")]
        Hupa,

        #endregion

        #region I

        [Language(LongName = "Iban", ThreeLetterCode = "iba")]
        Iban,

        [Language(LongName = "Icelandic", TwoLetterCode = "is", ThreeLetterCode = "ice", ThreeLetterCodeAlt = "isl")]
        Icelandic,

        [Language(LongName = "Ido", TwoLetterCode = "io", ThreeLetterCode = "ido")]
        Ido,

        [Language(LongName = "Igbo", TwoLetterCode = "ig", ThreeLetterCode = "ibo")]
        Igbo,

        [Language(LongName = "Iloko", ThreeLetterCode = "ilo")]
        Iloko,

        [Language(LongName = "Inari Sami", ThreeLetterCode = "smn")]
        InariSami,

        [Language(LongName = "Indonesian", TwoLetterCode = "id", ThreeLetterCode = "ind")]
        Indonesian,

        [Language(LongName = "Ingush", ThreeLetterCode = "inh")]
        Ingush,

        // Interlingua (International Auxiliary Language Association)
        [Language(LongName = "Interlingua", TwoLetterCode = "ia", ThreeLetterCode = "ina")]
        Interlingua,

        // Interlingue; Occidental
        [Language(LongName = "Interlingue", TwoLetterCode = "ie", ThreeLetterCode = "ile")]
        Interlingue,

        [Language(LongName = "Inuktitut", TwoLetterCode = "iu", ThreeLetterCode = "iku")]
        Inuktitut,

        [Language(LongName = "Inupiaq", TwoLetterCode = "ik", ThreeLetterCode = "ipk")]
        Inupiaq,

        [Language(LongName = "Irish", TwoLetterCode = "ga", ThreeLetterCode = "gle")]
        Irish,

        [Language(LongName = "Irish, Middle (900-1200)", ThreeLetterCode = "mga")]
        MiddleIrish,

        [Language(LongName = "Irish, Old (to 900)", ThreeLetterCode = "sga")]
        OldIrish,

        [Language(LongName = "Italian", TwoLetterCode = "it", ThreeLetterCode = "ita")]
        Italian,

        #endregion

        #region J

        [Language(LongName = "Japanese", TwoLetterCode = "ja", ThreeLetterCode = "jap")]
        Japanese,

        [Language(LongName = "Javanese", TwoLetterCode = "jv", ThreeLetterCode = "jav")]
        Javanese,

        [Language(LongName = "Judeo-Arabic", ThreeLetterCode = "jrb")]
        JudeoArabic,

        [Language(LongName = "Judeo-Persian", ThreeLetterCode = "jpr")]
        JudeoPersian,

        #endregion

        #region K

        [Language(LongName = "Kabardian", ThreeLetterCode = "kbd")]
        Kabardian,

        [Language(LongName = "Kabyle", ThreeLetterCode = "kab")]
        Kabyle,

        // Kachin; Jingpho
        [Language(LongName = "Kachin", ThreeLetterCode = "kac")]
        Kachin,

        // Kalaallisut; Greenlandic
        [Language(LongName = "Kalaallisut", TwoLetterCode = "kl", ThreeLetterCode = "kal")]
        Kalaallisut,

        // Kalmyk; Oirat
        [Language(LongName = "Kalmyk", ThreeLetterCode = "xal")]
        Kalmyk,

        [Language(LongName = "Kamba", ThreeLetterCode = "kam")]
        Kamba,

        [Language(LongName = "Kannada", TwoLetterCode = "kn", ThreeLetterCode = "kan")]
        Kannada,

        [Language(LongName = "Kanuri", TwoLetterCode = "kr", ThreeLetterCode = "kau")]
        Kanuri,

        [Language(LongName = "Karachay-Balkar", ThreeLetterCode = "krc")]
        KarachayBalkar,

        [Language(LongName = "Kara-Kalpak", ThreeLetterCode = "kaa")]
        KaraKalpak,

        [Language(LongName = "Karelian", ThreeLetterCode = "krl")]
        Karelian,

        [Language(LongName = "Kashmiri", TwoLetterCode = "ks", ThreeLetterCode = "kas")]
        Kashmiri,

        [Language(LongName = "Kashubian", ThreeLetterCode = "csb")]
        Kashubian,

        [Language(LongName = "Kawi", ThreeLetterCode = "kaw")]
        Kawi,

        [Language(LongName = "Kazakh", TwoLetterCode = "kk", ThreeLetterCode = "kaz")]
        Kazakh,

        [Language(LongName = "Khasi", ThreeLetterCode = "kha")]
        Khasi,

        // Khotanese; Sakan
        [Language(LongName = "Khotanese", ThreeLetterCode = "kho")]
        Khotanese,

        // Kikuyu; Gikuyu
        [Language(LongName = "Kikuyu", TwoLetterCode = "ki", ThreeLetterCode = "kik")]
        Kikuyu,

        [Language(LongName = "Kimbundu", ThreeLetterCode = "kmb")]
        Kimbundu,

        [Language(LongName = "Kinyarwanda", TwoLetterCode = "rw", ThreeLetterCode = "kin")]
        Kinyarwanda,

        // Kirghiz; Kyrgyz
        [Language(LongName = "Kirghiz", TwoLetterCode = "ky", ThreeLetterCode = "kir")]
        Kirghiz,

        // Klingon; tlhIngan-Hol
        [Language(LongName = "Klingon", ThreeLetterCode = "tlh")]
        Klingon,

        [Language(LongName = "Komi", TwoLetterCode = "kv", ThreeLetterCode = "kom")]
        Komi,

        [Language(LongName = "Kongo", TwoLetterCode = "kg", ThreeLetterCode = "kon")]
        Kongo,

        [Language(LongName = "Konkani", ThreeLetterCode = "kok")]
        Konkani,

        [Language(LongName = "Korean", TwoLetterCode = "ko", ThreeLetterCode = "kor")]
        Korean,

        [Language(LongName = "Kosraean", ThreeLetterCode = "kos")]
        Kosraean,

        [Language(LongName = "Kpelle", ThreeLetterCode = "kpe")]
        Kpelle,

        // Kuanyama; Kwanyama
        [Language(LongName = "Kuanyama", TwoLetterCode = "kj", ThreeLetterCode = "kua")]
        Kuanyama,

        [Language(LongName = "Kumyk", ThreeLetterCode = "kum")]
        Kumyk,

        [Language(LongName = "Kurdish", TwoLetterCode = "ku", ThreeLetterCode = "kur")]
        Kurdish,

        [Language(LongName = "Kurukh", ThreeLetterCode = "kru")]
        Kurukh,

        [Language(LongName = "Kutenai", ThreeLetterCode = "kut")]
        Kutenai,

        #endregion

        #region L

        [Language(LongName = "Ladino", ThreeLetterCode = "lad")]
        Ladino,

        [Language(LongName = "Lahnda", ThreeLetterCode = "lah")]
        Lahnda,

        [Language(LongName = "Lamba", ThreeLetterCode = "lam")]
        Lamba,

        [Language(LongName = "Lao", TwoLetterCode = "lo", ThreeLetterCode = "lao")]
        Lao,

        [Language(LongName = "Latin", TwoLetterCode = "la", ThreeLetterCode = "lat")]
        Latin,

        [Language(LongName = "Latvian", TwoLetterCode = "lv", ThreeLetterCode = "lav")]
        Latvian,

        [Language(LongName = "Lezghian", ThreeLetterCode = "lez")]
        Lezghian,

        // Limburgan; Limburger; Limburgish
        [Language(LongName = "Limburgan", TwoLetterCode = "li", ThreeLetterCode = "lim")]
        Limburgan,

        [Language(LongName = "Lingala", TwoLetterCode = "ln", ThreeLetterCode = "lin")]
        Lingala,

        [Language(LongName = "Lithuanian", TwoLetterCode = "lt", ThreeLetterCode = "lit")]
        Lithuanian,

        [Language(LongName = "Lojban", ThreeLetterCode = "jbo")]
        Lojban,

        // Low German; Low Saxon
        [Language(LongName = "Low German", ThreeLetterCode = "nds")]
        LowGerman,

        [Language(LongName = "Lower Sorbian", ThreeLetterCode = "dsb")]
        LowerSorbian,

        [Language(LongName = "Lozi", ThreeLetterCode = "loz")]
        Lozi,

        [Language(LongName = "Luba-Katanga", TwoLetterCode = "lu", ThreeLetterCode = "lub")]
        LubaKatanga,

        [Language(LongName = "Luba-Lulua", ThreeLetterCode = "lua")]
        LubaLulua,

        [Language(LongName = "Luiseno", ThreeLetterCode = "lui")]
        Luiseno,

        [Language(LongName = "Lule Sami", ThreeLetterCode = "smj")]
        LuleSami,

        [Language(LongName = "Lunda", ThreeLetterCode = "lun")]
        Lunda,

        [Language(LongName = "Luo (Kenya and Tanzania)", ThreeLetterCode = "luo")]
        Luo,

        [Language(LongName = "Lushai", ThreeLetterCode = "lus")]
        Lushai,

        // Luxembourgish; Letzeburgesch
        [Language(LongName = "Luxembourgish", TwoLetterCode = "lb", ThreeLetterCode = "ltz")]
        Luxembourgish,

        #endregion

        #region M

        [Language(LongName = "Macedonian", TwoLetterCode = "mk", ThreeLetterCode = "mac", ThreeLetterCodeAlt = "mkd")]
        Macedonian,

        [Language(LongName = "Madurese", ThreeLetterCode = "mad")]
        Madurese,

        [Language(LongName = "Magahi", ThreeLetterCode = "mag")]
        Magahi,

        [Language(LongName = "Maithili", ThreeLetterCode = "mai")]
        Maithili,

        [Language(LongName = "Makasar", ThreeLetterCode = "mak")]
        Makasar,

        [Language(LongName = "Malagasy", TwoLetterCode = "mg", ThreeLetterCode = "mlg")]
        Malagasy,

        [Language(LongName = "Malay", TwoLetterCode = "ms", ThreeLetterCode = "may", ThreeLetterCodeAlt = "msa")]
        Malay,

        [Language(LongName = "Malayalam", TwoLetterCode = "ml", ThreeLetterCode = "mal")]
        Malayalam,

        [Language(LongName = "Maltese", TwoLetterCode = "mt", ThreeLetterCode = "mlt")]
        Maltese,

        [Language(LongName = "Manchu", ThreeLetterCode = "mnc")]
        Manchu,

        [Language(LongName = "Mandar", ThreeLetterCode = "mdr")]
        Mandar,

        [Language(LongName = "Mandingo", ThreeLetterCode = "man")]
        Mandingo,

        [Language(LongName = "Manipuri", ThreeLetterCode = "mni")]
        Manipuri,

        [Language(LongName = "Manx", TwoLetterCode = "gv", ThreeLetterCode = "glv")]
        Manx,

        [Language(LongName = "Maori", TwoLetterCode = "mi", ThreeLetterCode = "mao", ThreeLetterCodeAlt = "mri")]
        Maori,

        // Mapudungun; Mapuche
        [Language(LongName = "Mapudungun", ThreeLetterCode = "arn")]
        Mapudungun,

        [Language(LongName = "Marathi", TwoLetterCode = "mr", ThreeLetterCode = "mar")]
        Marathi,

        [Language(LongName = "Mari", ThreeLetterCode = "chm")]
        Mari,

        [Language(LongName = "Marshallese", TwoLetterCode = "mh", ThreeLetterCode = "mah")]
        Marshallese,

        [Language(LongName = "Marwari", ThreeLetterCode = "mwr")]
        Marwari,

        [Language(LongName = "Masai", ThreeLetterCode = "mas")]
        Masai,

        [Language(LongName = "Mende", ThreeLetterCode = "men")]
        Mende,

        // Mi'kmaq; Micmac
        [Language(LongName = "Mi'kmaq", ThreeLetterCode = "mic")]
        Mikmaq,

        [Language(LongName = "Minangkabau", ThreeLetterCode = "min")]
        Minangkabau,

        [Language(LongName = "Mirandese", ThreeLetterCode = "mwl")]
        Mirandese,

        [Language(LongName = "Mohawk", ThreeLetterCode = "moh")]
        Mohawk,

        [Language(LongName = "Moksha", ThreeLetterCode = "mdf")]
        Moksha,

        [Language(LongName = "Mongo", ThreeLetterCode = "lol")]
        Mongo,

        [Language(LongName = "Mongolian", TwoLetterCode = "mn", ThreeLetterCode = "mon")]
        Mongolian,

        [Language(LongName = "Montenegrin", ThreeLetterCode = "cnr")]
        Montenegrin,

        [Language(LongName = "Mossi", ThreeLetterCode = "mos")]
        Mossi,

        #endregion

        #region N

        [Language(LongName = "N'Ko", ThreeLetterCode = "nqo")]
        NKo,

        [Language(LongName = "Nauru", TwoLetterCode = "na", ThreeLetterCode = "nau")]
        Nauru,

        // Navajo; Navaho
        [Language(LongName = "Navajo", TwoLetterCode = "nv", ThreeLetterCode = "nav")]
        Navajo,

        [Language(LongName = "Ndonga", TwoLetterCode = "ng", ThreeLetterCode = "ndo")]
        Ndonga,

        [Language(LongName = "Neapolitan", ThreeLetterCode = "nap")]
        Neapolitan,

        // Nepal Bhasa; Newari
        [Language(LongName = "Nepal Bhasa", ThreeLetterCode = "new")]
        NepalBhasa,

        [Language(LongName = "Nepali", TwoLetterCode = "ne", ThreeLetterCode = "nep")]
        Nepali,

        [Language(LongName = "Nias", ThreeLetterCode = "nia")]
        Nias,

        [Language(LongName = "Niuean", ThreeLetterCode = "niu")]
        Niuean,

        // Commented out to avoid confusion
        //[Language(LongName = "No linguistic content; Not applicable", ThreeLetterCode = "zxx")]
        //NoLinguisticContent,

        [Language(LongName = "Nogai", ThreeLetterCode = "nog")]
        Nogai,

        [Language(LongName = "Norse, Old", ThreeLetterCode = "non")]
        OldNorse,

        [Language(LongName = "North Ndebele", TwoLetterCode = "nd", ThreeLetterCode = "nde")]
        NorthNdebele,

        [Language(LongName = "Northern Frisian", ThreeLetterCode = "frr")]
        NorthernFrisian,

        [Language(LongName = "Northern Sami", TwoLetterCode = "se", ThreeLetterCode = "sme")]
        NorthernSami,

        [Language(LongName = "Norwegian", TwoLetterCode = "no", ThreeLetterCode = "nor")]
        Norwegian,

        [Language(LongName = "Norwegian Bokmål", TwoLetterCode = "nb", ThreeLetterCode = "nob")]
        NorwegianBokmal,

        [Language(LongName = "Norwegian Nynorsk", TwoLetterCode = "nn", ThreeLetterCode = "nno")]
        NorwegianNynorsk,

        [Language(LongName = "Nyamwezi", ThreeLetterCode = "nym")]
        Nyamwezi,

        [Language(LongName = "Nyankole", ThreeLetterCode = "nyn")]
        Nyankole,

        [Language(LongName = "Nyoro", ThreeLetterCode = "nyo")]
        Nyoro,

        [Language(LongName = "Nzima", ThreeLetterCode = "nzi")]
        Nzima,

        #endregion

        #region O

        [Language(LongName = "Occitan (post 1500)", TwoLetterCode = "oc", ThreeLetterCode = "oci")]
        Occitan,

        // Occitan, Old (to 1500); Provençal, Old (to 1500)
        [Language(LongName = "Occitan, Old (to 1500)", ThreeLetterCode = "pro")]
        OldOccitan,

        [Language(LongName = "Ojibwa", TwoLetterCode = "oj", ThreeLetterCode = "oji")]
        Ojibwa,

        [Language(LongName = "Oriya", TwoLetterCode = "or", ThreeLetterCode = "ori")]
        Oriya,

        [Language(LongName = "Oromo", TwoLetterCode = "om", ThreeLetterCode = "orm")]
        Oromo,

        [Language(LongName = "Osage", ThreeLetterCode = "osa")]
        Osage,

        // Ossetian; Ossetic
        [Language(LongName = "Ossetian", TwoLetterCode = "os", ThreeLetterCode = "oss")]
        Ossetian,

        #endregion

        #region P

        [Language(LongName = "Pahlavi", ThreeLetterCode = "pal")]
        Pahlavi,

        [Language(LongName = "Palauan", ThreeLetterCode = "pau")]
        Palauan,

        [Language(LongName = "Pali", TwoLetterCode = "pi", ThreeLetterCode = "pli")]
        Pali,

        // Pampanga; Kapampangan
        [Language(LongName = "Pampanga", ThreeLetterCode = "pam")]
        Pampanga,

        [Language(LongName = "Pangasinan", ThreeLetterCode = "pag")]
        Pangasinan,

        // Panjabi; Punjabi
        [Language(LongName = "Panjabi", TwoLetterCode = "pa", ThreeLetterCode = "pan")]
        Panjabi,

        [Language(LongName = "Papiamento", ThreeLetterCode = "pap")]
        Papiamento,

        // Pedi; Sepedi; Northern Sotho
        [Language(LongName = "Pedi", ThreeLetterCode = "nso")]
        Pedi,

        [Language(LongName = "Persian", TwoLetterCode = "fa", ThreeLetterCode = "per", ThreeLetterCodeAlt = "fas")]
        Persian,

        [Language(LongName = "Persian, Old (ca.600-400 B.C.)", ThreeLetterCode = "peo")]
        OldPersian,

        [Language(LongName = "Phoenician", ThreeLetterCode = "phn")]
        Phoenician,

        [Language(LongName = "Polish", TwoLetterCode = "pl", ThreeLetterCode = "pol")]
        Polish,

        [Language(LongName = "Portuguese", TwoLetterCode = "pt", ThreeLetterCode = "por")]
        Portuguese,

        // Pushto; Pashto
        [Language(LongName = "Pushto", TwoLetterCode = "ps", ThreeLetterCode = "pus")]
        Pushto,

        #endregion

        #region Q

        // qaa-qtz: Reserved for local use

        [Language(LongName = "Quechua", TwoLetterCode = "qu", ThreeLetterCode = "que")]
        Quechua,

        #endregion

        #region R

        [Language(LongName = "Rajasthani", ThreeLetterCode = "raj")]
        Rajasthani,

        [Language(LongName = "Rapanui", ThreeLetterCode = "rap")]
        Rapanui,

        // Rarotongan; Cook Islands Maori
        [Language(LongName = "Rarotongan", ThreeLetterCode = "rar")]
        Rarotongan,

        // Romanian; Moldavian; Moldovan
        [Language(LongName = "Romanian", TwoLetterCode = "ro", ThreeLetterCode = "rum", ThreeLetterCodeAlt = "ron")]
        Romanian,

        [Language(LongName = "Romansh", TwoLetterCode = "rm", ThreeLetterCode = "roh")]
        Romansh,

        [Language(LongName = "Romany", ThreeLetterCode = "rom")]
        Romany,

        [Language(LongName = "Rundi", TwoLetterCode = "rn", ThreeLetterCode = "run")]
        Rundi,

        [Language(LongName = "Russian", TwoLetterCode = "ru", ThreeLetterCode = "rus")]
        Russian,

        #endregion

        #region S

        [Language(LongName = "Samaritan Aramaic", ThreeLetterCode = "sam")]
        SamaritanAramaic,

        [Language(LongName = "Samoan", TwoLetterCode = "sm", ThreeLetterCode = "smo")]
        Samoan,

        [Language(LongName = "Sandawe", ThreeLetterCode = "sad")]
        Sandawe,

        [Language(LongName = "Sango", TwoLetterCode = "sg", ThreeLetterCode = "sag")]
        Sango,

        [Language(LongName = "Sanskrit", TwoLetterCode = "sa", ThreeLetterCode = "san")]
        Sanskrit,

        [Language(LongName = "Santali", ThreeLetterCode = "sat")]
        Santali,

        [Language(LongName = "Sardinian", TwoLetterCode = "sc", ThreeLetterCode = "srd")]
        Sardinian,

        [Language(LongName = "Sasak", ThreeLetterCode = "sas")]
        Sasak,

        [Language(LongName = "Scots", ThreeLetterCode = "sco")]
        Scots,

        [Language(LongName = "Selkup", ThreeLetterCode = "sel")]
        Selkup,

        [Language(LongName = "Serbian", TwoLetterCode = "sr", ThreeLetterCode = "srp")]
        Serbian,

        [Language(LongName = "Serer", ThreeLetterCode = "srr")]
        Serer,

        [Language(LongName = "Shan", ThreeLetterCode = "shn")]
        Shan,

        [Language(LongName = "Shona", TwoLetterCode = "sn", ThreeLetterCode = "sna")]
        Shona,

        // Sichuan Yi; Nuosu
        [Language(LongName = "Sichuan Yi", TwoLetterCode = "ii", ThreeLetterCode = "iii")]
        SichuanYi,

        [Language(LongName = "Sicilian", ThreeLetterCode = "scn")]
        Sicilian,

        [Language(LongName = "Sidamo", ThreeLetterCode = "sid")]
        Sidamo,

        [Language(LongName = "Sign Languages", ThreeLetterCode = "sgn")]
        SignLanguages,

        [Language(LongName = "Siksika", ThreeLetterCode = "bla")]
        Siksika,

        [Language(LongName = "Sindhi", TwoLetterCode = "sd", ThreeLetterCode = "snd")]
        Sindhi,

        // Sinhala; Sinhalese
        [Language(LongName = "Sinhala", TwoLetterCode = "si", ThreeLetterCode = "sin")]
        Sinhala,

        [Language(LongName = "Skolt Sami", ThreeLetterCode = "sms")]
        SkoltSami,

        [Language(LongName = "Slovak", TwoLetterCode = "sk", ThreeLetterCode = "slo", ThreeLetterCodeAlt = "slk")]
        Slovak,

        [Language(LongName = "Slovenian", TwoLetterCode = "sl", ThreeLetterCode = "slv")]
        Slovenian,

        [Language(LongName = "Sogdian", ThreeLetterCode = "sog")]
        Sogdian,

        [Language(LongName = "Somali", TwoLetterCode = "so", ThreeLetterCode = "som")]
        Somali,

        [Language(LongName = "Soninke", ThreeLetterCode = "snk")]
        Soninke,

        [Language(LongName = "Sotho, Southern", TwoLetterCode = "st", ThreeLetterCode = "sot")]
        Sotho,

        [Language(LongName = "South Ndebele", TwoLetterCode = "nr", ThreeLetterCode = "nbl")]
        SouthNdebele,

        [Language(LongName = "Southern Altai", ThreeLetterCode = "alt")]
        SouthernAltai,

        [Language(LongName = "Southern Sami", ThreeLetterCode = "sma")]
        SouthernSami,

        // Spanish; Castilian
        [Language(LongName = "Spanish", TwoLetterCode = "es", ThreeLetterCode = "spa")]
        Spanish,

        [Language(LongName = "Sranan Tongo", ThreeLetterCode = "srn")]
        SrananTongo,

        [Language(LongName = "Standard Moroccan Tamazight", ThreeLetterCode = "zgh")]
        StandardMoroccanTamazight,

        [Language(LongName = "Sukuma", ThreeLetterCode = "suk")]
        Sukuma,

        [Language(LongName = "Sumerian", ThreeLetterCode = "sux")]
        Sumerian,

        [Language(LongName = "Sundanese", TwoLetterCode = "su", ThreeLetterCode = "sun")]
        Sundanese,

        [Language(LongName = "Susu", ThreeLetterCode = "sus")]
        Susu,

        [Language(LongName = "Susu", TwoLetterCode = "sw", ThreeLetterCode = "swa")]
        Swahili,

        [Language(LongName = "Swatio", TwoLetterCode = "ss", ThreeLetterCode = "ssw")]
        Swati,

        [Language(LongName = "Swedish", TwoLetterCode = "sv", ThreeLetterCode = "swe")]
        Swedish,

        // Swiss German; Alemannic; Alsatian
        [Language(LongName = "Swiss German", ThreeLetterCode = "gsw")]
        SwissGerman,

        [Language(LongName = "Syriac", ThreeLetterCode = "syr")]
        Syriac,

        [Language(LongName = "Syriac, Classical", ThreeLetterCode = "syc")]
        ClassicalSyriac,

        #endregion

        #region T

        [Language(LongName = "Tagalog", TwoLetterCode = "tl", ThreeLetterCode = "tgl")]
        Tagalog,

        [Language(LongName = "Tahitian", TwoLetterCode = "ty", ThreeLetterCode = "tah")]
        Tahitian,

        [Language(LongName = "Tajik", TwoLetterCode = "tg", ThreeLetterCode = "tgk")]
        Tajik,

        [Language(LongName = "Tamashek", ThreeLetterCode = "tmh")]
        Tamashek,

        [Language(LongName = "Tamil", TwoLetterCode = "ta", ThreeLetterCode = "tam")]
        Tamil,

        [Language(LongName = "Tatar", TwoLetterCode = "tt", ThreeLetterCode = "tat")]
        Tatar,

        [Language(LongName = "Telugu", TwoLetterCode = "te", ThreeLetterCode = "tel")]
        Telugu,

        [Language(LongName = "Tereno", ThreeLetterCode = "ter")]
        Tereno,

        [Language(LongName = "Tetum", ThreeLetterCode = "tet")]
        Tetum,

        [Language(LongName = "Thai", TwoLetterCode = "th", ThreeLetterCode = "tha")]
        Thai,

        [Language(LongName = "Tibetan", TwoLetterCode = "bo", ThreeLetterCode = "tib", ThreeLetterCodeAlt = "bod")]
        Tibetan,

        [Language(LongName = "Tigre", ThreeLetterCode = "tig")]
        Tigre,

        [Language(LongName = "Tigrinya", TwoLetterCode = "ti", ThreeLetterCode = "tir")]
        Tigrinya,

        [Language(LongName = "Timne", ThreeLetterCode = "tem")]
        Timne,

        [Language(LongName = "Tiv", ThreeLetterCode = "tiv")]
        Tiv,

        [Language(LongName = "Tlingit", ThreeLetterCode = "tli")]
        Tlingit,

        [Language(LongName = "Tok Pisin", ThreeLetterCode = "tpi")]
        TokPisin,

        [Language(LongName = "Tokelau", ThreeLetterCode = "tkl")]
        Tokelau,

        [Language(LongName = "Tonga (Nyasa)", ThreeLetterCode = "tog")]
        TongaNyasa,

        [Language(LongName = "Tonga (Tonga Islands)", TwoLetterCode = "to", ThreeLetterCode = "ton")]
        TongaIslands,

        [Language(LongName = "Tsimshian", ThreeLetterCode = "tsi")]
        Tsimshian,

        [Language(LongName = "Tsonga", TwoLetterCode = "ts", ThreeLetterCode = "tso")]
        Tsonga,

        [Language(LongName = "Tswana", TwoLetterCode = "tn", ThreeLetterCode = "tsn")]
        Tswana,

        [Language(LongName = "Tumbuka", ThreeLetterCode = "tum")]
        Tumbuka,

        [Language(LongName = "Turkish", TwoLetterCode = "tr", ThreeLetterCode = "tur")]
        Turkish,

        [Language(LongName = "Turkish, Ottoman (1500-1928)", ThreeLetterCode = "ota")]
        OttomanTurkish,

        [Language(LongName = "Turkmen", TwoLetterCode = "tk", ThreeLetterCode = "tuk")]
        Turkmen,

        [Language(LongName = "Tuvalu", ThreeLetterCode = "tvl")]
        Tuvalu,

        [Language(LongName = "Tuvinian", ThreeLetterCode = "tyv")]
        Tuvinian,

        [Language(LongName = "Twi", TwoLetterCode = "tw", ThreeLetterCode = "twi")]
        Twi,

        #endregion

        #region U

        [Language(LongName = "Udmurt", ThreeLetterCode = "udm")]
        Udmurt,

        [Language(LongName = "Ugaritic", ThreeLetterCode = "uga")]
        Ugaritic,

        // Uighur; Uyghur
        [Language(LongName = "Uighur", TwoLetterCode = "ug", ThreeLetterCode = "uig")]
        Uighur,

        [Language(LongName = "Ukrainian", TwoLetterCode = "uk", ThreeLetterCode = "ukr")]
        Ukrainian,

        [Language(LongName = "Umbundu", ThreeLetterCode = "umb")]
        Umbundu,

        // Commented out to avoid confusion
        //[Language(LongName = "Undetermined", ThreeLetterCode = "und")]
        //Undetermined,

        [Language(LongName = "Upper Sorbian", ThreeLetterCode = "hsb")]
        UpperSorbian,

        [Language(LongName = "Urdu", TwoLetterCode = "ur", ThreeLetterCode = "urd")]
        Urdu,

        [Language(LongName = "Uzbek", TwoLetterCode = "uz", ThreeLetterCode = "uzb")]
        Uzbek,

        #endregion

        #region V

        [Language(LongName = "Vai", ThreeLetterCode = "vai")]
        Vai,

        [Language(LongName = "Venda", TwoLetterCode = "ve", ThreeLetterCode = "ven")]
        Venda,

        [Language(LongName = "Vietnamese", TwoLetterCode = "vi", ThreeLetterCode = "vie")]
        Vietnamese,

        [Language(LongName = "Volapük", TwoLetterCode = "vo", ThreeLetterCode = "vol")]
        Volapuk,

        [Language(LongName = "Votic", ThreeLetterCode = "vot")]
        Votic,

        #endregion

        #region W

        [Language(LongName = "Walloon", TwoLetterCode = "wa", ThreeLetterCode = "wln")]
        Walloon,

        [Language(LongName = "Waray", ThreeLetterCode = "war")]
        Waray,

        [Language(LongName = "Washo", ThreeLetterCode = "was")]
        Washo,

        [Language(LongName = "Welsh", TwoLetterCode = "cy", ThreeLetterCode = "wel", ThreeLetterCodeAlt = "cym")]
        Welsh,

        [Language(LongName = "Western Frisian", TwoLetterCode = "fy", ThreeLetterCode = "fry")]
        WesternFrisian,

        // Wolaitta; Wolaytta
        [Language(LongName = "Wolaitta", ThreeLetterCode = "wal")]
        Wolaitta,

        [Language(LongName = "Wolof", TwoLetterCode = "wo", ThreeLetterCode = "wol")]
        Wolof,

        #endregion

        #region X

        [Language(LongName = "Xhosa", TwoLetterCode = "xh", ThreeLetterCode = "xho")]
        Xhosa,

        #endregion

        #region Y

        [Language(LongName = "Yakut", ThreeLetterCode = "sah")]
        Yakut,

        [Language(LongName = "Yao", ThreeLetterCode = "yao")]
        Yao,

        [Language(LongName = "Yapese", ThreeLetterCode = "yap")]
        Yapese,

        [Language(LongName = "Yiddish", TwoLetterCode = "yi", ThreeLetterCode = "yid")]
        Yiddish,

        [Language(LongName = "Yoruba", TwoLetterCode = "yo", ThreeLetterCode = "yor")]
        Yoruba,

        #endregion

        #region Z

        [Language(LongName = "Zapotec", ThreeLetterCode = "zap")]
        Zapotec,

        // Zaza; Dimili; Dimli; Kirdki; Kirmanjki; Zazaki
        [Language(LongName = "Zaza", ThreeLetterCode = "zza")]
        Zaza,

        [Language(LongName = "Zenaga", ThreeLetterCode = "zen")]
        Zenaga,

        // Zhuang; Chuang
        [Language(LongName = "Zhuang", TwoLetterCode = "za", ThreeLetterCode = "zha")]
        Zhuang,

        [Language(LongName = "Zulu", TwoLetterCode = "zu", ThreeLetterCode = "zul")]
        Zulu,

        [Language(LongName = "Zuni", ThreeLetterCode = "zun")]
        Zuni,

        #endregion

        #region Language Families

        /*
        [Language(LongName = "Afro-Asiatic languages", ThreeLetterCode = "afa")]
        AfroAsiaticLanguages,

        [Language(LongName = "Algonquian languages", ThreeLetterCode = "alg")]
        AlgonquianLanguages,

        [Language(LongName = "Altaic languages", ThreeLetterCode = "tut")]
        AltaicLanguages,

        [Language(LongName = "Apache languages", ThreeLetterCode = "apa")]
        ApacheLanguages,

        [Language(LongName = "Artificial languages", ThreeLetterCode = "art")]
        ArtificialLanguages,

        [Language(LongName = "Athapascan languages", ThreeLetterCode = "ath")]
        AthapascanLanguages,

        [Language(LongName = "Australian languages", ThreeLetterCode = "aus")]
        AustralianLanguages,

        [Language(LongName = "Austronesian languages", ThreeLetterCode = "map")]
        AustronesianLanguages,

        [Language(LongName = "Baltic languages", ThreeLetterCode = "bat")]
        BalticLanguages,

        [Language(LongName = "Bamileke languages", ThreeLetterCode = "bai")]
        BamilekeLanguages,

        [Language(LongName = "Banda languages", ThreeLetterCode = "bad")]
        BandaLanguages,

        [Language(LongName = "Bantu languages", ThreeLetterCode = "bnt")]
        BantuLanguages,

        [Language(LongName = "Batak languages", ThreeLetterCode = "btk")]
        BatakLanguages,

        [Language(LongName = "Berber languages", ThreeLetterCode = "ber")]
        BerberLanguages,

        [Language(LongName = "Bihari languages", TwoLetterCode = "bh", ThreeLetterCode = "bih")]
        BihariLanguages,

        [Language(LongName = "Caucasian languages", ThreeLetterCode = "cau")]
        CaucasianLanguages,

        [Language(LongName = "Celtic languages", ThreeLetterCode = "cel")]
        CelticLanguages,

        [Language(LongName = "Central American Indian languages", ThreeLetterCode = "cai")]
        CentralAmericanIndianLanguages,

        [Language(LongName = "Chamic languages", ThreeLetterCode = "cmc")]
        ChamicLanguages,

        [Language(LongName = "Cushitic languages", ThreeLetterCode = "cus")]
        CushiticLanguages,

        [Language(LongName = "Dravidian languages", ThreeLetterCode = "dra")]
        DravidianLanguages,

        [Language(LongName = "Finno-Ugrian languages", ThreeLetterCode = "fiu")]
        FinnoUgrianLanguages,

        [Language(LongName = "Germanic languages", ThreeLetterCode = "gem")]
        GermanicLanguages,

        [Language(LongName = "Himachali languages; Western Pahari languages", ThreeLetterCode = "him")]
        HimachaliLanguages,

        [Language(LongName = "Ijo languages", ThreeLetterCode = "ijo")]
        IjoLanguages,

        [Language(LongName = "Indic languages", ThreeLetterCode = "inc")]
        IndicLanguages,

        [Language(LongName = "Indo-European languages", ThreeLetterCode = "ine")]
        IndoEuropeanLanguages,

        [Language(LongName = "Iranian languages", ThreeLetterCode = "ira")]
        IranianLanguages,

        [Language(LongName = "Iroquoian languages", ThreeLetterCode = "iro")]
        IroquoianLanguages,

        [Language(LongName = "Karen languages", ThreeLetterCode = "kar")]
        KarenLanguages,

        [Language(LongName = "Khoisan languages", ThreeLetterCode = "khi")]
        KhoisanLanguages,

        [Language(LongName = "Kru languages", ThreeLetterCode = "kro")]
        KruLanguages,

        [Language(LongName = "Land Dayak languages", ThreeLetterCode = "day")]
        LandDayakLanguages,

        [Language(LongName = "Manobo languages", ThreeLetterCode = "mno")]
        ManoboLanguages,

        [Language(LongName = "Mayan languages", ThreeLetterCode = "myn")]
        MayanLanguages,

        [Language(LongName = "Mon-Khmer languages", ThreeLetterCode = "mkh")]
        MonKhmerLanguages,

        // Commented out to avoid confusion
        //[Language(LongName = "Multiple languages", ThreeLetterCode = "mul")]
        //MultipleLanguages,

        [Language(LongName = "Munda languages", ThreeLetterCode = "mun")]
        MundaLanguages,

        [Language(LongName = "Nahuatl languages", ThreeLetterCode = "nah")]
        NahuatlLanguages,

        [Language(LongName = "Niger-Kordofanian languages", ThreeLetterCode = "nic")]
        NigerKordofanianLanguages,

        [Language(LongName = "Nilo-Saharan languages", ThreeLetterCode = "ssa")]
        NiloSaharanLanguages,

        [Language(LongName = "North American Indian languages", ThreeLetterCode = "nai")]
        NorthAmericanIndianLanguages,

        [Language(LongName = "Nubian languages", ThreeLetterCode = "nub")]
        NubianLanguages,

        [Language(LongName = "Otomian languages", ThreeLetterCode = "oto")]
        OtomianLanguages,

        [Language(LongName = "Papuan languages", ThreeLetterCode = "paa")]
        PapuanLanguages,

        [Language(LongName = "Philippine languages", ThreeLetterCode = "phi")]
        PhilippineLanguages,

        [Language(LongName = "Prakrit languages", ThreeLetterCode = "pra")]
        PrakritLanguages,

        [Language(LongName = "Romance languages", ThreeLetterCode = "roa")]
        RomanceLanguages,

        [Language(LongName = "Salishan languages", ThreeLetterCode = "sal")]
        SalishanLanguages,

        [Language(LongName = "Sami languages", ThreeLetterCode = "smi")]
        SamiLanguages,

        [Language(LongName = "Semitic languages", ThreeLetterCode = "sem")]
        SemiticLanguages,

        [Language(LongName = "Sino-Tibetan languages", ThreeLetterCode = "sit")]
        SinoTibetanLanguages,

        [Language(LongName = "Siouan languages", ThreeLetterCode = "sio")]
        SiouanLanguages,

        [Language(LongName = "Slavic languages", ThreeLetterCode = "sla")]
        SlavicLanguages,

        [Language(LongName = "Songhai languages", ThreeLetterCode = "son")]
        SonghaiLanguages,

        [Language(LongName = "Sorbian languages", ThreeLetterCode = "wen")]
        SorbianLanguages,

        [Language(LongName = "South American Indian languages", ThreeLetterCode = "sai")]
        SouthAmericanIndianLanguages,

        [Language(LongName = "Tai languages", ThreeLetterCode = "tai")]
        TaiLanguages,

        [Language(LongName = "Tupi languages", ThreeLetterCode = "tup")]
        TupiLanguages,

        [Language(LongName = "Uncoded languages", ThreeLetterCode = "mis")]
        UncodedLanguages,

        [Language(LongName = "Wakashan languages", ThreeLetterCode = "wak")]
        WakashanLanguages,

        [Language(LongName = "Yupik languages", ThreeLetterCode = "ypk")]
        YupikLanguages,

        [Language(LongName = "Zande languages", ThreeLetterCode = "znd")]
        ZandeLanguages,
        */

        #endregion
    }

    /// <summary>
    /// All possible language selections
    /// </summary>
    public enum LanguageSelection
    {
        [HumanReadable(LongName = "Bios settings")]
        BiosSettings,

        [HumanReadable(LongName = "Language selector")]
        LanguageSelector,

        [HumanReadable(LongName = "Options menu")]
        OptionsMenu,
    }

    /// <summary>
    /// All possible media types
    /// </summary>
    public enum MediaType
    {
        [HumanReadable(Available = false, LongName = "Unknown", ShortName = "unknown")]
        NONE = 0,

        #region Punched Media

        [HumanReadable(Available = false, LongName = "Aperture card", ShortName = "aperture")]
        ApertureCard,

        [HumanReadable(Available = false, LongName = "Jacquard Loom card", ShortName = "jacquard loom card")]
        JacquardLoomCard,

        [HumanReadable(Available = false, LongName = "Magnetic stripe card", ShortName = "magnetic stripe")]
        MagneticStripeCard,

        [HumanReadable(Available = false, LongName = "Optical phonecard", ShortName = "optical phonecard")]
        OpticalPhonecard,

        [HumanReadable(Available = false, LongName = "Punched card", ShortName = "punchcard")]
        PunchedCard,

        [HumanReadable(Available = false, LongName = "Punched tape", ShortName = "punchtape")]
        PunchedTape,

        #endregion

        #region Tape

        [HumanReadable(Available = false, LongName = "Cassette Tape", ShortName = "cassette")]
        Cassette,

        [HumanReadable(Available = false, LongName = "Data Tape Cartridge", ShortName = "data cartridge")]
        DataCartridge,

        [HumanReadable(Available = false, LongName = "Open Reel Tape", ShortName = "open reel")]
        OpenReel,

        #endregion

        #region Disc / Disc

        [HumanReadable(LongName = "BD-ROM", ShortName = "bdrom")]
        BluRay,

        [HumanReadable(LongName = "CD-ROM", ShortName = "cdrom")]
        CDROM,

        [HumanReadable(LongName = "DVD-ROM", ShortName = "dvd")]
        DVD,

        [HumanReadable(LongName = "Floppy Disk", ShortName = "fd")]
        FloppyDisk,

        [HumanReadable(Available = false, LongName = "Floptical", ShortName = "floptical")]
        Floptical,

        [HumanReadable(LongName = "GD-ROM", ShortName = "gdrom")]
        GDROM,

        [HumanReadable(LongName = "HD-DVD-ROM", ShortName = "hddvd")]
        HDDVD,

        [HumanReadable(LongName = "Hard Disk", ShortName = "hdd")]
        HardDisk,

        [HumanReadable(Available = false, LongName = "Iomega Bernoulli Disk", ShortName = "bernoulli")]
        IomegaBernoulliDisk,

        [HumanReadable(Available = false, LongName = "Iomega Jaz", ShortName = "jaz")]
        IomegaJaz,

        [HumanReadable(Available = false, LongName = "Iomega Zip", ShortName = "zip")]
        IomegaZip,

        [HumanReadable(LongName = "LD-ROM / LV-ROM", ShortName = "ldrom")]
        LaserDisc, // LD-ROM and LV-ROM variants

        [HumanReadable(Available = false, LongName = "64DD Disk", ShortName = "64dd")]
        Nintendo64DD,

        [HumanReadable(Available = false, LongName = "Famicom Disk System Disk", ShortName = "fds")]
        NintendoFamicomDiskSystem,

        [HumanReadable(LongName = "GameCube Game Disc", ShortName = "gc")]
        NintendoGameCubeGameDisc,

        [HumanReadable(LongName = "Wii Optical Disc", ShortName = "wii")]
        NintendoWiiOpticalDisc,

        [HumanReadable(LongName = "Wii U Optical Disc", ShortName = "wiiu")]
        NintendoWiiUOpticalDisc,

        [HumanReadable(LongName = "UMD", ShortName = "umd")]
        UMD,

        #endregion

        #region Unsorted Formats

        [HumanReadable(Available = false, LongName = "Cartridge", ShortName = "cart")]
        Cartridge,

        [HumanReadable(Available = false, LongName = "CED", ShortName = "ced")]
        CED,

        [HumanReadable(Available = false, LongName = "Compact Flash", ShortName = "cf")]
        CompactFlash,

        [HumanReadable(Available = false, LongName = "MMC", ShortName = "mmc")]
        MMC,

        [HumanReadable(Available = false, LongName = "SD Card", ShortName = "sd")]
        SDCard,

        [HumanReadable(Available = false, LongName = "Flash Drive", ShortName = "fkd")]
        FlashDrive,

        #endregion
    }

    /// <summary>
    /// List of all known systems
    /// </summary>
    /// TODO: Remove marker items
    public enum RedumpSystem
    {
        #region BIOS Sets

        [System(LongName = "Microsoft Xbox (BIOS)", ShortName = "xbox-bios", HasDat = true)]
        MicrosoftXboxBIOS,

        [System(LongName = "Nintendo GameCube (BIOS)", ShortName = "gc-bios", HasDat = true)]
        NintendoGameCubeBIOS,

        [System(LongName = "Sony PlayStation (BIOS)", ShortName = "psx-bios", HasDat = true)]
        SonyPlayStationBIOS,

        [System(LongName = "Sony PlayStation 2 (BIOS)", ShortName = "ps2-bios", HasDat = true)]
        SonyPlayStation2BIOS,

        #endregion

        #region Disc-Based Consoles

        [System(Category = SystemCategory.DiscBasedConsole, LongName = "Atari Jaguar CD Interactive Multimedia System", ShortName = "ajcd", HasCues = true, HasDat = true)]
        AtariJaguarCDInteractiveMultimediaSystem,

        [System(Category = SystemCategory.DiscBasedConsole, LongName = "Bandai Playdia Quick Interactive System", ShortName = "qis", HasCues = true, HasDat = true)]
        BandaiPlaydiaQuickInteractiveSystem,

        [System(Category = SystemCategory.DiscBasedConsole, LongName = "Bandai Pippin", ShortName = "pippin", HasCues = true, HasDat = true)]
        BandaiPippin,

        [System(Category = SystemCategory.DiscBasedConsole, LongName = "Commodore Amiga CD32", ShortName = "cd32", HasCues = true, HasDat = true)]
        CommodoreAmigaCD32,

        [System(Category = SystemCategory.DiscBasedConsole, LongName = "Commodore Amiga CDTV", ShortName = "cdtv", HasCues = true, HasDat = true)]
        CommodoreAmigaCDTV,

        [System(Category = SystemCategory.DiscBasedConsole, Available = false, LongName = "Envizions EVO Smart Console")]
        EnvizionsEVOSmartConsole,

        [System(Category = SystemCategory.DiscBasedConsole, Available = false, LongName = "Fujitsu FM Towns Marty")]
        FujitsuFMTownsMarty,

        [System(Category = SystemCategory.DiscBasedConsole, LongName = "Hasbro VideoNow", ShortName = "hvn", IsBanned = true, HasCues = true, HasDat = true)]
        HasbroVideoNow,

        [System(Category = SystemCategory.DiscBasedConsole, LongName = "Hasbro VideoNow Color", ShortName = "hvnc", IsBanned = true, HasCues = true, HasDat = true)]
        HasbroVideoNowColor,

        [System(Category = SystemCategory.DiscBasedConsole, LongName = "Hasbro VideoNow Jr.", ShortName = "hvnjr", IsBanned = true, HasCues = true, HasDat = true)]
        HasbroVideoNowJr,

        [System(Category = SystemCategory.DiscBasedConsole, LongName = "Hasbro VideoNow XP", ShortName = "hvnxp", IsBanned = true, HasCues = true, HasDat = true)]
        HasbroVideoNowXP,

        [System(Category = SystemCategory.DiscBasedConsole, LongName = "Mattel Fisher-Price iXL", ShortName = "ixl", HasCues = true, HasDat = true)]
        MattelFisherPriceiXL,

        [System(Category = SystemCategory.DiscBasedConsole, LongName = "Mattel HyperScan", ShortName = "hs", HasCues = true, HasDat = true)]
        MattelHyperScan,

        [System(Category = SystemCategory.DiscBasedConsole, LongName = "Microsoft Xbox", ShortName = "xbox", HasCues = true, HasDat = true)]
        MicrosoftXbox,

        [System(Category = SystemCategory.DiscBasedConsole, LongName = "Microsoft Xbox 360", ShortName = "xbox360", HasCues = true, HasDat = true)]
        MicrosoftXbox360,

        [System(Category = SystemCategory.DiscBasedConsole, LongName = "Microsoft Xbox One", ShortName = "xboxone", IsBanned = true, HasDat = true)]
        MicrosoftXboxOne,

        [System(Category = SystemCategory.DiscBasedConsole, LongName = "Microsoft Xbox Series X", ShortName = "xboxsx", IsBanned = true)]
        MicrosoftXboxSeriesXS,

        [System(Category = SystemCategory.DiscBasedConsole, LongName = "Memorex Visual Information System", ShortName = "vis", HasCues = true, HasDat = true)]
        MemorexVisualInformationSystem,

        [System(Category = SystemCategory.DiscBasedConsole, LongName = "NEC PC Engine CD & TurboGrafx CD", ShortName = "pce", HasCues = true, HasDat = true)]
        NECPCEngineCDTurboGrafxCD,

        [System(Category = SystemCategory.DiscBasedConsole, LongName = "NEC PC-FX & PC-FXGA", ShortName = "pc-fx", HasCues = true, HasDat = true)]
        NECPCFXPCFXGA,

        [System(Category = SystemCategory.DiscBasedConsole, LongName = "Nintendo GameCube", ShortName = "gc", HasDat = true)]
        NintendoGameCube,

        [System(Category = SystemCategory.DiscBasedConsole, Available = false, LongName = "Nintendo-Sony Super NES CD-ROM System")]
        NintendoSonySuperNESCDROMSystem,

        [System(Category = SystemCategory.DiscBasedConsole, LongName = "Nintendo Wii", ShortName = "wii", HasDat = true)]
        NintendoWii,

        [System(Category = SystemCategory.DiscBasedConsole, LongName = "Nintendo Wii U", ShortName = "wiiu", IsBanned = true, HasDat = true, HasKeys = true)]
        NintendoWiiU,

        [System(Category = SystemCategory.DiscBasedConsole, LongName = "Panasonic 3DO Interactive Multiplayer", ShortName = "3do", HasCues = true, HasDat = true)]
        Panasonic3DOInteractiveMultiplayer,

        [System(Category = SystemCategory.DiscBasedConsole, LongName = "Philips CD-i", ShortName = "cdi", HasCues = true, HasDat = true)]
        PhilipsCDi,

        [System(Category = SystemCategory.DiscBasedConsole, LongName = "Philips CD-i Digital Video", ShortName = "cdi-video", IsBanned = true)]
        PhilipsCDiDigitalVideo,

        [System(Category = SystemCategory.DiscBasedConsole, Available = false, LongName = "Pioneer LaserActive")]
        PioneerLaserActive,

        [System(Category = SystemCategory.DiscBasedConsole, LongName = "Sega Dreamcast", ShortName = "dc", HasCues = true, HasDat = true, HasGdi = true)]
        SegaDreamcast,

        [System(Category = SystemCategory.DiscBasedConsole, LongName = "Sega Mega CD & Sega CD", ShortName = "mcd", HasCues = true, HasDat = true)]
        SegaMegaCDSegaCD,

        [System(Category = SystemCategory.DiscBasedConsole, LongName = "Sega Saturn", ShortName = "ss", HasCues = true, HasDat = true)]
        SegaSaturn,

        [System(Category = SystemCategory.DiscBasedConsole, LongName = "Neo Geo CD", ShortName = "ngcd", HasCues = true, HasDat = true)]
        SNKNeoGeoCD,

        [System(Category = SystemCategory.DiscBasedConsole, LongName = "Sony PlayStation", ShortName = "psx", HasCues = true, HasDat = true, HasLsd = true, HasSbi = true)]
        SonyPlayStation,

        [System(Category = SystemCategory.DiscBasedConsole, LongName = "Sony PlayStation 2", ShortName = "ps2", HasCues = true, HasDat = true)]
        SonyPlayStation2,

        [System(Category = SystemCategory.DiscBasedConsole, LongName = "Sony PlayStation 3", ShortName = "ps3", HasCues = true, HasDat = true, HasDkeys = true, HasKeys = true)]
        SonyPlayStation3,

        [System(Category = SystemCategory.DiscBasedConsole, LongName = "Sony PlayStation 4", ShortName = "ps4", IsBanned = true, HasDat = true)]
        SonyPlayStation4,

        [System(Category = SystemCategory.DiscBasedConsole, LongName = "Sony PlayStation 5", ShortName = "ps5", IsBanned = true)]
        SonyPlayStation5,

        [System(Category = SystemCategory.DiscBasedConsole, LongName = "Sony PlayStation Portable", ShortName = "psp", HasDat = true)]
        SonyPlayStationPortable,

        [System(Category = SystemCategory.DiscBasedConsole, LongName = "VM Labs NUON", ShortName = "nuon", HasDat = true)]
        VMLabsNUON,

        [System(Category = SystemCategory.DiscBasedConsole, LongName = "VTech V.Flash & V.Smile Pro", ShortName = "vflash", HasCues = true, HasDat = true)]
        VTechVFlashVSmilePro,

        [System(Category = SystemCategory.DiscBasedConsole, LongName = "ZAPiT Games Game Wave Family Entertainment System", ShortName = "gamewave", HasDat = true)]
        ZAPiTGamesGameWaveFamilyEntertainmentSystem,

        // End of console section delimiter
        MarkerDiscBasedConsoleEnd,

        #endregion

        #region Cartridge-Based and Other Consoles

        /*
        [System(Category = SystemCategory.OtherConsole, Available = false, LongName = "Amstrad GX-4000")]
        AmstradGX4000,

        [System(Category = SystemCategory.OtherConsole, Available = false, LongName = "APF Microcomputer System")]
        APFMicrocomputerSystem,

        [System(Category = SystemCategory.OtherConsole, Available = false, LongName = "Atari 2600 & VCS")]
        Atari2600VCS,

        [System(Category = SystemCategory.OtherConsole, Available = false, LongName = "Atari 5200")]
        Atari5200,

        [System(Category = SystemCategory.OtherConsole, Available = false, LongName = "Atari 7800")]
        Atari7800,

        [System(Category = SystemCategory.OtherConsole, Available = false, LongName = "Atari Jaguar")]
        AtariJaguar,

        [System(Category = SystemCategory.OtherConsole, Available = false, LongName = "Atari XEGS")]
        AtariXEGS,

        [System(Category = SystemCategory.OtherConsole, Available = false, LongName = "Audiosonic 1292 Advanced Programmable Video System")]
        Audiosonic1292AdvancedProgrammableVideoSystem,

        [System(Category = SystemCategory.OtherConsole, Available = false, LongName = "Bally Astrocade")]
        BallyAstrocade,

        [System(Category = SystemCategory.OtherConsole, Available = false, LongName = "Bit Corporation Dina")]
        BitCorporationDina,

        [System(Category = SystemCategory.OtherConsole, Available = false, LongName = "Casio Loopy")]
        CasioLoopy,

        [System(Category = SystemCategory.OtherConsole, Available = false, LongName = "Casio PV-1000")]
        CasioPV1000,

        [System(Category = SystemCategory.OtherConsole, Available = false, LongName = "Commodore 64 Games System")]
        Commodore64GamesSystem,

        [System(Category = SystemCategory.OtherConsole, Available = false, LongName = "Daewoo Electronics Zemmix")]
        DaewooElectronicsZemmix,

        [System(Category = SystemCategory.OtherConsole, Available = false, LongName = "Emerson Arcadia 2001")]
        EmersonArcadia2001,

        [System(Category = SystemCategory.OtherConsole, Available = false, LongName = "Epoch Cassette Vision")]
        EpochCassetteVision,

        [System(Category = SystemCategory.OtherConsole, Available = false, LongName = "Epoch Super Cassette Vision")]
        EpochSuperCassetteVision,

        [System(Category = SystemCategory.OtherConsole, Available = false, LongName = "Fairchild Channel F")]
        FairchildChannelF,

        [System(Category = SystemCategory.OtherConsole, Available = false, LongName = "Funtech Super A'Can")]
        FuntechSuperACan,

        [System(Category = SystemCategory.OtherConsole, Available = false, LongName = "GCE Vectrex")]
        GCEVectrex,

        [System(Category = SystemCategory.OtherConsole, Available = false, LongName = "Heber BBC Bridge Companion")]
        HeberBBCBridgeCompanion,

        [System(Category = SystemCategory.OtherConsole, Available = false, LongName = "Interton VC-4000")]
        IntertonVC4000,

        [System(Category = SystemCategory.OtherConsole, Available = false, LongName = "JungleTac Vii")]
        JungleTacVii,

        [System(Category = SystemCategory.OtherConsole, Available = false, LongName = "LeapFrog ClickStart")]
        LeapFrogClickStart,

        [System(Category = SystemCategory.OtherConsole, Available = false, LongName = "LJN VideoArt")]
        LJNVideoArt,

        [System(Category = SystemCategory.OtherConsole, Available = false, LongName = "Magnavox Odyssey 2")]
        MagnavoxOdyssey2,

        [System(Category = SystemCategory.OtherConsole, Available = false, LongName = "Mattel Intellivision")]
        MattelIntellivision,

        [System(Category = SystemCategory.OtherConsole, Available = false, LongName = "NEC PC Engine & TurboGrafx-16")]
        NECPCEngineTurboGrafx16,

        [System(Category = SystemCategory.OtherConsole, Available = false, LongName = "Nichibutsu MyVision")]
        NichibutsuMyVision,

        [System(Category = SystemCategory.OtherConsole, Available = false, LongName = "Nintendo 64")]
        Nintendo64,

        [System(Category = SystemCategory.OtherConsole, Available = false, LongName = "Nintendo 64DD")]
        Nintendo64DD,

        [System(Category = SystemCategory.OtherConsole, Available = false, LongName = "Nintendo Famicom & Nintendo Entertainment System")]
        NintendoFamicomNintendoEntertainmentSystem,

        [System(Category = SystemCategory.OtherConsole, Available = false, LongName = "Nintendo Famicom Disk System")]
        NintendoFamicomDiskSystem,

        [System(Category = SystemCategory.OtherConsole, Available = false, LongName = "Nintendo Super Famicom & Super Nintendo Entertainment System")]
        NintendoSuperFamicomSuperNintendoEntertainmentSystem,

        [System(Category = SystemCategory.OtherConsole, Available = false, LongName = "Nintendo Switch")]
        NintendoSwitch,

        [System(Category = SystemCategory.OtherConsole, Available = false, LongName = "Philips Videopac+ & G7400")]
        PhilipsVideopacPlusG7400,

        [System(Category = SystemCategory.OtherConsole, Available = false, LongName = "RCA Studio-II")]
        RCAStudioII,

        [System(Category = SystemCategory.OtherConsole, Available = false, LongName = "Sega 32X")]
        Sega32X,

        [System(Category = SystemCategory.OtherConsole, Available = false, LongName = "Sega Mark III & Master System")]
        SegaMarkIIIMasterSystem,

        [System(Category = SystemCategory.OtherConsole, Available = false, LongName = "Sega MegaDrive & Genesis")]
        SegaMegaDriveGenesis,

        [System(Category = SystemCategory.OtherConsole, Available = false, LongName = "Sega SG-1000")]
        SegaSG1000,

        [System(Category = SystemCategory.OtherConsole, Available = false, LongName = "SNK NeoGeo")]
        SNKNeoGeo,

        [System(Category = SystemCategory.OtherConsole, Available = false, LongName = "SSD COMPANY LIMITED XaviXPORT")]
        SSDCOMPANYLIMITEDXaviXPORT,

        [System(Category = SystemCategory.OtherConsole, Available = false, LongName = "ViewMaster Interactive Vision")]
        ViewMasterInteractiveVision,

        [System(Category = SystemCategory.OtherConsole, Available = false, LongName = "V.Tech CreatiVision")]
        VTechCreatiVision,

        [System(Category = SystemCategory.OtherConsole, Available = false, LongName = "V.Tech V.Smile")]
        VTechVSmile,

        [System(Category = SystemCategory.OtherConsole, Available = false, LongName = "V.Tech Socrates")]
        VTechSocrates,

        [System(Category = SystemCategory.OtherConsole, Available = false, LongName = "Worlds of Wonder ActionMax")]
        WorldsOfWonderActionMax,

        // End of other console delimiter
        MarkerOtherConsoleEnd,
        */

        #endregion

        #region Computers

        [System(Category = SystemCategory.Computer, LongName = "Acorn Archimedes", ShortName = "arch", HasCues = true, HasDat = true)]
        AcornArchimedes,

        [System(Category = SystemCategory.Computer, LongName = "Apple Macintosh", ShortName = "mac", HasCues = true, HasDat = true)]
        AppleMacintosh,

        [System(Category = SystemCategory.Computer, LongName = "Commodore Amiga CD", ShortName = "acd", HasCues = true, HasDat = true)]
        CommodoreAmigaCD,

        [System(Category = SystemCategory.Computer, LongName = "Fujitsu FM Towns series", ShortName = "fmt", HasCues = true, HasDat = true)]
        FujitsuFMTownsseries,

        [System(Category = SystemCategory.Computer, LongName = "IBM PC compatible", ShortName = "pc", HasCues = true, HasDat = true, HasLsd = true, HasSbi = true)]
        IBMPCcompatible,

        [System(Category = SystemCategory.Computer, LongName = "NEC PC-88 series", ShortName = "pc-88", HasCues = true, HasDat = true)]
        NECPC88series,

        [System(Category = SystemCategory.Computer, LongName = "NEC PC-98 series", ShortName = "pc-98", HasCues = true, HasDat = true)]
        NECPC98series,

        [System(Category = SystemCategory.Computer, LongName = "Sharp X68000", ShortName = "x86kcd", HasCues = true, HasDat = true)]
        SharpX68000,

        // End of computer section delimiter
        MarkerComputerEnd,

        #endregion

        #region Arcade

        [System(Category = SystemCategory.Arcade, Available = false, LongName = "Amiga CUBO CD32")]
        AmigaCUBOCD32,

        [System(Category = SystemCategory.Arcade, Available = false, LongName = "American Laser Games 3DO")]
        AmericanLaserGames3DO,

        [System(Category = SystemCategory.Arcade, Available = false, LongName = "Atari 3DO")]
        Atari3DO,

        [System(Category = SystemCategory.Arcade, Available = false, LongName = "Atronic")]
        Atronic,

        [System(Category = SystemCategory.Arcade, Available = false, LongName = "AUSCOM System 1")]
        AUSCOMSystem1,

        [System(Category = SystemCategory.Arcade, Available = false, LongName = "Bally Game Magic")]
        BallyGameMagic,

        [System(Category = SystemCategory.Arcade, Available = false, LongName = "Capcom CP System III")]
        CapcomCPSystemIII,

        [System(Category = SystemCategory.Arcade, LongName = "funworld Photo Play", ShortName = "fpp", HasCues = true, HasDat = true)]
        funworldPhotoPlay,

        [System(Category = SystemCategory.Arcade, Available = false, LongName = "Global VR PC-based Systems")]
        GlobalVRVarious,

        [System(Category = SystemCategory.Arcade, Available = false, LongName = "Global VR Vortek")]
        GlobalVRVortek,

        [System(Category = SystemCategory.Arcade, Available = false, LongName = "Global VR Vortek V3")]
        GlobalVRVortekV3,

        [System(Category = SystemCategory.Arcade, Available = false, LongName = "ICE PC-based Hardware")]
        ICEPCHardware,

        [System(Category = SystemCategory.Arcade, LongName = "Incredible Technologies Eagle", ShortName = "ite", HasCues = true, HasDat = true)]
        IncredibleTechnologiesEagle,

        [System(Category = SystemCategory.Arcade, Available = false, LongName = "Incredible Technologies PC-based Systems")]
        IncredibleTechnologiesVarious,

        [System(Category = SystemCategory.Arcade, LongName = "Konami e-Amusement", ShortName = "kea", HasCues = true, HasDat = true)]
        KonamieAmusement,

        [System(Category = SystemCategory.Arcade, LongName = "Konami FireBeat", ShortName = "kfb", HasCues = true, HasDat = true)]
        KonamiFireBeat,

        [System(Category = SystemCategory.Arcade, LongName = "Konami M2", ShortName = "km2", IsBanned = true, HasCues = true, HasDat = true)]
        KonamiM2,

        [System(Category = SystemCategory.Arcade, Available = false, LongName = "Konami Python")]
        KonamiPython,

        [System(Category = SystemCategory.Arcade, Available = false, LongName = "Konami Python 2")]
        KonamiPython2,

        [System(Category = SystemCategory.Arcade, LongName = "Konami System 573", ShortName = "ks573")]
        KonamiSystem573,

        [System(Category = SystemCategory.Arcade, LongName = "Konami System GV", ShortName = "ksgv", HasCues = true, HasDat = true)]
        KonamiSystemGV,

        [System(Category = SystemCategory.Arcade, LongName = "Konami Twinkle", ShortName = "kt")]
        KonamiTwinkle,

        [System(Category = SystemCategory.Arcade, Available = false, LongName = "Konami PC-based Systems")]
        KonamiVarious,

        [System(Category = SystemCategory.Arcade, Available = false, LongName = "Merit Industries Boardwalk")]
        MeritIndustriesBoardwalk,

        [System(Category = SystemCategory.Arcade, Available = false, LongName = "Merit Industries MegaTouch Force")]
        MeritIndustriesMegaTouchForce,

        [System(Category = SystemCategory.Arcade, Available = false, LongName = "Merit Industries MegaTouch ION")]
        MeritIndustriesMegaTouchION,

        [System(Category = SystemCategory.Arcade, Available = false, LongName = "Merit Industries MegaTouch Maxx")]
        MeritIndustriesMegaTouchMaxx,

        [System(Category = SystemCategory.Arcade, Available = false, LongName = "Merit Industries MegaTouch XL")]
        MeritIndustriesMegaTouchXL,

        [System(Category = SystemCategory.Arcade, LongName = "Namco · Sega · Nintendo Triforce", ShortName = "triforce", HasCues = true, HasDat = true, HasGdi = true)]
        NamcoSegaNintendoTriforce,

        [System(Category = SystemCategory.Arcade, LongName = "Namco System 12", ShortName = "ns12")]
        NamcoSystem12,

        [System(Category = SystemCategory.Arcade, LongName = "Namco System 246 / System 256", ShortName = "ns246", HasCues = true, HasDat = true)]
        NamcoSystem246256,

        [System(Category = SystemCategory.Arcade, Available = false, LongName = "New Jatre CD-i")]
        NewJatreCDi,

        [System(Category = SystemCategory.Arcade, Available = false, LongName = "Nichibutsu High Rate System")]
        NichibutsuHighRateSystem,

        [System(Category = SystemCategory.Arcade, Available = false, LongName = "Nichibutsu Super CD")]
        NichibutsuSuperCD,

        [System(Category = SystemCategory.Arcade, Available = false, LongName = "Nichibutsu X-Rate System")]
        NichibutsuXRateSystem,

        [System(Category = SystemCategory.Arcade, LongName = "Panasonic M2", ShortName = "m2", IsBanned = true, HasCues = true, HasDat = true)]
        PanasonicM2,

        [System(Category = SystemCategory.Arcade, Available = false, LongName = "PhotoPlay PC-based Systems")]
        PhotoPlayVarious,

        [System(Category = SystemCategory.Arcade, Available = false, LongName = "Raw Thrills PC-based Systems")]
        RawThrillsVarious,

        [System(Category = SystemCategory.Arcade, Available = false, LongName = "Sega ALLS")]
        SegaALLS,

        [System(Category = SystemCategory.Arcade, LongName = "Sega Chihiro", ShortName = "chihiro", HasCues = true, HasDat = true, HasGdi = true)]
        SegaChihiro,

        [System(Category = SystemCategory.Arcade, Available = false, LongName = "Sega Europa-R")]
        SegaEuropaR,

        [System(Category = SystemCategory.Arcade, LongName = "Sega Lindbergh", ShortName = "lindbergh", HasDat = true)]
        SegaLindbergh,

        [System(Category = SystemCategory.Arcade, LongName = "Sega Naomi", ShortName = "naomi", HasCues = true, HasDat = true, HasGdi = true)]
        SegaNaomi,

        [System(Category = SystemCategory.Arcade, LongName = "Sega Naomi 2", ShortName = "naomi2", HasCues = true, HasDat = true, HasGdi = true)]
        SegaNaomi2,

        [System(Category = SystemCategory.Arcade, Available = false, LongName = "Sega Nu")]
        SegaNu,

        [System(Category = SystemCategory.Arcade, LongName = "Sega RingEdge", ShortName = "sre", HasDat = true)]
        SegaRingEdge,

        [System(Category = SystemCategory.Arcade, LongName = "Sega RingEdge 2", ShortName = "sre2", HasDat = true)]
        SegaRingEdge2,

        [System(Category = SystemCategory.Arcade, Available = false, LongName = "Sega RingWide")]
        SegaRingWide,

        [System(Category = SystemCategory.Arcade, Available = false, LongName = "Sega System 32")]
        SegaSystem32,

        [System(Category = SystemCategory.Arcade, LongName = "Sega Titan Video", ShortName = "stv")]
        SegaTitanVideo,

        [System(Category = SystemCategory.Arcade, Available = false, LongName = "Seibu CATS System")]
        SeibuCATSSystem,

        [System(Category = SystemCategory.Arcade, LongName = "TAB-Austria Quizard", ShortName = "quizard", HasCues = true, HasDat = true)]
        TABAustriaQuizard,

        [System(Category = SystemCategory.Arcade, Available = false, LongName = "Tsunami TsuMo Multi-Game Motion System")]
        TsunamiTsuMoMultiGameMotionSystem,

        // End of arcade section delimiter
        MarkerArcadeEnd,

        #endregion

        #region Other

        [System(Category = SystemCategory.Other, LongName = "Audio CD", ShortName = "audio-cd", IsBanned = true, HasCues = true, HasDat = true)]
        AudioCD,

        [System(Category = SystemCategory.Other, LongName = "BD-Video", ShortName = "bd-video", IsBanned = true, HasDat = true)]
        BDVideo,

        [System(Category = SystemCategory.Other, Available = false, LongName = "DVD-Audio")]
        DVDAudio,

        [System(Category = SystemCategory.Other, LongName = "DVD-Video", ShortName = "dvd-video", IsBanned = true, HasDat = true)]
        DVDVideo,

        [System(Category = SystemCategory.Other, LongName = "Enhanced CD", ShortName = "enhanced-cd", IsBanned = true)]
        EnhancedCD,

        [System(Category = SystemCategory.Other, LongName = "HD DVD-Video", ShortName = "hddvd-video", IsBanned = true, HasDat = true)]
        HDDVDVideo,

        [System(Category = SystemCategory.Other, LongName = "Navisoft Naviken 2.1", ShortName = "navi21", IsBanned = true, HasCues = true, HasDat = true)]
        NavisoftNaviken21,

        [System(Category = SystemCategory.Other, LongName = "Palm OS", ShortName = "palm", HasCues = true, HasDat = true)]
        PalmOS,

        [System(Category = SystemCategory.Other, LongName = "Photo CD", ShortName = "photo-cd", HasCues = true, HasDat = true)]
        PhotoCD,

        [System(Category = SystemCategory.Other, LongName = "PlayStation GameShark Updates", ShortName = "psxgs", HasCues = true, HasDat = true)]
        PlayStationGameSharkUpdates,

        [System(Category = SystemCategory.Other, LongName = "Pocket PC", ShortName = "ppc", HasCues = true, HasDat = true)]
        PocketPC,

        [System(Category = SystemCategory.Other, Available = false, LongName = "Rainbow Disc")]
        RainbowDisc,

        [System(Category = SystemCategory.Other, LongName = "Sega Prologue 21 Multimedia Karaoke System", ShortName = "sp21", HasCues = true, HasDat = true)]
        SegaPrologue21MultimediaKaraokeSystem,

        [System(Category = SystemCategory.Other, Available = false, LongName = "Sony Electronic Book")]
        SonyElectronicBook,

        [System(Category = SystemCategory.Other, Available = false, LongName = "Super Audio CD")]
        SuperAudioCD,

        [System(Category = SystemCategory.Other, LongName = "Tao iKTV", ShortName = "iktv")]
        TaoiKTV,

        [System(Category = SystemCategory.Other, LongName = "Tomy Kiss-Site", ShortName = "ksite", HasCues = true, HasDat = true)]
        TomyKissSite,

        [System(Category = SystemCategory.Other, LongName = "Video CD", ShortName = "vcd", IsBanned = true, HasCues = true, HasDat = true)]
        VideoCD,

        // End of other section delimiter
        MarkerOtherEnd,

        #endregion
    }

    /// <summary>
    /// List of all known regions
    /// </summary>
    /// <remarks>
    /// https://en.wikipedia.org/wiki/ISO_3166-1_alpha-2
    /// 
    /// Because of how Redump stores region IDs, the second letter of each
    /// code is lower-cased. In any other system, both letters would be
    /// capitalized properly.
    /// </remarks>
    public enum Region
    {
        // TODO: Should "regions" and multi-country sets be phased out?
        // TODO: Should "regions" be moved to the end?

        #region A

        [HumanReadable(LongName = "Afghanistan", ShortName = "Af")]
        Afghanistan,

        [HumanReadable(LongName = "Åland Islands", ShortName = "Ax")]
        AlandIslands,

        [HumanReadable(LongName = "Albania", ShortName = "Al")]
        Albania,

        [HumanReadable(LongName = "Algeria", ShortName = "Dz")]
        Algeria,

        [HumanReadable(LongName = "American Samoa", ShortName = "As")]
        AmericanSamoa,

        [HumanReadable(LongName = "Andorra", ShortName = "Ad")]
        Andorra,

        [HumanReadable(LongName = "Angola", ShortName = "Ao")]
        Angola,

        [HumanReadable(LongName = "Anguilla", ShortName = "Ai")]
        Anguilla,

        [HumanReadable(LongName = "Antarctica", ShortName = "Aq")]
        Antarctica,

        [HumanReadable(LongName = "Antigua and Barbuda", ShortName = "Ag")]
        AntiguaAndBarbuda,

        [HumanReadable(LongName = "Argentina", ShortName = "Ar")]
        Argentina,

        [HumanReadable(LongName = "Armenia", ShortName = "Am")]
        Armenia,

        [HumanReadable(LongName = "Aruba", ShortName = "Aw")]
        Aruba,

        [HumanReadable(LongName = "Ascension Island", ShortName = "Ac")]
        AscensionIsland,

        [HumanReadable(LongName = "Asia", ShortName = "A")]
        Asia,

        [HumanReadable(LongName = "Asia, Europe", ShortName = "A,E")]
        AsiaEurope,

        [HumanReadable(LongName = "Asia, USA", ShortName = "A,U")]
        AsiaUSA,

        [HumanReadable(LongName = "Australia", ShortName = "Au")]
        Australia,

        [HumanReadable(LongName = "Australia, Germany", ShortName = "Au,G")]
        AustraliaGermany,

        [HumanReadable(LongName = "Australia, New Zealand", ShortName = "Au,Nz")]
        AustraliaNewZealand,

        [HumanReadable(LongName = "Austria", ShortName = "At")]
        Austria,

        [HumanReadable(LongName = "Austria, Switzerland", ShortName = "At,Ch")]
        AustriaSwitzerland,

        [HumanReadable(LongName = "Azerbaijan", ShortName = "Az")]
        Azerbaijan,

        #endregion

        #region B

        [HumanReadable(LongName = "Bahamas", ShortName = "Bs")]
        Bahamas,

        [HumanReadable(LongName = "Bahrain", ShortName = "Bh")]
        Bahrain,

        [HumanReadable(LongName = "Bangladesh", ShortName = "Bd")]
        Bangladesh,

        [HumanReadable(LongName = "Barbados", ShortName = "Bb")]
        Barbados,

        [HumanReadable(LongName = "Belarus", ShortName = "By")]
        Belarus,

        [HumanReadable(LongName = "Belgium", ShortName = "Be")]
        Belgium,

        [HumanReadable(LongName = "Belgium, Netherlands", ShortName = "Be,N")]
        BelgiumNetherlands,

        [HumanReadable(LongName = "Belize", ShortName = "Bz")]
        Belize,

        [HumanReadable(LongName = "Benin", ShortName = "Bj")]
        Benin,

        [HumanReadable(LongName = "Bermuda", ShortName = "Bm")]
        Bermuda,

        [HumanReadable(LongName = "Bhutan", ShortName = "Bt")]
        Bhutan,

        [HumanReadable(LongName = "Bolivia", ShortName = "Bo")]
        Bolivia,

        [HumanReadable(LongName = "Bonaire, Sint Eustatius and Saba", ShortName = "Bq")]
        Bonaire,

        [HumanReadable(LongName = "Bosnia and Herzegovina", ShortName = "Ba")]
        BosniaAndHerzegovina,

        [HumanReadable(LongName = "Botswana", ShortName = "Bw")]
        Botswana,

        [HumanReadable(LongName = "Bouvet Island", ShortName = "Bv")]
        BouvetIsland,

        // Should be "Br"
        [HumanReadable(LongName = "Brazil", ShortName = "B")]
        Brazil,

        [HumanReadable(LongName = "British Indian Ocean Territory", ShortName = "Io")]
        BritishIndianOceanTerritory,

        [HumanReadable(LongName = "Brunei Darussalam", ShortName = "Bn")]
        BruneiDarussalam,

        [HumanReadable(LongName = "Bulgaria", ShortName = "Bg")]
        Bulgaria,

        [HumanReadable(LongName = "Burkina Faso", ShortName = "Bf")]
        BurkinaFaso,

        [HumanReadable(LongName = "Burundi", ShortName = "Bi")]
        Burundi,

        #endregion

        #region C

        [HumanReadable(LongName = "Cabo Verde", ShortName = "Cv")]
        CaboVerde,

        [HumanReadable(LongName = "Cambodia", ShortName = "Kh")]
        Cambodia,

        [HumanReadable(LongName = "Cameroon", ShortName = "Cm")]
        Cameroon,

        [HumanReadable(LongName = "Canada", ShortName = "Ca")]
        Canada,

        [HumanReadable(LongName = "Canary Islands", ShortName = "Ic")]
        CanaryIslands,

        [HumanReadable(LongName = "Cayman Islands", ShortName = "Ky")]
        CaymanIslands,

        [HumanReadable(LongName = "Central African Republic", ShortName = "Cf")]
        CentralAfricanRepublic,

        [HumanReadable(LongName = "Ceuta, Melilla", ShortName = "Ea")]
        CeutaMelilla,

        [HumanReadable(LongName = "Chad", ShortName = "Td")]
        Chad,

        [HumanReadable(LongName = "Chile", ShortName = "Cl")]
        Chile,

        // Should be "Cn"
        [HumanReadable(LongName = "China", ShortName = "C")]
        China,

        [HumanReadable(LongName = "Christmas Island", ShortName = "Cx")]
        ChristmasIsland,

        [HumanReadable(LongName = "Clipperton Island", ShortName = "Cp")]
        ClippertonIsland,

        [HumanReadable(LongName = "Cocos (Keeling) Islands", ShortName = "Cc")]
        CocosIslands,

        [HumanReadable(LongName = "Colombia", ShortName = "Co")]
        Colombia,

        [HumanReadable(LongName = "Comoros", ShortName = "Km")]
        Comoros,

        [HumanReadable(LongName = "Congo", ShortName = "Cg")]
        Congo,

        [HumanReadable(LongName = "Cook Islands", ShortName = "Ck")]
        CookIslands,

        [HumanReadable(LongName = "Costa Rica", ShortName = "Cr")]
        CostaRica,

        [HumanReadable(LongName = "Côte d'Ivoire", ShortName = "Ci")]
        CoteDIvoire,

        [HumanReadable(LongName = "Croatia", ShortName = "Hr")]
        Croatia,

        [HumanReadable(LongName = "Cuba", ShortName = "Cu")]
        Cuba,

        [HumanReadable(LongName = "Curaçao", ShortName = "Cw")]
        Curacao,

        [HumanReadable(LongName = "Cyprus", ShortName = "Cy")]
        Cyprus,

        [HumanReadable(LongName = "Czechia", ShortName = "Cz")]
        Czechia,

        [HumanReadable(LongName = "Czechoslovakia", ShortName = "Cs")]
        Czechoslovakia,

        #endregion

        #region D

        // Zaire was "Zr"
        [HumanReadable(LongName = "Democratic Republic of the Congo (Zaire)", ShortName = "Cd")]
        DemocraticRepublicOfTheCongo,

        [HumanReadable(LongName = "Denmark", ShortName = "Dk")]
        Denmark,

        [HumanReadable(LongName = "Diego Garcia", ShortName = "Dg")]
        DiegoGarcia,

        [HumanReadable(LongName = "Djibouti", ShortName = "Dj")]
        Djibouti,

        [HumanReadable(LongName = "Dominica", ShortName = "Dm")]
        Dominica,

        [HumanReadable(LongName = "Dominican Republic", ShortName = "Do")]
        DominicanRepublic,

        #endregion

        #region E

        [HumanReadable(LongName = "Ecuador", ShortName = "Ec")]
        Ecuador,

        [HumanReadable(LongName = "Egypt", ShortName = "Eg")]
        Egypt,

        [HumanReadable(LongName = "El Salvador", ShortName = "Sv")]
        ElSalvador,

        [HumanReadable(LongName = "Equatorial Guinea", ShortName = "Gq")]
        EquatorialGuinea,

        [HumanReadable(LongName = "Eritrea", ShortName = "Er")]
        Eritrea,

        [HumanReadable(LongName = "Estonia", ShortName = "Ee")]
        Estonia,

        [HumanReadable(LongName = "Eswatini", ShortName = "Sz")]
        Eswatini,

        [HumanReadable(LongName = "Ethiopia", ShortName = "Et")]
        Ethiopia,

        [HumanReadable(LongName = "Europe", ShortName = "E")]
        Europe,

        [HumanReadable(LongName = "Europe, Asia", ShortName = "E,A")]
        EuropeAsia,

        [HumanReadable(LongName = "Europe, Australia", ShortName = "E,Au")]
        EuropeAustralia,

        [HumanReadable(LongName = "Europe, Canada", ShortName = "E,Ca")]
        EuropeCanada,

        [HumanReadable(LongName = "Europe, Germany", ShortName = "E,G")]
        EuropeGermany,

        // Commented out to avoid confusion
        //[HumanReadable(LongName = "European Union", ShortName = "Eu")]
        //EuropeanUnion,

        // Commented out to avoid confusion
        //[HumanReadable(LongName = "Eurozone", ShortName = "Ez")]
        //Eurozone,

        [HumanReadable(LongName = "Export", ShortName = "Ex")]
        Export,

        #endregion

        #region F

        [HumanReadable(LongName = "Falkland Islands (Malvinas)", ShortName = "Fk")]
        FalklandIslands,

        [HumanReadable(LongName = "Faroe Islands", ShortName = "Fo")]
        FaroeIslands,

        [HumanReadable(LongName = "Federated States of Micronesia", ShortName = "Fm")]
        FederatedStatesOfMicronesia,

        [HumanReadable(LongName = "Fiji", ShortName = "Fj")]
        Fiji,

        // Formerly "Sf"
        [HumanReadable(LongName = "Finland", ShortName = "Fi")]
        Finland,

        // Should be "Fr"
        [HumanReadable(LongName = "France", ShortName = "F")]
        France,

        // Commented out to avoid confusion
        //[HumanReadable(LongName = "France, Metropolitan", ShortName = "Fx")]
        //FranceMetropolitan,

        [HumanReadable(LongName = "France, Spain", ShortName = "F,S")]
        FranceSpain,

        [HumanReadable(LongName = "French Guiana", ShortName = "Gf")]
        FrenchGuiana,

        [HumanReadable(LongName = "French Polynesia", ShortName = "Pf")]
        FrenchPolynesia,

        [HumanReadable(LongName = "French Southern Territories", ShortName = "Tf")]
        FrenchSouthernTerritories,

        #endregion

        #region G

        [HumanReadable(LongName = "Gabon", ShortName = "Ga")]
        Gabon,

        [HumanReadable(LongName = "Gambia", ShortName = "Gm")]
        Gambia,

        [HumanReadable(LongName = "Georgia", ShortName = "Ge")]
        Georgia,

        // Should be "De"
        [HumanReadable(LongName = "Germany", ShortName = "G")]
        Germany,

        [HumanReadable(LongName = "Ghana", ShortName = "Gh")]
        Ghana,

        [HumanReadable(LongName = "Gibraltar", ShortName = "Gi")]
        Gibraltar,

        [HumanReadable(LongName = "Greater China", ShortName = "GC")]
        GreaterChina,

        [HumanReadable(LongName = "Greece", ShortName = "Gr")]
        Greece,

        [HumanReadable(LongName = "Greenland", ShortName = "Gl")]
        Greenland,

        [HumanReadable(LongName = "Grenada", ShortName = "Gd")]
        Grenada,

        [HumanReadable(LongName = "Guadeloupe", ShortName = "Gp")]
        Guadeloupe,

        [HumanReadable(LongName = "Guam", ShortName = "Gu")]
        Guam,

        [HumanReadable(LongName = "Guatemala", ShortName = "Gt")]
        Guatemala,

        [HumanReadable(LongName = "Guernsey", ShortName = "Gg")]
        Guernsey,

        [HumanReadable(LongName = "Guinea", ShortName = "Gn")]
        Guinea,

        [HumanReadable(LongName = "Guinea-Bissau", ShortName = "Gw")]
        GuineaBissau,

        [HumanReadable(LongName = "Guyana", ShortName = "Gy")]
        Guyana,

        #endregion

        #region H

        [HumanReadable(LongName = "Haiti", ShortName = "Ht")]
        Haiti,

        [HumanReadable(LongName = "Heard Island and McDonald Islands", ShortName = "Hm")]
        HeardIslandAndMcDonaldIslands,

        [HumanReadable(LongName = "Holy See (Vatican City)", ShortName = "Va")]
        HolySee,

        [HumanReadable(LongName = "Honduras", ShortName = "Hn")]
        Honduras,

        [HumanReadable(LongName = "Hong Kong", ShortName = "Hk")]
        HongKong,

        // Should be "Hu"
        [HumanReadable(LongName = "Hungary", ShortName = "H")]
        Hungary,

        #endregion

        #region I

        [HumanReadable(LongName = "Iceland", ShortName = "Is")]
        Iceland,

        [HumanReadable(LongName = "India", ShortName = "In")]
        India,

        [HumanReadable(LongName = "Indonesia", ShortName = "Id")]
        Indonesia,

        [HumanReadable(LongName = "Iran", ShortName = "Ir")]
        Iran,

        [HumanReadable(LongName = "Iraq", ShortName = "Iq")]
        Iraq,

        [HumanReadable(LongName = "Ireland", ShortName = "Ie")]
        Ireland,

        [HumanReadable(LongName = "Island of Sark", ShortName = "Cq")]
        IslandOfSark,

        [HumanReadable(LongName = "Isle of Man", ShortName = "Im")]
        IsleOfMan,

        [HumanReadable(LongName = "Israel", ShortName = "Il")]
        Israel,

        // Should be "It"
        [HumanReadable(LongName = "Italy", ShortName = "I")]
        Italy,

        #endregion

        #region J

        [HumanReadable(LongName = "Jamaica", ShortName = "Jm")]
        Jamaica,

        // Should be "Jp"
        [HumanReadable(LongName = "Japan", ShortName = "J")]
        Japan,

        [HumanReadable(LongName = "Japan, Asia", ShortName = "J,A")]
        JapanAsia,

        [HumanReadable(LongName = "Japan, Europe", ShortName = "J,E")]
        JapanEurope,

        [HumanReadable(LongName = "Japan, Korea", ShortName = "J,K")]
        JapanKorea,

        [HumanReadable(LongName = "Japan, USA", ShortName = "J,U")]
        JapanUSA,

        [HumanReadable(LongName = "Jersey", ShortName = "Je")]
        Jersey,

        [HumanReadable(LongName = "Jordan", ShortName = "Jo")]
        Jordan,

        #endregion

        #region K

        [HumanReadable(LongName = "Kazakhstan", ShortName = "Kz")]
        Kazakhstan,

        [HumanReadable(LongName = "Kenya", ShortName = "Ke")]
        Kenya,

        [HumanReadable(LongName = "Kiribati", ShortName = "Ki")]
        Kiribati,

        [HumanReadable(LongName = "Korea (Democratic People's Republic of Korea)", ShortName = "Kp")]
        NorthKorea,

        // Should be "Kr"
        [HumanReadable(LongName = "Korea (Republic of Korea)", ShortName = "K")]
        SouthKorea,

        [HumanReadable(LongName = "Kuwait", ShortName = "Kw")]
        Kuwait,

        [HumanReadable(LongName = "Kyrgyzstan", ShortName = "Kg")]
        Kyrgyzstan,

        #endregion

        #region L

        [HumanReadable(LongName = "(Laos) Lao People's Democratic Republic", ShortName = "La")]
        Laos,

        [HumanReadable(LongName = "Latin America", ShortName = "LAm")]
        LatinAmerica,

        [HumanReadable(LongName = "Latvia", ShortName = "Lv")]
        Latvia,

        [HumanReadable(LongName = "Lebanon", ShortName = "Lb")]
        Lebanon,

        [HumanReadable(LongName = "Lesotho", ShortName = "Ls")]
        Lesotho,

        [HumanReadable(LongName = "Liberia", ShortName = "Lr")]
        Liberia,

        [HumanReadable(LongName = "Libya", ShortName = "Ly")]
        Libya,

        [HumanReadable(LongName = "Liechtenstein", ShortName = "Li")]
        Liechtenstein,

        [HumanReadable(LongName = "Lithuania", ShortName = "Lt")]
        Lithuania,

        [HumanReadable(LongName = "Luxembourg", ShortName = "Lu")]
        Luxembourg,

        #endregion

        #region M

        [HumanReadable(LongName = "Macao", ShortName = "Mo")]
        Macao,

        [HumanReadable(LongName = "Madagascar", ShortName = "Mg")]
        Madagascar,

        [HumanReadable(LongName = "Malawi", ShortName = "Mw")]
        Malawi,

        [HumanReadable(LongName = "Malaysia", ShortName = "My")]
        Malaysia,

        [HumanReadable(LongName = "Maldives", ShortName = "Mv")]
        Maldives,

        [HumanReadable(LongName = "Mali", ShortName = "Ml")]
        Mali,

        [HumanReadable(LongName = "Malta", ShortName = "Mt")]
        Malta,

        [HumanReadable(LongName = "Marshall Islands", ShortName = "Mh")]
        MarshallIslands,

        [HumanReadable(LongName = "Martinique", ShortName = "Mq")]
        Martinique,

        [HumanReadable(LongName = "Mauritania", ShortName = "Mr")]
        Mauritania,

        [HumanReadable(LongName = "Mauritius", ShortName = "Mu")]
        Mauritius,

        [HumanReadable(LongName = "Mayotte", ShortName = "Yt")]
        Mayotte,

        [HumanReadable(LongName = "Mexico", ShortName = "Mx")]
        Mexico,

        [HumanReadable(LongName = "Monaco", ShortName = "Mc")]
        Monaco,

        [HumanReadable(LongName = "Mongolia", ShortName = "Mn")]
        Mongolia,

        [HumanReadable(LongName = "Montenegro", ShortName = "Me")]
        Montenegro,

        [HumanReadable(LongName = "Montserrat", ShortName = "Ms")]
        Montserrat,

        [HumanReadable(LongName = "Morocco", ShortName = "Ma")]
        Morocco,

        [HumanReadable(LongName = "Mozambique", ShortName = "Mz")]
        Mozambique,

        // Burma was "Bu"
        [HumanReadable(LongName = "Myanmar (Burma)", ShortName = "Mm")]
        Myanmar,

        #endregion

        #region N

        [HumanReadable(LongName = "Namibia", ShortName = "Na")]
        Namibia,

        [HumanReadable(LongName = "Nauru", ShortName = "Nr")]
        Nauru,

        [HumanReadable(LongName = "Nepal", ShortName = "Np")]
        Nepal,

        // Should be "Nl"
        [HumanReadable(LongName = "Netherlands", ShortName = "N")]
        Netherlands,

        [HumanReadable(LongName = "Netherlands Antilles", ShortName = "An")]
        NetherlandsAntilles,

        // Commented out to avoid confusion
        //[HumanReadable(LongName = "Neutral Zone", ShortName = "Nt")]
        //NeutralZone,

        [HumanReadable(LongName = "New Caledonia", ShortName = "Nc")]
        NewCaledonia,

        [HumanReadable(LongName = "New Zealand", ShortName = "Nz")]
        NewZealand,

        [HumanReadable(LongName = "Nicaragua", ShortName = "Ni")]
        Nicaragua,

        [HumanReadable(LongName = "Niger", ShortName = "Ne")]
        Niger,

        [HumanReadable(LongName = "Nigeria", ShortName = "Ng")]
        Nigeria,

        [HumanReadable(LongName = "Niue", ShortName = "Nu")]
        Niue,

        [HumanReadable(LongName = "Norfolk Island", ShortName = "Nf")]
        NorfolkIsland,

        [HumanReadable(LongName = "North Macedonia", ShortName = "Mk")]
        NorthMacedonia,

        [HumanReadable(LongName = "Northern Mariana Islands", ShortName = "Mp")]
        NorthernMarianaIslands,

        [HumanReadable(LongName = "Norway", ShortName = "No")]
        Norway,

        #endregion

        #region O

        [HumanReadable(LongName = "Oman", ShortName = "Om")]
        Oman,

        #endregion

        #region P

        [HumanReadable(LongName = "Pakistan", ShortName = "Pk")]
        Pakistan,

        [HumanReadable(LongName = "Palau", ShortName = "Pw")]
        Palau,

        [HumanReadable(LongName = "Panama", ShortName = "Pa")]
        Panama,

        [HumanReadable(LongName = "Papua New Guinea", ShortName = "Pg")]
        PapuaNewGuinea,

        [HumanReadable(LongName = "Paraguay", ShortName = "Py")]
        Paraguay,

        [HumanReadable(LongName = "Peru", ShortName = "Pe")]
        Peru,

        [HumanReadable(LongName = "Philippines", ShortName = "Ph")]
        Philippines,

        [HumanReadable(LongName = "Pitcairn", ShortName = "Pn")]
        Pitcairn,

        // Should be "Pl"
        [HumanReadable(LongName = "Poland", ShortName = "P")]
        Poland,

        [HumanReadable(LongName = "Portugal", ShortName = "Pt")]
        Portugal,

        [HumanReadable(LongName = "Puerto Rico", ShortName = "Pr")]
        PuertoRico,

        #endregion

        #region Q

        [HumanReadable(LongName = "Qatar", ShortName = "Qa")]
        Qatar,

        #endregion

        #region R

        [HumanReadable(LongName = "Republic of Moldova", ShortName = "Md")]
        RepublicOfMoldova,

        [HumanReadable(LongName = "Réunion", ShortName = "Re")]
        Reunion,

        [HumanReadable(LongName = "Romania", ShortName = "Ro")]
        Romania,

        // Should be "Ru"
        [HumanReadable(LongName = "Russian Federation", ShortName = "R")]
        RussianFederation,

        [HumanReadable(LongName = "Rwanda", ShortName = "Rw")]
        Rwanda,

        #endregion

        #region S

        [HumanReadable(LongName = "Saint Barthélemy", ShortName = "Bl")]
        SaintBarthelemy,

        [HumanReadable(LongName = "Saint Helena, Ascension and Tristan da Cunha", ShortName = "Sh")]
        SaintHelena,

        [HumanReadable(LongName = "Saint Kitts and Nevis", ShortName = "Kn")]
        SaintKittsAndNevis,

        [HumanReadable(LongName = "Saint Lucia", ShortName = "Lc")]
        SaintLucia,

        [HumanReadable(LongName = "Saint Martin", ShortName = "Mf")]
        SaintMartin,

        [HumanReadable(LongName = "Saint Pierre and Miquelon", ShortName = "Pm")]
        SaintPierreAndMiquelon,

        [HumanReadable(LongName = "Saint Vincent and the Grenadines", ShortName = "Vc")]
        SaintVincentAndTheGrenadines,

        [HumanReadable(LongName = "Samoa", ShortName = "Ws")]
        Samoa,

        [HumanReadable(LongName = "San Marino", ShortName = "Sm")]
        SanMarino,

        [HumanReadable(LongName = "Sao Tome and Principe", ShortName = "St")]
        SaoTomeAndPrincipe,

        [HumanReadable(LongName = "Saudi Arabia", ShortName = "Sa")]
        SaudiArabia,

        [HumanReadable(LongName = "Scandinavia", ShortName = "Sca")]
        Scandinavia,

        [HumanReadable(LongName = "Senegal", ShortName = "Sn")]
        Senegal,

        [HumanReadable(LongName = "Serbia", ShortName = "Rs")]
        Serbia,

        [HumanReadable(LongName = "Seychelles", ShortName = "Sc")]
        Seychelles,

        [HumanReadable(LongName = "Sierra Leone", ShortName = "Sl")]
        SierraLeone,

        [HumanReadable(LongName = "Singapore", ShortName = "Sg")]
        Singapore,

        [HumanReadable(LongName = "Sint Maarten", ShortName = "Sx")]
        SintMaarten,

        [HumanReadable(LongName = "Slovakia", ShortName = "Sk")]
        Slovakia,

        [HumanReadable(LongName = "Slovenia", ShortName = "Si")]
        Slovenia,

        [HumanReadable(LongName = "Solomon Islands", ShortName = "Sb")]
        SolomonIslands,

        [HumanReadable(LongName = "Somalia", ShortName = "So")]
        Somalia,

        [HumanReadable(LongName = "South Africa", ShortName = "Za")]
        SouthAfrica,

        [HumanReadable(LongName = "South Georgia and the South Sandwich Islands", ShortName = "Gs")]
        SouthGeorgia,

        [HumanReadable(LongName = "South Sudan", ShortName = "Ss")]
        SouthSudan,

        // Should be "Es"
        [HumanReadable(LongName = "Spain", ShortName = "S")]
        Spain,

        [HumanReadable(LongName = "Spain, Portugal", ShortName = "S,Pt")]
        SpainPortugal,

        [HumanReadable(LongName = "Sri Lanka", ShortName = "Lk")]
        SriLanka,

        [HumanReadable(LongName = "State of Palestine", ShortName = "Ps")]
        StateOfPalestine,

        [HumanReadable(LongName = "Sudan", ShortName = "Sd")]
        Sudan,

        [HumanReadable(LongName = "Suriname", ShortName = "Sr")]
        Suriname,

        [HumanReadable(LongName = "Svalbard and Jan Mayen", ShortName = "Sj")]
        SvalbardAndJanMayen,

        // Should be "Se"
        [HumanReadable(LongName = "Sweden", ShortName = "Sw")]
        Sweden,

        [HumanReadable(LongName = "Switzerland", ShortName = "Ch")]
        Switzerland,

        [HumanReadable(LongName = "Syrian Arab Republic", ShortName = "Sy")]
        SyrianArabRepublic,

        #endregion

        #region T

        [HumanReadable(LongName = "Taiwan", ShortName = "Tw")]
        Taiwan,

        [HumanReadable(LongName = "Tajikistan", ShortName = "Tj")]
        Tajikistan,

        [HumanReadable(LongName = "Thailand", ShortName = "Th")]
        Thailand,

        // East Timor was "Tp" 
        [HumanReadable(LongName = "Timor-Leste (East Timor)", ShortName = "Tl")]
        TimorLeste,

        [HumanReadable(LongName = "Togo", ShortName = "Tg")]
        Togo,

        [HumanReadable(LongName = "Tokelau", ShortName = "Tk")]
        Tokelau,

        [HumanReadable(LongName = "Tonga", ShortName = "To")]
        Tonga,

        [HumanReadable(LongName = "Trinidad and Tobago", ShortName = "Tt")]
        TrinidadAndTobago,

        [HumanReadable(LongName = "Tristan da Cunha", ShortName = "Ta")]
        TristanDaCunha,

        [HumanReadable(LongName = "Tunisia", ShortName = "Tn")]
        Tunisia,

        [HumanReadable(LongName = "Turkey", ShortName = "Tr")]
        Turkey,

        [HumanReadable(LongName = "Turkmenistan", ShortName = "Tm")]
        Turkmenistan,

        [HumanReadable(LongName = "Turks and Caicos Islands", ShortName = "Tc")]
        TurksAndCaicosIslands,

        [HumanReadable(LongName = "Tuvalu", ShortName = "Tv")]
        Tuvalu,

        #endregion

        #region U

        [HumanReadable(LongName = "Uganda", ShortName = "Ug")]
        Uganda,

        // Should be both "Gb" and "Uk"
        // United Kingdom of Great Britain and Northern Ireland
        [HumanReadable(LongName = "UK", ShortName = "Uk")]
        UnitedKingdom,

        [HumanReadable(LongName = "UK, Australia", ShortName = "Uk,Au")]
        UKAustralia,

        [HumanReadable(LongName = "Ukraine", ShortName = "Ue")]
        Ukraine,

        [HumanReadable(LongName = "United Arab Emirates", ShortName = "Ae")]
        UnitedArabEmirates,

        // Commented out to avoid confusion
        //[HumanReadable(LongName = "United Nations", ShortName = "Un")]
        //UnitedNations,

        [HumanReadable(LongName = "United Republic of Tanzania", ShortName = "Tz")]
        UnitedRepublicOfTanzania,

        [HumanReadable(LongName = "United States Minor Outlying Islands", ShortName = "Um")]
        UnitedStatesMinorOutlyingIslands,

        [HumanReadable(LongName = "Uruguay", ShortName = "Uy")]
        Uruguay,

        // Should be "Us"
        // United States of America
        [HumanReadable(LongName = "USA", ShortName = "U")]
        UnitedStatesOfAmerica,

        [HumanReadable(LongName = "USA, Asia", ShortName = "U,A")]
        USAAsia,

        [HumanReadable(LongName = "USA, Australia", ShortName = "U,Au")]
        USAAustralia,

        [HumanReadable(LongName = "USA, Brazil", ShortName = "U,B")]
        USABrazil,

        [HumanReadable(LongName = "USA, Canada", ShortName = "U,Ca")]
        USACanada,

        [HumanReadable(LongName = "USA, Europe", ShortName = "U,E")]
        USAEurope,

        [HumanReadable(LongName = "USA, Germany", ShortName = "U,G")]
        USAGermany,

        [HumanReadable(LongName = "USA, Japan", ShortName = "U,J")]
        USAJapan,

        [HumanReadable(LongName = "USA, Korea", ShortName = "U,K")]
        USAKorea,

        [HumanReadable(LongName = "USSR", ShortName = "Su")]
        USSR,

        [HumanReadable(LongName = "Uzbekistan", ShortName = "Uz")]
        Uzbekistan,

        #endregion

        #region V

        [HumanReadable(LongName = "Vanuatu", ShortName = "Vu")]
        Vanuatu,

        [HumanReadable(LongName = "Venezuela", ShortName = "Ve")]
        Venezuela,

        [HumanReadable(LongName = "Viet Nam", ShortName = "Vn")]
        VietNam,

        [HumanReadable(LongName = "Virgin Islands (British)", ShortName = "Vg")]
        BritishVirginIslands,

        [HumanReadable(LongName = "Virgin Islands (US)", ShortName = "Vi")]
        USVirginIslands,

        #endregion

        #region W

        [HumanReadable(LongName = "Wallis and Futuna", ShortName = "Wf")]
        WallisAndFutuna,

        [HumanReadable(LongName = "Western Sahara", ShortName = "Eh")]
        WesternSahara,

        [HumanReadable(LongName = "World", ShortName = "W")]
        World,

        #endregion

        #region Y

        [HumanReadable(LongName = "Yemen", ShortName = "Ye")]
        Yemen,

        [HumanReadable(LongName = "Yugoslavia", ShortName = "Yu")]
        Yugoslavia,

        #endregion

        #region Z

        [HumanReadable(LongName = "Zambia", ShortName = "Zm")]
        Zambia,

        [HumanReadable(LongName = "Zimbabwe", ShortName = "Zw")]
        Zimbabwe,

        #endregion
    }

    /// <summary>
    /// List of all Redump site codes
    /// </summary>
    public enum SiteCode
    {
        [HumanReadable(ShortName = "[T:ACC]", LongName = "<b>Acclaim ID</b>:")]
        AcclaimID,

        [HumanReadable(ShortName = "[T:ACT]", LongName = "<b>Activision ID</b>:")]
        ActivisionID,

        [HumanReadable(ShortName = "[T:ALT]", LongName = "<b>Alternative Title</b>:")]
        AlternativeTitle,

        [HumanReadable(ShortName = "[T:ALTF]", LongName = "<b>Alternative Foreign Title</b>:")]
        AlternativeForeignTitle,

        [HumanReadable(ShortName = "[T:BID]", LongName = "<b>Bandai ID</b>:")]
        BandaiID,

        [HumanReadable(ShortName = "[T:BBFC]", LongName = "<b>BBFC Reg. No.</b>:")]
        BBFCRegistrationNumber,

        // TODO: This doesn't have a site tag yet
        [HumanReadable(ShortName = "<b>Disc Hologram ID</b>:", LongName = "<b>Disc Hologram ID</b>:")]
        DiscHologramID,

        // TODO: This doesn't have a site tag yet
        [HumanReadable(ShortName = "<b>DMI</b>:", LongName = "<b>DMI</b>:")]
        DMIHash,

        [HumanReadable(ShortName = "[T:DNAS]", LongName = "<b>DNAS Disc ID</b>:")]
        DNASDiscID,

        [HumanReadable(ShortName = "[T:EAID]", LongName = "<b>Electronic Arts ID</b>:")]
        ElectronicArtsID,

        [HumanReadable(ShortName = "[T:X]", LongName = "<b>Extras</b>:")]
        Extras,

        // TODO: This doesn't have a site tag yet
        [HumanReadable(ShortName = "<b>Filename</b>:", LongName = "<b>Filename</b>:")]
        Filename,

        [HumanReadable(ShortName = "[T:FIID]", LongName = "<b>Fox Interactive ID</b>:")]
        FoxInteractiveID,

        [HumanReadable(ShortName = "[T:GF]", LongName = "<b>Game Footage</b>:")]
        GameFootage,

        // TODO: This doesn't have a site tag yet
        [HumanReadable(ShortName = "<b>Games</b>:", LongName = "<b>Games</b>:")]
        Games,

        [HumanReadable(ShortName = "[T:G]", LongName = "<b>Genre</b>:")]
        Genre,

        [HumanReadable(ShortName = "[T:GTID]", LongName = "<b>GT Interactive ID</b>:")]
        GTInteractiveID,

        // TODO: This doesn't have a site tag yet
        [HumanReadable(ShortName = "<b>Internal Name</b>:", LongName = "<b>Internal Name</b>:")]
        InternalName,

        [HumanReadable(ShortName = "[T:ISN]", LongName = "<b>Internal Serial</b>:")]
        InternalSerialName,

        [HumanReadable(ShortName = "[T:ISBN]", LongName = "<b>ISBN</b>:")]
        ISBN,

        [HumanReadable(ShortName = "[T:ISSN]", LongName = "<b>ISSN</b>:")]
        ISSN,

        [HumanReadable(ShortName = "[T:JID]", LongName = "<b>JASRAC ID</b>:")]
        JASRACID,

        [HumanReadable(ShortName = "[T:KIRZ]", LongName = "<b>King Records ID</b>:")]
        KingRecordsID,

        [HumanReadable(ShortName = "[T:KOEI]", LongName = "<b>Koei ID</b>:")]
        KoeiID,

        [HumanReadable(ShortName = "[T:KID]", LongName = "<b>Konami ID</b>:")]
        KonamiID,

        [HumanReadable(ShortName = "[T:LAID]", LongName = "<b>Lucas Arts ID</b>:")]
        LucasArtsID,

        // TODO: This doesn't have a site tag yet
        [HumanReadable(ShortName = "<b>Microsoft ID</b>:", LongName = "<b>Microsoft ID</b>:")]
        MicrosoftID,

        // TODO: This doesn't have a site tag yet
        [HumanReadable(ShortName = "<b>Multisession</b>:", LongName = "<b>Multisession</b>:")]
        Multisession,

        [HumanReadable(ShortName = "[T:NGID]", LongName = "<b>Nagano ID</b>:")]
        NaganoID,

        [HumanReadable(ShortName = "[T:NID]", LongName = "<b>Namco ID</b>:")]
        NamcoID,

        [HumanReadable(ShortName = "[T:NYG]", LongName = "<b>Net Yaroze Games</b>:")]
        NetYarozeGames,

        [HumanReadable(ShortName = "[T:NPS]", LongName = "<b>Nippon Ichi Software ID</b>:")]
        NipponIchiSoftwareID,

        [HumanReadable(ShortName = "[T:OID]", LongName = "<b>Origin ID</b>:")]
        OriginID,

        [HumanReadable(ShortName = "[T:P]", LongName = "<b>Patches</b>:")]
        Patches,

        // TODO: This doesn't have a site tag yet
        [HumanReadable(ShortName = "<b>PFI</b>:", LongName = "<b>PFI</b>:")]
        PFIHash,

        [HumanReadable(ShortName = "[T:PD]", LongName = "<b>Playable Demos</b>:")]
        PlayableDemos,

        [HumanReadable(ShortName = "[T:PCID]", LongName = "<b>Pony Canyon ID</b>:")]
        PonyCanyonID,

        [HumanReadable(ShortName = "[T:PT2]", LongName = "<b>Postgap type</b>: Form 2")]
        PostgapType,

        [HumanReadable(ShortName = "[T:PPN]", LongName = "<b>PPN</b>:")]
        PPN,

        // TODO: This doesn't have a site tag yet
        [HumanReadable(ShortName = "<b>Ring non-zero data start</b>:", LongName = "<b>Ring non-zero data start</b>:")]
        RingNonZeroDataStart,

        [HumanReadable(ShortName = "[T:RD]", LongName = "<b>Rolling Demos</b>:")]
        RollingDemos,

        [HumanReadable(ShortName = "[T:SG]", LongName = "<b>Savegames</b>:")]
        Savegames,

        [HumanReadable(ShortName = "[T:SID]", LongName = "<b>Sega ID</b>:")]
        SegaID,

        [HumanReadable(ShortName = "[T:SNID]", LongName = "<b>Selen ID</b>:")]
        SelenID,

        [HumanReadable(ShortName = "[T:S]", LongName = "<b>Series</b>:")]
        Series,

        // TODO: This doesn't have a site tag yet
        [HumanReadable(ShortName = "<b>Sierra ID</b>:", LongName = "<b>Sierra ID</b>:")]
        SierraID,

        // TODO: This doesn't have a site tag yet
        [HumanReadable(ShortName = "<b>SS</b>:", LongName = "<b>SS</b>:")]
        SSHash,

        // TODO: This doesn't have a site tag yet
        [HumanReadable(ShortName = "<b>SS version</b>:", LongName = "<b>SS version</b>:")]
        SSVersion,

        [HumanReadable(ShortName = "[T:TID]", LongName = "<b>Taito ID</b>:")]
        TaitoID,

        [HumanReadable(ShortName = "[T:TD]", LongName = "<b>Tech Demos</b>:")]
        TechDemos,

        [HumanReadable(ShortName = "[T:UID]", LongName = "<b>Ubisoft ID</b>:")]
        UbisoftID,

        // TODO: This doesn't have a site tag yet
        [HumanReadable(ShortName = "<b>Universal Hash (SHA-1)</b>:", LongName = "<b>Universal Hash (SHA-1)</b>:")]
        UniversalHash,

        [HumanReadable(ShortName = "[T:VID]", LongName = "<b>Valve ID</b>:")]
        ValveID,

        [HumanReadable(ShortName = "[T:VFC]", LongName = "<b>VFC code</b>:")]
        VFCCode,

        [HumanReadable(ShortName = "[T:V]", LongName = "<b>Videos</b>:")]
        Videos,

        [HumanReadable(ShortName = "[T:VOL]", LongName = "<b>Volume Label</b>:")]
        VolumeLabel,

        [HumanReadable(ShortName = "[T:VCD]", LongName = "<b>V-CD</b>")]
        VCD,

        // TODO: This doesn't have a site tag yet
        [HumanReadable(ShortName = "<b>XeMID</b>:", LongName = "<b>XeMID</b>:")]
        XeMID,

        // TODO: This doesn't have a site tag yet
        [HumanReadable(ShortName = "<b>XMID</b>:", LongName = "<b>XMID</b>:")]
        XMID,
    }

    /// <summary>
    /// List of system categories
    /// </summary>
    public enum SystemCategory
    {
        NONE = 0,

        [HumanReadable(LongName = "Disc-Based Consoles")]
        DiscBasedConsole,

        [HumanReadable(LongName = "Other Consoles")]
        OtherConsole,

        [HumanReadable(LongName = "Computers")]
        Computer,

        [HumanReadable(LongName = "Arcade")]
        Arcade,

        [HumanReadable(LongName = "Other")]
        Other,
    };

    /// <summary>
    /// Generic yes/no values
    /// </summary>
    public enum YesNo
    {
        [HumanReadable(LongName = "Yes/No")]
        NULL = 0,

        [HumanReadable(LongName = "No")]
        No = 1,

        [HumanReadable(LongName = "Yes")]
        Yes = 2,
    }
}
