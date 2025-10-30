// Copyright 2025 Wieslaw Soltes
// SPDX-License-Identifier: Apache-2.0 OR MIT

using Vello;

namespace Vello.Tests;

public class BezPathTests
{
    [Fact]
    public void BezPath_Constructor_CreatesValidPath()
    {
        using var path = new BezPath();
        // Should not throw
    }

    [Fact]
    public void BezPath_FluentAPI_WorksCorrectly()
    {
        using var path = new BezPath();

        var result = path
            .MoveTo(10, 20)
            .LineTo(30, 40)
            .QuadTo(50, 60, 70, 80)
            .CurveTo(90, 100, 110, 120, 130, 140)
            .Close();

        Assert.Same(path, result); // Verify fluent API returns same instance
    }

    [Fact]
    public void BezPath_Clear_DoesNotThrow()
    {
        using var path = new BezPath();
        path.MoveTo(10, 20).LineTo(30, 40);
        path.Clear(); // Should not throw
    }

    [Fact]
    public void BezPath_Dispose_CanBeCalledMultipleTimes()
    {
        var path = new BezPath();
        path.Dispose();
        path.Dispose(); // Should not throw
    }

    [Fact]
    public void BezPath_AfterDispose_ThrowsObjectDisposedException()
    {
        var path = new BezPath();
        path.Dispose();

        Assert.Throws<ObjectDisposedException>(() => path.MoveTo(0, 0));
    }

    [Fact]
    public void BezPath_ComplexPath_CanBeBuilt()
    {
        using var path = new BezPath();

        // Create a star shape
        path.MoveTo(100, 10);
        for (int i = 0; i < 5; i++)
        {
            double angle1 = i * 2 * Math.PI / 5 - Math.PI / 2;
            double angle2 = (i + 0.5) * 2 * Math.PI / 5 - Math.PI / 2;

            path.LineTo(100 + 90 * Math.Cos(angle1), 100 + 90 * Math.Sin(angle1));
            path.LineTo(100 + 40 * Math.Cos(angle2), 100 + 40 * Math.Sin(angle2));
        }
        path.Close();

        // Should not throw
    }
}
