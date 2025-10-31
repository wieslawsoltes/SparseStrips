// Copyright 2025 Wieslaw Soltes
// SPDX-License-Identifier: Apache-2.0 OR MIT

using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Jobs;
using SkiaSharp;
using Vello;
using Vello.Geometry;

namespace Vello.Benchmarks;

/// <summary>
/// Complex scene benchmarks comparing Vello and SkiaSharp with varying shape counts.
/// Tests scalability with 1k, 10k, and 100k shapes.
/// Vello is tested in both single-threaded and multi-threaded (8 threads) configurations.
/// SkiaSharp is inherently single-threaded per render context.
/// </summary>
[MemoryDiagnoser]
[SimpleJob(RuntimeMoniker.Net80)]
[GroupBenchmarksBy(BenchmarkLogicalGroupRule.ByCategory)]
[CategoriesColumn]
public class ComplexSceneBenchmarks
{
    private const ushort Width = 1920;
    private const ushort Height = 1080;

    [Params(1000, 10000, 100000)]
    public int ShapeCount { get; set; }

    // ========================================================================
    // Vello Single-Threaded Benchmarks
    // ========================================================================

    [Benchmark(Description = "Vello - 1T")]
    [BenchmarkCategory("Vello_SingleThread")]
    public void Vello_ComplexScene_SingleThread()
    {
        using var ctx = new RenderContext(Width, Height, new RenderSettings(
            level: SimdLevel.Avx2,
            numThreads: 0, // Single-threaded
            mode: RenderMode.OptimizeSpeed
        ));
        using var pixmap = new Pixmap(Width, Height);

        // Background gradient
        var gradientStops = new ColorStop[]
        {
            new(0.0f, new Color(240, 248, 255, 255)), // Alice blue
            new(1.0f, new Color(135, 206, 235, 255))  // Sky blue
        };

        ctx.SetPaintLinearGradient(0, 0, Width, Height, gradientStops);
        ctx.FillRect(Rect.FromXYWH(0, 0, Width, Height));

        // Generate semi-random but deterministic shapes
        var random = new Random(42); // Fixed seed for reproducibility

        for (int i = 0; i < ShapeCount; i++)
        {
            double x = random.NextDouble() * (Width - 60);
            double y = random.NextDouble() * (Height - 60);
            double size = 10 + random.NextDouble() * 50;

            // Vary shape types for realistic complexity
            int shapeType = i % 4;

            // Set varying colors
            byte r = (byte)(random.Next(256));
            byte g = (byte)(random.Next(256));
            byte b = (byte)(random.Next(256));
            byte a = (byte)(180 + random.Next(76)); // 70-100% opacity

            ctx.SetPaint(new Color(r, g, b, a));

            switch (shapeType)
            {
                case 0: // Rectangle
                    ctx.FillRect(Rect.FromXYWH(x, y, size, size * 0.7));
                    break;

                case 1: // Circle (approximated with path)
                    using (var circle = new BezPath())
                    {
                        double cx = x + size / 2;
                        double cy = y + size / 2;
                        double radius = size / 2;
                        circle.MoveTo(cx + radius, cy);
                        for (int j = 1; j <= 16; j++)
                        {
                            double angle = j * Math.PI * 2.0 / 16.0;
                            circle.LineTo(cx + radius * Math.Cos(angle), cy + radius * Math.Sin(angle));
                        }
                        circle.Close();
                        ctx.FillPath(circle);
                    }
                    break;

                case 2: // Triangle
                    using (var triangle = new BezPath())
                    {
                        triangle.MoveTo(x + size / 2, y);
                        triangle.LineTo(x + size, y + size);
                        triangle.LineTo(x, y + size);
                        triangle.Close();
                        ctx.FillPath(triangle);
                    }
                    break;

                case 3: // Bezier curve shape
                    using (var curve = new BezPath())
                    {
                        curve.MoveTo(x, y);
                        curve.CurveTo(x + size * 0.5, y - size * 0.3,
                                     x + size, y + size * 0.3,
                                     x + size, y + size);
                        curve.LineTo(x, y + size);
                        curve.Close();
                        ctx.FillPath(curve);
                    }
                    break;
            }
        }

        ctx.Flush();
        ctx.RenderToPixmap(pixmap);
    }

    // ========================================================================
    // Vello Multi-Threaded Benchmarks (8 threads)
    // ========================================================================

    [Benchmark(Description = "Vello - 8T")]
    [BenchmarkCategory("Vello_MultiThread")]
    public void Vello_ComplexScene_MultiThread8T()
    {
        using var ctx = new RenderContext(Width, Height, new RenderSettings(
            level: SimdLevel.Avx2,
            numThreads: 8, // Multi-threaded
            mode: RenderMode.OptimizeSpeed
        ));
        using var pixmap = new Pixmap(Width, Height);

        // Background gradient
        var gradientStops = new ColorStop[]
        {
            new(0.0f, new Color(240, 248, 255, 255)), // Alice blue
            new(1.0f, new Color(135, 206, 235, 255))  // Sky blue
        };

        ctx.SetPaintLinearGradient(0, 0, Width, Height, gradientStops);
        ctx.FillRect(Rect.FromXYWH(0, 0, Width, Height));

        // Generate semi-random but deterministic shapes (identical to single-threaded)
        var random = new Random(42); // Fixed seed for reproducibility

        for (int i = 0; i < ShapeCount; i++)
        {
            double x = random.NextDouble() * (Width - 60);
            double y = random.NextDouble() * (Height - 60);
            double size = 10 + random.NextDouble() * 50;

            int shapeType = i % 4;

            byte r = (byte)(random.Next(256));
            byte g = (byte)(random.Next(256));
            byte b = (byte)(random.Next(256));
            byte a = (byte)(180 + random.Next(76));

            ctx.SetPaint(new Color(r, g, b, a));

            switch (shapeType)
            {
                case 0: // Rectangle
                    ctx.FillRect(Rect.FromXYWH(x, y, size, size * 0.7));
                    break;

                case 1: // Circle
                    using (var circle = new BezPath())
                    {
                        double cx = x + size / 2;
                        double cy = y + size / 2;
                        double radius = size / 2;
                        circle.MoveTo(cx + radius, cy);
                        for (int j = 1; j <= 16; j++)
                        {
                            double angle = j * Math.PI * 2.0 / 16.0;
                            circle.LineTo(cx + radius * Math.Cos(angle), cy + radius * Math.Sin(angle));
                        }
                        circle.Close();
                        ctx.FillPath(circle);
                    }
                    break;

                case 2: // Triangle
                    using (var triangle = new BezPath())
                    {
                        triangle.MoveTo(x + size / 2, y);
                        triangle.LineTo(x + size, y + size);
                        triangle.LineTo(x, y + size);
                        triangle.Close();
                        ctx.FillPath(triangle);
                    }
                    break;

                case 3: // Bezier curve shape
                    using (var curve = new BezPath())
                    {
                        curve.MoveTo(x, y);
                        curve.CurveTo(x + size * 0.5, y - size * 0.3,
                                     x + size, y + size * 0.3,
                                     x + size, y + size);
                        curve.LineTo(x, y + size);
                        curve.Close();
                        ctx.FillPath(curve);
                    }
                    break;
            }
        }

        ctx.Flush();
        ctx.RenderToPixmap(pixmap);
    }

    // ========================================================================
    // SkiaSharp Benchmarks (inherently single-threaded per render context)
    // ========================================================================

    [Benchmark(Baseline = true, Description = "SkiaSharp")]
    [BenchmarkCategory("SkiaSharp")]
    public void SkiaSharp_ComplexScene()
    {
        var info = new SKImageInfo(Width, Height, SKColorType.Rgba8888, SKAlphaType.Premul);
        using var surface = SKSurface.Create(info);
        using var canvas = surface.Canvas;

        canvas.Clear(SKColors.Transparent);

        // Background gradient
        var colors = new SKColor[]
        {
            new SKColor(240, 248, 255, 255), // Alice blue
            new SKColor(135, 206, 235, 255)  // Sky blue
        };
        var positions = new float[] { 0.0f, 1.0f };

        using (var shader = SKShader.CreateLinearGradient(
            new SKPoint(0, 0),
            new SKPoint(Width, Height),
            colors,
            positions,
            SKShaderTileMode.Clamp))
        using (var gradientPaint = new SKPaint
        {
            Shader = shader,
            IsAntialias = true,
            Style = SKPaintStyle.Fill
        })
        {
            canvas.DrawRect(0, 0, Width, Height, gradientPaint);
        }

        // Generate semi-random but deterministic shapes (identical to Vello)
        var random = new Random(42); // Fixed seed for reproducibility

        for (int i = 0; i < ShapeCount; i++)
        {
            float x = (float)(random.NextDouble() * (Width - 60));
            float y = (float)(random.NextDouble() * (Height - 60));
            float size = (float)(10 + random.NextDouble() * 50);

            int shapeType = i % 4;

            byte r = (byte)(random.Next(256));
            byte g = (byte)(random.Next(256));
            byte b = (byte)(random.Next(256));
            byte a = (byte)(180 + random.Next(76));

            using var paint = new SKPaint
            {
                Color = new SKColor(r, g, b, a),
                IsAntialias = true,
                Style = SKPaintStyle.Fill
            };

            switch (shapeType)
            {
                case 0: // Rectangle
                    canvas.DrawRect(x, y, size, size * 0.7f, paint);
                    break;

                case 1: // Circle
                    float cx = x + size / 2;
                    float cy = y + size / 2;
                    float radius = size / 2;
                    canvas.DrawCircle(cx, cy, radius, paint);
                    break;

                case 2: // Triangle
                    using (var triangle = new SKPath())
                    {
                        triangle.MoveTo(x + size / 2, y);
                        triangle.LineTo(x + size, y + size);
                        triangle.LineTo(x, y + size);
                        triangle.Close();
                        canvas.DrawPath(triangle, paint);
                    }
                    break;

                case 3: // Bezier curve shape
                    using (var curve = new SKPath())
                    {
                        curve.MoveTo(x, y);
                        curve.CubicTo(x + size * 0.5f, y - size * 0.3f,
                                     x + size, y + size * 0.3f,
                                     x + size, y + size);
                        curve.LineTo(x, y + size);
                        curve.Close();
                        canvas.DrawPath(curve, paint);
                    }
                    break;
            }
        }

        surface.Flush();
    }
}
