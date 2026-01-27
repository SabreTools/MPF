namespace MPF.ExecutionContexts.Dreamdump
{
    /// <summary>
    /// Dumping flags for Dreamdump
    /// </summary>
    public static class FlagStrings
    {
        #region Special

        public const string ForceQTOC = "--force-qtoc";
        public const string Train = "--train";
        public const string Retries = "--retries";

        #endregion

        #region Paths

        public const string ImageName = "--image-name";
        public const string ImagePath = "--image-path";

        #endregion

        #region Drive Part

        public const string ReadOffset = "--read-offset";
        public const string ReadAtOnce = "--read-at-once"; // [0,40] (Linux), [0,20] (Windows)
        public const string Speed = "--speed";
        public const string SectorOrder = "--sector-order";
        public const string Drive = "--drive";

        #endregion
    }
}
