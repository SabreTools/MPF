using System;
using SabreTools.CommandLine;

namespace MPF.Frontend.Features
{
    public class ListCodesFeature : Feature
    {
        #region Feature Definition

        public const string DisplayName = "listcodes";

        private static readonly string[] _flags = ["lc", "listcodes"];

        private const string _description = "List supported comment/content site codes";

        #endregion

        public ListCodesFeature()
            : base(DisplayName, _flags, _description)
        {
        }

        /// <inheritdoc/>
        public override bool Execute()
        {
            Console.WriteLine("Supported Site Codes:");
            foreach (string siteCode in SabreTools.RedumpLib.Data.Extensions.ListSiteCodes())
            {
                Console.WriteLine(siteCode);
            }

            return true;
        }

        /// <inheritdoc/>
        public override bool VerifyInputs() => true;
    }
}
