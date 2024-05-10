### WIP (xxxx-xx-xx)

- Update Redumper to build 325
- Fix CleanRip not pulling info (Deterous)
- Fix XboxOne/XboxSX Filename bug (Deterous)

### 3.1.8 (2024-05-09)

- Option for default Redumper leadin retries (Deterous)
- Omit false positives on formatting protections
- Critical update to BinaryObjectScanner 3.1.10
- Add _PFI.bin support for UIC

### 3.1.7 (2024-04-28)

- Critical update to BinaryObjectScanner 3.1.9

### 3.1.6 (2024-04-27)

- Fix parameter parsing for `=` symbol (Deterous)
- Define better default categories (Deterous)
- Custom non-redump Redumper options (Deterous)
- Update packages
- Update packages

### 3.1.5 (2024-04-05)

- Handle `.0.physical` files from Redumpers
- Read C2 error count from Redumper logs
- Read last instance of hash data from Redumper
- Add Konami Python 2 system detection
- Fix outdated information in README
- Fix missing information in README
- Language selections unchecked by default
- Update BinaryObjectScanner to 3.1.3
- Fix information pulling for redumper (fuzz6001)
- Update packages
- Update BinaryObjectScanner to 3.1.4
- Detect Xbox Series X discs (Deterous)
- Enable Windows targeting for test project
- Fix test project project includes
- Fix CleanRip hash output for Check (Deterous)
- Enable label-side mastering SID and toolstamp
- Enable remaining fields for label-side information
- Update BinaryObjectScanner to 3.1.5

### 3.1.4 (2024-03-16)

- Update BinaryObjectScanner to 3.1.2

### 3.1.3 (2024-03-15)

- Gate debug publishing behind use all flag
- Hide layerbreaks if value is 0
- Make GHA debug-only
- Remove GHA pull request builds
- Add PR check workflow
- Don't link to AppVeyor artifacts page anymore
- Add PS3 CFW support to MPF.Check (Deterous)
- Hide size if value is 0 (Deterous)
- Fix title normalization (Deterous)
- Ensure no labels are empty
- Use SabreTools.Hashing
- Update to SabreTools.RedumpLib 1.3.5
- Update packages to latest
- Enable LibIRD for all .NET frameworks (Deterous)
- Try updating PR check action
- Fix config access persmission (Deterous)
- Fix Check UI deadlock (Deterous)
- Fix formatting output formatting
- Update LibIRD to 0.9.0 (Deterous)
- Update packages
- Fix Redumper generic drive type (Deterous)
- Add MPF version to Submission info (Deterous)
- Update to RedumpLib 1.3.6

### 3.1.2 (2024-02-27)

- Remove debugging lines from build script
- Port build script fixes from BOS
- Fix double git hash version (feat. Deterous)
- Readd x86 builds by default
- Hide unavailable dumping programs (Deterous)
- Remove DIC and Aaru bundles from CI
- Add x86 builds to AppVeyor
- Make AppVeyor builds framework-dependent
- Fix misattributed artifact
- Update README with current build instructions
- Opt-in automatic IRD creation after PS3 dump (Deterous)
- Add CI via Github Workflows (Deterous)
- Reorganize solution items
- Split CI workflow files
- Add GHA CI status badges
- Rename badges for GHA
- More tweaks to CI
- Fix net35 build issue
- Remove now-unnecessary restore step
- Remove net35 from MPF... again
- Fix whitespace that got unwhitespaced
- Fix link in README
- Attempt to add CD to existing actions
- Try fixing the artifact upload
- Use recommendation from upload-artifact
- Revert artifact ID, use name?
- Try using download-artifact
- Download all artifacts?
- Use newer download version
- Build artifact before upload
- Change link to WIP builds in README
- Use commit SHA as body of rolling releases
- Don't omit body when setting body
- Remove unnecessary empty section
- Unified tag for rolling release
- Generate release notes automatically
- Remove generation, just in case
- Change link to WIP builds in README
- Show hashes in readonly data
- Update to BinaryObjectScanner 3.1.0
- Add Mattel HyperScan detection
- Pull PS3 Disc Key from redump (Deterous)

### 3.1.1 (2024-02-20)

- Remove .NET 6 from auto-builds
- Make Redumper the default for new users
- Fix DIC log parsing for SS version (Deterous)
- Write outputs with UTF-8
- Add funworld Photo Play detection
- Fix Aaru drive parameter generation
- Limit DVD protection outputs
- Add a GUI for PS3 IRD Creation (Deterous)
- Update LibIRD, disable UI elements when creating IRD (Deterous)

### 3.1.0 (2024-02-06)

- Update RedumpLib
- Update Redumper to build 294
- Fix commented out code
- Make missing hash data clearer
- Get BD PIC Identifier for redumper (Deterous)
- Support redumper skeleton and hash files (Deterous)
- Support ringcode and PIC for triple/quad-layer (fuzz6001)
- Cleanup !protectionInfo.txt (Deterous)
- Update Redumper to build 311 (Deterous)
- Use PSX/PS2 serial as filename when Volume Label not present (Deterous)
- Allow variables in output path (Deterous)
- Check for presence of complete dump from other programs (Deterous)
- Retrieve volume label from logs (Deterous)
- Correct missing space in PVD (fuzz6001)
- Prevent crashing on invalid parameters (Deterous)
- Detect CDTV discs (Deterous)
- Differentiate CD32 from CDTV (Deterous)
- Normalise Disc Titles in Submission Info (Deterous)
- Skip warning line in Redumper log
- Add a GUI for MPF.Check (Deterous)
- Fix information pulling for CleanRip and UIC
- Add UMD handling for the disc info window
- Detect Photo CD
- Parse PSX/PS2/KP2 exe date from logs (Deterous)
- Exclude extra tracks when finding disc matches (Deterous)
- Verbose Redumper log by default (Deterous)
- Retrieve serial from Cleanrip
- Fix build from rushed code
- Remove `-disc2` from Cleanrip serial
- Enable Windows builds on Linux and Mac
- Fix compiler warning (Deterous)

### 3.0.3 (2023-12-04)

- Fix broken tests
- Fix using SHA-1 for track checks
- Fix build warning for NRE
- Remove .NET Framework 3.5 from build script
- Handle or suppress some messages

### 3.0.2 (2023-12-01)

- Read CSS for some copy protections
- Add Disc ID and Key fields in info window
- Replace build script with Powershell
- Fix Powershell build script
- Fix cross-framework UI rendering
- Fix most .NET Framework 3.5 issues
- Fix cross-framework UI styles
- Update Redumper to build 271
- Update USE_ALL in Powershell script
- Import WinFX for .NET Framework 3.5
- Reference .NET Framework 3.0 for 3.5
- Handle most VS and dotnet differences

### 3.0.1 (2023-11-30)

- Add Bandai Pippin detection
- Zip manufacturer files for Redumper
- Fix BE flag logic bug in DIC
- Support ancient .NET in Core
- Support ancient .NET in UI Core
- Support C# 12 syntax
- Support ancient .NET in Check
- Support ancient .NET in UI
- Fix TLS for older .NET
- Perform more ancient .NET support work
- Prepare XAML for ancient .NET support
- Suppress deprecation warnings
- Fix reversed ringcode test
- Add C#12 syntax to tests
- Trim PS3 serial and add unrelated notes
- Update RedumpLib and use moved methods
- Perform some post-move cleanup
- More C# 12 cleanup in Core
- Update to BinaryObjectScanner 3.0.1
- Update Xunit packages
- Get Core building with Framework 4.0
- Get UI.Core building with Framework 4.0
- Get Check building with Framework 4.0
- Use TryGetValue on dictionaries
- Support proper async in .NET Framework 4.0
- Temporarily remove .NET Framework 4.0
- Update to BinaryObjectScanner 3.0.2
- Re-enable .NET Framework 4.0 building in Core
- Re-enable .NET Framework 4.0 building in UI.Core
- Re-enable .NET Framework 4.0 building in Check
- Support .NET Framework 3.5
- Update compatibility libraries
- Support .NET Framework 2.0
- Support .NET Framework 3.5 in UI.Core
- Get UI building with Framework 4.0
- Temporarily remove .NET Framework 4.0 from UI
- Get UI building with Framework 4.0 again
- Support .NET Framework 3.5 in UI
- Update Redumper to build 268

### 3.0.0 (2023-11-14)

- Remove .NET Framework 4.8 from build
- Remove .NET Framework 4.8 from projects
- Remove .NET Framework 4.8 gated code
- Address lingering modern .NET syntax
- Update README for Legacy
- Remove redundant information
- Remove 4.8 note from build script
- Even more README cleanup
- Consolidate build script information
- Remove lingering script reference
- Make releases less verbose
- Remove .NET Framework 4.8 from issue report
- Remove Unshield reference
- Update documentation for BOS change
- Bump library versions
- Update to .NET 8

### 2.7.5 (2023-11-06)

- Remove psxt001z Pkg Ref in MPF.Test (Deterous)
- Update Redumper to build 247
- Focus main window after child window closes (Deterous)
- Try to get PS3 data from SFB
- Fix PS3 version finding
- Pull PS3 Firmware Version (Deterous)
- Fix default layerbreak output
- Enable browsing for Redumper path (Deterous)
- Update to MMI 3.0.0 (Deterous)

### 2.7.4 (2023-10-31)

- Move Hash enum and simplify
- Add other non-cryptographic hashes
- Avoid unncessary allocation in hashing
- Make Hasher more consistent
- Move all hashing to Hasher
- Separate out static hashing
- Remove unncessary summary
- Version-gate new switch statement
- Ignore empty protection results
- Remove and Sort Usings
- Update Redumper to build 244
- Enable Bluray dumping with Redumper
- Add Bluray layerbreak support for Redumper
- Remove now-obsolete notes
- Compile most regex statements
- Handle a couple of messages
- Remove .manufacturer for Bluray
- Fix typo that disables DIC during media check (Deterous)
- Fix build
- Remove duplicate check
- Add PIC output for Redumper
- Update Redumper to build 246
- Enable HD-DVD for Redumper in UI
- Attempt to fix window owner issue

### 2.7.3 (2023-10-26)

- Split InfoTool into 2 classes
- Add path variants for PlayStation info
- Convert Drive to use paths internally
- Create skeleton for first-run
- Show options window on first run
- Fix drive letter issue in UI
- Add first-run Options title, fix saving bug
- Fix multiple handler invocation
- Create method for applying theme
- Fix drive letter check
- Simply theme application
- Add framework for deleteable files
- Add deleteable file lists for Redumper and DIC
- Add optional file deletion
- Rearrange OptionsWindow to be easier to navigate
- Fix up DumpEnvironment a bit
- Add filename suffix setting (nw)
- Wire through filename suffix
- Expose suffix setting
- Set UDF CD threshold at 800MB (Deterous)
- Update package versions
- Attempt to parse out PS5 params.json
- Fix CRC32 hashing
- Update XUnit packages
- Update to BurnOutSharp 2.9.0
- Update to MMI 3.0.0-preview.4
- Fix two small nullability issues
- Use Array.Empty in hasher
- Always use relative path internally (Deterous)

### 2.7.2 (2023-10-17)

- Fix options loading for Check
- Cleanup and gated code
- Gate some switch expressions
- Suppress some unnecessary messages
- Disable dumping button when Redumper selected with unsupported media type (Deterous)
- Improve check for which program supports which media (Deterous)
- Remove code for getting Universal Hash from DIC logs (Deterous)
- Fix Redumper retry count not showing
- Enable parameters checkbox by default
- Update Redumper to build 230
- Fix GetPVD in dic (fuzz6001)
- Get SecuROM data from Redumper
- Clean up issue templates
- Support Redumper 231 outputs

### 2.7.1 (2023-10-11)

- Add pull-all flag for Check
- Fix errant version detection issue
- Allow user-supplied information in Check
- Add BE read flag for Redumper
- Add generic drive flag for Redumper
- Handle numeric disc titles as issue numbers
- Unify handling of enable/disable events
- Enable nullability in Check
- Remove all but .NET 6 for AppVeyor packaging
- Place message boxes at center of main window (Deterous)
- Enable nullability in MPF
- Enable nullability in MPF.UI.Core
- Enable nullability in MPF.Test
- Handle some suggested changes
- Var-ify many instances
- Version-gate some newer syntax
- Fix Check always showing Help text
- Version-gate some more newer syntax
- Enable path browse button by default
- Remove unnecessary namespacing

### 2.7.0 (2023-10-11)

- Attempt to replace NRT
- Try alternate archive naming
- Publish, but don't package, release builds
- Add note about git being required
- Try out using version number in AppVeyor
- Job number not build ID
- Fix errant space in variable name
- Build number not job number
- Use System.IO.Hashing for CRC32
- Don't reverse the CRC32 output
- Add submission info preamble
- Fix missing comma
- Remove IMAPI2 with no substitution
- Remove framework exceptions
- Slight reorganization of code
- Update README with support information
- Remove msbuild-specific AppVeyor items
- Be trickier when it comes to type from size
- Fix "detected" message
- Be smarter about media type based on system
- Consolidate into MPF.Core
- Fix failing tests
- Remove debug symbols in release builds (Deterous)
- Allow nullability for modern .NET
- Fix failing tests
- Remove unnecessary include
- Move element items to Core
- Move LogLevel enumeration
- Split logging code a bit more
- Split info window code a bit more
- Clarify build instructions in README (Deterous)
- Split options window code a bit more
- Use binding for more disc info textboxes
- Handle Redump password changing better
- Refine options window bindings
- Refine options window bindings further
- Refine info window bindings further
- Move decoupled view models
- Fix log output
- Start migrating MainViewModel
- Perform most of MainViewModel changes
- Small updtes to MainViewModel
- Remove MainWindow from MainViewModel
- Use callback for logging, fix Options window
- Remove WinForms from MainViewModel
- Remove some message boxes from MainViewModel
- Remove more Windows from MainViewModel
- Fix null reference exception in disc type
- Detected type and selected type are different
- Remove message boxes from MainViewModel
- Move MainViewModel to Core
- Remove LogOutputViewModel
- Consolidate some constants
- Fix media type ordering
- Fix dumping button not enabling
- Recentralize some Check functionality
- Fix media combobox not updating (Deterous)

### 2.6.6 (2023-10-04)

- Update Nuget packages
- Update to MMI 3.0.0-preview.2
- Remove errant character from script
- Add placeholders for release builds
- Fully sync AppVeyor build with script
- Stop compiling Chime finally
- Address some warnings and infos
- Add setting for pulling comment/contents
- Move to config.json
- Omit track 0.2 and 00.2 from hash search
- Tweak README again
- Fix redumper EDC detection output
- Fix XGD4 PIC reading
- Ensure popups are topmost
- Try out more UI functionality
- Skip system detection on inactive drives
- Fix path tests
- Clean up csproj files
- Update redumper to build 221
- Ensure multisession info is populated
- Be clearer with protection outputs

### 2.6.5 (2023-09-27)

- Normalize Redumper CSS output
- Update redumper to build 219
- Add .NET 7 to build scripts
- Disable subdump download in AppVeyor
- Normalize publish scripts
- Update AppVeyor to match scripts
- Add release publish scripts
- Handle invalid characters when changing program
- Fix options not saving on update
- Combine build scripts
- Force information window to top
- Reset debug option to false

### 2.6.4 (2023-09-25)

- Add CD Projekt ID field
- Fix speed setting in Aaru
- Add helper for changing dumping program from UI
- Handle extension changing only
- Swap order of operations for changing program
- Fix dumping path in DD
- Fix PlayStation serial code region parsing (Deterous)
- Retrofit README
- Migrate to Nuget package for Redump
- Remove dd for Windows
- Migrate to Nuget package for XMID
- Migrate to Nuget package for cuesheets
- Migrate to Nuget package for PIC
- Fix timestamp (Ragowit)
- Not building against .NET Standard
- Move RedumpSystemComboBoxItem
- Move Constants
- Move WPFCustomMessageBox
- Remove EnableProgressProcessing
- Remove EnableLogFormatting
- Remove App references from LogViewModel
- Remove log formatting code
- Move LogOutput
- Move to csproj tag for internals
- Move OptionsWindow
- Begin decoupling App
- Fix build
- More decoupling App
- Make Options non-cloneable
- Set parent on MainViewModel init
- Make logger a local reference
- Move options into MainViewModel
- Move MainWindow
- Add .NET 7 as a build target (not AppVeyor)
- Fix tests that have been broken for a while

### 2.6.3 (2023-08-15)

- Update redumper to build 195
- Add known .NET 6 limitations to README
- Remove `_drive.txt` from required UIC outputs
- Make `use` flag required for MPF.Check
- Add dumping date to log
- Non-zero data start only for audio discs
- Update SafeDisc Sanitization (TheRogueArchivist)

### 2.6.2 (2023-07-25)

- Ensure custom parameters properly set
- Universal hash only for audio discs
- Support LibCrypt data from Redumper
- Always show extension for Redumper
- Normalize old universal hash text
- Skip extra tracks during checking
- Modify the track count on checking
- Attempt to match universal hash
- Fix .NET Framework 4.8 build
- Fix universal hash URL generation
- Add Windows publish script
- Add *nix publish script
- Add build instructions to README
- Clarify build instructions

### 2.6.1 (2023-07-19)

- Simplify Redumper error value extraction
- Don't pull comment fields that auto-populate
- Set best compression levels for log files
- Be more explicit about .NET 6 limitation
- Set extensions for Redumper in UI
- Fix comment field pulling again

### 2.6 (2023-07-14)

- Update README
- Add warning to login tab
- Pull hardware info from redumper log (fuzz6001)
- Increase the version of AppVeyor (fuzz6001)
- Add Windows 7 note to README
- Update redumper to build 113
- Split MMI invocation in drive listing
- Update README
- Add Hasbro iON
- Get the write offset from the latest redumper (fuzz6001)
- Update redumper to build 115
- Update to DIC 20230401
- Add DIC `.` notice to README
- Resync with Redump
- Update redumper to build 118
- Clarify non-Redump systems
- Add UltraCade
- Re-enable BD33 and BD66
- Add PIC models for BD (unused)
- Handle PIC based on disc type
- UMDs always have "2 layers"
- Add dumping program selection to main UI
- Update to DIC 20230413
- Comment out `.` handling for DIC
- Ensure dumping program box can enable/disable
- Disable special SmartE handling for DIC
- Remove path from PS1/PS2 serial
- Make TOC file optional for CD/GD
- Add datafile models
- Add datafile helper method
- Start migrating to datafile serialization
- Prepare for future DIC changes
- Add suppl support to Xbox
- Support single digit subs
- Fix info tool hash finding
- Fix missing size for ISO data
- Attempt to more accurately parse layerbreaks
- Fix upcoming suppl DAT parsing
- Ensure blank lines don't interfere
- De-indent ringcode data
- Be more specific with runtime identifiers
- Fix subdump output path in AV config
- Fix subdump mkdir path in AV config
- Single file packing for .NET 6 again
- Add missing BD disc type identifier string
- Truncate PIC data for PS4/PS5
- Add internal theme support with class
- Add PIC identifier to SubmissionInfo
- Fix other media type method
- Fix non-zero offset text
- Add executable listing for XSX
- Update redumper to build 151
- Add more safety to DAT generation
- Get write offset from redumper 119 (fuzz6001)
- Update hardware information pull from redumper (fuzz6001)
- Add MCD/SS header support for Redumper
- Fix MCD region(s) parsing
- Update to DIC 20230606
- Update redumper to build 166
- Unblock Redumper DVD support
- Support Redumper DVD layerbreak
- Add placeholder and TODOs for Redumper
- Add TODO with notes to Redumper
- Fix 2-layer DVD support in Redumper
- Fix VSCode build
- Fix Redumper DAT/layer parsing
- Update Redumper PS1 output parsing
- Fix previous commit, clean up helpers
- Update redumper to build 173
- Initial support for Redumper CSS outputs
- Hook up CSS output for testing
- Update redumper to build 174
- Add support for redumper CD synonyms
- Adjust CSS title key parsing
- Fix non-reading loop
- Normalize Redumper CSS outputs
- Normalize multi-instance site tags
- Add missing Aaru error log to zip
- Check Redumper dat section for completeness
- Update redumper to build 176
- UMDs are Sony discs
- Strip colons from Redumper disc key
- Use `HashSet` for disc or book type
- Add ordering to disc or book type
- Update redumper to build 183
- Parse and format Redumper CD multisession data
- New layerbreak takes precedence
- Reduce pulled information for Xbox and X360
- Ensure we found the tags we're skipping
- Omit pulling universal hash
- Update Nuget packages to newest stable

### 2.5 (2023-03-12)

- Add _drive file to zip for UIC
- Add Xbox Series X and PS5 to list, fix Acorn
- Add Xbox Series X short name to list
- Remove windows from test target
- Remove .NET 6.0 from tests, add TODO
- Add .NET 6.0 to tests, remove msbuild args
- Add HD-DVD to speed definitions
- Add PS3 internal serial and version parsing (tjanas)
- Update Nuget packages to newest stable
- Simplify path selection in UI
- Fix broken normalization test
- Update drive info before dumping
- Update redumper strings
- Add new parameter and mode validation
- Add redumper to the UI
- Update redumper to build 81
- Fix incorrect naming in Options window
- Initial attempt at parsing redumper outputs
- Update README
- Reenable write offset for all CDs
- Add missing redumper output file
- Force a filename for redumper
- Fix incorrect SetParameters
- Fix a couple redumper things
- Update AppVeyor version
- Output security sectors to info
- Remove x86 requirement for build
- Fix XGD media type outputs
- Fix typo in ToInternalProgram
- Fix redumper error count parsing
- Update to Aaru v5.3.2 LTS
- Update options loader with sane defaults
- Fix AppVeyor pathing
- Skip during detection
- Address some UI concerns
- Tweak AppVeyor, show Check 6.0 builds
- More strict when custom parameters editing
- Use msbuild for .NET Framework 4.8
- Update nuget packages
- ReadAllText not ReadAllLines
- Add drive format (fs) to log
- Go back to pre .NET 7 Aaru
- Be smarter about old paths
- Fix relative paths for DIC
- Add nicer failure message
- Update to DIC 20230201
- Use relative path output for DIC
- Remove usage of Aaru
- Remove Aaru as submodule
- Fix Aaru removal
- Enable .NET 6 Windows builds
- Update Redumper to build_106
- Reformat CICM for old .NET versions
- Attempt to handle no drives
- Handle no drives better
- Handle no drives betterer
- Packaging requires `-windows` for framework
- Fix other `-windows` places for .NET 6
- Fix typo in DICMultiSectorReadValue
- Semi-unify drive finding
- Introduce cross-platform MMI
- Remove System.Management
- Add Redumper Universal Hash support
- Fix Redumper write offset support
- Add Redumper non-zero data start
- Use media size for type detection on .NET 6
- Trim PIC for PS3
- Get the version of redumper (fuzz6001)
- Update VSCode config files
- Can't publish single file for UI
- Handle missing extension gracefully
- Fix incorrect option slider display
- Handle undetected discs on refresh
- Originally intended behaviour of the Update Label button (IcySon55)
- Update to BurnOutSharp 2.7.0
- Get error count from recent redumper log (fuzz6001)
- Minor cosmetic changes
- Set saner defaults for dumping speeds
- Attempt to support Windows 7
- Add `win7-x64` to identifier list
- Remove unsupported identifiers
- Add identifiers in more places
- Move drive finding inside of the try/catch
- Update to DIC 20230309
- Fix errant forward slashes
- Add TOC back as optional file
- Fix Redumper path generation
- Use and trim quotes for Redumper
- Fix incorrect image name setting
- Ensure Redumper parameters are set
- Ensure min values are taken care of
- Update parameters with `=` handling
- Ensure drive and speed are set
- Handle quotes embedded
- Return list of missing files for Redumper
- Readd accidentally deleted line
- Detect EOF during the search for error counts (fuzz6001)

### 2.4 (2022-10-26)

- Update to DIC 20211001
- Fix Redump disc title pulling
- Add /mr default flag options
- Make FillFromRedump private again
- Fix DVD layer finding corner case
- Fix failing module tests
- Add option to limit region and language selections
- Cap X360 directory check to 500MB
- Assign normalized path to parameters
- Move path normalization to better place
- Specifically include Unsafe Nuget package
- Update Nuget packages to newest stable
- Add Xbox One system detection
- Update to DIC 20220301
- Force internal drive refresh
- Add single drive refresh button (IcySon55)
- Fix "missing" option in window
- Track last used drive on refresh
- Clean up pre-dump validation
- Update to Aaru v5.3.1 LTS
- Add upper case `<TAB>` processing
- Reorder event handers
- Handle sanitized protections edge case
- Update Aaru Nuget package
- Return faster on empty protection sets
- Remove redundant check around volume label
- Fix tabs in Games and Videos boxes
- Make fully and partially matching IDs more apparent
- Add write offset as read-only field
- Explicitly clear list, just in case
- Add both fully and partially matching to info file
- Fix submission info clone
- Clear out fully matched IDs from the partial list
- Refine copy protection section showing
- Update Nuget packages
- Normalize newlines in comments and contents
- Increase JSON accuracy for disc types
- Further separate out protection scan outputs
- Compress JSON for artifacts alone
- Even stricter output for copy protection section
- Report dictionary to InfoTool
- Even even stricter copy protection output
- Disable PVD creation for Aaru
- Explicitly sanitize '?' from path
- Combine cases in protection scan
- Convert status label to TextBlock
- Add option for copy protection file output
- Make protection field user-editable
- Add warning to tooltip
- Create core UI library
- Rename MPF.GUI to MPF.UI
- Add multisession pseudo-tag
- Add multisession helper method skeleton
- Move and update options loader; clean up Check
- Move helper methods around
- Consolidate Redump login testing
- Separate common arguments to new helper
- Parse and format CD multisession data
- Convert triple space to tab
- Fix clone issue with copy protection
- Be more picky about multisession
- Sanitize whitespace around tabs
- Convert internal libraries to .NET Standard 2.0
- Update AppVeyor to VS2022
- Update Nuget packages
- Remove needless csproj constants
- Update copyright date to 2022
- Revert AppVeyor to VS2019 for now
- Add size-based media type detection for non-Framework
- Gate ManagmentObject use further
- Use built-in NETFRAMEWORK directive
- Update to BurnOutSharp 2.1.0
- .NET 6.0 and Cleanup
- Remove .NET Core 3.1 from test project for now
- Remove .NET Core 3.1 entirely
- Add filesystem logging for .NET 6
- Avoid whitespace changes for PVD, Header, and Cuesheet
- Sync to newest CICM
- Update solution file for VS2022
- Add Aaru as a submodule for .NET 6
- Multisession is multi-line
- Use Aaru for media type retrival (.NET 6)
- Fix CD-R multisession info
- Better get drive list (.NET 6)
- Add optical media support method (.NET 6)
- Simplify IsOptical (.NET 6)
- Reorganize GetMediaType
- Possibly fix tab regex replacement
- Add PIC.bin to log zip
- Organize projects in solution
- Implement Drive.Create for safety
- Fix incomplete system name detection
- Add Sharp X68k detection
- Add Bandai Playdia QIS detection
- Framework for XBONE filenames
- Add files for XBONE
- Update to DIC 20220707
- Fix .bin file paths; update internal filename generation
- Disable nonstandard BD-ROM sizes
- Trim leading file paths for XBONE
- Give .NET 6 priority for web calls
- Update to BurnOutSharp 2.3.0 (nw)
- Add Mattel Fisher-Price iXL detection
- Update to BurnOutSharp 2.3.1 (nw)
- Fix serial parsing for Dreamcast
- Fix missing parenthesis
- Add internal name for Cleanrip outputs
- Fix psxt001z namespace
- Fix missing assignment
- Update to new constructor
- Update PSX content check
- Update Aaru submodule to latest devel
- Add Sony Electronic Book system
- Verify GD-ROM outputs, finally
- Electronic not Electric
- Fix ringcode guide images
- Add skeleton for Redumper
- Download Redumper with AppVeyor
- Add Redumper constants
- Add Redumper parameter values
- Add Redumper command support and reset
- Add Redumper dumping command
- Add Redumper command validation
- Add Redumper command generation
- Create Redumper extensions class
- Minor Redumper cleanup
- Add important Redumper note
- Update to DIC 20220909
- Possibly fix PIC parsing
- Add PS3 folder/file checks for detection
- Use default directory for folder browsing, if possible
- Add unused command filename parser
- Add initial framework for reporting dumping program
- Report specific DIC version, if possible
- Add dumping info section skeleton
- Update Aaru submodules
- Fix missing info reference change
- Enable separated protection info output by default
- Try adding MPF logs to zip
- Create MPF log helper and filter for deletion
- Fix missing info in Aaru
- Add hardware info to DIC and Aaru
- Fix hardware info
- Add specialized CDS/SafeDisc filter
- Add unused article formatter
- Add language filtering to formatter
- Add multi-language helper for filter
- Update Nuget packages
- Remove deprecated protection setting
- Update BurnOutSharp to 2.3.4
- Add compression result reason to log
- Add System.Memory package to MPF.Library
- Add CodePages package to MPF.Library
- Remove extraneous packages
- Change location of dumping info
- Add MS ZipFile package to MPF.Library
- Add + to positive offsets
- Disable layerbreak generation for BD
- Disable XGD version reporting
- Disable XGD layerbreak reporting
- Disable XGD1 PVD reporting
- Put Redump limitations behind existing flag
- Add framework for reported disc type
- Add disc type parsing for Aaru and DIC
- Fix multiple DiscType for DIC
- Fix multiple DiscType* for DIC
- Add logging to !submissionInfo writing failure
- Add logging to !submissionInfo formatting failure
- Update issue templates to be more accurate
- Fix NRE with offsets
- Fix readonly Filename info display
- Fix layerbreak-based checks
- Add PS4 serial finding (tjanas)
- Add unused notification method
- Move to unused Chime class
- Add PS5 serial finding (tjanas)
- Fix offset formatting (fuzz6001)

### 2.3 (2022-02-05)

- Start overhauling Redump information pulling, again
- Add internal structure for special site codes
- Add new tabs for special site information
- Clean up default handling of fields
- Try to handle multi-line fields during parsing
- Fix CSSKey log handling
- Show most read-only fields in new tab
- Add horizontal scroll to user input
- Tweak new disc information fields and tabs
- Allow internal serial and volume label to be hidden
- Be smarter about showing update checks
- Ensure all fields are read-only on read-only tab
- Add model for 2-layer ringcode guide
- Add first attempt 2-layer ringcode guide
- Add even more safety to clone
- Sanitize filename after check
- Handle pulled linebreaks better
- Skip anti-modchip string in some cases
- Handle pulled linebreaks better, again
- Tweak more Disc Info window formatting
- Skip unnecessary newlines in parsing
- Force scroll visibility, tweak text sizes again
- Fix newline skipping
- Add missing continue statement
- Add newlines for mutliline special fields
- Omit volume label for "Audio CD"
- Remove Enter/Escape registration on disc info window
- Logically group more things in disc info window
- Remove tab key from disc info window
- Add `<tab>` processing
- Unban newly opened consoles
- Tweak minimalized layout a bit more
- Add tab setting
- Further disc info window tweaks
- Changed IsEnabled to IsReadOnly
- Fix scrolling issues in disc info window
- Convert postgap and VCD fields to checkboxes
- Adjust width ratios for disc info window
- Fix IsReadOnly
- Only include booleans if the value is true
- Add hidden debug option for "ShowDebugViewMenuItem"
- Fix incorrect header check
- Make protection read-only field multiline
- Reformat Saturn internal date
- Fix Sega CD internal serial reading
- Fix crash on invalid parameters
- Differentiate XMID and XeMID
- Conditionally pull region from Redump
- Be smarter about volume labels
- Use volume label in checks, not formatted version
- Sync with Redump region and language selection
- Try to delete old log archive before writing
- Add support for all ISO language codes
- Add support for all ISO region codes
- Ensure ordering in output site tags
- Make site code formatting helper method
- Better helper method organization
- Start supporting ordered tags and non-tags
- Add more non-tag support; rearrange info window
- Fix incorrect language three-letter code
- Hook up additional Xbox field to disc info window
- Fix non-tag tag shortnames
- Fix parsing of non-tag tags again
- Disable unnecessary cuesheet parsing
- Fix incorrect region two-letter code
- Adjust long names for some languages
- Add Sierra ID to list of pseudo-tags
- Add another hand-formatted version of SS tag
- Move internal serial before volume label
- Check for $SystemUpdate folder for X360 discs
- Slightly rename UK and USA regions for UI
- Add another hand-formatted version of SS tag
- Add verification reminders for pulled tags
- Read longer string for Saturn internal serial
- Ensure version only pulled if one doesn't exist
- Ensure Games pseudo-tag is multi-line
- Add alternate pseudo-tag for Playable Demos
- Make error clearer if something is unsupported in Check
- Ensure drive is not null for volume labels
- Check explicitly for no matches on Redump pull
- Normalize PS1/PS2 executable names
- Adjust paths for DIC just before dumping

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
