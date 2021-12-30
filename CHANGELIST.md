### WIP (xxxx-xx-xx)
- Start overhauling Redump information pulling, again
- Add internal structure for special site codes
- Add new tabs for special site information

### 2.2 (2021-12-30)
- Fix Saturn header finding
- Add Pocket PC support
- Add HD-DVD-Video support
- Convert to using separate Redump library code
- Update to Aaru v5.3.0-rc1
- Update to BurnOutSharp 1.8.0
- Update support packages
- Add on-startup "check for updates" option
- Update to Aaru v5.3.0-rc2
- Add .NET 5.0 as build target
- Remove .NET Core 3.1 and .NET 5.0 from AppVeyor build artifacts
- Null-safeguard RedumpLib conversions
- Move cuesheet code to separate DLL
- Move CICMMetadata to top-level
- Separate out remaining functionality into individual DLLs
- Update to Aaru v5.3.0 LTS
- Update to DIC 20211001
- HTTP encode password for Redump login, again
- Fix NullReferenceException in anti-modchip scans
- Update arcade metadata (Shizmob)
- Add JSON output option for MPF.Check
- Fix JSON output serialization
- Ensure corrupt directories on media don't throw errors
- Add retry to Redump external calls
- Make anti-modchip scans even safer
- Remove lower bound checking on LBA values for DIC
- Remove offset for audio discs on DIC output
- Start adding regression tests for DIC
- Ensure parameters box is safer during options save
- Fix protection sanitization and add regression tests
- Update to DIC 20211101
- Add protection sanitization for StarForce
- Trim filenames for DVD protection from DIC
- Fill out internal tests around Redump library
- Refine the "missing disc" text
- Overhaul XeMID handling
- Fix output dialog issues in Options window
- Fix saving path settings if set from dialog
- Allow default system if skipping system detection enabled
- Add internal support for all Redump site codes
- Use the Volume Label special site code
- Capture newlines in Redump fields
- Invert condition for volume label
- Fix missing ISN usages
- Fix ISN string
- Validate track count when matching Redump
- Allow for better matching of multi track discs
- Temporarily disable pulling comments from Redump pages
- Add safety around volume labels

### 2.1 (2021-07-22)
- Enum, no more
- Sony works backward
- Add experimental dark mode
- Allow users to customize protection scanning
- Fix negative offsets for `/a` flag
- Always check for all DIC log files, just in case
- Check for the zipped logs for dealing with overwrites
- Be smarter about checking for zipped logs
- Change offset value for HVN discs
- Update to DIC 20210601
- Fix Aaru command normalization
- Handle carriage returns better
- Add logging helper class
- Set matcher on carriage return for log formatting
- Replace dumping program output processing
- Fix volume name detection for XBOX discs
- Add new setting for including artifacts in serialzied JSON
- Fix logging in to Redump for verifications
- Gracefully handle timeouts during login
- Update to DIC 20210701
- Support `/mr` value parameter
- Support new mainInfo header
- Add BD to IBM-PC supported discs
- Fix launching MPF on Windows 7
- Fix log window background turning white
- Add DMI/PFI/SS to log zipfile
- Update to BurnOutSharp 1.7.0

### 2.0 (2021-04-23)
- Rename DICUI to Media Preservation Frontend (MPF)
- Add handling for BEh drive _mainInfo.txt changes
- Fix multiline regex fields during info pulling
- Add preliminary support for user-defined default system
- Change labels in media info window depending on media type
- Update to Aaru v5.2
- Only pull disc information if every track returns at least one ID
- Add new supported Redump regions
- Remove Philips CD-i Digital Video from supported profiles
- Remove experimental Avalonia UI, will wait for MAUI next year
- ~~Updated to DIC version 20210102~~
- Add support for `/mr` DIC flag
- UI initialization code refactored to have more consistent results
- Support DIC `.imgtmp`, `.scmtmp`, and `.subtmp` possible output files
- Fix BCA formatting for CleanRip outputs
- Add hashing for UMDs from UIC outputs
- Fix information gathering for UIC outputs
- Fix issue with duplicate security sector data in XGD DiscImageCreator outputs
- ~~Update to BurnOutSharp 1.5.1~~
- ~~Update to DIC version 20210202~~
- Add VCD detection
- Fix UI not updating properly on drive change
- Add Xbox Series and PS5 to supported systems
- Add PS5 type detection and version extraction
- Add internal support for 3- and 4-layer discs
- Revamp disc information window
- Add PIC layerbreak extraction
- Overhaul main window and logging panel
- Overhaul options window
- Update attributions and about text
- ~~Updated to DIC version 20210301~~
- Add user-selectable Language Selection via dropdown in disc submission window for PS2
- Separate out Aaru- and DIC-specific settings
- Add new options based on original "Paranoid Mode" mega-option
- Internal overhaul of options and dump environment
- VideoNow discs are audio only
- Hook up default system in options
- Make inner and outer layers in UI and outputs more clear
- Program output to log by default, setting otherwise
- DVDs and BDs can have label-side data
- Remove .NET Framework 4.7.2 support
- Make logging window a bit safer
- Support new Redump languages and regions
- Implement internal log queue
- It's a secret...
- Updated to DIC version 20210401
- Support log file compression
- Add ring code guide button to disc submission window
- ~~Update to BurnOutSharp 1.6.0~~
- Fix "rewinding" issue when inputting output paths with spaces
- Add version to About box
- Update to BurnOutSharp 1.6.1

### 1.18 (2020-11-10)
- Add more information extraction and generation for Aaru
- Remove instances of CD Check from copy protection (again, sorry)
- Fix multiline submission info outputs
- Fix PVD retrieval for multi-session discs
- Updated to DIC version 20200921
- Add and fix multiple Sega disc header pieces or submission info
- Fixed issues in parsing the alternate mainInfo format
- Fixed issue with logging clear not working properly
- ~~Updated to BurnOutSharp 1.4.1~~
- Added split archives for AppVeyor builds
- Remove subdump from both UI and run steps
- Removed default config file
- Fixed copy protect scan using wrong drive when using UI option
- Changed default to skip fixed drives
- Fixed default media type when skipping type detection
- Attempt sector reading for Saturn system detection
- Fixed default media type when detection fails
- Add option to allow users to select dumping program
- ~~Updated to DIC version 20201101~~
- Add support for `/ps` DIC flag
- Updated to BurnOutSharp 1.5.0
- Added HD-DVD-Video detection

### 1.17.1 (2020-09-14)
- Shuffled some shared, internal UI variables
- Synced WPF and Avalonia UI internals
- Made the disc information window less prone to bugs
- Fixed DIC flags based on code (not documentation)
- Added support for old(?) DIC flags: `/fix` and `/re`

### 1.17 (2020-09-12)
- Updated to Aaru version 5.1
- Updated to BurnOutSharp version 1.4.0
- Updated to DIC version 20200716
- Added experimental Avalonia UI
- Created wiki
- Removed .NET 4.6.2 and .NET Core 3.0 builds
- Added .NET 4.8 and .NET Core 3.1 builds
- Fix numerous things related to PS1/PS2
- Make subdump running optional
- Overhaul DICUI.Check with more options
- Numerous small bug- and regression-fixes

### 1.16.1 (2020-05-07)

- Add preliminary support for DD for Windows (end to end still NW)
- Add CNF parsing for Konami Python 2 discs (PS2-based)
- Updated included Aaru version
- Massive cleanup effort to detangle large chunks of code
- Miscellaneous bugfixes that came from the above

### 1.16 (2020-04-13)

- Updated to DIC version 20200403
- UI updates based on user feedback
- Added support for Aaru (formerly DiscImageChef)
- Added more support for different output file formats (such as CleanRip)
- Add PS1/PS2 serial extraction and matching
- Fix PS1 date support when both PSX.EXE and normal executable are both present
- Update BurnOutSharp
- Many MANY bits of internal cleanup

### 1.15 (2019-11-16)

- Updated to DIC version 20191116
- Support non-optical, non-floppy stuff better
- Update protection scanning and fix some things

### 1.14 (2019-10-01)

- Updated to DIC version 20191001
- Added builds for .NET 4.6.2, .NET 4.7.2, and .NET Core 3.0
- Updated and fixed a bunch of things related to Redump
- Fixed path persistence when changing system and media type
- Added more system autodetects
- Added new, optional, disc information filling window (WIP)

### 1.13 (2019-07-02)

- Added new DIC commands and flags
- Made DICUI Check more robust
- Added and updated systems along with format cleanup
- Created a new SubmissionInfo template (and internals)
- Added automatic grabbing of Redump information, if possible
- Better media format support (BD, UMD)
- Initial disc type detection

### 1.12.2 (2019-04-19)

- Added DICUI Check, a new standalone tool for parsing DIC output from platforms unsupported by DICUI
- Added a few machines/formats
- Updated to DIC version 20190326
- Added DMI data extraction for Xbox and X360

### 1.12.1 (2019-01-28)

- Fixed !submissionInfo.txt output for CD-ROM and GD-ROM

### 1.12 (2019-01-27)

- Added a few new systems and formats
- Added new DIC commands and flags
- Updated the `!submissionInfo.txt` file order
- Fixed Audio CD handling
- Added Sega CD / Mega CD header extraction
- Readded Floppy Disk as a supported format
- And more! See the full Git commit list for more details

### 1.11 (2018-09-20)

- Fix formatting of XBOX and XBOX 360 security sector output
- Add new XBOX swap commands and outputs
- Fixes for some PlayStation 2 and 4 outputs
- Added external programs to AppVeyor builds
- Fixed `.` in path issues with DIC; attempted to fix issues with `&`
- Combined XBOX 360 XGD 2/3 due to new DIC support with Kreon drives
- Fixed (semi-)longstanding bug with XBOX disc layer detection
- Added DVD-Video protection output
- Made custom parameters work a little more intuitively
- Added *EXPERIMENTAL* Winforms-based UI
- And more! See the full Git commit list for more details

### 1.10 (2018-07-29)

- Added many new options for user customization
- Added unit testing and an AppVeyor build
- Many code refactorings
- **LOG WINDOW**
- Separated out protection scan and Unshield ports to new projects
- Added "empty drive" support; should help with 3DO and HFS dumping
- And much more! See the full Git commit list for more details

### 1.07 (2018-06-27)

- Separated system and media type for easier navigation
- Combined instances of single- and dual-layer discs
- Removed reliance on **sg-raw** and **psxt001z**
- Added system and disc type to the submission info
- First attempt at getting current disc type
- Made the three PSX-specific fields (**EDC**, **Anti-modchip**, and **LibCrypt**) automatically filled in, when possible
- Many, many, many behind the scenes updates for speed, future features, and stability

### 1.06 (2018-06-15)

- Fixed not being able to use the `/c2` flag properly
- Fixed times when the ability to start dumping was improperly allowed
- Added full support for XBOX and XBOX360 (XDG1, XDG2) dumping through DIC (using a Kreon, or presumably a 0800)

### 1.05a (2018-06-14)

- Fixed some ordering and nullability issues
- Added automatic fields for PS1, PS2, Saturn

### 1.05 (2018-06-14)

- Miscellaneous fixes around custom parameter validation, dump information accuracy, settings window, and TODO cleanup
- Add many more supported platforms, mostly arcade (based on publicly available information)
- Add floppy disk dumping support
- Add optional disc eject on completion
- Add subdump for Sega Saturn
- Fully support newest version of DIC including all new flags and commands
- PlayStation and Saturn discs still don't have all internal information automatically generated

### 1.04b (2018-06-13)

- Added subIntention reading
- Fixed extra extensions being appended
- Fixed internationalization error (number formatting)
- Fixed "Custom Input" not working

### 1.04a (2018-06-13)

- Fixed issue with empty trays
- Added settings dialog

### 1.04 (2018-06-13)

- Behind-the-scenes fixes and formatting
- Better checks for external programs
- Automatically changing disc information
- Custom parameters (and parameter validation)
- Automatic drive speed selection
- Automatic submission information creation
- Add ability to stop a dump from the UI

### 1.03 (2018-06-08)

- edccchk now run on all CD-Roms
- Discs unsupported by Windows are now regonized
- Extra \ when accepting default save has been removed.
 
### 1.02b (2018-05-18)

- Added missing DLL

### 1.02 (2018-05-18)

- Fixed XBOX One and PS4 Drive Speed issue.
- Started implementing DiscImageCreator Path selection.
- Conforming my naming for objects and variable

### 1.01d (2018-05-18)

-Combine IBM PC-CD options, misc fixes.