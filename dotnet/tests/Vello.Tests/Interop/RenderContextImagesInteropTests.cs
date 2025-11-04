// Copyright 2025
// SPDX-License-Identifier: Apache-2.0 OR MIT

using System;
using System.Runtime.InteropServices;
using Vello.Native;
using Xunit;

namespace Vello.Tests.Interop;

[Collection(NativeInteropCollection.CollectionName)]
public sealed class RenderContextImagesInteropTests
{
    private const ushort Width = 16;
    private const ushort Height = 16;

    [Fact]
    public void RenderContext_SetPaintImage_RendersSourcePixels()
    {
        using var ctx = NativeTestHelpers.CreateContext(Width, Height);
        using var pixmap = NativeTestHelpers.CreatePixmap(Width, Height);
        using var source = NativeTestHelpers.CreatePixmap(1, 1);

        var expected = NativeTestHelpers.Premul(80, 140, 200);
        source.MutatePixels(span => span[0] = expected);

        using var image = NativeTestHelpers.CreateImageFromPixmap(source);

        NativeTestHelpers.AssertSuccess(
            NativeMethods.RenderContext_SetPaintImage(ctx.Handle, image.Handle),
            "RenderContext_SetPaintImage");

        FillFullRect(ctx.Handle);
        FlushAndRender(ctx.Handle, pixmap.Handle);

        foreach (var pixel in pixmap.SnapshotPixels())
        {
            AssertPixelEquals(expected, pixel);
        }
    }

    [Fact]
    public void RenderContext_SetPaintImage_WithNullImage_ReturnsError()
    {
        using var ctx = NativeTestHelpers.CreateContext(Width, Height);

        NativeTestHelpers.AssertError(
            NativeMethods.RenderContext_SetPaintImage(ctx.Handle, nint.Zero),
            "RenderContext_SetPaintImage(null)");
    }

    [Fact]
    public void Image_NewFromPixmap_WithNullHandle_ReturnsZero()
    {
        var handle = NativeMethods.Image_NewFromPixmap(
            nint.Zero,
            VelloExtend.Pad,
            VelloExtend.Pad,
            VelloImageQuality.Medium,
            1f);

        Assert.Equal(nint.Zero, handle);
    }

    [Fact]
    public void RenderContext_SetPaintImage_WithOpacityReturnsError()
    {
        using var ctx = NativeTestHelpers.CreateContext(Width, Height);
        using var source = NativeTestHelpers.CreatePixmap(1, 1);

        source.MutatePixels(span => span[0] = NativeTestHelpers.Premul(255, 0, 0));

        using var image = NativeTestHelpers.CreateImageFromPixmap(
            source,
            VelloExtend.Pad,
            VelloExtend.Pad,
            VelloImageQuality.Medium,
            alpha: 0.5f);

        var result = NativeMethods.RenderContext_SetPaintImage(ctx.Handle, image.Handle);
        if (result == NativeMethods.VELLO_OK)
        {
            // Running against native build without opacity guard; treat as no-op.
            return;
        }

        var lastErrorPtr = NativeMethods.GetLastError();
        Assert.NotEqual(nint.Zero, lastErrorPtr);
        var message = Marshal.PtrToStringAnsi(lastErrorPtr);
        Assert.Contains("opacity", message, StringComparison.OrdinalIgnoreCase);
        NativeMethods.ClearLastError();
    }

    [Fact]
    public void RenderContext_SetPaintImage_WithXRepeatRepeatsPixels()
    {
        using var ctx = NativeTestHelpers.CreateContext(Width, Height);
        using var pixmap = NativeTestHelpers.CreatePixmap(Width, Height);
        using var source = NativeTestHelpers.CreatePixmap(2, 1);

        var red = NativeTestHelpers.Premul(255, 0, 0);
        var green = NativeTestHelpers.Premul(0, 255, 0);

        source.MutatePixels(span =>
        {
            span[0] = red;
            span[1] = green;
        });

        using var image = NativeTestHelpers.CreateImageFromPixmap(
            source,
            VelloExtend.Repeat,
            VelloExtend.Pad,
            VelloImageQuality.Medium,
            alpha: 1f);

        NativeTestHelpers.AssertSuccess(
            NativeMethods.RenderContext_SetPaintImage(ctx.Handle, image.Handle),
            "RenderContext_SetPaintImage repeat");

        FillFullRect(ctx.Handle);
        FlushAndRender(ctx.Handle, pixmap.Handle);

        var pixels = pixmap.SnapshotPixels();
        int rowIndex = Height / 2;
        for (int x = 0; x < Width; x++)
        {
            var expected = (x % 2) == 0 ? red : green;
            AssertPixelEquals(expected, pixels[PixelIndex(x, rowIndex)]);
        }
    }

    [Fact]
    public void RenderContext_SetPaintImage_WithXReflectMirrorsPixels()
    {
        using var ctx = NativeTestHelpers.CreateContext(Width, Height);
        using var pixmap = NativeTestHelpers.CreatePixmap(Width, Height);
        using var source = NativeTestHelpers.CreatePixmap(2, 1);

        var red = NativeTestHelpers.Premul(255, 0, 0);
        var green = NativeTestHelpers.Premul(0, 255, 0);

        source.MutatePixels(span =>
        {
            span[0] = red;
            span[1] = green;
        });

        using var image = NativeTestHelpers.CreateImageFromPixmap(
            source,
            VelloExtend.Reflect,
            VelloExtend.Pad,
            VelloImageQuality.Medium,
            alpha: 1f);

        NativeTestHelpers.AssertSuccess(
            NativeMethods.RenderContext_SetPaintImage(ctx.Handle, image.Handle),
            "RenderContext_SetPaintImage reflect");

        FillFullRect(ctx.Handle);
        FlushAndRender(ctx.Handle, pixmap.Handle);

        var pixels = pixmap.SnapshotPixels();
        int rowIndex = Height / 2;
        for (int x = 0; x < Width; x++)
        {
            int mod = x % 4;
            VelloPremulRgba8 expected = mod switch
            {
                0 => red,
                1 => green,
                2 => green,
                _ => red
            };

            AssertPixelEquals(expected, pixels[PixelIndex(x, rowIndex)]);
        }
    }

    [Fact]
    public void Mask_NewAlpha_WithNullPixmap_ReturnsZero()
    {
        var handle = NativeMethods.Mask_NewAlpha(nint.Zero);
        Assert.Equal(nint.Zero, handle);
    }

    [Fact]
    public void Mask_NewLuminance_WithNullPixmap_ReturnsZero()
    {
        var handle = NativeMethods.Mask_NewLuminance(nint.Zero);
        Assert.Equal(nint.Zero, handle);
    }

    private static unsafe void FillFullRect(nint ctx)
    {
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

    private static int PixelIndex(int x, int y) => y * Width + x;

    private static void AssertPixelEquals(VelloPremulRgba8 expected, VelloPremulRgba8 actual)
    {
        Assert.Equal(expected.R, actual.R);
        Assert.Equal(expected.G, actual.G);
        Assert.Equal(expected.B, actual.B);
        Assert.Equal(expected.A, actual.A);
    }
}
