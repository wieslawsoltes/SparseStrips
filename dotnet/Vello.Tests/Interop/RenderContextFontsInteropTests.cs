// Copyright 2025
// SPDX-License-Identifier: Apache-2.0 OR MIT

using System;
using Vello.Native;
using Xunit;

namespace Vello.Tests.Interop;

[Collection(NativeInteropCollection.CollectionName)]
public sealed class RenderContextFontsInteropTests
{
    [Fact]
    public unsafe void FontData_New_WithNullPointer_ReturnsZeroHandle()
    {
        nint handle = NativeMethods.FontData_New((byte*)0, 0, 0);
        Assert.Equal(nint.Zero, handle);
        NativeMethods.FontData_Free(handle); // should be a no-op if zero
    }

    [Fact]
    public unsafe void RenderContext_FillGlyphs_WithNullFont_ReturnsError()
    {
        using var ctx = NativeTestHelpers.CreateContext(16, 16);

        VelloGlyph[] glyphs =
        {
            new VelloGlyph
            {
                Id = 1,
                X = 0,
                Y = 0
            }
        };

        int result;
        fixed (VelloGlyph* glyphPtr = glyphs)
        {
            result = NativeMethods.RenderContext_FillGlyphs(
                ctx.Handle,
                font: nint.Zero,
                fontSize: 16f,
                glyphs: glyphPtr,
                glyphCount: (nuint)glyphs.Length);
        }

        Assert.NotEqual(NativeMethods.VELLO_OK, result);
    }

    [Fact]
    public unsafe void RenderContext_FillGlyphs_WithNullGlyphPointer_ReturnsError()
    {
        using var ctx = NativeTestHelpers.CreateContext(16, 16);

        int result = NativeMethods.RenderContext_FillGlyphs(
            ctx.Handle,
            font: (nint)1,
            fontSize: 16f,
            glyphs: (VelloGlyph*)0,
            glyphCount: 1);

        Assert.NotEqual(NativeMethods.VELLO_OK, result);
    }
}
