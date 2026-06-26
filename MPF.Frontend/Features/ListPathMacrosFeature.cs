using System;
using SabreTools.CommandLine;

namespace MPF.Frontend.Features
{
    public class ListPathMacrosFeature : Feature
    {
        #region Feature Definition

        public const string DisplayName = "listpathmacros";

        private static readonly string[] _flags = ["lpm", "listpath"];

        private const string _description = "List supported path macros for configuration and input";

        #endregion

        public ListPathMacrosFeature()
            : base(DisplayName, _flags, _description)
        {
        }

        /// <inheritdoc/>
        public override bool Execute()
        {
            Console.WriteLine("Supported Path Macros:");
            Console.WriteLine();
            Console.WriteLine("%SYSTEM%     System display name");
            Console.WriteLine("%SYS%        System code (upper-case)");
            Console.WriteLine("%sys%        System code (lower-case)");
            Console.WriteLine("%MEDIA%      Detected media type");
            Console.WriteLine("%PROGRAM%    Dumping program");
            Console.WriteLine("%PROG%       Dumping program short name");
            Console.WriteLine("%LABEL%      Detected volume label");
            Console.WriteLine("%DATE%       Date in 'yyyyMMdd' format");
            Console.WriteLine("%DATETIME%   Date and time in 'yyyyMMdd-HHmmss' format");
            Console.WriteLine();
            Console.WriteLine("Please note that all path macros can be used in both the configuration");
            Console.WriteLine("file and any user-input output paths.");

            return true;
        }

        /// <inheritdoc/>
        public override bool VerifyInputs() => true;
    }
}
