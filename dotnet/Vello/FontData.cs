// Copyright 2025 Wieslaw Soltes
// SPDX-License-Identifier: Apache-2.0 OR MIT

using System.Text;
using Vello.Native;

namespace Vello;

/// <summary>
/// Represents font data loaded from a TrueType or OpenType font file.
/// </summary>
public sealed class FontData : IDisposable
{
    private nint _handle;
    private bool _disposed;

    /// <summary>
    /// Creates a new FontData from font file bytes.
    /// </summary>
    /// <param name="fontBytes">The font file bytes (TTF, OTF, etc.)</param>
    /// <param name="index">Font index in collection (0 for single fonts)</param>
    public unsafe FontData(byte[] fontBytes, uint index = 0)
    {
        ArgumentNullException.ThrowIfNull(fontBytes);
        if (fontBytes.Length == 0)
            throw new ArgumentException("Font data cannot be empty", nameof(fontBytes));

        fixed (byte* dataPtr = fontBytes)
        {
            _handle = NativeMethods.FontData_New(dataPtr, (nuint)fontBytes.Length, index);
            if (_handle == 0)
                throw new VelloException("Failed to create font data");
        }
    }

    /// <summary>
    /// Loads a font from a file.
    /// </summary>
    public static FontData FromFile(string path, uint index = 0)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);
        byte[] fontBytes = File.ReadAllBytes(path);
        return new FontData(fontBytes, index);
    }

    /// <summary>
    /// Converts UTF-8 text to glyphs with simple character-to-glyph mapping.
    /// Note: This performs basic glyph ID mapping only, not full text shaping.
    /// For complex scripts, ligatures, or proper kerning, use a text shaping library.
    /// </summary>
    public unsafe Glyph[] TextToGlyphs(string text)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        ArgumentNullException.ThrowIfNull(text);

        // Convert text to UTF-8
        byte[] utf8Bytes = Encoding.UTF8.GetBytes(text + "\0"); // Null-terminated

        // Allocate enough space for glyphs (worst case: one glyph per character)
        var glyphs = new VelloGlyph[text.Length];
        nuint count;

        fixed (byte* textPtr = utf8Bytes)
        fixed (VelloGlyph* glyphsPtr = glyphs)
        {
            VelloException.ThrowIfError(
                NativeMethods.FontData_TextToGlyphs(
                    _handle,
                    textPtr,
                    glyphsPtr,
                    (nuint)glyphs.Length,
                    &count));
        }

        // Convert to public Glyph type
        var result = new Glyph[count];
        for (int i = 0; i < (int)count; i++)
        {
            result[i] = new Glyph(glyphs[i].Id, glyphs[i].X, glyphs[i].Y);
        }

        return result;
    }

    internal nint Handle
    {
        get
        {
            ObjectDisposedException.ThrowIf(_disposed, this);
            return _handle;
        }
    }

    public void Dispose()
    {
        if (!_disposed)
        {
            if (_handle != 0)
            {
                NativeMethods.FontData_Free(_handle);
                _handle = 0;
            }
            _disposed = true;
        }
        GC.SuppressFinalize(this);
    }

    ~FontData() => Dispose();
}
