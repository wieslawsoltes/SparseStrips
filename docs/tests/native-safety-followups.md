# Native FFI Safety Follow-ups

> Last updated: 2025-02-14  
> Owner: Interop team → coordinate with `vello_cpu_ffi` maintainers

## Context
While extending the managed interop harness (Phase 1), we observed that a few error paths still terminate the native runtime instead of reporting structured `VELLO_*` error codes. These cases block additional negative-path tests because xUnit terminates the process when the native side aborts.

The issues appear to originate in Rust-side assertions over pointer lifetimes (primarily `slice::from_raw_parts_mut` and `smallvec` bounds). We need explicit guardrails in the C ABI before we can finish test coverage.

## Issues to Address

### 1. Double Free / Use-After-Free on Pixmap Handles
- **Repro (C#):**
  ```csharp
  nint ctx = NativeMethods.RenderContext_New(32, 32);
  nint pixmap = NativeMethods.Pixmap_New(32, 32);
  NativeMethods.Pixmap_Free(pixmap);
  NativeMethods.RenderContext_RenderToPixmap(ctx, pixmap); // → abort
  ```
- **Observed Native Failure:** `thread '<unnamed>' panicked ... slice::from_raw_parts_mut requires the pointer to be aligned and non-null`.
- **Expected Contract:** Return `VELLO_ERROR_INVALID_HANDLE` (or similar) when the pixmap handle is no longer valid.
- **Ask:** Insert a handle validation check before dereferencing pixmap data in `render_to_pixmap` (and any other entry points that touch pixmap internals). Managed side will then assert on the error code.

### 2. Double Free on RenderContext Handles
- **Repro (C#):**
  ```csharp
  var ctx = NativeMethods.RenderContext_New(32, 32);
  NativeMethods.RenderContext_Free(ctx);
  NativeMethods.RenderContext_Free(ctx); // → abort
  ```
- **Observed Native Failure:** Same panic pattern as above (freed pointer reused).
- **Expected Contract:** Swallow or report `VELLO_ERROR_INVALID_HANDLE` when `*_Free` is called on an already-freed handle.
- **Ask:** Either make the `*_Free` functions idempotent (set internal pointer to null) or return a deterministic error instead of panicking.

### 3. Zero-Dimension Pixmap Resize
- **Repro (C#):**
  ```csharp
  var pixmap = NativeMethods.Pixmap_New(32, 32);
  NativeMethods.Pixmap_Resize(pixmap, 0, 32); // → abort
  ```
- **Observed Native Failure:** `smallvec` panic when allocating a zero-sized buffer.
- **Expected Contract:** Return `VELLO_ERROR_INVALID_PARAMETER` for zero width/height.
- **Ask:** Clamp or validate requested dimensions before reallocating backing storage.

## Suggested Fix Strategy
1. Add explicit validation to `vello_pixmap_resize`, `vello_render_context_render_to_pixmap`, and the relevant `*_free` implementations.
2. Convert panic paths into well-defined error codes (likely `VELLO_ERROR_INVALID_HANDLE` or `VELLO_ERROR_INVALID_PARAMETER`).
3. Optionally extend the C ABI with `*_IsValid` helpers if lifetime tracking becomes non-trivial.

## Next Steps for Managed Harness
Once the above safeguards are merged:
- Enable negative-path tests for double-free contexts/pixmaps (`RenderContext_RenderToPixmap` with freed handles, repeated `*_Free` calls).
- Re-enable resize-with-zero tests and assert on `VELLO_ERROR_INVALID_PARAMETER`.
- Update documentation to reflect the strengthened contracts.
