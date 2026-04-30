# TODO

## Main UI

- Fix Log Output collapse box including dynamic resizing.
- Reduce Log Output collapse button vertical padding slightly.
- Fix dark mode.

## Notes

- Work stays in `MPF.Avalonia`; keep `MPF.UI` untouched.
- Current branch is `feature/avalonia-port`.
- `MainWindow` currently uses `MinWidth="750"`.
- The `Log Output` area still needs visual polish against the reference screenshots.
- We attempted custom section-header treatments earlier and intentionally reverted them; keep the simple in-box headers unless we revisit that on purpose.
