// Copyright 2025 Wieslaw Soltes
// SPDX-License-Identifier: Apache-2.0 OR MIT

using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;
using SkiaSharp;

namespace Vello.Benchmarks;

/// <summary>
/// Overhead benchmarks for SkiaSharp
/// Measures the cost of fundamental operations:
/// - Surface creation
/// - Canvas creation
/// - Flush operation
/// </summary>
[MemoryDiagnoser]
[SimpleJob(RuntimeMoniker.Net80)]
public class SkiaSharpOverheadBenchmarks
{
    private const int Width = 800;
    private const int Height = 600;

    // ========================================================================
    // Surface Creation Benchmarks
    // ========================================================================

    [Benchmark(Description = "Surface Creation - 800x600")]
    public void SurfaceCreation_800x600()
    {
        using var surface = SKSurface.Create(new SKImageInfo(800, 600, SKColorType.Rgba8888, SKAlphaType.Premul));
    }

    [Benchmark(Description = "Surface Creation - 1920x1080")]
    public void SurfaceCreation_1920x1080()
    {
        using var surface = SKSurface.Create(new SKImageInfo(1920, 1080, SKColorType.Rgba8888, SKAlphaType.Premul));
    }

    [Benchmark(Description = "Surface Creation - 3840x2160")]
    public void SurfaceCreation_3840x2160()
    {
        using var surface = SKSurface.Create(new SKImageInfo(3840, 2160, SKColorType.Rgba8888, SKAlphaType.Premul));
    }

    // ========================================================================
    // Bitmap Creation Benchmarks (equivalent to Pixmap)
    // ========================================================================

    [Benchmark(Description = "Bitmap Creation - 800x600")]
    public void BitmapCreation_800x600()
    {
        using var bitmap = new SKBitmap(new SKImageInfo(800, 600, SKColorType.Rgba8888, SKAlphaType.Premul));
    }

    [Benchmark(Description = "Bitmap Creation - 1920x1080")]
    public void BitmapCreation_1920x1080()
    {
        using var bitmap = new SKBitmap(new SKImageInfo(1920, 1080, SKColorType.Rgba8888, SKAlphaType.Premul));
    }

    [Benchmark(Description = "Bitmap Creation - 3840x2160")]
    public void BitmapCreation_3840x2160()
    {
        using var bitmap = new SKBitmap(new SKImageInfo(3840, 2160, SKColorType.Rgba8888, SKAlphaType.Premul));
    }

    // ========================================================================
    // Flush Benchmarks (with surface reuse)
    // ========================================================================

    private SKSurface? _surface;

    [GlobalSetup]
    public void Setup()
    {
        _surface = SKSurface.Create(new SKImageInfo(Width, Height, SKColorType.Rgba8888, SKAlphaType.Premul));
    }

    [GlobalCleanup]
    public void Cleanup()
    {
        _surface?.Dispose();
    }

    [Benchmark(Description = "Flush - Empty Canvas")]
    public void Flush_Empty()
    {
        var canvas = _surface!.Canvas;
        canvas.Clear(SKColors.Transparent);
        canvas.Flush();
    }

    [Benchmark(Description = "Flush - With Rect")]
    public void Flush_WithRect()
    {
        var canvas = _surface!.Canvas;
        canvas.Clear(SKColors.Transparent);

        using var paint = new SKPaint
        {
            Color = SKColors.Magenta,
            Style = SKPaintStyle.Fill
        };

        canvas.DrawRect(100, 100, 400, 300, paint);
        canvas.Flush();
    }

    // ========================================================================
    // Combined Operation Benchmarks
    // ========================================================================

    [Benchmark(Description = "Surface + Canvas")]
    public void Combined_SurfaceAndCanvas()
    {
        using var surface = SKSurface.Create(new SKImageInfo(Width, Height, SKColorType.Rgba8888, SKAlphaType.Premul));
        var canvas = surface.Canvas;
    }

    [Benchmark(Description = "Surface + Bitmap")]
    public void Combined_SurfaceAndBitmap()
    {
        using var surface = SKSurface.Create(new SKImageInfo(Width, Height, SKColorType.Rgba8888, SKAlphaType.Premul));
        using var bitmap = new SKBitmap(new SKImageInfo(Width, Height, SKColorType.Rgba8888, SKAlphaType.Premul));
    }

    // ========================================================================
    // Paint Object Creation (equivalent to context state)
    // ========================================================================

    [Benchmark(Description = "Paint Creation")]
    public void PaintCreation()
    {
        using var paint = new SKPaint
        {
            Color = SKColors.Magenta,
            Style = SKPaintStyle.Fill,
            IsAntialias = true
        };
    }

    [Benchmark(Description = "Paint Creation (Stroke)")]
    public void PaintCreation_Stroke()
    {
        using var paint = new SKPaint
        {
            Color = SKColors.Blue,
            Style = SKPaintStyle.Stroke,
            StrokeWidth = 5,
            StrokeCap = SKStrokeCap.Round,
            StrokeJoin = SKStrokeJoin.Round,
            IsAntialias = true
        };
    }
}
