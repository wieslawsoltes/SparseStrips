# Vello Hybrid Binding Exploration Plan

## Current Architecture Observations
- `extern/vello/sparse_strips/vello_hybrid/src/scene.rs:90` defines `Scene`, which mirrors the CPU `RenderContext` API by turning peniko paints and geometry into sparse strips and alpha tiles while caching encoded paints and glyph data. This provides the stateful CPU half of the hybrid renderer that the bindings need to expose.
- `extern/vello/sparse_strips/vello_hybrid/src/render/wgpu.rs:129` exposes `Renderer::render`, requiring the caller to supply an initialized `wgpu::Device`, `Queue`, `CommandEncoder`, and output `TextureView`. The renderer prepares GPU resources (`Programs`) and schedules draws via a `Scheduler`.
- `extern/vello/sparse_strips/vello_hybrid/src/schedule.rs:404` drives the tile scheduler that maps `Scene`’s wide tiles into GPU work. Any FFI wrapper must ensure tight coupling between `Scene` generation and `Renderer` scheduling so cached strip data stays valid across frames.
- `dotnet/src/Vello.Native/NativeMethods.cs:11` and `dotnet/src/Vello/Core/RenderContext.cs:621` implement the CPU bindings that Avalonia currently consumes through `RenderToBuffer`. The hybrid story must deliver a comparable API while accommodating GPU surfaces instead of CPU buffers.
- `dotnet/src/Vello.Avalonia/Controls/VelloSurface.cs:98` shows the consumer pathway: render into a `RenderContext`, obtain RGBA pixels, and blit into an Avalonia `WriteableBitmap`. Transitioning to hybrid requires a new presentation path (swap chain, texture sharing, or GPU copy-back) without regressing this control.

## Binding Objectives
1. Expose `vello_hybrid` primitives over a C-compatible surface that closely follows the current CPU binding ergonomics for paints, paths, recordings, and resources.
2. Hide `wgpu` initialization complexity behind Rust-side handles so .NET code only needs lightweight descriptors (backend, surface source, optional adapter preferences).
3. Provide at least two output paths: (a) rendering directly into a platform surface for accelerated presentation, and (b) GPU-to-CPU read-back for fallback scenarios such as the existing `VelloSurface`.
4. Preserve parity with existing resource APIs (images, gradients, glyph caching) and make atlas configuration adjustable from managed code.

## Implementation Roadmap

### 1. Rust Hybrid FFI Crate
- Create a new `vello_hybrid_ffi` (`cdylib`) crate that depends on `vello_hybrid`, `wgpu`, and `wgpu-hal`, similar in structure to `vello_cpu_ffi`.
- Define opaque handle types for `HybridScene`, `HybridRenderer`, `HybridDeviceContext`, and `HybridSurface`.
- Re-export error handling helpers, version queries, and SIMD detection (reusing mechanisms from `vello_cpu_ffi`) to keep the managed layer consistent.

### 2. GPU Context & Surface Management
- Implement helpers to create and destroy a shared `wgpu::Instance`, pick adapters, and open devices/queues. Provide descriptor structs that the FFI marshals (backend, power preference, feature toggles).
- Support surface creation per platform: HWND/DXGI (Windows), CAMetalLayer/NSView (macOS), VkSurface (Linux), plus an off-screen texture path. Use conditional compilation to keep per-platform surface shims clean.
- Maintain a per-context default command encoder and staging buffers; expose explicit `BeginFrame`, `RenderScene`, and `EndFrame` calls so the managed layer can drive presentation timing.

### 3. Scene & Resource Interop
- Mirror the CPU FFI for paints, transforms, strokes, layers, recordings, and glyph APIs, but backed by `Scene` (`extern/.../scene.rs:90`). Reuse existing conversion utilities where possible to avoid divergence.
- Expose atlas configuration knobs via `AtlasConfig` (`extern/.../multi_atlas.rs:18`) and allow clients to upload images through FFI methods that call `Renderer::upload_image` (`extern/.../render/wgpu.rs:169`).
- Provide recording support that keeps cached strips valid across frames, coordinating the lifetime of `StripStorage` with the scheduler pipeline (`extern/.../schedule.rs:404`).

### 4. Rendering & Presentation Flow
- Add FFI entry points to:
  1. Attach/detach a surface to a renderer, creating swap-chain-like textures with proper size and format.
  2. Render a `Scene` into the active surface using `Renderer::render`, with protection against mismatched dimensions.
  3. Optionally read back into a CPU buffer by issuing a copy to a staging buffer and mapping it.
- Support dynamic resizing by reconfiguring textures and updating `RenderTargetConfig`, keeping `Scene::new_with` and `Renderer::new_with` in sync.

### 5. .NET Binding Layer
- Add a new P/Invoke module (e.g., `HybridNativeMethods`) in `dotnet/src/Vello.Native` modeled after the CPU bindings (`dotnet/src/Vello.Native/NativeMethods.cs:11`), including safe handle wrappers for scenes, renderers, and GPU contexts.
- Introduce managed wrappers (`HybridRenderContext`, `HybridRenderer`, `HybridSurface`) under `dotnet/src/Vello`, providing idiomatic disposal, Span-based APIs, and async-friendly frame submission helpers.
- Bridge existing higher-level constructs (paints, paths, recordings) by sharing structs/enums with the CPU path to minimize duplication.

### 6. Avalonia Integration
- Prototype a new control (e.g., `HybridVelloSurface`) that acquires the native window handle from Avalonia, forwards it to the hybrid renderer, and schedules GPU-presented frames. Fall back to copy-back mode when the platform handle is unavailable (e.g., Software compositor).
- Update `MotionMarkScene` consumers to toggle between CPU and hybrid renderers for performance benchmarking.
- Ensure render loop coordination respects Avalonia’s dispatcher thread and frame pacing so frames present without tearing.

### 7. Testing & Validation
- Extend Rust integration tests to cover scene encoding + renderer output using headless `wgpu` backends (GL, Vulkan, D3D12) where CI permits.
- Add C# interop tests mirroring `dotnet/tests/Vello.Tests/Interop/*` to validate paints, recordings, and resource uploads through the hybrid pipeline.
- Build stress tests for atlas growth, recording replay, and surface resizing; compare output hashes between CPU and hybrid paths for deterministic scenes.

## Risks & Open Questions
- Platform surface acquisition differs per OS; Avalonia may require new hooks to expose raw handles consistently.
- `wgpu` feature availability varies widely—need capability queries and graceful fallbacks, including a pure CPU path.
- GPU-to-CPU read-back may negate performance gains; consider staging textures or direct swap-chain presentation for primary scenarios.
- Synchronization around command submission and queue flushing must be carefully managed to avoid deadlocks when invoked from managed threads.

## Suggested Next Steps
1. Spike the `vello_hybrid_ffi` crate with minimal scene creation, device init, and off-screen rendering to a CPU buffer.
2. Draft the managed P/Invoke surface and a thin `HybridRenderContext` that can render the existing MotionMark scene via read-back.
3. Iterate on surface presentation and atlas/image interop, then optimize resource lifetimes and error handling before integrating into Avalonia UI.

## SkiaSharp GRContext Interop
- **Goal:** Allow Vello Hybrid to render into textures that a `SkiaSharp` GPU context (`GrDirectContext`) can consume without round-tripping through the CPU.
- **Current blockers:**
  - `wgpu` does not expose stable APIs for retrieving backend-native texture handles (`ID3D12Resource`, `VkImage`, `MTLTexture`). Access is possible only behind the `wgpu` `hal` feature gate and requires `unsafe` code with backend-specific downcasts.
  - Skia requires ownership details when wrapping external textures (memory allocation, queue family, layout transitions). `vello_hybrid` relies on `wgpu` for these responsibilities, so exporting a texture breaks encapsulation unless we mirror `wgpu-hal` internals.
  - Device/queue sharing is mandatory. Skia cannot simply “import” a texture from another device; both APIs must address the same underlying GPU device (e.g., the same `ID3D12Device`). `wgpu` today does not let us retrieve or inject an external device handle in a controlled fashion.
- **Viable near-term approach:** Render with Vello Hybrid into an off-screen `wgpu::Texture`, copy to a CPU staging buffer via `queue.read_texture`, then upload into a Skia bitmap/texture. This preserves correctness but adds a GPU→CPU→GPU copy.
- **Long-term exploration steps:**
  1. Prototype backend-specific interop for one platform (e.g., Vulkan): enable `wgpu`’s `hal` feature, call `Texture::as_hal::<wgpu_hal::vulkan::Api>`, and wrap the resulting `vk::Image` in a `SkiaSharp.SKImage` via `GRVkImageInfo`. Validate synchronization and lifetime management.
  2. Investigate whether `wgpu` roadmap includes explicit “external texture export/import” APIs. If so, align the FFI design to plug into those once stabilized.
  3. Consider inverting control: initialize `wgpu` from a Skia-owned device using the experimental `wgpu-hal` constructors so both libraries share ownership. This requires significant unsafe plumbing and per-backend feature checks.
- **Recommendation:** Treat SkiaSharp texture interop as an advanced, platform-specific enhancement. Ship hybrid bindings with the CPU read-back fallback first, then evaluate deeper GPU sharing once `wgpu` exposes the necessary hooks or a single-platform proof of concept demonstrates manageable complexity.
