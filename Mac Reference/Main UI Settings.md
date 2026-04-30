# Mac Reference: Main UI Settings

Snapshot date: 2026-04-29
Branch: `feature/avalonia-port`

This file captures the current macOS-tuned Main UI settings for `MPF.Avalonia` so we can restore or compare them later if Windows/Linux changes drift the Mac layout.

## Main Window

File: `MPF.Avalonia/Windows/MainWindow.axaml`

- `MinWidth="750"`
- `Width="700"`
- `Height="780"`
- Root border: `Padding="12"`
- Root grid: `RowSpacing="10"`

### Main sections

- `Settings` panel: `Padding="10,12"`
- `Controls` panel: `Padding="10,12"`
- `Status` panel: `Padding="10,12"`

### Settings grid

- `ColumnDefinitions="130,*"`
- `ColumnSpacing="24"`
- `RowSpacing="8"`

### Controls row

- Buttons use `Width="160"`
- Buttons use `Padding="5,2"`
- Controls row `Spacing="12"`

### Log Output header

- `Expander` name: `LogPanel`
- Header text margin: `Margin="10,0,0,0"`
- Header text vertical alignment: `VerticalAlignment="Center"`

## Main Window macOS override

File: `MPF.Avalonia/Windows/MainWindow.axaml.cs`

Inside `ConfigurePlatformMenus()`:

- In-window top menu is hidden on macOS
- Root border macOS padding override:
  - `new Thickness(12, 2, 12, 6)`

This is the current Mac-specific outer spacing baseline.

## Log Output control

File: `MPF.Avalonia/UserControls/LogOutput.axaml`

### Black console area

- `Height="220"`
- `Margin="0,0,0,2"`
- `CornerRadius="0,0,0,0"`

### Log text

- Items host top margin: `Margin="0,10,0,0"`
- Per-line text margin: `Margin="10,0,0,2"`
- Font: `Consolas, Menlo, monospace`
- Font size: `11`

### Footer/button area

- Outer grid `RowSpacing="4"`
- Footer wrapper padding: `Padding="0,0,0,4"`
- Button row margin: `Margin="0,-2,0,0"`
- Button row spacing: `16`
- `Clear Log` / `Save Log` width: `110`

## Dynamic log resize

Files:

- `MPF.Avalonia/UserControls/LogOutput.axaml.cs`
- `MPF.Avalonia/Windows/MainWindow.axaml.cs`

Current behavior:

- Default black console height constant: `220`
- When `Log Output` is expanded and the user drags the main window taller, the black console grows with the extra height
- When `Log Output` is collapsed, the console resets to default height

## Shared styles that affect Main UI

File: `MPF.Avalonia/App.axaml`

- `Button`:
  - `MinHeight="28"`
  - `Padding="14,6"`
  - `CornerRadius="5"`
- `TextBox`:
  - `MinHeight="28"`
  - `VerticalContentAlignment="Center"`
- `ComboBox`:
  - `MinHeight="28"`
  - `Padding="10,0,28,0"`
  - `VerticalContentAlignment="Center"`
- `EnableParametersCheckBox` toggle:
  - `Width="28"`
  - `Height="28"`
  - Gray checked state

## Notes

- This is a reference snapshot, not an active config source.
- If future Windows/Linux changes alter the Mac look, compare against this file first.
