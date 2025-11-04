// Copyright 2025 Wieslaw Soltes
// SPDX-License-Identifier: Apache-2.0 OR MIT

using Vello;
using Vello.Geometry;

namespace Vello.Tests;

public class TextRenderingTests
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

    [Fact]
    public void Glyph_Constructor_SetsCorrectValues()
    {
        var glyph = new Glyph(123, 45.5f, 67.8f);

        Assert.Equal(123u, glyph.Id);
        Assert.Equal(45.5f, glyph.X);
        Assert.Equal(67.8f, glyph.Y);
    }

    [Fact]
    public void Glyph_SimpleConstructor_SetsPositionToZero()
    {
        var glyph = new Glyph(456);

        Assert.Equal(456u, glyph.Id);
        Assert.Equal(0f, glyph.X);
        Assert.Equal(0f, glyph.Y);
    }

    [Fact]
    public void FontData_Constructor_NullBytes_ThrowsException()
    {
        Assert.Throws<ArgumentNullException>(() => new FontData(null!, 0));
    }

    [Fact]
    public void FontData_Constructor_EmptyBytes_ThrowsException()
    {
        Assert.Throws<ArgumentException>(() => new FontData(Array.Empty<byte>(), 0));
    }

    [Fact]
    public void FontData_FromFile_EmptyPath_ThrowsException()
    {
        Assert.Throws<ArgumentException>(() => FontData.FromFile("", 0));
    }

    [Fact]
    public void FontData_FromFile_NullPath_ThrowsException()
    {
        Assert.Throws<ArgumentNullException>(() => FontData.FromFile(null!, 0));
    }

    [Fact]
    public void FontData_FromFile_NonexistentFile_ThrowsIOException()
    {
        // Should throw some kind of IO exception for nonexistent paths
        var exception = Assert.ThrowsAny<Exception>(() => FontData.FromFile("/nonexistent/font.ttf", 0));
        Assert.True(exception is IOException || exception is DirectoryNotFoundException || exception is FileNotFoundException,
            "Expected an IO-related exception");
    }

    [Fact]
    public void FontData_LoadSystemFont_WorksCorrectly()
    {
        var fontPath = FindSystemFont();
        if (fontPath == null)
        {
            // Skip test if no system font available
            return;
        }

        using var font = FontData.FromFile(fontPath);
        // Should not throw
    }

    [Fact]
    public void FontData_TextToGlyphs_ReturnsGlyphs()
    {
        var fontPath = FindSystemFont();
        if (fontPath == null)
        {
            return; // Skip if no font available
        }

        using var font = FontData.FromFile(fontPath);
        var glyphs = font.TextToGlyphs("Hello");

        Assert.NotEmpty(glyphs);
        Assert.True(glyphs.Length <= 5); // Should be at most 5 glyphs for "Hello"
    }

    [Fact]
    public void FontData_TextToGlyphs_SimpleText_ReturnsGlyphs()
    {
        var fontPath = FindSystemFont();
        if (fontPath == null)
        {
            return;
        }

        using var font = FontData.FromFile(fontPath);
        var glyphs = font.TextToGlyphs("A");

        Assert.NotEmpty(glyphs);
        Assert.Single(glyphs);
    }

    [Fact]
    public void FontData_TextToGlyphs_NullText_ThrowsException()
    {
        var fontPath = FindSystemFont();
        if (fontPath == null)
        {
            return;
        }

        using var font = FontData.FromFile(fontPath);
        Assert.Throws<ArgumentNullException>(() => font.TextToGlyphs(null!));
    }

    [Fact]
    public void FontData_Dispose_CanBeCalledMultipleTimes()
    {
        var fontPath = FindSystemFont();
        if (fontPath == null)
        {
            return;
        }

        var font = FontData.FromFile(fontPath);
        font.Dispose();
        font.Dispose(); // Should not throw
    }

    [Fact]
    public void FontData_AfterDispose_ThrowsObjectDisposedException()
    {
        var fontPath = FindSystemFont();
        if (fontPath == null)
        {
            return;
        }

        var font = FontData.FromFile(fontPath);
        font.Dispose();

        Assert.Throws<ObjectDisposedException>(() => font.TextToGlyphs("test"));
    }

    [Fact]
    public void RenderContext_FillGlyphs_RendersText()
    {
        var fontPath = FindSystemFont();
        if (fontPath == null)
        {
            return;
        }

        using var context = new RenderContext(200, 100);
        using var pixmap = new Pixmap(200, 100);
        using var font = FontData.FromFile(fontPath);

        var glyphs = font.TextToGlyphs("Test");

        context.SetPaint(new Color(255, 0, 0, 255));
        context.FillGlyphs(font, 24.0f, glyphs);
        context.Flush();
        context.RenderToPixmap(pixmap);

        // Should render without throwing
        var pixels = pixmap.GetPixels();
        Assert.True(pixels.Length > 0);
    }

    [Fact]
    public void RenderContext_FillText_RendersText()
    {
        var fontPath = FindSystemFont();
        if (fontPath == null)
        {
            return;
        }

        using var context = new RenderContext(200, 100);
        using var pixmap = new Pixmap(200, 100);
        using var font = FontData.FromFile(fontPath);

        context.SetPaint(new Color(0, 255, 0, 255));
        context.FillText(font, 32.0f, "Hello", 10, 50);
        context.Flush();
        context.RenderToPixmap(pixmap);
        // Should not throw
    }

    [Fact]
    public void RenderContext_StrokeGlyphs_RendersTextOutline()
    {
        var fontPath = FindSystemFont();
        if (fontPath == null)
        {
            return;
        }

        using var context = new RenderContext(200, 100);
        using var pixmap = new Pixmap(200, 100);
        using var font = FontData.FromFile(fontPath);

        var glyphs = font.TextToGlyphs("Test");

        context.SetPaint(new Color(0, 0, 255, 255));
        context.SetStroke(new Stroke(2.0f));
        context.StrokeGlyphs(font, 24.0f, glyphs);
        context.Flush();
        context.RenderToPixmap(pixmap);
        // Should not throw
    }

    [Fact]
    public void RenderContext_StrokeText_RendersTextOutline()
    {
        var fontPath = FindSystemFont();
        if (fontPath == null)
        {
            return;
        }

        using var context = new RenderContext(200, 100);
        using var pixmap = new Pixmap(200, 100);
        using var font = FontData.FromFile(fontPath);

        context.SetPaint(new Color(255, 0, 255, 255));
        context.SetStroke(new Stroke(1.5f));
        context.StrokeText(font, 28.0f, "Outline", 10, 50);
        context.Flush();
        context.RenderToPixmap(pixmap);
        // Should not throw
    }
}
