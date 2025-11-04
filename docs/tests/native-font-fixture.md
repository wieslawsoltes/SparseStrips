# Native Font Fixture TODO

## Background
Phase 4 testing requires direct coverage of the font/glyph rendering APIs exposed by `Vello.Native.NativeMethods`. Those tests must drive `FontData_New`, `FontData_Free`, and the glyph rendering entry points via raw pointers, without managed wrappers.

## Status
- **Font Asset:** `Inter-Regular.ttf` (SIL OFL) downloaded from Google Fonts (`fonts.gstatic.com`) and stored at `dotnet/tests/Vello.Tests/TestAssets/fonts/Inter-Regular.ttf`. Attribution lives next to the binary (`Inter-Regular-OFL.txt`).
- **Glyph Plan:** The glyph test renders the ASCII string "AB" via direct glyph IDs computed from the font's cmap table.
- **Metrics:** Current assertion checks for the presence of dark glyph pixels on a white background; extend with stricter bounds if needed.
- **Platform Notes:** No platform-specific handling observed so far; font loads identically on macOS (ARM64) test host.
