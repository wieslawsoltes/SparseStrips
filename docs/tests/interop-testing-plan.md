# Vello Interop Testing Plan

_Last updated: 2025-02-14_

## Vision
- Achieve **100 % coverage of the `Vello.Native.NativeMethods` API surface** by calling the native entry points directly (no managed convenience wrappers).
- Build a reusable test harness that owns native resources, asserts result codes, and inspects raw outputs (pixels, masks, buffers, etc.).
- Keep tests deterministic, isolated, and fast to run in CI (small pixmaps, fixed seeds, narrow tolerances).

## Strategy
We will proceed in **five phases**, grouped by API families. Each phase introduces:
1. A thin helper shim to manage native resources (allocated via `NativeMethods` only).
2. Positive-path tests that drive the APIs and assert deterministic outcomes.
3. Negative-path tests that deliberately violate invariants and confirm error codes (`VELLO_*`).
4. Cross-checks between related APIs (e.g., render context + recorder, pixmap + mask).

> **Test Style:** All tests must call `NativeMethods` directly. Helpers may wrap pointer choreography (e.g., RAII structs disposing `RenderContext_Free`, `Pixmap_Free`), but must not rely on managed Vello wrappers (no `RenderContext`, `Pixmap`, `Recording`, etc.).

---

## Phase 0 – Harness Foundations (Done / Review)
- [x] Establish `InteropTests` project area under `dotnet/Vello.Tests/Interop`.
- [x] Define `NativeTestHelpers` for context/pixmap allocation (direct `NativeMethods` usage).
- [x] Add first smoke tests (render solid fill, recording round-trip) to validate harness.

> ✅ Completed earlier work; keep helpers updated as new resource types appear.

---

## Phase 1 – Core RenderContext & Pixmap APIs
**Coverage Target:** Rendering lifecycle, basic paint, pixmap accessors.

| API | Positive Tests | Negative Tests | Notes |
| --- | --- | --- | --- |
| `RenderContext_New`, `_Free` | Construct + dispose multiple contexts | Double-free, zero dimensions | Ensure no leaks (valgrind/manual) |
| `RenderContext_Width/Height` | Assert values match creation parameters | Query after free (expect error) | |
| `RenderContext_Reset` | Issue fill, reset, reissue (pixels empty) | Reset null context | |
| `RenderContext_SetPaintSolid`, `_FillRect`, `_Flush`, `_RenderToPixmap` | Solid-color diff vs expected pixel array | Null pixmap, out-of-range rect | |
| `Pixmap_New`, `_Free`, `_Width`, `_Height`, `_Resize` | Resize up/down, verify bounds | Resize with zero or huge dims | |
| `Pixmap_Data`, `_DataMut` (if available) | Inspect raw bytes, confirm layout | Null pointer retrieval | |

**Artifacts:** Pixel comparison utilities that work with raw `PremulRgba8` payloads.

---

## Phase 2 – Paints, Gradients, Transforms, Aliasing
**Coverage Target:** All `RenderContext_SetPaint*`, transform APIs, paint kind queries.

| API | Positive Tests | Negative Tests |
| --- | --- | --- |
| `SetPaintLinearGradient`, `Radial`, `Sweep` | Render small pixmaps, sample anchor pixels, verify gradients | Provide ≤1 stops, invalid radius/angles |
| `RenderContext_Get/ResetTransform`, `SetTransform` | Apply translation/rotation matrix, draw shape, verify pixel shift | Set null pointer, invalid matrix (NaNs) |
| `RenderContext_SetPaintTransform`, `_GetPaintTransform`, `_ResetPaintTransform` | Apply scaling to gradient, confirm scaled effect | Reset with null context |
| `RenderContext_GetPaintKind` | Assert correct enum returned for each paint | Query after clearing paint |
| `RenderContext_SetAliasingThreshold` | Set threshold values, ensure property retains state | Pass values >255 or negative (other than sentinel) |
| `RenderContext_RenderToBuffer` | Render to byte buffer, validate length & sample data | Buffer too small (expect error) |

**Helpers:** gradient sampling functions (e.g., expected color from linear interpolation); matrix comparers.

---

## Phase 3 – Paths, Recorder, Recording, Blend/Layer Stack
**Coverage Target:** All `BezPath_*`, recorder methods, layer/mask operations.

| API Group | Positive Tests | Negative Tests |
| --- | --- | --- |
| `BezPath_New/MoveTo/LineTo/QuadTo/CurveTo/Close/Clear` | Build shapes directly, link with `RenderContext_FillPath/StrokePath` | Call operations on freed path, invalid sequence (e.g., LineTo before MoveTo) |
| `RenderContext_FillPath`, `StrokePath` | Compare direct vs recorded pixel output | Null path handles |
| `Recording_New/Record/Prepare/Execute/Len/Clear` | Record multi-op scene, ensure len increments, clear resets | Execute after free, record with null callback |
| `Recorder_*` methods | Use recorder to fill/stroke paths, set paint solid | Null recorder pointer, invalid path handle |
| `RenderContext_PushLayer/PopLayer`, `PushClipLayer`, `PushOpacityLayer`, `PushMaskLayer` | Compose layers (blend, opacity, mask) and check resulting pixels | Pop without push, mask size mismatch, null mask |

**Additional:** Validate `RenderContext_SetFillRule`/`GetFillRule` across recorded and direct renders.

---

## Phase 4 – Masks, Images, Fonts, Glyph Rendering
**Coverage Target:** Remaining resource-heavy APIs.

| API | Positive Tests | Negative Tests |
| --- | --- | --- |
| `Mask_NewAlpha`, `Mask_NewLuminance`, `Mask_GetWidth/Height`, `Mask_Free` | Create masks from pixmaps, push mask layer, confirm masked output | Free mask twice, use after free |
| `Image_NewFromPixmap`, `SetPaintImage`, `Image_Free` | Render pixmap, reuse as image paint, confirm results | Null pixmap, invalid extend enum |
| `FontData_New`, `FontData_Free`, glyph APIs | Load tiny font blob, render glyphs through native path, sample pixel bounds | Invalid font data (expect error) |
| Text helpers (`RenderContext_FillGlyphs`, etc.) | Optional: once deterministic glyph set is available |

**Fixtures Needed:** Small font binary, reference mask/pixmap assets committed under `Vello.Tests/TestAssets`.

---

## Phase 5 – Versioning, SIMD, Diagnostics & FFI Utilities
**Coverage Target:** Single-call utilities and error introspection.

| API | Positive Tests | Negative Tests |
| --- | --- | --- |
| `Version`, `SimdDetect` | Ensure version string non-null, SIMD detects expected level (allow fallback) | N/A (utilities) |
| `GetLastError`, `ClearLastError` | Trigger known error, verify error string, ensure clear resets | Call clear without prior error (should be no-op) |
| Remaining utilities (e.g., `RenderContext_Reset`, `RenderContext_ResetTransform`) | Already covered in earlier phases—ensure overlap |

**Finalization:** Confirm every `NativeMethods` entry has at least one direct call in the suite (positive and/or negative).

---

## Test Harness Guidelines
- **Resource Guards:** Introduce small `SafeHandle`-like structs (internal to tests) that wrap `IntPtr` and call the corresponding `NativeMethods_*_Free` in `Dispose()`. This keeps tests short while obeying the “native-only” rule.
- **Error Assertions:** Compare returned `int` values to `NativeMethods.VELLO_OK`, capture `GetLastError()` for deeper diagnostics, and clear explicitly after each failure.
- **Pixel Utilities:** Keep pixmaps small (≤32×32) to minimize per-test cost. Introduce helper functions for sampling edges, centres, and verifying gradients/strokes.
- **Randomness:** Use deterministic seeds (`Random(42)`) whenever randomness is unavoidable.
- **Parallelism:** Disable test parallelization for interop fixtures if native library is not thread-safe in current harness (xUnit `[CollectionDefinition]`).

---

## Tracking & Reporting
- Maintain a checklist mapping each `NativeMethods` function to its test cases (table under `docs/tests/native-methods-coverage.md` – to be added).
- Enforce coverage via CI script that scans test assemblies for direct P/Invoke usage (e.g., reflection to confirm method tokens are invoked).
- Update this plan whenever new APIs are added or discovery reveals missing edge cases.

---

## Current Status Snapshot
- ✅ Phases 0–2 implemented.
- ✅ Phase 3 (initial recording/layer coverage) underway; masks/images/fonts outstanding.
- ⚙️ Phase 4 requires assets and deterministic glyph fixtures.
- ⚙️ Phase 5 pending version/SIMD/error-string assertions.

> Next concrete task: build the resource guard helpers and start Phase 1 detailed coverage (RenderContext + Pixmap). Once helpers are stable, progress through the phases systematically.
