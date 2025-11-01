// Copyright 2025
// SPDX-License-Identifier: Apache-2.0 OR MIT

using System;

namespace Vello.Native.FastPath;

/// <summary>
/// Managed wrapper around a mask created from a pixmap.
/// </summary>
public ref struct NativeMask : IDisposable
{
    private nint _handle;

    /// <summary>
    /// Creates a mask from a source pixmap.
    /// </summary>
    /// <param name="source">Pixmap used to derive mask coverage.</param>
    /// <param name="luminance">Set to <c>true</c> to derive coverage from luminance instead of alpha.</param>
    public NativeMask(NativePixmap source, bool luminance = false)
    {
        if (!source.IsAllocated)
        {
            throw new ArgumentException("Pixmap is not initialized.", nameof(source));
        }

        _handle = NativeResult.EnsureHandle(
            luminance
                ? NativeMethods.Mask_NewLuminance(source.Handle)
                : NativeMethods.Mask_NewAlpha(source.Handle),
            luminance
                ? nameof(NativeMethods.Mask_NewLuminance)
                : nameof(NativeMethods.Mask_NewAlpha));
    }

    /// <summary>
    /// Gets the native mask handle.
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
    /// Gets a value indicating whether the mask owns a native handle.
    /// </summary>
    public bool IsAllocated => _handle != nint.Zero;

    /// <summary>
    /// Gets the mask width.
    /// </summary>
    public ushort Width
    {
        get
        {
            EnsureNotDisposed();
            return NativeMethods.Mask_GetWidth(_handle);
        }
    }

    /// <summary>
    /// Gets the mask height.
    /// </summary>
    public ushort Height
    {
        get
        {
            EnsureNotDisposed();
            return NativeMethods.Mask_GetHeight(_handle);
        }
    }

    /// <summary>
    /// Releases the native mask.
    /// </summary>
    public void Dispose()
    {
        if (_handle == nint.Zero)
        {
            return;
        }

        NativeMethods.Mask_Free(_handle);
        _handle = nint.Zero;
    }

    private void EnsureNotDisposed()
    {
        if (_handle == nint.Zero)
        {
            throw new ObjectDisposedException(nameof(NativeMask));
        }
    }
}
