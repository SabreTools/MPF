# Media Preservation Frontend (MPF)

DiscImageCreator/Aaru UI in C#

[![Build status](https://ci.appveyor.com/api/projects/status/3ldav3v0c373jeqa?svg=true)](https://ci.appveyor.com/project/mnadareski/MPF/build/artifacts)

This is a community project, so if you have some time and knowledge to give, we'll be glad to add you as a contributor to this project. If you have any suggestions, issues, bugs, or crashes, please look at the [Issues](https://github.com/SabreTools/MPF/issues) page first to see if it has been reported before and try out the latest AppVeyor WIP build below to see if it has already been addressed. If it hasn't, please open an issue that's as descriptive as you can be. Help me make this a better program for everyone :)

## Releases

For those who would rather use the most recent stable build, download the latest release here:
[Releases Page](https://github.com/SabreTools/MPF/releases)

For those who like to test the newest features, download the latest AppVeyor WIP build here: [AppVeyor](https://ci.appveyor.com/project/mnadareski/MPF/build/artifacts)

## Media Preservation Frontend (MPF)

MPF is the main, UI-centric application of the MPF suite. This program allows users to use DiscImageCreator, Aaru, Redumper, or dd for Windows in a more user-friendly way. Each backend dumping program is supported as fully as possible to ensure that all information is captured on output. There are many customization options and quality of life settings that can be access through the Options menu.

### System Requirements

- Windows 8.1 (x86 or x64) or newer
    - Users who wish to use MPF on Windows 7 need to disable strong name validation due to `Microsoft.Management.Infrastructure` being unsigned. Add the following registry keys (accurate at time of writing):
    ```
        [HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\StrongName\Verification\*,31bf3856ad364e35]
        [HKEY_LOCAL_MACHINE\SOFTWARE\Wow6432Node\Microsoft\StrongName\Verification\*,31bf3856ad364e35]
    ```
    - Alternatively, look at this [StackOverflow question](https://stackoverflow.com/questions/403731/strong-name-validation-failed) for more information.

- .NET Framework 4.8 or .NET 6.0 Runtimes (.NET 6.0 is mostly functional due to a dependency issues but may be unstable in some situations)
- As much hard drive space as the amount of discs you will be dumping (20+ GB recommended)

Ensure that your operating system is as up-to-date as possible, since some features may rely on those updates.

## Media Preservation Frontend Checker (MPF.Check)

MPF.Check is a commandline-only program that allows users to generate submission information from their personal rips. This program supports the outputs from DiscImageCreator, Aaru, Redumper, dd for Windows, Cleanrip, and UmdImageCreator. Running this program without any parameters will display the help text, including all supported parameters.

### System Requirements

- Windows 8.1 (x86 or x64) or newer, GNU/Linux x64, or OSX x64
- .NET Framework 4.8 (Windows or `mono` only) or .NET 6.0 Runtimes

## Information

**WARNING**: For those who use DiscImageCreator for dumping, MPF will replace `.` with `_` in any inputted paths which may be unexpected. If there are `.` outside of the extension in either the path or the filename, DiscImageCreator will fail to perform the final hashing step properly. Unfortunately, a malformed `.dat` will still be generated, thus appearing to have succeeded and causing confusion with users. As this is avoidable by a simple replacement, MPF will continue to do that until DiscImageCreator is fully fixed.

For all additional information, including information about the individual components included in the project and what dumping programs are supported, please see [the wiki](https://github.com/SabreTools/MPF/wiki) for more details.

## Changelist

A list of all changes in each stable release and current WIP builds can now be found [here](https://github.com/SabreTools/MPF/blob/master/CHANGELIST.md).

## External Libraries

MPF uses some external libraries to assist with additional information gathering after the dumping process.

- **BurnOutSharp** - Protection scanning - [GitHub](https://github.com/mnadareski/BurnOutSharp)
- **UnshieldSharp** - Protection scanning - [GitHub](https://github.com/mnadareski/UnshieldSharp)
- **WPFCustomMessageBox.thabse** - Custom message boxes in UI - [GitHub](https://github.com/thabse/WPFCustomMessageBox)

## Contributors

Here are the talented people who have contributed to the project so far:

- **darksabre76** - Project Lead / Backend Design / UI Maintenence
- **ReignStumble** - Former Project Lead / UI Design
- **Jakz** - Primary Feature Contributor
- **NHellFire** - Feature Contributor
- **Shadów** - UI Support

## Notable Testers

These are the tireless individuals who have dedicated countless hours to help test the many features of MPF and have worked with the development team closely:

- **Dizzzy/user7** - Additonal thanks for the original concept
- **Kludge**
- **ajshell1**
- **Whovian**
- **Gameboi64**
- **silasqwerty**

## Community Shout-Outs

Thanks to these communities for their use, testing, and feedback. I can't even hope to be able to thank everyone individually.

- **VGPC Discord** - Fast feedback and a lot of testing
- **Redump Community** - Near-daily use to assist with metadata gathering
