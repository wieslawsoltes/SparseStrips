// Copyright 2025 Wieslaw Soltes
// SPDX-License-Identifier: Apache-2.0 OR MIT

using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;
using Vello;
using Vello.Geometry;

namespace Vello.Benchmarks;

/// <summary>
/// Overhead benchmarks for Vello .NET bindings
/// Measures the cost of fundamental operations:
/// - Context creation (single-threaded and multi-threaded)
/// - Pixmap creation
/// - Flush operation
/// </summary>
[MemoryDiagnoser]
[SimpleJob(RuntimeMoniker.Net80)]
public class VelloOverheadBenchmarks
{
    private const ushort Width = 800;
    private const ushort Height = 600;

    // ========================================================================
    // Context Creation Benchmarks
    // ========================================================================

    [Benchmark(Description = "Context Creation - Single Thread")]
    public void ContextCreation_SingleThread()
    {
        using var ctx = new RenderContext(Width, Height, new RenderSettings(
            level: SimdLevel.Avx2,
            numThreads: 0,
            mode: RenderMode.OptimizeSpeed
        ));
    }

    [Benchmark(Description = "Context Creation - Multi Thread 8T")]
    public void ContextCreation_MultiThread8T()
    {
        using var ctx = new RenderContext(Width, Height, new RenderSettings(
            level: SimdLevel.Avx2,
            numThreads: 8,
            mode: RenderMode.OptimizeSpeed
        ));
    }

    // ========================================================================
    // Pixmap Creation Benchmarks
    // ========================================================================

    [Benchmark(Description = "Pixmap Creation - 800x600")]
    public void PixmapCreation_800x600()
    {
        using var pixmap = new Pixmap(800, 600);
    }

    [Benchmark(Description = "Pixmap Creation - 1920x1080")]
    public void PixmapCreation_1920x1080()
    {
        using var pixmap = new Pixmap(1920, 1080);
    }

    [Benchmark(Description = "Pixmap Creation - 3840x2160")]
    public void PixmapCreation_3840x2160()
    {
        using var pixmap = new Pixmap(3840, 2160);
    }

    // ========================================================================
    // Flush Benchmarks (with context reuse)
    // ========================================================================

    private RenderContext? _ctxST;
    private RenderContext? _ctx8T;

    [GlobalSetup]
    public void Setup()
    {
        _ctxST = new RenderContext(Width, Height, new RenderSettings(
            level: SimdLevel.Avx2,
            numThreads: 0,
            mode: RenderMode.OptimizeSpeed
        ));

        _ctx8T = new RenderContext(Width, Height, new RenderSettings(
            level: SimdLevel.Avx2,
            numThreads: 8,
            mode: RenderMode.OptimizeSpeed
        ));
    }

    [GlobalCleanup]
    public void Cleanup()
    {
        _ctxST?.Dispose();
        _ctx8T?.Dispose();
    }

    [Benchmark(Description = "Flush - Single Thread (Empty)")]
    public void Flush_SingleThread_Empty()
    {
        var ctx = _ctxST!;
        ctx.Reset();
        ctx.Flush();
    }

    [Benchmark(Description = "Flush - Multi Thread 8T (Empty)")]
    public void Flush_MultiThread8T_Empty()
    {
        var ctx = _ctx8T!;
        ctx.Reset();
        ctx.Flush();
    }

    [Benchmark(Description = "Flush - Single Thread (With Rect)")]
    public void Flush_SingleThread_WithRect()
    {
        var ctx = _ctxST!;
        var rect = Rect.FromXYWH(100, 100, 400, 300);

        ctx.Reset();
        ctx.SetPaint(Color.Magenta);
        ctx.FillRect(rect);
        ctx.Flush();
    }

    [Benchmark(Description = "Flush - Multi Thread 8T (With Rect)")]
    public void Flush_MultiThread8T_WithRect()
    {
        var ctx = _ctx8T!;
        var rect = Rect.FromXYWH(100, 100, 400, 300);

        ctx.Reset();
        ctx.SetPaint(Color.Magenta);
        ctx.FillRect(rect);
        ctx.Flush();
    }

    // ========================================================================
    // Combined Operation Benchmarks
    // ========================================================================

    [Benchmark(Description = "Context + Pixmap - Single Thread")]
    public void Combined_ContextAndPixmap_SingleThread()
    {
        using var ctx = new RenderContext(Width, Height, new RenderSettings(
            level: SimdLevel.Avx2,
            numThreads: 0,
            mode: RenderMode.OptimizeSpeed
        ));
        using var pixmap = new Pixmap(Width, Height);
    }

    [Benchmark(Description = "Context + Pixmap - Multi Thread 8T")]
    public void Combined_ContextAndPixmap_MultiThread8T()
    {
        using var ctx = new RenderContext(Width, Height, new RenderSettings(
            level: SimdLevel.Avx2,
            numThreads: 8,
            mode: RenderMode.OptimizeSpeed
        ));
        using var pixmap = new Pixmap(Width, Height);
    }
}
