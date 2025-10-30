// Copyright 2025 Wieslaw Soltes
// SPDX-License-Identifier: Apache-2.0 OR MIT

using BenchmarkDotNet.Attributes;
using SkiaSharp;

namespace Vello.Benchmarks;

/// <summary>
/// SkiaSharp v3.x benchmarks for comparison with Vello .NET
/// Implements identical operations to Vello benchmarks for fair comparison
/// </summary>
[MemoryDiagnoser]
[GroupBenchmarksBy(BenchmarkDotNet.Configs.BenchmarkLogicalGroupRule.ByCategory)]
[CategoriesColumn]
public class SkiaSharpBenchmarks
{
    // Standard benchmark size
    private const int Width = 1920;
    private const int Height = 1080;

    // Small size for faster benchmarks (matching Vello)
    private const int SmallWidth = 800;
    private const int SmallHeight = 600;

    // ========================================================================
    // Path Rendering Benchmarks
    // ========================================================================

    [Benchmark(Description = "Fill Rectangle - SkiaSharp")]
    [BenchmarkCategory("FillRect")]
    public void FillRect_SkiaSharp()
    {
        var info = new SKImageInfo(SmallWidth, SmallHeight, SKColorType.Rgba8888, SKAlphaType.Premul);
        using var surface = SKSurface.Create(info);
        using var canvas = surface.Canvas;
        using var paint = new SKPaint
        {
            Color = SKColors.Magenta,
            IsAntialias = true,
            Style = SKPaintStyle.Fill
        };

        canvas.Clear(SKColors.Transparent);
        canvas.DrawRect(100, 100, 400, 300, paint);
        surface.Flush();
    }

    [Benchmark(Description = "Stroke Rectangle - SkiaSharp")]
    [BenchmarkCategory("StrokeRect")]
    public void StrokeRect_SkiaSharp()
    {
        var info = new SKImageInfo(SmallWidth, SmallHeight, SKColorType.Rgba8888, SKAlphaType.Premul);
        using var surface = SKSurface.Create(info);
        using var canvas = surface.Canvas;
        using var paint = new SKPaint
        {
            Color = SKColors.Blue,
            IsAntialias = true,
            Style = SKPaintStyle.Stroke,
            StrokeWidth = 5.0f,
            StrokeJoin = SKStrokeJoin.Miter,
            StrokeCap = SKStrokeCap.Round
        };

        canvas.Clear(SKColors.Transparent);
        canvas.DrawRect(100, 100, 400, 300, paint);
        surface.Flush();
    }

    [Benchmark(Description = "Fill Path Simple - SkiaSharp")]
    [BenchmarkCategory("FillPathSimple")]
    public void FillPathSimple_SkiaSharp()
    {
        var info = new SKImageInfo(SmallWidth, SmallHeight, SKColorType.Rgba8888, SKAlphaType.Premul);
        using var surface = SKSurface.Create(info);
        using var canvas = surface.Canvas;
        using var paint = new SKPaint
        {
            Color = SKColors.Red,
            IsAntialias = true,
            Style = SKPaintStyle.Fill
        };

        // Simple triangle path
        using var path = new SKPath();
        path.MoveTo(200, 100);
        path.LineTo(400, 400);
        path.LineTo(50, 400);
        path.Close();

        canvas.Clear(SKColors.Transparent);
        canvas.DrawPath(path, paint);
        surface.Flush();
    }

    [Benchmark(Description = "Fill Path Complex - SkiaSharp")]
    [BenchmarkCategory("FillPathComplex")]
    public void FillPathComplex_SkiaSharp()
    {
        var info = new SKImageInfo(SmallWidth, SmallHeight, SKColorType.Rgba8888, SKAlphaType.Premul);
        using var surface = SKSurface.Create(info);
        using var canvas = surface.Canvas;
        using var paint = new SKPaint
        {
            Color = SKColors.Green,
            IsAntialias = true,
            Style = SKPaintStyle.Fill
        };

        // Complex path with curves
        using var path = new SKPath();
        path.MoveTo(100, 100);
        path.CubicTo(200, 50, 300, 150, 400, 100);
        path.CubicTo(450, 200, 400, 300, 350, 350);
        path.CubicTo(250, 400, 150, 350, 100, 250);
        path.Close();

        canvas.Clear(SKColors.Transparent);
        canvas.DrawPath(path, paint);
        surface.Flush();
    }

    [Benchmark(Description = "Stroke Path Complex - SkiaSharp")]
    [BenchmarkCategory("StrokePathComplex")]
    public void StrokePathComplex_SkiaSharp()
    {
        var info = new SKImageInfo(SmallWidth, SmallHeight, SKColorType.Rgba8888, SKAlphaType.Premul);
        using var surface = SKSurface.Create(info);
        using var canvas = surface.Canvas;
        using var paint = new SKPaint
        {
            Color = new SKColor(128, 0, 128, 255), // Purple
            IsAntialias = true,
            Style = SKPaintStyle.Stroke,
            StrokeWidth = 8.0f,
            StrokeJoin = SKStrokeJoin.Round,
            StrokeCap = SKStrokeCap.Round
        };

        // Complex curved path
        using var path = new SKPath();
        path.MoveTo(100, 300);
        path.CubicTo(200, 100, 400, 100, 500, 300);
        path.CubicTo(600, 500, 200, 500, 300, 300);

        canvas.Clear(SKColors.Transparent);
        canvas.DrawPath(path, paint);
        surface.Flush();
    }

    // ========================================================================
    // Gradient Benchmarks
    // ========================================================================

    [Benchmark(Description = "Linear Gradient - SkiaSharp")]
    [BenchmarkCategory("LinearGradient")]
    public void LinearGradient_SkiaSharp()
    {
        var info = new SKImageInfo(SmallWidth, SmallHeight, SKColorType.Rgba8888, SKAlphaType.Premul);
        using var surface = SKSurface.Create(info);
        using var canvas = surface.Canvas;

        var colors = new SKColor[]
        {
            SKColors.Red,
            SKColors.Yellow,
            SKColors.Blue
        };

        var positions = new float[] { 0.0f, 0.5f, 1.0f };

        using var shader = SKShader.CreateLinearGradient(
            new SKPoint(0, 0),
            new SKPoint(800, 600),
            colors,
            positions,
            SKShaderTileMode.Clamp
        );

        using var paint = new SKPaint
        {
            Shader = shader,
            IsAntialias = true,
            Style = SKPaintStyle.Fill
        };

        canvas.Clear(SKColors.Transparent);
        canvas.DrawRect(50, 50, 700, 500, paint);
        surface.Flush();
    }

    [Benchmark(Description = "Radial Gradient - SkiaSharp")]
    [BenchmarkCategory("RadialGradient")]
    public void RadialGradient_SkiaSharp()
    {
        var info = new SKImageInfo(SmallWidth, SmallHeight, SKColorType.Rgba8888, SKAlphaType.Premul);
        using var surface = SKSurface.Create(info);
        using var canvas = surface.Canvas;

        var colors = new SKColor[]
        {
            SKColors.White,
            SKColors.Cyan,
            new SKColor(0, 0, 128, 255) // Navy
        };

        var positions = new float[] { 0.0f, 0.5f, 1.0f };

        using var shader = SKShader.CreateRadialGradient(
            new SKPoint(400, 300),
            250,
            colors,
            positions,
            SKShaderTileMode.Clamp
        );

        using var paint = new SKPaint
        {
            Shader = shader,
            IsAntialias = true,
            Style = SKPaintStyle.Fill
        };

        canvas.Clear(SKColors.Transparent);
        canvas.DrawRect(100, 50, 600, 500, paint);
        surface.Flush();
    }

    // ========================================================================
    // Transform Benchmarks
    // ========================================================================

    [Benchmark(Description = "Transforms - SkiaSharp")]
    [BenchmarkCategory("Transforms")]
    public void Transforms_SkiaSharp()
    {
        var info = new SKImageInfo(SmallWidth, SmallHeight, SKColorType.Rgba8888, SKAlphaType.Premul);
        using var surface = SKSurface.Create(info);
        using var canvas = surface.Canvas;
        using var paint = new SKPaint
        {
            Color = new SKColor(255, 165, 0, 255), // Orange
            IsAntialias = true,
            Style = SKPaintStyle.Fill
        };

        canvas.Clear(SKColors.Transparent);

        // Apply rotation transform (45 degrees)
        canvas.RotateRadians(0.785398f);

        // Draw 5 rectangles with transform
        for (int i = 0; i < 5; i++)
        {
            float offset = i * 50.0f;
            canvas.DrawRect(offset, offset, 40, 40, paint);
        }

        surface.Flush();
    }

    // ========================================================================
    // Blending and Compositing Benchmarks
    // ========================================================================

    [Benchmark(Description = "Blend Modes - SkiaSharp")]
    [BenchmarkCategory("BlendModes")]
    public void BlendModes_SkiaSharp()
    {
        var info = new SKImageInfo(SmallWidth, SmallHeight, SKColorType.Rgba8888, SKAlphaType.Premul);
        using var surface = SKSurface.Create(info);
        using var canvas = surface.Canvas;

        canvas.Clear(SKColors.Transparent);

        // Base layer - red rectangle
        using (var paint = new SKPaint
        {
            Color = SKColors.Red,
            IsAntialias = true,
            Style = SKPaintStyle.Fill
        })
        {
            canvas.DrawRect(100, 100, 300, 200, paint);
        }

        // Blend layer - blue rectangle with Multiply blend mode
        using (var paint = new SKPaint
        {
            Color = SKColors.Blue,
            IsAntialias = true,
            Style = SKPaintStyle.Fill,
            BlendMode = SKBlendMode.Multiply
        })
        {
            canvas.DrawRect(250, 200, 300, 200, paint);
        }

        surface.Flush();
    }

    [Benchmark(Description = "Opacity Layer - SkiaSharp")]
    [BenchmarkCategory("OpacityLayer")]
    public void OpacityLayer_SkiaSharp()
    {
        var info = new SKImageInfo(SmallWidth, SmallHeight, SKColorType.Rgba8888, SKAlphaType.Premul);
        using var surface = SKSurface.Create(info);
        using var canvas = surface.Canvas;

        canvas.Clear(SKColors.Transparent);

        // Use SaveLayer for opacity
        using var layerPaint = new SKPaint
        {
            Color = new SKColor(255, 255, 255, 128) // 50% opacity
        };

        canvas.SaveLayer(layerPaint);

        using var paint = new SKPaint
        {
            Color = SKColors.Green,
            IsAntialias = true,
            Style = SKPaintStyle.Fill
        };

        canvas.DrawRect(100, 100, 400, 300, paint);
        canvas.Restore();

        surface.Flush();
    }

    // ========================================================================
    // Clipping Benchmarks
    // ========================================================================

    [Benchmark(Description = "Clip Layer - SkiaSharp")]
    [BenchmarkCategory("ClipLayer")]
    public void ClipLayer_SkiaSharp()
    {
        var info = new SKImageInfo(SmallWidth, SmallHeight, SKColorType.Rgba8888, SKAlphaType.Premul);
        using var surface = SKSurface.Create(info);
        using var canvas = surface.Canvas;

        canvas.Clear(SKColors.Transparent);

        // Clip path (circle approximation with 32 sides)
        using var clipPath = new SKPath();
        clipPath.MoveTo(400 + 150, 300);
        for (int i = 1; i <= 32; i++)
        {
            double angle = i * Math.PI * 2.0 / 32.0;
            float x = (float)(400.0 + 150.0 * Math.Cos(angle));
            float y = (float)(300.0 + 150.0 * Math.Sin(angle));
            clipPath.LineTo(x, y);
        }
        clipPath.Close();

        canvas.Save();
        canvas.ClipPath(clipPath);

        using var paint = new SKPaint
        {
            Color = new SKColor(238, 130, 238, 255), // Violet
            IsAntialias = true,
            Style = SKPaintStyle.Fill
        };

        canvas.DrawRect(200, 150, 400, 300, paint);
        canvas.Restore();

        surface.Flush();
    }

    // ========================================================================
    // Blurred Rounded Rectangle Benchmarks
    // ========================================================================

    [Benchmark(Description = "Blurred Rounded Rect - SkiaSharp")]
    [BenchmarkCategory("BlurredRoundedRect")]
    public void BlurredRoundedRect_SkiaSharp()
    {
        var info = new SKImageInfo(SmallWidth, SmallHeight, SKColorType.Rgba8888, SKAlphaType.Premul);
        using var surface = SKSurface.Create(info);
        using var canvas = surface.Canvas;

        canvas.Clear(SKColors.Transparent);

        using var maskFilter = SKMaskFilter.CreateBlur(SKBlurStyle.Normal, 10.0f);
        using var paint = new SKPaint
        {
            Color = new SKColor(0, 128, 128, 255), // Teal
            IsAntialias = true,
            Style = SKPaintStyle.Fill,
            MaskFilter = maskFilter
        };

        var rect = new SKRect(100, 100, 500, 400);
        canvas.DrawRoundRect(rect, 20.0f, 20.0f, paint);

        surface.Flush();
    }

    // ========================================================================
    // Complex Scene Benchmark
    // ========================================================================

    [Benchmark(Description = "Complex Scene - SkiaSharp")]
    [BenchmarkCategory("ComplexScene")]
    public void ComplexScene_SkiaSharp()
    {
        var info = new SKImageInfo(SmallWidth, SmallHeight, SKColorType.Rgba8888, SKAlphaType.Premul);
        using var surface = SKSurface.Create(info);
        using var canvas = surface.Canvas;

        canvas.Clear(SKColors.Transparent);

        // Background gradient
        var colors = new SKColor[]
        {
            new SKColor(173, 216, 230, 255), // Light blue
            new SKColor(0, 0, 128, 255) // Navy
        };
        var positions = new float[] { 0.0f, 1.0f };

        using var shader = SKShader.CreateLinearGradient(
            new SKPoint(0, 0),
            new SKPoint(800, 600),
            colors,
            positions,
            SKShaderTileMode.Clamp
        );

        using (var gradientPaint = new SKPaint
        {
            Shader = shader,
            IsAntialias = true,
            Style = SKPaintStyle.Fill
        })
        {
            canvas.DrawRect(0, 0, 800, 600, gradientPaint);
        }

        // Draw multiple shapes
        for (int i = 0; i < 10; i++)
        {
            float x = i * 70.0f + 50.0f;
            float y = 100.0f + (i % 3) * 150.0f;

            // Rectangle with opacity
            using (var layerPaint = new SKPaint
            {
                Color = new SKColor(255, 255, 255, 179) // 70% opacity
            })
            {
                canvas.SaveLayer(layerPaint);

                using var rectPaint = new SKPaint
                {
                    Color = SKColors.Red,
                    IsAntialias = true,
                    Style = SKPaintStyle.Fill
                };

                canvas.DrawRect(x, y, 50, 50, rectPaint);
                canvas.Restore();
            }

            // Circle
            using var circlePaint = new SKPaint
            {
                Color = SKColors.Yellow,
                IsAntialias = true,
                Style = SKPaintStyle.Fill
            };

            float cx = x + 25.0f;
            float cy = y + 80.0f;
            float r = 20.0f;

            canvas.DrawCircle(cx, cy, r, circlePaint);
        }

        surface.Flush();
    }
}
