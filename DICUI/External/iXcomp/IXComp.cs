using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace DICUI.External.iXcomp
{
    // TODO: Replace this with a C# implementation based on Unshield
    public class IXComp
    {
        /// <summary>
        /// List all files found within an InstallShield CAB file
        /// </summary>
        /// <param name="input">CAB file to check</param>
        /// <param name="version">Output tool version</param>
        /// <returns>List of files found in the CAB</returns>
        public static List<string> ListFiles(string input, out int version)
        {
            // Version 6
            version = 6;
            Process p = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Programs", "i6comp.exe"),
                    Arguments = "l -o -r -d \"" + input + "\"",
                    CreateNoWindow = true,
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                },
            };
            p.Start();
            p.WaitForExit(1000);
            var i6output = p.StandardOutput.ReadToEnd().Replace("\r\n", "\n").Split('\n').Where(s => s.Length > 50).Select(s => s.Substring(50)).ToList();

            if (i6output.Count() > 0)
                return i6output;

            // Version 5
            version = 5;
            p = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Programs", "i5comp.exe"),
                    Arguments = "l -o -r -d \"" + input + "\"",
                    CreateNoWindow = true,
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                },
            };
            p.Start();
            p.WaitForExit(1000);
            var i5output = p.StandardOutput.ReadToEnd().Replace("\r\n", "\n").Split('\n').Where(s => s.Length > 47).Select(s => s.Substring(47)).ToList();

            if (i5output.Count() > 0)
                return i6output;

            version = -1;
            return new List<string>();
        }

        /// <summary>
        /// Extract all files found within an InstallShield CAB file
        /// </summary>
        /// <param name="cabfile">CAB file to check</param>
        /// <param name="outDir">Output directory to extract to</param>
        /// <param name="version">Tool version</param>
        /// <returns>True if the files extracted succesfully, false otherwise</returns>
        public static bool ExtractAll(string cabfile, string outDir, int version)
        {
            string exe = null;
            switch(version)
            {
                case 6:
                    exe = "i6comp.exe";
                    break;
                case 5:
                    exe = "i5comp.exe";
                    break;
            }

            if (exe == null)
                return false;

            Process p = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    WorkingDirectory = outDir,
                    FileName = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Programs", exe),
                    Arguments = "x -r -d \"" + cabfile + "\"",
                    CreateNoWindow = true,
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                },
            };
            p.Start();
            p.WaitForExit();
            return true;
        }
    }
}
