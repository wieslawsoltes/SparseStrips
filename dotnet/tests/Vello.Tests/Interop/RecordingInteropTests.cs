// Copyright 2025
// SPDX-License-Identifier: Apache-2.0 OR MIT

using Vello.Native;
using Vello.Native.FastPath;
using Xunit;

namespace Vello.Tests.Interop;

public class RecordingInteropTests
{
    [Fact]
    public void NativeRecordingReportsCachedStripsAfterPrepare()
    {
        using var ctx = NativeTestHelpers.CreateContext();
        using var recording = new NativeRecording();

        ctx.Record(recording, static (ref NativeRecorder recorder) =>
        {
            recorder.SetPaintSolid(255, 0, 0, 255);
            recorder.FillRect(new VelloRect
            {
                X0 = 1,
                Y0 = 1,
                X1 = 15,
                Y1 = 15
            });
        });

        Assert.False(recording.HasCachedStrips);
        var stripCountBefore = recording.StripCount;
        var alphaCountBefore = recording.AlphaByteCount;

        ctx.PrepareRecording(recording);

        Assert.True(recording.HasCachedStrips);
        Assert.True(recording.StripCount >= stripCountBefore);
        Assert.True(recording.AlphaByteCount >= alphaCountBefore);
    }

    [Fact]
    public void NativeRecorderSupportsTransformAndClipOperations()
    {
        using var ctx = NativeTestHelpers.CreateContext();
        using var recording = new NativeRecording();
        using var pixmap = NativeTestHelpers.CreatePixmap();

        ctx.Record(recording, static (ref NativeRecorder recorder) =>
        {
            recorder.SetPaintSolid(0, 0, 255, 255);

            var stroke = new VelloStroke
            {
                Width = 2f,
                MiterLimit = 4f,
                Join = VelloJoin.Round,
                StartCap = VelloCap.Round,
                EndCap = VelloCap.Round
            };
            recorder.SetStroke(stroke);
            recorder.SetFillRule(VelloFillRule.EvenOdd);

            var transform = new VelloAffine
            {
                M11 = 1,
                M12 = 0,
                M13 = 2,
                M21 = 0,
                M22 = 1,
                M23 = 3
            };
            recorder.SetTransform(transform);

            var paintTransform = new VelloAffine
            {
                M11 = 1,
                M12 = 0,
                M13 = 0.5,
                M21 = 0,
                M22 = 1,
                M23 = 0.5
            };
            recorder.SetPaintTransform(paintTransform);

            using var clip = new NativeBezPath();
            clip.MoveTo(2, 2);
            clip.LineTo(14, 2);
            clip.LineTo(14, 14);
            clip.Close();

            recorder.PushClipLayer(clip);
            recorder.FillRect(new VelloRect
            {
                X0 = 0,
                Y0 = 0,
                X1 = 16,
                Y1 = 16
            });
            recorder.PopLayer();
            recorder.ResetPaintTransform();
        });

        ctx.PrepareRecording(recording);
        ctx.ExecuteRecording(recording);
        ctx.Flush();
        ctx.RenderToPixmap(pixmap);

        Assert.NotEqual(0u, recording.Length);
        Assert.True(pixmap.GetPixelCount() > 0u);
    }
}
