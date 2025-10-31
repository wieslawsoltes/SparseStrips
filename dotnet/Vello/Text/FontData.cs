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
    /// Creates a new FontData from font file bytes (zero-allocation for ReadOnlySpan sources).
    /// </summary>
    /// <param name="fontBytes">The font file bytes (TTF, OTF, etc.)</param>
    /// <param name="index">Font index in collection (0 for single fonts)</param>
    public unsafe FontData(ReadOnlySpan<byte> fontBytes, uint index = 0)
    {
        if (fontBytes.IsEmpty)
            throw new ArgumentException("Font data cannot be empty", nameof(fontBytes));

        fixed (byte* dataPtr = fontBytes)
        {
            _handle = NativeMethods.FontData_New(dataPtr, (nuint)fontBytes.Length, index);
            if (_handle == 0)
                throw new VelloException("Failed to create font data");
        }
    }

    /// <summary>
    /// Creates a new FontData from font file bytes (array overload).
    /// For zero-allocation, use the ReadOnlySpan&lt;byte&gt; overload.
    /// </summary>
    public FontData(byte[] fontBytes, uint index = 0)
        : this((ReadOnlySpan<byte>)(fontBytes ?? throw new ArgumentNullException(nameof(fontBytes))), index)
    {
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
    /// Zero-allocation version that writes to provided span.
    /// Note: This performs basic glyph ID mapping only, not full text shaping.
    /// For complex scripts, ligatures, or proper kerning, use a text shaping library.
    /// </summary>
    /// <param name="text">The text to convert to glyphs</param>
    /// <param name="destination">The destination span to write glyphs to. Must be at least text.Length in size.</param>
    /// <returns>The number of glyphs written to the destination span</returns>
    public unsafe int TextToGlyphs(string text, Span<Glyph> destination)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        ArgumentNullException.ThrowIfNull(text);

        if (destination.Length < text.Length)
            throw new ArgumentException($"Destination span must be at least {text.Length} elements", nameof(destination));

        if (text.Length == 0)
            return 0;

        // Convert text to UTF-8 with null terminator
        // Use stackalloc for small strings (≤256 chars = ~768 bytes for UTF-8)
        const int StackAllocThreshold = 256;
        int maxUtf8Length = Encoding.UTF8.GetMaxByteCount(text.Length) + 1; // +1 for null terminator

        Span<byte> utf8Bytes = maxUtf8Length <= StackAllocThreshold
            ? stackalloc byte[maxUtf8Length]
            : new byte[maxUtf8Length];

        int utf8Length = Encoding.UTF8.GetBytes(text, utf8Bytes);
        utf8Bytes[utf8Length] = 0; // Null terminator

        // Allocate temporary native glyphs buffer
        // Use stackalloc for typical text (≤256 glyphs = 3KB)
        Span<VelloGlyph> nativeGlyphs = text.Length <= StackAllocThreshold
            ? stackalloc VelloGlyph[text.Length]
            : new VelloGlyph[text.Length];

        nuint count;

        fixed (byte* textPtr = utf8Bytes)
        fixed (VelloGlyph* glyphsPtr = nativeGlyphs)
        {
            VelloException.ThrowIfError(
                NativeMethods.FontData_TextToGlyphs(
                    _handle,
                    textPtr,
                    glyphsPtr,
                    (nuint)nativeGlyphs.Length,
                    &count));
        }

        // Convert to public Glyph type in destination span
        for (int i = 0; i < (int)count; i++)
        {
            destination[i] = new Glyph(nativeGlyphs[i].Id, nativeGlyphs[i].X, nativeGlyphs[i].Y);
        }

        return (int)count;
    }

    /// <summary>
    /// Converts UTF-8 text to glyphs with simple character-to-glyph mapping.
    /// Returns allocated array. Consider using the Span overload for zero-allocation scenarios.
    /// Note: This performs basic glyph ID mapping only, not full text shaping.
    /// For complex scripts, ligatures, or proper kerning, use a text shaping library.
    /// </summary>
    public Glyph[] TextToGlyphs(string text)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        ArgumentNullException.ThrowIfNull(text);

        if (text.Length == 0)
            return Array.Empty<Glyph>();

        // Allocate result array
        var result = new Glyph[text.Length];
        int count = TextToGlyphs(text, result);

        // Resize if necessary
        if (count < text.Length)
        {
            Array.Resize(ref result, count);
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
