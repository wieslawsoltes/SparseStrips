// Copyright 2025 Wieslaw Soltes
// SPDX-License-Identifier: Apache-2.0 OR MIT

using Vello;
using Vello.Geometry;
using Xunit;

namespace Vello.Tests;

public class PaintGetterTests
{
    [Fact]
    public void GetPaintKind_Solid_ReturnsCorrectKind()
    {
        using var context = new RenderContext(100, 100);
        context.SetPaint(new Color(255, 128, 64, 200));

        Assert.Equal(PaintKind.Solid, context.GetPaintKind());
    }

    [Fact]
    public void GetPaintKind_LinearGradient_ReturnsCorrectKind()
    {
        using var context = new RenderContext(100, 100);

        Span<ColorStop> stops = stackalloc ColorStop[2];
        stops[0] = new ColorStop(0.0f, Color.Red);
        stops[1] = new ColorStop(1.0f, Color.Blue);

        context.SetPaintLinearGradient(
            0, 0, 100, 100,
            stops,
            GradientExtend.Pad);

        Assert.Equal(PaintKind.LinearGradient, context.GetPaintKind());
    }

    [Fact]
    public void GetPaintKind_RadialGradient_ReturnsCorrectKind()
    {
        using var context = new RenderContext(100, 100);

        Span<ColorStop> stops = stackalloc ColorStop[2];
        stops[0] = new ColorStop(0.0f, Color.White);
        stops[1] = new ColorStop(1.0f, Color.Black);

        context.SetPaintRadialGradient(
            50, 50, 40,
            stops,
            GradientExtend.Pad);

        Assert.Equal(PaintKind.RadialGradient, context.GetPaintKind());
    }

    [Fact]
    public void GetPaintKind_SweepGradient_ReturnsCorrectKind()
    {
        using var context = new RenderContext(100, 100);

        Span<ColorStop> stops = stackalloc ColorStop[2];
        stops[0] = new ColorStop(0.0f, Color.Red);
        stops[1] = new ColorStop(1.0f, Color.Blue);

        context.SetPaintSweepGradient(
            50, 50, 0.0f, 360.0f,
            stops,
            GradientExtend.Pad);

        Assert.Equal(PaintKind.SweepGradient, context.GetPaintKind());
    }

    [Fact]
    public void GetPaintKind_Image_ReturnsCorrectKind()
    {
        using var context = new RenderContext(100, 100);
        using var pixmap = new Pixmap(50, 50);
        using var image = Image.FromPixmap(pixmap, GradientExtend.Pad, GradientExtend.Pad);

        context.SetPaintImage(image);

        Assert.Equal(PaintKind.Image, context.GetPaintKind());
    }

    [Fact]
    public void GetPaintKind_AfterMultiplePaintChanges_ReturnsLatest()
    {
        using var context = new RenderContext(100, 100);

        // Start with solid
        context.SetPaint(Color.Red);
        Assert.Equal(PaintKind.Solid, context.GetPaintKind());

        // Change to linear gradient
        Span<ColorStop> stops = stackalloc ColorStop[2];
        stops[0] = new ColorStop(0.0f, Color.Red);
        stops[1] = new ColorStop(1.0f, Color.Blue);

        context.SetPaintLinearGradient(0, 0, 100, 100, stops, GradientExtend.Pad);
        Assert.Equal(PaintKind.LinearGradient, context.GetPaintKind());

        // Change back to solid
        context.SetPaint(Color.Green);
        Assert.Equal(PaintKind.Solid, context.GetPaintKind());
    }

    [Fact]
    public void GetPaintKind_DefaultPaint_ReturnsSolid()
    {
        using var context = new RenderContext(100, 100);

        // Default paint should be solid black
        Assert.Equal(PaintKind.Solid, context.GetPaintKind());
    }

    [Fact]
    public void GetPaintKind_AfterReset_PreservesPaint()
    {
        using var context = new RenderContext(100, 100);

        // Set a gradient
        Span<ColorStop> stops = stackalloc ColorStop[2];
        stops[0] = new ColorStop(0.0f, Color.Red);
        stops[1] = new ColorStop(1.0f, Color.Blue);

        context.SetPaintRadialGradient(50, 50, 40, stops, GradientExtend.Pad);
        Assert.Equal(PaintKind.RadialGradient, context.GetPaintKind());

        // Reset clears the scene but preserves paint state
        context.Reset();
        Assert.Equal(PaintKind.RadialGradient, context.GetPaintKind());
    }
}
