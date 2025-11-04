using System;
using System.Linq;
using Vello;
using Vello.Geometry;
using Vello.Native;
using Xunit;

namespace Vello.Tests.Interop;

public class RenderContextInteropTests
{
    private const ushort Width = 32;
    private const ushort Height = 32;

    [Fact]
    public void RenderContextDirectFillProducesSolidColor()
    {
        var pixels = RenderScene(ctx =>
        {
            ctx.SetPaint(new Color(255, 0, 0, 255));
            ctx.FillRect(Rect.FromXYWH(0, 0, Width, Height));
        });

        var expected = new PremulRgba8(255, 0, 0, 255);
        Assert.All(pixels, pixel => Assert.Equal(expected, pixel));
    }

    [Fact]
    public void RecordingPlaybackMatchesDirectRendering()
    {
        var directPixels = RenderScene(ctx =>
        {
            ctx.SetPaint(new Color(0, 255, 0, 255));
            ctx.FillRect(Rect.FromXYWH(0, 0, Width, Height));
        });

        var recordingPixels = RenderScene(ctx =>
        {
            using var recording = new Recording();
            ctx.Record(recording, recorder =>
            {
                recorder.SetPaint(new Color(0, 255, 0, 255));
                recorder.FillRect(Rect.FromXYWH(0, 0, Width, Height));
            });
            ctx.PrepareRecording(recording);
            ctx.ExecuteRecording(recording);
        });

        Assert.Equal(directPixels, recordingPixels);
    }

    [Fact]
    public void LinearGradientProducesExpectedEdgeColors()
    {
        var pixels = RenderScene(ctx =>
        {
            var stops = new[]
            {
                new ColorStop(0.0f, new Color(0, 0, 0, 255)),
                new ColorStop(1.0f, new Color(255, 255, 255, 255))
            };

            ctx.SetPaintLinearGradient(0, 0, Width, 0, stops);
            ctx.FillRect(Rect.FromXYWH(0, 0, Width, Height));
        });

        Assert.Equal(ExpectedGradientValue(0), pixels[0]);
        Assert.Equal(ExpectedGradientValue(Width - 1), pixels[^1]);
        Assert.Equal(ExpectedGradientValue(Width / 2), pixels[Width / 2]);
    }

    [Fact]
    public void LinearGradientRecordingMatchesExpected()
    {
        var stops = new[]
        {
            new ColorStop(0.0f, new Color(0, 0, 0, 255)),
            new ColorStop(1.0f, new Color(255, 255, 255, 255))
        };

        var pixels = RenderScene(ctx =>
        {
            using var recording = new Recording();
            ctx.Record(recording, recorder =>
            {
                ctx.SetPaintLinearGradient(0, 0, Width, 0, stops);
                recorder.FillRect(Rect.FromXYWH(0, 0, Width, Height));
            });
            ctx.PrepareRecording(recording);
            ctx.ExecuteRecording(recording);
        });

        Assert.Equal(ExpectedGradientValue(0), pixels[0]);
        Assert.Equal(ExpectedGradientValue(Width - 1), pixels[^1]);
        Assert.Equal(ExpectedGradientValue(Width / 2), pixels[Width / 2]);
    }

    [Fact]
    public void StrokeRectProducesOutline()
    {
        var pixels = RenderScene(ctx =>
        {
            ctx.SetPaint(new Color(0, 0, 255, 255));
            ctx.SetStroke(new Stroke(width: 2.0f, join: Join.Bevel, startCap: Cap.Butt, endCap: Cap.Butt));
            ctx.StrokeRect(Rect.FromXYWH(4, 4, Width - 8, Height - 8));
        });

        AssertTrueIsBlue(pixels[Width * 4 + 4]);
        AssertTransparent(pixels[Width * (Height / 2) + (Width / 2)]);
    }

    [Fact]
    public void RecordedStrokeProducesOutline()
    {
        var stroke = new Stroke(width: 2.0f, join: Join.Bevel, startCap: Cap.Butt, endCap: Cap.Butt);

        var pixels = RenderScene(ctx =>
        {
            ctx.SetStroke(stroke);
            using var recording = new Recording();
            ctx.Record(recording, recorder =>
            {
                recorder.SetPaint(new Color(0, 0, 255, 255));
                recorder.StrokeRect(Rect.FromXYWH(4, 4, Width - 8, Height - 8));
            });
            ctx.PrepareRecording(recording);
            ctx.ExecuteRecording(recording);
        });

        AssertTrueIsBlue(pixels[Width * 4 + 4]);
        AssertTransparent(pixels[Width * (Height / 2) + (Width / 2)]);
    }

    [Fact]
    public void RadialGradientProducesExpectedCenter()
    {
        var pixels = RenderScene(ctx =>
        {
            var stops = new[]
            {
                new ColorStop(0.0f, new Color(255, 0, 0, 255)),
                new ColorStop(1.0f, new Color(0, 0, 255, 255))
            };

            ctx.SetPaintRadialGradient(Width / 2.0, Height / 2.0, Width / 2.0, stops);
            ctx.FillRect(Rect.FromXYWH(0, 0, Width, Height));
        });

        var center = pixels[(Height / 2) * Width + (Width / 2)];
        Assert.InRange(center.R, 240, 255);
        Assert.InRange(center.B, 0, 20);
        Assert.InRange(center.A, 240, 255);

        var edge = pixels[0];
        Assert.InRange(edge.B, 240, 255);
        Assert.InRange(edge.R, 0, 20);
        Assert.Equal(255, edge.A);
    }

    [Fact]
    public void SweepGradientSetsPaintKind()
    {
        using var ctx = CreateContext();
        var stops = new[]
        {
            new ColorStop(0.0f, new Color(255, 255, 0, 255)),
            new ColorStop(1.0f, new Color(255, 0, 0, 255))
        };

        ctx.SetPaintSweepGradient(Width / 2.0, Height / 2.0, 0f, (float)(2 * Math.PI), stops);
        Assert.Equal(PaintKind.SweepGradient, ctx.GetPaintKind());
    }

    [Fact]
    public void InvalidPixmapHandleThrowsNativeError()
    {
        var result = NativeMethods.RenderContext_RenderToPixmap(0, 0);
        Assert.NotEqual(NativeMethods.VELLO_OK, result);
    }

    [Fact]
    public unsafe void NativeLinearGradientRejectsTooFewStops()
    {
        nint ctx = NativeMethods.RenderContext_New(Width, Height);
        try
        {
            VelloColorStop stop = new()
            {
                Offset = 0,
                R = 0,
                G = 0,
                B = 0,
                A = 255
            };

            var error = NativeMethods.RenderContext_SetPaintLinearGradient(
                ctx,
                0,
                0,
                Width,
                Height,
                &stop,
                1,
                VelloExtend.Pad);

            Assert.NotEqual(NativeMethods.VELLO_OK, error);
        }
        finally
        {
            NativeMethods.RenderContext_Free(ctx);
        }
    }

    [Fact(Skip = "Requires native guard for freed pixmap handles")]
    public void NativeRenderToFreedPixmapReturnsError()
    {
        // TODO: enable once the native layer reports a deterministic error instead of aborting.
        // nint ctx = NativeMethods.RenderContext_New(Width, Height);
        // nint pixmap = NativeMethods.Pixmap_New(Width, Height);
        // NativeMethods.Pixmap_Free(pixmap);
        // var error = NativeMethods.RenderContext_RenderToPixmap(ctx, pixmap);
        // Assert.NotEqual(NativeMethods.VELLO_OK, error);
    }

    private static RenderContext CreateContext() =>
        new(Width, Height, new RenderSettings(SimdLevel.Avx2, numThreads: 0, mode: RenderMode.OptimizeSpeed));

    private static PremulRgba8[] RenderScene(Action<RenderContext> draw)
    {
        using var ctx = CreateContext();
        using var pixmap = new Pixmap(Width, Height);

        draw(ctx);

        ctx.Flush();
        ctx.RenderToPixmap(pixmap);

        return pixmap.GetPixels().ToArray();
    }

    private static PremulRgba8 ExpectedGradientValue(int x)
    {
        double t = (x + 0.5) / Width;
        byte value = (byte)Math.Clamp(Math.Floor(t * 255.0), 0, 255);
        return new PremulRgba8(value, value, value, 255);
    }

    private static void AssertTrueIsBlue(PremulRgba8 pixel)
    {
        Assert.InRange(pixel.B, 200, 255);
        Assert.InRange(pixel.A, 200, 255);
    }

    private static void AssertTransparent(PremulRgba8 pixel)
    {
        Assert.InRange(pixel.A, 0, 5);
    }
}
