// Copyright 2025 Wieslaw Soltes
// SPDX-License-Identifier: Apache-2.0 OR MIT

using Vello.Native;

namespace Vello;

/// <summary>
/// Represents a mask for alpha or luminance-based rendering
/// </summary>
public sealed class Mask : IDisposable
{
    private nint _handle;
    private bool _disposed;

    internal nint Handle
    {
        get
        {
            ObjectDisposedException.ThrowIf(_disposed, this);
            return _handle;
        }
    }

    internal Mask(nint handle)
    {
        _handle = handle;
    }

    /// <summary>
    /// Creates an alpha mask from a pixmap (uses the alpha channel)
    /// </summary>
    public static Mask NewAlpha(Pixmap pixmap)
    {
        ArgumentNullException.ThrowIfNull(pixmap);
        var handle = NativeMethods.Mask_NewAlpha(pixmap.Handle);
        if (handle == IntPtr.Zero)
            throw new VelloException("Failed to create alpha mask");
        return new Mask(handle);
    }

    /// <summary>
    /// Creates a luminance mask from a pixmap (uses RGB luminance calculation)
    /// </summary>
    public static Mask NewLuminance(Pixmap pixmap)
    {
        ArgumentNullException.ThrowIfNull(pixmap);
        var handle = NativeMethods.Mask_NewLuminance(pixmap.Handle);
        if (handle == IntPtr.Zero)
            throw new VelloException("Failed to create luminance mask");
        return new Mask(handle);
    }

    /// <summary>
    /// Gets the width of the mask
    /// </summary>
    public ushort Width => NativeMethods.Mask_GetWidth(Handle);

    /// <summary>
    /// Gets the height of the mask
    /// </summary>
    public ushort Height => NativeMethods.Mask_GetHeight(Handle);

    public void Dispose()
    {
        if (!_disposed)
        {
            if (_handle != IntPtr.Zero)
            {
                NativeMethods.Mask_Free(_handle);
                _handle = IntPtr.Zero;
            }
            _disposed = true;
        }
    }

    ~Mask()
    {
        Dispose();
    }
}
