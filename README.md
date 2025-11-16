# Vello Sparse Strips .NET Bindings

[![NuGet](https://img.shields.io/nuget/v/Vello.svg)](https://www.nuget.org/packages/Vello/)
[![NuGet](https://img.shields.io/nuget/v/Vello.Native.svg)](https://www.nuget.org/packages/Vello.Native/)
[![NuGet](https://img.shields.io/nuget/v/Vello.Avalonia.svg)](https://www.nuget.org/packages/Vello.Avalonia/)

Comprehensive .NET 8.0 bindings for the [Vello Sparse Strips](https://github.com/linebender/vello/tree/main/sparse_strips) CPU renderer with 100% API coverage, zero-allocation paths, and end-to-end tooling across native and managed layers.

---

## Highlights

- **Full surface area** – All 34 `RenderContext` methods, gradients, glyphs, masks, and blend modes implemented.
- **Modern .NET stack** – Source generators via `LibraryImport`, blittable structs, `Span<T>` APIs, and idiomatic `IDisposable` patterns.
- **High performance** – Zero-copy pixel access, SIMD (SSE2/AVX/AVX2/AVX512/NEON) detection, and multi-threaded render contexts.
- **Cross-platform** – Windows, Linux, and macOS across x64 and ARM64, plus experimental WebAssembly builds.
- **Verified quality** – 113 tests (unit, diagnostic, integration, performance) backed by BenchmarkDotNet harnesses.
- **Batteries included** – Fifteen sample apps, Avalonia UI host, and detailed docs for architecture, native builds, and FFI design.

---

## NuGet Packages

| Package | Description | Latest |
|---------|-------------|--------|
| `Vello` | Public, high-level C# API that surfaces the entire render pipeline. | [![NuGet](https://img.shields.io/nuget/v/Vello.svg)](https://www.nuget.org/packages/Vello/) |
| `Vello.Native` | Internal P/Invoke layer that loads the Rust FFI artifacts. | [![NuGet](https://img.shields.io/nuget/v/Vello.Native.svg)](https://www.nuget.org/packages/Vello.Native/) |
| `Vello.Avalonia` | Avalonia control + helpers for embedding live render contexts in .NET UI apps. | [![NuGet](https://img.shields.io/nuget/v/Vello.Avalonia.svg)](https://www.nuget.org/packages/Vello.Avalonia/) |

---

## Architecture Overview

```
SparseStrips/
├── extern/vello/              # Git submodule - Vello upstream repo
│   └── sparse_strips/
│       ├── vello_cpu/         # Core CPU renderer (Rust)
│       └── vello_common/      # Shared math/utilities
│
├── vello_cpu_ffi/             # Rust C-ABI FFI wrapper
│   ├── src/lib.rs             # Exported functions
│   └── build.rs               # Platform-specific build glue
│
├── dotnet/
│   ├── src/                   # Shipping managed packages
│   │   ├── Vello/             # Public API
│   │   ├── Vello.Native/      # Low-level bindings
│   │   └── Vello.Avalonia/    # UI integration helpers
│   ├── samples/               # 15 demos (CLI + UI)
│   └── tests/                 # Unit, diagnostics, perf, and integration suites
│
├── docs/                      # Design + status docs
└── scripts/                   # Platform build helpers
```

See `docs/FFI_DESIGN.md`, `docs/API_COVERAGE.md`, and `docs/STATUS.md` for deeper architecture notes.

---

## System Requirements

- Rust 1.86+ with the relevant targets (MSVC, GNU, Clang, wasm32) for native builds.
- .NET 8.0 SDK for managed projects, tests, and sample apps.
- Platform toolchains:
  - Windows: MSVC Build Tools or MinGW
  - Linux: GCC or Clang + `musl`/`glibc` dev packages as needed
  - macOS: Xcode Command Line Tools

---

## Installation

### Managed packages

Add the packages required by your application profile:

```bash
dotnet add package Vello
dotnet add package Vello.Avalonia     # Optional UI host
```

The `Vello.Native` package is pulled automatically as an implementation detail.

### Native artifacts

The bindings expect the `vello_cpu_ffi` dynamic library to sit next to your application binaries. Use the provided scripts (see [Building From Source](#building-from-source)) or ship the prebaked binaries produced by CI.

---

## Quick Start

```csharp
using Vello;
using Vello.Geometry;

// Create 800x600 render context
using var context = new RenderContext(800, 600);

// Set magenta paint
context.SetPaint(Color.Magenta);

// Draw filled rectangle
context.FillRect(Rect.FromXYWH(100, 100, 200, 150));

// Render to pixmap and inspect pixels
using var pixmap = new Pixmap(800, 600);
context.RenderToPixmap(pixmap);
ReadOnlySpan<PremulRgba8> pixels = pixmap.GetPixels();
Console.WriteLine($"First pixel: R={pixels[0].R}, G={pixels[0].G}, B={pixels[0].B}");
```

---

## User Guide

### 1. Prepare the native runtime

1. Initialize the Vello submodule (`git submodule update --init --recursive`).
2. Install Rust 1.86+ and platform targets (`rustup target add x86_64-pc-windows-msvc`, etc.).
3. Run the platform script from `scripts/` (e.g., `./scripts/build-linux.sh`). Each script produces both `Debug` and `Release` binaries under `vello_cpu_ffi/target/<triple>/<profile>/`.

Detailed notes for every platform live in `docs/native-build.md`, covering toolchain quirks, environment variables, and artifact locations.

### 2. Build and run the managed project

1. Restore and build the bindings via `dotnet build dotnet/src/Vello`.
2. Reference `Vello` (and optionally `Vello.Avalonia`) in your application.
3. Ensure the native binaries accompany your executable (copy to output folder or use `NativeLibrary.SetDllImportResolver` if you need custom probing).

### 3. Rendering workflow

1. **Configure** `RenderSettings` to pin SIMD level, thread count, and quality mode.
2. **Construct** a `RenderContext` sized for your output surface.
3. **Prepare** paints, gradients, glyph buffers, images, and masks—`Span<T>` APIs avoid heap allocations.
4. **Execute** draw commands (fill/stroke, clips, masks) in your desired order.
5. **Resolve** into a `Pixmap`, copy bytes, or stream PNG data.

Example configuration:

```csharp
var settings = new RenderSettings(
    level: SimdLevel.Avx2,
    numThreads: Environment.ProcessorCount,
    mode: RenderMode.OptimizeSpeed);

using var context = new RenderContext(1920, 1080, settings);
```

### 4. Avalonia integration

The `Vello.Avalonia` package exposes `VelloSurface`, a control that manages the render-context lifecycle, handles resize events, and blits frames into Avalonia bitmaps. Reference the namespace and bind a renderer:

```xml
<Window xmlns="https://github.com/avaloniaui"
        xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
        xmlns:vello="clr-namespace:Vello.Avalonia.Controls;assembly=Vello.Avalonia"
        x:Class="MyApp.MainWindow">
  <vello:VelloSurface Width="800"
                      Height="600"
                      Renderer="{Binding Renderer}"
                      UseMultithreadedRendering="True" />
</Window>
```

`Renderer` accepts any `IVelloRenderer` implementation. A minimal renderer might look like:

```csharp
using Vello;
using Vello.Avalonia.Rendering;
using Vello.Geometry;

public sealed class SimpleRenderer : IVelloRenderer
{
    public void Render(RenderContext context, int pixelWidth, int pixelHeight)
    {
        context.Clear();
        context.SetPaint(Color.FromRgba(0.15f, 0.18f, 0.24f, 1f));
        context.FillRect(Rect.FromXYWH(0, 0, pixelWidth, pixelHeight));

        Span<ColorStop> stops = stackalloc ColorStop[2];
        stops[0] = new ColorStop(0f, Color.DeepSkyBlue);
        stops[1] = new ColorStop(1f, Color.MediumPurple);
        context.SetPaintLinearGradient(0, 0, pixelWidth, pixelHeight, stops);

        var rect = Rect.FromXYWH(pixelWidth / 4f, pixelHeight / 4f, pixelWidth / 2f, pixelHeight / 2f);
        context.FillRect(rect);
    }
}
```

Expose an instance of `SimpleRenderer` (or a more advanced scene graph) through your view model and bind it to `Renderer`. See `dotnet/samples/Vello.Samples/Avalonia` for a full MVVM integration with live frame statistics.

---

## Building From Source

1. **Sync dependencies**

   ```bash
   git submodule update --init --recursive
   ```

2. **Build native libraries**

   ```powershell
   # Windows (PowerShell)
   .\scripts\build-windows.ps1
   ```

   ```bash
   # Linux / macOS / WebAssembly
   ./scripts/build-linux.sh
   ./scripts/build-macos.sh
   ./scripts/build-wasm.sh   # requires wasm-tools workload + Emscripten
   ```

3. **Build managed artifacts**

   ```bash
   dotnet build dotnet/Vello.sln
   ```

The scripts focus on `vello_cpu_ffi`. Use `dotnet publish` to ship self-contained applications. Refer to `docs/native-build.md` for manual toolchain setup and troubleshooting.

---

## Performance Profile

- **Zero-allocation text/gradient/glyph rendering** via aggressive `Span<T>` usage and stackalloc heuristics.
- **Zero-copy PNG I/O** – load from `ReadOnlySpan<byte>`, export into caller-provided buffers, and expose raw pixel bytes.
- **SIMD-aware rendering** – runtime detection plus explicit overrides for SSE2 through AVX512 and NEON.
- **Multi-threaded pipelines** – configurable worker pool for large surfaces.

```csharp
Span<ColorStop> stops = stackalloc ColorStop[3];
stops[0] = new ColorStop(0.0f, Color.Red);
stops[1] = new ColorStop(0.5f, Color.Green);
stops[2] = new ColorStop(1.0f, Color.Blue);
context.SetPaintLinearGradient(0, 0, 400, 300, stops);        // zero allocations

Span<Glyph> glyphs = stackalloc Glyph[text.Length];
int count = font.TextToGlyphs(text, glyphs);
context.FillGlyphs(font, 48.0f, glyphs.Slice(0, count));      // zero allocations
```

More details live in `docs/perf/` and `docs/IMPLEMENTATION_PLAN.md`.

---

## Platform Support

| Platform | x64 | ARM64 | Status |
|----------|-----|-------|--------|
| Windows  | ✅  | ✅    | Supported |
| Linux    | ✅  | ✅    | Supported |
| macOS    | ✅  | ✅    | Supported |
| WebAssembly | ⚙️  | ⚙️  | Experimental |

---

## Quality & Testing

- `dotnet/tests/Vello.Tests` – 85 unit tests (95%+ pass rate; remaining experiments tracked in `docs/tests`).
- `dotnet/tests/Vello.DiagnosticTests` – deep validation harness covering corner cases, threading, and disposal.
- `dotnet/tests/Vello.Benchmarks` – BenchmarkDotNet suites for frame time, glyph throughput, and PNG I/O.
- `dotnet/tests/Vello.IntegrationTest` – package validation and smoke tests executed before publishing.
- Native benchmarks in `rust_api_bench` and `docs/perf/` ensure parity with upstream Vello performance.

Run the full suite via:

```bash
dotnet test dotnet/Vello.sln
```

---

## Samples & Tooling

- `dotnet/samples/Vello.Samples` – 15 showcase scenarios (images, gradients, typography, masking).
- `dotnet/samples/Vello.Examples` – CLI-first examples suitable for CI or scripting.
- `dotnet/samples/MTTest` – multithreading exploration harness.
- `BENCHMARKS.md` – summary of recent perf investigations.

---

## Documentation Hub

- `docs/API_COVERAGE.md` – coverage matrix vs. upstream Vello APIs.
- `docs/FFI_DESIGN.md` and `docs/ffi-interop-guidelines.md` – FFI architecture, safety rules, and calling conventions.
- `docs/native-build.md` – native toolchain setup and artifact layout per platform.
- `docs/IMPLEMENTATION_PLAN.md` & `docs/implementation-plan-missing-apis.md` – roadmap and completed milestones.
- `docs/STATUS.md` – up-to-date delivery status, outstanding work, and risk log.
- `docs/tests/` – test plans and diagnostic methodologies.
- `docs/vello-hybrid-bindings-plan.md` & `docs/vello_cpu_recording_api_plan.md` – exploratory work for future bindings.

---

## Project Status

**Feature complete.** All implementation phases (FFI, .NET bindings, advanced features, and docs) are complete, with 34/34 `RenderContext` methods exposed. See `docs/STATUS.md` for the authoritative state and `docs/API_COVERAGE.md` for the line-by-line checklist.

---

## Contributing

Contributions are welcome—whether bug fixes, feature work, documentation, or performance experiments. Please open an issue to discuss significant changes, keep commits scoped, and ensure `dotnet format` + `dotnet test` pass locally. Native and managed guidelines live in `docs/FFI_DESIGN.md` and `docs/tests/`.

---

## License

Dual-licensed under Apache-2.0 OR MIT, matching upstream Vello.

- Apache License, Version 2.0 – see `LICENSE-APACHE` or <http://www.apache.org/licenses/LICENSE-2.0>
- MIT License – see `LICENSE-MIT` or <http://opensource.org/licenses/MIT>

---

## Acknowledgments

Built on top of [Vello Sparse Strips](https://github.com/linebender/vello/tree/main/sparse_strips) by the Linebender community and the contributors who created the original renderer. Thank you!
