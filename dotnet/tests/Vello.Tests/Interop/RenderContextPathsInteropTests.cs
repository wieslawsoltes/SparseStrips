// Copyright 2025
// SPDX-License-Identifier: Apache-2.0 OR MIT

using System;
using System.Linq;
using System.Runtime.InteropServices;
using Vello.Native;
using Vello.Native.FastPath;
using Xunit;

namespace Vello.Tests.Interop;

[Collection(NativeInteropCollection.CollectionName)]
public sealed class RenderContextPathsInteropTests
{
    private const ushort Width = 32;
    private const ushort Height = 32;

    [Fact]
    public void RenderContext_FillPath_RendersTriangle()
    {
        using var ctx = NativeTestHelpers.CreateContext(Width, Height);
        using var pixmap = NativeTestHelpers.CreatePixmap(Width, Height);
        using var path = new NativeBezPath();

        NativeTestHelpers.AssertSuccess(
            NativeMethods.RenderContext_SetPaintSolid(ctx.Handle, 240, 80, 60, 255),
            "RenderContext_SetPaintSolid");

        path.MoveTo(8, 24);
        path.LineTo(24, 24);
        path.LineTo(16, 8);
        path.Close();

        NativeTestHelpers.AssertSuccess(
            NativeMethods.RenderContext_FillPath(ctx.Handle, path.Handle),
            "RenderContext_FillPath");

        FlushAndRender(ctx.Handle, pixmap.Handle);
        var pixels = pixmap.SnapshotPixels();

        AssertPixelEquals(NativeTestHelpers.Premul(240, 80, 60), pixels[PixelIndex(16, 16)]);
        AssertTransparent(pixels[PixelIndex(4, 4)]);
    }

    [Fact]
    public void RenderContext_FillPath_WithNullPath_ReturnsError()
    {
        using var ctx = NativeTestHelpers.CreateContext(Width, Height);

        NativeTestHelpers.AssertError(
            NativeMethods.RenderContext_FillPath(ctx.Handle, nint.Zero),
            "RenderContext_FillPath(null)");
    }

    [Fact]
    public unsafe void RenderContext_StrokePath_DrawsOutline()
    {
        using var ctx = NativeTestHelpers.CreateContext(Width, Height);
        using var pixmap = NativeTestHelpers.CreatePixmap(Width, Height);
        using var path = CreateRectanglePath(8, 8, 24, 24);

        VelloStroke stroke = new()
        {
            Width = 2f,
            MiterLimit = 4f,
            Join = VelloJoin.Miter,
            StartCap = VelloCap.Butt,
            EndCap = VelloCap.Butt
        };

        NativeTestHelpers.AssertSuccess(
            NativeMethods.RenderContext_SetStroke(ctx.Handle, &stroke),
            "RenderContext_SetStroke");

        NativeTestHelpers.AssertSuccess(
            NativeMethods.RenderContext_SetPaintSolid(ctx.Handle, 0, 0, 255, 255),
            "RenderContext_SetPaintSolid");

        NativeTestHelpers.AssertSuccess(
            NativeMethods.RenderContext_StrokePath(ctx.Handle, path.Handle),
            "RenderContext_StrokePath");

        FlushAndRender(ctx.Handle, pixmap.Handle);
        var pixels = pixmap.SnapshotPixels();

        AssertPixelEquals(NativeTestHelpers.Premul(0, 0, 255), pixels[PixelIndex(8, 8)]);
        AssertTransparent(pixels[PixelIndex(16, 16)]);
    }

    [Fact]
    public void RenderContext_StrokePath_WithNullPath_ReturnsError()
    {
        using var ctx = NativeTestHelpers.CreateContext(Width, Height);

        NativeTestHelpers.AssertError(
            NativeMethods.RenderContext_StrokePath(ctx.Handle, nint.Zero),
            "RenderContext_StrokePath(null)");
    }

    [Fact]
    public void BezPath_Methods_WithNullPointer_ReturnError()
    {
        NativeTestHelpers.AssertError(
            NativeMethods.BezPath_MoveTo(nint.Zero, 0, 0),
            "BezPath_MoveTo(null)");

        NativeTestHelpers.AssertError(
            NativeMethods.BezPath_LineTo(nint.Zero, 0, 0),
            "BezPath_LineTo(null)");

        NativeTestHelpers.AssertError(
            NativeMethods.BezPath_Close(nint.Zero),
            "BezPath_Close(null)");
    }

    [Fact]
    public void RenderContext_Record_ReplayMatchesDirect()
    {
        using var directCtx = NativeTestHelpers.CreateContext(Width, Height);
        using var directPixmap = NativeTestHelpers.CreatePixmap(Width, Height);
        RenderSolidRectDirect(directCtx.Handle, directPixmap.Handle, NativeTestHelpers.Premul(20, 120, 220));
        var directPixels = directPixmap.SnapshotPixels();

        using var recordCtx = NativeTestHelpers.CreateContext(Width, Height);
        using var recordPixmap = NativeTestHelpers.CreatePixmap(Width, Height);
        using var recording = new NativeRecording();

        Action<nint> action = recorderHandle =>
        {
            NativeTestHelpers.AssertSuccess(
                NativeMethods.Recorder_SetPaintSolid(recorderHandle, 20, 120, 220, 255),
                "Recorder_SetPaintSolid");

            unsafe
            {
                VelloRect rect = new()
                {
                    X0 = 4,
                    Y0 = 4,
                    X1 = Width - 4,
                    Y1 = Height - 4
                };

                NativeTestHelpers.AssertSuccess(
                    NativeMethods.Recorder_FillRect(recorderHandle, &rect),
                    "Recorder_FillRect");
            }
        };

        var gcHandle = GCHandle.Alloc(action);
        try
        {
            NativeTestHelpers.AssertSuccess(
                NativeMethods.RenderContext_Record(
                    recordCtx.Handle,
                    recording.Handle,
                    RecorderCallbackPtr,
                    GCHandle.ToIntPtr(gcHandle)),
                "RenderContext_Record");
        }
        finally
        {
            gcHandle.Free();
        }

        Assert.True(recording.Length > 0, "Recording should capture at least one command.");

        NativeTestHelpers.AssertSuccess(
            NativeMethods.RenderContext_PrepareRecording(recordCtx.Handle, recording.Handle),
            "RenderContext_PrepareRecording");

        NativeTestHelpers.AssertSuccess(
            NativeMethods.RenderContext_ExecuteRecording(recordCtx.Handle, recording.Handle),
            "RenderContext_ExecuteRecording");

        FlushAndRender(recordCtx.Handle, recordPixmap.Handle);
        var recordedPixels = recordPixmap.SnapshotPixels();

        Assert.True(directPixels.SequenceEqual(recordedPixels), "Recorded playback should match direct rendering.");
    }

    [Fact]
    public void Recorder_StrokePath_MatchesDirect()
    {
        using var directCtx = NativeTestHelpers.CreateContext(Width, Height);
        using var directPixmap = NativeTestHelpers.CreatePixmap(Width, Height);
        using var strokePath = CreateRectanglePath(8, 8, 24, 24);

        var strokeColor = NativeTestHelpers.Premul(200, 40, 40, 255);

        unsafe
        {
            VelloStroke stroke = new()
            {
                Width = 3f,
                MiterLimit = 4f,
                Join = VelloJoin.Miter,
                StartCap = VelloCap.Butt,
                EndCap = VelloCap.Butt
            };

            NativeTestHelpers.AssertSuccess(
                NativeMethods.RenderContext_SetStroke(directCtx.Handle, &stroke),
                "RenderContext_SetStroke");
        }

        NativeTestHelpers.AssertSuccess(
            NativeMethods.RenderContext_SetPaintSolid(directCtx.Handle, strokeColor.R, strokeColor.G, strokeColor.B, strokeColor.A),
            "RenderContext_SetPaintSolid");

        NativeTestHelpers.AssertSuccess(
            NativeMethods.RenderContext_StrokePath(directCtx.Handle, strokePath.Handle),
            "RenderContext_StrokePath");

        FlushAndRender(directCtx.Handle, directPixmap.Handle);
        var directPixels = directPixmap.SnapshotPixels();

        using var recordCtx = NativeTestHelpers.CreateContext(Width, Height);
        using var recordPixmap = NativeTestHelpers.CreatePixmap(Width, Height);
        using var recording = new NativeRecording();

        var strokePathHandle = strokePath.Handle;

        Action<nint> action = recorderHandle =>
        {
            unsafe
            {
                VelloStroke stroke = new()
                {
                    Width = 3f,
                    MiterLimit = 4f,
                    Join = VelloJoin.Miter,
                    StartCap = VelloCap.Butt,
                    EndCap = VelloCap.Butt
                };

                NativeTestHelpers.AssertSuccess(
                    NativeMethods.Recorder_SetStroke(recorderHandle, &stroke),
                    "Recorder_SetStroke");
            }

            NativeTestHelpers.AssertSuccess(
                NativeMethods.Recorder_SetPaintSolid(recorderHandle, strokeColor.R, strokeColor.G, strokeColor.B, strokeColor.A),
                "Recorder_SetPaintSolid");

            NativeTestHelpers.AssertSuccess(
                NativeMethods.Recorder_StrokePath(recorderHandle, strokePathHandle),
                "Recorder_StrokePath");
        };

        var gcHandle = GCHandle.Alloc(action);
        try
        {
            NativeTestHelpers.AssertSuccess(
                NativeMethods.RenderContext_Record(
                    recordCtx.Handle,
                    recording.Handle,
                    RecorderCallbackPtr,
                    GCHandle.ToIntPtr(gcHandle)),
                "RenderContext_Record");
        }
        finally
        {
            gcHandle.Free();
        }

        NativeTestHelpers.AssertSuccess(
            NativeMethods.RenderContext_PrepareRecording(recordCtx.Handle, recording.Handle),
            "RenderContext_PrepareRecording");

        NativeTestHelpers.AssertSuccess(
            NativeMethods.RenderContext_ExecuteRecording(recordCtx.Handle, recording.Handle),
            "RenderContext_ExecuteRecording");

        FlushAndRender(recordCtx.Handle, recordPixmap.Handle);
        var recordedPixels = recordPixmap.SnapshotPixels();

        Assert.True(directPixels.SequenceEqual(recordedPixels), "Recorded stroke should match direct stroke rendering.");
    }

    [Fact]
    public void Recorder_SetTransform_MatchesDirectTransform()
    {
        using var directCtx = NativeTestHelpers.CreateContext(Width, Height);
        using var directPixmap = NativeTestHelpers.CreatePixmap(Width, Height);
        using var recordCtx = NativeTestHelpers.CreateContext(Width, Height);
        using var recordPixmap = NativeTestHelpers.CreatePixmap(Width, Height);
        using var recording = new NativeRecording();

        VelloRect rect = new()
        {
            X0 = 0,
            Y0 = 0,
            X1 = 10,
            Y1 = 10
        };

        VelloAffine directTransform = new()
        {
            M11 = 1,
            M12 = 0,
            M13 = 6,
            M21 = 0,
            M22 = 1,
            M23 = 4
        };

        unsafe
        {
            NativeTestHelpers.AssertSuccess(
                NativeMethods.RenderContext_SetTransform(directCtx.Handle, &directTransform),
                "RenderContext_SetTransform");
        }

        NativeTestHelpers.AssertSuccess(
            NativeMethods.RenderContext_SetPaintSolid(directCtx.Handle, 90, 180, 90, 255),
            "RenderContext_SetPaintSolid");

        unsafe
        {
            var rectDirect = rect;
            NativeTestHelpers.AssertSuccess(
                NativeMethods.RenderContext_FillRect(directCtx.Handle, &rectDirect),
                "RenderContext_FillRect");
        }

        VelloAffine recorderTransform = directTransform;

        Action<nint> recorderAction = recorderHandle =>
        {
            NativeTestHelpers.AssertSuccess(
                NativeMethods.Recorder_SetPaintSolid(recorderHandle, 90, 180, 90, 255),
                "Recorder_SetPaintSolid");

            unsafe
            {
                VelloAffine localTransform = recorderTransform;
                NativeTestHelpers.AssertSuccess(
                    NativeMethods.Recorder_SetTransform(recorderHandle, &localTransform),
                    "Recorder_SetTransform");

                VelloRect localRect = rect;
                NativeTestHelpers.AssertSuccess(
                    NativeMethods.Recorder_FillRect(recorderHandle, &localRect),
                    "Recorder_FillRect");
            }
        };

        var recorderHandleGc = GCHandle.Alloc(recorderAction);
        try
        {
            NativeTestHelpers.AssertSuccess(
                NativeMethods.RenderContext_Record(
                    recordCtx.Handle,
                    recording.Handle,
                    RecorderCallbackPtr,
                    GCHandle.ToIntPtr(recorderHandleGc)),
                "RenderContext_Record");
        }
        finally
        {
            recorderHandleGc.Free();
        }

        NativeTestHelpers.AssertSuccess(
            NativeMethods.RenderContext_PrepareRecording(recordCtx.Handle, recording.Handle),
            "RenderContext_PrepareRecording");

        NativeTestHelpers.AssertSuccess(
            NativeMethods.RenderContext_ExecuteRecording(recordCtx.Handle, recording.Handle),
            "RenderContext_ExecuteRecording");

        FlushAndRender(directCtx.Handle, directPixmap.Handle);
        FlushAndRender(recordCtx.Handle, recordPixmap.Handle);

        Assert.True(
            directPixmap.SnapshotPixels().SequenceEqual(recordPixmap.SnapshotPixels()),
            "Recorder transform should match direct transform output.");
    }

    [Fact]
    public void RenderContext_PushOpacityLayer_AppliesOpacity()
    {
        using var ctx = NativeTestHelpers.CreateContext(Width, Height);
        using var pixmap = NativeTestHelpers.CreatePixmap(Width, Height);

        NativeTestHelpers.AssertSuccess(
            NativeMethods.RenderContext_PushOpacityLayer(ctx.Handle, 0.5f),
            "RenderContext_PushOpacityLayer");

        NativeTestHelpers.AssertSuccess(
            NativeMethods.RenderContext_SetPaintSolid(ctx.Handle, 255, 0, 0, 255),
            "RenderContext_SetPaintSolid");

        FillFullRect(ctx.Handle);

        NativeTestHelpers.AssertSuccess(
            NativeMethods.RenderContext_PopLayer(ctx.Handle),
            "RenderContext_PopLayer");

        FlushAndRender(ctx.Handle, pixmap.Handle);
        var pixel = pixmap.SnapshotPixels()[PixelIndex(Width / 2, Height / 2)];

        Assert.InRange(pixel.R, 120, 136);
        Assert.InRange(pixel.A, 120, 136);
    }

    [Fact]
    public void RenderContext_PushClipLayer_RestrictsDrawing()
    {
        using var ctx = NativeTestHelpers.CreateContext(Width, Height);
        using var pixmap = NativeTestHelpers.CreatePixmap(Width, Height);
        using var clipPath = new NativeBezPath();

        clipPath.MoveTo(8, 8);
        clipPath.LineTo(24, 8);
        clipPath.LineTo(24, 24);
        clipPath.LineTo(8, 24);
        clipPath.Close();

        NativeTestHelpers.AssertSuccess(
            NativeMethods.RenderContext_PushClipLayer(ctx.Handle, clipPath.Handle),
            "RenderContext_PushClipLayer");

        NativeTestHelpers.AssertSuccess(
            NativeMethods.RenderContext_SetPaintSolid(ctx.Handle, 0, 255, 0, 255),
            "RenderContext_SetPaintSolid");

        FillFullRect(ctx.Handle);

        NativeTestHelpers.AssertSuccess(
            NativeMethods.RenderContext_PopLayer(ctx.Handle),
            "RenderContext_PopLayer");

        FlushAndRender(ctx.Handle, pixmap.Handle);
        var pixels = pixmap.SnapshotPixels();

        AssertPixelEquals(NativeTestHelpers.Premul(0, 255, 0), pixels[PixelIndex(16, 16)]);
        AssertTransparent(pixels[PixelIndex(4, 4)]);
    }

    [Fact]
    public unsafe void RenderContext_PushBlendLayer_PlusModeRetainsBackground()
    {
        using var baselineCtx = NativeTestHelpers.CreateContext(Width, Height);
        using var baselinePixmap = NativeTestHelpers.CreatePixmap(Width, Height);

        NativeTestHelpers.AssertSuccess(
            NativeMethods.RenderContext_SetPaintSolid(baselineCtx.Handle, 0, 0, 255, 255),
            "RenderContext_SetPaintSolid");
        FillFullRect(baselineCtx.Handle);

        NativeTestHelpers.AssertSuccess(
            NativeMethods.RenderContext_SetPaintSolid(baselineCtx.Handle, 255, 0, 0, 255),
            "RenderContext_SetPaintSolid");
        FillFullRect(baselineCtx.Handle);

        FlushAndRender(baselineCtx.Handle, baselinePixmap.Handle);
        var baselinePixel = baselinePixmap.SnapshotPixels()[PixelIndex(Width / 2, Height / 2)];

        using var ctx = NativeTestHelpers.CreateContext(Width, Height);
        using var pixmap = NativeTestHelpers.CreatePixmap(Width, Height);

        NativeTestHelpers.AssertSuccess(
            NativeMethods.RenderContext_SetPaintSolid(ctx.Handle, 0, 0, 255, 255),
            "RenderContext_SetPaintSolid");
        FillFullRect(ctx.Handle);

        VelloBlendMode blend = new()
        {
            Mix = VelloMix.Normal,
            Compose = VelloCompose.Plus
        };

        NativeTestHelpers.AssertSuccess(
            NativeMethods.RenderContext_PushBlendLayer(ctx.Handle, &blend),
            "RenderContext_PushBlendLayer");

        NativeTestHelpers.AssertSuccess(
            NativeMethods.RenderContext_SetPaintSolid(ctx.Handle, 255, 0, 0, 255),
            "RenderContext_SetPaintSolid");
        FillFullRect(ctx.Handle);

        NativeTestHelpers.AssertSuccess(
            NativeMethods.RenderContext_PopLayer(ctx.Handle),
            "RenderContext_PopLayer");

        FlushAndRender(ctx.Handle, pixmap.Handle);
        var blendedPixel = pixmap.SnapshotPixels()[PixelIndex(Width / 2, Height / 2)];

        Assert.Equal(255, baselinePixel.R);
        Assert.Equal(0, baselinePixel.B);

        Assert.Equal(255, blendedPixel.R);
        Assert.True(blendedPixel.B > 0, "Blend layer should preserve blue contribution from background.");
    }

    [Fact]
    public void Mask_NewLuminance_UsesBrightness()
    {
        using var ctx = NativeTestHelpers.CreateContext(Width, Height);
        using var pixmap = NativeTestHelpers.CreatePixmap(Width, Height);
        using var maskSource = NativeTestHelpers.CreatePixmap(Width, Height);

        maskSource.MutatePixels(span =>
        {
            for (int y = 0; y < Height; y++)
            {
                for (int x = 0; x < Width; x++)
                {
                    int index = y * Width + x;
                    span[index] = x < Width / 2
                        ? NativeTestHelpers.Premul(255, 255, 255)
                        : NativeTestHelpers.Premul(0, 0, 0);
                }
            }
        });

        using var luminanceMask = new NativeMask(maskSource, luminance: true);

        Assert.Equal(Width, luminanceMask.Width);
        Assert.Equal(Height, luminanceMask.Height);

        NativeTestHelpers.AssertSuccess(
            NativeMethods.RenderContext_PushMaskLayer(ctx.Handle, luminanceMask.Handle),
            "RenderContext_PushMaskLayer(Luminance)");

        NativeTestHelpers.AssertSuccess(
            NativeMethods.RenderContext_SetPaintSolid(ctx.Handle, 0, 0, 255, 255),
            "RenderContext_SetPaintSolid");

        FillFullRect(ctx.Handle);

        NativeTestHelpers.AssertSuccess(
            NativeMethods.RenderContext_PopLayer(ctx.Handle),
            "RenderContext_PopLayer");

        FlushAndRender(ctx.Handle, pixmap.Handle);
        var pixels = pixmap.SnapshotPixels();

        AssertPixelEquals(NativeTestHelpers.Premul(0, 0, 255), pixels[PixelIndex(Width / 4, Height / 2)]);
        AssertTransparent(pixels[PixelIndex(Width - 4, Height / 2)]);
    }

    [Fact]
    public void RenderContext_PushMaskLayer_UsesMaskAlpha()
    {
        using var ctx = NativeTestHelpers.CreateContext(Width, Height);
        using var pixmap = NativeTestHelpers.CreatePixmap(Width, Height);
        using var maskSource = NativeTestHelpers.CreatePixmap(Width, Height);

        maskSource.MutatePixels(span =>
        {
            for (int y = 0; y < Height; y++)
            {
                for (int x = 0; x < Width; x++)
                {
                    int index = y * Width + x;
                    byte alpha = x < Width / 2 ? (byte)255 : (byte)0;
                    span[index] = NativeTestHelpers.Premul(0, 0, 0, alpha);
                }
            }
        });

        using var mask = new NativeMask(maskSource);

        NativeTestHelpers.AssertSuccess(
            NativeMethods.RenderContext_PushMaskLayer(ctx.Handle, mask.Handle),
            "RenderContext_PushMaskLayer");

        NativeTestHelpers.AssertSuccess(
            NativeMethods.RenderContext_SetPaintSolid(ctx.Handle, 255, 0, 0, 255),
            "RenderContext_SetPaintSolid");

        FillFullRect(ctx.Handle);

        NativeTestHelpers.AssertSuccess(
            NativeMethods.RenderContext_PopLayer(ctx.Handle),
            "RenderContext_PopLayer");

        FlushAndRender(ctx.Handle, pixmap.Handle);
        var pixels = pixmap.SnapshotPixels();

        AssertPixelEquals(NativeTestHelpers.Premul(255, 0, 0), pixels[PixelIndex(Width / 4, Height / 2)]);
        AssertTransparent(pixels[PixelIndex(Width - 4, Height / 2)]);
    }

    private static NativeBezPath CreateRectanglePath(double x0, double y0, double x1, double y1)
    {
        var path = new NativeBezPath();
        path.MoveTo(x0, y0);
        path.LineTo(x1, y0);
        path.LineTo(x1, y1);
        path.LineTo(x0, y1);
        path.Close();
        return path;
    }

    [Fact]
    public void RenderContext_FillPath_EvenOddCreatesHole()
    {
        using var path = CreateRectanglePath(4, 4, 28, 28);
        path.MoveTo(10, 10);
        path.LineTo(22, 10);
        path.LineTo(22, 22);
        path.LineTo(10, 22);
        path.Close();

        {
            using var ctx = NativeTestHelpers.CreateContext(Width, Height);
            using var pixmap = NativeTestHelpers.CreatePixmap(Width, Height);

            NativeTestHelpers.AssertSuccess(
                NativeMethods.RenderContext_SetFillRule(ctx.Handle, VelloFillRule.NonZero),
                "RenderContext_SetFillRule(NonZero)");

            NativeTestHelpers.AssertSuccess(
                NativeMethods.RenderContext_SetPaintSolid(ctx.Handle, 0, 0, 255, 255),
                "RenderContext_SetPaintSolid");

            NativeTestHelpers.AssertSuccess(
                NativeMethods.RenderContext_FillPath(ctx.Handle, path.Handle),
                "RenderContext_FillPath");

            FlushAndRender(ctx.Handle, pixmap.Handle);
            var filled = pixmap.SnapshotPixels();
            AssertPixelEquals(NativeTestHelpers.Premul(0, 0, 255), filled[PixelIndex(16, 16)]);
        }

        {
            using var ctx = NativeTestHelpers.CreateContext(Width, Height);
            using var pixmap = NativeTestHelpers.CreatePixmap(Width, Height);

            NativeTestHelpers.AssertSuccess(
                NativeMethods.RenderContext_SetFillRule(ctx.Handle, VelloFillRule.EvenOdd),
                "RenderContext_SetFillRule(EvenOdd)");

            NativeTestHelpers.AssertSuccess(
                NativeMethods.RenderContext_SetPaintSolid(ctx.Handle, 0, 0, 255, 255),
                "RenderContext_SetPaintSolid");

            NativeTestHelpers.AssertSuccess(
                NativeMethods.RenderContext_FillPath(ctx.Handle, path.Handle),
                "RenderContext_FillPath");

            FlushAndRender(ctx.Handle, pixmap.Handle);
            var hollow = pixmap.SnapshotPixels();
            AssertTransparent(hollow[PixelIndex(16, 16)]);
        }
    }

    [Fact]
    public void Recorder_SetFillRule_MatchesDirect()
    {
        using var path = CreateRectanglePath(4, 4, 28, 28);
        path.MoveTo(10, 10);
        path.LineTo(22, 10);
        path.LineTo(22, 22);
        path.LineTo(10, 22);
        path.Close();

        using var directCtx = NativeTestHelpers.CreateContext(Width, Height);
        using var directPixmap = NativeTestHelpers.CreatePixmap(Width, Height);

        NativeTestHelpers.AssertSuccess(
            NativeMethods.RenderContext_SetFillRule(directCtx.Handle, VelloFillRule.EvenOdd),
            "RenderContext_SetFillRule(EvenOdd)");

        NativeTestHelpers.AssertSuccess(
            NativeMethods.RenderContext_SetPaintSolid(directCtx.Handle, 0, 0, 255, 255),
            "RenderContext_SetPaintSolid");

        NativeTestHelpers.AssertSuccess(
            NativeMethods.RenderContext_FillPath(directCtx.Handle, path.Handle),
            "RenderContext_FillPath");

        FlushAndRender(directCtx.Handle, directPixmap.Handle);
        var directPixels = directPixmap.SnapshotPixels();

        using var recordCtx = NativeTestHelpers.CreateContext(Width, Height);
        using var recordPixmap = NativeTestHelpers.CreatePixmap(Width, Height);
        using var recording = new NativeRecording();

        var pathHandle = path.Handle;

        Action<nint> action = recorderHandle =>
        {
            NativeTestHelpers.AssertSuccess(
                NativeMethods.Recorder_SetPaintSolid(recorderHandle, 0, 0, 255, 255),
                "Recorder_SetPaintSolid");

            NativeTestHelpers.AssertSuccess(
                NativeMethods.Recorder_SetFillRule(recorderHandle, VelloFillRule.EvenOdd),
                "Recorder_SetFillRule");

            NativeTestHelpers.AssertSuccess(
                NativeMethods.Recorder_FillPath(recorderHandle, pathHandle),
                "Recorder_FillPath");
        };

        var handle = GCHandle.Alloc(action);
        try
        {
            NativeTestHelpers.AssertSuccess(
                NativeMethods.RenderContext_Record(
                    recordCtx.Handle,
                    recording.Handle,
                    RecorderCallbackPtr,
                    GCHandle.ToIntPtr(handle)),
                "RenderContext_Record");
        }
        finally
        {
            handle.Free();
        }

        NativeTestHelpers.AssertSuccess(
            NativeMethods.RenderContext_PrepareRecording(recordCtx.Handle, recording.Handle),
            "RenderContext_PrepareRecording");

        NativeTestHelpers.AssertSuccess(
            NativeMethods.RenderContext_ExecuteRecording(recordCtx.Handle, recording.Handle),
            "RenderContext_ExecuteRecording");

        FlushAndRender(recordCtx.Handle, recordPixmap.Handle);
        Assert.True(
            directPixels.SequenceEqual(recordPixmap.SnapshotPixels()),
            "Recorder FillRule must match direct context output.");
    }

    private static unsafe void RenderSolidRectDirect(nint ctx, nint pixmap, VelloPremulRgba8 color)
    {
        NativeTestHelpers.AssertSuccess(
            NativeMethods.RenderContext_SetPaintSolid(ctx, color.R, color.G, color.B, color.A),
            "RenderContext_SetPaintSolid");

        VelloRect rect = new()
        {
            X0 = 4,
            Y0 = 4,
            X1 = Width - 4,
            Y1 = Height - 4
        };

        NativeTestHelpers.AssertSuccess(
            NativeMethods.RenderContext_FillRect(ctx, &rect),
            "RenderContext_FillRect");

        FlushAndRender(ctx, pixmap);
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

    private static void AssertTransparent(VelloPremulRgba8 pixel)
    {
        Assert.InRange(pixel.A, 0, 5);
        Assert.Equal(0, pixel.R);
        Assert.Equal(0, pixel.G);
        Assert.Equal(0, pixel.B);
    }

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate void RecorderCallback(nint userData, nint recorder);

    private static readonly RecorderCallback RecorderCallbackInstance = RecorderCallbackImpl;
    private static readonly nint RecorderCallbackPtr = Marshal.GetFunctionPointerForDelegate(RecorderCallbackInstance);

    private static void RecorderCallbackImpl(nint userData, nint recorder)
    {
        var handle = GCHandle.FromIntPtr(userData);
        var action = (Action<nint>)handle.Target!;
        action(recorder);
    }
}
