using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Runtime;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;

namespace DICUI.External.unshield
{
    public class Unshield
    {
        public static string ListFiles(string input)
        {
            Process p = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Programs", "unshield.exe"),
                    Arguments = "l " + input,
                    CreateNoWindow = true,
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                },
            };
            p.Start();
            p.WaitForExit();
            return p.StandardOutput.ReadToEnd();
        }

        public static void ExtractFile(string input, string filename, string output)
        {
            Process p = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Programs", "unshield.exe"),
                    Arguments = "-d " + output + " x " + input + " " + filename,
                    CreateNoWindow = true,
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                },
            };
            p.Start();
            p.WaitForExit();
        }
    }
}
