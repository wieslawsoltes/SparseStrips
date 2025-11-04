// Copyright 2025
// SPDX-License-Identifier: Apache-2.0 OR MIT

using System;
using System.Runtime.InteropServices;

namespace Vello.Native.FastPath;

/// <summary>
/// Managed wrapper around a native pixmap for direct pixel access.
/// </summary>
public ref struct NativePixmap : IDisposable
{
    private nint _handle;

    /// <summary>
    /// Initializes a new pixmap with the specified dimensions.
    /// </summary>
    /// <param name="width">Pixmap width in pixels.</param>
    /// <param name="height">Pixmap height in pixels.</param>
    public NativePixmap(ushort width, ushort height)
    {
        _handle = NativeResult.EnsureHandle(
            NativeMethods.Pixmap_New(width, height),
            nameof(NativeMethods.Pixmap_New));
    }

    /// <summary>
    /// Gets the native pixmap handle.
    /// </summary>
    public nint Handle
    {
        get
        {
            EnsureNotDisposed();
            return _handle;
        }
    }

    /// <summary>
    /// Gets a value indicating whether the pixmap owns a native handle.
    /// </summary>
    public bool IsAllocated => _handle != nint.Zero;

    /// <summary>
    /// Gets the current pixmap width.
    /// </summary>
    public ushort Width
    {
        get
        {
            EnsureNotDisposed();
            return NativeMethods.Pixmap_Width(_handle);
        }
    }

    /// <summary>
    /// Gets the current pixmap height.
    /// </summary>
    public ushort Height
    {
        get
        {
            EnsureNotDisposed();
            return NativeMethods.Pixmap_Height(_handle);
        }
    }

    /// <summary>
    /// Captures the underlying pixel buffer as a managed array.
    /// </summary>
    /// <returns>Array snapshot containing premultiplied RGBA pixels.</returns>
    public unsafe VelloPremulRgba8[] SnapshotPixels()
    {
        EnsureNotDisposed();

        nint dataPtr = nint.Zero;
        nuint elementCount = 0;

        NativeResult.ThrowIfFailed(
            NativeMethods.Pixmap_Data(_handle, &dataPtr, &elementCount),
            nameof(NativeMethods.Pixmap_Data));

        if (dataPtr == nint.Zero || elementCount == 0)
        {
            throw new InvalidOperationException("Pixmap_Data returned an empty buffer.");
        }

        int pixelCount = checked((int)elementCount);
        return new ReadOnlySpan<VelloPremulRgba8>((void*)dataPtr, pixelCount).ToArray();
    }

    /// <summary>
    /// Gets the total number of pixels in the pixmap.
    /// </summary>
    /// <returns>The pixel count.</returns>
    public unsafe nuint GetPixelCount()
    {
        EnsureNotDisposed();

        nint dataPtr = nint.Zero;
        nuint elementCount = 0;

        NativeResult.ThrowIfFailed(
            NativeMethods.Pixmap_Data(_handle, &dataPtr, &elementCount),
            nameof(NativeMethods.Pixmap_Data));

        return elementCount;
    }

    /// <summary>
    /// Provides mutable span access to the pixel buffer.
    /// </summary>
    /// <param name="mutator">Delegate that mutates the pixel span.</param>
    public unsafe void MutatePixels(PixelSpanMutator mutator)
    {
        EnsureNotDisposed();
        ArgumentNullException.ThrowIfNull(mutator);

        nint dataPtr = nint.Zero;
        nuint elementCount = 0;

        NativeResult.ThrowIfFailed(
            NativeMethods.Pixmap_DataMut(_handle, &dataPtr, &elementCount),
            nameof(NativeMethods.Pixmap_DataMut));

        if (dataPtr == nint.Zero || elementCount == 0)
        {
            throw new InvalidOperationException("Pixmap_DataMut returned an empty buffer.");
        }

        int pixelCount = checked((int)elementCount);
        var span = new Span<VelloPremulRgba8>((void*)dataPtr, pixelCount);
        mutator(span);
    }

    /// <summary>
    /// Resizes the pixmap to the specified dimensions.
    /// </summary>
    /// <param name="width">Target width in pixels.</param>
    /// <param name="height">Target height in pixels.</param>
    public void Resize(ushort width, ushort height)
    {
        EnsureNotDisposed();
        NativeResult.ThrowIfFailed(
            NativeMethods.Pixmap_Resize(_handle, width, height),
            nameof(NativeMethods.Pixmap_Resize));
    }

    /// <summary>
    /// Releases the native pixmap handle.
    /// </summary>
    public void Dispose()
    {
        if (_handle == nint.Zero)
        {
            return;
        }

        NativeMethods.Pixmap_Free(_handle);
        _handle = nint.Zero;
    }

    private void EnsureNotDisposed()
    {
        if (_handle == nint.Zero)
        {
            throw new ObjectDisposedException(nameof(NativePixmap));
        }
    }
}
