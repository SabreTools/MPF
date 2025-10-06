using System;
using SabreTools.CommandLine;

namespace MPF.Frontend.Features
{
    public class ListSystemsFeature : Feature
    {
        #region Feature Definition

        public const string DisplayName = "listsystems";

        private static readonly string[] _flags = ["ls", "listsystems"];

        private const string _description = "List supported system types";

        #endregion

        public ListSystemsFeature()
            : base(DisplayName, _flags, _description)
        {
        }

        /// <inheritdoc/>
        public override bool Execute()
        {
            Console.WriteLine("Supported Systems:");
            foreach (string system in SabreTools.RedumpLib.Data.Extensions.ListSystems())
            {
                Console.WriteLine(system);
            }

            return true;
        }

        /// <inheritdoc/>
        public override bool VerifyInputs() => true;
    }
}
