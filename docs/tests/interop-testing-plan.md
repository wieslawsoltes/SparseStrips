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
- [x] Establish `InteropTests` project area under `dotnet/tests/Vello.Tests/Interop`.
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

> **Status Update (2025-02-14):** Direct interop tests now exercise invalid rect bounds, null-handle flush errors, and `Pixmap_DataMut` write-backs. Harden native safeguards for double-free contexts and zero-dimension pixmap resizes (currently aborting) before adding those negative-path tests. Details tracked in `docs/tests/native-safety-followups.md`.
>
> **Status Update (2025-02-14, later):** Added coverage for `Pixmap_Sample` (expected pixel retrieval plus null/out-of-range guards).

> **Phase 2 Progress (2025-02-14):** Added native-only coverage for linear/radial/sweep gradients (`RenderContext_SetPaint*Gradient`), extend modes (repeat/reflect, including diagonal spans), paint-kind queries, geometry & paint transform round-tripping plus observable paint effects (translation/scale/rotation), aliasing threshold clamping, and buffer rendering checks (`RenderContext_RenderToBuffer`, including pixmap parity). Sweep extend now cross-checked against the pad baseline; additional skew visuals remain pending until native fixtures are available.

> **Phase 3 Progress (2025-02-14):** Direct `BezPath` fills, stroke-path tests, recorder parity (stroke/transform/fill-rule), and layer stack coverage (opacity, additive blend, clip, mask) are in place. Remaining work: negative misuse cases (pop-without-push, freed handles) once native guards land.

> **Phase 4 Progress (2025-02-14):** Image paint coverage landed via `RenderContext_SetPaintImage` (solid pixmap source) with null-handle guards. Applying opacity to images currently triggers a native panic, so alpha-specific tests remain blocked until the engine supports that path.
>
> **Status Update (2025-02-15):** Inter-Regular.ttf added under `dotnet/tests/Vello.Tests/TestAssets/fonts/`; glyph rendering (`RenderContext_FillGlyphs`) now exercises cmap-derived glyph IDs. The FFI guards image opacity by returning `VELLO_ERROR_INVALID_PARAMETER`, and the managed tests assert the guard instead of panicking.

**Phase 1 Completion Checklist**
- [x] RenderContext construction/destruction happy path (`RenderContextPixmapInteropTests`).
- [x] Dimension queries (`RenderContext_Width/Height`) and zero-dimension context behaviour.
- [x] Reset lifecycle, flush, fill, and render-to-pixmap positives and null-handle negatives.
- [x] Pixmap allocation, resize (positive), raw data/data_mut access, sampling, and null-pointer guards.
- [ ] Negative tests that require native safeguards (double-free, zero-dimension resize) – **blocked** pending native fixes (`docs/tests/native-safety-followups.md`).

**Phase 2 Completion Checklist**
- [x] Linear, radial, and sweep gradients positive coverage (including extend repeat/reflect + diagonal spans).
- [x] Gradient negative cases (null stops pointer, stop-count < 2) for linear/radial variants.
- [x] Transform APIs (set/get/reset + paint transforms with observable translation/scale/rotation effects).
- [x] Paint kind queries after various paint changes.
- [x] Aliasing threshold setter (range, clamp, null context error).
- [x] Render-to-buffer positive path and negative cases (null pointer, undersized buffer) plus pixmap parity check.
- [x] Additional gradient extend scenarios and transform edge cases (sweep extend verified against pad baseline; paint transform effects covered via translation/scale/rotation comparisons. Further skew visuals deferred pending native fixtures).

**Phase 3 Kickoff Checklist**
- [x] Direct `BezPath` construction + fill-path rendering (triangle fill, null pointer guards).
- [x] Recording lifecycle: record via native callback, prepare/execute, pixel parity with direct rendering, `Recording_Len` assertions.
- [x] Layer basics: opacity, blend (compose plus), clip, and mask layers validated via pixel sampling and guard checks.
- [x] Stroke-path coverage and recorder stroke operations.
- [x] Recorder transform and stroke parity with direct context APIs.
- [x] Recorder fill-rule parity checks.
- [ ] Pop-without-push / misuse error assertions (await native guardrails).

**Phase 4 Progress Checklist**
- [x] Image paints via `Image_NewFromPixmap`/`RenderContext_SetPaintImage` (solid source pixmaps) with null-handle guards.
- [ ] Image opacity/scaling/stretch validation (native support still pending beyond the guard).
- [x] Image opacity guard added (FFI now returns `VELLO_ERROR_INVALID_PARAMETER` for alpha < 1).
- [x] Image extend modes (repeat/reflect) validated against expected pixel patterns.
- [x] Mask luminance flow validated.
- [x] Glyph rendering via `RenderContext_FillGlyphs` using Inter-Regular.ttf (cmap-parsed glyph IDs).
- [x] Font negative paths (null data pointer, null font handle, null glyph pointer) covered via `RenderContextFontsInteropTests`.

**Phase 5 Progress Checklist**
- [x] Version/Simd detection sanity checks (`Version`, `SimdDetect`).
- [x] Error handling validation (`GetLastError`, `ClearLastError`).
- [ ] Remaining utilities (ensure final sweep once new APIs land).

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
- ✅ Phase 3 feature coverage complete; awaiting native guardrails for misuse negatives (pop-without-push, freed handles).
- ✅ Phase 4 image/mask/font coverage in place (opacity/scaling positives still blocked by native limitations).
- ✅ Phase 5 diagnostics (`Version`, `SimdDetect`, `GetLastError`, `ClearLastError`) covered; remaining utilities to re-evaluate when new APIs arrive.

> Next concrete task: build the resource guard helpers and start Phase 1 detailed coverage (RenderContext + Pixmap). Once helpers are stable, progress through the phases systematically.
