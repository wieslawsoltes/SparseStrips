// Copyright 2025
// SPDX-License-Identifier: Apache-2.0 OR MIT

using Vello.Native;
using Vello.Native.FastPath;
using Xunit;

namespace Vello.Tests.Interop;

internal static class NativeTestHelpers
{
    public const ushort DefaultWidth = 16;
    public const ushort DefaultHeight = 16;

    public static NativeRenderContext CreateContext(
        ushort width = DefaultWidth,
        ushort height = DefaultHeight) =>
        new(width, height);

    public static NativePixmap CreatePixmap(
        ushort width = DefaultWidth,
        ushort height = DefaultHeight) =>
        new(width, height);

    public static NativeImage CreateImageFromPixmap(
        NativePixmap pixmap,
        VelloExtend xExtend = VelloExtend.Pad,
        VelloExtend yExtend = VelloExtend.Pad,
        VelloImageQuality quality = VelloImageQuality.Medium,
        float alpha = 1f) =>
        NativeImage.FromPixmap(pixmap, xExtend, yExtend, quality, alpha);

    public static void AssertSuccess(int errorCode, string operation)
    {
        try
        {
            NativeResult.ThrowIfFailed(errorCode, operation);
        }
        catch (NativeException ex)
        {
            Assert.Fail(ex.Message);
        }
    }

    public static void AssertError(int errorCode, string operation)
    {
        if (errorCode == NativeMethods.VELLO_OK)
        {
            Assert.Fail($"{operation} unexpectedly succeeded.");
        }

        var errorPtr = NativeMethods.GetLastError();
        if (errorPtr != nint.Zero)
        {
            NativeMethods.ClearLastError();
        }
    }

    public static VelloPremulRgba8 Premul(byte r, byte g, byte b, byte a = 255) =>
        new() { R = r, G = g, B = b, A = a };
}
