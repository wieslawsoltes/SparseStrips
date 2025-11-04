// Copyright 2025 Wieslaw Soltes
// SPDX-License-Identifier: Apache-2.0 OR MIT

using System.Diagnostics;
using Vello;
using Vello.Geometry;

namespace Vello.DiagnosticTests;

/// <summary>
/// Detailed profiling to understand the remaining performance gap
/// </summary>
public static class DetailedProfilingTest
{
    public static void Run()
    {
        Console.WriteLine("\n=== Detailed Profiling: Where Does Time Go? ===\n");

        const int warmup = 100;
        const int iterations = 10000;
        const ushort width = 800;
        const ushort height = 600;

        // Warm up
        for (int i = 0; i < warmup; i++)
        {
            using var ctx = new RenderContext(width, height);
            using var pixmap = new Pixmap(width, height);
            var rect = Rect.FromXYWH(100, 100, 400, 300);
            ctx.SetPaint(Color.Magenta);
            ctx.FillRect(rect);
            ctx.Flush();
            ctx.RenderToPixmap(pixmap);
        }

        Console.WriteLine($"Warmed up with {warmup} iterations\n");

        // Test 1: Measure each operation individually
        MeasureIndividualOperations(iterations, width, height);

        // Test 2: Measure with and without allocation
        CompareAllocationImpact(iterations, width, height);

        // Test 3: Breakdown of context creation
        BreakdownContextCreation(iterations, width, height);
    }

    static void MeasureIndividualOperations(int iterations, ushort width, ushort height)
    {
        Console.WriteLine("Test 1: Individual Operation Timing\n");

        using var ctx = new RenderContext(width, height);
        using var pixmap = new Pixmap(width, height);
        var rect = Rect.FromXYWH(100, 100, 400, 300);

        // Just SetPaint
        var sw = Stopwatch.StartNew();
        for (int i = 0; i < iterations; i++)
        {
            ctx.SetPaint(Color.Magenta);
        }
        var setPaintTime = sw.Elapsed.TotalMilliseconds / iterations * 1000;

        // SetPaint + FillRect
        sw.Restart();
        for (int i = 0; i < iterations; i++)
        {
            ctx.SetPaint(Color.Magenta);
            ctx.FillRect(rect);
        }
        var fillRectTime = sw.Elapsed.TotalMilliseconds / iterations * 1000;

        // SetPaint + FillRect + Flush
        sw.Restart();
        for (int i = 0; i < iterations; i++)
        {
            ctx.SetPaint(Color.Magenta);
            ctx.FillRect(rect);
            ctx.Flush();
        }
        var flushTime = sw.Elapsed.TotalMilliseconds / iterations * 1000;

        // Full operation with Reset (reuse pattern)
        sw.Restart();
        for (int i = 0; i < iterations; i++)
        {
            ctx.Reset();
            ctx.SetPaint(Color.Magenta);
            ctx.FillRect(rect);
            ctx.Flush();
            ctx.RenderToPixmap(pixmap);
        }
        var fullWithResetTime = sw.Elapsed.TotalMilliseconds / iterations * 1000;

        Console.WriteLine($"  SetPaint only:              {setPaintTime:F3} µs");
        Console.WriteLine($"  + FillRect:                 {fillRectTime:F3} µs");
        Console.WriteLine($"  + Flush:                    {flushTime:F3} µs");
        Console.WriteLine($"  + RenderToPixmap + Reset:   {fullWithResetTime:F3} µs");
        Console.WriteLine($"\n  FillRect cost:              {fillRectTime - setPaintTime:F3} µs");
        Console.WriteLine($"  Flush cost:                 {flushTime - fillRectTime:F3} µs");
        Console.WriteLine($"  RenderToPixmap cost:        {fullWithResetTime - flushTime:F3} µs");
    }

    static void CompareAllocationImpact(int iterations, ushort width, ushort height)
    {
        Console.WriteLine($"\n\nTest 2: Allocation Impact ({iterations} iterations)\n");

        var rect = Rect.FromXYWH(100, 100, 400, 300);

        // Pattern 1: Reuse (best case)
        long reuseTime;
        using (var ctx = new RenderContext(width, height))
        using (var pixmap = new Pixmap(width, height))
        {
            var sw = Stopwatch.StartNew();
            for (int i = 0; i < iterations; i++)
            {
                ctx.Reset();
                ctx.SetPaint(Color.Magenta);
                ctx.FillRect(rect);
                ctx.Flush();
                ctx.RenderToPixmap(pixmap);
            }
            reuseTime = sw.ElapsedTicks;
        }

        // Pattern 2: Recreate context, reuse pixmap
        long recreateCtxTime;
        using (var pixmap = new Pixmap(width, height))
        {
            var sw = Stopwatch.StartNew();
            for (int i = 0; i < iterations; i++)
            {
                using var ctx = new RenderContext(width, height);
                ctx.SetPaint(Color.Magenta);
                ctx.FillRect(rect);
                ctx.Flush();
                ctx.RenderToPixmap(pixmap);
            }
            recreateCtxTime = sw.ElapsedTicks;
        }

        // Pattern 3: Recreate pixmap, reuse context
        long recreatePixmapTime;
        using (var ctx = new RenderContext(width, height))
        {
            var sw = Stopwatch.StartNew();
            for (int i = 0; i < iterations; i++)
            {
                using var pixmap = new Pixmap(width, height);
                ctx.Reset();
                ctx.SetPaint(Color.Magenta);
                ctx.FillRect(rect);
                ctx.Flush();
                ctx.RenderToPixmap(pixmap);
            }
            recreatePixmapTime = sw.ElapsedTicks;
        }

        // Pattern 4: Recreate both (benchmark pattern)
        var sw4 = Stopwatch.StartNew();
        for (int i = 0; i < iterations; i++)
        {
            using var ctx = new RenderContext(width, height);
            using var pixmap = new Pixmap(width, height);
            ctx.SetPaint(Color.Magenta);
            ctx.FillRect(rect);
            ctx.Flush();
            ctx.RenderToPixmap(pixmap);
        }
        var recreateBothTime = sw4.ElapsedTicks;

        var reuseMs = reuseTime * 1000.0 / Stopwatch.Frequency / iterations;
        var recreateCtxMs = recreateCtxTime * 1000.0 / Stopwatch.Frequency / iterations;
        var recreatePixmapMs = recreatePixmapTime * 1000.0 / Stopwatch.Frequency / iterations;
        var recreateBothMs = recreateBothTime * 1000.0 / Stopwatch.Frequency / iterations;

        Console.WriteLine($"  Reuse both:                     {reuseMs:F1} µs (baseline)");
        Console.WriteLine($"  Recreate Context:               {recreateCtxMs:F1} µs (+{recreateCtxMs - reuseMs:F1} µs)");
        Console.WriteLine($"  Recreate Pixmap:                {recreatePixmapMs:F1} µs (+{recreatePixmapMs - reuseMs:F1} µs)");
        Console.WriteLine($"  Recreate Both (benchmark):      {recreateBothMs:F1} µs (+{recreateBothMs - reuseMs:F1} µs)");
        Console.WriteLine($"\n  Context overhead:               {recreateCtxMs - reuseMs:F1} µs");
        Console.WriteLine($"  Pixmap overhead:                {recreatePixmapMs - reuseMs:F1} µs");
        Console.WriteLine($"  Both overhead:                  {recreateBothMs - reuseMs:F1} µs");
    }

    static void BreakdownContextCreation(int iterations, ushort width, ushort height)
    {
        Console.WriteLine($"\n\nTest 3: Context Creation Breakdown ({iterations} iterations)\n");

        // Just allocation (no use)
        var sw = Stopwatch.StartNew();
        for (int i = 0; i < iterations; i++)
        {
            using var ctx = new RenderContext(width, height);
        }
        var allocOnlyTime = sw.Elapsed.TotalMilliseconds / iterations * 1000;

        // Allocation + single operation
        sw.Restart();
        for (int i = 0; i < iterations; i++)
        {
            using var ctx = new RenderContext(width, height);
            ctx.SetPaint(Color.Magenta);
        }
        var allocPlusOpTime = sw.Elapsed.TotalMilliseconds / iterations * 1000;

        // Full operation
        sw.Restart();
        for (int i = 0; i < iterations; i++)
        {
            using var ctx = new RenderContext(width, height);
            using var pixmap = new Pixmap(width, height);
            ctx.SetPaint(Color.Magenta);
            ctx.FillRect(Rect.FromXYWH(100, 100, 400, 300));
            ctx.Flush();
            ctx.RenderToPixmap(pixmap);
        }
        var fullTime = sw.Elapsed.TotalMilliseconds / iterations * 1000;

        Console.WriteLine($"  Context allocation only:        {allocOnlyTime:F1} µs");
        Console.WriteLine($"  + one operation (SetPaint):     {allocPlusOpTime:F1} µs");
        Console.WriteLine($"  + full render:                  {fullTime:F1} µs");
        Console.WriteLine($"\n  First operation overhead:       {allocPlusOpTime - allocOnlyTime:F1} µs");
        Console.WriteLine($"  Rendering overhead:             {fullTime - allocPlusOpTime:F1} µs");
    }
}
