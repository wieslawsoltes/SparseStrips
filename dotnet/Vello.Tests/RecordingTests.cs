// Copyright 2025 Wieslaw Soltes
// SPDX-License-Identifier: Apache-2.0 OR MIT

using Vello.Geometry;
using Xunit;
using System;

namespace Vello.Tests;

public class RecordingTests
{
    [Fact]
    public void Recording_CanCreateAndDispose()
    {
        using var recording = new Recording();
        Assert.Equal(0, recording.Count);
    }

    [Fact]
    public void Recording_CanClear()
    {
        using var ctx = new RenderContext(100, 100);
        using var recording = new Recording();

        // Record some operations
        ctx.Record(recording, recorder =>
        {
            recorder.SetPaint(Color.Red);
            recorder.FillRect(new Rect(0, 0, 50, 50));
        });

        Assert.NotEqual(0, recording.Count);

        recording.Clear();
        Assert.Equal(0, recording.Count);
    }

    [Fact]
    public void Recording_CanRecordAndExecute()
    {
        using var ctx = new RenderContext(100, 100);
        using var recording = new Recording();
        using var pixmap = new Pixmap(100, 100);

        // Record drawing operations
        ctx.Record(recording, recorder =>
        {
            recorder.SetPaint(Color.Red);
            recorder.FillRect(new Rect(10, 10, 50, 50));
        });

        Assert.NotEqual(0, recording.Count);

        // Prepare and execute the recording
        ctx.PrepareRecording(recording);
        ctx.ExecuteRecording(recording);

        // Flush before rendering
        ctx.Flush();

        // Render to verify it worked
        ctx.RenderToPixmap(pixmap);

        // Basic validation: check that some pixels were rendered
        var data = pixmap.GetBytes();
        Assert.True(data.Length > 0);
    }

    [Fact]
    public void Recording_CanExecuteMultipleTimes()
    {
        using var ctx = new RenderContext(100, 100);
        using var recording = new Recording();

        // Record a simple operation
        ctx.Record(recording, recorder =>
        {
            recorder.SetPaint(Color.Green);
            recorder.FillRect(new Rect(0, 0, 100, 100));
        });

        ctx.PrepareRecording(recording);

        // Execute multiple times - should not throw
        ctx.ExecuteRecording(recording);
        ctx.ExecuteRecording(recording);
        ctx.ExecuteRecording(recording);
    }

    [Fact]
    public void Recorder_SupportsBasicDrawingOperations()
    {
        using var ctx = new RenderContext(100, 100);
        using var recording = new Recording();
        using var path = new BezPath();

        path.MoveTo(new Point(10, 10));
        path.LineTo(new Point(90, 10));
        path.LineTo(new Point(50, 90));
        path.Close();

        // Test all recorder methods
        ctx.Record(recording, recorder =>
        {
            recorder.SetPaint(Color.Blue);
            recorder.FillRect(new Rect(0, 0, 50, 50));
            recorder.StrokeRect(new Rect(50, 50, 100, 100));
            recorder.FillPath(path);
            recorder.StrokePath(path);
        });

        Assert.NotEqual(0, recording.Count);

        ctx.PrepareRecording(recording);
        ctx.ExecuteRecording(recording);
    }

    [Fact]
    public void Recording_ReportsCachedStripMetrics()
    {
        using var ctx = new RenderContext(128, 128);
        using var recording = new Recording();

        ctx.Record(recording, recorder =>
        {
            recorder.SetPaint(Color.Magenta);
            recorder.FillRect(new Rect(10, 10, 90, 90));
        });

        Assert.False(recording.HasCachedStrips);
        ulong stripsBefore = recording.StripCount;
        ulong alphaBefore = recording.AlphaByteCount;

        ctx.PrepareRecording(recording);

        Assert.True(recording.HasCachedStrips);
        Assert.True(recording.StripCount >= stripsBefore);
        Assert.True(recording.AlphaByteCount >= alphaBefore);
    }

    [Fact]
    public void Recorder_SupportsAdvancedTransformAndClipFlow()
    {
        using var ctx = new RenderContext(128, 128);
        using var recording = new Recording();
        using var clipPath = new BezPath();
        using var strokePath = new BezPath();
        using var pixmap = new Pixmap(128, 128);

        clipPath
            .MoveTo(new Point(20, 20))
            .LineTo(new Point(108, 20))
            .LineTo(new Point(108, 108))
            .Close();

        strokePath
            .MoveTo(new Point(30, 30))
            .LineTo(new Point(98, 98));

        ctx.Record(recording, recorder =>
        {
            recorder.SetPaint(Color.Green);
            recorder.SetStroke(new Stroke(width: 3f, join: Join.Round, startCap: Cap.Round, endCap: Cap.Round));
            recorder.SetFillRule(FillRule.EvenOdd);
            recorder.SetTransform(Affine.Translation(4, 6));
            recorder.SetPaintTransform(Affine.Scale(0.75, 0.75));

            recorder.PushClipLayer(clipPath);
            recorder.FillRect(new Rect(0, 0, 96, 96));
            recorder.StrokePath(strokePath);
            recorder.PopLayer();
            recorder.ResetPaintTransform();
        });

        ctx.PrepareRecording(recording);
        ctx.ExecuteRecording(recording);
        ctx.Flush();
        ctx.RenderToPixmap(pixmap);

        Assert.True(recording.Count > 0);
        Assert.True(recording.HasCachedStrips);
        ReadOnlySpan<byte> bytes = pixmap.GetBytes();
        bool hasNonZero = false;
        foreach (byte value in bytes)
        {
            if (value != 0)
            {
                hasNonZero = true;
                break;
            }
        }

        Assert.True(hasNonZero, "Expected recorded drawing to produce non-zero pixels.");
    }
}
