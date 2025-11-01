// Copyright 2025
// SPDX-License-Identifier: Apache-2.0 OR MIT

using System;
using System.Runtime.InteropServices;
using Vello.Native;
using Xunit;

namespace Vello.Tests.Interop;

[Collection(NativeInteropCollection.CollectionName)]
public sealed class UtilitiesInteropTests
{
    [Fact]
    public void Version_ReturnsNonEmptyString()
    {
        nint ptr = NativeMethods.Version();
        Assert.NotEqual(nint.Zero, ptr);

        string? version = Marshal.PtrToStringAnsi(ptr);
        Assert.False(string.IsNullOrWhiteSpace(version));
    }

    [Fact]
    public void SimdDetect_ReturnsKnownLevel()
    {
        VelloSimdLevel level = NativeMethods.SimdDetect();
        Assert.True(Enum.IsDefined(typeof(VelloSimdLevel), level));
    }

    [Fact]
    public void GetLastError_ReturnsMessageAfterFailure()
    {
        NativeMethods.ClearLastError();
        Assert.Equal(nint.Zero, NativeMethods.GetLastError());

        unsafe
        {
            var result = NativeMethods.RenderContext_FillRect(nint.Zero, (VelloRect*)0);
            Assert.NotEqual(NativeMethods.VELLO_OK, result);
        }

        nint errorPtr = NativeMethods.GetLastError();
        Assert.NotEqual(nint.Zero, errorPtr);
        string? message = Marshal.PtrToStringAnsi(errorPtr);
        Assert.Contains("Null", message, StringComparison.OrdinalIgnoreCase);

        NativeMethods.ClearLastError();
        Assert.Equal(nint.Zero, NativeMethods.GetLastError());
    }

    [Fact]
    public void ClearLastError_WithoutError_IsNoOp()
    {
        NativeMethods.ClearLastError();
        Assert.Equal(nint.Zero, NativeMethods.GetLastError());

        NativeMethods.ClearLastError();
        Assert.Equal(nint.Zero, NativeMethods.GetLastError());
    }
}
