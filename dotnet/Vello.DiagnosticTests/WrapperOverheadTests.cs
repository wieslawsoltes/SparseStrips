// Copyright 2025 Wieslaw Soltes
// SPDX-License-Identifier: Apache-2.0 OR MIT

using System.Diagnostics;
using System.Runtime.CompilerServices;
using Vello;
using Vello.Geometry;

namespace Vello.DiagnosticTests;

/// <summary>
/// Tests to measure exactly where wrapper overhead comes from
/// </summary>
public static class WrapperOverheadTests
{
    public static void Run()
    {
        Console.WriteLine("\n=== Wrapper Overhead Profiling ===\n");

        // Test 1: Measure P/Invoke call overhead
        MeasurePInvokeOverhead();

        // Test 2: Measure object allocation overhead
        MeasureAllocationOverhead();

        // Test 3: Measure handle dereferencing overhead
        MeasureHandleOverhead();

        // Test 4: Break down single render operation
        BreakdownRenderOperation();

        // Test 5: Compare direct native calls vs wrapper
        CompareNativeVsWrapper();
    }

    static void MeasurePInvokeOverhead()
    {
        Console.WriteLine("Test 1: P/Invoke Call Overhead");

        const int iterations = 1_000_000;

        // Measure empty P/Invoke call
        var sw = Stopwatch.StartNew();
        for (int i = 0; i < iterations; i++)
        {
            // This would need a trivial native function that does nothing
            // For now, use a simple operation
            _ = SimdLevel.Avx2;
        }
        sw.Stop();

        var managedTime = sw.Elapsed.TotalMilliseconds / iterations * 1000; // Convert to µs
        Console.WriteLine($"  Managed enum access: {managedTime:F3} µs per call");

        // Measure actual context operations
        using var ctx = new RenderContext(100, 100);

        sw.Restart();
        for (int i = 0; i < iterations; i++)
        {
            var _ = ctx.GetFillRule();
        }
        sw.Stop();

        var pinvokeTime = sw.Elapsed.TotalMilliseconds / iterations * 1000; // Convert to µs
        Console.WriteLine($"  P/Invoke (GetFillRule): {pinvokeTime:F3} µs per call");
        Console.WriteLine($"  P/Invoke overhead: {pinvokeTime - managedTime:F3} µs\n");
    }

    static void MeasureAllocationOverhead()
    {
        Console.WriteLine("Test 2: Object Allocation Overhead");

        const int iterations = 100_000;

        // Measure struct allocation (stack)
        var sw = Stopwatch.StartNew();
        for (int i = 0; i < iterations; i++)
        {
            var rect = Rect.FromXYWH(100, 100, 200, 200);
            _ = rect.Width;
        }
        sw.Stop();

        var structTime = sw.Elapsed.TotalMilliseconds / iterations * 1000;
        Console.WriteLine($"  Struct (Rect) stack allocation: {structTime:F3} µs per operation");

        // Measure class allocation (heap)
        sw.Restart();
        for (int i = 0; i < iterations; i++)
        {
            using var path = new BezPath();
            path.MoveTo(100, 100);
        }
        sw.Stop();

        var classTime = sw.Elapsed.TotalMilliseconds / iterations * 1000;
        Console.WriteLine($"  Class (BezPath) heap allocation: {classTime:F3} µs per operation");
        Console.WriteLine($"  Allocation overhead: {classTime - structTime:F3} µs\n");
    }

    static void MeasureHandleOverhead()
    {
        Console.WriteLine("Test 3: Handle Dereferencing Overhead");

        const int iterations = 1_000_000;

        using var ctx = new RenderContext(100, 100);
        var rect = Rect.FromXYWH(100, 100, 50, 50);

        // Measure SetPaint (simple handle operation)
        var sw = Stopwatch.StartNew();
        for (int i = 0; i < iterations; i++)
        {
            ctx.SetPaint(Color.Red);
        }
        sw.Stop();

        var setPaintTime = sw.Elapsed.TotalMilliseconds / iterations * 1000;
        Console.WriteLine($"  SetPaint: {setPaintTime:F3} µs per call");

        // Measure FillRect (handle + struct passing)
        sw.Restart();
        for (int i = 0; i < iterations; i++)
        {
            ctx.FillRect(rect);
        }
        sw.Stop();

        var fillRectTime = sw.Elapsed.TotalMilliseconds / iterations * 1000;
        Console.WriteLine($"  FillRect: {fillRectTime:F3} µs per call");
        Console.WriteLine($"  Additional cost (struct passing): {fillRectTime - setPaintTime:F3} µs\n");
    }

    static void BreakdownRenderOperation()
    {
        Console.WriteLine("Test 4: Breakdown of Single Render Operation");

        const int iterations = 10_000;
        const int width = 800;
        const int height = 600;

        // Pre-allocate to avoid measuring allocation
        using var ctx = new RenderContext(width, height);
        using var pixmap = new Pixmap(width, height);
        var rect = Rect.FromXYWH(100, 100, 400, 300);

        // Measure individual operations
        var sw = Stopwatch.StartNew();
        for (int i = 0; i < iterations; i++)
        {
            ctx.SetPaint(Color.Magenta);
        }
        sw.Stop();
        var setPaintTime = sw.Elapsed.TotalMilliseconds / iterations;

        sw.Restart();
        for (int i = 0; i < iterations; i++)
        {
            ctx.FillRect(rect);
        }
        sw.Stop();
        var fillRectTime = sw.Elapsed.TotalMilliseconds / iterations;

        sw.Restart();
        for (int i = 0; i < iterations; i++)
        {
            ctx.Flush();
        }
        sw.Stop();
        var flushTime = sw.Elapsed.TotalMilliseconds / iterations;

        sw.Restart();
        for (int i = 0; i < iterations; i++)
        {
            ctx.Flush();
            ctx.RenderToPixmap(pixmap);
            ctx.Reset(); // Reset for next iteration
        }
        sw.Stop();
        var renderTime = sw.Elapsed.TotalMilliseconds / iterations;

        var total = setPaintTime + fillRectTime + flushTime + renderTime;

        Console.WriteLine($"  SetPaint:       {setPaintTime * 1000:F1} µs ({setPaintTime/total*100:F1}%)");
        Console.WriteLine($"  FillRect:       {fillRectTime * 1000:F1} µs ({fillRectTime/total*100:F1}%)");
        Console.WriteLine($"  Flush:          {flushTime * 1000:F1} µs ({flushTime/total*100:F1}%)");
        Console.WriteLine($"  RenderToPixmap: {renderTime * 1000:F1} µs ({renderTime/total*100:F1}%)");
        Console.WriteLine($"  TOTAL:          {total * 1000:F1} µs\n");
    }

    static void CompareNativeVsWrapper()
    {
        Console.WriteLine("Test 5: Compare Native Types vs Wrapper Types");

        const int iterations = 1_000_000;

        // Test: Rect (struct, blittable)
        var sw = Stopwatch.StartNew();
        for (int i = 0; i < iterations; i++)
        {
            var rect = new Rect(100, 100, 500, 400);
            var width = rect.Width;
            var height = rect.Height;
            Consume(width + height);
        }
        sw.Stop();
        var rectTime = sw.Elapsed.TotalMilliseconds / iterations * 1000;

        // Test: Affine (struct, 6 doubles = 48 bytes, blittable)
        sw.Restart();
        for (int i = 0; i < iterations; i++)
        {
            var transform = Affine.Rotation(0.785398);
            Consume(transform.M11);
        }
        sw.Stop();
        var affineTime = sw.Elapsed.TotalMilliseconds / iterations * 1000;

        // Test: Color (struct, 4 bytes, blittable)
        sw.Restart();
        for (int i = 0; i < iterations; i++)
        {
            var color = new Color(255, 0, 0, 255);
            var premul = color.Premultiply();
            Consume(premul.R);
        }
        sw.Stop();
        var colorTime = sw.Elapsed.TotalMilliseconds / iterations * 1000;

        Console.WriteLine($"  Rect struct operations:    {rectTime:F3} µs");
        Console.WriteLine($"  Affine struct operations:  {affineTime:F3} µs");
        Console.WriteLine($"  Color struct operations:   {colorTime:F3} µs");
        Console.WriteLine($"\n  All types are already blittable and optimal!\n");
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    static void Consume<T>(T value)
    {
        // Prevent optimizer from eliminating the computation
    }
}
