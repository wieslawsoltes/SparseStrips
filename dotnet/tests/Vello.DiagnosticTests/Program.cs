// Copyright 2025 Wieslaw Soltes
// SPDX-License-Identifier: Apache-2.0 OR MIT

using System.Diagnostics;
using Vello;
using Vello.Geometry;

namespace Vello.DiagnosticTests;

/// <summary>
/// Diagnostic tests to investigate multi-threading behavior in .NET bindings
/// </summary>
class Program
{
    static void Main(string[] args)
    {
        if (args.Length > 0 && args[0] == "--overhead")
        {
            WrapperOverheadTests.Run();
            return;
        }

        if (args.Length > 0 && args[0] == "--microbench")
        {
            MicrobenchmarkInvestigation.Run();
            return;
        }

        if (args.Length > 0 && args[0] == "--detailed")
        {
            DetailedProfilingTest.Run();
            return;
        }

        Console.WriteLine("=== Vello .NET Multi-Threading Diagnostic Tests ===\n");

        // Test 1: Verify native library uses threads
        Console.WriteLine("Test 1: Thread Count During Rendering");
        TestThreadCount();

        // Test 2: Timing with different canvas sizes
        Console.WriteLine("\nTest 2: Canvas Size Impact on MT Performance");
        TestCanvasSizes();

        // Test 3: Repeated renders to measure consistency
        Console.WriteLine("\nTest 3: Repeated Renders (Warm-up Analysis)");
        TestRepeatedRenders();

        // Test 4: Thread scaling
        Console.WriteLine("\nTest 4: Thread Count Scaling");
        TestThreadScaling();

        Console.WriteLine("\n=== Diagnostic Tests Complete ===");
        Console.WriteLine("\nRun with --overhead flag to profile wrapper overhead");
    }

    static void TestThreadCount()
    {
        const int width = 800;
        const int height = 600;

        // Get baseline thread count
        var baselineThreads = Process.GetCurrentProcess().Threads.Count;
        Console.WriteLine($"Baseline threads: {baselineThreads}");

        // Single-threaded render
        using (var ctx = new RenderContext(width, height, new RenderSettings(
            level: SimdLevel.Avx2,
            numThreads: 0,
            mode: RenderMode.OptimizeSpeed
        )))
        using (var pixmap = new Pixmap(width, height))
        {
            var rect = Rect.FromXYWH(100, 100, 400, 300);
            ctx.SetPaint(Color.Magenta);
            ctx.FillRect(rect);
            ctx.Flush();

            var beforeRender = Process.GetCurrentProcess().Threads.Count;
            ctx.RenderToPixmap(pixmap);
            var afterRender = Process.GetCurrentProcess().Threads.Count;

            Console.WriteLine($"Single-threaded: before={beforeRender}, after={afterRender}, delta={afterRender - beforeRender}");
        }

        // Multi-threaded render (8 threads)
        using (var ctx = new RenderContext(width, height, new RenderSettings(
            level: SimdLevel.Avx2,
            numThreads: 8,
            mode: RenderMode.OptimizeSpeed
        )))
        using (var pixmap = new Pixmap(width, height))
        {
            var rect = Rect.FromXYWH(100, 100, 400, 300);
            ctx.SetPaint(Color.Magenta);
            ctx.FillRect(rect);
            ctx.Flush();

            var beforeRender = Process.GetCurrentProcess().Threads.Count;
            ctx.RenderToPixmap(pixmap);
            var afterRender = Process.GetCurrentProcess().Threads.Count;

            Console.WriteLine($"Multi-threaded (8): before={beforeRender}, after={afterRender}, delta={afterRender - beforeRender}");
        }
    }

    static void TestCanvasSizes()
    {
        var sizes = new[] {
            (800, 600, "800x600 (small)"),
            (1920, 1080, "1920x1080 (HD)"),
            (3840, 2160, "3840x2160 (4K)")
        };

        foreach (var (width, height, label) in sizes)
        {
            Console.WriteLine($"\n{label}:");

            // Create complex scene for better MT visibility
            var iterations = width <= 1920 ? 100 : 10; // Fewer iterations for 4K

            // Single-threaded
            var sw = Stopwatch.StartNew();
            for (int i = 0; i < iterations; i++)
            {
                using var ctx = new RenderContext((ushort)width, (ushort)height, new RenderSettings(
                    level: SimdLevel.Avx2,
                    numThreads: 0,
                    mode: RenderMode.OptimizeSpeed
                ));
                using var pixmap = new Pixmap((ushort)width, (ushort)height);
                RenderComplexScene(ctx);
                ctx.Flush();
                ctx.RenderToPixmap(pixmap);
            }
            var stTime = sw.ElapsedMilliseconds;

            // Multi-threaded (8 threads)
            sw.Restart();
            for (int i = 0; i < iterations; i++)
            {
                using var ctx = new RenderContext((ushort)width, (ushort)height, new RenderSettings(
                    level: SimdLevel.Avx2,
                    numThreads: 8,
                    mode: RenderMode.OptimizeSpeed
                ));
                using var pixmap = new Pixmap((ushort)width, (ushort)height);
                RenderComplexScene(ctx);
                ctx.Flush();
                ctx.RenderToPixmap(pixmap);
            }
            var mtTime = sw.ElapsedMilliseconds;

            var speedup = (double)stTime / mtTime;
            Console.WriteLine($"  Single-threaded: {stTime}ms ({iterations} iterations, {(double)stTime/iterations:F2}ms avg)");
            Console.WriteLine($"  Multi-threaded:  {mtTime}ms ({iterations} iterations, {(double)mtTime/iterations:F2}ms avg)");
            Console.WriteLine($"  Speedup: {speedup:F2}x {(speedup > 1 ? "✓" : speedup < 0.95 ? "⚠️ SLOWER" : "~")}");
        }
    }

    static void TestRepeatedRenders()
    {
        const int warmupRuns = 10;
        const int testRuns = 100;
        const int width = 1920;
        const int height = 1080;

        // Warm-up
        for (int i = 0; i < warmupRuns; i++)
        {
            using var ctx = new RenderContext(width, height, new RenderSettings(SimdLevel.Avx2, 8, RenderMode.OptimizeSpeed));
            using var pixmap = new Pixmap(width, height);
            RenderComplexScene(ctx);
            ctx.Flush();
            ctx.RenderToPixmap(pixmap);
        }

        Console.WriteLine($"Warmed up with {warmupRuns} runs");

        // Test single-threaded
        var stTimes = new List<long>();
        for (int i = 0; i < testRuns; i++)
        {
            using var ctx = new RenderContext(width, height, new RenderSettings(SimdLevel.Avx2, 0, RenderMode.OptimizeSpeed));
            using var pixmap = new Pixmap(width, height);

            var sw = Stopwatch.StartNew();
            RenderComplexScene(ctx);
            ctx.Flush();
            ctx.RenderToPixmap(pixmap);
            sw.Stop();

            stTimes.Add(sw.ElapsedTicks);
        }

        // Test multi-threaded
        var mtTimes = new List<long>();
        for (int i = 0; i < testRuns; i++)
        {
            using var ctx = new RenderContext(width, height, new RenderSettings(SimdLevel.Avx2, 8, RenderMode.OptimizeSpeed));
            using var pixmap = new Pixmap(width, height);

            var sw = Stopwatch.StartNew();
            RenderComplexScene(ctx);
            ctx.Flush();
            ctx.RenderToPixmap(pixmap);
            sw.Stop();

            mtTimes.Add(sw.ElapsedTicks);
        }

        var stAvg = stTimes.Average() * 1000.0 / Stopwatch.Frequency;
        var mtAvg = mtTimes.Average() * 1000.0 / Stopwatch.Frequency;
        var stMin = stTimes.Min() * 1000.0 / Stopwatch.Frequency;
        var mtMin = mtTimes.Min() * 1000.0 / Stopwatch.Frequency;
        var stMax = stTimes.Max() * 1000.0 / Stopwatch.Frequency;
        var mtMax = mtTimes.Max() * 1000.0 / Stopwatch.Frequency;

        Console.WriteLine($"Single-threaded ({testRuns} runs): avg={stAvg:F2}ms, min={stMin:F2}ms, max={stMax:F2}ms");
        Console.WriteLine($"Multi-threaded ({testRuns} runs):  avg={mtAvg:F2}ms, min={mtMin:F2}ms, max={mtMax:F2}ms");
        Console.WriteLine($"Speedup: {stAvg/mtAvg:F2}x");
    }

    static void TestThreadScaling()
    {
        const int width = 1920;
        const int height = 1080;
        const int iterations = 50;

        var threadCounts = new[] { 0, 1, 2, 4, 8, 16 };

        Console.WriteLine($"Canvas: {width}x{height}, Iterations: {iterations}\n");
        Console.WriteLine("Threads | Time (ms) | Avg (ms) | Speedup vs ST");
        Console.WriteLine("--------|-----------|----------|---------------");

        long? baselineTime = null;

        foreach (var numThreads in threadCounts)
        {
            var sw = Stopwatch.StartNew();
            for (int i = 0; i < iterations; i++)
            {
                using var ctx = new RenderContext(width, height, new RenderSettings(
                    level: SimdLevel.Avx2,
                    numThreads: (ushort)numThreads,
                    mode: RenderMode.OptimizeSpeed
                ));
                using var pixmap = new Pixmap(width, height);
                RenderComplexScene(ctx);
                ctx.Flush();
                ctx.RenderToPixmap(pixmap);
            }
            var totalTime = sw.ElapsedMilliseconds;
            var avgTime = (double)totalTime / iterations;

            if (!baselineTime.HasValue)
                baselineTime = totalTime;

            var speedup = (double)baselineTime.Value / totalTime;
            var label = numThreads == 0 ? "ST" : numThreads.ToString();

            Console.WriteLine($"{label,7} | {totalTime,9} | {avgTime,8:F2} | {speedup,6:F2}x");
        }
    }

    static void RenderComplexScene(RenderContext ctx)
    {
        // Gradient background
        var gradientStops = new[]
        {
            new ColorStop(0.0f, new Color(173, 216, 230, 255)), // Light blue
            new ColorStop(1.0f, new Color(0, 0, 128, 255))      // Navy
        };

        ctx.SetPaintLinearGradient(
            0, 0,
            ctx.Width, ctx.Height,
            gradientStops
        );
        ctx.FillRect(Rect.FromXYWH(0, 0, ctx.Width, ctx.Height));

        // Draw multiple shapes with opacity layers
        for (int i = 0; i < 20; i++)
        {
            float x = i * (ctx.Width / 20.0f);
            float y = 100.0f + (i % 3) * 150.0f;

            // Rectangle with opacity
            ctx.PushOpacityLayer(0.7f);
            ctx.SetPaint(Color.Red);
            ctx.FillRect(Rect.FromXYWH(x, y, 50, 50));
            ctx.PopLayer();

            // Circle (approximated with path)
            using var path = new BezPath();
            float cx = x + 25.0f;
            float cy = y + 80.0f;
            float r = 20.0f;

            path.MoveTo(cx + r, cy);
            for (int j = 1; j <= 32; j++)
            {
                double angle = j * Math.PI * 2.0 / 32.0;
                path.LineTo(cx + r * Math.Cos(angle), cy + r * Math.Sin(angle));
            }
            path.Close();

            ctx.SetPaint(Color.Yellow);
            ctx.FillPath(path);
        }
    }
}
