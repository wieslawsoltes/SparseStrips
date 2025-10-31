// Copyright 2025 Wieslaw Soltes
// SPDX-License-Identifier: Apache-2.0 OR MIT

using System.Diagnostics;
using Vello;
using Vello.Geometry;

namespace Vello.DiagnosticTests;

/// <summary>
/// Investigate why microbenchmarks show 8-10x slower performance vs Rust
/// despite wrapper overhead being only 3-4 µs
/// </summary>
public static class MicrobenchmarkInvestigation
{
    public static void Run()
    {
        Console.WriteLine("\n=== Microbenchmark Slowdown Investigation ===\n");

        // Rust benchmark: 70.9 µs (single-threaded fill_rect)
        // .NET benchmark: 585.3 µs (single-threaded FillRect)
        // Ratio: 8.3x slower
        //
        // But we proved:
        // - P/Invoke overhead: 0.008 µs
        // - Wrapper operations: 4 µs
        // - So where's the other 510 µs?

        Console.WriteLine("Test 1: Recreate Exact Benchmark Conditions");
        MeasureExactBenchmarkScenario();

        Console.WriteLine("\nTest 2: Compare Different Allocation Strategies");
        CompareAllocationStrategies();

        Console.WriteLine("\nTest 3: Measure Context Creation Overhead");
        MeasureContextCreation();

        Console.WriteLine("\nTest 4: Isolate RenderToPixmap");
        IsolateRenderToPixmap();

        Console.WriteLine("\nTest 5: Compare With/Without Context Recreation");
        CompareContextReuse();
    }

    static void MeasureExactBenchmarkScenario()
    {
        // This matches the BenchmarkDotNet test exactly
        const int iterations = 1000;
        const int width = 800;
        const int height = 600;

        var times = new List<long>();

        for (int i = 0; i < iterations; i++)
        {
            var sw = Stopwatch.StartNew();

            // Exact benchmark code
            using var ctx = new RenderContext(width, height);
            using var pixmap = new Pixmap(width, height);
            var rect = Rect.FromXYWH(100, 100, 400, 300);

            ctx.SetPaint(Color.Magenta);
            ctx.FillRect(rect);
            ctx.Flush();
            ctx.RenderToPixmap(pixmap);

            sw.Stop();
            times.Add(sw.ElapsedTicks);
        }

        var avgMs = times.Average() * 1000.0 / Stopwatch.Frequency;
        var minMs = times.Min() * 1000.0 / Stopwatch.Frequency;
        var maxMs = times.Max() * 1000.0 / Stopwatch.Frequency;

        Console.WriteLine($"  Exact benchmark scenario ({iterations} iterations):");
        Console.WriteLine($"    Average: {avgMs:F1} µs");
        Console.WriteLine($"    Min:     {minMs:F1} µs");
        Console.WriteLine($"    Max:     {maxMs:F1} µs");
        Console.WriteLine($"    Rust:    70.9 µs (from benchmark)");
        Console.WriteLine($"    Ratio:   {avgMs / 70.9:F2}x slower");
    }

    static void CompareAllocationStrategies()
    {
        const int iterations = 1000;
        const int width = 800;
        const int height = 600;

        // Strategy 1: Create new context/pixmap each iteration (benchmark style)
        var sw = Stopwatch.StartNew();
        for (int i = 0; i < iterations; i++)
        {
            using var ctx = new RenderContext(width, height);
            using var pixmap = new Pixmap(width, height);
            var rect = Rect.FromXYWH(100, 100, 400, 300);
            ctx.SetPaint(Color.Magenta);
            ctx.FillRect(rect);
            ctx.Flush();
            ctx.RenderToPixmap(pixmap);
        }
        var createEachTime = sw.Elapsed.TotalMilliseconds / iterations * 1000;

        // Strategy 2: Reuse context/pixmap (what a real app might do)
        using (var ctx = new RenderContext(width, height))
        using (var pixmap = new Pixmap(width, height))
        {
            var rect = Rect.FromXYWH(100, 100, 400, 300);

            sw.Restart();
            for (int i = 0; i < iterations; i++)
            {
                ctx.Reset();
                ctx.SetPaint(Color.Magenta);
                ctx.FillRect(rect);
                ctx.Flush();
                ctx.RenderToPixmap(pixmap);
            }
            var reuseTime = sw.Elapsed.TotalMilliseconds / iterations * 1000;

            Console.WriteLine($"  Create each time (benchmark):  {createEachTime:F1} µs");
            Console.WriteLine($"  Reuse context/pixmap:          {reuseTime:F1} µs");
            Console.WriteLine($"  Context creation overhead:     {createEachTime - reuseTime:F1} µs");
        }
    }

    static void MeasureContextCreation()
    {
        const int iterations = 10000;
        const int width = 800;
        const int height = 600;

        // Measure context creation only
        var sw = Stopwatch.StartNew();
        for (int i = 0; i < iterations; i++)
        {
            using var ctx = new RenderContext(width, height);
        }
        var ctxTime = sw.Elapsed.TotalMilliseconds / iterations * 1000;

        // Measure pixmap creation only
        sw.Restart();
        for (int i = 0; i < iterations; i++)
        {
            using var pixmap = new Pixmap(width, height);
        }
        var pixmapTime = sw.Elapsed.TotalMilliseconds / iterations * 1000;

        // Measure both
        sw.Restart();
        for (int i = 0; i < iterations; i++)
        {
            using var ctx = new RenderContext(width, height);
            using var pixmap = new Pixmap(width, height);
        }
        var bothTime = sw.Elapsed.TotalMilliseconds / iterations * 1000;

        Console.WriteLine($"  RenderContext creation: {ctxTime:F1} µs");
        Console.WriteLine($"  Pixmap creation:        {pixmapTime:F1} µs");
        Console.WriteLine($"  Both:                   {bothTime:F1} µs");
        Console.WriteLine($"  Total allocation cost:  {bothTime:F1} µs (per iteration)");
    }

    static void IsolateRenderToPixmap()
    {
        const int iterations = 10000;
        const int width = 800;
        const int height = 600;

        using var ctx = new RenderContext(width, height);
        using var pixmap = new Pixmap(width, height);
        var rect = Rect.FromXYWH(100, 100, 400, 300);

        // Measure just the rendering commands (no RenderToPixmap)
        var sw = Stopwatch.StartNew();
        for (int i = 0; i < iterations; i++)
        {
            ctx.Reset();
            ctx.SetPaint(Color.Magenta);
            ctx.FillRect(rect);
            ctx.Flush();
        }
        var commandsTime = sw.Elapsed.TotalMilliseconds / iterations * 1000;

        // Measure RenderToPixmap only
        ctx.Reset();
        ctx.SetPaint(Color.Magenta);
        ctx.FillRect(rect);
        ctx.Flush();

        sw.Restart();
        for (int i = 0; i < iterations; i++)
        {
            ctx.RenderToPixmap(pixmap);
            ctx.Reset();
            ctx.SetPaint(Color.Magenta);
            ctx.FillRect(rect);
            ctx.Flush();
        }
        var renderTime = sw.Elapsed.TotalMilliseconds / iterations * 1000;

        Console.WriteLine($"  Commands only (SetPaint+FillRect+Flush): {commandsTime:F1} µs");
        Console.WriteLine($"  RenderToPixmap + reset:                  {renderTime:F1} µs");
        Console.WriteLine($"  Pure RenderToPixmap:                     {renderTime - commandsTime:F1} µs");
    }

    static void CompareContextReuse()
    {
        const int iterations = 1000;
        const int width = 800;
        const int height = 600;
        var rect = Rect.FromXYWH(100, 100, 400, 300);

        // Without reuse (benchmark style)
        var swTotal = Stopwatch.StartNew();
        for (int i = 0; i < iterations; i++)
        {
            using var ctx = new RenderContext(width, height);
            using var pixmap = new Pixmap(width, height);
            ctx.SetPaint(Color.Magenta);
            ctx.FillRect(rect);
            ctx.Flush();
            ctx.RenderToPixmap(pixmap);
        }
        var noReuse = swTotal.Elapsed.TotalMilliseconds / iterations * 1000;

        // With reuse
        using (var ctx = new RenderContext(width, height))
        using (var pixmap = new Pixmap(width, height))
        {
            swTotal.Restart();
            for (int i = 0; i < iterations; i++)
            {
                ctx.Reset();
                ctx.SetPaint(Color.Magenta);
                ctx.FillRect(rect);
                ctx.Flush();
                ctx.RenderToPixmap(pixmap);
            }
        }
        var withReuse = swTotal.Elapsed.TotalMilliseconds / iterations * 1000;

        Console.WriteLine($"  Without context reuse (like benchmarks): {noReuse:F1} µs");
        Console.WriteLine($"  With context reuse (real app):           {withReuse:F1} µs");
        Console.WriteLine($"  Difference:                              {noReuse - withReuse:F1} µs");
        Console.WriteLine($"\n  Analysis:");
        Console.WriteLine($"    If Rust benchmark reuses context: explains {(noReuse - withReuse) / (noReuse - 70.9) * 100:F0}% of slowdown");
        Console.WriteLine($"    Remaining unexplained: {noReuse - withReuse - (noReuse - 70.9):F1} µs");
    }
}
