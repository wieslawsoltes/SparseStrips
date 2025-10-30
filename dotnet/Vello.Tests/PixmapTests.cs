// Copyright 2025 Wieslaw Soltes
// SPDX-License-Identifier: Apache-2.0 OR MIT

using Vello;

namespace Vello.Tests;

public class PixmapTests
{
    [Fact]
    public void Pixmap_Constructor_CreateValidPixmap()
    {
        using var pixmap = new Pixmap(100, 200);

        Assert.Equal(100, pixmap.Width);
        Assert.Equal(200, pixmap.Height);
    }

    [Fact]
    public void Pixmap_Constructor_SmallDimensions_WorksCorrectly()
    {
        // Even 1x1 pixmaps should work
        using var pixmap = new Pixmap(1, 1);
        Assert.Equal(1, pixmap.Width);
        Assert.Equal(1, pixmap.Height);
    }

    [Fact]
    public void Pixmap_GetPixels_ReturnsCorrectLength()
    {
        using var pixmap = new Pixmap(50, 40);
        var pixels = pixmap.GetPixels();

        Assert.Equal(50 * 40, pixels.Length);
    }

    [Fact]
    public void Pixmap_GetPixels_IsZeroCopy()
    {
        using var pixmap = new Pixmap(10, 10);
        var pixels1 = pixmap.GetPixels();
        var pixels2 = pixmap.GetPixels();

        // Both spans should point to the same memory
        unsafe
        {
            fixed (PremulRgba8* ptr1 = pixels1)
            fixed (PremulRgba8* ptr2 = pixels2)
            {
                Assert.Equal((nint)ptr1, (nint)ptr2);
            }
        }
    }

    [Fact]
    public void Pixmap_ToByteArray_ReturnsCorrectLength()
    {
        using var pixmap = new Pixmap(25, 30);
        var bytes = pixmap.ToByteArray();

        Assert.Equal(25 * 30 * 4, bytes.Length); // 4 bytes per pixel (RGBA)
    }

    [Fact]
    public void Pixmap_Dispose_CanBeCalledMultipleTimes()
    {
        var pixmap = new Pixmap(10, 10);
        pixmap.Dispose();
        pixmap.Dispose(); // Should not throw
    }

    [Fact]
    public void Pixmap_AfterDispose_ThrowsObjectDisposedException()
    {
        var pixmap = new Pixmap(10, 10);
        pixmap.Dispose();

        Assert.Throws<ObjectDisposedException>(() => pixmap.GetPixels());
    }

    [Fact]
    public void Pixmap_PngRoundTrip_WorksCorrectly()
    {
        using var original = new Pixmap(50, 50);

        // Export to PNG
        byte[] pngData = original.ToPng();
        Assert.NotEmpty(pngData);

        // Load from PNG
        using var loaded = Pixmap.FromPng(pngData);
        Assert.Equal(original.Width, loaded.Width);
        Assert.Equal(original.Height, loaded.Height);
    }

    [Fact]
    public void Pixmap_FromPng_InvalidData_ThrowsException()
    {
        byte[] invalidData = new byte[] { 1, 2, 3, 4 };
        Assert.Throws<VelloException>(() => Pixmap.FromPng(invalidData));
    }

    [Fact]
    public void Pixmap_FromPng_EmptyData_ThrowsException()
    {
        Assert.Throws<ArgumentException>(() => Pixmap.FromPng(Array.Empty<byte>()));
    }
}
