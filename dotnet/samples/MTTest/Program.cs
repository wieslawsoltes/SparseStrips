// Copyright 2025 Wieslaw Soltes
// SPDX-License-Identifier: Apache-2.0 OR MIT

using System;
using System.Diagnostics;
using Vello;
using Vello.Geometry;

Console.WriteLine("=== Vello MT Test ===\n");

// Test ST
Console.WriteLine("Testing Single Thread (numThreads=0):");
var stTime = TestRender(numThreads: 0, label: "ST");

Console.WriteLine("\nTesting Multi Thread (numThreads=8):");
var mtTime = TestRender(numThreads: 8, label: "MT");

Console.WriteLine($"\n=== Results ===");
Console.WriteLine($"ST: {stTime}ms");
Console.WriteLine($"MT: {mtTime}ms");
Console.WriteLine($"Speedup: {(double)stTime / mtTime:F2}x");

if (mtTime >= stTime * 0.9)
{
    Console.WriteLine("\n⚠️  WARNING: No significant MT speedup detected! Bug confirmed.");
}
else
{
    Console.WriteLine("\n✓ MT speedup working as expected.");
}

long TestRender(ushort numThreads, string label)
{
    // Create context with specified thread count
    using var ctx = new RenderContext(800, 600, new RenderSettings(
        level: SimdLevel.Avx2,
        numThreads: numThreads,
        mode: RenderMode.OptimizeSpeed
    ));

    // Verify settings were applied
    var settings = ctx.GetRenderSettings();
    Console.WriteLine($"  Created context: NumThreads={settings.NumThreads}, Level={settings.Level}, Mode={settings.Mode}");

    // Create pixmap for rendering
    using var pixmap = new Pixmap(800, 600);

    // Warmup
    for (int i = 0; i < 10; i++)
    {
        ctx.SetPaint(new Color((byte)(i % 255), 100, 200, 255));
        ctx.FillRect(Rect.FromXYWH(i * 5.0, i * 3.0, 50, 50));
    }
    ctx.Flush();
    ctx.RenderToPixmap(pixmap);
    ctx.Reset();

    // Timed run - complex scene with 100 shapes
    var sw = Stopwatch.StartNew();

    for (int i = 0; i < 100; i++)
    {
        ctx.SetPaint(new Color((byte)(i % 255), (byte)((i * 2) % 255), (byte)((i * 3) % 255), 255));
        ctx.FillRect(Rect.FromXYWH(i * 5.0, i * 3.0, 50, 50));
    }

    ctx.Flush();
    ctx.RenderToPixmap(pixmap);

    sw.Stop();
    var elapsed = sw.ElapsedMilliseconds;

    Console.WriteLine($"  Elapsed: {elapsed}ms");

    return elapsed;
}
