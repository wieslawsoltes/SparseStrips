// Copyright 2025 Wieslaw Soltes
// SPDX-License-Identifier: Apache-2.0 OR MIT

using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Diagnosers;
using BenchmarkDotNet.Jobs;
using Vello;
using Vello.Geometry;

namespace Vello.Benchmarks;

/// <summary>
/// Comprehensive benchmarks for Vello .NET public API
/// Mirrors the Rust API benchmarks for direct comparison
/// Uses cold path: recreates RenderContext and Pixmap every iteration
/// </summary>
[MemoryDiagnoser]
[SimpleJob(RuntimeMoniker.Net80, baseline: true)]
[GroupBenchmarksBy(BenchmarkLogicalGroupRule.ByCategory)]
[CategoriesColumn]
public class VelloApiBenchmarks
{
    // Standard benchmark size
    private const ushort Width = 1920;
    private const ushort Height = 1080;

    // Small size for faster benchmarks
    private const ushort SmallWidth = 800;
    private const ushort SmallHeight = 600;

    // ========================================================================
    // Path Rendering Benchmarks
    // ========================================================================

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
        var rect = Rect.FromXYWH(100, 100, 400, 300);

        ctx.SetPaint(Color.Magenta);
        ctx.FillRect(rect);
        ctx.Flush();
        ctx.RenderToPixmap(pixmap);
    }

    [Benchmark(Description = "Fill Rectangle - Multi Thread 8T")]
    [BenchmarkCategory("FillRect")]
    public void FillRect_MultiThread8T()
    {
        using var ctx = new RenderContext(SmallWidth, SmallHeight, new RenderSettings(
            level: SimdLevel.Avx2,
            numThreads: 8,
            mode: RenderMode.OptimizeSpeed
        ));
        using var pixmap = new Pixmap(SmallWidth, SmallHeight);
        var rect = Rect.FromXYWH(100, 100, 400, 300);

        ctx.SetPaint(Color.Magenta);
        ctx.FillRect(rect);
        ctx.Flush();
        ctx.RenderToPixmap(pixmap);
    }

    [Benchmark(Description = "Stroke Rectangle - Single Thread")]
    [BenchmarkCategory("StrokeRect")]
    public void StrokeRect_SingleThread()
    {
        using var ctx = new RenderContext(SmallWidth, SmallHeight, new RenderSettings(
            level: SimdLevel.Avx2,
            numThreads: 0,
            mode: RenderMode.OptimizeSpeed
        ));
        using var pixmap = new Pixmap(SmallWidth, SmallHeight);
        var rect = Rect.FromXYWH(100, 100, 400, 300);
        var stroke = new Stroke(
            width: 5.0f,
            join: Join.Miter,
            startCap: Cap.Round,
            endCap: Cap.Round
        );

        ctx.SetPaint(Color.Blue);
        ctx.SetStroke(stroke);
        ctx.StrokeRect(rect);
        ctx.Flush();
        ctx.RenderToPixmap(pixmap);
    }

    [Benchmark(Description = "Stroke Rectangle - Multi Thread 8T")]
    [BenchmarkCategory("StrokeRect")]
    public void StrokeRect_MultiThread8T()
    {
        using var ctx = new RenderContext(SmallWidth, SmallHeight, new RenderSettings(
            level: SimdLevel.Avx2,
            numThreads: 8,
            mode: RenderMode.OptimizeSpeed
        ));
        using var pixmap = new Pixmap(SmallWidth, SmallHeight);
        var rect = Rect.FromXYWH(100, 100, 400, 300);
        var stroke = new Stroke(
            width: 5.0f,
            join: Join.Miter,
            startCap: Cap.Round,
            endCap: Cap.Round
        );

        ctx.SetPaint(Color.Blue);
        ctx.SetStroke(stroke);
        ctx.StrokeRect(rect);
        ctx.Flush();
        ctx.RenderToPixmap(pixmap);
    }

    [Benchmark(Description = "Fill Path Simple - Single Thread")]
    [BenchmarkCategory("FillPathSimple")]
    public void FillPathSimple_SingleThread()
    {
        using var ctx = new RenderContext(SmallWidth, SmallHeight, new RenderSettings(
            level: SimdLevel.Avx2,
            numThreads: 0,
            mode: RenderMode.OptimizeSpeed
        ));
        using var pixmap = new Pixmap(SmallWidth, SmallHeight);

        // Simple triangle path
        using var path = new BezPath();
        path.MoveTo(200, 100);
        path.LineTo(400, 400);
        path.LineTo(50, 400);
        path.Close();

        ctx.SetPaint(Color.Red);
        ctx.FillPath(path);
        ctx.Flush();
        ctx.RenderToPixmap(pixmap);
    }

    [Benchmark(Description = "Fill Path Simple - Multi Thread 8T")]
    [BenchmarkCategory("FillPathSimple")]
    public void FillPathSimple_MultiThread8T()
    {
        using var ctx = new RenderContext(SmallWidth, SmallHeight, new RenderSettings(
            level: SimdLevel.Avx2,
            numThreads: 8,
            mode: RenderMode.OptimizeSpeed
        ));
        using var pixmap = new Pixmap(SmallWidth, SmallHeight);

        // Simple triangle path
        using var path = new BezPath();
        path.MoveTo(200, 100);
        path.LineTo(400, 400);
        path.LineTo(50, 400);
        path.Close();

        ctx.SetPaint(Color.Red);
        ctx.FillPath(path);
        ctx.Flush();
        ctx.RenderToPixmap(pixmap);
    }

    [Benchmark(Description = "Fill Path Complex - Single Thread")]
    [BenchmarkCategory("FillPathComplex")]
    public void FillPathComplex_SingleThread()
    {
        using var ctx = new RenderContext(SmallWidth, SmallHeight, new RenderSettings(
            level: SimdLevel.Avx2,
            numThreads: 0,
            mode: RenderMode.OptimizeSpeed
        ));
        using var pixmap = new Pixmap(SmallWidth, SmallHeight);

        // Complex path with curves
        using var path = new BezPath();
        path.MoveTo(100, 100);
        path.CurveTo(200, 50, 300, 150, 400, 100);
        path.CurveTo(450, 200, 400, 300, 350, 350);
        path.CurveTo(250, 400, 150, 350, 100, 250);
        path.Close();

        ctx.SetPaint(Color.Green);
        ctx.FillPath(path);
        ctx.Flush();
        ctx.RenderToPixmap(pixmap);
    }

    [Benchmark(Description = "Fill Path Complex - Multi Thread 8T")]
    [BenchmarkCategory("FillPathComplex")]
    public void FillPathComplex_MultiThread8T()
    {
        using var ctx = new RenderContext(SmallWidth, SmallHeight, new RenderSettings(
            level: SimdLevel.Avx2,
            numThreads: 8,
            mode: RenderMode.OptimizeSpeed
        ));
        using var pixmap = new Pixmap(SmallWidth, SmallHeight);

        // Complex path with curves
        using var path = new BezPath();
        path.MoveTo(100, 100);
        path.CurveTo(200, 50, 300, 150, 400, 100);
        path.CurveTo(450, 200, 400, 300, 350, 350);
        path.CurveTo(250, 400, 150, 350, 100, 250);
        path.Close();

        ctx.SetPaint(Color.Green);
        ctx.FillPath(path);
        ctx.Flush();
        ctx.RenderToPixmap(pixmap);
    }

    [Benchmark(Description = "Stroke Path Complex - Single Thread")]
    [BenchmarkCategory("StrokePathComplex")]
    public void StrokePathComplex_SingleThread()
    {
        using var ctx = new RenderContext(SmallWidth, SmallHeight, new RenderSettings(
            level: SimdLevel.Avx2,
            numThreads: 0,
            mode: RenderMode.OptimizeSpeed
        ));
        using var pixmap = new Pixmap(SmallWidth, SmallHeight);

        // Complex curved path
        using var path = new BezPath();
        path.MoveTo(100, 300);
        path.CurveTo(200, 100, 400, 100, 500, 300);
        path.CurveTo(600, 500, 200, 500, 300, 300);

        var stroke = new Stroke(
            width: 8.0f,
            join: Join.Round,
            startCap: Cap.Round,
            endCap: Cap.Round
        );

        ctx.SetPaint(new Color(128, 0, 128, 255)); // Purple
        ctx.SetStroke(stroke);
        ctx.StrokePath(path);
        ctx.Flush();
        ctx.RenderToPixmap(pixmap);
    }

    [Benchmark(Description = "Stroke Path Complex - Multi Thread 8T")]
    [BenchmarkCategory("StrokePathComplex")]
    public void StrokePathComplex_MultiThread8T()
    {
        using var ctx = new RenderContext(SmallWidth, SmallHeight, new RenderSettings(
            level: SimdLevel.Avx2,
            numThreads: 8,
            mode: RenderMode.OptimizeSpeed
        ));
        using var pixmap = new Pixmap(SmallWidth, SmallHeight);

        // Complex curved path
        using var path = new BezPath();
        path.MoveTo(100, 300);
        path.CurveTo(200, 100, 400, 100, 500, 300);
        path.CurveTo(600, 500, 200, 500, 300, 300);

        var stroke = new Stroke(
            width: 8.0f,
            join: Join.Round,
            startCap: Cap.Round,
            endCap: Cap.Round
        );

        ctx.SetPaint(new Color(128, 0, 128, 255)); // Purple
        ctx.SetStroke(stroke);
        ctx.StrokePath(path);
        ctx.Flush();
        ctx.RenderToPixmap(pixmap);
    }

    // ========================================================================
    // Gradient Benchmarks
    // ========================================================================

    [Benchmark(Description = "Linear Gradient - Single Thread")]
    [BenchmarkCategory("LinearGradient")]
    public void LinearGradient_SingleThread()
    {
        using var ctx = new RenderContext(SmallWidth, SmallHeight, new RenderSettings(
            level: SimdLevel.Avx2,
            numThreads: 0,
            mode: RenderMode.OptimizeSpeed
        ));
        using var pixmap = new Pixmap(SmallWidth, SmallHeight);

        var stops = new ColorStop[]
        {
            new(0.0f, Color.Red),
            new(0.5f, Color.Yellow),
            new(1.0f, Color.Blue)
        };

        ctx.SetPaintLinearGradient(0, 0, 800, 600, stops);
        ctx.FillRect(Rect.FromXYWH(50, 50, 700, 500));
        ctx.Flush();
        ctx.RenderToPixmap(pixmap);
    }

    [Benchmark(Description = "Linear Gradient - Multi Thread 8T")]
    [BenchmarkCategory("LinearGradient")]
    public void LinearGradient_MultiThread8T()
    {
        using var ctx = new RenderContext(SmallWidth, SmallHeight, new RenderSettings(
            level: SimdLevel.Avx2,
            numThreads: 8,
            mode: RenderMode.OptimizeSpeed
        ));
        using var pixmap = new Pixmap(SmallWidth, SmallHeight);

        var stops = new ColorStop[]
        {
            new(0.0f, Color.Red),
            new(0.5f, Color.Yellow),
            new(1.0f, Color.Blue)
        };

        ctx.SetPaintLinearGradient(0, 0, 800, 600, stops);
        ctx.FillRect(Rect.FromXYWH(50, 50, 700, 500));
        ctx.Flush();
        ctx.RenderToPixmap(pixmap);
    }

    [Benchmark(Description = "Radial Gradient - Single Thread")]
    [BenchmarkCategory("RadialGradient")]
    public void RadialGradient_SingleThread()
    {
        using var ctx = new RenderContext(SmallWidth, SmallHeight, new RenderSettings(
            level: SimdLevel.Avx2,
            numThreads: 0,
            mode: RenderMode.OptimizeSpeed
        ));
        using var pixmap = new Pixmap(SmallWidth, SmallHeight);

        var stops = new ColorStop[]
        {
            new(0.0f, Color.White),
            new(0.5f, Color.Cyan),
            new(1.0f, new Color(0, 0, 128, 255)) // Navy
        };

        ctx.SetPaintRadialGradient(400, 300, 250, stops);
        ctx.FillRect(Rect.FromXYWH(100, 50, 600, 500));
        ctx.Flush();
        ctx.RenderToPixmap(pixmap);
    }

    [Benchmark(Description = "Radial Gradient - Multi Thread 8T")]
    [BenchmarkCategory("RadialGradient")]
    public void RadialGradient_MultiThread8T()
    {
        using var ctx = new RenderContext(SmallWidth, SmallHeight, new RenderSettings(
            level: SimdLevel.Avx2,
            numThreads: 8,
            mode: RenderMode.OptimizeSpeed
        ));
        using var pixmap = new Pixmap(SmallWidth, SmallHeight);

        var stops = new ColorStop[]
        {
            new(0.0f, Color.White),
            new(0.5f, Color.Cyan),
            new(1.0f, new Color(0, 0, 128, 255)) // Navy
        };

        ctx.SetPaintRadialGradient(400, 300, 250, stops);
        ctx.FillRect(Rect.FromXYWH(100, 50, 600, 500));
        ctx.Flush();
        ctx.RenderToPixmap(pixmap);
    }

    // ========================================================================
    // Transform Benchmarks
    // ========================================================================

    [Benchmark(Description = "Transforms - Single Thread")]
    [BenchmarkCategory("Transforms")]
    public void Transforms_SingleThread()
    {
        using var ctx = new RenderContext(SmallWidth, SmallHeight, new RenderSettings(
            level: SimdLevel.Avx2,
            numThreads: 0,
            mode: RenderMode.OptimizeSpeed
        ));
        using var pixmap = new Pixmap(SmallWidth, SmallHeight);
        var transform = Affine.Rotation(0.785398); // 45 degrees

        ctx.SetTransform(transform);
        ctx.SetPaint(new Color(255, 165, 0, 255)); // Orange

        // Draw 5 rectangles with transform
        for (int i = 0; i < 5; i++)
        {
            double offset = i * 50.0;
            ctx.FillRect(Rect.FromXYWH(offset, offset, 40, 40));
        }

        ctx.Flush();
        ctx.RenderToPixmap(pixmap);
    }

    [Benchmark(Description = "Transforms - Multi Thread 8T")]
    [BenchmarkCategory("Transforms")]
    public void Transforms_MultiThread8T()
    {
        using var ctx = new RenderContext(SmallWidth, SmallHeight, new RenderSettings(
            level: SimdLevel.Avx2,
            numThreads: 8,
            mode: RenderMode.OptimizeSpeed
        ));
        using var pixmap = new Pixmap(SmallWidth, SmallHeight);
        var transform = Affine.Rotation(0.785398); // 45 degrees

        ctx.SetTransform(transform);
        ctx.SetPaint(new Color(255, 165, 0, 255)); // Orange

        // Draw 5 rectangles with transform
        for (int i = 0; i < 5; i++)
        {
            double offset = i * 50.0;
            ctx.FillRect(Rect.FromXYWH(offset, offset, 40, 40));
        }

        ctx.Flush();
        ctx.RenderToPixmap(pixmap);
    }

    // ========================================================================
    // Blending and Compositing Benchmarks
    // ========================================================================

    [Benchmark(Description = "Blend Modes - Single Thread")]
    [BenchmarkCategory("BlendModes")]
    public void BlendModes_SingleThread()
    {
        using var ctx = new RenderContext(SmallWidth, SmallHeight, new RenderSettings(
            level: SimdLevel.Avx2,
            numThreads: 0,
            mode: RenderMode.OptimizeSpeed
        ));
        using var pixmap = new Pixmap(SmallWidth, SmallHeight);

        var rect1 = Rect.FromXYWH(100, 100, 300, 200);
        var rect2 = Rect.FromXYWH(250, 200, 300, 200);

        // Base layer
        ctx.SetPaint(Color.Red);
        ctx.FillRect(rect1);

        // Blend layer
        ctx.PushBlendLayer(new BlendMode(Mix.Multiply, Compose.SrcOver));
        ctx.SetPaint(Color.Blue);
        ctx.FillRect(rect2);
        ctx.PopLayer();

        ctx.Flush();
        ctx.RenderToPixmap(pixmap);
    }

    [Benchmark(Description = "Blend Modes - Multi Thread 8T")]
    [BenchmarkCategory("BlendModes")]
    public void BlendModes_MultiThread8T()
    {
        using var ctx = new RenderContext(SmallWidth, SmallHeight, new RenderSettings(
            level: SimdLevel.Avx2,
            numThreads: 8,
            mode: RenderMode.OptimizeSpeed
        ));
        using var pixmap = new Pixmap(SmallWidth, SmallHeight);

        var rect1 = Rect.FromXYWH(100, 100, 300, 200);
        var rect2 = Rect.FromXYWH(250, 200, 300, 200);

        // Base layer
        ctx.SetPaint(Color.Red);
        ctx.FillRect(rect1);

        // Blend layer
        ctx.PushBlendLayer(new BlendMode(Mix.Multiply, Compose.SrcOver));
        ctx.SetPaint(Color.Blue);
        ctx.FillRect(rect2);
        ctx.PopLayer();

        ctx.Flush();
        ctx.RenderToPixmap(pixmap);
    }

    [Benchmark(Description = "Opacity Layer - Single Thread")]
    [BenchmarkCategory("OpacityLayer")]
    public void OpacityLayer_SingleThread()
    {
        using var ctx = new RenderContext(SmallWidth, SmallHeight, new RenderSettings(
            level: SimdLevel.Avx2,
            numThreads: 0,
            mode: RenderMode.OptimizeSpeed
        ));
        using var pixmap = new Pixmap(SmallWidth, SmallHeight);

        ctx.PushOpacityLayer(0.5f);
        ctx.SetPaint(Color.Green);
        ctx.FillRect(Rect.FromXYWH(100, 100, 400, 300));
        ctx.PopLayer();

        ctx.Flush();
        ctx.RenderToPixmap(pixmap);
    }

    [Benchmark(Description = "Opacity Layer - Multi Thread 8T")]
    [BenchmarkCategory("OpacityLayer")]
    public void OpacityLayer_MultiThread8T()
    {
        using var ctx = new RenderContext(SmallWidth, SmallHeight, new RenderSettings(
            level: SimdLevel.Avx2,
            numThreads: 8,
            mode: RenderMode.OptimizeSpeed
        ));
        using var pixmap = new Pixmap(SmallWidth, SmallHeight);

        ctx.PushOpacityLayer(0.5f);
        ctx.SetPaint(Color.Green);
        ctx.FillRect(Rect.FromXYWH(100, 100, 400, 300));
        ctx.PopLayer();

        ctx.Flush();
        ctx.RenderToPixmap(pixmap);
    }

    // ========================================================================
    // Clipping Benchmarks
    // ========================================================================

    [Benchmark(Description = "Clip Layer - Single Thread")]
    [BenchmarkCategory("ClipLayer")]
    public void ClipLayer_SingleThread()
    {
        using var ctx = new RenderContext(SmallWidth, SmallHeight, new RenderSettings(
            level: SimdLevel.Avx2,
            numThreads: 0,
            mode: RenderMode.OptimizeSpeed
        ));
        using var pixmap = new Pixmap(SmallWidth, SmallHeight);

        // Clip path (circle approximation)
        using var clipPath = new BezPath();
        clipPath.MoveTo(400 + 150, 300);
        for (int i = 1; i <= 32; i++)
        {
            double angle = i * Math.PI * 2.0 / 32.0;
            double x = 400.0 + 150.0 * Math.Cos(angle);
            double y = 300.0 + 150.0 * Math.Sin(angle);
            clipPath.LineTo(x, y);
        }
        clipPath.Close();

        ctx.PushClipLayer(clipPath);
        ctx.SetPaint(new Color(238, 130, 238, 255)); // Violet
        ctx.FillRect(Rect.FromXYWH(200, 150, 400, 300));
        ctx.PopLayer();

        ctx.Flush();
        ctx.RenderToPixmap(pixmap);
    }

    [Benchmark(Description = "Clip Layer - Multi Thread 8T")]
    [BenchmarkCategory("ClipLayer")]
    public void ClipLayer_MultiThread8T()
    {
        using var ctx = new RenderContext(SmallWidth, SmallHeight, new RenderSettings(
            level: SimdLevel.Avx2,
            numThreads: 8,
            mode: RenderMode.OptimizeSpeed
        ));
        using var pixmap = new Pixmap(SmallWidth, SmallHeight);

        // Clip path (circle approximation)
        using var clipPath = new BezPath();
        clipPath.MoveTo(400 + 150, 300);
        for (int i = 1; i <= 32; i++)
        {
            double angle = i * Math.PI * 2.0 / 32.0;
            double x = 400.0 + 150.0 * Math.Cos(angle);
            double y = 300.0 + 150.0 * Math.Sin(angle);
            clipPath.LineTo(x, y);
        }
        clipPath.Close();

        ctx.PushClipLayer(clipPath);
        ctx.SetPaint(new Color(238, 130, 238, 255)); // Violet
        ctx.FillRect(Rect.FromXYWH(200, 150, 400, 300));
        ctx.PopLayer();

        ctx.Flush();
        ctx.RenderToPixmap(pixmap);
    }

    // ========================================================================
    // Blurred Rounded Rectangle Benchmarks
    // ========================================================================

    [Benchmark(Description = "Blurred Rounded Rect - Single Thread")]
    [BenchmarkCategory("BlurredRoundedRect")]
    public void BlurredRoundedRect_SingleThread()
    {
        using var ctx = new RenderContext(SmallWidth, SmallHeight, new RenderSettings(
            level: SimdLevel.Avx2,
            numThreads: 0,
            mode: RenderMode.OptimizeSpeed
        ));
        using var pixmap = new Pixmap(SmallWidth, SmallHeight);

        ctx.SetPaint(new Color(0, 128, 128, 255)); // Teal
        ctx.FillBlurredRoundedRect(Rect.FromXYWH(100, 100, 400, 300), 20.0f, 10.0f);

        ctx.Flush();
        ctx.RenderToPixmap(pixmap);
    }

    [Benchmark(Description = "Blurred Rounded Rect - Multi Thread 8T")]
    [BenchmarkCategory("BlurredRoundedRect")]
    public void BlurredRoundedRect_MultiThread8T()
    {
        using var ctx = new RenderContext(SmallWidth, SmallHeight, new RenderSettings(
            level: SimdLevel.Avx2,
            numThreads: 8,
            mode: RenderMode.OptimizeSpeed
        ));
        using var pixmap = new Pixmap(SmallWidth, SmallHeight);

        ctx.SetPaint(new Color(0, 128, 128, 255)); // Teal
        ctx.FillBlurredRoundedRect(Rect.FromXYWH(100, 100, 400, 300), 20.0f, 10.0f);

        ctx.Flush();
        ctx.RenderToPixmap(pixmap);
    }

    // ========================================================================
    // Complex Scene Benchmark
    // ========================================================================

    [Benchmark(Description = "Complex Scene - Single Thread")]
    [BenchmarkCategory("ComplexScene")]
    public void ComplexScene_SingleThread()
    {
        using var ctx = new RenderContext(SmallWidth, SmallHeight, new RenderSettings(
            level: SimdLevel.Avx2,
            numThreads: 0,
            mode: RenderMode.OptimizeSpeed
        ));
        using var pixmap = new Pixmap(SmallWidth, SmallHeight);

        // Background gradient
        var gradientStops = new ColorStop[]
        {
            new(0.0f, new Color(173, 216, 230, 255)), // Light blue
            new(1.0f, new Color(0, 0, 128, 255)) // Navy
        };

        ctx.SetPaintLinearGradient(0, 0, 800, 600, gradientStops);
        ctx.FillRect(Rect.FromXYWH(0, 0, 800, 600));

        // Draw multiple shapes
        for (int i = 0; i < 10; i++)
        {
            double x = i * 70.0 + 50.0;
            double y = 100.0 + (i % 3) * 150.0;

            // Rectangle with opacity
            ctx.PushOpacityLayer(0.7f);
            ctx.SetPaint(Color.Red);
            ctx.FillRect(Rect.FromXYWH(x, y, 50, 50));
            ctx.PopLayer();

            // Circle (approximated with path)
            using var circle = new BezPath();
            double cx = x + 25.0;
            double cy = y + 80.0;
            double r = 20.0;
            circle.MoveTo(cx + r, cy);
            for (int j = 1; j <= 16; j++)
            {
                double angle = j * Math.PI * 2.0 / 16.0;
                circle.LineTo(cx + r * Math.Cos(angle), cy + r * Math.Sin(angle));
            }
            circle.Close();

            ctx.SetPaint(Color.Yellow);
            ctx.FillPath(circle);
        }

        ctx.Flush();
        ctx.RenderToPixmap(pixmap);
    }

    [Benchmark(Description = "Complex Scene - Multi Thread 8T")]
    [BenchmarkCategory("ComplexScene")]
    public void ComplexScene_MultiThread8T()
    {
        using var ctx = new RenderContext(SmallWidth, SmallHeight, new RenderSettings(
            level: SimdLevel.Avx2,
            numThreads: 8,
            mode: RenderMode.OptimizeSpeed
        ));
        using var pixmap = new Pixmap(SmallWidth, SmallHeight);

        // Background gradient
        var gradientStops = new ColorStop[]
        {
            new(0.0f, new Color(173, 216, 230, 255)), // Light blue
            new(1.0f, new Color(0, 0, 128, 255)) // Navy
        };

        ctx.SetPaintLinearGradient(0, 0, 800, 600, gradientStops);
        ctx.FillRect(Rect.FromXYWH(0, 0, 800, 600));

        // Draw multiple shapes
        for (int i = 0; i < 10; i++)
        {
            double x = i * 70.0 + 50.0;
            double y = 100.0 + (i % 3) * 150.0;

            // Rectangle with opacity
            ctx.PushOpacityLayer(0.7f);
            ctx.SetPaint(Color.Red);
            ctx.FillRect(Rect.FromXYWH(x, y, 50, 50));
            ctx.PopLayer();

            // Circle (approximated with path)
            using var circle = new BezPath();
            double cx = x + 25.0;
            double cy = y + 80.0;
            double r = 20.0;
            circle.MoveTo(cx + r, cy);
            for (int j = 1; j <= 16; j++)
            {
                double angle = j * Math.PI * 2.0 / 16.0;
                circle.LineTo(cx + r * Math.Cos(angle), cy + r * Math.Sin(angle));
            }
            circle.Close();

            ctx.SetPaint(Color.Yellow);
            ctx.FillPath(circle);
        }

        ctx.Flush();
        ctx.RenderToPixmap(pixmap);
    }
}
