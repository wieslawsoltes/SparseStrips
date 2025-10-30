// Copyright 2025 Wieslaw Soltes
// SPDX-License-Identifier: Apache-2.0 OR MIT

using Vello.Native;

namespace Vello;

/// <summary>
/// Represents a raster image that can be used as paint
/// </summary>
public sealed class Image : IDisposable
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

    private Image(nint handle)
    {
        _handle = handle;
    }

    /// <summary>
    /// Creates an image from a pixmap with specified sampling parameters
    /// </summary>
    /// <param name="pixmap">Source pixmap</param>
    /// <param name="xExtend">Horizontal extend mode (default: Pad)</param>
    /// <param name="yExtend">Vertical extend mode (default: Pad)</param>
    /// <param name="quality">Sampling quality (default: Medium)</param>
    /// <param name="alpha">Alpha multiplier 0.0-1.0 (default: 1.0)</param>
    public static Image FromPixmap(
        Pixmap pixmap,
        GradientExtend xExtend = GradientExtend.Pad,
        GradientExtend yExtend = GradientExtend.Pad,
        ImageQuality quality = ImageQuality.Medium,
        float alpha = 1.0f)
    {
        ArgumentNullException.ThrowIfNull(pixmap);

        var handle = NativeMethods.Image_NewFromPixmap(
            pixmap.Handle,
            (VelloExtend)xExtend,
            (VelloExtend)yExtend,
            (VelloImageQuality)quality,
            alpha);

        if (handle == IntPtr.Zero)
            throw new VelloException("Failed to create image");

        return new Image(handle);
    }

    public void Dispose()
    {
        if (!_disposed)
        {
            if (_handle != IntPtr.Zero)
            {
                NativeMethods.Image_Free(_handle);
                _handle = IntPtr.Zero;
            }
            _disposed = true;
        }
    }

    ~Image()
    {
        Dispose();
    }
}
