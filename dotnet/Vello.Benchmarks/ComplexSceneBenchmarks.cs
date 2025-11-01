// Copyright 2025 Wieslaw Soltes
// SPDX-License-Identifier: Apache-2.0 OR MIT

using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Jobs;
using SkiaSharp;
using Vello;
using Vello.Geometry;
using Vello.Native;
using Vello.Native.FastPath;

namespace Vello.Benchmarks;

/// <summary>
/// Complex scene benchmarks comparing Vello and SkiaSharp with varying shape counts.
/// Tests scalability with 1k, 10k, and 100k shapes.
/// Vello is tested in both single-threaded and multi-threaded (8 threads) configurations.
/// SkiaSharp is inherently single-threaded per render context.
/// </summary>
[MemoryDiagnoser]
[SimpleJob(RuntimeMoniker.Net90)]
[CategoriesColumn]
public class ComplexSceneBenchmarks
{
    private const ushort Width = 1920;
    private const ushort Height = 1080;

    [Params(1000, 10000, 100000)]
    public int ShapeCount { get; set; }

    private unsafe void ExecuteVelloNativeBenchmark(ushort numThreads)
    {
        var settings = new VelloRenderSettings
        {
            Level = VelloSimdLevel.Avx2,
            NumThreads = numThreads,
            RenderMode = VelloRenderMode.OptimizeSpeed
        };

        using var ctx = NativeRenderContext.CreateWith(Width, Height, ref settings);
        using var pixmap = new NativePixmap(Width, Height);

        VelloColorStop* gradientStops = stackalloc VelloColorStop[2];
        gradientStops[0] = new VelloColorStop
        {
            Offset = 0.0f,
            R = 240,
            G = 248,
            B = 255,
            A = 255
        };
        gradientStops[1] = new VelloColorStop
        {
            Offset = 1.0f,
            R = 135,
            G = 206,
            B = 235,
            A = 255
        };

        var stops = new ReadOnlySpan<VelloColorStop>(gradientStops, 2);
        ctx.SetPaintLinearGradient(
            0,
            0,
            Width,
            Height,
            stops,
            VelloExtend.Pad);

        var background = new VelloRect
        {
            X0 = 0,
            Y0 = 0,
            X1 = Width,
            Y1 = Height
        };

        ctx.FillRect(background);

        var stroke = new VelloStroke
        {
            Width = 1.0f,
            MiterLimit = 4.0f,
            Join = VelloJoin.Bevel,
            StartCap = VelloCap.Butt,
            EndCap = VelloCap.Butt
        };

        ctx.SetStroke(stroke);

        var random = new Random(42);

        for (int i = 0; i < ShapeCount; i++)
        {
            double x = random.NextDouble() * (Width - 60);
            double y = random.NextDouble() * (Height - 60);
            double size = 10 + random.NextDouble() * 50;
            int shapeType = i % 4;

            byte r = (byte)random.Next(256);
            byte g = (byte)random.Next(256);
            byte b = (byte)random.Next(256);
            byte a = (byte)(180 + random.Next(76));

            ctx.SetPaintSolid(r, g, b, a);

            switch (shapeType)
            {
                case 0:
                    {
                        var rect = new VelloRect
                        {
                            X0 = x,
                            Y0 = y,
                            X1 = x + size,
                            Y1 = y + size * 0.7
                        };

                        ctx.FillRect(rect);
                        break;
                    }

                case 1:
                    {
                        var rect = new VelloRect
                        {
                            X0 = x,
                            Y0 = y,
                            X1 = x + size,
                            Y1 = y + size
                        };

                        ctx.StrokeRect(rect);
                        break;
                    }

                case 2:
                    DrawTriangle(ctx, x, y, size);
                    break;

                case 3:
                    DrawCurve(ctx, x, y, size);
                    break;
            }
        }

        ctx.Flush();
        ctx.RenderToPixmap(pixmap);
    }

    private static void DrawTriangle(NativeRenderContext ctx, double x, double y, double size)
    {
        using var path = new NativeBezPath();
        path.MoveTo(x + size / 2.0, y);
        path.LineTo(x + size, y + size);
        path.LineTo(x, y + size);
        path.Close();
        ctx.FillPath(path);
    }

    private static void DrawCurve(NativeRenderContext ctx, double x, double y, double size)
    {
        using var path = new NativeBezPath();
        path.MoveTo(x, y);
        path.CurveTo(
            x + size * 0.5,
            y - size * 0.3,
            x + size,
            y + size * 0.3,
            x + size,
            y + size);
        path.LineTo(x, y + size);
        path.Close();
        ctx.FillPath(path);
    }

    private static Recording CreateRecording(int shapeCount)
    {
        var recording = new Recording();

        using var ctx = new RenderContext(Width, Height, new RenderSettings(
            level: SimdLevel.Avx2,
            numThreads: 0,
            mode: RenderMode.OptimizeSpeed));

        ctx.SetStroke(new Stroke(
            width: 1.0f,
            join: Join.Bevel,
            startCap: Cap.Butt,
            endCap: Cap.Butt,
            miterLimit: 4.0f));

        ctx.Record(recording, recorder =>
        {
            var random = new Random(42);

            for (int i = 0; i < shapeCount; i++)
            {
                double x = random.NextDouble() * (Width - 60);
                double y = random.NextDouble() * (Height - 60);
                double size = 10 + random.NextDouble() * 50;
                int shapeType = i % 4;

                byte r = (byte)random.Next(256);
                byte g = (byte)random.Next(256);
                byte b = (byte)random.Next(256);
                byte a = (byte)(180 + random.Next(76));

                recorder.SetPaint(new Color(r, g, b, a));

                switch (shapeType)
                {
                    case 0:
                        recorder.FillRect(Rect.FromXYWH(x, y, size, size * 0.7));
                        break;

                    case 1:
                        recorder.StrokeRect(Rect.FromXYWH(x, y, size, size));
                        break;

                    case 2:
                        using (var triangle = new BezPath())
                        {
                            triangle.MoveTo(x + size / 2, y);
                            triangle.LineTo(x + size, y + size);
                            triangle.LineTo(x, y + size);
                            triangle.Close();
                            recorder.FillPath(triangle);
                        }
                        break;

                    case 3:
                        using (var curve = new BezPath())
                        {
                            curve.MoveTo(x, y);
                            curve.CurveTo(x + size * 0.5, y - size * 0.3,
                                          x + size, y + size * 0.3,
                                          x + size, y + size);
                            curve.LineTo(x, y + size);
                            curve.Close();
                            recorder.FillPath(curve);
                        }
                        break;
                }
            }
        });

        return recording;
    }

    // ========================================================================
    // Vello Single-Threaded Benchmarks
    // ========================================================================

    [Benchmark(Description = "Vello - 1T")]
    [BenchmarkCategory("Vello_SingleThread", "All")]
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

                case 1: // Stroked Rectangle
                    ctx.StrokeRect(Rect.FromXYWH(x, y, size, size));
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
    [BenchmarkCategory("Vello_MultiThread", "All")]
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

                case 1: // Stroked Rectangle
                    ctx.StrokeRect(Rect.FromXYWH(x, y, size, size));
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
    // Vello Recording Benchmarks (record once per run, then replay)
    // ========================================================================

    [Benchmark(Description = "Vello Recording - 1T")]
    [BenchmarkCategory("Vello_Recording_SingleThread", "All")]
    public void Vello_ComplexScene_Recording_SingleThread()
    {
        using var recording = CreateRecording(ShapeCount);
        using var ctx = new RenderContext(Width, Height, new RenderSettings(
            level: SimdLevel.Avx2,
            numThreads: 0,
            mode: RenderMode.OptimizeSpeed
        ));
        using var pixmap = new Pixmap(Width, Height);

        var gradientStops = new ColorStop[]
        {
            new(0.0f, new Color(240, 248, 255, 255)), // Alice blue
            new(1.0f, new Color(135, 206, 235, 255))  // Sky blue
        };

        ctx.SetPaintLinearGradient(0, 0, Width, Height, gradientStops);
        ctx.FillRect(Rect.FromXYWH(0, 0, Width, Height));

        ctx.SetStroke(new Stroke(
            width: 1.0f,
            join: Join.Bevel,
            startCap: Cap.Butt,
            endCap: Cap.Butt,
            miterLimit: 4.0f));

        ctx.PrepareRecording(recording);
        ctx.ExecuteRecording(recording);

        ctx.Flush();
        ctx.RenderToPixmap(pixmap);
    }

    [Benchmark(Description = "Vello Recording - 8T")]
    [BenchmarkCategory("Vello_Recording_MultiThread", "All")]
    public void Vello_ComplexScene_Recording_MultiThread8T()
    {
        using var recording = CreateRecording(ShapeCount);
        using var ctx = new RenderContext(Width, Height, new RenderSettings(
            level: SimdLevel.Avx2,
            numThreads: 8,
            mode: RenderMode.OptimizeSpeed
        ));
        using var pixmap = new Pixmap(Width, Height);

        var gradientStops = new ColorStop[]
        {
            new(0.0f, new Color(240, 248, 255, 255)), // Alice blue
            new(1.0f, new Color(135, 206, 235, 255))  // Sky blue
        };

        ctx.SetPaintLinearGradient(0, 0, Width, Height, gradientStops);
        ctx.FillRect(Rect.FromXYWH(0, 0, Width, Height));

        ctx.SetStroke(new Stroke(
            width: 1.0f,
            join: Join.Bevel,
            startCap: Cap.Butt,
            endCap: Cap.Butt,
            miterLimit: 4.0f));

        ctx.PrepareRecording(recording);
        ctx.ExecuteRecording(recording);

        ctx.Flush();
        ctx.RenderToPixmap(pixmap);
    }

    // ========================================================================
    // Vello Native Benchmarks (direct native API usage)
    // ========================================================================

    [Benchmark(Description = "Vello Native - 1T")]
    [BenchmarkCategory("Vello_Native_SingleThread", "All")]
    public unsafe void Vello_Native_ComplexScene_SingleThread()
    {
        ExecuteVelloNativeBenchmark(0);
    }

    [Benchmark(Description = "Vello Native - 8T")]
    [BenchmarkCategory("Vello_Native_MultiThread", "All")]
    public unsafe void Vello_Native_ComplexScene_MultiThread8T()
    {
        ExecuteVelloNativeBenchmark(8);
    }

    // ========================================================================
    // SkiaSharp Benchmarks (inherently single-threaded per render context)
    // ========================================================================

    [Benchmark(Baseline = true, Description = "SkiaSharp")]
    [BenchmarkCategory("SkiaSharp", "All")]
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

                case 1: // Stroked Rectangle
                    paint.Style = SKPaintStyle.Stroke;
                    canvas.DrawRect(x, y, size, size, paint);
                    paint.Style = SKPaintStyle.Fill;
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
