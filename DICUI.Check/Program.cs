using System;
using System.IO;
using DICUI.Data;
using DICUI.Utilities;

namespace DICUI.Check
{
    public class Program
    {
        public static void Main(string[] args)
        {
            if (args.Length < 2)
            {
                DisplayHelp("Invalid number of arguments");
                return;
            }

            // Check for a valid mapped type and/or system
            bool valid = MediaTypeFromString(args[0], out MediaType mediaType, out KnownSystem knownSystem);
            if (!valid)
            {
                DisplayHelp($"{args[0]} is not a recognized media type");
                return;
            }

            // Make a new Progress object
            var progress = new Progress<Result>();
            progress.ProgressChanged += ProgressUpdated;

            // Loop through all the rest of the args
            for (int i = 1; i < args.Length; i++)
            {
                // Check for a file
                if (!File.Exists(args[i]))
                {
                    DisplayHelp($"{args[i]} does not exist");
                    return;
                }

                // Now populate an environment
                var env = new DumpEnvironment
                {
                    OutputDirectory = "",
                    OutputFilename = args[i],
                    System = knownSystem,
                    Type = mediaType,
                    ScanForProtection = false,
                };
                env.FixOutputPaths();

                // Finally, attempt to do the output dance
                var result = env.VerifyAndSaveDumpOutput(progress);
                Console.WriteLine(result.Message);
            }
        }

        private static void DisplayHelp(string error = null)
        {
            if (error != null)
                Console.WriteLine(error);

            Console.WriteLine("Usage:");
            Console.WriteLine("DICUI.Check.exe <mediatype> </path/to/output.bin> ...");
            Console.WriteLine();
            Console.WriteLine(@"Media Types:\r\n
cd / cdrom      - CD-ROM
dvd             - DVD-ROM
gd / gdrom      - GD-ROM
hddvd           - HD-DVD
bd / bluray     - BD-ROM
fd / floppy     - Floppy Disk
umd             - UMD
xbox            - XBOX DVD");
        }

        private static bool MediaTypeFromString(string val, out MediaType type, out KnownSystem system)
        {
            switch (val.ToLowerInvariant())
            {
                case "cd":
                case "cdrom":
                    type = MediaType.CDROM;
                    system = KnownSystem.IBMPCCompatible;
                    return true;
                case "dvd":
                    type = MediaType.DVD;
                    system = KnownSystem.IBMPCCompatible;
                    return true;
                case "gd":
                case "gdrom":
                    type = MediaType.GDROM;
                    system = KnownSystem.SegaDreamcast;
                    return true;
                case "hddvd":
                    type = MediaType.HDDVD;
                    system = KnownSystem.HDDVDVideo;
                    return true;
                case "bd":
                case "bluray":
                    type = MediaType.BluRay;
                    system = KnownSystem.BDVideo;
                    return true;
                case "fd":
                case "floppy":
                    type = MediaType.FloppyDisk;
                    system = KnownSystem.IBMPCCompatible;
                    return true;
                case "umd":
                    type = MediaType.UMD;
                    system = KnownSystem.SonyPlayStationPortable;
                    return true;
                case "xbox":
                    type = MediaType.DVD;
                    system = KnownSystem.MicrosoftXBOX;
                    return true;
                default:
                    type = MediaType.NONE;
                    system = KnownSystem.NONE;
                    return false;
            }
        }

        private static void ProgressUpdated(object sender, Result value)
        {
            Console.WriteLine(value.Message);
        }
    }
}
