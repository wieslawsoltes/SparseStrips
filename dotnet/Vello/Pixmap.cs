// Copyright 2025 Wieslaw Soltes
// SPDX-License-Identifier: Apache-2.0 OR MIT

using Vello.Native;

namespace Vello;

/// <summary>
/// A pixmap containing premultiplied RGBA8 pixel data.
/// </summary>
public sealed class Pixmap : IDisposable
{
    private nint _handle;
    private bool _disposed;

    public Pixmap(ushort width, ushort height)
    {
        _handle = NativeMethods.Pixmap_New(width, height);
        if (_handle == 0)
            throw new VelloException("Failed to create Pixmap");
    }

    internal nint Handle
    {
        get
        {
            ObjectDisposedException.ThrowIf(_disposed, this);
            return _handle;
        }
    }

    public ushort Width
    {
        get
        {
            ObjectDisposedException.ThrowIf(_disposed, this);
            return NativeMethods.Pixmap_Width(_handle);
        }
    }

    public ushort Height
    {
        get
        {
            ObjectDisposedException.ThrowIf(_disposed, this);
            return NativeMethods.Pixmap_Height(_handle);
        }
    }

    /// <summary>
    /// Get a read-only span of pixels (zero-copy access).
    /// The span is only valid while the Pixmap is alive.
    /// </summary>
    public unsafe ReadOnlySpan<PremulRgba8> GetPixels()
    {
        ObjectDisposedException.ThrowIf(_disposed, this);

        nint ptr;
        nuint len;
        VelloException.ThrowIfError(
            NativeMethods.Pixmap_Data(_handle, &ptr, &len));

        return new ReadOnlySpan<PremulRgba8>(
            ptr.ToPointer(),
            (int)len);
    }

    /// <summary>
    /// Copy pixel data to a byte array.
    /// </summary>
    public byte[] ToByteArray()
    {
        var pixels = GetPixels();
        var bytes = new byte[pixels.Length * 4];

        for (int i = 0; i < pixels.Length; i++)
        {
            bytes[i * 4 + 0] = pixels[i].R;
            bytes[i * 4 + 1] = pixels[i].G;
            bytes[i * 4 + 2] = pixels[i].B;
            bytes[i * 4 + 3] = pixels[i].A;
        }

        return bytes;
    }

    /// <summary>
    /// Load a pixmap from PNG data.
    /// </summary>
    public static unsafe Pixmap FromPng(byte[] pngData)
    {
        ArgumentNullException.ThrowIfNull(pngData);
        if (pngData.Length == 0)
            throw new ArgumentException("PNG data cannot be empty", nameof(pngData));

        fixed (byte* dataPtr = pngData)
        {
            nint handle = NativeMethods.Pixmap_FromPng(dataPtr, (nuint)pngData.Length);
            if (handle == 0)
                throw new VelloException("Failed to load PNG");

            return new Pixmap(handle);
        }
    }

    /// <summary>
    /// Load a pixmap from a PNG file.
    /// </summary>
    public static Pixmap FromPngFile(string path)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);
        byte[] pngData = File.ReadAllBytes(path);
        return FromPng(pngData);
    }

    /// <summary>
    /// Save pixmap as PNG data.
    /// </summary>
    public unsafe byte[] ToPng()
    {
        ObjectDisposedException.ThrowIf(_disposed, this);

        byte* dataPtr;
        nuint len;

        VelloException.ThrowIfError(
            NativeMethods.Pixmap_ToPng(_handle, &dataPtr, &len));

        try
        {
            var result = new byte[len];
            new ReadOnlySpan<byte>(dataPtr, (int)len).CopyTo(result);
            return result;
        }
        finally
        {
            NativeMethods.PngDataFree(dataPtr, len);
        }
    }

    /// <summary>
    /// Save pixmap to a PNG file.
    /// </summary>
    public void SaveAsPng(string path)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);
        byte[] pngData = ToPng();
        File.WriteAllBytes(path, pngData);
    }

    /// <summary>
    /// Internal constructor for pixmaps created from native handles (e.g., FromPng).
    /// </summary>
    private Pixmap(nint handle)
    {
        _handle = handle;
    }

    public void Dispose()
    {
        if (!_disposed)
        {
            if (_handle != 0)
            {
                NativeMethods.Pixmap_Free(_handle);
                _handle = 0;
            }
            _disposed = true;
        }
        GC.SuppressFinalize(this);
    }

    ~Pixmap() => Dispose();
}
