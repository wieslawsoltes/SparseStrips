# Vello CPU Benchmarks: Rust vs .NET

This document describes the comprehensive benchmarking suite comparing the native Rust `vello_cpu` API with the .NET managed bindings.

## Overview

We have created identical benchmarks for both Rust and .NET implementations covering all major public API features:

- **Path Rendering** - Fill/stroke rectangles, simple paths, complex curved paths
- **Gradients** - Linear and radial gradients with multiple color stops
- **Transforms** - Affine transformations (rotation, scale, translation)
- **Blending & Compositing** - Mix modes, compose modes, opacity layers
- **Clipping** - Path-based clipping layers
- **Effects** - Blurred rounded rectangles
- **Complex Scenes** - Multi-element rendering scenarios

Each benchmark is run in two configurations:
1. **Single Thread** (`numThreads=0`) - Sequential execution
2. **Multi Thread 8T** (`numThreads=8`) - Parallel execution with 8 worker threads

## Benchmark Locations

### Rust Benchmarks
- **Location**: `/Users/wieslawsoltes/GitHub/SparseStrips/rust_api_bench/`
- **Framework**: Criterion (Rust standard benchmarking framework)
- **Features**: HTML reports, statistical analysis, comparison mode

### .NET Benchmarks
- **Location**: `/Users/wieslawsoltes/GitHub/SparseStrips/dotnet/Vello.Benchmarks/`
- **Framework**: BenchmarkDotNet (industry-standard .NET benchmarking)
- **Features**: Memory diagnostics, statistical analysis, multiple runtime support

## Running the Benchmarks

### Prerequisites

**Rust:**
- Rust 1.86+ toolchain
- Cargo

**.NET:**
- .NET 8.0 SDK
- Release build of native library

### Rust Benchmarks

```bash
cd /Users/wieslawsoltes/GitHub/SparseStrips/rust_api_bench

# Run all benchmarks
cargo bench

# Run specific benchmark
cargo bench fill_rect

# Run with custom iterations
cargo bench -- --sample-size 50
```

**Output Location**: `target/criterion/`

### .NET Benchmarks

```bash
cd /Users/wieslawsoltes/GitHub/SparseStrips/dotnet

# Run all benchmarks
dotnet run --project Vello.Benchmarks/Vello.Benchmarks.csproj -c Release

# Run specific category
dotnet run --project Vello.Benchmarks/Vello.Benchmarks.csproj -c Release -- --filter *FillRect*

# Run with memory diagnostics
dotnet run --project Vello.Benchmarks/Vello.Benchmarks.csproj -c Release -- --memory
```

**Output Location**: `BenchmarkDotNet.Artifacts/results/`

## Benchmark Categories

### 1. Path Rendering

| Benchmark | Description | Canvas Size |
|-----------|-------------|-------------|
| `fill_rect` / `FillRect` | Simple rectangle fill | 800x600 |
| `stroke_rect` / `StrokeRect` | Rectangle stroke with round caps | 800x600 |
| `fill_path_simple` / `FillPathSimple` | Triangle path | 800x600 |
| `fill_path_complex` / `FillPathComplex` | Bezier curves with complex shapes | 800x600 |
| `stroke_path_complex` / `StrokePathComplex` | Curved path with 8px stroke | 800x600 |

### 2. Gradients

| Benchmark | Description | Canvas Size |
|-----------|-------------|-------------|
| `linear_gradient` / `LinearGradient` | 3-stop linear gradient | 800x600 |
| `radial_gradient` / `RadialGradient` | 3-stop radial gradient | 800x600 |

### 3. Transforms

| Benchmark | Description | Canvas Size |
|-----------|-------------|-------------|
| `transforms` / `Transforms` | 5 rectangles with rotation transform | 800x600 |

### 4. Blending & Compositing

| Benchmark | Description | Canvas Size |
|-----------|-------------|-------------|
| `blend_modes` / `BlendModes` | Multiply blend mode, 2 overlapping rects | 800x600 |
| `opacity_layer` / `OpacityLayer` | 50% opacity layer | 800x600 |

### 5. Clipping

| Benchmark | Description | Canvas Size |
|-----------|-------------|-------------|
| `clip_layer` / `ClipLayer` | Circular clip path (32-sided polygon) | 800x600 |

### 6. Effects

| Benchmark | Description | Canvas Size |
|-----------|-------------|-------------|
| `blurred_rounded_rect` / `BlurredRoundedRect` | 20px radius, 10px blur | 800x600 |

### 7. Complex Scenes

| Benchmark | Description | Canvas Size |
|-----------|-------------|-------------|
| `complex_scene` / `ComplexScene` | Gradient + 10 shapes (20 elements) | 800x600 |

## Metrics Collected

### Rust (Criterion)

- **Time**: Mean, median, standard deviation
- **Throughput**: Operations per second
- **Outliers**: Mild and severe outliers detection
- **Comparison**: Against previous runs (if available)

### .NET (BenchmarkDotNet)

- **Time**: Mean, median, standard deviation, min/max
- **Memory**: Gen0/Gen1/Gen2 collections, allocated bytes
- **Throughput**: Operations per second
- **Statistical Analysis**: R², confidence intervals

## Expected Performance Characteristics

### Single-Threaded

Rust should show marginally better performance due to:
- Zero-cost abstractions
- No GC overhead
- Direct memory access

.NET should be competitive due to:
- Zero-copy Span<T> APIs
- Blittable P/Invoke
- Modern JIT optimizations

### Multi-Threaded (8T)

Both should show significant speedup:
- Rust: Using rayon work-stealing
- .NET: Native library handles threading

Performance gap should narrow as thread overhead dominates.

## Memory Comparison

### Rust

- **Stack Allocation**: All intermediate buffers
- **Zero Allocations**: For typical operations
- **Memory Safety**: Compile-time guarantees

### .NET

- **Managed Heap**: GC-managed allocations
- **Span<T> Optimizations**: Zero-copy for Phases 1 & 2
- **P/Invoke Overhead**: Minimal with blittable types
- **Expected Allocations**:
  - Text rendering ≤256 chars: 0 allocations (Phase 1)
  - Gradients ≤32 stops: 0 allocations (Phase 1)
  - Path operations: Some managed wrapper allocations
  - Context/Pixmap: IDisposable pattern

## Interpreting Results

### Performance Comparison

**Rust faster by <20%**: Excellent parity, .NET bindings are high-quality
**Rust faster by 20-50%**: Expected, managed overhead is reasonable
**Rust faster by >50%**: Investigate .NET implementation for inefficiencies

### Memory Comparison

**Key Metrics**:
- **Gen0 Collections**: Should be minimal with Span<T> optimizations
- **Allocated Bytes**: Should be low for simple operations
- **Native Memory**: Both use same native vello_cpu library

### Throughput

**Operations/sec** should scale linearly with thread count up to available cores.

## Continuous Benchmarking

To track performance over time:

```bash
# Rust - Criterion stores baseline
cd rust_api_bench
cargo bench --save-baseline main
# After changes
cargo bench --baseline main

# .NET - BenchmarkDotNet comparison mode
cd dotnet
dotnet run --project Vello.Benchmarks -c Release -- --baseline
# After changes
dotnet run --project Vello.Benchmarks -c Release
```

## Troubleshooting

### Rust Benchmarks Not Running

```bash
# Ensure vello submodule is initialized
git submodule update --init --recursive

# Check Cargo.toml dependencies
cd rust_api_bench
cargo check
```

### .NET Benchmarks Fail

```bash
# Rebuild native library first
cd /Users/wieslawsoltes/GitHub/SparseStrips
./build.sh

# Then rebuild benchmarks
cd dotnet
dotnet build Vello.Benchmarks -c Release
```

### Performance Anomalies

- **Thermal throttling**: Run benchmarks with cooling breaks
- **Background processes**: Close unnecessary applications
- **Power mode**: Use AC power, disable power saving
- **CPU frequency**: Check governor settings (Linux)

## Benchmark Code Structure

### Rust (`rust_api_bench/benches/api_benchmark.rs`)

```rust
fn bench_fill_rect(c: &mut Criterion) {
    let mut group = c.benchmark_group("fill_rect");

    group.bench_function("single_thread", |b| {
        let mut ctx = RenderContext::new_with(WIDTH, HEIGHT, single_threaded_settings());
        let mut pixmap = Pixmap::new(WIDTH, HEIGHT);

        b.iter(|| {
            ctx.reset();
            ctx.set_paint(css::MAGENTA);
            ctx.fill_rect(black_box(&rect));
            ctx.flush();
            ctx.render_to_pixmap(&mut pixmap);
        });
    });

    // ... multi_thread_8T variant
}
```

### .NET (`dotnet/Vello.Benchmarks/ApiBenchmarks.cs`)

```csharp
[Benchmark(Description = "Fill Rectangle - Single Thread")]
[BenchmarkCategory("FillRect")]
public void FillRect_SingleThread()
{
    using var ctx = new RenderContext(SmallWidth, SmallHeight, new RenderSettings(
        level: SimdLevel.Avx2,
        numThreads: 0,
        mode: RenderMode.OptimizeSpeed
    ));
    using var pixmap = new Pixmap(SmallWidth, SmallHeight);

    ctx.SetPaint(Color.Magenta);
    ctx.FillRect(rect);
    ctx.Flush();
    ctx.RenderToPixmap(pixmap);
}

// ... MultiThread8T variant
```

## Contributing Benchmarks

To add new benchmarks:

1. Add to Rust: `rust_api_bench/benches/api_benchmark.rs`
2. Add to .NET: `dotnet/Vello.Benchmarks/ApiBenchmarks.cs`
3. Keep implementations identical
4. Update this documentation

## Example Results Format

### Rust (Criterion)

```
fill_rect/single_thread time:   [4.2345 ms 4.2567 ms 4.2801 ms]
Found 3 outliers among 100 measurements (3.00%)
  2 (2.00%) high mild
  1 (1.00%) high severe

fill_rect/multi_thread_8T time: [1.2345 ms 1.2456 ms 1.2578 ms]
                        change: [-70.123% -70.000% -69.876%] (p = 0.00 < 0.05)
                        Performance has improved.
```

### .NET (BenchmarkDotNet)

```
|                      Method |     Mean |    Error |   StdDev | Ratio | Gen0 | Allocated |
|---------------------------- |---------:|---------:|---------:|------:|-----:|----------:|
|   FillRect_SingleThread     | 4.356 ms | 0.045 ms | 0.042 ms |  1.00 |    - |     152 B |
|   FillRect_MultiThread8T    | 1.287 ms | 0.012 ms | 0.011 ms |  0.30 |    - |     184 B |
```

## Summary

This benchmarking suite provides comprehensive performance and memory comparison between Rust and .NET implementations, enabling:

- **Validation**: Ensures .NET bindings have acceptable performance
- **Optimization**: Identifies performance bottlenecks
- **Regression Detection**: Catches performance degradations
- **Documentation**: Demonstrates performance characteristics to users

Run these benchmarks regularly to maintain high-quality .NET bindings!
