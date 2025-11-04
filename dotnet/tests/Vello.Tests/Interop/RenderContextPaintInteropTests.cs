// Copyright 2025
// SPDX-License-Identifier: Apache-2.0 OR MIT

using System;
using System.Runtime.InteropServices;
using Vello.Native;
using Vello.Native.FastPath;
using Xunit;

namespace Vello.Tests.Interop;

[Collection(NativeInteropCollection.CollectionName)]
public sealed class RenderContextPaintInteropTests
{
    private const ushort Width = 24;
    private const ushort Height = 12;

    [Fact]
    public unsafe void RenderContext_SetPaintLinearGradient_RendersExpectedEdgeColors()
    {
        using var ctx = NativeTestHelpers.CreateContext(Width, Height);
        using var pixmap = NativeTestHelpers.CreatePixmap(Width, Height);

        var stops = stackalloc VelloColorStop[2];
        stops[0] = new VelloColorStop { Offset = 0f, R = 0, G = 0, B = 0, A = 255 };
        stops[1] = new VelloColorStop { Offset = 1f, R = 255, G = 255, B = 255, A = 255 };

        NativeTestHelpers.AssertSuccess(
            NativeMethods.RenderContext_SetPaintLinearGradient(
                ctx.Handle,
                0,
                0,
                Width,
                0,
                stops,
                2,
                VelloExtend.Pad),
            "RenderContext_SetPaintLinearGradient");

        FillFullRect(ctx);
        FlushAndRender(ctx.Handle, pixmap.Handle);

        var pixels = pixmap.SnapshotPixels();
        AssertPixelApprox(NativeTestHelpers.Premul(0, 0, 0), pixels[PixelIndex(0, 0)]);
        AssertPixelApprox(NativeTestHelpers.Premul(255, 255, 255), pixels[PixelIndex(Width - 1, 0)]);
        AssertGradientMonotonic(pixels);
    }

    [Fact]
    public unsafe void RenderContext_SetPaintLinearGradient_WithRepeatExtend_RepeatsPattern()
    {
        using var ctx = NativeTestHelpers.CreateContext(Width, Height);
        using var pixmap = NativeTestHelpers.CreatePixmap(Width, Height);

        var stops = stackalloc VelloColorStop[2];
        stops[0] = new VelloColorStop { Offset = 0f, R = 0, G = 0, B = 0, A = 255 };
        stops[1] = new VelloColorStop { Offset = 1f, R = 255, G = 255, B = 255, A = 255 };

        double span = Width / 4.0;

        NativeTestHelpers.AssertSuccess(
            NativeMethods.RenderContext_SetPaintLinearGradient(
                ctx.Handle,
                0,
                0,
                span,
                0,
                stops,
                2,
                VelloExtend.Repeat),
            "RenderContext_SetPaintLinearGradient repeat");

        FillFullRect(ctx);
        FlushAndRender(ctx.Handle, pixmap.Handle);

        var pixels = pixmap.SnapshotPixels();
        int midRow = Height / 2;
        int segment = Width / 4;
        var samples = new[] { 1, segment + 1, (2 * segment) + 1, (3 * segment) + 1 };

        foreach (int x in samples)
        {
            var expected = ExpectedLinearGradientColor(PixelCenter(x), 0, span, VelloExtend.Repeat);
            AssertPixelApprox(expected, pixels[PixelIndex(x, midRow)]);
        }
    }

    [Fact]
    public unsafe void RenderContext_SetPaintLinearGradient_WithReflectExtend_MirrorsPattern()
    {
        using var ctx = NativeTestHelpers.CreateContext(Width, Height);
        using var pixmap = NativeTestHelpers.CreatePixmap(Width, Height);

        var stops = stackalloc VelloColorStop[2];
        stops[0] = new VelloColorStop { Offset = 0f, R = 0, G = 0, B = 0, A = 255 };
        stops[1] = new VelloColorStop { Offset = 1f, R = 255, G = 255, B = 255, A = 255 };

        double span = Width / 4.0;

        NativeTestHelpers.AssertSuccess(
            NativeMethods.RenderContext_SetPaintLinearGradient(
                ctx.Handle,
                0,
                0,
                span,
                0,
                stops,
                2,
                VelloExtend.Reflect),
            "RenderContext_SetPaintLinearGradient reflect");

        FillFullRect(ctx);
        FlushAndRender(ctx.Handle, pixmap.Handle);

        var pixels = pixmap.SnapshotPixels();
        int midRow = Height / 2;
        int segment = Width / 4;

        for (int offset = 0; offset <= segment; offset += Math.Max(1, segment / 2))
        {
            int xForward = offset;
            int xMirror = segment + offset;
            var expectedForward = ExpectedLinearGradientColor(PixelCenter(xForward), 0, span, VelloExtend.Reflect);
            var expectedMirror = ExpectedLinearGradientColor(PixelCenter(xMirror), 0, span, VelloExtend.Reflect);

            AssertPixelApprox(expectedForward, pixels[PixelIndex(xForward, midRow)]);
            AssertPixelApprox(expectedMirror, pixels[PixelIndex(xMirror, midRow)]);
        }
    }

    [Fact]
    public unsafe void RenderContext_SetPaintLinearGradient_DiagonalRepeat_MatchesExpectations()
    {
        using var ctx = NativeTestHelpers.CreateContext(Width, Height);
        using var pixmap = NativeTestHelpers.CreatePixmap(Width, Height);

        var stops = stackalloc VelloColorStop[2];
        stops[0] = new VelloColorStop { Offset = 0f, R = 0, G = 0, B = 0, A = 255 };
        stops[1] = new VelloColorStop { Offset = 1f, R = 255, G = 255, B = 255, A = 255 };

        double spanX = Width / 4.0;
        double spanY = Height / 4.0;

        NativeTestHelpers.AssertSuccess(
            NativeMethods.RenderContext_SetPaintLinearGradient(
                ctx.Handle,
                0,
                0,
                spanX,
                spanY,
                stops,
                2,
                VelloExtend.Repeat),
            "RenderContext_SetPaintLinearGradient diagonal repeat");

        FillFullRect(ctx);
        FlushAndRender(ctx.Handle, pixmap.Handle);

        var pixels = pixmap.SnapshotPixels();
        var samplePoints = new (int x, int y)[]
        {
            (1, 1),
            ((int)Math.Round(spanX) + 1, (int)Math.Round(spanY) + 1),
            ((int)Math.Round(2 * spanX) + 1, (int)Math.Round(2 * spanY) + 1)
        };

        foreach (var (x, y) in samplePoints)
        {
            if (x >= Width || y >= Height)
            {
                continue;
            }

            var expected = ExpectedLinearGradientColor2D(
                PixelCenter(x),
                PixelCenter(y),
                0,
                0,
                spanX,
                spanY,
                VelloExtend.Repeat);

            AssertPixelApprox(expected, pixels[PixelIndex(x, y)]);
        }
    }

    [Fact]
    public unsafe void RenderContext_GetPaintKind_ReturnsGradient()
    {
        using var ctx = NativeTestHelpers.CreateContext(Width, Height);

        var stops = stackalloc VelloColorStop[2];
        stops[0] = new VelloColorStop { Offset = 0f, R = 0, G = 0, B = 0, A = 255 };
        stops[1] = new VelloColorStop { Offset = 1f, R = 255, G = 0, B = 0, A = 255 };

        NativeTestHelpers.AssertSuccess(
            NativeMethods.RenderContext_SetPaintLinearGradient(
                ctx.Handle,
                0,
                0,
                Width,
                Height,
                stops,
                2,
                VelloExtend.Pad),
            "RenderContext_SetPaintLinearGradient");

        Assert.Equal(VelloPaintKind.LinearGradient, NativeMethods.RenderContext_GetPaintKind(ctx.Handle));
    }

    [Fact]
    public unsafe void RenderContext_SetTransform_AppliesTranslation()
    {
        using var ctx = NativeTestHelpers.CreateContext(Width, Height);
        using var pixmap = NativeTestHelpers.CreatePixmap(Width, Height);

        NativeTestHelpers.AssertSuccess(
            NativeMethods.RenderContext_SetPaintSolid(ctx.Handle, 0, 0, 255, 255),
            "RenderContext_SetPaintSolid");

        VelloAffine translate = new()
        {
            M11 = 1,
            M12 = 0,
            M13 = 5,
            M21 = 0,
            M22 = 1,
            M23 = 3
        };

        NativeTestHelpers.AssertSuccess(
            NativeMethods.RenderContext_SetTransform(ctx.Handle, &translate),
            "RenderContext_SetTransform");

        VelloRect rect = new()
        {
            X0 = 0,
            Y0 = 0,
            X1 = 4,
            Y1 = 4
        };

        NativeTestHelpers.AssertSuccess(
            NativeMethods.RenderContext_FillRect(ctx.Handle, &rect),
            "RenderContext_FillRect");

        FlushAndRender(ctx.Handle, pixmap.Handle);

        var pixels = pixmap.SnapshotPixels();

        // Original origin should remain transparent.
        AssertTransparent(pixels[PixelIndex(0, 0)]);

        // Translated region should be filled.
        for (int y = 3; y < 7; y++)
        {
            for (int x = 5; x < 9; x++)
            {
                AssertPixelEquals(NativeTestHelpers.Premul(0, 0, 255), pixels[PixelIndex(x, y)]);
            }
        }
    }

    [Fact]
    public unsafe void RenderContext_GetTransform_RoundTripsValues()
    {
        using var ctx = NativeTestHelpers.CreateContext(Width, Height);

        VelloAffine translate = new()
        {
            M11 = 1,
            M12 = 2,
            M13 = 3,
            M21 = 4,
            M22 = 5,
            M23 = 6
        };

        NativeTestHelpers.AssertSuccess(
            NativeMethods.RenderContext_SetTransform(ctx.Handle, &translate),
            "RenderContext_SetTransform");

        VelloAffine output = default;
        NativeTestHelpers.AssertSuccess(
            NativeMethods.RenderContext_GetTransform(ctx.Handle, &output),
            "RenderContext_GetTransform");

        AssertAffineEquals(translate, output);

        NativeTestHelpers.AssertSuccess(
            NativeMethods.RenderContext_ResetTransform(ctx.Handle),
            "RenderContext_ResetTransform");

        VelloAffine identity = default;
        NativeTestHelpers.AssertSuccess(
            NativeMethods.RenderContext_GetTransform(ctx.Handle, &identity),
            "RenderContext_GetTransform (identity)");

        AssertAffineEquals(new VelloAffine { M11 = 1, M22 = 1 }, identity);
    }

    [Fact]
    public unsafe void RenderContext_SetPaintRadialGradient_RendersCenterAndEdge()
    {
        using var ctx = NativeTestHelpers.CreateContext(Width, Height);
        using var pixmap = NativeTestHelpers.CreatePixmap(Width, Height);

        var stops = stackalloc VelloColorStop[2];
        stops[0] = new VelloColorStop { Offset = 0f, R = 255, G = 0, B = 0, A = 255 };
        stops[1] = new VelloColorStop { Offset = 1f, R = 0, G = 0, B = 255, A = 255 };

        NativeTestHelpers.AssertSuccess(
            NativeMethods.RenderContext_SetPaintRadialGradient(
                ctx.Handle,
                Width / 2.0,
                Height / 2.0,
                Width / 2.0,
                stops,
                2,
                VelloExtend.Pad),
            "RenderContext_SetPaintRadialGradient");

        FillFullRect(ctx);
        FlushAndRender(ctx.Handle, pixmap.Handle);

        var pixels = pixmap.SnapshotPixels();
        AssertPixelApprox(NativeTestHelpers.Premul(255, 0, 0), pixels[PixelIndex(Width / 2, Height / 2)]);
        AssertPixelApprox(NativeTestHelpers.Premul(0, 0, 255), pixels[PixelIndex(0, 0)]);
    }

    [Fact]
    public void RenderContext_SetPaintSweepGradient_ExtendRepeatCyclesColors()
    {
        var padPixels = RenderSweepGradientPixels(VelloExtend.Pad);
        var repeatPixels = RenderSweepGradientPixels(VelloExtend.Repeat);

        var topIndex = PixelIndex(Width / 2, 1);
        var padTop = padPixels[topIndex];
        var repeatTop = repeatPixels[topIndex];

        Assert.True(
            padTop.R != repeatTop.R || padTop.G != repeatTop.G || padTop.B != repeatTop.B,
            "Repeat extend should yield a different top sample than pad extend.");

        bool anyDifference = false;
        for (int i = 0; i < padPixels.Length; i++)
        {
            var padPixel = padPixels[i];
            var repeatPixel = repeatPixels[i];
            if (padPixel.R != repeatPixel.R || padPixel.G != repeatPixel.G || padPixel.B != repeatPixel.B)
            {
                anyDifference = true;
                break;
            }
        }

        Assert.True(anyDifference, "Repeat extend should alter sweep gradient distribution compared to pad.");
    }

    [Fact]
    public unsafe void RenderContext_SetPaintRadialGradient_WithTooFewStops_ReturnsError()
    {
        using var ctx = NativeTestHelpers.CreateContext(Width, Height);

        VelloColorStop stop = new()
        {
            Offset = 0,
            R = 255,
            G = 0,
            B = 0,
            A = 255
        };

        NativeTestHelpers.AssertError(
            NativeMethods.RenderContext_SetPaintRadialGradient(
                ctx.Handle,
                Width / 2.0,
                Height / 2.0,
                Width / 2.0,
                &stop,
                1,
                VelloExtend.Pad),
            "RenderContext_SetPaintRadialGradient(stop count < 2)");
    }

    [Fact]
    public unsafe void RenderContext_SetPaintSweepGradient_UpdatesPaintKind()
    {
        using var ctx = NativeTestHelpers.CreateContext(Width, Height);

        var stops = stackalloc VelloColorStop[2];
        stops[0] = new VelloColorStop { Offset = 0f, R = 255, G = 255, B = 0, A = 255 };
        stops[1] = new VelloColorStop { Offset = 1f, R = 255, G = 0, B = 0, A = 255 };

        NativeTestHelpers.AssertSuccess(
            NativeMethods.RenderContext_SetPaintSweepGradient(
                ctx.Handle,
                Width / 2.0,
                Height / 2.0,
                0f,
                (float)(2 * Math.PI),
                stops,
                2,
                VelloExtend.Pad),
            "RenderContext_SetPaintSweepGradient");

        Assert.Equal(VelloPaintKind.SweepGradient, NativeMethods.RenderContext_GetPaintKind(ctx.Handle));
    }

    [Fact]
    public unsafe void RenderContext_SetPaintTransform_RoundTrips()
    {
        using var ctx = NativeTestHelpers.CreateContext(Width, Height);

        VelloAffine paintTransform = new()
        {
            M11 = 2,
            M12 = 0.5,
            M13 = 1,
            M21 = -0.25,
            M22 = 1.5,
            M23 = -2
        };

        NativeTestHelpers.AssertSuccess(
            NativeMethods.RenderContext_SetPaintTransform(ctx.Handle, &paintTransform),
            "RenderContext_SetPaintTransform");

        VelloAffine readBack = default;
        NativeTestHelpers.AssertSuccess(
            NativeMethods.RenderContext_GetPaintTransform(ctx.Handle, &readBack),
            "RenderContext_GetPaintTransform");

        AssertAffineEquals(paintTransform, readBack);

        NativeTestHelpers.AssertSuccess(
            NativeMethods.RenderContext_ResetPaintTransform(ctx.Handle),
            "RenderContext_ResetPaintTransform");

        VelloAffine identity = default;
        NativeTestHelpers.AssertSuccess(
            NativeMethods.RenderContext_GetPaintTransform(ctx.Handle, &identity),
            "RenderContext_GetPaintTransform identity");

        AssertAffineEquals(new VelloAffine { M11 = 1, M22 = 1 }, identity);
    }

    [Fact]
    public unsafe void RenderContext_SetPaintTransform_Translation_ShiftsGradient()
    {
        using var ctx = NativeTestHelpers.CreateContext(Width, Height);
        using var pixmap = NativeTestHelpers.CreatePixmap(Width, Height);

        var stops = stackalloc VelloColorStop[2];
        stops[0] = new VelloColorStop { Offset = 0f, R = 0, G = 0, B = 0, A = 255 };
        stops[1] = new VelloColorStop { Offset = 1f, R = 255, G = 255, B = 255, A = 255 };

        double span = Width / 4.0;
        double shift = span / 2.0;

        NativeTestHelpers.AssertSuccess(
            NativeMethods.RenderContext_SetPaintLinearGradient(
                ctx.Handle,
                0,
                0,
                span,
                0,
                stops,
                2,
                VelloExtend.Repeat),
            "RenderContext_SetPaintLinearGradient repeat");

        VelloAffine transform = new()
        {
            M11 = 1,
            M12 = 0,
            M13 = shift,
            M21 = 0,
            M22 = 1,
            M23 = 0
        };

        NativeTestHelpers.AssertSuccess(
            NativeMethods.RenderContext_SetPaintTransform(ctx.Handle, &transform),
            "RenderContext_SetPaintTransform translate");

        FillFullRect(ctx);
        FlushAndRender(ctx.Handle, pixmap.Handle);

        var pixels = pixmap.SnapshotPixels();
        int midRow = Height / 2;
        int segment = Width / 4;
        var sampleColumns = new[] { 0, segment, segment * 2, segment * 3 };

        foreach (int x in sampleColumns)
        {
            double position = PixelCenter(x) + shift;
            var expected = ExpectedLinearGradientColor(position, 0, span, VelloExtend.Repeat);
            AssertPixelApprox(expected, pixels[PixelIndex(x, midRow)]);
        }
    }

    [Fact]
    public void RenderContext_SetPaintTransform_ScalesGradient()
    {
        var baseline = RenderLinearGradientPixels(Width, VelloExtend.Pad);

        VelloAffine transform = new()
        {
            M11 = 0.5,
            M12 = 0,
            M13 = 0,
            M21 = 0,
            M22 = 1,
            M23 = 0
        };

        var scaled = RenderLinearGradientPixels(Width, VelloExtend.Pad, transform);

        int midRow = Height / 2;
        int sampleColumn = Width / 2;

        byte baselineValue = baseline[PixelIndex(sampleColumn, midRow)].R;
        byte scaledValue = scaled[PixelIndex(sampleColumn, midRow)].R;

        Assert.True(
            scaledValue > baselineValue,
            $"Expected scaled gradient to brighten sample at column {sampleColumn}: baseline={baselineValue}, scaled={scaledValue}");
    }

    [Fact]
    public void RenderContext_SetPaintTransform_RotatesGradient()
    {
        var baseline = RenderVerticalGradientPixels(VelloExtend.Pad);

        double angle = -Math.PI / 2.0;
        double cos = Math.Cos(angle);
        double sin = Math.Sin(angle);

        VelloAffine rotation = new()
        {
            M11 = cos,
            M12 = sin,
            M13 = 0,
            M21 = -sin,
            M22 = cos,
            M23 = 0
        };

        var rotated = RenderVerticalGradientPixels(VelloExtend.Pad, rotation);

        int midRow = Height / 2;
        int leftColumn = 1;
        int rightColumn = Math.Max(1, Height - 2); // keep within unclamped range

        byte baselineLeft = baseline[PixelIndex(leftColumn, midRow)].R;
        byte baselineRight = baseline[PixelIndex(rightColumn, midRow)].R;
        Assert.Equal(baselineLeft, baselineRight); // vertical gradient, horizontal slice constant

        byte rotatedLeft = rotated[PixelIndex(leftColumn, midRow)].R;
        byte rotatedRight = rotated[PixelIndex(rightColumn, midRow)].R;
        Assert.True(
            rotatedLeft < rotatedRight,
            $"Expected rotation to swap gradient axes: left={rotatedLeft}, right={rotatedRight}");
    }

    [Fact]
    public unsafe void RenderContext_SetPaintTransform_WithNullPointer_ReturnsError()
    {
        using var ctx = NativeTestHelpers.CreateContext(Width, Height);

        NativeTestHelpers.AssertError(
            NativeMethods.RenderContext_SetPaintTransform(ctx.Handle, null),
            "RenderContext_SetPaintTransform(null)");
    }

    [Fact]
    public unsafe void RenderContext_GetPaintTransform_WithNullOutPointer_ReturnsError()
    {
        using var ctx = NativeTestHelpers.CreateContext(Width, Height);

        NativeTestHelpers.AssertError(
            NativeMethods.RenderContext_GetPaintTransform(ctx.Handle, null),
            "RenderContext_GetPaintTransform(null)");
    }

    [Fact]
    public unsafe void RenderContext_SetTransform_WithNullPointer_ReturnsError()
    {
        using var ctx = NativeTestHelpers.CreateContext(Width, Height);

        NativeTestHelpers.AssertError(
            NativeMethods.RenderContext_SetTransform(ctx.Handle, null),
            "RenderContext_SetTransform(null)");
    }

    [Fact]
    public unsafe void RenderContext_SetPaintLinearGradient_WithNullStopsPointer_ReturnsError()
    {
        using var ctx = NativeTestHelpers.CreateContext(Width, Height);

        NativeTestHelpers.AssertError(
            NativeMethods.RenderContext_SetPaintLinearGradient(
                ctx.Handle,
                0,
                0,
                Width,
                0,
                null,
                2,
                VelloExtend.Pad),
            "RenderContext_SetPaintLinearGradient(null stops)");
    }

    [Fact]
    public unsafe void RenderContext_GetTransform_WithNullOutPointer_ReturnsError()
    {
        using var ctx = NativeTestHelpers.CreateContext(Width, Height);

        NativeTestHelpers.AssertError(
            NativeMethods.RenderContext_GetTransform(ctx.Handle, null),
            "RenderContext_GetTransform(null)");
    }

    [Fact]
    public void RenderContext_RenderToBuffer_WritesSolidColor()
    {
        using var ctx = NativeTestHelpers.CreateContext(Width, Height);

        NativeTestHelpers.AssertSuccess(
            NativeMethods.RenderContext_SetPaintSolid(ctx.Handle, 20, 30, 40, 255),
            "RenderContext_SetPaintSolid");

        unsafe
        {
            VelloRect rect = new()
            {
                X0 = 0,
                Y0 = 0,
                X1 = Width,
                Y1 = Height
            };

            NativeTestHelpers.AssertSuccess(
                NativeMethods.RenderContext_FillRect(ctx.Handle, &rect),
                "RenderContext_FillRect");
        }

        NativeTestHelpers.AssertSuccess(
            NativeMethods.RenderContext_Flush(ctx.Handle),
            "RenderContext_Flush");

        byte[] buffer = new byte[Width * Height * 4];
        unsafe
        {
            fixed (byte* ptr = buffer)
            {
                NativeTestHelpers.AssertSuccess(
                    NativeMethods.RenderContext_RenderToBuffer(
                        ctx.Handle,
                        ptr,
                        (nuint)buffer.Length,
                        Width,
                        Height,
                        VelloRenderMode.OptimizeSpeed),
                    "RenderContext_RenderToBuffer");
            }
        }

        var pixels = MemoryMarshal.Cast<byte, VelloPremulRgba8>(buffer.AsSpan());
        foreach (var pixel in pixels)
        {
            AssertPixelEquals(NativeTestHelpers.Premul(20, 30, 40), pixel);
        }
    }

    [Fact]
    public void RenderContext_RenderToBuffer_WithTooSmallBuffer_ReturnsError()
    {
        using var ctx = NativeTestHelpers.CreateContext(Width, Height);
        byte[] buffer = new byte[(Width * Height * 4) - 1];

        unsafe
        {
            fixed (byte* ptr = buffer)
            {
                NativeTestHelpers.AssertError(
                    NativeMethods.RenderContext_RenderToBuffer(
                        ctx.Handle,
                        ptr,
                        (nuint)buffer.Length,
                        Width,
                        Height,
                        VelloRenderMode.OptimizeSpeed),
                    "RenderContext_RenderToBuffer(small)");
            }
        }
    }

    [Fact]
    public void RenderContext_RenderToBuffer_WithNullPointer_ReturnsError()
    {
        using var ctx = NativeTestHelpers.CreateContext(Width, Height);

        unsafe
        {
            NativeTestHelpers.AssertError(
                NativeMethods.RenderContext_RenderToBuffer(
                    ctx.Handle,
                    null,
                    0,
                    Width,
                    Height,
                    VelloRenderMode.OptimizeSpeed),
                "RenderContext_RenderToBuffer(null)");
        }
    }

    [Fact]
    public void RenderContext_RenderToBuffer_MatchesPixmapOutput()
    {
        using var ctx = NativeTestHelpers.CreateContext(Width, Height);
        using var pixmap = NativeTestHelpers.CreatePixmap(Width, Height);

        NativeTestHelpers.AssertSuccess(
            NativeMethods.RenderContext_SetPaintSolid(ctx.Handle, 50, 100, 150, 255),
            "RenderContext_SetPaintSolid");

        unsafe
        {
            VelloRect rect = new()
            {
                X0 = 0,
                Y0 = 0,
                X1 = Width,
                Y1 = Height
            };

            NativeTestHelpers.AssertSuccess(
                NativeMethods.RenderContext_FillRect(ctx.Handle, &rect),
                "RenderContext_FillRect");
        }

        NativeTestHelpers.AssertSuccess(
            NativeMethods.RenderContext_Flush(ctx.Handle),
            "RenderContext_Flush");

        byte[] buffer = new byte[Width * Height * 4];
        unsafe
        {
            fixed (byte* ptr = buffer)
            {
                NativeTestHelpers.AssertSuccess(
                    NativeMethods.RenderContext_RenderToBuffer(
                        ctx.Handle,
                        ptr,
                        (nuint)buffer.Length,
                        Width,
                        Height,
                        VelloRenderMode.OptimizeSpeed),
                    "RenderContext_RenderToBuffer");
            }
        }

        NativeTestHelpers.AssertSuccess(
            NativeMethods.RenderContext_RenderToPixmap(ctx.Handle, pixmap.Handle),
            "RenderContext_RenderToPixmap");

        var bufferPixels = MemoryMarshal.Cast<byte, VelloPremulRgba8>(buffer.AsSpan());
        var pixmapPixels = pixmap.SnapshotPixels();

        for (int i = 0; i < pixmapPixels.Length; i++)
        {
            AssertPixelEquals(pixmapPixels[i], bufferPixels[i]);
        }
    }

    [Fact]
    public void RenderContext_SetAliasingThreshold_AcceptsRange()
    {
        using var ctx = NativeTestHelpers.CreateContext(Width, Height);

        NativeTestHelpers.AssertSuccess(
            NativeMethods.RenderContext_SetAliasingThreshold(ctx.Handle, 0),
            "RenderContext_SetAliasingThreshold(0)");

        NativeTestHelpers.AssertSuccess(
            NativeMethods.RenderContext_SetAliasingThreshold(ctx.Handle, 255),
            "RenderContext_SetAliasingThreshold(255)");

        NativeTestHelpers.AssertSuccess(
            NativeMethods.RenderContext_SetAliasingThreshold(ctx.Handle, -1),
            "RenderContext_SetAliasingThreshold(-1)");
    }

    [Fact]
    public void RenderContext_SetAliasingThreshold_WithNullContext_ReturnsError()
    {
        NativeTestHelpers.AssertError(
            NativeMethods.RenderContext_SetAliasingThreshold(nint.Zero, 10),
            "RenderContext_SetAliasingThreshold(null)");
    }

    [Fact]
    public void RenderContext_SetAliasingThreshold_ClampsValuesAbove255()
    {
        using var ctx = NativeTestHelpers.CreateContext(Width, Height);

        NativeTestHelpers.AssertSuccess(
            NativeMethods.RenderContext_SetAliasingThreshold(ctx.Handle, 500),
            "RenderContext_SetAliasingThreshold(>255)");
    }

    private static unsafe void FillFullRect(NativeRenderContext ctx)
    {
        VelloRect rect = new()
        {
            X0 = 0,
            Y0 = 0,
            X1 = Width,
            Y1 = Height
        };

        NativeTestHelpers.AssertSuccess(
            NativeMethods.RenderContext_FillRect(ctx.Handle, &rect),
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

    private static int PixelIndex(int x, int y) => (y * Width) + x;

    private static double PixelCenter(int x) => x + 0.5;

    private static unsafe VelloPremulRgba8[] RenderLinearGradientPixels(
        double span,
        VelloExtend extend,
        VelloAffine? paintTransform = null)
    {
        using var ctx = NativeTestHelpers.CreateContext(Width, Height);
        using var pixmap = NativeTestHelpers.CreatePixmap(Width, Height);

        var stops = stackalloc VelloColorStop[2];
        stops[0] = new VelloColorStop { Offset = 0f, R = 0, G = 0, B = 0, A = 255 };
        stops[1] = new VelloColorStop { Offset = 1f, R = 255, G = 255, B = 255, A = 255 };

        NativeTestHelpers.AssertSuccess(
            NativeMethods.RenderContext_SetPaintLinearGradient(
                ctx.Handle,
                0,
                0,
                span,
                0,
                stops,
                2,
                extend),
            "RenderContext_SetPaintLinearGradient helper");

        if (paintTransform.HasValue)
        {
            VelloAffine transform = paintTransform.Value;
            NativeTestHelpers.AssertSuccess(
                NativeMethods.RenderContext_SetPaintTransform(ctx.Handle, &transform),
                "RenderContext_SetPaintTransform helper");
        }

        FillFullRect(ctx);
        FlushAndRender(ctx.Handle, pixmap.Handle);

        return pixmap.SnapshotPixels();
    }

    private static unsafe VelloPremulRgba8[] RenderVerticalGradientPixels(
        VelloExtend extend,
        VelloAffine? paintTransform = null)
    {
        using var ctx = NativeTestHelpers.CreateContext(Width, Height);
        using var pixmap = NativeTestHelpers.CreatePixmap(Width, Height);

        var stops = stackalloc VelloColorStop[2];
        stops[0] = new VelloColorStop { Offset = 0f, R = 0, G = 0, B = 0, A = 255 };
        stops[1] = new VelloColorStop { Offset = 1f, R = 255, G = 255, B = 255, A = 255 };

        NativeTestHelpers.AssertSuccess(
            NativeMethods.RenderContext_SetPaintLinearGradient(
                ctx.Handle,
                0,
                0,
                0,
                Height,
                stops,
                2,
                extend),
            "RenderContext_SetPaintLinearGradient vertical helper");

        if (paintTransform.HasValue)
        {
            VelloAffine transform = paintTransform.Value;
            NativeTestHelpers.AssertSuccess(
                NativeMethods.RenderContext_SetPaintTransform(ctx.Handle, &transform),
                "RenderContext_SetPaintTransform vertical helper");
        }

        FillFullRect(ctx);
        FlushAndRender(ctx.Handle, pixmap.Handle);

        return pixmap.SnapshotPixels();
    }

    private static unsafe VelloPremulRgba8[] RenderSweepGradientPixels(VelloExtend extend)
    {
        using var ctx = NativeTestHelpers.CreateContext(Width, Height);
        using var pixmap = NativeTestHelpers.CreatePixmap(Width, Height);

        var stops = stackalloc VelloColorStop[2];
        stops[0] = new VelloColorStop { Offset = 0f, R = 255, G = 0, B = 0, A = 255 };
        stops[1] = new VelloColorStop { Offset = 1f, R = 0, G = 0, B = 255, A = 255 };

        NativeTestHelpers.AssertSuccess(
            NativeMethods.RenderContext_SetPaintSweepGradient(
                ctx.Handle,
                Width / 2.0,
                Height / 2.0,
                0,
                (float)Math.PI,
                stops,
                2,
                extend),
            "RenderContext_SetPaintSweepGradient helper");

        FillFullRect(ctx);
        FlushAndRender(ctx.Handle, pixmap.Handle);

        return pixmap.SnapshotPixels();
    }

    private static VelloPremulRgba8 ExpectedLinearGradientColor(double position, double start, double end, VelloExtend extend)
    {
        byte value = EvaluateLinearGradient(position, start, end, extend);
        return NativeTestHelpers.Premul(value, value, value);
    }

    private static VelloPremulRgba8 ExpectedLinearGradientColor2D(
        double px,
        double py,
        double x0,
        double y0,
        double x1,
        double y1,
        VelloExtend extend)
    {
        double dx = x1 - x0;
        double dy = y1 - y0;
        double lengthSquared = (dx * dx) + (dy * dy);
        if (lengthSquared <= double.Epsilon)
        {
            return ExpectedLinearGradientColor(0, 0, 1, extend);
        }

        double projection = ((px - x0) * dx + (py - y0) * dy) / Math.Sqrt(lengthSquared);
        double span = Math.Sqrt(lengthSquared);
        return ExpectedLinearGradientColor(projection, 0, span, extend);
    }

    private static byte EvaluateLinearGradient(double position, double start, double end, VelloExtend extend)
    {
        double span = end - start;
        if (Math.Abs(span) < double.Epsilon)
        {
            return 0;
        }

        double t = (position - start) / span;

        switch (extend)
        {
            case VelloExtend.Pad:
                t = Math.Clamp(t, 0d, 1d);
                break;
            case VelloExtend.Repeat:
                t = t - Math.Floor(t);
                break;
            case VelloExtend.Reflect:
                double mod = t % 2d;
                if (mod < 0)
                {
                    mod += 2d;
                }
                t = mod <= 1d ? mod : 2d - mod;
                break;
        }

        int value = (int)Math.Round(t * 255d);
        return (byte)Math.Clamp(value, 0, 255);
    }

    private static void AssertPixelEquals(VelloPremulRgba8 expected, VelloPremulRgba8 actual)
    {
        Assert.Equal(expected.R, actual.R);
        Assert.Equal(expected.G, actual.G);
        Assert.Equal(expected.B, actual.B);
        Assert.Equal(expected.A, actual.A);
    }

    private static void AssertPixelApprox(VelloPremulRgba8 expected, VelloPremulRgba8 actual, byte tolerance = 16)
    {
        Assert.InRange(actual.R, Clamp(expected.R - tolerance), Clamp(expected.R + tolerance));
        Assert.InRange(actual.G, Clamp(expected.G - tolerance), Clamp(expected.G + tolerance));
        Assert.InRange(actual.B, Clamp(expected.B - tolerance), Clamp(expected.B + tolerance));
        Assert.Equal(expected.A, actual.A);
    }

    private static void AssertGradientMonotonic(VelloPremulRgba8[] pixels)
    {
        byte prev = 0;
        for (int x = 0; x < Width; x++)
        {
            byte value = pixels[PixelIndex(x, Height / 2)].R;
            Assert.True(value >= prev, $"Gradient not monotonic at column {x}: {value} < {prev}");
            prev = value;
        }
    }

    private static void AssertTransparent(VelloPremulRgba8 pixel)
    {
        Assert.Equal(0, pixel.A);
        Assert.Equal(0, pixel.R);
        Assert.Equal(0, pixel.G);
        Assert.Equal(0, pixel.B);
    }

    private static void AssertAffineEquals(VelloAffine expected, VelloAffine actual, double tolerance = 1e-6)
    {
        Assert.InRange(actual.M11, expected.M11 - tolerance, expected.M11 + tolerance);
        Assert.InRange(actual.M12, expected.M12 - tolerance, expected.M12 + tolerance);
        Assert.InRange(actual.M13, expected.M13 - tolerance, expected.M13 + tolerance);
        Assert.InRange(actual.M21, expected.M21 - tolerance, expected.M21 + tolerance);
        Assert.InRange(actual.M22, expected.M22 - tolerance, expected.M22 + tolerance);
        Assert.InRange(actual.M23, expected.M23 - tolerance, expected.M23 + tolerance);
    }

    private static byte Clamp(int value) => (byte)Math.Clamp(value, 0, 255);
}
