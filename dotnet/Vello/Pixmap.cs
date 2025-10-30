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
    /// Get pixel data as bytes (zero-copy access, reinterpreted view).
    /// The span is only valid while the Pixmap is alive.
    /// Each pixel is 4 bytes: R, G, B, A (premultiplied).
    /// </summary>
    public unsafe ReadOnlySpan<byte> GetBytes()
    {
        ObjectDisposedException.ThrowIf(_disposed, this);

        nint ptr;
        nuint len;
        VelloException.ThrowIfError(
            NativeMethods.Pixmap_Data(_handle, &ptr, &len));

        // Each PremulRgba8 is 4 bytes, so multiply length by 4
        return new ReadOnlySpan<byte>(ptr.ToPointer(), (int)len * 4);
    }

    /// <summary>
    /// Copy pixel data as bytes to the destination span.
    /// Each pixel is 4 bytes: R, G, B, A (premultiplied).
    /// </summary>
    /// <param name="destination">Destination span (must be at least Width * Height * 4 bytes)</param>
    public void CopyBytesTo(Span<byte> destination)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);

        var sourceBytes = GetBytes();
        if (destination.Length < sourceBytes.Length)
            throw new ArgumentException($"Destination span too small. Required: {sourceBytes.Length}, Got: {destination.Length}", nameof(destination));

        sourceBytes.CopyTo(destination);
    }

    /// <summary>
    /// Copy pixel data to a byte array.
    /// Note: Consider using GetBytes() for zero-copy access or CopyBytesTo() for better performance.
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
    /// Load a pixmap from PNG data (zero-allocation for ReadOnlySpan sources).
    /// </summary>
    /// <param name="pngData">PNG-encoded image data</param>
    /// <returns>A new Pixmap containing the decoded image</returns>
    public static unsafe Pixmap FromPng(ReadOnlySpan<byte> pngData)
    {
        if (pngData.IsEmpty)
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
    /// Load a pixmap from PNG data (array overload).
    /// For zero-allocation, use the ReadOnlySpan&lt;byte&gt; overload.
    /// </summary>
    public static Pixmap FromPng(byte[] pngData)
    {
        ArgumentNullException.ThrowIfNull(pngData);
        return FromPng(pngData.AsSpan());
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
    /// Tries to write PNG-encoded data to the destination span.
    /// </summary>
    /// <param name="destination">Destination span for PNG data</param>
    /// <param name="bytesWritten">Number of bytes written if successful</param>
    /// <returns>True if successful, false if destination too small</returns>
    public unsafe bool TryToPng(Span<byte> destination, out int bytesWritten)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);

        byte* dataPtr;
        nuint len;

        VelloException.ThrowIfError(
            NativeMethods.Pixmap_ToPng(_handle, &dataPtr, &len));

        try
        {
            if (len > (nuint)destination.Length)
            {
                bytesWritten = 0;
                return false;
            }

            new ReadOnlySpan<byte>(dataPtr, (int)len).CopyTo(destination);
            bytesWritten = (int)len;
            return true;
        }
        finally
        {
            NativeMethods.PngDataFree(dataPtr, len);
        }
    }

    /// <summary>
    /// Get the size needed for PNG encoding.
    /// Useful for allocating the right buffer size before calling TryToPng.
    /// </summary>
    /// <returns>Number of bytes needed for PNG encoding</returns>
    public unsafe int GetPngSize()
    {
        ObjectDisposedException.ThrowIf(_disposed, this);

        byte* dataPtr;
        nuint len;

        VelloException.ThrowIfError(
            NativeMethods.Pixmap_ToPng(_handle, &dataPtr, &len));

        try
        {
            return (int)len;
        }
        finally
        {
            NativeMethods.PngDataFree(dataPtr, len);
        }
    }

    /// <summary>
    /// Save pixmap as PNG data.
    /// Note: Consider using TryToPng() for zero-allocation scenarios with pre-allocated buffers.
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
