# DICUI

DiscImageCreator/Aaru UI in C#

[![Build status](https://ci.appveyor.com/api/projects/status/3ldav3v0c373jeqa?svg=true)](https://ci.appveyor.com/project/mnadareski/dicui/build/artifacts)

This is a community project, so if you have some time and knowledge to give, we'll be glad to add you as a contributor to this project. If you have any suggestions, issues, bugs, or crashes, please look at the [Issues](https://github.com/SabreTools/DICUI/issues) page first to see if it has been reported before and try out the latest AppVeyor WIP build below to see if it has already been addressed. If it hasn't, please open an issue that's as descriptive as you can be. Help me make this a better program for everyone :)

## Releases

For those who would rather use the most recent stable build, download the latest release here:
[Releases Page](https://github.com/SabreTools/DICUI/releases)

For those who like to test the newest features, download the latest AppVeyor WIP build here: [AppVeyor](https://ci.appveyor.com/project/mnadareski/dicui/build/artifacts)

## System Requirements

Even though this is written in C#, this program can only be used on Windows systems due to one of the base programs, DiscImageCreator, being Windows-only. There is some preliminary support for Linux underway, and we will try to integrate with that when the time comes.

- Windows 7 (newest version of Windows recommended) or Mono-compatible Linux environment (DICUI.Check only)
- .NET Framework 4.6.2, .NET Framework 4.7.2, .NET Framework 4.8, or .NET Core 3.1 Runtimes (.NET Core 3.1 is only partially functional due to a dependency, use at your own risk)
- 1 GB of free RAM
- As much hard drive space as the amount of discs you will be dumping (20+ GB recommended)

Ensure that your operating system is as up-to-date as possible, since some features may rely on those updates.

## Information

For all additional information, including information about the individual components included in the project and what dumping programs are supported, please see [the wiki](https://github.com/SabreTools/DICUI/wiki) for more details.

## Changelist

A list of all changes in each stable release can now be found [here](https://github.com/SabreTools/DICUI/blob/master/CHANGELIST.md).

## External Libraries

DICUI uses some external libraries to assist with additional information gathering after the dumping process.

- **BurnOutSharp** - Protection scanning - [GitHub](https://github.com/mnadareski/BurnOutSharp)
- **UnshieldSharp** - Protection scanning - [GitHub](https://github.com/mnadareski/UnshieldSharp)

## Contributors

Here are the talented people who have contributed to the project so far:

- **darksabre76** - Project Lead / Backend Design
- **ReignStumble** - Former Project Lead / UI Design
- **Jakz** - Primary Feature Contributor
- **NHellFire** - Feature Contributor

## Notable Testers

These are the tireless individuals who have dedicated countless hours to help test the many features of DICUI and have worked with the development team closely:

- **Dizzzy/user7** - Concept, ideas, tester
- **Kludge** - Primary stress tester
- **ajshell1** - Tester
- **eientei95** - Tester
- **VGPC Discord** - Testing and feedback
