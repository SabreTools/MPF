namespace MPF.Core.Data
{
    /// <summary>
    /// Disc Information and Emergency Brake data shall be read from the PIC zone. DI units that
    /// contain physical information shall be returned.Emergency Brake data shall be returned.The
    /// information shall be collected from the layer specified in the Layer field of the CDB. If any data
    /// can be returned, 4 100 bytes shall be returned.
    /// </summary>
    /// <see href="https://www.t10.org/ftp/t10/document.05/05-206r0.pdf"/>
    /// <see href="https://github.com/aaru-dps/Aaru.Decoders/blob/devel/Bluray/DI.cs"/>
    public class PICDiscInformation
    {
        #region Fields

        /// <summary>
        /// 2048 bytes for BD-ROM, 3584 bytes for BD-R/RE
        /// </summary>
        /// <remarks>Big-endian format</remarks>
        public ushort DataStructureLength { get; set; }

        /// <summary>
        /// Should be 0x00
        /// </summary>
        public byte Reserved0 { get; set; }

        /// <summary>
        /// Should be 0x00
        /// </summary>
        public byte Reserved1 { get; set; }

        /// <summary>
        /// Disc information and emergency brake units
        /// </summary>
        public PICDiscInformationUnit[] Units { get; set; }

        #endregion
    }
}
