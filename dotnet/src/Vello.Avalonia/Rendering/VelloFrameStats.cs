namespace Vello.Avalonia.Rendering;

/// <summary>
/// Represents averaged timing statistics for frames rendered by a <see cref="Controls.VelloSurface"/>.
/// </summary>
public readonly struct VelloFrameStats
{
    public VelloFrameStats(double frameTimeMilliseconds, double framesPerSecond, int pixelWidth, int pixelHeight)
    {
        FrameTimeMilliseconds = frameTimeMilliseconds;
        FramesPerSecond = framesPerSecond;
        PixelWidth = pixelWidth;
        PixelHeight = pixelHeight;
    }

    /// <summary>
    /// Gets the average frame time over the sampling window, in milliseconds.
    /// </summary>
    public double FrameTimeMilliseconds { get; }

    /// <summary>
    /// Gets the average frames per second over the sampling window.
    /// </summary>
    public double FramesPerSecond { get; }

    /// <summary>
    /// Gets the pixel width of the rendered surface.
    /// </summary>
    public int PixelWidth { get; }

    /// <summary>
    /// Gets the pixel height of the rendered surface.
    /// </summary>
    public int PixelHeight { get; }
}
