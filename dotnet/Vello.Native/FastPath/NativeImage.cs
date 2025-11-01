// Copyright 2025
// SPDX-License-Identifier: Apache-2.0 OR MIT

using System;

namespace Vello.Native.FastPath;

/// <summary>
/// Managed wrapper around a native image resource.
/// </summary>
public ref struct NativeImage : IDisposable
{
    private nint _handle;

    private NativeImage(nint handle)
    {
        _handle = handle;
    }

    /// <summary>
    /// Gets the native image handle.
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
    /// Gets a value indicating whether the image owns a native handle.
    /// </summary>
    public bool IsAllocated => _handle != nint.Zero;

    /// <summary>
    /// Creates a new image by sampling a pixmap.
    /// </summary>
    /// <param name="pixmap">Pixmap source.</param>
    /// <param name="xExtend">Extend mode on the X axis.</param>
    /// <param name="yExtend">Extend mode on the Y axis.</param>
    /// <param name="quality">Sampling quality.</param>
    /// <param name="alpha">Global alpha multiplier.</param>
    /// <returns>A managed wrapper owning the native image.</returns>
    public static NativeImage FromPixmap(
        NativePixmap pixmap,
        VelloExtend xExtend = VelloExtend.Pad,
        VelloExtend yExtend = VelloExtend.Pad,
        VelloImageQuality quality = VelloImageQuality.Medium,
        float alpha = 1f)
    {
        if (!pixmap.IsAllocated)
        {
            throw new ArgumentException("Pixmap is not initialized.", nameof(pixmap));
        }

        var handle = NativeResult.EnsureHandle(
            NativeMethods.Image_NewFromPixmap(pixmap.Handle, xExtend, yExtend, quality, alpha),
            nameof(NativeMethods.Image_NewFromPixmap));

        return new NativeImage(handle);
    }

    /// <summary>
    /// Wraps an existing native image handle.
    /// </summary>
    /// <param name="handle">Native image handle.</param>
    /// <returns>A managed wrapper assuming ownership of the handle.</returns>
    public static NativeImage FromHandle(nint handle)
    {
        if (handle == nint.Zero)
        {
            throw new ArgumentException("Handle must not be null.", nameof(handle));
        }

        return new NativeImage(handle);
    }

    /// <summary>
    /// Releases the native image.
    /// </summary>
    public void Dispose()
    {
        if (_handle == nint.Zero)
        {
            return;
        }

        NativeMethods.Image_Free(_handle);
        _handle = nint.Zero;
    }

    private void EnsureNotDisposed()
    {
        if (_handle == nint.Zero)
        {
            throw new ObjectDisposedException(nameof(NativeImage));
        }
    }
}
