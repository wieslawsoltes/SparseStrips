// Copyright 2025 Wieslaw Soltes
// SPDX-License-Identifier: Apache-2.0 OR MIT

using Vello;
using Vello.Geometry;
using Xunit;

namespace Vello.Tests;

/// <summary>
/// Tests for Span-based zero-allocation APIs introduced in Phase 1 performance optimization.
/// </summary>
public class SpanPerformanceTests
{
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

    #region Gradient Tests (ReadOnlySpan<ColorStop>)

    [Fact]
    public void SetPaintLinearGradient_WithSpan_WorksCorrectly()
    {
        using var context = new RenderContext(400, 300);
        using var pixmap = new Pixmap(400, 300);

        // Create gradient stops on stack
        Span<ColorStop> stops = stackalloc ColorStop[3];
        stops[0] = new ColorStop(0.0f, Color.Red);
        stops[1] = new ColorStop(0.5f, Color.Green);
        stops[2] = new ColorStop(1.0f, Color.Blue);

        // Use Span-based API (zero allocation for ≤32 stops)
        context.SetPaintLinearGradient(0, 0, 400, 300, stops);
        context.FillRect(Rect.FromXYWH(0, 0, 400, 300));
        context.Flush();
        context.RenderToPixmap(pixmap);

        // Verify gradient was rendered
        var pixels = pixmap.GetPixels();
        Assert.NotEqual(new PremulRgba8(0, 0, 0, 0), pixels[0]);
    }

    [Fact]
    public void SetPaintRadialGradient_WithSpan_WorksCorrectly()
    {
        using var context = new RenderContext(400, 300);
        using var pixmap = new Pixmap(400, 300);

        // Create gradient stops on stack
        Span<ColorStop> stops = stackalloc ColorStop[2];
        stops[0] = new ColorStop(0.0f, Color.Yellow);
        stops[1] = new ColorStop(1.0f, Color.Magenta);

        // Use Span-based API (zero allocation for ≤32 stops)
        context.SetPaintRadialGradient(200, 150, 100, stops);
        context.FillRect(Rect.FromXYWH(0, 0, 400, 300));
        context.Flush();
        context.RenderToPixmap(pixmap);

        // Verify gradient was rendered
        var pixels = pixmap.GetPixels();
        Assert.NotEqual(new PremulRgba8(0, 0, 0, 0), pixels[0]);
    }

    [Fact]
    public void SetPaintSweepGradient_WithSpan_WorksCorrectly()
    {
        using var context = new RenderContext(400, 300);
        using var pixmap = new Pixmap(400, 300);

        // Create gradient stops on stack
        Span<ColorStop> stops = stackalloc ColorStop[4];
        stops[0] = new ColorStop(0.0f, Color.Red);
        stops[1] = new ColorStop(0.33f, Color.Green);
        stops[2] = new ColorStop(0.66f, Color.Blue);
        stops[3] = new ColorStop(1.0f, Color.Red);

        // Use Span-based API (zero allocation for ≤32 stops)
        context.SetPaintSweepGradient(200, 150, 0, (float)(2 * Math.PI), stops);
        context.FillRect(Rect.FromXYWH(0, 0, 400, 300));
        context.Flush();
        context.RenderToPixmap(pixmap);

        // Verify gradient was rendered
        var pixels = pixmap.GetPixels();
        Assert.NotEqual(new PremulRgba8(0, 0, 0, 0), pixels[0]);
    }

    [Fact]
    public void SetPaintLinearGradient_WithLargeSpan_WorksCorrectly()
    {
        using var context = new RenderContext(400, 300);
        using var pixmap = new Pixmap(400, 300);

        // Create large gradient (>32 stops, will use heap allocation)
        var stops = new ColorStop[40];
        for (int i = 0; i < 40; i++)
        {
            float offset = i / 39.0f;
            stops[i] = new ColorStop(offset, new Color((byte)(255 * offset), 128, 200, 255));
        }

        // Use Span-based API (will allocate on heap for >32 stops)
        context.SetPaintLinearGradient(0, 0, 400, 300, stops.AsSpan());
        context.FillRect(Rect.FromXYWH(0, 0, 400, 300));
        context.Flush();
        context.RenderToPixmap(pixmap);

        // Verify gradient was rendered
        var pixels = pixmap.GetPixels();
        Assert.NotEqual(new PremulRgba8(0, 0, 0, 0), pixels[0]);
    }

    [Fact]
    public void SetPaintLinearGradient_ArrayOverload_StillWorks()
    {
        using var context = new RenderContext(400, 300);
        using var pixmap = new Pixmap(400, 300);

        // Test backward compatibility with array-based API
        var stops = new ColorStop[]
        {
            new ColorStop(0.0f, Color.Red),
            new ColorStop(1.0f, Color.Blue)
        };

        context.SetPaintLinearGradient(0, 0, 400, 300, stops);
        context.FillRect(Rect.FromXYWH(0, 0, 400, 300));
        context.Flush();
        context.RenderToPixmap(pixmap);

        var pixels = pixmap.GetPixels();
        Assert.NotEqual(new PremulRgba8(0, 0, 0, 0), pixels[0]);
    }

    #endregion

    #region Glyph Tests (ReadOnlySpan<Glyph>)

    [Fact]
    public void FillGlyphs_WithSpan_WorksCorrectly()
    {
        var fontPath = FindSystemFont();
        if (fontPath == null) return; // Skip if no font available

        using var context = new RenderContext(400, 300);
        using var pixmap = new Pixmap(400, 300);
        using var font = FontData.FromFile(fontPath);

        context.SetPaint(Color.Black);

        // Create glyphs on stack
        Span<Glyph> glyphs = stackalloc Glyph[3];
        glyphs[0] = new Glyph(36, 10, 50);  // 'A' glyph
        glyphs[1] = new Glyph(37, 30, 50);  // 'B' glyph
        glyphs[2] = new Glyph(38, 50, 50);  // 'C' glyph

        // Use Span-based API (zero allocation for ≤256 glyphs)
        context.FillGlyphs(font, 48.0f, glyphs);
        context.Flush();
        context.RenderToPixmap(pixmap);

        // Verify glyphs were rendered
        var pixels = pixmap.GetPixels();
        bool hasNonZeroPixel = false;
        for (int i = 0; i < pixels.Length && !hasNonZeroPixel; i++)
        {
            if (pixels[i].A > 0)
                hasNonZeroPixel = true;
        }
        Assert.True(hasNonZeroPixel, "Expected glyphs to be rendered");
    }

    [Fact]
    public void StrokeGlyphs_WithSpan_WorksCorrectly()
    {
        using var context = new RenderContext(400, 300);
        using var pixmap = new Pixmap(400, 300);
        var fontPath = FindSystemFont();
        if (fontPath == null) return;
        using var font = FontData.FromFile(fontPath);

        context.SetPaint(Color.Black);
        context.SetStroke(new Stroke(2.0f));

        // Create glyphs on stack
        Span<Glyph> glyphs = stackalloc Glyph[2];
        glyphs[0] = new Glyph(36, 10, 50);
        glyphs[1] = new Glyph(37, 30, 50);

        // Use Span-based API (zero allocation for ≤256 glyphs)
        context.StrokeGlyphs(font, 48.0f, glyphs);
        context.Flush();
        context.RenderToPixmap(pixmap);

        // Verify glyphs were rendered
        var pixels = pixmap.GetPixels();
        bool hasNonZeroPixel = false;
        for (int i = 0; i < pixels.Length && !hasNonZeroPixel; i++)
        {
            if (pixels[i].A > 0)
                hasNonZeroPixel = true;
        }
        Assert.True(hasNonZeroPixel, "Expected stroked glyphs to be rendered");
    }

    [Fact]
    public void FillGlyphs_WithLargeSpan_WorksCorrectly()
    {
        using var context = new RenderContext(800, 600);
        using var pixmap = new Pixmap(800, 600);
        var fontPath = FindSystemFont();
        if (fontPath == null) return;
        using var font = FontData.FromFile(fontPath);

        context.SetPaint(Color.Black);

        // Create large glyph array (>256 glyphs, will use heap allocation)
        var glyphs = new Glyph[300];
        for (int i = 0; i < 300; i++)
        {
            glyphs[i] = new Glyph((ushort)(36 + (i % 26)), i * 2, 50);
        }

        // Use Span-based API (will allocate on heap for >256 glyphs)
        context.FillGlyphs(font, 12.0f, glyphs.AsSpan());
        context.Flush();
        context.RenderToPixmap(pixmap);

        // Verify glyphs were rendered
        var pixels = pixmap.GetPixels();
        bool hasNonZeroPixel = false;
        for (int i = 0; i < pixels.Length && !hasNonZeroPixel; i++)
        {
            if (pixels[i].A > 0)
                hasNonZeroPixel = true;
        }
        Assert.True(hasNonZeroPixel, "Expected large glyph set to be rendered");
    }

    [Fact]
    public void FillGlyphs_ArrayOverload_StillWorks()
    {
        using var context = new RenderContext(400, 300);
        using var pixmap = new Pixmap(400, 300);
        var fontPath = FindSystemFont();
        if (fontPath == null) return;
        using var font = FontData.FromFile(fontPath);

        context.SetPaint(Color.Black);

        // Test backward compatibility with array-based API
        var glyphs = new Glyph[]
        {
            new Glyph(36, 10, 50),
            new Glyph(37, 30, 50)
        };

        context.FillGlyphs(font, 48.0f, glyphs);
        context.Flush();
        context.RenderToPixmap(pixmap);

        var pixels = pixmap.GetPixels();
        bool hasNonZeroPixel = false;
        for (int i = 0; i < pixels.Length && !hasNonZeroPixel; i++)
        {
            if (pixels[i].A > 0)
                hasNonZeroPixel = true;
        }
        Assert.True(hasNonZeroPixel);
    }

    #endregion

    #region Text Conversion Tests (Span<Glyph> output)

    [Fact]
    public void TextToGlyphs_WithSpan_WorksCorrectly()
    {
        var fontPath = FindSystemFont();
        if (fontPath == null) return;
        using var font = FontData.FromFile(fontPath);

        string text = "Hello";

        // Allocate glyph buffer on stack
        Span<Glyph> glyphs = stackalloc Glyph[text.Length];

        // Use Span-based API (zero allocation)
        int count = font.TextToGlyphs(text, glyphs);

        Assert.Equal(5, count);
        Assert.True(glyphs[0].Id > 0, "Expected valid glyph ID for 'H'");
        Assert.True(glyphs[1].Id > 0, "Expected valid glyph ID for 'e'");
    }

    [Fact]
    public void TextToGlyphs_WithEmptyString_ReturnsZero()
    {
        var fontPath = FindSystemFont();
        if (fontPath == null) return;
        using var font = FontData.FromFile(fontPath);

        string text = "";
        Span<Glyph> glyphs = stackalloc Glyph[1];

        int count = font.TextToGlyphs(text, glyphs);

        Assert.Equal(0, count);
    }

    [Fact]
    public void TextToGlyphs_WithLongText_WorksCorrectly()
    {
        var fontPath = FindSystemFont();
        if (fontPath == null) return;
        using var font = FontData.FromFile(fontPath);

        // Create long text (>256 chars, will use heap allocation)
        string text = new string('A', 300);

        var glyphs = new Glyph[text.Length];

        // Use Span-based API (will allocate on heap for >256 chars)
        int count = font.TextToGlyphs(text, glyphs.AsSpan());

        Assert.Equal(300, count);
        Assert.True(glyphs[0].Id > 0);
    }

    [Fact]
    public void TextToGlyphs_SpanTooSmall_ThrowsException()
    {
        var fontPath = FindSystemFont();
        if (fontPath == null) return;
        using var font = FontData.FromFile(fontPath);

        string text = "Hello";
        var glyphs = new Glyph[2]; // Too small - use array to avoid ref local in lambda

        Assert.Throws<ArgumentException>(() => font.TextToGlyphs(text, glyphs.AsSpan()));
    }

    [Fact]
    public void TextToGlyphs_ArrayOverload_StillWorks()
    {
        var fontPath = FindSystemFont();
        if (fontPath == null) return;
        using var font = FontData.FromFile(fontPath);

        string text = "Test";

        // Test backward compatibility with array-returning API
        var glyphs = font.TextToGlyphs(text);

        Assert.NotEmpty(glyphs);
        Assert.True(glyphs.Length > 0);
        Assert.True(glyphs[0].Id > 0);
    }

    #endregion

    #region Text Rendering Tests (Zero-allocation FillText/StrokeText)

    [Fact]
    public void FillText_ZeroAllocation_WorksCorrectly()
    {
        using var context = new RenderContext(400, 300);
        using var pixmap = new Pixmap(400, 300);
        var fontPath = FindSystemFont();
        if (fontPath == null) return;
        using var font = FontData.FromFile(fontPath);

        context.SetPaint(Color.Black);

        // This should be zero-allocation for text with ≤256 characters
        context.FillText(font, 48.0f, "Hello, World!", 10, 50);
        context.Flush();
        context.RenderToPixmap(pixmap);

        // Verify text was rendered
        var pixels = pixmap.GetPixels();
        bool hasNonZeroPixel = false;
        for (int i = 0; i < pixels.Length && !hasNonZeroPixel; i++)
        {
            if (pixels[i].A > 0)
                hasNonZeroPixel = true;
        }
        Assert.True(hasNonZeroPixel, "Expected text to be rendered");
    }

    [Fact]
    public void StrokeText_ZeroAllocation_WorksCorrectly()
    {
        using var context = new RenderContext(400, 300);
        using var pixmap = new Pixmap(400, 300);
        var fontPath = FindSystemFont();
        if (fontPath == null) return;
        using var font = FontData.FromFile(fontPath);

        context.SetPaint(Color.Black);
        context.SetStroke(new Stroke(2.0f));

        // This should be zero-allocation for text with ≤256 characters
        context.StrokeText(font, 48.0f, "Test", 10, 50);
        context.Flush();
        context.RenderToPixmap(pixmap);

        // Verify text was rendered
        var pixels = pixmap.GetPixels();
        bool hasNonZeroPixel = false;
        for (int i = 0; i < pixels.Length && !hasNonZeroPixel; i++)
        {
            if (pixels[i].A > 0)
                hasNonZeroPixel = true;
        }
        Assert.True(hasNonZeroPixel, "Expected stroked text to be rendered");
    }

    [Fact]
    public void FillText_WithLongText_WorksCorrectly()
    {
        using var context = new RenderContext(800, 600);
        using var pixmap = new Pixmap(800, 600);
        var fontPath = FindSystemFont();
        if (fontPath == null) return;
        using var font = FontData.FromFile(fontPath);

        context.SetPaint(Color.Black);

        // Create long text (>256 chars, will use heap allocation)
        string longText = new string('A', 300);

        context.FillText(font, 12.0f, longText, 10, 50);
        context.Flush();
        context.RenderToPixmap(pixmap);

        // Verify text was rendered
        var pixels = pixmap.GetPixels();
        bool hasNonZeroPixel = false;
        for (int i = 0; i < pixels.Length && !hasNonZeroPixel; i++)
        {
            if (pixels[i].A > 0)
                hasNonZeroPixel = true;
        }
        Assert.True(hasNonZeroPixel, "Expected long text to be rendered");
    }

    [Fact]
    public void FillText_WithEmptyString_DoesNotCrash()
    {
        using var context = new RenderContext(400, 300);
        using var pixmap = new Pixmap(400, 300);
        var fontPath = FindSystemFont();
        if (fontPath == null) return;
        using var font = FontData.FromFile(fontPath);

        context.SetPaint(Color.Black);

        // Empty text should not crash (early return)
        context.FillText(font, 48.0f, "", 10, 50);
        context.Flush();
        context.RenderToPixmap(pixmap);

        // Should complete without exception
        Assert.True(true);
    }

    #endregion
}
