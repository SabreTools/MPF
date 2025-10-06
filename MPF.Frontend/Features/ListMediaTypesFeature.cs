using System;
using SabreTools.CommandLine;

namespace MPF.Frontend.Features
{
    public class ListMediaTypesFeature : Feature
    {
        #region Feature Definition

        public const string DisplayName = "listmedia";

        private static readonly string[] _flags = ["lm", "listmedia"];

        private const string _description = "List supported media types";

        #endregion

        public ListMediaTypesFeature()
            : base(DisplayName, _flags, _description)
        {
        }

        /// <inheritdoc/>
        public override bool Execute()
        {
            Console.WriteLine("Supported Media Types:");
            foreach (string mediaType in SabreTools.RedumpLib.Data.Extensions.ListMediaTypes())
            {
                Console.WriteLine(mediaType);
            }

            return true;
        }

        /// <inheritdoc/>
        public override bool VerifyInputs() => true;
    }
}
