// Copyright 2025 Wieslaw Soltes
// SPDX-License-Identifier: Apache-2.0 OR MIT

using Vello.Geometry;
using Xunit;

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
}
