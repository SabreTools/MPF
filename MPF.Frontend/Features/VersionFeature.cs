using System;
using MPF.Frontend.Tools;
using SabreTools.CommandLine;

namespace MPF.Frontend.Features
{
    public class VersionFeature : Feature
    {
        #region Feature Definition

        public const string DisplayName = "version";

        private static readonly string[] _flags = ["version"];

        private const string _description = "Display the program version";

        #endregion

        public VersionFeature()
            : base(DisplayName, _flags, _description)
        {
        }

        /// <inheritdoc/>
        public override bool Execute()
        {
            Console.WriteLine(FrontendTool.GetCurrentVersion() ?? "Unknown version");
            return true;
        }

        /// <inheritdoc/>
        public override bool VerifyInputs() => true;
    }
}
