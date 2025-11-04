// Copyright 2025 Wieslaw Soltes
// SPDX-License-Identifier: Apache-2.0 OR MIT

using Vello;
using Vello.Geometry;
using Xunit;

namespace Vello.Tests;

/// <summary>
/// Tests for Phase 2 Span-based APIs (PNG I/O, Pixmap byte access, FontData constructor).
/// </summary>
public class Phase2PerformanceTests
{
    #region Pixmap GetBytes / CopyBytesTo Tests

    [Fact]
    public void Pixmap_GetBytes_ReturnsCorrectByteCount()
    {
        using var pixmap = new Pixmap(100, 50);

        var bytes = pixmap.GetBytes();

        // 100 * 50 pixels * 4 bytes per pixel
        Assert.Equal(100 * 50 * 4, bytes.Length);
    }

    [Fact]
    public void Pixmap_GetBytes_ZeroCopyAccess()
    {
        using var context = new RenderContext(100, 50);
        using var pixmap = new Pixmap(100, 50);

        // Draw something
        context.SetPaint(Color.Red);
        context.FillRect(Rect.FromXYWH(10, 10, 20, 20));
        context.Flush();
        context.RenderToPixmap(pixmap);

        // Zero-copy byte access
        var bytes = pixmap.GetBytes();

        // Verify some red pixels exist
        bool hasRedPixel = false;
        for (int i = 0; i < bytes.Length; i += 4)
        {
            if (bytes[i] > 200) // High red value
            {
                hasRedPixel = true;
                break;
            }
        }

        Assert.True(hasRedPixel, "Expected to find red pixels");
    }

    [Fact]
    public void Pixmap_CopyBytesTo_CopiesCorrectly()
    {
        using var context = new RenderContext(50, 50);
        using var pixmap = new Pixmap(50, 50);

        // Draw blue square
        context.SetPaint(Color.Blue);
        context.FillRect(Rect.FromXYWH(0, 0, 50, 50));
        context.Flush();
        context.RenderToPixmap(pixmap);

        // Copy to destination span
        Span<byte> destination = new byte[50 * 50 * 4];
        pixmap.CopyBytesTo(destination);

        // Verify blue pixels (R=0, G=0, B=255, A=255)
        Assert.Equal(0, destination[0]);   // R
        Assert.Equal(0, destination[1]);   // G
        Assert.Equal(255, destination[2]); // B
        Assert.Equal(255, destination[3]); // A
    }

    [Fact]
    public void Pixmap_CopyBytesTo_ThrowsIfDestinationTooSmall()
    {
        using var pixmap = new Pixmap(100, 100);

        var tooSmall = new byte[100]; // Way too small - use array to avoid ref local in lambda

        Assert.Throws<ArgumentException>(() => pixmap.CopyBytesTo(tooSmall.AsSpan()));
    }

    #endregion

    #region PNG I/O with Span Tests

    [Fact]
    public void Pixmap_FromPng_WithSpan_WorksCorrectly()
    {
        // Create a pixmap and export to PNG
        using var original = new Pixmap(50, 30);
        byte[] pngData = original.ToPng();

        // Load from ReadOnlySpan (zero-allocation)
        using var loaded = Pixmap.FromPng(pngData.AsSpan());

        Assert.Equal(50, loaded.Width);
        Assert.Equal(30, loaded.Height);
    }

    [Fact]
    public void Pixmap_FromPng_ArrayOverload_StillWorks()
    {
        // Create a pixmap and export to PNG
        using var original = new Pixmap(50, 30);
        byte[] pngData = original.ToPng();

        // Load from array (backward compatibility)
        using var loaded = Pixmap.FromPng(pngData);

        Assert.Equal(50, loaded.Width);
        Assert.Equal(30, loaded.Height);
    }

    [Fact]
    public void Pixmap_TryToPng_SucceedsWithLargeEnoughBuffer()
    {
        using var pixmap = new Pixmap(50, 50);

        // Get the required size
        int requiredSize = pixmap.GetPngSize();

        // Allocate buffer
        Span<byte> buffer = new byte[requiredSize];

        // Try to encode
        bool success = pixmap.TryToPng(buffer, out int bytesWritten);

        Assert.True(success);
        Assert.Equal(requiredSize, bytesWritten);
        Assert.True(bytesWritten > 0);
    }

    [Fact]
    public void Pixmap_TryToPng_FailsWithSmallBuffer()
    {
        using var pixmap = new Pixmap(100, 100);

        // Too small buffer
        Span<byte> buffer = new byte[100];

        // Try to encode (should fail)
        bool success = pixmap.TryToPng(buffer, out int bytesWritten);

        Assert.False(success);
        Assert.Equal(0, bytesWritten);
    }

    [Fact]
    public void Pixmap_GetPngSize_ReturnsPositiveValue()
    {
        using var pixmap = new Pixmap(100, 100);

        int size = pixmap.GetPngSize();

        Assert.True(size > 0, "PNG size should be positive");
        Assert.True(size > 100, "PNG should be larger than 100 bytes");
    }

    [Fact]
    public void Pixmap_ToPng_RoundTrip_PreservesData()
    {
        using var context = new RenderContext(50, 50);
        using var original = new Pixmap(50, 50);

        // Draw red rectangle
        context.SetPaint(Color.Red);
        context.FillRect(Rect.FromXYWH(10, 10, 30, 30));
        context.Flush();
        context.RenderToPixmap(original);

        // Export and reload
        byte[] pngData = original.ToPng();
        using var reloaded = Pixmap.FromPng(pngData);

        // Verify dimensions
        Assert.Equal(50, reloaded.Width);
        Assert.Equal(50, reloaded.Height);

        // Verify some red pixels exist
        var pixels = reloaded.GetPixels();
        bool hasRedPixel = false;
        for (int i = 0; i < pixels.Length; i++)
        {
            if (pixels[i].R > 200)
            {
                hasRedPixel = true;
                break;
            }
        }
        Assert.True(hasRedPixel);
    }

    #endregion

    #region FontData Constructor with Span Tests

    [Fact]
    public void FontData_Constructor_WithSpan_WorksCorrectly()
    {
        var fontPath = FindSystemFont();
        if (fontPath == null) return;

        // Read font bytes
        byte[] fontBytes = File.ReadAllBytes(fontPath);

        // Create FontData from ReadOnlySpan (zero-allocation)
        using var font = new FontData(fontBytes.AsSpan());

        // Verify it works by converting text
        var glyphs = font.TextToGlyphs("Test");
        Assert.NotEmpty(glyphs);
    }

    [Fact]
    public void FontData_Constructor_ArrayOverload_StillWorks()
    {
        var fontPath = FindSystemFont();
        if (fontPath == null) return;

        // Read font bytes
        byte[] fontBytes = File.ReadAllBytes(fontPath);

        // Create FontData from array (backward compatibility)
        using var font = new FontData(fontBytes);

        // Verify it works
        var glyphs = font.TextToGlyphs("Test");
        Assert.NotEmpty(glyphs);
    }

    [Fact]
    public void FontData_Constructor_WithEmptySpan_ThrowsException()
    {
        var empty = Array.Empty<byte>(); // Use array to avoid ref local in lambda

        Assert.Throws<ArgumentException>(() => new FontData(empty.AsSpan()));
    }

    [Fact]
    public void FontData_Constructor_WithIndex_WorksCorrectly()
    {
        var fontPath = FindSystemFont();
        if (fontPath == null) return;

        byte[] fontBytes = File.ReadAllBytes(fontPath);

        // Create with index 0
        using var font = new FontData(fontBytes.AsSpan(), index: 0);

        var glyphs = font.TextToGlyphs("A");
        Assert.NotEmpty(glyphs);
    }

    #endregion

    #region Helper Methods

    private static string? FindSystemFont()
    {
        string[] possibleFonts = new[]
        {
            "/System/Library/Fonts/Helvetica.ttc",
            "/System/Library/Fonts/Supplemental/Arial.ttf",
            "/usr/share/fonts/truetype/dejavu/DejaVuSans.ttf",
            "C:\\Windows\\Fonts\\arial.ttf",
            "C:\\Windows\\Fonts\\segoeui.ttf",
        };

        return possibleFonts.FirstOrDefault(File.Exists);
    }

    #endregion
}
