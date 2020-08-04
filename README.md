# DICUI

DiscImageCreator/Aaru UI in C#

[![Build status](https://ci.appveyor.com/api/projects/status/3ldav3v0c373jeqa?svg=true)](https://ci.appveyor.com/project/mnadareski/dicui/build/artifacts)

This is a community project, so if you have some time and knowledge to give, we'll be glad to add you to the contributor of this project :)

## System Requirements

Even though this is written in C#, this program can only be used on Windows systems due to one of the base programs, DiscImageCreator, being Windows-only. There is some preliminary support for Linux underway, and we will try to integrate with that when the time comes.

- Windows 7 (newest version of Windows recommended) or Mono-compatible Linux environment (DICUI.Check only)
- .NET Framework 4.6.2, .NET Framework 4.7.2, .NET Framework 4.8, or .NET Core 3.1 Runtimes (.NET Core 3.1 is only partially functional due to a dependency, use at your own risk)
- 1 GB of free RAM
- As much hard drive space as amount of discs you will be dumping (20+ GB recommended)

Ensure that your operating system is as up-to-date as possible, since some features may rely on those updates.

## Components

The DICUI project contains two actual programs: DICUI and DICUI.Check. Both output the same, standardized submissionInfo format based on Redump submission criteria. Below is a brief comparison of both:

| Program Name | Interface | OS Compatibility | Can Dump Directly | Notes |
|--------------|-----------|------------------|-------------------|-------|
| DICUI | GUI | Windows 7+ (.NET 4.6.2 and above) | Yes | Includes tailored profiles that can be selected from the main window for easier dumping with supported programs. It is able to access the disc post-dump to gather more information automatically, including versions and copy protection status. Configuration is either using the Options window within the GUI or directly in the configuration file. |
| DICUI.Check | Commandline | Windows 7+ (.NET 4.6.2 and above), `mono` | No | Meant as a wrapper around the post-dump steps of DICUI to be run on pre-existing dumps. It does not attempt to accss original media to gather more information, so only data derived from the output files themselves is included. Configuration is purely done by per-run parameters. |

## Releases

For those who would rather the most recently stable build, ownload the latest release here:
[Releases Page](https://github.com/SabreTools/DICUI/releases)

For those who like to test the newest features, download the latest AppVeyor WIP build here: [AppVeyor](https://ci.appveyor.com/project/mnadareski/dicui/build/artifacts)

## Changelist

A list of all changes in each stable release can now be found [here](https://github.com/SabreTools/DICUI/blob/master/CHANGELIST.md).

## Comparison of Dumping Programs

DICUI used to only be a frontend for DiscImageCreator (hence the name) but multiple dumpers and parsed outputs are now available. To make it easier on users, below is a table representing the various dumping programs and their relevant features.

| Program Name | Developer | Can Dump | Can Check | Redump Compatible Output | Notes |
|--------------|-----------|----------|-----------|--------------------------|-------|
| [Aaru](https://github.com/aaru-dps/Aaru) | Claunia | Yes | Yes | Partial | Alternative dumping program, developed to support a wide range of media types. Fully open development with outputs that contain most of what is needed for Redump standards. WIP and beta builds available. |
| [CleanRip](https://github.com/emukidid/cleanrip) | emukidid | No | Yes | Partial | Output file parsing only, developed for dumping directly on the Wii. Outputs mostly Redump compatible outputs based on parsing of output files. No WIP builds available. |
| [dd for Windows](http://www.chrysocome.net/dd) | chrysocome.net | Yes | No | No | Alternative dumping program, build used built by chrysocome.net. Does not output anything remotely Redump compatible due to block dumping nature of the program and no external metadata file generation. No WIP builds available. |
| [DiscImageCreator](https://github.com/saramibreak/DiscImageCreator) | Sarami | Yes | Yes | Yes | Default dumping program, developed specifically for dumping to Redump standards. WIP builds partially available. |
| [UmdImageCreator](https://github.com/saramibreak/UmdImageCreator) | Sarami | No | Yes | Yes | Output file parsing only, developed specifically for dumping to Redump standards. No WIP builds available. |

For users who are concerned about having exact matches to submit or verify against Redump, **DiscImageCreator** is essentially your only option. None of the other supported dumping tools can produce the full array of outputs that are used by Redump submissions. This is considered the standard for the Redump project which will likely not change anytime soon.

**Aaru** is, in the long term, a much better solution for dumping given the amount of disc metadata is included in the default AIF output and associated dumping files. Things like hashes will likely match for verification's sake, but the output AIF files, due to their unique format, will not be an external match. However, AIF outputs can be converted to BIN/CUE using Aaru.

**Note:** In general, WIP builds for all above programs are not supported with new flags or features until they make it into the stable release. Sometimes branches will be created for larger upcoming updates to make the transition easier.

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