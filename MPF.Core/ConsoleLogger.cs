using System;
using BinaryObjectScanner;
using MPF.Core.Data;

namespace MPF.Core
{
    public static class ConsoleLogger
    {
        /// <summary>
        /// Simple process counter to write to console
        /// </summary>
#if NET48
        public static void ProgressUpdated(object sender, Result value)
#else
        public static void ProgressUpdated(object? sender, Result value)
#endif
        {
            Console.WriteLine(value.Message);
        }

        /// <summary>
        /// Simple process counter to write to console
        /// </summary>
#if NET48
        public static void ProgressUpdated(object sender, ProtectionProgress value)
#else
        public static void ProgressUpdated(object? sender, ProtectionProgress value)
#endif
        {
            Console.WriteLine($"{value.Percentage * 100:N2}%: {value.Filename} - {value.Protection}");
        }
    }
}
