using System;
using BinaryObjectScanner;

namespace MPF.Frontend
{
    public static class ConsoleLogger
    {
        /// <summary>
        /// Simple process counter to write to console
        /// </summary>
        public static void ProgressUpdated(object? sender, ResultEventArgs value)
        {
            string prefix = (bool?)value switch
            {
                true => "SUCCESS: ",
                false => "FAILURE: ",
                _ => "",
            };

            Console.WriteLine($"{prefix}{value.Message}");
        }

        /// <summary>
        /// Simple process counter to write to console
        /// </summary>
        public static void ProgressUpdated(object? sender, ProtectionProgress value)
        {
            string prefix = string.Empty;
            for (int i = 0; i < value.Depth; i++)
            {
                prefix += "--> ";
            }

            Console.WriteLine($"{prefix}{value.Percentage * 100:N2}%: {value.Filename} - {value.Protection}");
        }
    }
}
