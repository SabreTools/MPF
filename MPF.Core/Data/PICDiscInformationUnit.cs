namespace MPF.Core.Data
{
    /// <see href="https://www.t10.org/ftp/t10/document.05/05-206r0.pdf"/>
    /// <see href="https://github.com/aaru-dps/Aaru.Decoders/blob/devel/Bluray/DI.cs"/>
    public class PICDiscInformationUnit
    {
        #region Fields

        #region Header

        /// <summary>
        /// Disc Information Identifier "DI"
        /// Emergency Brake Identifier "EB"
        /// </summary>
        public string DiscInformationIdentifier { get; set; }

        /// <summary>
        /// Disc Information Format
        /// </summary>
        public byte DiscInformationFormat { get; set; }

        /// <summary>
        /// Number of DI units in each DI block
        /// </summary>
        public byte NumberOfUnitsInBlock { get; set; }

        /// <summary>
        /// Should be 0x00
        /// </summary>
        public byte Reserved0 { get; set; }

        /// <summary>
        /// DI unit Sequence Number
        /// </summary>
        public byte SequenceNumber { get; set; }

        /// <summary>
        /// Number of bytes in use in this DI unit
        /// </summary>
        public byte BytesInUse { get; set; }

        /// <summary>
        /// Should be 0x00
        /// </summary>
        public byte Reserved1 { get; set; }

        #endregion

        // TODO: Write models for the dependent contents, if possible
        #region Body

        /// <summary>
        /// Disc Type Identifier
        /// = "BDO" for BD-ROM
        /// = "BDU" for BD-ROM Ultra
        /// = "BDW" for BD-RE
        /// = "BDR" for BD-R
        /// </summary>
        public string DiscTypeIdentifier { get; set; }

        /// <summary>
        /// Disc Size/Class/Version
        /// </summary>
        public byte DiscSizeClassVersion { get; set; }

        /// <summary>
        /// DI Unit Format dependent contents
        /// </summary>
        /// <remarks>52 bytes for BD-ROM, 100 bytes for BD-R/RE</remarks>
        public byte[] FormatDependentContents { get; set; }

        #endregion

        #region Trailer (BD-R/RE only)

        /// <summary>
        /// Disc Manufacturer ID
        /// </summary>
        /// <remarks>6 bytes</remarks>
        public byte[] DiscManufacturerID { get; set; }

        /// <summary>
        /// Media Type ID
        /// </summary>
        /// <remarks>3 bytes</remarks>
        public byte[] MediaTypeID { get; set; }

        /// <summary>
        /// Time Stamp
        /// </summary>
        public ushort TimeStamp { get; set; }

        /// <summary>
        /// Product Revision Number
        /// </summary>
        public byte ProductRevisionNumber { get; set; }

        #endregion

        #endregion

        #region Constants

        public const string DiscTypeIdentifierROM = "BDO";

        public const string DiscTypeIdentifierROMUltra = "BDU";

        public const string DiscTypeIdentifierReWritable = "BDW";

        public const string DiscTypeIdentifierRecordable = "BDR";

        #endregion
    }
}
