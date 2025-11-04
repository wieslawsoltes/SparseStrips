using Avalonia.Media.Imaging;
using Vello;

namespace Vello.Avalonia.Rendering;

/// <summary>
/// Provides helpers for copying Vello render buffers into Avalonia bitmaps.
/// </summary>
public static class VelloBitmapBlitter
{
    /// <summary>
    /// Copies the current contents of the supplied <paramref name="context"/> into the <paramref name="target"/> bitmap.
    /// When the bitmap stride differs from the tight RGBA stride, the method uses <paramref name="scratchBuffer"/>
    /// as a temporary staging area, growing it as needed.
    /// </summary>
    /// <param name="context">The render context containing the latest rendered pixels.</param>
    /// <param name="target">The destination bitmap to copy into.</param>
    /// <param name="scratchBuffer">
    /// A reusable scratch buffer that will be resized automatically when larger frames are rendered.
    /// </param>
    public static unsafe void Blit(RenderContext context, WriteableBitmap target, ref byte[]? scratchBuffer)
    {
        using var locked = target.Lock();

        int width = target.PixelSize.Width;
        int height = target.PixelSize.Height;
        int stride = locked.RowBytes;

        var destination = new Span<byte>((void*)locked.Address, stride * height);

        if (stride == width * 4)
        {
            context.RenderToBuffer(destination, (ushort)width, (ushort)height);
            return;
        }

        Span<byte> scratch = AcquireScratch(ref scratchBuffer, width, height);
        context.RenderToBuffer(scratch, (ushort)width, (ushort)height);
        CopyRows(scratch, destination, width, height, stride);
    }

    /// <summary>
    /// Copies pixel rows between buffers that may have different strides.
    /// </summary>
    /// <param name="source">Tightly packed RGBA source buffer.</param>
    /// <param name="target">Destination buffer that may include padding between rows.</param>
    /// <param name="width">Row width, in pixels.</param>
    /// <param name="height">Number of rows to copy.</param>
    /// <param name="targetStride">Stride of the destination buffer, in bytes.</param>
    public static void CopyRows(ReadOnlySpan<byte> source, Span<byte> target, int width, int height, int targetStride)
    {
        int rowBytes = width * 4;
        for (int y = 0; y < height; y++)
        {
            source.Slice(y * rowBytes, rowBytes)
                .CopyTo(target.Slice(y * targetStride, rowBytes));
        }
    }

    private static Span<byte> AcquireScratch(ref byte[]? scratchBuffer, int width, int height)
    {
        int required = checked(width * height * 4);
        if (scratchBuffer is null || scratchBuffer.Length < required)
        {
            scratchBuffer = new byte[required];
        }

        return scratchBuffer.AsSpan(0, required);
    }
}
