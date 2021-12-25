using System.Text.RegularExpressions;

namespace RedumpLib.Data
{
    public static class Constants
    {
        #region Comment Field Tags

        // TODO: Make this into a proper enum with LongName and ShortName

        public const string AcclaimIDCommentField = "[T:ACC]";
        public const string AcclaimIDCommentString = "<b>Acclaim ID</b>:";
        public const string ActivisionIDCommentField = "[T:ACT]";
        public const string ActivisionIDCommentString = "<b>Activision ID</b>:";
        public const string AlternativeTitleCommentField = "[T:ALT]";
        public const string AlternativeTitleCommentString = "<b>Alternative Title</b>:";
        public const string AlternativeForeignTitleCommentField = "[T:ALTF]";
        public const string AlternativeForeignTitleCommentString = "<b>Alternative Foreign Title</b>:";
        public const string BandaiIDCommentField = "[T:BID]";
        public const string BandaiIDCommentString = "<b>Bandai ID</b>:";
        public const string BBFCRegistrationNumberCommentField = "[T:BBFC]";
        public const string BBFCRegistrationNumberCommentString = "<b>BBFC Reg. No.</b>:";
        public const string DNASDiscIDCommentField = "[T:DNAS]";
        public const string DNASDiscIDCommentString = "<b>DNAS Disc ID</b>:";
        public const string ElectronicArtsIDCommentField = "[T:EAID]";
        public const string ElectronicArtsIDCommentString = "<b>Electronic Arts ID</b>:";
        public const string ExtrasCommentField = "[T:X]";
        public const string ExtrasCommentString = "<b>Extras</b>:";
        public const string FoxInteractiveIDCommentField = "[T:FIID]";
        public const string FoxInteractiveIDCommentString = "<b>Fox Interactive ID</b>:";
        public const string GameFootageCommentField = "[T:GF]";
        public const string GameFootageCommentString = "<b>Game Footage</b>:";
        public const string GenreCommentField = "[T:G]";
        public const string GenreCommentString = "<b>Genre</b>:";
        public const string GTInteractiveIDCommentField = "[T:GTID]";
        public const string GTInteractiveIDCommentString = "<b>GT Interactive ID</b>:";
        public const string InternalSerialNameCommentField = "[T:ISN]";
        public const string InternalSerialNameCommentString = "<b>Internal Serial</b>:";
        public const string ISBNCommentField = "[T:ISBN]";
        public const string ISBNCommentString = "<b>ISBN</b>:";
        public const string ISSNCommentField = "[T:ISSN]";
        public const string ISSNCommentString = "<b>ISSN</b>:";
        public const string JASRACIDCommentField = "[T:JID]";
        public const string JASRACIDCommentString = "<b>JASRAC ID</b>:";
        public const string KingRecordsIDCommentField = "[T:KIRZ]";
        public const string KingRecordsIDCommentString = "<b>King Records ID</b>:";
        public const string KoeiIDCommentField = "[T:KOEI]";
        public const string KoeiIDCommentString = "<b>Koei ID</b>:";
        public const string KonamiIDCommentField = "[T:KID]";
        public const string KonamiIDCommentString = "<b>Konami ID</b>:";
        public const string LucasArtsIDCommentField = "[T:LAID]";
        public const string LucasArtsIDCommentString = "<b>Lucas Arts ID</b>:";
        public const string NaganoIDCommentField = "[T:NGID]";
        public const string NaganoIDCommentString = "<b>Nagano ID</b>:";
        public const string NamcoIDCommentField = "[T:NID]";
        public const string NamcoIDCommentString = "<b>Namco ID</b>:";
        public const string NetYarozeGamesCommentField = "[T:NYG]";
        public const string NetYarozeGamesCommentSring = "Net Yaroze Games</b>:";
        public const string NipponIchiSoftwareIDCommentField = "[T:NPS]";
        public const string NipponIchiSoftwareIDCommentString = "<b>Nippon Ichi Software ID</b>:";
        public const string OriginIDCommentField = "[T:OID]";
        public const string OriginIDCommentString = "<b>Origin ID</b>:";
        public const string PatchesCommentField = "[T:P]";
        public const string PatchesCommentString = "<b>Patches</b>:";
        public const string PlayableDemosCommentField = "[T:PD]";
        public const string PlayableDemosCommentString = "<b>Playable Demos</b>:";
        public const string PonyCanyonIDCommentField = "[T:PCID]";
        public const string PonyCanyonIDCommentString = "<b>Pony Canyon ID</b>:";
        public const string PostgapTypeCommentField = "[T:PT2]";
        public const string PostgapTypeCommentString = "<b>Postgap type</b>: Form 2";
        public const string PPNCommentField = "[T:PPN]";
        public const string PPNCommentString = "<b>PPN</b>:";
        public const string RollingDemosCommentField = "[T:RD]";
        public const string RollingDemosCommentString = "<b>Rolling Demos</b>:";
        public const string SavegamesCommentField = "[T:SG]";
        public const string SavegamesCommentString = "<b>Savegames</b>:";
        public const string SegaIDCommentField = "[T:SID]";
        public const string SegaIDCommentString = "<b>Sega ID</b>:";
        public const string SelenIDCommentField = "[T:SNID]";
        public const string SelenIDCommentString = "<b>Selen ID</b>:";
        public const string SeriesCommentField = "[T:S]";
        public const string SeriesCommentString = "<b>Series</b>:";
        public const string TaitoIDCommentField = "[T:TID]";
        public const string TaitoIDCommentString = "<b>Taito ID</b>:";
        public const string TechDemosCommentField = "[T:TD]";
        public const string TechDemosCommentString = "<b>Tech Demos</b>:";
        public const string UbisoftIDCommentField = "[T:UID]";
        public const string UbisoftIDCommentString = "<b>Ubisoft ID</b>:";
        public const string ValveIDCommentField = "[T:VID]";
        public const string ValveIDCommentString = "<b>Valve ID</b>:";
        public const string VFCCodeCommentField = "[T:VFC]";
        public const string VGCCodeCommentString = "<b>VFC code</b>:";
        public const string VideosCommentField = "[T:V]";
        public const string VideosCommentString = "<b>Videos</b>:";
        public const string VolumeLabelCommentField = "[T:VOL]";
        public const string VolumeLabelCommentString = "<b>Volume Label</b>:";
        public const string VCDCommentField = "[T:VCD]";
        public const string VCDCommentString = "<b>V-CD</b>";

        #endregion

        // TODO: Add RegexOptions.Compiled
        #region Regular Expressions

        /// <summary>
        /// Regex matching the added field on a disc page
        /// </summary>
        public static Regex AddedRegex = new Regex(@"<tr><th>Added</th><td>(.*?)</td></tr>");

        /// <summary>
        /// Regex matching the barcode field on a disc page
        /// </summary>
        public static Regex BarcodeRegex = new Regex(@"<tr><th>Barcode</th></tr><tr><td>(.*?)</td></tr>");

        /// <summary>
        /// Regex matching the BCA field on a disc page
        /// </summary>
        public static Regex BcaRegex = new Regex(@"<h3>BCA .*?/></h3></td><td .*?></td></tr>"
            + "<tr><th>Row</th><th>Contents</th><th>ASCII</th></tr>"
            + "<tr><td>(?<row1number>.*?)</td><td>(?<row1contents>.*?)</td><td>(?<row1ascii>.*?)</td></tr>"
            + "<tr><td>(?<row2number>.*?)</td><td>(?<row2contents>.*?)</td><td>(?<row2ascii>.*?)</td></tr>"
            + "<tr><td>(?<row3number>.*?)</td><td>(?<row3contents>.*?)</td><td>(?<row3ascii>.*?)</td></tr>"
            + "<tr><td>(?<row4number>.*?)</td><td>(?<row4contents>.*?)</td><td>(?<row4ascii>.*?)</td></tr>", RegexOptions.Singleline);

        /// <summary>
        /// Regex matching the category field on a disc page
        /// </summary>
        public static Regex CategoryRegex = new Regex(@"<tr><th>Category</th><td>(.*?)</td></tr>");

        /// <summary>
        /// Regex matching the comments field on a disc page
        /// </summary>
        public static Regex CommentsRegex = new Regex(@"<tr><th>Comments</th></tr><tr><td>(.*?)</td></tr>", RegexOptions.Singleline);

        /// <summary>
        /// Regex matching the contents field on a disc page
        /// </summary>
        public static Regex ContentsRegex = new Regex(@"<tr><th>Contents</th></tr><tr .*?><td>(.*?)</td></tr>", RegexOptions.Singleline);

        /// <summary>
        /// Regex matching individual disc links on a results page
        /// </summary>
        public static Regex DiscRegex = new Regex(@"<a href=""/disc/(\d+)/"">");

        /// <summary>
        /// Regex matching the disc number or letter field on a disc page
        /// </summary>
        public static Regex DiscNumberLetterRegex = new Regex(@"\((.*?)\)");

        /// <summary>
        /// Regex matching the dumpers on a disc page
        /// </summary>
        public static Regex DumpersRegex = new Regex(@"<a href=""/discs/dumper/(.*?)/"">");

        /// <summary>
        /// Regex matching the edition field on a disc page
        /// </summary>
        public static Regex EditionRegex = new Regex(@"<tr><th>Edition</th><td>(.*?)</td></tr>");

        /// <summary>
        /// Regex matching the error count field on a disc page
        /// </summary>
        public static Regex ErrorCountRegex = new Regex(@"<tr><th>Errors count</th><td>(.*?)</td></tr>");

        /// <summary>
        /// Regex matching the foreign title field on a disc page
        /// </summary>
        public static Regex ForeignTitleRegex = new Regex(@"<h2>(.*?)</h2>");

        /// <summary>
        /// Regex matching the "full match" ID list from a WIP disc page
        /// </summary>
        public static Regex FullMatchRegex = new Regex(@"<td class=""static"">full match ids: (.*?)</td>");

        /// <summary>
        /// Regex matching the languages field on a disc page
        /// </summary>
        public static Regex LanguagesRegex = new Regex(@"<img src=""/images/languages/(.*?)\.png"" alt="".*?"" title="".*?"" />\s*");

        /// <summary>
        /// Regex matching the last modified field on a disc page
        /// </summary>
        public static Regex LastModifiedRegex = new Regex(@"<tr><th>Last modified</th><td>(.*?)</td></tr>");

        /// <summary>
        /// Regex matching the media field on a disc page
        /// </summary>
        public static Regex MediaRegex = new Regex(@"<tr><th>Media</th><td>(.*?)</td></tr>");

        /// <summary>
        /// Regex matching individual WIP disc links on a results page
        /// </summary>
        public static Regex NewDiscRegex = new Regex(@"<a (style=.*)?href=""/newdisc/(\d+)/"">");

        /// <summary>
        /// Regex matching the "partial match" ID list from a WIP disc page
        /// </summary>
        public static Regex PartialMatchRegex = new Regex(@"<td class=""static"">partial match ids: (.*?)</td>");

        /// <summary>
        /// Regex matching the PVD field on a disc page
        /// </summary>
        public static Regex PvdRegex = new Regex(@"<h3>Primary Volume Descriptor (PVD) <img .*?/></h3></td><td .*?></td></tr>"
            + @"<tr><th>Record / Entry</th><th>Contents</th><th>Date</th><th>Time</th><th>GMT</th></tr>"
            + @"<tr><td>Creation</td><td>(?<creationbytes>.*?)</td><td>(?<creationdate>.*?)</td><td>(?<creationtime>.*?)</td><td>(?<creationtimezone>.*?)</td></tr>"
            + @"<tr><td>Modification</td><td>(?<modificationbytes>.*?)</td><td>(?<modificationdate>.*?)</td><td>(?<modificationtime>.*?)</td><td>(?<modificationtimezone>.*?)</td></tr>"
            + @"<tr><td>Expiration</td><td>(?<expirationbytes>.*?)</td><td>(?<expirationdate>.*?)</td><td>(?<expirationtime>.*?)</td><td>(?<expirationtimezone>.*?)</td></tr>"
            + @"<tr><td>Effective</td><td>(?<effectivebytes>.*?)</td><td>(?<effectivedate>.*?)</td><td>(?<effectivetime>.*?)</td><td>(?<effectivetimezone>.*?)</td></tr>", RegexOptions.Singleline);

        /// <summary>
        /// Regex matching the region field on a disc page
        /// </summary>
        public static Regex RegionRegex = new Regex(@"<tr><th>Region</th><td><a href=""/discs/region/(.*?)/"">");

        /// <summary>
        /// Regex matching a double-layer disc ringcode information
        /// </summary>
        public static Regex RingCodeDoubleRegex = new Regex(@"", RegexOptions.Singleline); // Varies based on available fields, like Addtional Mould

        /// <summary>
        /// Regex matching a single-layer disc ringcode information
        /// </summary>
        public static Regex RingCodeSingleRegex = new Regex(@"", RegexOptions.Singleline); // Varies based on available fields, like Addtional Mould

        /// <summary>
        /// Regex matching the serial field on a disc page
        /// </summary>
        public static Regex SerialRegex = new Regex(@"<tr><th>Serial</th><td>(.*?)</td></tr>");

        /// <summary>
        /// Regex matching the system field on a disc page
        /// </summary>
        public static Regex SystemRegex = new Regex(@"<tr><th>System</th><td><a href=""/discs/system/(.*?)/"">");

        /// <summary>
        /// Regex matching the title field on a disc page
        /// </summary>
        public static Regex TitleRegex = new Regex(@"<h1>(.*?)</h1>");

        /// <summary>
        /// Regex matching the current nonce token for login
        /// </summary>
        public static Regex TokenRegex = new Regex(@"<input type=""hidden"" name=""csrf_token"" value=""(.*?)"" />");

        /// <summary>
        /// Regex matching a single track on a disc page
        /// </summary>
        public static Regex TrackRegex = new Regex(@"<tr><td>(?<number>.*?)</td><td>(?<type>.*?)</td><td>(?<pregap>.*?)</td><td>(?<length>.*?)</td><td>(?<sectors>.*?)</td><td>(?<size>.*?)</td><td>(?<crc32>.*?)</td><td>(?<md5>.*?)</td><td>(?<sha1>.*?)</td></tr>", RegexOptions.Singleline);

        /// <summary>
        /// Regex matching the track count on a disc page
        /// </summary>
        public static Regex TrackCountRegex = new Regex(@"<tr><th>Number of tracks</th><td>(.*?)</td></tr>");

        /// <summary>
        /// Regex matching the version field on a disc page
        /// </summary>
        public static Regex VersionRegex = new Regex(@"<tr><th>Version</th><td>(.*?)</td></tr>");

        /// <summary>
        /// Regex matching the write offset field on a disc page
        /// </summary>
        public static Regex WriteOffsetRegex = new Regex(@"<tr><th>Write offset</th><td>(.*?)</td></tr>");

        #endregion

        #region URLs

        /// <summary>
        /// Redump disc page URL template
        /// </summary>
        public const string DiscPageUrl = @"http://redump.org/disc/{0}/";

        /// <summary>
        /// Redump last modified search URL
        /// </summary>
        public const string LastModifiedUrl = @"http://redump.org/discs/sort/modified/dir/desc?page={0}";

        /// <summary>
        /// Redump login page URL
        /// </summary>
        public const string LoginUrl = "http://forum.redump.org/login/";

        /// <summary>
        /// Redump CUE pack URL template
        /// </summary>
        public const string PackCuesUrl = @"http://redump.org/cues/{0}/";

        /// <summary>
        /// Redump DAT pack URL template
        /// </summary>
        public const string PackDatfileUrl = @"http://redump.org/datfile/{0}/";

        /// <summary>
        /// Redump DKEYS pack URL template
        /// </summary>
        public const string PackDkeysUrl = @"http://redump.org/dkeys/{0}/";

        /// <summary>
        /// Redump GDI pack URL template
        /// </summary>
        public const string PackGdiUrl = @"http://redump.org/gdi/{0}/";

        /// <summary>
        /// Redump KEYS pack URL template
        /// </summary>
        public const string PackKeysUrl = @"http://redump.org/keys/{0}/";

        /// <summary>
        /// Redump LSD pack URL template
        /// </summary>
        public const string PackLsdUrl = @"http://redump.org/lsd/{0}/";

        /// <summary>
        /// Redump SBI pack URL template
        /// </summary>
        public const string PackSbiUrl = @"http://redump.org/sbi/{0}/";

        /// <summary>
        /// Redump quicksearch URL template
        /// </summary>
        public const string QuickSearchUrl = @"http://redump.org/discs/quicksearch/{0}/?page={1}";

        /// <summary>
        /// Redump user dumps URL template
        /// </summary>
        public const string UserDumpsUrl = @"http://redump.org/discs/dumper/{0}/?page={1}";

        /// <summary>
        /// Redump WIP disc page URL template
        /// </summary>
        public const string WipDiscPageUrl = @"http://redump.org/newdisc/{0}/";

        /// <summary>
        /// Redump WIP dumps queue URL
        /// </summary>
        public const string WipDumpsUrl = @"http://redump.org/discs-wip/";

        #endregion

        #region URL Extensions

        /// <summary>
        /// Changes page subpath
        /// </summary>
        public const string ChangesExt = "changes/";

        /// <summary>
        /// Cuesheet download subpath
        /// </summary>
        public const string CueExt = "cue/";

        /// <summary>
        /// Edit page subpath
        /// </summary>
        public const string EditExt = "edit/";

        /// <summary>
        /// GDI download subpath
        /// </summary>
        public const string GdiExt = "gdi/";

        /// <summary>
        /// Key download subpath
        /// </summary>
        public const string KeyExt = "key/";

        /// <summary>
        /// LSD download subpath
        /// </summary>
        public const string LsdExt = "lsd/";

        /// <summary>
        /// MD5 download subpath
        /// </summary>
        public const string Md5Ext = "md5/";

        /// <summary>
        /// SBI download subpath
        /// </summary>
        public const string SbiExt = "sbi/";

        /// <summary>
        /// SFV download subpath
        /// </summary>
        public const string SfvExt = "sfv/";

        /// <summary>
        /// SHA1 download subpath
        /// </summary>
        public const string Sha1Ext = "sha1/";

        #endregion

    }
}