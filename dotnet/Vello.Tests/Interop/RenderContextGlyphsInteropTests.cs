// Copyright 2025
// SPDX-License-Identifier: Apache-2.0 OR MIT

using System;
using System.Buffers.Binary;
using System.IO;
using System.Text;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Vello.Native;
using Xunit;

namespace Vello.Tests.Interop;

[Collection(NativeInteropCollection.CollectionName)]
public sealed class RenderContextGlyphsInteropTests
{
    private static readonly string FontPath = Path.GetFullPath(Path.Combine(
        AppContext.BaseDirectory,
        "..",
        "..",
        "..",
        "..",
        "..",
        "dotnet",
        "Vello.Tests",
        "TestAssets",
        "fonts",
        "Inter-Regular.ttf"));

    [Fact]
    public unsafe void RenderContext_FillGlyphs_RendersExpectedPixels()
    {
        Assert.True(File.Exists(FontPath),
            $"Expected font asset at {FontPath}. See docs/tests/native-font-fixture.md.");

        byte[] fontData = File.ReadAllBytes(FontPath);
        fixed (byte* fontPtr = fontData)
        {
            nint fontHandle = NativeMethods.FontData_New(fontPtr, (nuint)fontData.Length, 0);
            Assert.NotEqual(nint.Zero, fontHandle);

            try
            {
                using var ctx = NativeTestHelpers.CreateContext(64, 32);
                using var pixmap = NativeTestHelpers.CreatePixmap(64, 32);

                // Set white background so glyph pixels stand out.
                NativeTestHelpers.AssertSuccess(
                    NativeMethods.RenderContext_SetPaintSolid(ctx.Handle, 255, 255, 255, 255),
                    "RenderContext_SetPaintSolid background");
                FillFullRect(ctx.Handle);

                NativeTestHelpers.AssertSuccess(
                    NativeMethods.RenderContext_SetPaintSolid(ctx.Handle, 0, 0, 0, 255),
                    "RenderContext_SetPaintSolid glyph");

                VelloGlyph[] glyphs = new[]
                {
                    new VelloGlyph { Id = GetGlyphId('A', fontData), X = 4f, Y = 24f },
                    new VelloGlyph { Id = GetGlyphId('B', fontData), X = 24f, Y = 24f }
                };

                Assert.All(glyphs, g => Assert.NotEqual<uint>(0, g.Id));

                fixed (VelloGlyph* glyphPtr = glyphs)
                {
                    NativeTestHelpers.AssertSuccess(
                        NativeMethods.RenderContext_FillGlyphs(
                            ctx.Handle,
                            fontHandle,
                            fontSize: 20f,
                            glyphs: glyphPtr,
                            glyphCount: (nuint)glyphs.Length),
                        "RenderContext_FillGlyphs");
                }

                FlushAndRender(ctx.Handle, pixmap.Handle);
                var pixels = pixmap.SnapshotPixels();

                Assert.True(CountDarkPixels(pixels) > 0, "Expected black glyph pixels on white background.");
            }
            finally
            {
                NativeMethods.FontData_Free(fontHandle);
            }
        }
    }

    [Fact]
    public unsafe void FontData_TextToGlyphs_ReturnsGlyphIds()
    {
        Assert.True(File.Exists(FontPath),
            $"Expected font asset at {FontPath}. See docs/tests/native-font-fixture.md.");

        byte[] fontData = File.ReadAllBytes(FontPath);
        fixed (byte* fontPtr = fontData)
        {
            nint fontHandle = NativeMethods.FontData_New(fontPtr, (nuint)fontData.Length, 0);
            Assert.NotEqual(nint.Zero, fontHandle);

            try
            {
                byte[] textBytes = Encoding.UTF8.GetBytes("AB\0");
                VelloGlyph[] glyphs = new VelloGlyph[4];
                nuint count = 0;
                int result;

                fixed (byte* textPtr = textBytes)
                fixed (VelloGlyph* glyphPtr = glyphs)
                {
                    nuint* countPtr = &count;
                    result = NativeMethods.FontData_TextToGlyphs(
                        fontHandle,
                        textPtr,
                        glyphPtr,
                        (nuint)glyphs.Length,
                        countPtr);
                }

                Assert.Equal(NativeMethods.VELLO_OK, result);
                Assert.Equal((nuint)2, count);
                Assert.NotEqual(0u, glyphs[0].Id);
                Assert.NotEqual(0u, glyphs[1].Id);
            }
            finally
            {
                NativeMethods.FontData_Free(fontHandle);
            }
        }
    }

    private static uint GetGlyphId(char ch, byte[] fontData)
    {
        uint character = ch;

        int numTables = ReadUInt16(fontData, 4);
        int cmapOffset = -1;
        for (int i = 0; i < numTables; i++)
        {
            int tableOffset = 12 + (i * 16);
            uint tag = ReadUInt32(fontData, tableOffset);
            if (tag == 0x636D6170) // 'cmap'
            {
                cmapOffset = (int)ReadUInt32(fontData, tableOffset + 8);
                break;
            }
        }

        if (cmapOffset < 0)
        {
            throw new InvalidOperationException("cmap table not found in font.");
        }

        int cmapTable = cmapOffset;
        int numSubTables = ReadUInt16(fontData, cmapTable + 2);

        int bestOffset = -1;
        for (int i = 0; i < numSubTables; i++)
        {
            int recordOffset = cmapTable + 4 + (i * 8);
            ushort platformId = ReadUInt16(fontData, recordOffset);
            ushort encodingId = ReadUInt16(fontData, recordOffset + 2);
            uint subtableOffset = ReadUInt32(fontData, recordOffset + 4);

            bool preferredWindows = platformId == 3 && (encodingId == 10 || encodingId == 1);
            bool unicode = platformId == 0;

            if (preferredWindows)
            {
                bestOffset = cmapTable + (int)subtableOffset;
                break;
            }

            if (bestOffset < 0 && unicode)
            {
                bestOffset = cmapTable + (int)subtableOffset;
            }
        }

        if (bestOffset < 0)
        {
            throw new InvalidOperationException("No Unicode cmap subtable found.");
        }

        ushort format = ReadUInt16(fontData, bestOffset);
        if (format != 4)
        {
            throw new NotSupportedException($"Unsupported cmap format: {format}");
        }

        ushort length = ReadUInt16(fontData, bestOffset + 2);
        ushort segCountX2 = ReadUInt16(fontData, bestOffset + 6);
        int segCount = segCountX2 / 2;

        int endCodeOffset = bestOffset + 14;
        int reservedPadOffset = endCodeOffset + segCount * 2;
        int startCodeOffset = reservedPadOffset + 2;
        int idDeltaOffset = startCodeOffset + segCount * 2;
        int idRangeOffsetOffset = idDeltaOffset + segCount * 2;
        int glyphIdArrayOffset = idRangeOffsetOffset + segCount * 2;

        for (int i = 0; i < segCount; i++)
        {
            ushort endCode = ReadUInt16(fontData, endCodeOffset + i * 2);
            ushort startCode = ReadUInt16(fontData, startCodeOffset + i * 2);

            if (character < startCode || character > endCode)
            {
                continue;
            }

            ushort idDelta = ReadUInt16(fontData, idDeltaOffset + i * 2);
            ushort idRangeOffset = ReadUInt16(fontData, idRangeOffsetOffset + i * 2);

            if (idRangeOffset == 0)
            {
                return (uint)((character + idDelta) & 0xFFFF);
            }

            int idRangeOffsetPos = idRangeOffsetOffset + i * 2;
            int glyphIndexPos = idRangeOffsetPos + idRangeOffset + 2 * (int)(character - startCode);
            if (glyphIndexPos < glyphIdArrayOffset || glyphIndexPos + 1 >= bestOffset + length)
            {
                return 0;
            }

            ushort glyphId = ReadUInt16(fontData, glyphIndexPos);
            if (glyphId == 0)
            {
                return 0;
            }

            return (uint)((glyphId + idDelta) & 0xFFFF);
        }

        return 0;
    }

    private static ushort ReadUInt16(byte[] data, int offset) =>
        BinaryPrimitives.ReadUInt16BigEndian(data.AsSpan(offset, sizeof(ushort)));

    private static uint ReadUInt32(byte[] data, int offset) =>
        BinaryPrimitives.ReadUInt32BigEndian(data.AsSpan(offset, sizeof(uint)));

    private static unsafe void FillFullRect(nint ctx)
    {
        VelloRect rect = new()
        {
            X0 = 0,
            Y0 = 0,
            X1 = 64,
            Y1 = 32
        };

        NativeTestHelpers.AssertSuccess(
            NativeMethods.RenderContext_FillRect(ctx, &rect),
            "RenderContext_FillRect");
    }

    private static void FlushAndRender(nint ctx, nint pixmap)
    {
        NativeTestHelpers.AssertSuccess(
            NativeMethods.RenderContext_Flush(ctx),
            "RenderContext_Flush");

        NativeTestHelpers.AssertSuccess(
            NativeMethods.RenderContext_RenderToPixmap(ctx, pixmap),
            "RenderContext_RenderToPixmap");
    }

    private static int CountDarkPixels(ReadOnlySpan<VelloPremulRgba8> pixels)
    {
        int count = 0;
        foreach (var pixel in pixels)
        {
            if (pixel.A > 0 && pixel.R < 200 && pixel.G < 200 && pixel.B < 200)
            {
                count++;
            }
        }

        return count;
    }
}
