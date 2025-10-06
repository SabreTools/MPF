
using System;
using System.IO;
using MPF.Frontend;
using MPF.Frontend.Tools;
using SabreTools.RedumpLib.Data;

namespace MPF.CLI.Features
{
    internal sealed class InteractiveFeature : BaseFeature
    {
        #region Feature Definition

        public const string DisplayName = "interactive";

        private static readonly string[] _flags = ["i", "interactive"];

        private const string _description = "Enable interactive mode";

        #endregion

        public InteractiveFeature()
            : base(DisplayName, _flags, _description)
        {
            CommandOptions = new Program.CommandOptions();
            Options = OptionsLoader.LoadFromConfig();
        }

        /// <inheritdoc/>
        public override bool ProcessArgs(string[] args, int index)
        {
            // Cache all args as inputs
            for (int i = 1; i < args.Length; i++)
            {
                Inputs.Add(args[i]);
            }

            // Create return values
            CommandOptions = new Program.CommandOptions
            {
                MediaType = MediaType.NONE,
                FilePath = Path.Combine(Options.DefaultOutputPath ?? "ISO", "track.bin"),
            };
            System = Options.DefaultSystem;

            // Create state values
            string? result = string.Empty;

        root:
            Console.Clear();
            Console.WriteLine("MPF.CLI Interactive Mode - Main Menu");
            Console.WriteLine("-------------------------");
            Console.WriteLine();
            Console.WriteLine($"1) Set system (Currently '{System}')");
            Console.WriteLine($"2) Set dumping program (Currently '{Options.InternalProgram}')");
            Console.WriteLine($"3) Set media type (Currently '{CommandOptions.MediaType}')");
            Console.WriteLine($"4) Set device path (Currently '{CommandOptions.DevicePath}')");
            Console.WriteLine($"5) Set mounted path (Currently '{CommandOptions.MountedPath}')");
            Console.WriteLine($"6) Set file path (Currently '{CommandOptions.FilePath}')");
            Console.WriteLine($"7) Set override speed (Currently '{CommandOptions.DriveSpeed}')");
            Console.WriteLine($"8) Set custom parameters (Currently '{CommandOptions.CustomParams}')");
            Console.WriteLine();
            Console.WriteLine($"Q) Exit the program");
            Console.WriteLine($"X) Start dumping");
            Console.Write("> ");

            result = Console.ReadLine();
            switch (result)
            {
                case "1":
                    goto system;
                case "2":
                    goto dumpingProgram;
                case "3":
                    goto mediaType;
                case "4":
                    goto devicePath;
                case "5":
                    goto mountedPath;
                case "6":
                    goto filePath;
                case "7":
                    goto overrideSpeed;
                case "8":
                    goto customParams;

                case "q":
                case "Q":
                    Environment.Exit(0);
                    break;
                case "x":
                case "X":
                    Console.Clear();
                    goto exit;
                case "z":
                case "Z":
                    Console.WriteLine("It is pitch black. You are likely to be eaten by a grue.");
                    Console.Write("> ");
                    Console.ReadLine();
                    goto root;
                default:
                    Console.WriteLine($"Invalid selection: {result}");
                    Console.ReadLine();
                    goto root;
            }

        system:
            Console.WriteLine();
            Console.WriteLine("Input the system and press Enter:");
            Console.Write("> ");
            result = Console.ReadLine();
            System = Extensions.ToRedumpSystem(result);
            goto root;

        dumpingProgram:
            Console.WriteLine();
            Console.WriteLine("Input the dumping program and press Enter:");
            Console.Write("> ");
            result = Console.ReadLine();
            Options.InternalProgram = result.ToInternalProgram();
            goto root;

        mediaType:
            Console.WriteLine();
            Console.WriteLine("Input the media type and press Enter:");
            Console.Write("> ");
            result = Console.ReadLine();
            CommandOptions.MediaType = OptionsLoader.ToMediaType(result);
            goto root;

        devicePath:
            Console.WriteLine();
            Console.WriteLine("Input the device path and press Enter:");
            Console.Write("> ");
            CommandOptions.DevicePath = Console.ReadLine();
            goto root;

        mountedPath:
            Console.WriteLine();
            Console.WriteLine("Input the mounted path and press Enter:");
            Console.Write("> ");
            CommandOptions.MountedPath = Console.ReadLine();
            goto root;

        filePath:
            Console.WriteLine();
            Console.WriteLine("Input the file path and press Enter:");
            Console.Write("> ");

            result = Console.ReadLine();
            if (!string.IsNullOrEmpty(result))
                result = Path.GetFullPath(result!);

            CommandOptions.FilePath = result;
            goto root;

        overrideSpeed:
            Console.WriteLine();
            Console.WriteLine("Input the override speed and press Enter:");
            Console.Write("> ");

            result = Console.ReadLine();
            if (!int.TryParse(result, out int speed))
                speed = -1;

            CommandOptions.DriveSpeed = speed;
            goto root;

        customParams:
            Console.WriteLine();
            Console.WriteLine("Input the custom parameters and press Enter:");
            Console.Write("> ");
            CommandOptions.CustomParams = Console.ReadLine();
            goto root;

        exit:
            return true;
        }

        /// <inheritdoc/>
        public override bool VerifyInputs() => true;
    }
}
