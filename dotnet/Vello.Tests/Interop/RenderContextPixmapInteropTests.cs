// Copyright 2025
// SPDX-License-Identifier: Apache-2.0 OR MIT

using System;
using Vello.Native;
using Vello.Native.FastPath;
using Xunit;

namespace Vello.Tests.Interop;

[Collection(NativeInteropCollection.CollectionName)]
public sealed class RenderContextPixmapInteropTests
{
    private const ushort Width = 12;
    private const ushort Height = 10;

    [Fact]
    public void RenderContext_New_ReturnsValidHandle()
    {
        using var ctx = NativeTestHelpers.CreateContext(Width, Height);
        Assert.NotEqual(nint.Zero, ctx.Handle);
    }

    [Fact]
    public void RenderContext_WidthHeight_MatchCreationParameters()
    {
        using var ctx = NativeTestHelpers.CreateContext(Width, Height);

        Assert.Equal(Width, NativeMethods.RenderContext_Width(ctx.Handle));
        Assert.Equal(Height, NativeMethods.RenderContext_Height(ctx.Handle));
    }

    [Fact]
    public void RenderContext_Reset_AllowsReissueOfCommands()
    {
        using var ctx = NativeTestHelpers.CreateContext(Width, Height);
        using var pixmap = NativeTestHelpers.CreatePixmap(Width, Height);

        DrawSolidRect(ctx.Handle, 10, 20, 30, 255);
        FlushAndRender(ctx.Handle, pixmap.Handle);
        AssertSolidColor(pixmap, NativeTestHelpers.Premul(10, 20, 30, 255));

        NativeTestHelpers.AssertSuccess(
            NativeMethods.RenderContext_Reset(ctx.Handle),
            "RenderContext_Reset");

        using var emptyPixmap = NativeTestHelpers.CreatePixmap(Width, Height);
        FlushAndRender(ctx.Handle, emptyPixmap.Handle);
        AssertAllTransparent(emptyPixmap);
    }

    [Fact]
    public void RenderContext_Reset_AllowsSubsequentDrawing()
    {
        using var ctx = NativeTestHelpers.CreateContext(Width, Height);
        using var pixmap = NativeTestHelpers.CreatePixmap(Width, Height);

        DrawSolidRect(ctx.Handle, 0, 0, 0, 255);
        NativeTestHelpers.AssertSuccess(
            NativeMethods.RenderContext_Reset(ctx.Handle),
            "RenderContext_Reset");

        DrawSolidRect(ctx.Handle, 5, 15, 25, 255);
        FlushAndRender(ctx.Handle, pixmap.Handle);

        AssertSolidColor(pixmap, NativeTestHelpers.Premul(5, 15, 25));
    }

    [Fact]
    public void RenderContext_RenderToPixmap_FillsWithSolidColor()
    {
        using var ctx = NativeTestHelpers.CreateContext(Width, Height);
        using var pixmap = NativeTestHelpers.CreatePixmap(Width, Height);

        DrawSolidRect(ctx.Handle, 255, 0, 0, 255);
        FlushAndRender(ctx.Handle, pixmap.Handle);

        AssertSolidColor(pixmap, NativeTestHelpers.Premul(255, 0, 0));
    }

    [Fact]
    public void Pixmap_New_ReportsDimensions()
    {
        using var pixmap = NativeTestHelpers.CreatePixmap(Width, Height);

        Assert.Equal(Width, NativeMethods.Pixmap_Width(pixmap.Handle));
        Assert.Equal(Height, NativeMethods.Pixmap_Height(pixmap.Handle));
    }

    [Fact]
    public void Pixmap_Resize_UpdatesDimensions()
    {
        using var pixmap = NativeTestHelpers.CreatePixmap(Width, Height);

        NativeTestHelpers.AssertSuccess(
            NativeMethods.Pixmap_Resize(pixmap.Handle, 20, 24),
            "Pixmap_Resize");

        Assert.Equal(20, NativeMethods.Pixmap_Width(pixmap.Handle));
        Assert.Equal(24, NativeMethods.Pixmap_Height(pixmap.Handle));
    }

    [Fact]
    public void Pixmap_Data_ReturnsExpectedPixelCount()
    {
        using var pixmap = NativeTestHelpers.CreatePixmap(Width, Height);

        nuint length = pixmap.GetPixelCount();
        Assert.Equal((nuint)(Width * Height), length);
    }

    [Fact]
    public void RenderContext_CreatesIndependentContexts()
    {
        using var ctxA = NativeTestHelpers.CreateContext(Width, Height);
        using var ctxB = NativeTestHelpers.CreateContext(Width, Height);
        using var pixmapA = NativeTestHelpers.CreatePixmap(Width, Height);
        using var pixmapB = NativeTestHelpers.CreatePixmap(Width, Height);

        DrawSolidRect(ctxA.Handle, 200, 10, 10, 255);
        DrawSolidRect(ctxB.Handle, 10, 200, 10, 255);

        FlushAndRender(ctxA.Handle, pixmapA.Handle);
        FlushAndRender(ctxB.Handle, pixmapB.Handle);

        AssertSolidColor(pixmapA, NativeTestHelpers.Premul(200, 10, 10));
        AssertSolidColor(pixmapB, NativeTestHelpers.Premul(10, 200, 10));
    }

    [Fact]
    public void RenderContext_New_WithZeroDimensions_YieldsZeroSizedContext()
    {
        nint ctx = NativeMethods.RenderContext_New(0, Height);
        Assert.NotEqual(nint.Zero, ctx);

        try
        {
            Assert.Equal((ushort)0, NativeMethods.RenderContext_Width(ctx));
            Assert.Equal(Height, NativeMethods.RenderContext_Height(ctx));
        }
        finally
        {
            NativeMethods.RenderContext_Free(ctx);
        }
    }

    [Fact]
    public void RenderContext_Reset_WithNullHandle_ReturnsError()
    {
        NativeTestHelpers.AssertError(
            NativeMethods.RenderContext_Reset(nint.Zero),
            "RenderContext_Reset(null)");
    }

    [Fact]
    public void RenderContext_SetPaintSolid_WithNullHandle_ReturnsError()
    {
        NativeTestHelpers.AssertError(
            NativeMethods.RenderContext_SetPaintSolid(nint.Zero, 1, 2, 3, 4),
            "RenderContext_SetPaintSolid(null)");
    }

    [Fact]
    public void RenderContext_RenderToPixmap_WithNullPixmap_ReturnsError()
    {
        using var ctx = NativeTestHelpers.CreateContext(Width, Height);

        NativeTestHelpers.AssertError(
            NativeMethods.RenderContext_RenderToPixmap(ctx.Handle, nint.Zero),
            "RenderContext_RenderToPixmap(null)");
    }

    [Fact]
    public void RenderContext_Flush_WithNullHandle_ReturnsError()
    {
        NativeTestHelpers.AssertError(
            NativeMethods.RenderContext_Flush(nint.Zero),
            "RenderContext_Flush(null)");
    }

    [Fact]
    public unsafe void RenderContext_FillRect_DrawsWithinSpecifiedBounds()
    {
        using var ctx = NativeTestHelpers.CreateContext(Width, Height);
        using var pixmap = NativeTestHelpers.CreatePixmap(Width, Height);
        var expected = NativeTestHelpers.Premul(40, 50, 60);

        NativeTestHelpers.AssertSuccess(
            NativeMethods.RenderContext_SetPaintSolid(ctx.Handle, expected.R, expected.G, expected.B, expected.A),
            "RenderContext_SetPaintSolid(bounds test)");

        VelloRect rect = new()
        {
            X0 = 2,
            Y0 = 3,
            X1 = 6,
            Y1 = 7
        };

        NativeTestHelpers.AssertSuccess(
            NativeMethods.RenderContext_FillRect(ctx.Handle, &rect),
            "RenderContext_FillRect(bounds test)");

        FlushAndRender(ctx.Handle, pixmap.Handle);

        var pixels = pixmap.SnapshotPixels();
        for (int y = 0; y < Height; y++)
        {
            for (int x = 0; x < Width; x++)
            {
                var pixel = pixels[y * Width + x];
                bool withinBounds = x >= rect.X0 && x < rect.X1 && y >= rect.Y0 && y < rect.Y1;
                if (withinBounds)
                {
                    AssertPixelEquals(expected, pixel);
                }
                else
                {
                    AssertTransparent(pixel);
                }
            }
        }
    }

    [Fact]
    public void Pixmap_Resize_WithNullPixmap_ReturnsError()
    {
        NativeTestHelpers.AssertError(
            NativeMethods.Pixmap_Resize(nint.Zero, Width, Height),
            "Pixmap_Resize(null)");
    }

    [Fact]
    public void Pixmap_Data_WithNullPixmap_ReturnsError()
    {
        unsafe
        {
            nint data = nint.Zero;
            nuint len = 0;
            NativeTestHelpers.AssertError(
                NativeMethods.Pixmap_Data(nint.Zero, &data, &len),
                "Pixmap_Data(null)");
        }
    }

    [Fact]
    public void Pixmap_Data_WithNullOutputPointer_ReturnsError()
    {
        using var pixmap = NativeTestHelpers.CreatePixmap(Width, Height);
        unsafe
        {
            nuint* len = stackalloc nuint[1];
            NativeTestHelpers.AssertError(
                NativeMethods.Pixmap_Data(pixmap.Handle, (nint*)0, len),
                "Pixmap_Data(out_ptr=null)");

            nint* ptr = stackalloc nint[1];
            NativeTestHelpers.AssertError(
                NativeMethods.Pixmap_Data(pixmap.Handle, ptr, (nuint*)0),
                "Pixmap_Data(out_len=null)");
        }
    }

    [Fact]
    public void Pixmap_DataMut_WithNullOutputPointer_ReturnsError()
    {
        using var pixmap = NativeTestHelpers.CreatePixmap(Width, Height);
        unsafe
        {
            nuint* len = stackalloc nuint[1];
            NativeTestHelpers.AssertError(
                NativeMethods.Pixmap_DataMut(pixmap.Handle, (nint*)0, len),
                "Pixmap_DataMut(out_ptr=null)");

            nint* ptr = stackalloc nint[1];
            NativeTestHelpers.AssertError(
                NativeMethods.Pixmap_DataMut(pixmap.Handle, ptr, (nuint*)0),
                "Pixmap_DataMut(out_len=null)");
        }
    }

    [Fact]
    public void Pixmap_DataMut_AllowsPixelMutation()
    {
        using var pixmap = NativeTestHelpers.CreatePixmap(Width, Height);
        var expected = NativeTestHelpers.Premul(12, 34, 56, 78);

        pixmap.MutatePixels(span =>
        {
            span.Clear();
            span[0] = expected;
        });

        var pixels = pixmap.SnapshotPixels();
        AssertPixelEquals(expected, pixels[0]);

        for (int i = 1; i < pixels.Length; i++)
        {
            AssertTransparent(pixels[i]);
        }
    }

    [Fact]
    public unsafe void Pixmap_Sample_ReturnsExpectedPixel()
    {
        using var pixmap = NativeTestHelpers.CreatePixmap(Width, Height);
        var expected = NativeTestHelpers.Premul(90, 80, 70, 255);
        int index = (Height / 2) * Width + (Width / 2);

        pixmap.MutatePixels(span =>
        {
            span.Clear();
            span[index] = expected;
        });

        VelloPremulRgba8 sampled = default;
        NativeTestHelpers.AssertSuccess(
            NativeMethods.Pixmap_Sample(
                pixmap.Handle,
                (ushort)(Width / 2),
                (ushort)(Height / 2),
                &sampled),
            "Pixmap_Sample");

        AssertPixelEquals(expected, sampled);
    }

    [Fact]
    public unsafe void Pixmap_Sample_WithOutOfBoundsCoordinates_ReturnsError()
    {
        using var pixmap = NativeTestHelpers.CreatePixmap(Width, Height);
        VelloPremulRgba8 sampled = default;

        NativeTestHelpers.AssertError(
            NativeMethods.Pixmap_Sample(
                pixmap.Handle,
                Width,
                0,
                &sampled),
            "Pixmap_Sample(x>=width)");

        NativeTestHelpers.AssertError(
            NativeMethods.Pixmap_Sample(
                pixmap.Handle,
                0,
                Height,
                &sampled),
            "Pixmap_Sample(y>=height)");
    }

    [Fact]
    public unsafe void Pixmap_Sample_WithNullArguments_ReturnsError()
    {
        using var pixmap = NativeTestHelpers.CreatePixmap(Width, Height);
        VelloPremulRgba8 sampled = default;

        NativeTestHelpers.AssertError(
            NativeMethods.Pixmap_Sample(
                nint.Zero,
                0,
                0,
                &sampled),
            "Pixmap_Sample(null pixmap)");

        NativeTestHelpers.AssertError(
            NativeMethods.Pixmap_Sample(
                pixmap.Handle,
                0,
                0,
                null),
            "Pixmap_Sample(null out)");
    }

    private static unsafe void DrawSolidRect(nint ctx, byte r, byte g, byte b, byte a)
    {
        NativeTestHelpers.AssertSuccess(
            NativeMethods.RenderContext_SetPaintSolid(ctx, r, g, b, a),
            "RenderContext_SetPaintSolid");

        VelloRect rect = new()
        {
            X0 = 0,
            Y0 = 0,
            X1 = Width,
            Y1 = Height
        };

        NativeTestHelpers.AssertSuccess(
            NativeMethods.RenderContext_FillRect(ctx, &rect),
            "RenderContext_FillRect");
    }

    private static void FlushAndRender(nint ctx, nint pixmap)
    {
        NativeTestHelpers.AssertSuccess(
            NativeMethods.RenderContext_Flush(ctx),
            "RenderContext_Flush");

        NativeTestHelpers.AssertSuccess(
            NativeMethods.RenderContext_RenderToPixmap(ctx, pixmap),
            "RenderContext_RenderToPixmap");
    }

    private static void AssertSolidColor(NativePixmap pixmap, VelloPremulRgba8 expected)
    {
        foreach (var pixel in pixmap.SnapshotPixels())
        {
            AssertPixelEquals(expected, pixel);
        }
    }

    private static void AssertAllTransparent(NativePixmap pixmap)
    {
        foreach (var pixel in pixmap.SnapshotPixels())
        {
            AssertTransparent(pixel);
        }
    }

    private static void AssertPixelEquals(VelloPremulRgba8 expected, VelloPremulRgba8 actual)
    {
        Assert.Equal(expected.R, actual.R);
        Assert.Equal(expected.G, actual.G);
        Assert.Equal(expected.B, actual.B);
        Assert.Equal(expected.A, actual.A);
    }

    private static void AssertTransparent(VelloPremulRgba8 pixel)
    {
        Assert.InRange(pixel.A, 0, 5);
        Assert.Equal(0, pixel.R);
        Assert.Equal(0, pixel.G);
        Assert.Equal(0, pixel.B);
    }
}
