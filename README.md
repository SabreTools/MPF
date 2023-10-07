# Media Preservation Frontend (MPF)

Redumper/Aaru/DiscImageCreator UI in C#

[![Build status](https://ci.appveyor.com/api/projects/status/3ldav3v0c373jeqa?svg=true)](https://ci.appveyor.com/project/mnadareski/MPF/build/artifacts)

This is a community project, so if you have some time and knowledge to give, we'll be glad to add you as a contributor to this project. If you have any suggestions, issues, bugs, or crashes, please look at the [Issues](https://github.com/SabreTools/MPF/issues) page first to see if it has been reported before and try out the latest AppVeyor WIP build below to see if it has already been addressed. If it hasn't, please open an issue that's as descriptive as you can be. Help me make this a better program for everyone :)

## Releases

For those who would rather use the most recent stable build, download the latest release here:
[Releases Page](https://github.com/SabreTools/MPF/releases)

For those who like to test the newest features, download the latest AppVeyor WIP build here: [AppVeyor](https://ci.appveyor.com/project/mnadareski/MPF/build/artifacts)

## Media Preservation Frontend (MPF)

MPF is the main, UI-centric application of the MPF suite. This program allows users to use Redumper, Aaru, or DiscImageCreator in a more user-friendly way. Each backend dumping program is supported as fully as possible to ensure that all information is captured on output. There are many customization options and quality of life settings that can be access through the Options menu.

### System Requirements

- Windows 8.1 (x64) or newer
    - Users who wish to use MPF on Windows 7 need to disable strong name validation due to `Microsoft.Management.Infrastructure` being unsigned. Add the following registry keys (accurate at time of writing):
    ```
        [HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\StrongName\Verification\*,31bf3856ad364e35]
        [HKEY_LOCAL_MACHINE\SOFTWARE\Wow6432Node\Microsoft\StrongName\Verification\*,31bf3856ad364e35]
    ```
    - Alternatively, look at this [StackOverflow question](https://stackoverflow.com/questions/403731/strong-name-validation-failed) for more information.

- .NET Framework 4.8, .NET 6.0, or .NET 7.0 Runtimes
- As much hard drive space as the amount of discs you will be dumping (20+ GB recommended)

Ensure that your operating system is as up-to-date as possible, since some features may rely on those updates.

### Support Limitations

The main UI has some known limitations that are documented in code and in some prior support tickets:

- Windows-only due to reliance on WPF and Winforms
    - MAUI is not a viable alternative due to lack of out-of-box support for Linux
    - Avalonia is being heavily considered as an alternative

### Build Instructions

To build for .NET Framework 4.8, .NET 6.0, or .NET 7.0 (all Windows only), ensure that the .NET 7.0 SDK (or later) is installed and included in your PATH. Then, run the following commands from command prompt, Powershell, or Terminal:

```
dotnet build MPF\MPF.csproj --framework [net48|net6.0-windows|net7.0-windows] --runtime win-x64 --self-contained
```

## Media Preservation Frontend Checker (MPF.Check)

MPF.Check is a commandline-only program that allows users to generate submission information from their personal rips. This program supports the outputs from Redumper, Aaru, DiscImageCreator, Cleanrip, and UmdImageCreator. Running this program without any parameters will display the help text, including all supported parameters.

### System Requirements

- Windows 8.1 (x64) or newer, GNU/Linux x64, or OSX x64
- .NET Framework 4.8 (Windows or `mono` only), .NET 6.0, or .NET 7.0 Runtimes

### Build Instructions

To build for .NET Framework 4.8 (Windows only), .NET 6.0, and .NET 7.0 (both all supported OSes), ensure that the .NET 7.0 SDK (or later) is installed and included in your PATH. Then, run the following commands from command prompt, Powershell, Terminal, or shell:

```
dotnet build MPF.Check\MPF.Check.csproj --framework [net48|net6.0|net7.0] --runtime [win-x64|linux-x64|osx-x64] --self-contained
```

Choose one of `[win-x64|linux-x64|osx-x64]` depending on the machine you are targeting.

## Information

For all additional information, including information about the individual components included in the project and what dumping programs are supported, please see [the wiki](https://github.com/SabreTools/MPF/wiki) for more details.

## Changelist

A list of all changes in each stable release and current WIP builds can now be found [here](https://github.com/SabreTools/MPF/blob/master/CHANGELIST.md).

## External Libraries

MPF uses some external libraries to assist with additional information gathering after the dumping process.

- **BurnOutSharp** - Protection scanning - [GitHub](https://github.com/mnadareski/BurnOutSharp)
- **UnshieldSharp** - Protection scanning - [GitHub](https://github.com/mnadareski/UnshieldSharp)
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
