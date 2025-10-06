using System;
using System.Collections.Generic;
using SabreTools.CommandLine;

namespace MPF.Frontend.Features
{
    public class ListProgramsFeature : Feature
    {
        #region Feature Definition

        public const string DisplayName = "listprograms";

        private static readonly string[] _flags = ["lp", "listprograms"];

        private const string _description = "List supported dumping program outputs";

        #endregion

        public ListProgramsFeature()
            : base(DisplayName, _flags, _description)
        {
        }

        /// <inheritdoc/>
        public override bool Execute()
        {
            Console.WriteLine("Supported Programs:");
            foreach (string program in ListPrograms())
            {
                Console.WriteLine(program);
            }

            return true;
        }

        /// <inheritdoc/>
        public override bool VerifyInputs() => true;

        /// <summary>
        /// List all programs with their short usable names
        /// </summary>
        private static List<string> ListPrograms()
        {
            var programs = new List<string>();

            foreach (var val in Enum.GetValues(typeof(InternalProgram)))
            {
                if (((InternalProgram)val!) == InternalProgram.NONE)
                    continue;

                programs.Add($"{((InternalProgram?)val).ShortName()} - {((InternalProgram?)val).LongName()}");
            }

            return programs;
        }
    }
}
