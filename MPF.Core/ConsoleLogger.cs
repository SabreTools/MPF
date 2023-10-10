using System;
using BurnOutSharp;
using MPF.Core.Data;

namespace MPF.Core
{
    public static class ConsoleLogger
    {
        /// <summary>
        /// Simple process counter to write to console
        /// </summary>
        public static void ProgressUpdated(object sender, Result value)
        {
            Console.WriteLine(value.Message);
        }

        /// <summary>
        /// Simple process counter to write to console
        /// </summary>
        public static void ProgressUpdated(object sender, ProtectionProgress value)
        {
            Console.WriteLine($"{value.Percentage * 100:N2}%: {value.Filename} - {value.Protection}");
        }
    }
}
