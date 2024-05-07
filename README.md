# Media Preservation Frontend (MPF)

Redumper/Aaru/DiscImageCreator UI in C#

[![Build status](https://ci.appveyor.com/api/projects/status/3ldav3v0c373jeqa?svg=true)](https://ci.appveyor.com/project/mnadareski/MPF)
[![UI Build](https://github.com/SabreTools/MPF/actions/workflows/build_ui.yml/badge.svg)](https://github.com/SabreTools/MPF/actions/workflows/build_ui.yml)
[![Check Build](https://github.com/SabreTools/MPF/actions/workflows/build_check.yml/badge.svg)](https://github.com/SabreTools/MPF/actions/workflows/build_check.yml)

This is a community project, so if you have some time and knowledge to give, we'll be glad to add you as a contributor to this project. If you have any suggestions, issues, bugs, or crashes, please look at the [Issues](https://github.com/SabreTools/MPF/issues) page first to see if it has been reported before and try out the latest AppVeyor WIP build below to see if it has already been addressed. If it hasn't, please open an issue that's as descriptive as you can be. Help me make this a better program for everyone :)

## Releases

For the most recent stable build, download the latest release here: [Releases Page](https://github.com/SabreTools/MPF/releases)

For the latest WIP build here: [Rolling Release](https://github.com/SabreTools/MPF/releases/tag/rolling)

## Media Preservation Frontend UI (MPF)

MPF is the main, UI-centric application of the MPF suite. This program allows users to use Redumper, Aaru, or DiscImageCreator in a more user-friendly way. Each backend dumping program is supported as fully as possible to ensure that all information is captured on output. There are many customization options and quality of life settings that can be access through the Options menu.

### Support Limitations

The main UI has some known limitations that are documented in code and in some prior support tickets:

- Windows-only due to reliance on WPF and Winforms
  - MAUI is not a viable alternative due to lack of out-of-box support for Linux
  - Avalonia is being heavily considered as an alternative
- For those who need .NET Framework 4.8, there is an official fork: [MPF Legacy](https://github.com/Deterous/MPF-Legacy)
- For those who require broader archive/installer compatibility for protection scanning (Windows-only), please use the x86 builds as there are some specific scanning libraries that only work with that build
  - This is actively being worked on as part of [Binary Object Scanner](https://github.com/SabreTools/BinaryObjectScanner)
  - Please consider contributing if you have experience in dealing with multiple archive and installer types

## Media Preservation Frontend Checker (MPF.Check)

MPF.Check is a commandline-only program that allows users to generate submission information from their personal rips. This program supports the outputs from Redumper, Aaru, DiscImageCreator, Cleanrip, and UmdImageCreator. Running this program without any parameters will display the help text, including all supported parameters.

## System Requirements

Both MPF UI and MPF.Check have the same system requirements for running, with the exception that MPF UI is Windows-only.

- [Supported OS versions for .NET 8](https://github.com/dotnet/core/blob/main/release-notes/8.0/supported-os.md)
  - Requires [.NET 8.0 Runtime](https://dotnet.microsoft.com/en-us/download/dotnet/8.0) if built without bundled runtime

Ensure that your operating system and runtimes are as up-to-date as possible, since some features may rely on those updates.

## Build Instructions

To build for .NET 8.0, ensure that the [.NET 8.0 SDK](https://dotnet.microsoft.com/en-us/download/dotnet/8.0) (or later) is installed and included in your `PATH`. Then, run the following commands from command prompt, Powershell, Terminal, or shell:

**MPF UI (Windows only):**

```bash
dotnet build MPF/MPF.csproj --framework net8.0-windows --runtime [win-x86|win-x64]
```

**MPF.Check (Windows, OSX, Linux):**

```bash
dotnet build MPF.Check/MPF.Check.csproj --framework net8.0 --runtime [win-x86|win-x64|linux-x64|osx-x64]
```

Choose one of `win-x86`, `win-x64`, `linux-x64`, or `osx-x64` depending on the machine you are targeting.

### Build Scripts

Windows users may run `publish-win.ps1` and Linux users may run `publish-nix.sh` to perform a full release build. Both scripts will build and package all variants of MPF UI and MPF.Check with commandline switches to control what is included.

- `publish-win.ps1` requires [7-zip commandline](https://www.7-zip.org/download.html) and [Git for Windows](https://git-scm.com/downloads) to be installed and in `PATH`
- `publish-nix.sh` requires `zip` and `git` to be installed and in `PATH`

## Information

For all additional information, including information about the individual components included in the project and what dumping programs are supported, please see [the wiki](https://github.com/SabreTools/MPF/wiki) for more details.

## Changelist

A list of all changes in each stable release and current WIP builds can now be found [here](https://github.com/SabreTools/MPF/blob/master/CHANGELIST.md).

## External Libraries

MPF uses some external libraries to assist with additional information gathering after the dumping process.

- **Binary Object Scanner** - Protection scanning - [GitHub](https://github.com/SabreTools/BinaryObjectScanner)
- **WPFCustomMessageBox.thabse** - Custom message boxes in UI - [GitHub](https://github.com/thabse/WPFCustomMessageBox)

## Contributors

Here are the talented people who have contributed to the project so far in ways that GitHub doesn't like to track:

- **ReignStumble** - Former Project Lead / UI Design
- **Shadów** - UI Support

For all others who have contributed in some way, please see [here](https://github.com/SabreTools/MPF/graphs/contributors).

## Notable Testers

These are the tireless individuals who have dedicated countless hours to help test the many features of MPF and have worked with the development team closely:

- [**ajshell1**](https://github.com/ajshell1)
- [**Billy**](https://github.com/InternalLoss)
- [**David 'Foxhack' Silva**](https://github.com/FoxhackDN)
- [**ehw**](https://github.com/ehw)
- [**fuzzball**](https://github.com/fuzz6001)
- [**Gameboi64**](https://github.com/gameboi64)
- [**Intothisworld**](https://github.com/Intothisworld)
- [**John Veness**](https://github.com/JohnVeness)
- **Kludge**
- [**Matt Sephton**](https://github.com/gingerbeardman)
- [**NightsoN Blaze**](https://github.com/nightson)
- [**NovaSAurora**](https://github.com/NovaSAurora)
- [**Seventy7**](https://github.com/7Seventy7) - Additonal thanks for the original concept
- [**Silent**](https://github.com/CookiePLMonster)
- [**Terry Janas**](https://github.com/tjanas)
- [**TheRogueArchivist**](https://github.com/TheRogueArchivist)
- [**Whovian9369**](https://github.com/Whovian9369)

## Community Shout-Outs

Thanks to these communities for their use, testing, and feedback. I can't even hope to be able to thank everyone individually.

- [**VGPC Discord**](https://discord.gg/AHTfxQV) - Fast feedback and a lot of testing
- [**Redump Community**](http://redump.org/) - Near-daily use to assist with metadata gathering
